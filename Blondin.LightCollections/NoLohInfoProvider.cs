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
        public const int LohMinSize = 85000;

        public static int GetMemoryFootprint(Type type)
        {
            if (type.IsClass || type.IsPrimitive) return 4;
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

    internal struct ChunkAndIndex
    {
        internal static readonly ConstructorInfo _constructor = typeof(ChunkAndIndex).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(int), typeof(int) }, null);
        internal static readonly ChunkAndIndex Empty = new ChunkAndIndex(-1, -1);

        internal int Chunk;
        internal int IndexInChunk;
        internal bool IsEmpty { get { return Chunk == -1 && IndexInChunk == -1; } }

        internal ChunkAndIndex(int chunk, int indexInChunk)
        {
            this.Chunk = chunk;
            this.IndexInChunk = indexInChunk;
        }
    }

    public static class NoLohInfoProvider<T>
    {
        public static int ElementSizeInBytes { get; private set; }
        public static int MaxArrayElementCount { get; private set; }
        public static IReadOnlyList<NoLohChunkData> ProgressiveArraySize { get; private set; }
        internal static Func<int, ChunkAndIndex> _smallIndexChunkAndIndexProvider;
        public static int FirstIndexUsingFixedArraySize { get; private set; }

        static NoLohInfoProvider()
        {
            ElementSizeInBytes = NoLohInfoProvider.GetMemoryFootprint(typeof(T));
            MaxArrayElementCount = NoLohInfoProvider.LohMinSize / ElementSizeInBytes - 1;
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
            var expression = default(ConditionalExpression);
            int i = ProgressiveArraySize.Count - 1;
            foreach (var data in ProgressiveArraySize.Reverse())
            {
                expression = Expression.Condition(
                    Expression.LessThan(lambdaIndexArg, Expression.Constant(data.StartIndex + data.Size)),
                    Expression.New(ChunkAndIndex._constructor,
                        Expression.Constant(i--),
                        Expression.Subtract(lambdaIndexArg, Expression.Constant(data.StartIndex))),
                    (Expression)expression ?? Expression.Default(typeof(ChunkAndIndex)));
            }
            _smallIndexChunkAndIndexProvider = Expression.Lambda<Func<int, ChunkAndIndex>>(
                expression,
                lambdaIndexArg).Compile();
        }
    }
}
