using System;

namespace MDLSoft.NHibernate.Dao.DynQueries
{
    public class PagedQueryGenerator : IDynQuery
    {
        const string COMPLETE_QUERY = @"SELECT TOP {@top}
                                        *
                                        FROM 
                                        (
                                        SELECT *
                                        ,ROW_NUMBER() OVER(ORDER BY {@order}) as NHibernateSortRow 
                                        from (
                                                 {@query}
                                        ) as tabla
                                        ) as tabla2
                                        WHERE tabla2.NHibernateSortRow  > {@lastRow}
                                        ORDER BY {@order}";

        private readonly IDynQuery internalQuery;
        private readonly int pageSize;
        private readonly int currentPage;
        private readonly string order;

        public PagedQueryGenerator(IDynQuery internalQuery, int pageSize, int currentPage, string order)
        {
            this.internalQuery = internalQuery;
            this.pageSize = pageSize;
            this.currentPage = currentPage;
            this.order = order;
        }


        public IDynQuery WithOrder(bool value)
        {
            throw new NotImplementedException();
        }

        //TODO:Optimizar primer pagina
        public string Render()
        {
            string @internal = internalQuery.WithOrder(false).Render();

            return COMPLETE_QUERY
                .Replace("{@top}", pageSize.ToString())
                .Replace("{@query}", @internal)
                .Replace("{@order}", order)
                .Replace("{@lastRow}", (pageSize * (currentPage - 1)).ToString())
                ;
        }
    }
}