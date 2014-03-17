using System;
using Microsoft.SPOT;

namespace Skewworks.NETMF
{
   /// <summary>
   /// AppDomain specific client for receiving messages across domains
   /// </summary>
   [Serializable]
   public class MessageClient : MarshalByRefObject
   {
      #region Constructor

      internal MessageClient()
      { }

      #endregion

      #region Events

      /// <summary>
      /// This event is raised whenever a message broadcast is received from another AppDomain
      /// </summary>
      public event OnMessageBroadcast MessageBroadcast;

      /// <summary>
      /// Raises the MessageBroadcast event.
      /// </summary>
      /// <param name="sender">Sender object of the broadcast</param>
      /// <param name="message">Message to broadcast</param>
      /// <param name="args">Message arguments</param>
      protected virtual void OnMessageBroadcast(object sender, string message, object[] args = null)
      {
         try
         {
            if (MessageBroadcast != null)
            {
               MessageBroadcast(sender, message, args);
            }
         }
         catch (Exception ex)
         {
            Debug.Print("OnBroadcast Error: {" + AppDomain.CurrentDomain.FriendlyName + "} " + ex.Message);
         }
      }

      #endregion

      #region Internal Methods

      internal void BroadcastMessage(object sender, string message, object[] args)
      {
         OnMessageBroadcast(sender, message, args);
      }

      #endregion
   }
}
