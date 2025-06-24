using System;

namespace PCL.Core.LifecycleManagement;

/// <summary>
/// 若要获取服务项自身的上下文实例，请使用 <see cref="Lifecycle.GetContext"/> 。
/// </summary>
public class LifecycleContext(
    ILifecycleService service,
    Action<LifecycleLogItem> onLog,
    Action onRequestExit)
{
    public void CustomLog(
        string message,
        Exception? ex = null,
        LifecycleLogLevel level = LifecycleLogLevel.Trace,
        LifecycleActionLevel? actionLevel = null
    ) => onLog(new LifecycleLogItem(service, message, ex, level, actionLevel));
    
    public void Trace(string message, Exception? ex = null, LifecycleActionLevel? actionLevel = null) => CustomLog(message, ex, LifecycleLogLevel.Trace, actionLevel);
    public void Debug(string message, Exception? ex = null, LifecycleActionLevel? actionLevel = null) => CustomLog(message, ex, LifecycleLogLevel.Debug, actionLevel);
    public void Info(string message, Exception? ex = null, LifecycleActionLevel? actionLevel = null) => CustomLog(message, ex, LifecycleLogLevel.Info, actionLevel);
    public void Warn(string message, Exception? ex = null, LifecycleActionLevel? actionLevel = null) => CustomLog(message, ex, LifecycleLogLevel.Warning, actionLevel);
    public void Error(string message, Exception? ex = null, LifecycleActionLevel? actionLevel = null) => CustomLog(message, ex, LifecycleLogLevel.Error, actionLevel);
    public void Fatal(string message, Exception? ex = null, LifecycleActionLevel? actionLevel = null) => CustomLog(message, ex, LifecycleLogLevel.Fatal, actionLevel);

    /// <summary>
    /// 请求退出程序。仅可在 <see cref="LifecycleState.BeforeLoading"/> 时使用。
    /// </summary>
    public void RequestExit() => onRequestExit();

    // -- SYSTEM INSTANCE --
    
    private class SystemLifecycleService : ILifecycleService
    {
        public string Name => "系统";
        public string Identifier => "system";
        public bool SupportAsyncStart => false;
        public void Start() { }
        public void Stop() { }
    }

    /// <summary>
    /// 系统默认上下文，无特殊需求请勿使用。
    /// </summary>
    public static readonly LifecycleContext System = Lifecycle.GetContext(new SystemLifecycleService());
}
