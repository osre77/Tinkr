using System;
using Microsoft.SPOT;

namespace Skewworks.NETMF
{

    /// <summary>
    /// point coordinates
    /// </summary>
    [Serializable]
    public struct point
    {
        public int X;
        public int Y;
        public point(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
        public override string ToString()
        {
            return this.X + ", " + this.Y;
        }
    }

    [Serializable]
    public struct precisionpoint
    {
        public float X;
        public float Y;
        public precisionpoint(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
        }
        public override string ToString()
        {
            return this.X + ", " + this.Y;
        }
    }

    /// <summary>
    /// Rectangle (rect) Structure
    /// </summary>
    [Serializable]
    public struct rect
    {

        #region Variables

        private int x;
        private int y;
        private int w;
        private int h;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new rect
        /// </summary>
        /// <param name="X">X location of rect</param>
        /// <param name="Y">Y location of rect</param>
        /// <param name="Width">Width of rect</param>
        /// <param name="Height">Height of rect</param>
        public rect(int X, int Y, int Width, int Height)
        {
            this.x = X;
            this.y = Y;
            this.w = Width;
            this.h = Height;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets/Sets X location of rect
        /// </summary>
        public int X
        {
            get { return x; }
            set { x = value; }
        }

        /// <summary>
        /// Gets/Sets Y location of rect
        /// </summary>
        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        /// <summary>
        /// Gets/Sets Width of rect
        /// </summary>
        public int Width
        {
            get { return w; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Width cannot be less than 0");
                w = value;
            }
        }

        /// <summary>
        /// Gets/Sets Height of rect
        /// </summary>
        public int Height
        {
            get { return h; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Height cannot be less than 0");
                h = value;
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
            if (w == 0)
            {
                x = newrect.X;
                y = newrect.Y;
                w = newrect.Width;
                h = newrect.Height;
                return;
            }

            int x1, y1, x2, y2;
            x1 = (x < newrect.X) ? x : newrect.X;
            y1 = (y < newrect.Y) ? y : newrect.Y;
            x2 = (x + this.Width > newrect.X + newrect.Width) ? x + w : newrect.X + newrect.Width;
            y2 = (y + this.Height > newrect.Y + newrect.Height) ? y + h : newrect.Y + newrect.Height;
            x = x1;
            y = y1;
            w = x2 - x1;
            h = y2 - y1;
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

            int x1, y1, x2, y2;
            x1 = (region1.X < region2.X) ? region1.X : region2.X;
            y1 = (region1.Y < region2.Y) ? region1.Y : region2.Y;
            x2 = (region1.X + region1.Width > region2.X + region2.Width) ? region1.X + region1.Width : region2.X + region2.Width;
            y2 = (region1.Y + region1.Height > region2.Y + region2.Height) ? region1.Y + region1.Height : region2.Y + region2.Height;
            return new rect(x1, y1, x2 - x1, y2 - y1);
        }

        /// <summary>
        /// Checks if a point is inside the rect
        /// </summary>
        /// <param name="X">X location</param>
        /// <param name="Y">Y location</param>
        /// <returns>True if point is inside rect</returns>
        public bool Contains(int X, int Y)
        {
            if (X >= x && X <= x + w && Y >= y && Y <= y + h)
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
            if (e.X >= x && e.X <= x + w && e.Y >= y && e.Y <= y + h)
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
            return !(area.X >= (x + w)
                    || (area.X + area.Width) <= x
                    || area.Y >= (y + h)
                    || (area.Y + area.Height) <= y
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

            rect rct = new rect();

            // For X1 & Y1 we'll want the highest value
            rct.X = (region1.X > region2.X) ? region1.X : region2.X;
            rct.Y = (region1.Y > region2.Y) ? region1.Y : region2.Y;

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
        /// <returns>{X, Y, Width, Height}</returns>
        public override string ToString()
        {
            return "{" + this.X + ", " + this.Y + ", " + this.Width + ", " + this.Height + "}";
        }

        #endregion

    }


    /// <summary>
    /// Structure containing object Height & Width
    /// </summary>
    [Serializable]
    public struct size
    {

        #region Variables

        /// <summary>
        /// Width of the object
        /// </summary>
        public int Width;

        /// <summary>
        /// Height of the object
        /// </summary>
        public int Height;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new Size
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        public size(int Width, int Height)
        {
            if (Width < 0 || Height < 0)
                throw new ArgumentOutOfRangeException("Height and Width cannot be less than 0");
            this.Width = Width;
            this.Height = Height;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds Height & Width to existing size
        /// </summary>
        /// <param name="AddWidth"></param>
        /// <param name="AddHeight"></param>
        public void Grow(int AddWidth, int AddHeight)
        {
            Width += AddWidth;
            Height += AddHeight;
        }

        /// <summary>
        /// Subtracts Height & Width from existing size
        /// </summary>
        /// <param name="SubtractWidth"></param>
        /// <param name="SubtractHeight"></param>
        public void Shrink(int SubtractWidth, int SubtractHeight)
        {
            Width += SubtractWidth;
            if (Width < 0)
                Width = 0;
            Height += SubtractHeight;
            if (Height < 0)
                Height = 0;
        }

        /// <summary>
        /// Returns a string representation of the Size
        /// </summary>
        /// <returns>{Width, Height}</returns>
        public override string ToString()
        {
            return "{" + Width + ", " + Height + "}";
        }

        #endregion

    }

    [Serializable]
    public struct TouchEventArgs
    {
        public point location;
        public int type;
        public TouchEventArgs(point e, int type)
        {
            this.location = e;
            this.type = type;
        }
        public override string ToString()
        {
            return this.location.ToString() + "; " + this.type;
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
        /// <param name="Title">Application Title</param>
        /// <param name="Description">Application Description</param>
        /// <param name="Company">Owning Company</param>
        /// <param name="Copyright">Application Copyright</param>
        /// <param name="Version">Application Version</param>
        public ApplicationDetails(string Title, string Description, string Company, string Copyright, string Version)
        {
            this.Company = Company;
            this.Copyright = Copyright;
            this.Title = Title;
            this.Description = Description;
            this.Version = Version;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// String representation of the object
        /// </summary>
        /// <returns>Title (v Version)</returns>
        public override string ToString()
        {
            return this.Title + " (v " + this.Version + ")";
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
            this.ImageData = data;
            this.ImageSize = size;
            this.ImageType = type;
        }

    }
}
