using System;

namespace Skewworks.NETMF.Controls
{
   /// <summary>
   /// Provides an interface to use as a basis for controls.
   /// </summary>
   public interface IControl
   {
      /// <summary>
      /// Adds or removes callback methods for Tap events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a Tap occurs
      /// </remarks>
      event OnTap Tap;

      /// <summary>
      /// Adds or removes callback methods for GotFocus events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a control gets focus
      /// </remarks>
      event OnGotFocus GotFocus;

      /// <summary>
      /// Adds or removes callback methods for LostFocus events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a control loses focus
      /// </remarks>
      event OnLostFocus LostFocus;

      /// <summary>
      /// Adds or removes callback methods for ButtonPressed events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a button press occurs
      /// </remarks>
      event OnButtonPressed ButtonPressed;

      /// <summary>
      /// Adds or removes callback methods for ButtonReleased events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a button release occurs
      /// </remarks>
      event OnButtonReleased ButtonReleased;

      /// <summary>
      /// Adds or removes callback methods for DoubleTap events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a Double Tap occurs
      /// </remarks>
      event OnDoubleTap DoubleTap;

      /// <summary>
      /// Adds or removes callback methods for KeyboardAltKey events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a keyboard alt key press/release occurs
      /// </remarks>
      event OnKeyboardAltKey KeyboardAltKey;

      /// <summary>
      /// Adds or removes callback methods for KeyboardKey events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a keyboard key press/release occurs
      /// </remarks>
      event OnKeyboardKey KeyboardKey;

      /// <summary>
      /// Adds or removes callback methods for TapHold events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a Tap and Hold occurs
      /// </remarks>
      event OnTapHold TapHold;

      /// <summary>
      /// Adds or removes callback methods for TouchDown events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a touch down occurs
      /// </remarks>
      event OnTouchDown TouchDown;

      /// <summary>
      /// Adds or removes callback methods for TouchGesture events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a touch gesture occurs
      /// </remarks>
      event OnTouchGesture TouchGesture;

      /// <summary>
      /// Adds or removes callback methods for TouchMove events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a touch move occurs
      /// </remarks>
      event OnTouchMove TouchMove;

      /// <summary>
      /// Adds or removes callback methods for TouchUp events
      /// </summary>
      /// <remarks>
      /// Applications can subscribe to this event to be notified when a touch release occurs
      /// </remarks>
      event OnTouchUp TouchUp;


      /// <summary>
      /// Gets the controls ability to receive focus
      /// </summary>
      /// <remarks>
      /// Controls can only receive button and keyboard messages when focused
      /// </remarks>
      bool CanFocus { get; }

      /// <summary>
      /// Gets the disposing state of the control
      /// </summary>
      /// <remarks>
      /// Controls being disposed are not available for use
      /// </remarks>
      bool Disposing { get; }

      /// <summary>
      /// Gets/Sets the enabled state of the control
      /// </summary>
      /// <remarks>
      /// Controls must be enabled to respond to touch, keyboard, or button events
      /// </remarks>
      bool Enabled { get; set; }

      /// <summary>
      /// Gets/Sets the height of the control in pixels
      /// </summary>
      int Height { get; set; }

      /// <summary>
      /// Gets the coordinates of the last point touched
      /// </summary>
      /// <remarks>
      /// If control is currently being touch LastTouch returns current position
      /// </remarks>
      point LastTouch { get; }

      /// <summary>
      /// Gets the absolute X position of the control accounting for parental offsets
      /// </summary>
      int Left { get; }

      /// <summary>
      /// Gets the name of the control
      /// </summary>
      string Name { get; }

      /// <summary>
      /// Gets/Sets the control's container
      /// </summary>
      /// <remarks>
      /// Parent is automatically set when you add a control to a container
      /// </remarks>
      IContainer Parent { get; set; }

      /// <summary>
      /// Gets the exact location of the control in pixels on the screen
      /// </summary>
      /// <remarks>
      /// X and Y are relative to the parent container, which might have an offset as well.
      /// ScreenBounds returns the absolute coordinates of the control.
      /// </remarks>
      rect ScreenBounds { get; }

      /// <summary>
      /// Gets the absolute Y position of the control accounting for parental offsets
      /// </summary>
      int Top { get; }

      /// <summary>
      /// Gets the current touch state of the control
      /// </summary>
      /// <remarks>
      /// Returns true if the control is currently being touched
      /// </remarks>
      bool Touching { get; }

      /// <summary>
      /// Gets/Sets the visibility of the control
      /// </summary>
      /// <remarks>
      /// Controls that are not visible will not be rendered or respond to touch, button or keyboard events
      /// </remarks>
      bool Visible { get; set; }

      /// <summary>
      /// Gets/Sets the width of the control in pixels
      /// </summary>
      int Width { get; set; }

