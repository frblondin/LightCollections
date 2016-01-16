using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Blondin.LightCollections
{
    internal class NoLohData<TValue>
    {
        internal readonly List<TValue[]> Values = new List<TValue[]>();
        internal int Size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EnsureSize(int newSize)
        {
            while (newSize > Size)
            {
                var chunkSize = Values.Count < NoLohInfoProvider<TValue>.ProgressiveArraySize.Count ?
                    NoLohInfoProvider<TValue>.ProgressiveArraySize[Values.Count].Size :
                    NoLohInfoProvider<TValue>.MaxArrayElementCount;
                var chunk = new TValue[chunkSize];
                Values.Add(chunk);
                Size += chunkSize;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetAllValues(TValue value)
        {
            foreach (var chunk in Values)
            {
                for (int i = 0; i < chunk.Length; i++)
                    chunk[i] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ClearAllChunks()
        {
            foreach (var chunk in Values)
            {
                Array.Clear(chunk, 0, chunk.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ChunkAndIndex GetChunkAndIndexInChunk(int index)
        {
            if (index < NoLohInfoProvider<TValue>.FirstIndexUsingFixedArraySize)
            {
                return NoLohInfoProvider<TValue>._smallIndexChunkAndIndexProvider(index);
            }
            else
            {
                index -= NoLohInfoProvider<TValue>.FirstIndexUsingFixedArraySize;
                int arrayIndexOfFirstIndexUsingFixedArraySize = NoLohInfoProvider<TValue>.ProgressiveArraySize.Count;
                return new ChunkAndIndex(
                    arrayIndexOfFirstIndexUsingFixedArraySize + index / NoLohInfoProvider<TValue>.MaxArrayElementCount,
                    index % NoLohInfoProvider<TValue>.MaxArrayElementCount);
            }
        }
    }
}
