using System;
using Microsoft.SPOT;

namespace Skewworks.NETMF.Modal
{
    public enum PromptResult
    {
        OK = 0,
        Cancel = 1,
        Yes = 2,
        No = 3,
        Abort = 4,
        Continue = 5,
        Retry = 6,
    }

    public enum PromptType
    {
        OK = 0,
        OKCancel = 1,
        YesNo = 2,
        YesNoCancel = 3,
        AbortContinue = 4,
        AbortRetryContinue = 5,
    }
}
