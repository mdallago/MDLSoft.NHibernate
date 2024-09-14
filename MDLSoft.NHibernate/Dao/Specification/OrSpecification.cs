using System;
using System.Linq.Expressions;

namespace MDLSoft.NHibernate.Dao.Specification
{
    public class OrSpecification<T> : Specification<T>
    {
        private readonly Expression<Func<T, bool>> spec1;
        private readonly Expression<Func<T, bool>> spec2;

        public OrSpecification(Expression<Func<T, bool>> spec1, Expression<Func<T, bool>> spec2)
        {
            this.spec1 = spec1;
            this.spec2 = spec2;
        }

        public override Expression<Func<T, bool>> Spec()
        {
            ParameterExpression param = Expression.Parameter(typeof(T), "x");
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(Expression.Invoke(spec1, param),Expression.Invoke(spec2, param)), param);
        }
    }
}