using System;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF
{
   [Serializable]
   public class SystemColors : MarshalByRefObject
   {

      #region Constructors

      internal SystemColors()
      {
         BorderColor = Colors.DarkGray;
         ContainerBackground = Colors.LightGray;
         ControlBottom = Colors.LightGray;
         ControlTop = Colors.White;
         FontColor = Colors.CharcoalDust;
         PressedControlBottom = ColorUtility.ColorFromRGB(154, 196, 62);
         PressedControlTop = ColorUtility.ColorFromRGB(154, 196, 62);
         SelectedFontColor = Colors.White;
         SelectionColor = Colors.LightBlue;
         TextShadow = Colors.White;
         SelectedTextShadow = Colors.Blue;
         PressedTextColor = Colors.White;
         PressedTextShadow = ColorUtility.ColorFromRGB(129, 164, 48);
         AltSelectionColor = ColorUtility.ColorFromRGB(220, 229, 231);
         AltSelectedFontColor = 0;
         ScrollbarBackground = Colors.White;
         ScrollbarGrip = 0;
         WindowColor = Colors.White;
      }

      #endregion

      #region Properties

      public Color BorderColor { get; set; }
      public Color ContainerBackground { get; set; }
      public Color ControlBottom { get; set; }
      public Color ControlTop { get; set; }
      public Color FontColor { get; set; }
      public Color PressedControlBottom { get; set; }
      public Color PressedControlTop { get; set; }
      public Color SelectedFontColor { get; set; }
      public Color SelectionColor { get; set; }
      public Color TextShadow { get; set; }
      public Color SelectedTextShadow { get; set; }
      public Color PressedTextColor { get; set; }
      public Color PressedTextShadow { get; set; }
      public Color AltSelectionColor { get; set; }
      public Color AltSelectedFontColor { get; set; }
      public Color ScrollbarBackground { get; set; }
      public Color ScrollbarGrip { get; set; }
      public Color WindowColor { get; set; }
      #endregion

   }
}
