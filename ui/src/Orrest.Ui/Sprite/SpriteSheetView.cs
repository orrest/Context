using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace Orrest.Ui.Controls;

/// <summary>
/// 精灵图动画控件：横向等宽排布的帧序列图（sprite sheet）在 <see cref="SKElement"/> 上逐帧渲染。
/// 平时静止显示 <see cref="RestFrame"/> 指定帧；调用 <see cref="Play"/> 从第一帧按
/// <see cref="FramesPerSecond"/> 循环播放 <see cref="LoopDuration"/> 秒后回到静帧。
/// 仅播放期间挂接渲染循环，静止时零渲染开销。
/// <see cref="Source"/> 支持 pack URI（如 pack://application:,,,/Assets/sheet.png）、应用内相对资源路径或文件路径。
/// </summary>
public class SpriteSheetView : SKElement
{
    private readonly Stopwatch _clock = new();
    private SKBitmap? _sheet;
    private bool _playing;
    private bool _renderingHooked;

    /// <summary>
    /// 标识 <see cref="Source"/> 依赖属性。
    /// </summary>
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        nameof(Source), typeof(string), typeof(SpriteSheetView), new PropertyMetadata(null, OnSheetChanged));

    /// <summary>
    /// 标识 <see cref="FrameCount"/> 依赖属性。
    /// </summary>
    public static readonly DependencyProperty FrameCountProperty = DependencyProperty.Register(
        nameof(FrameCount), typeof(int), typeof(SpriteSheetView), new PropertyMetadata(1, OnSheetChanged));

    /// <summary>
    /// 标识 <see cref="FramesPerSecond"/> 依赖属性。
    /// </summary>
    public static readonly DependencyProperty FramesPerSecondProperty = DependencyProperty.Register(
        nameof(FramesPerSecond), typeof(double), typeof(SpriteSheetView), new PropertyMetadata(6.0));

    /// <summary>
    /// 标识 <see cref="LoopDuration"/> 依赖属性。
    /// </summary>
    public static readonly DependencyProperty LoopDurationProperty = DependencyProperty.Register(
        nameof(LoopDuration), typeof(double), typeof(SpriteSheetView), new PropertyMetadata(3.0));

    /// <summary>
    /// 标识 <see cref="RestFrame"/> 依赖属性。
    /// </summary>
    public static readonly DependencyProperty RestFrameProperty = DependencyProperty.Register(
        nameof(RestFrame), typeof(int), typeof(SpriteSheetView), new PropertyMetadata(-1));

    /// <summary>
    /// 初始化 <see cref="SpriteSheetView"/>：挂接生命周期与绘制事件。
    /// </summary>
    public SpriteSheetView()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        PaintSurface += OnPaintSurface;
    }

    /// <summary>
    /// 精灵图来源：pack URI、应用内相对资源路径或文件路径。
    /// </summary>
    public string? Source
    {
        get { return (string?)GetValue(SourceProperty); }
        set { SetValue(SourceProperty, value); }
    }

    /// <summary>
    /// 精灵图中的帧数（横向等宽切分），默认 1（整图单帧）。
    /// </summary>
    public int FrameCount
    {
        get { return (int)GetValue(FrameCountProperty); }
        set { SetValue(FrameCountProperty, value); }
    }

    /// <summary>
    /// 播放帧率（帧/秒），默认 6。
    /// </summary>
    public double FramesPerSecond
    {
        get { return (double)GetValue(FramesPerSecondProperty); }
        set { SetValue(FramesPerSecondProperty, value); }
    }

    /// <summary>
    /// 循环播放总时长（秒），默认 3；到时后回到静帧。
    /// </summary>
    public double LoopDuration
    {
        get { return (double)GetValue(LoopDurationProperty); }
        set { SetValue(LoopDurationProperty, value); }
    }

    /// <summary>
    /// 静止显示的帧索引（0 基），默认 -1 表示最后一帧。
    /// </summary>
    public int RestFrame
    {
        get { return (int)GetValue(RestFrameProperty); }
        set { SetValue(RestFrameProperty, value); }
    }

    /// <summary>
    /// 从第一帧按 <see cref="FramesPerSecond"/> 循环播放 <see cref="LoopDuration"/> 秒，
    /// 结束后回到静帧；播放中重复调用则重新开始。
    /// </summary>
    public void Play()
    {
        if (_sheet is null || FrameCount <= 1)
        {
            return;
        }

        _clock.Restart();
        _playing = true;
        HookRendering();
        InvalidateVisual();
    }

    private static void OnSheetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SpriteSheetView)d).LoadSheet();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_sheet is null)
        {
            LoadSheet();
        }

        InvalidateVisual();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _playing = false;
        UnhookRendering();
    }

    private void LoadSheet()
    {
        _sheet = null;
        _playing = false;
        UnhookRendering();

        var source = Source;
        if (string.IsNullOrEmpty(source) || DesignerProperties.GetIsInDesignMode(this))
        {
            return;
        }

        try
        {
            using var stream = OpenSource(source);
            _sheet = SKBitmap.Decode(stream);
        }
        catch (Exception)
        {
            // 加载失败保持空白：无效资源不应中断宿主窗口
        }

        InvalidateVisual();
    }

    private static Stream OpenSource(string source)
    {
        if (File.Exists(source))
        {
            return File.OpenRead(source);
        }

        var uri = new Uri(source, UriKind.RelativeOrAbsolute);
        var resource = Application.GetResourceStream(uri);
        if (resource is null)
        {
            throw new FileNotFoundException($"找不到精灵图资源：{source}", source);
        }

        return resource.Stream;
    }

    private void HookRendering()
    {
        if (_renderingHooked)
        {
            return;
        }

        CompositionTarget.Rendering += OnRendering;
        _renderingHooked = true;
    }

    private void UnhookRendering()
    {
        if (!_renderingHooked)
        {
            return;
        }

        CompositionTarget.Rendering -= OnRendering;
        _renderingHooked = false;
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        if (_playing && _clock.Elapsed.TotalSeconds >= LoopDurationSeconds())
        {
            _playing = false;
            UnhookRendering();
        }

        InvalidateVisual();
    }

    private double LoopDurationSeconds()
    {
        return LoopDuration > 0 ? LoopDuration : 3.0;
    }

    private int CurrentFrame()
    {
        var last = FrameCount - 1;
        if (!_playing)
        {
            var rest = RestFrame;
            return rest < 0 || rest > last ? last : rest;
        }

        var fps = FramesPerSecond > 0 ? FramesPerSecond : 6.0;
        var index = (int)(_clock.Elapsed.TotalSeconds * fps) % FrameCount;
        return Math.Clamp(index, 0, last);
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        if (_sheet is null)
        {
            return;
        }

        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var frame = CurrentFrame();
        var frameWidth = _sheet.Width / FrameCount;
        var source = new SKRect(frame * frameWidth, 0, (frame + 1) * frameWidth, _sheet.Height);

        var scale = Math.Min(e.Info.Width / source.Width, e.Info.Height / source.Height);
        var width = source.Width * scale;
        var height = source.Height * scale;
        var destination = new SKRect(
            (e.Info.Width - width) / 2f,
            (e.Info.Height - height) / 2f,
            (e.Info.Width + width) / 2f,
            (e.Info.Height + height) / 2f);

        // 像素画用最近邻采样：缩放后仍保持像素锐利
        canvas.DrawBitmap(_sheet, source, destination, new SKSamplingOptions(SKFilterMode.Nearest));
    }
}
