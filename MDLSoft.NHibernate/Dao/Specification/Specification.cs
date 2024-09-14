using System;
using System.Linq.Expressions;

namespace MDLSoft.NHibernate.Dao.Specification
{
    public abstract class Specification<T>
    {
        public abstract Expression<Func<T, bool>> Spec();

        public static Specification<T> operator &(Specification<T> spec1, Specification<T> spec2)
        {
            return new AndSpecification<T>(spec1.Spec(), spec2.Spec());
        }

        public static Specification<T> operator |(Specification<T> spec1, Specification<T> spec2)
        {
            return new OrSpecification<T>(spec1.Spec(), spec2.Spec());
        }

        public static Specification<T> operator !(Specification<T> spec1)
        {
            return new NegateSpecification<T>(spec1.Spec());
        }
    }
}