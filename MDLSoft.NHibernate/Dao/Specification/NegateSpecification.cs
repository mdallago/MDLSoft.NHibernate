using System;
using System.Linq.Expressions;

namespace MDLSoft.NHibernate.Dao.Specification
{
    public class NegateSpecification<T> : Specification<T>
    {
        private readonly Expression<Func<T, bool>> expression;

        public NegateSpecification(Expression<Func<T, bool>> expression)
        {
            this.expression = expression;
        }

        public override Expression<Func<T, bool>> Spec()
        {
            ParameterExpression param = Expression.Parameter(typeof(T), "x");
            return Expression.Lambda<Func<T, bool>>(Expression.Not(Expression.Invoke(expression, param)), param);
        }
    }
}