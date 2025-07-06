using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using PCL.Core.Helper;
using PCL.Core.LifecycleManagement;
using PCL.Core.Utils.Logger;
using PCL.Core.Utils.Net;

namespace PCL.Core.Service;

[LifecycleService(LifecycleState.Loading, Priority = int.MaxValue)]
public class HttpClientService : ILifecycleService
{
    public string Identifier => "Network";
    public string Name => "网络服务";
    public bool SupportAsyncStart => true;
    
    private readonly LifecycleContext Context;
    private HttpClientService() { Context = Lifecycle.GetContext(this); }

    private static readonly HttpProxyManager ProxyManager = new HttpProxyManager();
    private static HttpClientHandler CurrentHandler = new HttpClientHandler()
    {
        UseProxy = true,
        Proxy = ProxyManager
    };
    private static HttpClient CurrentClient = new HttpClient(CurrentHandler);
    private static HttpClientHandler? PreviousHandler = null;
    private static HttpClient? PreviousClient = null;
    private static readonly object OperationLock = new object();

    public static HttpClient GetClient()
    {
        lock (OperationLock)
        {
            return CurrentClient;
        }
    }

    public static void RefreshClient()
    {
        lock (OperationLock)
        {
            PreviousClient?.Dispose();
            PreviousHandler?.Dispose();
            PreviousClient = CurrentClient;
            PreviousHandler = CurrentHandler;
            CurrentHandler = new HttpClientHandler();
            CurrentClient = new HttpClient(CurrentHandler);
        }
    }

    public void Start()
    {
        Context.Trace("正在初始化 HttpClient 服务");
        NativeInterop.RunInNewThread(() =>
            {

                while (true)
                {
                    Thread.Sleep(TimeSpan.FromHours(4));
                    RefreshClient();
                }
            }
        );
    }

    public void Stop()
    {
        lock (OperationLock)
        {
            CurrentClient?.Dispose();
            CurrentHandler?.Dispose();
            PreviousClient?.Dispose();
            PreviousHandler?.Dispose();
        }
    }
}