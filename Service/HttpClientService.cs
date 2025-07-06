using System;
using System.Net.Http;
using System.Threading;
using PCL.Core.Helper;
using PCL.Core.LifecycleManagement;
using PCL.Core.Utils.Net;

namespace PCL.Core.Service;

[LifecycleService(LifecycleState.Loading, Priority = int.MaxValue)]
public class HttpClientService : ILifecycleService
{
    public string Identifier => "network";
    public string Name => "网络服务";
    public bool SupportAsyncStart => true;
    
    private readonly LifecycleContext Context;
    private HttpClientService() { Context = Lifecycle.GetContext(this); }

    private static readonly HttpProxyManager ProxyManager = new();
    private static HttpClientHandler _currentHandler = new()
    {
        UseProxy = true,
        Proxy = ProxyManager
    };
    private static HttpClient _currentClient = new(_currentHandler);
    private static HttpClientHandler? _previousHandler;
    private static HttpClient? _previousClient;
    private static readonly object OperationLock = new();

    public static HttpClient GetClient()
    {
        lock (OperationLock)
        {
            return _currentClient;
        }
    }

    public static void RefreshClient()
    {
        lock (OperationLock)
        {
            _previousClient?.Dispose();
            _previousHandler?.Dispose();
            _previousClient = _currentClient;
            _previousHandler = _currentHandler;
            _currentHandler = new HttpClientHandler();
            _currentClient = new HttpClient(_currentHandler);
        }
    }
    
    private static readonly ManualResetEventSlim StopEvent = new(false);

    public void Start()
    {
        Context.Trace("正在初始化 HttpClient 服务");
        NativeInterop.RunInNewThread(() => {
            while (true)
            {
                if (StopEvent.Wait(TimeSpan.FromHours(4))) break;
                RefreshClient();
            }
        });
    }

    public void Stop()
    {
        lock (OperationLock)
        {
            StopEvent.Set();
            _currentClient.Dispose();
            _currentHandler.Dispose();
            _previousClient?.Dispose();
            _previousHandler?.Dispose();
        }
    }
}
