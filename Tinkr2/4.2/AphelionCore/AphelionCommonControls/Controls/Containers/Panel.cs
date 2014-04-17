using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
   /// <summary>
   /// Nested container
   /// </summary>
   /// <remarks>
   /// Use panels inside of other <see cref="Container"/>s to group controls together.
   /// </remarks>
   [Serializable]
   public class Panel : Container
   {
      #region Variables

      // Background
      private Color _bkg;
      private Bitmap _img;
      private ScaleMode _scale;
      private bool _trans;
      private bool _border;

      // Auto Scroll
      private bool _autoScroll;
      private int _maxX, _maxY;
      private int _minX, _minY;
      private bool _moving;
      private bool _touch;

      #endregion

      #region Constructors

      /// <summary>
      /// Creates a new Panel
      /// </summary>
      /// <param name="name">Name of the panel</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="width">Width in pixel</param>
      /// <param name="height">Height in pixel</param>
      public Panel(string name, int x, int y, int width, int height)
      {
         Name = name;
         _bkg = Core.SystemColors.ContainerBackground;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
      }

      /// <summary>
      /// Creates a new Panel
      /// </summary>
      /// <param name="name">Name of the panel</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="width">Width in pixel</param>
      /// <param name="height">Height in pixel</param>
      /// <param name="backColor">Background color</param>
      public Panel(string name, int x, int y, int width, int height, Color backColor)
      {
         Name = name;
         _bkg = backColor;
         // ReSharper disable DoNotCallOverridableMethodsInConstructor
         X = x;
         Y = y;
         Width = width;
         Height = height;
         // ReSharper restore DoNotCallOverridableMethodsInConstructor
      }

      #endregion

      #region Properties

      /// <summary>
      /// Gets/Sets AutoScroll feature for Panel
      /// </summary>
      /// <remarks>
      /// Panel will automatically display scrollbars as needed when true
      /// </remarks>
      public bool AutoScroll
      {
         get { return _autoScroll; }
         set
         {
            if (_autoScroll == value)
            {
               return;
            }
            _autoScroll = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets Panel's background color
      /// </summary>
      public Color BackColor
      {
         get { return _bkg; }
         set
         {
            if (value == _bkg)
            {
               return;
            }
            _bkg = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets the background image to display on the Panel
      /// </summary>
      /// <seealso cref="BackgroundImageScaleMode"/>
      public Bitmap BackgroundImage
      {
         get { return _img; }
         set
         {
            if (_img == value)
            {
               return;
            }
            _img = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets the Scale Mode to use when rendering background image
      /// </summary>
      /// <seealso cref="BackgroundImage"/>
      public ScaleMode BackgroundImageScaleMode
      {
         get { return _scale; }
         set
         {
            if (_scale == value)
            {
               return;
            }
            _scale = value;

            if (_img != null)
            {
               Invalidate();
            }
         }
      }

      /// <summary>
      /// Gets/Sets if the Panel should have a border
      /// </summary>
      public bool DrawBorder
      {
         get { return _border; }
         set
         {
            if (_border == value)
            {
               return;
            }
            _border = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets/Sets if the background is transparent
      /// </summary>
      public bool TransparentBackground
      {
         get { return _trans; }
         set
         {
            if (_trans == value)
            {
               return;
            }
            _trans = value;
            Invalidate();
         }
      }

      /// <summary>
      /// Gets the current touch state of the control
      /// </summary>
      /// <remarks>
      /// Returns true if the control is currently being touched.
      /// If the Touching property of the parent or of any child is true, then Panel.Touching returns true as well.
      /// </remarks>
      public override bool Touching
      {
         get
         {
            if (base.Touching)
            {
               return true;
            }
            if (Children != null)
            {
               for (int i = 0; i < Children.Length; i++)
               {
                  if (Children[i].Touching)
                  {
                     return true;
                  }
               }
            }
            return false;
         }
      }

      /// <summary>
      /// Gets/Sets the X position in pixels
      /// </summary>
      /// <remarks>
      /// X is a relative location inside the parent, Left is the exact location on the screen.
      /// Updates the offsets of all childes as well.
      /// </remarks>
      public override int X
      {
         get { return base.X; }
         set
         {
            if (base.X == value)
            {
               return;
            }
            base.X = value;
            if (Children != null)
            {
               for (int i = 0; i < Children.Length; i++)
               {
                  Children[i].UpdateOffsets();
               }
            }
         }
      }

      /// <summary>
      /// Gets/Sets the Y position in pixels
      /// </summary>
      /// <remarks>
      /// Y is a relative location inside the parent, Top is the exact location on the screen.
      /// Updates the offsets of all childes as well.
      /// </remarks>
      public override int Y
      {
         get { return base.Y; }
         set
         {
            if (base.Y == value)
               return;
            base.Y = value;
            if (Children != null)
            {
               for (int i = 0; i < Children.Length; i++)
                  Children[i].UpdateOffsets();
            }
         }
      }

      #endregion

      #region Button Methods

      /// <summary>
      /// Override this message to handle button pressed events internally.
      /// </summary>
      /// <param name="buttonId">Integer ID corresponding to the affected button</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Forwards the message to <see cref="Container.ActiveChild"/> or if null, to the child under <see cref="Core.MousePosition"/>
      /// </remarks>
      protected override void ButtonPressedMessage(int buttonId, ref bool handled)
      {
         //TODO: check if this code shouldn't go to Container
         if (Children != null)
         {
            if (ActiveChild != null)
            {
               ActiveChild.SendButtonEvent(buttonId, true);
               //TODO: check if handled should be set true
            }
            else
            {
               for (int i = 0; i < Children.Length; i++)
               {
                  if (Children[i].ScreenBounds.Contains(Core.MousePosition))
                  {
                     handled = true;
                     Children[i].SendButtonEvent(buttonId, true);
                     break;
                  }
               }
            }
         }
      }

      /// <summary>
      /// Override this message to handle button released events internally.
      /// </summary>
      /// <param name="buttonId">Integer ID corresponding to the affected button</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Forwards the message to <see cref="Container.ActiveChild"/> or if null, to the child under <see cref="Core.MousePosition"/>
      /// </remarks>
      protected override void ButtonReleasedMessage(int buttonId, ref bool handled)
      {
         //TODO: check if this code shouldn't go to Container
         if (Children != null)
         {
            if (ActiveChild != null)
            {
               ActiveChild.SendButtonEvent(buttonId, false);
               //TODO: check if handled should be set true
            }
            else
            {
               for (int i = 0; i < Children.Length; i++)
               {
                  if (Children[i].ScreenBounds.Contains(Core.MousePosition))
                  {
                     handled = true;
                     Children[i].SendButtonEvent(buttonId, false);
                     break;
                  }
               }
            }
         }
      }

      #endregion

      #region Keyboard Methods

      /// <summary>
      /// Override this message to handle alt key events internally.
      /// </summary>
      /// <param name="key">Integer value of the Alt key affected</param>
      /// <param name="pressed">True if the key is currently being pressed; false if released</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Forwards the message to <see cref="Container.ActiveChild"/>
      /// </remarks>
      protected override void KeyboardAltKeyMessage(int key, bool pressed, ref bool handled)
      {
         //TODO: check if this code shouldn't go to Container
         if (ActiveChild != null)
         {
            handled = true;
            ActiveChild.SendKeyboardAltKeyEvent(key, pressed);
         }
         base.KeyboardAltKeyMessage(key, pressed, ref handled);
      }

      /// <summary>
      /// Override this message to handle key events internally.
      /// </summary>
      /// <param name="key">Integer value of the key affected</param>
      /// <param name="pressed">True if the key is currently being pressed; false if released</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Forwards the message to <see cref="Container.ActiveChild"/>
      /// </remarks>
      protected override void KeyboardKeyMessage(char key, bool pressed, ref bool handled)
      {
         //TODO: check if this code shouldn't go to Container
         if (ActiveChild != null)
         {
            handled = true;
            ActiveChild.SendKeyboardKeyEvent(key, pressed);
         }
         base.KeyboardKeyMessage(key, pressed, ref handled);
      }

      #endregion

      #region Touch Methods

      /// <summary>
      /// Override this message to handle touch events internally.
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Forwards the message to the top most child under the point.
      /// The hit child is made the active child.
      /// </remarks>
      protected override void TouchDownMessage(object sender, point point, ref bool handled)
      {
         //TODO: check if this code shouldn't go to Container
         // Check Controls
         if (Children != null)
         {
            lock (Children)
            {
               for (int i = Children.Length - 1; i >= 0; i--)
               {
                  if (Children[i].Visible && Children[i].HitTest(point))
                  {
                     ActiveChild = Children[i];
                     Children[i].SendTouchDown(this, point);
                     handled = true;
                     return;
                  }
               }
            }
         }

         if (ActiveChild != null)
         {
            ActiveChild.Blur();
            Render(ActiveChild.ScreenBounds, true);
            ActiveChild = null;
         }

         _touch = true;
      }

      /// <summary>
      /// Override this message to handle touch events internally.
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="type">Type of touch gesture</param>
      /// <param name="force">Force associated with gesture (0.0 to 1.0)</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Forwards the message to <see cref="Container.ActiveChild"/>
      /// </remarks>
      protected override void TouchGestureMessage(object sender, TouchType type, float force, ref bool handled)
      {
         //TODO: check if this code shouldn't go to Container
         if (ActiveChild != null)
         {
            handled = true;
            ActiveChild.SendTouchGesture(sender, type, force);
         }
      }

      /// <summary>
      /// Override this message to handle touch events internally.
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Moves the form if touch down was on the form.
      /// If no then forwards the message to <see cref="Container.ActiveChild"/> or if null, to the child under point
      /// </remarks>
      protected override void TouchMoveMessage(object sender, point point, ref bool handled)
      {
         try
         {
            if (_touch)
            {
               bool bUpdated = false;

               int diffY = 0;
               int diffX = 0;

               // Scroll Y
               if (_maxY > Height)
               {
                  diffY = point.Y - LastTouch.Y;

                  if (diffY > 0 && _minY < Top)
                  {
                     diffY = System.Math.Min(diffY, _minY + Top);
                     bUpdated = true;
                  }
                  else if (diffY < 0 && _maxY > Height)
                  {
                     diffY = System.Math.Min(-diffY, _maxY - _minY - Height);
                     bUpdated = true;
                  }
                  else
                  {
                     diffY = 0;
                  }

                  _moving = true;
               }

               // Scroll X
               if (_maxX > Width)
               {
                  diffX = point.X - LastTouch.X;

                  if (diffX > 0 && _minX < Left)
                  {
                     diffX = System.Math.Min(diffX, _minX + Left);
                     bUpdated = true;
                  }
                  else if (diffX < 0 && _maxX > Width)
                  {
                     diffX = System.Math.Min(-diffX, _maxX - _minX - Width);
                     bUpdated = true;
                  }
                  else
                  {
                     diffX = 0;
                  }

                  _moving = true;
                  handled = true;
               }

               LastTouch = point;
               if (bUpdated)
               {
                  var ptOff = new point(diffX, diffY);
                  for (int i = 0; i < Children.Length; i++)
                  {
                     Children[i].UpdateOffsets(ptOff);
                  }
                  Render(true);
                  handled = true;
               }
               else if (_moving)
               {
                  Render(true);
               }
            }
            else
            {
               //TODO: check if this code shouldn't go to Container (or may be even the whole method?)
               // Check Controls
               if (ActiveChild != null && ActiveChild.Touching)
               {
                  ActiveChild.SendTouchMove(this, point);
                  handled = true;
                  return;
               }
               if (Children != null)
               {
                  for (int i = Children.Length - 1; i >= 0; i--)
                  {
                     if (Children[i].Touching || Children[i].HitTest(point))
                     {
                        Children[i].SendTouchMove(this, point);
                     }
                  }
               }
            }
         }
         finally
         {
            base.TouchMoveMessage(sender, point, ref handled);
         }
      }

      /// <summary>
      /// Override this message to handle touch events internally.
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      /// <remarks>
      /// Finishes the from moving if touch down was on the form.
      /// If not then it forwards the message to <see cref="Container.ActiveChild"/> or if null, to the child under point
      /// </remarks>
      protected override void TouchUpMessage(object sender, point point, ref bool handled)
      {
         _touch = false;

         if (_moving)
         {
            _moving = false;
            Render(true);
         }
         else
         {
            //TODO: check if this code shouldn't go to Container
            // Check Controls
            if (Children != null)
            {
               for (int i = Children.Length - 1; i >= 0; i--)
               {
                  try
                  {
                     if (Children[i].HitTest(point) && !handled)
                     {
                        handled = true;
                        Children[i].SendTouchUp(this, point);
                     }
                     else if (Children[i].Touching)
                     {
                        Children[i].SendTouchUp(this, point);
                     }
                  }
                  // ReSharper disable once EmptyGeneralCatchClause
                  catch // This can happen if the user clears the Form during a tap
                  {}
               }
            }
         }
         base.TouchUpMessage(sender, point, ref handled);
      }

      #endregion

      #region GUI

      /// <summary>
      /// Renders the control contents
      /// </summary>
      /// <param name="x">X position in screen coordinates</param>
      /// <param name="y">Y position in screen coordinates</param>
      /// <param name="width">Width in pixel</param>
      /// <param name="height">Height in pixel</param>
      /// <remarks>
      /// Renders the form and all its childes.
      /// </remarks>
      protected override void OnRender(int x, int y, int width, int height)
      {
         var area = new rect(x, y, width, height);

         if (!_trans)
         {
            DrawBackground();
         }

         _maxX = Width;
         _maxY = Height;

         //TODO: check if this code shouldn't go to Container

         // Render controls
         if (Children != null)
         {
            for (int i = 0; i < Children.Length; i++)
            {
               if (Children[i] != null)
               {
                  if (Children[i].ScreenBounds.Intersects(area))
                  {
                     Children[i].Render();
                  }

                  if (Children[i].Y < _minY)
                  {
                     _minY = Children[i].Y;
                  }
                  else if (Children[i].Y + Children[i].Height > _maxY)
                  {
                     _maxY = Children[i].Y + Children[i].Height;
                  }

                  if (Children[i].X < _minX)
                  {
                     _minX = Children[i].X;
                  }
                  else if (Children[i].X + Children[i].Width > _maxX)
                  {
                     _maxX = Children[i].X + Children[i].Width;
                  }

               }
            }
         }

         if (_border)
         {
            Core.Screen.DrawRectangle(Core.SystemColors.BorderColor, 1, Left, Top, Width, Height, 0, 0, 0, 0, 0, 0, 0, 0, 256);
         }

         base.OnRender(x, y, width, height);
      }

      #endregion

      #region Private Methods

      private void DrawBackground()
      {
         Core.Screen.DrawRectangle(_bkg, 0, Left, Top, Width, Height, 0, 0, _bkg, 0, 0, _bkg, 0, 0, 256);

         if (_img != null)
         {
            switch (_scale)
            {
               case ScaleMode.Center:
                  Core.Screen.DrawImage(Left + (Core.ScreenWidth / 2 - _img.Width / 2), Top + (Core.ScreenHeight / 2 - _img.Height / 2), _img, 0, 0, _img.Width, _img.Height);
                  break;

               case ScaleMode.Normal:
                  Core.Screen.DrawImage(Left, Top, _img, 0, 0, _img.Width, _img.Height);
                  break;

               case ScaleMode.Scale:
                  float multiplier;

                  if (_img.Height > _img.Width)
                  {
                     // Portrait
                     if (Height > Width)
                     {
                        multiplier = Width/(float) _img.Width;
                     }
                     else
                     {
                        multiplier = Height / (float)_img.Height;
                     }
                  }
                  else
                  {
                     // Landscape
                     if (Height > Width)
                     {
                        multiplier = Width/(float) _img.Width;
                     }
                     else
                     {
                        multiplier = Height / (float)_img.Height;
                     }
                  }

                  int dsW = (int)(_img.Width * multiplier);
                  int dsH = (int)(_img.Height * multiplier);
                  int dX = (int)(Width / 2.0f - dsW / 2.0f);
                  int dY = (int)(Height / 2.0f - dsH / 2.0f);

                  Core.Screen.StretchImage(Left + dX, Top + dY, _img, dsW, dsH, 256);
                  break;

               case ScaleMode.Stretch:
                  Core.Screen.StretchImage(Left, Top, _img, Core.ScreenWidth, Core.ScreenHeight, 256);
                  break;

               case ScaleMode.Tile:
                  Core.Screen.TileImage(Left, Top, _img, Core.ScreenWidth, Core.ScreenHeight, 256);
                  break;
            }
         }
      }
      #endregion
   }
}
