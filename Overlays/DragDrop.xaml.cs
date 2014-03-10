using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Overlays
{
    /// <summary>
    /// Interaction logic for DragDrop.xaml
    /// </summary>
    public partial class DragDrop
    {
        private Point startPosition;

        public static readonly DependencyProperty IsDragInProgressProperty =
       DependencyProperty.Register("IsDragInProgress", typeof(bool),
       typeof(DragDrop), new FrameworkPropertyMetadata(false));

        public bool IsDragInProgress
        {
            get { return (bool)GetValue(IsDragInProgressProperty); }
            set { SetValue(IsDragInProgressProperty, value); }
        }

        public event EventHandler DragFinished;

        public DragDrop()
        {
            InitializeComponent();
        }

        public Control RoutedElement { get; set; }

        private void OnDragFinished()
        {
            if (DragFinished != null) DragFinished(this, EventArgs.Empty);
        }

        private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPosition = e.GetPosition(this);
            IsDragInProgress = true;
            CaptureMouse();
        }

        private void UserControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsDragInProgress = false;
            ReleaseMouseCapture();
            OnDragFinished();
        }

        private void UserControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (RoutedElement == null) return;

            if (IsDragInProgress)
            {
                var delta = startPosition - e.GetPosition(this);

                RoutedElement.Margin = new Thickness(RoutedElement.Margin.Left - delta.X, RoutedElement.Margin.Top - delta.Y, RoutedElement.Margin.Right, RoutedElement.Margin.Bottom);
            }
        }
    }
}
