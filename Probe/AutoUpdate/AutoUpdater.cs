using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using Probe.Utility;
using Probe.WebClient;
using Newtonsoft.Json.Linq;

namespace Probe.AutoUpdate
{
    /// <summary>
    /// Provides methods for auto update application and verify current version.
    /// </summary>
    public static class AutoUpdater
    {
        public static event EventHandler NewVersionInstalled;

        private static BackgroundWorker updateWorker;

        /// <summary>
        /// Verifies this instance.
        /// </summary>
        /// <returns>Returns true if current version is valid otherwise returns false.</returns>
        public static bool Verify()
        {
            return true;
        }

        /// <summary>
        /// Starts the update from URL.
        /// </summary>
        public static void CheckUpdates()
        {
            if (updateWorker != null && updateWorker.IsBusy) return;

            //ask server for a new version
            var r = WebLayer.JSONRequest("check_updates", new JObject() { { "client_version", Assembly.GetExecutingAssembly().GetName().Version.ToString()} });
            var success = r["success"].Value<bool>();
            if (!success) return;
            var backgroundActions = new List<KeyValuePair<string,string>>();
            if (r["data"]["actions"].Type == JTokenType.Array)
            {
                foreach (var action in (JArray)r["data"]["actions"])
                {
                    if (action.Type != JTokenType.String) continue;
                    switch (action.Value<string>())
                    {
                        case "download":
                            if (r["data"]["download"].Type == JTokenType.Array)
                            {
                                foreach (var d in (JArray)r["data"]["download"])
                                {
                                    if (d.Type != JTokenType.String) continue;
                                    backgroundActions.Add(new KeyValuePair<string, string>("download", d.Value<string>()));
                                }
                            }
                            break;
                        case "manual reinstall" :
                            CustomEvents.Instance.Add(EventsType.ClientNeedToBeReinstalled);
                            return;
                        case "restart" :
                            CustomEvents.Instance.Add(EventsType.WillRestartForUpdate);
                            backgroundActions.Add(new KeyValuePair<string, string>("restart", ""));
                            break;
                    }
                }
            }
            
            //install update
            if (backgroundActions.Count == 0) return;
            updateWorker = new BackgroundWorker();
            updateWorker.DoWork += BackgroudUpdate;
            updateWorker.RunWorkerAsync(backgroundActions);
        }

        static void BackgroudUpdate(object sender, DoWorkEventArgs e)
        {
            var actions = (List<KeyValuePair<string, string>>)e.Argument;
            foreach (var action in actions)
            {
                switch (action.Key)
                {
                    case "download":
                        var parentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        parentDirectory = Path.GetDirectoryName(parentDirectory);
                        var segments = new Uri(action.Value).Segments;
                        var path = Path.Combine(parentDirectory, segments[segments.Length - 1]);
                        WebLayer.DownloadFile(action.Value, path);
                        if (path.EndsWith(".zip"))
                        {
                            //extract
                            using (ZipInputStream s = new ZipInputStream(File.OpenRead(path)))
                            {
                                ZipEntry theEntry;
                                while ((theEntry = s.GetNextEntry()) != null)
                                {
                                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                                    string fileName = Path.GetFileName(theEntry.Name);

                                    // create directory
                                    if (directoryName.Length > 0)
                                    {
                                        Directory.CreateDirectory(Path.Combine(parentDirectory, directoryName));
                                    }

                                    if (fileName != String.Empty)
                                    {
                                        using (FileStream streamWriter = File.Create(Path.Combine(parentDirectory, theEntry.Name)))
                                        {
                                            int size = 2048;
                                            byte[] data = new byte[2048];
                                            while (true)
                                            {
                                                size = s.Read(data, 0, data.Length);
                                                if (size > 0)
                                                {
                                                    streamWriter.Write(data, 0, size);
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            //remove
                            File.Delete(path);
                        }
                        //extract
                        break;
                    case "restart":
                        RestartApplication();
                        break;
                }
            }
            //restart
        }

        private static void OnNewVersionInstalled(EventArgs e)
        {
            if(NewVersionInstalled != null) NewVersionInstalled(null, EventArgs.Empty);
        }

        private static void RestartApplication()
        {
            CustomEvents.Instance.Add(EventsType.NewVersionInstalled);
            Thread.Sleep(4000);
            
            Program.RestartApp();
        }
    }
}
