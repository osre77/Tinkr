using System;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF
{
   /// <summary>
   /// Class for globally defining colors throughout the system
   /// </summary>
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

      /// <summary>
      /// Gets/Sets the system wide border color to be used by default RGB(169, 169, 169)
      /// </summary>
      public Color BorderColor { get; set; }

      /// <summary>
      /// Gets/Sets the system wide container background color to be used by default RGB(211, 211, 211)
      /// </summary>
      public Color ContainerBackground { get; set; }

      /// <summary>
      /// Gets/Sets the system wide control bottom color to be used by default RGB(211, 211, 211)
      /// </summary>
      public Color ControlBottom { get; set; }

      /// <summary>
      /// Gets/Sets the system wide control top color to be used by default RGB(255, 255, 255)
      /// </summary>
      public Color ControlTop { get; set; }

      /// <summary>
      /// Gets/Sets the system wide font color to be used by default RGB(82, 82, 82)
      /// </summary>
      public Color FontColor { get; set; }

      /// <summary>
      /// Gets/Sets the system wide control pressed bottom color to be used by default RGB(154, 196, 62)
      /// </summary>
      public Color PressedControlBottom { get; set; }

      /// <summary>
      /// Gets/Sets the system wide control pressed top color to be used by default RGB(154, 196, 62)
      /// </summary>
      public Color PressedControlTop { get; set; }

      /// <summary>
      /// Gets/Sets the system wide selected font color to be used by default RGB(255, 255, 255)
      /// </summary>
      public Color SelectedFontColor { get; set; }

      /// <summary>
      /// Gets/Sets the system wide selection color to be used by default RGB(27, 161, 226)
      /// </summary>
      public Color SelectionColor { get; set; }

      /// <summary>
      /// Gets/Sets the system wide text shadow color to be used by default RGB(255, 255, 255)
      /// </summary>
      public Color TextShadow { get; set; }

      /// <summary>
      /// Gets/Sets the system wide selected text shadow color to be used by default RGB(0, 0, 255)
      /// </summary>
      public Color SelectedTextShadow { get; set; }

      /// <summary>
      /// Gets/Sets the system wide pressed text color to be used by default RGB(255, 255, 255)
      /// </summary>
      public Color PressedTextColor { get; set; }

      /// <summary>
      /// Gets/Sets the system wide pressed text shadow color to be used by defaultRGB(129, 164, 48)
      /// </summary>
      public Color PressedTextShadow { get; set; }

      /// <summary>
      /// Gets/Sets the system wide alternate selection color to be used by defaultRGB(220, 229, 231)
      /// </summary>
      public Color AltSelectionColor { get; set; }

      /// <summary>
      /// Gets/Sets the system wide selected alternated font color to be used by default RGB(0, 0, 0)
      /// </summary>
      public Color AltSelectedFontColor { get; set; }

      /// <summary>
      /// Gets/Sets the system wide scrollbar background color to be used by default RGB(255, 255, 255)
      /// </summary>
      public Color ScrollbarBackground { get; set; }

      /// <summary>
      /// Gets/Sets the system wide scrollbar grip color to be used by default RGB(0, 0, 0)
      /// </summary>
      public Color ScrollbarGrip { get; set; }

      /// <summary>
      /// Gets/Sets the system wide window color to be used by default RGB(255, 255, 255)
      /// </summary>
      public Color WindowColor { get; set; }

      #endregion

   }
}
