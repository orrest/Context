using Dictionary.Entities;

namespace Dictionary.Abstractions;

public interface IDictionaryService
{
    /// <summary>查询单词释义；单词不存在返回 null。</summary>
    Task<WordDefinition?> LookUpAsync(string word, CancellationToken cancellationToken = default);
}
