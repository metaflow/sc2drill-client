using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Overlays
{
    /// <summary>
    /// Interaction logic for ProbeStateControl.xaml
    /// </summary>
    public partial class ProbeStateControl : UserControl
    {
        public ProbeStateControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ProbeStateProperty =
         DependencyProperty.Register("ProbeState", typeof(ProbeState),
         typeof(ProbeStateControl), new FrameworkPropertyMetadata(ProbeState.NotReady));

        public ProbeState ProbeState
        {
            get { return (ProbeState)GetValue(ProbeStateProperty); }
            set { SetValue(ProbeStateProperty, value); }
        }
    }
}
