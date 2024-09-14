namespace MDLSoft.NHibernate.Dao
{
    public interface  IQuery<TResult> : IQuery
    {
        TResult Execute();
    }

    public interface  IQuery
    {
        
    }
}