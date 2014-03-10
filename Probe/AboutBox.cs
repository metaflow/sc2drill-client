using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Probe.Properties;
using Probe.Utility;
using Probe.WebClient;

namespace Probe
{
    partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
            lTitle.Text = String.Format("desktop client '{0}' {1} ({2})", AssemblyProduct, AssemblyVersion, Settings.Default.InstanceCode);
            foreach (Control control in this.Controls)
            {
                if (!(control is LinkLabel)) control.MouseUp += AboutBox_MouseUp;
            }
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

        private void AboutBox_MouseUp(object sender, MouseEventArgs e)
        {
            Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            WebLayer.OpenBrowser(WebLayer.PredefinedUrl.Root);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            WebLayer.OpenBrowser("http://joelbelessa.deviantart.com/art/Protoss-Probe-FanArt-195768612");

        }

        private void AboutBox_Deactivate(object sender, EventArgs e)
        {
            Close();
        }

        private void linkLicense_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var path = "eula.txt";
            var suggestions = new List<string>();
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (directory != null) suggestions.Add(Path.Combine(directory, path));
            directory = Path.GetDirectoryName(directory);
            if (directory != null)
            {
                suggestions.Add(Path.Combine(directory, path));
            }
            foreach (var suggestion in suggestions)
            {
                if (File.Exists(suggestion))
                {
                    System.Diagnostics.Process.Start(suggestion);
                    return;       
                }
            }
            CustomEvents.Instance.Add(EventsType.Message, "Could not found file eula.txt.\r\nPlease use contact form to get an agreement.");
        }
    }
}
