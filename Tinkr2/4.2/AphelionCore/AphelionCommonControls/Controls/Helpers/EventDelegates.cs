using System;

namespace Skewworks.NETMF.Controls
{

    [Serializable]
    public delegate void OnCaretChanged(object sender, int location);

    [Serializable]
    public delegate void OnItemSelected(object sender, int index);

    [Serializable]
    public delegate void OnNodeTap(TreeviewNode node, point e);

    [Serializable]
    public delegate void OnNodeExpanded(object sender, TreeviewNode node);

    [Serializable]
    public delegate void OnNodeCollapsed(object sender, TreeviewNode node);

    [Serializable]
    public delegate void OnPathChanged(object sender, string value);

    [Serializable]
    public delegate void OnProgressChanged(Object sender, long value);

    [Serializable]
    public delegate void OnTextEditorStart(object sender, ref TextEditorArgs args);

    [Serializable]
    public delegate void OnTextEditorClosing(object sender, ref string text, ref bool cancel);

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
