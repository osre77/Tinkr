using System;
using Microsoft.SPOT;

using Skewworks.NETMF;

namespace Skewworks.Tinkr
{
    [Serializable]
    public delegate void OnResized(Object sender, rect bounds);

    [Serializable]
    public delegate void OnWindowClosed(Object sender);

    [Serializable]
    public delegate void OnWindowMinimized(Object sender);
}
