using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF
{
   public class Overlay
   {
      public Bitmap Image { get; set; }
      public string Text { get; set; }
      public Color ForeColor { get; set; }
      public Color BackColor { get; set; }
      public Color BorderColor { get; set; }
      public ushort Opacity { get; set; }
      public point Position { get; set; }
      public double FadeAfter { get; set; }
      public size Size { get; set; }
      public Font Font { get; set; }
      internal long FadeAt { get; set; }
   }
}
