using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public enum ConnectionModeEnum
        {
            Normal,
            Force,
            SilentErrors,
            DoNotConnect
        }

        ServerConnection()
        {
            _connected = false;
            ConnectionMode = ConnectionModeEnum.Normal;
        }

        private bool _connected;
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
            }
        }

        public void Connect()
        {
            if (Connected || (ConnectionMode == ConnectionModeEnum.DoNotConnect)) return;
            TaskProcessor.AddUnique(new ConnectionTask());
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
            CustomEvents.Instance.Add(EventsType.SessionStarted);
            SessionOpened = true;
            return SessionOpened;
        }

        internal void CloseSession()
        {
        }

        internal bool CheckInstanceCode()
        {
            return true;
        }
    }
}
