namespace Plugin;

/// <summary>
/// 命令提供者标记接口：实现类经 DI 注册为 <c>ICommandProvider</c> 后，
/// 其带 <see cref="HotkeyCommandAttribute"/> 的公共 ICommand 属性会被命令目录扫描收录。
/// </summary>
public interface ICommandProvider
{
}
