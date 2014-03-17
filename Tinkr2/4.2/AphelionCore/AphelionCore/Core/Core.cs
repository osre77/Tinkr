using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.IO;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Skewworks.NETMF.Applications;
using Skewworks.NETMF.Controls;

// ReSharper disable StringLastIndexOfIsCultureSpecific.1
namespace Skewworks.NETMF
{
   /// <summary>
   /// Default entry point to Skewworks Core.
   /// </summary>
   /// <remarks>Singleton object, compatible with multiple AppDomain applications.</remarks>
   [Serializable]
   public class Core : MarshalByRefObject
   {
      #region Variables

      // Singleton
      private static Core _instance;
      private static readonly object SyncRoot = new Object();

      // Screen
      private readonly Bitmap _screen;
      private readonly int _sW;
      private readonly int _sH;
      private readonly int _sBpp;
      private readonly int _sRot;

      // Touch
      // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
      private readonly SafeTouchConnection _safetouch;
      private readonly Thread _myTouch;
      // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

      // Controls
      private IContainer _active;
      private readonly SystemColors _colors;
      private bool _useVkb;
      private bool _flatChk;
      private bool _flatRdo;
      private bool _flatTxt;

      // Mouse
      private readonly Bitmap _mouse;
      private readonly Bitmap _mouseBuffer;
      private bool _showMouse;
      private int _mouseX;
      private int _mouseY;
      private bool _mouseAvail;

      // Keyboard
      private bool _shift, _ctrl, _alt;

      // Messaging
      private static readonly MessageClient Mc = new MessageClient();
      private MessageClient[] _mcs;

      // Overlay
      private Overlay _overlay;
      private Thread _monOv;
      private Bitmap _overlayBuffer;

      // Clipboard
      private object _clipboard;

      // Applications
      private int _nextDomain;
      private IApplicationLauncher[] _appRefs;
      
      //private AppDomain _target;
      
      private IContainer _defaultReturn;

      #endregion

      #region Constructor

      /// <summary>
      /// Prevents public instancing to preserve singleton
      /// </summary>
      private Core(TouchCollection touchCollection, Bitmap splash, Color backColor)
      {
         if (AppDomain.CurrentDomain.FriendlyName != "default")
            return;

         // Create screen
         HardwareProvider.HwProvider.GetLCDMetrics(out _sW, out _sH, out _sBpp, out _sRot);
         _screen = new Bitmap(_sW, _sH);
         _screen.DrawRectangle(0, 0, 0, 0, _sW, _sH, 0, 0, backColor, 0, 0, backColor, 0, 0, 256);
         if (splash != null)
            _screen.DrawImage(_screen.Width / 2 - splash.Width / 2, _screen.Height / 2 - splash.Height / 2, splash, 0, 0, splash.Width, splash.Height);
         _screen.Flush();

         // Touch Collection
         if (touchCollection == TouchCollection.NativeSingleTouch)
         {
            _safetouch = new SafeTouchConnection();
            TouchCollectorConfiguration.CollectionMode = CollectionMode.InkOnly;
            TouchCollectorConfiguration.CollectionMethod = CollectionMethod.Native;
            Touch.Initialize(_safetouch);

            _myTouch = new Thread(MyTouch_Tick)
            {
               Priority = ThreadPriority.AboveNormal
            };
            _myTouch.Start();
         }

         // Controls
         _colors = new SystemColors();
         _useVkb = true;

         // Mouse
         _mouse = Resources.GetBitmap(Resources.BitmapResources.arrow);
         _mouseBuffer = new Bitmap(_mouse.Width, _mouse.Height);
         _mouseX = _sW / 2;
         _mouseY = _sH / 2;

         // Messaging
         AddMessageClient(Mc);
      }

      #endregion

      #region Events

      /// <summary>
      /// This event is raised when the application is closed.
      /// </summary>
      public event OnApplicationClosed ApplicationClosed;

      /// <summary>
      /// Raises the ApplicationClosed event.
      /// </summary>
      /// <param name="sender">Sender object of the event.</param>
      /// <param name="appDetails"><see cref="ApplicationDetails"/> of the application.</param>
      protected virtual void OnApplicationClosed(object sender, ApplicationDetails appDetails)
      {
         if (ApplicationClosed != null)
         {
            ApplicationClosed(sender, appDetails);
         }
      }

      /// <summary>
      /// This event is raised when the application is launched.
      /// </summary>
      public event OnApplicationLaunched ApplicationLaunched;

      /// <summary>
      /// Raises the ApplicationLaunched event.
      /// </summary>
      /// <param name="sender">Sender object of the event.</param>
      /// <param name="threadId">Thread id of the application.</param>
      protected virtual void OnApplicationLaunched(object sender, Guid threadId)
      {
         if (ApplicationLaunched != null)
         {
            ApplicationLaunched(sender, threadId);
         }
      }

      /// <summary>
      /// This event is raised whenever a touch event of any type occurs
      /// </summary>
      public event OnTouchEvent TouchEvent;

      /// <summary>
      /// Raises the TouchEvent
      /// </summary>
      /// <param name="sender">Sender object of the event</param>
      /// <param name="touchType"><see cref="TouchType"/> of the event</param>
      /// <param name="point">Point of the touch event</param>
      protected virtual void OnTouchEvent(object sender, TouchType touchType, point point)
      {
         if (TouchEvent != null)
         {
            TouchEvent(sender, touchType, point);
         }
      }

      /// <summary>
      /// This event is raised whenever an alternate key is pressed on an attached keyboard.
      /// </summary>
      public event OnKeyboardAltKeyEvent KeyboardAltKeyEvent;

      /// <summary>
      /// Raises the KeyboardAltKeyEvent.
      /// </summary>
      /// <param name="key">Key code</param>
      /// <param name="pressed">true if pressed; false if released.</param>
      protected virtual void OnKeyboardAltKeyEvent(int key, bool pressed)
      {
         if (KeyboardAltKeyEvent != null)
         {
            KeyboardAltKeyEvent(key, pressed);
         }
      }

      /// <summary>
      /// This event is raised whenever an normal key is pressed on an attached keyboard.
      /// </summary>
      public event OnKeyboardKeyEvent KeyboardKeyEvent;

      /// <summary>
      /// Raises the KeyboardKeyEvent event.
      /// </summary>
      /// <param name="key">Key code</param>
      /// <param name="pressed">true if pressed; false if released.</param>
      protected virtual void OnKeyboardKeyEvent(char key, bool pressed)
      {
         if (KeyboardKeyEvent != null)
            KeyboardKeyEvent(key, pressed);
      }

      #endregion

      #region Properties

      /// <summary>
      /// Gets the number of active applications.
      /// </summary>
      public static int ActiveApplications
      {
         get
         {
            if (_instance._appRefs == null)
               return 0;
            return _instance._appRefs.Length;
         }
      }

      /// <summary>
      /// Gets/Sets currently active container
      /// </summary>
      /// <remarks>
      /// Only the active container will be rendered.
      /// </remarks>
      public static IContainer ActiveContainer
      {
         get { return _instance._active; }
         set
         {
            if (_instance._active == value)
            {
               return;
            }

            if (_instance._monOv != null)
            {
               _instance._monOv.Suspend();
            }

            // Deactivate current value
            if (_instance._active != null)
            {
               _instance._active.Blur();
            }

            // Update value
            _instance._active = value;

            // Activate new value & render
            if (_instance._active != null)
            {
               _instance._active.Activate();
               _instance._active.Render(true);
            }

            if (_instance._monOv != null)
            {
               _instance._monOv.Resume();
            }

            if (AppDomain.CurrentDomain.FriendlyName == "default")
            {
               _instance._defaultReturn = value;
            }
         }
      }

