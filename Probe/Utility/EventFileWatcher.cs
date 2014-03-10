using System;
using System.Diagnostics;
using System.IO;

namespace Probe.Utility
{
    class EventFileWatcher : FileSystemWatcher
    {
        public EventFileWatcher()
        {
            Filter = "*.events";
            Path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#if DEBUG
            Path = @"c:\";
#endif
            IncludeSubdirectories = true;
            Created += EventFileCreated;
        }

        void EventFileCreated(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(e.FullPath)) return;
            Debug.Print("event file found {0}", e.FullPath);
            CustomEvents.Instance.Add(EventsType.GameGoingToStart, e.FullPath);
        }
    }
}
    