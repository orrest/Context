using Dictionary;
using Dictionary.Abstractions;
using Dictionary.Commands;
using Dictionary.Views;
using Plugin;
using Microsoft.Extensions.DependencyInjection;

namespace Context.Tests;

public class DictionaryDependencyInjectionTests
{
    [Fact]
    public void AddDictionary_RegistersResolvableServices()
    {
        var services = new ServiceCollection();
        services.AddDictionary();

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IDictionaryService>());
        // Func 工厂已注册即可（调用会构造 WPF Window，需 STA，不在单测中执行）
        Assert.NotNull(provider.GetRequiredService<Func<DictionaryWindow>>());
        // 命令提供者经标记接口注册（单例，持词典窗口复用状态），供宿主命令目录收集
        Assert.IsType<DictionaryCommands>(provider.GetServices<ICommandProvider>().Single());
    }
}
