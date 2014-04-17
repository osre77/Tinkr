using System;
using System.Threading;

namespace Skewworks.NETMF.Controls
{
   /// <summary>
   /// Base class for creating controls
   /// </summary>
   [Serializable]
   public class Control : MarshalByRefObject, IControl
   {
      #region Variables

      // Name & Tag
      private string _name;
      private object _tag;

      // Size & Location
      private int _x;
      private int _y;
      private int _offsetX;
      private int _offsetY;
      private int _w;
      private int _h;

      // Owner
      private IContainer _parent;

      // States
      private bool _enabled = true;
      private bool _visible = true;
      private bool _suspended;

      // Touch
      private bool _mDown;            // Touching when true
      private point _ptTapHold;       // Location where touch down occurred
      private Thread _thHold;         // Tap & Hold thread
      private long _lStop;            // Stop waiting for hold after this tick
      private TapState _eTapHold;     // Current tap state
      private long _lastTap;          // Tick count of last time occurrence

      // Dispose
      private bool _disposing;

      #endregion

      /// <summary>
      /// Initializes the control
      /// </summary>
      protected Control()
      { }

      /// <summary>
      /// Initializes the control
      /// </summary>
      /// <param name="name">Name of the control</param>
      protected Control(string name)
      {
         _name = name;
      }

      /// <summary>
      /// Initializes the control
      /// </summary>
      /// <param name="name">Name of the control</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      protected Control(string name, int x, int y) :
         this(name)
      {
         _x = x;
         _y = y;
      }

      /// <summary>
      /// Initializes the control
      /// </summary>
      /// <param name="name">Name of the control</param>
      /// <param name="x">X position relative to it's parent</param>
      /// <param name="y">Y position relative to it's parent</param>
      /// <param name="width">Width of the control in pixel</param>
      /// <param name="height">Height of the control in pixel</param>
      protected Control(string name, int x, int y, int width, int height) :
         this(name, x, y)
      {
         _w = width;
         _h = height;
      }

      #region Events

      /// <summary>
      /// Adds or removes callback methods for ButtonPressed events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a button press occurs
      /// </remarks>
      public event OnButtonPressed ButtonPressed;

      /// <summary>
      /// Fires the ButtonPressed event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="buttonId">Integer ID corresponding to the affected button</param>
      protected virtual void OnButtonPressed(object sender, int buttonId)
      {
         if (ButtonPressed != null)
         {
            ButtonPressed(sender, buttonId);
         }
      }

      /// <summary>
      /// Adds or removes callback methods for ButtonReleased events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a button release occurs
      /// </remarks>
      public event OnButtonReleased ButtonReleased;

      /// <summary>
      /// Fires the ButtonReleased event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="buttonId">Integer ID corresponding to the affected button</param>
      protected virtual void OnButtonReleased(object sender, int buttonId)
      {
         if (ButtonReleased != null)
         {
            ButtonReleased(sender, buttonId);
         }
      }

      /// <summary>
      /// Adds or removes callback methods for DoubleTap events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a Double Tap occurs
      /// </remarks>
      public event OnDoubleTap DoubleTap;

      /// <summary>
      /// Fires the DoubleTap event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen event is occurring</param>
      protected virtual void OnDoubleTap(object sender, point point)
      {
         if (DoubleTap != null)
         {
            DoubleTap(sender, point);
         }
      }

      /// <summary>
      /// Adds or removes callback methods for GotFocus events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a control gets focus
      /// </remarks>
      public event OnGotFocus GotFocus;

      /// <summary>
      /// Fires the GotFocus event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      protected virtual void OnGotFocus(object sender)
      {
         if (GotFocus != null)
         {
            GotFocus(sender);
         }
      }

      /// <summary>
      /// Adds or removes callback methods for KeyboardAltKey events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a keyboard alt key press/release occurs
      /// </remarks>
      public event OnKeyboardAltKey KeyboardAltKey;

      /// <summary>
      /// Fires the KeyboardAltKey event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="key">Integer value of the Alt key affected</param>
      /// <param name="pressed">True if the key is currently being pressed; false if released</param>
      protected virtual void OnKeyboardAltKey(object sender, int key, bool pressed)
      {
         if (KeyboardAltKey != null)
         {
            KeyboardAltKey(sender, key, pressed);
         }
      }

