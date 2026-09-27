using Context.Domain.Entities;

namespace Context.UseCases.Abstractions;

public interface IDictionaryService
{
    /// <summary>查询单词释义；单词不存在返回 null。</summary>
    Task<WordDefinition?> LookUpAsync(string word, CancellationToken cancellationToken = default);
}
