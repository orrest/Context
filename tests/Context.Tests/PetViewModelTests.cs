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
        var petViewModel = CreatePetViewModel(messenger);

        DictionaryRequestedMessage? received = null;
        messenger.Register<DictionaryRequestedMessage>(this, (_, message) => received = message);

        petViewModel.OpenDictionaryCommand.Execute(null);

        Assert.NotNull(received);
    }

    private static PetViewModel CreatePetViewModel(IMessenger messenger)
    {
        var wallpaperRepository = Substitute.For<IWallpaperRepository>();
        var wallpaperSetter = Substitute.For<IDesktopWallpaperSetter>();
        return new PetViewModel(
            messenger,
            new WallpaperService(wallpaperRepository, wallpaperSetter));
    }
}
