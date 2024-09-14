using System;

namespace MDLSoft.NHibernate.Dao.DynQueries
{
    public class CountQueryGenerator : IDynQuery
    {
        const string COMPLETE_QUERY = @"select count(*) as Count
                                        from 
                                        (
                                        {@query}
                                        ) as t";

        private readonly IDynQuery internalQuery;

        public CountQueryGenerator(IDynQuery internalQuery)
        {
            this.internalQuery = internalQuery;
        }

        public IDynQuery WithOrder(bool value)
        {
            throw new NotImplementedException();
        }

        public string Render()
        {
            var @internal = internalQuery.WithOrder(false).Render();
            return COMPLETE_QUERY.Replace("{@query}", @internal);
        }
    }
}