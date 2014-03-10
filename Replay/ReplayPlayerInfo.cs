using System;

namespace Probe.Replay
{
    public class ReplayPlayerInfo
    {
        public string Name { get; set; }
        public bool IsHuman { get; set; }
        public UInt32 Uid { get; set; }
        public int Index { get; set; }
        public string Realm { get; set; }
        public string League { get; set; }
        public int Rank { get; set; }
    }
}
