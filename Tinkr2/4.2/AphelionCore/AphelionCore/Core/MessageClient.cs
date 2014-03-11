using System;
using Microsoft.SPOT;

namespace Skewworks.NETMF
{
   [Serializable]
   public class MessageClient : MarshalByRefObject
   {

      #region Constructor

      internal MessageClient()
      {
      }

      #region Events

      public event OnMessageBroadcast MessageBroadcast;
      protected virtual void OnMessageBroadcast(object sender, string message, object[] args = null)
      {
         try
         {
            if (MessageBroadcast != null)
               MessageBroadcast(sender, message, args);
         }
         catch (Exception ex)
         {
            Debug.Print("OnBroadcast Error: {" + AppDomain.CurrentDomain.FriendlyName + "} " + ex.Message);
         }
      }

      #endregion

      #endregion

      #region Internal Methods

      internal void BroadcastMessage(object sender, string message, object[] args)
      {
         OnMessageBroadcast(sender, message, args);
      }

      #endregion

   }
}
