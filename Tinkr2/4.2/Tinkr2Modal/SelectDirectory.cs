using System;
using System.IO;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF;
using Skewworks.NETMF.Controls;

using Button = Skewworks.NETMF.Controls.Button;

namespace Skewworks.Tinkr.Modal
{
   [Serializable]
   public class SelectDirectory
   {

      #region Variables

      private static ManualResetEvent _activeBlock;            // Used to display Modal Forms
      private static string _result;

      #endregion

      #region Public Methods

      public static string Show(string title, Font titleFont, Font bodyFont)
      {
         IContainer prv = Core.ActiveContainer;
         var frmModal = new Form("modalAlertForm", Colors.Ghost);
         _result = string.Empty;

         Core.Screen.SetClippingRectangle(0, 0, frmModal.Width, frmModal.Height);
         Core.Screen.DrawRectangle(0, 0, 4, 4, frmModal.Width - 8, frmModal.Height - 8, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 256);
         Core.ShadowRegion(4, 4, frmModal.Width - 8, frmModal.Height - 8);

         Core.Screen.DrawTextInRect(title, 8, 8, frmModal.Width - 16, titleFont.Height, Bitmap.DT_TrimmingCharacterEllipsis, Colors.Charcoal, titleFont);

         var tv1 = new Treeview("tv1", bodyFont, 8, 16 + titleFont.Height, frmModal.Width - 16, frmModal.Height - 41 - bodyFont.Height - titleFont.Height);

         VolumeInfo[] vi = VolumeInfo.GetVolumes();

         for (int i = 0; i < vi.Length; i++)
         {
            var viNode = new TreeviewNode(vi[i].RootDirectory) { Tag = vi[i].RootDirectory };

            if (vi[i].IsFormatted)
            {
               if (Directory.GetDirectories(vi[i].RootDirectory).Length > 0)
               {
                  var ni = new TreeviewNode(string.Empty) { Tag = null };
                  viNode.AddNode(ni);
               }
            }

            tv1.AddNode(viNode);
         }
         tv1.NodeExpanded += tv1_NodeExpanded;
         frmModal.AddChild(tv1);

         var btnSelect = new Button("btnSel", "Select", bodyFont, 8, frmModal.Height - 22 - bodyFont.Height)
         {
            BorderColor = ColorUtility.ColorFromRGB(13, 86, 124),
            NormalColorTop = Colors.LightBlue,
            NormalColorBottom = ColorUtility.ColorFromRGB(26, 129, 182),
            PressedColorBottom = Colors.LightBlue
         };
         btnSelect.PressedColorTop = btnSelect.NormalColorBottom;
         btnSelect.NormalTextColor = Colors.White;
         btnSelect.PressedTextColor = Colors.White;
         btnSelect.Tap += (sender, e) => ReturnSelection(tv1);
         frmModal.AddChild(btnSelect);

         Color cR1 = ColorUtility.ColorFromRGB(226, 92, 79);
         Color cR2 = ColorUtility.ColorFromRGB(202, 48, 53);
         var btnCancel = new Button("btnCancel", "Cancel", bodyFont, 0, btnSelect.Y, Colors.White, Colors.White, Colors.DarkRed, Colors.DarkRed)
         {
            NormalColorTop = cR1,
            NormalColorBottom = cR2,
            PressedColorTop = cR2,
            PressedColorBottom = cR1
         };
         btnCancel.X = frmModal.Width - btnCancel.Width - 8;
         btnCancel.Tap += (sender, e) => _activeBlock.Set();

         frmModal.AddChild(btnCancel);

         Core.SilentlyActivate(frmModal);
         frmModal.Render(new rect(4, 12 + titleFont.Height, frmModal.Width - 8, frmModal.Height - 20 - titleFont.Height));
         Core.Screen.Flush();

         ModalBlock();

         Core.ActiveContainer = prv;

         return _result;
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

      private static void ReturnSelection(Treeview tv1)
      {
         if (tv1.SelectedNode == null)
            _result = string.Empty;
         else
            _result = (string)tv1.SelectedNode.Tag;
         _activeBlock.Set();
      }

      private static void tv1_NodeExpanded(object sender, TreeviewNode node)
      {
         if (node.Nodes[0].Tag != null)
            return;

         string[] sDirs = new StringSorter(Directory.GetDirectories((string)node.Tag)).InsensitiveSort();
         var nodes = new TreeviewNode[sDirs.Length];
         for (int i = 0; i < nodes.Length; i++)
         {
            nodes[i] = new TreeviewNode(sDirs[i].Substring(sDirs[i].LastIndexOf('\\') + 1)) { Tag = sDirs[i] };
            if (Directory.GetDirectories(sDirs[i]).Length > 0)
            {
               var ni = new TreeviewNode(string.Empty) { Tag = null };
               nodes[i].AddNode(ni);
            }
         }
         node.Nodes = nodes;

      }

      #endregion

   }
}
