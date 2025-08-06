using System;

namespace PCL.Core.ProgramSetup.SourceManage;

public class InstanceSetupSourceManager : ISetupSourceManager, IDisposable
{
    #region ISetupSourceManager

    public string? Get(string key, string? gamePath)
    {
        if (gamePath is null)
            throw new ArgumentException("获取游戏实例配置时未提供游戏路径", nameof(gamePath));
        throw new NotImplementedException();
    }

    public string? Set(string key, string value, string? gamePath)
    {
        if (gamePath is null)
            throw new ArgumentException("获取游戏实例配置时未提供游戏路径", nameof(gamePath));
        throw new NotImplementedException();
    }

    public string? Remove(string key, string? gamePath)
    {
        if (gamePath is null)
            throw new ArgumentException("获取游戏实例配置时未提供游戏路径", nameof(gamePath));
        throw new NotImplementedException();
    }

    #endregion

    public InstanceSetupSourceManager()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }
}