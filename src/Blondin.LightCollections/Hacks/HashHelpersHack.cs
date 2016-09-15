using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Blondin.LightCollections
{
    internal static class HashHelpers
    {
        private static readonly Type RealHashHelpersType = typeof(string).Assembly.GetType("System.Collections.HashHelpers");

        internal static readonly Func<int, int> ExpandPrime;
        internal static readonly Func<int, int> GetPrime;

        private static ConditionalWeakTable<object, SerializationInfo> s_SerializationInfoTable;
        internal static ConditionalWeakTable<object, SerializationInfo> SerializationInfoTable
        {
            get
            {
                if (s_SerializationInfoTable == null)
                    s_SerializationInfoTable = (ConditionalWeakTable<object, SerializationInfo>)RealHashHelpersType.GetProperty("SerializationInfoTable", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

                return s_SerializationInfoTable;
            }
        }

        static HashHelpers()
        {
            var intArg = Expression.Parameter(typeof(int));

            var expandPrimeLambda = Expression.Lambda<Func<int, int>>(
                Expression.Call(RealHashHelpersType.GetMethod("ExpandPrime", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static),
                    intArg),
                intArg);
            ExpandPrime = expandPrimeLambda.Compile();

            var getPrimeLambda = Expression.Lambda<Func<int, int>>(
                Expression.Call(RealHashHelpersType.GetMethod("GetPrime", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static),
                    intArg),
                intArg);
            GetPrime = getPrimeLambda.Compile();
        }
    }
}
