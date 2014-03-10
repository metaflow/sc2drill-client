using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Overlays.Properties;
using Probe.Common;
using Image = System.Windows.Controls.Image;

namespace Overlays
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public bool IsBoLoaded { get { return boFrame.IsBOLoaded; } }

        public MainWindow()
        {
            InitializeComponent();
        }

        public void SetOverlayVisibility(Visibility value)
        {
            if (!Dispatcher.CheckAccess())
            {
                Action act = () => SetOverlayVisibility(value);
                Dispatcher.Invoke(act);
                return;
            }
            buttonsOverlay.Visibility = value;
        }

        public void SetOverlayTransparency(bool transparent)
        {
            buttonsOverlay.SetOverlayTransparency(transparent);
        }

        public void SetButtonsOverlayMode(ButtonsOverlayMode mode)
        {
            buttonsOverlay.SetMode(mode);
        }

        public void SetBuildOverDisplayMode(BuildOrderListDisplayMode mode = BuildOrderListDisplayMode.LimitedList)
        {
            Action act = () => boFrame.StartBO(mode);
            Dispatcher.Invoke(act);
        }

        public void ShowStateIndicator()
        {
            Action act = () => stateIndicator.Visibility = Visibility.Visible;
            Dispatcher.Invoke(act);
        }

        public void HideStateIndicator()
        {
            Action act = () => stateIndicator.Visibility = Visibility.Hidden;
            Dispatcher.Invoke(act);
        }

        public void SetStateIndicator(ProbeState state)
        {
            Action act = () => stateIndicator.ProbeState = state;
            Dispatcher.Invoke(act);
        }

        public void SetHotKeys(List<KeyValuePair<string, string>> hotKeys)
        {
            buttonsOverlay.SetHotKeys(hotKeys);
        }

        public int AddBoItem(string text)
        {
            return boFrame.AddBOItem(text);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            buttonsOverlay.Width = Settings.Default.HideHotkeyWindowWidth;
            buttonsOverlay.Height = Settings.Default.HideHotkeyWindowHeight;
#if DEBUG
            Topmost = false;
#endif
        }

        public void HideBuildOrder()
        {
            Action act = () => boFrame.Hide();
            Dispatcher.Invoke(act);
        }

        public void StopBuildOrder()
        {
            boFrame.FinishBO();
            HideBuildOrder();
        }

        public void SetBuildOrderStepState(int index, BOItem.BOItemState state)
        {
            Action act = () => boFrame.SetBuildOrderStepState(index, state);
            Dispatcher.Invoke(act);
        }

        public void ShowAllOverlays()
        {
            Action act = () => Visibility = Visibility.Visible;
            Dispatcher.Invoke(act);
        }

        public void HideAllOverlays()
        {
            Action act = () => Visibility = Visibility.Hidden;
            Dispatcher.Invoke(act);
        }

        public IntPtr Handle
        {
            get
            {
                Func<IntPtr> act = () => { return new WindowInteropHelper(this).Handle; };
                
                return (IntPtr)Dispatcher.Invoke(act);
            }
        }

        public void SetHotKeyBarLogo(string logoImagePath)
        {
            Action act = () =>  buttonsOverlay.SetHotKeyBarLogo(logoImagePath);
             
            Dispatcher.Invoke(act);
        }

        public void SetCurrentRace(PlayerRace race)
        {
            if (!Dispatcher.CheckAccess())
            {
                Action act = () => SetCurrentRace(race);
                Dispatcher.Invoke(act);
                return;
            }
            string postfix = "protoss";
            switch (race)
            {
                case PlayerRace.Terran:
                    postfix = "terran";
                    break;
                case PlayerRace.Zerg:
                    postfix = "zerg";
                    break;
            }
            boostNotification.SetImage(new BitmapImage(new Uri(String.Format("/Overlays;component/Images/notify_boost_{0}.gif", postfix), UriKind.Relative)));
        }

        public void SetNotificationVisibility(NotificationType notificationType, Visibility visibility)
        {
            if (!Dispatcher.CheckAccess())
            {
                Action act = () => SetNotificationVisibility(notificationType, visibility);
                Dispatcher.Invoke(act);
                return;
            }

            switch (notificationType)
            {
                case NotificationType.Map:
                    mapNotification.Visibility = visibility;
                    break;
                case NotificationType.Resources:
                    resourcesNotification.Visibility = visibility;
                    break;
                case NotificationType.Boost:
                    boostNotification.Visibility = visibility;
                    break;
            }
        }

        public void UpdateResourcesState(int minerals, int gas, int currentSupply, int maxSupply)
        {
            if (!Dispatcher.CheckAccess())
            {
                Action act = () => UpdateResourcesState(minerals,gas,currentSupply,maxSupply);
                Dispatcher.Invoke(act);
                return;
            }
            if (!resourcesState.IsVisible) return;
            resourcesState.SetResourcesState(minerals, gas, currentSupply, maxSupply);
        }

        public void SetResourceBarsVisibility(Visibility visibility)
        {
            if (!Dispatcher.CheckAccess())
            {
                Action act = () => SetResourceBarsVisibility(visibility);
                Dispatcher.Invoke(act);
                return;
            }
            resourcesState.Visibility = visibility;
        }

        public void ProcessMouseMove(MouseEventArgs mouseEventArgs)
        {
            /*
            if (resourcesState.Visibility == Visibility.Visible) resourcesState.ProcessMouseMove(mouseEventArgs);
            if (mapNotification.Visibility == Visibility.Visible) mapNotification.ProcessMouseMove(mouseEventArgs);
            if (resourcesNotification.Visibility == Visibility.Visible) resourcesNotification.ProcessMouseMove(mouseEventArgs);
            if (boostNotification.Visibility == Visibility.Visible) boostNotification.ProcessMouseMove(mouseEventArgs);
             * 
            if (boFrame.Visibility == Visibility.Visible) boFrame.ProcessMouseMove(mouseEventArgs);*/
        }

        public void DisplayUIElementAfterAWhile(UIElement uiElement)
        {
            var t = new System.Timers.Timer() {AutoReset = false, Enabled =  false, Interval = 2000};
            t.Elapsed += delegate { SetEvementVisibility(uiElement, Visibility.Visible); };
            t.Start();
        }

        private void SetEvementVisibility(UIElement uiElement, Visibility visibility)
        {
            if (!Dispatcher.CheckAccess())
            {
                Action act = () => SetEvementVisibility(uiElement, visibility);
                Dispatcher.Invoke(act);
                return;
            }
            uiElement.Visibility = visibility;
        }
    }
}
