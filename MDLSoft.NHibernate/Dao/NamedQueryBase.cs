using NHibernate;

namespace MDLSoft.NHibernate.Dao
{
    public abstract class NamedQueryBase<TResult> : INamedQuery
    {
        private readonly ISessionFactory sessionFactory;

        protected NamedQueryBase(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public virtual string QueryName
        {
            get { return GetType().Name; }
        }

        protected abstract void SetParameters(global::NHibernate.IQuery query);

        public abstract TResult Execute(global::NHibernate.IQuery query);
        
        public TResult Execute()
        {
            var query = GetNamedQuery();
            return Execute(query);
        }

        protected virtual global::NHibernate.IQuery GetNamedQuery()
        {
            var query = sessionFactory.GetCurrentSession()
                .GetNamedQuery(((INamedQuery) this).QueryName);
            SetParameters(query);
            return query;
        }
    }
}