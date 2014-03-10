using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MpqLib.Mpq;

namespace Probe.Replay
{
    public class MpqKeyValue : Dictionary<long, object> { } ;

    public static class MpqFile
    {
        public static object ParseSerializedData(MemoryStream stream)
        {
            var dataType = stream.ReadByte();
            long numElements;
            switch (dataType)
            {
                case 0x02: // binary data
                    var dataLen = ParseVlfNumber(stream);
                    byte[] buff = new byte[dataLen];
                    stream.Read(buff, 0, (int)dataLen);
                    return buff;
                case 0x04: // simple array
                    var array = new List<object>();
                    stream.Position += 2;
                    numElements = ParseVlfNumber(stream);
                    while (numElements > 0)
                    {
                        array.Add(ParseSerializedData(stream));
                        numElements--;
                    }
                    return array;
                case 0x05: // array with keys
                    var r5 = new MpqKeyValue();
                    numElements = ParseVlfNumber(stream);
                    while (numElements > 0)
                    {
                        var index = ParseVlfNumber(stream);
                        r5[index] = ParseSerializedData(stream);
                        numElements--;
                    }
                    return r5;
                case 0x06: // number of one byte
                    return stream.ReadByte();
                case 0x07: // number of four bytes
                    return ReadUInt32(stream);
                case 0x09: // number in VLF
                    return ParseVlfNumber(stream);
                default:
                    return null;
                    //throw new ArgumentOutOfRangeException(string.Format("Unknown data type in function parseDetailsValue {0}", dataType));
            }
        }

        private static UInt32 ReadUInt32(MemoryStream stream)
        {
            var buff = new byte[4];
            stream.Read(buff, 0, 4);
            return BitConverter.ToUInt32(buff, 0);
        }

        private static long ParseVlfNumber(MemoryStream stream)
        {
            long number = 0;
            var first = true;
            var multiplier = 1;
            var bytes = 0;
            int i;
            do
            {
                i = stream.ReadByte();
                number += (i & 0x7F) * (long)Math.Pow(2, bytes * 7);
                if (first)
                {
                    if ((number & 1) != 0)
                    {
                        multiplier = -1;
                        number--;
                    }
                    first = false;
                }
                bytes++;
            } while ((i & 0x80) != 0);

            number *= multiplier;
            number /= 2; // can't use right-shift because the datatype will be float for large values on 32-bit systems
            return number;
        }

        public static byte[] ReadFilePart(CArchive archive, string fileName)
        {
            var files = archive.FindFiles(fileName);
            byte[] buffer;

            if (files.Any())
            {
                var file = files.Single();
                buffer = new byte[file.Size];
                archive.ExportFile(fileName, buffer);
            }
            else
            {
                throw new FileNotFoundException("The archive does not contain the requested file", fileName);
            }

            return buffer;
        }

        public static long ParseTimeStamp(MemoryStream stream)
        {
            var one = stream.ReadByte();
            if ((one & 3) > 0)
            { // check if value is two bytes or more
                var two = stream.ReadByte();
                two = (((one >> 2) << 8) | two);
                if ((one & 3) >= 2)
                {
                    var tmp = stream.ReadByte();
                    two = ((two << 8) | tmp);
                    if ((one & 3) == 3)
                    {
                        tmp = stream.ReadByte();
                        two = ((two << 8) | tmp);
                    }
                }
                return two;
            }
            return (one >> 2);
        }
    }
}
