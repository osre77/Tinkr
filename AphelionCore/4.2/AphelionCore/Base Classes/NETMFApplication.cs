using System;
using System.Threading;

using Microsoft.SPOT;

namespace Skewworks.NETMF.Applications
{
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

        public void EntryPoint(Guid applicationId, string applicationPath, string[] args)
        {
            if (_mainThread != null)
                return;

            _appId = applicationId;
            _appPath = applicationPath;
            _args = args;
            _mainThread = new Thread(Main);
            _mainThread.Priority = ThreadPriority.Normal;
            _mainThread.Start();
        }

        public void Terminate()
        {
            long lTimeout = DateTime.Now.Ticks + (TimeSpan.TicksPerSecond * 5);

            // Begin termination sequence
            Thread thTerm = new Thread(Terminating);
            thTerm.Priority = ThreadPriority.AboveNormal;
            thTerm.Start();

            // Wait for completion or timeout
            while (DateTime.Now.Ticks < lTimeout && thTerm.IsAlive)
                Thread.Sleep(100);

            // Abort termination if needed
            if (thTerm.IsAlive)
                thTerm.Abort();

            // Abort main thread if needed
            if (_mainThread.IsAlive)
                _mainThread.Abort();

            // Null
            thTerm = null;
            _mainThread = null;
            _appPath = null;
            _args = null;
        }

        #endregion

        #region Properties

        public string ApplicationPath
        {
            get { return _appPath; }
        }

        public virtual ApplicationDetails ApplicationDetails
        {
            get { return new ApplicationDetails(); }
        }

        public virtual ApplicationImage ApplicationImage
        {
            get { return new ApplicationImage(); }
        }

        public string[] StartupArgs
        {
            get { return _args; }
        }

        public Guid ThreadId
        {
            get { return _appId; }
        }

        public ThreadPriority ThreadPriority
        {
            get { return _mainThread.Priority; }
            set { _mainThread.Priority = value; }
        }

        #endregion

        #region Virtual Methods

        public virtual void Main()
        {
        }

        /// <summary>
        /// Sends a message to the application
        /// </summary>
        /// <param name="sender">Sender of the message</param>
        /// <param name="message">Message</param>
        /// <returns>Response message</returns>
        public virtual string SendMessage(object sender, string message, object args = null)
        {
            return string.Empty;
        }

        public virtual void Terminating()
        {
        }

        #endregion

    }
}
