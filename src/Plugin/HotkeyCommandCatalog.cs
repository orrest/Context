using System.Reflection;
using System.Windows.Input;

namespace Plugin;

/// <summary>
/// 命令目录默认实现：构造时反射扫描所有命令提供者的公共实例 ICommand 属性中
/// 带 <see cref="HotkeyCommandAttribute"/> 者，聚合为声明集合并缓存。
/// 提供者经 DI 注册为 <c>ICommandProvider</c>，由容器多实现收集注入——
/// 新插件注册即自动进入目录，聚合代码零改动。
/// </summary>
public sealed class HotkeyCommandCatalog(IEnumerable<ICommandProvider> commandProviders) : IHotkeyCommandCatalog
{
    private readonly IReadOnlyList<CommandDeclaration> _commands =
    [
        .. commandProviders.SelectMany(Scan)
    ];

    public IReadOnlyList<CommandDeclaration> GetCommands()
    {
        return _commands;
    }

    private static IEnumerable<CommandDeclaration> Scan(ICommandProvider provider)
    {
        var properties = provider.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute<HotkeyCommandAttribute>();
            if (attribute is null || !typeof(ICommand).IsAssignableFrom(property.PropertyType))
            {
                continue;
            }

            if (property.GetValue(provider) is not ICommand command)
            {
                continue;
            }

            yield return new CommandDeclaration(
                attribute.Id, attribute.DisplayName, attribute.DefaultKeyGesture, command);
        }
    }
}
