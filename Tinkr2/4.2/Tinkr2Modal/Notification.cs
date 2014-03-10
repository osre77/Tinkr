using System;
using System.Threading;

using Microsoft.SPOT;

using Skewworks.NETMF;

namespace Skewworks.Tinkr.Modal
{
    public class Notification
    {

        public static void Show(string text, Font font)
        {
            size sz = FontManager.ComputeExtentEx(font, text);
            Overlay ov = new Overlay();

            ov.Size = new size(sz.Width + 16, sz.Height + 8);
            ov.Position = new point(Core.ScreenWidth / 2 - ov.Size.Width / 2, 8);
            ov.Image = null;
            ov.BackColor = Colors.Black;
            ov.BorderColor = Colors.White;
            ov.ForeColor = Colors.White;
            ov.Text = text;
            ov.Opacity = 256;
            ov.FadeAfter = 1.5;
            ov.Font = font;

            new Thread(() => Core.ScreenOverlay = ov).Start();
        }

        public static void Show(string text, Font font, Bitmap image)
        {
            if (image == null)
            {
                Show(text, font);
                return;
            }

            size sz = FontManager.ComputeExtentEx(font, text);
            Overlay ov = new Overlay();

            sz.Width += image.Width + 8;
            if (sz.Height < image.Height)
                sz.Height = image.Height;

            ov.Size = new size(sz.Width + 16, sz.Height + 8);
            ov.Position = new point(Core.ScreenWidth / 2 - ov.Size.Width / 2, 8);
            ov.Image = image;
            ov.BackColor = Colors.Black;
            ov.BorderColor = Colors.White;
            ov.ForeColor = Colors.White;
            ov.Text = text;
            ov.Opacity = 256;
            ov.FadeAfter = 1.5;
            ov.Font = font;

            new Thread(() => Core.ScreenOverlay = ov).Start();
        }

    }
}
