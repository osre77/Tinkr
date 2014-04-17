using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;
using Skewworks.NETMF;
using Skewworks.NETMF.Controls;
using Skewworks.NETMF.Resources;

namespace TestSandbox.CommonControls
{
   public class ButtonsForm
   {
      public static ButtonsForm Instance { get; private set; }

      public static void Show()
      {
         if (Instance == null)
         {
            Instance = new ButtonsForm();
         }
         Instance.DoShow();
      }

      private readonly Form _form;

      private void DoShow()
      {
         Core.ActiveContainer = _form;
      }

      private ButtonsForm()
      {
         _form = new Form("ButtonsForm");

         _form.AddMenuStrip(
            MenuHelper.CreateMenuItem("Nav",
               MenuHelper.CreateMenuItem("Home", (sender, point) => Program.ShowMainForm())),
            MenuHelper.CreateMenuItem("Test",
               MenuHelper.CreateMenuItem("Size", TestSize)/*,
               MenuHelper.CreateMenuItem("Alignment", TestAlignment),
               MenuHelper.CreateMenuItem("Padding", TestPadding)*/,
                                                                  MenuHelper.CreateMenuItem("Events", TestEvents)
               ));
      }

      private Panel GetNewRootPanel()
      {
         var c = _form.GetChildByName("Root");
         if (c != null)
         {
            _form.RemoveChild(c);
            c.Dispose();
         }
         var p = new Panel("Root", 0, 28, _form.Width, _form.Height - 28);

         _form.AddChild(p);
         return p;
      }

      private Button[] _buttons;
      private int _n;
      private Panel _panel;

      private void TestSize(object sender, point point)
      {
         _panel = GetNewRootPanel();
         _panel.Suspended = true;
         _panel.BackColor = 0;

         var l = new Button("L1", "Auto size", Fonts.Droid9, 5, 5);
         _panel.AddChild(l);

         l = new Button("L2", "Fixed size large", Fonts.Droid9, 5, 40, 200, 50);
         _panel.AddChild(l);

         l = new Button("L3", "Fixed size small", Fonts.Droid9, 5, 100, 20, 25);
         _panel.AddChild(l);

         _buttons = new Button[2];
         _n = 0;

         var btn = new Button("Font", "F", Fonts.Droid11, 210, 5);
         btn.Tap += TestSizeFont;
         _panel.AddChild(btn);

         btn = new Button("Text", "T", Fonts.Droid11, 250, 5);
         btn.Tap += TestSizeText;
         _panel.AddChild(btn);

         btn = new Button("Color", "C", Fonts.Droid11, 290, 5);
         btn.Tap += TestSizeColor;
         _panel.AddChild(btn);


         _buttons[0] = new Button("Ls0", "Fixed size", Fonts.Droid9, 210, 40, 120, 60);
         _panel.AddChild(_buttons[0]);

         _buttons[1] = new Button("Ls0", "Auto size", Fonts.Droid9, 210, 120);
         _panel.AddChild(_buttons[1]);

         _panel.Suspended = false;
      }

      private void TestSizeFont(object sender, point point)
      {
         _panel.Suspended = true;
         for (int n = 0; n < _buttons.Length; ++n)
         {
            if (_buttons[n].Font == Fonts.Droid9)
            {
               _buttons[n].Font = Fonts.Droid12;
            }
            else
            {
               _buttons[n].Font = Fonts.Droid9;
            }
         }
         _panel.Suspended = false;
      }

      private void TestSizeText(object sender, point point)
      {
         _panel.Suspended = true;
         if (_buttons != null && _buttons.Length >= 2)
         {
            if (_buttons[0].Text == "Fixed size")
            {
               _buttons[0].Text = "A somewhat longer text\n2n line";
               _buttons[1].Text = "A somewhat longer text\n2n line";
            }
            else
            {
               _buttons[0].Text = "Fixed size";
               _buttons[1].Text = "Auto size";
            }
         }
         _panel.Suspended = false;
      }

