using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Probe.Game;

namespace Probe.Estimators
{
    class RegularityEstimator
    {
        public event Action<JToken> OnEvent;
        public List<TimeSpan> TimeMarks = new List<TimeSpan>();
        public TimeSpan MinimalTimespan = new TimeSpan(0);
        private TimeSpan _latestTapTime;

        private bool _active;

        public void Start()
        {
            TimeMarks.Clear();
            TimeMarks.Add(new TimeSpan(0));
            _latestTapTime = new TimeSpan(0);
            _active = true;
        }

        public void AddEvent(string context)
        {
            if (!_active) return;

            OnEvent(context);

            var t = GameClock.Instance.GetGameTime().Subtract(_latestTapTime);

            if (t.CompareTo(MinimalTimespan) <= 0) return;

            _latestTapTime = GameClock.Instance.GetGameTime();
            TimeMarks.Add(_latestTapTime);
        }

        protected void AddEvent()
        {
            AddEvent("");
        }
    }
}
