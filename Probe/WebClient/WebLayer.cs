using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Probe.Properties;
using Probe.Utility;

namespace Probe.WebClient
{
    public static class WebLayer
    {
        public const string ProtocolVersion = "1";
        
        public static string HttpPost(string uri, string parameters)
        {
            //CustomEvents.AddLog(string.Format("HttpPost >> {0} {1}", uri, parameters));
            // parameters: name1=value1&name2=value2	
            WebRequest webRequest = WebRequest.Create(uri);
            //string ProxyString = 
            //   System.Configuration.ConfigurationManager.AppSettings
            //   [GetConfigKey("proxy")];
            //webRequest.Proxy = new WebProxy (ProxyString, true);
            //Commenting out above required change to App.Config
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            byte[] bytes = Encoding.ASCII.GetBytes(parameters);
            Stream os = null;
            try
            { // send the Post
                webRequest.ContentLength = bytes.Length;   //Count bytes to send
                os = webRequest.GetRequestStream();
                os.Write(bytes, 0, bytes.Length);         //Send it
                WebResponse webResponse = webRequest.GetResponse();
                if (webResponse == null)
                { return null; }
                StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                var result = sr.ReadToEnd().Trim();
                //CustomEvents.AddLog(string.Format("HttpPost << {0}", result));
                return result;
            }
            catch (WebException ex)
            {
                if (ex.Message == "The remote server returned an error: (503) Server Unavailable.")
                {
                    CustomEvents.Instance.Add(EventsType.ServerUnavailable);
                }
                else
                {
                    CustomEvents.Instance.AddException(EventsType.ConnectionError, "", ex);
                }
            }
            finally
            {
                if (os != null)
                {
                    os.Close();
                }
            }

            return null;
        } // end HttpPost

        public static void OpenBrowser(string s)
        {
            try
            {
                Process.Start(s);
            }
            catch (Exception)
            {
                MessageBox.Show(string.Format("Cannot open default browser. Please open\n\n{0}\n\nin your browser", s), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        public static void DownloadFile(string uri, string targetPath)
        {
            var c = new System.Net.WebClient();
            c.DownloadFile(uri, targetPath);
        }
    }
}
