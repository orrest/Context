using Context.Domain.Entities;

namespace Context.UseCases.Abstractions;

public interface IHotkeyBindingsRepository
{
    /// <summary>读取全部显式绑定；未配置的命令视为采用其声明默认手势。</summary>
    Task<IReadOnlyList<HotkeyBinding>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>保存绑定（按 CommandId 整体覆盖）。</summary>
    Task SaveAsync(HotkeyBinding binding, CancellationToken cancellationToken = default);

    /// <summary>删除绑定（该命令回退到声明默认手势）。</summary>
    Task DeleteAsync(string commandId, CancellationToken cancellationToken = default);
}
