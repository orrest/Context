using System.Net.Http;
using Context.Domain.Entities;
using Context.Presentation.ViewModels;
using Context.UseCases.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Context.Tests;

public class DictionaryViewModelTests
{
    private readonly IDictionaryService _dictionaryService = Substitute.For<IDictionaryService>();

    private DictionaryViewModel CreateViewModel()
    {
        return new DictionaryViewModel(_dictionaryService);
    }

    [Fact]
    public async Task SearchCommand_FillsResult()
    {
        _dictionaryService.LookUpAsync("hello", Arg.Any<CancellationToken>())
            .Returns(new WordDefinition { Word = "hello", Meanings = ["interj. 嘿", "n. 表示问候"] });
        var viewModel = CreateViewModel();
        viewModel.SearchText = "hello";

        await viewModel.SearchCommand.ExecuteAsync(null);

        Assert.Equal("hello", viewModel.ResultWord);
        Assert.Equal(2, viewModel.Meanings.Count);
        Assert.Equal("interj. 嘿", viewModel.Meanings[0]);
    }

    [Fact]
    public async Task SearchCommand_TrimsInput()
    {
        var viewModel = CreateViewModel();
        viewModel.SearchText = "  hello  ";

        await viewModel.SearchCommand.ExecuteAsync(null);

        await _dictionaryService.Received(1).LookUpAsync("hello", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchCommand_EmptyInput_DoesNotCall()
    {
        var viewModel = CreateViewModel();
        viewModel.SearchText = "   ";

        await viewModel.SearchCommand.ExecuteAsync(null);

        Assert.Empty(viewModel.Meanings);
        await _dictionaryService.DidNotReceiveWithAnyArgs().LookUpAsync(default!, default);
    }

    [Fact]
    public async Task SearchCommand_NotFound_ShowsHint()
    {
        _dictionaryService.LookUpAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((WordDefinition?)null);
        var viewModel = CreateViewModel();
        viewModel.SearchText = "zzzz";

        await viewModel.SearchCommand.ExecuteAsync(null);

        Assert.Null(viewModel.ResultWord);
        Assert.Equal("未找到该单词的释义。", viewModel.Meanings.Single());
    }

    [Fact]
    public async Task SearchCommand_Failure_ShowsHint()
    {
        _dictionaryService.LookUpAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("网络失败"));
        var viewModel = CreateViewModel();
        viewModel.SearchText = "hello";

        await viewModel.SearchCommand.ExecuteAsync(null);

        Assert.Equal("查询失败，请稍后重试。", viewModel.Meanings.Single());
    }
}
