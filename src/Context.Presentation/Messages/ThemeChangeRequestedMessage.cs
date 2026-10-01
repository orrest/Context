namespace Context.Presentation.Messages;

/// <summary>请求切换应用主题（由桌宠菜单发出，主视图接收并应用）。</summary>
/// <param name="IsDark">是否切换为暗色主题。</param>
public sealed record ThemeChangeRequestedMessage(bool IsDark);
