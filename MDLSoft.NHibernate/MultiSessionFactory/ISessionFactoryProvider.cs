using System;
using System.Collections.Generic;
using NHibernate;

namespace MDLSoft.NHibernate.MultiSessionFactory
{
    public interface ISessionFactoryProvider : IEnumerable<ISessionFactory>, IDisposable
    {
        ISessionFactory GetFactory(string factoryId);
        event EventHandler<EventArgs> BeforeCloseSessionFactory;
    }

    public delegate ISession SessionCloseDelegate(ISession session);
    public delegate void SessionDisposeDelegate(ISession session);
}