using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Probe.Engine;
using Probe.Properties;
using Probe.Utility;
using Newtonsoft.Json.Linq;


namespace Probe.WebClient
{
    public class ServerConnection
    {
        #region singleton
        private static readonly ServerConnection _instance = new ServerConnection();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static ServerConnection()
        {
        }

        public static ServerConnection Instance
        {
            get
            {
                return _instance;
            }
        }
        #endregion

        public AsyncTaskProcessor TaskProcessor;

        ServerConnection()
        {
            _updateSessionActivityTimer.Elapsed += UpdateSessionActivityTimerElapsed;
            _connected = false;
            ConnectionMode = ConnectionModeEnum.Normal;
        }

        void UpdateSessionActivityTimerElapsed(object sender, ElapsedEventArgs e)
        {
            TaskProcessor.AddUnique(new UpdateSessionActivity());
        }

        private bool _connected;
        private Timer _updateSessionActivityTimer = new Timer(5 * 60 * 1000) { AutoReset = true, Enabled = false };
        public bool SessionOpened { get; private set; }

        public static int MaxConnectionAttempts { get { return 10; } }
        public static long ConnectionAttemptInterval { get { return 30 * 1000; } }
        
        public bool Connected
        {
            get { return _connected; }
            internal set
            {
                CustomEvents.Instance.Add(value ? EventsType.ReadyState : EventsType.DisconnectedState);
                if (_connected == value) return;
                _connected = value;
                ConnectionAttempts = 0;
                CustomEvents.Instance.Add(value ? EventsType.Connected : EventsType.Disconnected);
                _updateSessionActivityTimer.Enabled = Connected;
            }
        }

        public void Connect()
        {
            if (Connected || (ConnectionMode == ConnectionModeEnum.DoNotConnect)) return;
            TaskProcessor.AddUnique(new ConnectionTask());
        }

        public enum ConnectionModeEnum
        {
            Normal,
            Force,
            SilentErrors,
            DoNotConnect
        }

        public ConnectionModeEnum ConnectionMode { get; internal set; }

        public int ConnectionAttempts { get; set; }
        

        public void Connect(ConnectionModeEnum connectionMode)
        {
            ConnectionMode = connectionMode;
            Connect();
        }

        internal bool OpenSession()
        {
            var r = WebLayer.JSONRequest("start_session", null);
            if (!r["success"].Value<bool>())
            {
                if ((r["data"] != null) && ((r["message"] == null) || (string.IsNullOrEmpty(r["message"].Value<string>()))) && (r["data"]["reason"] != null))
                    switch (r["data"]["reason"].Value<string>())
                    {
                        case "another instance":
                            switch (ConnectionMode)
                            {
                                case ConnectionModeEnum.Normal:
                                    CustomEvents.Instance.Add(EventsType.SessionOfOtherInstance);
                                    break;
                                case ConnectionModeEnum.Force:
                                    WebLayer.OpenBrowser(WebLayer.PredefinedUrl.ForceOpenClientSession);
                                    break;
                                case ConnectionModeEnum.SilentErrors:
                                    break;
                            }
                            break;
                        default:
                            switch (ConnectionMode)
                            {
                                case ConnectionModeEnum.Normal:
                                    CustomEvents.Instance.Add(EventsType.CannotStartSession);
                                    break;
                                case ConnectionModeEnum.Force:
                                    WebLayer.OpenBrowser(WebLayer.PredefinedUrl.BindClient);
                                    break;
                                case ConnectionModeEnum.SilentErrors:
                                    break;
                            }
                            break;
                    }
            }
            else
            {
                CustomEvents.Instance.Add(EventsType.SessionStarted);
            }
            SessionOpened = r["success"].Value<bool>();
            return SessionOpened;
        }

        internal void CloseSession()
        {
            if (!SessionOpened) return;
            WebLayer.JSONRequest("close_session", null);
        }

        internal bool CheckInstanceCode()
        {
#if DEBUG
            Settings.Default.InstanceCode = "AAA";
            Settings.Default.Save();
#endif
            var result = true;
            if (string.IsNullOrEmpty(Settings.Default.InstanceCode))
            {
                var r = WebLayer.JSONRequest("get_new_instance_code", null);
                result = r["success"].Value<bool>();
                if (result)
                {
                    Settings.Default.InstanceCode = r["data"]["code"].Value<string>();
                    Settings.Default.Save();
                }
                else
                {
                    if (r["message"] == null || string.IsNullOrEmpty(r["message"].Value<string>()))
                        CustomEvents.Instance.Add(EventsType.UnexpectedCase, "cannot get instance code");
                }
            }
            return result;
        }
    }
}
