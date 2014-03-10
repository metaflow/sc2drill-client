using System;
using System.IO;

namespace Probe.Common
{
    public static class SimpleLog
    {
        public static event Action<string> OnLog;
        public static event Action<string> OnTempLog;
        private static object _logFileLock = new object();
        public static string LogFilePath = "app.log";

        public static void LogTemp(string s)
        {
            Action<string> handler = OnTempLog;
            if (handler != null) handler(s);
        }

        public static void Log(string s)
        {
            Action<string> handler = OnLog;
            if (handler != null) handler(s);
            LogTemp(s);

            lock (_logFileLock)
            {
                try
                {
                    using (var writer = new StreamWriter(LogFilePath, true))
                    {
                        writer.WriteLine(string.Format("{0:o}: {1}", DateTime.Now, s));
                        writer.Close();
                    }
                }
                catch { }
            }
        }
    }
}
