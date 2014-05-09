using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Overlays;
using Probe.BuildOrders;
using Probe.Common;
using Probe.Utility;
using Probe.WebClient;

namespace Probe.Game
{
    class KeyWatcherSettings
    {
        [Flags]
        public enum WatchType
        {
            None = 0x0,
            Notify = 0x1,
            Measure = 0x2
        }
        public WatchType Type { get; internal set; }
        public TimeSpan StartInterval { get; internal set; }
        public TimeSpan NotifyInterval { get; internal set; }
        public TimeSpan TargetInterval { get; internal set; }
        public KeyList Keys { get; internal set; }

        public static WatchType StringToWatchType(string s)
        {
            var d = new Dictionary<string, WatchType>
                        {
                            {"none", WatchType.None},
                            {"notify", WatchType.Notify},
                            {"notify_measure", WatchType.Notify | WatchType.Measure},
                            {"measure", WatchType.Measure}
                        };
            return d.ContainsKey(s) ? d[s] : WatchType.None;
        }
    }

    class UserSettings
    {
        #region singleton
        private static readonly UserSettings _instance = new UserSettings();


        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static UserSettings()
        {
        }

        UserSettings()
        {
        }

        public static UserSettings Instance
        {
            get
            {
                return _instance;
            }
        }
        #endregion

        public bool Loaded { get; private set; }
        public KeyList ApplicationStartPauseKeyList { get; private set; }
        public KeyList ApplicationStopKeyList { get; private set; }
        public KeyWatcherSettings Map { get; private set; }
        public KeyWatcherSettings Resources { get; private set; }
        public KeyWatcherSettings Production { get; private set; }
        public KeyList PauseKeyCombination { get; private set; }
        public GameClock.GameSpeedEnum Speed { get; private set; }
        public bool UseLedIndicator { get; private set; }
        public bool RecordingSoundNotify { get; private set; }
        public bool UploadReplays { get; private set; }
        public Dictionary<KeyList, BuildOrder> Keys2BuildOrders { get; private set; }
        public bool StartWithAnykey { get; private set; }
        public BuildOrderListDisplayMode BuildOrderDisplayMode { get; private set; }
        public bool DisplayButtonsOverlay { get; private set; }
        public bool PronounceBuildOrder { get; private set; }
        public TimeSpan BuildOrderStepComingInterval { get; private set; }
        public TimeSpan BuildOrderStepPrepareExecutionInterval { get; private set; }
        public TimeSpan BuildOrderStepActiveInterval { get; private set; }
        public KeyList ToggleOverlayTransparencyKeyList { get; private set; }
        public Boolean StartBulkReplayUpload { get; private set; }
        public KeyList NotifyBoostsKeys { get; private set; }
        public bool NotifyBoosts { get; private set; }
        public bool CaptureResources { get; private set; }
        public DateTime LastBuldReplayUploadTime { get; set; }
        public List<string> UploadReplaysHash { get; set; }

        public bool SyncronizeOverlaysWithGame { get; private set; }

        public bool ShutDownWithSc2 { get; private set; }

