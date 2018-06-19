using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Blondin.LightCollections
{
    public partial class LightDictionary<TKey, TValue>
    {
        [DebuggerTypeProxy(typeof(DictionaryKeyCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
        [Serializable]
        public sealed class KeyCollection : ICollection<TKey>, ICollection, IReadOnlyCollection<TKey>
        {
            private LightDictionary<TKey, TValue> dictionary;

            public KeyCollection(LightDictionary<TKey, TValue> dictionary)
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

            public void CopyTo(TKey[] array, int index)
            {
                if (array == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                }

                if (index < 0 || index > array.Length)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException2(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                }

                if (array.Length - index < dictionary.Count)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }

                var count = dictionary.count;
                var entries = dictionary._entries;
                var temp = 0;
                for (int i = 0; i < entries.VirtualArrayCount && temp < count; i++)
                    for (int j = 0; j < entries.Values[i].Length && temp < count; j++)
                    {
                        if (entries.Values[i][j].hashCode >= 0)
                        {
                            array[index++] = entries.Values[i][j].key;
                            temp++;
                        }
                    }
            }

            public int Count
            {
                get { return dictionary.Count; }
            }

            bool ICollection<TKey>.IsReadOnly
            {
                get { return true; }
            }

            void ICollection<TKey>.Add(TKey item)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
            }

            void ICollection<TKey>.Clear()
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
            }

            bool ICollection<TKey>.Contains(TKey item)
            {
                return dictionary.ContainsKey(item);
            }

            bool ICollection<TKey>.Remove(TKey item)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
                return false;
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
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
                    ThrowHelper.ThrowArgumentOutOfRangeException2(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                }

                if (array.Length - index < dictionary.Count)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }

                TKey[] keys = array as TKey[];
                if (keys != null)
                {
                    CopyTo(keys, index);
                }
                else
                {
                    object[] objects = array as object[];
                    if (objects == null)
                    {
                        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                    }

                    var entries = dictionary._entries;
                    try
                    {
                        for (int i = 0; i < entries.VirtualArrayCount; i++)
                            for (int j = 0; j < entries.Values[i].Length; j++)
                            {
                                if (entries.Values[i][j].hashCode >= 0) objects[index++] = entries.Values[i][j].key;
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
            public struct Enumerator : IEnumerator<TKey>, System.Collections.IEnumerator
            {
                private LightDictionary<TKey, TValue> dictionary;
                private int index;
                private int absoluteIndex;
                private int version;
                private TKey currentKey;

                internal Enumerator(LightDictionary<TKey, TValue> dictionary)
                {
                    this.dictionary = dictionary;
                    version = dictionary.version;
                    index = 0;
                    absoluteIndex = 0;
                    currentKey = default(TKey);
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
                            currentKey = dictionary._entries.Values[chunk][indexInChunk].key;
                            IncrementIndex(ref chunk, ref indexInChunk);
                            index = MergeChunkAndIndexInChunk(dictionary._maxEntryChunkElementCount, chunk, indexInChunk);
                            return true;
                        }
                        IncrementIndex(ref chunk, ref indexInChunk);
                    }
                    index = MergeChunkAndIndexInChunk(dictionary._maxEntryChunkElementCount, chunk, indexInChunk);

                    absoluteIndex = dictionary.count + 1;
                    currentKey = default(TKey);
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

                public TKey Current
                {
                    get
                    {
                        return currentKey;
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

                        return currentKey;
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
                    currentKey = default(TKey);
                }
            }
        }
    }
}
