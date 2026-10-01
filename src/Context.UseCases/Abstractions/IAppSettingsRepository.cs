using Context.Domain.Entities;

namespace Context.UseCases.Abstractions;

public interface IAppSettingsRepository
{
    /// <summary>读取应用设置；表为空（首次运行）时返回默认值（置顶开启）。</summary>
    Task<AppSettings> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>保存应用设置（单行表，整体覆盖）。</summary>
    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default);
}
