using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Skewworks.NETMF.Controls
{
   /// <summary>
   /// Default top-level container for controls
   /// </summary>
   /// <remarks>
   /// This is the container that should be used by default to represent a Screen/Window/Form.
   /// </remarks>
   /// <example>
   /// Form frm = new Form("frm");
   /// frm.AddChild(new Label("lbl1", "This is a label", Fonts.Droid11, 4, 4)); 
   /// Core.ActiveContainer = frm;
   /// </example>
   [Serializable]
   public class Form : Container
   {
      #region Variables

      // Background
      private Color _bkg;
      private Bitmap _img;
      private ScaleMode _scale;

      // Auto Scroll
      private bool _autoScroll;
      private int _maxX;
      private int _maxY;
#pragma warning disable 649
      private int _minX;
      private int _minY;
#pragma warning restore 649
      private bool _moving;

      // Size
      private readonly int _w;
      private readonly int _h;

      #endregion

      #region Constructors

      /// <summary>
      /// Creates a new Form
      /// </summary>
      /// <param name="name">Name of the Form</param>
      public Form(string name)
      {
         Name = name;
         _bkg = Core.SystemColors.ContainerBackground;
         _w = Core.ScreenWidth;
         _h = Core.ScreenHeight;
      }

      /// <summary>
      /// Creates a new Form
      /// </summary>
      /// <param name="name">Name of the Form</param>
      /// <param name="backColor">Background color</param>
      public Form(string name, Color backColor)
      {
         Name = name;
         _bkg = backColor;
         _w = Core.ScreenWidth;
         _h = Core.ScreenHeight;
      }

      #endregion

      #region Properties

      /// <summary>
      /// Form will automatically display scrollbars as needed when true
      /// </summary>
      public bool AutoScroll
      {
         get { return _autoScroll; }
         set
         {
            if (_autoScroll == value)
               return;
            _autoScroll = value;
            //if (value)
            //    CheckAutoScroll();
            Render(true);
         }
      }

      /// <summary>
      /// Gets/Sets background color
      /// </summary>
      public Color BackColor
      {
         get { return _bkg; }
         set
         {
            if (value == _bkg)
               return;

            _bkg = value;
            Render(true);
         }
      }

      /// <summary>
      /// Gets/Sets background image
      /// </summary>
      public Bitmap BackgroundImage
      {
         get { return _img; }
         set
         {
            if (_img == value)
               return;

            _img = value;
            Render(true);
         }
      }

      /// <summary>
      /// Gets/Sets scale mode for background image
      /// </summary>
      public ScaleMode BackgroundImageScaleMode
      {
         get { return _scale; }
         set
         {
            if (_scale == value)
               return;

            _scale = value;

            if (_img != null)
               Render(true);
         }
      }

      /// <summary>
      /// Forms do not have parents, attempting to set one will throw an exception
      /// </summary>
      public override IContainer Parent
      {
         get { return null; }
         set { throw new InvalidOperationException("Forms cannot have parents"); }
      }

      /// <summary>
      /// Height cannot be set on a Form; attempting to do so will throw an exception
      /// </summary>
      public override int Height
      {
         get { return _h; }
         set { throw new InvalidOperationException("Forms must always be the size of the screen"); }
      }

      public override IContainer TopLevelContainer
      {
         get { return this; }
      }

      /// <summary>
      /// Forms are not allowed to be invisible; attempting to change setting will throw exception
      /// </summary>
      public override bool Visible
      {
         get { return true; }
         set { throw new InvalidOperationException("Forms must always be visible"); }
      }

      /// <summary>
      /// Width cannot be set on a Form; attempting to do so will throw an exception
      /// </summary>
      public override int Width
      {
         get { return _w; }
         set { throw new InvalidOperationException("Forms must always be the size of the screen"); }
      }

      /// <summary>
      /// X must always be 0; attempting to change setting will throw exception
      /// </summary>
      public override int X
      {
         get { return 0; }
         set { throw new InvalidOperationException("Forms must always have X of 0"); }
      }

      /// <summary>
      /// Y must always be 0; attempting to change setting will throw exception
      /// </summary>
      public override int Y
      {
         get { return 0; }
         set { throw new InvalidOperationException("Forms must always have Y of 0"); }
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
            }
            else
            {
               for (var i = 0; i < Children.Length; i++)
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
         base.ButtonPressedMessage(buttonId, ref handled);
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
            }
            else
            {
               for (var i = 0; i < Children.Length; i++)
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
         base.ButtonReleasedMessage(buttonId, ref handled);
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
               for (var i = Children.Length - 1; i >= 0; i--)
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

         //TODO: check if _touch shouldn't be set to true here
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
         if (Touching)
         {
            var bUpdated = false;

            var diffY = 0;
            var diffX = 0;

            // Scroll Y
            if (_maxY > _h)
            {
               diffY = point.Y - LastTouch.Y;

               if (diffY > 0 && _minY < Top)
               {
                  diffY = System.Math.Min(diffY, _minY + Top);
                  bUpdated = true;
               }
               else if (diffY < 0 && _maxY > _h)
               {
                  diffY = System.Math.Min(-diffY, _maxY - _minY - _h);
                  bUpdated = true;
               }
               else
               {
                  diffY = 0;
               }

               _moving = true;
               //handled = true; //TODO: check why not handled
            }

            // Scroll X
            if (_maxX > _w)
            {
               diffX = point.X - LastTouch.X;

               if (diffX > 0 && _minX < Left)
               {
                  diffX = System.Math.Min(diffX, _minX + Left);
                  bUpdated = true;
               }
               else if (diffX < 0 && _maxX > _w)
               {
                  diffX = System.Math.Min(-diffX, _maxX - _minX - _w);
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
               for (var i = 0; i < Children.Length; i++)
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
               for (var i = Children.Length - 1; i >= 0; i--)
               {
                  if (Children[i].Touching || Children[i].HitTest(point))
                  {
                     Children[i].SendTouchMove(this, point);
                  }
               }
            }
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
         if (_moving)
         {
            _moving = false;
            Render(true);
            return;
         }
         //TODO: check if this code shouldn't go to Container
         // Check Controls
         if (Children != null)
         {
            for (var i = Children.Length - 1; i >= 0; i--)
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
               { }
            }
         }
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

         DrawBackground();

         _maxX = _w;
         _maxY = _h;

         //TODO: check if this code shouldn't go to Container
         // Render controls
         if (Children != null)
         {
            for (var i = 0; i < Children.Length; i++)
            {
               if (Children[i] != null)
               {
                  if (Children[i].ScreenBounds.Intersects(area))
                  {
                     Children[i].Render();
                  }

                  //if (Children[i].Y < _minY)
                  //    _minY = Children[i].Y;
                  //else if (Children[i].Y + Children[i].Height > _maxY)
                  //    _maxY = Children[i].Y + Children[i].Height;

                  //if (Children[i].X < _minX)
                  //    _minX = Children[i].X;
                  //else if (Children[i].X + Children[i].Width > _maxX)
                  //    _maxX = Children[i].X + Children[i].Width;
               }
            }
         }
      }

      #endregion

      #region Private Methods

      private void DrawBackground()
      {
         Core.Screen.DrawRectangle(_bkg, 0, 0, 0, _w, _h, 0, 0, _bkg, 0, 0, _bkg, 0, 0, 256);

         if (_img != null)
         {
            switch (_scale)
            {
               case ScaleMode.Center:
                  Core.Screen.DrawImage(Core.ScreenWidth / 2 - _img.Width / 2, Core.ScreenHeight / 2 - _img.Height / 2, _img, 0, 0, _img.Width, _img.Height);
                  break;

               case ScaleMode.Normal:
                  Core.Screen.DrawImage(0, 0, _img, 0, 0, _img.Width, _img.Height);
                  break;

               case ScaleMode.Scale:
                  float multiplier;

                  if (_img.Height > _img.Width)
                  {
                     // Portrait
                     if (_h > _w)
                     {
                        multiplier = _w/(float) _img.Width;
                     }
                     else
                     {
                        multiplier = _h / (float)_img.Height;
                     }
                  }
                  else
                  {
                     // Landscape
                     if (_h > _w)
                     {
                        multiplier = _w/(float) _img.Width;
                     }
                     else
                     {
                        multiplier = _h / (float)_img.Height;
                     }
                  }

                  var dsW = (int)(_img.Width * multiplier);
                  var dsH = (int)(_img.Height * multiplier);
                  var dX = (int)(_w / 2.0f - dsW / 2.0f);
                  var dY = (int)(_h / 2.0f - dsH / 2.0f);

                  Core.Screen.StretchImage(dX, dY, _img, dsW, dsH, 256);
                  break;

               case ScaleMode.Stretch:
                  Core.Screen.StretchImage(0, 0, _img, Core.ScreenWidth, Core.ScreenHeight, 256);
                  break;

               case ScaleMode.Tile:
                  Core.Screen.TileImage(0, 0, _img, Core.ScreenWidth, Core.ScreenHeight, 256);
                  break;
            }

         }
      }
      #endregion
   }
}
