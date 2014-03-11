using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.IO;
using Microsoft.SPOT.Net;
using Microsoft.SPOT.Presentation.Media;

using GHI.Premium.Hardware;
using GHI.Premium.IO;
using GHI.Premium.Net;
using GHI.Premium.SQLite;
using GHI.Premium.System;
using GHI.Premium.USBHost;

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

using Skewworks.VDS.Tinkr;

using Skewworks.NETMF.Applications;

using Button = Skewworks.NETMF.Controls.Button;

namespace TestProject
{
    public class Program
    {

        #region Variables

        static readonly string hex = "0123456789ABCDEF";
        private static bool _bCancel;

        static Music _music;

        // File System
        static string _root;
        static PersistentStorage _hdd;
        static string _curDir;

        // USB
        static USBH_Keyboard _keyboard;
        static USBH_Mouse _mouse;

        // WiFi
        static WiFiRS9110 _wifi;
        static WiFiNetworkInfo[] _scanResp;

        // SQLite
        static Database _db;

        #endregion

        public static void Main()
        {
            // Initialize Tinkr
            Core.Initialize(TouchCollection.NativeSingleTouch);

            // Subscribe to events
            Core.Instance.ApplicationClosed += (object sender, ApplicationDetails appDetails) => new Thread(Testbed).Start();

            // Launch
            new Thread(Testbed).Start();
        }

        static void Testbed()
        {
            // Create the main form
            Form frmMain = new Form("frmMain");

            // Add a button
            Button btn1 = new Button("btn1", "Launch Sample App", Fonts.Droid16, 4, 4);
            //btn1.Tap += (object sender, point e) => new Thread(LaunchApp).Start();
            frmMain.AddChild(btn1);

            // Activate the form
            Core.ActiveContainer = frmMain;
        }




        /// <summary>
        /// Calling this method helps ensure Garbage Collection kicks in
        /// If we didn't do this we'd run out of memory if we switch forms rapidly
        /// You could also use static forms so they weren't created and destroyed everytime.
        /// Doing so would eliminated the need for this method
        /// </summary>
        static void CleanSwitch()
        {
            // See if we already have a form active
            if (Core.ActiveContainer != null)
            {
                // Remove the active form
                Core.ActiveContainer = null;

                // Now force GC to get rid of the form
                Debug.GC(true);
            }
        }



        static void start_Tap(object sender, point e)
        {
            new Thread(filedlg).Start();
        }

        static void filedlg()
        {
            string _file = FileDialog.OpenFile(Fonts.Droid11, Fonts.Droid8);
        }


        #region USB

        static void EnableUSB()
        {
            USBHostController.DeviceConnectedEvent += DeviceConnectedEvent;
            USBHostController.DeviceDisconnectedEvent += DeviceDisconnectedEvent;
        }

        static void DeviceConnectedEvent(USBH_Device device)
        {
            if (device.TYPE == USBH_DeviceType.Keyboard)
            {
                _keyboard = new USBH_Keyboard(device);
                _keyboard.KeyDown += _keyboard_KeyDown;
                _keyboard.KeyUp += _keyboard_KeyUp;
                Debug.Print("Keyboard attached");
            }
            else if (device.TYPE == USBH_DeviceType.Mouse)
            {
                _mouse = new USBH_Mouse(device);
                _mouse.SetCursorBounds(0, Core.ScreenWidth, 0, Core.ScreenHeight);
                _mouse.SetCursor(0, 0);
                _mouse.Scale(20);
                _mouse.MouseMove += _mouse_MouseMove;
                _mouse.MouseDown += _mouse_MouseDown;
                _mouse.MouseUp += _mouse_MouseUp;
                Core.MouseAvailable = true;
                Debug.Print("Mouse attached");
            }
        }

        static void DeviceDisconnectedEvent(USBH_Device device)
        {
            if (device.TYPE == USBH_DeviceType.Keyboard)
            {
                _keyboard.KeyDown -= _keyboard_KeyDown;
                _keyboard.KeyUp -= _keyboard_KeyUp;
                _keyboard = null;
            }
            else if (device.TYPE == USBH_DeviceType.Mouse)
            {
                _mouse.MouseMove -= _mouse_MouseMove;
                _mouse = null;
                Core.MouseAvailable = false;
            }
        }

        static void _mouse_MouseDown(USBH_Mouse sender, USBH_MouseEventArgs args)
        {
            switch (args.ChangedButton)
            {
                case USBH_MouseButton.Left:
                    Core.RaiseButtonEvent((int)ButtonIDs.Click, true);
                    break;
                case USBH_MouseButton.Middle:
                    Core.RaiseButtonEvent((int)ButtonIDs.ClickMiddle, true);
                    break;
                case USBH_MouseButton.Right:
                    Core.RaiseButtonEvent((int)ButtonIDs.ClickRight, true);
                    break;
            }
        }

        static void _mouse_MouseMove(USBH_Mouse sender, USBH_MouseEventArgs args)
        {
            Core.MousePosition = new point(_mouse.Cursor.X, _mouse.Cursor.Y);
            if (_mouse.LeftButton == USBH_MouseButtonState.Pressed)
                Core.RaiseTouchEvent(TouchType.TouchMove, Core.MousePosition);
        }

        static void _mouse_MouseUp(USBH_Mouse sender, USBH_MouseEventArgs args)
        {
            switch (args.ChangedButton)
            {
                case USBH_MouseButton.Left:
                    Core.RaiseButtonEvent((int)ButtonIDs.Click, false);
                    break;
                case USBH_MouseButton.Middle:
                    Core.RaiseButtonEvent((int)ButtonIDs.ClickMiddle, false);
                    break;
                case USBH_MouseButton.Right:
                    Core.RaiseButtonEvent((int)ButtonIDs.ClickRight, false);
                    break;
            }
        }

        static void _keyboard_KeyDown(USBH_Keyboard sender, USBH_KeyboardEventArgs args)
        {
            if (args.KeyAscii == 0)
                Core.RaiseKeyboardAltKeyEvent((int)args.Key, true);
            else
                Core.RaiseKeyboardKeyEvent(args.KeyAscii, true);
        }

        static void _keyboard_KeyUp(USBH_Keyboard sender, USBH_KeyboardEventArgs args)
        {
            if (args.KeyAscii == 0)
                Core.RaiseKeyboardAltKeyEvent((int)args.Key, false);
            else
                Core.RaiseKeyboardKeyEvent(args.KeyAscii, false);
        }

        #endregion

        #region Hardware

        static void EnableSD()
        {
            _hdd = new PersistentStorage("SD");
            _hdd.MountFileSystem();

            _root = "\\SD\\";
            _curDir = "\\SD\\";
        }

        static void EnableSound()
        {
            _music = new Music();
            _music.SetVolume(0);
        }

        #endregion

    }

}



