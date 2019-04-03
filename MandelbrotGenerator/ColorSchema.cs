using System.Drawing;

namespace MandelbrotGenerator
{
    public static class ColorSchema
    {
        public static Color[] Mapping { get; } = {
                Color.FromArgb(66, 38, 15),
                                Color.FromArgb(66, 38, 15),
                                 Color.FromArgb(25, 7, 26),
        Color.FromArgb(9, 1, 47),
        Color.FromArgb(4, 4, 73),
        Color.FromArgb(0, 7, 100),
         Color.FromArgb(12, 44, 138),
        Color.FromArgb(24, 82, 177),
        Color.FromArgb(57, 125, 209),
        Color.FromArgb(134, 181, 229),
        Color.FromArgb(211, 236, 248),
        Color.FromArgb(241, 233, 191),
        Color.FromArgb(248, 201, 95),
        Color.FromArgb(255, 170, 0),
          Color.FromArgb(204, 128, 0),
         Color.FromArgb(153, 87, 0),
         Color.FromArgb(106, 52, 3)
        };

        public static Color GetColor(int iterations)
        {
            if (iterations == Settings.DefaultSettings.MaxIterations)
            {
                return Color.Black;
            }
            else
            {
                int i = iterations % 16;
                //int red = (iterations % 32) * 3;
                //if (red > 255)
                //    red = 255;

                //int green = (iterations % 16) * 2;
                //if (green > 255)
                //    green = 255;

                //int blue = (iterations % 128) * 14;
                //if (blue > 255)
                //    blue = 255;

                // return Color.FromArgb((int)red, (int)green, (int)blue);
                return Mapping[i];
            }
        }
    }
}