      /// <summary>
      /// Adds or removes callback methods for KeyboardKey events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a keyboard key press/release occurs
      /// </remarks>
      public event OnKeyboardKey KeyboardKey;

      /// <summary>
      /// Fires the KeyboardKey event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="key">Integer value of the key affected</param>
      /// <param name="pressed">True if the key is currently being pressed; false if released</param>
      protected virtual void OnKeyboardKey(object sender, char key, bool pressed)
      {
         if (KeyboardKey != null)
         {
            KeyboardKey(sender, key, pressed);
         }
      }

      /// <summary>
      /// Adds or removes callback methods for LostFocus events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a control loses focus
      /// </remarks>
      public event OnLostFocus LostFocus;

      /// <summary>
      /// Fires the LostFocus event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      protected virtual void OnLostFocus(object sender)
      {
         if (LostFocus != null)
         {
            LostFocus(sender);
         }
      }

      /// <summary>
      /// Adds or removes callback methods for Tap events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a Tap occurs
      /// </remarks>
      public event OnTap Tap;

      /// <summary>
      /// Fires the Tap event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen event is occurring</param>
      public virtual void OnTap(object sender, point point)
      {
         if (Tap != null)
         {
            Tap(sender, point);
         }
      }

      /// <summary>
      /// Adds or removes callback methods for TapHold events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a Tap and Hold occurs
      /// </remarks>
      public event OnTapHold TapHold;

      /// <summary>
      /// Fires the TapHold event
      /// </summary>
      /// <param name="sender">>Object sending the event</param>
      /// <param name="point">Point on screen event is occurring</param>
      public virtual void OnTapHold(object sender, point point)
      {
         if (TapHold != null)
         {
            TapHold(sender, point);
         }
      }

      /// <summary>
      /// Adds or removes callback methods for TouchDown events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a touch down occurs
      /// </remarks>
      public event OnTouchDown TouchDown;

      /// <summary>
      /// Fires the TouchDown event
      /// </summary>
      /// <param name="sender">>Object sending the event</param>
      /// <param name="point">Point on screen event is occurring</param>
      public virtual void OnTouchDown(object sender, point point)
      {
         if (TouchDown != null)
         {
            TouchDown(sender, point);
         }
      }

      /// <summary>
      /// Adds or removes callback methods for TouchGesture events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a touch gesture occurs
      /// </remarks>
      public event OnTouchGesture TouchGesture;

      /// <summary>
      /// Fires the TouchGesture event
      /// </summary>
      /// <param name="sender">>Object sending the event</param>
      /// <param name="type">Type of gesture being sent</param>
      /// <param name="force">Force associated with gesture (0.0 to 1.0)</param>
      public virtual void OnTouchGesture(object sender, TouchType type, float force)
      {
         if (TouchGesture != null)
         {
            TouchGesture(sender, type, force);
         }
      }

      /// <summary>
      /// Adds or removes callback methods for TouchMove events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a touch move occurs
      /// </remarks>
      public event OnTouchMove TouchMove;

      /// <summary>
      /// Fires the TouchMove event
      /// </summary>
      /// <param name="sender">>Object sending the event</param>
      /// <param name="point">Point on screen event is occurring</param>
      public virtual void OnTouchMove(object sender, point point)
      {
         if (TouchMove != null)
         {
            TouchMove(sender, point);
         }
      }

      /// <summary>
      /// Adds or removes callback methods for TouchUp events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a touch release occurs
      /// </remarks>
      public event OnTouchUp TouchUp;

      /// <summary>
      /// Fires the TouchUp event
      /// </summary>
      /// <param name="sender">>Object sending the event</param>
      /// <param name="point">Point on screen event is occurring</param>
      public virtual void OnTouchUp(object sender, point point)
      {
         if (TouchUp != null)
         {
            TouchUp(sender, point);
         }
      }

      #endregion

      #region Properties

      /// <summary>
      /// Allows internally direct access to the Suspended flag of the control
      /// </summary>
      protected internal bool InternalSuspended
      {
         get { return _suspended; }
         set { _suspended = value; }
      }

