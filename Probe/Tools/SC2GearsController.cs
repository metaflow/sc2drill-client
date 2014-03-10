using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Probe.Common;
using Probe.WebClient;

namespace Probe.Tools
{
    internal class SC2GearsController : IToolController
    {
        private const string ExecutableName = "Sc2gears.exe";
        private const string ToolFolderName = "Sc2gears";
        //TODO:Need to add reference to our server
        private const string DownloadPath = @"http://download60.mediafire.com/73lj78csqffg/6fj5253ltwc9xdt/Sc2gears-5.5.zip";
        private readonly string _path;

        public SC2GearsController()
        {
            _path = Path.Combine(Paths.Tools, Path.Combine(ToolFolderName, ExecutableName));
        }

        string IToolController.Name
        {
            get { return "Sc2Gears"; }
        }

        string IToolController.Description
        {
            get { return "Tool for detailed replays analysis"; }
        }

        Image IToolController.Icon
        {
            get { return Properties.Resources.sc2gears_16; }
        }

        bool IToolController.IsInstalled
        {
            get { return File.Exists(_path); }
        }

        public bool HasUpdates
        {
            get { return false; }
        }

        void IToolController.Install()
        {
            var dir = Paths.Tools;//Path.GetDirectoryName(_path);

            if(dir == null) return;

            var arc = Path.Combine(dir, "sc2gears.zip");

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            WebLayer.DownloadFile(DownloadPath, arc);

            UnZipFile(arc,true);

            File.Delete(arc);
        }

        private void UnZipFile(string InputPathOfZipFile, bool skipRoot)
        {
            if (File.Exists(InputPathOfZipFile))
            {
                var baseDirectory = Path.GetDirectoryName(InputPathOfZipFile);

                if(string.IsNullOrEmpty(baseDirectory)) return;

                using (var zipStream = new ZipInputStream(File.OpenRead(InputPathOfZipFile)))
                {
                    ZipEntry theEntry;
                    while ((theEntry = zipStream.GetNextEntry()) != null)
                    {
                        if (theEntry.IsFile)
                        {
                            if (!string.IsNullOrEmpty(theEntry.Name))
                            {
                                var strNewFile = Path.Combine(baseDirectory ,theEntry.Name);
                                if (File.Exists(strNewFile))
                                {
                                    continue;
                                }

                                using (var streamWriter = File.Create(strNewFile))
                                {
                                    var data = new byte[2048];
                                    while (true)
                                    {
                                        var size = zipStream.Read(data, 0, data.Length);
                                        if (size > 0)
                                            streamWriter.Write(data, 0, size);
                                        else
                                            break;
                                    }
                                    streamWriter.Close();
                                }
                            }
                        }
                        else if (theEntry.IsDirectory)
                        {
                            string strNewDirectory = @"" + baseDirectory + @"\" + theEntry.Name;
                            if (!Directory.Exists(strNewDirectory))
                            {
                                Directory.CreateDirectory(strNewDirectory);
                            }
                        }
                    }
                    zipStream.Close();
                }
            }
        }

        void IToolController.Run()
        {
            if (File.Exists(_path)) Process.Start(_path);
        }
    }
}
