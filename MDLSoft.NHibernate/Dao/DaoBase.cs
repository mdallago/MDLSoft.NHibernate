using NHibernate;

namespace MDLSoft.NHibernate.Dao
{
    public class DaoBase<T> : DaoReadOnlyBase<T>, IDao<T> where T : class //Entity
    {
        private readonly ISessionFactory factory;

        public DaoBase(ISessionFactory factory) : base(factory)
        {
            this.factory = factory;
        }

        public T MakePersistent(T entity)
        {
            Session.Persist(entity);
            return entity;
        }

        public void MakeTransient(T entity)
        {
            Session.Delete(entity);
        }

        public T Save(T entity)
        {
            Session.SaveOrUpdate(entity);
            return entity;
        }
    }
}
