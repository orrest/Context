using System.Net;
using Dictionary.Http;

namespace Context.Tests;

public class IcibaDictionaryServiceTests
{
    private const string ValidWordJson = """
        {
          "pageProps": {
            "initialReduxState": {
              "word": {
                "wordInfo": {
                  "bidec": {
                    "word_name": "hello",
                    "parts": [
                      {
                        "part_name": "interj.",
                        "means": [ { "word_mean": "嘿" }, { "word_mean": "你好" } ]
                      },
                      {
                        "part_name": "n.",
                        "means": [ { "word_mean": "表示问候" } ]
                      }
                    ]
                  }
                }
              }
            }
          }
        }
        """;

    private string _currentBuildId = "OLD";
    private int _homePageCallCount;
    private bool _failNextWordRequest;
    private readonly List<string> _requestedWordPaths = [];
    private string _wordJsonResponse = ValidWordJson;

    private IcibaDictionaryService CreateService()
    {
        var httpClient = new HttpClient(new StubHttpHandler(Serve));
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("test");
        httpClient.BaseAddress = new Uri("https://www.iciba.com/");
        return new IcibaDictionaryService(httpClient);
    }

    private HttpResponseMessage Serve(HttpRequestMessage request)
    {
        if (request.RequestUri!.AbsoluteUri.Contains("/_next/data/"))
        {
            _requestedWordPaths.Add(request.RequestUri.PathAndQuery);
            if (_failNextWordRequest)
            {
                _failNextWordRequest = false;
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return request.RequestUri.PathAndQuery.Contains($"/_next/data/{_currentBuildId}/")
                ? Json(_wordJsonResponse)
                : new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        _homePageCallCount++;
        if (_homePageCallCount == 2)
        {
            // 模拟站点在两次访问之间重新部署，buildId 刷新
            _currentBuildId = "NEW";
        }

        return Json(string.Format("{{\"buildId\":\"{0}\"}}", _currentBuildId));
    }

    private static HttpResponseMessage Json(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json),
    };

    [Fact]
    public async Task LookUpAsync_ParsesBidec_ToMeanings()
    {
        var service = CreateService();

        var definition = await service.LookUpAsync("hello");

        Assert.NotNull(definition);
        Assert.Equal("hello", definition.Word);
        Assert.Equal(["interj. 嘿", "interj. 你好", "n. 表示问候"], definition.Meanings);
        Assert.Equal(1, _homePageCallCount);
    }

    [Fact]
    public async Task LookUpAsync_RetriesWithRefreshedBuildId_WhenStale()
    {
        _failNextWordRequest = true;
        var service = CreateService();

        var definition = await service.LookUpAsync("hello");

        Assert.NotNull(definition);
        // 第一次用 OLD 构建ID 请求 404，刷新为 NEW 后重试成功
        Assert.Contains(_requestedWordPaths, p => p.Contains("/_next/data/OLD/word.json"));
        Assert.Contains(_requestedWordPaths, p => p.Contains("/_next/data/NEW/word.json"));
        Assert.Equal(2, _homePageCallCount);
    }

    [Fact]
    public async Task LookUpAsync_ReturnsNull_WhenWordNotFound()
    {
        _wordJsonResponse = """{ "pageProps": { "initialReduxState": { "word": { "wordInfo": {} } } } }""";
        var service = CreateService();

        var definition = await service.LookUpAsync("zzzz");

        Assert.Null(definition);
        Assert.Equal(1, _homePageCallCount);
    }

    [Fact]
    public async Task LookUpAsync_TrimsWord()
    {
        var service = CreateService();

        await service.LookUpAsync("  hello  ");

        Assert.Contains("w=hello", _requestedWordPaths.Single());
    }

    [Fact]
    public async Task LookUpAsync_EmptyWord_ReturnsNull_WithoutHttp()
    {
        var service = CreateService();

        var definition = await service.LookUpAsync("   ");

        Assert.Null(definition);
        Assert.Empty(_requestedWordPaths);
        Assert.Equal(0, _homePageCallCount);
    }
}
