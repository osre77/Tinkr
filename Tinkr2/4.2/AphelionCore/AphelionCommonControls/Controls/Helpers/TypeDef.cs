using System;
using Microsoft.SPOT;

namespace Skewworks.NETMF.Controls
{
   /// <summary>
   /// Arguments for a text editor
   /// </summary>
   public struct TextEditorArgs
   {
      /// <summary>
      /// Font to use in editor
      /// </summary>
      public Font EditorFont;

      /// <summary>
      /// Title of editor
      /// </summary>
      public string EditorTitle;

      /// <summary>
      /// Default value for editor
      /// </summary>
      public string DefaultValue;

      /// <summary>
      /// Creates new text editor arguments
      /// </summary>
      /// <param name="editorFont">Font to use in editor</param>
      /// <param name="editorTitle">Title of editor</param>
      /// <param name="defaultValue">Default value for editor</param>
      public TextEditorArgs(Font editorFont, string editorTitle, string defaultValue)
      {
         EditorFont = editorFont;
         EditorTitle = editorTitle;
         DefaultValue = defaultValue;
      }
   }
}
