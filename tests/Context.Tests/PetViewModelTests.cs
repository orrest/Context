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
        var repository = Substitute.For<IWallpaperRepository>();
        var setter = Substitute.For<IDesktopWallpaperSetter>();
        var petViewModel = new PetViewModel(
            messenger,
            () => new WallpaperViewModel(new WallpaperService(repository, setter)));

        DictionaryRequestedMessage? received = null;
        messenger.Register<DictionaryRequestedMessage>(this, (_, message) => received = message);

        petViewModel.OpenDictionaryCommand.Execute(null);

        Assert.NotNull(received);
    }
}
