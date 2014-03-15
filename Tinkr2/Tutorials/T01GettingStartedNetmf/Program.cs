using System;
using Microsoft.SPOT;

// Step 1: Add needed namespaces
using System.Threading;
using Microsoft.SPOT.Hardware;

using Skewworks.NETMF;
using Skewworks.NETMF.Controls;
using Skewworks.NETMF.Resources;
using Skewworks.NETMF.Modal;

// Step 1.1: Since there is also class called Button in the namespace Microsoft.SPOT.Hardware, we define the Tinkr Button explicitly
using Button = Skewworks.NETMF.Controls.Button;

namespace T01GettingStartedNetmf
{
   public class Program
   {
      public static void Main()
      {
         // Step 2: Initialize Tinkr
         Core.Initialize(TouchCollection.NativeSingleTouch);

         // Step 3: Calibrate touch screen if necessary
         CalibrateTouch();

         // Step 4: Create and show a simple form
         SetupMainForm();
      }

      private static void CalibrateTouch()
      {
         // Step 3.1: Check if LCD is already calibrated
         if (SettingsManager.LCD == null)
         {
            // No calibration found
            
            // Step 3.2: calibrate now
            SettingsManager.CalibrateTouch(new CalibrationConfiguration());

            // Step 3.3: Wait for settings to save
            //           Calibration is saved using ExtendedWeakReference (EWR) into the onborad flash memory
            Thread.Sleep(100);

            // Step 3.4: Restart device
            PowerState.RebootDevice(true);
         }
         else
         {
            // calibration found -> restore it

            // Step 3.5: Restore existing calibration
            SettingsManager.RestoreLCDCalibration();
         }
      }

      private static void SetupMainForm()
      {
         // Step 4.1: Create the main form
         //             "mainForm":      Every control in Tinkr has a name
         //             Colors.DarkGray: Every Form has also a background color
         //           The width and height of a form is automatically set to screen size
         var frm = new Form("mainForm", Colors.DarkGray);

         // Step 4.2: Create a simple label
         //             "label":       The name of the label
         //             "Tutorial...": Text of the label
         //             Fonts.Droid12Bold: The font used to render the label
         //             10, 10:        The top left x and y coordinates of the label
         //           The size of the label is automatically calculated acoording to the content when using this constructor
         var label = new Label("label", "Tutorial 01: Getting started (NETMF)", Fonts.Droid12Bold, 10, 10);

         // Step 4.3: Add the label to the form
         //           Every control needs to be added to the form.
         //           Every control derived from Container can hold controls
         //           These contols are rendered recursively
         frm.AddChild(label);

         // Step 4.4: Create a button
         //             "button":      The name of the button
         //             "Show prompt": The content text of the button
         //             Fonts.Droid11: The font used to render the content text
         //             10, 40:        The top left x and y coordinates of the button
         //           The size of the button is automatically calculated according to the content when using this constructor
         var button = new Button("button", "Show prompt", Fonts.Droid11, 10, 40);

         // Step 4.5: Add a tap handler to the button
         //           The tap event is like the click event in a desktop application.
         //           But since NETMF devices usually have a touch screen it's called tap.
         button.Tap += ButtonOnTap;

         // Step 4.6: Add the button to the form
         frm.AddChild(button);

         // Step 4.7: Set the form as the active container.
         //           The active container is rendered and all touch events are directed to it.
         Core.ActiveContainer = frm;
      }

      private static void ButtonOnTap(object sender, point point1)
      {
         // Step 5: Open a prompt (MessageBox) when the button is taped

         // Step 5.1: Spawn a new thread for the prompt
         //           Because new touch events can only be processed when the last one (like this tap) has finished,
         //           we need to show the prompt asynchrony.
         //           To do so we start a new thread that does nothing else than displaying the prompt.
         //           If the prompt is shown directly from a touch event handler, the buttons in the prompt would not respond!
         new Thread(() =>
         {
            // Step 5.1: Show the prompt
            //           A prompt is a modal window, that is rendered above the ActiveContainer 
            //           and will receive all touch events as long as it's open.
            //             "My 1st prompt": Title of the prompt
            //             "Proudly pr...": Message of the prompt
            //             Fonts.Droid9:    The font used to render the title of the prompt
            //             Fonts.Droid11:   The font used to render the message of the prompt
            Prompt.Show("My 1st prompt", "Proudly presented by Tinkr", Fonts.Droid9, Fonts.Droid11);

         }).Start(); // don't forget to start the thread!
      }
   }
}
