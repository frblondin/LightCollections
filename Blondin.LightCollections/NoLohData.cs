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

        internal TValue[][] Values = new TValue[16][];
        internal int VirtualArrayCount;
        internal int Size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EnsureSize(int newSize)
        {
            while (newSize > Size)
            {
                var chunkSize = VirtualArrayCount < _progressiceArraySize.Count ?
                    _progressiceArraySize[VirtualArrayCount].Size :
                    _maxArrayElementCount;
                var chunk = new TValue[chunkSize];

                if (Values.Length == VirtualArrayCount)
                {
                    var newValues = new TValue[Values.Length * 2][];
                    Array.Copy(Values, newValues, VirtualArrayCount);
                    Values = newValues;
                }

                VirtualArrayCount++;
                Values[VirtualArrayCount - 1] = chunk;
                Size += chunkSize;
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
