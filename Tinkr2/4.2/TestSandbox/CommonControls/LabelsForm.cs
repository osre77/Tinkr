using System;
using Skewworks.NETMF;
using Skewworks.NETMF.Controls;
using Skewworks.NETMF.Resources;

namespace TestSandbox.CommonControls
{
   public class LabelsForm
   {
      public static LabelsForm Instance { get; private set; }

      public static void Show()
      {
         if (Instance == null)
         {
            Instance = new LabelsForm();
         }
         Instance.DoShow();
      }

      private readonly Form _form;

      private void DoShow()
      {
         Core.ActiveContainer = _form;
      }

      private LabelsForm()
      {
         _form = new Form("LabelsForm");

         _form.AddMenuStrip(
            MenuHelper.CreateMenuItem("Nav",
               MenuHelper.CreateMenuItem("Home", (sender, point) => Program.ShowMainForm())),
            MenuHelper.CreateMenuItem("Test",
               MenuHelper.CreateMenuItem("Fonts", TestFonts),
               MenuHelper.CreateMenuItem("Size", TestSize),
               MenuHelper.CreateMenuItem("Alignment", TestAlignment),
               MenuHelper.CreateMenuItem("Padding", TestPadding)
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

      private void TestFonts(object sender, point point)
      {
         var panel = GetNewRootPanel();
         panel.BackColor = 0;

         int x = 1;
         var l = new Label("L1", "Droid8", Fonts.Droid8, x, 1, false);
         panel.AddChild(l);

         l = new Label("L2", "Droid9", Fonts.Droid9, x, l.Y + l.Height + 1, false);
         panel.AddChild(l);

         l = new Label("L3", "Droid11", Fonts.Droid11, x, l.Y + l.Height + 1, false);
         panel.AddChild(l);

         l = new Label("L4", "Droid12", Fonts.Droid12, x, l.Y + l.Height + 1, false);
         panel.AddChild(l);

         l = new Label("L5", "Droid16", Fonts.Droid12, x, l.Y + l.Height + 1, false);
         panel.AddChild(l);


         x = l.X + l.Width + 2;
         
         l = new Label("I1", "Italic8", Fonts.Droid8Italic, x, 1, false);
         panel.AddChild(l);

         l = new Label("I2", "Italic9", Fonts.Droid9Italic, x, l.Y + l.Height + 1, false);
         panel.AddChild(l);

         l = new Label("I3", "Italic11", Fonts.Droid11Italic, x, l.Y + l.Height + 1, false);
         panel.AddChild(l);

         l = new Label("I4", "Italic12", Fonts.Droid12Italic, x, l.Y + l.Height + 1, false);
         panel.AddChild(l);

         l = new Label("I5", "Italic16", Fonts.Droid16Italic, x, l.Y + l.Height + 1, false);
         panel.AddChild(l);


         x = l.X + l.Width + 2;

         l = new Label("B1", "Bold8", Fonts.Droid8Bold, x, 1, false);
         panel.AddChild(l);

         l = new Label("B2", "Bold9", Fonts.Droid9Bold, x, l.Y + l.Height + 1, false);
         panel.AddChild(l);

         l = new Label("B3", "Bold11", Fonts.Droid11Bold, x, l.Y + l.Height + 1, false);
         panel.AddChild(l);

         l = new Label("B4", "Bold12", Fonts.Droid12Bold, x, l.Y + l.Height + 1, false);
         panel.AddChild(l);

         l = new Label("B5", "Bold16", Fonts.Droid16Bold, x, l.Y + l.Height + 1, false);
         panel.AddChild(l);


         x = l.X + l.Width + 2;

         l = new Label("BI1", "BoldItalic 8", Fonts.Droid8BoldItalic, x, 1, false);
         panel.AddChild(l);

         l = new Label("BI2", "BoldItalic 9", Fonts.Droid9BoldItalic, x, l.Y + l.Height + 1, false);
         panel.AddChild(l);

         l = new Label("BI3", "BoldItalic 11", Fonts.Droid11BoldItalic, x, l.Y + l.Height + 1, false);
         panel.AddChild(l);

         l = new Label("BI4", "BoldItalic 12", Fonts.Droid12BoldItalic, x, l.Y + l.Height + 1, false);
         panel.AddChild(l);

         l = new Label("BI5", "BoldItalic 16", Fonts.Droid16BoldItalic, x, l.Y + l.Height + 1, false);
         panel.AddChild(l);

         int y = l.Y + l.Height + 5;
         x = 1;
         l = new Label("L1", "Red", Fonts.Droid11, x, y, Colors.Red, false);
         panel.AddChild(l);

         l = new Label("L1", "Green", Fonts.Droid11, x, l.Y + l.Height + 1, Colors.Green, false);
         panel.AddChild(l);

         l = new Label("L1", "Blue", Fonts.Droid11, x, l.Y + l.Height + 1, Colors.Blue, false);
         panel.AddChild(l);

         l = new Label("L1", "Red", Fonts.Droid11, x, l.Y + l.Height + 1, Colors.Red, false);
         l.BackColor = Colors.DarkGray;
         panel.AddChild(l);

         l = new Label("L1", "Green", Fonts.Droid11, x, l.Y + l.Height + 1, Colors.Green, false);
         l.BackColor = Colors.Yellow;
         panel.AddChild(l);

         l = new Label("L1", "Blue", Fonts.Droid11, x, l.Y + l.Height + 1, Colors.Blue, false);
         l.BackColor = Colors.Brown;
         panel.AddChild(l);

         x = l.X + l.Width + 30;

         l = new Label("BI5", "Back", Fonts.Droid16, x, y, false);
         panel.AddChild(l);

         l = new Label("BI5", "Transparent", Fonts.Droid8, x + 1, y + 2, Colors.Red);
         panel.AddChild(l);
      }

      private Label[] _labels;
      private int _n;

      private void TestSize(object sender, point point)
      {
         var panel = GetNewRootPanel();
         panel.BackColor = 0;

         var l = new Label("L1", "Auto size", Fonts.Droid9, 1, 1, false);
         panel.AddChild(l);

         l = new Label("L2", "Fixed size large", Fonts.Droid9, 1, 30, 200, 30, false);
         panel.AddChild(l);

         l = new Label("L3", "Fixed size small", Fonts.Droid9, 1, 65, 20, 10, false);
         panel.AddChild(l);

         l = new Label("L4", "Fixed width", Fonts.Droid9, 1, 95, 200, Colors.Black, false);
         panel.AddChild(l);


         

         _labels = new Label[2];

         var btn = new Button("Font", "F", Fonts.Droid11, 210, 5);
         btn.Tap += TestSizeFont;
         panel.AddChild(btn);

         btn = new Button("Text", "T", Fonts.Droid11, 245, 5);
         btn.Tap += TestSizeText;
         panel.AddChild(btn);

         btn = new Button("Color", "C", Fonts.Droid11, 280, 5);
         btn.Tap += TestSizeColor;
         panel.AddChild(btn);

         btn = new Button("BackColor", "B", Fonts.Droid11, 315, 5);
         btn.Tap += TestSizeBackColor;
         panel.AddChild(btn);

         _labels[0] = new Label("Ls0", "Fixed size", Fonts.Droid9, 210, 35, 200, 30, false);
         panel.AddChild(_labels[0]);

         _labels[1] = new Label("Ls0", "Auto size", Fonts.Droid9, 210, 80, false);
         panel.AddChild(_labels[1]);


      }

      private void TestSizeFont(object sender, point point)
      {
         for (int n = 0; n < _labels.Length; ++n)
         {
            if (_labels[n].Font == Fonts.Droid9)
            {
               _labels[n].Font = Fonts.Droid12;
            }
            else
            {
               _labels[n].Font = Fonts.Droid9;
            }
         }
      }

      private void TestSizeText(object sender, point point)
      {
         if (_labels != null && _labels.Length >= 2)
         {
            if (_labels[0].Text == "Fixed size")
            {
               _labels[0].Text = "A somewhat longer text\n2n line";
               _labels[1].Text = "A somewhat longer text\n2n line";
            }
            else
            {
               _labels[0].Text = "Fixed size";
               _labels[1].Text = "Auto size";
            }
         }
      }

      private void TestSizeColor(object sender, point point)
      {
         if (_labels != null && _labels.Length >= 2)
         {
            if (_labels[0].Color == Core.SystemColors.FontColor)
            {
               _labels[0].Color = Colors.Green;
               _labels[1].Color = Colors.Blue;
            }
            else
            {
               _labels[0].Color = Core.SystemColors.FontColor;
               _labels[1].Color = Core.SystemColors.FontColor;
            }
         }
      }

      private void TestSizeBackColor(object sender, point point)
      {
         if (_labels != null && _labels.Length >= 2)
         {
            if (_labels[0].BackColor == Core.SystemColors.ContainerBackground)
            {
               _labels[0].BackColor = Colors.Orange;
               _labels[1].BackColor = Colors.Orange;
            }
            else
            {
               _labels[0].BackColor = Core.SystemColors.ContainerBackground;
               _labels[1].BackColor = Core.SystemColors.ContainerBackground;
            }
         }
      }


      private void TestAlignment(object sender, point point)
      {
         var panel = GetNewRootPanel();
         panel.BackColor = 0;

         var btn = new Button("HL", "L", Fonts.Droid11, 10, 10);
         btn.Tag = HorizontalAlignment.Left;
         btn.Tap += TestAlignmentHorizontal;
         panel.AddChild(btn);

         btn = new Button("HC", "C", Fonts.Droid11, 150, 10);
         btn.Tag = HorizontalAlignment.Center;
         btn.Tap += TestAlignmentHorizontal;
         panel.AddChild(btn);

         btn = new Button("HR", "R", Fonts.Droid11, 280, 10);
         btn.Tag = HorizontalAlignment.Right;
         btn.Tap += TestAlignmentHorizontal;
         panel.AddChild(btn);


         btn = new Button("VT", "T", Fonts.Droid11, 315, 50);
         btn.Tag = VerticalAlignment.Top;
         btn.Tap += TestAlignmentVertical;
         panel.AddChild(btn);

         btn = new Button("VC", "C", Fonts.Droid11, 315, 85);
         btn.Tag = VerticalAlignment.Center;
         btn.Tap += TestAlignmentVertical;
         panel.AddChild(btn);

         btn = new Button("VB", "B", Fonts.Droid11, 315, 120);
         btn.Tag = VerticalAlignment.Bottom;
         btn.Tap += TestAlignmentVertical;
         panel.AddChild(btn);

         _labels = new Label[2];
         _labels[0] = new Label("L1", "Auto size\nL2", Fonts.Droid11, 10, 50, false);
         panel.AddChild(_labels[0]);

         _labels[1] = new Label("L2", "Fixed size\nL2", Fonts.Droid11, 10, 90, 300, 60, false);
         panel.AddChild(_labels[1]);
      }

      private void TestAlignmentHorizontal(object sender, point point)
      {
         _labels[0].TextAlignment = (HorizontalAlignment)((Button) sender).Tag;
         _labels[1].TextAlignment = (HorizontalAlignment)((Button)sender).Tag;
      }

      private void TestAlignmentVertical(object sender, point point)
      {
         _labels[0].VerticalAlignment = (VerticalAlignment)((Button)sender).Tag;
         _labels[1].VerticalAlignment = (VerticalAlignment)((Button)sender).Tag;
      }


      private void TestPadding(object sender, point point)
      {
         var panel = GetNewRootPanel();
         panel.BackColor = 0;

         var btn = new Button("Padding", "Padding", Fonts.Droid11, 10, 10);
         btn.Tag = HorizontalAlignment.Left;
         btn.Tap += TestPaddingSwitch;
         panel.AddChild(btn);

         _labels = new Label[2];
         _n = 0;

         _labels[0] = new Label("L1", "Auto size\nL2", Fonts.Droid11, 10, 50, false);
         _labels[0].Padding = new Thickness(5);
         panel.AddChild(_labels[0]);

         _labels[1] = new Label("L2", "5", Fonts.Droid11, 10, 120, 300, 60, false);
         _labels[1].Padding = new Thickness(5);
         panel.AddChild(_labels[1]);
      }

      private void TestPaddingSwitch(object sender, point point)
      {
         switch (_n)
         {
            case 0:
               _labels[0].Padding = new Thickness(10, 3);
               _labels[1].Padding = new Thickness(10, 3);
               _labels[1].Text = "10, 3";
               break;
            case 1:
               _labels[0].Padding = new Thickness(5, 10, 15, 20);
               _labels[1].Padding = new Thickness(5, 10, 15, 20);
               _labels[1].Text = "5, 10, 15, 20";
               break;
            case 2:
               _labels[0].Padding = new Thickness(0);
               _labels[1].Padding = new Thickness(0);
               _labels[1].Text = "0";
               break;
            case 3:
               _labels[0].Padding = new Thickness(10);
               _labels[1].Padding = new Thickness(10);
               _labels[1].Text = "10";
               break;
            case 4:
               _labels[0].Padding = new Thickness(5);
               _labels[1].Padding = new Thickness(5);
               _labels[1].Text = "5";
               break;
         }
         if (++_n > 4)
         {
            _n = 0;
         }
      }
   }
}
