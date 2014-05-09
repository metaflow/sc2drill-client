using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using ImageProcessing;

namespace ImageOperations
{
    public partial class Form1 : Form
    {
        private Bitmap _sourceBitmap;
        private Bitmap _processing;
        public Form1()
        {
            InitializeComponent();
            _sourceBitmap = (Bitmap) pictureSource.Image;
            var b = new Bitmap(pictureSource.Image.Width, pictureSource.Image.Height, PixelFormat.Format32bppArgb);
            var g = Graphics.FromImage(b);
            g.DrawImage(pictureSource.Image, 0, 0, b.Width, b.Height);
            _sourceBitmap = b;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openImageDialog.ShowDialog() != DialogResult.OK) return;
            _sourceBitmap = new Bitmap(openImageDialog.FileName, true);
            var b = new Bitmap(_sourceBitmap.Width, _sourceBitmap.Height, PixelFormat.Format32bppArgb);
            var g = Graphics.FromImage(b);
            g.DrawImage(_sourceBitmap, 0, 0, b.Width, b.Height);
            _sourceBitmap = b;
            pictureSource.Size = _sourceBitmap.Size;
            pictureSource.Image = _sourceBitmap;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            _processing = ImageProcessor.ThresholdGrays(Convert.ToByte(textThreshold.Text), Convert.ToByte(textGrayTolerance.Text), _sourceBitmap);
            SetResultBitmap(_processing);
        }

        private void SetResultBitmap(Bitmap bitmap)
        {
            pictureResult.Size = new Size(bitmap.Width, bitmap.Height);
//            var b = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
//            var g = Graphics.FromImage(b);
//            g.DrawImage(bitmap, 0, 0, b.Width, b.Height);
            pictureResult.Size = bitmap.Size;
            pictureResult.Image = bitmap;
        }

        private void bFindLetters_Click(object sender, EventArgs e)
        {
            var x = 0;
            var s = 49;
            var letters = ImageProcessor.GetLetterMasks(Convert.ToByte(textLetterThreshold.Text),_processing);
            var result = new Bitmap(((s+1) * letters.Count), s, PixelFormat.Format32bppRgb);
            this.Text = "";
            foreach (ImageProcessor.Letter l in letters)
            {
                for (int i = 0; i < s; i++)
                {
                    for (int j = 0; j < s; j++)
                    {
                        if (l.Mask[(ImageProcessor.Letter.Size * i / s), (ImageProcessor.Letter.Size * j / s)] > 0) result.SetPixel(x + i, j, Color.Red);
                    }
                }
                this.Text += ImageProcessor.RecognizeLetter(l);
                x += s + 1;
            }
            
            SetResultBitmap(result);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var s = ImageProcessor.Recognize(_processing);
            this.Text = String.Join("|",s.ToArray());
        }

        private void btnBenchmark_Click(object sender, EventArgs e)
        {
            var start = DateTime.Now;
            for (int i = 0; i < 5000; i++)
            {
                var b = ImageProcessor.ThresholdGrays(200, 40, _sourceBitmap);
                var s = ImageProcessor.Recognize(b);
            }
            this.Text = DateTime.Now.Subtract(start).TotalMilliseconds.ToString();
        }
    }
}