      /// <summary>
      /// Gets the controls ability to receive focus
      /// </summary>
      /// <remarks>
      /// Controls can only receive button and keyboard messages when focused
      /// </remarks>
      public virtual bool CanFocus
      {
         get { return true; }
      }

      /// <summary>
      /// Gets the disposing state of the control
      /// </summary>
      /// <remarks>
      /// Controls being disposed are not available for use
      /// </remarks>
      public bool Disposing
      {
         get { return _disposing; }
      }

      /// <summary>
      /// Gets/Sets the enabled state of the control
      /// </summary>
      /// <remarks>
      /// Controls must be enabled to respond to touch, keyboard, or button events
      /// </remarks>
      public bool Enabled
      {
         get { return _enabled; }
         set
         {
            if (_enabled == value)
               return;
            _enabled = value;
            Render(true);
         }
      }

      /// <summary>
      /// Gets if the control is currently focused
      /// </summary>
      /// <remarks>
      /// Being focused means to be the active child of the parent.
      /// </remarks>
      public bool Focused
      {
         get
         {
            if (_parent == null)
            {
               return false;
            }
            return _parent.ActiveChild == this;
         }
      }

      /// <summary>
      /// Gets/Sets the height of the control in pixels
      /// </summary>
      public virtual int Height
      {
         get { return _h; }
         set
         {
            if (_h == value)
            {
               return;
            }

            // Create Update Region
            var r = new rect(Left, Top, Width, Math.Max(_h, value));

            _h = value;
            Invalidate(r);
            /*if (_parent != null)
            {
               _parent.Render(r, true);
            }*/
         }
      }

      /// <summary>
      /// Gets the coordinates of the last point touched
      /// </summary>
      /// <remarks>
      /// If control is currently being touch LastTouch returns current position
      /// </remarks>
      public point LastTouch
      {
         get { return _ptTapHold; }
         protected set { _ptTapHold = value; }
      }

      /// <summary>
      /// Gets or sets the location of the control
      /// </summary>
      /// <remarks>
      /// Location is equivalent to X and Y.
      /// </remarks>
      public virtual point Location
      {
         get { return new point(_x, _y); }
         set
         {
            bool changed = false;
            var r = new rect(Left, Top, Width, Height);

            if (_x != value.X)
            {
               r.X = Math.Min(_x, value.X);
               r.Width = Math.Abs(_x - value.X) + Width;
               _x = value.X;
               changed = true;
            }

            if (_y != value.Y)
            {
               //rect r = new rect(Left, System.Math.Min(_y, value), Width, System.Math.Abs(_y - value) + Height);
               r.Y = Math.Min(_y, value.Y);
               r.Height = Math.Abs(_y - value.Y) + Height;
               _y = value.Y;
               changed = true;
            }

            if (changed)// && _parent != null)
            {
               Invalidate(r);
               //_parent.Render(r, true);
            }

         }
      }

      /// <summary>
      /// Gets the absolute X position of the control accounting for parental offsets
      /// </summary>
      public int Left
      {
         get { return X + _offsetX; }
      }

      /// <summary>
      /// Gets the name of the control
      /// </summary>
      public string Name
      {
         get { return _name; }
         protected set { _name = value; }
      }

      /// <summary>
      /// Gets/Sets the control's container
      /// </summary>
      /// <remarks>
      /// Parent is automatically set when you add a control to a container
      /// </remarks>
      public virtual IContainer Parent
      {
         get { return _parent; }
         set
         {
            if (_parent == value)
            {
               return;
            }

            if (_parent != null)
            {
               _parent.RemoveChild(this);
            }

            _parent = value;
            UpdateOffsets();
         }
      }

      /// <summary>
      /// Gets the exact location of the control in pixels on the screen
      /// </summary>
      /// <remarks>
      /// X and Y are relative to the parent container, which might have an offset as well.
      /// ScreenBounds returns the absolute coordinates of the control.
      /// </remarks>
      public rect ScreenBounds
      {
         get { return new rect(Left, Top, Width, Height); }
      }

