using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Blondin.LightCollections
{
    [DebuggerTypeProxy(typeof(DictionaryDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(false)]
    public class LightDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue>, ISerializable, IDeserializationCallback
    {
        private struct Entry
        {
            public int hashCode;    // Lower 31 bits of hash code, -1 if unused
            public ChunkAndIndex next;        // Index of next entry, -1 if last
            public TKey key;           // Key of entry
            public TValue value;         // Value of entry
        }

        private int _size;
        private NoLohData<ChunkAndIndex> _buckets;
        private NoLohData<Entry> _entries;
        private int count;
        private int version;
        private ChunkAndIndex freeList;
        private int freeCount;
        private IEqualityComparer<TKey> comparer;
        private KeyCollection keys;
        private ValueCollection values;
        private Object _syncRoot;

        // constants for serialization
        private const String VersionName = "Version";
        private const String HashSizeName = "HashSize";  // Must save buckets.Length
        private const String KeyValuePairsName = "KeyValuePairs";
        private const String ComparerName = "Comparer";

        public LightDictionary() : this(0, null) { }

        public LightDictionary(int capacity) : this(capacity, null) { }

        public LightDictionary(IEqualityComparer<TKey> comparer) : this(0, comparer) { }

        public LightDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            if (capacity < 0) ThrowHelper.ThrowArgumentOutOfRangeException1(ExceptionArgument.capacity);
            if (capacity > 0) Initialize(capacity);
            this.comparer = comparer ?? EqualityComparer<TKey>.Default;

#if FEATURE_CORECLR
            if (HashHelpers.s_UseRandomizedStringHashing && comparer == EqualityComparer<string>.Default)
            {
                this.comparer = (IEqualityComparer<TKey>) NonRandomizedStringEqualityComparer.Default;
            }
#endif // FEATURE_CORECLR
        }

        public LightDictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }

        public LightDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) :
            this(dictionary != null ? dictionary.Count : 0, comparer)
        {

            if (dictionary == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
            }

            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                Add(pair.Key, pair.Value);
            }
        }

        protected LightDictionary(SerializationInfo info, StreamingContext context)
        {
            //We can't do anything with the keys and values until the entire graph has been deserialized
            //and we have a resonable estimate that GetHashCode is not going to fail.  For the time being,
            //we'll just cache this.  The graph is not valid until OnDeserialization has been called.
            HashHelpers.SerializationInfoTable.Add(this, info);
        }

        public IEqualityComparer<TKey> Comparer
        {
            get
            {
                return comparer;
            }
        }

        public int Count
        {
            get { return count - freeCount; }
        }

        public KeyCollection Keys
        {
            get
            {
                Contract.Ensures(Contract.Result<KeyCollection>() != null);
                if (keys == null) keys = new KeyCollection(this);
                return keys;
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                if (keys == null) keys = new KeyCollection(this);
                return keys;
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get
            {
                if (keys == null) keys = new KeyCollection(this);
                return keys;
            }
        }

        public ValueCollection Values
        {
            get
            {
                Contract.Ensures(Contract.Result<ValueCollection>() != null);
                if (values == null) values = new ValueCollection(this);
                return values;
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                if (values == null) values = new ValueCollection(this);
                return values;
            }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            {
                if (values == null) values = new ValueCollection(this);
                return values;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                var i = FindEntry(key);
                if (!i.IsEmpty) return _entries.Values[i.Chunk][i.IndexInChunk].value;
                ThrowHelper.ThrowKeyNotFoundException();
                return default(TValue);
            }
            set
            {
                Insert(key, value, false);
            }
        }

        public void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            Add(keyValuePair.Key, keyValuePair.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            var i = FindEntry(keyValuePair.Key);
            if (i.Chunk >= 0 && EqualityComparer<TValue>.Default.Equals(_entries.Values[i.Chunk][i.IndexInChunk].value, keyValuePair.Value))
            {
                return true;
            }
            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            var i = FindEntry(keyValuePair.Key);
            if (i.Chunk >= 0 && EqualityComparer<TValue>.Default.Equals(_entries.Values[i.Chunk][i.IndexInChunk].value, keyValuePair.Value))
            {
                Remove(keyValuePair.Key);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            if (count > 0)
            {
                _buckets.SetAllValues(ChunkAndIndex.Empty);
                _entries.ClearAllChunks();
                freeList = ChunkAndIndex.Empty;
                count = 0;
                freeCount = 0;
                version++;
            }
        }

        public bool ContainsKey(TKey key)
        {
            return FindEntry(key).Chunk >= 0;
        }

        public bool ContainsValue(TValue value)
        {
            if (value == null)
            {
                for (int i = 0; i < _entries.Values.Count; i++)
                {
                    for (int j = 0; j < _entries.Values[i].Length; j++)
                        if (_entries.Values[i][j].hashCode >= 0 && _entries.Values[i][j].value == null) return true;
                }
            }
            else
            {
                EqualityComparer<TValue> c = EqualityComparer<TValue>.Default;
                for (int i = 0; i < _entries.Values.Count; i++)
                {
                    for (int j = 0; j < _entries.Values[i].Length; j++)
                        if (_entries.Values[i][j].hashCode >= 0 && c.Equals(_entries.Values[i][j].value, value)) return true;
                }
            }
            return false;
        }

        private void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            if (array == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            }

            if (index < 0 || index > array.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException2(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            }

            if (array.Length - index < Count)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
            }

            var entries = this._entries;
            for (int i = 0; i < entries.Values.Count; i++)
                for (int j = 0; j < entries.Values[i].Length; j++)
                {
                    if (entries.Values[i][j].hashCode >= 0)
                    {
                        array[index++] = new KeyValuePair<TKey, TValue>(entries.Values[i][j].key, entries.Values[i][j].value);
                        if (index >= count) return;
                    }
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.info);
            }
            info.AddValue(VersionName, version);

#if FEATURE_RANDOMIZED_STRING_HASHING
            info.AddValue(ComparerName, HashHelpers.GetEqualityComparerForSerialization(comparer), typeof(IEqualityComparer<TKey>));
#else
            info.AddValue(ComparerName, comparer, typeof(IEqualityComparer<TKey>));
#endif

            info.AddValue(HashSizeName, _size); //This is the length of the bucket array.
            if (_size != null)
            {
                // TODO: We should rather store raw arrays to avoid storing large arrays
                KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[Count];
                CopyTo(array, 0);
                info.AddValue(KeyValuePairsName, array, typeof(KeyValuePair<TKey, TValue>[]));
            }
        }

        private ChunkAndIndex FindEntry(TKey key)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }

            if (_buckets != null)
            {
                int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
                var bucket = _buckets.GetChunkAndIndexInChunk(hashCode % _size);
                for (var pos = _buckets.Values[bucket.Chunk][bucket.IndexInChunk];
                    !pos.IsEmpty;
                    pos = _entries.Values[pos.Chunk][pos.IndexInChunk].next)
                {
                    if (_entries.Values[pos.Chunk][pos.IndexInChunk].hashCode == hashCode &&
                        comparer.Equals(_entries.Values[pos.Chunk][pos.IndexInChunk].key, key)) return pos;
                }
            }
            return ChunkAndIndex.Empty;
        }

        private void Initialize(int capacity)
        {
            _size = HashHelpers.GetPrime(capacity);
            _buckets = new NoLohData<ChunkAndIndex>();
            _entries = new NoLohData<Entry>();
            
            _buckets.EnsureSize(_size);
            _buckets.SetAllValues(ChunkAndIndex.Empty);

            _entries.EnsureSize(_size);

            freeList = ChunkAndIndex.Empty;
        }
        private void Debug()
        {
            int hashCode = comparer.GetHashCode(default(TKey)) & 0x7FFFFFFF;
            var targetBucket = _buckets.GetChunkAndIndexInChunk(hashCode % _size);
            var pos = _buckets.Values[targetBucket.Chunk][targetBucket.IndexInChunk];
            if (!pos.IsEmpty && _entries.Values[pos.Chunk][pos.IndexInChunk].hashCode == 0 &&
                _entries.Values[pos.Chunk][pos.IndexInChunk].key.Equals(default(TKey)) &&
                _entries.Values[pos.Chunk][pos.IndexInChunk].value == null)
            {
                throw new Exception("hjhlkj");
            }
        }
        private void Insert(TKey key, TValue value, bool add)
        {

            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }

            if (_buckets == null) Initialize(0);
            int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
            var targetBucket = _buckets.GetChunkAndIndexInChunk(hashCode % _size);

