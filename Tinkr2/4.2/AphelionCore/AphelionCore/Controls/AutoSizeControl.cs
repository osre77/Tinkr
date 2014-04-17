using System;

namespace Skewworks.NETMF.Controls
{
   /// <summary>
   /// AutoSizeControl is a base class for all controls that needs automatic size calculation.
   /// </summary>
   public abstract class AutoSizeControl : Control
   {
      private AutoSizeModes _autoSizeMode;
      private Thickness _padding;

      /// <summary>
      /// Initializes the control
      /// </summary>
      /// <param name="name">Name of the control</param>
      /// <remarks>
      /// The <see cref="Control.Width"/> and <see cref="Control.Height"/> is calculated automatically to fit the font.
      /// <see cref="AutoSizeControl.AutoSizeMode"/> is set to <see cref="AutoSizeModes.WidthAndHeight"/>
      /// </remarks>
      protected AutoSizeControl(string name) :
         base(name)
      {
         _autoSizeMode = AutoSizeModes.WidthAndHeight;
      }

      /// <summary>
      /// Initializes the control
      /// </summary>
      /// <param name="name">Name of the control</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <remarks>
      /// The <see cref="Control.Width"/> and <see cref="Control.Height"/> is calculated automatically to fit the font.
      /// <see cref="AutoSizeControl.AutoSizeMode"/> is set to <see cref="AutoSizeModes.WidthAndHeight"/>
      /// </remarks>
      protected AutoSizeControl(string name, int x, int y) :
         base(name, x, y)
      {
         _autoSizeMode = AutoSizeModes.WidthAndHeight;
      }

      /// <summary>
      /// Initializes the control
      /// </summary>
      /// <param name="name">Name of the control</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="width">Width of the control in pixel</param>
      /// <param name="height">Height of the control in pixel</param>
      /// <remarks>
      /// <see cref="AutoSizeControl.AutoSizeMode"/> is set to <see cref="AutoSizeModes.None"/>
      /// </remarks>
      protected AutoSizeControl(string name, int x, int y, int width, int height) :
         base(name, x, y, width, height)
      {
         _autoSizeMode = AutoSizeModes.None;
      }

      /// <summary>
      /// Gets/Sets the auto size mode
      /// </summary>
      /// <remarks>
      /// Auto size for <see cref="Width"/> and <see cref="Height"/> can be switched on and off separately.
      /// </remarks>
      public AutoSizeModes AutoSizeMode
      {
         get { return _autoSizeMode; }
         set
         {
            if (_autoSizeMode == value)
            {
               return;
            }
            _autoSizeMode = value;
            if (_autoSizeMode != AutoSizeModes.None)
            {
               MeasureControl();
            }
         }
      }

      /// <summary>
      /// Sets the padding without measuring or invalidating the control
      /// </summary>
      /// <param name="padding">New padding</param>
      protected void SetPadding(Thickness padding)
      {
         _padding = padding;
      }

      /// <summary>
      /// Gets/Sets the internal spacing of the control border to it's content
      /// </summary>
      public Thickness Padding
      {
         get { return _padding; }
         set
         {
            if (_padding == value)
            {
               return;
            }
            _padding = value;
            if (!MeasureControl())
            {
               Invalidate();
            }
         }
      }

      /// <summary>
      /// Gets/Sets the width of the control in pixels
      /// </summary>
      /// <remarks>
      /// <see cref="AutoSizeModes.Width"/> is removed from <see cref="AutoSizeMode"/>
      /// </remarks>
      public override int Width
      {
         get { return base.Width; }
         set
         {
            AutoSizeMode = AutoSizeMode & ~AutoSizeModes.Width;
            base.Width = value;
         }
      }

      /// <summary>
      /// Gets/Sets the height of the control in pixels
      /// </summary>
      /// <remarks>
      /// <see cref="AutoSizeModes.Height"/> is removed from <see cref="AutoSizeMode"/>
      /// </remarks>
      public override int Height
      {
         get { return base.Height; }
         set
         {
            AutoSizeMode = AutoSizeMode & ~AutoSizeModes.Height;
            base.Height = value;
         }
      }

      /// <summary>
      /// Measures the size of the control and updates <see cref="Width"/> and <see cref="Height"/>
      /// </summary>
      /// <returns>Returns true if the size of the control was changed and Invalidated; else false</returns>
      protected bool MeasureControl()
      {
         if (AutoSizeMode == AutoSizeModes.None)
         {
            return false;
         }
         var sz = OnMeasureControl(AutoSizeMode);
         if (sz.Width == Width && sz.Height == Height)
         {
            return false;
         }
         var oldSuspended = InternalSuspended;
         InternalSuspended = true;
         var bounds = ScreenBounds;
         try
         {
            if ((AutoSizeMode & AutoSizeModes.Width) != 0)
            {
               base.Width = sz.Width;
            }
            if ((AutoSizeMode & AutoSizeModes.Height) != 0)
            {
               base.Height = sz.Height;
            }
         }
         finally
         {
            InternalSuspended = oldSuspended;
         }
         bounds.Combine(ScreenBounds);
         Invalidate(bounds);
         return true;
      }

      /// <summary>
      /// Is called when ever the size of the control needs to be calculated.
      /// </summary>
      /// <param name="autoSizeMode">Current <see cref="AutoSizeMode"/></param>
      /// <returns>Returns the needed size for the control</returns>
      protected abstract size OnMeasureControl(AutoSizeModes autoSizeMode);
   }
}
