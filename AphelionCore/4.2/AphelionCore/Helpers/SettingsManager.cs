using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Skewworks.NETMF.Controls;

namespace Skewworks.NETMF
{
    public class SettingsManager
    {

        #region Variables

        private static ManualResetEvent _activeBlock = null;            // Used to display Modal Forms
        private static bool _bLoaded;

        #endregion

        #region Properties

        /// <summary>
        /// Returns current LCD settings
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
        /// Calibrates touch input for LCDs (blocking)
        /// </summary>
        public static void CalibrateTouch(CalibrationConfiguration cc)
        {
            IContainer prv = Core.ActiveContainer;
            CalibrationWindow cw = new CalibrationWindow(cc);
            Core.ActiveContainer = cw;

            _activeBlock = new ManualResetEvent(false);
            _activeBlock.Reset();

            while (!_activeBlock.WaitOne(1000, false))
                ;

            _activeBlock = null;

            Core.ActiveContainer = prv;
            cw = null;
        }

        #endregion

        #region LCD Settings

        private class LCDEWR { }
        private static ExtendedWeakReference LCDEWRSettings;
        private static LCDSettings _lcd;

        [Serializable]
        public class LCDSettings
        {
            public LCDSettings(ScreenCalibration iCalibrate, int iCalibrationpoints, short[] icalibrationSX, short[] icalibrationSY, short[] icalibrationCX, short[] icalibrationCY)
            {
                this.calibrateLCD = iCalibrate;
                this.calibrationpoints = iCalibrationpoints;
                this.calibrationSX = icalibrationSX;
                this.calibrationSY = icalibrationSY;
                this.calibrationCX = icalibrationCX;
                this.calibrationCY = icalibrationCY;
            }

            // Fields must be serializable.
            public ScreenCalibration calibrateLCD;
            public int calibrationpoints;
            public short[] calibrationSX;
            public short[] calibrationSY;
            public short[] calibrationCX;
            public short[] calibrationCY;
        }

