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

      private static ManualResetEvent _activeBlock;            // Used to display Modal Forms
      private static int _selIndex;

      #endregion

      #region Public Methods

      public static int Show(string[] values, Font font, int defaultSelection = 0, bool zebraStripe = false)
      {
         if (values == null)
         {
            return -1;
         }
         var li = new ListboxItem[values.Length];
         for (int i = 0; i < li.Length; i++)
         {
            li[i] = new ListboxItem(values[i]);
         }
         return InternalShow(li, font, defaultSelection, zebraStripe);
      }

      public static int Show(ListboxItem[] values, Font font, int defaultSelection = 0, bool zebraStripe = false)
      {
         if (values == null)
         {
            return -1;
         }
         var li = new ListboxItem[values.Length];
         for (int i = 0; i < li.Length; i++)
         {
            li[i] = new ListboxItem(values[i].Text);
         }
         return InternalShow(li, font, defaultSelection, zebraStripe);
      }

      private static int InternalShow(ListboxItem[] values, Font font, int defaultSelection = 0, bool zebraStripe = false)
      {
         IContainer prv = Core.ActiveContainer;
         var frmModal = new Form("modalAlertForm");
         var btn1 = new Button("btnModal", "Select", font, 0, 0);
         btn1.X = frmModal.Width - btn1.Width - 4;
         btn1.Y = frmModal.Height - btn1.Height - 4;

         btn1.BorderColor = ColorUtility.ColorFromRGB(13, 86, 124);
         btn1.NormalColorTop = Colors.LightBlue;
         btn1.NormalColorBottom = ColorUtility.ColorFromRGB(26, 129, 182);
         btn1.PressedColorBottom = Colors.LightBlue;
         btn1.PressedColorTop = btn1.NormalColorBottom;
         btn1.NormalTextColor = Colors.White;
         btn1.PressedTextColor = Colors.White;

         var lst1 = new Listbox("lstModal", font, 4, 4, frmModal.Width - 8, frmModal.Height - btn1.Height - 12, values)
         {
            ZebraStripe = zebraStripe
         };
         lst1.SelectedIndexChanged += delegate { btn1.Enabled = lst1.SelectedIndex > -1; };
         lst1.SelectedIndex = defaultSelection;

         btn1.Tap += delegate { Unblock(lst1.SelectedIndex); };

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
         var localBlocker = new ManualResetEvent(false);

         _activeBlock = localBlocker;

         // Wait for Result
         localBlocker.Reset();
         while (!localBlocker.WaitOne(1000, false))
         { }

         // Unblock
         _activeBlock = null;
      }

      private static void Unblock(int index)
      {
         _selIndex = index;
         _activeBlock.Set();
      }

      #endregion

   }
}
