using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NHibernate.Criterion;

namespace MDLSoft.NHibernate.Dao
{
    public interface IDaoReadOnly<T>
    {
        T Get(object id);
        T GetProxy(object id);
        //void Refresh(T entity);
        IEnumerable<T> Retrieve(Expression<Func<T, bool>> predicate);
        IEnumerable<T> GetAll();
        int Count(Expression<Func<T, bool>> predicate);
        TResult Max<TResult>(Expression<Func<T, TResult>> selector);
        IList<T> SearchPaged(DataSearchPaged data, IList<ICriterion> criterias, out long totalRegisterCount);
        IList<T> Search(DataSearchBase data, IList<ICriterion> criterias, out long totalRegisterCount);
        IList<T> Search(DataSearchBase data, DetachedCriteria detachedCriteria, out long totalRegisterCount);
        IList<T> SearchPaged(DataSearchPaged data, DetachedCriteria detachedCriteria, out long totalRegisterCount);
    }
}