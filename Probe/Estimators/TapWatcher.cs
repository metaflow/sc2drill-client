using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Probe.Estimators;
using Probe.Game;
using Probe.Utility;

namespace Probe.Estimators
{
    internal class TapWatcher : RegularityEstimator, IKeyCombinationListener
    {
        public enum KeyEventModifier
        {
            None = 0x0000,
            Add = 0x1000,
            Remove = 0x2000
        }

        private List<int> _targetItems = new List<int>();
        private List<int> _pressedKeys = new List<int>();

        public event Action<List<int>> OnTargetKeysChange = delegate { return; };

        private void OnTapCycleCompleted()
        {
            AddEvent(string.Join(",", _pressedKeys.Select(x => x.ToString()).ToArray()));
            _pressedKeys.Clear();
        }
        
        public new void Start()
        {
            base.Start();
            _pressedKeys.Clear();
            _targetItems.Clear();
            OnTargetKeysChange(_targetItems);
        }

        public void ProcessKeysMatchEvent(List<KeyboardEventContext> matchedCodes)
        {
            if (!GameClock.Instance.Running) return;
            foreach (KeyboardEventContext context in matchedCodes)
            {
                var modifier = KeyEventModifier.None;
                if (context.Data != null) modifier = (KeyEventModifier)context.Data;
                var value = context.EventCode;
                if ((modifier == KeyEventModifier.Add) && (!_targetItems.Contains(value)))
                {
                    _targetItems.Add(value);
                    //Debug.Print("production add key {0}", value);
                    GameLog.Instance.AddEntry("tap", "add key", value.ToString());
                    OnTargetKeysChange(_targetItems);
                }
                if ((modifier == KeyEventModifier.Remove) && (_targetItems.Contains(value)))
                {
                    _targetItems.Remove(value);
                    _pressedKeys.Remove(value);
                    OnTargetKeysChange(_targetItems);
                    //Debug.Print("production remove key {0}", value);
                    GameLog.Instance.AddEntry("tap", "remove key", value.ToString());
                    if (_pressedKeys.Count == _targetItems.Count)
                    {
                        OnTapCycleCompleted();
                        continue;
                    }
                }
                if (!_targetItems.Contains(value) || _pressedKeys.Contains(value)) continue;
                _pressedKeys.Add(value);
                //Debug.Print("production key pressed {0}", value);
                if (_pressedKeys.Count < _targetItems.Count) continue;
                OnTapCycleCompleted();
            }
        }

        public void ProcessKeysMismatchEvent(List<KeyboardEventContext> matched)
        {
            
        }
    }
}
