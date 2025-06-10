using System;
using System.Net;
using System.Threading.Tasks;

namespace PCL.Core.Service;

/// <summary>
/// 用于处理 HTTP 客户端请求的委托。
/// </summary>
/// <param name="context">请求上下文，提供对请求和响应对象的访问</param>
public delegate void WebClientRequest(HttpListenerContext context);

/// <summary>
/// HTTP 服务端。
/// </summary>
public class WebServer : IDisposable
{
    private readonly HttpListener _listener = new();

    private WebClientRequest? _requestCallback;
    
    private bool _started = false;
    private bool _running;
    
    private WebClientRequest RequestCallback =>
        _requestCallback?? (ctx => { ctx.Response.StatusCode = (int)HttpStatusCode.NoContent; });

    /// <summary>
    /// 设置接收到客户端请求时触发的事件。
    /// </summary>
    public virtual void SetRequestCallback(WebClientRequest? callback)
    {
        _requestCallback = callback;
    }

    /// <summary>
    /// 初始化 HTTP 服务端实例并开始监听。
    /// </summary>
    /// <param name="listen">监听地址</param>
    /// <param name="requestCallback">客户端请求回调</param>
    public WebServer(string listen = "127.0.0.1:8080", WebClientRequest? requestCallback = null)
    {
        _listener.Prefixes.Add($"http://{listen}/");
        _requestCallback = requestCallback;
        _listener.Start();
    }

    /// <summary>
    /// 开始响应客户端请求。
    /// </summary>
    /// <exception cref="InvalidOperationException">已开始响应，无法重复操作</exception>
    public async Task StartResponse()
    {
        if (_started) throw new InvalidOperationException("Server already started");
        _started = true;

        _running = true;
        while (_running)
        {
            var context = await _listener.GetContextAsync();
            await Task.Run(() =>
            {
                Exception? possibleEx = null;
                try
                {
                    RequestCallback(context);
                }
                catch (Exception e)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    possibleEx = e;
                }
                context.Response.Close();
                if (possibleEx is { } ex) throw ex; // TODO log
            });
        }

        _started = false;
    }

    /// <summary>
    /// 停止响应客户端请求。不会立即停止，而是等待下一次响应结束后停止。
    /// 若要立即停止，请使用 <see cref="Stop"/>，但要注意这会使此实例无法继续使用。
    /// </summary>
    public void StopResponse()
    {
        _running = false;
    }

    /// <summary>
    /// 停止 HTTP 监听服务端。请避免直接调用，而是使用 <see cref="Dispose"/>。
    /// </summary>
    public void Stop() => _listener.Stop();

    /// <summary>
    /// 关闭 HTTP 监听服务端。请避免直接调用，而是使用 <see cref="Dispose"/>。
    /// </summary>
    public void Close() => _listener.Close();

    /// <summary>
    /// 销毁此实例。
    /// </summary>
    public void Dispose()
    {
        Stop();
        Close();
        GC.SuppressFinalize(this);
    }
}
