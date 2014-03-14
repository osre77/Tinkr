using System;
using System.IO;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.IO;

using Skewworks.NETMF;
using Skewworks.NETMF.Controls;
using Skewworks.NETMF.Modal;
using Skewworks.NETMF.Resources;

namespace Clix2Demo
{
    public class Program
    {

        /// <summary>
        /// Program entry point
        /// </summary>
        public static void Main()
        {
            // Initialize core with splash image and new background color
            Core.Initialize(TouchCollection.NativeSingleTouch, Resources.GetBitmap(Resources.BitmapResources.netmf_skewworks_logo), Colors.White);

            // Touch calibration
            if (SettingsManager.LCD == null)
            {
                // No calibration found; calibrate now
                SettingsManager.CalibrateTouch(new CalibrationConfiguration());

                // Wait for settings to save
                Thread.Sleep(100);

                // Restart device
                PowerState.RebootDevice(true);
            }
            else
            {
                // Restore existing calibration
                SettingsManager.RestoreLCDCalibration();
            }

            DemoSelect();


            VolumeInfo[] vi = VolumeInfo.GetVolumes();
            for (int i = 0; i < vi.Length; i++)
            {
                if (!vi[i].IsFormatted)
                    vi[i].Format(0);
            }

            string[] dirs = new string[] { "dell", "Drivers", "inetpub", "Intel", "Keil", "PerfLogs", "Program Files", "Skewworks", "Temp", "Users" };
            for (int i = 0; i < dirs.Length; i++)
            {
                if (!Directory.Exists("\\Root\\" + dirs[i]))
                    Directory.CreateDirectory("\\Root\\" + dirs[i]);
            }

            string[] files = new string[] { "banner2.jpg", "Clix.docx", "Clix.pdf", "EULA.rtf", "welcome2.jpg" };
            int[] bytes = new int[] { 9, 673, 720, 39, 121, 53, 53, 17 };
            byte[] b;
            for (int i = 0; i < files.Length; i++)
            {
                if (!File.Exists("\\Root\\" + files[i]))
                {
                    b = new byte[bytes[i] * 1024];
                    FileStream fs = new FileStream("\\Root\\" + files[i], FileMode.CreateNew);
                    fs.Write(b, 0, b.Length);
                    fs.Close();
                    b = null;
                    Debug.GC(true);
                }
            }

            FileDialog.OpenFile("Open File", Fonts.Droid16, Fonts.Droid12);

        }

        #region Demo Selection

        /// <summary>
        /// Display demo selection screen
        /// </summary>
        private static void DemoSelect()
        {
            // Create form
            Form frm = new Form("frm", Colors.DarkGray);

            // Create menu
            MenuStrip ms = new MenuStrip("ms", Fonts.Droid9, 0, 0, frm.Width, 24);

            // Add menu items
            MenuItem miView = new MenuItem("miView", "View");
            ms.AddMenuItem(miView);

            // "View" sub items
            MenuItem miBasic = new MenuItem("miBasic", "Basic Controls");
            miBasic.Tap += new OnTap(miBasic_Tap);
            miView.AddMenuItem(miBasic);

            MenuItem miCollection = new MenuItem("miCollection", "Collection Controls");
            miView.AddMenuItem(miCollection);

            MenuItem miInput = new MenuItem("miInput", "Input Controls");
            miView.AddMenuItem(miInput);

            MenuItem miDialogs = new MenuItem("miDialogs", "Dialogs");
            miView.AddMenuItem(miDialogs);

            // "Dialogs" sub items
            MenuItem midPrompt = new MenuItem("midPrompt", "Prompt Dialog");
            miDialogs.AddMenuItem(midPrompt);

            MenuItem midSel = new MenuItem("midSel", "Selection Dialog");
            miDialogs.AddMenuItem(midSel);

            MenuItem midOpen = new MenuItem("midOpen", "Open File Dialog");
            miDialogs.AddMenuItem(midOpen);

            MenuItem midSave = new MenuItem("midSave", "Save File Dialog");
            miDialogs.AddMenuItem(midSave);
            
            // Add menu to form
            frm.AddChild(ms);

            // Activate form
            Core.ActiveContainer = frm;
        }

        #endregion

        #region Basic Controls

        static void miBasic_Tap(object sender, point e)
        {
            // Create form
            Form frm = new Form("frm");

            /*
            frm.AddChild(new Label("This is a label", 4, 4));
            frm.AddChild(new RichTextLabel("This is a <b>Rich<i>Text</i><color '0,0,255'>Label</color></b>; it supports changing of fonts and colors.", 4, 24, frm.Width - 8, 34));

            Panel pnl = new Panel(192, 63, 100, 100, Colors.Wheat);
            pnl.AddChild(new Label("This label is inside a panel! And to the left is a picturebox.", 0, 0, 100, 100));
            frm.AddChild(pnl);

            frm.AddChild(new Picturebox(Resources.GetBitmap(Resources.BitmapResources.fblogo), 4, 63));

            CommandButton cmdBack = new CommandButton("< Go Back", 4, frmMain.Height - 29, 95, 25);
            cmdBack.Tap += new OnTap((object sender, point e) => ShowMainForm());
            frm.AddChild(cmdBack);
            */

            frm.AddChild(new Label("lbl", "This is a label", Fonts.Droid11, 4, 4));
            frm.AddChild(new RichTextLabel("rtl", "This is a <b>Rich<i>Text</i><color '0,0,255'>Label</color></b>; it supports changing of fonts and colors.", Fonts.Droid11, 4, 24, frm.Width - 8, 34));

            // Activate form
            Core.ActiveContainer = frm;
        }

        #endregion

    }
}
