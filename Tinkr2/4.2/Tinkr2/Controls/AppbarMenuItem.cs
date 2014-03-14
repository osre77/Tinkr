using System;
using Skewworks.NETMF.Controls;

namespace Skewworks.Tinkr.Controls
{
   [Serializable]
   public class AppbarMenuItem : Control
   {

      #region Variables

      private string _text;
      int _x, _y, _w, _h;

      #endregion

      #region Constructors

      public AppbarMenuItem(string name, string text)
      {
         Name = name;
         _text = text;
      }

      #endregion

      #region Properties

      public override IContainer Parent
      {
         get
         {
            return base.Parent;
         }
         set
         {
            throw new Exception("AppbarMenuItems can only be added to Appbars");
         }
      }

      public override int Height
      {
         get { return _h; }
         set
         {
            if (_h == value)
               return;
            _h = value;
            Invalidate();
         }
      }

      public string Text
      {
         get { return _text; }
         set
         {
            if (_text == value)
               return;
            _text = value;
            Invalidate();
         }
      }

      public override int Width
      {
         get { return _w; }
         set
         {
            if (_w == value)
               return;
            _w = value;
            Invalidate();
         }
      }

      public override int X
      {
         get { return _x; }
         set
         {
            if (_x == value)
               return;
            _x = value;
            Invalidate();
         }
      }

      public override int Y
      {
         get { return _y; }
         set
         {
            if (_y == value)
               return;
            _y = value;
            Invalidate();
         }
      }

      #endregion

      #region Internal Methods

      protected internal void UpdateBounds(int x, int y, int width, int height)
      {
         _x = X;
         _y = Y;
         _w = Width;
         _h = Height;
      }

      #endregion

   }
}