      /// <summary>
      /// Gets a list of currently running Skewworks Application ids
      /// </summary>
      public static Guid[] ApplicationThreadIds
      {
         get
         {
            if (_instance._appRefs == null)
            {
               return null;
            }

            var g = new Guid[_instance._appRefs.Length];
            for (int i = 0; i < g.Length; i++)
            {
               g[i] = _instance._appRefs[i].ThreadId;
            }

            return g;
         }
      }

      /// <summary>
      /// Gets/Sets clipboard value.
      /// </summary>
      public static object Clipboard
      {
         get { return _instance._clipboard; }
         set { _instance._clipboard = value; }
      }

      /// <summary>
      /// Singleton reference to Aphelion Core
      /// </summary>
      protected internal static Core Host
      {
         set
         {
            _instance = value;
         }
      }

      /// <summary>
      /// Gets/Sets flat check box state
      /// </summary>
      /// <remarks>
      /// When true check boxes are rendered in flat mode.
      /// </remarks>
      public static bool FlatCheckboxes
      {
         get { return _instance._flatChk; }
         set
         {
            if (_instance._flatChk == value)
               return;

            _instance._flatChk = value;

            if (_instance._active != null)
            {
               _instance._active.Invalidate();
            }
         }
      }

      /// <summary>
      /// Gets/Sets flat radio button state
      /// </summary>
      /// <remarks>
      /// When true radio buttons are rendered in flat mode.
      /// </remarks>
      public static bool FlatRadios
      {
         get { return _instance._flatRdo; }
         set
         {
            if (_instance._flatRdo == value)
               return;

            _instance._flatRdo = value;

            if (_instance._active != null)
            {
               _instance._active.Invalidate();
            }
         }
      }

      /// <summary>
      /// Gets/Sets flat text box state
      /// </summary>
      /// <remarks>
      /// When true text boxes are rendered in flat mode
      /// </remarks>
      public static bool FlatTextboxes
      {
         get { return _instance._flatTxt; }
         set
         {
            if (_instance._flatTxt == value)
            {
               return;
            }

            _instance._flatTxt = value;

            if (_instance._active != null)
            {
               _instance._active.Invalidate();
            }
         }
      }

      /// <summary>
      /// Sets all Flat... properties to the same value.
      /// </summary>
      /// <param name="flat">true for flat rendering state</param>
      /// <remarks>
      /// Sets the <see cref="FlatCheckboxes"/>, <see cref="FlatRadios"/> and <see cref="FlatTextboxes"/> properties to the same value.
      /// </remarks>
      public void SetAllFlatStates(bool flat)
      {
         _instance._flatChk = flat;
         _instance._flatRdo = flat;
         _instance._flatTxt = flat;

         if (_instance._active != null)
         {
            _instance._active.Invalidate();
         }
      }

      /// <summary>
      /// Gets if the Tinkr core is already initialized.
      /// </summary>
      public bool Initialized
      {
         get { return _instance != null; }
      }

      /// <summary>
      /// Gets the singleton instance of the Tinkr core, or null if not initialized.
      /// </summary>
      public static Core Instance
      {
         get { return _instance; }
      }

      /// <summary>
      /// Gets if the alt key on the keyboard is down.
      /// </summary>
      public static bool KeyboardAltDown
      {
         get { return _instance._alt; }
      }

      /// <summary>
      /// Gets if the Ctrl key on the keyboard is down.
      /// </summary>
      public static bool KeyboardCtrlDown
      {
         get { return _instance._ctrl; }
      }

      /// <summary>
      /// Gets if the shift key on the keyboard is down.
      /// </summary>
      public static bool KeyboardShiftDown
      {
         get { return _instance._shift; }
      }

      /// <summary>
      /// Gets the <see cref="MessageClient"/> for current AppDomain.
      /// </summary>
      public static MessageClient MessageClient
      {
         get { return Mc; }
      }

      /// <summary>
      /// Gets/Sets mouse availability.
      /// </summary>
      public static bool MouseAvailable
      {
         get { return _instance._mouseAvail; }
         set
         {
            if (_instance._mouseAvail == value)
            {
               return;
            }
            _instance._mouseAvail = value;
            if (_instance._showMouse)
            {
               SafeFlush();
            }
         }
      }

      /// <summary>
      /// Gets/Sets current mouse position.
      /// </summary>
      public static point MousePosition
      {
         get { return new point(_instance._mouseX, _instance._mouseY); }
         set
         {
            if (value.X < 0)
            {
               value.X = 0;
            }
            if (value.Y < 0)
            {
               value.Y = 0;
            }
            if (value.X > _instance._screen.Width)
            {
               value.X = _instance._screen.Width;
            }
            if (value.Y > _instance._screen.Height)
            {
               value.Y = _instance._screen.Height;
            }

            if (!_instance._mouseAvail || !_instance._showMouse ||
                (_instance._mouseX == value.X && _instance._mouseY == value.Y))
            {
               return;
            }

            int rx = System.Math.Min(value.X, _instance._mouseX);
            int ry = System.Math.Min(value.Y, _instance._mouseY);
            int rw = System.Math.Abs(value.X - _instance._mouseX) + 12;
            int rh = System.Math.Abs(value.Y - _instance._mouseY) + 19;

            // Restore previous location
            _instance._screen.DrawImage(_instance._mouseX, _instance._mouseY, _instance._mouseBuffer, 0, 0, 12, 19);

            // Update location
            _instance._mouseX = value.X;
            _instance._mouseY = value.Y;

            if (_instance._showMouse)
            {
               if (rx < 0)
               {
                  rx = 0;
               }
               if (ry < 0)
               {
                  ry = 0;
               }
               if (rw > _instance._sW)
               {
                  rw = _instance._sW;
               }
               if (rh > _instance._sH)
               {
                  rh = _instance._sH;
               }

               _instance._screen.SetClippingRectangle(rx, ry, rw, rh);

               // Update buffer
               _instance._mouseBuffer.DrawImage(0, 0, _instance._screen, value.X, value.Y, 12, 19);
               _instance._screen.DrawImage(value.X, value.Y, _instance._mouse, 0, 0, 12, 19);
               _instance._screen.Flush(rx, ry, rw, rh);

               _instance._screen.SetClippingRectangle(0, 0, _instance._screen.Width, _instance._screen.Height);
            }
         }
      }

      /// <summary>
      /// Gets if the premium features are available.
      /// </summary>
      /// <remarks>
      /// Since Tinkr is open source now, this property has no use anymore and will be removed in a future version.
      /// </remarks>
      [Obsolete("Property will be removed in a future release")]
      public static bool PremimumFeaturesAvailable
      {
         get { return true; }
      }

      /// <summary>
      /// Gets the screen buffer bitmap.
      /// </summary>
      public static Bitmap Screen
      {
         get { return _instance._screen; }
      }

      /// <summary>
      /// Gets the BPP for the screen.
      /// </summary>
      public static int ScreenBitsPerPixel
      {
         get { return _instance._sBpp; }
      }

      /// <summary>
      /// Gets the width of the screen.
      /// </summary>
      public static int ScreenWidth
      {
         get { return _instance._sW; }
      }

