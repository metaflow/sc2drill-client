using System;
using System.Diagnostics;
using System.Timers;
using Probe.Game;
using Probe.Properties;
using Probe.Utility;
using Probe.WebClient;
using Newtonsoft.Json.Linq;

namespace Probe.Engine
{
    public class ConnectionTask : IAsyncTask
    {

        public void Run()
        {
            CustomEvents.Instance.Add(EventsType.ConnectingState);
            Success = false;
            if (ServerConnection.Instance.Connected || (ServerConnection.Instance.ConnectionMode == ServerConnection.ConnectionModeEnum.DoNotConnect)) return;
            Success = false;
            CustomEvents.Instance.CheckErrorLog();
            Debug.Print("CheckInstanceCode");
            Success = ServerConnection.Instance.CheckInstanceCode();
            Debug.Print("Open session");
            if (Success) Success = ServerConnection.Instance.OpenSession();
            if (Success)
            {
                Debug.Print("Load settings");
                if (!UserSettings.Instance.Loaded) UserSettings.Instance.Load();
                Success = UserSettings.Instance.Loaded;
            }

            if (Success)
            {
                ServerConnection.Instance.ConnectionMode = ServerConnection.ConnectionModeEnum.Normal;
            }
            else
            {
                ServerConnection.Instance.CloseSession();
            }
        }

        public void OnComplete()
        {
            CustomEvents.Instance.Add(EventsType.ConnectionCycleCompleted);
            ServerConnection.Instance.Connected = Success;
            if (Success) return;
            ServerConnection.Instance.ConnectionAttempts++;
            if (ServerConnection.Instance.ConnectionAttempts > ServerConnection.MaxConnectionAttempts) return;
            ServerConnection.Instance.ConnectionMode = ServerConnection.ConnectionModeEnum.SilentErrors;
            var t = new Timer() { AutoReset = false, Enabled = false, Interval = ServerConnection.ConnectionAttemptInterval };
            t.Elapsed += delegate { ServerConnection.Instance.Connect(); };
            t.Start();
        }

        public bool Success { get; private set; }

        public AsyncTaskProcessor Processor { get; set; }
    }
}