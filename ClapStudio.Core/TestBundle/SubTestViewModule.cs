using ClapStudio.Interface.Bundles;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace ClapStudio.Core.TestBundle
{
    public class SubTestViewModule(string moduleID, string moduleName, byte r, byte g, byte b) : AViewModule(moduleID, moduleName, "A simple test view", "Subtest")
    {
        private readonly byte m_R = r;
        private readonly byte m_G = g;
        private readonly byte m_B = b;

        public override FrameworkElement CreateView() => new UserControl()
        {
            Background = new SolidColorBrush(Color.FromArgb(255, m_R, m_G, m_B)),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Content = new Border()
            {
                Background = new SolidColorBrush(Color.FromArgb(255, m_R, m_G, m_B)),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            }
        };
    }
}
