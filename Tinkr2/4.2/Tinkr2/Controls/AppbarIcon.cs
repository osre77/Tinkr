using System;
using Microsoft.SPOT;

using Skewworks.NETMF;
using Skewworks.NETMF.Controls;

namespace Skewworks.Tinkr.Controls
{
   [Serializable]
   public class AppbarIcon : Control
   {

      #region Variables

      private Bitmap _image;
      private rect _bounds;
      private Appbar _owner;

      #endregion

      #region Constructors

      public AppbarIcon(string name, Bitmap image)
      {
         Name = name;
         _image = image;
      }

      #endregion

      #region Properties

      internal protected rect Bounds
      {
         get { return _bounds; }
         set { _bounds = value; }
      }

      public override int Height
      {
         get
         {
            if (_image == null)
               return 0;
            return _image.Height;
         }
         set { throw new Exception("AppbarIcon height cannot be modified"); }
      }

      public Bitmap Image
      {
         get { return _image; }
         set
         {
            if (_image == value)
               return;
            _image = value;
            if (_owner != null)
               _owner.Invalidate();
         }
      }

      internal Appbar Owner
      {
         get { return _owner; }
         set { _owner = value; }
      }

      public override IContainer Parent
      {
         get
         {
            return base.Parent;
         }
         set
         {
            throw new Exception("AppbarIcons can only be added to Appbars");
         }
      }

      public override int Width
      {
         get
         {
            if (_image == null)
               return 0;
            return _image.Width;
         }
         set { throw new Exception("AppbarIcon width cannot be modified"); }
      }

      #endregion

   }
}