      /// <summary>
      /// Gets the height of the screen.
      /// </summary>
      public static int ScreenHeight
      {
         get { return _instance._sH; }
      }

      /// <summary>
      /// Gets/Sets current screen overlay.
      /// </summary>
      public static Overlay ScreenOverlay
      {
         get { return _instance._overlay; }
         set
         {
            if (_instance._overlay == value)
            {
               return;
            }

            bool bWait = false;
            if (_instance._overlay != null)
            {
               bWait = true;
               while (_instance._overlay != null)
               {
                  Thread.Sleep(100);
               }
            }

            if (bWait)
            {
               Thread.Sleep(1000);
            }

            if (_instance._monOv != null)
            {
               _instance._monOv.Abort();
            }

            _instance._overlay = value;

            _instance._monOv = new Thread(_instance.MonitorOverlay);
            _instance._monOv.Start();
         }
      }

      /// <summary>
      /// Gets screen rotation in degrees.
      /// </summary>
      public static int ScreenRotation
      {
         get { return _instance._sRot; }
      }

      /// <summary>
      /// Gets/Sets show mouse cursor visibility.
      /// </summary>
      public static bool ShowMouseCursor
      {
         get { return _instance._showMouse; }
         set
         {
            if (_instance._showMouse == value)
            {
               return;
            }

            _instance._showMouse = value;

            if (!value)
            {
               SafeFlush(_instance._mouseX, _instance._mouseY, 12, 19);
            }
            else if (_instance._mouseAvail)
            {
               _instance._mouseBuffer.DrawImage(0, 0, _instance._screen, _instance._mouseX, _instance._mouseY, 12, 19);
               _instance._screen.DrawImage(_instance._mouseX, _instance._mouseY, _instance._mouse, 0, 0, 12, 19);
               _instance.FlushMouse(_instance._mouseX, _instance._mouseY, 12, 19);
            }
         }
      }

      /// <summary>
      /// Gets the system wide colors.
      /// </summary>
      public static SystemColors SystemColors
      {
         get { return _instance._colors; }
      }

      /// <summary>
      /// Gets/Sets if the virtual keyboard should be used.
      /// </summary>
      public static bool UseVirtualKeyboard
      {
         get { return _instance._useVkb; }
         set { _instance._useVkb = value; }
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// Sets focus to an application, restoring its last active container
      /// </summary>
      /// <param name="app">Guid of application to activate</param>
      /// <returns>True if successful; else false</returns>
      public static bool ActivateApplication(Guid app)
      {
         for (int i = 0; i < _instance._appRefs.Length; i++)
         {
            if (_instance._appRefs[i].ThreadId.Equals(app))
            {
               ActiveContainer = _instance._appRefs[i].LastActive;
               return true;
            }
         }

         return false;
      }

      /// <summary>
      /// Returns information from a Skewworks Application.
      /// </summary>
      /// <param name="threadId">Guid of application to get information from</param>
      /// <returns>Returns the <see cref="ApplicationDetails"/> of the application.</returns>
      public static ApplicationDetails GetApplicationDetails(Guid threadId)
      {
         if (_instance._appRefs != null)
         {
            for (int i = 0; i < _instance._appRefs.Length; i++)
            {
               if (_instance._appRefs[i].ThreadId.Equals(threadId))
                  return _instance._appRefs[i].Details;
            }
         }

         return new ApplicationDetails();
      }

      /// <summary>
      /// Returns the image of the application
      /// </summary>
      /// <param name="threadId">Guid of application to get image from</param>
      /// <returns>Returns the image of the application</returns>
      public static ApplicationImage GetApplicationImage(Guid threadId)
      {
         if (_instance._appRefs != null)
         {
            for (int i = 0; i < _instance._appRefs.Length; i++)
            {
               if (_instance._appRefs[i].ThreadId.Equals(threadId))
               {
                  return _instance._appRefs[i].Image;
               }
            }
         }
         return new ApplicationImage();
      }

      /// <summary>
      /// Broadcasts a message to all applications in all AppDomains.
      /// </summary>
      /// <param name="sender">Object sending the message.</param>
      /// <param name="message">Message being sent.</param>
      /// <param name="args">Array of arguments to send.</param>
      public static void BroadcastMessage(object sender, string message, object[] args)
      {
         _instance.BroadcastMessage(sender, message, args, Mc);
      }

      /// <summary>
      /// Automatically sets clipping region for a control.
      /// </summary>
      /// <param name="control">Control to clip for</param>
      /// <param name="x">X of control</param>
      /// <param name="y">Y of control</param>
      /// <param name="width">Width of control</param>
      /// <param name="height">Height of control</param>
      public static void ClipForControl(IControl control, int x, int y, int width, int height)
      {
         var area = new rect(x, y, width, height);
         rect r;
         if (control.Parent == null)
         {
            r = new rect(0, 0, _instance._sW, _instance._sH);
         }
         else
         {
            IControl c = control.Parent;
            r = c.ScreenBounds;
         }

         r = rect.Intersect(r, area);

         _instance._screen.SetClippingRectangle(r.X, r.Y, r.Width, r.Height);
      }

      /// <summary>
      /// Flushes all cached files of all volumes to the physical media.
      /// </summary>
      public static void FlushFileSystem()
      {
         try
         {
            VolumeInfo[] vi = VolumeInfo.GetVolumes();
            for (int i = 0; i < vi.Length; i++)
            {
               vi[i].FlushAll();
            }
         }
         // ReSharper disable once EmptyGeneralCatchClause
         catch
         { }
      }

      /// <summary>
      /// Returns a Bitmap from raw file bytes.
      /// </summary>
      /// <param name="data">Bytes to convert into Bitmap.</param>
      /// <returns>Returns the <see cref="Bitmap"/> created from data.</returns>
      public static Bitmap ImageFromBytes(byte[] data)
      {
         if (data[0] == 0xFF && data[1] == 0xD8)
         {
            // JPEG
            return new Bitmap(data, Bitmap.BitmapImageType.Jpeg);
         }

         if (data[0] == 0x42 && data[1] == 0x4D)
         {
            // BMP
            return new Bitmap(data, Bitmap.BitmapImageType.Bmp);
         }

         if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x38)
         {
            // GIF
            return new Bitmap(data, Bitmap.BitmapImageType.Gif);
         }

         // Unsupported Image Type
         return null;
      }

      /// <summary>
      /// Initializes the core.
      /// </summary>
      /// <param name="touchCollection">Touch collection mode to use.</param>
      /// <param name="splash">Splash image to display until first container is activated.</param>
      /// <param name="backColor">Background color to display until first container is activated.</param>
      /// <remarks>
      /// Initialize is the first call to Tinkr that has to be made. It needs to be called only once.
      /// Additional calls will be ignored.
      /// </remarks>
      public static void Initialize(TouchCollection touchCollection, Bitmap splash = null, Color backColor = 0)
      {
         if (_instance == null)
         {
            lock (SyncRoot)
            {
               if (_instance == null)
               {
                  // ReSharper disable once PossibleMultipleWriteAccessInDoubleCheckLocking
                  _instance = new Core(touchCollection, splash, backColor);
               }
            }
         }
      }

