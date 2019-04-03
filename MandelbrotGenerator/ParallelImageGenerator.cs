using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace MandelbrotGenerator
{
    public class ParallelImageGenerator : IAsyncImageGenerator
    {

        public event EventHandler<EventArgs<Tuple<Area, Bitmap, TimeSpan>>> ImageGenerated;
        public void CancelAsync()
        {
            //            CancelRequested = true;
        }

        private void OnImageGenerated(Area area, Bitmap bitmap, TimeSpan duration)
        {
            ImageGenerated?.Invoke(this, new EventArgs<Tuple<Area, Bitmap, TimeSpan>>(new Tuple<Area, Bitmap, TimeSpan>(area, bitmap, duration)));
        }

        public void GenerateImageAsync(Area area)
        {
            // TODO: problem: hier könnte es synchronistionsproblem geben, weil CancelRequest neu gesetzt wird bevor GEnerateImage abgebrochen wird 
            //  CancelRequested = false;

            Bitmap bitmap = new Bitmap(area.Width, area.Height);

            object rowLock = new object();

            int numThreads = 4;
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

                    while (actRow < area.Height)
                    {
                        // lock (rowLock)
                        {
                            if (actRow < area.Height)
                            {
                                y = actRow;
                                actRow++;
                            }
                        }
                       // Console.WriteLine($"Thread  {name}, y = {y},  actRow = {actRow},   area,Height = {area.Height}");

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
            TimeSpan duration = stopwatch.Elapsed;

            if (bitmap != null)
                OnImageGenerated(area, bitmap, duration);

        }
    }
}