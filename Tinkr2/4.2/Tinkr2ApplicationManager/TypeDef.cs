using System;
using Microsoft.SPOT;

namespace Skewworks.Tinkr
{
    /// <summary>
    /// Application Details
    /// (Skewworks NETMF Application Standard v2.0)
    /// </summary>
    [Serializable]
    public struct ApplicationDetails
    {

        #region Variables

        /// <summary>
        /// Owning Company
        /// </summary>
        public string Company;

        /// <summary>
        /// Application Copyright
        /// </summary>
        public string Copyright;

        /// <summary>
        /// Application Title
        /// </summary>
        public string Title;

        /// <summary>
        /// Application Description
        /// </summary>
        public string Description;

        /// <summary>
        /// Application Version
        /// </summary>
        public string Version;

        #endregion

        #region Constructor

        /// <summary>
        /// Application Details
        /// (Skewworks NETMF Application Standard v2.0)
        /// </summary>
        /// <param name="Title">Application Title</param>
        /// <param name="Description">Application Description</param>
        /// <param name="Company">Owning Company</param>
        /// <param name="Copyright">Application Copyright</param>
        /// <param name="Version">Application Version</param>
        public ApplicationDetails(string Title, string Description, string Company, string Copyright, string Version)
        {
            this.Company = Company;
            this.Copyright = Copyright;
            this.Title = Title;
            this.Description = Description;
            this.Version = Version;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// String representation of the object
        /// </summary>
        /// <returns>Title (v Version)</returns>
        public override string ToString()
        {
            return this.Title + " (v " + this.Version + ")";
        }

        #endregion

    }

    [Serializable]
    public struct ApplicationImage
    {

        #region Variables

        /// <summary>
        /// Raw Image Data
        /// </summary>
        public byte[] ImageData;

        /// <summary>
        /// Size of image
        /// </summary>
        public size ImageSize;

        /// <summary>
        /// Image Type
        /// </summary>
        public ImageType ImageType;

        #endregion

        public ApplicationImage(byte[] data, size size, ImageType type)
        {
            this.ImageData = data;
            this.ImageSize = size;
            this.ImageType = type;
        }

    }

    [Serializable]
    public struct size
    {

        #region Variables

        /// <summary>
        /// Width
        /// </summary>
        public int Width;

        /// <summary>
        /// Height
        /// </summary>
        public int Height;

        #endregion

        #region Constructor

        /// <summary>
        /// Create new size
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public size(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        #endregion

    }
}
