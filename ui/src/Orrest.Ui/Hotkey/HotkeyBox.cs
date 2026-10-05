using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Orrest.Ui.Controls;

/// <summary>
/// 快捷键录入框：获得键盘焦点后直接按下组合键即录入键手势（如 "F8"、"Ctrl+Shift+W"），
/// 无需也无法手工键入文本。手势经 <see cref="KeyGesture"/> 实时校验——非法组合
/// （如不带修饰键的字母键）不录入、文本不变；Back/Delete 清空手势（空串，语义为回退默认）；
/// 单独修饰键与 Escape 忽略。
/// 无独立主题字典：构造时以 <see cref="FrameworkElement.SetResourceReference"/> 挂到
/// TextBox 隐式样式（照 OWindow 先例），主题切换自动跟随。
/// </summary>
public class HotkeyBox : TextBox
{
    /// <summary>标识 <see cref="Gesture"/> 依赖属性。</summary>
    public static readonly DependencyProperty GestureProperty = DependencyProperty.Register(
        nameof(Gesture), typeof(string), typeof(HotkeyBox),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnGestureChanged));

    /// <summary>标识 <see cref="Command"/> 依赖属性。</summary>
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(HotkeyBox), new PropertyMetadata(null));

    /// <summary>初始化 HotkeyBox：挂接 TextBox 隐式样式并设为只读（文本仅经按键捕获改变）。</summary>
    public HotkeyBox()
    {
        // 复用 TextBox 隐式样式（隐式样式按精确类型匹配，子类需显式挂引用；
        // DefaultStyleKey 保持 TextBox，主题回退模板亦可用）
        SetResourceReference(StyleProperty, typeof(TextBox));
        IsReadOnly = true;
        IsReadOnlyCaretVisible = false;
    }

    /// <summary>当前键手势（"F8" / "Ctrl+Shift+W"；空串表示未设置，采用命令默认手势）。</summary>
    public string Gesture
    {
        get { return (string)GetValue(GestureProperty); }
        set { SetValue(GestureProperty, value); }
    }

    /// <summary>手势录入或清空后执行的命令（无参，沿用"变更即 Apply"模式）。</summary>
    public ICommand Command
    {
        get { return (ICommand)GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }

    /// <inheritdoc />
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        // Alt 组合键经 Key.System 传递，真实键在 SystemKey
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        // 单独修饰键不构成手势；Escape 放弃本次录入（不清空已有值）
        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
            or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin
            or Key.System or Key.None or Key.Escape)
        {
            e.Handled = true;
            return;
        }

        e.Handled = true;

        // Back/Delete：清空手势（回默认）
        if (key is Key.Back or Key.Delete)
        {
            ApplyGesture(string.Empty);
            return;
        }

        var gestureText = BuildGestureText(key);
        if (gestureText is not null)
        {
            ApplyGesture(gestureText);
        }

        // 非法组合（如裸字母键）：不录入、文本不变
    }

    private static string? BuildGestureText(Key key)
    {
        try
        {
            var gesture = new KeyGesture(key, Keyboard.Modifiers);
            return gesture.GetDisplayStringForCulture(CultureInfo.CurrentCulture);
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }

    private void ApplyGesture(string gestureText)
    {
        Gesture = gestureText;
        var command = Command;
        if (command is not null && command.CanExecute(null))
        {
            command.Execute(null);
        }
    }

    private static void OnGestureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // 绑定从 ViewModel 初始化（加载当前值）时同步显示文本
        var box = (HotkeyBox)d;
        if (!string.Equals(box.Text, box.Gesture, StringComparison.Ordinal))
        {
            box.Text = box.Gesture;
        }
    }
}
