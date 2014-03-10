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
    /// Interaction logic for BannerControl.xaml
    /// </summary>
    public partial class BannerControl : UserControl
    {
        public BannerControl()
        {
            InitializeComponent();
        }

        public string ImagePath { get; set; }

        public void SetLogoImage(string logoImagePath)
        {
            var bi = new BitmapImage();
            // BitmapImage.UriSource must be in a BeginInit/EndInit block.

            bi.BeginInit();
            bi.UriSource = new Uri(logoImagePath, UriKind.RelativeOrAbsolute);
            bi.EndInit();

            imageControl.Source = bi;
        }
    }
}