      /// <summary>
      /// Gets/Sets the suspended state
      /// </summary>
      /// <remarks>
      /// When Suspended is set to false the control will automatically refresh. While true the control will not render or respond to events.
      /// </remarks>
      public virtual bool Suspended
      {
         get
         {
            if (_parent != null && _parent.Suspended)
            {
               return true;
            }
            return _suspended;
         }
         set
         {
            if (_suspended == value)
            {
               return;
            }
            _suspended = value;
            if (!_suspended)
            {
               Invalidate();
            }
         }
      }

      /// <summary>
      /// Gets or sets a user defined tag for the control
      /// </summary>
      public object Tag
      {
         get { return _tag; }
         set { _tag = value; }
      }

      /// <summary>
      /// Gets the absolute Y position of the control accounting for parental offsets
      /// </summary>
      public int Top
      {
         get { return Y + _offsetY; }
      }

      /// <summary>
      /// Gets the current touch state of the control
      /// </summary>
      /// <remarks>
      /// Returns true if the control is currently being touched
      /// </remarks>
      public virtual bool Touching
      {
         get { return _mDown; }
      }

      /// <summary>
      /// Gets/Sets the visibility of the control
      /// </summary>
      /// <remarks>
      /// Controls that are not visible will not be rendered or respond to touch, button or keyboard events
      /// </remarks>
      public virtual bool Visible
      {
         get { return _visible; }
         set
         {
            if (_visible == value)
            {
               return;
            }
            _visible = value;
            if (_parent != null)
            {
               _parent.Render(new rect(Left, Top, Width, Height), true);
            }
         }
      }

      /// <summary>
      /// Gets/Sets the width of the control in pixels
      /// </summary>
      public virtual int Width
      {
         get { return _w; }
         set
         {
            if (_w == value)
            {
               return;
            }

            // Create Update Region
            var r = new rect(Left, Top, Math.Max(_w, value), Height);

            _w = value;
            Invalidate(r);
            /*if (_parent != null)
            {
               _parent.Render(r, true);
            }*/
         }
      }

      /// <summary>
      /// Gets/Sets the X position in pixels
      /// </summary>
      /// <remarks>
      /// X is a relative location inside the parent, Left is the exact location on the screen
      /// </remarks>
      public virtual int X
      {
         get { return _x; }
         set
         {
            if (_x == value)
            {
               return;
            }

            // Create update region
            var r = new rect(Math.Min(_x, value), Top, Math.Abs(_x - value) + Width, Height);

            _x = value;
            Invalidate(r);
            /*if (_parent != null)
            {
               _parent.Render(r, true);
            }*/
         }
      }

      /// <summary>
      /// Gets/Sets the Y position in pixels
      /// </summary>
      /// <remarks>
      /// Y is a relative location inside the parent, Top is the exact location on the screen
      /// </remarks>
      public virtual int Y
      {
         get { return _y; }
         set
         {
            if (_y == value)
            {
               return;
            }

            // Create update region
            var r = new rect(Left, Math.Min(_y, value), Width, Math.Abs(_y - value) + Height);

            _y = value;
            Invalidate(r);
            /*if (_parent != null)
            {
               _parent.Render(r, true);
            }*/
         }
      }

      #endregion

      #region Buttons

      /// <summary>
      /// Override this message to handle button pressed events internally.
      /// </summary>
      /// <param name="buttonId">Integer ID corresponding to the affected button</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      protected virtual void ButtonPressedMessage(int buttonId, ref bool handled) 
      { }

      /// <summary>
      /// Override this message to handle button released events internally.
      /// </summary>
      /// <param name="buttonId">Integer ID corresponding to the affected button</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      protected virtual void ButtonReleasedMessage(int buttonId, ref bool handled) 
      { }

      /// <summary>
      /// Directly inform control of button events
      /// </summary>
      /// <param name="buttonId">Integer ID corresponding to the affected button</param>
      /// <param name="pressed">True if the button is currently being pressed; false if released</param>
      public void SendButtonEvent(int buttonId, bool pressed)
      {
         bool handled = false;
         if (pressed)
         {
            ButtonPressedMessage(buttonId, ref handled);
            if (handled)
            {
               return;
            }

            OnButtonPressed(this, buttonId);
            if (buttonId == (int) ButtonIDs.Click || buttonId == (int) ButtonIDs.Select)
            {
               SendTouchDown(this, Core.MousePosition);
            }
         }
         else
         {
            ButtonReleasedMessage(buttonId, ref handled);
            if (handled)
            {
               return;
            }

            if (buttonId == (int) ButtonIDs.Click || buttonId == (int) ButtonIDs.Select)
            {
               SendTouchUp(this, Core.MousePosition);
            }
            OnButtonReleased(this, buttonId);
         }
      }

