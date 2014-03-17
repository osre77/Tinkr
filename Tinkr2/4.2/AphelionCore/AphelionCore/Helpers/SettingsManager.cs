using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Skewworks.NETMF.Controls;

// ReSharper disable StringIndexOfIsCultureSpecific.1
namespace Skewworks.NETMF
{
   /// <summary>
   /// Class for managing all EWR settings
   /// </summary>
   public class SettingsManager
   {
      #region Variables

      private static ManualResetEvent _activeBlock;            // Used to display Modal Forms
      private static bool _bLoaded;

      #endregion

      #region Properties

      /// <summary>
      /// Gets the current LCD settings
      /// </summary>
      public static LCDSettings LCD
      {
         get
         {
            if (!_bLoaded)
            {
               LoadLCDSettings();
               _bLoaded = true;
            }
            return _lcd;
         }
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// Starts the touch calibration
      /// </summary>
      /// <param name="cc">Configuration to use when calibrating the screen</param>
      /// <remarks>
      /// This will show the touch calibration window.
      /// </remarks>
      public static void CalibrateTouch(CalibrationConfiguration cc)
      {
         IContainer prv = Core.ActiveContainer;
         var cw = new CalibrationWindow(cc);
         Core.ActiveContainer = cw;

         _activeBlock = new ManualResetEvent(false);
         _activeBlock.Reset();

         while (!_activeBlock.WaitOne(1000, false))
         { }

         _activeBlock = null;

         Core.ActiveContainer = prv;
      }

      #endregion

      #region LCD Settings

      // dummy type to identify the EWR
      private class LcdEwr 
      { }

      private static ExtendedWeakReference _lcdEwrSettings;
      private static LCDSettings _lcd;

      /// <summary>
      /// LCD settings for the touch screen
      /// </summary>
      [Serializable]
      public class LCDSettings
      {
         /// <summary>
         /// Creates new LCD settings
         /// </summary>
         /// <param name="calibrate">Calibration mode</param>
         /// <param name="calibrationPoints">Number of points used for calibration</param>
         /// <param name="calibrationSx">SX values of calibration</param>
         /// <param name="calibrationSy">SY values of calibration</param>
         /// <param name="calibrationCx">CX values of calibration</param>
         /// <param name="calibrationCy">CY values of calibration</param>
         public LCDSettings(ScreenCalibration calibrate, int calibrationPoints, short[] calibrationSx, short[] calibrationSy, short[] calibrationCx, short[] calibrationCy)
         {
            calibrateLCD = calibrate;
            calibrationpoints = calibrationPoints;
            calibrationSX = calibrationSx;
            calibrationSY = calibrationSy;
            calibrationCX = calibrationCx;
            calibrationCY = calibrationCy;
         }

         // ReSharper disable InconsistentNaming

         // Fields must be realizable.

         /// <summary>
         /// Calibration mode
         /// </summary>
         public ScreenCalibration calibrateLCD;

         /// <summary>
         /// Number of points used for calibration
         /// </summary>
         public int calibrationpoints;

         /// <summary>
         /// SX values of calibration
         /// </summary>
         public short[] calibrationSX;

         /// <summary>
         /// SY values of calibration
         /// </summary>
         public short[] calibrationSY;

         /// <summary>
         /// CX values of calibration
         /// </summary>
         public short[] calibrationCX;

         /// <summary>
         /// CY values of calibration
         /// </summary>
         public short[] calibrationCY;

         // ReSharper restore InconsistentNaming
      }

      private static void LoadLCDSettings()
      {
         // Grab the data
         _lcdEwrSettings = ExtendedWeakReference.RecoverOrCreate(typeof(LcdEwr), 0, ExtendedWeakReference.c_SurvivePowerdown);
         _lcdEwrSettings.Priority = (Int32)ExtendedWeakReference.PriorityLevel.Critical;
         _lcd = (LCDSettings)_lcdEwrSettings.Target;
      }

      /// <summary>
      /// Restores touch calibration from EWR
      /// </summary>
      public static void RestoreLCDCalibration()
      {
         // Set calibration
         Touch.ActiveTouchPanel.SetCalibration(_lcd.calibrationpoints, _lcd.calibrationSX, _lcd.calibrationSY, _lcd.calibrationCX, _lcd.calibrationCY);
      }

      /// <summary>
      /// Restores touch calibration from supplied points
      /// </summary>
      /// <param name="points">Number of points used for calibration</param>
      /// <param name="sx">SX values of calibration</param>
      /// <param name="sy">SY values of calibration</param>
      /// <param name="cx">CX values of calibration</param>
      /// <param name="cy">CY values of calibration</param>
      public static void RestoreLCDCalibration(int points, short[] sx, short[] sy, short[] cx, short[] cy)
      {
         // Set calibration
         Touch.ActiveTouchPanel.SetCalibration(points, sx, sy, cx, cy);
      }

      /// <summary>
      /// Save touch calibration to EWR
      /// </summary>
      /// <param name="calibrationPoints">Number of points used for calibration</param>
      /// <param name="calibrationSx">SX values of calibration</param>
      /// <param name="calibrationSy">SY values of calibration</param>
      /// <param name="calibrationCx">CX values of calibration</param>
      /// <param name="calibrationCy">CY values of calibration</param>
      /// <returns>Returns true if the calibration was successfully saved.</returns>
      public static bool SaveLCDCalibration(int calibrationPoints, short[] calibrationSx, short[] calibrationSy, short[] calibrationCx, short[] calibrationCy)
      {
         // Save calibration
         if (_lcd == null)
         {
            _lcd = new LCDSettings(ScreenCalibration.Restore, calibrationPoints, calibrationSx, calibrationSy, calibrationCx, calibrationCy);
         }
         else
         {
            _lcd.calibrateLCD = ScreenCalibration.Restore;
            _lcd.calibrationpoints = calibrationPoints;
            _lcd.calibrationSX = calibrationSx;
            _lcd.calibrationSY = calibrationSy;
            _lcd.calibrationCX = calibrationCx;
            _lcd.calibrationCY = calibrationCy;
         }

         _lcdEwrSettings.Target = _lcd;

         return true;
      }

      #endregion

      #region Calibration Control

      private sealed class CalibrationWindow : Container
      {
         #region Variables

         readonly point[] _calpoints;
         int _currentCalpoint;
         readonly short[] _sx;
         readonly short[] _sy;
         readonly short[] _cx;
         readonly short[] _cy;
         readonly int _x;
         readonly int _y;
         readonly int _calibrationpointCount;

         bool _step1;
         readonly CalibrationConfiguration _cc;

         #endregion

         #region Constructor

         public CalibrationWindow(CalibrationConfiguration cc)
         {
            int i;
            _sy = null;
            _cc = cc;
            _step1 = true;

            X = 0;
            Y = 0;
            Width = Core.Screen.Width;
            Height = Core.Screen.Height;


            // Ask the touch system how many points are needed to 
            // calibrate.
            Touch.ActiveTouchPanel.GetCalibrationPointCount(ref _calibrationpointCount);

            // Create the calibration point array.
            _calpoints = new point[_calibrationpointCount];
            _sx = new short[_calibrationpointCount];
            _sy = new short[_calibrationpointCount];
            _cx = new short[_calibrationpointCount];
            _cy = new short[_calibrationpointCount];

            // Get the points for calibration.
            for (i = 0; i < _calibrationpointCount; i++)
            {
               Touch.ActiveTouchPanel.GetCalibrationPoint(i, ref _x, ref _y);
               _calpoints[i].X = _x;
               _calpoints[i].Y = _y;

               _sx[i] = (short)_x;
               _sy[i] = (short)_y;
            }

            // Start the calibration process.
            _currentCalpoint = 0;
            Touch.ActiveTouchPanel.StartCalibration();
         }

         #endregion

         #region Properties

         private bool Done { get; set; }

         #endregion

         #region Touch Overrides

         // ReSharper disable once RedundantAssignment
         protected override void TouchUpMessage(object sender, point point, ref bool handled)
         {
            handled = true;
            try
            {
               if (_step1)
               {
                  _step1 = false;
                  Render(true);
                  return;
               }

               if (Done)
               {
                  return;
               }

               ++_currentCalpoint;
               _cx[_currentCalpoint - 1] = (short) point.X;
               _cy[_currentCalpoint - 1] = (short) point.Y;

               if (_currentCalpoint == _calpoints.Length)
               {
                  Touch.ActiveTouchPanel.SetCalibration(_calpoints.Length, _sx, _sy, _cx, _cy);

                  if (ConfirmCalibration())
                  {
                     // The last point has been reached , so set the 
                     // calibration.
                     Touch.ActiveTouchPanel.SetCalibration(_calpoints.Length, _sx, _sy, _cx, _cy);
                     SaveLCDCalibration(_calpoints.Length, _sx, _sy, _cx, _cy);
                     Done = true;
                     _activeBlock.Set();
                     return;
                  }
                  Touch.ActiveTouchPanel.StartCalibration();
                  _currentCalpoint = 0;
               }

               Render(true);
            }
            finally
            {
               base.TouchUpMessage(sender, point, ref handled);
            }
         }

         #endregion

         #region Private Methods

         private bool ConfirmCalibration()
         {
            var bChecked = new bool[3];
            var rects = new rect[3];
            long lStop = 0;
            int y = Core.Screen.Height / 2 - (_cc.ConfirmationBoxSize + _cc.Font.Height);
            int x = Core.Screen.Width / 2 - (_cc.ConfirmationBoxSize * 3 + _cc.ConfirmationBoxSpacing * 2) / 2;
            const long l100Ms = TimeSpan.TicksPerMillisecond * 100;
            string confLeft;
            string confRight;

            int i = _cc.ConfirmationText.IndexOf("[SECONDS]");
            if (i == 0)
            {
               confLeft = _cc.ConfirmationText;
               confRight = string.Empty;
            }
            else
            {
               confLeft = _cc.ConfirmationText.Substring(0, i);
               confRight = _cc.ConfirmationText.Substring(i + 9);
            }


            rects[0] = new rect(x, y, _cc.ConfirmationBoxSize, _cc.ConfirmationBoxSize);
            rects[1] = new rect(x + _cc.ConfirmationBoxSize + _cc.ConfirmationBoxSpacing, y, _cc.ConfirmationBoxSize, _cc.ConfirmationBoxSize);
            rects[2] = new rect(rects[1].X + _cc.ConfirmationBoxSize + _cc.ConfirmationBoxSpacing, y, _cc.ConfirmationBoxSize, _cc.ConfirmationBoxSize);

            while (true)
            {
               string s;
               if (lStop == 0)
               {
                  Core.Screen.SetClippingRectangle(0, 0, Core.Screen.Width, Core.Screen.Height);
                  Core.Screen.DrawRectangle(0, 0, 0, 0, Width, Height, 0, 0, _cc.BackgroundGradientTop, 0, 0, _cc.BackgroundGradientBottom, 0, Height, 256);

                  // Draw Check boxes
                  DrawCheckbox(rects[0].X, rects[0].Y, false);
                  DrawCheckbox(rects[1].X, rects[1].Y, false);
                  DrawCheckbox(rects[2].X, rects[2].Y, false);

                  // Draw Text
                  if (confRight == string.Empty)
                     s = confLeft;
                  else
                     s = confLeft + _cc.ConfirmationTimeout + confRight;
                  Core.Screen.DrawTextInRect(s, 0, y + 21, Core.Screen.Width, _cc.Font.Height * 2, Bitmap.DT_AlignmentCenter, _cc.ForeColor, _cc.Font);

                  Core.Screen.Flush();

                  lStop = DateTime.Now.Ticks + (TimeSpan.TicksPerSecond * _cc.ConfirmationTimeout);
               }
               else if (lStop < DateTime.Now.Ticks)
                  return false;

               // Get Touch Down
               point p = Core.ManualTouchPoint(DateTime.Now.Ticks + l100Ms).location;

               for (i = 0; i < 3; i++)
               {
                  if (rects[i].Contains(p))
                  {
                     bChecked[i] = true;
                     break;
                  }
               }

               float remain = (float)(lStop - DateTime.Now.Ticks) / TimeSpan.TicksPerSecond;

               // Refresh Screen
               Core.Screen.DrawRectangle(0, 0, 0, 0, Width, Height, 0, 0, _cc.BackgroundGradientTop, 0, 0, _cc.BackgroundGradientBottom, 0, Height, 256);
               DrawCheckbox(rects[0].X, rects[0].Y, bChecked[0]);
               DrawCheckbox(rects[1].X, rects[1].Y, bChecked[1]);
               DrawCheckbox(rects[2].X, rects[2].Y, bChecked[2]);

               if (confRight == string.Empty)
               {
                  s = confLeft;
               }
               else
               {
                  s = confLeft + System.Math.Round(remain) + confRight;
               }
               Core.Screen.DrawTextInRect(s, 0, y + 21, Core.Screen.Width, _cc.Font.Height * 2, Bitmap.DT_AlignmentCenter, _cc.ForeColor, _cc.Font);
               Core.Screen.Flush();

               if (bChecked[0] && bChecked[1] && bChecked[2])
               {
                  Thread.Sleep(100);
                  return true;
               }
            }
         }

         private void DrawCheckbox(int x, int y, bool Checked)
         {
            Core.Screen.DrawRectangle(Colors.DarkGray, 1, x, y, _cc.ConfirmationBoxSize, _cc.ConfirmationBoxSize, 0, 0, Colors.Wheat, x, y, Colors.LightGray, x, y + 14, 256);
            Core.Screen.DrawLine(Colors.Wheat, 1, x, y + _cc.ConfirmationBoxSize, x + _cc.ConfirmationBoxSize, y + _cc.ConfirmationBoxSize);
            Core.Screen.DrawLine(Colors.Wheat, 1, x + _cc.ConfirmationBoxSize, y, x + _cc.ConfirmationBoxSize, y + _cc.ConfirmationBoxSize);

            if (Checked)
            {
               Core.Screen.DrawRectangle(ColorUtility.ColorFromRGB(73, 187, 0), 1, x + 4, y + 4, _cc.ConfirmationBoxSize - 8, _cc.ConfirmationBoxSize - 8, 0, 0, ColorUtility.ColorFromRGB(160, 243, 0), x, y, ColorUtility.ColorFromRGB(73, 187, 0), x, y + _cc.ConfirmationBoxSize, 256);
            }
         }

         #endregion

         #region GUI

         protected override void OnRender(int x, int y, int width, int height)
         {
            if (_step1)
            {
               Core.Screen.DrawRectangle(0, 0, 0, 0, Width, Height, 0, 0, _cc.BackgroundGradientTop, 0, 0, _cc.BackgroundGradientBottom, 0, Height, 256);
               Core.Screen.DrawTextInRect(_cc.InitialText, 0, Height / 2 - _cc.Font.Height / 2, Width, _cc.Font.Height, Bitmap.DT_AlignmentCenter, _cc.ForeColor, _cc.Font);
            }
            else
            {
               Core.Screen.DrawRectangle(0, 0, 0, 0, Width, Height, 0, 0, _cc.BackgroundGradientTop, 0, 0, _cc.BackgroundGradientBottom, 0, Height, 256);
               Core.Screen.DrawLine(_cc.CrossHairColor, 1, _calpoints[_currentCalpoint].X - 10, _calpoints[_currentCalpoint].Y, _calpoints[_currentCalpoint].X - 2, _calpoints[_currentCalpoint].Y);
               Core.Screen.DrawLine(_cc.CrossHairColor, 1, _calpoints[_currentCalpoint].X + 10, _calpoints[_currentCalpoint].Y, _calpoints[_currentCalpoint].X + 2, _calpoints[_currentCalpoint].Y);
               Core.Screen.DrawLine(_cc.CrossHairColor, 1, _calpoints[_currentCalpoint].X, _calpoints[_currentCalpoint].Y - 10, _calpoints[_currentCalpoint].X, _calpoints[_currentCalpoint].Y - 2);
               Core.Screen.DrawLine(_cc.CrossHairColor, 1, _calpoints[_currentCalpoint].X, _calpoints[_currentCalpoint].Y + 10, _calpoints[_currentCalpoint].X, _calpoints[_currentCalpoint].Y + 2);
               Core.Screen.DrawTextInRect(_cc.CalibrationText, 0, Height - _cc.Font.Height - 8, Width, _cc.Font.Height, Bitmap.DT_AlignmentCenter, _cc.ForeColor, _cc.Font);
            }
         }
         #endregion
      }
      #endregion
   }

