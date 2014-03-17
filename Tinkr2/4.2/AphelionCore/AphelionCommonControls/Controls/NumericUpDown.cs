using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
   [Serializable]
   public class NumericUpDown : Control
   {

      #region Variables

      private Color _bkgTop;
      private Color _bkgBtm;
      private Font _font;
      private Color _btnTop;
      private Color _btnBtm;
      private Color _fore;
      private Color _disabled;
      private Color _btnTxt;
      private Color _btnTxtShd;

      private int _min;
      private int _max = 100;
      private int _val;
      private bool _force;
      private int _h;

      private int _chgVal;
      private bool _rep;

      private Thread _changer;
      private int _btnW;

      private bool _moved;

      #endregion

      #region Constructors

      public NumericUpDown(string name, Font font, int x, int y, int width, bool forceZeros = false, int minimum = 0, int maximum = 100, int value = 0)
      {
         _val = 0;
         Name = name;
         _font = font;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _h = _font.Height + 7;
         _force = forceZeros;
         _min = minimum;
         _max = maximum;
         _val = value;
         DefaultColors();
      }

      public NumericUpDown(string name, Font font, int x, int y, int width, int height, bool forceZeros = false, int minimum = 0, int maximum = 100, int value = 0)
      {
         _val = 0;
         Name = name;
         _font = font;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _h = height;
         _force = forceZeros;
         _min = minimum;
         _max = maximum;
         _val = value;
         DefaultColors();
      }

      #endregion

      #region Events

      public event OnValueChanged ValueChanged;

      protected virtual void OnValueChanged(Object sender, int value)
      {
         if (ValueChanged != null)
            ValueChanged(sender, value);
      }

      #endregion

      #region  Properties

      public Color BackgroundBottom
      {
         get { return _bkgBtm; }
         set
         {
            if (_bkgBtm == value)
               return;
            _bkgBtm = value;
            Invalidate();
         }
      }

      public Color BackgroundTop
      {
         get { return _bkgTop; }
         set
         {
            if (_bkgTop == value)
               return;
            _bkgTop = value;
            Invalidate();
         }
      }

      public Color ButtonBackgroundBottom
      {
         get { return _btnBtm; }
         set
         {
            if (_btnBtm == value)
               return;
            _btnBtm = value;
            Invalidate();
         }
      }

      public Color ButtonBackgroundTop
      {
         get { return _btnTop; }
         set
         {
            if (_btnTop == value)
               return;
            _btnTop = value;
            Invalidate();
         }
      }

      public Color ButtonTextColor
      {
         get { return _btnTxt; }
         set
         {
            if (_btnTxt == value)
               return;
            _btnTxt = value;
            Invalidate();
         }
      }

      public Color ButtonTextShadowColor
      {
         get { return _btnTxtShd; }
         set
         {
            if (_btnTxtShd == value)
               return;
            _btnTxtShd = value;
            Invalidate();
         }
      }

      public Color DisabledTextColor
      {
         get { return _disabled; }
         set
         {
            if (_disabled == value)
               return;
            _disabled = value;
            if (!Enabled)
               Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets font
      /// </summary>
      public Font Font
      {
         get { return _font; }
         set
         {
            if (_font == value)
               return;
            _font = value;
            if (Height < _font.Height + 7)
               Height = Font.Height + 7;

            Invalidate();
         }
      }

      /// <summary>
      /// Forces leading zeros when true
      /// </summary>
      public bool ForceZeros
      {
         get { return _force; }
         set
         {
            if (_force == value)
               return;
            _force = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets minimum value
      /// </summary>
      public int Minimum
      {
         get { return _min; }
         set
         {
            if (value >= _max)
               value = _max - 1;
            if (_min == value)
               return;
            _min = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets maximum value
      /// </summary>
      public int Maximum
      {
         get { return _max; }
         set
         {
            if (value <= _min)
               value = _min + 1;
            if (_max == value)
               return;
            _max = value;
            Invalidate();
         }
      }

      public Color TextColor
      {
         get { return _fore; }
         set
         {
            if (_fore == value)
               return;
            _fore = value;
            if (Enabled)
               Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets current value
      /// </summary>
      public int Value
      {
         get { return _val; }
         set
         {
            if (value < _min)
               value = _min;
            if (value > _max)
               value = _max;
            if (_val == value)
               return;
            _val = value;
            Invalidate();
            OnValueChanged(this, _val);
         }
      }

      /// <summary>
      /// Gets height (set ignored)
      /// </summary>
      public override int Height
      {
         get { return _h; }
         set
         {
            if (value < _font.Height + 7)
               value = _font.Height + 7;
            if (_h == value)
               return;
            _h = value;
            Invalidate();
         }
      }

      #endregion

      #region Button Methods

      protected override void ButtonPressedMessage(int buttonId, ref bool handled)
      {
         if (buttonId == (int)ButtonIDs.Up || buttonId == (int)ButtonIDs.Right)
         {
            Value = _val + 1;
            _chgVal = 1;
            _rep = true;
            _changer = new Thread(AutoIncDec)
            {
               Priority = ThreadPriority.AboveNormal
            };
            _changer.Start();
         }
         else if (buttonId == (int)ButtonIDs.Down || buttonId == (int)ButtonIDs.Left)
         {
            Value = _val - 1;
            _chgVal = -1;
            _rep = true;
            _changer = new Thread(AutoIncDec)
            {
               Priority = ThreadPriority.AboveNormal
            };
            _changer.Start();
         }
      }

      protected override void ButtonReleasedMessage(int buttonId, ref bool handled)
      {
         if (buttonId == (int)ButtonIDs.Down || buttonId == (int)ButtonIDs.Up)
            _rep = false;
      }

      #endregion

      #region Keyboard

      protected override void KeyboardAltKeyMessage(int key, bool pressed, ref bool handled)
      {
         if (pressed)
         {
            switch (key)
            {
               case 82:        // Up
               case 79:        // Right
                  Value = _val + 1;
                  _chgVal = 1;
                  _rep = true;
                  _changer = new Thread(AutoIncDec)
                  {
                     Priority = ThreadPriority.AboveNormal
                  };
                  _changer.Start();
                  break;
               case 80:        // Left
               case 81:        // Down
                  Value = _val - 1;
                  _chgVal = -1;
                  _rep = true;
                  _changer = new Thread(AutoIncDec)
                  {
                     Priority = ThreadPriority.AboveNormal
                  };
                  _changer.Start();
                  break;
            }
         }
         else
         {
            switch (key)
            {
               case 82:        // Up
               case 79:        // Right
               case 80:        // Left
               case 81:        // Down
                  _rep = false;
                  break;
            }
         }
         base.KeyboardAltKeyMessage(key, pressed, ref handled);
      }

      #endregion

      #region Touch Methods

      protected override void TouchDownMessage(object sender, point point, ref bool handled)
      {
         point.X -= Left;
         if (point.X < _btnW)
         {
            // Dec
            Value = _val - 1;
            _chgVal = -1;

            _rep = true;
            _changer = new Thread(AutoIncDec)
            {
               Priority = ThreadPriority.AboveNormal
            };
            _changer.Start();
         }
         else if (point.X >= Width - _btnW - 1)
         {
            // Inc
            Value = _val + 1;
            _chgVal = 1;

            _rep = true;
            _changer = new Thread(AutoIncDec)
            {
               Priority = ThreadPriority.AboveNormal
            };
            _changer.Start();
         }
      }

      protected override void TouchGestureMessage(object sender, TouchType type, float force, ref bool handled)
      {

      }

      protected override void TouchMoveMessage(object sender, point point, ref bool handled)
      {
         if (Touching)
         {
            if (!_moved)
            {
               int diff = System.Math.Abs(LastTouch.X - point.X);
               if (diff > 20)
               {
                  if (point.X > LastTouch.X)
                  {
                     Value = _val + 1;
                     _chgVal = 1;
                  }
                  else
                  {
                     Value = _val - 1;
                     _chgVal = -1;
                  }
                  _rep = true;
                  _changer = new Thread(AutoIncDec)
                  {
                     Priority = ThreadPriority.AboveNormal
                  };
                  _changer.Start();
                  _moved = true;
               }
            }
            else
            {
               int diff = System.Math.Abs(LastTouch.X - point.X);
               if (diff > 20)
               {
                  if (point.X > LastTouch.X)
                  {
                     _chgVal = 1;
                  }
                  else
                  {
                     _chgVal = -1;
                  }
               }
            }
         }
         base.TouchMoveMessage(sender, point, ref handled);
      }

      protected override void TouchUpMessage(object sender, point point, ref bool handled)
      {
         _moved = false;
         _rep = false;
         base.TouchUpMessage(sender, point, ref handled);
      }

      #endregion

      #region GUI

      protected override void OnRender(int x, int y, int width, int height)
      {
         _btnW = System.Math.Max(_font.CharWidth('-'), _font.CharWidth('+')) + 8;
         int txtY = _h / 2 - _font.Height / 2;

         // Draw Shadow
         Core.Screen.DrawRectangle(Colors.White, 1, Left + 1, Top + 1, Width - 1, Height - 1, 1, 1, 0, 0, 0, 0, 0, 0, 0);

         // Draw Border
         Core.Screen.DrawRectangle((Focused) ? Core.SystemColors.SelectionColor : Core.SystemColors.BorderColor, 1,
             Left, Top, Width - 1, Height - 1, 1, 1, 0, 0, 0, 0, 0, 0, 0);

         // Draw Minus Button
         if (Enabled)
         {
            Core.Screen.DrawRectangle(0, 0, Left + 1, Top + 1, _btnW, Height - 3, 0, 0, _btnTop, Left, Top, _btnBtm, Left, Top + Height, 256);
            Core.Screen.DrawTextInRect("-", Left + 2, Top + txtY + 1, _btnW, _font.Height, Bitmap.DT_AlignmentCenter, _btnTxtShd, _font);
         }
         else
            Core.Screen.DrawRectangle(0, 0, Left + 1, Top + 1, _btnW, Height - 3, 0, 0, _btnTop, Left, Top, _btnTop, Left, Top + Height, 256);
         Core.Screen.DrawTextInRect("-", Left + 1, Top + txtY, _btnW, _font.Height, Bitmap.DT_AlignmentCenter, _btnTxt, _font);

         // Draw Number
         Core.Screen.DrawRectangle((Focused) ? Core.SystemColors.SelectionColor : Core.SystemColors.BorderColor, 1,
             Left + _btnW + 1, Top, Width - _btnW - _btnW - 3, Height - 1, 0, 0, _bkgTop, Left, Top, (Enabled) ? _bkgBtm : _bkgTop,
             Left, Top + Height, 256);
         Core.Screen.DrawTextInRect(IntToNString(Value, Maximum.ToString().Length), Left + _btnW + 4, Top + txtY,
             Width - _btnW - _btnW - 9, Height - 1, Bitmap.DT_AlignmentRight, (Enabled) ? _fore : _disabled, _font);

         // Draw Minus Button
         Core.Screen.DrawRectangle(0, 0, Left + Width - _btnW - 2, Top + 1, _btnW, Height - 3, 0, 0, _btnTop, Left, Top,
             (Enabled) ? _btnBtm : _btnTop, Left, Top + Height, 256);
         if (Enabled)
            Core.Screen.DrawTextInRect("+", Left + Width - _btnW - 1, Top + txtY + 1, _btnW, _font.Height, Bitmap.DT_AlignmentCenter,
                _btnTxtShd, _font);
         Core.Screen.DrawTextInRect("+", Left + Width - _btnW - 2, Top + txtY, _btnW, _font.Height, Bitmap.DT_AlignmentCenter, _btnTxt, _font);

         if (Focused)
         {
            Core.Screen.DrawRectangle(0, 0, x + 1, y + 1, width - 3, 1, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 90);
            Core.Screen.DrawRectangle(0, 0, x + 1, y + height - 3, width - 3, 1, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 90);
            Core.Screen.DrawRectangle(0, 0, x + 1, y + 2, 1, height - 5, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 90);
            Core.Screen.DrawRectangle(0, 0, x + width - 3, y + 2, 1, height - 5, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 90);
            Core.Screen.DrawRectangle(0, 0, x + 2, y + 2, width - 5, 1, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 40);
            Core.Screen.DrawRectangle(0, 0, x + 2, y + height - 4, width - 5, 1, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 40);
            Core.Screen.DrawRectangle(0, 0, x + 2, y + 4, 1, height - 7, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 40);
            Core.Screen.DrawRectangle(0, 0, x + width - 4, y + 3, 1, height - 7, 0, 0, Core.SystemColors.SelectionColor, 0, 0, Core.SystemColors.SelectionColor, 0, 0, 40);
         }
      }

      #endregion

      #region Private Methods

      private void AutoIncDec()
      {
         int iWait = 750;
         while (_rep)
         {
            long lEnd = DateTime.Now.Ticks + (TimeSpan.TicksPerMillisecond * iWait);
            while (_rep && DateTime.Now.Ticks < lEnd)
               Thread.Sleep(1);
            if (!_rep)
               return;
            Value = _val + _chgVal;
            switch (iWait)
            {
               case 750:
                  iWait = 500;
                  break;
               case 500:
                  iWait = 250;
                  break;
               case 250:
                  iWait = 75;
                  break;
            }
            if (!_rep)
               return;
         }
      }

      private void DefaultColors()
      {
         _bkgTop = Core.SystemColors.ControlTop;
         _bkgBtm = Core.SystemColors.ControlBottom;
         _btnTop = Colors.LightGray;
         _btnBtm = Colors.DarkGray;
         _fore = Core.SystemColors.FontColor;
         _btnTxt = Colors.White;
         _disabled = Colors.DarkGray;
         _btnTxtShd = Colors.Gray;
      }

      private string IntToNString(int value, int len)
      {
         if (!_force)
            return value.ToString();

         string val = value.ToString();
         if (val.Length >= len) return val;

         while (val.Length < len)
            val = "0" + val;

         return val;
      }

      #endregion

   }
}
