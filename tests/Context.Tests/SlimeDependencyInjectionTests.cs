using Microsoft.Extensions.DependencyInjection;
using Slime;
using Slime.Views;

namespace Context.Tests;

public class SlimeDependencyInjectionTests
{
    [Fact]
    public void AddSlime_RegistersSlimeView()
    {
        var services = new ServiceCollection();
        services.AddSlime();

        // 桌宠视图是 WPF UserControl（加载 Lottie 资源），不在单测中实例化：
        // 仅断言注册描述，实际构造由宿主主窗口完成
        Assert.Contains(services, d => d.ServiceType == typeof(SlimeView));
    }
}
