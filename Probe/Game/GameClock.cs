using System;
using System.Diagnostics;
using Probe.Utility;

namespace Probe.Game
{
    class GameClock
    {
        public enum GameSpeedEnum
        {
            Slow, Slower, Normal, Fast, Faster
        }

        private double _speedMultiplier = 1;
        private GameSpeedEnum _speed = GameSpeedEnum.Faster;
        private DateTime _startTime;
        private TimeSpan _stoppedTime = new TimeSpan(0);
        private bool _paused;
        private DateTime _pausedOn;
        private TimeSpan _totalPausedTime;

        public bool Paused
        {
            get { return _paused; }
            set {
                if (_paused == value) return;
                if (value)
                {
                    _stoppedTime = GetRealTime();
                    _pausedOn = DateTime.Now;
                }
                else
                {
                    _totalPausedTime = _totalPausedTime.Add(DateTime.Now.Subtract(_pausedOn));
                }
                _paused = value;
                CustomEvents.Instance.Add(EventsType.GameTimeStateChanged);
            }
        }

        public bool Running
        {
            get { return (Started && !Paused); }
            set
            {
                if (value)
                {
                    if (Started) return;
                    Start();
                }
                else
                {
                    Stop();
                }
            }
        }

        public bool Started { get; private set; }

        #region singleton
        private static readonly GameClock _instance = new GameClock();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static GameClock()
        {
        }

        GameClock()
        {
        }

        public static GameClock Instance
        {
            get
            {
                return _instance;
            }
        }
        #endregion
    
        public TimeSpan ConvertToRealTime(TimeSpan timeSpan)
        {
            return new TimeSpan((long)(timeSpan.Ticks * _speedMultiplier));
        }

        public TimeSpan ConvertToGameTime(TimeSpan timeSpan)
        {
            return new TimeSpan((long)(timeSpan.Ticks / _speedMultiplier));
        }

        public GameSpeedEnum Speed
        {
            get { return _speed; }
            set { 
                _speed = value;
                switch (_speed)
                {
                    case GameSpeedEnum.Slow:
                        _speedMultiplier = 1.66;
                        break;
                    case GameSpeedEnum.Slower:
                        _speedMultiplier = 1.25;
                        break;
                    case GameSpeedEnum.Normal:
                        _speedMultiplier = 1;
                        break;
                    case GameSpeedEnum.Fast:
                        _speedMultiplier = 0.8275;
                        break;
                    case GameSpeedEnum.Faster:
                        _speedMultiplier = 0.725;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
#if DEBUG
                _speedMultiplier = 0.2;
#endif
            }
        }

        private void Start()
        {
            Started = true;
            _startTime = DateTime.Now;
            _totalPausedTime = new TimeSpan(0);
            CustomEvents.Instance.Add(EventsType.GameTimeStateChanged);
        }

        public TimeSpan GetGameTime()
        {
            return ConvertToGameTime(GetRealTime());
        }

        private void Stop()
        {
            _stoppedTime = GetRealTime();
            Started = false;
            _paused = false;
            CustomEvents.Instance.Add(EventsType.GameTimeStateChanged);
        }

        public TimeSpan GetRealTime()
        {
            return Running ? DateTime.Now.Subtract(_startTime).Subtract(_totalPausedTime) : _stoppedTime;
        }

        public TimeSpan ConvertEnergyToGameTime(int energy)
        {
            return new TimeSpan((long)(energy * TimeSpan.TicksPerSecond / 0.5625));
        }
    }
}