      /// <summary>
      /// Launches a Skewworks Application in a new AppDomain.
      /// </summary>
      /// <param name="filename">Full path to the application's PE file.</param>
      /// <param name="args">Arguments to pass to application</param>
      /// <remarks>
      /// This is a non-blocking method which allows you to have multiple applications running side-by-side.
      /// To start an application blocking use <see cref="ShellNETMF(string, string)"/>
      /// </remarks>
      public static bool LaunchApplication(string filename, string[] args = null)
      {
         return _instance.LaunchApp(filename, args);
      }

      /// <summary>
      /// Launches a Skewworks Application in a new AppDomain.
      /// </summary>
      /// <param name="appData">Raw bytes of application's PE file</param>
      /// <param name="args">Arguments to pass to application</param>
      /// <remarks>
      /// This is a non-blocking method which allows you to have multiple applications running side-by-side.
      /// To start an application blocking use <see cref="ShellNETMF(byte[], string)"/>
      /// </remarks>
      public static bool LaunchApplication(byte[] appData, string[] args = null)
      {
         return _instance.LaunchApp(appData, args);
      }

      // ReSharper disable once UnusedParameter.Local
      //TODO: check why args is not used
      private bool LaunchApp(string filename, string[] args = null)
      {
         int id = _nextDomain++;
         AppDomain ad;

         try
         {
            ad = AppDomain.CreateDomain("app" + id);
         }
         catch (Exception)
         {
            return false;
         }

         try
         {
            var launcher = (IApplicationLauncher)ad.CreateInstanceAndUnwrap(typeof(IApplicationLauncher).Assembly.FullName, typeof(ApplicationLauncher).FullName);

            if (launcher.LoadApp(_instance, filename, GetDependencies(filename)))
            {
               if (_appRefs == null)
                  _appRefs = new[] { launcher };
               else
               {
                  var tmp = new IApplicationLauncher[_appRefs.Length + 1];
                  Array.Copy(_appRefs, tmp, _appRefs.Length);
                  tmp[tmp.Length - 1] = launcher;
                  _appRefs = tmp;
               }

               OnApplicationLaunched(this, launcher.ThreadId);
               return true;
            }
         }
         catch (Exception)
         {
            // Unload app domain
            AppDomain.Unload(ad);
         }

         return false;
      }

      // ReSharper disable once UnusedParameter.Local
      //TODO: check why args is not used
      private bool LaunchApp(byte[] appData, string[] args = null)
      {
         int id = _nextDomain++;
         AppDomain ad;

         try
         {
            ad = AppDomain.CreateDomain("app" + id);
         }
         catch (Exception)
         {
            return false;
         }

         try
         {
            var launcher = (IApplicationLauncher)ad.CreateInstanceAndUnwrap(typeof(IApplicationLauncher).Assembly.FullName, typeof(ApplicationLauncher).FullName);

            if (launcher.LoadApp(_instance, appData))
            {
               if (_appRefs == null)
                  _appRefs = new[] { launcher };
               else
               {
                  var tmp = new IApplicationLauncher[_appRefs.Length + 1];
                  Array.Copy(_appRefs, tmp, _appRefs.Length);
                  tmp[tmp.Length - 1] = launcher;
                  _appRefs = tmp;
               }

               OnApplicationLaunched(this, launcher.ThreadId);
               return true;
            }
         }
         catch (Exception)
         {
            // Unload app domain
            AppDomain.Unload(ad);
         }

         return false;
      }

      /// <summary>
      /// Manually grab a touch from native touch controller.
      /// </summary>
      /// <param name="endTicks">Time in ticks when waiting for the event should be terminated (e.g. DateTime.Now.Ticks + l100Ms).</param>
      /// <returns>
      /// Returns the <see cref="TouchEventArgs"/> retrieved from the touchscreen.
      /// If a timeout occurs the position of the return value is -1, -1.
      /// </returns>
      public static TouchEventArgs ManualTouchPoint(long endTicks)
      {
         while (true)
         {
            if (DateTime.Now.Ticks > endTicks)
            {
               return new TouchEventArgs(new point(-1, -1), 0);
            }

            // Check the Pen
            int x = 0;
            int y = 0;
            TouchCollectorConfiguration.GetLastTouchPoint(ref x, ref y);

            if (x != 1022 && x > 0 || y != 1022 && y > 0)
            {
               //_instance._x = x;
               //_instance._y = y;

               // Pen down
               if (!_instance._penDown)
               {
                  // Pen Down
                  _instance._penDown = true;
                  _instance._lastX = x;
                  _instance._lastY = y;
                  return new TouchEventArgs(new point(x, y), 0);
               }
            }
            else
            {
               if (_instance._penDown)
               {
                  // Pen Tapped
                  _instance._penDown = false;
                  return new TouchEventArgs(new point(_instance._lastX, _instance._lastY), 1);
               }
            }

            Thread.Sleep(5);
         }
      }

      /// <summary>
      /// Manually inform core of a button event.
      /// </summary>
      /// <param name="buttonId">Id of button affected</param>
      /// <param name="pressed">true if the button is pressed; false if released.</param>
      /// <remarks>
      /// Use RaiseButtonEvent to inject a button event into the system.
      /// </remarks>
      public static void RaiseButtonEvent(int buttonId, bool pressed)
      {
         if (_instance._active != null)
         {
            _instance._active.SendButtonEvent(buttonId, pressed);
         }
      }

      /// <summary>
      /// Manually inform core of a keyboard alt key event.
      /// </summary>
      /// <param name="key">Id of alt key affected</param>
      /// <param name="pressed">true if the key is pressed; false if released.</param>
      /// <remarks>
      /// Use RaiseKeyboardAltKeyEvent to inject a alt key event into the system.
      /// </remarks>
      public static void RaiseKeyboardAltKeyEvent(int key, bool pressed)
      {
         switch (key)
         {
            case 225:
            case 229:       // Left & Right Shifts
               _instance._shift = pressed;
               break;
            case 224:
            case 228:       // Left & Right Controls
               _instance._ctrl = pressed;
               break;
            case 226:
            case 230:       // Left & Right Alts
               _instance._alt = pressed;
               break;
         }

         if (_instance._active != null)
         {
            _instance._active.SendKeyboardAltKeyEvent(key, pressed);
         }
         _instance.OnKeyboardAltKeyEvent(key, pressed);
      }

      /// <summary>
      /// Manually inform core of a keyboard key event.
      /// </summary>
      /// <param name="key">Character of key affected.</param>
      /// <param name="pressed">true if the key is pressed; false if released.</param>
      /// <remarks>
      /// Use RaiseKeyboardKeyEvent to inject a key event into the system.
      /// </remarks>
      public static void RaiseKeyboardKeyEvent(char key, bool pressed)
      {
         if (_instance._active != null)
         {
            _instance._active.SendKeyboardKeyEvent(key, pressed);
         }
         _instance.OnKeyboardKeyEvent(key, pressed);
      }

      /// <summary>
      /// Manually inform core of a touch event
      /// </summary>
      /// <param name="touchType"><see cref="TouchType"/> of touch event.</param>
      /// <param name="pt">Location of touch event.</param>
      /// <param name="force">Force of touch event (gesture only).</param>
      /// <remarks>
      /// Use RaiseTouchEvent to inject a touch event into the system.
      /// </remarks>
      public static void RaiseTouchEvent(TouchType touchType, point pt, float force = 1.0f)
      {
         _instance.RaiseTouch(touchType, pt, force);
      }

