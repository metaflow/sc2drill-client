using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessing
{
    public class ImageProcessor
    {
        public class Letter
        {
            public byte[,] Mask;
            public static byte Size = 7;
            public Rectangle Rect;
        };
        

        public static Bitmap ThresholdGrays(byte thresh, byte GrayTolerance, Bitmap b)
        {
            Bitmap cp = (Bitmap)b.Clone();
            BitmapData bData = cp.LockBits(new Rectangle(0, 0, cp.Width, cp.Height), ImageLockMode.ReadWrite, cp.PixelFormat);

            /* GetBitsPerPixel just does a switch on the PixelFormat and returns the number */
            byte bitsPerPixel = BitmapHelper.GetBitsPerPixel(cp);

            /*the size of the image in bytes */
            var size = Math.Abs(bData.Stride * bData.Height);

            /*Allocate buffer for image*/
            byte[] data = new byte[size];

            /*This overload copies data of /size/ into /data/ from location specified (/Scan0/)*/
            System.Runtime.InteropServices.Marshal.Copy(bData.Scan0, data, 0, size);
            var grayThresh = thresh - GrayTolerance - 1;
            int step = bitsPerPixel / 8;
            for (int i = 0; i < size; i += step)
            {
                var f = (data[i] > grayThresh) && (data[i + 1] > grayThresh) && (data[i + 2] > grayThresh);
                if (f) f = (Math.Abs(data[i] - data[i + 1]) < GrayTolerance) && (Math.Abs(data[i + 1] - data[i + 2]) < GrayTolerance) && (Math.Abs(data[i] - data[i + 2]) < GrayTolerance);
                if (f)
                {
                    double magnitude = 1 / 3d * (data[i] + data[i + 1] + data[i + 2]);
                    f = magnitude > thresh;
                }

                if (f)
                {
                    data[i] = 255;
                    data[i + 1] = 255;
                    data[i + 2] = 255;
                }
                else
                {

                    data[i] = 0;
                    data[i + 1] = 0;
                    data[i + 2] = 0;
                }
                //data[i] is the first of 3 bytes of color

            }

            /* This override copies the data back into the location specified */
            System.Runtime.InteropServices.Marshal.Copy(data, 0, bData.Scan0, data.Length);

            cp.UnlockBits(bData);
            return cp;
        }

        public static byte[,] ConvertToBitmask(Bitmap source)
        {
            byte[,] result = new byte[source.Width, source.Height];
            Bitmap cp = (Bitmap)source.Clone();
            BitmapData bData = cp.LockBits(new Rectangle(0, 0, cp.Width, cp.Height), ImageLockMode.ReadOnly, cp.PixelFormat);

            /* GetBitsPerPixel just does a switch on the PixelFormat and returns the number */
            byte bitsPerPixel = BitmapHelper.GetBitsPerPixel(cp);

            /*the size of the image in bytes */
            var size = Math.Abs(bData.Stride * bData.Height);

            /*Allocate buffer for image*/
            byte[] data = new byte[size];
            int step = bitsPerPixel / 8;
            System.Runtime.InteropServices.Marshal.Copy(bData.Scan0, data, 0, size);
            var multHeight = source.Height * step;
            var multWidth = source.Width * step;
            for (int j = 0; j < multHeight; j += step)
            {
                var shift = j * source.Width;
                for (int i = 0; i < multWidth; i += step)
                {
                    if (data[shift + i] != 0) result[i / step, j / step] = 1;
                }
            }
            /*This overload copies data of /size/ into /data/ from location specified (/Scan0/)*/
            cp.Dispose();
            return result;
        }

        public static List<Rectangle> FindLetters(byte[,] mask, Size maskSize)
        {
            var result = new List<Rectangle>();
            //h-scan
            var hLow = maskSize.Height;
            var hHigh = 0;

            for (int i = 0; i < maskSize.Height; i++)
            {
                var hS = 0;
                for (int j = 0; j < maskSize.Width; j++)
                {
                    hS += mask[j, i];
                    if (hS > 10) break;
                }
                if (hS <= 10) continue;
                hLow = i;
                break;
            }

            for (int i = maskSize.Height - 1; i >= 0; i--)
            {
                var hS = 0;
                for (int j = 0; j < maskSize.Width; j++)
                {
                    hS += mask[j, i];
                    if (hS > 10) break;
                }
                if (hS <= 10) continue;
                hHigh = i;
                break;
            }

            var vScan = new Int32[maskSize.Width];

            for (int i = 0; i < maskSize.Width; i++)
            {
                for (int j = hLow; j <= hHigh; j++)
                {
                    vScan[i] += mask[i, j];
                }
            }

            var letterStart = 0;
            var letterWidth = 0;
            var maxScan = 0;
            var thinWidth = 0;

            for (int i = 0; i < maskSize.Width; i++)
            {
                if (vScan[i] == 0)
                {
                    while (letterWidth > 0 && vScan[letterStart + letterWidth - 1] < 2)
                    {
                        letterWidth--;
                    }

                    if (letterWidth > 0 && maxScan > 3)
                    {
                        result.Add(new Rectangle(letterStart, hLow, letterWidth, hHigh - hLow + 1));
                    }
                    letterStart = -1;
                    letterWidth = 0;
                    maxScan = 0;
                    thinWidth = 0;
                }
                else
                {
                    if (letterWidth > 0) letterWidth++;
                    if ((vScan[i] > 3) && letterStart == -1)
                    {
                        letterStart = i;
                        letterWidth++;
                    }
                    thinWidth++;
                    if ((thinWidth > 3) && letterStart == -1)
                    {
                        letterStart = i - thinWidth + 1;
                        letterWidth = thinWidth;
                    }
                    maxScan = Math.Max(maxScan, vScan[i]);
                }
            }

            return result;
        }

        public static List<Letter> GetLetterMasks(byte threshold, Bitmap source)
        {
            var result = new List<Letter>();
            List<Rectangle> boxes = FindLetters(ConvertToBitmask(source), source.Size);
            foreach (Rectangle rectangle in boxes)
            {
                var b = new Bitmap(Letter.Size, Letter.Size, PixelFormat.Format32bppRgb);
                var t = Graphics.FromImage(b);
                t.DrawImage(source, new Rectangle(0, 0, Letter.Size, Letter.Size), rectangle, GraphicsUnit.Pixel);
                b = ThresholdGrays(threshold, 40, b);
                result.Add(new Letter() { Mask = ConvertToBitmask(b), Rect = rectangle });
            }
            return result;
        }

        private static Dictionary<Char, List<KeyValuePair<Point, int>>> letterSignatures = new Dictionary<char, List<KeyValuePair<Point, int>>>()
              {
              {'0', new List<KeyValuePair<Point, int>>(){
                  new KeyValuePair<Point, int>(new Point(3,3), -2),
                  new KeyValuePair<Point, int>(new Point(6,0), -2),
                  new KeyValuePair<Point, int>(new Point(3,0), 1),
                  new KeyValuePair<Point, int>(new Point(0,0), -1),
                  new KeyValuePair<Point, int>(new Point(0,4), 2),
                  new KeyValuePair<Point, int>(new Point(0,6), -2)
              }},
              {'1', new List<KeyValuePair<Point, int>>()
                           {
                               new KeyValuePair<Point, int>(new Point(3,3), -2),
                               new KeyValuePair<Point, int>(new Point(0,4), -2),
                               new KeyValuePair<Point, int>(new Point(0,6), -2),
                               new KeyValuePair<Point, int>(new Point(6,6), 2),
                               new KeyValuePair<Point, int>(new Point(6,0), 1),
                               new KeyValuePair<Point, int>(new Point(2,0), -1)
                           }},
                           {'4', new List<KeyValuePair<Point, int>>()
                           {
                               new KeyValuePair<Point, int>(new Point(3,3), -1),
                               new KeyValuePair<Point, int>(new Point(0,0), -1),
                               new KeyValuePair<Point, int>(new Point(0,1), -3),
                               new KeyValuePair<Point, int>(new Point(6,0), 1),
                               new KeyValuePair<Point, int>(new Point(6,6), 1),
                               new KeyValuePair<Point, int>(new Point(1,0), -3)
                           }},
                           {'5', new List<KeyValuePair<Point, int>>()
                           {
                               new KeyValuePair<Point, int>(new Point(3,3), -1),
                               new KeyValuePair<Point, int>(new Point(0,0), +3),
                               new KeyValuePair<Point, int>(new Point(0,4), -2),
                               new KeyValuePair<Point, int>(new Point(6,1), -1),
                               new KeyValuePair<Point, int>(new Point(5,1), -2),
                               new KeyValuePair<Point, int>(new Point(0,2), 1)
                           }},
                           {'7', new List<KeyValuePair<Point, int>>()
                           {
                               new KeyValuePair<Point, int>(new Point(2,2), -1),
                               new KeyValuePair<Point, int>(new Point(1,2), -1),
                               new KeyValuePair<Point, int>(new Point(0,0), +2),
                               new KeyValuePair<Point, int>(new Point(6,4), -3),
                               new KeyValuePair<Point, int>(new Point(6,1), 1),
                               new KeyValuePair<Point, int>(new Point(0,2), -1),
                               new KeyValuePair<Point, int>(new Point(2,6), 1)
                           }},
                           {'3', new List<KeyValuePair<Point, int>>()
                           {
                               new KeyValuePair<Point, int>(new Point(1,3), -2),
                               new KeyValuePair<Point, int>(new Point(0,3), -2),
                               new KeyValuePair<Point, int>(new Point(6,4), 2),
                               new KeyValuePair<Point, int>(new Point(2,3), -2),
                               new KeyValuePair<Point, int>(new Point(3,3), 1),
                               new KeyValuePair<Point, int>(new Point(1,6), 1)
                           }
                           },
                           {'2', new List<KeyValuePair<Point, int>>()
                           {
                               new KeyValuePair<Point, int>(new Point(1,3), -2),
                               new KeyValuePair<Point, int>(new Point(0,3), -2),
                               new KeyValuePair<Point, int>(new Point(6,4), -1),
                               new KeyValuePair<Point, int>(new Point(6,6), 1),
                               new KeyValuePair<Point, int>(new Point(6,5), -1),
                               new KeyValuePair<Point, int>(new Point(5,5), -1),
                               new KeyValuePair<Point, int>(new Point(0,6), 1),
                               new KeyValuePair<Point, int>(new Point(5,6), 1)
                           }},
                           {'6', new List<KeyValuePair<Point, int>>()
                           {
                               new KeyValuePair<Point, int>(new Point(3,3), 2),
                               new KeyValuePair<Point, int>(new Point(0,3), 2),
                               new KeyValuePair<Point, int>(new Point(6,2), -2),
                               new KeyValuePair<Point, int>(new Point(5,2), -2),
                               new KeyValuePair<Point, int>(new Point(0,4), 1),
                               new KeyValuePair<Point, int>(new Point(5,1), 1)
                           }},
                           {'8', new List<KeyValuePair<Point, int>>()
                           {
                               new KeyValuePair<Point, int>(new Point(3,3), 1),
                               new KeyValuePair<Point, int>(new Point(0,4), 2),
                               new KeyValuePair<Point, int>(new Point(6,1), 1),
                               new KeyValuePair<Point, int>(new Point(6,2), 2),
                               new KeyValuePair<Point, int>(new Point(5,5), 1),
                               new KeyValuePair<Point, int>(new Point(2,3), 1),
                               new KeyValuePair<Point, int>(new Point(5,3), 1),
                               new KeyValuePair<Point, int>(new Point(6,4), 1),
                           }},
                           {'9', new List<KeyValuePair<Point, int>>()
                           {
                               new KeyValuePair<Point, int>(new Point(3,3), 2),
                               new KeyValuePair<Point, int>(new Point(1,4), -2),
                               new KeyValuePair<Point, int>(new Point(0,4), -2),
                               new KeyValuePair<Point, int>(new Point(6,2), 1),
                               new KeyValuePair<Point, int>(new Point(3,0), 1),
                               new KeyValuePair<Point, int>(new Point(3,6), 2)
                           }}
                        
              };

        public static Char RecognizeLetter(Letter l)
        {
            var bestScore = 3;
            var result = ' ';
            foreach (KeyValuePair<char, List<KeyValuePair<Point, int>>> entry in letterSignatures)
            {
                var score = 0;
                foreach (KeyValuePair<Point, int> pair in entry.Value)
                {
                    score += (l.Mask[pair.Key.X, pair.Key.Y] != 0) ? pair.Value : -pair.Value;
                }
                if (score > bestScore)
                {
                    result = entry.Key;
                    bestScore = score;
                }
            }
            return result;
        }

        public static List<string> Recognize(Bitmap source)
        {
            var result = new List<string>();
            var letters = GetLetterMasks(50, source);
            string current = "";
            var lastX = -100;
            foreach (Letter letter in letters)
            {
                if (letter.Rect.Left - lastX > letter.Rect.Height)
                {
                    if (!string.IsNullOrEmpty(current)) result.Add(current);
                    current = "";
                }
                lastX = letter.Rect.Right;
                current += RecognizeLetter(letter);
            }
            if (!string.IsNullOrEmpty(current)) result.Add(current);
            return result;
        }


        private static byte[,] FilterBrightes(int count, Bitmap source)
        {
            byte[,] result = new byte[source.Width, source.Height];
            Bitmap cp = (Bitmap)source.Clone();
            BitmapData bData = cp.LockBits(new Rectangle(0, 0, cp.Width, cp.Height), ImageLockMode.ReadOnly, cp.PixelFormat);

            /* GetBitsPerPixel just does a switch on the PixelFormat and returns the number */
            byte bitsPerPixel = BitmapHelper.GetBitsPerPixel(cp);

            /*the size of the image in bytes */
            var size = Math.Abs(bData.Stride * bData.Height);

            /*Allocate buffer for image*/
            byte[] data = new byte[size];

            var magnitudes = new List<KeyValuePair<Point, byte>>();

            System.Runtime.InteropServices.Marshal.Copy(bData.Scan0, data, 0, size);

            for (int j = 0; j < source.Height; j++)
            {
                var shift = j * source.Width;
                for (int i = 0; i < source.Width; i++)
                {
                    var k = (shift + i) * (bitsPerPixel / 8);
                    magnitudes.Add(new KeyValuePair<Point, byte>(new Point(i, j), (byte)((data[k] + data[k + 1] + data[k + 2]) / 3)));
                }
            }

            magnitudes.Sort((a, b) => -a.Value.CompareTo(b.Value));

            for (int i = 0; i < count; i++)
            {
                result[magnitudes[i].Key.X, magnitudes[i].Key.Y] = 1;
            }
            cp.Dispose();
            return result;
        }

        public static Dictionary<char, KeyValuePair<string, int>> DebugRecognizeLetter(Letter l)
        {
            var bestScore = 3;
            var scores = new Dictionary<Char, KeyValuePair<string,int>>();
            foreach (KeyValuePair<char, List<KeyValuePair<Point, int>>> entry in letterSignatures)
            {
                var score = 0;
                var scorePath = "";
                foreach (KeyValuePair<Point, int> pair in entry.Value)
                {
                    score += (l.Mask[pair.Key.X, pair.Key.Y] != 0) ? pair.Value : -pair.Value;
                    scorePath += "|" + ((l.Mask[pair.Key.X, pair.Key.Y] != 0) ? pair.Value : -pair.Value);
                }
                if (score > bestScore)
                {
                    bestScore = score;
                }
                scores.Add(entry.Key, new KeyValuePair<string, int>(scorePath, score));
            }
            return scores;
        }

        public static Color DetectVerticalLinesColor(Bitmap source)
        {
            var result = Color.Black;
            Bitmap cp = (Bitmap)source.Clone();
            BitmapData bData = cp.LockBits(new Rectangle(0, 0, cp.Width, cp.Height), ImageLockMode.ReadOnly, cp.PixelFormat);
            /* GetBitsPerPixel just does a switch on the PixelFormat and returns the number */
            byte bitsPerPixel = BitmapHelper.GetBitsPerPixel(cp);

            /*the size of the image in bytes */
            var size = Math.Abs(bData.Stride * bData.Height);

            /*Allocate buffer for image*/
            byte[] data = new byte[size];
            System.Runtime.InteropServices.Marshal.Copy(bData.Scan0, data, 0, size);
            int step = bitsPerPixel / 8;
            var multHeight = source.Height * step;
            var multWidth = source.Width * step;
            var colorCounts = new List<Dictionary<Color, int>>();
            for (int i = 0; i < source.Width; i++)
            {
                colorCounts.Add(new Dictionary<Color, int>());
            }
            for (int j = 0; j < multHeight; j += step)
            {
                var shift = j * source.Width;
                for (int i = 0; i < multWidth; i += step)
                {
                    var magnitude = data[shift + i] + data[shift + i + 1] + data[shift + i + 2];
                    if (magnitude < 150) continue;
                    var c = Color.FromArgb(data[shift + i + 2], data[shift + i + 1], data[shift + i]);
                    if (!colorCounts[i / step].ContainsKey(c)) colorCounts[i/step][c] = 0;
                    colorCounts[i/step][c] += 1;
                }
            }
            var maxCount = 0;
            foreach (var colorCount in colorCounts)
            {
                foreach (var p in colorCount)
                {
                    if (p.Value > maxCount)
                    {
                        result = p.Key;
                        maxCount = p.Value;
                    }
                }
            }
            return result;
        }

        public static int GetColorDistance(Color colorA, Color colorB)
        {
            return Math.Abs(colorA.R - colorB.R) + Math.Abs(colorA.G - colorB.G) + Math.Abs(colorA.B - colorB.B);
        }
    }
}
