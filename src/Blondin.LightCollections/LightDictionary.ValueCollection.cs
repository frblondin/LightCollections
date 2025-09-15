using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Blondin.LightCollections
{
    public partial class LightDictionary<TKey, TValue>
    {
        [DebuggerTypeProxy(typeof(DictionaryValueCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        [Serializable]
        public sealed class ValueCollection : ICollection<TValue>, ICollection, IReadOnlyCollection<TValue>
        {
            private LightDictionary<TKey, TValue> dictionary;

            public ValueCollection(LightDictionary<TKey, TValue> dictionary)
            {
                if (dictionary == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
                }
                this.dictionary = dictionary;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            public void CopyTo(TValue[] array, int index)
            {
                if (array == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                }

                if (index < 0 || index > array.Length)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                }

                if (array.Length - index < dictionary.Count)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }

                var count = dictionary.count;
                var entries = dictionary._entries;
                var temp = 0;
                for (int i = 0; temp < count && i < entries.VirtualArrayCount; i++)
                    for (int j = 0; temp < count && j < entries.Values[i].Length; j++)
                    {
                        if (entries.Values[i][j].hashCode >= 0)
                        {
                            array[index++] = entries.Values[i][j].value;
                            temp++;
                        }
                    }
            }

            public int Count
            {
                get { return dictionary.Count; }
            }

            bool ICollection<TValue>.IsReadOnly
            {
                get { return true; }
            }

            void ICollection<TValue>.Add(TValue item)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
            }

            bool ICollection<TValue>.Remove(TValue item)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
                return false;
            }

            void ICollection<TValue>.Clear()
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
            }

            bool ICollection<TValue>.Contains(TValue item)
            {
                return dictionary.ContainsValue(item);
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(dictionary);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                if (array == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                }

                if (array.Rank != 1)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
                }

                if (array.GetLowerBound(0) != 0)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
                }

                if (index < 0 || index > array.Length)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                }

                if (array.Length - index < dictionary.Count)
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);

                TValue[] values = array as TValue[];
                if (values != null)
                {
                    CopyTo(values, index);
                }
                else
                {
                    object[] objects = array as object[];
                    if (objects == null)
                    {
                        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                    }

                    var entries = dictionary._entries;
                    var count = dictionary.Count;
                    try
                    {
                        var temp = 0;
                        for (int i = 0; temp < count && i < entries.VirtualArrayCount; i++)
                            for (int j = 0; temp < count && j < entries.Values[i].Length; j++)
                            {
                                if (entries.Values[i][j].hashCode >= 0) objects[index++] = entries.Values[i][j].value;
                                temp++;
                            }
                    }
                    catch (ArrayTypeMismatchException)
                    {
                        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                    }
                }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            Object ICollection.SyncRoot
            {
                get { return ((ICollection)dictionary).SyncRoot; }
            }

            [Serializable]
            public struct Enumerator : IEnumerator<TValue>, System.Collections.IEnumerator
            {
                private LightDictionary<TKey, TValue> dictionary;
                private int index;
                private int absoluteIndex;
                private int version;
                private TValue currentValue;

                internal Enumerator(LightDictionary<TKey, TValue> dictionary)
                {
                    this.dictionary = dictionary;
                    version = dictionary.version;
                    index = 0;
                    absoluteIndex = 0;
                    currentValue = default(TValue);
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    if (version != dictionary.version)
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                    }

                    int chunk = -1, indexInChunk = -1;
                    ExtractChunkAndIndexInChunk(index, dictionary._maxEntryChunkElementCount, ref chunk, ref indexInChunk);
                    while (absoluteIndex < dictionary.count)
                    {
                        if (dictionary._entries.Values[chunk][indexInChunk].hashCode >= 0)
                        {
                            currentValue = dictionary._entries.Values[chunk][indexInChunk].value;
                            IncrementIndex(ref chunk, ref indexInChunk);
                            index = MergeChunkAndIndexInChunk(dictionary._maxEntryChunkElementCount, chunk, indexInChunk);
                            return true;
                        }
                        IncrementIndex(ref chunk, ref indexInChunk);
                    }
                    index = MergeChunkAndIndexInChunk(dictionary._maxEntryChunkElementCount, chunk, indexInChunk);

                    absoluteIndex = dictionary.count + 1;
                    currentValue = default(TValue);
                    return false;
                }

                private void IncrementIndex(ref int chunk, ref int indexInChunk)
                {
                    indexInChunk++;
                    if (indexInChunk >= dictionary._entries.Values[chunk].Length)
                    {
                        chunk++;
                        indexInChunk = 0;
                    }
                    absoluteIndex++;
                }

                public TValue Current
                {
                    get
                    {
                        return currentValue;
                    }
                }

                Object System.Collections.IEnumerator.Current
                {
                    get
                    {
                        if (absoluteIndex == 0 || (absoluteIndex == dictionary.count + 1))
                        {
                            ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                        }

                        return currentValue;
                    }
                }

                void System.Collections.IEnumerator.Reset()
                {
                    if (version != dictionary.version)
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                    }
                    index = 0;
                    absoluteIndex = 0;
                    currentValue = default(TValue);
                }
            }
        }
    }
}
