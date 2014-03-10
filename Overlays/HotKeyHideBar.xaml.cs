using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Overlays.Properties;

namespace Overlays
{
    /// <summary>
    /// Interaction logic for HotKeyHideBar.xaml
    /// </summary>
    internal partial class HotKeyHideBar
    {
        private bool isLeftDown;
        private Point startPosition;

        public HotKeyHideBar()
        {
            InitializeComponent();
        }

        public void SetMode(ButtonsOverlayMode mode)
        {
            hotKeys.Visibility = mode == ButtonsOverlayMode.HotKeys ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
            logoImage.Visibility = mode == ButtonsOverlayMode.Banner ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }

        public void SetHotKeys(List<KeyValuePair<string, string>> hotKeys)
        {
            this.hotKeys.SetHotKeys(hotKeys);
        }

        public void SetOverlayTransparency(bool transparent)
        {
            if(transparent)
            {
                logoImage.Opacity = 0.1;
                hotKeys.Opacity = 0.05;
            }
            else
            {
                logoImage.Opacity = 1;
                hotKeys.Opacity = 1;
            }
        }

        private void Polygon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isLeftDown = true;
            startPosition = e.GetPosition(this);
            SizePoligon.CaptureMouse();
        }

        private void Polygon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isLeftDown = false;
            SizePoligon.ReleaseMouseCapture();

            Settings.Default.HideHotkeyWindowWidth = Width;
            Settings.Default.HideHotkeyWindowHeight = Height;
            Settings.Default.Save();
        }

        private void Polygon_MouseMove(object sender, MouseEventArgs e)
        {
            if(isLeftDown)
            {
                const int MinRectangle = 20;

                var delta = startPosition - e.GetPosition(this);

                var newWidth = Width + delta.X;
                var newHeight = Height + delta.Y;

                if (newWidth > MinRectangle) Width = newWidth;
                if (newHeight > MinRectangle) Height = newHeight;
            }
        }

        private void MainGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            SetOverlayTransparency(true);
        }

        private void MainGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            SetOverlayTransparency(false);
        }

        public void SetHotKeyBarLogo(string logoImagePath)
        {
            logoImage.SetLogoImage(logoImagePath);
        }
    }
}
