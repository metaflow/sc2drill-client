using System;
using System.Diagnostics;
using System.Drawing;
using Probe.WebClient;

namespace Probe.Tools
{
    internal class WebToolController : IToolController
    {
        private readonly string url;

        public WebToolController(string name, string url, string description = null)
        {
            this.url = url;
            Name = name;
            Description = description;
        }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public Image Icon
        {
            get { return Properties.Resources.link; }
        }

        public bool IsInstalled { get { return true; } }

        public bool HasUpdates
        {
            get { return false; }
        }

        public void Install(){}

        public void Run()
        {
            WebLayer.OpenBrowser(url);
        }
    }
}
