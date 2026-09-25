using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Orrest.Ui.Converters;

/// <summary>
/// 值为 null 时折叠元素，否则可见。
/// </summary>
public sealed class NullToCollapsedConverter : IValueConverter
{
    /// <summary>
    /// 值为 null 时返回 <see cref="Visibility.Collapsed"/>，否则返回 <see cref="Visibility.Visible"/>。
    /// </summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is null ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>
    /// 不支持反向转换。
    /// </summary>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
