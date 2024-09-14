using System;
using NHibernate.Cfg;

namespace MDLSoft.NHibernate.MultiSessionFactory
{
    public class ConfiguringEventArgs : EventArgs
    {
        public ConfiguringEventArgs(Configuration configuration)
        {
            Configuration = configuration;
            Configured = false;
        }

        public Configuration Configuration { get; private set; }

        public bool Configured { get; set; }
    }
}