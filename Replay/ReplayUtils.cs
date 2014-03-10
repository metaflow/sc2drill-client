using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Probe.Common;

namespace Probe.Replay
{
    public static class ReplayUtils
    {
        public static List<ReplayInfo> GetReplaysToUpload(DateTime lastUploadDate)
        {
            var result = new List<ReplayInfo>();

            var replays = Directory.GetFiles(Paths.Sc2Directory, Paths.ReplayPattern, SearchOption.AllDirectories);

            foreach (var path in replays)
            {
                try
                {
                    var fi = new FileInfo(path);
                    if (fi.CreationTime >= lastUploadDate)
                    {
                        var info = ReplayParser.Parse(path);
                        if (result.Where(r => r.Hash.Equals(info.Hash)).FirstOrDefault() == null)
                        {
                            result.Add(info);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //todo: process error
                }
            }

            return result;
        }
    }
}
