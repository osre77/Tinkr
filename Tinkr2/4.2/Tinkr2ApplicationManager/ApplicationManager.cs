using System;
using System.Reflection;

using Microsoft.SPOT;

namespace Skewworks.Tinkr
{
    [Serializable]
    public class ApplicationManager : MarshalByRefObject
    {

        #region Variables

        private static ApplicationManager _instance;
        private static object _syncRoot = new object();

        private IApplicationLauncher[] _appRefs;
        private int _nextId;

        #endregion

        #region Constructors

        public ApplicationManager()
        {
            if (AppDomain.CurrentDomain.FriendlyName != "default")
                throw new Exception("Cannot instance ApplicationManager outside of default domain");
        }

        #endregion

        #region Properties

        public static int ActiveThreads
        {
            get
            {
                if (_instance._appRefs == null)
                    return 0;
                return _instance._appRefs.Length;
            }
        }

        public static Guid[] ThreadIds
        {
            get
            {
                if (_instance._appRefs == null)
                    return null;

                Guid[] g = new Guid[_instance._appRefs.Length];
                for (int i = 0; i < g.Length; i++)
                    g[i] = _instance._appRefs[i].ThreadId;

                return g;
            }
        }

        #endregion

        #region Events

        public event OnApplicationClosed ApplicationClosed;
        protected virtual void OnApplicationClosed(object sender, ApplicationDetails appDetails)
        {
            if (ApplicationClosed != null)
                ApplicationClosed(sender, appDetails);
        }

        public event OnApplicationLaunched ApplicationLaunched;
        protected virtual void OnApplicationLaunched(object sender, Guid threadId)
        {
            if (ApplicationLaunched != null)
                ApplicationLaunched(sender, threadId);
        }

        #endregion

        #region Public Methods

        public static void Initialize()
        {
            if (_instance == null)
            {
                lock (_syncRoot)
                {
                    if (_instance == null)
                        _instance = new ApplicationManager();
                }
            }
        }

        public static ApplicationDetails GetApplicationDetails(Guid threadId)
        {
            if (_instance._appRefs != null)
            {
                for (int i = 0; i < _instance._appRefs.Length; i++)
                {
                    if (_instance._appRefs[i].ThreadId.Equals(threadId))
                        return _instance._appRefs[i].Details;
                }
            }

            return new ApplicationDetails();
        }

        public static ApplicationImage GetApplicationImage(Guid threadId)
        {
            if (_instance._appRefs != null)
            {
                for (int i = 0; i < _instance._appRefs.Length; i++)
                {
                    if (_instance._appRefs[i].ThreadId.Equals(threadId))
                        return _instance._appRefs[i].Image;
                }
            }

            return new ApplicationImage();
        }

        public static bool LaunchApplication(byte[] applicationData)
        {
            IApplicationLauncher ILauncher;
            int id = _instance._nextId++;
            AppDomain AD = null;

            try
            {
                AD = AppDomain.CreateDomain("app" + id);
            }
            catch (Exception)
            {
                return false;
            }

            try
            {
                ILauncher = (IApplicationLauncher)AD.CreateInstanceAndUnwrap(typeof(IApplicationLauncher).Assembly.FullName, typeof(ApplicationLauncher).FullName);

                if (ILauncher.LoadApp(applicationData, _instance))
                {
                    if (_instance._appRefs == null)
                        _instance._appRefs = new ApplicationLauncher[] { (ApplicationLauncher)ILauncher };
                    else
                    {
                        ApplicationLauncher[] tmp = new ApplicationLauncher[_instance._appRefs.Length + 1];
                        Array.Copy(_instance._appRefs, tmp, _instance._appRefs.Length);
                        tmp[tmp.Length - 1] = (ApplicationLauncher)ILauncher;
                        _instance._appRefs = tmp;
                    }

                    _instance.OnApplicationLaunched(_instance, ILauncher.ThreadId);
                    return true;
                }
            }
            catch (Exception)
            {
                // Unload app domain
                AppDomain.Unload(AD);
            }

            return false;
        }

        public static bool TerminateApplication(Guid threadId)
        {
            if (_instance._appRefs != null)
            {
                for (int i = 0; i < _instance._appRefs.Length; i++)
                {
                    if (_instance._appRefs[i].ThreadId.Equals(threadId))
                    {
                        try
                        {
                            AppDomain ad = _instance._appRefs[i].Domain;
                            ApplicationDetails det = _instance._appRefs[i].Details;

                            _instance._appRefs[i].Terminate();
                            AppDomain.Unload(ad);

                            if (_instance._appRefs.Length == 1)
                                _instance._appRefs = null;
                            else
                            {
                                IApplicationLauncher[] tmp = new IApplicationLauncher[_instance._appRefs.Length - 1];
                                int c = 0;
                                for (int j = 0; j < tmp.Length; j++)
                                {
                                    if (j != i)
                                        tmp[c++] = _instance._appRefs[j];
                                }

                                _instance._appRefs = tmp;
                            }

                            _instance.ApplicationClosed(_instance, det);
                            return true;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                }
            }

            return false;
        }

        #endregion

        #region Application Launching

        private interface IApplicationLauncher
        {

            AppDomain Domain { get; }
            Guid ThreadId { get; }
            ApplicationDetails Details { get; }
            ApplicationImage Image { get; }

            bool LoadApp(byte[] data, ApplicationManager appMan);
            void Terminate();
        }

        [Serializable]
        private class ApplicationLauncher : MarshalByRefObject, IApplicationLauncher
        {

            #region Variables

            private NETMFApplication _app;

            #endregion

            #region Properties

            public ApplicationDetails Details
            {
                get { return _app.ApplicationDetails; }
            }

            public ApplicationImage Image
            {
                get { return _app.ApplicationImage; }
            }

            public Guid ThreadId
            {
                get { return _app.ThreadId; }
            }

            public AppDomain Domain
            {
                get { return AppDomain.CurrentDomain; }
            }

            #endregion

            public bool LoadApp(byte[] data, ApplicationManager appMan)
            {
                // Set the instance across domains
                ApplicationManager._instance = appMan;

                // Launch Application
                try
                {
                    Assembly asm = Assembly.Load(data);

                    Type[] t = asm.GetTypes();
                    for (int i = 0; i < t.Length; i++)
                    {
                        if (t[i].BaseType.FullName == "Skewworks.NETMF.NETMFApplication")
                        {
                            try
                            {
                                _app = (NETMFApplication)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(asm.FullName, t[i].FullName);
                                _app.EntryPoint(Guid.NewGuid(), string.Empty, null);
                                return true;
                            }
                            catch (Exception) { }
                        }
                    }
                }
                catch (Exception)
                {
                    return false;
                }

                return false;
            }

            public void Terminate()
            {
                _app.Terminate();
            }

        }

        #endregion

    }
}
