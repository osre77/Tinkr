using System;

using Microsoft.SPOT;

using Skewworks.NETMF.Applications;

namespace Skewworks.NETMF
{

    /// <summary>
    /// Event Delegate for Application Closed
    /// </summary>
    /// <param name="sender">Object raising the event</param>
    /// <param name="ApplicationDetails">Details of the closed application</param>
    [Serializable]
    public delegate void OnApplicationClosed(object sender, ApplicationDetails appDetails);

    /// <summary>
    /// Event Delegate for Application Launched
    /// </summary>
    /// <param name="sender">Object raising the event</param>
    /// <param name="app">Application launched</param>
    [Serializable]
    public delegate void OnApplicationLaunched(object sender, Guid threadId);

    [Serializable]
    public delegate void OnButtonPressed(object sender, int buttonID);

    [Serializable]
    public delegate void OnButtonReleased(object sender, int buttonID);

    [Serializable]
    public delegate void OnCheckChanged(object sender, bool Checked);

    /// <summary>
    /// Event delegate for Double Tap
    /// </summary>
    /// <param name="sender">Object raising the event</param>
    /// <param name="e">Location of double tap</param>
    [Serializable]
    public delegate void OnDoubleTap(object sender, point e);

    /// <summary>
    /// Event delegate for Got Focus
    /// </summary>
    /// <param name="sender">Object raising the event</param>
    [Serializable]
    public delegate void OnGotFocus(object sender);

    [Serializable]
    public delegate void OnKeyboardAltKeyEvent(int key, bool pressed);

    [Serializable]
    public delegate void OnKeyboardKeyEvent(char key, bool pressed);

    [Serializable]
    public delegate void OnKeyboardAltKey(object sender, int key, bool pressed);

    [Serializable]
    public delegate void OnKeyboardKey(object sender, char key, bool pressed);

    /// <summary>
    /// Event delegate for Lost Focus
    /// </summary>
    /// <param name="sender">Object raising the event</param>
    [Serializable]
    public delegate void OnLostFocus(object sender);

    /// <summary>
    /// Event delete for message broadcasts
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    [Serializable]
    public delegate void OnMessageBroadcast(object sender, string message, object[] args = null);

    [Serializable]
    public delegate void OnSelectedIndexChanged(object sender, int Index);

    /// <summary>
    /// Event delegate for Tap
    /// </summary>
    /// <param name="sender">Object raising the event</param>
    /// <param name="e">Location of tap</param>
    [Serializable]
    public delegate void OnTap(object sender, point e);

    /// <summary>
    /// Event delegate for Tap Hold
    /// </summary>
    /// <param name="sender">Object raising the event</param>
    /// <param name="e">Location of tap and hold</param>
    [Serializable]
    public delegate void OnTapHold(object sender, point e);

    [Serializable]
    public delegate void OnTextChanged(object sender, string value);

    /// <summary>
    /// Event delegate for Touch Down
    /// </summary>
    /// <param name="sender">Object raising the event</param>
    /// <param name="e">Location of touch down</param>
    [Serializable]
    public delegate void OnTouchDown(object sender, point e);

    /// <summary>
    /// Event delegate for Touch Event
    /// </summary>
    /// <param name="sender">Object raising the event</param>
    /// <param name="e">Type of touch reported</param>
    [Serializable]
    public delegate void OnTouchEvent(object sender, TouchType e, point pt);

    /// <summary>
    /// Event delegate for Touch Gesture
    /// </summary>
    /// <param name="sender">Object raising the event</param>
    /// <param name="e">Type of touch gesture</param>
    [Serializable]
    public delegate void OnTouchGesture(object sender, TouchType e, float force);

    /// <summary>
    /// Event delegate for Touch Move
    /// </summary>
    /// <param name="sender">Object raising the event</param>
    /// <param name="e">Location of touch move</param>
    [Serializable]
    public delegate void OnTouchMove(object sender, point e);

    /// <summary>
    /// Event delegate for Touch Up
    /// </summary>
    /// <param name="sender">Object raising the event</param>
    /// <param name="e">Location of Touch Up</param>
    [Serializable]
    public delegate void OnTouchUp(object sender, point e);

    [Serializable]
    public delegate void OnValueChanged(Object sender, int value);

}