#if FEATURE_RANDOMIZED_STRING_HASHING
            int collisionCount = 0;
#endif
            for (var pos = _buckets.Values[targetBucket.Chunk][targetBucket.IndexInChunk];
                !pos.IsEmpty;
                pos = _entries.Values[pos.Chunk][pos.IndexInChunk].next)
            {
                if (_entries.Values[pos.Chunk][pos.IndexInChunk].hashCode == hashCode &&
                    comparer.Equals(_entries.Values[pos.Chunk][pos.IndexInChunk].key, key))
                {
                    if (add)
                    {
                        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_AddingDuplicate);
                    }
                    _entries.Values[pos.Chunk][pos.IndexInChunk].value = value;
                    version++;
                    return;
                }

#if FEATURE_RANDOMIZED_STRING_HASHING
                collisionCount++;
#endif
            }
            ChunkAndIndex index;
            if (freeCount > 0)
            {
                index = freeList;
                freeList = _entries.Values[index.Chunk][index.IndexInChunk].next;
                freeCount--;
            }
            else
            {
                if (count == _size)
                {
                    Resize();
                    targetBucket = _buckets.GetChunkAndIndexInChunk(hashCode % _size);
                }
                index = _entries.GetChunkAndIndexInChunk(count);
                count++;
            }

            _entries.Values[index.Chunk][index.IndexInChunk].hashCode = hashCode;
            _entries.Values[index.Chunk][index.IndexInChunk].next = _buckets.Values[targetBucket.Chunk][targetBucket.IndexInChunk];
            _entries.Values[index.Chunk][index.IndexInChunk].key = key;
            _entries.Values[index.Chunk][index.IndexInChunk].value = value;
            _buckets.Values[targetBucket.Chunk][targetBucket.IndexInChunk] = index;
            version++;

