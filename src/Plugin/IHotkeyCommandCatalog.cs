namespace Plugin;

/// <summary>快捷键命令目录：聚合所有插件（命令提供者）声明的可绑定命令。</summary>
public interface IHotkeyCommandCatalog
{
    /// <summary>当前已注册插件声明的全部命令。</summary>
    IReadOnlyList<CommandDeclaration> GetCommands();
}
