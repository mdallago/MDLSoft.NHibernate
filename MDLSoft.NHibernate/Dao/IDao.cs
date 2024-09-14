namespace MDLSoft.NHibernate.Dao
{
    public interface IDao<T> : IDaoReadOnly<T>
    {
        T MakePersistent(T entity);
        T Save(T entity);
        void MakeTransient(T entity);
    }
}
