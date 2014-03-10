using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Probe.Replay
{
    public static class FileHash
    {
        public static string MD5(string fileName)
        {
            //todo: move to another class
            var tryCount = 10;
            while (tryCount > 0)
            {
                try
                {
                    var file = new FileStream(fileName, FileMode.Open);
                    MD5 md5 = new MD5CryptoServiceProvider();
                    var retVal = md5.ComputeHash(file);
                    file.Close();

                    var sb = new StringBuilder();
                    for (var i = 0; i < retVal.Length; i++)
                    {
                        sb.Append(retVal[i].ToString("x2"));
                    }
                    return sb.ToString();
                }
                catch (IOException)
                {
                    Thread.Sleep(500);
                    tryCount--;
                }
            }

            return string.Empty;
        }
    }
}
