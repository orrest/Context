using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Dictionary.Entities;
using Dictionary.Abstractions;

namespace Dictionary.Http;

// dict.iciba.com signed lookup API, shared by EN-to-CN and CN-to-EN queries:
// both directions return gloss pairs in message.baesInfo.symbols[].parts[].
// dict.iciba.com 签名查词接口，英译中与中译英共用：
// 两个方向的词性释义都在 message.baesInfo.symbols[].parts[] 中，解析路径一致。
public sealed class IcibaDictionaryService : IDictionaryService
{
    private const string QueryPath = "/dictionary/word/query/web";
    private const string ClientId = "6";
    private const string Key = "1000006";
    // Public constant shipped inside the iciba web client, paired with key 1000006.
    // 官网 Web 端内置的公开常量，与 key 1000006 配对使用。
    private const string SignSalt = "7ece94d9f9c202b0d2ec557dg4r9bc";

    private readonly HttpClient _httpClient;

    public IcibaDictionaryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WordDefinition?> LookUpAsync(string word, CancellationToken cancellationToken = default)
    {
        var trimmed = word.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var signature = ComputeSignature($"{QueryPath}{ClientId}{Key}{timestamp}{trimmed}{SignSalt}");
        // Do NOT append transSensitiveCheck like the official client: the server then includes
        // it in signature validation and our signature would always fail. Verified live 2026-10-08.
        // 不要像官方客户端那样追加 transSensitiveCheck：服务端会将其纳入签名校验，
        // 我方签名未包含该值将必然校验失败（2026-10-08 线上实测）。
        var requestUri = $"{QueryPath}?client={ClientId}&key={Key}&timestamp={timestamp}" +
                         $"&word={Uri.EscapeDataString(trimmed)}&signature={signature}";
        var json = await _httpClient.GetStringAsync(requestUri, cancellationToken);
        return Parse(json);
    }

    internal static string ComputeSignature(string source)
    {
        // Server validates lowercase hex MD5 over UTF-8 bytes of path + client + key + timestamp + word + salt.
        // 服务端校验「路径 + client + key + timestamp + 原词 + 盐值」的 UTF-8 字节的小写十六进制 MD5。
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    internal static WordDefinition? Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        // On validation errors the API returns message as a plain string, not an object.
        // 校验失败时接口返回的 message 是普通字符串而非对象。
        if (!TryGet(root, "message", out var message) ||
            message.ValueKind != JsonValueKind.Object ||
            !TryGet(message, "baesInfo", out var baesInfo) ||
            baesInfo.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var wordName = TryGet(baesInfo, "word_name", out var nameElement)
            ? nameElement.GetString() ?? string.Empty
            : string.Empty;
        var meanings = new List<string>();
        if (TryGet(baesInfo, "symbols", out var symbols) && symbols.ValueKind == JsonValueKind.Array)
        {
            foreach (var symbol in symbols.EnumerateArray())
            {
                if (!TryGet(symbol, "parts", out var parts) || parts.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var part in parts.EnumerateArray())
                {
                    var partName = TryGet(part, "part", out var partElement)
                        ? partElement.GetString()
                        : null;
                    if (!TryGet(part, "means", out var means) || means.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (var mean in means.EnumerateArray())
                    {
                        // means are plain strings here, unlike the object array of the legacy word.json API.
                        // 此接口的 means 是字符串数组，区别于旧 word.json 接口的对象数组。
                        if (mean.ValueKind == JsonValueKind.String &&
                            mean.GetString() is { Length: > 0 } text)
                        {
                            meanings.Add(string.IsNullOrWhiteSpace(partName) ? text : $"{partName} {text}");
                        }
                    }
                }
            }
        }

        if (meanings.Count == 0)
        {
            return null;
        }

        return new WordDefinition { Word = wordName, Meanings = meanings };
    }

    private static bool TryGet(JsonElement element, string propertyName, out JsonElement value)
    {
        return element.TryGetProperty(propertyName, out value) && value.ValueKind != JsonValueKind.Null;
    }
}
