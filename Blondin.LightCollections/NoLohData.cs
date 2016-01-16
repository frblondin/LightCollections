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
        // Turns out that copying information from NoLohInfoProvider<TValue> is faster than
        // accessing the members of NoLohInfoProvider<TValue> in method calls
        private readonly int _firstIndexUsingFixedArraySize = NoLohInfoProvider<TValue>.FirstIndexUsingFixedArraySize;
        private readonly NoLohInfoProvider<TValue>.GetChunkAndIndexInChunkDelegate _getChunkAndIndexInChunkProvider = NoLohInfoProvider<TValue>._smallIndexChunkAndIndexProvider;
        private readonly int _arrayIndexOfFirstIndexUsingFixedArraySize = NoLohInfoProvider<TValue>.ProgressiveArraySize.Count;
        private readonly int _maxArrayElementCount = NoLohInfoProvider<TValue>.MaxArrayElementCount;
        private readonly IReadOnlyList<NoLohChunkData> _progressiceArraySize = NoLohInfoProvider<TValue>.ProgressiveArraySize;

        internal readonly List<TValue[]> Values = new List<TValue[]>();
        internal int Size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EnsureSize(int newSize)
        {
            while (newSize > Size)
            {
                var chunkSize = Values.Count < _progressiceArraySize.Count ?
                    _progressiceArraySize[Values.Count].Size :
                    _maxArrayElementCount;
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
        internal void GetChunkAndIndexInChunk(int index, ref int chunk, ref int indexInChunk)
        {
            if (index < _firstIndexUsingFixedArraySize)
            {
                _getChunkAndIndexInChunkProvider(index, ref chunk, ref indexInChunk);
            }
            else
            {
                index -= _firstIndexUsingFixedArraySize;
                chunk = _arrayIndexOfFirstIndexUsingFixedArraySize + index / _maxArrayElementCount;
                indexInChunk = index % _maxArrayElementCount;
            }
        }
    }
}
