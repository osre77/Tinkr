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

        private static ManualResetEvent _activeBlock = null;            // Used to display Modal Forms

        #endregion

        #region Events

        public event OnTextEditorClosing TextEditorClosing;
        protected virtual void OnTextEditorClosing(object sender, ref string Text, ref bool Cancel)
        {
            if (TextEditorClosing != null)
                TextEditorClosing(sender, ref Text, ref Cancel);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Displays virtual keyboard (blocking, modal)
        /// </summary>
        /// <param name="Font"></param>
        /// <param name="DefaultValue"></param>
        /// <param name="PasswordChar"></param>
        /// <param name="Multiline"></param>
        /// <param name="Layout"></param>
        /// <param name="Title"></param>
        /// <returns></returns>
        public string Show(Font Font, string DefaultValue = "", char PasswordChar = ' ', KeyboardLayout Layout = KeyboardLayout.QWERTY, string Title = null)
        {
            IContainer prv = Core.ActiveContainer;

            VKB vkb = new VKB(Font, DefaultValue, PasswordChar, Layout, Title);
            vkb.VirtualKeysDone += new OnVirtualKeysDone((object sender) => new Thread(CloseEditor).Start());
            Core.ActiveContainer = vkb;

            ManualResetEvent localBlocker = new ManualResetEvent(false);

            _activeBlock = localBlocker;

            // Wait for Result
            localBlocker.Reset();
            while (!localBlocker.WaitOne(1000, false))
                ;

            vkb._continue = false;
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
            VKB vkb = (VKB)Core.ActiveContainer;
            string txt = vkb.Text;

            OnTextEditorClosing(this, ref txt, ref bCancel);
            vkb.Text = txt;

            if (!bCancel)
                _activeBlock.Set();
        }

        #endregion

    }
}