      /// <summary>
      /// Gets/Sets the X position in pixels
      /// </summary>
      /// <remarks>
      /// X is a relative location inside the parent, Left is the exact location on the screen
      /// </remarks>
      int X { get; set; }

      /// <summary>
      /// Gets/Sets the Y position in pixels
      /// </summary>
      /// <remarks>
      /// Y is a relative location inside the parent, Top is the exact location on the screen
      /// </remarks>
      int Y { get; set; }

      /// <summary>
      /// Gets/Sets the suspended state
      /// </summary>
      /// <remarks>
      /// When Suspended is set to false the control will automatically refresh. While true the control will not render or respond to events.
      /// </remarks>
      bool Suspended { get; set; }

      /// <summary>
      /// Activates the control
      /// </summary>
      /// <remarks>
      /// Activate is called by a container when a control becomes focused. Calling Activate by itself will only invoke Invalidate().
      /// </remarks>
      void Activate();

      /// <summary>
      /// Deactivates the control
      /// </summary>
      /// <remarks>
      /// Called by the parent when a control loses focus. If called by itself this will only result in Invalidate() being invoked.
      /// </remarks>
      void Blur();

      /// <summary>
      /// Frees, releases, or resets allocated unmanaged resources.
      /// </summary>
      void Dispose();

      /// <summary>
      /// Checks if a point is inside of the control
      /// </summary>
      /// <param name="point">Point to check if is inside control's ScreenBounds</param>
      /// <returns>Returns true if the point is inside the bounds of the control; else false</returns>
      /// <remarks>
      /// HitTest checks a point based on the control's ScreenBounds (Left, Top, Width, Height). 
      /// The results of this method are used to determine if a control is being affected by touch events or should be rendered during partial screen updates.
      /// </remarks>
      bool HitTest(point point);

      /// <summary>
      /// Safely redraws control
      /// </summary>
      /// <remarks>
      /// Invalidating a control means that every control in the parent container that intersects the controls area is redrawn. This helps keep z-ordering intact.
      /// </remarks>
      void Invalidate();

      /// <summary>
      /// Safely redraws control
      /// </summary>
      /// <param name="area">Defines the area of the control to be redrawn</param>
      /// <remarks>
      /// If rect area is null the entire control will be redrawn.
      /// Invalidating a control means that every control in the parent container that intersects the controls area is redrawn. This helps keep z-ordering intact.
      /// </remarks>
      void Invalidate(rect area);

      /// <summary>
      /// Unsafely renders control
      /// </summary>
      /// <param name="flush">When true the updates will be pushed to the screen, otherwise they will sit in the buffer</param>
      /// <remarks>
      /// Rendering a control will not cause other controls in the same space to be rendered, calling this method can break z-index ordering.
      /// If it is certain no other controls will overlap the rendered control calling Render() can result in faster speeds than Invalidate().
      /// </remarks>
      void Render(bool flush = false);

      /// <summary>
      /// Directly inform control of button events
      /// </summary>
      /// <param name="buttonId">Integer ID corresponding to the affected button</param>
      /// <param name="pressed">True if the button is currently being pressed; false if released</param>
      void SendButtonEvent(int buttonId, bool pressed);

      /// <summary>
      /// Directly inform control of keyboard alt key events
      /// </summary>
      /// <param name="key">Integer value of the Alt key affected</param>
      /// <param name="pressed">True if the key is currently being pressed; false if released</param>
      void SendKeyboardAltKeyEvent(int key, bool pressed);

      /// <summary>
      /// Directly inform control of keyboard key events
      /// </summary>
      /// <param name="key">Integer value of the key affected</param>
      /// <param name="pressed">True if the key is currently being pressed; false if released</param>
      void SendKeyboardKeyEvent(char key, bool pressed);

      /// <summary>
      /// Directly inform control of touch down event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      void SendTouchDown(object sender, point point);

      /// <summary>
      /// Directly inform control of touch up event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      void SendTouchUp(object sender, point point);
      
      /// <summary>
      /// Directly inform control of touch move event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="point">Point on screen touch event is occurring</param>
      void SendTouchMove(object sender, point point);
      
      /// <summary>
      /// Directly inform control of touch gesture event
      /// </summary>
      /// <param name="sender">Object sending the event</param>
      /// <param name="type">Type of gesture being sent</param>
      /// <param name="force">Force associated with gesture (0.0 to 1.0)</param>
      void SendTouchGesture(object sender, TouchType type, float force);

      /// <summary>
      /// Update the X/Y position offset of the control
      /// </summary>
      void UpdateOffsets();

      /// <summary>
      /// Update the X/Y position offset of the control
      /// </summary>
      /// <param name="pt">When supplied this sets an exact offset for the control, regardless of its parent position and offset</param>
      /// <remarks>
      /// If point pt is passed the native offsetting is ignored in favor of the supplied coordinated; this can yeild unexpected results.
      /// </remarks>
      void UpdateOffsets(point pt);
   }
}
