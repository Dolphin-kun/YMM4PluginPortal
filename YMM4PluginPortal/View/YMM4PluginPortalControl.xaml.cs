using System.Windows;
using System.Windows.Controls;

namespace YMM4PluginPortal.View
{
    /// <summary>
    /// YMM4PluginPortalControl.xaml の相互作用ロジック
    /// </summary>
    public partial class YMM4PluginPortalControl : UserControl
    {
        public YMM4PluginPortalControl()
        {
            InitializeComponent();
        }

        private void YMM4PluginPortalControl_Loaded(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);
            if (parentWindow is not null)
            {
                parentWindow.Title = "YMM4 プラグインポータル";
            }
        }
    }
}