      /// <summary>
      /// Safely flushes the screen accounting for overlays and mouse cursor.
      /// </summary>
      public static void SafeFlush()
      {
         bool drawMouse = false;

         // Update Mouse Buffer
         if (MouseAvailable && _instance._showMouse)
         {
            _instance._mouseBuffer.DrawImage(0, 0, _instance._screen, _instance._mouseX, _instance._mouseY, 12, 19);
            drawMouse = true;
         }

         if (_instance._overlay != null)
         {
            if (_instance._overlayBuffer == null)
            {
               _instance._overlayBuffer = new Bitmap(_instance._overlay.Size.Width, _instance._overlay.Size.Height);
            }

            _instance._overlayBuffer.DrawImage(0, 0, 
               _instance._screen, _instance._overlay.Position.X, _instance._overlay.Position.Y,
               _instance._overlayBuffer.Width, _instance._overlayBuffer.Height);

            _instance._screen.DrawRectangle(_instance._overlay.BorderColor, 1, _instance._overlay.Position.X, _instance._overlay.Position.Y,
                _instance._overlay.Size.Width, _instance._overlay.Size.Height, 0, 0, _instance._overlay.BackColor, 0, 0,
                _instance._overlay.BackColor, 0, 0, 128);

            if (_instance._overlay.Image == null)
            {
               _instance._screen.DrawText(_instance._overlay.Text, _instance._overlay.Font, _instance._overlay.ForeColor,
                   _instance._overlay.Position.X + 8, _instance._overlay.Position.Y + 4);
            }
            else
            {
               _instance._screen.DrawImage(_instance._overlay.Position.X + 8, _instance._overlay.Position.Y + 4, _instance._overlay.Image, 0, 0,
                   _instance._overlay.Image.Width, _instance._overlay.Image.Height);
               _instance._screen.DrawText(_instance._overlay.Text, _instance._overlay.Font, _instance._overlay.ForeColor,
                   _instance._overlay.Position.X + 16 + _instance._overlay.Image.Width,
                   _instance._overlay.Position.Y + 4 + (_instance._overlay.Image.Height / 2 - _instance._overlay.Font.Height / 2));
            }

            _instance._screen.DrawImage(_instance._overlay.Position.X, _instance._overlay.Position.Y, _instance._overlayBuffer, 0, 0,
                _instance._overlayBuffer.Width, _instance._overlayBuffer.Height, (ushort)(256 - _instance._overlay.Opacity));
         }

         if (drawMouse)
         {
            _instance._screen.DrawImage(_instance._mouseX, _instance._mouseY, _instance._mouse, 0, 0, 12, 19);
         }

         _instance._screen.Flush();
      }

      /// <summary>
      /// Safely flushes the screen accounting for overlays and mouse cursor.
      /// </summary>
      /// <param name="x">X location of screen to flush.</param>
      /// <param name="y">Y location of screen to flush.</param>
      /// <param name="width">Width of the screen area to flush.</param>
      /// <param name="height">Height of the screen area to flush.</param>
      public static void SafeFlush(int x, int y, int width, int height)
      {
         _instance._screen.SetClippingRectangle(0, 0, _instance._screen.Width, _instance._screen.Height);

         if (x < 0)
         {
            x = 0;
         }
         if (y < 0)
         {
            y = 0;
         }
         if (width > _instance._sW)
         {
            width = _instance._sW;
         }
         if (height > _instance._sH)
         {
            height = _instance._sH;
         }

         bool drawMouse = false;
         var r = new rect(x, y, width, height);

         // Update Mouse Buffer
         if (MouseAvailable && _instance._showMouse && r.Contains(new point(_instance._mouseX, _instance._mouseY)))
         {
            _instance._mouseBuffer.DrawImage(0, 0, _instance._screen, _instance._mouseX, _instance._mouseY, 12, 19);
            drawMouse = true;
         }

         if (_instance._overlay != null)
         {
            if (_instance._overlayBuffer == null)
            {
               _instance._overlayBuffer = new Bitmap(_instance._overlay.Size.Width, _instance._overlay.Size.Height);
            }

            _instance._overlayBuffer.DrawImage(0, 0, _instance._screen, _instance._overlay.Position.X, _instance._overlay.Position.Y,
                _instance._overlayBuffer.Width, _instance._overlayBuffer.Height);

            _instance._screen.DrawRectangle(_instance._overlay.BorderColor, 1, _instance._overlay.Position.X, _instance._overlay.Position.Y,
                _instance._overlay.Size.Width, _instance._overlay.Size.Height, 0, 0, _instance._overlay.BackColor, 0, 0,
                _instance._overlay.BackColor, 0, 0, 128);

            if (_instance._overlay.Image == null)
            {
               _instance._screen.DrawText(_instance._overlay.Text, _instance._overlay.Font, _instance._overlay.ForeColor,
                   _instance._overlay.Position.X + 8, _instance._overlay.Position.Y + 4);
            }
            else
            {
               _instance._screen.DrawImage(_instance._overlay.Position.X + 8, _instance._overlay.Position.Y + 4, _instance._overlay.Image, 0, 0,
                   _instance._overlay.Image.Width, _instance._overlay.Image.Height);
               _instance._screen.DrawText(_instance._overlay.Text, _instance._overlay.Font, _instance._overlay.ForeColor,
                   _instance._overlay.Position.X + 16 + _instance._overlay.Image.Width,
                   _instance._overlay.Position.Y + 4 + (_instance._overlay.Image.Height / 2 - _instance._overlay.Font.Height / 2));
            }

            _instance._screen.DrawImage(_instance._overlay.Position.X, _instance._overlay.Position.Y, _instance._overlayBuffer, 0, 0,
                _instance._overlayBuffer.Width, _instance._overlayBuffer.Height, (ushort)(256 - _instance._overlay.Opacity));
         }

         if (drawMouse)
         {
            _instance._screen.DrawImage(_instance._mouseX, _instance._mouseY, _instance._mouse, 0, 0, 12, 19);
         }

         _instance._screen.Flush(x, y, width, height);
      }

      /// <summary>
      /// Shells a full NETMF application in a new AppDomain.
      /// </summary>
      /// <param name="filename">Full path to the file PE to be shelled.</param>
      /// <param name="domain">Name to give the new AppDomain.</param>
      /// <returns>true if completed successfully; else false</returns>
      /// <remarks>
      /// This is a blocking method, the primary application will not respond until the shelled application terminates.
      /// To start an application non blocking use <see cref="LaunchApplication(string, string[])"/>
      /// </remarks>
      public static bool ShellNETMF(string filename, string domain)
      {
         AppDomain ad = AppDomain.CreateDomain(domain);
         var launcher = (IApplicationLauncher)ad.CreateInstanceAndUnwrap(typeof(IApplicationLauncher).Assembly.FullName, typeof(ApplicationLauncher).FullName);
         bool bOk = launcher.ShellApp(_instance, filename, GetDependencies(filename));
         AppDomain.Unload(ad);
         return bOk;
      }

      /// <summary>
      /// Shells a full NETMF application in a new AppDomain.
      /// </summary>
      /// <param name="appData">Raw bytes of application PE file to be shelled.</param>
      /// <param name="domain">Name to give the new AppDomain.</param>
      /// <returns>true if completed successfully; else false</returns>
      /// <remarks>
      /// This is a blocking method, the primary application will not respond until the shelled application terminates.
      /// To start an application non blocking use <see cref="LaunchApplication(byte[], string[])"/>
      /// </remarks>
      public static bool ShellNETMF(byte[] appData, string domain)
      {
         AppDomain ad = AppDomain.CreateDomain(domain);
         var launcher = (IApplicationLauncher)ad.CreateInstanceAndUnwrap(typeof(IApplicationLauncher).Assembly.FullName, typeof(ApplicationLauncher).FullName);
         bool bOk = launcher.ShellApp(_instance, appData);
         AppDomain.Unload(ad);
         return bOk;
      }

