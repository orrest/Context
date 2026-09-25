using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Context.App;

/// <summary>
/// byte[] 图片内容 → BitmapImage。ConverterParameter 为解码宽度（像素），用于缩略图降采样；
/// 不传参数时按原始分辨率解码（预览图保持全清晰度）。
/// </summary>
public sealed class BytesToImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not byte[] { Length: > 0 } bytes)
        {
            return null;
        }

        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        if (parameter is string widthText && int.TryParse(widthText, out var decodeWidth))
        {
            image.DecodePixelWidth = decodeWidth;
        }
        image.StreamSource = new MemoryStream(bytes);
        image.EndInit();
        image.Freeze();
        return image;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
