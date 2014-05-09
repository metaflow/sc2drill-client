using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using ImageProcessing;
using Newtonsoft.Json.Linq;
using Overlays;
using Probe.BuildOrders;
using Probe.Common;
using Probe.Estimators;
using Probe.Game;
using Probe.Replay;
using Probe.Utility;
using Probe.WebClient;

namespace Probe.Engine
{
    public class Controller : IKeyCombinationListener
    {
        #region singleton
        private static readonly Controller _instance = new Controller();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Controller()
        {
        }

        public static Controller Instance
        {
            get
            {
                return _instance;
            }
        }
        #endregion

        private AsyncTaskProcessor _tasks = new AsyncTaskProcessor();

        private TapWatcher _tapWatcher;

        //private RightClickWatcher _rightClickWatcher = new RightClickWatcher();
        private KeyEstimator _mapKeyEstimator = new KeyEstimator();
        private KeyEstimator _resourcesKeyEstimator = new KeyEstimator();

        private ReplayFileWatcher replayFileWatcher = new ReplayFileWatcher();
        private EventFileWatcher eventFileWatcher = new EventFileWatcher();
        private RaceDetector raceDetector = new RaceDetector();
        private BoostNotifier boostNotifier = new BoostNotifier();
        private bool GameDetected { get; set; }

        private enum ControllerKeyEvents
        {
            StateToggle,
            Stop,
            BuildOrderAction,
            ToggleOverlaysVisibility
        }

        Controller()
        {
            CustomEvents.Instance.OnEvent += OnCustomEvent;
            ServerConnection.Instance.TaskProcessor = _tasks;
            GameDetected = false;
        }


        private bool InitializeConfiguration()
        {
            try
            {
                //CustomEvents.Instance.AddException(EventsType.GeneralError, "details", new Exception("exception"));
                var keys = KeyboardEventsHandler.Instance;
                keys.Clear();

                var s = UserSettings.Instance;
                if (!s.Loaded) return false;

                GameClock.Instance.Speed = s.Speed;

                keys.AddListener(s.ApplicationStartPauseKeyList, (int)ControllerKeyEvents.StateToggle, this);
                keys.AddListener(s.PauseKeyCombination, (int)ControllerKeyEvents.StateToggle, this);
                keys.AddListener(s.ApplicationStopKeyList, (int)ControllerKeyEvents.Stop, this);
                keys.AddListener(s.ToggleOverlayTransparencyKeyList, (int)ControllerKeyEvents.ToggleOverlaysVisibility, this);

                _tapWatcher = new TapWatcher();

                if ((s.Production.Type & KeyWatcherSettings.WatchType.Measure) == KeyWatcherSettings.WatchType.Measure)
                {
                    for (int i = 0; i < s.Production.Keys.Count; i++)
                    {
                        keys.AddListener(new KeyList() { s.Production.Keys[i] }, i, _tapWatcher);
                        keys.AddListener(new KeyList() { s.Production.Keys[i], Keys.Control }, i, TapWatcher.KeyEventModifier.Add, _tapWatcher);
                        keys.AddListener(new KeyList() { s.Production.Keys[i], Keys.Shift }, i, TapWatcher.KeyEventModifier.Add, _tapWatcher);
                        keys.AddListener(new KeyList() { s.Production.Keys[i], Keys.Alt }, i, TapWatcher.KeyEventModifier.Remove, _tapWatcher);
                    }
                    _tapWatcher.OnEvent += context => CustomEvents.Instance.Add(EventsType.ProductionChecked, context);
                }

                _mapKeyEstimator = new KeyEstimator();

                if ((s.Map.Type & KeyWatcherSettings.WatchType.Measure) == KeyWatcherSettings.WatchType.Measure)
                {
                    keys.AddListener(s.Map.Keys, 0, _mapKeyEstimator);
                    _mapKeyEstimator.OnEvent += context => CustomEvents.Instance.Add(EventsType.MapChecked, context);
                }

                _resourcesKeyEstimator = new KeyEstimator();

                if ((s.Resources.Type & KeyWatcherSettings.WatchType.Measure) == KeyWatcherSettings.WatchType.Measure)
                {
                    keys.AddListener(s.Resources.Keys, 0, _resourcesKeyEstimator);
                    _resourcesKeyEstimator.OnEvent += context => CustomEvents.Instance.Add(EventsType.ResourcesChecked, context);
                }

                if (UserSettings.Instance.Keys2BuildOrders != null)
                {
                    foreach (var pair in UserSettings.Instance.Keys2BuildOrders)
                    {
                        keys.AddListener(pair.Key, (int)ControllerKeyEvents.BuildOrderAction, pair.Value, this);
                    }
                }

#if !DEBUG
                UIController.StartOverlaySync(); //todo: move to appropriate place
#endif

                //_rightClickWatcher = new RightClickWatcher();

                //keys.Hook.OnMouseActivity += (sender, e) => _rightClickWatcher.ProcessMouseEvent(e);
                //keys.Hook.KeyDown += (sender, e) => _rightClickWatcher.ProcessKeyDown(e.KeyCode);

                if (UserSettings.Instance.NotifyBoosts)
                {
                    KeyboardEventsHandler.Instance.Hook.KeyDown += raceDetector.OnKeyDown;
                    keys.AddListener(UserSettings.Instance.NotifyBoostsKeys, (int)BoostNotifier.BoostKeyboardEvent.Restart, boostNotifier);
                    keys.AddListener(new KeyList() { Keys.Q }, (int)BoostNotifier.BoostKeyboardEvent.QueenStarted, boostNotifier);
                }

                //
                return true;
            }
            catch (Exception ex)
            {
                CustomEvents.Instance.AddException(EventsType.GeneralError, "", ex);
                return false;
            }
        }

