using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF;
using Skewworks.NETMF.Controls;

namespace Skewworks.NETMF.Modal
{
    public class Prompt
    {

        #region Variables

        private static ManualResetEvent _activeBlock = null;            // Used to display Modal Forms
        private static Button[] _btns;
        private static PromptResult _result;
        private static int _iResult;

        #endregion

        #region Public Methods

        public static PromptResult Show(string Title, string Message, Font TitleFont, Font MessageFont, PromptType Type = PromptType.OK, Bitmap Icon = null)
        {
            int w, h, x, y, i;
            int xOffset = 0;

            Core.Screen.SetClippingRectangle(0, 0, Core.ScreenWidth, Core.ScreenHeight);

            Color cR1 = ColorUtility.ColorFromRGB(226, 92, 79);
            Color cR2 = ColorUtility.ColorFromRGB(202, 48, 53);

            IContainer prv = Core.ActiveContainer;
            Form frmModal = new Form("modalAlertForm", Colors.Ghost);
            frmModal.TouchDown += new OnTouchDown(frmModal_TouchDown);
            frmModal.TouchUp += new OnTouchUp(frmModal_TouchUp);
            Core.SilentlyActivate(frmModal);

            // Determine required size
            TitleFont.ComputeExtent(Title, out w, out h);
            size sz = FontManager.ComputeExtentEx(MessageFont, Message);
            if (sz.Width > w)
                w = sz.Width;

            if (Icon != null)
            {
                xOffset = Icon.Width + 16;
                w += xOffset;
            }

            // Adjust for Buttons
            switch (Type)
            {
                case PromptType.AbortContinue:
                    i = FontManager.ComputeExtentEx(MessageFont, "AbortContinue").Width + 30;
                    break;
                case PromptType.AbortRetryContinue:
                    i = FontManager.ComputeExtentEx(MessageFont, "AbortRetryContinue").Width + 40;
                    break;
                case PromptType.OKCancel:
                    i = FontManager.ComputeExtentEx(MessageFont, "   OK   Cancel").Width + 30;
                    break;
                case PromptType.YesNo:
                    i = FontManager.ComputeExtentEx(MessageFont, "  Yes     No   ").Width + 30;
                    break;
                case PromptType.YesNoCancel:
                    i = FontManager.ComputeExtentEx(MessageFont, "  Yes     No   Cancel").Width + 30;
                    break;
                default:
                    i = FontManager.ComputeExtentEx(MessageFont, "   OK   ").Width + 20;
                    break;
            }
            if (w < i)
                w = i;

            // Add Padding
            w += 32;

            // Now make sure we're within the screen width
            if (w > Core.ScreenWidth - 32)
                w = Core.ScreenWidth - 32;

            // Get height of the 2 strings
            MessageFont.ComputeTextInRect(Message, out y, out i, w - 32 - xOffset);
            if (Icon != null && i < Icon.Height)
                i = Icon.Height;
            h = i + TitleFont.Height + 74 + (MessageFont.Height);

            // Check the height in screen
            if (h > Core.ScreenHeight - 32)
                h = Core.ScreenHeight - 32;

            // Center
            x = Core.ScreenWidth / 2 - w / 2;
            y = Core.ScreenHeight / 2 - h / 2;

            Core.ShadowRegion(x, y, w, h);
            Core.Screen.DrawRectangle(0, 0, x, y, w, 1, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 179);
            Core.Screen.DrawRectangle(0, 0, x, y + h - 1, w, 1, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 179);
            Core.Screen.DrawRectangle(0, 0, x, y + 1, 1, h - 2, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 179);
            Core.Screen.DrawRectangle(0, 0, x + w - 1, y + 1, 1, h - 2, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 179);
            Core.Screen.DrawRectangle(0, 0, x + 1, y + 1, w - 2, h - 2, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 256);
            Core.Screen.DrawRectangle(0, 0, x + 1, y + 1, w - 2, 1, 0, 0, Colors.White, 0, 0, Colors.White, 0, 0, 51);
            Core.Screen.DrawRectangle(0, 0, x + 1, y + 2, w - 2, 1, 0, 0, Colors.White, 0, 0, Colors.White, 0, 0, 26);

            // Tilebar text
            Core.Screen.DrawTextInRect(Title, x + 16 + xOffset, y + 16, w - 32 - xOffset, TitleFont.Height, Bitmap.DT_TrimmingCharacterEllipsis, Colors.CharcoalDust, TitleFont);

            // Body text
            Core.Screen.DrawTextInRect(Message, x + 16 + xOffset, y + 32 + TitleFont.Height, w - 32 - xOffset, h - 74 - TitleFont.Height, Bitmap.DT_TrimmingCharacterEllipsis + Bitmap.DT_WordWrap, Colors.CharcoalDust, MessageFont);

            // Icon
            if (Icon != null)
                Core.Screen.DrawImage(x + 16, y + 16, Icon, 0, 0, Icon.Width, Icon.Height);

            // Buttons
            x = Core.ScreenWidth / 2 - w / 2;
            y = Core.ScreenHeight / 2 - h / 2;
            switch (Type)
            {
                case PromptType.AbortContinue:
                    _btns = new Button[2];
                    _btns[1] = new Button("btnContinue", "Continue", MessageFont, 0, y + h - MessageFont.Height - 26);
                    _btns[0] = new Button("btnAbort", "Abort", MessageFont, 0, _btns[1].Y, Colors.White, Colors.White, Colors.DarkRed, Colors.DarkRed);
                    _btns[0].NormalColorTop = cR1;
                    _btns[0].NormalColorBottom = cR2;
                    _btns[0].PressedColorTop = cR2;
                    _btns[0].PressedColorBottom = cR1;
                    _btns[1].Tap += new OnTap((object sender, point e) => EndModal(PromptResult.Continue));
                    _btns[0].Tap += new OnTap((object sender, point e) => EndModal(PromptResult.Abort));

                    x = x + (w / 2) - ((_btns[1].Width + _btns[0].Width + 16) / 2);
                    _btns[0].X = x;
                    _btns[1].X = x + _btns[0].Width + 16;
                    break;
                case PromptType.AbortRetryContinue:
                    _btns = new Button[3];
                    _btns[2] = new Button("btnContinue", "Continue", MessageFont, 0, y + h - MessageFont.Height - 26);
                    _btns[1] = new Button("btnRetry", "Retry", MessageFont, 0, _btns[2].Y);
                    _btns[0] = new Button("btnAbort", "Abort", MessageFont, 0, _btns[1].Y, Colors.White, Colors.White, Colors.DarkRed, Colors.DarkRed);
                    _btns[0].NormalColorTop = cR1;
                    _btns[0].NormalColorBottom = cR2;
                    _btns[0].PressedColorTop = cR2;
                    _btns[0].PressedColorBottom = cR1;
                    _btns[2].Tap += new OnTap((object sender, point e) => EndModal(PromptResult.Continue));
                    _btns[1].Tap += new OnTap((object sender, point e) => EndModal(PromptResult.Retry));
                    _btns[0].Tap += new OnTap((object sender, point e) => EndModal(PromptResult.Abort));


                    x = x + (w / 2) - ((_btns[2].Width + _btns[1].Width + _btns[0].Width + 32) / 2);
                    _btns[0].X = x;
                    _btns[1].X = x + _btns[0].Width + 16;
                    _btns[2].X = x + _btns[0].Width + _btns[1].Width + 32;
                    break;
                case PromptType.OKCancel:
                    _btns = new Button[2];
                    _btns[1] = new Button("btnCancel", "Cancel", MessageFont, 0, y + h - MessageFont.Height - 26, Colors.White, Colors.White, Colors.DarkRed, Colors.DarkRed);
                    _btns[1].NormalColorTop = cR1;
                    _btns[1].NormalColorBottom = cR2;
                    _btns[1].PressedColorTop = cR2;
                    _btns[1].PressedColorBottom = cR1;
                    _btns[0] = new Button("btnOK", "  OK  ", MessageFont, 0, _btns[1].Y);
                    _btns[1].Tap += new OnTap((object sender, point e) => EndModal(PromptResult.Cancel));
                    _btns[0].Tap += new OnTap((object sender, point e) => EndModal(PromptResult.OK));


                    x = x + (w / 2) - ((_btns[1].Width + _btns[0].Width + 16) / 2);
                    _btns[0].X = x;
                    _btns[1].X = x + _btns[0].Width + 16;
                    break;
                case PromptType.YesNo:
                    _btns = new Button[2];
                    _btns[1] = new Button("btnNo", "  No  ", MessageFont, 0, y + h - MessageFont.Height - 26, Colors.White, Colors.White, Colors.DarkRed, Colors.DarkRed);
                    _btns[1].NormalColorTop = cR1;
                    _btns[1].NormalColorBottom = cR2;
                    _btns[1].PressedColorTop = cR2;
                    _btns[1].PressedColorBottom = cR1;
                    _btns[0] = new Button("btnYes", " Yes ", MessageFont, 0, _btns[1].Y);
                    _btns[1].Tap += new OnTap((object sender, point e) => EndModal(PromptResult.No));
                    _btns[0].Tap += new OnTap((object sender, point e) => EndModal(PromptResult.Yes));


                    x = x + (w / 2) - ((_btns[1].Width + _btns[0].Width + 16) / 2);
                    _btns[0].X = x;
                    _btns[1].X = x + _btns[0].Width + 16;
                    break;
                case PromptType.YesNoCancel:
                    _btns = new Button[3];
                    _btns[2] = new Button("btnCancel", "Cancel", MessageFont, 0, y + h - MessageFont.Height - 26, Colors.White, Colors.White, Colors.DarkRed, Colors.DarkRed);
                    _btns[2].NormalColorTop = cR1;
                    _btns[2].NormalColorBottom = cR2;
                    _btns[2].PressedColorTop = cR2;
                    _btns[2].PressedColorBottom = cR1;
                    _btns[1] = new Button("btnNo", "  No  ", MessageFont, 0, y + h - MessageFont.Height - 26);
                    _btns[0] = new Button("btnYes", " Yes ", MessageFont, 0, _btns[1].Y);
                    _btns[2].Tap += new OnTap((object sender, point e) => EndModal(PromptResult.Cancel));
                    _btns[1].Tap += new OnTap((object sender, point e) => EndModal(PromptResult.No));
                    _btns[0].Tap += new OnTap((object sender, point e) => EndModal(PromptResult.Yes));


                    x = x + (w / 2) - ((_btns[2].Width + _btns[1].Width + _btns[0].Width + 32) / 2);
                    _btns[0].X = x;
                    _btns[1].X = x + _btns[0].Width + 16;
                    _btns[2].X = x + _btns[0].Width + _btns[1].Width + 32;
                    break;
                default:
                    _btns = new Button[1];
                    _btns[0] = new Button("btnOK", "  OK  ", MessageFont, 0, y + h - MessageFont.Height - 26);
                    _btns[0].Tap += new OnTap((object sender, point e) => EndModal(PromptResult.OK));

                    _btns[0].X = x + (w / 2) - (_btns[0].Width / 2);
                    break;
            }

            // Add Buttons
            for (i = 0; i < _btns.Length; i++)
                frmModal.AddChild(_btns[i]);

            Core.Screen.Flush();

            ModalBlock();

            Core.ActiveContainer = prv;

            return _result;
        }

