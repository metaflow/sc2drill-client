using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Probe.Game;
using Probe.Replay;
using Probe.Utility;
using Probe.WebClient;

namespace Probe.Engine
{
    public class BulkUploadReplaysTask : IAsyncTask
    {
        private List<UploadReplayTask> _tasks = new List<UploadReplayTask>();
        public DateTime LastUploadTime { get; set; }

        public void Run()
        {
            var uploadTasks = new AsyncTaskProcessor();
            var now = DateTime.Now;

            var replays = ReplayUtils.GetReplaysToUpload(UserSettings.Instance.LastBuldReplayUploadTime);

            foreach (var replayInfo in replays)
            {
                var task = new UploadReplayTask {ReplayInfo = replayInfo, UploadType = UploadReplayTask.ReplayUploadType.BulkUpload};
                uploadTasks.Add(task);
                _tasks.Add(task);
            }

            //awaiting all replays will be uploaded
            while (uploadTasks.Busy) Thread.Sleep(1000);

            if(_tasks.Where(t=>!t.Success).FirstOrDefault() == null)
            {
                WebLayer.UpdateBulkUploadTime(now);
            }
        }

        public void OnComplete()
        {
         
        }

        public bool Success
        {
            get { return true; }
        }

        public AsyncTaskProcessor Processor { get; set; }
    }
}
