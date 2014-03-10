using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Probe.Common;
using UserControl = System.Windows.Controls.UserControl;

namespace Overlays
{
    /// <summary>
    /// Interaction logic for NotificationPointer.xaml
    /// </summary>
    public partial class ResourcesState : UserControl
    {
        public ResourcesState()
        {
            InitializeComponent();
        }
        private void DragDropItem_DragFinished(object sender, EventArgs e)
        {
            Properties.Settings.Default.ResourcesCaptureMargin = Margin;
            Properties.Settings.Default.Save();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dragDropItem.RoutedElement = this;
            var m = Properties.Settings.Default.ResourcesCaptureMargin;
            if (m != new Thickness(0)) Margin = m;
        }

        private static List<KeyValuePair<int, int>> _mineralsBarBinding = new List<KeyValuePair<int, int>>()
                                                       {
                                                           new KeyValuePair<int, int>(50,15),
                                                           new KeyValuePair<int, int>(100,32),
                                                           new KeyValuePair<int, int>(150,49),
                                                           new KeyValuePair<int, int>(200,65),
                                                           new KeyValuePair<int, int>(300,98),
                                                           new KeyValuePair<int, int>(400,130),
                                                           new KeyValuePair<int, int>(600,178),
                                                           new KeyValuePair<int, int>(800,225)
                                                       };

        private static List<KeyValuePair<int, int>> _supplyBinding = new List<KeyValuePair<int, int>>()
                                                       {
                                                           new KeyValuePair<int, int>(-200,224),
                                                           new KeyValuePair<int, int>(1,208),
                                                           new KeyValuePair<int, int>(2,192),
                                                           new KeyValuePair<int, int>(3,176),
                                                           new KeyValuePair<int, int>(4,161),
                                                           new KeyValuePair<int, int>(5,143),
                                                           new KeyValuePair<int, int>(6,127),
                                                           new KeyValuePair<int, int>(7,111),
                                                           new KeyValuePair<int, int>(8,93),
                                                           new KeyValuePair<int, int>(16,45),
                                                           new KeyValuePair<int, int>(32,0)
                                                       };

        public void SetResourcesState(int minerals, int gas, int currentSupply, int maxSupply)
        {
#if DEBUG
            label1.Visibility = Visibility.Visible;
            label1.Content = string.Format("M:{0};G:{1};S:{2}/{3}", minerals, gas, currentSupply, maxSupply);
#endif
            var w = 0;
            for (int i = _mineralsBarBinding.Count-1; i >= 0; i--)
            {
                if (minerals < _mineralsBarBinding[i].Key) continue;
                w = _mineralsBarBinding[i].Value;
                break;
            }
            imageMinerals.Width = w;
            w = 0;
            for (int i = _mineralsBarBinding.Count - 1; i >= 0; i--)
            {
                if (gas < _mineralsBarBinding[i].Key) continue;
                w = _mineralsBarBinding[i].Value;
                break;
            }
            imageGas.Width = w;

            var diff = maxSupply - currentSupply;
            w = 0;
            for (int i = _supplyBinding.Count - 1; i >= 0; i--)
            {
                if (diff < _supplyBinding[i].Key) continue;
                w = _supplyBinding[i].Value;
                break;
            }
            imageSupply.Width = w;
        }

        public void ProcessMouseMove(MouseEventArgs mouseEventArgs)
        {
            var r = new System.Drawing.Rectangle((int)Margin.Left, (int)Margin.Top, (int)ActualWidth, (int)ActualHeight);
            var v = r.Contains(mouseEventArgs.Location) ? Visibility.Hidden : Visibility.Visible;
            if (v == imageMinerals.Visibility) return;
            imageMinerals.Visibility = v;
            imageGas.Visibility = v;
            imageSupply.Visibility = v;
            //Debug.Print("r:{0}, m:{1}", r, mouseEventArgs.Location);
        }

        private void shyControl(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var o = (UIElement) sender;
            o.Visibility = Visibility.Hidden;
            var w = (MainWindow)Window.GetWindow(o);
            w.DisplayUIElementAfterAWhile(o);
        }
    }
    
}
