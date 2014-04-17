using System;

namespace Skewworks.NETMF.Controls
{
   /// <summary>
   /// Delegate for caret changed events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="location">New location of the caret.</param>
   [Serializable]
   public delegate void OnCaretChanged(object sender, int location);

   /// <summary>
   /// Delegate for item selected events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="index">Index of selected item</param>
   [Serializable]
   public delegate void OnItemSelected(object sender, int index);

   /// <summary>
   /// Delegate for node tap events
   /// </summary>
   /// <param name="node">Node that was taped</param>
   /// <param name="point">Point at which the node was taped</param>
   [Serializable]
   public delegate void OnNodeTap(TreeviewNode node, point point);

   /// <summary>
   /// Delegate for node expanded events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="node">Node that was expanded</param>
   [Serializable]
   public delegate void OnNodeExpanded(object sender, TreeviewNode node);

   /// <summary>
   /// Delegate for node collapsed events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="node">Node that has collapsed</param>
   [Serializable]
   public delegate void OnNodeCollapsed(object sender, TreeviewNode node);

   /// <summary>
   /// Delegate for path changed events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="value">New path value</param>
   [Serializable]
   public delegate void OnPathChanged(object sender, string value);

   /// <summary>
   /// Delegate for progress changed events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="value">New progress value</param>
   [Serializable]
   public delegate void OnProgressChanged(Object sender, long value);

   /// <summary>
   /// Delegate for  events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="args">Arguments for the text editor</param>
   [Serializable]
   public delegate void OnTextEditorStart(object sender, ref TextEditorArgs args);

   /// <summary>
   /// Delegate for text editor closing events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="text">Entered text</param>
   /// <param name="cancel">true if editing was canceled; else false</param>
   [Serializable]
   public delegate void OnTextEditorClosing(object sender, ref string text, ref bool cancel);

   /// <summary>
   /// Delegate for virtual keys done events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   [Serializable]
   public delegate void OnVirtualKeysDone(object sender);

   /// <summary>
   /// Delegate for virtual key tap events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="value">Value of the key</param>
   [Serializable]
   public delegate void OnVirtualKeyTap(object sender, string value);

   /// <summary>
   /// Delegate for virtual key backspace events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   [Serializable]
   public delegate void OnVirtualKeyBackspace(object sender);

   /// <summary>
   /// Delegate for virtual key clear events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   [Serializable]
   public delegate void OnVirtualKeyClear(object sender);

   /// <summary>
   /// Delegate for virtual key caret move events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   /// <param name="amount">Amount of the caret move</param>
   [Serializable]
   public delegate void OnVirtualKeyCaretMove(object sender, int amount);

   /// <summary>
   /// Delegate for virtual key delete events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   [Serializable]
   public delegate void OnVirtualKeyDelete(object sender);

   /// <summary>
   /// Delegate for virtual key return events
   /// </summary>
   /// <param name="sender">Sender object of the event</param>
   [Serializable]
   public delegate void OnVirtualKeyReturn(object sender);
}
