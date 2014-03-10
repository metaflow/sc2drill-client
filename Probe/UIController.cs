using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Overlays;
using Probe.BuildOrders;
using Probe.Common;
using Probe.Game;
using Probe.Utility;

namespace Probe
{
    public static class UIController
    {
        private static BackgroundWorker _overlayStateController;

        private static MainWindow _uiWindow;
        private static MainWindow uiWindow
        {
            get
            {
                if (_uiWindow == null) _uiWindow = new MainWindow();
                return _uiWindow;
            }
        }

        public static bool IsBoStarted
        {
            get { return uiWindow.IsBoLoaded; }
        }

        /// <summary>
        /// Sets SC2 control panel visibility.
        /// </summary>
        /// <param name="visible">Control panel visibility</param>
        public static void SetOverlayTransparency(bool visible)
        {
            uiWindow.SetOverlayTransparency(visible);
        }

        public static void ShowBuildOrderList(IEnumerable<string> boItems, BuildOrderListDisplayMode displayMode)
        {
            uiWindow.Visibility = Visibility.Visible;
            uiWindow.SetBuildOverDisplayMode(displayMode);

            foreach (var boItem in boItems)
            {
                uiWindow.AddBoItem(boItem);
            }

            uiWindow.Show();
        }

        public static void HideBuildOrder()
        {
            uiWindow.HideBuildOrder();
        }

        public static void ShowHotkeyBar()
        {
            uiWindow.Visibility = Visibility.Visible;
            uiWindow.SetOverlayVisibility(Visibility.Visible);
        }

        public static void HideHotkeyBar()
        {
            uiWindow.SetOverlayVisibility(Visibility.Hidden);
        }

        public static void SetHotkeyBarMode(ButtonsOverlayMode mode)
        {
            uiWindow.SetButtonsOverlayMode(mode);
        }

        /// <summary>
        /// Sets the hot keys displayed on banner.
        /// </summary>
        /// <example> 
        /// { {"ctrl+del","something"}, {"ctrl+shift+f","something else"} };
        /// </example>
        /// <param name="hotKeys">The hot keys. Key - hotkeys combination. Value - action description</param>
        public static void SetHotKeys(List<KeyValuePair<string, string>> pairs)
        {
            uiWindow.SetHotKeys(pairs);
        }

        public static void HideAll()
        {
            CustomEvents.Instance.AddLog("hide all");
            HideBuildOrder();
            HideHotkeyBar();
            HideAllNotifications();
            HideResourceBars();
        }

        private static void HideResourceBars()
        {
            uiWindow.SetResourceBarsVisibility(Visibility.Hidden);
        }

        private static void HideAllNotifications()
        {
            foreach (NotificationType n in Enum.GetValues(typeof (NotificationType)))
            {
                HideNotification(n);
            }
        }

        public static void ShowBuildOrder(BuildOrderListDisplayMode displayMode)
        {
            uiWindow.SetBuildOverDisplayMode(displayMode);
            if (displayMode == BuildOrderListDisplayMode.None) return;
            uiWindow.Visibility = Visibility.Visible;
            uiWindow.Show();
            //if (IsBoStarted) uiWindow.ShowBuildOrder();
        }

        public static void Stop()
        {
            HideAll();
            uiWindow.StopBuildOrder();
        }

        public static void AddBuildOrderStep(BuildOrderStep step)
        {
            var i = uiWindow.AddBoItem(step.Message);
            Debug.Assert(i == step.Index);
        }

        public static void SetBuildOrderItemState(int index, BOItem.BOItemState state)
        {
            uiWindow.SetBuildOrderStepState(index, state);
        }

        public static void ShowStateIndicator()
        {
            HideAll();
            uiWindow.Visibility = Visibility.Visible;
            uiWindow.ShowStateIndicator();
        }

        public static void HideStateIndicator()
        {
            uiWindow.HideStateIndicator();
        }

        public static void SetStateIndicator(ProbeState state)
        {
            uiWindow.SetStateIndicator(state);
        }

