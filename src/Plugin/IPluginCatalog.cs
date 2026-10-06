namespace Plugin;

/// <summary>插件目录：聚合所有插件（命令提供者）声明的可绑定命令与设置项。</summary>
public interface IPluginCatalog
{
    /// <summary>当前已注册插件声明的全部命令。</summary>
    IReadOnlyList<CommandDeclaration> GetCommands();

    /// <summary>当前已注册插件声明的全部设置项。</summary>
    IReadOnlyList<SettingDeclaration> GetSettings();
}
