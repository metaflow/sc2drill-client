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
        public void Run()
        {
            var r = WebLayer.JSONRequest("add_game", new JObject() {
                {"events", GameLog.Instance.ToJArray()},
                {"length", Convert.ToInt32(GameClock.Instance.GetGameTime().TotalSeconds)}
            });
            Success = r["success"].Value<bool>();
            if (Success)
            {
                if (r.SelectToken("data.game_id") != null)
                {
                    GameLog.Instance.GameId = r.SelectToken("data.game_id").Value<string>();
                } else
                {
                    GameLog.Instance.GameId = "0";
                }
            }
            
        }

        public void OnComplete() {}

        public bool Success { get; private set; }

        public AsyncTaskProcessor Processor { get; set; }
    }
}
