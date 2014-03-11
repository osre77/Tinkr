using System;
using Microsoft.SPOT;

namespace Skewworks.NETMF
{
    public class Overlay
    {

        public Bitmap Image { get; set; }
        public ushort Opacity { get; set; }
        public point Position { get; set; }
        public double FadeAfter { get; set; }
        internal long FadeAt { get; set; }
    }
}
