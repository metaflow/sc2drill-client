using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Probe.Common;
using Probe.WebClient;

namespace Probe.Tools
{
    internal class SCFusionController : IToolController
    {
        private const string ExecutableName = "SC2Fusion.exe";
        private const string ToolFolderName = "CarbonTwelve's Starcraft Fusion";
        private const string UpdateFileName = "sc2fupd";
        private const string DownloadPath = @"http://scbuildorder.googlecode.com/files/SCFusion_v0.5.exe";
        private string _path;
        private string _updatePath;
        private BackgroundWorker _updDownloader;

        public SCFusionController()
        {
            var toolPath = Path.Combine(Paths.Tools, ToolFolderName);

            _path = Path.Combine(toolPath, ExecutableName);
            _updatePath = Path.Combine(toolPath, UpdateFileName);
        }


        string IToolController.Name
        {
            get { return "Starcraft Fusion"; }
        }

        string IToolController.Description
        {
            get { return "Tool to calculate build orders"; }
        }

        public Image Icon
        {
            get { return Properties.Resources.sc2fusion_16; }
        }

        bool IToolController.IsInstalled
        {
            get { return File.Exists(_path); }
        }

        public bool HasUpdates
        {
            get
            {
                if (File.Exists(_updatePath))
                {
                    return true;
                }

                if (_updDownloader == null || !_updDownloader.IsBusy)
                {
                    _updDownloader = new BackgroundWorker();
                    _updDownloader.DoWork += _updDownloader_DoWork;
                    _updDownloader.RunWorkerAsync();
                }

                return false;
            }
        }

        void _updDownloader_DoWork(object sender, DoWorkEventArgs e)
        {
            //TODO: Implement check update logic and download update to _updatePath
            //WebLayer.DownloadFile(_updateUrl,_updatePath);
        }

        void IToolController.Install()
        {
            if(HasUpdates)
            {
                File.Copy(_updatePath,_path,true);
                File.Delete(_updatePath);
                return;
            }

            var dir = Path.GetDirectoryName(_path);

            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            WebLayer.DownloadFile(DownloadPath, _path);
        }

        void IToolController.Run()
        {
            if (File.Exists(_path)) Process.Start(_path);
        }
    }
}
