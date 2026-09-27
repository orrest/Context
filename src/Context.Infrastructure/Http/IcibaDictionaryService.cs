using System.Text.Json;
using Context.Domain.Entities;
using Context.UseCases.Abstractions;

namespace Context.Infrastructure.Http;

/// <summary>
/// 金山词霸查词（iciba Next.js 数据接口）：
/// 1. 从站点任意页面 HTML 的 __NEXT_DATA__ 中解析当前 buildId；
/// 2. GET /_next/data/{buildId}/word.json?w={word}；
/// 3. 释义取 pageProps.initialReduxState.word.wordInfo.bidec：
///    parts[].part_name（词性）+ parts[].means[].word_mean（释义）。
/// buildId 随站点重新部署变化，缓存在实例内；请求失效（404）时自动刷新一次并重试。
/// </summary>
public sealed class IcibaDictionaryService(HttpClient httpClient) : IDictionaryService
{
    private const string BuildIdMarker = "\"buildId\":\"";
    private string? _buildId;

    public async Task<WordDefinition?> LookUpAsync(string word, CancellationToken cancellationToken = default)
    {
        var trimmed = word.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        if (string.IsNullOrEmpty(_buildId))
        {
            await RefreshBuildIdAsync(cancellationToken);
        }

        try
        {
            return await LookUpCoreAsync(trimmed, cancellationToken);
        }
        catch (HttpRequestException)
        {
            // buildId 随站点部署失效时 word.json 返回 404：刷新一次后重试
            await RefreshBuildIdAsync(cancellationToken);
            return await LookUpCoreAsync(trimmed, cancellationToken);
        }
    }

    private async Task<WordDefinition?> LookUpCoreAsync(string word, CancellationToken cancellationToken)
    {
        var json = await httpClient.GetStringAsync(
            $"/_next/data/{_buildId}/word.json?w={Uri.EscapeDataString(word)}", cancellationToken);
        return Parse(json);
    }

    private async Task RefreshBuildIdAsync(CancellationToken cancellationToken)
    {
        var html = await httpClient.GetStringAsync("/", cancellationToken);
        _buildId = ExtractBuildId(html);
    }

    internal static string? ExtractBuildId(string html)
    {
        var start = html.IndexOf(BuildIdMarker, StringComparison.Ordinal);
        if (start < 0)
        {
            return null;
        }

        start += BuildIdMarker.Length;
        var end = html.IndexOf('"', start);
        return end < 0 ? null : html[start..end];
    }

    internal static WordDefinition? Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (!TryGet(root, "pageProps", out var pageProps) ||
            !TryGet(pageProps, "initialReduxState", out var reduxState) ||
            !TryGet(reduxState, "word", out var word) ||
            !TryGet(word, "wordInfo", out var wordInfo) ||
            !TryGet(wordInfo, "bidec", out var bidec) ||
            bidec.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var wordName = TryGet(bidec, "word_name", out var nameElement)
            ? nameElement.GetString() ?? string.Empty
            : string.Empty;
        var meanings = new List<string>();
        if (TryGet(bidec, "parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
        {
            foreach (var part in parts.EnumerateArray())
            {
                var partName = TryGet(part, "part_name", out var partNameElement)
                    ? partNameElement.GetString()
                    : null;
                if (!TryGet(part, "means", out var means) || means.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var mean in means.EnumerateArray())
                {
                    if (TryGet(mean, "word_mean", out var meanElement) &&
                        meanElement.GetString() is { Length: > 0 } text)
                    {
                        meanings.Add(string.IsNullOrWhiteSpace(partName) ? text : $"{partName} {text}");
                    }
                }
            }
        }

        return new WordDefinition { Word = wordName, Meanings = meanings };
    }

    private static bool TryGet(JsonElement element, string propertyName, out JsonElement value)
    {
        return element.TryGetProperty(propertyName, out value) && value.ValueKind != JsonValueKind.Null;
    }
}
