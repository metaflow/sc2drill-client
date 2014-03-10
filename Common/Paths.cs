using System;
using System.IO;
using System.Reflection;

namespace Probe.Common
{
    public class Paths
    {
        public const string ReplayPattern = "*.SC2Replay";

        public static string Tools
        {
            get { return GetSubDirectiry("Tools"); }
        }

        public static string LogoImagePath
        {
            get { return Path.Combine(Images, "logo.png"); }
        }

        public static string Images
        {
            get { return GetSubDirectiry("Images"); }
        }

        public static string Root
        {
            get
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.RootPath) || !Directory.Exists(Properties.Settings.Default.RootPath))
                {
                    var current = Assembly.GetExecutingAssembly().Location;

                    var root = Directory.GetParent(current).Parent;

                    Properties.Settings.Default.RootPath = root.FullName;
                    Properties.Settings.Default.Save();
                }

                return Properties.Settings.Default.RootPath;
            }
        }

        public static string Sc2Directory
        {
            get
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StarCraft II");
                if (!Directory.Exists(path)) path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                return path;
            }
        }

        private static string GetSubDirectiry(string subDirName)
        {
             var path = Path.Combine(Root, subDirName);
                
                if(!Directory.Exists(path)) Directory.CreateDirectory(path);

                return path;
        }
    }
}
