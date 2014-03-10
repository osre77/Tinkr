using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF
{
    [Serializable]
    public class SystemColors : MarshalByRefObject
    {

        #region Constructors

        internal SystemColors()
        {
            this.BorderColor = Colors.DarkGray;
            this.ContainerBackground = Colors.LightGray;
            this.ControlBottom = Colors.LightGray;
            this.ControlTop = Colors.White;
            this.FontColor = Colors.CharcoalDust;
            this.PressedControlBottom = ColorUtility.ColorFromRGB(154, 196, 62);
            this.PressedControlTop = ColorUtility.ColorFromRGB(154, 196, 62);
            this.SelectedFontColor = Colors.White;
            this.SelectionColor = Colors.LightBlue;
            this.TextShadow = Colors.White;
            this.SelectedTextShadow = Colors.Blue;
            this.PressedTextColor = Colors.White;
            this.PressedTextShadow = ColorUtility.ColorFromRGB(129, 164, 48);
            this.AltSelectionColor = ColorUtility.ColorFromRGB(220, 229, 231);
            this.AltSelectedFontColor = 0;
            this.ScrollbarBackground = Colors.White;
            this.ScrollbarGrip = 0;
            this.WindowColor = Colors.White;
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
