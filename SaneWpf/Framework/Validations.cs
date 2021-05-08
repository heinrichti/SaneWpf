using System;
using System.Linq.Expressions;

namespace SaneWpf.Framework
{
    public static class Validations
    {
        public static void Add<T, TU>(
            T viewModel,
            Expression<Func<T, TU>> property,
            Func<TU, bool> errorCondition, 
            Func<T, Validation> validation)
        {
        }
    }
}
