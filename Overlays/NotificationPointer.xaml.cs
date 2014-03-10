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
    public partial class NotificationPointer : UserControl
    {
        public NotificationPointer()
        {
            InitializeComponent();
        }
        private void DragDropItem_DragFinished(object sender, EventArgs e)
        {
            if (Tag != null)
                switch (Tag.ToString())
                {
                    case "map":
                        Properties.Settings.Default.MapNotificationMargin = Margin;
                        break;
                    case "resources":
                        Properties.Settings.Default.ResourcesNotificationMargin = Margin;
                        break;
                    case "boost":
                        Properties.Settings.Default.BoostNotificationMargin = Margin;
                        break;
                    case "resourcesCapture":
                        Properties.Settings.Default.ResourcesCaptureMargin = Margin;
                        break;
                }
            Properties.Settings.Default.Save();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dragDropItem.RoutedElement = this;
            var m = new Thickness(0);
            if (Tag != null)
                switch (Tag.ToString())
                {
                    case "map":
                        m = Properties.Settings.Default.MapNotificationMargin;
                        break;
                    case "resources":
                        m = Properties.Settings.Default.ResourcesNotificationMargin;
                        break;
                    case "boost":
                        m = Properties.Settings.Default.BoostNotificationMargin;
                        break;
                    case "resourcesCapture":
                        m = Properties.Settings.Default.ResourcesCaptureMargin;
                        break;
                }
            if (m != new Thickness(0)) Margin = m;
        }

        public void SetImage(BitmapImage bitmapImage)
        {
            notifyImage.Source = bitmapImage;
            notifyImage.Width = bitmapImage.Width;
            notifyImage.Height = bitmapImage.Height;
            containerGrid.Width = notifyImage.Width;
            containerGrid.Height = notifyImage.Height;
            Height = notifyImage.Height;
            Width = notifyImage.Width;
        }

        public void ProcessMouseMove(MouseEventArgs mouseEventArgs)
        {
            var r = new System.Drawing.Rectangle((int)Margin.Left, (int)Margin.Top, (int)ActualWidth, (int)ActualHeight);
            var v = r.Contains(mouseEventArgs.Location) ? Visibility.Hidden : Visibility.Visible;
            notifyImage.Visibility = v;
        }

        private void shyControl(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var o = (UIElement)sender;
            o.Visibility = Visibility.Hidden;
            var w = (MainWindow)Window.GetWindow(o);
            w.DisplayUIElementAfterAWhile(o);
        }
    }
    
}
