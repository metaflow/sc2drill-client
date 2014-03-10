using System;
using System.Diagnostics;
using Probe.WebClient;
using Newtonsoft.Json.Linq;

namespace Probe.Engine
{
    class UpdateSessionActivity : IAsyncTask
    {
        public void Run()
        {
            Debug.Print("Update activity");
            Success = false;
            if (!ServerConnection.Instance.Connected) return;
            var r = WebLayer.JSONRequest("update_activity", null);
            Success = r["success"].Value<bool>();
        }

        public void OnComplete()
        {
            ServerConnection.Instance.Connected = Success;
            if (!Success) return;
        }

        public bool Success { get; private set; }

        public AsyncTaskProcessor Processor { get; set; }
    }
}