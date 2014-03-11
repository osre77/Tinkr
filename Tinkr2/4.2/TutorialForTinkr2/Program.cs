using System;
using System.IO;
using System.Text;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using Skewworks.Gadgeteer.CP7Helper;

using Skewworks.NETMF;
using Skewworks.NETMF.Applications;
using Skewworks.NETMF.Controls;
using Skewworks.NETMF.Graphics;
using Skewworks.NETMF.Modal;
using Skewworks.NETMF.Resources;

using Skewworks.Tinkr;
using Skewworks.Tinkr.Controls;
using Skewworks.Tinkr.Modal;

using Skewworks.VDS;
using Skewworks.VDS.Tinkr;

using Skewworks.NETMF.Tinkr2.Tutorial.Custom_Controls;

using Button = Skewworks.NETMF.Controls.Button;

namespace Skewworks.NETMF.Tinkr2.Tutorial
{
    public class Program
    {

        // Tutorial application for Tinkr 2.x
        // 1) Works with emulator and physical devices
        // 2) Native touch assumed for physical devices
        // 3) Emulator usage assumes default Microsoft(r) Emulator

        /// <summary>
        /// Application entry point
        /// </summary>
        public static void Main()
        {
            // Initialize the core
            Core.Initialize(TouchCollection.NativeSingleTouch, Resources.GetBitmap(Resources.BitmapResources.logo));

            // Calibrate or Restore touch
            if (SettingsManager.LCD == null)
                SettingsManager.CalibrateTouch(new CalibrationConfiguration());
            else
                SettingsManager.RestoreLCDCalibration();

            // Start Demo
            DemoHome();

            // Don't let application exit
            // Only required for non-native touch
            Thread.Sleep(-1);
        }

        #region Demo Home Page

        static void DemoHome()
        {
            // Create a form, default background color
            Form frmHome = new Form("frmHome");

            // Create a list of demo application
            string[] strDemo = new string[] {
                                "Working with touch"
                                };

            // Sort the list
            strDemo = new StringSorter(strDemo).InsensitiveSort();

            // Create a label
            Label lblTitle = new Label("lblTitle", "Select a demo", Fonts.Droid11Bold, 4, 4);

            // Add label to form
            frmHome.AddChild(lblTitle);

            // Create a button
            Button btnGo = new Button("btnGo", "View Demo", Fonts.Droid11, 0, 0);

            // Button is automatically sized when created
            // We'll use that to position the button
            btnGo.X = frmHome.Width - btnGo.Width - 4;
            btnGo.Y = frmHome.Height - btnGo.Height - 4;

            // Add button to form
            frmHome.AddChild(btnGo);

            // Create a listbox
            Listbox lstDemos = new Listbox("lstDemos", Fonts.Droid12, 4, lblTitle.Y + lblTitle.Height + 4, frmHome.Width - 8, frmHome.Height - btnGo.Height - lblTitle.Height - 16);
            
            // Add demos to listbox
            for (int i = 0; i < strDemo.Length; i++)
                lstDemos.AddItem(new ListboxItem(strDemo[i]));

            // Add listbox to form
            frmHome.AddChild(lstDemos);

            // Bind btnGo's tap event
            btnGo.Tap += (object sender, point e) => new Thread(LaunchDemo).Start();

            // Activate the form
            Core.ActiveContainer = frmHome;
        }

        static void LaunchDemo()
        {
            // Get selected demo
            switch (((Listbox)Core.ActiveContainer.GetChildByName("lstDemos")).SelectedItem.Text)
            {
                case "Working with touch":
                break;
            }
        }

        #endregion

    }
}
