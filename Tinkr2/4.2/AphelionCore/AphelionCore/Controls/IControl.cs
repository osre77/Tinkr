using System;

namespace Skewworks.NETMF.Controls
{
    public interface IControl
    {
        event OnTap Tap;
        event OnGotFocus GotFocus;
        event OnLostFocus LostFocus;
        event OnButtonPressed ButtonPressed;
        event OnButtonReleased ButtonReleased;
        event OnDoubleTap DoubleTap;
        event OnKeyboardAltKey KeyboardAltKey;
        event OnKeyboardKey KeyboardKey;
        event OnTapHold TapHold;
        event OnTouchDown TouchDown;
        event OnTouchGesture TouchGesture;
        event OnTouchMove TouchMove;
        event OnTouchUp TouchUp;

        bool CanFocus { get; }
        bool Disposing { get; }
        bool Enabled { get; set; }
        int Height { get; set; }
        point LastTouch { get; }
        int Left { get; }
        string Name { get; }
        IContainer Parent { get; set; }
        rect ScreenBounds { get; }
        int Top { get; }
        bool Touching { get; }
        bool Visible { get; set; }
        int Width { get; set; }
        int X { get; set; }
        int Y { get; set; }
        bool Suspended { get; set; }

        void Activate();
        void Blur();
        void Dispose();
        bool HitTest(point e);
        void Invalidate();
        void Invalidate(rect area);
        void Render(bool flush = false);
        void SendButtonEvent(int buttonId, bool pressed);
        void SendKeyboardAltKeyEvent(int key, bool pressed);
        void SendKeyboardKeyEvent(char key, bool pressed);
        void SendTouchDown(object sender, point e);
        void SendTouchUp(object sender, point e);
        void SendTouchMove(object sender, point e);
        void SendTouchGesture(object sender, TouchType e, float force);
        void UpdateOffsets();
        void UpdateOffsets(point pt);

    }
}