      /// <summary>
      /// Activate a container without rendering it
      /// </summary>
      /// <param name="container">Container to activate</param>
      public static void SilentlyActivate(IContainer container)
      {
         if (_instance._active != null)
            _instance._active.Blur();
         _instance._active = container;
      }

      /// <summary>
      /// Draws a 2 pixel wide alpha blended shadow around the supplied area.
      /// </summary>
      /// <param name="x">X  location of the region to be shadowed.</param>
      /// <param name="y">Y  location of the region to be shadowed.</param>
      /// <param name="width">Width of the region to be shadowed.</param>
      /// <param name="height">Height of the region to be shadowed.</param>
      public static void ShadowRegion(int x, int y, int width, int height)
      {
         // Inner Line
         _instance._screen.DrawRectangle(0, 0, x + 2, y - 1, width - 4, 1, 0, 0, 0, 0, 0, 0, 0, 0, 102);
         _instance._screen.DrawRectangle(0, 0, x + 2, y + height, width - 4, 1, 0, 0, 0, 0, 0, 0, 0, 0, 102);
         _instance._screen.DrawRectangle(0, 0, x - 1, y + 2, 1, height - 4, 0, 0, 0, 0, 0, 0, 0, 0, 102);
         _instance._screen.DrawRectangle(0, 0, x + width, y + 2, 1, height - 4, 0, 0, 0, 0, 0, 0, 0, 0, 102);
         _instance._screen.DrawRectangle(0, 0, x, y - 1, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 76);
         _instance._screen.DrawRectangle(0, 0, x + width - 2, y - 1, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 76);
         _instance._screen.DrawRectangle(0, 0, x, y + height, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 76);
         _instance._screen.DrawRectangle(0, 0, x + width - 2, y + height, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 76);
         _instance._screen.DrawRectangle(0, 0, x - 1, y, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 76);
         _instance._screen.DrawRectangle(0, 0, x - 1, y + height - 2, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 76);
         _instance._screen.DrawRectangle(0, 0, x + width, y, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 76);
         _instance._screen.DrawRectangle(0, 0, x + width, y + height - 2, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 76);
         _instance._screen.DrawRectangle(0, 0, x - 1, y - 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 51);
         _instance._screen.DrawRectangle(0, 0, x + width, y - 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 51);
         _instance._screen.DrawRectangle(0, 0, x - 1, y + height, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 51);
         _instance._screen.DrawRectangle(0, 0, x + width, y + height, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 51);

         // Outer Line
         _instance._screen.DrawRectangle(0, 0, x + 3, y - 2, width - 6, 1, 0, 0, 0, 0, 0, 0, 0, 0, 51);
         _instance._screen.DrawRectangle(0, 0, x + 3, y + height + 1, width - 6, 1, 0, 0, 0, 0, 0, 0, 0, 0, 51);
         _instance._screen.DrawRectangle(0, 0, x - 2, y + 3, 1, height - 6, 0, 0, 0, 0, 0, 0, 0, 0, 51);
         _instance._screen.DrawRectangle(0, 0, x + width + 1, y + 3, 1, height - 6, 0, 0, 0, 0, 0, 0, 0, 0, 51);
         _instance._screen.DrawRectangle(0, 0, x + 1, y - 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 26);
         _instance._screen.DrawRectangle(0, 0, x + width - 3, y - 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 26);
         _instance._screen.DrawRectangle(0, 0, x + 1, y + height + 1, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 26);
         _instance._screen.DrawRectangle(0, 0, x + width - 3, y + height + 1, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 26);
         _instance._screen.DrawRectangle(0, 0, x - 2, y + 1, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 26);
         _instance._screen.DrawRectangle(0, 0, x - 2, y + height - 3, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 26);
         _instance._screen.DrawRectangle(0, 0, x + width + 1, y + 1, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 26);
         _instance._screen.DrawRectangle(0, 0, x + width + 1, y + height - 3, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 26);
      }

      /// <summary>
      /// Draws a 4 pixel wide alpha blended shadow inside the supplied area.
      /// </summary>
      /// <param name="x">X location of the region to be shadowed.</param>
      /// <param name="y">Y location of the region to be shadowed.</param>
      /// <param name="width">Width of the region to be shadowed.</param>
      /// <param name="height">Height of the region to be shadowed.</param>
      /// <param name="color">Color to use when rendering shadow.</param>
      public static void ShadowRegionInset(int x, int y, int width, int height, Color color = Colors.Black)
      {
         ushort[] alpha = { 76, 51, 26, 13 };
         int i;
         int v1 = 0;
         int v2 = 1;
         int v3 = 2;
         for (i = 0; i < alpha.Length; i++)
         {
            _instance._screen.DrawRectangle(0, 0, x + v1, y + v1, width - v1, 1, 0, 0, color, 0, 0, color, 0, 0, alpha[i]);
            _instance._screen.DrawRectangle(0, 0, x + v1, y + height - v2, width - v1, 1, 0, 0, color, 0, 0, color, 0, 0, alpha[i]);
            _instance._screen.DrawRectangle(0, 0, x + v1, y + v2, 1, height - v3, 0, 0, color, 0, 0, color, 0, 0, alpha[i]);
            _instance._screen.DrawRectangle(0, 0, x + width - v2, y + v2, 1, height - v3, 0, 0, color, 0, 0, color, 0, 0, alpha[i]);
            v1 += 1;
            v2 += 1;
            v3 += 1;
         }

      }

      /// <summary>
      /// Terminates a running application.
      /// </summary>
      /// <param name="threadId">Guid of application to terminate.</param>
      /// <remarks>
      /// Use TerminateApplication to terminate an application that was launched by 
      /// <see cref="LaunchApplication(string, string[])"/> or <see cref="LaunchApplication(byte[], string[])"/>
      /// </remarks>
      public static bool TerminateApplication(Guid threadId)
      {
         return _instance.TermApp(threadId);
      }

      private bool TermApp(Guid threadId)
      {
         if (_appRefs != null)
         {
            for (int i = 0; i < _appRefs.Length; i++)
            {
               if (_appRefs[i].ThreadId.Equals(threadId))
               {
                  try
                  {
                     ActiveContainer = _defaultReturn;

                     AppDomain ad = _appRefs[i].Domain;
                     ApplicationDetails det = _appRefs[i].Details;

                     _appRefs[i].Terminate();
                     AppDomain.Unload(ad);

                     if (_appRefs.Length == 1)
                     {
                        _appRefs = null;
                     }
                     else
                     {
                        var tmp = new IApplicationLauncher[_appRefs.Length - 1];
                        int c = 0;
                        for (int j = 0; j < tmp.Length; j++)
                        {
                           if (j != i)
                              tmp[c++] = _appRefs[j];
                        }

                        _appRefs = tmp;
                     }

                     _instance.ApplicationClosed(this, det);
                     return true;
                  }
                  catch (Exception)
                  {
                     _instance.ApplicationClosed(this, new ApplicationDetails());
                     return false;
                  }
               }
            }
         }

         return false;
      }

