using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF.Controls;

namespace Skewworks.NETMF.Modal
{
    [Serializable]
    public class SelectionDialog : MarshalByRefObject
    {

        #region Variables

        private static ManualResetEvent _activeBlock = null;            // Used to display Modal Forms
        private static int _selIndex;

        #endregion

        #region Public Methods

        public static int Show(string[] values, Font font, int defaultSelection = 0, bool zebraStripe = false)
        {
            if (values == null)
                return -1;
            ListboxItem[] li = new ListboxItem[values.Length];
            for (int i = 0; i < li.Length; i++)
                li[i] = new ListboxItem(values[i]);
            return show(li, font, defaultSelection, zebraStripe);
        }

        public static int Show(ListboxItem[] values, Font font, int defaultSelection = 0, bool zebraStripe = false)
        {
            if (values == null)
                return -1;
            ListboxItem[] li = new ListboxItem[values.Length];
            for (int i = 0; i < li.Length; i++)
                li[i] = new ListboxItem(values[i].Text);
            return show(li, font, defaultSelection, zebraStripe);
        }

        private static int show(ListboxItem[] Values, Font Font, int DefaultSelection = 0, bool ZebraStripe = false)
        {
            IContainer prv = Core.ActiveContainer;
            Form frmModal = new Form("modalAlertForm");
            Button btn1 = new Button("btnModal", "Select", Font, 0, 0);
            btn1.X = frmModal.Width - btn1.Width - 4;
            btn1.Y = frmModal.Height - btn1.Height - 4;

            btn1.BorderColor = ColorUtility.ColorFromRGB(13, 86, 124);
            btn1.NormalColorTop = Colors.LightBlue;
            btn1.NormalColorBottom = ColorUtility.ColorFromRGB(26, 129, 182);
            btn1.PressedColorBottom = Colors.LightBlue;
            btn1.PressedColorTop = btn1.NormalColorBottom;
            btn1.NormalTextColor = Colors.White;
            btn1.PressedTextColor = Colors.White;

            Listbox lst1 = new Listbox("lstModal", Font, 4, 4, frmModal.Width - 8, frmModal.Height - btn1.Height - 12, Values);
            lst1.ZebraStripe = ZebraStripe;
            lst1.SelectedIndexChanged += new OnSelectedIndexChanged((object sender, int value) => btn1.Enabled = lst1.SelectedIndex > -1);
            lst1.SelectedIndex = DefaultSelection;

            btn1.Tap += new OnTap((object sender, point e) => Unblock(lst1.SelectedIndex));

            frmModal.AddChild(lst1);
            frmModal.AddChild(btn1);

            Core.ActiveContainer = frmModal;

            ModalBlock();

            Core.ActiveContainer = prv;
            return _selIndex;
        }

        #endregion

        #region Private Methods

        private static void ModalBlock()
        {
            ManualResetEvent localBlocker = new ManualResetEvent(false);

            _activeBlock = localBlocker;

            // Wait for Result
            localBlocker.Reset();
            while (!localBlocker.WaitOne(1000, false))
                ;

            // Unblock
            _activeBlock = null;
        }

        private static void Unblock(int Index)
        {
            _selIndex = Index;
            _activeBlock.Set();
        }

        #endregion

    }
}
