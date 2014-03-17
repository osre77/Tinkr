using System;
using System.Threading;

namespace Skewworks.NETMF.Applications
{
   /// <summary>
   /// Class representing a Skewworks NETMF application
   /// </summary>
   //TODO: this class should be abstract
   [Serializable]
   public class NETMFApplication : MarshalByRefObject
   {
      #region Variables

      private string[] _args;
      private string _appPath;
      private Thread _mainThread;
      private Guid _appId;

      #endregion

      #region Public Methods

      /// <summary>
      /// Initializes and starts the application.
      /// </summary>
      /// <param name="applicationId">Unique id of the application</param>
      /// <param name="applicationPath">Path of the application</param>
      /// <param name="args">Arguments passed to the application</param>
      /// <remarks>
      /// Any application that should be started by the Tinkr core must implement a class derived from <see cref="NETMFApplication"/>.
      /// </remarks>
      public void EntryPoint(Guid applicationId, string applicationPath, string[] args)
      {
         if (_mainThread != null)
            return;

         _appId = applicationId;
         _appPath = applicationPath;
         _args = args;
         _mainThread = new Thread(Main)
         {
            Priority = ThreadPriority.Normal
         };
         _mainThread.Start();
      }

      /// <summary>
      /// Terminate the application
      /// </summary>
      public void Terminate()
      {
         long lTimeout = DateTime.Now.Ticks + (TimeSpan.TicksPerSecond * 5);

         // Begin termination sequence
         var thTerm = new Thread(Terminating)
         {
            Priority = ThreadPriority.AboveNormal
         };
         thTerm.Start();

         // Wait for completion or timeout
         while (DateTime.Now.Ticks < lTimeout && thTerm.IsAlive)
         {
            Thread.Sleep(100);
         }

         // Abort termination if needed
         if (thTerm.IsAlive)
         {
            thTerm.Abort();
         }

         // Abort main thread if needed
         if (_mainThread.IsAlive)
         {
            _mainThread.Abort();
         }

         // Null
         _mainThread = null;
         _appPath = null;
         _args = null;
      }

      #endregion

      #region Properties

      /// <summary>
      /// Gets the application path
      /// </summary>
      public string ApplicationPath
      {
         get { return _appPath; }
      }

      /// <summary>
      /// Gets the <see cref="ApplicationDetails"/>
      /// </summary>
      public virtual ApplicationDetails ApplicationDetails
      {
         get { return new ApplicationDetails(); }
      }

      /// <summary>
      /// Gets the <see cref="ApplicationImage"/>
      /// </summary>
      public virtual ApplicationImage ApplicationImage
      {
         get { return new ApplicationImage(); }
      }

      /// <summary>
      /// Gets the application startup args
      /// </summary>
      public string[] StartupArgs
      {
         get { return _args; }
      }

      /// <summary>
      /// Gets the unique id of the application
      /// </summary>
      public Guid ThreadId
      {
         get { return _appId; }
      }

      /// <summary>
      /// Gets or sets the thread priority of the application main thread.
      /// </summary>
      public ThreadPriority ThreadPriority
      {
         get { return _mainThread.Priority; }
         set { _mainThread.Priority = value; }
      }

      #endregion

      #region Virtual Methods

      /// <summary>
      /// Main method of the application
      /// </summary>
      /// <remarks>
      /// This method is similar to the NETMF Main() method.
      /// Exiting this method will exit the application.
      /// </remarks>
      //TODO: this method should be abstract
      public virtual void Main()
      { }

      /// <summary>
      /// Sends a message to this application
      /// </summary>
      /// <param name="sender">Sender of the message</param>
      /// <param name="message">Message</param>
      /// <param name="args">Message arguments</param>
      /// <returns>Response message</returns>
      /// <remarks>
      /// Override this method to receive messages.
      /// </remarks>
      public virtual string SendMessage(object sender, string message, object args = null)
      {
         return string.Empty;
      }

      /// <summary>
      /// Is called when the application should terminate.
      /// </summary>
      /// <remarks>
      /// Override this method. When this method is called then the Main method must exit as soon as possible.
      /// This method is called in a different thread to the application main thread.
      /// </remarks>
      //TODO: this method should be abstract
      public virtual void Terminating()
      { }

      #endregion

   }
}
