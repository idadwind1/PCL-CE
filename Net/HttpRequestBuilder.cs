using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using PCL.Core.App;

namespace PCL.Core.Net;

public class HttpRequestBuilder
{
    private readonly HttpRequestMessage _request;
    private HttpResponseMessage? _response;
    private bool _useCookie;
    
    protected HttpRequestBuilder(string url,HttpMethod method)
    {
        this._request = new HttpRequestMessage(method,url);
    }
    /// <summary>
    /// 创建一个 HttpRequestBuilder 对象
    /// </summary>
    /// <param name="url">url</param>
    /// <param name="method">HTTP 方法</param>
    /// <returns>HttpRequestBuilder</returns>
    public static HttpRequestBuilder Create(string url,HttpMethod method)
    {
        return new HttpRequestBuilder(url,method);
    }
    /// <summary>
    /// 设置请求载荷
    /// </summary>
    /// <param name="content">请求载荷</param>
    /// <returns>HttpRequestBuilder</returns>
    public HttpRequestBuilder WithContent(HttpContent content)
    {
        this._request.Content = content;
        return this;
    }
    /// <summary>
    /// 设置请求使用的 Cookie
    /// </summary>
    /// <param name="cookie">Cookie</param>
    /// <returns>HttpRequestBuilder</returns>
    public HttpRequestBuilder WithCookie(string cookie)
    {
        this._useCookie = true;
        this._request.Headers.TryAddWithoutValidation("Cookie",cookie);
        return this;
    }
    /// <summary>
    /// 批量设置 Headers
    /// </summary>
    /// <param name="headers">实现了 IDictionary 的 对象</param>
    /// <returns>HttpRequestBuilder</returns>
    public HttpRequestBuilder SetHeaders(IDictionary<string, string> headers)
    {
        foreach (var kvp in headers)
        {
            this._request.Headers.Add(kvp.Key,kvp.Value);
        }

        return this;
    }
    /// <summary>
    /// 设置单个 Header
    /// </summary>
    /// <param name="key">Header Name</param>
    /// <param name="value">Header Value</param>
    /// <returns>HttpRequestBuilder</returns>
    public HttpRequestBuilder SetHeader(string key, string value)
    {
        if (key.StartsWith("Content", StringComparison.OrdinalIgnoreCase) && this._request.Content is not null)
        {
            this._request.Content.Headers.TryAddWithoutValidation(key, value);
        }
        else
        {
            this._request.Headers.TryAddWithoutValidation(key, value);
        }

        return this;
    }
    /// <summary>
    /// 启动网络请求
    /// </summary>
    /// <returns>HttpRequestBuilder</returns>
    public async Task<HttpRequestBuilder> Build(int? retry = null,Func<int,TimeSpan>? retryPolicy =null)
    {
        using var client = NetworkService.GetClient(this._useCookie);
        this._response = await NetworkService.GetRetryPolicy(retry,retryPolicy)
            .ExecuteAsync(async () => await client.SendAsync(this._request));
        return this;
    }
    /// <summary>
    /// 获取响应的 HttpResponseMessage 对象，如果请求尚未完成，则返回 null
    /// </summary>
    /// <returns>HttpResponseMessage?</returns>
    public HttpResponseMessage? GetResponse()
    {
        return this._response;
    }
    /// <summary>
    /// 读取响应载荷
    /// </summary>
    /// <returns>string?</returns>
    public string? ReadResponseAsString()
    {
        return this.ReadResponseAsStringAsync().GetAwaiter().GetResult();
    }
    /// <summary>
    /// 读取响应载荷 （异步）
    /// </summary>
    /// <returns>string?</returns>
    public async Task<string?> ReadResponseAsStringAsync()
    {
        if (this._response?.Content is not null)
        {
            return await this._response.Content.ReadAsStringAsync();
        }
        

        return null;
    }
    /// <summary>
    /// 读取响应载荷
    /// </summary>
    /// <returns>byte[]?</returns>
    public byte[]? ReadResponseAsByteArray()
    {
        return this.ReadResponseAsByteArrayAsync().GetAwaiter().GetResult();
    }
    /// <summary>
    /// 读取响应载荷（异步）
    /// </summary>
    /// <returns>string?</returns>
    public async Task<byte[]?> ReadResponseAsByteArrayAsync()
    {
        if (this._response?.Content is not null)
        {
            return await this._response.Content.ReadAsByteArrayAsync();
        }

        return null;
    }
    /// <summary>
    /// 读取响应流
    /// </summary>
    /// <returns>string?</returns>
    public Stream? ReadResponseAsStream()
    {
        return this.ReadResponseAsStreamAsync().GetAwaiter().GetResult();
    }
    /// <summary>
    /// 读取响应流（异步）
    /// </summary>
    /// <returns>string?</returns>
    public async Task<Stream?> ReadResponseAsStreamAsync()
    {
        if (this._response?.Content is not null)
        {
            return await this._response.Content.ReadAsStreamAsync();
        }

        return null;
    }
}