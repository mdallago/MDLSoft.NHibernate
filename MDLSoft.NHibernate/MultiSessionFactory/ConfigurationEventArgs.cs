using System;
using NHibernate.Cfg;

namespace MDLSoft.NHibernate.MultiSessionFactory
{
    public class ConfigurationEventArgs : EventArgs
    {
        public ConfigurationEventArgs(Configuration configuration)
        {
            Configuration = configuration;
        }

        public Configuration Configuration { get; private set; }
    }
}