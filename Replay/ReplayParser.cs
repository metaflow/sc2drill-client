using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using MpqLib.Mpq;

namespace Probe.Replay
{
    public static class ReplayParser
    {
        private static byte[] _replayDetails;
        private static byte[] _messageEvents;
        private static byte[] _replayInitData;
        //private static List<ReplayPlayerInfo> _players;
        //private static long _gameCTime;
        //private static int _recorderIndex;
        //private static string _mapHash;


        public static ReplayInfo Parse(string path)
        {
            // basic file info
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;

            try
            {
                var result = new ReplayInfo();
                
                result.FileInfo = new FileInfo(path);
                result.Hash = FileHash.MD5(path);

                // parse mpq replay format
                result.MapHash = "";
                using (var archive = new CArchive(path))
                {
                    _replayDetails = MpqFile.ReadFilePart(archive, "replay.details");
                    _messageEvents = MpqFile.ReadFilePart(archive, "replay.message.events");
                    _replayInitData = MpqFile.ReadFilePart(archive, "replay.initData");
                }

                ParseDetailsFile(result);
                ParseChatLog(result);
                ParseInitData(result);

                foreach (var playerInfo in result.Players)
                    playerInfo.Realm = result.Realm;

                result.ProfileNumber = 0;
                
                return result;
            }
            catch 
            {
                return null;
            }
        }

        private static void ParseInitData(ReplayInfo info)
        {
            using (MemoryStream ms = new MemoryStream(_replayInitData))
            {
                byte[] b = new byte[3];
                while (ms.Position < ms.Length)
                {
                    if (ms.ReadByte() != 's') continue;
                    ms.Read(b, 0, 3);
                    if (Encoding.ASCII.GetString(b) != "2ma") continue;
                    ms.Position += 2; // \0\0EU
                    byte[] buffer = new byte[2];
                    ms.Read(buffer, 0, 2);
                    info.Realm = Encoding.ASCII.GetString(buffer);
                    buffer = new byte[32];
                    ms.Read(buffer, 0, 32);
                    info.MapHash = BitConverter.ToString(buffer);
                    //we need very last hash so don't quit here
                }
            }
        }

        private static void ParseChatLog(ReplayInfo info)
        {
            using (var ms = new MemoryStream(_messageEvents))
            {
                var recorderArray = new Dictionary<int, bool>();
                for (int i = 0; i < info.Players.Count; i++)
                {
                    recorderArray[i + 1] = info.Players[i].IsHuman;
                }
                while (ms.Position < ms.Length)
                {
                    MpqFile.ParseTimeStamp(ms);
                    var playerId = ms.ReadByte() & 0x0F;
                    var opcode = ms.ReadByte();
                    if (opcode == 0x80)
                    { // header weird thingy?
                        ms.Position += 4;
                        recorderArray[playerId] = false;
                    }
                    else if ((opcode & 0x80) == 0)
                    { // message
                        var messageTarget = opcode & 3;
                        var messageLength = ms.ReadByte();
                        if ((opcode & 8) == 8) messageLength += 64;
                        if ((opcode & 16) == 16) messageLength += 128;
                        var buffer = new byte[messageLength];
                        var message = ms.Read(buffer, 0, messageLength);
                    }
                    else if (opcode == 0x83)
                    { // ping on map? 8 bytes?
                        ms.Position += 8;
                    }
                }
                info.RecorderIndex = 0;
                foreach (KeyValuePair<int, bool> pair in recorderArray)
                {
                    if (!pair.Value) continue;
                    if (info.RecorderIndex > 0)
                    {
                        // found a second recorder, so something is clearly broken
                        info.RecorderIndex = 0;
                        break;
                    }
                    info.RecorderIndex = pair.Key;
                }
            }
        }

        private static void ParseDetailsFile(ReplayInfo info)
        {
            info.Players.Clear();

            using (MemoryStream ms = new MemoryStream(_replayDetails))
            {
                var array = (MpqKeyValue)MpqFile.ParseSerializedData(ms);

                var playerArray = (List<object>)(array[0]);
                var i = 1;
                foreach (var player in playerArray)
                {
                    var p = new ReplayPlayerInfo();
                    p.Name = Encoding.ASCII.GetString((byte[])((MpqKeyValue)player)[0]);
                    p.Uid = Convert.ToUInt32(((MpqKeyValue)((MpqKeyValue)player)[1])[4]);
                    p.Index = i;
                    p.IsHuman = p.Uid != 0;
                    info.Players.Add(p);
                    i++;
                }

                Encoding.ASCII.GetString((byte[])array[1]);
                info.GameCTime = (long)Math.Floor(((long)array[5] - 116444735995904000) / 10000000.0);
            }
        }
    }
}
