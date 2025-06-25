using System;

namespace PCL.Core.LifecycleManagement;

/// <summary>
/// 若要获取服务项自身的上下文实例，请使用 <see cref="Lifecycle.GetContext"/> 。
/// </summary>
public class LifecycleContext(
    ILifecycleService service,
    Action<LifecycleLogItem> onLog,
    Action<int> onRequestExit,
    Action<string?> onRequestRestart)
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
    /// <param name="statusCode">程序返回的状态码</param>
    public void RequestExit(int statusCode = 0) => onRequestExit(statusCode);
    
    /// <summary>
    /// 请求在程序退出时重启。调用该方法后，程序将在正常退出流程中自动执行重启，通常与退出程序结合使用。
    /// </summary>
    /// <param name="arguments">重启进程时使用的命令行参数</param>
    public void RequestRestartOnExit(string? arguments = null) => onRequestRestart(arguments);

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
