using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Multi;

namespace MDLSoft.NHibernate.Dao
{
    public class DaoReadOnlyBase<T> : IDaoReadOnly<T> where T : class
    {
        private readonly ISessionFactory factory;

        public DaoReadOnlyBase(ISessionFactory factory)
        {
            this.factory = factory;
        }

        protected ISession Session => factory.GetCurrentSession();

        public T Get(object id)
        {
            return Session.Get<T>(id);
        }

        public T GetProxy(object id)
        {
            return Session.Load<T>(id);
        }
        /*
        public void Refresh(T entity)
        {
            if ((entity != null) && entity.Id == 0)
                return;

            var id = Session.GetIdentifier(entity);
            Session.Evict(entity);
            Session.Load(entity, id);
        }
        */
        public IEnumerable<T> Retrieve(Expression<Func<T, bool>> predicate)
        {
            //return Session.QueryOver<T>().Where(predicate).List();
            return Session.Query<T>().Where(predicate);
        }

        public IEnumerable<T> GetAll()
        {
            return Session.CreateCriteria<T>().List<T>();
        }

        public int Count(Expression<Func<T, bool>> predicate)
        {
            //return Session.QueryOver<T>().Where(predicate).List().Count;
            return Session.Query<T>().Count(predicate);
        }

        public TResult Max<TResult>(Expression<Func<T, TResult>> selector)
        {
            return Session.Query<T>().Any() ? Session.Query<T>().Max(selector) : default(TResult);
        }

        private IList<TE> InternalSearchPaged<TE>(DataSearchPaged data, ICriteria query, out long totalRegisterCount)
        {
            if (data.Page != 0 && data.Rows != 0)
            {
                query.SetFirstResult((data.Page - 1) * data.Rows);
                query.SetMaxResults(data.Rows);
            }

            foreach (var order in data.Sidx.Split('|').Select(ord => (data.Sord.ToLower() == "asc") ? Order.Asc(ord) : Order.Desc(ord)))
            {
                query.AddOrder(order);
            }

            var countCriteria = CriteriaTransformer.TransformToRowCount(query);

            var queries = Session.CreateQueryBatch().Add<TE>(query).Add<int>(countCriteria);
            var all = queries.GetResult<TE>(0);
            totalRegisterCount = Convert.ToInt64(queries.GetResult<int>(1).Single());
            return all;
        }

        public IList<T> SearchPaged(DataSearchPaged data, IList<ICriterion> criterias, out long totalRegisterCount)
        {
            var query = Session.CreateCriteria(typeof(T));

            if (criterias == null)
            {
                return InternalSearchPaged<T>(data, query, out totalRegisterCount);
            }

            foreach (var cri in criterias)
            {
                query.Add(cri);
            }

            return InternalSearchPaged<T>(data, query, out totalRegisterCount);
        }

        public IList<T> SearchPaged(DataSearchPaged data, DetachedCriteria detachedCriteria, out long totalRegisterCount)
        {
            var crit = detachedCriteria.GetExecutableCriteria(Session);
            return InternalSearchPaged<T>(data, crit, out totalRegisterCount);
        }

        public IList<T> Search(DataSearchBase data, IList<ICriterion> criterias, out long totalRegisterCount)
        {
            var dataSearch = new DataSearchPaged { Sidx = data.Sidx, Sord = data.Sord };
            //long outRows;
            return SearchPaged(dataSearch, criterias, out totalRegisterCount);
        }

        public IList<T> Search(DataSearchBase data, DetachedCriteria detachedCriteria, out long totalRegisterCount)
        {
            var dataSearch = new DataSearchPaged { Sidx = data.Sidx, Sord = data.Sord };
            //long outRows;
            var crit = detachedCriteria.GetExecutableCriteria(Session);
            return InternalSearchPaged<T>(dataSearch, crit, out totalRegisterCount);
        }

        public IList<TE> SearchPaged<TE>(DataSearchPaged data, DetachedCriteria detachedCriteria, out long totalRegisterCount)
        {
            var crit = detachedCriteria.GetExecutableCriteria(Session);
            return InternalSearchPaged<TE>(data, crit, out totalRegisterCount);
        }

        public IList<TE> Search<TE>(DataSearchPaged data, DetachedCriteria detachedCriteria, out long totalRegisterCount)
        {
            var crit = detachedCriteria.GetExecutableCriteria(Session);
            return InternalSearchPaged<TE>(data, crit, out totalRegisterCount);
        }
    }
}