        private int counter = 0;

        private void UpdateResourcesCapture()
        {
            try
            {
                var capture = ScreenCapture.GetArea(ScreenMap.Get(ScreenCapture.ScreenSize).MineralArea);

                if (raceDetector.Race == PlayerRace.None) raceDetector.DetectByImage(capture);

                var b = ImageProcessor.ThresholdGrays(200, 40, capture);
                
                var recognized = ImageProcessor.Recognize(b);

                switch (recognized.Count)
                {
                    case 4:
                        var supply = Convert.ToInt32(recognized[2]);
                        var supplyCap = Convert.ToInt32(recognized[3]);
                        UIController.UpdateResourcesState(Convert.ToInt32(recognized[0]), Convert.ToInt32(recognized[1]), supply, supplyCap);
                        CustomEvents.Instance.AddLog(String.Format("{0}", string.Join("|", recognized.ToArray())));
                        GameLog.Instance.SupplyBlock = (supplyCap != 200) & (supply >= supplyCap);
                        break;
                    case 2:
                        //red supply
                        UIController.UpdateResourcesState(Convert.ToInt32(recognized[0]), Convert.ToInt32(recognized[1]), 0, 0);
                        GameLog.Instance.SupplyBlock = true;
                        break;
                    default:
                        #if DEBUG
                            int i;
                            counter += 1;
                            i = counter;
                            UIController.UpdateResourcesState(i, i - 20, i / 5, 50);
                            Debug.Print(i.ToString());
                        #endif
                        break;
                }
            }
            catch (Exception ex)
            {
                CustomEvents.Instance.AddLog(ex.StackTrace);
            }
        }


