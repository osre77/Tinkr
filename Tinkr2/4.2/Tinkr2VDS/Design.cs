using System;
using System.IO;
using System.Text;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF;
using Skewworks.NETMF.Controls;
using Skewworks.NETMF.Resources;

using Skewworks.Tinkr;
using Skewworks.Tinkr.Controls;

namespace Skewworks.VDS.Tinkr
{
    public class Design
    {

        #region Public Methods

        public static Form LoadForm(string filename)
        {
            FileStream fs = null;
            byte[] b;

            try
            {
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                b = new byte[4];

                // Read header
                fs.Read(b, 0, b.Length);
                if (new string(Encoding.UTF8.GetChars(b)) != "TnkR" && new string(Encoding.UTF8.GetChars(b)) != "CLiX")
                    throw new Exception("Invalid header");

                // Read Version
                fs.Read(b, 0, b.Length);
                if (b[0] != 2 || b[1] != 2)
                    throw new Exception("Invalid version");

                // Load Form
                return _LoadForm((Stream)fs);
            }
            catch (Exception)
            {
                if (fs != null)
                    fs.Dispose();
            }

            return null;
        }

        public static Form LoadForm(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            byte[] b;
            b = new byte[4];

            data = null;

            // Read header
            ms.Read(b, 0, b.Length);
            if (new string(Encoding.UTF8.GetChars(b)) != "TnkR" && new string(Encoding.UTF8.GetChars(b)) != "CLiX")
                throw new Exception("Invalid header");

            // Read Version
            ms.Read(b, 0, b.Length);
            if (b[0] != 2 || b[1] != 2)
                throw new Exception("Invalid version");

            // Load Form
            return _LoadForm((Stream)ms);
        }

        #endregion

        #region Load Methods

        static void LoadChildren(Stream fs, IContainer parent, int offsetX = 0, int offsetY = 0)
        {
            IControl ctrl = null;

            while (true)
            {
                int id = fs.ReadByte();
                switch (id)
                {
                    case 0:     // Button
                        ctrl = (LoadButton(fs));
                        break;
                    case 1:     // Checkbox
                        ctrl = (LoadCheckbox(fs));
                        break;
                    case 2:     // Combobox
                        ctrl = (LoadCombobox(fs));
                        break;
                    case 3:     // Filebox
                        ctrl = (LoadFilebox(fs));
                        break;
                    case 5:     // Label
                        ctrl = (LoadLabel(fs));
                        break;
                    case 6:     // Listbox
                        ctrl = (LoadListbox(fs));
                        break;
                    case 7:     // ListboxItem
                        break;
                    case 8:     // MenuStrip
                        ctrl = (LoadMenuStrip(fs));
                        break;
                    case 9:     // MenuItem
                        ctrl = (LoadMenuItem(fs));
                        break;
                    case 10:    // NumericUpDown
                        ctrl = (LoadNumericUpDown(fs));
                        break;
                    case 11:    // Panel
                        ctrl = (LoadPanel(fs));
                        break;
                    case 12:    // Picturebox
                        ctrl = (LoadPicturebox(fs));
                        break;
                    case 13:    // Progressbar
                        ctrl = (LoadProgressbar(fs));
                        break;
                    case 14:    // RadioButton
                        ctrl = (LoadRadioButton(fs));
                        break;
                    case 15:    // RichTextLabel
                        ctrl = (LoadRichTextLabel(fs));
                        break;
                    case 16:    // Scrollbar
                        ctrl = (LoadScrollbar(fs));
                        break;
                    case 17:    // Slider
                        ctrl = (LoadSlider(fs));
                        break;
                    case 18:    // Tab
                        break;
                    case 19:    // TabDialog
                        ctrl = (LoadTabDialog(fs));
                        break;
                    case 20:    // Textbox
                        ctrl = (LoadTextbox(fs));
                        break;
                    case 21:    // Treeview
                        ctrl = (LoadTreeview(fs));
                        break;
                    case 22:    // TreviewNode
                        break;
                    case 23:    // Appbar
                        ctrl = (LoadAppbar(fs));
                        break;
                    case 26:    // GraphicLock
                        ctrl = (LoadGraphicLock(fs));
                        break;
                    case 27:    // LineGraph
                        ctrl = (LoadLineGraph(fs));
                        break;
                    case 29:    // Listview
                        ctrl = (LoadListView(fs));
                        break;
                    case 32:    // SlidePanelDialog
                        ctrl = (LoadSlidePanelDialog(fs));
                        break;
                    case 34:    // TextArea
                        ctrl = (LoadTextArea(fs));
                        break;
                    case 35:    // Window
                        ctrl = (LoadWindow(fs));
                        break;
                    case 255:   // End of Container
                        return;
                    default:    // Unknown/Invalid
                        throw new Exception("Control Ident failure (" + id + ")");
                }

                ctrl.X -= offsetX;
                ctrl.Y -= offsetY;
                parent.AddChild(ctrl);
            }
        }

