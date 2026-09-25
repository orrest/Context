using System.Net;
using Context.Infrastructure.Http;

namespace Context.Tests;

public class BingWallpaperRepositoryTests
{
    private const string ArchiveJson = """
        {
          "images": [
            {
              "startdate": "20260923",
              "url": "/th?id=OHR.Test_ZH-CN123_1920x1080.jpg&rf=LaDigue_1920x1080.jpg&pid=hp",
              "urlbase": "/th?id=OHR.Test_ZH-CN123",
              "copyright": "测试壁纸 (© 摄影师)"
            }
          ]
        }
        """;

    [Fact]
    public async Task GetRecentAsync_BuildsUhdImageUrls_AndThumbnail()
    {
        var repository = CreateRepository(request =>
        {
            Assert.Contains("HPImageArchive.aspx", request.RequestUri!.AbsoluteUri);
            return Json(ArchiveJson);
        });

        var wallpapers = await repository.GetRecentAsync(8);

        var wallpaper = Assert.Single(wallpapers);
        Assert.Equal(new DateOnly(2026, 9, 23), wallpaper.Date);
        Assert.Equal("测试壁纸 (© 摄影师)", wallpaper.Title);
        Assert.Equal("/th?id=OHR.Test_ZH-CN123_UHD.jpg&w=3840&h=2160&rs=1&c=4", wallpaper.ImageUrl);
        Assert.Equal("/th?id=OHR.Test_ZH-CN123_1920x1080.jpg&rf=LaDigue_1920x1080.jpg&pid=hp&w=384&h=216", wallpaper.ThumbnailUrl);
    }

    [Fact]
    public async Task GetRecentAsync_ClampsDaysToApiLimit()
    {
        string? requestedQuery = null;
        var repository = CreateRepository(request =>
        {
            requestedQuery = request.RequestUri!.Query;
            return Json("""{ "images": [] }""");
        });

        var wallpapers = await repository.GetRecentAsync(100);

        Assert.Empty(wallpapers);
        Assert.Contains("n=8", requestedQuery);
    }

    private static BingWallpaperRepository CreateRepository(
        Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var httpClient = new HttpClient(new StubHandler(responder))
        {
            BaseAddress = new Uri("https://cn.bing.com/"),
        };
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("test");
        return new BingWallpaperRepository(httpClient);
    }

    private static HttpResponseMessage Json(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json),
        };
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(responder(request));
        }
    }
}
