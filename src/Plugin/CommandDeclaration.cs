using System.Windows.Input;

namespace Plugin;

/// <summary>快捷键目录的通用条目：由命令目录从插件声明聚合而来。</summary>
public sealed record CommandDeclaration(string Id, string DisplayName, string DefaultKeyGesture, ICommand Command);
