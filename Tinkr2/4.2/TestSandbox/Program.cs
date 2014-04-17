using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Skewworks.NETMF;
using Skewworks.NETMF.Controls;
using Skewworks.NETMF.Resources;
using TestSandbox.CommonControls;

namespace TestSandbox
{
   public class Program
   {
      public static void Main()
      {
         Core.Initialize(TouchCollection.NativeSingleTouch);

         CalibrateTouch();

         InitMainForm();

         ShowMainForm();
         //Thread.Sleep(-1);
      }

      private static void CalibrateTouch()
      {
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
      }

      public static Form MainForm { get; private set; }

      private static void InitMainForm()
      {
         MainForm = new Form("MainForm", Colors.DarkGray);

         MainForm.AddMenuStrip(
            MenuHelper.CreateMenuItem("Common-1",
               MenuHelper.CreateMenuItem("Labels", (sender, point) => LabelsForm.Show()),
               MenuHelper.CreateMenuItem("Buttons", (sender, point) => ButtonsForm.Show())));
      }

      public static void ShowMainForm()
      {
         Core.ActiveContainer = MainForm;
      }
   }
}
