using System;

namespace Skewworks.NETMF.Applications
{
   /// <summary>
   /// Structure representing an application image.
   /// </summary>
   [Serializable]
   public struct ApplicationImage
   {
      #region Variables

      /// <summary>
      /// Gets the raw image data
      /// </summary>
      public byte[] ImageData;

      /// <summary>
      /// Gets the size of the image
      /// </summary>
      public size ImageSize;

      /// <summary>
      /// Gets the type of the image.
      /// </summary>
      public ImageType ImageType;

      #endregion

      /// <summary>
      /// Creates a new Application image structure.
      /// </summary>
      /// <param name="data">Raw data of the image.</param>
      /// <param name="size">Size of the image.</param>
      /// <param name="type">Type of the image.</param>
      public ApplicationImage(byte[] data, size size, ImageType type)
      {
         ImageData = data;
         ImageSize = size;
         ImageType = type;
      }

   }
}