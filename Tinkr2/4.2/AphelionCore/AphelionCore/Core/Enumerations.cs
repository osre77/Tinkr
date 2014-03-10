using System;
using Microsoft.SPOT;

namespace Skewworks.NETMF
{

    /// <summary>
    /// Border styles
    /// </summary>
    public enum BorderStyle
    {
        Border3D = 0,
        BorderFlat = 1,
        BorderNone = 2
    }

    public enum ButtonIDs
    {
        Click = 0,
        ClickRight = 1,
        ClickMiddle = 2,
        Select = 3,
        Back = 4,
        Tab = 5,
        TabBack = 6,
        Home = 7,
        Special = 8,
        Left = 20,
        Up = 21,
        Right = 22,
        Down = 23,
        Menu = 24,
        None = 99,
    }

    public enum DockLocation
    {
        Top = 0,
        Right = 1,
        Bottom = 2,
        Left = 3
    }

    /// <summary>
    /// Position to align object horizontaly
    /// </summary>
    public enum HorizontalAlignment
    {
        Left = 0,
        Center = 1,
        Right = 2,
    }

    public enum ImageType
    {
        Native = 0,
        Image32 = 1,
    }

    public enum KeyboardLayout
    {
        QWERTY = 0,
        AZERTY = 1,
        Numeric = 99,
    }

    /// <summary>
    /// Enumeration of Orientations
    /// </summary>
    public enum Orientation
    {
        Horizontal = 0,
        Vertical = 1,
    }

    /// <summary>
    /// Enumeration of Press States
    /// </summary>
    public enum PressState
    {
        Normal = 0,
        Pressed = 1,
    }

    /// <summary>
    /// Available Scale Modes
    /// </summary>
    public enum ScaleMode
    {
        Normal = 0,
        Stretch = 1,
        Scale = 2,
        Center = 3,
        Tile = 4,
    }

    /// <summary>
    /// Screen Calibration Modes
    /// </summary>
    public enum ScreenCalibration
    {
        None = 0,
        Gather = 1,
        Restore = 2,
    }

    /// <summary>
    /// Enumeration of Tap States
    /// </summary>
    internal enum TapState
    {
        Normal = 0,
        TapHoldWaiting = 1,
        TapHoldComplete = 2,
    }

    /// <summary>
    /// Touch Collection Modes
    /// </summary>
    public enum TouchCollection
    {
        ManualOrMultiTouch = 0,
        NativeSingleTouch = 1,
    }

    /// <summary>
    /// Enumeration of valid touch types
    /// </summary>
    public enum TouchType
    {
        NoTouch = 0,
        TouchDown = 1,
        TouchUp = 2,
        TouchMove = 3,
        NoGesture = 10,
        GestureBegin = 11,
        GestureEnd = 12,
        GestureRight = 13,
        GestureUpRight = 14,
        GestureUp = 15,
        GestureUpLeft = 16,
        GestureLeft = 17,
        GestureDownLeft = 18,
        GestureDown = 19,
        GestureDownRight = 20,
        GestureTap = 21,
        GestureDoubleTap = 22,
        GestureZoomIn = 72,
        GestureZoomOut = 73,
        GestureZoom = 124,
        GesturePan = 125,
        GestureRotate = 126,
        GestureTwoFingerTap = 127,
        GestureRollover = 128,
        GestureUserDefined = 210,
    }

}
