using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace SaneWpf.Framework
{
    public static class Validations
    {
        [Conditional("DEBUG")]
        public static void Add<T, TU>(
            T viewModel,
            Expression<Func<T, TU>> property,
            Func<TU, bool> errorCondition, 
            Func<T, Validation> validation)
        {
        }
    }
}
