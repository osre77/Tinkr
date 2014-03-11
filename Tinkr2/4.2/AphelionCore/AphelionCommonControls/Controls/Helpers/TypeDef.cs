using System;
using Microsoft.SPOT;

namespace Skewworks.NETMF.Controls
{

    public struct TextEditorArgs
    {
        public Font EditorFont;
        public string EditorTitle;
        public string DefaultValue;
        public TextEditorArgs(Font EditorFont, string EditorTitle, string DefaultValue)
        {
            this.EditorFont = EditorFont;
            this.EditorTitle = EditorTitle;
            this.DefaultValue = DefaultValue;
        }
    }

}
