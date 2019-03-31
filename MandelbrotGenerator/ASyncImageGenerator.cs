using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace MandelbrotGenerator
{
    public class ASyncImageGenerator : IAsyncImageGenerator
    {
        private bool CancelRequested = false;

        public event EventHandler<EventArgs<Tuple<Area, Bitmap, TimeSpan>>> ImageGenerated;
        public void CancelAsync()
        {
            CancelRequested = true;
        }

        private void OnImageGenerated(Area area, Bitmap bitmap, TimeSpan duration)
        {
            ImageGenerated?.Invoke(this, new EventArgs<Tuple<Area, Bitmap, TimeSpan>>(new Tuple<Area, Bitmap, TimeSpan>(area, bitmap, duration)));
        }

        public void GenerateImageAsync(Area area)
        {
            // TODO: problem: hier könnte es synchronistionsproblem geben, weil CancelRequest neu gesetzt wird bevor GEnerateImage abgebrochen wird 
            CancelRequested = false;
            Thread thread = new Thread(() =>
            {
                // TODO: hier die Zeitmessung und weiteren Rückgabewert
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Bitmap bitmap = GenerateImage(area);
                stopwatch.Stop();
                TimeSpan duration = stopwatch.Elapsed;

                if (bitmap != null)
                    OnImageGenerated(area, bitmap, duration);
            });
            thread.Start();
        }

        public Bitmap GenerateImage(Area area)
        {
            int maxIterations;
            double zBorder;
            double cReal, cImg, zReal, zImg, zNewReal, zNewImg;

            maxIterations = Settings.DefaultSettings.MaxIterations;
            zBorder = Settings.DefaultSettings.ZBorder * Settings.DefaultSettings.ZBorder;

            Bitmap bitmap = new Bitmap(area.Width, area.Height);

            for (int i = 0; i < area.Width; i++)
            {
                for (int j = 0; j < area.Height; j++)
                {
                    cReal = area.MinReal + i * area.PixelWidth;
                    cImg = area.MinImg + j * area.PixelHeight;
                    zReal = 0;
                    zImg = 0;

                    int k = 0;
                    while (((zReal * zReal + zImg * zImg) < zBorder) && (k < maxIterations))
                    {
                        if (CancelRequested)
                        {
                            CancelRequested = false;
                            return null;
                        };
                        zNewReal = zReal * zReal - zImg * zImg + cReal;
                        zNewImg = 2 * zReal * zImg + cImg;

                        zReal = zNewReal;
                        zImg = zNewImg;

                        k++;
                    }
                    bitmap.SetPixel(i, j, ColorSchema.GetColor(k));
                }
            }
            return bitmap;
        }
    }
}