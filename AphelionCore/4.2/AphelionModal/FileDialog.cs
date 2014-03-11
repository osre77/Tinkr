using System;
using System.IO;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.IO;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF;
using Skewworks.NETMF.Controls;

using Button = Skewworks.NETMF.Controls.Button;

namespace Skewworks.NETMF.Modal
{
    public class FileDialog
    {


        #region Variables

        private static ManualResetEvent _activeBlock = null;            // Used to display Modal Forms
        private static bool _IsSave;
        private static string _inputResult;
        private static bool _CheckExist;

        #endregion

        #region Public Methods

        /// <summary>
        /// Displays Open File dialog (blocking, modal)
        /// </summary>
        /// <returns></returns>
        public static string OpenFile(Font TitleFont, Font BodyFont)
        {
            return OpenFile("Open File", TitleFont, BodyFont, "", null);
        }

        /// <summary>
        /// Displays Open File dialog (blocking, modal)
        /// </summary>
        /// <param name="Title"></param>
        /// <returns></returns>
        public static string OpenFile(string Title, Font TitleFont, Font BodyFont)
        {
            return OpenFile(Title, TitleFont, BodyFont, "", null);
        }

        /// <summary>
        /// Displays Open File dialog (blocking, modal)
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="StartDirectory"></param>
        /// <returns></returns>
        public static string OpenFile(string Title, Font TitleFont, Font BodyFont, string StartDirectory)
        {
            return OpenFile(Title, TitleFont, BodyFont, StartDirectory, null);
        }

        /// <summary>
        /// Displays Open File dialog (blocking, modal)
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="StartDirectory"></param>
        /// <param name="Extensions"></param>
        /// <returns></returns>
        public static string OpenFile(string Title, Font TitleFont, Font BodyFont, string StartDirectory, string[] Extensions)
        {
            _IsSave = false;
            _CheckExist = false;
            CreateDialog(Title, TitleFont, BodyFont, StartDirectory, Extensions);
            return _inputResult;
        }

        /// <summary>
        /// Displays Save File dialog (blocking, modal)
        /// </summary>
        /// <param name="CheckFileExistance"></param>
        /// <returns></returns>
        public static string SaveFile(Font TitleFont, Font BodyFont, bool CheckFileExistance)
        {
            return SaveFile("Save File", TitleFont, BodyFont, CheckFileExistance, "", null);
        }

        /// <summary>
        /// Displays Save File dialog (blocking, modal)
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="CheckFileExistance"></param>
        /// <returns></returns>
        public static string SaveFile(string Title, Font TitleFont, Font BodyFont, bool CheckFileExistance)
        {
            return SaveFile(Title, TitleFont, BodyFont, CheckFileExistance, "", null);
        }

        /// <summary>
        /// Displays Save File dialog (blocking, modal)
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="CheckFileExistance"></param>
        /// <param name="StartDirectory"></param>
        /// <returns></returns>
        public static string SaveFile(string Title, Font TitleFont, Font BodyFont, bool CheckFileExistance, string StartDirectory)
        {
            return SaveFile(Title, TitleFont, BodyFont, CheckFileExistance, StartDirectory, null);
        }

        /// <summary>
        /// Displays Save File dialog (blocking, modal)
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="CheckFileExistance"></param>
        /// <param name="StartDirectory"></param>
        /// <param name="Extensions"></param>
        /// <returns></returns>
        public static string SaveFile(string Title, Font TitleFont, Font BodyFont, bool CheckFileExistance, string StartDirectory, string[] Extensions)
        {
            _IsSave = true;
            _CheckExist = CheckFileExistance;
            CreateDialog(Title, TitleFont, BodyFont, StartDirectory, Extensions);
            return _inputResult;
        }

        #endregion

        #region Private Methods

