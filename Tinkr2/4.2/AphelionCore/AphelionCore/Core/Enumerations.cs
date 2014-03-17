using System;

namespace Skewworks.NETMF
{

   /// <summary>
   /// Border styles
   /// </summary>
   public enum BorderStyle
   {
      /// <summary>
      /// 3D border
      /// </summary>
      Border3D = 0,

      /// <summary>
      /// Flat border
      /// </summary>
      BorderFlat = 1,

      /// <summary>
      /// No boarder
      /// </summary>
      BorderNone = 2
   }

   /// <summary>
   /// Predefined button id's
   /// </summary>
   public enum ButtonIDs
   {
      /// <summary>
      /// Click (mouse)
      /// </summary>
      Click = 0,

      /// <summary>
      /// Right click (mouse)
      /// </summary>
      ClickRight = 1,

      /// <summary>
      /// Middle button click (mouse)
      /// </summary>
      ClickMiddle = 2,

      /// <summary>
      /// Select button
      /// </summary>
      Select = 3,

      /// <summary>
      /// Back button
      /// </summary>
      Back = 4,

      /// <summary>
      /// Tab button
      /// </summary>
      Tab = 5,

      /// <summary>
      /// Tab back button
      /// </summary>
      TabBack = 6,

      /// <summary>
      /// Home button
      /// </summary>
      Home = 7,

      /// <summary>
      /// Special button
      /// </summary>
      Special = 8,

      /// <summary>
      /// Left button
      /// </summary>
      Left = 20,

      /// <summary>
      /// Up button
      /// </summary>
      Up = 21,

      /// <summary>
      /// Right button
      /// </summary>
      Right = 22,

      /// <summary>
      /// Down button
      /// </summary>
      Down = 23,

      /// <summary>
      /// Menu button
      /// </summary>
      Menu = 24,

      /// <summary>
      /// No button
      /// </summary>
      None = 99,
   }

   /// <summary>
   /// Docking locations
   /// </summary>
   public enum DockLocation
   {
      /// <summary>
      /// Dock on top edge
      /// </summary>
      Top = 0,

      /// <summary>
      /// Dock on right edge
      /// </summary>
      Right = 1,

      /// <summary>
      /// Dock on bottom edge
      /// </summary>
      Bottom = 2,

      /// <summary>
      /// Dock on left edge
      /// </summary>
      Left = 3
   }

   /// <summary>
   /// Position to align object horizontally
   /// </summary>
   public enum HorizontalAlignment
   {
      /// <summary>
      /// Align left
      /// </summary>
      Left = 0,

      /// <summary>
      /// Align centered
      /// </summary>
      Center = 1,

      /// <summary>
      /// Align right
      /// </summary>
      Right = 2,
   }

   /// <summary>
   /// Image types
   /// </summary>
   public enum ImageType
   {
      /// <summary>
      /// Native image (NETMF built in image type)
      /// </summary>
      Native = 0,

      /// <summary>
      /// Skewworks 32 bit image with alpha blending
      /// </summary>
      Image32 = 1,
   }

   /// <summary>
   /// Predefined keyboard layouts
   /// </summary>
   public enum KeyboardLayout
   // ReSharper disable InconsistentNaming
   {
      /// <summary>
      /// QWERTY layout
      /// </summary>
      QWERTY = 0,


      /// <summary>
      /// AZERTY layout
      /// </summary>
      AZERTY = 1,

      /// <summary>
      /// Numeric keyboard layout
      /// </summary>
      Numeric = 99,
   }
   // ReSharper restore InconsistentNaming

   /// <summary>
   /// Enumeration of Orientations
   /// </summary>
   public enum Orientation
   {
      /// <summary>
      /// Horizontal orientation
      /// </summary>
      Horizontal = 0,

      /// <summary>
      /// Vertical orientation
      /// </summary>
      Vertical = 1,
   }

   /// <summary>
   /// Enumeration of Press States
   /// </summary>
   public enum PressState
   {
      /// <summary>
      /// Normal state
      /// </summary>
      Normal = 0,

      /// <summary>
      /// Pressed state
      /// </summary>
      Pressed = 1,
   }

   /// <summary>
   /// Available Scale Modes
   /// </summary>
   public enum ScaleMode
   {
      /// <summary>
      /// Normal scale mode. No scaling is done
      /// </summary>
      Normal = 0,

