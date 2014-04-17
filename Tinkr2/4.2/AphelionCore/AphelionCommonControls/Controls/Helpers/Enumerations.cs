using System;

namespace Skewworks.NETMF.Controls
{
   /// <summary>
   /// Enumeration of different file list modes
   /// </summary>
   public enum FileListMode
   {
      /// <summary>
      /// Show name only
      /// </summary>
      NameOnly = 0,

      /// <summary>
      /// SHow name and size
      /// </summary>
      NameSize = 1,

      /// <summary>
      /// Show name size and last write time
      /// </summary>
      NameSizeModified = 2,
   }

   /// <summary>
   /// File type enumeration
   /// </summary>
   public enum FileType
   {
      /// <summary>
      /// File
      /// </summary>
      File = 0,

      /// <summary>
      /// Folder
      /// </summary>
      Folder = 1,
   }
}
