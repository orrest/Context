using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using SkiaSharp;
using SkiaSharp.Skottie;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace Orrest.Ui.Controls;

/// <summary>
/// Lottie 动画播放控件：基于 Skottie（Skia 的 Lottie 实现）在 <see cref="SKElement"/> 上逐帧渲染。
/// <see cref="Source"/> 支持 pack URI（如 pack://application:,,,/Assets/anim.json）、应用内相对资源路径或文件路径。
/// </summary>
public class LottieView : SKElement
{
    private readonly Stopwatch _clock = new();
    private Animation? _animation;
    private bool _renderingHooked;

    /// <summary>
    /// 标识 <see cref="Source"/> 依赖属性。
    /// </summary>
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        nameof(Source), typeof(string), typeof(LottieView), new PropertyMetadata(null, OnSourceChanged));

    /// <summary>
    /// 标识 <see cref="Repeat"/> 依赖属性。
    /// </summary>
    public static readonly DependencyProperty RepeatProperty = DependencyProperty.Register(
        nameof(Repeat), typeof(bool), typeof(LottieView), new PropertyMetadata(true));

    /// <summary>
    /// 标识 <see cref="Zoom"/> 依赖属性。
    /// </summary>
    public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
        nameof(Zoom), typeof(double), typeof(LottieView), new PropertyMetadata(1.0));

    /// <summary>
    /// 初始化 <see cref="LottieView"/>：挂接生命周期与绘制事件。
    /// </summary>
    public LottieView()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        PaintSurface += OnPaintSurface;
    }

    /// <summary>
    /// Lottie JSON 动画来源：pack URI、应用内相对资源路径或文件路径。
    /// </summary>
    public string? Source
    {
        get { return (string?)GetValue(SourceProperty); }
        set { SetValue(SourceProperty, value); }
    }

    /// <summary>
    /// 是否循环播放，默认 true（桌宠动画需要持续循环）。
    /// </summary>
    public bool Repeat
    {
        get { return (bool)GetValue(RepeatProperty); }
        set { SetValue(RepeatProperty, value); }
    }

    /// <summary>
    /// 渲染缩放倍率，默认 1.0（整个动画视口等比适配控件）。
    /// 动画内容仅占视口一小块时（如空白较多的 svg2lottie 导出），调大该值让内容充满控件。
    /// </summary>
    public double Zoom
    {
        get { return (double)GetValue(ZoomProperty); }
        set { SetValue(ZoomProperty, value); }
    }

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((LottieView)d).LoadAnimation();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_animation is null)
        {
            LoadAnimation();
        }

        HookRendering();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        UnhookRendering();
    }

    private void LoadAnimation()
    {
        _animation = null;
        UnhookRendering();

        var source = Source;
        if (string.IsNullOrEmpty(source) || DesignerProperties.GetIsInDesignMode(this))
        {
            return;
        }

        try
        {
            using var stream = OpenSource(source);
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            if (Animation.TryParse(json, out var parsed))
            {
                _animation = parsed;
            }
        }
        catch (Exception)
        {
            // 加载失败保持空白：无效动画不应中断宿主窗口
        }

        if (_animation is not null)
        {
            _clock.Restart();
            HookRendering();
        }
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
            throw new FileNotFoundException($"找不到 Lottie 资源：{source}", source);
        }

        return resource.Stream;
    }

    private void HookRendering()
    {
        if (_renderingHooked || _animation is null)
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
        if (_animation is null)
        {
            return;
        }

        if (!Repeat && _clock.Elapsed >= _animation.Duration)
        {
            UnhookRendering();
        }

        InvalidateVisual();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        if (_animation is null)
        {
            return;
        }

        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var elapsed = _clock.Elapsed.TotalSeconds;
        var duration = _animation.Duration.TotalSeconds;
        var time = Repeat ? elapsed % duration : Math.Min(elapsed, duration);

        var scale = Math.Min(e.Info.Width / _animation.Size.Width, e.Info.Height / _animation.Size.Height) * Zoom;
        var width = (float)(_animation.Size.Width * scale);
        var height = (float)(_animation.Size.Height * scale);
        var destination = new SKRect(
            (e.Info.Width - width) / 2f,
            (e.Info.Height - height) / 2f,
            (e.Info.Width + width) / 2f,
            (e.Info.Height + height) / 2f);

        _animation.SeekFrameTime(time, null);
        _animation.Render(canvas, destination);
    }
}
