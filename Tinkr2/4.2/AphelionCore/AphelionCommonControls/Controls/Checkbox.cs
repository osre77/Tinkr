using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
   /// <summary>
   /// Displays an interactive check box
   /// </summary>
   [Serializable]
   public class Checkbox : Control
   {
      #region Variables

      private bool _checked;
      private string _text;
      private Font _font;
      private Font _chkFnt;
      private int _chkH, _chkX, _chkY;

      private Color _fore;
      private Color _brdr;
      private Color _bkgTop;
      private Color _bkgBtm;
      private Color _markUn;
      private Color _mark;

      #endregion

      #region Constructors

      /// <summary>
      /// Creates a new check box
      /// </summary>
      /// <param name="name">Name of the check box</param>
      /// <param name="text">Content text of the button</param>
      /// <param name="font">Font to render the content text</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="checkValue">Initial value of the check box</param>
      /// <remarks>
      /// The <see cref="Control.Width"/> and <see cref="Control.Height"/> are calculated automatically to fit the content text.
      /// </remarks>
      public Checkbox(string name, string text, Font font, int x, int y, bool checkValue = false) :
         base(name, x, y)
      {
         _text = text;
         _font = font;
         _checked = checkValue;
         GetChkFont(font.Height);
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         Width = _chkH + 12 + FontManager.ComputeExtentEx(font, text).Width;
         Height = System.Math.Max(_chkH, _font.Height);
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         DefaultColors();
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="name">Name of the check box</param>
      /// <param name="text">Content text of the button</param>
      /// <param name="font">Font to render the content text</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="height">Height in pixel</param>
      /// <param name="checkValue">Initial value of the check box</param>
      /// <remarks>
      /// The <see cref="Control.Width"/> is calculated automatically to fit the content text.
      /// </remarks>
      public Checkbox(string name, string text, Font font, int x, int y, int height, bool checkValue = false) :
         base(name, x, y, 0, height)
      {
         _text = text;
         _font = font;
         _checked = checkValue;
         GetChkFont(height);
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         Width = _chkH + 12 + FontManager.ComputeExtentEx(font, text).Width;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor

         DefaultColors();
      }

      #endregion

      #region Events

      /// <summary>
      /// Adds or removes callback methods for CheckChanged events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a button release occurs
      /// </remarks>
      public event OnCheckChanged CheckChanged;

      /// <summary>
      /// Fires the <see cref="CheckChanged"/> event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="checkValue">New check value</param>
      protected virtual void OnCheckChanged(object sender, bool checkValue)
      {
         if (CheckChanged != null)
         {
            CheckChanged(sender, checkValue);
         }
      }

      #endregion

      #region Properties

      /// <summary>
      /// Gets/Sets bottom color to use in background gradient
      /// </summary>
      public Color BackgroundBottom
      {
         get { return _bkgBtm; }
         set
         {
            if (_bkgBtm == value)
            {
               return;
            }
            _bkgBtm = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets top color to use in background gradient
      /// </summary>
      public Color BackgroundTop
      {
         get { return _bkgTop; }
         set
         {
            if (_bkgTop == value)
            {
               return;
            }
            _bkgTop = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets the color to use for the border
      /// </summary>
      public Color BorderColor
      {
         get { return _brdr; }
         set
         {
            if (_brdr == value)
            {
               return;
            }
            _brdr = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets the color to use for the text
      /// </summary>
      public Color ForeColor
      {
         get { return _fore; }
         set
         {
            if (_fore == value)
            {
               return;
            }
            _fore = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets the font to use for the text
      /// </summary>
      public Font Font
      {
         get { return _font; }
         set
         {
            if (_font == value)
            {
               return;
            }
            _font = value;
            GetChkFont(_font.Height);
            Width = _chkH + 12 + FontManager.ComputeExtentEx(_font, _text).Width;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets the height of the control in pixels
      /// </summary>
      public override int Height
      {
         get { return base.Height; }
         set
         {
            base.Height = value;
            GetChkFont(Height);
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets the color to use for the mark when unchecked
      /// </summary>
      public Color MarkColor
      {
         get { return _markUn; }
         set
         {
            if (_markUn == value)
            {
               return;
            }
            _markUn = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets the color to use for the mark when checked
      /// </summary>
      public Color MarkSelectedColor
      {
         get { return _mark; }
         set
         {
            if (_mark == value)
            {
               return;
            }
            _mark = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets the text of the check box
      /// </summary>
      public string Text
      {
         get { return _text; }
         set
         {
            if (_text == value)
            {
               return;
            }
            _text = value;
            Width = _chkH + 12 + FontManager.ComputeExtentEx(_font, _text).Width;
         }
      }

      /// <summary>
      /// Gets/Sets value of the check box
      /// </summary>
      public bool Value
      {
         get { return _checked; }
         set
         {
            if (_checked == value)
            {
               return;
            }
            _checked = value;
            Invalidate();
            OnCheckChanged(this, _checked);
         }
      }

      #endregion

      #region Buttons

      /// <summary>
      /// Override this message to handle button pressed events internally.
      /// </summary>
      /// <param name="buttonId">Integer ID corresponding to the affected button</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Toggles the checked value on <see cref="ButtonIDs.Click"/> or <see cref="ButtonIDs.Select"/>
      /// </remarks>
      protected override void ButtonPressedMessage(int buttonId, ref bool handled)
      {
         if (buttonId == (int) ButtonIDs.Click || buttonId == (int) ButtonIDs.Select)
         {
            Value = !_checked;
         }
         base.ButtonPressedMessage(buttonId, ref handled);
      }

      #endregion

      #region Keyboard

      /// <summary>
      /// Override this message to handle key events internally.
      /// </summary>
      /// <param name="key">Integer value of the key affected</param>
      /// <param name="pressed">True if the key is currently being pressed; false if released</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Toggles the checked value on return key (key == 10)
      /// </remarks>
      protected override void KeyboardKeyMessage(char key, bool pressed, ref bool handled)
      {
         if (key == 10)
         {
            Value = !_checked;
         }
         base.KeyboardKeyMessage(key, pressed, ref handled);
      }

      #endregion

      #region Touch

      /// <summary>
      /// Override this message to handle touch events internally.
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Toggles the checked value
      /// </remarks>
      protected override void TouchUpMessage(object sender, point point, ref bool handled)
      {
         if (Touching)
         {
            Value = !_checked;
         }
         base.TouchUpMessage(sender, point, ref handled);
      }

      #endregion

      #region GUI

      private void DefaultColors()
      {
         _fore = Core.SystemColors.FontColor;
         _brdr = Core.SystemColors.BorderColor;
         _bkgBtm = Core.SystemColors.ControlBottom;
         _bkgTop = Core.SystemColors.ControlTop;
         _mark = Core.SystemColors.SelectionColor;
         _markUn = Colors.DarkGray;
      }

      /// <summary>
      /// Renders the control contents
      /// </summary>
      /// <param name="x">X position in screen coordinates</param>
      /// <param name="y">Y position in screen coordinates</param>
      /// <param name="width">Width in pixel</param>
      /// <param name="height">Height in pixel</param>
      /// <remarks>
      /// Renders the check box
      /// </remarks>
      protected override void OnRender(int x, int y, int width, int height)
      {
         int cY = Top + (Height / 2 - _chkH / 2);

         // Draw Fill
         //int ho = (Height - 3) / 2;

         if (Core.FlatCheckboxes)
         {
            if (Enabled)
            {
               Core.Screen.DrawRectangle(0, 0, Left, cY, _chkH, _chkH, 0, 0, _bkgBtm, Left, cY, _bkgBtm, Left,
                  cY + (_chkH/2), 256);
            }
            else
            {
               Core.Screen.DrawRectangle(0, 0, Left, cY, _chkH, _chkH, 0, 0, _bkgTop, Left, cY, _bkgTop, Left, cY + (_chkH / 2), 256);
            }

            if (Focused)
            {
               Core.ShadowRegionInset(Left, cY, _chkH, _chkH, Core.SystemColors.SelectionColor);
            }
         }
         else
         {
            // Draw Outline
            Core.Screen.DrawRectangle((Parent.ActiveChild == this) ? Core.SystemColors.SelectionColor : _brdr, 1, Left, cY, _chkH, _chkH, 1, 1, 0, 0, 0, 0, 0, 0, 256);

            if (Enabled)
            {
               Core.Screen.DrawRectangle(0, 0, Left + 1, cY + 1, _chkH - 2, _chkH - 2, 0, 0, _bkgTop, Left, cY, _bkgBtm,
                  Left, cY + (_chkH/2), 256);
            }
            else
            {
               Core.Screen.DrawRectangle(0, 0, Left + 1, cY + 1, _chkH - 2, _chkH - 2, 0, 0, _bkgTop, Left, cY, _bkgTop, Left, cY + (_chkH / 2), 256);
            }

            if (Focused)
            {
               Core.ShadowRegionInset(Left + 1, cY + 1, _chkH - 2, _chkH - 2, Core.SystemColors.SelectionColor);
            }
         }

         // Draw Check mark
         cY -= _chkY;
         int cX = _chkH / 2 - _chkX;
         Core.Screen.DrawTextInRect("a", Left + cX, cY, _chkFnt.CharWidth('a'), _chkFnt.Height, Bitmap.DT_AlignmentCenter, (_checked) ? _mark : _markUn, _chkFnt);

         // Draw Text
         Core.Screen.DrawText(_text, _font, _fore, Left + _chkH + 6, Top + (height / 2 - _font.Height / 2));


         // no need to call base.OnRender
      }

      private void GetChkFont(int baseH)
      {
         _chkH = baseH;
         _chkY = 0;

         if (_chkH < 6)
         {
            _chkH = 6;
         }

         if (_chkH < 14)
         {
            _chkH = 12;
            _chkX = 6;
            _chkY += 3;
            _chkFnt = Resources.GetFont(Resources.FontResources.chk8);
         }
         else if (_chkH < 16)
         {
            _chkH = 14;
            _chkX = 7;
            _chkY += 3;
            _chkFnt = Resources.GetFont(Resources.FontResources.chk12);
         }
         else if (_chkH < 21)
         {
            _chkH = 16;
            _chkX = 9;
            _chkY += 5;
            _chkFnt = Resources.GetFont(Resources.FontResources.chk16);
         }
         else if (_chkH < 26)
         {
            _chkH = 21;
            _chkX = 14;
            _chkY += 7;
            _chkFnt = Resources.GetFont(Resources.FontResources.chk24);
         }
         else if (_chkH < 36)
         {
            _chkH = 26;
            _chkX = 20;
            _chkY += 11;
            _chkFnt = Resources.GetFont(Resources.FontResources.chk32);
         }
         else if (_chkH < 51)
         {
            _chkH = 36;
            _chkX = 30;
            _chkY += 17;
            _chkFnt = Resources.GetFont(Resources.FontResources.chk48);
         }
         else
         {
            _chkH = 51;
            _chkX = 45;
            _chkY += 26;
            _chkFnt = Resources.GetFont(Resources.FontResources.chk72);
         }
      }
      #endregion
   }
}
