using System;
using System.IO;
using System.Text;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF;
using Skewworks.NETMF.Controls;
using Skewworks.NETMF.Resources;

namespace Skewworks.VDS.Clix
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
            if (new string(Encoding.UTF8.GetChars(b)) != "CLiX")
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

      static void LoadChildren(Stream fs, IContainer parent)
      {
         while (true)
         {
            int id = fs.ReadByte();
            switch (id)
            {
               case 0:     // Button
                  parent.AddChild(LoadButton(fs));
                  break;
               case 1:     // Checkbox
                  parent.AddChild(LoadCheckbox(fs));
                  break;
               case 2:     // Combobox
                  parent.AddChild(LoadCombobox(fs));
                  break;
               case 3:     // Filebox
                  parent.AddChild(LoadFilebox(fs));
                  break;
               case 5:     // Label
                  parent.AddChild(LoadLabel(fs));
                  break;
               case 6:     // Listbox
                  parent.AddChild(LoadListbox(fs));
                  break;
               case 7:     // ListboxItem

                  break;
               case 8:     // MenuStrip
                  parent.AddChild(LoadMenuStrip(fs));
                  break;
               case 9:     // MenuItem
                  parent.AddChild(LoadMenuItem(fs));
                  break;
               case 10:    // NumericUpDown
                  parent.AddChild(LoadNumericUpDown(fs));
                  break;
               case 11:    // Panel
                  parent.AddChild(LoadPanel(fs));
                  break;
               case 12:    // Picturebox
                  parent.AddChild(LoadPicturebox(fs));
                  break;
               case 13:    // Progressbar
                  parent.AddChild(LoadProgressbar(fs));
                  break;
               case 14:    // RadioButton
                  parent.AddChild(LoadRadioButton(fs));
                  break;
               case 15:    // RichTextLabel
                  parent.AddChild(LoadRichTextLabel(fs));
                  break;
               case 16:    // Scrollbar
                  parent.AddChild(LoadScrollbar(fs));
                  break;
               case 17:    // Slider
                  parent.AddChild(LoadSlider(fs));
                  break;
               case 18:    // Tab
                  break;
               case 19:    // TabDialog
                  parent.AddChild(LoadTabDialog(fs));
                  break;
               case 20:    // Textbox
                  parent.AddChild(LoadTextbox(fs));
                  break;
               case 21:    // Treeview
                  parent.AddChild(LoadTreeview(fs));
                  break;
               case 22:    // TreviewNode
                  break;
               case 255:   // End of Container
                  return;
               default:    // Unknown/Invalid
                  throw new Exception("Control Ident failure (" + id + ")");
            }
         }
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
            BackgroundImageImageScaleModeMode = ReadScaleMode(fs),
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

         int len = ReadInt(fs);
         for (int i = 0; i < len; i++)
         {
            if (fs.ReadByte() != 7)
               throw new Exception("Invalid item type; expected 7");
            obj.AddItem(new ListboxItem(ReadString(fs), ReadFont(fs), ReadColor(fs), ReadImage(fs), ReadBool(fs), ReadBool(fs)));
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

         int len = ReadInt(fs);
         for (int i = 0; i < len; i++)
            obj.AddMenuItem(LoadMenuItem(fs));

         return obj;
      }

      static MenuItem LoadMenuItem(Stream fs)
      {
         if (fs.ReadByte() != 9)
            throw new Exception("Invalid item type; expected 9");

         var obj = new MenuItem(ReadString(fs), ReadString(fs)) { Enabled = ReadBool(fs), Visible = ReadBool(fs) };

         int len = ReadInt(fs);
         for (int i = 0; i < len; i++)
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

         int len = ReadInt(fs);
         for (int i = 0; i < len; i++)
         {
            if (fs.ReadByte() != 18)
               throw new Exception("Invalid item type; expected 18");

            var t = new Tab("tab" + i, ReadString(fs));
            LoadChildren(fs, t);

            obj.AddChild(t);
         }


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

         int len = ReadInt(fs);
         for (int i = 0; i < len; i++)
            obj.AddNode(LoadTreeviewNode(fs));

         return obj;
      }

      static TreeviewNode LoadTreeviewNode(Stream fs)
      {
         if (fs.ReadByte() != 21)
            throw new Exception("Invalid item type; expected 21");

         var obj = new TreeviewNode(ReadString(fs)) { Expanded = ReadBool(fs) };

         int len = ReadInt(fs);
         for (int i = 0; i < len; i++)
            obj.AddNode(LoadTreeviewNode(fs));

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
         var b = new byte[len];
         fs.Read(b, 0, b.Length);
         Bitmap bmp = Core.ImageFromBytes(b);
         Debug.GC(true);

         return bmp;
      }

      static int ReadInt(Stream fs)
      {
         var b = new byte[4];
         fs.Read(b, 0, b.Length);
         int i = b[0] << 24;
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
         int len = ReadInt(fs);
         var b = new byte[len];
         fs.Read(b, 0, b.Length);
         return new string(Encoding.UTF8.GetChars(b));
      }

      #endregion

   }
}
