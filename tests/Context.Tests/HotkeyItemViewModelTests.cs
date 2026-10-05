using CommunityToolkit.Mvvm.Messaging;
using Context.Domain.Entities;
using Context.Presentation.Messages;
using Context.Presentation.ViewModels;
using Context.UseCases.Abstractions;
using NSubstitute;

namespace Context.Tests;

public class HotkeyItemViewModelTests
{
    [Fact]
    public async Task ApplyHotkeyCommand_NonEmptyGesture_SavesBindingAndBroadcasts()
    {
        var messenger = new WeakReferenceMessenger();
        var repository = Substitute.For<IHotkeyBindingsRepository>();
        var item = new HotkeyItemViewModel(messenger, repository);
        item.Initialize("wallpaper.next", "壁纸：切换下一张", "F8");
        item.KeyGesture = "Ctrl+F9";

        HotkeysChangedMessage? received = null;
        messenger.Register<HotkeysChangedMessage>(this, (_, message) => received = message);

        await item.ApplyHotkeyCommand.ExecuteAsync(null);

        await repository.Received(1).SaveAsync(
            Arg.Is<HotkeyBinding>(binding =>
                binding.CommandId == "wallpaper.next" && binding.KeyGesture == "Ctrl+F9"),
            Arg.Any<CancellationToken>());
        Assert.NotNull(received);
    }

    [Fact]
    public async Task ApplyHotkeyCommand_EmptyGesture_DeletesBindingAndBroadcasts()
    {
        var messenger = new WeakReferenceMessenger();
        var repository = Substitute.For<IHotkeyBindingsRepository>();
        var item = new HotkeyItemViewModel(messenger, repository);
        item.Initialize("wallpaper.next", "壁纸：切换下一张", "F8");
        item.KeyGesture = string.Empty;

        HotkeysChangedMessage? received = null;
        messenger.Register<HotkeysChangedMessage>(this, (_, message) => received = message);

        await item.ApplyHotkeyCommand.ExecuteAsync(null);

        await repository.Received(1).DeleteAsync("wallpaper.next", Arg.Any<CancellationToken>());
        await repository.DidNotReceiveWithAnyArgs().SaveAsync(
            default!, Arg.Any<CancellationToken>());
        Assert.NotNull(received);
    }
}
