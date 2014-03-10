using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Probe.Common;
using Probe.Replay;

namespace Probe.Utility
{
    class ReplayFileWatcher : FileSystemWatcher
    {
        public ReplayFileWatcher()
        {
            Filter = Paths.ReplayPattern;
            
            Path = Paths.Sc2Directory;
#if DEBUG
            Path = @"c:\";
#endif
            IncludeSubdirectories = true;
            Created += ReplayFileCreated;
        }
        
        void ReplayFileCreated(object sender, FileSystemEventArgs e)
        {
            Debug.Print("replay found {0}", e.FullPath);
            if (!File.Exists(e.FullPath)) return;
#if !DEBUG
            if (DateTime.Now.Subtract(File.GetLastWriteTime(e.FullPath)).TotalHours > 1) return;
#endif
            CustomEvents.Instance.Add(EventsType.ReplayFileCreated, ReplayParser.Parse(e.FullPath));
        }
    }
}
