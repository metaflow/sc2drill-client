using System;
using System.Collections.Generic;
using Probe.Common;
using Probe.Game;
using Probe.Utility;

namespace Probe.Estimators
{
    class BoostNotifier : IKeyCombinationListener
    {
        private bool _waitingForQueen = false;
        private PlayerRace _race;
        public PlayerRace Race { get { return _race; } set { 
            _race = value;
            _waitingForQueen = false;
        } }

        internal enum BoostKeyboardEvent
        {
            Restart,
            QueenStarted
        }

        public void ProcessKeysMatchEvent(List<KeyboardEventContext> matchedCodes)
        {
            foreach (var code in matchedCodes)
            {
                switch ((BoostKeyboardEvent)code.EventCode)
                {
                    case BoostKeyboardEvent.Restart:
                        RestartChronoPeriod();
                        break;
                    case BoostKeyboardEvent.QueenStarted:
                        if (_waitingForQueen)
                        {
                            CreateEvent(GameClock.Instance.GetGameTime().Add(Constants.ZergQueenBuildTime));
                            _waitingForQueen = false;
                        }
                        break;
                }
            }
        }

        public void ProcessKeysMismatchEvent(List<KeyboardEventContext> matched) { }

        public void RestartChronoPeriod()
        {
            if (Race == PlayerRace.None) return;
            CreateEvent(GameClock.Instance.GetGameTime().Add(GameClock.Instance.ConvertEnergyToGameTime(Constants.BoostEnergy[Race])));
        }

        private void CreateEvent(TimeSpan startTime)
        {
            GameTimeEventHandler.Instance.RemoveEvents(GameTimeEvent.GameTimeEventType.BoostNotification);
            var e = new GameTimeRecurringEvent()
            {
                EventType = GameTimeEvent.GameTimeEventType.BoostNotification,
                RepeatInterval = GameClock.Instance.ConvertEnergyToGameTime(Constants.BoostEnergy[Race]),
                Time = startTime
            };
            e.OnEvent += () => CustomEvents.Instance.Add(EventsType.BoostNotify);
            GameTimeEventHandler.Instance.AddEvent(e);
        }

        public void PrepareNotification()
        {
            GameTimeEventHandler.Instance.RemoveEvents(GameTimeEvent.GameTimeEventType.BoostNotification);
            switch (Race)
            {
                case PlayerRace.Protoss:
                    var boostPeriod = GameClock.Instance.ConvertEnergyToGameTime(Constants.BoostEnergy[PlayerRace.Protoss]).TotalMilliseconds;
                    var periods = Math.Floor(GameClock.Instance.GetGameTime().TotalMilliseconds / boostPeriod);
                    CreateEvent(new TimeSpan((long)((periods + 1) * boostPeriod * TimeSpan.TicksPerMillisecond)));
                    break;
                case PlayerRace.Terran:
                    CreateEvent(Constants.TerranStandardFirstMuleTime);
                    break;
                case PlayerRace.Zerg:
                    _waitingForQueen = true;
                    CreateEvent(new TimeSpan(0, 0, 1, 0).Add(Constants.ZergStandardQueenTime)); //if we'll not catch first queen
                    break;
            }
        }
    }
}
