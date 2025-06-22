using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL.Core.Lifecycle;

/// <summary>
/// 启动器生命周期管理
/// </summary>
[LifecycleService(LifecycleState.BeforeLoading, Priority = int.MaxValue)]
public sealed class Lifecycle : ILifecycleService
{
    public string Identifier => "lifecycle";
    public string Name => "生命周期";
    public bool SupportAsyncStart => false;
    public void Start() { }
    public void Stop() { }
    
    private static LifecycleContext? _context;
    private Lifecycle() { _context = GetContext(this); }
    private static LifecycleContext Context => _context ?? LifecycleContext.Empty;
    
    private static ILifecycleLogService? _logService;
    private static readonly List<LifecycleLogItem> PendingLogs = [];
    private static readonly Dictionary<string, LifecycleServiceInfo> RunningServiceInfoMap = [];
    private static readonly LinkedList<ILifecycleService> RunningServiceList = [];
    private static readonly Dictionary<string, ILifecycleService> ManualServiceMap = [];

    private static void _PushLog(LifecycleLogItem item, ILifecycleLogService service)
    {
        service.OnLog(item.Source, item.Message, item.Ex, item.Level, item.ActionLevel);
    }

    private static void _SavePendingLogs()
    {
        if (PendingLogs.Count == 0) return;
        try
        {
            // 直接写入剩余未输出日志到程序目录
            var path = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName)!, "PCL", "Log", "LastPending.log");
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            foreach (var item in PendingLogs) writer.WriteLine(item);
        }
        catch (Exception ex)
        {
            Console.WriteLine("保存日志时出错，临时输出所有内容到控制台");
            Console.WriteLine(ex);
            foreach (var item in PendingLogs) Console.WriteLine(item);
        }
    }

    private static string _ServiceName(ILifecycleService service, LifecycleState? state = null)
    {
#if DEBUG
        state ??= CurrentState;
        return $"{service.Name} ({state}/{service.Identifier})";
#else
        return service.Name;
#endif
    }

    private static void _StartService(ILifecycleService service, bool manual = false)
    {
        ILifecycleLogService? logService = null;
        // 检测日志服务
        if (service is ILifecycleLogService ls)
        {
            if (_logService != null) throw new InvalidOperationException("日志服务只能有一个");
            logService = ls;
        }
        // 运行服务项并添加到正在运行列表
        var state = manual ? LifecycleState.Manual : CurrentState;
        var name = _ServiceName(service, state);
        try
        {
            var serviceInfo = new LifecycleServiceInfo(service, state);
            Context.Debug($"{name} 启动成功");
            RunningServiceList.AddFirst(service);
            RunningServiceInfoMap[service.Identifier] = serviceInfo;
        }
        catch (Exception ex)
        {
            Context.Warn($"{name} 启动失败，尝试停止", ex);
            _StopService(service, false);
        }
        // 若日志服务已启动则清空日志缓冲
        if (logService == null) return;
        lock (PendingLogs)
        {
            foreach (var item in PendingLogs) _PushLog(item, logService);
            _logService = logService;
        }
    }

    private static ILifecycleService _CreateService(Type type)
    {
        var fullname = type.FullName;
        try
        {
            Context.Trace($"正在实例化 {fullname}");
            var instance = (ILifecycleService)Activator.CreateInstance(type, true)!;
            var supportAsyncText = instance.SupportAsyncStart ? "异步" : "同步";
            Context.Trace($"实例化完成: {instance.Name} ({instance.Identifier}), 启动方式: {supportAsyncText}");
            return instance;
        }
        catch (Exception ex)
        {
            Context.Fatal($"注册服务项实例化失败: {fullname}", ex);
            throw;
        }
    }

    private static void _InitializeAndStartStateServices(Type[] services)
    {
        if (services.Length == 0) return; // 跳过空列表
        var countStart = DateTime.Now; //开始计时
        var asyncInstances = new List<ILifecycleService>();
        // 运行非异步启动服务并存储异步启动服务
        foreach (var service in services)
        {
            var instance = _CreateService(service);
            if (instance.SupportAsyncStart) asyncInstances.Add(instance);
            else _StartService(instance);
        }
        // 运行异步启动服务并等待所有服务启动完成
        var taskList = asyncInstances.Select(
            instance => Task.Run(() => _StartService(instance))).ToArray();
        Task.WaitAll(taskList);
        var count = DateTime.Now - countStart; // 结束计时
        Context.Debug($"状态 {CurrentState} 共用时 {Math.Round(count.TotalMilliseconds)} ms");
    }

    private static bool _isApplicationStarted = false;
    private static bool _requestedExit = false;
    private static ILifecycleService? _requestExitService;
    
    /// <summary>
    /// 在整个程序启动时调用，切勿重复调用
    /// </summary>
    /// <exception cref="InvalidOperationException">尝试重复调用</exception>
    public static void StartInitialize()
    {
        // 检测重复调用
        if (_isApplicationStarted) throw new InvalidOperationException("应用已启动");
        _isApplicationStarted = true;
        // 实例化并存储手动服务
        foreach (var service in LifecycleServiceTypes.GetServiceTypes(LifecycleState.Manual))
        {
            var instance = _CreateService(service);
            var identifier = instance.Identifier;
            if (ManualServiceMap.ContainsKey(identifier))
            {
                Context.Warn($"{_ServiceName(instance, LifecycleState.Manual)} 标识符重复，已跳过");
                continue;
            }
            ManualServiceMap[identifier] = instance;
        }
        // 运行预加载服务
        _InitializeAndStartStateServices(LifecycleServiceTypes.GetServiceTypes(LifecycleState.BeforeLoading));
        if (_requestedExit)
        {
            // 有服务请求退出，开始执行退出流程
            Context.Info($"{_ServiceName(_requestExitService!)} 已请求退出程序");
            _Exit();
            return;
        }
        // 开始运行其他自启动服务
        var index = (int)LifecycleState.Loading;
        const int endIndex = (int)LifecycleState.WindowCreating;
        while (index++ <= endIndex)
        {
            var state = (LifecycleState)index;
            CurrentState = state;
            Context.Debug($"状态改变: {state}");
            _InitializeAndStartStateServices(LifecycleServiceTypes.GetServiceTypes(state));
        }
    }

    private static void _StopService(ILifecycleService service, bool async, bool manual = false)
    {
        var name = _ServiceName(service, manual ? LifecycleState.Manual : CurrentState);
        Context.Trace($"正在停止 {name}");
        if (async) Task.Run(Stop);
        else Stop();
        return;
        
        void Stop()
        {
            try
            {
                service.Stop();
                Context.Debug($"{name} 已停止");
            }
            catch (Exception ex)
            {
                // 若出错直接忽略
                Context.Warn($"停止 {name} 时出错，已跳过", ex);
            }
            RunningServiceInfoMap.Remove(service.Identifier); // 从正在运行列表移除，忽略是否成功
        }
    }

    private static void _Exit()
    {
        Context.Debug("开始退出程序，停止正在运行的服务");
        ILifecycleLogService? logService = null;
        foreach (var service in RunningServiceList.ToArray())
        {
            if (service is ILifecycleLogService ls)
            {
                // 跳过日志服务
                Context.Trace($"已跳过日志服务 {_ServiceName(ls)}");
                logService = ls;
                continue;
            }
            _StopService(service, true);
        }
        if (logService != null)
        {
            Context.Trace($"退出过程已结束，正在停止日志服务");
            _StopService(logService, false);
        }
        _SavePendingLogs();
        Environment.Exit(0);
    }
    
    /// <summary>
    /// 当前的生命周期状态，会随生命周期变化随时更新。
    /// </summary>
    public static LifecycleState CurrentState { get; private set; } = LifecycleState.BeforeLoading;
    
    /// <summary>
    /// 日志服务启动状态
    /// </summary>
    public static bool IsLogServiceStarted => _logService != null;
    
    /// <summary>
    /// 所有正在运行的服务项标识符（即 <see cref="ILifecycleService.Identifier"/> 属性）
    /// </summary>
    public static ICollection<string> RunningServices => RunningServiceInfoMap.Keys;
    
    /// <summary>
    /// 检查指定标识符的服务项是否正在运行
    /// </summary>
    /// <param name="identifier">服务项标识符（即 <see cref="ILifecycleService.Identifier"/> 属性）</param>
    /// <returns>服务项是否正在运行</returns>
    public static bool IsServiceRunning(string identifier) => RunningServiceInfoMap.ContainsKey(identifier);

    /// <summary>
    /// 根据标识符获取正在运行的服务项的相关信息
    /// </summary>
    /// <param name="identifier">服务项标识符（即 <see cref="ILifecycleService.Identifier"/> 属性）</param>
    /// <returns>服务项信息</returns>
    public static LifecycleServiceInfo? GetServiceInfo(string identifier)
    {
        RunningServiceInfoMap.TryGetValue(identifier, out var info);
        return info;
    }

    /// <summary>
    /// 手动请求启动一个周期为 <see cref="LifecycleState.Manual"/> 的服务项。
    /// </summary>
    /// <param name="identifier">服务项标识符（即 <see cref="ILifecycleService.Identifier"/> 属性）</param>
    /// <param name="async">是否异步启动，默认遵循服务项自身的声明</param>
    /// <returns>是否成功请求启动，若该服务项正在运行或周期不是 <see cref="LifecycleState.Manual"/> 则无法启动</returns>
    public static bool StartService(string identifier, bool? async = null)
    {
        ManualServiceMap.TryGetValue(identifier, out var service);
        if (service == null || IsServiceRunning(identifier)) return false;
        async ??= service.SupportAsyncStart;
        if (async == true) Task.Run(() => _StartService(service, true));
        else _StartService(service, true);
        return true;
    }

    /// <summary>
    /// 手动请求停止一个周期为 <see cref="LifecycleState.Manual"/> 的服务项。
    /// </summary>
    /// <param name="identifier">服务项标识符（即 <see cref="ILifecycleService.Identifier"/> 属性）</param>
    /// <param name="async">是否异步停止，默认为 <c>true</c></param>
    /// <returns>是否成功请求停止，若该服务项未运行或周期不是 <see cref="LifecycleState.Manual"/> 则无法停止</returns>
    public static bool StopService(string identifier, bool async = true)
    {
        ManualServiceMap.TryGetValue(identifier, out var service);
        if (service == null || !IsServiceRunning(identifier)) return false;
        _StopService(service, async, true);
        return true;
    }
    
    /// <summary>
    /// 获取指定服务项对应的上下文实例用于日志输出、多任务通信等。一般情况下只推荐获取自身上下文。
    /// </summary>
    /// <param name="self">服务项实例</param>
    /// <returns>上下文实例</returns>
    public static LifecycleContext GetContext(ILifecycleService self) => new(
        service: self,
        onLog: item =>
        {
            lock (PendingLogs)
            {
                if (_logService == null) PendingLogs.Add(item);
                else _PushLog(item, _logService);
            }
        },
        onRequestExit: () =>
        {
            if (CurrentState != LifecycleState.BeforeLoading)
                throw new InvalidOperationException("只能在 BeforeLoading 时请求退出");
            _requestedExit = true;
            _requestExitService = self;
        }
    );
}
