using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Probe.Replay
{
    public class Sc2Ranks
    {
         #region singleton
        private static readonly Sc2Ranks _instance = new Sc2Ranks();

        private const string LeagueSectionRegex = "<div\\s+class=\"w960\\s+leagues\">.+?<span\\s+class=\"headertext\">(?<matchup>\\d+\\s+vs\\s+\\d+)</span>.+?</tr></table></div>";
        private const string LeagueSummaryRegex = "<span\\s+class=\"badge\\s+badge-(?<league>\\w+)\\s+badge-medium-\\d+\">.+?<span\\s+class=\"number\">(?<points>\\d+(,\\d+)?)</span>\\s+points.+?won\\s+<span\\s+class=\"green\">(?<won>\\d+(,\\d+)?)</span>.+?Rank\\s+<span\\s+class=\"number\">(?<rank>\\d+)</span>";
        private const string LeagueRegex = "<span\\s+class=\"badge\\s+badge-(?<league>\\w+)\\s+badge-medium-\\d+\">";
        //private const string LeagueSummarySimple = "\"divisionrank topborder\".*Rank.*>(?<rank>\\d+)<.*<a.*>(?<league>[^<]*)<";

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Sc2Ranks()
        {
        }

        public static Sc2Ranks Instance
        {
            get
            {
                return _instance;
            }
        }
        #endregion

        public void FillPlayerInfo(ReplayPlayerInfo playerInfo)
        {
            var url = string.Format("http://www.sc2ranks.com/{0}/{1}/{2}", playerInfo.Realm, playerInfo.Uid, playerInfo.Name);
            
            var response = WebRequest.Create(url).GetResponse();

            if (response == null) return;

            var stream = response.GetResponseStream();

            if (stream == null) return;

            var content = String.Empty;
            using (var sr = new StreamReader(stream))
            {
                content = sr.ReadToEnd();
                sr.Close();
            }

            var match = Regex.Match(content, LeagueSectionRegex);

            while (match.Success)
            {
                if (match.Groups["matchup"].ToString() == "1 vs 1")
                {
                    var m2 = Regex.Match(match.ToString(), LeagueRegex);
                    playerInfo.League = m2.Groups["league"].ToString();
                    playerInfo.Rank = 0;
                }
                match = match.NextMatch();
            }
        }
    }
}
