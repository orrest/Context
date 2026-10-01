using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Orrest.Ui.Interop;

/// <summary>
/// OWindow 的窗口框处理：保留原生窗口框（WindowStyle 原生生效、缩放/Aero Snap/任务栏感知最大化均为原生行为），
/// 仅抹除原生标题栏内容——清除 WS_SYSMENU（去系统菜单与三按钮、图标）、非客户区不绘制标题条、
/// 标题区透明——并启用 DWM 系统圆角（最大化时自动切直角）与暗色模式、背景材质。
/// 机制与 WPF-UI FluentWindow 一致。窗口显示后改变 ResizeMode/WindowStyle 会被 WPF 重写样式位，
/// 届时需重新调用 <see cref="StripNativeTitleBar"/>。
/// </summary>
public static partial class WindowChromeInterop
{
    private const int GWL_STYLE = -16;
    private const int WS_SYSMENU = 0x00080000;

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
    private const int DWMWA_CAPTION_COLOR = 35;

    private const int DWMWCP_ROUND = 2;               // DWM_WINDOW_CORNER_PREFERENCE
    private const uint DWMWA_COLOR_NONE = 0xFFFFFFFE; // 标题区透明

    private const uint WTA_NONCLIENT = 1; // WINDOWTHEMEATTRIBUTETYPE
    private const uint WTNCA_NODRAWCAPTION = 0x00000001;
    private const uint WTNCA_NODRAWICON = 0x00000002;
    private const uint WTNCA_NOSYSMENU = 0x00000004;
    private const uint WTNCA_NOMIRRORHELP = 0x00000008;

    /// <summary>对已创建句柄的窗口抹除原生标题栏内容并启用系统圆角（各 DWM 调用失败均静默忽略，兼容旧系统）。</summary>
    /// <param name="window">句柄已创建（<c>SourceInitialized</c> 之后）的目标窗口。</param>
    public static void StripNativeTitleBar(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        // 清除 WS_SYSMENU：移除系统菜单与原生最小化/最大化/关闭按钮、图标
        var style = GetWindowLongW(hwnd, GWL_STYLE);
        SetWindowLongW(hwnd, GWL_STYLE, style & ~WS_SYSMENU);

        // 非客户区不绘制标题条内容（WS_SYSMENU 清除后残留的空白标题条也不再绘制）
        var options = new WTA_OPTIONS
        {
            dwFlags = WTNCA_NODRAWCAPTION | WTNCA_NODRAWICON | WTNCA_NOSYSMENU | WTNCA_NOMIRRORHELP,
            dwMask = WTNCA_NODRAWCAPTION | WTNCA_NODRAWICON | WTNCA_NOSYSMENU | WTNCA_NOMIRRORHELP,
        };
        _ = SetWindowThemeAttribute(hwnd, WTA_NONCLIENT, ref options, (uint)Marshal.SizeOf<WTA_OPTIONS>());

        // 标题区透明（Win11）
        var colorNone = DWMWA_COLOR_NONE;
        _ = DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref colorNone, sizeof(uint));

        // 系统圆角
        var round = (uint)DWMWCP_ROUND;
        _ = DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref round, sizeof(uint));
    }

    /// <summary>设置 DWM 暗色模式（影响背景材质明暗与系统绘制的边框）。</summary>
    public static void SetDarkMode(Window window, bool dark)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        var value = dark ? 1u : 0u;
        _ = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(uint));
    }

    /// <summary>设置 DWM 系统背景材质（取值即 DWMSBT：1=None、2=Mica、3=Acrylic；Win11 22H2+ 生效，失败静默忽略）。</summary>
    public static void SetBackdropType(Window window, uint backdropType)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        _ = DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(uint));
    }

    // 方法名与 DLL 精确导出名一致（*W 后缀）：生成签名 ExactSpelling=true，不做 A/W 自动解析；
    // user32 的长/短指针变体在 64 位进程上由系统自动合并（同 Tray/WindowStyles.cs 既有约定）。
    [LibraryImport("user32.dll")]
    private static partial int GetWindowLongW(IntPtr hWnd, int nIndex);

    [LibraryImport("user32.dll")]
    private static partial int SetWindowLongW(IntPtr hWnd, int nIndex, int dwNewLong);

    [LibraryImport("dwmapi.dll")]
    private static partial int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref uint value, int size);

    [LibraryImport("uxtheme.dll")]
    private static partial int SetWindowThemeAttribute(IntPtr hwnd, uint attributeType, ref WTA_OPTIONS attribute, uint size);

    [StructLayout(LayoutKind.Sequential)]
    private struct WTA_OPTIONS
    {
        public uint dwFlags;
        public uint dwMask;
    }
}
