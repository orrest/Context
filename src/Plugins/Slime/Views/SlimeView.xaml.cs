using System.Windows.Controls;

namespace Slime.Views;

public partial class SlimeView : UserControl
{
    public SlimeView()
    {
        InitializeComponent();
    }

    /// <summary>桌宠动画循环播放一段时长（全局呼出热键触发）。</summary>
    public void Play()
    {
        Animation.Play();
    }
}
