using System;
using System.Threading;

using Microsoft.SPOT;

using Skewworks.NETMF.Controls;

namespace Skewworks.NETMF.Modal
{
   [Serializable]
   public class VirtualKeyboard
   {

      #region Variables

      private static ManualResetEvent _activeBlock;            // Used to display Modal Forms

      #endregion

      #region Events

      public event OnTextEditorClosing TextEditorClosing;
      protected virtual void OnTextEditorClosing(object sender, ref string text, ref bool cancel)
      {
         if (TextEditorClosing != null)
            TextEditorClosing(sender, ref text, ref cancel);
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// Displays virtual keyboard (blocking, modal)
      /// </summary>
      /// <param name="font"></param>
      /// <param name="defaultValue"></param>
      /// <param name="passwordChar"></param>
      /// <param name="layout"></param>
      /// <param name="title"></param>
      /// <returns></returns>
      public string Show(Font font, string defaultValue = "", char passwordChar = ' ', KeyboardLayout layout = KeyboardLayout.QWERTY, string title = null)
      {
         IContainer prv = Core.ActiveContainer;

         var vkb = new Vkb(font, defaultValue, passwordChar, layout, title);
         vkb.VirtualKeysDone += delegate { new Thread(CloseEditor).Start(); };
         Core.ActiveContainer = vkb;

         var localBlocker = new ManualResetEvent(false);

         _activeBlock = localBlocker;

         // Wait for Result
         localBlocker.Reset();
         while (!localBlocker.WaitOne(1000, false))
         { }

         vkb.Continue = false;
         // Unblock
         _activeBlock = null;

         Core.ActiveContainer = prv;
         return vkb.Text;
      }

      #endregion

      #region Private Methods

      private void CloseEditor()
      {
         bool bCancel = false;
         var vkb = (Vkb)Core.ActiveContainer;
         string txt = vkb.Text;

         OnTextEditorClosing(this, ref txt, ref bCancel);
         vkb.Text = txt;

         if (!bCancel)
            _activeBlock.Set();
      }

      #endregion

   }
}
