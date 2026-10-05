using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Orrest.Ui.Hotkey;

/// <summary>
/// 全局热键控制器：经 Win32 <c>RegisterHotKey</c> 把键手势注册为系统级热键——
/// 任意应用处于前台时按下，宿主窗口即收到 WM_HOTKEY（区别于 WPF InputBindings
/// 的应用内快捷键，后者仅在本应用拥有键盘焦点时生效）。
/// 手势经 <see cref="Apply"/> 随时更换（先注销再注册）；空串/无法解析为注销停用；
/// 目标组合已被其他应用占用时注册失败，静默保持停用。
/// </summary>
public sealed partial class GlobalHotkeyController : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const int HotkeyId = 1;
    private const uint ModNorepeat = 0x4000; // 按住不放不连续触发

    private readonly IntPtr _hwnd;
    private readonly HwndSource _source;
    private bool _registered;
    private bool _disposed;

    /// <summary>创建控制器并挂接窗口消息钩子；句柄未创建时立即强制创建。</summary>
    public GlobalHotkeyController(Window window)
    {
        _hwnd = new WindowInteropHelper(window).EnsureHandle();
        _source = HwndSource.FromHwnd(_hwnd) ?? throw new InvalidOperationException("窗口句柄尚未创建。");
        _source.AddHook(WndProc);
    }

    /// <summary>热键按下（系统级触发，与当前前台应用无关）。</summary>
    public event EventHandler? Pressed;

    /// <summary>应用手势：注销旧热键后注册新值；空串/解析失败为停用。</summary>
    public void Apply(string? gestureText)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Unregister();
        if (ParseGesture(gestureText) is not { } gesture)
        {
            return; // 空串/无效手势串静默停用（裸字母键等不受支持组合亦然）
        }

        // ModifierKeys 与 MOD_* 位值一致（Alt=1/Control=2/Shift=4/Win=8）
        _registered = RegisterHotKey(
            _hwnd, HotkeyId,
            (uint)gesture.Modifiers | ModNorepeat,
            (uint)KeyInterop.VirtualKeyFromKey(gesture.Key));
    }

    /// <summary>注销热键并移除窗口消息钩子。</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Unregister();
        _source.RemoveHook(WndProc);
    }

    private void Unregister()
    {
        if (!_registered)
        {
            return;
        }

        UnregisterHotKey(_hwnd, HotkeyId);
        _registered = false;
    }

    /// <summary>解析手势串；空串/无效串返回 null。</summary>
    private static KeyGesture? ParseGesture(string? gestureText)
    {
        if (string.IsNullOrWhiteSpace(gestureText))
        {
            return null;
        }

        try
        {
            return new KeyGestureConverter().ConvertFromString(gestureText) as KeyGesture;
        }
        catch (Exception exception) when (exception is FormatException or NotSupportedException)
        {
            return null;
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            handled = true;
            Pressed?.Invoke(this, EventArgs.Empty);
        }

        return IntPtr.Zero;
    }

    // 与 TrayIconController 同款：static partial 不带 extern，方法名与 DLL 精确导出名一致
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr hWnd, int id);
}