        private static void LoadLCDSettings()
        {
            // Grab the data
            LCDEWRSettings = ExtendedWeakReference.RecoverOrCreate(typeof(LCDEWR), 0, ExtendedWeakReference.c_SurvivePowerdown);
            LCDEWRSettings.Priority = (Int32)ExtendedWeakReference.PriorityLevel.Critical;
            _lcd = (LCDSettings)LCDEWRSettings.Target;
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
        /// <param name="points"></param>
        /// <param name="SX"></param>
        /// <param name="SY"></param>
        /// <param name="CX"></param>
        /// <param name="CY"></param>
        public static void RestoreLCDCalibration(int points, short[] SX, short[] SY, short[] CX, short[] CY)
        {
            // Set calibration
            Touch.ActiveTouchPanel.SetCalibration(points, SX, SY, CX, CY);
        }

        /// <summary>
        /// Save touch calibration to EWR
        /// </summary>
        /// <param name="iCalibrationpoints"></param>
        /// <param name="icalibrationSX"></param>
        /// <param name="icalibrationSY"></param>
        /// <param name="icalibrationCX"></param>
        /// <param name="icalibrationCY"></param>
        /// <returns></returns>
        public static bool SaveLCDCalibration(int iCalibrationpoints, short[] icalibrationSX, short[] icalibrationSY, short[] icalibrationCX, short[] icalibrationCY)
        {
            // Save calibration
            if (_lcd == null)
                _lcd = new LCDSettings(ScreenCalibration.Restore, iCalibrationpoints, icalibrationSX, icalibrationSY, icalibrationCX, icalibrationCY);
            else
            {
                _lcd.calibrateLCD = ScreenCalibration.Restore;
                _lcd.calibrationpoints = iCalibrationpoints;
                _lcd.calibrationSX = icalibrationSX;
                _lcd.calibrationSY = icalibrationSY;
                _lcd.calibrationCX = icalibrationCX;
                _lcd.calibrationCY = icalibrationCY;
            }

            LCDEWRSettings.Target = _lcd;

            return true;
        }

        #endregion

        #region Cailbration Control

        private class CalibrationWindow : Container
        {

            #region Variables

            private bool _done;

            point[] calpoints = null;
            int currentCalpoint = 0;
            short[] sx = null;
            short[] sy = null;
            short[] cx = null;
            short[] cy = null;
            int i = 0;
            int x = 0;
            int y = 0;
            int calibrationpointCount = 0;

            bool _step1;
            CalibrationConfiguration _cc;

            #endregion

            #region Constructor

            public CalibrationWindow(CalibrationConfiguration cc)
            {
                _cc = cc;
                _step1 = true;

                X = 0;
                Y = 0;
                Width = Core.Screen.Width;
                Height = Core.Screen.Height;


                // Ask the touch system how many points are needed to 
                // calibrate.
                Touch.ActiveTouchPanel.GetCalibrationPointCount(ref calibrationpointCount);

                // Create the calibration point array.
                calpoints = new point[calibrationpointCount];
                sx = new short[calibrationpointCount];
                sy = new short[calibrationpointCount];
                cx = new short[calibrationpointCount];
                cy = new short[calibrationpointCount];

                // Get the points for calibration.
                for (i = 0; i < calibrationpointCount; i++)
                {
                    Touch.ActiveTouchPanel.GetCalibrationPoint(i, ref x, ref y);
                    calpoints[i].X = x;
                    calpoints[i].Y = y;

                    sx[i] = (short)x;
                    sy[i] = (short)y;
                }

                // Start the calibration process.
                currentCalpoint = 0;
                Touch.ActiveTouchPanel.StartCalibration();
            }

            #endregion

            #region Properties

            public bool Done
            {
                get { return _done; }
            }

            #endregion

            #region Touch Overrides

            protected override void TouchUpMessage(object sender, point e, ref bool handled)
            {
                handled = true;

                if (_step1)
                {
                    _step1 = false;
                    Render(true);
                    return;
                }

                if (_done)
                    return;

                ++currentCalpoint;
                cx[currentCalpoint - 1] = (short)e.X;
                cy[currentCalpoint - 1] = (short)e.Y;

                if (currentCalpoint == calpoints.Length)
                {
                    Touch.ActiveTouchPanel.SetCalibration(calpoints.Length, sx, sy, cx, cy);

                    if (ConfirmCalibration())
                    {
                        // The last point has been reached , so set the 
                        // calibration.
                        Touch.ActiveTouchPanel.SetCalibration(calpoints.Length, sx, sy, cx, cy);
                        SettingsManager.SaveLCDCalibration(calpoints.Length, sx, sy, cx, cy);
                        _done = true;
                        _activeBlock.Set();
                        return;
                    }
                    else
                    {
                        Touch.ActiveTouchPanel.StartCalibration();
                        currentCalpoint = 0;
                    }
                }

                Render(true);
            }

            #endregion

            #region Private Methods

            private bool ConfirmCalibration()
            {
                bool[] bChecked = new bool[3];
                rect[] rects = new rect[3];
                long lStop = 0;
                int y = Core.Screen.Height / 2 - (_cc.ConfirmationBoxSize + _cc.Font.Height);
                int x = Core.Screen.Width / 2 - (_cc.ConfirmationBoxSize * 3 + _cc.ConfirmationBoxSpacing * 2) / 2;
                Color cBlu = ColorUtility.ColorFromRGB(101, 155, 252);
                point p;
                long l100ms = TimeSpan.TicksPerMillisecond * 100;
                int i;
                float remain;
                string s;
                string _confLeft, _confRight;

                i = _cc.ConfirmationText.IndexOf("[SECONDS]");
                if (i == 0)
                {
                    _confLeft = _cc.ConfirmationText;
                    _confRight = string.Empty;
                }
                else
                {
                    _confLeft = _cc.ConfirmationText.Substring(0, i);
                    _confRight = _cc.ConfirmationText.Substring(i + 9);
                }


                rects[0] = new rect(x, y, _cc.ConfirmationBoxSize, _cc.ConfirmationBoxSize);
                rects[1] = new rect(x + _cc.ConfirmationBoxSize + _cc.ConfirmationBoxSpacing, y, _cc.ConfirmationBoxSize, _cc.ConfirmationBoxSize);
                rects[2] = new rect(rects[1].X + _cc.ConfirmationBoxSize + _cc.ConfirmationBoxSpacing, y, _cc.ConfirmationBoxSize, _cc.ConfirmationBoxSize);

                while (true)
                {
                    if (lStop == 0)
                    {
                        Core.Screen.SetClippingRectangle(0, 0, Core.Screen.Width, Core.Screen.Height);
                        Core.Screen.DrawRectangle(0, 0, 0, 0, Width, Height, 0, 0, _cc.BackgroundGradientTop, 0, 0, _cc.BackgroundGradientBottom, 0, Height, 256);

                        // Draw Checkboxes
                        DrawCheckbox(rects[0].X, rects[0].Y, false);
                        DrawCheckbox(rects[1].X, rects[1].Y, false);
                        DrawCheckbox(rects[2].X, rects[2].Y, false);

                        // Draw Text
                        if (_confRight == string.Empty)
                            s = _confLeft;
                        else
                            s = _confLeft + _cc.ConfirmationTimeout + _confRight;
                        Core.Screen.DrawTextInRect(s, 0, y + 21, Core.Screen.Width, _cc.Font.Height * 2, Bitmap.DT_AlignmentCenter, _cc.ForeColor, _cc.Font);

                        Core.Screen.Flush();

                        lStop = DateTime.Now.Ticks + (TimeSpan.TicksPerSecond * _cc.ConfirmationTimeout);
                    }
                    else if (lStop < DateTime.Now.Ticks)
                        return false;

                    // Get Touch Down
                    p = Core.ManualTouchPoint(DateTime.Now.Ticks + l100ms).location;

                    for (i = 0; i < 3; i++)
                    {
                        if (rects[i].Contains(p))
                        {
                            bChecked[i] = true;
                            break;
                        }
                    }

                    remain = (float)(lStop - DateTime.Now.Ticks) / TimeSpan.TicksPerSecond;

                    // Refresh Screen
                    Core.Screen.DrawRectangle(0, 0, 0, 0, Width, Height, 0, 0, _cc.BackgroundGradientTop, 0, 0, _cc.BackgroundGradientBottom, 0, Height, 256);
                    DrawCheckbox(rects[0].X, rects[0].Y, bChecked[0]);
                    DrawCheckbox(rects[1].X, rects[1].Y, bChecked[1]);
                    DrawCheckbox(rects[2].X, rects[2].Y, bChecked[2]);

                    if (_confRight == string.Empty)
                        s = _confLeft;
                    else
                        s = _confLeft + System.Math.Round((double)remain) + _confRight;
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
                    Core.Screen.DrawRectangle(ColorUtility.ColorFromRGB(73, 187, 0), 1, x + 4, y + 4, _cc.ConfirmationBoxSize - 8, _cc.ConfirmationBoxSize - 8, 0, 0, ColorUtility.ColorFromRGB(160, 243, 0), x, y, ColorUtility.ColorFromRGB(73, 187, 0), x, y + _cc.ConfirmationBoxSize, 256);

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
                    Core.Screen.DrawLine(_cc.CrossHairColor, 1, calpoints[currentCalpoint].X - 10, calpoints[currentCalpoint].Y, calpoints[currentCalpoint].X - 2, calpoints[currentCalpoint].Y);
                    Core.Screen.DrawLine(_cc.CrossHairColor, 1, calpoints[currentCalpoint].X + 10, calpoints[currentCalpoint].Y, calpoints[currentCalpoint].X + 2, calpoints[currentCalpoint].Y);
                    Core.Screen.DrawLine(_cc.CrossHairColor, 1, calpoints[currentCalpoint].X, calpoints[currentCalpoint].Y - 10, calpoints[currentCalpoint].X, calpoints[currentCalpoint].Y - 2);
                    Core.Screen.DrawLine(_cc.CrossHairColor, 1, calpoints[currentCalpoint].X, calpoints[currentCalpoint].Y + 10, calpoints[currentCalpoint].X, calpoints[currentCalpoint].Y + 2);
                    Core.Screen.DrawTextInRect(_cc.CalibrationText, 0, Height - _cc.Font.Height - 8, Width, _cc.Font.Height, Bitmap.DT_AlignmentCenter, _cc.ForeColor, _cc.Font);
                }
            }

            #endregion

        }

        #endregion

    }

