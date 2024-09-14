using System;
using System.Collections;
using System.Collections.Generic;
using log4net;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Engine;

namespace MDLSoft.NHibernate.MultiSessionFactory
{
    [Serializable]
    public class MultiSessionFactoryProvider : ISessionFactoryProvider
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MultiSessionFactoryProvider));

        [NonSerialized]
        private IConfigurationProvider mfc;
        private string defaultSessionFactoryName;
        private Dictionary<string, ISessionFactory> sfs = new Dictionary<string, ISessionFactory>(6, StringComparer.OrdinalIgnoreCase);

        public MultiSessionFactoryProvider(string defaultFactory) : this(new DefaultMultiFactoryConfigurationProvider(), defaultFactory) { }

        public MultiSessionFactoryProvider(IConfigurationProvider configurationProvider, string defaultFactory)
        {
            mfc = configurationProvider ?? throw new ArgumentNullException("configurationProvider");
            defaultSessionFactoryName = defaultFactory;
            Initialize();
        }

        #region ISessionFactoryProvider Members

        public ISessionFactory GetFactory(string factoryId)
        {
            return InternalGetFactory(factoryId);
        }

        public event EventHandler<EventArgs> BeforeCloseSessionFactory;

        #endregion

        private void Initialize()
        {
            if (sfs.Count != 0)
            {
                return;
            }

            log.Debug("Initialize new session factories reading the configuration.");
            foreach (Configuration cfg in mfc.Configure())
            {
                var sf = (ISessionFactoryImplementor)cfg.BuildSessionFactory();
                var sessionFactoryName = sf.Settings.SessionFactoryName;

                if (string.IsNullOrEmpty(defaultSessionFactoryName))
                {
                    defaultSessionFactoryName = sessionFactoryName;
                }

                sfs.Add(sessionFactoryName, sf);
            }
            mfc = null; // after built the SessionFactories the configuration is not needed
        }

        private ISessionFactory InternalGetFactory(string factoryId)
        {
            try
            {
                return sfs[factoryId];
            }
            catch (KeyNotFoundException)
            {
                return sfs[defaultSessionFactoryName];
            }
        }

        private void DoBeforeCloseSessionFactory()
        {
            BeforeCloseSessionFactory?.Invoke(this, EventArgs.Empty);
        }

        #region Implementation of IEnumerable

        public IEnumerator<ISessionFactory> GetEnumerator()
        {
            Initialize();
            return sfs.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Initialize();
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IDisposable

        private bool disposed;

        ~MultiSessionFactoryProvider()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                foreach (var sessionFactory in sfs.Values)
                {
                    if (sessionFactory != null)
                    {
                        DoBeforeCloseSessionFactory();
                        sessionFactory.Close();
                    }
                }
                sfs = new Dictionary<string, ISessionFactory>(6);
            }
            disposed = true;
        }

        #endregion
    }
}