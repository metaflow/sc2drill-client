using System;
using System.Windows;

namespace Overlays
{
    /// <summary>
    /// Interaction logic for BOItem.xaml
    /// </summary>
    public partial class BOItem
    {
        public enum BOItemState
        {
            None,
            PrepareExecution,
            Coming,
            Current,
            Finished
        }

        public BOItem()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(BOItemState),
            typeof(BOItem), new FrameworkPropertyMetadata(BOItemState.None));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string),
            typeof(BOItem), new FrameworkPropertyMetadata(string.Empty));

        public BOItemState State
        {
            get { return (BOItemState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
    }
}
