using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF
{
   /// <summary>
   /// Class for creating on-screen overlays
   /// </summary>
   public class Overlay
   {
      /// <summary>
      /// Gets/Sets associated image
      /// </summary>
      public Bitmap Image { get; set; }

      /// <summary>
      /// Gets/Sets associated text
      /// </summary>
      public string Text { get; set; }

      /// <summary>
      /// Gets/Sets color to use when rendering text
      /// </summary>
      public Color ForeColor { get; set; }

      /// <summary>
      /// Gets/Sets background color
      /// </summary>
      public Color BackColor { get; set; }

      /// <summary>
      /// Gets/Sets border color
      /// </summary>
      public Color BorderColor { get; set; }

      /// <summary>
      /// Gets/Sets current opacity level
      /// </summary>
      public ushort Opacity { get; set; }

      /// <summary>
      /// Gets/Sets display position
      /// </summary>
      public point Position { get; set; }

      /// <summary>
      /// Gets/Sets the amount of time in seconds to wait before fading
      /// </summary>
      public double FadeAfter { get; set; }

      /// <summary>
      /// Gets/Sets size of overlay
      /// </summary>
      public size Size { get; set; }

      /// <summary>
      /// Gets/Sets font
      /// </summary>
      public Font Font { get; set; }

      /// <summary>
      /// Absolute time value computed from <see cref="FadeAfter"/>. For internal usage only.
      /// </summary>
      internal long FadeAt { get; set; }
   }
}
