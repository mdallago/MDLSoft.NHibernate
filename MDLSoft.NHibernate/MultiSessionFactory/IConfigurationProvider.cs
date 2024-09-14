using System;
using System.Collections.Generic;
using NHibernate.Cfg;

namespace MDLSoft.NHibernate.MultiSessionFactory
{
    public interface IConfigurationProvider
    {
        IEnumerable<Configuration> Configure();
        event EventHandler<ConfiguringEventArgs> BeforeConfigure;
        event EventHandler<ConfigurationEventArgs> AfterConfigure;
    }
}