using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Orrest.Ui.Tray;

/// <summary>
/// 把窗口从 Alt+Tab 列表中移除（设置 WS_EX_TOOLWINDOW），
/// 同时清除 APPWINDOW 位，避免任务栏出现按钮。
/// </summary>
public static partial class WindowStyles
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_APPWINDOW = 0x00040000;

    /// <summary>强制 <see cref="Window"/> 携带 WS_EX_TOOLWINDOW 并清除 WS_EX_APPWINDOW。</summary>
    /// <param name="window">要隐藏的目标窗口。</param>
    public static void HideFromAltTab(Window window)
    {
        var hwnd = new WindowInteropHelper(window).EnsureHandle();
        var exStyle = GetWindowLongW(hwnd, GWL_EXSTYLE);
        exStyle |= WS_EX_TOOLWINDOW;
        exStyle &= ~WS_EX_APPWINDOW;
        SetWindowLongW(hwnd, GWL_EXSTYLE, exStyle);
    }

    // 方法名与 user32 导出名一致，避免依赖 EntryPoint；user32 的长/短指针变体
    // 在 64 位进程上由系统自动合并，GWL_EXSTYLE 等 32 位索引值不受影响。
    [LibraryImport("user32.dll")]
    private static partial int GetWindowLongW(IntPtr hWnd, int nIndex);

    [LibraryImport("user32.dll")]
    private static partial int SetWindowLongW(IntPtr hWnd, int nIndex, int dwNewLong);
}
