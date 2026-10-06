using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Context.Domain.Entities;
using Context.Presentation.Messages;
using Context.Presentation.Services;
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
            .Returns(new AppSettings { Theme = "Dark", IsTopmost = false, GlobalHotkey = "Ctrl+Alt+Space" });
        var settingsViewModel = CreateSettingsViewModel(settingsRepository);

        await settingsViewModel.InitializeCommand.ExecuteAsync(null);

        Assert.Equal("Dark", settingsViewModel.Theme);
        Assert.False(settingsViewModel.IsTopmost);
        Assert.Equal("Ctrl+Alt+Space", settingsViewModel.GlobalHotkey);
    }

    [Fact]
    public async Task ApplyCommand_PersistsSettingsAndBroadcastsAppliedMessage()
    {
        var messenger = new WeakReferenceMessenger();
        var settingsRepository = Substitute.For<IAppSettingsRepository>();
        var settingsViewModel = CreateSettingsViewModel(settingsRepository, messenger);
        settingsViewModel.Theme = "Dark";
        settingsViewModel.IsTopmost = false;
        settingsViewModel.GlobalHotkey = "Ctrl+Alt+Space";

        SettingsAppliedMessage? received = null;
        messenger.Register<SettingsAppliedMessage>(this, (_, message) => received = message);

        await settingsViewModel.ApplyCommand.ExecuteAsync(null);

        await settingsRepository.Received(1).SaveAsync(
            Arg.Is<AppSettings>(settings =>
                settings.Theme == "Dark" && !settings.IsTopmost && settings.GlobalHotkey == "Ctrl+Alt+Space"),
            Arg.Any<CancellationToken>());
        Assert.NotNull(received);
        Assert.Equal("Dark", received.Theme);
        Assert.False(received.IsTopmost);
        Assert.Equal("Ctrl+Alt+Space", received.GlobalHotkey);
    }

    [Fact]
    public async Task InitializeCommand_BuildsHotkeyItems_WithBoundOrDefaultGesture()
    {
        var messenger = new WeakReferenceMessenger();
        var catalog = Substitute.For<IPluginCatalog>();
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
            pluginCatalog: catalog,
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

    [Fact]
    public async Task InitializeCommand_BuildsSettingItems_WithPersistedOrDefaultValue()
    {
        var catalog = Substitute.For<IPluginCatalog>();
        catalog.GetSettings().Returns(
        [
            new SettingDeclaration(
                "music.cacheDirectory", "音乐：缓存目录", @"C:\Users\OR\Music", _ => { }),
        ]);
        var pluginSettingsRepository = Substitute.For<IPluginSettingsRepository>();
        pluginSettingsRepository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<PluginSetting> { new("music.cacheDirectory", @"D:\Music") });
        var settingsViewModel = CreateSettingsViewModel(
            pluginCatalog: catalog,
            pluginSettingsRepository: pluginSettingsRepository);

        await settingsViewModel.InitializeCommand.ExecuteAsync(null);

        var item = Assert.Single(settingsViewModel.Settings);
        Assert.Equal("music.cacheDirectory", item.SettingId);
        Assert.Equal("音乐：缓存目录", item.DisplayName);
        Assert.Equal(@"D:\Music", item.Value); // 持久化值优先于声明默认值
    }

    private static SettingsViewModel CreateSettingsViewModel(
        IAppSettingsRepository? settingsRepository = null,
        IMessenger? messenger = null,
        IPluginCatalog? pluginCatalog = null,
        IHotkeyBindingsRepository? hotkeyBindingsRepository = null,
        IPluginSettingsRepository? pluginSettingsRepository = null)
    {
        messenger ??= new WeakReferenceMessenger();
        pluginCatalog ??= Substitute.For<IPluginCatalog>();
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

        if (pluginSettingsRepository is null)
        {
            pluginSettingsRepository = Substitute.For<IPluginSettingsRepository>();
            pluginSettingsRepository
                .GetAllAsync(Arg.Any<CancellationToken>())
                .Returns(new List<PluginSetting>());
        }

        Func<HotkeyItemViewModel> hotkeyItemFactory =
            () => new HotkeyItemViewModel(messenger, hotkeyBindingsRepository);
        var settingsApplier = new PluginSettingsApplier(pluginCatalog, pluginSettingsRepository);
        Func<SettingItemViewModel> settingItemFactory =
            () => new SettingItemViewModel(pluginSettingsRepository, settingsApplier);
        return new SettingsViewModel(
            messenger,
            settingsRepository,
            pluginCatalog,
            hotkeyBindingsRepository,
            pluginSettingsRepository,
            hotkeyItemFactory,
            settingItemFactory);
    }
}
