using System;
using System.IO;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using Microsoft.SPOT.Presentation.Media;
using Skewworks.NETMF.Controls;

using Button = Skewworks.NETMF.Controls.Button;

// ReSharper disable StringIndexOfIsCultureSpecific.1
namespace Skewworks.NETMF.Modal
{
   public class FileDialog
   {
      #region Variables

      private static ManualResetEvent _activeBlock;            // Used to display Modal Forms
      private static bool _isSave;
      private static string _inputResult;
      private static bool _checkExist;

      #endregion

      #region Public Methods

      /// <summary>
      /// Displays Open File dialog (blocking, modal)
      /// </summary>
      /// <returns></returns>
      public static string OpenFile(Font titleFont, Font bodyFont)
      {
         return OpenFile("Open File", titleFont, bodyFont, "", null);
      }

      /// <summary>
      /// Displays Open File dialog (blocking, modal)
      /// </summary>
      /// <param name="title"></param>
      /// <param name="titleFont"></param>
      /// <param name="bodyFont"></param>
      /// <returns></returns>
      public static string OpenFile(string title, Font titleFont, Font bodyFont)
      {
         return OpenFile(title, titleFont, bodyFont, "", null);
      }

      /// <summary>
      /// Displays Open File dialog (blocking, modal)
      /// </summary>
      /// <param name="title"></param>
      /// <param name="bodyFont"></param>
      /// <param name="startDirectory"></param>
      /// <param name="titleFont"></param>
      /// <returns></returns>
      public static string OpenFile(string title, Font titleFont, Font bodyFont, string startDirectory)
      {
         return OpenFile(title, titleFont, bodyFont, startDirectory, null);
      }

      /// <summary>
      /// Displays Open File dialog (blocking, modal)
      /// </summary>
      /// <param name="title"></param>
      /// <param name="bodyFont"></param>
      /// <param name="startDirectory"></param>
      /// <param name="extensions"></param>
      /// <param name="titleFont"></param>
      /// <returns></returns>
      public static string OpenFile(string title, Font titleFont, Font bodyFont, string startDirectory, string[] extensions)
      {
         _isSave = false;
         _checkExist = false;
         CreateDialog(title, titleFont, bodyFont, startDirectory, extensions);
         return _inputResult;
      }

      /// <summary>
      /// Displays Save File dialog (blocking, modal)
      /// </summary>
      /// <param name="bodyFont"></param>
      /// <param name="checkFileExistance"></param>
      /// <param name="titleFont"></param>
      /// <returns></returns>
      public static string SaveFile(Font titleFont, Font bodyFont, bool checkFileExistance)
      {
         return SaveFile("Save File", titleFont, bodyFont, checkFileExistance, "", null);
      }

      /// <summary>
      /// Displays Save File dialog (blocking, modal)
      /// </summary>
      /// <param name="title"></param>
      /// <param name="bodyFont"></param>
      /// <param name="checkFileExistance"></param>
      /// <param name="titleFont"></param>
      /// <returns></returns>
      public static string SaveFile(string title, Font titleFont, Font bodyFont, bool checkFileExistance)
      {
         return SaveFile(title, titleFont, bodyFont, checkFileExistance, "", null);
      }

      /// <summary>
      /// Displays Save File dialog (blocking, modal)
      /// </summary>
      /// <param name="title"></param>
      /// <param name="bodyFont"></param>
      /// <param name="checkFileExistance"></param>
      /// <param name="startDirectory"></param>
      /// <param name="titleFont"></param>
      /// <returns></returns>
      public static string SaveFile(string title, Font titleFont, Font bodyFont, bool checkFileExistance, string startDirectory)
      {
         return SaveFile(title, titleFont, bodyFont, checkFileExistance, startDirectory, null);
      }

      /// <summary>
      /// Displays Save File dialog (blocking, modal)
      /// </summary>
      /// <param name="title"></param>
      /// <param name="bodyFont"></param>
      /// <param name="checkFileExistance"></param>
      /// <param name="startDirectory"></param>
      /// <param name="extensions"></param>
      /// <param name="titleFont"></param>
      /// <returns></returns>
      public static string SaveFile(string title, Font titleFont, Font bodyFont, bool checkFileExistance, string startDirectory, string[] extensions)
      {
         _isSave = true;
         _checkExist = checkFileExistance;
         CreateDialog(title, titleFont, bodyFont, startDirectory, extensions);
         return _inputResult;
      }

