using CommunityToolkit.Mvvm.ComponentModel;

namespace Context.Presentation.ViewModels;

/// <summary>
/// 主视图自身的 ViewModel：通过工厂创建并持有桌宠子 ViewModel，
/// 由 ContentControl + DataTemplate 渲染对应模块视图。
/// </summary>
public sealed class MainViewModel : ObservableObject
{
    public PetViewModel Pet { get; }

    public MainViewModel(Func<PetViewModel> petViewModelFactory)
    {
        Pet = petViewModelFactory();
    }
}
