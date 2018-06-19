using System;
using System.Collections;
using System.Collections.Generic;

namespace Blondin.LightCollections
{
    public partial class LightDictionary<TKey, TValue>
    {
        [Serializable]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>,
            IDictionaryEnumerator
        {
            private LightDictionary<TKey, TValue> dictionary;
            private int version;
            private int index;
            private int absoluteIndex;
            private KeyValuePair<TKey, TValue> current;
            private int getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;

            internal Enumerator(LightDictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
            {
                this.dictionary = dictionary;
                version = dictionary.version;
                index = 0;
                absoluteIndex = 0;
                this.getEnumeratorRetType = getEnumeratorRetType;
                current = new KeyValuePair<TKey, TValue>();
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
                        current = new KeyValuePair<TKey, TValue>(dictionary._entries.Values[chunk][indexInChunk].key, dictionary._entries.Values[chunk][indexInChunk].value);
                        IncrementIndex(ref chunk, ref indexInChunk);
                        index = MergeChunkAndIndexInChunk(dictionary._maxEntryChunkElementCount, chunk, indexInChunk);
                        return true;
                    }
                    IncrementIndex(ref chunk, ref indexInChunk);
                }
                index = MergeChunkAndIndexInChunk(dictionary._maxEntryChunkElementCount, chunk, indexInChunk);

                absoluteIndex = dictionary.count + 1;
                current = new KeyValuePair<TKey, TValue>();
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

            public KeyValuePair<TKey, TValue> Current
            {
                get { return current; }
            }

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get
                {
                    if (absoluteIndex == 0 || (absoluteIndex == dictionary.count + 1))
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }

                    if (getEnumeratorRetType == DictEntry)
                    {
                        return new System.Collections.DictionaryEntry(current.Key, current.Value);
                    }
                    else
                    {
                        return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                    }
                }
            }

            void IEnumerator.Reset()
            {
                if (version != dictionary.version)
                {
                    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                }

                index = 0;
                absoluteIndex = 0;
                current = new KeyValuePair<TKey, TValue>();
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    if (absoluteIndex == 0 || (absoluteIndex == dictionary.count + 1))
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }

                    return new DictionaryEntry(current.Key, current.Value);
                }
            }

            object IDictionaryEnumerator.Key
            {
                get
                {
                    if (absoluteIndex == 0 || (absoluteIndex == dictionary.count + 1))
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }

                    return current.Key;
                }
            }

            object IDictionaryEnumerator.Value
            {
                get
                {
                    if (absoluteIndex == 0 || (absoluteIndex == dictionary.count + 1))
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }

                    return current.Value;
                }
            }
        }
    }
}
