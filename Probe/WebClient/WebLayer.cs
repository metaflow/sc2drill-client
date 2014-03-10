using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Probe.Properties;
using Probe.Utility;

namespace Probe.WebClient
{
    public static class WebLayer
    {
        public const string ProtocolVersion = "1";

        public enum PredefinedUrl
        {
            Root,
            Service,
            BindClient,
            ForceOpenClientSession,
            DownloadClient,
            Settings,
            UploadReplay,
            Contact
        }

        public static string GetPredefinedUrl(PredefinedUrl url)
        {
            switch (url)
            {
                case PredefinedUrl.Root:
//#if DEBUG
                    try
                    {
                        var ping = new Ping();
                        var reply = ping.Send("sc2.localhost");
                        if (reply.Status == IPStatus.Success) return "http://sc2.localhost";    
                    }catch(PingException){}
                    
                    return "http://sc2drill.com";
//#endif
                    return "http://sc2drill.com";
                case PredefinedUrl.Service:
                    return GetPredefinedUrl(PredefinedUrl.Root) + "/service/sc2";
                case PredefinedUrl.BindClient:
                    return GetPredefinedUrl(PredefinedUrl.Root) + "/register_client/" + Settings.Default.InstanceCode;
                case PredefinedUrl.ForceOpenClientSession:
                    return GetPredefinedUrl(PredefinedUrl.Root) + "/use_client/" + Settings.Default.InstanceCode;
                case PredefinedUrl.DownloadClient:
                    return GetPredefinedUrl(PredefinedUrl.Root) + "/download-client";
                case PredefinedUrl.Settings:
                    return GetPredefinedUrl(PredefinedUrl.Root) + "/settings";
                case PredefinedUrl.UploadReplay:
                    return GetPredefinedUrl(PredefinedUrl.Root) + "/service/upload_replay";
                case PredefinedUrl.Contact:
                    return GetPredefinedUrl(PredefinedUrl.Root) + "/contact";
                default:
                    throw new ArgumentOutOfRangeException("url");
            }
        }

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

        public static JObject JSONRequest(string type, JObject data)
        {
            var result = new JObject { { "success", false } };
            var response = string.Empty;
            try
            {
                var request = new JObject() { { "type", type }, { "data", data }, { "client", new JObject() { { "protocol_version", ProtocolVersion }, { "instance", Settings.Default.InstanceCode } } } };
                response = HttpPost(GetPredefinedUrl(PredefinedUrl.Service), string.Format("parameters={0}", Uri.EscapeDataString(request.ToString(Formatting.None))));
                if (response == null) return result;
                response = response.Replace(@"\x26", "&");
                response = response.Replace(@"\x3e", ">");
                response = response.Replace(@"\x3c", "<");
                result = JObject.Parse(response);
                ProcessServerResponse(result);
            }
            catch (Exception e)
            {
                CustomEvents.Instance.AddException(EventsType.JSONRequestException, response, e);
            }
            return result;
        }

        private static void ProcessServerResponse(JObject response)
        {
            var message = response["message"].Value<String>();
            if (message != string.Empty)
            {
                CustomEvents.Instance.Add(EventsType.ServerMessage, message);
            }
            if (response["actions"] != null)
            {
                CustomEvents.Instance.Add(EventsType.ServerAction, response["actions"]);
            }
        }

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

        public static void OpenBrowser(PredefinedUrl predefinedUrl)
        {
            OpenBrowser(GetPredefinedUrl(predefinedUrl));
        }

        public static void DownloadFile(string uri, string targetPath)
        {
            var c = new System.Net.WebClient();
            c.DownloadFile(uri, targetPath);
        }

        public static void UpdateBulkUploadTime(DateTime time)
        {
            try
            {
                WebLayer.JSONRequest("last_update", new JObject
                                                              {
                                                                  { "time", time.ToString()} 
                                                              });
            }
            catch (Exception e)
            {
                CustomEvents.Instance.AddException(EventsType.JSONRequestException, "UpdateBulkUploadTime", e);
            }
        }

        public static JObject UploadFile(string filePath, string url, string arguments)
        {
            var result = new JObject() { { "success", false } };
            try
            {
                var client = new System.Net.WebClient();
                client.Headers.Add("Content-Type", "application/octet-stream");
                byte[] r = client.UploadFile(url + arguments, "POST", filePath);
                String s = Encoding.UTF8.GetString(r, 0, r.Length);
                if (string.IsNullOrEmpty(s)) return result;
                result = JObject.Parse(s);
                ProcessServerResponse(result);
            }
            catch (Exception e)
            {
                CustomEvents.Instance.AddException(EventsType.JSONRequestException, "", e);
            }
            return result;
        }
    }
}