    #region Calibration Configuration

    public class CalibrationConfiguration
    {

        #region Constants

        public const string INIT_CALI_TEXT = "Tap to Begin Calibration";
        public const string CNFG_CALI_TEXT = "Tap Crosshairs to Calibrate";
        public const string CONF_CALI_TEXT = "Tap all 3 boxes to confirm calibration.\nRestarting Calibration in [SECONDS] seconds";

        #endregion

        #region Variables

        private Color _bkg1;        // Starting background gradient color
        private Color _bkg2;        // Ending background gradient color
        private Color _fore;        // Text color
        private Font _fnt;          // Font
        private Color _cross;       // Crosshair color
        private string _init;       // Initial Text
        private string _cali;       // Calibration Text
        private string _cnft;       // Confirmation Text
        private int _cTime;         // Confirmation Timeout
        private int _bxSz;          // Box Size
        private int _space;         // Box Spacing

        #endregion

        #region Constructor

        public CalibrationConfiguration()
        {
            _bkg1 = Colors.White;
            _bkg2 = Colors.White;
            _fore = ColorUtility.ColorFromRGB(0, 161, 241);
            _cross = ColorUtility.ColorFromRGB(246, 83, 20);
            _init = INIT_CALI_TEXT;
            _cali = CNFG_CALI_TEXT;
            _cnft = CONF_CALI_TEXT;
            _cTime = 30;
            _bxSz = 20;
            _fnt = Resources.GetFont(Resources.FontResources.droid12);
            _space = 50;
        }

        #endregion

        #region Properties

        public Color BackgroundGradientBottom
        {
            get { return _bkg2; }
            set { _bkg2 = value; }
        }

        public Color BackgroundGradientTop
        {
            get { return _bkg1; }
            set { _bkg1 = value; }
        }

        public string CalibrationText
        {
            get { return _cali; }
            set { _cali = value; }
        }

        public int ConfirmationBoxSize
        {
            get { return _bxSz; }
            set { _bxSz = value; }
        }

        public int ConfirmationBoxSpacing
        {
            get { return _space; }
            set { _space = value; }
        }

        public string ConfirmationText
        {
            get { return _cnft; }
            set { _cnft = value; }
        }

        public int ConfirmationTimeout
        {
            get { return _cTime; }
            set { _cTime = value; }
        }

        public Color CrossHairColor
        {
            get { return _cross; }
            set { _cross = value; }
        }

        public Font Font
        {
            get { return _fnt; }
            set { _fnt = value; }
        }

        public Color ForeColor
        {
            get { return _fore; }
            set { _fore = value; }
        }

        public string InitialText
        {
            get { return _init; }
            set { _init = value; }
        }

        #endregion

    }

    #endregion

}