        public static void StartOverlaySync()
        {
            if (_overlayStateController != null)
            {
                _overlayStateController.CancelAsync();
            }

            _overlayStateController = new BackgroundWorker();
            _overlayStateController.DoWork += OverlayStateControllerDoWork;

            if (_uiWindow.Visibility != Visibility.Visible) uiWindow.ShowAllOverlays();

            _overlayStateController.RunWorkerAsync(uiWindow.Handle);
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        static void OverlayStateControllerDoWork(object sender, DoWorkEventArgs e)
        {
            if (!(UserSettings.Instance.SyncronizeOverlaysWithGame || UserSettings.Instance.ShutDownWithSc2)) return;

            var isSc2Started = false;

            try
            {
                var overlayHandle = (IntPtr)e.Argument;

                while (!_overlayStateController.CancellationPending)
                {
                    var procs = Process.GetProcessesByName("SC2");

                    if (procs.Length > 0)
                    {
                        isSc2Started = true;

                        if (UserSettings.Instance.SyncronizeOverlaysWithGame)
                        {
                            var sc2Process = procs[0];

                            var sc2Handle = sc2Process.MainWindowHandle;

                            var currentFocused = GetForegroundWindow();

                            if (currentFocused == sc2Handle || currentFocused == overlayHandle)
                            {
                                uiWindow.ShowAllOverlays();
                            }
                            else
                            {
                                uiWindow.HideAllOverlays();
                            }
                        }
                    }
                    else
                    {
                        if (UserSettings.Instance.SyncronizeOverlaysWithGame)
                        {
                            uiWindow.HideAllOverlays();
                        }

                        if(isSc2Started && UserSettings.Instance.ShutDownWithSc2)
                        {
                            CustomEvents.Instance.Add(EventsType.CloseRequest);
                            CustomEvents.Instance.Add(EventsType.Close);
                            return;
                        }
                    }
                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                CustomEvents.Instance.AddException(EventsType.GeneralError, "Overlay synchronization", ex);
                return;
            }
        }

        public static void SetHotKeyBarLogo(string logoImagePath)
        {
            uiWindow.SetHotKeyBarLogo(logoImagePath);
        }

        public static void SetCurrentRace(PlayerRace race)
        {
            uiWindow.SetCurrentRace(race);
        }

        public static void ShowNotification(NotificationType notificationType, Boolean autoHide)
        {
            uiWindow.SetNotificationVisibility(notificationType, Visibility.Visible);
            if (autoHide)
            {
                var e = new GameTimeEvent()
                {
                    EventType = GameTimeEvent.GameTimeEventType.Service,
                    Time = GameClock.Instance.GetGameTime().Add(Constants.UINotificationIconHideTime)
                };
                e.OnEvent += () => uiWindow.SetNotificationVisibility(notificationType, Visibility.Hidden);
                GameTimeEventHandler.Instance.AddEvent(e);   
            }
        }

        public static void HideNotification(NotificationType notificationType)
        {
            uiWindow.SetNotificationVisibility(notificationType, Visibility.Hidden);
        }

        public static void UpdateResourcesState(int minerals, int gas, int currentSupply, int maxSupply)
        {
            if (!GameClock.Instance.Running) return;
            uiWindow.UpdateResourcesState(minerals, gas, currentSupply, maxSupply);
        }

        public static void ShowResourceBars()
        {
            uiWindow.SetResourceBarsVisibility(Visibility.Visible);
        }
        

        public static void ProcessMouseMove(MouseEventArgs mouseEventArgs)
        {
            uiWindow.ProcessMouseMove(mouseEventArgs);
        }

        public static void PreloadUI()
        {
            CustomEvents.Instance.AddLog("preload ui");
            ShowResourceBars();
            ShowHotkeyBar();
            foreach (NotificationType n in Enum.GetValues(typeof(NotificationType)))
            {
                ShowNotification(n, false);
            }
            var t = new System.Timers.Timer() {AutoReset = false, Enabled = false, Interval = 500};
            t.Elapsed += delegate { HideAll(); };
            t.Start();
        }
    }
}
