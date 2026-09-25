using System.Windows;
using System.Windows.Controls;

namespace Orrest.Ui.Controls;

/// <summary>
/// 徽标控件：圆角底 + 文本，作为自定义控件（TemplatedControl）的骨架示例。
/// 默认样式位于 Themes/Generic.xaml。
/// </summary>
public class Badge : Control
{
    static Badge()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Badge), new FrameworkPropertyMetadata(typeof(Badge)));
    }

    /// <summary>
    /// 标识 <see cref="Text"/> 依赖属性。
    /// </summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(Badge), new PropertyMetadata(string.Empty));

    /// <summary>
    /// 徽标显示的文本。
    /// </summary>
    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }
}
