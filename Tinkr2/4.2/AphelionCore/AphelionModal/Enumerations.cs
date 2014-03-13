using System;

namespace Skewworks.NETMF.Modal
{
   public enum PromptResult
   {
      // ReSharper disable once InconsistentNaming
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
      // ReSharper disable once InconsistentNaming
      OK = 0,
      // ReSharper disable once InconsistentNaming
      OKCancel = 1,
      YesNo = 2,
      YesNoCancel = 3,
      AbortContinue = 4,
      AbortRetryContinue = 5,
   }
}