      #endregion

      #region Keyboard

      /// <summary>
      /// Override this message to handle alt key events internally.
      /// </summary>
      /// <param name="key">Integer value of the Alt key affected</param>
      /// <param name="pressed">True if the key is currently being pressed; false if released</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      protected virtual void KeyboardAltKeyMessage(int key, bool pressed, ref bool handled)
      { }

      /// <summary>
      /// Override this message to handle key events internally.
      /// </summary>
      /// <param name="key">Integer value of the key affected</param>
      /// <param name="pressed">True if the key is currently being pressed; false if released</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      protected virtual void KeyboardKeyMessage(char key, bool pressed, ref bool handled)
      { }

      /// <summary>
      /// Directly inform control of keyboard alt key events
      /// </summary>
      /// <param name="key">Integer value of the Alt key affected</param>
      /// <param name="pressed">True if the key is currently being pressed; false if released</param>
      public void SendKeyboardAltKeyEvent(int key, bool pressed)
      {
         bool handled = false;
         KeyboardAltKeyMessage(key, pressed, ref handled);
         if (!handled)
         {
            OnKeyboardAltKey(this, key, pressed);
         }
      }

      /// <summary>
      /// Directly inform control of keyboard key events
      /// </summary>
      /// <param name="key">Integer value of the key affected</param>
      /// <param name="pressed">True if the key is currently being pressed; false if released</param>
      public void SendKeyboardKeyEvent(char key, bool pressed)
      {
         bool handled = false;
         KeyboardKeyMessage(key, pressed, ref handled);
         if (!handled)
         {
            OnKeyboardKey(this, key, pressed);
         }
      }

      #endregion

      #region Touch

      /// <summary>
      /// Override this message to handle touch events internally.
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      protected virtual void TouchDownMessage(object sender, point point, ref bool handled)
      { }

      /// <summary>
      /// Override this message to handle touch events internally.
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="type">Type of touch gesture</param>
      /// <param name="force">Force associated with gesture (0.0 to 1.0)</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      protected virtual void TouchGestureMessage(object sender, TouchType type, float force, ref bool handled)
      { }

      /// <summary>
      /// Override this message to handle touch events internally.
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      protected virtual void TouchMoveMessage(object sender, point point, ref bool handled)
      { }

      /// <summary>
      /// Override this message to handle touch events internally.
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      /// <param name="handled">true if the event is handled. Set to true if handled.</param>
      protected virtual void TouchUpMessage(object sender, point point, ref bool handled)
      { }

      /// <summary>
      /// Directly inform control of touch down event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      public void SendTouchDown(object sender, point point)
      {
         // Exit if needed
         if (!_enabled || !_visible || _suspended)
         {
            return;
         }

         bool bHandled = false;

         // Allow Override
         TouchDownMessage(sender, point, ref bHandled);

         // Exit if handled
         if (bHandled)
         {
            return;
         }

         // Set down state
         _mDown = true;

         // Begin Tap/Hold
         _ptTapHold = point;
         _eTapHold = TapState.TapHoldWaiting;
         _lStop = DateTime.Now.Ticks + (500 * TimeSpan.TicksPerMillisecond);
         if (_thHold == null || !_thHold.IsAlive)
         {
            _thHold = new Thread(TapHoldWaiter);
            _thHold.Start();
         }

         // Raise Event
         OnTouchDown(sender, point);
      }

