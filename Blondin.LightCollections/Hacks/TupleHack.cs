using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    internal static class TupleHack
    {
        internal static readonly Func<int, int, int> CombineHashCodes2;
        internal static readonly Func<int, int, int, int> CombineHashCodes3;
        internal static readonly Func<int, int, int, int, int> CombineHashCodes4;
        internal static readonly Func<int, int, int, int, int, int> CombineHashCodes5;
        internal static readonly Func<int, int, int, int, int, int, int> CombineHashCodes6;
        internal static readonly Func<int, int, int, int, int, int, int, int> CombineHashCodes7;
        internal static readonly Func<int, int, int, int, int, int, int, int, int> CombineHashCodes8;

        static TupleHack()
        {
            var hashParams = Enumerable.Range(0, 8).Select(_ => Expression.Parameter(typeof(int))).ToList();

            CombineHashCodes2 = CreateHashCodeLambda<Func<int, int, int>>(2, hashParams);
            CombineHashCodes3 = CreateHashCodeLambda<Func<int, int, int, int>>(3, hashParams);
            CombineHashCodes4 = CreateHashCodeLambda<Func<int, int, int, int, int>>(4, hashParams);
            CombineHashCodes5 = CreateHashCodeLambda<Func<int, int, int, int, int, int>>(5, hashParams);
            CombineHashCodes6 = CreateHashCodeLambda<Func<int, int, int, int, int, int, int>>(6, hashParams);
            CombineHashCodes7 = CreateHashCodeLambda<Func<int, int, int, int, int, int, int, int>>(7, hashParams);
            CombineHashCodes8 = CreateHashCodeLambda<Func<int, int, int, int, int, int, int, int, int>>(8, hashParams);
        }

        private static TDelegate CreateHashCodeLambda<TDelegate>(int parameterCount, IList<ParameterExpression> hashParams)
        {
            return Expression.Lambda<TDelegate>(
                Expression.Call(ResolveCombineMethod(2), hashParams.Take(2).ToArray()),
                hashParams.Take(2).ToArray()).Compile();
        }

        private static MethodInfo ResolveCombineMethod(int parameterCount)
        {
            return typeof(Tuple).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Single(m => m.Name.Equals("CombineHashCodes") && m.GetParameters().Length == parameterCount);
        }
    }
}
