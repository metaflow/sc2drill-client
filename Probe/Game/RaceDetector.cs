using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ImageProcessing;
using Probe.Common;
using Probe.Utility;


namespace Probe.Game
{
    class RaceDetector
    {
        private string _rawInput;
        private PlayerRace _race = PlayerRace.None;
        public PlayerRace Race
        {
            get { return _race; }
            private set {
                if (_race == value) return;
                _race = value;
                if (_race == PlayerRace.None) return;
                CustomEvents.Instance.Add(EventsType.RaceDetected);
            }
        }
        public void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (!(Race == PlayerRace.None && GameClock.Instance.Running)) return;
            _rawInput += KeysHelper.KeyToString(keyEventArgs.KeyCode) + ",";
#if DEBUG
            //Race = PlayerRace.Zerg;
            //CustomEvents.Instance.Add(EventsType.RaceDetected);
            //return;
#endif
            if (GameClock.Instance.GetGameTime().CompareTo(new TimeSpan(0, 0, 2, 0)) > 0) GuessRace();
        }

        private void GuessRace()
        {
            var d = new Dictionary<PlayerRace, List<Regex>>();
            d.Add(PlayerRace.Protoss, new List<Regex>() { new Regex("E"), new Regex("B,G"), new Regex("B,A") });
            d.Add(PlayerRace.Terran, new List<Regex>() { new Regex("B,S"), new Regex("B,B"), new Regex("B,R") });
            d.Add(PlayerRace.Zerg, new List<Regex>() { new Regex("D"), new Regex("V"), new Regex("B,E") });
            var counts = new Dictionary<PlayerRace, Int32>() { { PlayerRace.Protoss, 0 }, { PlayerRace.Terran, 0 }, { PlayerRace.Zerg, 0 } };
            foreach (KeyValuePair<PlayerRace, List<Regex>> pair in d)
            {
                foreach (Regex regex in pair.Value)
                {
                    counts[pair.Key] += regex.Matches(_rawInput).Count;
                }
            }
            var bestCount = 0;
            var r = PlayerRace.None;
            foreach (KeyValuePair<PlayerRace, int> pair in counts)
            {
                if (pair.Value > bestCount)
                {
                    r = pair.Key;
                    bestCount = pair.Value;
                }
            }
            Race = r;
        }

        public void Start()
        {
            _rawInput = ",";
            Race = PlayerRace.None;
        }

        public void DetectByImage(Bitmap image)
        {
            var barColor = ImageProcessor.DetectVerticalLinesColor(image);
            CustomEvents.Instance.AddLog(string.Format("detected bar color {0}", barColor));
            var bestDiff = 100;
            var bestRace = PlayerRace.None;
            foreach (var raceColor in Constants.RaceColors)
            {
                var d = ImageProcessor.GetColorDistance(barColor, raceColor.Value);
                if (d >= bestDiff) continue;
                bestRace = raceColor.Key;
                bestDiff = d;
            }
            Race = bestRace;
        }
    }
}
