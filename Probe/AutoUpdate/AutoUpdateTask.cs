using System;
using Probe.Engine;

namespace Probe.AutoUpdate
{
    public class AutoUpdateTask : IAsyncTask
    {
        public void Run()
        {
           AutoUpdater.CheckUpdates();

            Success = true;
        }

        public void OnComplete()
        {
            
        }

        public bool Success { get; private set; }

        public AsyncTaskProcessor Processor { get; set; }
    }
}
