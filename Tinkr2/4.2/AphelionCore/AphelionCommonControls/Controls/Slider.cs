using System;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
   [Serializable]
   public class Slider : Control
   {

      #region Variables

      private bool _gDown;
      private int _min;
      private int _max;
      private int _val;
      private int _lrg;
      private Orientation _orientation;
      private int _gripSize;

      private rect _grpRect;

      //private int _mY = 0;
      //private int _mX = 0;

      private Color _bkg;
      private Color _top;
      private Color _btm;

      #endregion

      #region Constructors

      public Slider(string name, int x, int y, int width, int height, Orientation orientation, int gripSize = 10)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _orientation = orientation;
         _gripSize = gripSize;
         _max = 100;
         _val = 30;
         _lrg = 10;
         DefaultColors();
      }

      public Slider(string name, int x, int y, int width, int height, Orientation orientation, int gripSize, int minimum = 0, int maximum = 100, int value = 0)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _orientation = orientation;
         _gripSize = gripSize;
         _min = minimum;
         _max = maximum;
         _val = value;
         _lrg = 10;
         DefaultColors();
      }

      #endregion

      #region Touch Invokes

      protected override void TouchDownMessage(object sender, point point, ref bool handled)
      {
         if (_grpRect.Contains(point))
         {
            //_mX = e.X;
            //_mY = e.Y;
            _gDown = true;
            return;
         }

         if (_orientation == Orientation.Horizontal)
            Value = _val + ((point.X < _grpRect.X) ? -_lrg : _lrg);
         else
            Value = _val + ((point.Y < _grpRect.Y) ? -_lrg : _lrg);
      }

      protected override void TouchMoveMessage(object sender, point point, ref bool handled)
      {
         if (_gDown)
         {
            if (_orientation == Orientation.Horizontal)
               Value = _min + (int)((_max - _min) * ((float)point.X) / (Width - _gripSize));
            else
               Value = _min + (int)((_max - _min) * ((float)point.Y) / (Height - _gripSize));

            //_mX = e.X;
            //_mY = e.Y;   
         }
         base.TouchMoveMessage(sender, point, ref handled);
      }

      protected override void TouchUpMessage(object sender, point point, ref bool handled)
      {
         _gDown = false;
         base.TouchUpMessage(sender, point, ref handled);
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

      public Color BackColor
      {
         get { return _bkg; }
         set
         {
            if (_bkg == value)
               return;
            _bkg = value;
            Invalidate();
         }
      }

      public Color GradientBottom
      {
         get { return _btm; }
         set
         {
            if (_btm == value)
               return;
            _btm = value;
            Invalidate();
         }
      }

      public Color GradientTop
      {
         get { return _top; }
         set
         {
            if (_top == value)
               return;
            _top = value;
            Invalidate();
         }
      }

      public int GripSize
      {
         get { return _gripSize; }
         set
         {
            if (_gripSize == value)
               return;
            _gripSize = value;
            Invalidate();
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

      private void DefaultColors()
      {
         _bkg = Colors.Black;
         _top = Core.SystemColors.ControlTop;
         _btm = Core.SystemColors.ControlBottom;
      }

      // ReSharper disable RedundantAssignment
      protected override void OnRender(int x, int y, int width, int height)
      // ReSharper restore RedundantAssignment
      {
         int iOffset;

         x = Left;
         y = Top;

         if (_orientation == Orientation.Vertical)
         {
            Core.Screen.DrawRectangle(0, 0, x + Width / 2 - 3, y, 6, Height, 0, 0, _bkg, 0, 0, _bkg, 0, 0, 128);
            iOffset = y + (int)((Height - _gripSize) * ((_val - _min) / (float)(_max - _min)));
            Core.Screen.DrawRectangle((Focused) ? Core.SystemColors.SelectionColor : Core.SystemColors.BorderColor, 1, x, iOffset, Width - 1, _gripSize, 0, 0,
                _top, x, y, _btm, x + Width / 2, y, 256);
            _grpRect = new rect(x, iOffset, Width, _gripSize);
         }
         else
         {
            Core.Screen.DrawRectangle(0, 0, x, y + Height / 2 - 3, Width, 6, 0, 0, _bkg, 0, 0, _bkg, 0, 0, 128);
            iOffset = x + (int)((Width - _gripSize) * ((_val - _min) / (float)(_max - _min)));
            Core.Screen.DrawRectangle((Focused) ? Core.SystemColors.SelectionColor : Core.SystemColors.BorderColor, 1, iOffset, y, _gripSize, Height - 1, 0, 0,
                 _top, x, y, _btm, x, y + Height / 2, 256);
            _grpRect = new rect(iOffset, y, _gripSize, Height);
         }
      }

      #endregion

   }
}
