using System.Windows;
using System.Windows.Controls;

namespace Overlays.HotKeys
{
    /// <summary>
    /// Interaction logic for ValueItemControl.xaml
    /// </summary>
    internal partial class ActionItemControl
    {
        public ActionItemControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty HotKeyActionProperty =
       DependencyProperty.Register("HotKeyAction", typeof(string),
       typeof(HotKeyItemControl), new FrameworkPropertyMetadata(string.Empty));

        public string HotKeyAction
        {
            get { return (string)GetValue(HotKeyActionProperty); }
            set { SetValue(HotKeyActionProperty, value); }
        }
    }
}