        private void CreateTimeEvents(RegularityEstimator estimator, KeyWatcherSettings settings, GameTimeEvent.GameTimeEventType eventType, EventsType raisedEvent)
        {
            if ((settings.Type & KeyWatcherSettings.WatchType.Notify) == 0) return;

            var e = new GameTimeRecurringEvent
            {
                RepeatInterval = settings.NotifyInterval,
                Time = settings.StartInterval,
                EventType = eventType
            };

            e.OnEvent += () => CustomEvents.Instance.Add(raisedEvent);
            GameTimeEventHandler.Instance.AddEvent(e);
            estimator.OnEvent += context =>
                                     {
                                         var eventTime = GameClock.Instance.GetGameTime().Add(settings.TargetInterval);
                                         if (settings.StartInterval.CompareTo(eventTime) > 0) return;
                                         GameTimeEventHandler.Instance.RemoveEvents(eventType);
                                         var newEvent = new GameTimeRecurringEvent()
                                                            {
                                                                EventType = eventType,
                                                                Time = eventTime,
                                                                RepeatInterval = settings.NotifyInterval
                                                            };
                                         newEvent.OnEvent += () => CustomEvents.Instance.Add(raisedEvent);
                                         GameTimeEventHandler.Instance.AddEvent(newEvent);
                                     };
        }

        private void ToggleState()
        {
            if (!GameClock.Instance.Started)
            {
                Start();
                return;
            }
            GameClock.Instance.Paused = !GameClock.Instance.Paused;
            if (GameClock.Instance.Paused)
            {
                CustomEvents.Instance.Add(EventsType.RecordingPaused);
            }
            else
            {
                CustomEvents.Instance.Add(EventsType.RecordingUnPaused);
            }
        }

        private void Stop()
        {
            if (!GameClock.Instance.Started) return;
            GameClock.Instance.Running = false;
            CustomEvents.Instance.Add(EventsType.RecordingStopped);
            CustomEvents.Instance.Add(EventsType.ReadyState);
            UIController.HideAll();
        }

        private void EndRecording()
        {
            if (!GameClock.Instance.Started) return;
            Stop();
            //check if we should submit a game
            var s = UserSettings.Instance;
            if (((s.Production.Type & KeyWatcherSettings.WatchType.Measure) != 0)
                || ((s.Map.Type & KeyWatcherSettings.WatchType.Measure) != 0)
                || ((s.Resources.Type & KeyWatcherSettings.WatchType.Measure) != 0))
            {
                _tasks.AddUnique(new SendGameTask());
            }
            CustomEvents.Instance.Add(EventsType.RecordingCompleted);
        }


        private void Start()
        {
            if (GameClock.Instance.Started || (!GameDetected)) return;
            GameClock.Instance.Running = true;
            GameLog.Instance.Start();
            CreateTimeEvents(_tapWatcher, UserSettings.Instance.Production, GameTimeEvent.GameTimeEventType.ProductionNotification, EventsType.ProductionMissingNotify);
            CreateTimeEvents(_mapKeyEstimator, UserSettings.Instance.Map, GameTimeEvent.GameTimeEventType.MapNotification, EventsType.MapControlMissingNotify);
            CreateTimeEvents(_resourcesKeyEstimator, UserSettings.Instance.Resources, GameTimeEvent.GameTimeEventType.ResourcesNotification, EventsType.ResourcesControlMissingNotify);
            _tapWatcher.Start();
            _mapKeyEstimator.Start();
            _resourcesKeyEstimator.Start();
            if (UserSettings.Instance.NotifyBoosts)
            {
                raceDetector.Start();
                boostNotifier.Race = PlayerRace.None;
            }
            
            if (UserSettings.Instance.CaptureResources)
            {
                var e = new GameTimeRecurringEvent()
                {
                    EventType = GameTimeEvent.GameTimeEventType.UpdateResourcesCapture,
                    Enabled = true,
                    RepeatInterval = new TimeSpan(0, 0, 0, 1),
                    Time = new TimeSpan(0, 0, 0, 10)
                };
                e.OnEvent += UpdateResourcesCapture;
                GameTimeEventHandler.Instance.AddEvent(e);
            }
            //_rightClickWatcher.Start();
            CustomEvents.Instance.Add(EventsType.RecordingStarted, "");
        }

