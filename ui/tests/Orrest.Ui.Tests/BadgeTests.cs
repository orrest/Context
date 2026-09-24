using System.Windows;
using Orrest.Ui.Controls;

namespace Orrest.Ui.Tests;

public class BadgeTests
{
    [Fact]
    public void Text_DefaultsToEmpty()
    {
        var metadata = Badge.TextProperty.GetMetadata(typeof(Badge));

        Assert.Equal(string.Empty, metadata.DefaultValue);
    }

    [Fact]
    public void Text_CanBeSetAndRead()
    {
        // WPF Control 构造函数要求 STA 线程；xUnit 默认线程池是 MTA
        RunInSta(() =>
        {
            var badge = new Badge { Text = "新" };

            Assert.Equal("新", badge.Text);
        });
    }

    [Fact]
    public void TextProperty_IsOwnedByBadge()
    {
        Assert.Equal(typeof(Badge), Badge.TextProperty.OwnerType);
    }

    private static void RunInSta(Action action)
    {
        var thread = new Thread(() => action());
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
