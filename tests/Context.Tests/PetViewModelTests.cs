using CommunityToolkit.Mvvm.Messaging;
using Context.Domain.Entities;
using Context.Presentation.Messages;
using Context.Presentation.ViewModels;
using Context.UseCases.Abstractions;
using Context.UseCases.Services;
using NSubstitute;

namespace Context.Tests;

public class PetViewModelTests
{
    [Fact]
    public void OpenDictionaryCommand_SendsDictionaryRequestedMessage()
    {
        var messenger = new WeakReferenceMessenger();
        var petViewModel = CreatePetViewModel(messenger: messenger);

        DictionaryRequestedMessage? received = null;
        messenger.Register<DictionaryRequestedMessage>(this, (_, message) => received = message);

        petViewModel.OpenDictionaryCommand.Execute(null);

        Assert.NotNull(received);
    }

    [Fact]
    public async Task InitializeCommand_LoadsTopmostFromRepository()
    {
        var settingsRepository = Substitute.For<IAppSettingsRepository>();
        settingsRepository
            .GetAsync(Arg.Any<CancellationToken>())
            .Returns(new AppSettings { IsTopmost = false });
        var petViewModel = CreatePetViewModel(settingsRepository);

        await petViewModel.InitializeCommand.ExecuteAsync(null);

        Assert.False(petViewModel.IsTopmost);
    }

    [Fact]
    public async Task ToggleTopmostCommand_TogglesPropertyAndSaves()
    {
        var settingsRepository = Substitute.For<IAppSettingsRepository>();
        var petViewModel = CreatePetViewModel(settingsRepository);

        await petViewModel.ToggleTopmostCommand.ExecuteAsync(null);

        Assert.False(petViewModel.IsTopmost);
        await settingsRepository.Received(1).SaveAsync(
            Arg.Is<AppSettings>(settings => settings.IsTopmost == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ToggleThemeCommand_TogglesPropertyAndBroadcastsThemeChange()
    {
        var messenger = new WeakReferenceMessenger();
        var petViewModel = CreatePetViewModel(messenger: messenger);

        ThemeChangeRequestedMessage? received = null;
        messenger.Register<ThemeChangeRequestedMessage>(this, (_, message) => received = message);

        petViewModel.ToggleThemeCommand.Execute(null);

        Assert.True(petViewModel.IsDarkTheme);
        Assert.NotNull(received);
        Assert.True(received.IsDark);
    }

    private static PetViewModel CreatePetViewModel(
        IAppSettingsRepository? settingsRepository = null,
        IMessenger? messenger = null)
    {
        var wallpaperRepository = Substitute.For<IWallpaperRepository>();
        var wallpaperSetter = Substitute.For<IDesktopWallpaperSetter>();
        return new PetViewModel(
            messenger ?? new WeakReferenceMessenger(),
            () => new WallpaperViewModel(new WallpaperService(wallpaperRepository, wallpaperSetter)),
            settingsRepository ?? Substitute.For<IAppSettingsRepository>());
    }
}