        public void ProcessKeysMatchEvent(List<KeyboardEventContext> matched)
        {
            foreach (KeyboardEventContext e in matched)
            {
                if ((ControllerKeyEvents)e.EventCode != ControllerKeyEvents.Stop) continue;
                EndRecording();
                return;
            }
            foreach (KeyboardEventContext e in matched)
            {
                switch ((ControllerKeyEvents)e.EventCode)
                {
                    case ControllerKeyEvents.StateToggle:
                        ToggleState();
                        break;
                    case ControllerKeyEvents.BuildOrderAction:
                        Start();
                        if (BuildOrderHandler.Instance.Started)
                            BuildOrderHandler.Instance.Interrupt();
                        else
                            BuildOrderHandler.Instance.Start((BuildOrder)e.Data);
                        break;
                    case ControllerKeyEvents.ToggleOverlaysVisibility:
                        if (!GameDetected) continue;
                        UIController.SetOverlayTransparency(true);
                        break;
                }
            }
        }

        public void ProcessKeysMismatchEvent(List<KeyboardEventContext> matched)
        {
            foreach (KeyboardEventContext e in matched)
            {
                switch ((ControllerKeyEvents)e.EventCode)
                {
                    case ControllerKeyEvents.ToggleOverlaysVisibility:
                        if (!GameDetected) continue;
                        UIController.SetOverlayTransparency(false);
                        break;
                }
            }
        }

        
        /// <summary> 
        /// events processing 
        /// </summary>
        /// <param name="eventsType"></param>
        /// <param name="details">depends on event type</param>
        /// <remarks>main thread</remarks>
        private void OnCustomEvent(EventsType eventsType, object details)
        {
            switch (eventsType)
            {
                #region Disconnected
                case EventsType.Disconnected:
                    KeyboardEventsHandler.Instance.Stop();
                    replayFileWatcher.EnableRaisingEvents = false;
                    eventFileWatcher.EnableRaisingEvents = false;
                    UIController.SetStateIndicator(ProbeState.Offline);
                    Stop();
                    break;
                #endregion
                #region ConnectingState
                case EventsType.ConnectingState:
                    UIController.SetStateIndicator(ProbeState.NotReady);
                    break;
                #endregion
                case EventsType.DisconnectedState:
                    UIController.SetStateIndicator(ProbeState.Offline);
                    break;
                case EventsType.Connected:
                    InitializeConfiguration();
                    KeyboardEventsHandler.Instance.Start();
                    if (UserSettings.Instance.UseLedIndicator) KeyboardLeds.Set(Keys.Scroll, false);
                    replayFileWatcher.EnableRaisingEvents = true; //watch for replay anyway
                    eventFileWatcher.EnableRaisingEvents = true;

                    if (UserSettings.Instance.StartBulkReplayUpload)
                    {
                        AddUniqueAsyncTask(new BulkUploadReplaysTask
                                               {
                                                   LastUploadTime = UserSettings.Instance.LastBuldReplayUploadTime
                                               });
                    }
                    UIController.SetStateIndicator(ProbeState.Ready);
                    UIController.SetCurrentRace(PlayerRace.Terran); //until we find specific race
                    //test

                    

#if DEBUG
                    CustomEvents.Instance.Add(EventsType.GameGoingToStart);
#endif
                    break;
                case EventsType.RecordingStopped:
                    //_rightClickWatcher.Stop();
                    if (UserSettings.Instance.UseLedIndicator)
                    {
                        KeyboardLeds.StopBlink();
                        KeyboardLeds.DelayedSet(Keys.Scroll, false, 600);
                    }
                    UIController.Stop();
                    break;
                case EventsType.RecordingCompleted:
                    if (UserSettings.Instance.RecordingSoundNotify)
                    {
                        Speaker.ClearPlaylist();
                        Speaker.Speak("recording completed.wav");
                    }
                    UIController.ShowStateIndicator();
                    break;
                case EventsType.RecordingPaused:
                    if (UserSettings.Instance.UseLedIndicator) KeyboardLeds.StartBlink(Keys.Scroll, 500);
                    if (UserSettings.Instance.RecordingSoundNotify)
                    {
                        Speaker.ClearPlaylist();
                        Speaker.Speak("recording paused.wav");
                    }
                    UIController.HideAll();
                    break;
                #region EventsType.CloseRequest
                case EventsType.CloseRequest:
                    UIController.HideStateIndicator();
                    GameDetected = false;
                    Stop();
                    ServerConnection.Instance.CloseSession();
                    if (UserSettings.Instance.UseLedIndicator)
                    {
                        KeyboardLeds.Set(Keys.Scroll, Control.IsKeyLocked(Keys.Scroll));
                        KeyboardLeds.Set(Keys.CapsLock, Control.IsKeyLocked(Keys.CapsLock));
                        KeyboardLeds.Set(Keys.NumLock, Control.IsKeyLocked(Keys.NumLock));
                    }
                    break;
                #endregion
                case EventsType.RecordingStarted:
                case EventsType.RecordingUnPaused:
                    if (UserSettings.Instance.UseLedIndicator)
                    {
                        KeyboardLeds.StopBlink();
                        KeyboardLeds.DelayedSet(Keys.Scroll, true, 600);
                    }
                    if (UserSettings.Instance.RecordingSoundNotify)
                    {
                        Speaker.ClearPlaylist();
                        Speaker.Speak("recording started.wav");
                    }
                    if (UserSettings.Instance.DisplayButtonsOverlay)
                    {
                        UIController.SetHotkeyBarMode(ButtonsOverlayMode.Banner);
                        UIController.ShowHotkeyBar();
                    }
                    else
                    {
                        UIController.HideHotkeyBar();
                    }
                    if (UserSettings.Instance.CaptureResources)
                    {
                        UIController.ShowResourceBars();
                    }
                    break;
                case EventsType.MapControlMissingNotify:
                    Speaker.Speak("notify minimap.wav");
                    UIController.ShowNotification(NotificationType.Map, (UserSettings.Instance.Map.Type & KeyWatcherSettings.WatchType.Measure) == 0);
                    break;
                case EventsType.MapChecked:
                    GameLog.Instance.AddEntry("map", "checked", (JToken)details);
                    UIController.HideNotification(NotificationType.Map);
                    break;
                case EventsType.ResourcesControlMissingNotify:
                    Speaker.Speak("notify resources.wav");
                    UIController.ShowNotification(NotificationType.Resources, (UserSettings.Instance.Resources.Type & KeyWatcherSettings.WatchType.Measure) == 0);
                    break;
                case EventsType.ResourcesChecked:
                    GameLog.Instance.AddEntry("resources", "checked", (JToken)details);
                    UIController.HideNotification(NotificationType.Resources);
                    break;
                case EventsType.ProductionMissingNotify:
                    Speaker.Speak("notify production.wav");
                    UIController.ShowNotification(NotificationType.Production, (UserSettings.Instance.Production.Type & KeyWatcherSettings.WatchType.Measure) == 0);
                    break;
                case EventsType.ProductionChecked:
                    GameLog.Instance.AddEntry("tap", "cycle completed", (JToken)details);
                    UIController.HideNotification(NotificationType.Resources);
                    break;
                case EventsType.WillRestartForUpdate:
                case EventsType.ClientNeedToBeReinstalled:
                    ServerConnection.Instance.ConnectionMode = ServerConnection.ConnectionModeEnum.DoNotConnect;
                    break;
                case EventsType.ReplayFileCreated:
                    GameDetected = false;
#if DEBUG
                    GameDetected = true;
#endif
                    if (GameClock.Instance.Started) EndRecording();
                    if (UserSettings.Instance.UploadReplays)
                    {
                        replayFileWatcher.EnableRaisingEvents = false;
                        AddUniqueAsyncTask(new UploadReplayTask
                                               {
                                                   ReplayInfo = (ReplayInfo)details,
                                                   UploadType = UploadReplayTask.ReplayUploadType.CurrentGame
                                               });
                    }
                    break;
                case EventsType.ReplayFileUploaded:
                    replayFileWatcher.EnableRaisingEvents = true;
                    break;
                case EventsType.GameGoingToStart:
                    if (GameDetected) return;
                    GameDetected = true;
                    if (UserSettings.Instance.DisplayButtonsOverlay)
                    {
                        UIController.SetHotKeys(GetHotkeysForOverlay());
                        UIController.SetHotkeyBarMode(ButtonsOverlayMode.HotKeys);
                        UIController.ShowHotkeyBar();
                    }

                    UIController.HideStateIndicator();
                    if (UserSettings.Instance.StartWithAnykey) KeyboardEventsHandler.Instance.Hook.KeyPress += AnykeyStart;
                    break;
                case EventsType.BuildOrderItemComing:
                    UIController.SetBuildOrderItemState((int)details, BOItem.BOItemState.Coming);
                    break;
                case EventsType.BuildOrderItemPrepareExecution:
                    UIController.SetBuildOrderItemState((int)details, BOItem.BOItemState.PrepareExecution);
                    break;
                case EventsType.BuildOrderItemTriggered:
                    if (UserSettings.Instance.PronounceBuildOrder) Speaker.Speak(BuildOrderHandler.Instance.GetItemText((int)details), true);
                    UIController.SetBuildOrderItemState((int)details, BOItem.BOItemState.Current);
                    //UIController.BuildOrderItemTriggered((int) details);
                    Debug.Print("build order step {0} triggered", details);
                    break;
                case EventsType.BuildOrderItemFinished:
                    UIController.SetBuildOrderItemState((int)details, BOItem.BOItemState.Finished);
                    break;
                case EventsType.Restart:
                    Program.RestartApp();
                    break;
                case EventsType.RaceDetected:
                    CustomEvents.Instance.AddLog(string.Format("detected race {0}", raceDetector.Race));
                    boostNotifier.Race = raceDetector.Race;
                    UIController.SetCurrentRace(raceDetector.Race);
                    boostNotifier.PrepareNotification();
                    break;
                case EventsType.BoostNotify:
                    UIController.ShowNotification(NotificationType.Boost, true);
                    Speaker.Speak(Constants.BoostNotification[raceDetector.Race]);
                    break;
            }
        }

        private List<KeyValuePair<string, string>> GetHotkeysForOverlay()
        {
            var result = new List<KeyValuePair<string, string>>();
            UserSettings settings = UserSettings.Instance;
            var key = settings.StartWithAnykey ? "-any button-" : settings.ApplicationStartPauseKeyList.ToString();
            result.Add(new KeyValuePair<string, string>(key, "start game"));
            result.Add(new KeyValuePair<string, string>(settings.ApplicationStartPauseKeyList.ToString(), "pause / resume"));
            result.Add(new KeyValuePair<string, string>(settings.ApplicationStopKeyList.ToString(), "stop"));
            foreach (KeyValuePair<KeyList, BuildOrder> pair in settings.Keys2BuildOrders)
            {
                result.Add(new KeyValuePair<string, string>(pair.Key.ToString(), pair.Value.Title));
            }
            return result;
        }

        void AnykeyStart(object sender, KeyPressEventArgs e)
        {
            Debug.Print("anykey start");
            Start();
            KeyboardEventsHandler.Instance.Hook.KeyPress -= AnykeyStart;
        }

        public void AddUniqueAsyncTask(IAsyncTask task)
        {
            _tasks.AddUnique(task);
        }
    }

}
