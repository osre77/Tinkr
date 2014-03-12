using System;
using System.Threading;

using Microsoft.SPOT;

namespace Skewworks.NETMF.Controls
{
   [Serializable]
   public class Scrollbar : Control
   {

      #region Variables

      private bool _gDown;
      private int _min;
      private int _max;
      private int _val;
      private int _sml = 1;
      private int _lrg = 10;
      private Orientation _orientation;

      private rect _decRect;
      private rect _incRect;
      private rect _grpRect;
      private int _chgVal;
      private Thread _changer;

      private int _mY;
      private int _mX;

      private bool _auto = true;

      private readonly Bitmap _up = Resources.GetBitmap(Resources.BitmapResources.up);
      private readonly Bitmap _down = Resources.GetBitmap(Resources.BitmapResources.down);
      private readonly Bitmap _left = Resources.GetBitmap(Resources.BitmapResources.left);
      private readonly Bitmap _right = Resources.GetBitmap(Resources.BitmapResources.right);

      #endregion

      #region Constructors

      public Scrollbar(string name, int x, int y, int width, int height, Orientation orientation)
      {
         _mX = 0;
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _orientation = orientation;
         _max = 100;
      }

      public Scrollbar(string name, int x, int y, int width, int height, Orientation orientation, int minimum, int maximum, int value)
      {
         _mX = 0;
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _orientation = orientation;
         _min = minimum;
         _max = maximum;
         _val = value;
      }

      #endregion

      #region Buttons

      protected override void ButtonPressedMessage(int buttonId, ref bool handled)
      {
         if (_orientation == Orientation.Horizontal)
         {
            if (buttonId == (int)ButtonIDs.Left)
               Value -= 1;
            else if (buttonId == (int)ButtonIDs.Right)
               Value += 1;
         }
         else
         {
            if (buttonId == (int)ButtonIDs.Up)
               Value -= 1;
            else if (buttonId == (int)ButtonIDs.Down)
               Value += 1;
         }
      }

      #endregion

      #region Keyboard

      protected override void KeyboardKeyMessage(char key, bool pressed, ref bool handled)
      {
         if (_orientation == Orientation.Horizontal)
         {
            if (key == 80)          // Left
               Value -= 1;
            else if (key == 79)     // Right
               Value += 1;
         }
         else
         {
            if (key == 82)          // Up
               Value -= 1;
            else if (key == 81)     // Down
               Value += 1;
         }
      }

      #endregion

      #region Touch

      protected override void TouchDownMessage(object sender, point e, ref bool handled)
      {
         if (_decRect.Contains(e))
         {
            Value = _val - _sml;
            _chgVal = -_sml;
            if (_auto)
            {
               _changer = new Thread(AutoIncDec)
               {
                  Priority = ThreadPriority.AboveNormal
               };
               _changer.Start();
            }
            return;
         }

         if (_incRect.Contains(e))
         {
            Value = _val + _sml;
            _chgVal = _sml;
            if (_auto)
            {
               _changer = new Thread(AutoIncDec)
               {
                  Priority = ThreadPriority.AboveNormal
               };
               _changer.Start();
            }
            return;
         }

         if (_grpRect.Contains(e))
         {
            _mX = e.X;
            _mY = e.Y;
            _gDown = true;
            return;
         }

         if (_orientation == Orientation.Horizontal)
            Value = _val + ((e.X < _grpRect.X) ? -_lrg : _lrg);
         else
            Value = _val + ((e.Y < _grpRect.Y) ? -_lrg : _lrg);
      }

      protected override void TouchMoveMessage(object sender, point e, ref bool handled)
      {
         if (!_gDown)
            return;

         if (_orientation == Orientation.Horizontal)
            Value = (int)((_max - _min) * (((_grpRect.X + (e.X - _mX)) - Left - 16) / (float)(Width - 32 - _grpRect.Width))) + _min;
         else
            Value = (int)((_max - _min) * (((_grpRect.Y + (e.Y - _mY)) - Top - 16) / (float)(Height - 32 - _grpRect.Height))) + _min;

         _mX = e.X;
         _mY = e.Y;
      }

