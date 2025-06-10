using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using PCL.Core.Utils;

namespace PCL.Core.Service;

/// <summary>
/// 用于 <see cref="RoutedWebServer"/> 响应客户端请求的服务端响应结构。
/// </summary>
public struct RoutedResponse()
{
    /// <summary>
    /// HTTP 状态码
    /// </summary>
    public HttpStatusCode? StatusCode = null;
    
    /// <summary>
    /// 此响应 <see cref="InputStream"/> 使用的字符编码
    /// </summary>
    public Encoding? ContentEncoding = null;
    
    /// <summary>
    /// [Header] 内容类型
    /// </summary>
    public string? ContentType = null;
    
    /// <summary>
    /// [Header] 重定向目标 URL 或路径。
    /// </summary>
    public string? RedirectLocation = null;
    
    /// <summary>
    /// [Header] 是否使用分块传输编码。
    /// </summary>
    public bool? SendChunked = null;
    
    /// <summary>
    /// 随响应添加的 Cookies。
    /// </summary>
    public CookieCollection? Cookies = null;
    
    /// <summary>
    /// 用于传输响应内容的输入流，若非空值，该流将被直接 <c>CopyTo</c> 到实际响应的 <c>OutputStream</c> 中。
    /// </summary>
    public Stream? InputStream = null;

    /// <summary>
    /// 向标准 <see cref="HttpListener"/> 的响应对象写入数据。
    /// </summary>
    /// <param name="target">目标对象</param>
    public void Pour(HttpListenerResponse target)
    {
        target.StatusCode = (int)(StatusCode ?? HttpStatusCode.OK);
        if (ContentType is {} contentType) target.ContentType = contentType;
        if (ContentEncoding is {} contentEncoding) target.ContentEncoding = contentEncoding;
        if (RedirectLocation is {} redirectLocation) target.RedirectLocation = redirectLocation;
        if (SendChunked is {} sendChunked) target.SendChunked = sendChunked;
        if (Cookies is {} cookies) target.Cookies = cookies;
        if (InputStream is {} inputStream) inputStream.CopyTo(target.OutputStream);
    }

    /// <summary>
    /// 默认的 404 Not Found 响应。
    /// </summary>
    public static readonly RoutedResponse NotFound = new() { StatusCode = HttpStatusCode.NotFound };

    /// <summary>
    /// 默认的 204 No Content 响应。
    /// </summary>
    public static readonly RoutedResponse NoContent = new() { StatusCode = HttpStatusCode.NoContent };

    /// <summary>
    /// 响应指定输入流的内容。
    /// </summary>
    /// <param name="stream">输入流</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="encoding">输入流内容使用的字符编码</param>
    public static RoutedResponse Input(Stream stream, string? contentType = null, Encoding? encoding = null) =>
        new() { InputStream = stream, ContentType = contentType ?? "application/octet-stream", ContentEncoding = encoding ?? Encoding.UTF8 };

    /// <summary>
    /// 响应指定文本内容。
    /// </summary>
    /// <param name="text">文本内容</param>
    public static RoutedResponse Text(string text) =>
        new() { InputStream = new StringStream(text), ContentEncoding = Encoding.UTF8 };

    /// <summary>
    /// 响应重定向。
    /// </summary>
    /// <param name="location">重定向目标 URL 或路径</param>
    /// <param name="statusCode">重定向状态码</param>
    public static RoutedResponse Redirect(string location, HttpStatusCode statusCode = HttpStatusCode.Found) =>
        new() { StatusCode = statusCode, RedirectLocation = location };
}

public delegate RoutedResponse RoutedClientRequest(string path, HttpListenerRequest request);

public delegate RoutedResponse RoutedClientRequestWithNothing();

public delegate void RoutedClientRequestWithContext(string path, HttpListenerContext context);

/// <summary>
/// 基于路径路由的 HTTP 服务端。将会按添加顺序匹配路由，从而返回指定访问路径的响应。
/// </summary>
public class RoutedWebServer : WebServer
{
    private readonly LinkedList<string> _pathList = [];
    private readonly Dictionary<string, RoutedClientRequestWithContext> _pathCallbackMap = [];
    
    private void RoutedCallback(HttpListenerContext context)
    {
        var path = context.Request.Url.AbsolutePath;
        var callbackPath = _pathList.FirstOrDefault(p => path.StartsWith(p));
        if (callbackPath == null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }
        var callback = _pathCallbackMap[callbackPath];
        callback(path.Substring(callbackPath.Length), context);
    }
    
    /// <summary>
    /// 创建基于路径路由的 HTTP 服务端实例。
    /// </summary>
    /// <param name="listen"></param>
    public RoutedWebServer(string listen = "127.0.0.1:8080") : base(listen)
    {
        base.SetRequestCallback(RoutedCallback);
    }

    /// <summary>
    /// 以 <see cref="RoutedClientRequestWithContext"/> 回调添加路由
    /// </summary>
    /// <param name="path">路由路径</param>
    /// <param name="callback">回调函数</param>
    /// <returns>是否添加成功，若已存在相同路径则无法添加</returns>
    public bool Route(string path, RoutedClientRequestWithContext callback)
    {
        if (_pathCallbackMap.ContainsKey(path)) return false;
        _pathList.AddLast(path);
        _pathCallbackMap[path] = callback;
        return true;
    }

    /// <summary>
    /// 以 <see cref="RoutedClientRequest"/> 回调添加路由
    /// </summary>
    /// <param name="path">路由路径</param>
    /// <param name="callback">回调函数</param>
    /// <returns>是否添加成功，若已存在相同路径则无法添加</returns>
    public bool Route(string path, RoutedClientRequest callback) => Route(path, (p, context) =>
    {
        var result = callback(p, context.Request);
        result.Pour(context.Response);
    });
    
    /// <summary>
    /// 以 <see cref="RoutedClientRequestWithNothing"/> 回调添加路由
    /// </summary>
    /// <param name="path">路由路径</param>
    /// <param name="callback">回调函数</param>
    /// <returns>是否添加成功，若已存在相同路径则无法添加</returns>
    public bool Route(string path, RoutedClientRequestWithNothing callback) => Route(path, (_, _) => callback());

    public bool RemoveRoutePath(string path)
    {
        if (_pathCallbackMap.ContainsKey(path)) return false;
        _pathList.Remove(path);
        _pathCallbackMap.Remove(path);
        return true;
    }

#pragma warning disable CS0809
    /// <summary>
    /// 请勿调用该方法，而是使用 Route() 添加不同的访问路径。若要调用该方法，请直接使用 <see cref="WebServer"/> 类。
    /// </summary>
    /// <exception cref="NotSupportedException">尝试调用该方法</exception>
    [Obsolete("请勿调用该方法，而是使用 Route() 添加不同的访问路径。", true)]
    public override void SetRequestCallback(WebClientRequest? callback)
    {
        throw new NotSupportedException();
    }
#pragma warning restore CS0809
}
