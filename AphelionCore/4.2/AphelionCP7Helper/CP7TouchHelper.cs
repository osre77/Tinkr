using System;

using Microsoft.SPOT;

using Skewworks.NETMF;

namespace Skewworks.Gadgeteer.CP7Helper
{
    public class CP7TouchHelper
    {

        #region Variables

        private point ptLast;
        private point _ptDownAt;
        private long _lgDownAt;
        private TouchType _tt;
        private bool _tDown;
        private bool _cancelSwipe;

        #endregion

        #region Public Methods

        public void CP7Pressed(point e)
        {
            ptLast = e;

            if (ptLast.X < 800)
            {

                if (!_tDown)
                {
                    _tDown = true;
                    _tt = TouchType.NoGesture;
                    _cancelSwipe = false;
                    _ptDownAt = ptLast;
                    _lgDownAt = DateTime.Now.Ticks;

                    try
                    {
                        Core.RaiseTouchEvent(TouchType.TouchDown, ptLast);
                    }
                    catch (Exception) { }
                }
                else
                {
                    if (!_cancelSwipe)
                        CalcDir(ptLast);

                    try
                    {
                        Core.RaiseTouchEvent(TouchType.TouchMove, ptLast);
                    }
                    catch (Exception) { }
                }
            }
        }

        public void CP7Released()
        {
            _tDown = false;

            if (!_cancelSwipe && _tt != TouchType.NoGesture)
                CalcForce(ptLast);

            try
            {
                if (ptLast.X > 800)
                {
                    if (ptLast.Y >= 0 && ptLast.Y <= 50)
                        Core.RaiseButtonEvent((int)ButtonIDs.Home, false);
                    else if (ptLast.Y >= 100 && ptLast.Y <= 150)
                        Core.RaiseButtonEvent((int)ButtonIDs.Menu, false);
                    else if (ptLast.Y >= 200 && ptLast.Y <= 250)
                        Core.RaiseButtonEvent((int)ButtonIDs.Back, false);
                }
                else
                    Core.RaiseTouchEvent(TouchType.TouchUp, ptLast);
            }
            catch (Exception) { }
        }

        public void CP7HomePressed()
        {
            try
            {
                Core.RaiseButtonEvent((int)ButtonIDs.Home, true);
            }
            catch (Exception) { }
        }

        public void CP7MenuPressed()
        {
            try
            {
                Core.RaiseButtonEvent((int)ButtonIDs.Menu, true);
            }
            catch (Exception) { }
        }

        public void CP7BackPressed()
        {
            try
            {
                Core.RaiseButtonEvent((int)ButtonIDs.Back, true);
            }
            catch (Exception) { }
        }

        #endregion

        #region Private Methods

        private void CalcDir(point e)
        {
            TouchType sw = TouchType.NoGesture;
            int d;

            d = (e.Y - _ptDownAt.Y);
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
            // Calc by time alone
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
            Core.RaiseTouchEvent(_tt, e, dDiff);
        }

        #endregion

    }
}
