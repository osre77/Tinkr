using System;
using System.Diagnostics;

namespace Skewworks.NETMF
{

   /// <summary>
   /// Integer point coordinates
   /// </summary>
   [Serializable]
   [DebuggerDisplay("Point({X}, {Y})")]
   // ReSharper disable once InconsistentNaming
   public struct point
   {
      /// <summary>
      /// X coordinate
      /// </summary>
      public int X;

      /// <summary>
      /// Y coordinate
      /// </summary>
      public int Y;

      /// <summary>
      /// Create a new point
      /// </summary>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      public point(int x, int y)
      {
         X = x;
         Y = y;
      }

      /// <summary>
      /// Returns a string that represents the current object.
      /// </summary>
      /// <returns>
      /// A string that represents the current object.
      /// </returns>
      /// <filterpriority>2</filterpriority>
      public override string ToString()
      {
         return X + ", " + Y;
      }
   }

   /// <summary>
   /// Floating-point point coordinates
   /// </summary>
   [Serializable]
   [DebuggerDisplay("Point({X}, {Y})")]
   // ReSharper disable once InconsistentNaming
   public struct precisionpoint
   {
      /// <summary>
      /// X coordinate
      /// </summary>
      public float X;

      /// <summary>
      /// Y coordinate
      /// </summary>
      public float Y;

      /// <summary>
      /// Create a new precision point
      /// </summary>
      /// <param name="x">X coordinate</param>
      /// <param name="y">Y coordinate</param>
      public precisionpoint(float x, float y)
      {
         X = x;
         Y = y;
      }

      /// <summary>
      /// Returns a string that represents the current object.
      /// </summary>
      /// <returns>
      /// A string that represents the current object.
      /// </returns>
      /// <filterpriority>2</filterpriority>
      public override string ToString()
      {
         return X + ", " + Y;
      }
   }

   /// <summary>
   /// Rectangle (rectangle) Structure
   /// </summary>
   [Serializable]
   [DebuggerDisplay("Rectangle({X}, {Y}, {Width}, {Height})")]
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
      /// Creates a new rectangle
      /// </summary>
      /// <param name="x">x location of rectangle</param>
      /// <param name="y">y location of rectangle</param>
      /// <param name="width">width of rectangle</param>
      /// <param name="height">height of rectangle</param>
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
      /// Gets/Sets x location of rectangle
      /// </summary>
      public int X
      {
         get { return _x; }
         set { _x = value; }
      }

      /// <summary>
      /// Gets/Sets y location of rectangle
      /// </summary>
      public int Y
      {
         get { return _y; }
         set { _y = value; }
      }

      /// <summary>
      /// Gets/Sets width of rectangle
      /// </summary>
      public int Width
      {
         get { return _w; }
         set
         {
            if (value < 0)
            {
               throw new IndexOutOfRangeException("width cannot be less than 0");
            }
            _w = value;
         }
      }

