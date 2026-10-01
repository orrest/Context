using CommunityToolkit.Mvvm.Messaging;
using Context.Domain.Entities;
using Context.Presentation.Messages;
using Context.Presentation.ViewModels;
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

    private static SettingsViewModel CreateSettingsViewModel(
        IAppSettingsRepository? settingsRepository = null,
        IMessenger? messenger = null)
    {
        return new SettingsViewModel(
            messenger ?? new WeakReferenceMessenger(),
            settingsRepository ?? Substitute.For<IAppSettingsRepository>());
    }
}
