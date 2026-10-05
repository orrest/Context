using Microsoft.Extensions.DependencyInjection;
using Slime.Views;

namespace Slime;

public static class DependencyInjection
{
    public static IServiceCollection AddSlime(this IServiceCollection services)
    {
        // 桌宠视图：由宿主主窗口（透明占位窗体）构造注入并作为其内容承载
        services.AddTransient<SlimeView>();

        return services;
    }
}
