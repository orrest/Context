using System.Reflection;
using System.Windows.Input;

namespace Plugin;

/// <summary>
/// 插件目录默认实现：构造时反射扫描所有命令提供者的公共实例属性——
/// 带 <see cref="HotkeyCommandAttribute"/> 的 ICommand 属性聚合为命令声明，
/// 带 <see cref="PluginSettingAttribute"/> 的可读写 string 属性聚合为设置声明
/// （属性当前值作为默认值）。
/// 提供者经 DI 注册为 <see cref="ICommandProvider"/>，由容器多实现收集注入——
/// 新插件注册即自动进入目录，聚合代码零改动。
/// </summary>
public sealed class PluginCatalog(IEnumerable<ICommandProvider> commandProviders) : IPluginCatalog
{
    private readonly IReadOnlyList<CommandDeclaration> _commands =
        [.. commandProviders.SelectMany(ScanCommands)];

    private readonly IReadOnlyList<SettingDeclaration> _settings =
        [.. commandProviders.SelectMany(ScanSettings)];

    public IReadOnlyList<CommandDeclaration> GetCommands()
    {
        return _commands;
    }

    public IReadOnlyList<SettingDeclaration> GetSettings()
    {
        return _settings;
    }

    private static IEnumerable<CommandDeclaration> ScanCommands(ICommandProvider provider)
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

    private static IEnumerable<SettingDeclaration> ScanSettings(ICommandProvider provider)
    {
        var properties = provider.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute<PluginSettingAttribute>();
            if (attribute is null || property.PropertyType != typeof(string))
            {
                continue;
            }

            var getter = property.GetGetMethod();
            var setter = property.GetSetMethod();
            if (getter is null || setter is null)
            {
                continue;
            }

            yield return new SettingDeclaration(
                attribute.Id,
                attribute.DisplayName,
                getter.Invoke(provider, null) as string,
                value => setter.Invoke(provider, [value]));
        }
    }
}
