using NHibernate;

namespace MDLSoft.NHibernate.Dao
{
    public abstract class SqlQueryBase<TResult> 
    {
        private readonly ISessionFactory sessionFactory;

        protected SqlQueryBase(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        protected abstract string GetSqlQuery();

        protected abstract TResult Execute(global::NHibernate.IQuery query);

        public TResult Execute()
        {
            var query = GetQuery();
            return Execute(query);
        }

        protected virtual global::NHibernate.IQuery GetQuery()
        {
            var query = sessionFactory.GetCurrentSession().CreateSQLQuery(GetSqlQuery());
            return query;
        }
    }
}