using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace MandelbrotGenerator
{
    public class ParallelImageGenerator : IAsyncImageGenerator
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
            Bitmap bitmap = new Bitmap(area.Width, area.Height);

            object rowLock = new object();
            int numThreads = Settings.DefaultSettings.Workers;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int actRow = 0;

            var threads = new List<Thread>();
            for (int m = 0; m < numThreads; m++)
            {
                Thread t = new Thread((name) =>
                {
                    int y = -1;
                    int maxIterations;
                    double zBorder;
                    double cReal, cImg, zReal, zImg, zNewReal, zNewImg;

                    maxIterations = Settings.DefaultSettings.MaxIterations;
                    zBorder = Settings.DefaultSettings.ZBorder * Settings.DefaultSettings.ZBorder;

                    while (actRow < area.Height && !CancelRequested)
                    {
                        lock (rowLock)
                        {
                            if (actRow < area.Height)
                            {
                                y = actRow;
                                actRow++;
                            }
                        }

                        for (int x = 0; x < area.Width; x++)
                        {
                            cReal = area.MinReal + x * area.PixelWidth;
                            cImg = area.MinImg + y * area.PixelHeight;
                            zReal = 0;
                            zImg = 0;

                            int k = 0;
                            while (((zReal * zReal + zImg * zImg) < zBorder) && (k < maxIterations))
                            {
                                zNewReal = zReal * zReal - zImg * zImg + cReal;
                                zNewImg = 2 * zReal * zImg + cImg;

                                zReal = zNewReal;
                                zImg = zNewImg;

                                k++;
                            }
                            lock (bitmap)
                            {
                                bitmap.SetPixel(x, y, ColorSchema.GetColor(k));
                            }
                        }
                    }
                });

                threads.Add(t);
                t.Start(m);
            }

            Console.WriteLine("Starting to wait...");
            foreach (Thread t in threads)
                t.Join();
            Console.WriteLine("All threads finished!");
            stopwatch.Stop();

            if (CancelRequested)
            {
                bitmap = null;
                CancelRequested = false; 
            }

            TimeSpan duration = stopwatch.Elapsed;
            Console.WriteLine($" time elapsed   {duration}");
            Console.WriteLine($" width      {area.Width},   height: {area.Height}");

            if (bitmap != null)
                OnImageGenerated(area, bitmap, duration);
        }
    }
}
