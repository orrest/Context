using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Context.Domain.Entities;
using Context.Presentation.Messages;
using Context.UseCases.Abstractions;

namespace Context.Presentation.ViewModels;

/// <summary>
/// 设置页「快捷键」Tab 的单行条目：展示命令并录入/清空其键手势，变更即持久化，
/// 并广播 <see cref="HotkeysChangedMessage"/> 令快捷键服务重装窗口绑定。
/// 清空手势 = 删除显式绑定，该命令回退到插件声明的默认手势。
/// </summary>
public partial class HotkeyItemViewModel(
    IMessenger messenger,
    IHotkeyBindingsRepository hotkeyBindingsRepository) : ObservableRecipient(messenger)
{
    /// <summary>命令唯一标识（快捷键绑定按此存储）。</summary>
    [ObservableProperty]
    public partial string CommandId { get; private set; } = string.Empty;

    /// <summary>命令展示名。</summary>
    [ObservableProperty]
    public partial string DisplayName { get; private set; } = string.Empty;

    /// <summary>当前生效键手势（未显式绑定时为命令默认手势；空串表示已清空、待回默认）。</summary>
    [ObservableProperty]
    public partial string KeyGesture { get; set; } = string.Empty;

    public void Initialize(string commandId, string displayName, string keyGesture)
    {
        CommandId = commandId;
        DisplayName = displayName;
        KeyGesture = keyGesture;
    }

    [RelayCommand]
    private async Task ApplyHotkeyAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(KeyGesture))
        {
            await hotkeyBindingsRepository.DeleteAsync(CommandId, cancellationToken);
        }
        else
        {
            await hotkeyBindingsRepository.SaveAsync(
                new HotkeyBinding(CommandId, KeyGesture), cancellationToken);
        }

        Messenger.Send(new HotkeysChangedMessage());
    }
}
