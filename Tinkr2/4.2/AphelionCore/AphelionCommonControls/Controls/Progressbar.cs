using System;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
   [Serializable]
   public class Progressbar : Control
   {

      #region Variables

      private long _min, _max, _val;
      private Color _top, _btm, _border, _bkg;
      HorizontalAlignment _align;

      #endregion

      #region Constructors

      public Progressbar(string name, int x, int y, int width, int height)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _max = 100;
         _align = HorizontalAlignment.Left;
         DefaultColors();
      }

      public Progressbar(string name, int x, int y, int width, int height, long minimum, long maximum, long value)
      {
         Name = name;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
         _min = minimum;
         _max = maximum;
         _val = value;
         _align = HorizontalAlignment.Left;
         DefaultColors();
      }

      #endregion

      #region Events

      public event OnProgressChanged ProgressChanged;

      protected virtual void OnProgressChanged(Object sender, long value)
      {
         if (ProgressChanged != null)
            ProgressChanged(sender, value);
      }

      #endregion

      #region Properties

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

      public Color BorderColor
      {
         get { return _border; }
         set
         {
            if (_border == value)
               return;
            _border = value;
            Invalidate();
         }
      }

      public override bool CanFocus
      {
         get { return false; }
      }

      /// <summary>
      /// Gets/Sets bottom (secondary) gradient color
      /// </summary>
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

      /// <summary>
      /// Gets/Sets top (primary) gradient color
      /// </summary>
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

      /// <summary>
      /// Gets/Sets minimum value
      /// </summary>
      public long Minimum
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
      public long Maximum
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

      public HorizontalAlignment ProgressAlign
      {
         get { return _align; }
         set
         {
            if (_align == value)
               return;
            _align = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets current value
      /// </summary>
      public long Value
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
            OnProgressChanged(this, _val);
            Invalidate();
         }
      }

      #endregion

      #region GUI

      protected override void OnRender(int x, int y, int w, int h)
      {
         // Draw Border & Background
         Core.Screen.DrawRectangle(Core.SystemColors.BorderColor, 1, Left, Top, Width, Height, 0, 0, _bkg, 0, 0, _bkg, 0, 0, 256);

         // Figure out progress fill
         if (_val == _min) return;
         long rng = _max - _min;
         long val = _val - _min;
         float prc = (val / (float)rng);
         int wid = (int)((Width - 2) * prc);
         if (wid < 1)
            return;

         // Draw progress fill
         Core.Screen.SetClippingRectangle(Left + 1, Top + 1, Width - 2, Height - 2);

         switch (_align)
         {
            case HorizontalAlignment.Left:
               Core.Screen.DrawRectangle(0, 0, Left + 1, Top + 1, wid, Height - 2, 0, 0, _top, Left, Top + 1, _btm, Left, Top + Height - 2, 256);
               break;
            case HorizontalAlignment.Right:
               Core.Screen.DrawRectangle(0, 0, Left + Width - wid - 1, Top + 1, wid, Height - 2, 0, 0, _top, Left, Top + 1, _btm, Left, Top + Height - 2, 256);
               break;
            default:
               Core.Screen.DrawRectangle(0, 0, Left + (Width / 2 - wid / 2), Top + 1, wid, Height - 2, 0, 0, _top, Left, Top + 1, _btm, Left, Top + Height - 2, 256);
               break;
         }

      }

      #endregion

      #region Private Methods

      private void DefaultColors()
      {
         _top = Core.SystemColors.AltSelectionColor;
         _btm = Core.SystemColors.SelectionColor;
         _border = Core.SystemColors.BorderColor;
         _bkg = Core.SystemColors.ContainerBackground;
      }

      #endregion

   }
}