      /// <summary>
      /// Scale to maximum size. The aspect ratio might be changed
      /// </summary>
      Stretch = 1,

      /// <summary>
      /// Scale to maximum size without changing the aspect ratio.
      /// </summary>
      Scale = 2,

      /// <summary>
      /// Show centered without scaling
      /// </summary>
      Center = 3,

      /// <summary>
      /// Show the unscaled image multiple times until the area is filled.
      /// </summary>
      Tile = 4,
   }

   /// <summary>
   /// Screen Calibration Modes
   /// </summary>
   public enum ScreenCalibration
   {
      /// <summary>
      /// No calibration
      /// </summary>
      None = 0,

      /// <summary>
      /// Gather calibration
      /// </summary>
      Gather = 1,

      /// <summary>
      /// Restore calibration
      /// </summary>
      Restore = 2,
   }

   /// <summary>
   /// Enumeration of Tap States
   /// </summary>
   internal enum TapState
   {
      /// <summary>
      /// No tap
      /// </summary>
      Normal = 0,

      /// <summary>
      /// Waiting for tap hold
      /// </summary>
      TapHoldWaiting = 1,

      /// <summary>
      /// Tap hold is complete
      /// </summary>
      TapHoldComplete = 2,
   }

   /// <summary>
   /// Touch Collection Modes
   /// </summary>
   public enum TouchCollection
   {
      /// <summary>
      /// Manual or multi touch panel. Touch events must be injected into core by user code.
      /// </summary>
      ManualOrMultiTouch = 0,

      /// <summary>
      /// Native touch mode. Touch is handled internally by the the fault touch handler.
      /// </summary>
      NativeSingleTouch = 1,
   }

   /// <summary>
   /// Enumeration of valid touch types (gestures)
   /// </summary>
   public enum TouchType
   {
      /// <summary>
      /// No touch
      /// </summary>
      NoTouch = 0,

      /// <summary>
      /// Touch is down
      /// </summary>
      TouchDown = 1,

      /// <summary>
      /// Touch is up
      /// </summary>
      TouchUp = 2,

      /// <summary>
      /// Touch is moved
      /// </summary>
      TouchMove = 3,

      /// <summary>
      /// No gesture
      /// </summary>
      NoGesture = 10,

      /// <summary>
      /// Begin of gesture
      /// </summary>
      GestureBegin = 11,

      /// <summary>
      /// Gesture ended
      /// </summary>
      GestureEnd = 12,

      /// <summary>
      /// Gesture right
      /// </summary>
      GestureRight = 13,

      /// <summary>
      /// Gesture up right
      /// </summary>
      GestureUpRight = 14,

      /// <summary>
      /// Gesture up
      /// </summary>
      GestureUp = 15,

      /// <summary>
      /// Gesture up left
      /// </summary>
      GestureUpLeft = 16,

      /// <summary>
      /// Gesture left
      /// </summary>
      GestureLeft = 17,

      /// <summary>
      /// Gesture down left
      /// </summary>
      GestureDownLeft = 18,

      /// <summary>
      /// Gesture down
      /// </summary>
      GestureDown = 19,

      /// <summary>
      /// Gesture down right
      /// </summary>
      GestureDownRight = 20,

      /// <summary>
      /// Gesture tap
      /// </summary>
      GestureTap = 21,

      /// <summary>
      /// Gesture double tap
      /// </summary>
      GestureDoubleTap = 22,

      /// <summary>
      /// Gesture zoom in
      /// </summary>
      GestureZoomIn = 72,

      /// <summary>
      /// Gesture Zoom out
      /// </summary>
      GestureZoomOut = 73,

      /// <summary>
      /// Gesture zoom
      /// </summary>
      GestureZoom = 124,

      /// <summary>
      /// Gesture pan
      /// </summary>
      GesturePan = 125,

      /// <summary>
      /// Gesture rotate
      /// </summary>
      GestureRotate = 126,

      /// <summary>
      /// Gesture two finger tap
      /// </summary>
      GestureTwoFingerTap = 127,

      /// <summary>
      /// Gesture rollover
      /// </summary>
      GestureRollover = 128,

      /// <summary>
      /// Gesture user defined
      /// </summary>
      GestureUserDefined = 210,
   }
}
