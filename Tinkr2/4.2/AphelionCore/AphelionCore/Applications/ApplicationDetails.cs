using System;

namespace Skewworks.NETMF.Applications
{
   /// <summary>
   /// Application Details
   /// </summary>
   [Serializable]
   public struct ApplicationDetails
   {
      #region Variables

      /// <summary>
      /// Gets the owning Company
      /// </summary>
      public string Company;

      /// <summary>
      /// Gets the application copyright
      /// </summary>
      public string Copyright;

      /// <summary>
      /// Gets the application title
      /// </summary>
      public string Title;

      /// <summary>
      /// Gets the application description
      /// </summary>
      public string Description;

      /// <summary>
      /// Gets the application version
      /// </summary>
      public string Version;

      #endregion

      #region Constructor

      /// <summary>
      /// Application Details
      /// </summary>
      /// <param name="title">Application Title</param>
      /// <param name="description">Application Description</param>
      /// <param name="company">Owning Company</param>
      /// <param name="copyright">Application Copyright</param>
      /// <param name="version">Application Version</param>
      public ApplicationDetails(string title, string description, string company, string copyright, string version)
      {
         Company = company;
         Copyright = copyright;
         Title = title;
         Description = description;
         Version = version;
      }

      #endregion

      #region Public Methods

      /// <summary>
      /// String representation of the object
      /// </summary>
      /// <returns>Title (v Version)</returns>
      public override string ToString()
      {
         return Title + " (v " + Version + ")";
      }

      #endregion
   }
}