        private static string CreateDialog(string Title, Font TitleFont, Font BodyFont, string StartDirectory, string[] Extentions)
        {
            IContainer prv = Core.ActiveContainer;
            lock (Core.Screen)
            {
                Form frmModal = new Form("modalAlertForm", Colors.Ghost);
                _inputResult = string.Empty;

                Core.Screen.SetClippingRectangle(0, 0, frmModal.Width, frmModal.Height);
                Core.Screen.DrawRectangle(0, 0, 4, 4, frmModal.Width - 8, frmModal.Height - 8, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 256);
                Core.ShadowRegion(4, 4, frmModal.Width - 8, frmModal.Height - 8);

                Core.Screen.DrawTextInRect(Title, 8, 8, frmModal.Width - 16, TitleFont.Height, Bitmap.DT_TrimmingCharacterEllipsis, Colors.Charcoal, TitleFont);

                Treeview tv1 = new Treeview("tv1", BodyFont, 8, 16 + TitleFont.Height, 120, frmModal.Height - 45 - BodyFont.Height - TitleFont.Height);

                VolumeInfo[] vi = VolumeInfo.GetVolumes();

                for (int i = 0; i < vi.Length; i++)
                {
                    TreeviewNode viNode = new TreeviewNode(vi[i].RootDirectory);
                    viNode.Tag = vi[i].RootDirectory;

                    if (vi[i].IsFormatted)
                    {
                        if (Directory.GetDirectories(vi[i].RootDirectory).Length > 0)
                        {
                            TreeviewNode ni = new TreeviewNode(string.Empty);
                            ni.Tag = null;
                            viNode.AddNode(ni);
                        }
                    }

                    tv1.AddNode(viNode);
                }
                tv1.NodeExpanded += tv1_NodeExpanded;
                frmModal.AddChild(tv1);

                Filebox fb1 = new Filebox("fb1", null, BodyFont, 132, tv1.Y, frmModal.Width - 141, tv1.Height);
                frmModal.AddChild(fb1);


                Color cR1 = ColorUtility.ColorFromRGB(226, 92, 79);
                Color cR2 = ColorUtility.ColorFromRGB(202, 48, 53);
                Button btnCancel = new Button("btnCancel", "Cancel", BodyFont, 8, frmModal.Height - 26 - BodyFont.Height, Colors.White, Colors.White, Colors.DarkRed, Colors.DarkRed);
                btnCancel.NormalColorTop = cR1;
                btnCancel.NormalColorBottom = cR2;
                btnCancel.PressedColorTop = cR2;
                btnCancel.PressedColorBottom = cR1;
                btnCancel.Height += 4;
                btnCancel.Tap += (object sender, point e) => _activeBlock.Set();

                Button btnSelect = new Button("btnSel", (_IsSave) ? "Save File" : "Open File", BodyFont, 8, frmModal.Height - 26 - BodyFont.Height);
                btnSelect.BorderColor = ColorUtility.ColorFromRGB(13, 86, 124);
                btnSelect.NormalColorTop = Colors.LightBlue;
                btnSelect.NormalColorBottom = ColorUtility.ColorFromRGB(26, 129, 182);
                btnSelect.PressedColorBottom = Colors.LightBlue;
                btnSelect.PressedColorTop = btnSelect.NormalColorBottom;
                btnSelect.NormalTextColor = Colors.White;
                btnSelect.PressedTextColor = Colors.White;
                btnSelect.X = frmModal.Width - btnSelect.Width - 8;
                btnSelect.Height += 4;
                btnSelect.Enabled = false;
                frmModal.AddChild(btnSelect);

                frmModal.AddChild(btnCancel);

                Textbox txt1 = new Textbox("txt1", string.Empty, BodyFont, Colors.Charcoal, btnCancel.X + btnCancel.Width + 4, btnCancel.Y, frmModal.Width - 28 - btnCancel.Width - btnSelect.Width, btnSelect.Height, ' ', !_IsSave);
                frmModal.AddChild(txt1);

                Core.SilentlyActivate(frmModal);
                frmModal.Render(new rect(4, 12 + TitleFont.Height, frmModal.Width - 8, frmModal.Height - 20 - TitleFont.Height), false);
                Core.Screen.Flush();

                tv1.NodeTapped += (TreeviewNode node, point e) => tv1_NodeTapped(node, fb1);
                fb1.PathChanged += (object sender, string value) => SetTVPath(value, tv1, fb1);
                fb1.SelectedIndexChanged += (object sender, int value) => fbIndexChange(fb1, txt1);
                txt1.TextChanged += (object sender, string value) => btnSelect.Enabled = txt1.Text != string.Empty;
                SetTVPath(StartDirectory, tv1, fb1);
                btnSelect.Tap += (object sender, point e) => ReturnSelection(tv1, txt1);
            }

            ModalBlock();

            Core.ActiveContainer = prv;

            return _inputResult;
        }

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

        private static void ReturnSelection(Treeview tv1, Textbox txt1)
        {
            if (tv1.Nodes != null)
            {
                if (tv1.SelectedNode == null)
                    tv1.SelectedNode = tv1.Nodes[0];

                _inputResult = Strings.NormalizeDirectory((string)tv1.SelectedNode.Tag) + txt1.Text;


                if (_CheckExist && File.Exists(_inputResult))
                {
                    _prompt = "Are you sure you wish to overwrite '" + _inputResult + "'?";
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
            TreeviewNode[] nodes = new TreeviewNode[sDirs.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new TreeviewNode(sDirs[i].Substring(sDirs[i].LastIndexOf('\\') + 1));
                nodes[i].Tag = sDirs[i];
                if (Directory.GetDirectories(sDirs[i]).Length > 0)
                {
                    TreeviewNode ni = new TreeviewNode(string.Empty);
                    ni.Tag = null;
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

        private static void SetTVPath(string Path, Treeview tv, Filebox fb)
        {
            if (tv.Nodes == null || Path == null || Path == string.Empty)
                return;

            string[] s = Path.Split('\\');

            TreeviewNode n;
            for (int i = 0; i < tv.Nodes.Length; i++)
            {
                if (tv.Nodes[i].Text == "\\" + s[1])
                {
                    n = tv.Nodes[i];
                    if (s.Length == 2)
                        tv.SelectedNode = n;
                    else
                    {
                        n.Expanded = true;
                        for (int j = 2; j < s.Length; j++)
                        {
                            n = GetChildNode(n, Path);
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

        private static void fbIndexChange(Filebox fb, Textbox txt)
        {
            if (fb.SelectedIsFile)
                txt.Text = Path.GetFileName(fb.SelectedValue);
            else
                txt.Text = string.Empty;
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

        private static string _prompt;

        private static void DoCheck()
        {
            PromptResult pr = Prompt.Show("Confirmation", "Are you sure you wish to overwrite '" + _inputResult + "'?",Resources.GetFont(Resources.FontResources.droid12bold), 
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