      /// <summary>
      /// Directly inform control of touch up event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      public void SendTouchUp(object sender, point point)
      {
         if (!_enabled || !_visible || _suspended)
         {
            _mDown = false;
            return;
         }

         bool bHandled = false;

         // Check Tap Hold
         _eTapHold = TapState.Normal;

         // Allow Override
         TouchUpMessage(sender, point, ref bHandled);

         // Exit if handled
         if (bHandled)
         {
            return;
         }

         // Perform normal tap
         if (_mDown)
         {
            if (new rect(Left, Top, Width, Height).Contains(point))
            {
               if (DateTime.Now.Ticks - _lastTap < (TimeSpan.TicksPerMillisecond * 500))
               {
                  OnDoubleTap(this, new point(point.X - Left, point.Y - Top));
                  _lastTap = 0;
               }
               else
               {
                  OnTap(this, new point(point.X - Left, point.Y - Top));
                  _lastTap = DateTime.Now.Ticks;
               }
            }
            _mDown = false;
            OnTouchUp(this, point);
         }
      }

      /// <summary>
      /// Directly inform control of touch move event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      public void SendTouchMove(object sender, point point)
      {
         if (!_enabled || !_visible || _suspended)
         {
            return;
         }

         bool bHandled = false;

         // Allow Override
         TouchMoveMessage(sender, point, ref bHandled);

         // Exit if handled
         if (bHandled)
         {
            return;
         }

         _eTapHold = TapState.Normal;
         OnTouchMove(this, point);
      }

      /// <summary>
      /// Directly inform control of touch gesture event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="type">Type of gesture being sent</param>
      /// <param name="force">Force associated with gesture (0.0 to 1.0)</param>
      public void SendTouchGesture(object sender, TouchType type, float force)
      {
         if (!_enabled || !_visible || _suspended)
         {
            return;
         }

         bool bHandled = false;

         // Allow Override
         TouchGestureMessage(sender, type, force, ref bHandled);

         // Exit if handled
         if (bHandled)
         {
            return;
         }

         OnTouchGesture(this, type, force);
      }

      /// <summary>
      /// Method for detecting Tap &amp; Hold
      /// </summary>
      private void TapHoldWaiter()
      {
         while (DateTime.Now.Ticks < _lStop)
         {
            Thread.Sleep(10);
            if (_eTapHold == TapState.Normal)
            {
               return;
            }
         }
         if (_eTapHold == TapState.Normal || !_enabled || !_visible || _suspended)
         {
            return;
         }

         //_mDown = false;
         _eTapHold = TapState.Normal;

         OnTapHold(this, _ptTapHold);
      }

      #endregion

      #region Disposing

      /// <summary>
      /// Disposes the control
      /// </summary>
      /// <remarks>
      /// All resources used by this control will be freed
      /// </remarks>
      public void Dispose()
      {
         if (_disposing)
         {
            return;
         }

         _disposing = true;

         // Remove Events
         DoubleTap = null;
         Tap = null;
         TapHold = null;
         TouchDown = null;
         TouchGesture = null;
         TouchMove = null;
         TouchUp = null;
      }

      #endregion

      #region Focus

      /// <summary>
      /// Activates the control
      /// </summary>
      /// <remarks>
      /// Activate is called by a container when a control becomes focused. Calling Activate by itself will only invoke Invalidate().
      /// </remarks>
      public virtual void Activate()
      {
         Invalidate();
      }

      /// <summary>
      /// Deactivates the control
      /// </summary>
      /// <remarks>
      /// Called by the parent when a control loses focus. If called by itself this will only result in Invalidate() being invoked.
      /// </remarks>
      public virtual void Blur()
      {
         _mDown = false;
         Invalidate();
      }

      /// <summary>
      /// Checks if a point is inside of the control
      /// </summary>
      /// <param name="point">Point to check if is inside control's ScreenBounds</param>
      /// <returns>Returns true if the point is inside the bounds of the control; else false</returns>
      /// <remarks>
      /// HitTest checks a point based on the control's ScreenBounds (Left, Top, Width, Height). 
      /// The results of this method are used to determine if a control is being affected by touch events or should be rendered during partial screen updates.
      /// </remarks>
      public virtual bool HitTest(point point)
      {
         return ScreenBounds.Contains(point);
      }

      #endregion

      #region GUI

