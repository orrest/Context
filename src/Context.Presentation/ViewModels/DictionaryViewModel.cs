using System.Collections.ObjectModel;
using Context.UseCases.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Context.Presentation.ViewModels;

/// <summary>词典弹窗 ViewModel：输入单词回车查询，展示释义。</summary>
public partial class DictionaryViewModel(IDictionaryService dictionaryService) : ObservableObject
{
    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    /// <summary>结果区的单词名；未查到时为 null（隐藏）。</summary>
    [ObservableProperty]
    public partial string? ResultWord { get; set; }

    public ObservableCollection<string> Meanings { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    public bool IsNotBusy
    {
        get { return !IsBusy; }
    }

    [RelayCommand]
    private async Task SearchAsync(CancellationToken cancellationToken)
    {
        var word = SearchText.Trim();
        if (word.Length == 0 || IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            Meanings.Clear();
            ResultWord = null;
            var definition = await dictionaryService.LookUpAsync(word, cancellationToken);
            if (definition is null)
            {
                Meanings.Add("未找到该单词的释义。");
                return;
            }

            ResultWord = definition.Word;
            foreach (var meaning in definition.Meanings)
            {
                Meanings.Add(meaning);
            }
        }
        catch
        {
            Meanings.Add("查询失败，请稍后重试。");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
