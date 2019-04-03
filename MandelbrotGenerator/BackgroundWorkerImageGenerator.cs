using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace MandelbrotGenerator
{
    public class BackgroundWorkerImageGenerator : IAsyncImageGenerator
    {
        private Area area;
        private Bitmap bitmap;
        private Stopwatch stopwatch;

        BackgroundWorker worker;

        public event EventHandler<EventArgs<Tuple<Area, Bitmap, TimeSpan>>> ImageGenerated;

        public BackgroundWorkerImageGenerator()
        {
            Console.WriteLine("constructor");
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
        }

        public void CancelAsync()
        {
            //if (worker != null)
            //    worker.CancelAsync();
        }

        private void OnImageGenerated(Area a, Bitmap b, TimeSpan d)
        {
            ImageGenerated?.Invoke(this, new EventArgs<Tuple<Area, Bitmap, TimeSpan>>(new Tuple<Area, Bitmap, TimeSpan>(a, b, d)));
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            stopwatch = new Stopwatch();
            stopwatch.Start();
            bitmap = GenerateImage(area, e);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("backgroundWorker1_RunWorkerCompleted");
            // First, handle the case where an exception was thrown.
            if (e.Cancelled)
            {
                Console.WriteLine("cancelled");
            }
            else
            {
                stopwatch.Stop();
                TimeSpan duration = stopwatch.Elapsed;
                Console.WriteLine("Backround Worker finished");
                if (bitmap != null)
                {
                    Console.WriteLine("Backround Worker finished   -bitmap != null - yeah !");
                    OnImageGenerated(area, bitmap, duration);
                }
                else
                {
                    Console.WriteLine("Backround Worker finished  but no bitmap :-(");
                }
            }
        }

        public void GenerateImageAsync(Area a)
        {
            area = a;
            // TODO: problem: hier könnte es synchronistionsproblem geben, weil CancelRequest neu gesetzt wird bevor GEnerateImage abgebrochen wird 
            if (worker != null && worker.IsBusy)
            {
                Console.WriteLine("GenerateImageAsync. calling CancelAsync");
                // worker.CancelAsync();
            }
            Console.WriteLine("GenerateImageAsync. starting BackgroundWOrker");
            worker.RunWorkerAsync();
        }

        public Bitmap GenerateImage(Area area, DoWorkEventArgs e)
        {
            int maxIterations;
            double zBorder;
            double cReal, cImg, zReal, zImg, zNewReal, zNewImg;

            maxIterations = Settings.DefaultSettings.MaxIterations;
            zBorder = Settings.DefaultSettings.ZBorder * Settings.DefaultSettings.ZBorder;

            Bitmap bitmap1 = new Bitmap(area.Width, area.Height);

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
                        if (worker.CancellationPending == true)
                        {
                            Console.WriteLine("GenerateImage. setting e.Cancel to true");
                            e.Cancel = true;
                            return null;
                        }

                        zNewReal = zReal * zReal - zImg * zImg + cReal;
                        zNewImg = 2 * zReal * zImg + cImg;

                        zReal = zNewReal;
                        zImg = zNewImg;

                        k++;
                    }
                    bitmap1.SetPixel(i, j, ColorSchema.GetColor(k));
                }
            }
            Console.WriteLine("returning bitmap1");
            return bitmap1;
        }
    }
}