        public void Load()
        {
            Loaded = false;
            var json = File.ReadAllText(Path.Combine(Application.StartupPath, "settings.json"));
            var j = JObject.Parse(json);
            var jw = new JTokenWrap(j);
            if (!j["success"].Value<bool>()) return;
            Loaded = true;

            Speed = (GameClock.GameSpeedEnum)Enum.Parse(typeof(GameClock.GameSpeedEnum), jw.GetString("data.settings.speed", "Faster"), true);
            ApplicationStartPauseKeyList = new KeyList() { KeysHelper.StringToKey((string)j["data"]["settings"]["app_control_key"]) };
            ApplicationStopKeyList = new KeyList() { KeysHelper.StringToKey((string)j["data"]["settings"]["app_control_key"]), Keys.Shift };
            PauseKeyCombination = new KeyList() { Keys.Pause };

            UseLedIndicator = jw.GetBool("data.settings.use_scroll_led", false);
            RecordingSoundNotify = jw.GetBool("data.settings.recording_sound_notify", true);

            UploadReplays = jw.GetBool("data.settings.upload_replay", false);
            DisplayButtonsOverlay = jw.GetBool("data.settings.buttons_overlay", false);

            BuildOrderStepComingInterval = new TimeSpan(0, 0, 0, 15);
            BuildOrderStepPrepareExecutionInterval = new TimeSpan(0, 0, 0, 5);
            BuildOrderStepActiveInterval = new TimeSpan(0, 0, 0, 15);

            StartWithAnykey = jw.GetBool("data.settings.start_with_anykey", false);

            var key = j["data"]["settings"]["map"]["key"].Type == JTokenType.Null ? Keys.None : KeysHelper.StringToKey(j["data"]["settings"]["map"]["key"].Value<string>());
            var start = new TimeSpan(0, 0, j["data"]["settings"]["map"]["start"].Type == JTokenType.Null ? 0 : j["data"]["settings"]["map"]["start"].Value<int>());
            var notify = new TimeSpan(0, 0, j["data"]["settings"]["map"]["notify"].Type == JTokenType.Null ? 0 : j["data"]["settings"]["map"]["notify"].Value<int>());
            var target = new TimeSpan(0, 0, j["data"]["settings"]["map"]["target"].Type == JTokenType.Null ? 0 : j["data"]["settings"]["map"]["target"].Value<int>());
            Map = new KeyWatcherSettings()
            {
                Keys = new KeyList() { key },
                Type = KeyWatcherSettings.StringToWatchType(j["data"]["settings"]["map"]["type"].Value<string>()),
                NotifyInterval = notify,
                StartInterval = start,
                TargetInterval = target
            };

            key = j["data"]["settings"]["resources"]["key"].Type == JTokenType.Null ? Keys.None : KeysHelper.StringToKey(j["data"]["settings"]["resources"]["key"].Value<string>());
            start = new TimeSpan(0, 0, j["data"]["settings"]["resources"]["start"].Type == JTokenType.Null ? 0 : j["data"]["settings"]["resources"]["start"].Value<int>());
            notify = new TimeSpan(0, 0, j["data"]["settings"]["resources"]["notify"].Type == JTokenType.Null ? 0 : j["data"]["settings"]["resources"]["notify"].Value<int>());
            target = new TimeSpan(0, 0, j["data"]["settings"]["resources"]["target"].Type == JTokenType.Null ? 0 : j["data"]["settings"]["resources"]["target"].Value<int>());
            Resources = new KeyWatcherSettings()
            {
                Keys = new KeyList() { key },
                Type = KeyWatcherSettings.StringToWatchType(j["data"]["settings"]["resources"]["type"].Value<string>()),
                NotifyInterval = notify,
                StartInterval = start,
                TargetInterval = target
            };

            start = new TimeSpan(0, 0, j["data"]["settings"]["production"]["start"].Type == JTokenType.Null ? 0 : j["data"]["settings"]["production"]["start"].Value<int>());
            notify = new TimeSpan(0, 0, j["data"]["settings"]["production"]["notify"].Type == JTokenType.Null ? 0 : j["data"]["settings"]["production"]["notify"].Value<int>());
            target = new TimeSpan(0, 0, j["data"]["settings"]["production"]["target"].Type == JTokenType.Null ? 0 : j["data"]["settings"]["production"]["target"].Value<int>());
            Production = new KeyWatcherSettings()
            {
                Keys = new KeyList(),
                Type = KeyWatcherSettings.StringToWatchType(j["data"]["settings"]["production"]["type"].Value<string>()),
                NotifyInterval = notify,
                StartInterval = start,
                TargetInterval = target
            };

            if (j["data"]["settings"]["production"]["keys"].Type == JTokenType.Array)
            {
                foreach (var t in (JArray)j["data"]["settings"]["production"]["keys"])
                {
                    if (t.Type == JTokenType.String) Production.Keys.Add(KeysHelper.StringToKey(t.Value<String>()));
                }
            }

            //parse build orders

            switch (jw.GetString("data.settings.build_orders_ui", "none"))
            {
                case "full":
                    BuildOrderDisplayMode = BuildOrderListDisplayMode.Full;
                    break;
                case "three":
                    BuildOrderDisplayMode = BuildOrderListDisplayMode.LimitedList;
                    break;
                default:
                    BuildOrderDisplayMode = BuildOrderListDisplayMode.None;
                    break;
            }

            PronounceBuildOrder = !jw.GetBool("data.settings.silent_build_order", false);

            Keys2BuildOrders = new Dictionary<KeyList, BuildOrder>();

            if ((j["data"]["settings"]["build_orders"] != null) && (j["data"]["settings"]["build_orders"].Type == JTokenType.Array))
            {
                foreach (JToken e in (JArray)j["data"]["settings"]["build_orders"])
                {
                    var ew = new JTokenWrap(e);
                    var keyList = new KeyList();

                    if (e["keys"].Type == JTokenType.Array)
                    {
                        foreach (var boKey in (JArray)e["keys"])
                        {
                            if (boKey.Type == JTokenType.String) keyList.Add(KeysHelper.StringToKey(boKey.Value<String>()));
                        }
                    }

                    var bo = new BuildOrder();

                    bo.Title = ew.GetString("title", "-");

                    switch (ew.GetString("type", "step"))
                    {
                        case "time":
                            bo.ExecutionExecutionType = BuildOrder.BuildOrderExecutionType.Time;
                            break;
                        default:
                            bo.ExecutionExecutionType = BuildOrder.BuildOrderExecutionType.Step;
                            break;
                    }

                    if (e["order"].Type == JTokenType.Array)
                    {
                        foreach (var boItem in (JArray)e["order"])
                        {
                            var newBoItem = new BuildOrderStep();
                            newBoItem.Time = (new TimeSpan(0, 0, boItem["time"].Type == JTokenType.Null ? 0 : boItem["time"].Value<int>())).Subtract(new TimeSpan(1000));
                            newBoItem.Message = boItem["message"].Value<string>();

                            bo.Add(newBoItem);
                        }
                    }

                    //sort items

                    switch (bo.ExecutionExecutionType)
                    {
                        case BuildOrder.BuildOrderExecutionType.Time:
                            bo.Sort((a, b) => a.Time.CompareTo(b.Time));
                            break;
                        case BuildOrder.BuildOrderExecutionType.Step:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    for (int i = 0; i < bo.Count; i++)
                    {
                        bo[i].Index = i;
                    }

                    if (keyList.Count == 0 || bo.Count == 0) continue;

                    Keys2BuildOrders.Add(keyList, bo);
                }
            }

            ToggleOverlayTransparencyKeyList = new KeyList { Keys.Alt };

            StartBulkReplayUpload = false; //
            LastBuldReplayUploadTime = DateTime.MinValue; // file date
            UploadReplaysHash = null;

            SyncronizeOverlaysWithGame = true;
            ShutDownWithSc2 = false; //if run with link
#if DEBUG
            NotifyBoosts = true; //todo: option on server
            NotifyBoostsKeys = new KeyList() {Keys.Add};
#endif
            NotifyBoosts = false; //todo: option on server
            NotifyBoostsKeys = new KeyList() { Keys.Add }; //todo: option on server
            CaptureResources = true; //todo: option on server

            //set custom logo
            if(!File.Exists(Paths.LogoImagePath))
            {
                Properties.Resources.splash.Save(Paths.LogoImagePath);
                //todo: UIController.SetHotKeyBarLogo(Paths.LogoImagePath);
            }
        }

    }
}
