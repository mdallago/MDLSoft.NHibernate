namespace MDLSoft.NHibernate.Dao.DynQueries
{
    public interface IDynQuery
    {
        IDynQuery WithOrder(bool value);
        string Render();
    }
}