      /// <summary>
      /// Safely redraws control
      /// </summary>
      /// <remarks>
      /// Invalidating a control means that every control in the parent container that intersects the controls area is redrawn. This helps keep z-ordering intact.
      /// </remarks>
      public void Invalidate()
      {
         if ((_parent == null && Core.ActiveContainer != this) || (_parent != null && _parent.Suspended) || !_visible ||
             _suspended)
         {
            return;
         }

         if (_parent == null)
         {
            Render(true);
         }
         else
         {
            _parent.TopLevelContainer.Render(ScreenBounds, true);
         }
      }

      /// <summary>
      /// Safely redraws control
      /// </summary>
      /// <param name="area">Defines the area of the control to be redrawn</param>
      /// <remarks>
      /// If rect area is null the entire control will be redrawn.
      /// Invalidating a control means that every control in the parent container that intersects the controls area is redrawn. This helps keep z-ordering intact.
      /// </remarks>
      public void Invalidate(rect area)
      {
         if ((_parent == null && Core.ActiveContainer != this) || (_parent != null && _parent.Suspended) || !_visible ||
             _suspended)
         {
            return;
         }

         if (_parent == null)
         {
            Render(true);
         }
         else
         {
            _parent.TopLevelContainer.Render(area, true);
         }
      }

      /// <summary>
      /// Unsafely renders control
      /// </summary>
      /// <param name="flush">When true the updates will be pushed to the screen, otherwise they will sit in the buffer</param>
      /// <remarks>
      /// Rendering a control will not cause other controls in the same space to be rendered, calling this method can break z-index ordering.
      /// If it is certain no other controls will overlap the rendered control calling Render() can result in faster speeds than Invalidate().
      /// </remarks>
      public void Render(bool flush = false)
      {
         // Check if we actually need to render
         if ((_parent == null && Core.ActiveContainer != this) || (_parent != null && _parent.Suspended) || !_visible ||
             _suspended)
         {
            return;
         }

         int cw, ch;

         if (_parent == null)
         {
            cw = Width;
            ch = Height;
         }
         else
         {
            cw = Math.Min(_parent.Width - X, Width);
            ch = Math.Min(_parent.Height - Y, Height);
         }

         if (cw < 1 || ch < 1)
         {
            return;
         }

         // Update clip
         Core.Screen.SetClippingRectangle(Left, Top, cw, ch);

         // Render control
         lock (Core.Screen)
         {
            OnRender(Left, Top, cw, ch);
         }

         // Reset clip
         //Core.Screen.SetClippingRectangle(0, 0, Core.ScreenWidth, Core.ScreenHeight);

         // Flush as needed
         if (flush)
         {
            Core.SafeFlush(Left, Top, cw, ch);
         }
      }

      /// <summary>
      /// Renders the control contents
      /// </summary>
      /// <param name="x">X position in screen coordinates</param>
      /// <param name="y">Y position in screen coordinates</param>
      /// <param name="width">Width in pixel</param>
      /// <param name="height">Height in pixel</param>
      /// <remarks>
      /// Override this method to render the contents of the control
      /// </remarks>
      protected virtual void OnRender(int x, int y, int width, int height)
      { }

      /// <summary>
      /// Update the X/Y position offset of the control
      /// </summary>
      public void UpdateOffsets()
      {
         if (_parent != null)
         {
            _offsetX = _parent.Left;
            _offsetY = _parent.Top;
         }
         else
         {
            _offsetX = 0;
            _offsetY = 0;
         }

         // Handle Containers
         var container = this as Container;
         if (container != null)
         {
            if (container.Children != null)
            {
               for (int i = 0; i < container.Children.Length; i++)
               {
                  container.Children[i].UpdateOffsets();
               }
            }
         }
      }

      /// <summary>
      /// Update the X/Y position offset of the control
      /// </summary>
      /// <param name="pt">When supplied this sets an exact offset for the control, regardless of its parent position and offset</param>
      /// <remarks>
      /// If point pt is passed the native offsetting is ignored in favor of the supplied coordinated; this can yield unexpected results.
      /// </remarks>
      public void UpdateOffsets(point pt)
      {
         _offsetX = pt.X;
         _offsetY = pt.Y;

         // Handle Containers
         var container = this as Container;
         if (container != null)
         {
            if (container.Children != null)
            {
               for (int i = 0; i < container.Children.Length; i++)
               {
                  container.Children[i].UpdateOffsets();
               }
            }
         }
      }

      #endregion
   }
}
