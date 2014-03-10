using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Overlays.HotKeys
{
    /// <summary>
    /// Interaction logic for HotKeyListControl.xaml
    /// </summary>
    internal partial class HotKeyListControl
    {
        public HotKeyListControl()
        {
            InitializeComponent();
        }

        public void SetHotKeys(List<KeyValuePair<string, string>> hotKeys)
        {
            Keys.Children.Clear();
            Separator.Children.Clear();
            Action.Children.Clear();
            
            if (hotKeys == null) return;
            
            foreach (var hotKey in hotKeys)
            {
                var key = new HotKeyItemControl { HotKeyText = hotKey.Key, HorizontalAlignment = HorizontalAlignment.Right };

                var action = new ActionItemControl
                                 {HotKeyAction = hotKey.Value, Width = 200, HorizontalAlignment = HorizontalAlignment.Left};

                var separator = new TextBlock {Text = " - ", VerticalAlignment = VerticalAlignment.Top, Foreground = Brushes.FloralWhite};

                Keys.Children.Add(key);
                Action.Children.Add(action);
                Separator.Children.Add(separator);
            }
        }
    }
}
