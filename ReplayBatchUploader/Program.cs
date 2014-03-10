using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Probe;
using Probe.Common;
using Probe.Engine;
using Probe.WebClient;

namespace ReplayBatchUploader
{
    class Program
    {
        static void Main(string[] args)
        {

            // parse arguments
            var r = new Regex(@"-{1,2}([^=]*)=?""?(.*)""?");

            var parameters = new Dictionary<string, string>();

            foreach (string s in args)
            {
                var m = r.Match(s);
                if (m.Success) parameters.Add(m.Groups[1].ToString().ToLower(), m.Groups[2].ToString());
            }

            if (!parameters.ContainsKey("d"))
            {
                PrintHelp();
                return;
            }

            var d = parameters["d"];

            if (!Directory.Exists(d))
            {
                Console.WriteLine("Directory {0} does not exsist", d);
                Console.ReadLine();
                return;
            }

            var files = Directory.EnumerateFiles(d, Paths.ReplayPattern, parameters.ContainsKey("r") ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            var tasks = new AsyncTaskProcessor();
            ServerConnection.Instance.TaskProcessor = tasks;
            ServerConnection.Instance.Connect();

            var count = 0;

            foreach (var f in files)
            {
                //BUG: Temporary disabled
                //tasks.AddUnique(new UploadReplayTask() {  ReplayInfo = f });
                Console.WriteLine("uploading {0}..", f);
                while (tasks.Busy) Thread.Sleep(1000);
                count++;
            }

            Console.WriteLine("{0} files added to upload queue", count);
            
            Console.WriteLine("completed");
            Console.ReadLine();
        }

        private static void PrintHelp()
        {
            Console.WriteLine("usage:\r\n--d <directory with replays>\r\n[--r] - recoursive");
            Console.ReadLine();
        }
    }
}
