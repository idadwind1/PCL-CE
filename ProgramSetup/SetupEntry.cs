using System.Collections.Generic;

namespace PCL.Core.ProgramSetup;

public sealed class SetupEntry(
    SetupEntrySource sourceType,
    string keyName,
    object defaultValue,
    bool isEncrypted = false)
{
    public readonly SetupEntrySource SourceType = sourceType;
    public readonly string KeyName = keyName;
    public readonly object DefaultValue = defaultValue;
    public readonly bool IsEncrypted = isEncrypted;

    #region ValueChanged event

    private static readonly Dictionary<string, List<ValueChangedHandler>> _HandlersDict = new();

    static SetupEntry()
    {
        SetupService.SetupChanged += _AllChangedHandler;
    }

    private static void _AllChangedHandler(SetupEntry entry, object? oldValue, object? newValue, string? gamePath)
    {
        ValueChangedHandler[] handlersCopy;
        lock (_HandlersDict)
        {
            if (!_HandlersDict.TryGetValue(entry.KeyName, out var handlers))
                return;
            handlersCopy = handlers.ToArray();
        }
        foreach (var handler in handlersCopy)
            handler.Invoke(oldValue ?? entry.DefaultValue, newValue ?? entry.DefaultValue, gamePath);
    }

    public static void UnsubscribeAllChangedHandlers()
    {
        lock (_HandlersDict)
            _HandlersDict.Clear();
    }

    public delegate void ValueChangedHandler(object oldValue, object newValue, string? gamePath);

    public event ValueChangedHandler? ValueChanged
    {
        add
        {
            if (value is null)
                return;
            lock (_HandlersDict)
            {
                if (!_HandlersDict.TryGetValue(KeyName, out var handlers))
                    _HandlersDict.Add(KeyName, handlers = new());
                handlers.Add(value);
            }
        }
        remove
        {
            if (value is null)
                return;
            lock (_HandlersDict)
            {
                if (!_HandlersDict.TryGetValue(KeyName, out var handlers))
                    return;
                handlers.Remove(value);
                if (handlers.Count == 0)
                    _HandlersDict.Remove(KeyName);
            }
        }
    }

    #endregion
}

public enum SetupEntrySource
{
    SystemGlobal,
    PathLocal,
    GameInstance
}