      #endregion

      #region Private Methods

      // ReSharper disable once UnusedMethodReturnValue.Local
      // ReSharper disable once UnusedParameter.Local
      //TODO: check why param extensions is not in use
      private static string CreateDialog(string title, Font titleFont, Font bodyFont, string startDirectory, string[] extentions)
      {
         IContainer prv = Core.ActiveContainer;
         lock (Core.Screen)
         {
            var frmModal = new Form("modalAlertForm", Colors.Ghost);
            _inputResult = string.Empty;

            Core.Screen.SetClippingRectangle(0, 0, frmModal.Width, frmModal.Height);
            Core.Screen.DrawRectangle(0, 0, 4, 4, frmModal.Width - 8, frmModal.Height - 8, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 256);
            Core.ShadowRegion(4, 4, frmModal.Width - 8, frmModal.Height - 8);

            Core.Screen.DrawTextInRect(title, 8, 8, frmModal.Width - 16, titleFont.Height, Bitmap.DT_TrimmingCharacterEllipsis, Colors.Charcoal, titleFont);

            var tv1 = new Treeview("tv1", bodyFont, 8, 16 + titleFont.Height, 120, frmModal.Height - 45 - bodyFont.Height - titleFont.Height);

            VolumeInfo[] vi = VolumeInfo.GetVolumes();

            for (int i = 0; i < vi.Length; i++)
            {
               var viNode = new TreeviewNode(vi[i].RootDirectory)
               {
                  Tag = vi[i].RootDirectory
               };

               if (vi[i].IsFormatted)
               {
                  if (Directory.GetDirectories(vi[i].RootDirectory).Length > 0)
                  {
                     var ni = new TreeviewNode(string.Empty)
                     {
                        Tag = null
                     };
                     viNode.AddNode(ni);
                  }
               }

               tv1.AddNode(viNode);
            }
            tv1.NodeExpanded += tv1_NodeExpanded;
            frmModal.AddChild(tv1);

            var fb1 = new Filebox("fb1", null, bodyFont, 132, tv1.Y, frmModal.Width - 141, tv1.Height);
            frmModal.AddChild(fb1);


            Color cR1 = ColorUtility.ColorFromRGB(226, 92, 79);
            Color cR2 = ColorUtility.ColorFromRGB(202, 48, 53);
            var btnCancel = new Button("btnCancel", "Cancel", bodyFont, 8, frmModal.Height - 26 - bodyFont.Height, Colors.White, Colors.White, Colors.DarkRed, Colors.DarkRed)
            {
               NormalColorTop = cR1,
               NormalColorBottom = cR2,
               PressedColorTop = cR2,
               PressedColorBottom = cR1
            };
            btnCancel.Height += 4;
            btnCancel.Tap += (sender, e) => _activeBlock.Set();

            var btnSelect = new Button("btnSel", (_isSave) ? "Save File" : "Open File", bodyFont, 8, frmModal.Height - 26 - bodyFont.Height)
            {
               BorderColor = ColorUtility.ColorFromRGB(13, 86, 124),
               NormalColorTop = Colors.LightBlue,
               NormalColorBottom = ColorUtility.ColorFromRGB(26, 129, 182),
               PressedColorBottom = Colors.LightBlue
            };
            btnSelect.PressedColorTop = btnSelect.NormalColorBottom;
            btnSelect.NormalTextColor = Colors.White;
            btnSelect.PressedTextColor = Colors.White;
            btnSelect.X = frmModal.Width - btnSelect.Width - 8;
            btnSelect.Height += 4;
            btnSelect.Enabled = false;
            frmModal.AddChild(btnSelect);

            frmModal.AddChild(btnCancel);

            var txt1 = new Textbox("txt1", string.Empty, bodyFont, Colors.Charcoal, btnCancel.X + btnCancel.Width + 4, btnCancel.Y, frmModal.Width - 28 - btnCancel.Width - btnSelect.Width, btnSelect.Height, ' ', !_isSave);
            frmModal.AddChild(txt1);

            Core.SilentlyActivate(frmModal);
            frmModal.Render(new rect(4, 12 + titleFont.Height, frmModal.Width - 8, frmModal.Height - 20 - titleFont.Height));
            Core.Screen.Flush();

            tv1.NodeTapped += (node, e) => tv1_NodeTapped(node, fb1);
            fb1.PathChanged += (sender, value) => SetTvPath(value, tv1, fb1);
            fb1.SelectedIndexChanged += (sender, value) => FbIndexChange(fb1, txt1);
            txt1.TextChanged += (sender, value) => btnSelect.Enabled = txt1.Text != string.Empty;
            SetTvPath(startDirectory, tv1, fb1);
            btnSelect.Tap += (sender, e) => ReturnSelection(tv1, txt1);
         }

         ModalBlock();

         Core.ActiveContainer = prv;

         return _inputResult;
      }

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

