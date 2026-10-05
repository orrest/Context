using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Context.Domain.Entities;
using Context.Presentation.Messages;
using Context.Presentation.ViewModels;
using Plugin;
using Context.UseCases.Abstractions;
using NSubstitute;

namespace Context.Tests;

public class SettingsViewModelTests
{
    [Fact]
    public async Task InitializeCommand_LoadsSettingsFromRepository()
    {
        var settingsRepository = Substitute.For<IAppSettingsRepository>();
        settingsRepository
            .GetAsync(Arg.Any<CancellationToken>())
            .Returns(new AppSettings { Theme = "Dark", IsTopmost = false });
        var settingsViewModel = CreateSettingsViewModel(settingsRepository);

        await settingsViewModel.InitializeCommand.ExecuteAsync(null);

        Assert.Equal("Dark", settingsViewModel.Theme);
        Assert.False(settingsViewModel.IsTopmost);
    }

    [Fact]
    public async Task ApplyCommand_PersistsSettingsAndBroadcastsAppliedMessage()
    {
        var messenger = new WeakReferenceMessenger();
        var settingsRepository = Substitute.For<IAppSettingsRepository>();
        var settingsViewModel = CreateSettingsViewModel(settingsRepository, messenger);
        settingsViewModel.Theme = "Dark";
        settingsViewModel.IsTopmost = false;

        SettingsAppliedMessage? received = null;
        messenger.Register<SettingsAppliedMessage>(this, (_, message) => received = message);

        await settingsViewModel.ApplyCommand.ExecuteAsync(null);

        await settingsRepository.Received(1).SaveAsync(
            Arg.Is<AppSettings>(settings => settings.Theme == "Dark" && !settings.IsTopmost),
            Arg.Any<CancellationToken>());
        Assert.NotNull(received);
        Assert.Equal("Dark", received.Theme);
        Assert.False(received.IsTopmost);
    }

    [Fact]
    public async Task InitializeCommand_BuildsHotkeyItems_WithBoundOrDefaultGesture()
    {
        var messenger = new WeakReferenceMessenger();
        var catalog = Substitute.For<IHotkeyCommandCatalog>();
        catalog.GetCommands().Returns(
        [
            new CommandDeclaration("wallpaper.next", "壁纸：切换下一张", "F8", Substitute.For<ICommand>()),
            new CommandDeclaration("other.command", "其他命令", "F6", Substitute.For<ICommand>()),
        ]);
        var bindingsRepository = Substitute.For<IHotkeyBindingsRepository>();
        bindingsRepository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<HotkeyBinding> { new("wallpaper.next", "Ctrl+F8") });
        var settingsViewModel = CreateSettingsViewModel(
            messenger: messenger,
            hotkeyCommandCatalog: catalog,
            hotkeyBindingsRepository: bindingsRepository);

        await settingsViewModel.InitializeCommand.ExecuteAsync(null);

        Assert.Equal(2, settingsViewModel.Hotkeys.Count);
        var wallpaper = settingsViewModel.Hotkeys[0];
        Assert.Equal("wallpaper.next", wallpaper.CommandId);
        Assert.Equal("壁纸：切换下一张", wallpaper.DisplayName);
        Assert.Equal("Ctrl+F8", wallpaper.KeyGesture); // 显式绑定优先
        var other = settingsViewModel.Hotkeys[1];
        Assert.Equal("F6", other.KeyGesture); // 未绑定回退命令默认手势
    }

    private static SettingsViewModel CreateSettingsViewModel(
        IAppSettingsRepository? settingsRepository = null,
        IMessenger? messenger = null,
        IHotkeyCommandCatalog? hotkeyCommandCatalog = null,
        IHotkeyBindingsRepository? hotkeyBindingsRepository = null)
    {
        messenger ??= new WeakReferenceMessenger();
        hotkeyCommandCatalog ??= Substitute.For<IHotkeyCommandCatalog>();
        if (hotkeyBindingsRepository is null)
        {
            hotkeyBindingsRepository = Substitute.For<IHotkeyBindingsRepository>();
            hotkeyBindingsRepository
                .GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(new List<HotkeyBinding>());
        }

        if (settingsRepository is null)
        {
            settingsRepository = Substitute.For<IAppSettingsRepository>();
            settingsRepository
                .GetAsync(Arg.Any<CancellationToken>())
                .Returns(new AppSettings());
        }

        Func<HotkeyItemViewModel> hotkeyItemFactory =
            () => new HotkeyItemViewModel(messenger, hotkeyBindingsRepository);
        return new SettingsViewModel(
            messenger,
            settingsRepository,
            hotkeyCommandCatalog,
            hotkeyBindingsRepository,
            hotkeyItemFactory);
    }
}
