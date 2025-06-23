using System;

namespace PCL.Core.Lifecycle;

public record LifecycleLogItem(
    ILifecycleService? Source,
    string Message,
    Exception? Ex = null,
    LifecycleLogLevel Level = LifecycleLogLevel.Trace,
    LifecycleActionLevel? ActionLevel = null)
{
    public DateTime Time { get; } = DateTime.Now;

    public override string ToString()
    {
        var source = Source == null ? "" : $" [{Source.Name} ({Source.Identifier})]";
        var basic = $"[{Time:HH:mm:ss.fff}:{Level}]{source} {Message}";
        return Ex == null ? basic : $"{basic}\n{Ex}";
    }
}