      private void TestSizeColor(object sender, point point)
      {
         _panel.Suspended = true;
         for (int n = 0; n < _buttons.Length; ++n)
         {
            switch (_n)
            {
               case 0:
                  _buttons[n].NormalTextColor = Color.White;
                  _buttons[n].NormalTextShadowColor = Colors.Blue;
                  _buttons[n].BorderColor = Colors.Yellow;
                  _buttons[n].NormalColorTop = Colors.DarkRed;
                  _buttons[n].NormalColorBottom = Colors.CharcoalDust;

                  _buttons[n].PressedTextColor = Color.Black;
                  _buttons[n].PressedTextShadowColor = Colors.Green;
                  _buttons[n].BorderColorPressed = Colors.Cyan;
                  _buttons[n].PressedColorTop = Colors.Brown;
                  _buttons[n].PressedColorBottom = Colors.Orange;
                  break;

               case 1:
                  _buttons[n].NormalTextColor = Core.SystemColors.FontColor;
                  _buttons[n].NormalTextShadowColor = Core.SystemColors.TextShadow;
                  _buttons[n].BorderColor = Core.SystemColors.BorderColor;
                  _buttons[n].NormalColorTop = Core.SystemColors.ControlTop;
                  _buttons[n].NormalColorBottom = Core.SystemColors.ControlBottom;

                  _buttons[n].PressedTextColor = Core.SystemColors.SelectedFontColor;
                  _buttons[n].PressedTextShadowColor = Core.SystemColors.PressedTextShadow;
                  _buttons[n].BorderColorPressed = Core.SystemColors.PressedControlTop;
                  _buttons[n].PressedColorTop = Core.SystemColors.PressedControlTop;
                  _buttons[n].PressedColorBottom = Core.SystemColors.PressedControlBottom;
                  break;
            }
         }
         _panel.Suspended = false;

         if (++_n > 1)
         {
            _n = 0;
         }
      }

      private void TestEvents(object sender, point point)
      {
         _panel = GetNewRootPanel();
         _panel.Suspended = true;
         _panel.BackColor = 0;

         var lst = new Listbox("lst", Fonts.Droid9, 5, 35, _panel.Width - 10, _panel.Height - 40);

         var btn = new Button("btn", "Tab me ... tap me hard", Fonts.Droid11, 5, 5);
         btn.Tap += (o, point1) => lst.AddItem(new ListboxItem("Tap"));
         btn.TapHold += (o, point1) => lst.AddItem(new ListboxItem("TapHold"));
         btn.DoubleTap += (o, point1) => lst.AddItem(new ListboxItem("DoubleTap"));
         btn.TouchDown += (o, point1) => lst.AddItem(new ListboxItem("TouchDown"));
         btn.TouchMove += (o, point1) => lst.AddItem(new ListboxItem("TouchMove"));
         btn.TouchUp += (o, point1) => lst.AddItem(new ListboxItem("TouchUp"));
         btn.TouchGesture += (o, type, force) => lst.AddItem(new ListboxItem("TouchGesture"));
         btn.GotFocus += (o) =>lst.AddItem(new ListboxItem("GotFocus"));
         btn.LostFocus += (o) => lst.AddItem(new ListboxItem("LostFocus"));
         btn.ButtonPressed += (o, id) => lst.AddItem(new ListboxItem("ButtonPressed"));
         btn.ButtonReleased += (o, id) => lst.AddItem(new ListboxItem("ButtonReleased"));
         btn.KeyboardAltKey += (o, key, pressed) => lst.AddItem(new ListboxItem("KeyboardAltKey"));
         btn.KeyboardKey += (o, key, pressed) => lst.AddItem(new ListboxItem("KeyboardKey"));

         _panel.AddChild(btn);

         btn = new Button("btn2", "Other button", Fonts.Droid11, 200, 5);
         _panel.AddChild(btn);

         _panel.AddChild(lst);

         _panel.Suspended = false;
      }
   }
}
