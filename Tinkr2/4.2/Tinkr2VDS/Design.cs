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

         try
         {
            fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var b = new byte[4];

            // Read header
            fs.Read(b, 0, b.Length);
            if (new string(Encoding.UTF8.GetChars(b)) != "TnkR" && new string(Encoding.UTF8.GetChars(b)) != "CLiX")
               throw new Exception("Invalid header");

            // Read Version
            fs.Read(b, 0, b.Length);
            if (b[0] != 2 || b[1] != 2)
               throw new Exception("Invalid version");

            // Load Form
            return _LoadForm(fs);
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
         var ms = new MemoryStream(data);
         var b = new byte[4];

         // Read header
         ms.Read(b, 0, b.Length);
         if (new string(Encoding.UTF8.GetChars(b)) != "TnkR" && new string(Encoding.UTF8.GetChars(b)) != "CLiX")
            throw new Exception("Invalid header");

         // Read Version
         ms.Read(b, 0, b.Length);
         if (b[0] != 2 || b[1] != 2)
            throw new Exception("Invalid version");

         // Load Form
         return _LoadForm(ms);
      }

      #endregion

      #region Load Methods

      static void LoadChildren(Stream fs, IContainer parent, int offsetX = 0, int offsetY = 0)
      {
         IControl ctrl = null;

         while (true)
         {
            var id = fs.ReadByte();
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

            if (ctrl != null)
            {
               ctrl.X -= offsetX;
               ctrl.Y -= offsetY;
               parent.AddChild(ctrl);
            }
         }
      }

      static Appbar LoadAppbar(Stream fs)
      {
         var obj = new Appbar(ReadString(fs), ReadInt(fs), ReadFont(fs), ReadFont(fs), (Skewworks.Tinkr.DockLocation)fs.ReadByte(), ReadBool(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            BackColor = ReadColor(fs),
            ForeColor = ReadColor(fs),
            TimeFormat = (TimeFormat)fs.ReadByte()
         };

         int i;
         var c = ReadInt(fs);

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
         var btn = new Button(ReadString(fs), ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadBool(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            BorderColor = ReadColor(fs),
            BorderColorPressed = ReadColor(fs),
            NormalColorBottom = ReadColor(fs),
            NormalColorTop = ReadColor(fs),
            NormalTextColor = ReadColor(fs),
            PressedColorBottom = ReadColor(fs),
            PressedColorTop = ReadColor(fs),
            PressedTextColor = ReadColor(fs),
            BackgroundImageScaleMode = ReadScaleMode(fs),
            BackgroundImage = ReadImage(fs)
         };

         return btn;
      }

      static Checkbox LoadCheckbox(Stream fs)
      {
         var obj = new Checkbox(ReadString(fs), ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadBool(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            Width = ReadInt(fs),
            Height = ReadInt(fs),
            BackgroundBottom = ReadColor(fs),
            BackgroundTop = ReadColor(fs),
            BorderColor = ReadColor(fs),
            ForeColor = ReadColor(fs),
            MarkColor = ReadColor(fs),
            MarkSelectedColor = ReadColor(fs)
         };
         return obj;
      }

      static Combobox LoadCombobox(Stream fs)
      {
         var obj = new Combobox(ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadBool(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            ZebraStripe = ReadBool(fs),
            ZebraStripeColor = ReadColor(fs),
            Items = new ListboxItem[ReadInt(fs)]
         };

         for (var i = 0; i < obj.Items.Length; i++)
         {
            if (fs.ReadByte() != 7)
               throw new Exception("Invalid item type; expected 7");
            obj.Items[i] = new ListboxItem(ReadString(fs), ReadFont(fs), ReadColor(fs), ReadImage(fs), ReadBool(fs), ReadBool(fs));
         }

         return obj;
      }

      static Filebox LoadFilebox(Stream fs)
      {
         var obj = new Filebox(ReadString(fs), ReadString(fs), ReadFont(fs), (FileListMode)fs.ReadByte(), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            AutoNavigate = ReadBool(fs)
         };
         return obj;
      }

      static Form _LoadForm(Stream fs)
      {
         if (fs.ReadByte() != 4)
            throw new Exception("Invalid control type");

         var frm = new Form(ReadString(fs))
         {
            BackColor = ReadColor(fs),
            Enabled = ReadBool(fs),
            BackgroundImageScaleMode = ReadScaleMode(fs),
            BackgroundImage = ReadImage(fs)
         };
         LoadChildren(fs, frm);

         return frm;
      }

      static GraphicLock LoadGraphicLock(Stream fs)
      {
         var obj = new GraphicLock(ReadString(fs), ReadInt(fs), ReadInt(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs)
         };
         return obj;
      }

      static Label LoadLabel(Stream fs)
      {
         var obj = new Label(ReadString(fs), ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadColor(fs), ReadBool(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            AutoSize = ReadBool(fs),
            BackColor = ReadColor(fs),
            TextAlignment = (HorizontalAlignment)fs.ReadByte()
         };
         return obj;
      }

      static LineGraph LoadLineGraph(Stream fs)
      {
         var obj = new LineGraph(ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            LineThichkness = ReadInt(fs)
         };

         var c = ReadInt(fs);
         for (var i = 0; i < c; i++)
         {
            var lgi = new LineGraphItem(ReadColor(fs)) { Visible = ReadBool(fs) };
            var c2 = ReadInt(fs);
            for (var j = 0; j < c2; j++)
               lgi.AddPoint(new precisionpoint(ReadFloat(fs), ReadFloat(fs)));
            obj.AddLine(lgi);
         }

         return obj;
      }

      static Listbox LoadListbox(Stream fs)
      {
         var obj = new Listbox(ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadBool(fs), ReadBool(fs), ReadSize(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            BackColor = ReadColor(fs),
            MinimumLineHeight = ReadInt(fs),
            SelectedBackColor = ReadColor(fs),
            SelectedTextColor = ReadColor(fs),
            ZebraStripe = ReadBool(fs),
            ZebraStripeColor = ReadColor(fs)
         };

         var len = ReadInt(fs);
         for (var i = 0; i < len; i++)
         {
            if (fs.ReadByte() != 7)
               throw new Exception("Invalid item type; expected 7");
            obj.AddItem(new ListboxItem(ReadString(fs), ReadFont(fs), ReadColor(fs), ReadImage(fs), ReadBool(fs), ReadBool(fs)));
         }

         return obj;
      }

      static Listview LoadListView(Stream fs)
      {
         var obj = new Listview(ReadString(fs), ReadFont(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            HeaderColor = ReadColor(fs),
            ItemColor = ReadColor(fs),
            SelectionColor = ReadColor(fs),
            SelectedTextColor = ReadColor(fs)
         };

         int i;
         var c = ReadInt(fs);

         for (i = 0; i < c; i++)
            obj.AddColumn(new ListviewColumn("col" + i, ReadString(fs), ReadInt(fs)));

         c = ReadInt(fs);
         for (i = 0; i < c; i++)
         {
            var s = new string[ReadInt(fs)];
            for (var j = 0; j < s.Length; j++)
               s[j] = ReadString(fs);
            obj.AddLine(new ListviewItem(s));
         }

         return obj;
      }

      static MenuStrip LoadMenuStrip(Stream fs)
      {
         var obj = new MenuStrip(ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs)
         };

         var len = ReadInt(fs);
         for (var i = 0; i < len; i++)
            obj.AddMenuItem(LoadMenuItem(fs));

         return obj;
      }

      static MenuItem LoadMenuItem(Stream fs)
      {
         if (fs.ReadByte() != 9)
            throw new Exception("Invalid item type; expected 9");

         var obj = new MenuItem(ReadString(fs), ReadString(fs)) { Enabled = ReadBool(fs), Visible = ReadBool(fs) };

         var len = ReadInt(fs);
         for (var i = 0; i < len; i++)
            obj.AddMenuItem(LoadMenuItem(fs));

         return obj;
      }

      static NumericUpDown LoadNumericUpDown(Stream fs)
      {
         var obj = new NumericUpDown(ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadBool(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            BackgroundBottom = ReadColor(fs),
            BackgroundTop = ReadColor(fs),
            ButtonBackgroundBottom = ReadColor(fs),
            ButtonBackgroundTop = ReadColor(fs),
            ButtonTextColor = ReadColor(fs),
            ButtonTextShadowColor = ReadColor(fs),
            DisabledTextColor = ReadColor(fs),
            TextColor = ReadColor(fs)
         };
         return obj;
      }

      static Panel LoadPanel(Stream fs)
      {
         var obj = new Panel(ReadString(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadColor(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            AutoScroll = ReadBool(fs),
            BackgroundImageScaleMode = ReadScaleMode(fs),
            BackgroundImage = ReadImage(fs)
         };

         LoadChildren(fs, obj);

         return obj;
      }

      static Picturebox LoadPicturebox(Stream fs)
      {
         var obj = new Picturebox(ReadString(fs), ReadImage(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadColor(fs), false,
             (BorderStyle)fs.ReadByte(), ReadScaleMode(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            AutoSize = ReadBool(fs)
         };
         return obj;
      }

      static Progressbar LoadProgressbar(Stream fs)
      {
         var obj = new Progressbar(ReadString(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            BackColor = ReadColor(fs),
            BorderColor = ReadColor(fs),
            GradientBottom = ReadColor(fs),
            GradientTop = ReadColor(fs),
            ProgressAlign = (HorizontalAlignment)fs.ReadByte()
         };
         return obj;
      }

      static RadioButton LoadRadioButton(Stream fs)
      {
         var obj = new RadioButton(ReadString(fs), ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadString(fs), ReadBool(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            Width = ReadInt(fs)
         };
         return obj;
      }

      static RichTextLabel LoadRichTextLabel(Stream fs)
      {
         var obj = new RichTextLabel(ReadString(fs), ReadString(fs), Fonts.Droid11,
             Colors.Black, ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs)
         };
         return obj;
      }

      static Scrollbar LoadScrollbar(Stream fs)
      {
         var obj = new Scrollbar(ReadString(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), (Orientation)fs.ReadByte(), ReadInt(fs), ReadInt(fs), ReadInt(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            AutoRepeating = ReadBool(fs),
            LargeChange = ReadInt(fs),
            SmallChange = ReadInt(fs)
         };
         return obj;
      }

      static SlidePanelDialog LoadSlidePanelDialog(Stream fs)
      {
         var obj = new SlidePanelDialog(ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadBool(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            AnimateSlide = ReadBool(fs),
            ForeColor = ReadColor(fs)
         };

         var len = ReadInt(fs);
         for (var i = 0; i < len; i++)
         {
            if (fs.ReadByte() != 33)
               throw new Exception("Invalid item type; expected 33");

            var t = new SlidePanel("panel" + i, ReadString(fs), ReadColor(fs));
            LoadChildren(fs, t);

            obj.AddChild(t);
         }

         return obj;
      }

      static Slider LoadSlider(Stream fs)
      {
         var obj = new Slider(ReadString(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), (Orientation)fs.ReadByte(), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            BackColor = ReadColor(fs),
            GradientBottom = ReadColor(fs),
            GradientTop = ReadColor(fs),
            LargeChange = ReadInt(fs)
         };
         return obj;
      }

      static TabDialog LoadTabDialog(Stream fs)
      {
         var obj = new TabDialog(ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            ForeColor = ReadColor(fs)
         };

         var len = ReadInt(fs);
         for (var i = 0; i < len; i++)
         {
            if (fs.ReadByte() != 18)
               throw new Exception("Invalid item type; expected 18");

            var t = new Tab("tab" + i, ReadString(fs));
            LoadChildren(fs, t, 0, obj.Font.Height + 12);

            obj.AddChild(t);
         }


         return obj;
      }

      static TextArea LoadTextArea(Stream fs)
      {
         var obj = new TextArea(ReadString(fs), ReadString(fs), ReadFont(fs), ReadColor(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs)
         };

         return obj;
      }

      static Textbox LoadTextbox(Stream fs)
      {
         var obj = new Textbox(ReadString(fs), ReadString(fs), ReadFont(fs), ReadColor(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), (char)fs.ReadByte(), ReadBool(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            EditorFont = ReadFont(fs),
            EditorLayout = (KeyboardLayout)fs.ReadByte(),
            EditorTitle = ReadString(fs),
            ShowCaret = ReadBool(fs)
         };
         return obj;
      }

      static Treeview LoadTreeview(Stream fs)
      {
         var obj = new Treeview(ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            SelectedTextColor = ReadColor(fs),
            TextColor = ReadColor(fs)
         };

         var len = ReadInt(fs);
         for (var i = 0; i < len; i++)
            obj.AddNode(LoadTreeviewNode(fs));

         return obj;
      }

      static TreeviewNode LoadTreeviewNode(Stream fs)
      {
         if (fs.ReadByte() != 22)
            throw new Exception("Invalid item type; expected 22");

         var obj = new TreeviewNode(ReadString(fs)) { Expanded = ReadBool(fs) };

         var len = ReadInt(fs);
         for (var i = 0; i < len; i++)
            obj.AddNode(LoadTreeviewNode(fs));

         return obj;
      }

      static Window LoadWindow(Stream fs)
      {
         var obj = new Window(ReadString(fs), ReadString(fs), ReadFont(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs), ReadInt(fs))
         {
            Enabled = ReadBool(fs),
            Visible = ReadBool(fs),
            AllowClose = ReadBool(fs),
            AllowMaximize = ReadBool(fs),
            AllowMinimize = ReadBool(fs)
         };

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
         var b = new byte[8];
         fs.Read(b, 0, b.Length);
         long i = b[0] << 56;
         i += b[1] << 48;
         i += b[2] << 40;
         i += b[3] << 32;
         i += b[4] << 24;
         i += b[5] << 16;
         i += b[6] << 8;
         i += b[7];
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
         var len = ReadInt(fs);

         if (len == 0)
            return null;

         Debug.GC(true);
         var b = new byte[len];
         fs.Read(b, 0, b.Length);
         var bmp = Core.ImageFromBytes(b);
         Debug.GC(true);

         return bmp;
      }

      static int ReadInt(Stream fs)
      {
         var b = new byte[4];
         fs.Read(b, 0, b.Length);
         var i = b[0] << 24;
         i += b[1] << 16;
         i += b[2] << 8;
         i += b[3];
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
         var len = ReadInt(fs);
         var b = new byte[len];
         fs.Read(b, 0, b.Length);
         return new string(Encoding.UTF8.GetChars(b));
      }

      #endregion

   }
}
