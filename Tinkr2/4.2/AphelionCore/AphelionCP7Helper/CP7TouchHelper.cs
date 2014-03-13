using System;
using Skewworks.NETMF;

namespace Skewworks.Gadgeteer.CP7Helper
{
   public class CP7TouchHelper
   {
      #region Variables

      private point _ptLast;
      private point _ptDownAt;
      private long _lgDownAt;
      private TouchType _tt;
      private bool _tDown;
      private bool _cancelSwipe;

      #endregion

      #region Public Methods

      public void CP7Pressed(point e)
      {
         _ptLast = e;

         if (_ptLast.X < 800)
         {

            if (!_tDown)
            {
               _tDown = true;
               _tt = TouchType.NoGesture;
               _cancelSwipe = false;
               _ptDownAt = _ptLast;
               _lgDownAt = DateTime.Now.Ticks;

               try
               {
                  Core.RaiseTouchEvent(TouchType.TouchDown, _ptLast);
               }
               // ReSharper disable once EmptyGeneralCatchClause
               catch
               { }
            }
            else
            {
               if (!_cancelSwipe)
                  CalcDir(_ptLast);

               try
               {
                  Core.RaiseTouchEvent(TouchType.TouchMove, _ptLast);
               }
               // ReSharper disable once EmptyGeneralCatchClause
               catch
               { }
            }
         }
      }

      public void CP7Released()
      {
         _tDown = false;

         if (!_cancelSwipe && _tt != TouchType.NoGesture)
            CalcForce(_ptLast);

         try
         {
            if (_ptLast.X > 800)
            {
               if (_ptLast.Y >= 0 && _ptLast.Y <= 50)
                  Core.RaiseButtonEvent((int)ButtonIDs.Home, false);
               else if (_ptLast.Y >= 100 && _ptLast.Y <= 150)
                  Core.RaiseButtonEvent((int)ButtonIDs.Menu, false);
               else if (_ptLast.Y >= 200 && _ptLast.Y <= 250)
                  Core.RaiseButtonEvent((int)ButtonIDs.Back, false);
            }
            else
               Core.RaiseTouchEvent(TouchType.TouchUp, _ptLast);
         }
         // ReSharper disable once EmptyGeneralCatchClause
         catch
         { }
      }

      public void CP7HomePressed()
      {
         try
         {
            Core.RaiseButtonEvent((int)ButtonIDs.Home, true);
         }
         // ReSharper disable once EmptyGeneralCatchClause
         catch
         { }
      }

      public void CP7MenuPressed()
      {
         try
         {
            Core.RaiseButtonEvent((int)ButtonIDs.Menu, true);
         }
         // ReSharper disable once EmptyGeneralCatchClause
         catch
         { }
      }

      public void CP7BackPressed()
      {
         try
         {
            Core.RaiseButtonEvent((int)ButtonIDs.Back, true);
         }
         // ReSharper disable once EmptyGeneralCatchClause
         catch
         { }
      }

      #endregion

      #region Private Methods

      private void CalcDir(point e)
      {
         var sw = TouchType.NoGesture;

         int d = (e.Y - _ptDownAt.Y);
         if (d > 50)
         {
            sw = TouchType.GestureDown;
         }
         else if (d < -50)
         {
            sw = TouchType.GestureUp;
         }

         d = (e.X - _ptDownAt.X);
         if (d > 50)
         {
            if (sw == TouchType.GestureUp)
            {
               sw = TouchType.GestureUpRight;
            }
            else if (sw == TouchType.GestureDown)
            {
               sw = TouchType.GestureDownRight;
            }
            else
            {
               sw = TouchType.GestureRight;
            }
         }
         else if (d < -50)
         {
            if (sw == TouchType.GestureUp)
            {
               sw = TouchType.GestureUpLeft;
            }
            else if (sw == TouchType.GestureDown)
            {
               sw = TouchType.GestureDownLeft;
            }
            else
            {
               sw = TouchType.GestureLeft;
            }
         }

         if (_tt == TouchType.NoGesture)
         {
            _tt = sw;
         }
         else if (_tt != sw)
         {
            _cancelSwipe = true;
         }
      }

      private void CalcForce(point e)
      {
         // Calc by time alone
         float dDiff = DateTime.Now.Ticks - _lgDownAt;

         if (dDiff > TimeSpan.TicksPerSecond * .75)
         {
            return;
         }

         // 1.0 = < 1/7th second
         dDiff = TimeSpan.TicksPerSecond / 7 / dDiff;
         if (dDiff > 0.99)
         {
            dDiff = 0.99f;
         }
         else if (dDiff < 0)
         {
            dDiff = 0;
         }

         // Raise TouchEvent
         Core.RaiseTouchEvent(_tt, e, dDiff);
      }

      #endregion

   }
}
