using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Blondin.LightCollections
{
    public static class NoLohInfoProvider
    {
        public const int LohStartSize = 85000;

        public static int GetMemoryFootprint(Type type)
        {
            if (type.IsClass || type.IsPrimitive) return 1;
            return (from field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    select GetMemoryFootprint(field.FieldType)).Sum();
        }
    }

    public struct NoLohChunkData
    {
        public readonly int StartIndex;
        public readonly int Size;
        public NoLohChunkData(int startIndex, int size)
        {
            this.StartIndex = startIndex;
            this.Size = size;
        }
    }

    public static class NoLohInfoProvider<T>
    {
        internal delegate void GetChunkAndIndexInChunkDelegate(int index, ref int chunk, ref int indexInChunk);

        public static int ElementSizeInBytes { get; private set; }
        public static int MaxArrayElementCount { get; private set; }
        public static IReadOnlyList<NoLohChunkData> ProgressiveArraySize { get; private set; }
        internal static GetChunkAndIndexInChunkDelegate _smallIndexChunkAndIndexProvider;
        public static int FirstIndexUsingFixedArraySize { get; private set; }

        static NoLohInfoProvider()
        {
            ElementSizeInBytes = NoLohInfoProvider.GetMemoryFootprint(typeof(T));
            MaxArrayElementCount = NoLohInfoProvider.LohStartSize / ElementSizeInBytes - 1;
            ComputeArraysAndFirstIndexUsingFixedSize();
            CompileSmallIndexChunkAndIndexProvider();
        }

        private static void ComputeArraysAndFirstIndexUsingFixedSize()
        {
            var arrays = new List<NoLohChunkData>();
            int size = 16;
            int index = 0;
            while (size < MaxArrayElementCount)
            {
                arrays.Add(new NoLohChunkData(index, size));
                index += size;
                size *= 2;
            }
            ProgressiveArraySize = arrays.AsReadOnly();
            FirstIndexUsingFixedArraySize = index;
        }

        private static void CompileSmallIndexChunkAndIndexProvider()
        {
            var lambdaIndexArg = Expression.Parameter(typeof(int));
            var lambdaChunkArg = Expression.Parameter(typeof(int).MakeByRefType());
            var lambdaIndexInChunkArg = Expression.Parameter(typeof(int).MakeByRefType());
            var expression = default(ConditionalExpression);
            int i = ProgressiveArraySize.Count - 1;
            foreach (var data in ProgressiveArraySize.Reverse())
            {
                expression = Expression.Condition(
                    Expression.LessThan(lambdaIndexArg, Expression.Constant(data.StartIndex + data.Size)),
                    Expression.Block(typeof(void),
                        Expression.Assign(lambdaChunkArg, Expression.Constant(i--)),
                        Expression.Assign(lambdaIndexInChunkArg, Expression.Subtract(lambdaIndexArg, Expression.Constant(data.StartIndex)))),
                    (Expression)expression ?? Expression.Empty());
            }
            var delegateType = Expression.GetDelegateType(
                typeof(int), typeof(int).MakeByRefType(), typeof(int).MakeByRefType(),
                typeof(void));
            _smallIndexChunkAndIndexProvider = (GetChunkAndIndexInChunkDelegate)Expression.Lambda(typeof(GetChunkAndIndexInChunkDelegate),
                expression,
                lambdaIndexArg, lambdaChunkArg, lambdaIndexInChunkArg).Compile();
        }
    }
}
