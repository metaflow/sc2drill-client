using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Probe.Game;
using Probe.WebClient;

namespace Probe.Engine
{
    class SendGameTask: IAsyncTask
    {
        public void Run() {}

        public void OnComplete() {}

        public bool Success { get; private set; }

        public AsyncTaskProcessor Processor { get; set; }
    }
}
