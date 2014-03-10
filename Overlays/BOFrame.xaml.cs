using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace Overlays
{
    /// <summary>
    /// Interaction logic for BOWindow.xaml
    /// </summary>
    public partial class BOFrame
    {
        private readonly List<BOItem> _steps = new List<BOItem>();
        public int MaxVisibleItems { get; set; }

        public BuildOrderListDisplayMode VisualMode { get; private set; }

        public int CurrentItemIndex { get; set; }

        public BOFrame()
        {
            InitializeComponent();
            MaxVisibleItems = 5;
        }

        public bool IsBOLoaded { get { return _steps.Count > 0; } }

        public void FinishBO()
        {
            Clear();
        }

        private void Clear()
        {
            Visibility = System.Windows.Visibility.Hidden;
            boStack.Children.Clear();
            _steps.Clear();
            CurrentItemIndex = 0;
        }

        public void StartBO(BuildOrderListDisplayMode mode)
        {
            VisualMode = mode;
            Show();
        }

        public int AddBOItem(string text)
        {
            var item = new BOItem
            {
                Message = text,
                State = BOItem.BOItemState.None
            };

            _steps.Add(item);

            return _steps.Count - 1;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            dragDropItem.RoutedElement = this;

            if (Properties.Settings.Default.BOMargin != new Thickness(0))
            {
                Margin = Properties.Settings.Default.BOMargin;
            }
        }

        private void DragDropItem_DragFinished(object sender, EventArgs e)
        {
            Properties.Settings.Default.BOMargin = Margin;
            Properties.Settings.Default.Save();
        }

        private void Show()
        {
            Visibility = (VisualMode == BuildOrderListDisplayMode.None) ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
            if (Visibility != System.Windows.Visibility.Visible) return;
            boStack.Children.Clear();
            switch (VisualMode)
            {
                case BuildOrderListDisplayMode.None:
                    break;
                case BuildOrderListDisplayMode.Full:
                    foreach (BOItem step in _steps)
                    {
                        boStack.Children.Add(step);   
                    }
                    break;
                case BuildOrderListDisplayMode.LimitedList:
                    UpdateLimitedListDisplay();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void  Hide()
        {
            Visibility = System.Windows.Visibility.Hidden;
        }

        private void UpdateDisplay()
        {
            if (VisualMode == BuildOrderListDisplayMode.LimitedList) UpdateLimitedListDisplay();
        }

        private void UpdateLimitedListDisplay()
        {
            if (_steps.Count == 0) return;
            var maxFinishedIndex = -1;
            for (int i = 0; i < _steps.Count; i++)
            {
                if (_steps[i].State != BOItem.BOItemState.Finished) break;
                maxFinishedIndex = i;
            }
            var minIndex = Math.Max(maxFinishedIndex, 0);
            var maxIndex = Math.Min(minIndex + MaxVisibleItems - 1, _steps.Count - 1);
            // update min index once again for case of last items of list
            minIndex = Math.Max(maxIndex - MaxVisibleItems + 1, 0);
            boStack.Children.Clear();
            for (int i = minIndex; i <= maxIndex; i++)
            {
                boStack.Children.Add(_steps[i]);
            }
        }

        public void SetBuildOrderStepState(int index, BOItem.BOItemState state)
        {
            if ((index < 0) || (index >= _steps.Count()))
            {
                Hide();
                return;
            }

            _steps[index].State = state;
            UpdateDisplay();
        }

        public void ProcessMouseMove(MouseEventArgs mouseEventArgs)
        {
            var r = new System.Drawing.Rectangle((int)Margin.Left, (int)Margin.Top, (int)ActualWidth, (int)ActualHeight);
            var v = r.Contains(mouseEventArgs.Location) ? Visibility.Hidden : Visibility.Visible;
            boStack.Visibility = v;
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
