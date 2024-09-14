using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using NHibernate;
using NHibernate.Context;
using NHibernate.Engine;
using NHibernate.Transaction;

namespace MDLSoft.NHibernate.MultiSessionFactory
{
    [Serializable]
    public abstract class CurrentSessionContext : ICurrentSessionContext
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(CurrentSessionContext));

        protected ISessionFactoryImplementor factory;

        protected CurrentSessionContext(ISessionFactoryImplementor factory)
        {
            this.factory = factory ?? throw new ArgumentNullException("factory");
        }

        protected virtual ISessionFactoryImplementor Factory
        {
            get { return factory; }
        }

        /// <summary> Mainly for subclass usage.  This impl always returns true. </summary>
        /// <returns> Whether or not the the session should be closed by transaction completion. </returns>
        protected virtual bool AutoCloseEnabled
        {
            get { return true; }
        }

        /// <summary> Mainly for subclass usage.  This impl always returns true. </summary>
        /// <returns> Whether or not the the session should be flushed prior transaction completion. </returns>
        protected virtual bool AutoFlushEnabled
        {
            get { return true; }
        }

        /// <summary> Mainly for subclass usage. This impl always returns after_transaction. </summary>
        /// <returns> The connection release mode for any built sessions. </returns>
        protected ConnectionReleaseMode ConnectionReleaseMode
        {
            get { return Factory.Settings.ConnectionReleaseMode; }
        }

        public ISession CurrentSession()
        {
            ISession current = ExistingSession(Factory);
            if (current == null)
            {
                current = BuildOrObtainSession();
                // register a cleanup synch

                var transaction = current.GetCurrentTransaction();
                if (transaction != null)
                {
                    transaction.RegisterSynchronization(BuildCleanupSynch());
                }

                // wrap the session in the transaction-protection proxy
                if (NeedsWrapping(current))
                {
                    current = Wrap(current);
                }
                // then bind it
                DoBind(current, Factory);
            }
            return current;
        }

        /// <summary>
        /// Get the dicitonary mapping session factory to its current session.
        /// </summary>
        protected abstract IDictionary<ISessionFactory, ISession> GetContextDictionary();

        /// <summary>
        /// Set the map mapping session factory to its current session.
        /// </summary>
        protected abstract void SetContextDictionary(IDictionary<ISessionFactory, ISession> value);

        protected virtual ITransactionCompletionSynchronization BuildCleanupSynch()
        {
            return new CleanupSynch(Factory);
        }

        private static bool NeedsWrapping(ISession session)
        {
            if (Wrapper == null)
                return false;
            return !Wrapper.IsWrapped(session);
        }

        /// <summary> 
        /// Strictly provided for subclassing purposes; specifically to allow long-session support.
        /// </summary>
        /// <returns> the built or (re)obtained session. </returns>
        /// <remarks>This implementation always just opens a new session.</remarks>
        protected virtual ISession BuildOrObtainSession()
        {
            return Factory.WithOptions()
                .Connection(null)
                .AutoClose(AutoCloseEnabled)
                .ConnectionReleaseMode(ConnectionReleaseMode)
                .OpenSession();
        }

        protected virtual ISession Wrap(ISession session)
        {
            if (Wrapper == null)
            {
                log.Warn("Session wrapper not available.");
                return session;
            }
            return Wrapper.Wrap(session, UnbindSession, null);
        }

        #region  Static helpers

        protected static CurrentSessionContext GetSessionFactoryContext(ISessionFactory factory)
        {
            var factoryImpl = factory as ISessionFactoryImplementor;

            if (factoryImpl == null)
            {
                throw new HibernateException("Session factory does not implement ISessionFactoryImplementor.");
            }

            if (factoryImpl.CurrentSessionContext == null)
            {
                throw new HibernateException("No current session context configured.");
            }

            var currentSessionContext = factoryImpl.CurrentSessionContext as CurrentSessionContext;
            if (currentSessionContext == null)
            {
                throw new HibernateException("Current session context does not extend class AbstractCurrentSessionContext.");
            }

            return currentSessionContext;
        }

        protected static IDictionary<ISessionFactory, ISession> GetContext(ISessionFactory factory)
        {
            return GetSessionFactoryContext(factory).GetContextDictionary();
        }

        protected static void SetContext(ISessionFactory factory, IDictionary<ISessionFactory, ISession> dic)
        {
            GetSessionFactoryContext(factory).SetContextDictionary(dic);
        }

        /// <summary> Associates the given session with the current thread of execution. </summary>
        /// <param name="session">The session to bind. </param>
        public static void Bind(ISession session)
        {
            ISessionFactory factory = session.SessionFactory;
            CleanupAnyOrphanedSession(factory);
            DoBind(session, factory);
        }

        private static void CleanupAnyOrphanedSession(ISessionFactory factory)
        {
            ISession orphan = DoUnbind(factory, false);
            if (orphan != null)
            {
                log.Warn("Already session bound on call to Bind(); make sure you clean up your sessions!");
                try
                {
                    var transaction = orphan.GetCurrentTransaction();
                    if (transaction != null && transaction.IsActive)
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch (Exception t)
                        {
                            log.Debug("Unable to rollback transaction for orphaned session", t);
                        }
                    }
                    orphan.Close();
                }
                catch (Exception t)
                {
                    log.Debug("Unable to close orphaned session", t);
                }
            }
        }

        public static ISessionWrapper Wrapper { get; set; }

        /// <summary> Unassociate a previously bound session from the current thread of execution. </summary>
        /// <returns> The session which was unbound. </returns>
        public static ISession Unbind(ISessionFactory factory)
        {
            return DoUnbind(factory, true);
        }

        protected static ISession UnbindSession(ISession session)
        {
            return DoUnbind(session.SessionFactory, true);
        }

        private static void DoBind(ISession session, ISessionFactory factory)
        {
            IDictionary<ISessionFactory, ISession> sessionDictionary = GetContext(factory);
            if (sessionDictionary == null)
            {
                sessionDictionary = new Dictionary<ISessionFactory, ISession>(4);
                SetContext(factory, sessionDictionary);
            }
            sessionDictionary[factory] = session;
        }

        private static ISession DoUnbind(ISessionFactory factory, bool releaseIfEmpty)
        {
            IDictionary<ISessionFactory, ISession> sessionDic = GetContext(factory);
            ISession session = null;
            if (sessionDic != null)
            {
                sessionDic.TryGetValue(factory, out session);
                sessionDic.Remove(factory);
                if (releaseIfEmpty && sessionDic.Count == 0)
                {
                    SetContext(factory, null);
                }
            }
            return session;
        }

        private static ISession ExistingSession(ISessionFactory factory)
        {
            IDictionary<ISessionFactory, ISession> sessionDic = GetContext(factory);
            if (sessionDic == null)
            {
                return null;
            }
            else
            {
                sessionDic.TryGetValue(factory, out ISession result);
                return result;
            }
        }

        #endregion

        #region Nested type: CleanupSynch

        /// <summary> Transaction synch used for cleanup of the internal session dictionary.</summary>
        [Serializable]
        protected class CleanupSynch : ITransactionCompletionSynchronization
        {
            protected internal ISessionFactory factory;

            public CleanupSynch(ISessionFactory factory)
            {
                this.factory = factory;
            }

            public void ExecuteBeforeTransactionCompletion()
            {
                if (GetSessionFactoryContext(factory).AutoFlushEnabled)
                {
                    factory.GetCurrentSession().Flush();
                }
            }

            public Task ExecuteBeforeTransactionCompletionAsync(CancellationToken cancellationToken)
            {
                if (GetSessionFactoryContext(factory).AutoFlushEnabled)
                {
                    return factory.GetCurrentSession().FlushAsync(cancellationToken);
                }
                return Task.CompletedTask;
            }

            public void ExecuteAfterTransactionCompletion(bool success)
            {
                if (GetSessionFactoryContext(factory).AutoCloseEnabled)
                {
                    factory.GetCurrentSession().Close();
                }
                Unbind(factory);
            }

            public Task ExecuteAfterTransactionCompletionAsync(bool success, CancellationToken cancellationToken)
            {
                if (GetSessionFactoryContext(factory).AutoCloseEnabled)
                {
                    factory.GetCurrentSession().Close();
                }
                Unbind(factory);
                return Task.CompletedTask;
            }
        }

        #endregion
    }
}