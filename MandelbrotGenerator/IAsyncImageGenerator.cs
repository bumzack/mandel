using System;
using System.Drawing;

namespace MandelbrotGenerator
{
    public interface IAsyncImageGenerator
    {
        void GenerateImageAsync(Area area);
        void CancelAsync();
        event EventHandler<EventArgs<Tuple<Area, Bitmap, TimeSpan>>> ImageGenerated;
    }
}