      private static void ReturnSelection(Treeview tv1, Textbox txt1)
      {
         if (tv1.Nodes != null)
         {
            if (tv1.SelectedNode == null)
               tv1.SelectedNode = tv1.Nodes[0];

            _inputResult = Strings.NormalizeDirectory((string)tv1.SelectedNode.Tag) + txt1.Text;


            if (_checkExist && File.Exists(_inputResult))
            {
               //_prompt = "Are you sure you wish to overwrite '" + _inputResult + "'?";
               new Thread(DoCheck).Start();
               return;
            }
         }
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
            nodes[i] = new TreeviewNode(sDirs[i].Substring(sDirs[i].LastIndexOf('\\') + 1))
            {
               Tag = sDirs[i]
            };
            if (Directory.GetDirectories(sDirs[i]).Length > 0)
            {
               var ni = new TreeviewNode(string.Empty)
               {
                  Tag = null
               };
               nodes[i].AddNode(ni);
            }
         }
         node.Nodes = nodes;

      }

      private static void tv1_NodeTapped(TreeviewNode node, Filebox fb)
      {
         if (node == null)
            return;
         fb.FilePath = (string)node.Tag;
      }

      private static void SetTvPath(string path, Treeview tv, Filebox fb)
      {
         if (tv.Nodes == null || path == null || path == string.Empty)
            return;

         string[] s = path.Split('\\');

         for (int i = 0; i < tv.Nodes.Length; i++)
         {
            if (tv.Nodes[i].Text == "\\" + s[1])
            {
               TreeviewNode n = tv.Nodes[i];
               if (s.Length == 2)
                  tv.SelectedNode = n;
               else
               {
                  n.Expanded = true;
                  for (int j = 2; j < s.Length; j++)
                  {
                     n = GetChildNode(n, path);
                     if (n == null)
                        break;
                     if (j < s.Length - 1)
                     {
                        if (n.Length > 0)
                           n.Expanded = true;
                        else
                        {
                           tv.SelectedNode = n;
                           tv1_NodeTapped(n, fb);
                           return;
                        }
                     }
                  }
                  tv.SelectedNode = n;
               }

               break;
            }
         }

         tv1_NodeTapped(tv.SelectedNode, fb);
      }

      private static void FbIndexChange(Filebox fb, Textbox txt)
      {
         txt.Text = fb.SelectedIsFile ? Path.GetFileName(fb.SelectedValue) : string.Empty;
      }

      private static TreeviewNode GetChildNode(TreeviewNode n, string s)
      {
         for (int i = 0; i < n.Length; i++)
         {
            if (s.IndexOf((string)n.Nodes[i].Tag) == 0)
               return n.Nodes[i];
         }

         return null;
      }

      //private static string _prompt;

      private static void DoCheck()
      {
         PromptResult pr = Prompt.Show("Confirmation", "Are you sure you wish to overwrite '" + _inputResult + "'?", Resources.GetFont(Resources.FontResources.droid12bold),
             Resources.GetFont(Resources.FontResources.droid12), PromptType.YesNoCancel);
         if (pr == PromptResult.Cancel)
            _inputResult = string.Empty;
         else if (pr == PromptResult.No)
            return;

         _activeBlock.Set();
      }

      #endregion
   }
}
// ReSharper restore StringIndexOfIsCultureSpecific.1
