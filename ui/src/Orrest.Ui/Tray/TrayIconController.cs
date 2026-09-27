using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace Orrest.Ui.Tray;

/// <summary>
/// 托盘图标控制器：P/Invoke Shell_NotifyIcon 的纯 WPF 实现（不依赖 WinForms）。
/// 回调消息挂在指定窗口的 HWND 上；Explorer 重启（TaskbarCreated 广播）时自动重新注册。
/// 双击触发 <see cref="IconDoubleClicked"/>；右键弹出 <see cref="Menu"/>，菜单项由调用方自行添加。
/// </summary>
public sealed partial class TrayIconController : IDisposable
{
    private const int WM_TRAYICON = 0x8001;        // WM_APP + 1
    private const int WM_LBUTTONDBLCLK = 0x0203;
    private const int WM_RBUTTONUP = 0x0205;
    private const int WM_CONTEXTMENU = 0x007B;
    private const int NIM_ADD = 0x0000;
    private const int NIM_MODIFY = 0x0001;
    private const int NIM_DELETE = 0x0002;
    private const int NIF_MESSAGE = 0x0001;
    private const int NIF_ICON = 0x0002;
    private const int NIF_TIP = 0x0004;

    private readonly Window _window;
    private readonly string _tooltip;
    private uint _taskbarCreatedMessage;
    private HwndSource? _hwndSource;
    private NOTIFYICONDATA _iconData;
    private bool _attached;

    /// <summary>创建托盘图标控制器；图标取自入口程序集，提示文本显示于悬停气泡。</summary>
    /// <param name="window">承载消息钩子的窗口（其 HWND 用于接收回调）。</param>
    /// <param name="tooltip">托盘悬停提示文本。</param>
    public TrayIconController(Window window, string tooltip)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentException.ThrowIfNullOrEmpty(tooltip);
        _window = window;
        _tooltip = tooltip;
        Menu = new ContextMenu();
    }

    /// <summary>托盘右键弹出的菜单；调用方在 <see cref="Attach"/> 前向其添加菜单项。</summary>
    public ContextMenu Menu { get; }

    /// <summary>托盘图标双击。</summary>
    public event EventHandler? IconDoubleClicked;

    /// <summary>在 SourceInitialized 时调用：注册托盘图标与窗口消息钩子。</summary>
    public void Attach()
    {
        if (_attached)
        {
            return;
        }

        var hwnd = new WindowInteropHelper(_window).EnsureHandle();
        _hwndSource = HwndSource.FromHwnd(hwnd) ?? throw new InvalidOperationException("窗口句柄尚未创建。");
        _hwndSource.AddHook(WndProc);
        _taskbarCreatedMessage = RegisterWindowMessageW("TaskbarCreated");

        _iconData = new NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = hwnd,
            uID = 1,
            uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
            uCallbackMessage = WM_TRAYICON,
            hIcon = ExtractIconHandle(),
        };
        InitializeTooltip();
        Shell_NotifyIconW(NIM_ADD, ref _iconData);
        _attached = true;
    }

    /// <summary>把提示文本以 UTF-16 写入 szTip 固定缓冲区（预留结尾 NUL）。</summary>
    private unsafe void InitializeTooltip()
    {
        var tip = _tooltip.AsSpan();
        if (tip.Length > 127)
        {
            tip = tip[..127];
        }

        for (var i = 0; i < tip.Length; i++)
        {
            _iconData.szTip[i] = tip[i];
        }

        _iconData.szTip[tip.Length] = 0;
    }

    /// <summary>窗口关闭时调用：移除托盘图标并清理。</summary>
    public void Detach()
    {
        if (!_attached)
        {
            return;
        }

        _hwndSource?.RemoveHook(WndProc);
        Shell_NotifyIconW(NIM_DELETE, ref _iconData);
        _hwndSource = null;
        _attached = false;
    }

    /// <summary>释放并移除托盘图标。</summary>
    public void Dispose()
    {
        Detach();
    }

    private void OnTrayIconMessage(IntPtr lParam)
    {
        switch ((uint)lParam & 0xFFFF)
        {
            case WM_LBUTTONDBLCLK:
                // 双击序列为 DOWN、UP、DBLCLK、UP：仅在 DBLCLK 触发一次，
                // 单击与双击的后续 UP 均不动作
                IconDoubleClicked?.Invoke(this, EventArgs.Empty);
                break;
            case WM_RBUTTONUP:
            case WM_CONTEXTMENU:
                Menu.IsOpen = true;
                break;
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_TRAYICON)
        {
            handled = true;
            OnTrayIconMessage(lParam);
        }
        else if (_taskbarCreatedMessage != 0 && msg == _taskbarCreatedMessage)
        {
            // Explorer 重启后任务栏重建，重新注册图标
            AddIcon();
        }

        return IntPtr.Zero;
    }

    private void AddIcon()
    {
        Shell_NotifyIconW(NIM_ADD, ref _iconData);
        _attached = true;
    }

    /// <summary>从入口程序集提取图标句柄；失败时回退到系统默认应用图标。</summary>
    private IntPtr ExtractIconHandle()
    {
        var location = Assembly.GetEntryAssembly()?.Location;
        short iconIndex = 0;
        if (!string.IsNullOrEmpty(location))
        {
            var extracted = ExtractAssociatedIconW(GetModuleHandleW(null), location, ref iconIndex);
            if (extracted != IntPtr.Zero)
            {
                return extracted;
            }
        }

        // IDI_APPLICATION = 32512；LoadIconW 返回共享句柄，不可销毁
        return LoadIconW(IntPtr.Zero, new IntPtr(32512));
    }

    // 声明使用 static partial（不带 extern）：本机 SDK 对 extern partial 声明与
    // 生成实现配对存在缺陷（CS0757/CS0179），去掉 extern 后正常。
    // 方法名一律与 DLL 精确导出名一致（*W 后缀）：生成签名 ExactSpelling=true，不做 A/W 自动解析。
    [LibraryImport("shell32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool Shell_NotifyIconW(int dwMessage, ref NOTIFYICONDATA lpData);

    [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr ExtractAssociatedIconW(IntPtr hInst, string iconPath, ref short iconIndex);

    [LibraryImport("user32.dll")]
    private static partial IntPtr LoadIconW(IntPtr hInstance, IntPtr lpIconName);

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr GetModuleHandleW(string? lpModuleName);

    [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    private static partial uint RegisterWindowMessageW(string lpString);

    // 全 blittable 结构（szTip 为 UTF-16 固定缓冲区）：[LibraryImport] 源生成不支持含
    // ByValTStr 字符串字段的结构体，字符串须由调用方手动写入缓冲区。
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct NOTIFYICONDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uID;
        public int uFlags;
        public int uCallbackMessage;
        public IntPtr hIcon;
        public fixed ushort szTip[128];
    }
}