        static Appbar LoadAppbar(Stream fs)
        {
            Appbar obj = new Appbar(ReadString(fs), ReadInt(fs), ReadFont(fs), ReadFont(fs), (Skewworks.Tinkr.DockLocation)fs.ReadByte(), ReadBool(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.BackColor = ReadColor(fs);
            obj.ForeColor = ReadColor(fs);
            obj.TimeFormat = (TimeFormat)fs.ReadByte();

            int i;
            int c = ReadInt(fs);

            Debug.GC(true);
            for (i = 0; i < c; i++)
            {
                obj.AddIcon(new AppbarIcon("icon" + i, ReadImage(fs)));
                Debug.GC(true);
            }

            c = ReadInt(fs); 
            for (i = 0; i < c; i++)
                obj.AddMenuItem(new AppbarMenuItem("mnu" + i, ReadString(fs)));

            return obj;
        }

        static Button LoadButton(Stream fs)
        {
            Button btn = new Button(ReadString(fs), ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadBool(fs));
            btn.Enabled = ReadBool(fs);
            btn.Visible = ReadBool(fs);

            btn.BorderColor = ReadColor(fs);
            btn.BorderColorPressed = ReadColor(fs);
            btn.NormalColorBottom = ReadColor(fs);
            btn.NormalColorTop = ReadColor(fs);
            btn.NormalTextColor = ReadColor(fs);
            btn.PressedColorBottom = ReadColor(fs);
            btn.PressedColorTop = ReadColor(fs);
            btn.PressedTextColor = ReadColor(fs);

            btn.BackgroundImageScaleMode = ReadScaleMode(fs);
            btn.BackgroundImage = ReadImage(fs);

            return btn;
        }

        static Checkbox LoadCheckbox(Stream fs)
        {
            Checkbox obj = new Checkbox(ReadString(fs), ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadBool(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.Width = ReadInt(fs);
            obj.Height = ReadInt(fs);
            obj.BackgroundBottom = ReadColor(fs);
            obj.BackgroundTop = ReadColor(fs);
            obj.BorderColor = ReadColor(fs);
            obj.ForeColor = ReadColor(fs);
            obj.MarkColor = ReadColor(fs);
            obj.MarkSelectedColor = ReadColor(fs);
            return obj;
        }

        static Combobox LoadCombobox(Stream fs)
        {
            Combobox obj = new Combobox(ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadBool(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.ZebraStripe = ReadBool(fs);
            obj.ZebraStripeColor = ReadColor(fs);

            obj.Items = new ListboxItem[ReadInt(fs)];
            for (int i = 0; i < obj.Items.Length; i++)
            {
                if (fs.ReadByte() != 7)
                    throw new Exception("Invalid item type; expected 7");
                obj.Items[i] = new ListboxItem(ReadString(fs), ReadFont(fs), ReadColor(fs), ReadImage(fs), ReadBool(fs), ReadBool(fs));
            }

            return obj;
        }

        static Filebox LoadFilebox(Stream fs)
        {
            Filebox obj = new Filebox(ReadString(fs), ReadString(fs), ReadFont(fs), (FileListMode)fs.ReadByte(), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.AutoNavigate = ReadBool(fs);
            return obj;
        }

        static Form _LoadForm(Stream fs)
        {
            if (fs.ReadByte() != 4)
                throw new Exception("Invalid control type");

            Form frm = new Form(ReadString(fs));
            frm.BackColor = ReadColor(fs);
            frm.Enabled = ReadBool(fs);
            frm.BackgroundImageScaleMode = ReadScaleMode(fs);
            frm.BackgroundImage = ReadImage(fs);
            LoadChildren(fs, frm);

            return frm;
        }

        static GraphicLock LoadGraphicLock(Stream fs)
        {
            GraphicLock obj = new GraphicLock(ReadString(fs),ReadInt(fs),ReadInt(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            return obj;
        }

        static Label LoadLabel(Stream fs)
        {
            Label obj = new Label(ReadString(fs), ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadColor(fs), ReadBool(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.AutoSize = ReadBool(fs);
            obj.BackColor = ReadColor(fs);
            obj.TextAlignment = (HorizontalAlignment)fs.ReadByte();
            return obj;
        }

        static LineGraph LoadLineGraph(Stream fs)
        {
            LineGraph obj = new LineGraph(ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.LineThichkness = ReadInt(fs);

            int c = ReadInt(fs);
            int c2;
            for (int i = 0; i < c; i++)
            {
                LineGraphItem lgi = new LineGraphItem(ReadColor(fs));
                lgi.Visible = ReadBool(fs);
                c2 = ReadInt(fs);
                for (int j = 0; j < c2; j++)
                    lgi.AddPoint(new precisionpoint(ReadFloat(fs), ReadFloat(fs)));
                obj.AddLine(lgi);
            }

            return obj;
        }

        static Listbox LoadListbox(Stream fs)
        {
            Listbox obj = new Listbox(ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadBool(fs), ReadBool(fs), ReadSize(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.BackColor = ReadColor(fs);
            obj.MinimumLineHeight = ReadInt(fs);
            obj.SelectedBackColor = ReadColor(fs);
            obj.SelectedTextColor = ReadColor(fs);
            obj.ZebraStripe = ReadBool(fs);
            obj.ZebraStripeColor = ReadColor(fs);

            int len = ReadInt(fs);
            for (int i = 0; i < len; i++)
            {
                if (fs.ReadByte() != 7)
                    throw new Exception("Invalid item type; expected 7");
                obj.AddItem(new ListboxItem(ReadString(fs), ReadFont(fs), ReadColor(fs), ReadImage(fs), ReadBool(fs), ReadBool(fs)));
            }

            return obj;
        }

        static Listview LoadListView(Stream fs)
        {
            Listview obj = new Listview(ReadString(fs), ReadFont(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.HeaderColor = ReadColor(fs);
            obj.ItemColor = ReadColor(fs);
            obj.SelectionColor = ReadColor(fs);
            obj.SelectedTextColor = ReadColor(fs);

            int i;
            int c = ReadInt(fs);

            for (i = 0; i < c; i++)
                obj.AddColumn(new ListviewColumn("col" + i, ReadString(fs), ReadInt(fs)));

            c = ReadInt(fs);
            for (i = 0; i < c; i++)
            {
                string[] s = new string[ReadInt(fs)];
                for (int j = 0; j < s.Length; j++)
                    s[j] = ReadString(fs);
                obj.AddLine(new ListviewItem(s));
            }

            return obj;
        }

        static MenuStrip LoadMenuStrip(Stream fs)
        {
            MenuStrip obj = new MenuStrip(ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);

            int len = ReadInt(fs);
            for (int i = 0; i < len; i++)
                obj.AddMenuItem(LoadMenuItem(fs));

            return obj;
        }

        static MenuItem LoadMenuItem(Stream fs)
        {
            if (fs.ReadByte() != 9)
                throw new Exception("Invalid item type; expected 9");

            MenuItem obj = new MenuItem(ReadString(fs), ReadString(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);

            int len = ReadInt(fs);
            for (int i = 0; i < len; i++)
                obj.AddMenuItem(LoadMenuItem(fs));

            return obj;
        }

        static NumericUpDown LoadNumericUpDown(Stream fs)
        {
            NumericUpDown obj = new NumericUpDown(ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadBool(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.BackgroundBottom = ReadColor(fs);
            obj.BackgroundTop = ReadColor(fs);
            obj.ButtonBackgroundBottom = ReadColor(fs);
            obj.ButtonBackgroundTop = ReadColor(fs);
            obj.ButtonTextColor = ReadColor(fs);
            obj.ButtonTextShadowColor = ReadColor(fs);
            obj.DisabledTextColor = ReadColor(fs);
            obj.TextColor = ReadColor(fs);
            return obj;
        }

        static Panel LoadPanel(Stream fs)
        {
            Panel obj = new Panel(ReadString(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadColor(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.AutoScroll = ReadBool(fs);
            obj.BackgroundImageScaleMode = ReadScaleMode(fs);
            obj.BackgroundImage = ReadImage(fs);

            LoadChildren(fs, obj);

            return obj;
        }

        static Picturebox LoadPicturebox(Stream fs)
        {
            Picturebox obj = new Picturebox(ReadString(fs), ReadImage(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadColor(fs), false,
                (BorderStyle)fs.ReadByte(), ReadScaleMode(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.AutoSize = ReadBool(fs);
            return obj;
        }

        static Progressbar LoadProgressbar(Stream fs)
        {
            Progressbar obj = new Progressbar(ReadString(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.BackColor = ReadColor(fs);
            obj.BorderColor = ReadColor(fs);
            obj.GradientBottom = ReadColor(fs);
            obj.GradientTop = ReadColor(fs);
            obj.ProgressAlign = (HorizontalAlignment)fs.ReadByte();
            return obj;
        }

        static RadioButton LoadRadioButton(Stream fs)
        {
            RadioButton obj = new RadioButton(ReadString(fs), ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadString(fs), ReadBool(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.Width = ReadInt(fs);
            return obj;
        }

        static RichTextLabel LoadRichTextLabel(Stream fs)
        {
            RichTextLabel obj = new RichTextLabel(ReadString(fs), ReadString(fs), Skewworks.NETMF.Resources.Fonts.Droid11,
                Colors.Black, ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            return obj;
        }

        static Scrollbar LoadScrollbar(Stream fs)
        {
            Scrollbar obj = new Scrollbar(ReadString(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), (Orientation)fs.ReadByte(), ReadInt(fs), ReadInt(fs), ReadInt(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.AutoRepeating = ReadBool(fs);
            obj.LargeChange = ReadInt(fs);
            obj.SmallChange = ReadInt(fs);
            return obj;
        }

        static SlidePanelDialog LoadSlidePanelDialog(Stream fs)
        {
            SlidePanelDialog obj = new SlidePanelDialog(ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadBool(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.AnimateSlide = ReadBool(fs);
            obj.ForeColor = ReadColor(fs);

            int len = ReadInt(fs);
            for (int i = 0; i < len; i++)
            {
                if (fs.ReadByte() != 33)
                    throw new Exception("Invalid item type; expected 33");

                SlidePanel t = new SlidePanel("panel" + i, ReadString(fs), ReadColor(fs));
                LoadChildren(fs, t);

                obj.AddChild(t);
            }

            return obj;
        }

        static Slider LoadSlider(Stream fs)
        {
            Slider obj = new Slider(ReadString(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), (Orientation)fs.ReadByte(), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.BackColor = ReadColor(fs);
            obj.GradientBottom = ReadColor(fs);
            obj.GradientTop = ReadColor(fs);
            obj.LargeChange = ReadInt(fs);
            return obj;
        }

        static TabDialog LoadTabDialog(Stream fs)
        {
            TabDialog obj = new TabDialog(ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.ForeColor = ReadColor(fs);

            int len = ReadInt(fs);
            for (int i = 0; i < len; i++)
            {
                if (fs.ReadByte() != 18)
                    throw new Exception("Invalid item type; expected 18");

                Tab t = new Tab("tab" + i, ReadString(fs));
                LoadChildren(fs, t, 0, obj.Font.Height + 12);

                obj.AddChild(t);
            }


            return obj;
        }

        static TextArea LoadTextArea(Stream fs)
        {
            TextArea obj = new TextArea(ReadString(fs), ReadString(fs), ReadFont(fs), ReadColor(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            
            return obj;
        }

        static Textbox LoadTextbox(Stream fs)
        {
            Textbox obj = new Textbox(ReadString(fs), ReadString(fs), ReadFont(fs), ReadColor(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), (char)fs.ReadByte(), ReadBool(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.EditorFont = ReadFont(fs);
            obj.EditorLayout = (KeyboardLayout)fs.ReadByte();
            obj.EditorTitle = ReadString(fs);
            obj.ShowCaret = ReadBool(fs);
            return obj;
        }

        static Treeview LoadTreeview(Stream fs)
        {
            Treeview obj = new Treeview(ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.SelectedTextColor = ReadColor(fs);
            obj.TextColor = ReadColor(fs);

            int len = ReadInt(fs);
            for (int i = 0; i < len; i++)
                obj.AddNode(LoadTreeviewNode(fs));

            return obj;
        }

        static TreeviewNode LoadTreeviewNode(Stream fs)
        {
            if (fs.ReadByte() != 22)
                throw new Exception("Invalid item type; expected 22");

            TreeviewNode obj = new TreeviewNode(ReadString(fs));
            obj.Expanded = ReadBool(fs);

            int len = ReadInt(fs);
            for (int i = 0; i < len; i++)
                obj.AddNode(LoadTreeviewNode(fs));

            return obj;
        }

        static Window LoadWindow(Stream fs)
        {
            Window obj = new Window(ReadString(fs), ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs));
            obj.Enabled = ReadBool(fs);
            obj.Visible = ReadBool(fs);
            obj.AllowClose = ReadBool(fs);
            obj.AllowMaximize = ReadBool(fs);
            obj.AllowMinimize = ReadBool(fs);

            LoadChildren(fs, obj, 5, 12 + obj.Font.Height);

            return obj;
        }

        #endregion

        #region IO Methods

        static bool ReadBool(Stream fs)
        {
            if (fs.ReadByte() == 1)
                return true;
            return false;
        }

        static Color ReadColor(Stream fs)
        {
            return ColorUtility.ColorFromRGB((byte)fs.ReadByte(), (byte)fs.ReadByte(), (byte)fs.ReadByte());
        }

        static float ReadFloat(Stream fs)
        {
            byte[] b = new byte[8];
            fs.Read(b, 0, b.Length);
            long i = (int)(b[0] << 56);
            i += (int)(b[1] << 48);
            i += (int)(b[2] << 40);
            i += (int)(b[3] << 32);
            i += (int)(b[4] << 24);
            i += (int)(b[5] << 16);
            i += (int)(b[6] << 8);
            i += (int)b[7];
            return ((float)i) / 1000;
        }

        static Font ReadFont(Stream fs)
        {
            switch (fs.ReadByte())
            {
                case 0:
                    return Fonts.Droid8;
                case 1:
                    return Fonts.Droid8Bold;
                case 2:
                    return Fonts.Droid8BoldItalic;
                case 3:
                    return Fonts.Droid8Italic;
                case 10:
                    return Fonts.Droid9;
                case 11:
                    return Fonts.Droid9Bold;
                case 12:
                    return Fonts.Droid9BoldItalic;
                case 13:
                    return Fonts.Droid9Italic;
                case 20:
                    return Fonts.Droid11;
                case 21:
                    return Fonts.Droid11Bold;
                case 22:
                    return Fonts.Droid11BoldItalic;
                case 23:
                    return Fonts.Droid11Italic;
                case 30:
                    return Fonts.Droid12;
                case 31:
                    return Fonts.Droid12Bold;
                case 32:
                    return Fonts.Droid12BoldItalic;
                case 33:
                    return Fonts.Droid12Italic;
                case 40:
                    return Fonts.Droid16;
                case 41:
                    return Fonts.Droid16Bold;
                case 42:
                    return Fonts.Droid16BoldItalic;
                case 43:
                    return Fonts.Droid16Italic;
                default:
                    return null;
            }
        }

        static Bitmap ReadImage(Stream fs)
        {
            int len = ReadInt(fs);

            if (len == 0)
                return null;

            Debug.GC(true);
            byte[] b = new byte[len];
            fs.Read(b, 0, b.Length);
            Bitmap bmp = Core.ImageFromBytes(b);
            b = null;
            Debug.GC(true);

            return bmp;
        }

        static int ReadInt(Stream fs)
        {
            byte[] b = new byte[4];
            fs.Read(b, 0, b.Length);
            int i = (int)(b[0] << 24);
            i += (int)(b[1] << 16);
            i += (int)(b[2] << 8);
            i += (int)b[3];
            return i;
        }

        static ScaleMode ReadScaleMode(Stream fs)
        {
            return (ScaleMode)fs.ReadByte();
        }

        static size ReadSize(Stream fs)
        {
            return new size(ReadInt(fs), ReadInt(fs));
        }

        static string ReadString(Stream fs)
        {
            int len = ReadInt(fs);
            byte[] b = new byte[len];
            fs.Read(b, 0, b.Length);
            return new string(Encoding.UTF8.GetChars(b));
        }

        #endregion

    }
}
