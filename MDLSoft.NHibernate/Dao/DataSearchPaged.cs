namespace MDLSoft.NHibernate.Dao
{
    public class DataSearchPaged : DataSearchBase
    {
        public int Page { get; set; }
        public int Rows { get; set; }
    }
}
