using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Probe.Utility;
using Timer = System.Timers.Timer;

namespace Probe.Game
{
    public class GameTimeEvent
    {
        public enum GameTimeEventType
        {
            ProductionNotification,
            MapNotification,
            ResourcesNotification,
            BuildOrderNotification,
            BoostNotification,
            Service,
            UpdateResourcesCapture
        }

        public GameTimeEventType EventType;
        public TimeSpan Time;
        public event Action OnEvent;

        public bool Enabled { get; set; }

        public TimeSpan CurrentTime { get; set; }

        public void InvokeOnEvent()
        {
            if (OnEvent != null) OnEvent();
        }

        public GameTimeEvent()
        {
            Enabled = true;
        }
    }

    public class GameTimeRecurringEvent : GameTimeEvent
    {
        public TimeSpan RepeatInterval;

        public GameTimeRecurringEvent()
        {
            OnEvent += delegate
            {
                Time = CurrentTime.Add(RepeatInterval);
                Enabled = true;
            };
        }
    }

    public class GameTimeEventHandler
    {
        #region singleton
        private static readonly GameTimeEventHandler _instance = new GameTimeEventHandler();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static GameTimeEventHandler()
        {
        }

        GameTimeEventHandler()
        {
            CustomEvents.Instance.OnEvent += OnCustomEvent;
            _timer.Elapsed += (s, e) => CheckEvents();
        }

        public static GameTimeEventHandler Instance
        {
            get
            {
                return _instance;
            }
        }
        #endregion

        private readonly Timer _timer = new Timer() { AutoReset = false, Enabled = false };

        private void CheckEvents()
        {
            _timer.Enabled = false;
            if (!GameClock.Instance.Running) return;
            var currentTime = GameClock.Instance.GetGameTime();
            //Debug.Print("time events check @{0}", currentTime);

            IEnumerable<GameTimeEvent> eventsToRun;
            lock (_eventsLock) eventsToRun = _events.Where(e => (e.Enabled && e.Time.CompareTo(currentTime) <= 0)).ToList();

            foreach (var e in eventsToRun)
            {
                e.Enabled = false; // disable event first so it can be re-enabled at .OnEvent()
                e.CurrentTime = currentTime;
                e.InvokeOnEvent();
            }

            var nextEventTime = new TimeSpan(1,0,0,0);

            lock (_eventsLock)
            {
                _events = _events.Where(e => e.Enabled).ToList();
                if (_events.Count > 0) nextEventTime = _events.Min(t => t.Time);
            }
            
            _timer.Interval = Math.Max(GameClock.Instance.ConvertToRealTime(nextEventTime.Subtract(currentTime)).TotalMilliseconds, 100);
            _timer.Start();
        }

        private object _eventsLock = new object();

        private void OnCustomEvent(EventsType eventsType, object context)
        {
            if (eventsType != EventsType.GameTimeStateChanged) return;
            if (GameClock.Instance.Running) CheckEventsInNewThread();
            if (!GameClock.Instance.Started) Clear();
        }

        private List<GameTimeEvent> _events = new List<GameTimeEvent>();

        public void AddEvent(GameTimeEvent timeEvent)
        {
            lock (_eventsLock)
            {
                _events.Add(timeEvent);
            }
            CheckEventsInNewThread();
        }

        private void CheckEventsInNewThread()
        {
            var t = new Thread(CheckEvents);
            t.Start();
        }

        public void AddEvents(IEnumerable<GameTimeEvent> events)
        {
            lock (_eventsLock)
            {
                _events.AddRange(events);
            }
            CheckEventsInNewThread();
        }

        public void Clear()
        {
            lock (_eventsLock) _events.Clear();
        }

        public void RemoveEvents(GameTimeEvent.GameTimeEventType gameTimeEventType)
        {
            lock (_eventsLock)
            {
                foreach (var e in _events)
                {
                    if (e.EventType == gameTimeEventType) e.Enabled = false;
                }
            }
        }
    }
}