   #region Calibration Configuration

   /// <summary>
   /// Configuration to be used for calibration
   /// </summary>
   /// <remarks>
   /// The calibration configuration allows to customize the appearance of the calibration window.
   /// </remarks>
   public class CalibrationConfiguration
   {
      #region Constants

      // ReSharper disable InconsistentNaming

      /// <summary>
      /// Default text for initial calibration text
      /// </summary>
      public const string INIT_CALI_TEXT = "Tap to Begin Calibration";

      /// <summary>
      /// Default text for calibration instruction
      /// </summary>
      public const string CNFG_CALI_TEXT = "Tap cross hairs to Calibrate";

      /// <summary>
      /// Default text for calibration verification
      /// </summary>
      public const string CONF_CALI_TEXT = "Tap all 3 boxes to confirm calibration.\nRestarting Calibration in [SECONDS] seconds";

      // ReSharper restore InconsistentNaming

      #endregion

      #region Constructor

      /// <summary>
      /// Creates a new calibration configuration
      /// </summary>
      public CalibrationConfiguration()
      {
         BackgroundGradientTop = Colors.White;
         BackgroundGradientBottom = Colors.White;
         ForeColor = ColorUtility.ColorFromRGB(0, 161, 241);
         CrossHairColor = ColorUtility.ColorFromRGB(246, 83, 20);
         InitialText = INIT_CALI_TEXT;
         CalibrationText = CNFG_CALI_TEXT;
         ConfirmationText = CONF_CALI_TEXT;
         ConfirmationTimeout = 30;
         ConfirmationBoxSize = 20;
         Font = Resources.GetFont(Resources.FontResources.droid12);
         ConfirmationBoxSpacing = 50;
      }

