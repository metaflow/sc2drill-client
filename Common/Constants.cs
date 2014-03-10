using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Probe.Common
{
    public static class Constants
    {
        public static readonly TimeSpan UINotificationIconHideTime = new TimeSpan(0, 0, 0, 15);
        public static readonly Dictionary<PlayerRace, string> BoostNotification = new Dictionary<PlayerRace, string>()
                                                                                  {
            {PlayerRace.Protoss, "boost"},
            {PlayerRace.Terran, "mule"},
            {PlayerRace.Zerg, "inject"}
        };
        public static readonly Dictionary<PlayerRace, int> BoostEnergy = new Dictionary<PlayerRace, int>()
                                                                                  {
            {PlayerRace.Protoss, 25},
            {PlayerRace.Terran, 50},
            {PlayerRace.Zerg, 25}
        };

        public static readonly TimeSpan TerranStandardFirstMuleTime = new TimeSpan(0,0,3,20);
        public static readonly TimeSpan ZergStandardQueenTime = new TimeSpan(0, 0, 4, 10);
        public static readonly TimeSpan ZergQueenBuildTime = new TimeSpan(0, 0, 0, 50);
        public static Dictionary<PlayerRace, Color> RaceColors = new Dictionary<PlayerRace, Color>()
                                                                             {
            {PlayerRace.Zerg, Color.FromArgb(206, 118, 59)},
            {PlayerRace.Terran, Color.FromArgb(34, 145, 57)},
            {PlayerRace.Protoss, Color.FromArgb(10, 126, 210)}
    };
    }
}
