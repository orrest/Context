using System.Net;
using Dictionary.Http;

namespace Context.Tests;

public class IcibaDictionaryServiceTests
{
    private const string SignSalt = "7ece94d9f9c202b0d2ec557dg4r9bc";

    private const string EnglishWordJson = """
        {
          "status": 1,
          "message": {
            "baesInfo": {
              "word_name": "hello",
              "symbols": [
                {
                  "word_symbol": "helˈəʊ",
                  "parts": [
                    { "part": "int.", "means": ["哈喽，喂", "你好"] },
                    { "part": "n.", "means": ["表示问候"] }
                  ]
                }
              ]
            }
          }
        }
        """;

    private const string ChineseWordJson = """
        {
          "status": 1,
          "message": {
            "baesInfo": {
              "word_name": "世界",
              "symbols": [
                {
                  "word_symbol": "shì jiè",
                  "parts": [
                    { "part": "n.", "means": ["world", "global", "earth"] }
                  ]
                }
              ]
            }
          }
        }
        """;

    private readonly List<string> _requestedPaths = [];
    private string _jsonResponse = EnglishWordJson;

    private IcibaDictionaryService CreateService()
    {
        var httpClient = new HttpClient(new StubHttpHandler(Serve));
        httpClient.BaseAddress = new Uri("https://dict.iciba.com/");
        return new IcibaDictionaryService(httpClient);
    }

    private HttpResponseMessage Serve(HttpRequestMessage request)
    {
        _requestedPaths.Add(request.RequestUri!.PathAndQuery);
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(_jsonResponse),
        };
    }

    [Fact]
    public void ComputeSignature_MatchesServerAcceptedSignature()
    {
        // Signature verified against the live API on 2026-10-08: the request with this
        // signature returned status=1, while tampered ones returned 10001.
        // 签名于 2026-10-08 对线上接口实测通过（status=1），篡改 timestamp/word/签名均返回 10001。
        var source = $"/dictionary/word/query/web610000061791446818094hello{SignSalt}";

        var signature = IcibaDictionaryService.ComputeSignature(source);

        Assert.Equal("27613da2ab7fcffa22920933acb92f71", signature);
    }

    [Fact]
    public async Task LookUpAsync_BuildsSignedRequest_WithTimestampAndWord()
    {
        var service = CreateService();

        await service.LookUpAsync("hello");

        var query = _requestedPaths.Single();
        Assert.StartsWith("/dictionary/word/query/web?", query);
        Assert.Contains("client=6", query);
        Assert.Contains("key=1000006", query);
        var timestamp = Extract(query, "timestamp=");
        var expected = IcibaDictionaryService.ComputeSignature(
            $"/dictionary/word/query/web61000006{timestamp}hello{SignSalt}");
        Assert.Equal(expected, Extract(query, "signature="));
    }

    [Fact]
    public async Task LookUpAsync_ParsesEnglishWord_ToChineseMeanings()
    {
        var service = CreateService();

        var definition = await service.LookUpAsync("hello");

        Assert.NotNull(definition);
        Assert.Equal("hello", definition.Word);
        Assert.Equal(["int. 哈喽，喂", "int. 你好", "n. 表示问候"], definition.Meanings);
    }

    [Fact]
    public async Task LookUpAsync_ParsesChineseWord_ToEnglishMeanings()
    {
        _jsonResponse = ChineseWordJson;
        var service = CreateService();

        var definition = await service.LookUpAsync("世界");

        Assert.NotNull(definition);
        Assert.Equal("世界", definition.Word);
        Assert.Equal(["n. world", "n. global", "n. earth"], definition.Meanings);
    }

    [Fact]
    public async Task LookUpAsync_ReturnsNull_WhenValidationFails()
    {
        _jsonResponse = """{ "message": "校验错误", "status": 10001 }""";
        var service = CreateService();

        var definition = await service.LookUpAsync("hello");

        Assert.Null(definition);
    }

    [Fact]
    public async Task LookUpAsync_ReturnsNull_WhenBaesInfoMissing()
    {
        _jsonResponse = """{ "status": 1, "message": {} }""";
        var service = CreateService();

        var definition = await service.LookUpAsync("zzzz");

        Assert.Null(definition);
    }

    [Fact]
    public async Task LookUpAsync_TrimsWord()
    {
        var service = CreateService();

        await service.LookUpAsync("  hello  ");

        Assert.Contains("word=hello", _requestedPaths.Single());
    }

    [Fact]
    public async Task LookUpAsync_EmptyWord_ReturnsNull_WithoutHttp()
    {
        var service = CreateService();

        var definition = await service.LookUpAsync("   ");

        Assert.Null(definition);
        Assert.Empty(_requestedPaths);
    }

    private static string Extract(string query, string key)
    {
        var start = query.IndexOf(key, StringComparison.Ordinal) + key.Length;
        var end = query.IndexOf('&', start);
        return end < 0 ? query[start..] : query[start..end];
    }
}