      /// <summary>
      /// Gets/Sets height of rectangle
      /// </summary>
      public int Height
      {
         get { return _h; }
         set
         {
            if (value < 0)
            {
               throw new IndexOutOfRangeException("height cannot be less than 0");
            }
            _h = value;
         }
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// Adds the area of a second rectangle to current rectangle
      /// </summary>
      /// <param name="newRect">rectangle to add</param>
      public void Combine(rect newRect)
      {
         if (_w == 0)
         {
            _x = newRect.X;
            _y = newRect.Y;
            _w = newRect.Width;
            _h = newRect.Height;
            return;
         }

         int x1 = (_x < newRect.X) ? _x : newRect.X;
         int y1 = (_y < newRect.Y) ? _y : newRect.Y;
         int x2 = (_x + Width > newRect.X + newRect.Width) ? _x + _w : newRect.X + newRect.Width;
         int y2 = (_y + Height > newRect.Y + newRect.Height) ? _y + _h : newRect.Y + newRect.Height;
         _x = x1;
         _y = y1;
         _w = x2 - x1;
         _h = y2 - y1;
      }

      /// <summary>
      /// Returns the combination of two rectangles
      /// </summary>
      /// <param name="region1">Rectangle 1</param>
      /// <param name="region2">Rectangle 2</param>
      /// <returns>Combined rectangle</returns>
      public rect Combine(rect region1, rect region2)
      {
         if (region1.Width == 0)
         {
            return region2;
         }
         if (region2.Width == 0)
         {
            return region1;
         }

         int x1 = (region1.X < region2.X) ? region1.X : region2.X;
         int y1 = (region1.Y < region2.Y) ? region1.Y : region2.Y;
         int x2 = (region1.X + region1.Width > region2.X + region2.Width) ? region1.X + region1.Width : region2.X + region2.Width;
         int y2 = (region1.Y + region1.Height > region2.Y + region2.Height) ? region1.Y + region1.Height : region2.Y + region2.Height;

         return new rect(x1, y1, x2 - x1, y2 - y1);
      }

      /// <summary>
      /// Checks if a point is inside the rectangle
      /// </summary>
      /// <param name="x">x location</param>
      /// <param name="y">y location</param>
      /// <returns>true if point is inside rectangle; else false</returns>
      public bool Contains(int x, int y)
      {
         return (x >= _x && x <= _x + _w && y >= _y && y <= _y + _h);
      }

      /// <summary>
      /// Checks if a point is inside the rectangle
      /// </summary>
      /// <param name="pointe">point to check</param>
      /// <returns>true if point is inside rectangle; else false</returns>
      public bool Contains(point pointe)
      {
         return (pointe.X >= _x && pointe.X <= _x + _w && pointe.Y >= _y && pointe.Y <= _y + _h);
      }

      /// <summary>
      /// Checks to see if two rectangles intersect
      /// </summary>
      /// <param name="area">rectangle to check</param>
      /// <returns>true if rectangles intersect; else false</returns>
      public bool Intersects(rect area)
      {
         return !(area.X >= (_x + _w)
                 || (area.X + area.Width) <= _x
                 || area.Y >= (_y + _h)
                 || (area.Y + area.Height) <= _y
                 );
      }

      /// <summary>
      /// Returns the intersection of two rectangles
      /// </summary>
      /// <param name="region1">Rectangle 1</param>
      /// <param name="region2">Rectangle 2</param>
      /// <returns>Intersected rectangle</returns>
      public static rect Intersect(rect region1, rect region2)
      {
         if (!region1.Intersects(region2))
         {
            return new rect(0, 0, 0, 0);
         }

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
      /// Returns a string that represents the current object.
      /// </summary>
      /// <returns>
      /// A string that represents the current object.
      /// </returns>
      /// <filterpriority>2</filterpriority>
      public override string ToString()
      {
         return "{" + X + ", " + Y + ", " + Width + ", " + Height + "}";
      }

      #endregion
   }


   /// <summary>
   /// Structure containing object height &amp; width
   /// </summary>
   [Serializable]
   [DebuggerDisplay("Size({Width}, {Height})")]
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
      /// <param name="width">Width</param>
      /// <param name="height">Height</param>
      public size(int width, int height)
      {
         if (width < 0)
         {
            throw new ArgumentOutOfRangeException("width", @"Must be > 0");
         }
         if (height < 0)
         {
            throw new ArgumentOutOfRangeException("height", @"Must be > 0");
         }
         Width = width;
         Height = height;
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// Adds height &amp; width to existing size
      /// </summary>
      /// <param name="addWidth">Additional width</param>
      /// <param name="addHeight">Additional height</param>
      public void Grow(int addWidth, int addHeight)
      {
         Width += addWidth;
         Height += addHeight;
      }

      /// <summary>
      /// Subtracts height &amp; width from existing size
      /// </summary>
      /// <param name="subtractWidth">Width to reduce</param>
      /// <param name="subtractHeight">Height to reduce</param>
      public void Shrink(int subtractWidth, int subtractHeight)
      {
         Width += subtractWidth;
         if (Width < 0)
         {
            Width = 0;
         }
         Height += subtractHeight;
         if (Height < 0)
         {
            Height = 0;
         }
      }

      /// <summary>
      /// Returns a string that represents the current object.
      /// </summary>
      /// <returns>
      /// A string that represents the current object.
      /// </returns>
      /// <filterpriority>2</filterpriority>
      public override string ToString()
      {
         return "{" + Width + ", " + Height + "}";
      }

      #endregion
   }

   /// <summary>
   /// Structure representing the thickness of something
   /// </summary>
   /// <remarks>
   /// The left, right, top and bottom thickness can be set individually.
   /// </remarks>
   [Serializable]
   [DebuggerDisplay("Thickness({Left}, {Top}, {Right}, {Bottom})")]
   public struct Thickness
   {
      /// <summary>
      /// Left thickness
      /// </summary>
      public int Left;

      /// <summary>
      /// Top thickness
      /// </summary>
      public int Top;

      /// <summary>
      /// Right thickness
      /// </summary>
      public int Right;

      /// <summary>
      /// Bottom thickness
      /// </summary>
      public int Bottom;

      /// <summary>
      /// Creates a uniform thickness
      /// </summary>
      /// <param name="uniform">Thickness applied to <see cref="Left"/>, <see cref="Top"/>, <see cref="Right"/> and <see cref="Bottom"/></param>
      public Thickness(int uniform) :
         this(uniform, uniform, uniform, uniform)
      { }

      /// <summary>
      /// Creates a thickness with symmetric horizontal and vertical values
      /// </summary>
      /// <param name="horizontal">Thickness applied to <see cref="Left"/> and <see cref="Right"/></param>
      /// <param name="vertical">Thickness applied to <see cref="Top"/> and <see cref="Bottom"/></param>
      public Thickness(int horizontal, int vertical) :
         this(horizontal, vertical, horizontal, vertical)
      { }

      /// <summary>
      /// Creates a new thickness
      /// </summary>
      /// <param name="left">Left thickness</param>
      /// <param name="top">Top thickness</param>
      /// <param name="right">Right thickness</param>
      /// <param name="bottom">Bottom thickness</param>
      public Thickness(int left, int top, int right, int bottom)
      {
         Left = left;
         Top = top;
         Right = right;
         Bottom = bottom;
      }

      /// <summary>
      /// Gets the sum of the horizontal thickness
      /// </summary>
      public int Horizontal
      {
         get { return Left + Right; }
      }

      /// <summary>
      /// Gets the sum of the vertical thickness
      /// </summary>
      public int Vertical
      {
         get { return Top + Bottom; }
      }

      /// <summary>
      /// Indicates whether this instance and a specified object are equal.
      /// </summary>
      /// <returns>
      /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false. 
      /// </returns>
      /// <param name="obj">The object to compare with the current instance. </param><filterpriority>2</filterpriority>
      public override bool Equals(object obj)
      {
         if (!(obj is Thickness))
         {
            return false;
         }
         return this == (Thickness)obj;
      }

      /// <summary>
      /// Serves as a hash function for a particular type. 
      /// </summary>
      /// <returns>
      /// A hash code for the current object.
      /// </returns>
      /// <filterpriority>2</filterpriority>
      public override int GetHashCode()
      {
         // ReSharper disable NonReadonlyFieldInGetHashCode
         return Left ^ Top ^ Right ^ Bottom;
         // ReSharper restore NonReadonlyFieldInGetHashCode
      }

      /// <summary>
      /// Compares tho Thickness for equality
      /// </summary>
      /// <param name="a">Thickness a</param>
      /// <param name="b">Thickness b</param>
      /// <returns>Returns true if a and b are equal; else false</returns>
      public static bool operator ==(Thickness a, Thickness b)
      {
         return a.Left == b.Left && a.Top == b.Top && a.Right == b.Right && a.Bottom == b.Bottom;
      }

      /// <summary>
      /// Compares tho Thickness for inequality
      /// </summary>
      /// <param name="a">Thickness a</param>
      /// <param name="b">Thickness b</param>
      /// <returns>Returns true if a and b are not equal; else false</returns>
      public static bool operator !=(Thickness a, Thickness b)
      {
         return !(a == b);
      }
   }


   /// <summary>
   /// Event argument for touch events
   /// </summary>
   [Serializable]
   public struct TouchEventArgs
   {
      // ReSharper disable once InconsistentNaming
      /// <summary>
      /// Location (point) of the touch event
      /// </summary>
      public point location;

      // ReSharper disable once InconsistentNaming
      /// <summary>
      /// Type of the touch event.
      /// </summary>
      public int type;

      /// <summary>
      /// Create new touch event arguments
      /// </summary>
      /// <param name="point">Point of the touch event</param>
      /// <param name="type">Type of the touch event</param>
      public TouchEventArgs(point point, int type)
      {
         location = point;
         this.type = type;
      }

      /// <summary>
      /// Returns a string that represents the current object.
      /// </summary>
      /// <returns>
      /// A string that represents the current object.
      /// </returns>
      /// <filterpriority>2</filterpriority>
      public override string ToString()
      {
         return location + "; " + type;
      }
   }
}
