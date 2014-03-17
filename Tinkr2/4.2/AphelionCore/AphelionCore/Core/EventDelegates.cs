using System;
using Skewworks.NETMF.Applications;

namespace Skewworks.NETMF
{
   /// <summary>
   /// Event delegate for application closed
   /// </summary>
   /// <param name="sender">Object raising the event</param>
   /// <param name="appDetails">Details of the closed application</param>
   [Serializable]
   public delegate void OnApplicationClosed(object sender, ApplicationDetails appDetails);

   /// <summary>
   /// Event delegate for application launched
   /// </summary>
   /// <param name="sender">Object raising the event</param>
   /// <param name="threadId">Application id</param>
   [Serializable]
   public delegate void OnApplicationLaunched(object sender, Guid threadId);

   /// <summary>
   /// Event delegate for button pressed
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="buttonId">Integer ID corresponding to the affected button</param>
   [Serializable]
   public delegate void OnButtonPressed(object sender, int buttonId);

   /// <summary>
   /// Event delegate for button released
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="buttonId">Integer ID corresponding to the affected button</param>
   [Serializable]
   public delegate void OnButtonReleased(object sender, int buttonId);

   /// <summary>
   /// Event delegate for check box state changed
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="isChecked">true if the check box was checked; else false</param>
   [Serializable]
   public delegate void OnCheckChanged(object sender, bool isChecked);

   /// <summary>
   /// Event delegate for double tap
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="point">Point of double tap</param>
   [Serializable]
   public delegate void OnDoubleTap(object sender, point point);

   /// <summary>
   /// Event delegate for got focus
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   [Serializable]
   public delegate void OnGotFocus(object sender);

   /// <summary>
   /// Event delegate for alt key events
   /// </summary>
   /// <param name="key">Key code</param>
   /// <param name="pressed">true if pressed; false if released</param>
   [Serializable]
   public delegate void OnKeyboardAltKeyEvent(int key, bool pressed);

   /// <summary>
   /// Event delegate for keyboard key events
   /// </summary>
   /// <param name="key">Key code</param>
   /// <param name="pressed">true if pressed; false if released</param>
   [Serializable]
   public delegate void OnKeyboardKeyEvent(char key, bool pressed);

   /// <summary>
   /// Event delegate for keyboard alt key events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="key">Key code</param>
   /// <param name="pressed">true if pressed; false if released</param>
   [Serializable]
   public delegate void OnKeyboardAltKey(object sender, int key, bool pressed);

   /// <summary>
   /// Event delegate for keyboard key events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="key">Key code</param>
   /// <param name="pressed">true if pressed; false if released</param>
   [Serializable]
   public delegate void OnKeyboardKey(object sender, char key, bool pressed);

   /// <summary>
   /// Event delegate for lost focus
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   [Serializable]
   public delegate void OnLostFocus(object sender);

   /// <summary>
   /// Event delete for message broadcasts
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="message">Message of the broadcast</param>
   /// <param name="args">Arguments of the message</param>
   [Serializable]
   public delegate void OnMessageBroadcast(object sender, string message, object[] args = null);

   /// <summary>
   /// Event delegate for selected index changed events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="index">New index</param>
   [Serializable]
   public delegate void OnSelectedIndexChanged(object sender, int index);

   /// <summary>
   /// Event delegate for tap
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="point">Point of tap</param>
   [Serializable]
   public delegate void OnTap(object sender, point point);

   /// <summary>
   /// Event delegate for tap hold
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="point">Point of tap and hold</param>
   [Serializable]
   public delegate void OnTapHold(object sender, point point);

   /// <summary>
   /// Event delegate for text changed events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="value">New text value</param>
   [Serializable]
   public delegate void OnTextChanged(object sender, string value);

   /// <summary>
   /// Event delegate for touch down
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="point">Point of touch down</param>
   [Serializable]
   public delegate void OnTouchDown(object sender, point point);

   /// <summary>
   /// Event delegate for touch event
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="type">Type of touch reported</param>
   /// <param name="point">Point of touch event</param>
   [Serializable]
   public delegate void OnTouchEvent(object sender, TouchType type, point point);

   /// <summary>
   /// Event delegate for touch gesture
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="type">Type of touch gesture</param>
   /// <param name="force">Force applied with the gesture (0..1)</param>
   [Serializable]
   public delegate void OnTouchGesture(object sender, TouchType type, float force);

   /// <summary>
   /// Event delegate for touch move
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="point">Point of touch move</param>
   [Serializable]
   public delegate void OnTouchMove(object sender, point point);

   /// <summary>
   /// Event delegate for touch up
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="pint">Point of Touch Up</param>
   [Serializable]
   public delegate void OnTouchUp(object sender, point pint);

   /// <summary>
   /// Event delegate for value changed events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="value">New value</param>
   [Serializable]
   public delegate void OnValueChanged(Object sender, int value);
}
