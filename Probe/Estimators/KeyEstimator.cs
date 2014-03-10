using System;
using System.Collections.Generic;
using Probe.Game;
using Probe.Utility;

namespace Probe.Estimators
{
    class KeyEstimator : RegularityEstimator, IKeyCombinationListener
    {
        public void ProcessKeysMatchEvent(List<KeyboardEventContext> matchedCodes)
        {
            if (!GameClock.Instance.Running) return;
            AddEvent();
        }

        public void ProcessKeysMismatchEvent(List<KeyboardEventContext> matched)
        {
            
        }
    }
}