      /// <summary>
      /// Unlocks premium level core features.
      /// </summary>
      /// <param name="key">License key to unlock features.</param>
      /// <remarks>
      /// Since Tinkr is open source now, this method has no use anymore and will be removed in a future version.
      /// </remarks>
      [Obsolete("Method will be removed in a future release")]
      public static void UnlockPremiumFeatures(string key)
      { }

      #endregion

      #region Private Methods

      private void RaiseTouch(TouchType touchType, point pt, float force)
      {
         // Send event to Active Container
         if (_active != null)
         {
            switch (touchType)
            {
               case TouchType.TouchDown:
                  _active.SendTouchDown(null, pt);
                  break;
               case TouchType.TouchMove:
                  _active.SendTouchMove(null, pt);
                  break;
               case TouchType.TouchUp:
                  _active.SendTouchUp(null, pt);
                  break;
               default:
                  _active.SendTouchGesture(null, touchType, force);
                  break;
            }
         }

         RaiseTouch(touchType, pt);
      }

      private void AddMessageClient(MessageClient mc)
      {
         if (_mcs == null)
            _mcs = new[] { mc };
         else
         {
            var tmp = new MessageClient[_mcs.Length + 1];
            Array.Copy(_mcs, tmp, _mcs.Length);
            tmp[tmp.Length - 1] = mc;
            _mcs = tmp;
         }
      }

      private void BroadcastMessage(object sender, string message, object[] args, MessageClient mc)
      {
         for (int i = 0; i < _instance._mcs.Length; i++)
         {
            if (_instance._mcs[i] != mc)
               _instance._mcs[i].BroadcastMessage(sender, message, args);
         }
      }

      private void FlushMouse(int x, int y, int width, int height)
      {
         if (x < 0)
            x = 0;
         if (y < 0)
            y = 0;
         if (width > _sW)
            width = _sW;
         if (height > _sH)
            height = _sH;
         _screen.Flush(x, y, width, height);
      }

      private void MonitorOverlay()
      {
         int x = _overlay.Position.X;
         int y = _overlay.Position.Y;
         int w = _overlay.Size.Width;
         int h = _overlay.Size.Height;

         _screen.SetClippingRectangle(x, y, w, h);
         _overlayBuffer = new Bitmap(w, h);
         _overlayBuffer.DrawImage(0, 0, _screen, x, y, w, h);

         _overlay.FadeAt = DateTime.Now.Ticks + (long)(TimeSpan.TicksPerSecond * _overlay.FadeAfter);

         SafeFlush(x, y, w, h);
         while (DateTime.Now.Ticks < _overlay.FadeAt)
         {
            Thread.Sleep(50);
         }

         while (_overlay.Opacity > 0)
         {
            if (_overlay.Opacity > 21)
            {
               _overlay.Opacity -= 20;
            }
            else
            {
               _overlay.Opacity = 0;
            }

            lock (_screen)
            {
               if (_active == null)
               {
                  _screen.SetClippingRectangle(x, y, w, h);
                  _screen.DrawImage(x, y, _overlayBuffer, 0, 0, w, h);
                  SafeFlush(x, y, w, h);
               }
               else
                  _active.Render(new rect(x, y, w, h), true);
            }

            Thread.Sleep(50);
         }

         _overlay = null;

         if (_active == null)
         {
            _screen.SetClippingRectangle(x, y, w, h);
            _screen.DrawImage(x, y, _overlayBuffer, 0, 0, w, h);
            SafeFlush(x, y, w, h);
         }
         else
         {
            _active.Render(true);
         }
      }

      private void RaiseTouch(TouchType touchType, point pt)
      {
         // Raise Touch Event
         OnTouchEvent(this, touchType, pt);
      }

      #endregion

      #region Shelling

      private static string[] GetDependencies(string filename)
      {
         string[] dep = null;
         string sDir = filename.Substring(0, filename.LastIndexOf("\\") + 1);

         // Check for app.config
         if (File.Exists(sDir + "app.config"))
         {
            string[] sFiles = new string(Encoding.UTF8.GetChars(File.ReadAllBytes(sDir + "app.config"))).Split(',');
            dep = new string[sFiles.Length];
            for (int f = 0; f < sFiles.Length; f++)
            {
               dep[f] = sDir + sFiles[f].Trim();
            }
         }
         else
         {
            // Load dependencies
            string[] s = Directory.GetFiles(sDir);
            for (int d = 0; d < s.Length; d++)
            {
               try
               {
                  if (Path.GetExtension(s[d]).ToLower() == ".pe" && s[d].ToLower() != filename.ToLower())
                  {
                     if (dep == null)
                     {
                        dep = new[] { s[d] };
                     }
                     else
                     {
                        var tmp = new string[dep.Length + 1];
                        Array.Copy(dep, tmp, dep.Length);
                        tmp[tmp.Length - 1] = s[d];
                        dep = tmp;
                     }
                  }
               }
               catch (Exception) { Debug.Print("Dependency failed: " + s[d]); }
            }
         }

         return dep;
      }

      private interface IApplicationLauncher
      {
         #region Properties

         AppDomain Domain { get; }
         Guid ThreadId { get; }
         ApplicationDetails Details { get; }
         ApplicationImage Image { get; }

         IContainer LastActive
         {
            get;
            set;
         }

         #endregion

         #region Methods

         bool ShellApp(Core instance, string filename, string[] dependencies);
         bool ShellApp(Core instance, byte[] pe);

         bool LoadApp(Core g, byte[] data);
         bool LoadApp(Core g, string filename, string[] dependencies);
         void Terminate();

         string SendMessage(object sender, string message, object[] args);

         #endregion

      }

      [Serializable]
      private class ApplicationLauncher : MarshalByRefObject, IApplicationLauncher
      {

         #region Variables

         NETMFApplication _app;
         IContainer _active;

         #endregion

         #region Properties

         public ApplicationDetails Details
         {
            get { return _app.ApplicationDetails; }
         }

         public ApplicationImage Image
         {
            get { return _app.ApplicationImage; }
         }

         public Guid ThreadId
         {
            get { return _app.ThreadId; }
         }

         public AppDomain Domain
         {
            get { return AppDomain.CurrentDomain; }
         }

         public IContainer LastActive
         {
            get { return _active; }
            set { _active = value; }
         }

         #endregion

         #region Launching Methods

         public bool LoadApp(Core g, string filename, string[] dependencies)
         {
            _instance = g;

            // Load Application

            // Dependencies
            if (dependencies != null)
            {
               for (int d = 0; d < dependencies.Length; d++)
                  Assembly.Load(File.ReadAllBytes(dependencies[d]));
            }

            // Launch Application
            try
            {
               var asm = Assembly.Load(File.ReadAllBytes(filename));

               Type[] t = asm.GetTypes();
               for (int i = 0; i < t.Length; i++)
               {
                  if (t[i].BaseType != null && t[i].BaseType.FullName == "Skewworks.NETMF.Applications.NETMFApplication")
                  {
                     try
                     {
                        _app = (NETMFApplication)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(asm.FullName, t[i].FullName);
                        _app.EntryPoint(Guid.NewGuid(), string.Empty, null);
                        return true;
                     }
                     // ReSharper disable once EmptyGeneralCatchClause
                     catch
                     { }
                  }
               }
            }
            catch (Exception)
            {
               return false;
            }

            return false;
         }

