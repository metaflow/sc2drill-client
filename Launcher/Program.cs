using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Forms;

namespace Launcher
{
    static class Program
    {
        private const string Sc2ExecutableName = "StarCraft II.exe";
        private const string UpdaterTypeName = "Probe.AutoUpdate.AutoUpdater";
        private const string VerifyMethodName = "Verify";
        private const string ExecutableName = "probe.exe";

        private const string LaunchStarcraftKey = "ls";
        private const int MaxSearchLevel = 3;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ProcessArguments(args);

            var dir = GetVersionFolder();
            if (Directory.Exists(dir)) Process.Start(Path.Combine(dir, ExecutableName));
        }

        private static void ProcessArguments(IEnumerable<string> args)
        {
            foreach (var arg in args)
            {
                try
                {
                    var command = arg.Replace("--", "").Trim();

                    if (command.Equals(LaunchStarcraftKey))
                    {
                        LaunchStarcraft();
                    }
                }
                catch (Exception ex)
                {
                    WriteEventToWindowsLog(ex.Message);
                }
            }
        }

        private static void LaunchStarcraft()
        {
            var sc2Path = Properties.Settings.Default.SC2Path;

            //find in default location
            if (!File.Exists(sc2Path))
            {
                sc2Path = FindSc2Executable(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "StarCraft II\\"));
            }

            //find in program files
            if (!File.Exists(sc2Path))
            {
                sc2Path = FindSc2Executable(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            }

            //find in launcher.exe root 
            if (!File.Exists(sc2Path))
            {
                sc2Path = FindSc2Executable(Directory.GetDirectoryRoot(Assembly.GetEntryAssembly().Location));
            }

            if (File.Exists(sc2Path))
            {
                Process.Start(sc2Path);

                Properties.Settings.Default.SC2Path = sc2Path;
                Properties.Settings.Default.Save();
            }
        }

        private static string FindSc2Executable(string initialPath, int level = 0)
        {
            try
            {
                if (level >= MaxSearchLevel) return string.Empty;

                var files = Directory.GetFiles(initialPath, Sc2ExecutableName, SearchOption.TopDirectoryOnly);

                foreach (var file in files)
                {
                    if (File.Exists(file)) return file;
                }

                var directoryes = Directory.GetDirectories(initialPath);

                foreach (var directory in directoryes)
                {
                    try
                    {
                        var path = FindSc2Executable(directory, level + 1);

                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }
                    catch (UnauthorizedAccessException) { }
                }
            }
            catch (Exception ex)
            {
                WriteEventToWindowsLog(ex.Message);
            }

            return string.Empty;
        }

        private static Type GetUpdater(string dir)
        {
            var files = new List<string>();

            files.AddRange(Directory.GetFiles(dir, "*.dll"));
            files.AddRange(Directory.GetFiles(dir, "*.exe"));

            foreach (var file in files)
            {
                var asm = Assembly.LoadFrom(file);

                var tUpdater = asm.GetType(UpdaterTypeName);

                if (tUpdater != null) return tUpdater;
            }

            return null;
        }

        private static string GetVersionFolder()
        {
#if DEBUG
            //get local version for in debug mode
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
#endif
            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (currentPath != null)
            {
                var dirs = new List<string>(Directory.GetDirectories(currentPath));

                dirs = dirs.OrderByDescending(dir => dir).ToList();

                foreach (var dir in dirs)
                {
                    if (VerifyVersion(dir)) return dir;
                }
            }

            return string.Empty;
        }

        private static bool VerifyVersion(string dir)
        {
            var result = false;

            try
            {
                //check dir name for version pattern
                if (!Regex.Match(dir, @".+\\\d+\.\d+\.\d+\.\d+").Success) return false;

                var updater = GetUpdater(dir);

                if (updater != null)
                {
                    MethodInfo method;

                    //check type for static
                    if (updater.IsAbstract && updater.IsSealed)
                    {
                        method = updater.GetMethod(VerifyMethodName);
                        result = (bool)method.Invoke(updater, null);
                    }
                    else
                    {
                        var updaterObj = Activator.CreateInstance(updater);
                        method = updater.GetMethod(VerifyMethodName);
                        result = (bool)method.Invoke(updaterObj, null);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteEventToWindowsLog(ex.Message);
            }

            return result;
        }

        private static void WriteEventToWindowsLog(string strEvent)
        {
            const string source = "Probe Launcher";

            if (!EventLog.SourceExists(source)) EventLog.CreateEventSource(source, source);

            var myEventLog = new EventLog();

            myEventLog.Source = source;

            myEventLog.WriteEntry(strEvent, EventLogEntryType.Error);
        }
    }
}