      protected override void TouchUpMessage(object sender, point e, ref bool handled)
      {
         _gDown = false;
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

      /// <summary>
      /// Gets/Sets auto repeat mode
      /// </summary>
      public bool AutoRepeating
      {
         get { return _auto; }
         set
         {
            _auto = value;
         }
      }

      /// <summary>
      /// Gets/Sets large change value
      /// </summary>
      public int LargeChange
      {
         get { return _lrg; }
         set
         {
            if (_lrg == value)
               return;
            if (value < 1)
               value = 1;
            _lrg = value;
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

      public Orientation Orientation
      {
         get { return _orientation; }
         set
         {
            if (_orientation == value)
               return;

            Suspended = true;
            int t = Width;
            Height = Width;
            Width = t;
            _orientation = value;

            Suspended = false;
         }
      }

      /// <summary>
      /// Gets/Sets small change value
      /// </summary>
      public int SmallChange
      {
         get { return _sml; }
         set
         {
            if (value < 1)
               value = 1;
            if (_sml == value)
               return;
            _sml = value;
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

      #endregion

      #region GUI

// ReSharper disable RedundantAssignment
      protected override void OnRender(int x, int y, int w, int h)
// ReSharper restore RedundantAssignment
      {
         int iGripSize, iOffset;

         x = Left;
         y = Top;

         if (_orientation == Orientation.Vertical)
         {
            if (Enabled)
            {
               Core.Screen.DrawRectangle(0, 0, x + 1, y + 1, Width - 2, 16, 0, 0, Core.SystemColors.ControlTop, Left, Top, Core.SystemColors.ControlBottom, Left + (Width / 2), Top, 256);
               Core.Screen.DrawImage(x + (Width / 2 - _up.Width / 2), y + (8 - _up.Height / 2), _up, 0, 0, _up.Width, _up.Height);
               Core.Screen.DrawRectangle(0, 0, x + 1, y + Height - 16, Width - 2, 16, 0, 0, Core.SystemColors.ControlTop, Left, Top, Core.SystemColors.ControlBottom, Left + (Width / 2), Top, 256);
               Core.Screen.DrawImage(x + (Width / 2 - _down.Width / 2), y + Height - 17 + (8 - _down.Height / 2), _down, 0, 0, _down.Width, _down.Height);

               _decRect = new rect(x + 1, y + 1, Width - 2, 16);
               _incRect = new rect(x + 1, y + Height - 17, Width - 2, 16);
            }
            else
            {
               Core.Screen.DrawRectangle(0, 0, x + 1, y + 1, Width - 1, 16, 0, 0, Core.SystemColors.ControlTop, Left, Top, Core.SystemColors.ControlTop, Left + (Width / 2), Top, 256);
               Core.Screen.DrawImage(x + (Width / 2 - _up.Width / 2), y + (8 - _up.Height / 2), _up, 0, 0, _up.Width, _up.Height, 128);
               Core.Screen.DrawRectangle(0, 0, x + 1, y + Height - 17, Width - 2, 16, 0, 0, Core.SystemColors.ControlTop, Left, Top, Core.SystemColors.ControlTop, Left + (Width / 2), Top, 256);
               Core.Screen.DrawImage(x + (Width / 2 - _down.Width / 2), y + Height - 17 + (8 - _down.Height / 2), _down, 0, 0, _down.Width, _down.Height, 128);

               _decRect = new rect(0, 0, 0, 0);
               _incRect = new rect(0, 0, 0, 0);
            }

            iGripSize = (Height - 34) - ((_max - _min) / _sml);
            if (iGripSize < 8)
               iGripSize = 8;

            iOffset = y + 16 + (int)((Height - 34 - iGripSize) * ((_val - _min) / (float)(_max - _min)));

            if (iOffset > y + 16)
               Core.Screen.DrawRectangle(0, 0, x + 1, y + 16, Width - 2, iOffset - y - 17, 0, 0,
                   Core.SystemColors.ControlBottom, x, y, (Enabled) ? Core.SystemColors.ControlTop : Core.SystemColors.ControlBottom, x + Width / 2, y, 256);

            if (iOffset + iGripSize < Top + Height - 21)
               Core.Screen.DrawRectangle(0, 0, x + 1, iOffset + iGripSize, Width - 2, (Top + Height - 18) - (iOffset + iGripSize), 0, 0,
                   Core.SystemColors.ControlBottom, x, y, (Enabled) ? Core.SystemColors.ControlTop : Core.SystemColors.ControlBottom, x + Width / 2, y, 256);

            Core.Screen.DrawRectangle(Colors.DarkGray, 1, x, iOffset, Width, iGripSize, 0, 0, Core.SystemColors.ControlTop, x, y, Core.SystemColors.ControlBottom, x + Width / 2, y, 256);

            Core.Screen.DrawLine(Colors.DarkGray, 1, x + 1, y + 16, x + Width - 2, y + 16);
            Core.Screen.DrawLine(Colors.DarkGray, 1, x + 1, y + Height - 18, x + Width - 2, y + Height - 18);

            _grpRect = new rect(Left, iOffset, Width, iGripSize);
         }
         else
         {
            if (Enabled)
            {
               Core.Screen.DrawRectangle(0, 0, x + 1, y + 1, 16, Height - 2, 0, 0, Core.SystemColors.ControlTop, Left, Top, Core.SystemColors.ControlBottom, Left, Top + (Height / 2), 256);
               Core.Screen.DrawImage(x + (8 - _left.Width / 2), y + (Height / 2 - _left.Height / 2), _left, 0, 0, _left.Width, _left.Height);
               Core.Screen.DrawRectangle(0, 0, x + Width - 17, y + 1, 16, Height - 2, 0, 0, Core.SystemColors.ControlTop, Left, Top, Core.SystemColors.ControlBottom, Left, Top + (Height / 2), 256);
               Core.Screen.DrawImage(x + Width - 17 + (8 - _right.Width / 2), y + (Height / 2 - _right.Height / 2), _right, 0, 0, _right.Width, _right.Height);

               _decRect = new rect(x + 1, y + 1, 16, Height - 3);
               _incRect = new rect(x + Width - 18, y + 1, 16, Height - 3);
            }
            else
            {
               Core.Screen.DrawRectangle(0, 0, x + 1, y + 1, 16, Height - 2, 0, 0, Core.SystemColors.ControlTop, Left, Top, Core.SystemColors.ControlTop, Left, Top + (Height / 2), 256);
               Core.Screen.DrawImage(x + (8 - _left.Width / 2), y + (Height / 2 - _left.Height / 2), _left, 0, 0, _left.Width, _left.Height, 128);
               Core.Screen.DrawRectangle(0, 0, x + 1, y + Height - 17, Width - 2, 16, 0, 0, Core.SystemColors.ControlTop, Left, Top, Core.SystemColors.ControlTop, Left + (Width / 2), Top, 256);
               Core.Screen.DrawImage(x + (Width / 2 - _right.Width / 2), y + Height - 17 + (8 - _right.Height / 2), _right, 0, 0, _right.Width, _right.Height, 128);

               _decRect = new rect(0, 0, 0, 0);
               _incRect = new rect(0, 0, 0, 0);
            }

            iGripSize = (Width - 34) - ((_max - _min) / _sml);
            if (iGripSize < 8)
               iGripSize = 8;

            iOffset = x + 16 + (int)((Width - 34 - iGripSize) * ((_val - _min) / (float)(_max - _min)));

            if (iOffset > x + 16)
               Core.Screen.DrawRectangle(0, 0, x + 16, y + 1, iOffset - x - 16, Height - 2, 0, 0,
                   (Enabled) ? Core.SystemColors.ControlBottom : Core.SystemColors.ControlTop, x, y, (Enabled) ? Core.SystemColors.ControlTop : Core.SystemColors.ControlBottom, x, y + Height / 2, 256);

            if (iOffset + iGripSize < x + Width - 21)
               Core.Screen.DrawRectangle(0, 0, iOffset + iGripSize, y + 1, (x + Width - 18) - (iOffset + iGripSize), Height - 2, 0, 0,
                   Core.SystemColors.ControlBottom, x, y, (Enabled) ? Core.SystemColors.ControlTop : Colors.LightGray, x, y + Height / 2, 256);

            Core.Screen.DrawRectangle(Colors.DarkGray, 1, iOffset, y, iGripSize, Height, 0, 0, Core.SystemColors.ControlTop, x, y, Core.SystemColors.ControlBottom, x, y + Height / 2, 256);

            Core.Screen.DrawLine(Colors.DarkGray, 1, x + 16, y + 1, x + 16, y + Height - 2);
            Core.Screen.DrawLine(Colors.DarkGray, 1, x + Width - 19, y + 1, x + Width - 19, y + Height - 2);

            _grpRect = new rect(iOffset, Top, iGripSize, Height);
         }

         Core.Screen.DrawRectangle((Focused) ? Core.SystemColors.SelectionColor : Core.SystemColors.BorderColor, 1, Left, Top, Width, Height, 1, 1, 0, 0, 0, 0, 0, 0, 0);
      }

      #endregion

      #region Private Methods

      private void AutoIncDec()
      {
         int iWait = 500;
         while (Touching)
         {
            Thread.Sleep(iWait);
            if (!Touching)
               return;
            switch (iWait)
            {
               case 500:
                  iWait = 250;
                  Value = _val + _chgVal;
                  break;
               case 250:
                  iWait = 125;
                  Value = _val + (_chgVal * 2);
                  break;
               case 125:
                  iWait = 75;
                  Value = _val + (_chgVal * 3);
                  break;
               default:
                  Value = _val + (_chgVal * 4);
                  break;
            }
         }
      }

      #endregion

   }
}