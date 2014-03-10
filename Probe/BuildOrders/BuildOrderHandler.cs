using System;
using System.Collections.Generic;
using System.Linq;
using Probe.Game;
using Probe.Utility;


namespace Probe.BuildOrders
{
    public class BuildOrderHandler
    {
        #region singleton
        private static readonly BuildOrderHandler _instance = new BuildOrderHandler();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static BuildOrderHandler()
        {
        }

        public static BuildOrderHandler Instance
        {
            get
            {
                return _instance;
            }
        }

        #endregion

        public bool Started { get { return _build != null; } }

        private BuildOrder _build;
        private const int BOCloseDelay = 5;
        private int _currentStepIndex = 0;

        BuildOrderHandler()
        {
            CustomEvents.Instance.OnEvent += OnCustomEvent;
        }

        private void OnCustomEvent(EventsType eventsType, object context)
        {
            switch (eventsType)
            {
                case EventsType.RecordingStopped:
                    Stop();
                    break;
                case EventsType.BuildOrderCompleted:
                    var hideEvent = new GameTimeEvent()
                                        {
                                            Enabled = true,
                                            EventType = GameTimeEvent.GameTimeEventType.BuildOrderNotification,
                                            Time = GameClock.Instance.GetGameTime() + new TimeSpan(0, 0, 0, BOCloseDelay)
                                        };
                    hideEvent.OnEvent += UIController.HideBuildOrder;
                    GameTimeEventHandler.Instance.AddEvent(hideEvent);
                    break;
            }
        }

        private void Stop()
        {
            _build = null;
            GameTimeEventHandler.Instance.RemoveEvents(GameTimeEvent.GameTimeEventType.BuildOrderNotification);
        }

     

        public void Start(BuildOrder build)
        {
            if (Started || !GameClock.Instance.Started) return;
            _build = build;
            _currentStepIndex = 0;
            switch (_build.ExecutionExecutionType)
            {
                case BuildOrder.BuildOrderExecutionType.Time:
                    CreateNotifications();
                    break;
                case BuildOrder.BuildOrderExecutionType.Step:
                    if (_currentStepIndex >= _build.Count) break;
                    var e = new GameTimeEvent
                               {
                                   Time = new TimeSpan(0),
                                   Enabled = true,
                                   EventType = GameTimeEvent.GameTimeEventType.BuildOrderNotification
                               };
                    e.OnEvent += () => CustomEvents.Instance.Add(EventsType.BuildOrderItemTriggered, _currentStepIndex);
                    GameTimeEventHandler.Instance.AddEvent(e);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            foreach (var step in _build)
            {
                UIController.AddBuildOrderStep(step);
            }
            UIController.ShowBuildOrder(UserSettings.Instance.BuildOrderDisplayMode);
        }
        
        private void CreateNotifications()
        {
            var gameTime = GameClock.Instance.GetGameTime();
            List<GameTimeEvent> events = new List<GameTimeEvent>();

            for (int i = 0; i < _build.Count; i++)
            {
                var item = _build[i];

                if (item.Time.CompareTo(gameTime) < 0)
                {
                    CustomEvents.Instance.Add(EventsType.BuildOrderItemFinished, item.Index);
                    continue;
                }

                var e = new GameTimeEvent()
                {
                    Time = item.Time.Subtract(UserSettings.Instance.BuildOrderStepComingInterval),
                    EventType = GameTimeEvent.GameTimeEventType.BuildOrderNotification
                };
                Action timerAction = () => CustomEvents.Instance.Add(EventsType.BuildOrderItemComing, item.Index);
                e.OnEvent += timerAction;
                events.Add(e);

                e = new GameTimeEvent()
                {
                    Time = item.Time.Subtract(UserSettings.Instance.BuildOrderStepPrepareExecutionInterval),
                    EventType = GameTimeEvent.GameTimeEventType.BuildOrderNotification
                };
                timerAction = () => CustomEvents.Instance.Add(EventsType.BuildOrderItemPrepareExecution, item.Index);
                e.OnEvent += timerAction;
                events.Add(e);

                e = new GameTimeEvent()
                {
                    Time = item.Time,
                    EventType = GameTimeEvent.GameTimeEventType.BuildOrderNotification
                };
                timerAction = () => CustomEvents.Instance.Add(EventsType.BuildOrderItemTriggered, item.Index);
                e.OnEvent += timerAction;
                events.Add(e);

                e = new GameTimeEvent()
                {
                    Time = item.Time.Add(UserSettings.Instance.BuildOrderStepActiveInterval),
                    EventType = GameTimeEvent.GameTimeEventType.BuildOrderNotification
                };
                timerAction = () => CustomEvents.Instance.Add(EventsType.BuildOrderItemFinished, item.Index);
                e.OnEvent += timerAction;
                events.Add(e);
            }

            var completionEvent = new GameTimeEvent()
            {
                Time = _build.Max(i => i.Time),
                Enabled = true,
                EventType = GameTimeEvent.GameTimeEventType.BuildOrderNotification
            };

            completionEvent.OnEvent += () => CustomEvents.Instance.Add(EventsType.BuildOrderCompleted);
            events.Add(completionEvent);

            GameTimeEventHandler.Instance.AddEvents(events);
        }

        public void Interrupt()
        {
            if (!Started) return;
            switch (_build.ExecutionExecutionType)
            {
                case BuildOrder.BuildOrderExecutionType.Time:
                    break;
                case BuildOrder.BuildOrderExecutionType.Step:
                    CustomEvents.Instance.Add(EventsType.BuildOrderItemFinished, _currentStepIndex);
                    _currentStepIndex++;
                    if (_currentStepIndex < _build.Count)
                        CustomEvents.Instance.Add(EventsType.BuildOrderItemTriggered, _currentStepIndex);

                    if (_currentStepIndex + 1 >= _build.Count)
                        CustomEvents.Instance.Add(EventsType.BuildOrderCompleted);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string GetItemText(int details)
        {
            return _build.Count > details ? _build[details].Message : string.Empty;
        }
    }
}
