namespace Plugin;

/// <summary>
/// 命令提供者标记接口：实现类经 DI 注册为 <c>ICommandProvider</c> 后，
/// 其带 <see cref="HotkeyCommandAttribute"/> 的公共 ICommand 属性会被插件目录扫描收录为命令，
/// 带 <see cref="PluginSettingAttribute"/> 的公共可读写 string 属性会被收录为设置项。
/// </summary>
public interface ICommandProvider
{
}