        public static int Show(string Title, string Message, string[] ButtonOptions, Font TitleFont, Font MessageFont, Bitmap Icon = null)
        {
            int w, h, x, y, i;
            int xOffset = 0;
            int btnsW = 16;

            // Create Buttons first this time
            _btns = new Button[ButtonOptions.Length];
            for (i = 0; i < _btns.Length; i++)
            {
                _btns[i] = new Button("btn" + i, ButtonOptions[i], MessageFont, 0, 0);
                btnsW += _btns[i].Width + 16;
            }

            Core.Screen.SetClippingRectangle(0, 0, Core.ScreenWidth, Core.ScreenHeight);

            Color cR1 = ColorUtility.ColorFromRGB(226, 92, 79);
            Color cR2 = ColorUtility.ColorFromRGB(202, 48, 53);

            IContainer prv = Core.ActiveContainer;
            Form frmModal = new Form("modalAlertForm", Colors.Ghost);
            frmModal.TouchDown += new OnTouchDown(frmModal_TouchDown);
            frmModal.TouchUp += new OnTouchUp(frmModal_TouchUp);
            Core.SilentlyActivate(frmModal);

            // Determine required size
            TitleFont.ComputeExtent(Title, out w, out h);
            size sz = FontManager.ComputeExtentEx(MessageFont, Message);
            if (sz.Width > w)
                w = sz.Width;

            if (Icon != null)
            {
                xOffset = Icon.Width + 16;
                w += xOffset;
            }

            // Adjust for button sizes
            if (w < btnsW)
                w = btnsW;

            // Add Padding
            w += 32;

            // Now make sure we're within the screen width
            if (w > Core.ScreenWidth - 32)
                w = Core.ScreenWidth - 32;

            // Get height of the 2 strings
            MessageFont.ComputeTextInRect(Message, out y, out i, w - 32 - xOffset);
            if (Icon != null && i < Icon.Height)
                i = Icon.Height;
            h = i + TitleFont.Height + 74 + (MessageFont.Height);

            // Check the height in screen
            if (h > Core.ScreenHeight - 32)
                h = Core.ScreenHeight - 32;

            // Center
            x = Core.ScreenWidth / 2 - w / 2;
            y = Core.ScreenHeight / 2 - h / 2;

            Core.ShadowRegion(x, y, w, h);
            Core.Screen.DrawRectangle(0, 0, x, y, w, 1, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 179);
            Core.Screen.DrawRectangle(0, 0, x, y + h - 1, w, 1, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 179);
            Core.Screen.DrawRectangle(0, 0, x, y + 1, 1, h - 2, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 179);
            Core.Screen.DrawRectangle(0, 0, x + w - 1, y + 1, 1, h - 2, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 179);
            Core.Screen.DrawRectangle(0, 0, x + 1, y + 1, w - 2, h - 2, 0, 0, Colors.Ghost, 0, 0, Colors.Ghost, 0, 0, 256);
            Core.Screen.DrawRectangle(0, 0, x + 1, y + 1, w - 2, 1, 0, 0, Colors.White, 0, 0, Colors.White, 0, 0, 51);
            Core.Screen.DrawRectangle(0, 0, x + 1, y + 2, w - 2, 1, 0, 0, Colors.White, 0, 0, Colors.White, 0, 0, 26);

            // Tilebar text
            Core.Screen.DrawTextInRect(Title, x + 16 + xOffset, y + 16, w - 32 - xOffset, TitleFont.Height, Bitmap.DT_TrimmingCharacterEllipsis, Colors.CharcoalDust, TitleFont);

            // Body text
            Core.Screen.DrawTextInRect(Message, x + 16 + xOffset, y + 32 + TitleFont.Height, w - 32 - xOffset, h - 74 - TitleFont.Height, Bitmap.DT_TrimmingCharacterEllipsis + Bitmap.DT_WordWrap, Colors.CharcoalDust, MessageFont);

            // Icon
            if (Icon != null)
                Core.Screen.DrawImage(x + 16, y + 16, Icon, 0, 0, Icon.Width, Icon.Height);

            // Position Buttons
            x = x + (w / 2) - (btnsW / 2);
            for (i = 0; i < _btns.Length; i++)
            {
                _btns[i].Tag = i;
                _btns[i].Y = y + h - 16 - _btns[i].Height;
                _btns[i].X = x;
                x += _btns[i].Width + 16;
                _btns[i].Tap += new OnTap((object sender, point e) => EndModal((Button)sender));
                frmModal.AddChild(_btns[i]);
            }

            Core.Screen.Flush();

            ModalBlock();

            Core.ActiveContainer = prv;

            return _iResult;
        }

        #endregion

        #region Private Methods

        private static void EndModal(PromptResult Result)
        {
            _result = Result;
            _activeBlock.Set();
        }

        private static void EndModal(Button Result)
        {
            _iResult = (int)Result.Tag;
            _activeBlock.Set();
        }

        private static void frmModal_TouchDown(object sender, point e)
        {
            for (int i = _btns.Length - 1; i >= 0; i--)
            {
                if (_btns[i].Visible && _btns[i].HitTest(e))
                {
                    _btns[i].SendTouchDown(sender, new point(e.X - _btns[i].Left, e.Y - _btns[i].Top));
                    return;
                }
            }
        }

        private static void frmModal_TouchUp(object sender, point e)
        {
            for (int i = _btns.Length - 1; i >= 0; i--)
                _btns[i].SendTouchUp(sender, new point(e.X - _btns[i].Left, e.Y - _btns[i].Top));

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

        #endregion

    }
}
