using System;

namespace Skewworks.NETMF
{

   /// <summary>
   /// point coordinates
   /// </summary>
   [Serializable]
   // ReSharper disable once InconsistentNaming
   public struct point
   {
      public int X;
      public int Y;

      public point(int x, int y)
      {
         X = x;
         Y = y;
      }

      public override string ToString()
      {
         return X + ", " + Y;
      }
   }

   [Serializable]
   // ReSharper disable once InconsistentNaming
   public struct precisionpoint
   {
      public float X;
      public float Y;

      public precisionpoint(float x, float y)
      {
         X = x;
         Y = y;
      }

      public override string ToString()
      {
         return X + ", " + Y;
      }
   }

   /// <summary>
   /// Rectangle (rect) Structure
   /// </summary>
   [Serializable]
   // ReSharper disable once InconsistentNaming
   public struct rect
   {

      #region Variables

      private int _x;
      private int _y;
      private int _w;
      private int _h;

      #endregion

      #region Constructor

      /// <summary>
      /// Creates a new rect
      /// </summary>
      /// <param name="x">x location of rect</param>
      /// <param name="y">y location of rect</param>
      /// <param name="width">width of rect</param>
      /// <param name="height">height of rect</param>
      public rect(int x, int y, int width, int height)
      {
         _x = x;
         _y = y;
         _w = width;
         _h = height;
      }

      #endregion

      #region Properties

      /// <summary>
      /// Gets/Sets x location of rect
      /// </summary>
      public int X
      {
         get { return _x; }
         set { _x = value; }
      }

      /// <summary>
      /// Gets/Sets y location of rect
      /// </summary>
      public int Y
      {
         get { return _y; }
         set { _y = value; }
      }

      /// <summary>
      /// Gets/Sets width of rect
      /// </summary>
      public int Width
      {
         get { return _w; }
         set
         {
            if (value < 0)
               throw new IndexOutOfRangeException("width cannot be less than 0");
            _w = value;
         }
      }

      /// <summary>
      /// Gets/Sets height of rect
      /// </summary>
      public int Height
      {
         get { return _h; }
         set
         {
            if (value < 0)
               throw new IndexOutOfRangeException("height cannot be less than 0");
            _h = value;
         }
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// Adds the area of a second rect to current rect
      /// </summary>
      /// <param name="newrect">rect to add</param>
      public void Combine(rect newrect)
      {
         if (_w == 0)
         {
            _x = newrect.X;
            _y = newrect.Y;
            _w = newrect.Width;
            _h = newrect.Height;
            return;
         }

         int x1 = (_x < newrect.X) ? _x : newrect.X;
         int y1 = (_y < newrect.Y) ? _y : newrect.Y;
         int x2 = (_x + Width > newrect.X + newrect.Width) ? _x + _w : newrect.X + newrect.Width;
         int y2 = (_y + Height > newrect.Y + newrect.Height) ? _y + _h : newrect.Y + newrect.Height;
         _x = x1;
         _y = y1;
         _w = x2 - x1;
         _h = y2 - y1;
      }

      /// <summary>
      /// Returns the combination of two rects
      /// </summary>
      /// <param name="region1">rect 1</param>
      /// <param name="region2">rect 2</param>
      /// <returns>Combined rect</returns>
      public rect Combine(rect region1, rect region2)
      {
         if (region1.Width == 0)
            return region2;
         if (region2.Width == 0)
            return region1;

         int x1 = (region1.X < region2.X) ? region1.X : region2.X;
         int y1 = (region1.Y < region2.Y) ? region1.Y : region2.Y;
         int x2 = (region1.X + region1.Width > region2.X + region2.Width) ? region1.X + region1.Width : region2.X + region2.Width;
         int y2 = (region1.Y + region1.Height > region2.Y + region2.Height) ? region1.Y + region1.Height : region2.Y + region2.Height;
         return new rect(x1, y1, x2 - x1, y2 - y1);
      }

      /// <summary>
      /// Checks if a point is inside the rect
      /// </summary>
      /// <param name="x">x location</param>
      /// <param name="y">y location</param>
      /// <returns>True if point is inside rect</returns>
      public bool Contains(int x, int y)
      {
         if (x >= _x && x <= _x + _w && y >= _y && y <= _y + _h)
            return true;
         return false;
      }

      /// <summary>
      /// Checks if a point is inside the rect
      /// </summary>
      /// <param name="e">point to check</param>
      /// <returns>True if point is inside rect</returns>
      public bool Contains(point e)
      {
         if (e.X >= _x && e.X <= _x + _w && e.Y >= _y && e.Y <= _y + _h)
            return true;
         return false;
      }

      /// <summary>
      /// Checks to see if two rects interset
      /// </summary>
      /// <param name="area">rect to check</param>
      /// <returns>True if rects intersect</returns>
      public bool Intersects(rect area)
      {
         return !(area.X >= (_x + _w)
                 || (area.X + area.Width) <= _x
                 || area.Y >= (_y + _h)
                 || (area.Y + area.Height) <= _y
                 );
      }

      /// <summary>
      /// Returns the intersection of two rects
      /// </summary>
      /// <param name="region1">rect 1</param>
      /// <param name="region2">rect 2</param>
      /// <returns>Intersected rect</returns>
      public static rect Intersect(rect region1, rect region2)
      {
         if (!region1.Intersects(region2))
            return new rect(0, 0, 0, 0);

         var rct = new rect
         {
            X = (region1.X > region2.X) ? region1.X : region2.X,
            Y = (region1.Y > region2.Y) ? region1.Y : region2.Y
         };

         // For X1 & Y1 we'll want the highest value

         // For X2 & Y2 we'll want the lowest value
         int r1V2 = region1.X + region1.Width;
         int r2V2 = region2.X + region2.Width;
         rct.Width = (r1V2 < r2V2) ? r1V2 - rct.X : r2V2 - rct.X;
         r1V2 = region1.Y + region1.Height;
         r2V2 = region2.Y + region2.Height;
         rct.Height = (r1V2 < r2V2) ? r1V2 - rct.Y : r2V2 - rct.Y;

         return rct;
      }

      /// <summary>
      /// Returns a string representation of the rect
      /// </summary>
      /// <returns>{x, y, width, height}</returns>
      public override string ToString()
      {
         return "{" + X + ", " + Y + ", " + Width + ", " + Height + "}";
      }

      #endregion

   }