      #endregion

      #region Properties

      /// <summary>
      /// Gets/Sets the bottom gradient color of the background
      /// </summary>
      public Color BackgroundGradientBottom { get; set; }

      /// <summary>
      /// Gets/Sets the top gradient color of the background
      /// </summary>
      public Color BackgroundGradientTop { get; set; }

      /// <summary>
      /// Gets/Sets the calibration instruction text
      /// </summary>
      public string CalibrationText { get; set; }

      /// <summary>
      /// Gets/Sets the confirmation box size
      /// </summary>
      public int ConfirmationBoxSize { get; set; }

      /// <summary>
      /// Gets/Sets the confirmation box spacing
      /// </summary>
      public int ConfirmationBoxSpacing { get; set; }

      /// <summary>
      /// Gets/Sets the confirmation text
      /// </summary>
      public string ConfirmationText { get; set; }

      /// <summary>
      /// Gets/Sets the confirmation timeout in seconds.
      /// </summary>
      public int ConfirmationTimeout { get; set; }

      /// <summary>
      /// Gets/Sets the color of the cross hairs
      /// </summary>
      public Color CrossHairColor { get; set; }

      /// <summary>
      /// Gets/Sets the font to be used to render the text
      /// </summary>
      public Font Font { get; set; }

      /// <summary>
      /// Gets/Sets the color to render the text
      /// </summary>
      public Color ForeColor { get; set; }

      /// <summary>
      /// Gets/Sets the initial text
      /// </summary>
      public string InitialText { get; set; }

      #endregion
   }
   #endregion
}
// ReSharper restore StringIndexOfIsCultureSpecific.1
