using System;

using Microsoft.SPOT;

namespace Skewworks.NETMF.Controls
{

    [Serializable]
    public delegate void OnCaretChanged(object sender, int location);

    [Serializable]
    public delegate void OnProgressChanged(Object sender, long value);

    [Serializable]
    public delegate void OnTextEditorStart(object sender, ref TextEditorArgs args);

    [Serializable]
    public delegate void OnTextEditorClosing(object sender, ref string Text, ref bool Cancel);

    [Serializable]
    public delegate void OnVirtualKeysDone(object sender);

    [Serializable]
    public delegate void OnVirtualKeyTap(object sender, string value);

    [Serializable]
    public delegate void OnVirtualKeyBackspace(object sender);

    [Serializable]
    public delegate void OnVirtualKeyClear(object sender);

    [Serializable]
    public delegate void OnVirtualKeyCaretMove(object sender, int amount);

    [Serializable]
    public delegate void OnVirtualKeyDelete(object sender);

    [Serializable]
    public delegate void OnVirtualKeyReturn(object sender);

}