   /// <summary>
   /// Structure containing object height & width
   /// </summary>
   [Serializable]
// ReSharper disable once InconsistentNaming
   public struct size
   {

      #region Variables

      /// <summary>
      /// width of the object
      /// </summary>
      public int Width;

      /// <summary>
      /// height of the object
      /// </summary>
      public int Height;

      #endregion

      #region Constructor

      /// <summary>
      /// Creates a new Size
      /// </summary>
      /// <param name="width"></param>
      /// <param name="height"></param>
      public size(int width, int height)
      {
         if (width < 0)
            throw new ArgumentOutOfRangeException("width", @"Must be > 0");
         if (height < 0)
            throw new ArgumentOutOfRangeException("height", @"Must be > 0");
         Width = width;
         Height = height;
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// Adds height & width to existing size
      /// </summary>
      /// <param name="addWidth"></param>
      /// <param name="addHeight"></param>
      public void Grow(int addWidth, int addHeight)
      {
         Width += addWidth;
         Height += addHeight;
      }

      /// <summary>
      /// Subtracts height & width from existing size
      /// </summary>
      /// <param name="subtractWidth"></param>
      /// <param name="subtractHeight"></param>
      public void Shrink(int subtractWidth, int subtractHeight)
      {
         Width += subtractWidth;
         if (Width < 0)
            Width = 0;
         Height += subtractHeight;
         if (Height < 0)
            Height = 0;
      }

      /// <summary>
      /// Returns a string representation of the Size
      /// </summary>
      /// <returns>{width, height}</returns>
      public override string ToString()
      {
         return "{" + Width + ", " + Height + "}";
      }

      #endregion

   }

   [Serializable]
   public struct TouchEventArgs
   {
// ReSharper disable once InconsistentNaming
      public point location;
// ReSharper disable once InconsistentNaming
      public int type;
      public TouchEventArgs(point e, int type)
      {
         location = e;
         this.type = type;
      }
      public override string ToString()
      {
         return location + "; " + type;
      }
   }


}

namespace Skewworks.NETMF.Applications
{
   /// <summary>
   /// Application Details
   /// (Skewworks NETMF Application Standard v2.0)
   /// </summary>
   [Serializable]
   public struct ApplicationDetails
   {

      #region Variables

      /// <summary>
      /// Owning Company
      /// </summary>
      public string Company;

      /// <summary>
      /// Application Copyright
      /// </summary>
      public string Copyright;

      /// <summary>
      /// Application Title
      /// </summary>
      public string Title;

      /// <summary>
      /// Application Description
      /// </summary>
      public string Description;

      /// <summary>
      /// Application Version
      /// </summary>
      public string Version;

      #endregion

      #region Constructor

      /// <summary>
      /// Application Details
      /// (Skewworks NETMF Application Standard v2.0)
      /// </summary>
      /// <param name="title">Application Title</param>
      /// <param name="description">Application Description</param>
      /// <param name="company">Owning Company</param>
      /// <param name="copyright">Application Copyright</param>
      /// <param name="version">Application Version</param>
      public ApplicationDetails(string title, string description, string company, string copyright, string version)
      {
         Company = company;
         Copyright = copyright;
         Title = title;
         Description = description;
         Version = version;
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// String representation of the object
      /// </summary>
      /// <returns>Title (v Version)</returns>
      public override string ToString()
      {
         return Title + " (v " + Version + ")";
      }

      #endregion

   }

   [Serializable]
   public struct ApplicationImage
   {

      #region Variables

      /// <summary>
      /// Raw Image Data
      /// </summary>
      public byte[] ImageData;

      /// <summary>
      /// Size of image
      /// </summary>
      public size ImageSize;

      /// <summary>
      /// Image Type
      /// </summary>
      public ImageType ImageType;

      #endregion

      public ApplicationImage(byte[] data, size size, ImageType type)
      {
         ImageData = data;
         ImageSize = size;
         ImageType = type;
      }

   }
}
