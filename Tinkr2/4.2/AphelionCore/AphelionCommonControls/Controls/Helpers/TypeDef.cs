using System;
using Microsoft.SPOT;

namespace Skewworks.NETMF.Controls
{
    public struct TextEditorArgs
    {
        public Font EditorFont;
        public string EditorTitle;
        public string DefaultValue;
        
       public TextEditorArgs(Font editorFont, string editorTitle, string defaultValue)
        {
            EditorFont = editorFont;
            EditorTitle = editorTitle;
            DefaultValue = defaultValue;
        }
    }
}
