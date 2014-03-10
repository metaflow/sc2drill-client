using System.Windows;

namespace Overlays.HotKeys
{
    /// <summary>
    /// Interaction logic for HotKeyItemControl.xaml
    /// </summary>
    internal partial class HotKeyItemControl
    {
        public HotKeyItemControl()
        {
            InitializeComponent();
        }

       
        public static readonly DependencyProperty HotKeyTextProperty =
         DependencyProperty.Register("HotKeyText", typeof(string),
         typeof(HotKeyItemControl), new FrameworkPropertyMetadata(string.Empty));
        
       public string HotKeyText
        {
            get { return (string)GetValue(HotKeyTextProperty); }
            set { SetValue(HotKeyTextProperty, value); }
        }
    }
}
