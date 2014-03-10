using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageProcessing
{
    public static class BitmapHelper
    {
        public static byte GetBitsPerPixel(Bitmap b)
        {
            switch (b.PixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                    return 1;
                case PixelFormat.Format4bppIndexed:
                    return 4;
                case PixelFormat.Format8bppIndexed:
                    return 8;
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                case PixelFormat.Format16bppArgb1555:
                    return 16;
                case PixelFormat.Format24bppRgb:
                    return 24;
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    return 32;
                case PixelFormat.Format48bppRgb:
                    return 48;
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return 64;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static long GetBitmapSum(Bitmap source)
        {
            long result = 0;
            Bitmap cp = (Bitmap)source.Clone();
            BitmapData bData = cp.LockBits(new Rectangle(0, 0, cp.Width, cp.Height), ImageLockMode.ReadOnly, cp.PixelFormat);
            /* GetBitsPerPixel just does a switch on the PixelFormat and returns the number */
            byte bitsPerPixel = GetBitsPerPixel(cp);

            /*the size of the image in bytes */
            var size = Math.Abs(bData.Stride * bData.Height);

            /*Allocate buffer for image*/
            byte[] data = new byte[size];
            System.Runtime.InteropServices.Marshal.Copy(bData.Scan0, data, 0, size);
            int step = bitsPerPixel / 8;
            var multHeight = source.Height * step;
            var multWidth = source.Width * step;
            for (int j = 0; j < multHeight; j += step)
            {
                var shift = j * source.Width;
                for (int i = 0; i < multWidth; i += step)
                {
                    result += data[shift + i] + data[shift + i + 1] + data[shift + i + 2];
                }
            }
            return result;
        }

        public static long GetBitmapSum(Bitmap source, int channel)
        {
            long result = 0;
            Bitmap cp = (Bitmap)source.Clone();
            BitmapData bData = cp.LockBits(new Rectangle(0, 0, cp.Width, cp.Height), ImageLockMode.ReadOnly, cp.PixelFormat);
            /* GetBitsPerPixel just does a switch on the PixelFormat and returns the number */
            byte bitsPerPixel = GetBitsPerPixel(cp);

            /*the size of the image in bytes */
            var size = Math.Abs(bData.Stride * bData.Height);

            /*Allocate buffer for image*/
            byte[] data = new byte[size];
            System.Runtime.InteropServices.Marshal.Copy(bData.Scan0, data, 0, size);
            int step = bitsPerPixel / 8;
            var multHeight = source.Height * step;
            var multWidth = source.Width * step;
            for (int j = 0; j < multHeight; j += step)
            {
                var shift = j * source.Width;
                for (int i = 0; i < multWidth; i += step)
                    result += data[shift + i + channel];
            }
            return result;
        }
    }
}
