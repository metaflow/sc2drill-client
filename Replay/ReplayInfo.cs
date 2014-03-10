using System;
using System.Collections.Generic;
using System.IO;

namespace Probe.Replay
{
    /// <summary>
    /// Represents replay info.
    /// </summary>
    public class ReplayInfo
    {
        public FileInfo FileInfo { get; set; }
        public string Hash { get; set; }
        public long ProfileNumber { get; set; }
        public long GameCTime { get; set; }
        public string MapHash { get; set; }
        public int RecorderIndex { get; set; }

        public string Realm { get; set; }

        public List<ReplayPlayerInfo> Players = new List<ReplayPlayerInfo>();
    }
}