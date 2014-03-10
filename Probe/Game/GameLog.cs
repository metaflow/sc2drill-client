using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace Probe.Game
{
    class GameLogEntry
    {
        public TimeSpan Time;
        public string Tag;
        public string Action;
        public JToken Data;

        public JObject ToJSON()
        {
            return new JObject() { { "time", Convert.ToInt32(Time.TotalSeconds) }, { "tag", Tag }, { "action", Action }, { "data", Data } };
        }
    }

    class GameLog
    {
        #region singleton
        private static readonly GameLog _instance = new GameLog();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static GameLog()
        {
        }

        GameLog()
        {
        }

        public static GameLog Instance
        {
            get
            {
                return _instance;
            }
        }

        public string GameId { get; set; }

        public bool ReplayAttached { get; set; }

        #endregion

        private List<GameLogEntry> _entries = new List<GameLogEntry>();
        private bool _supplyBlock;
        public bool SupplyBlock
        {
            get { return _supplyBlock; }
            set {
                if (_supplyBlock != value) AddEntry("supply block", value ? "set" : "unset", new JArray());
                _supplyBlock = value;
            }
        }

        public void AddEntry(string tag, string action, JToken data)
        {
            _entries.Add(new GameLogEntry() {Action = action, Data = data, Tag = tag, Time = GameClock.Instance.GetGameTime()});
        }

        public JArray ToJArray()
        {
            var result = new JObject();
            var items = new object[_entries.Count];
            for (int i = 0; i < _entries.Count; i++)
            {
                items[i] = _entries[i].ToJSON();
            }
            return new JArray(items);;
        }

        public void Start()
        {
            Clear();
        }

        public void Clear()
        {
            _entries.Clear();
            GameId = string.Empty;
            ReplayAttached = false;
        }
    }
}
