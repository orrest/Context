using System.Windows;
using Orrest.Ui.Converters;

namespace Orrest.Ui.Tests;

public class NullToCollapsedConverterTests
{
    private readonly NullToCollapsedConverter _converter = new();

    [Fact]
    public void Convert_Null_ReturnsCollapsed()
    {
        var result = _converter.Convert(null, typeof(Visibility), null, System.Globalization.CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void Convert_NotNull_ReturnsVisible()
    {
        var result = _converter.Convert("value", typeof(Visibility), null, System.Globalization.CultureInfo.InvariantCulture);

        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void ConvertBack_IsNotSupported()
    {
        Assert.Throws<NotSupportedException>(() =>
            _converter.ConvertBack(Visibility.Visible, typeof(object), null, System.Globalization.CultureInfo.InvariantCulture));
    }
}
