using System;
using System.Collections.Specialized;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Probe.Game;
using Probe.Properties;
using Probe.Replay;
using Probe.Utility;
using Probe.WebClient;

namespace Probe.Engine
{
    public class UploadReplayTask : IAsyncTask
    {
        public void Run()
        {
            try
            {
                var qs = new QueryString().Add("hash", ReplayInfo.Hash);
                qs.Add("profile", ReplayInfo.ProfileNumber.ToString());
                qs.Add("client_instance", Settings.Default.InstanceCode);
                switch (UploadType)
                {
                    case ReplayUploadType.CurrentGame:
                        if (GameLog.Instance.ReplayAttached)
                        {
                            GameLog.Instance.Clear();
                            Processor.AddUnique(new SendGameTask());
                            Processor.AddUnique(new UploadReplayTask() { ReplayInfo = ReplayInfo });
                            return;
                        }

                        if (string.IsNullOrEmpty(GameLog.Instance.GameId))
                        {
                            Processor.AddUnique(new SendGameTask());
                            Processor.AddUnique(new UploadReplayTask() { ReplayInfo = ReplayInfo });
                            return;
                        }
                        var r = WebLayer.UploadFile(ReplayInfo.FileInfo.FullName, WebLayer.GetPredefinedUrl(WebLayer.PredefinedUrl.UploadReplay), qs.Add("game", GameLog.Instance.GameId).ToString());
                        GameLog.Instance.ReplayAttached = true;
                        Success = r["success"].Value<bool>();
                        break;
                    case ReplayUploadType.BulkUpload:
                        var q = WebLayer.UploadFile(ReplayInfo.FileInfo.FullName, WebLayer.GetPredefinedUrl(WebLayer.PredefinedUrl.UploadReplay), qs.ToString());
                        GameLog.Instance.ReplayAttached = true;
                        Success = q["success"].Value<bool>();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }
            catch (Exception ex)
            {
                CustomEvents.Instance.AddException(EventsType.GeneralError, string.Format("upload replay file '{0}'", ReplayInfo), ex);
            }
        }

        public void OnComplete()
        {
            if (Success) CustomEvents.Instance.Add(EventsType.ReplayFileUploaded);
            Debug.Print("UploadReplayTask completed");
            IsCompleted = true;
        }

        public bool Success { get; private set; }

        public AsyncTaskProcessor Processor { get; set; }
        public ReplayInfo ReplayInfo { get; set; }
        public enum ReplayUploadType
        {
            CurrentGame,
            BulkUpload
        }
        public ReplayUploadType UploadType { get; set; }
        public bool IsCompleted { get; private set; }
    }
}
