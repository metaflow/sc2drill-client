using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Probe.Engine;
using Probe.Replay;
using Probe.Utility;
using Probe.WebClient;

namespace sc2test
{
    [TestClass]
    public class ServerCommunicationsTest
    {
        private AsyncTaskProcessor tasks;

        private JTokenWrap TestRequest(string caseName)
        {
            var r = WebLayer.JSONRequest("unit_test", new JObject() { { "case", caseName } });
            var w = new JTokenWrap(r);
            Assert.IsTrue(w.GetBool("success", false));
            return w;
        }

        [TestMethod]
        public void Initialization()
        {
            if (tasks == null) tasks = new AsyncTaskProcessor();
            ServerConnection.Instance.TaskProcessor = tasks;
            ServerConnection.Instance.Connect();
            while (tasks.Busy) Thread.Sleep(1000);
            Assert.IsTrue(ServerConnection.Instance.Connected);
            var w = TestRequest("get_username");
            Assert.AreEqual("usera", w.GetString("data.name", ""));
            TestRequest("clear_all_data");
            w = TestRequest("get_games_count");
            Assert.AreEqual(0, w.GetInteger("data.count",-1));
        }

        [TestMethod]
        public void InitializeBnAccountsTest()
        {
            Initialization();
            var w = TestRequest("get_bn_accounts");
            var accounts = w.GetArray("data.accounts", new JArray() {"not empty"});
            Assert.AreEqual(0, accounts.Count);
            w = new JTokenWrap(WebLayer.JSONRequest("unit_test", new JObject() { { "case", "set_bn_accounts" }, { "accounts", new JArray() { new JObject() { { "bnet_id", "12554" } } } } }));
            Assert.IsTrue(w.GetBool("success", false));
            w = TestRequest("get_bn_accounts");
            accounts = w.GetArray("data.accounts", new JArray());
            Assert.AreEqual(1, accounts.Count);
            Assert.AreEqual("12554", new JTokenWrap(accounts).GetString("[0].bnet_id", ""));
        }

        [TestMethod]
        public void UploadInGameReplay()
        {
            Initialization();
            var tempPath = Path.ChangeExtension(Path.GetTempFileName(), "sc2replay");
            File.WriteAllBytes(tempPath, Resources.meta_1);
            var t = new UploadReplayTask()
                        {
                            ReplayInfo = ReplayParser.Parse(tempPath),
                            UploadType = UploadReplayTask.ReplayUploadType.CurrentGame
                        };
            tasks.AddUnique(t);
            while (tasks.Busy) Thread.Sleep(1000);
            File.Delete(tempPath);
            var w = TestRequest("get_bn_accounts");
            var accounts = w.GetArray("data.accounts", new JArray());
            Assert.AreEqual(1, accounts.Count);
            Assert.AreEqual("1860931", new JTokenWrap(accounts).GetString("[0].bnet_id", ""));
            w = TestRequest("get_games_count");
            Assert.AreEqual(1, w.GetInteger("data.count", -1));
        }


        private void UploadReplay(byte[] replayData, UploadReplayTask.ReplayUploadType uploadType)
        {
            var tempPath = Path.ChangeExtension(Path.GetTempFileName(), "sc2replay");
            File.WriteAllBytes(tempPath, replayData);
            var t = new UploadReplayTask()
            {
                ReplayInfo = ReplayParser.Parse(tempPath),
                UploadType = uploadType
            };
            tasks.AddUnique(t);
            while (tasks.Busy) Thread.Sleep(1000);
            File.Delete(tempPath);
        }

        [TestMethod]
        public void UploadBulkGameReplay1()
        {
            Initialization();
            UploadReplay(Resources.meta_1, UploadReplayTask.ReplayUploadType.CurrentGame);
            var w = TestRequest("get_bn_accounts");
            var accounts = w.GetArray("data.accounts", new JArray());
            Assert.AreEqual(1, accounts.Count);
            Assert.AreEqual("1860931", new JTokenWrap(accounts).GetString("[0].bnet_id", ""));
            w = TestRequest("get_games_count");
            Assert.AreEqual(1, w.GetInteger("data.count", -1));
            UploadReplay(Resources.meta_2, UploadReplayTask.ReplayUploadType.BulkUpload);
            w = TestRequest("get_games_count");
            Assert.AreEqual(2, w.GetInteger("data.count", -1));
        }

        [TestMethod]
        public void UploadBulkGameReplay2()
        {
            Initialization();
            UploadReplay(Resources.meta_1, UploadReplayTask.ReplayUploadType.CurrentGame);
            var w = TestRequest("get_bn_accounts");
            var accounts = w.GetArray("data.accounts", new JArray());
            Assert.AreEqual(1, accounts.Count);
            Assert.AreEqual("1860931", new JTokenWrap(accounts).GetString("[0].bnet_id", ""));
            w = TestRequest("get_games_count");
            Assert.AreEqual(1, w.GetInteger("data.count", -1));
            UploadReplay(Resources.other_1, UploadReplayTask.ReplayUploadType.BulkUpload);
            w = TestRequest("get_games_count");
            Assert.AreEqual(1, w.GetInteger("data.count", -1));
        }
    }
}
