using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Probe.Properties;
using Probe.Utility;

namespace Probe
{
    static class Program
    {
        private static bool _errorInProcess;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var resourceName = string.Format("Probe.{0}.dll", new AssemblyName(args.Name).Name);
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    var assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };
            Application.EnableVisualStyles();
            Application.ThreadException += ApplicationThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            Application.SetCompatibleTextRenderingDefault(false);
            
            Application.Run(new MainForm());
        }

        public static void RestartApp()
        {
            var parentDir = Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            if (!string.IsNullOrEmpty(parentDir))
            {
                var launcherPath = Path.Combine(parentDir, "launcher.exe");

                if (File.Exists(launcherPath))
                {
                    Process.Start(launcherPath);
                    Application.Exit();
                }
                else
                {
                    var errMessage = string.Format("Wrong launcher path: [{0}]", launcherPath);
                    CustomEvents.Instance.AddException(EventsType.GeneralError, errMessage, new Exception());
                }
            }
        }

        public static string GetFramevorkVersion()
        {
            try
            {
                var installedVersions = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP");

                if (installedVersions != null)
                {
                    var versionNames = installedVersions.GetSubKeyNames();

                    var framework = versionNames[versionNames.Length - 1];

                    var sp = (int)installedVersions.OpenSubKey(framework).GetValue("SP", 0);

                    var spStr = sp == 0 ? "" : string.Format(" SP{0}", sp);

                    return string.Format("{0}{1}", framework, spStr);

                }
            }
            catch (Exception ex)
            {
                return string.Format("Unknown - {0}", ex.Message);
            }

            return "Unknown";
        }

        static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                UnhandledException((Exception)e.ExceptionObject);
            } finally
            {
               Application.Exit();   
            }
        }

        static void ApplicationThreadException(object sender, ThreadExceptionEventArgs args)
        {
            try
            {
                UnhandledException(args.Exception);
            }
            finally
            {
               Application.Exit();   
            }
        }

        static void UnhandledException(Exception e)
        {
            try
            {
                if (_errorInProcess) return; //prevent recursion in error processing
                _errorInProcess = true;
                CustomEvents.Instance.AddException(EventsType.GeneralError, "", e);
                Thread.Sleep(5000);
            }
            catch
            {
                try
                {
                    MessageBox.Show(@"Fatal Error. Application terminated", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
                finally
                {
                    Application.Exit();
                }
            }
        }
    }
}
