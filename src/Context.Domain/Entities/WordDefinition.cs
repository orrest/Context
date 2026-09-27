namespace Context.Domain.Entities;

/// <summary>词典查词结果：单词与各词性下的释义。</summary>
public sealed class WordDefinition
{
    /// <summary>查询单词。</summary>
    public required string Word { get; set; }

    /// <summary>释义列表（含词性前缀，如 "interj. 嘿"）。</summary>
    public required IReadOnlyList<string> Meanings { get; set; }
}
