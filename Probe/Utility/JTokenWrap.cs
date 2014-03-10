using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Probe.Utility
{
    public class JTokenWrap
    {
        public JToken Content;
        public JTokenWrap(JToken content)
        {
            Content = content;
        }

        public string GetString(string path, string defaultValue)
        {
            var t = Content.SelectToken(path);
            if (t != null && (t.Type == JTokenType.String || t.Type == JTokenType.Integer))
            {
                return t.Value<string>();
            }
            return defaultValue;
        }

        public bool GetBool(string path, bool defaultValue)
        {
            var t = Content.SelectToken(path);
            if (t != null && t.Type == JTokenType.Boolean)
            {
                return t.Value<bool>();
            }
            return defaultValue;
        }

        public JArray GetArray(string path, JArray defaultValue)
        {
            var t = Content.SelectToken(path);
            if (t != null && t.Type == JTokenType.Array)
            {
                return (JArray)t;
            }
            return defaultValue;
        }

        public int GetInteger(string path, int defaultValue)
        {
            var t = Content.SelectToken(path);
            try
            {
                return t.Value<int>();
            } catch {}
            return defaultValue;
        }
    }
}
