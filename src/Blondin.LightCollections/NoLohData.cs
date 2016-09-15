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
        internal readonly int _maxChunkElementCount = NoLohInfoProvider<TValue>.MaxChunkElementCount;
        private readonly IReadOnlyList<NoLohChunkData> _progressiceArraySize = NoLohInfoProvider<TValue>.ProgressiveArraySize;

        internal TValue[][] Values = new TValue[16][];
        internal int VirtualArrayCount;
        internal int Size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EnsureSize(int newSize)
        {
            if (newSize <= Size) return;

            if (newSize < _maxChunkElementCount)
            {
                var newChunk = new TValue[newSize];
                if (Values[0] != null) Array.Copy(Values[0], newChunk, Values[0].Length);
                Values[0] = newChunk;
                Size = newSize;
                VirtualArrayCount = 1;
            }

            // All chunks have a fixed length of _maxArrayElementCount

            // Make sure that the first chunk has a length of _maxArrayElementCount
            if (Size < _maxChunkElementCount)
            {
                var newChunk = new TValue[_maxChunkElementCount];
                if (Values[0] != null) Array.Copy(Values[0], newChunk, Values[0].Length);
                Values[0] = newChunk;
                Size = _maxChunkElementCount;
            }

            // Add all chunks
            while (newSize > Size)
            {
                var chunk = new TValue[_maxChunkElementCount];

                if (Values.Length == VirtualArrayCount)
                {
                    var newValues = new TValue[Values.Length * 2][];
                    Array.Copy(Values, newValues, VirtualArrayCount);
                    Values = newValues;
                }

                VirtualArrayCount++;
                Values[VirtualArrayCount - 1] = chunk;
                Size += _maxChunkElementCount;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetAllValues(TValue value)
        {
            for (int i = 0; i < VirtualArrayCount; i++)
            {
                for (int j = 0; j < Values[i].Length; j++)
                {
                    Values[i][j] = value;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ClearAllChunks()
        {
            for (int i = 0; i < VirtualArrayCount; i++)
            {
                Array.Clear(Values[i], 0, Values[i].Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void GetChunkAndIndexInChunkz(int index, ref int chunk, ref int indexInChunk)
        {
            if (index < _firstIndexUsingFixedArraySize)
            {
                _getChunkAndIndexInChunkProvider(index, ref chunk, ref indexInChunk);
            }
            else
            {
                index -= _firstIndexUsingFixedArraySize;
                chunk = _arrayIndexOfFirstIndexUsingFixedArraySize + index / _maxChunkElementCount;
                indexInChunk = index % _maxChunkElementCount;
            }
        }
    }
}
