using System;
using Microsoft.SPOT;

namespace Skewworks.Tinkr
{
    /// <summary>
    /// Event Delegate for Application Closed
    /// </summary>
    /// <param name="sender">Object raising the event</param>
    /// <param name="ApplicationDetails">Details of the closed application</param>
    [Serializable]
    public delegate void OnApplicationClosed(object sender, ApplicationDetails appDetails);

    /// <summary>
    /// Event Delegate for Application Launched
    /// </summary>
    /// <param name="sender">Object raising the event</param>
    /// <param name="app">Application launched</param>
    [Serializable]
    public delegate void OnApplicationLaunched(object sender, Guid threadId);
}