         public bool LoadApp(Core g, byte[] data)
         {
            // Set the instance across domains
            _instance = g;

            // Launch Application
            try
            {
               Assembly asm = Assembly.Load(data);

               Type[] t = asm.GetTypes();
               for (int i = 0; i < t.Length; i++)
               {
                  if (t[i].BaseType != null && t[i].BaseType.FullName == "Skewworks.NETMF.Applications.NETMFApplication")
                  {
                     try
                     {
                        _app = (NETMFApplication)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(asm.FullName, t[i].FullName);
                        _app.EntryPoint(Guid.NewGuid(), string.Empty, null);
                        return true;
                     }
                     // ReSharper disable once EmptyGeneralCatchClause
                     catch
                     { }
                  }
               }
            }
            catch (Exception)
            {
               return false;
            }

            return false;
         }

         public string SendMessage(object sender, string message, object[] args)
         {
            if (_app == null)
               return string.Empty;

            return _app.SendMessage(sender, message, args);
         }

         public void Terminate()
         {
            _app.Terminate();
         }

         #endregion

         #region Shelling Methods

         public bool ShellApp(Core instance, string filename, string[] dependencies)
         {
            // Load Application

            // Set Core Instance
            Host = instance;
            instance.AddMessageClient(MessageClient);

            // Dependencies
            if (dependencies != null)
            {
               for (int d = 0; d < dependencies.Length; d++)
                  Assembly.Load(File.ReadAllBytes(dependencies[d]));
            }

            try
            {
               var asm = Assembly.Load(File.ReadAllBytes(filename));
               if (asm == null)
                  return false;

               Type[] t = asm.GetTypes();
               for (int i = 0; i < t.Length; i++)
               {
                  if (t[i].BaseType.FullName == "System.Object")
                  {
                     try
                     {
                        var m = t[i].GetMethods();
                        if (m != null)
                        {
                           int j;
                           for (j = 0; j < m.Length; j++)
                           {
                              if (m[j].Name == "Main")
                              {
                                 m[j].Invoke(this, null);
                                 return true;
                              }
                           }
                        }
                     }
                        // ReSharper disable once EmptyGeneralCatchClause
                     catch
                     { }
                  }
               }
            }
            catch (Exception ex)
            {
               Debug.Print("Launch Application Error: " + ex);
            }

            return false;
         }

         public bool ShellApp(Core instance, byte[] pe)
         {
            // Load Application

            // Set Core Instance
            Host = instance;
            instance.AddMessageClient(MessageClient);


            try
            {
               var asm = Assembly.Load(pe);
               if (asm == null)
                  return false;

               var t = asm.GetTypes();
               for (int i = 0; i < t.Length; i++)
               {
                  if (t[i].BaseType.FullName == "System.Object")
                  {
                     try
                     {
                        var m = t[i].GetMethods();
                        if (m != null)
                        {
                           for (int j = 0; j < m.Length; j++)
                           {
                              if (m[j].Name == "Main")
                              {
                                 m[j].Invoke(this, null);
                                 return true;
                              }
                           }
                        }
                     }
                        // ReSharper disable once EmptyGeneralCatchClause
                     catch
                     { }
                  }
               }
            }
            catch (Exception ex)
            {
               Debug.Print("Launch Application Error: " + ex);
            }

            return false;
         }

         #endregion

      }

      #endregion

      #region Touch Collection

      //private int _x;
      //private int _y;
      private bool _penDown;
      private int _lastX, _lastY;
      private point _ptDownAt;
      private long _lgDownAt;
      private TouchType _tt;
      private bool _cancelSwipe;
      private bool _thrownAway;

      private void MyTouch_Tick()
      {
         int x = 0;
         int y = 0;

         while (true)
         {
            // Check the Pen
            TouchCollectorConfiguration.GetLastTouchPoint(ref x, ref y);

            point ptLast;
            if (x != 1022 && x > 0 || y != 1022 && y > 0)
            {
               //_x = x;
               //_y = y;

               if (!_thrownAway)
               {
                  _lastX = x;
                  _lastY = y;
                  ptLast = new point(x, y);
                  RaiseTouchEvent(TouchType.TouchDown, ptLast);
                  RaiseTouchEvent(TouchType.TouchUp, ptLast);
                  _thrownAway = true;
               }
               else
               {
                  // Pen down
                  if (_penDown)
                  {
                     // We already know about it
                     if (System.Math.Abs(_lastX - x) + System.Math.Abs(_lastY - y) > 10)
                     {
                        _lastX = x;
                        _lastY = y;
                        ptLast = new point(x, y);

                        if (!_cancelSwipe)
                           CalcDir(ptLast);

                        RaiseTouchEvent(TouchType.TouchMove, ptLast);
                     }
                  }
                  else
                  {
                     // Pen Down
                     _penDown = true;
                     _lastX = x;
                     _lastY = y;
                     ptLast = new point(x, y);

                     _ptDownAt = ptLast;
                     _lgDownAt = DateTime.Now.Ticks;
                     _cancelSwipe = false;
                     _tt = TouchType.NoGesture;

                     RaiseTouchEvent(TouchType.TouchDown, ptLast);
                  }
               }
            }
            else if (_penDown)
            {
               // Pen Tapped
               _penDown = false;
               ptLast = new point(_lastX, _lastY);

               if (!_cancelSwipe && _tt != TouchType.NoGesture)
                  CalcForce(ptLast);

               RaiseTouchEvent(TouchType.TouchUp, ptLast);
            }

            Thread.Sleep(25);
         }
         // ReSharper disable once FunctionNeverReturns
      }

      private void CalcDir(point e)
      {
         var sw = TouchType.NoGesture;

         int d = (e.Y - _ptDownAt.Y);
         if (d > 50)
            sw = TouchType.GestureDown;
         else if (d < -50)
            sw = TouchType.GestureUp;

         d = (e.X - _ptDownAt.X);
         if (d > 50)
         {
            if (sw == TouchType.GestureUp)
               sw = TouchType.GestureUpRight;
            else if (sw == TouchType.GestureDown)
               sw = TouchType.GestureDownRight;
            else
               sw = TouchType.GestureRight;
         }
         else if (d < -50)
         {
            if (sw == TouchType.GestureUp)
               sw = TouchType.GestureUpLeft;
            else if (sw == TouchType.GestureDown)
               sw = TouchType.GestureDownLeft;
            else
               sw = TouchType.GestureLeft;
         }

         if (_tt == TouchType.NoGesture)
            _tt = sw;
         else if (_tt != sw)
            _cancelSwipe = true;
      }

      private void CalcForce(point e)
      {
         // Calculate by time alone
         float dDiff = DateTime.Now.Ticks - _lgDownAt;

         if (dDiff > TimeSpan.TicksPerSecond * .75)
            return;

         // 1.0 = < 1/7th second
         dDiff = TimeSpan.TicksPerSecond / 7 / dDiff;
         if (dDiff > .99)
            dDiff = .99f;
         else if (dDiff < 0)
            dDiff = 0;

         // Raise TouchEvent
         RaiseTouchEvent(_tt, e, dDiff);
      }

      internal class SafeTouchConnection : IEventListener
      {
         public void InitializeForEventSource() { }

         public bool OnEvent(BaseEvent baseEvent)
         {
            return true;
         }
      }

      #endregion
   }
}
// ReSharper restore StringLastIndexOfIsCultureSpecific.1