#if FEATURE_RANDOMIZED_STRING_HASHING
 
#if FEATURE_CORECLR
            // In case we hit the collision threshold we'll need to switch to the comparer which is using randomized string hashing
            // in this case will be EqualityComparer<string>.Default.
            // Note, randomized string hashing is turned on by default on coreclr so EqualityComparer<string>.Default will 
            // be using randomized string hashing
 
            if (collisionCount > HashHelpers.HashCollisionThreshold && comparer == NonRandomizedStringEqualityComparer.Default) 
            {
                comparer = (IEqualityComparer<TKey>) EqualityComparer<string>.Default;
                Resize(entries.Length, true);
            }
#else
            if(collisionCount > HashHelpers.HashCollisionThreshold && HashHelpers.IsWellKnownEqualityComparer(comparer)) 
            {
                comparer = (IEqualityComparer<TKey>) HashHelpers.GetRandomizedEqualityComparer(comparer);
                Resize(entries.Length, true);
            }
#endif // FEATURE_CORECLR
 
#endif
            Debug();
        }

        public virtual void OnDeserialization(Object sender)
        {
            SerializationInfo siInfo;
            HashHelpers.SerializationInfoTable.TryGetValue(this, out siInfo);

            if (siInfo == null)
            {
                // It might be necessary to call OnDeserialization from a container if the container object also implements
                // OnDeserialization. However, remoting will call OnDeserialization again.
                // We can return immediately if this function is called twice. 
                // Note we set remove the serialization info from the table at the end of this method.
                return;
            }

            int realVersion = siInfo.GetInt32(VersionName);
            int hashsize = siInfo.GetInt32(HashSizeName);
            comparer = (IEqualityComparer<TKey>)siInfo.GetValue(ComparerName, typeof(IEqualityComparer<TKey>));

            if (hashsize != 0)
            {
                Resize(hashsize, false);
                freeList = ChunkAndIndex.Empty;

                KeyValuePair<TKey, TValue>[] array = (KeyValuePair<TKey, TValue>[])
                    siInfo.GetValue(KeyValuePairsName, typeof(KeyValuePair<TKey, TValue>[]));

                if (array == null)
                {
                    ThrowHelper.ThrowSerializationException(ExceptionResource.Serialization_MissingKeys);
                }

                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].Key == null)
                    {
                        ThrowHelper.ThrowSerializationException(ExceptionResource.Serialization_NullKey);
                    }
                    Insert(array[i].Key, array[i].Value, true);
                }
            }
            else
            {
                _buckets = null;
            }

            version = realVersion;
            HashHelpers.SerializationInfoTable.Remove(this);
        }

        private void Resize()
        {
            Resize(HashHelpers.ExpandPrime(count), false);
        }

        private void Resize(int newSize, bool forceNewHashCodes)
        {
            Contract.Assert(newSize >= _size);
            _buckets.EnsureSize(newSize);
            _buckets.SetAllValues(ChunkAndIndex.Empty);
            _entries.EnsureSize(newSize);
            if (forceNewHashCodes)
            {
                for (int i = 0; i < _entries.Values.Count; i++)
                    for (int j = 0; j < _entries.Values[i].Length; j++)
                    {
                        if (_entries.Values[i][j].hashCode != -1)
                        {
                            _entries.Values[i][j].hashCode = (comparer.GetHashCode(_entries.Values[i][j].key) & 0x7FFFFFFF);
                        }
                    }
            }
            int temp = 0;
            for (int i = 0; i < _entries.Values.Count && temp < count; i++)
                for (int j = 0; j < _entries.Values[i].Length && temp < count; j++)
                {
                    if (_entries.Values[i][j].hashCode >= 0)
                    {
                        var bucket = _buckets.GetChunkAndIndexInChunk(_entries.Values[i][j].hashCode % newSize);
                        _entries.Values[i][j].next = _buckets.Values[bucket.Chunk][bucket.IndexInChunk];
                        _buckets.Values[bucket.Chunk][bucket.IndexInChunk] = new ChunkAndIndex(i, j);
                        temp++;
                    }
                }
            _size = newSize;
            Debug();
        }

        public bool Remove(TKey key)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }

            if (_buckets != null)
            {
                int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
                var bucket = _buckets.GetChunkAndIndexInChunk(hashCode % _size);
                ChunkAndIndex last = ChunkAndIndex.Empty;
                for (var pos = _buckets.Values[bucket.Chunk][bucket.IndexInChunk];
                    !pos.IsEmpty;
                    last = pos, pos = _entries.Values[pos.Chunk][pos.IndexInChunk].next)
                {
                    if (_entries.Values[pos.Chunk][pos.IndexInChunk].hashCode == hashCode &&
                        comparer.Equals(_entries.Values[pos.Chunk][pos.IndexInChunk].key, key))
                    {
                        if (last.IsEmpty)
                        {
                            _buckets.Values[bucket.Chunk][bucket.IndexInChunk] = _entries.Values[pos.Chunk][pos.IndexInChunk].next;
                        }
                        else
                        {
                            _entries.Values[last.Chunk][last.IndexInChunk].next = _entries.Values[pos.Chunk][pos.IndexInChunk].next;
                        }
                        _entries.Values[pos.Chunk][pos.IndexInChunk].hashCode = -1;
                        _entries.Values[pos.Chunk][pos.IndexInChunk].next = freeList;
                        _entries.Values[pos.Chunk][pos.IndexInChunk].key = default(TKey);
                        _entries.Values[pos.Chunk][pos.IndexInChunk].value = default(TValue);
                        freeList = pos;
                        freeCount++;
                        version++;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var i = FindEntry(key);
            if (i.Chunk >= 0)
            {
                value = _entries.Values[i.Chunk][i.IndexInChunk].value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        // This is a convenience method for the internal callers that were converted from using Hashtable.
        // Many were combining key doesn't exist and key exists but null value (for non-value types) checks.
        // This allows them to continue getting that behavior with minimal code delta. This is basically
        // TryGetValue without the out param
        internal TValue GetValueOrDefault(TKey key)
        {
            var i = FindEntry(key);
            if (i.Chunk >= 0)
            {
                return _entries.Values[i.Chunk][i.IndexInChunk].value;
            }
            return default(TValue);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
            CopyTo(array, index);
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

            if (array.Length - index < Count)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
            }

            KeyValuePair<TKey, TValue>[] pairs = array as KeyValuePair<TKey, TValue>[];
            if (pairs != null)
            {
                CopyTo(pairs, index);
            }
            else if (array is DictionaryEntry[])
            {
                DictionaryEntry[] dictEntryArray = array as DictionaryEntry[];
                var entries = this._entries;
                for (int i = 0; i < entries.Values.Count; i++)
                    for (int j = 0; j < entries.Values[i].Length; j++)
                    {
                        if (entries.Values[i][j].hashCode >= 0)
                        {
                            dictEntryArray[index++] = new DictionaryEntry(entries.Values[i][j].key, entries.Values[i][j].value);
                        }
                }
            }
            else
            {
                object[] objects = array as object[];
                if (objects == null)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                }

                try
                {
                    var entries = this._entries;
                    for (int i = 0; i < entries.Values.Count; i++)
                        for (int j = 0; j < entries.Values[i].Length; j++)
                        {
                            if (entries.Values[i][j].hashCode >= 0)
                            {
                                objects[index++] = new KeyValuePair<TKey, TValue>(entries.Values[i][j].key, entries.Values[i][j].value);
                            }
                        }
                }
                catch (ArrayTypeMismatchException)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);
                }
                return _syncRoot;
            }
        }

        bool IDictionary.IsFixedSize
        {
            get { return false; }
        }

        bool IDictionary.IsReadOnly
        {
            get { return false; }
        }

        ICollection IDictionary.Keys
        {
            get { return (ICollection)Keys; }
        }

        ICollection IDictionary.Values
        {
            get { return (ICollection)Values; }
        }

        object IDictionary.this[object key]
        {
            get
            {
                if (IsCompatibleKey(key))
                {
                    var i = FindEntry((TKey)key);
                    if (i.Chunk >= 0)
                    {
                        return _entries.Values[i.Chunk][i.IndexInChunk].value;
                    }
                }
                return null;
            }
            set
            {
                if (key == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
                }
                ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>(value, ExceptionArgument.value);

                try
                {
                    TKey tempKey = (TKey)key;
                    try
                    {
                        this[tempKey] = (TValue)value;
                    }
                    catch (InvalidCastException)
                    {
                        ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(TValue));
                    }
                }
                catch (InvalidCastException)
                {
                    ThrowHelper.ThrowWrongKeyTypeArgumentException(key, typeof(TKey));
                }
            }
        }

        private static bool IsCompatibleKey(object key)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }
            return (key is TKey);
        }

        void IDictionary.Add(object key, object value)
        {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }
            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>(value, ExceptionArgument.value);

            try
            {
                TKey tempKey = (TKey)key;

                try
                {
                    Add(tempKey, (TValue)value);
                }
                catch (InvalidCastException)
                {
                    ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(TValue));
                }
            }
            catch (InvalidCastException)
            {
                ThrowHelper.ThrowWrongKeyTypeArgumentException(key, typeof(TKey));
            }
        }

        bool IDictionary.Contains(object key)
        {
            if (IsCompatibleKey(key))
            {
                return ContainsKey((TKey)key);
            }

            return false;
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new Enumerator(this, Enumerator.DictEntry);
        }

        void IDictionary.Remove(object key)
        {
            if (IsCompatibleKey(key))
            {
                Remove((TKey)key);
            }
        }

        [Serializable]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>,
            IDictionaryEnumerator
        {
            private LightDictionary<TKey, TValue> dictionary;
            private int version;
            private ChunkAndIndex index;
            private int absoluteIndex;
            private KeyValuePair<TKey, TValue> current;
            private int getEnumeratorRetType;  // What should Enumerator.Current return?

            internal const int DictEntry = 1;
            internal const int KeyValuePair = 2;

            internal Enumerator(LightDictionary<TKey, TValue> dictionary, int getEnumeratorRetType)
            {
                this.dictionary = dictionary;
                version = dictionary.version;
                index = new ChunkAndIndex();
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

                while (absoluteIndex < dictionary._size)
                {
                    if (dictionary._entries.Values[index.Chunk][index.IndexInChunk].hashCode >= 0)
                    {
                        current = new KeyValuePair<TKey, TValue>(dictionary._entries.Values[index.Chunk][index.IndexInChunk].key, dictionary._entries.Values[index.Chunk][index.IndexInChunk].value);
                        IncrementIndex();
                        return true;
                    }
                    IncrementIndex();
                }

                absoluteIndex = dictionary.count + 1;
                current = new KeyValuePair<TKey, TValue>();
                return false;
            }

            private void IncrementIndex()
            {
                index.IndexInChunk++;
                if (index.IndexInChunk >= dictionary._entries.Values[index.Chunk].Length)
                {
                    index.Chunk++;
                    index.IndexInChunk = 0;
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

                index = new ChunkAndIndex();
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
                for (int i = 0; i < entries.Values.Count && temp < count; i++)
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
                        for (int i = 0; i < entries.Values.Count; i++)
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
                private ChunkAndIndex index;
                private int absoluteIndex;
                private int version;
                private TKey currentKey;

                internal Enumerator(LightDictionary<TKey, TValue> dictionary)
                {
                    this.dictionary = dictionary;
                    version = dictionary.version;
                    index = new ChunkAndIndex();
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

                    while (absoluteIndex < dictionary._size)
                    {
                        if (dictionary._entries.Values[index.Chunk][index.IndexInChunk].hashCode >= 0)
                        {
                            currentKey = dictionary._entries.Values[index.Chunk][index.IndexInChunk].key;
                            IncrementIndex();
                            return true;
                        }
                        IncrementIndex();
                    }

                    absoluteIndex = dictionary.count + 1;
                    currentKey = default(TKey);
                    return false;
                }

                private void IncrementIndex()
                {
                    index.IndexInChunk++;
                    if (index.IndexInChunk >= dictionary._entries.Values[index.Chunk].Length)
                    {
                        index.Chunk++;
                        index.IndexInChunk = 0;
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
                    index = new ChunkAndIndex();
                    absoluteIndex = 0;
                    currentKey = default(TKey);
                }
            }
        }

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
                    ThrowHelper.ThrowArgumentOutOfRangeException2(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                }

                if (array.Length - index < dictionary.Count)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }

                var count = dictionary.count;
                var entries = dictionary._entries;
                var temp = 0;
                for (int i = 0; i < entries.Values.Count && temp < count; i++)
                    for (int j = 0; j < entries.Values[i].Length && temp < count; j++)
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
                    ThrowHelper.ThrowArgumentOutOfRangeException2(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
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
                    try
                    {
                        for (int i = 0; i < entries.Values.Count; i++)
                            for (int j = 0; j < entries.Values[i].Length; j++)
                            {
                                if (entries.Values[i][j].hashCode >= 0) objects[index++] = entries.Values[i][j].value;
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
                private ChunkAndIndex index;
                private int absoluteIndex;
                private int version;
                private TValue currentValue;

                internal Enumerator(LightDictionary<TKey, TValue> dictionary)
                {
                    this.dictionary = dictionary;
                    version = dictionary.version;
                    index = new ChunkAndIndex();
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

                    while (absoluteIndex < dictionary._size)
                    {
                        if (dictionary._entries.Values[index.Chunk][index.IndexInChunk].hashCode >= 0)
                        {
                            currentValue = dictionary._entries.Values[index.Chunk][index.IndexInChunk].value;
                            IncrementIndex();
                            return true;
                        }
                        IncrementIndex();
                    }
                    absoluteIndex = dictionary.count + 1;
                    currentValue = default(TValue);
                    return false;
                }

                private void IncrementIndex()
                {
                    index.IndexInChunk++;
                    if (index.IndexInChunk >= dictionary._entries.Values[index.Chunk].Length)
                    {
                        index.Chunk++;
                        index.IndexInChunk = 0;
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
                    index = new ChunkAndIndex();
                    absoluteIndex = 0;
                    currentValue = default(TValue);
                }
            }
        }
    }
}
