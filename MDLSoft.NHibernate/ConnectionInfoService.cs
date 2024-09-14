using Microsoft.Data.SqlClient;
using NHibernate;

namespace MDLSoft.NHibernate
{
    public interface IConnectionInfoService
    {
        string Server { get; }
        string Database { get; }
        string ConnectionString { get; }
    }

    public class ConnectionInfoService : IConnectionInfoService
    {
        private readonly ISessionFactory sessionFactory;
        public string Server { get; private set; }
        public string Database { get; private set; }
        public string ConnectionString { get; private set; }

        public ConnectionInfoService(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;   
            Initialize();
        }

        private void Initialize()
        {
            if (Server != null)
                return;
            ConnectionString = sessionFactory.GetCurrentSession().Connection.ConnectionString;
            var builder = new SqlConnectionStringBuilder(ConnectionString);
            Server = builder.DataSource;
            Database = builder.InitialCatalog;
        }
    }
}