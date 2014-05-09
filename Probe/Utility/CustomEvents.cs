using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Probe.Utility
{
    public enum EventsType
    {
        RecordingStarted,
        RecordingPaused,
        RecordingUnPaused,
        RecordingStopped,
        ConnectionError,
        GeneralError,
        JSONRequestException,
        MapControlMissingNotify,
        ResourcesControlMissingNotify,
        ProductionMissingNotify,
        ErrorSavedToFile,
        ErrorSentToServer,
        ServerMessage,
        ConnectingState,
        ReadyState,
        AnotherInstanceExists,
        Connected,
        DisconnectedState,
        CannotStartSession,
        SessionStarted,
        SessionOfOtherInstance,
        UnexpectedCase,
        Disconnected,
        CloseRequest,
        ServerAction,
        RecordingCompleted,
        NewVersionInstalled,
        ServerUnavailable,
        WillRestartForUpdate,
        ClientNeedToBeReinstalled,
        Message,
        ConnectionCycleCompleted,
        GameTimeStateChanged,
        ReplayFileCreated,
        ReplayFileUploaded,
        Close,
        GameGoingToStart,
        BuildOrderCompleted,
        BuildOrderItemTriggered,
        BuildOrderItemComing,
        BuildOrderItemPrepareExecution,
        BuildOrderItemFinished,
        Restart,
        RaceDetected,
        BoostNotify,
        MapChecked,
        ResourcesChecked,
        ProductionChecked
    }

    public class CustomEvent
    {
        public EventsType Type;
        public object Context;
    }

    class CustomEvents
    {
        #region singleton
        private static readonly CustomEvents _instance = new CustomEvents();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static CustomEvents()
        {
        }

        public static CustomEvents Instance
        {
            get
            {
                return _instance;
            }
        }

        #endregion

        public Action<EventsType, object> OnEvent;
        private Queue<CustomEvent> _eventsQueue = new Queue<CustomEvent>();
        private Object _lock = new Object();
        private BackgroundWorker queueProcessor = new BackgroundWorker();

        CustomEvents()
        {
            queueProcessor.DoWork += processQueue;
            queueProcessor.RunWorkerCompleted += queueMessageFound;
            queueProcessor.RunWorkerAsync();
        }

        void queueMessageFound(object sender, RunWorkerCompletedEventArgs e)
        {
            var r = (CustomEvent)e.Result;
            if (OnEvent != null) OnEvent(r.Type, r.Context);
            if (!queueProcessor.IsBusy) queueProcessor.RunWorkerAsync();
        }

        void processQueue(object sender, DoWorkEventArgs e)
        {
            while (_eventsQueue.Count == 0)
            {
                Thread.Sleep(500);
            }
            lock (_lock)
            {
                e.Result = _eventsQueue.Dequeue();
            }
        }


        private bool _enabled = true;
        private object _logFileLock = new object();

        public void AddLog(object message)
        {
            lock (_logFileLock)
            {
                try
                {
                    using (var writer = new StreamWriter(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\app.log", true))
                    {
                        writer.WriteLine(string.Format("{0:o}: {1}", DateTime.Now, message));
                        Debug.Print(message.ToString());
                        writer.Close();
                    }
                }
                catch {}
            }
        }

        public void Add(EventsType t, object details = null)
        {
            lock (_lock)
            {
                AddLog(string.Format("event {0}", t));
                _eventsQueue.Enqueue(new CustomEvent() { Type = t, Context = details });
            }
        }

        public void AddException(EventsType eventsType, string details, Exception exception)
        {
            if (!_enabled) return;
            try
            {
                _enabled = false; //do not process any futher errors
                Add(eventsType, details);
                AddLog(String.Format("Error:{0}", exception));
                if (eventsType == EventsType.ConnectionError)
                {
                    _enabled = true;
                    return;
                }

                bool errorSubmited = false;
#if !DEBUG
                var c = WebLayer.JSONRequest("exception", new JObject()
                                                              {
                                                                  { "exception", exception.ToString() }, 
                                                                  { "details", details }, 
                                                                  { "os", Environment.OSVersion.VersionString },
                                                                  { "framework", Program.GetFramevorkVersion()},
                                                                  { "runtime",Environment.Version.ToString() }
                                                              });

                errorSubmited = c["success"].Value<bool>();
#endif
                if (errorSubmited)
                {
                    Add(EventsType.ErrorSentToServer);
                }
                else
                {
                    using (var writer = new StreamWriter(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\error.log", true))
                    {
                        writer.WriteLine(string.Format("{0:o}: context: {1}\r\nexception{2}", DateTime.Now, details, exception));
                        writer.Close();
                    }

                    Add(EventsType.ErrorSavedToFile);
                }
            }
            finally
            {
                _enabled = true;
            }
        }
    }
}
