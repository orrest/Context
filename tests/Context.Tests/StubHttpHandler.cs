using System.Net;

namespace Context.Tests;

/// <summary>按请求响应函数返回结果的 HttpMessageHandler 测试桩。</summary>
internal sealed class StubHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(responder(request));
    }
}
