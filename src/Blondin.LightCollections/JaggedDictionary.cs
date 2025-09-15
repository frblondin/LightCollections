using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blondin.LightCollections
{
    /// <summary>Represents a n-dimension collection of keys and values.</summary>
    /// <typeparam name="TKey">The type of the keys in the dimensions of the jagged dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [DebuggerTypeProxy(typeof(JaggedDictionaryDebugView<,>))]
    [DebuggerDisplay("Depth = {Depth}, Count = {Count}")]
    public partial class JaggedDictionary<TKey, TValue> : IDictionary<IJaggedIndex<TKey>, TValue>
    {
        #region Enumerator
        internal struct Enumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
        {
            internal delegate T Extractor(IDictionaryEnumerator[] enums);

            private readonly int _depth;
            private readonly IDictionaryEnumerator[] _enumerators;
            private readonly Extractor _extractor;

            public Enumerator(JaggedDictionary<TKey, TValue> dictionary, Extractor extractor)
            {
                _depth = dictionary.Depth;
                _enumerators = new IDictionaryEnumerator[_depth];
                _enumerators[0] = dictionary._root.GetEnumerator();
                _extractor = extractor;
            }

            public T Current
            {
                get
                {
                    if (_enumerators[_depth - 1] == null)
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);

                    return _extractor(_enumerators);
                }
            }

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public bool MoveNext()
            {
                var i = 0;
                while (i < _depth)
                {
                    if (_enumerators[i] == null)
                    {
                        if (!_enumerators[i - 1].MoveNext())
                        {
                            i--;
                            if (i == 0) return false;
                            _enumerators[i] = null;
                            continue;
                        }
                        var upperDictionary = (IDictionary)_enumerators[i - 1].Value;
                        _enumerators[i] = upperDictionary.GetEnumerator();
                    }
                    if (i == _depth - 1)
                    {
                        if (!_enumerators[i].MoveNext())
                        {
                            if (i == 0) return false;
                            _enumerators[i] = null;
                            continue;
                        }
                        return true;
                    }
                    i++;
                }
                return false;
            }

            public void Reset()
            {
                for (var i = 0; i < _depth; i++) _enumerators[i] = null;
            }
        }
        #endregion

        private static readonly Enumerator<TValue>.Extractor ValueExtractor = (enums => (TValue)enums.Last().Value);
        private static readonly Enumerator<IJaggedIndex<TKey>>.Extractor KeyExtractor = (enums => JaggedIndex.Create<TKey>(enums.Select(e => (TKey)e.Key).ToArray()));

        private readonly IDictionaryFactory<TKey> _dictionaryFactory;
        private readonly System.Collections.IDictionary _root;
        /// <summary>Gets the depth.</summary>
        /// <value>The depth.</value>
        public int Depth { get; private set; }
        private int _count;

        /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        public int Count { get { return _count; } }

        /// <summary>Gets or sets the value associated with the specified jagged index.</summary>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="T:System.Collections.Generic.KeyNotFoundException" />, and a set operation creates a new element with the specified key.</returns>
        /// <param name="key">The key of the value to get or set.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is null.</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> does not exist in the collection.</exception>
        public TValue this[IJaggedIndex<TKey> key]
        {
            get
            {
                var dictionary = ResolveLeafDictionary(i => key[i], false);
                if (dictionary == null) throw new KeyNotFoundException();
                return dictionary[key[key.Depth - 1]];
            }
            set
            {
                var dictionary = ResolveLeafDictionary(i => key[i], true);
                var prevCount = dictionary.Count;
                dictionary[key[key.Depth - 1]] = value;
                if (prevCount != dictionary.Count) _count++;
            }
        }

        /// <summary>Gets or sets the value associated with the specified jagged index.</summary>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="T:System.Collections.Generic.KeyNotFoundException" />, and a set operation creates a new element with the specified key.</returns>
        /// <param name="key">The key of the value to get or set.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is null.</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> does not exist in the collection.</exception>
        public TValue this[params TKey[] keys]
        {
            get
            {
                return this[(IList<TKey>)keys];
            }
            set
            {
                this[(IList<TKey>)keys] = value;
            }
        }

        /// <summary>Gets or sets the value associated with the specified jagged index.</summary>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="T:System.Collections.Generic.KeyNotFoundException" />, and a set operation creates a new element with the specified key.</returns>
        /// <param name="key">The key of the value to get or set.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="key" /> is null.</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="key" /> does not exist in the collection.</exception>
        public TValue this[IList<TKey> keys, int depth = -1]
        {
            get
            {
                if (depth == -1) depth = keys.Count;
                var dictionary = ResolveLeafDictionary(i => keys[i], false);
                if (dictionary == null) throw new KeyNotFoundException();
                return dictionary[keys[keys.Count - 1]];
            }
            set
            {
                if (depth == -1) depth = keys.Count;
                var dictionary = ResolveLeafDictionary(i => keys[i], true);
                var prevCount = dictionary.Count;
                dictionary[keys[keys.Count - 1]] = value;
                if (prevCount != dictionary.Count) _count++;
            }
        }

        private object _syncRoot;
        object SyncRoot
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

        /// <summary>Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</summary>
        public bool IsReadOnly => false;

        /// <summary>Initializes a new instance of the <see cref="T:Blondin.JaggedDictionary`2" />
        /// class that is empty, has the default initial capacity, and uses the default equality
        /// comparer for the key type.</summary>
        public JaggedDictionary(int depth, IDictionaryFactory<TKey> dictionaryFactory = null)
        {
            if (depth < 1) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.capacity);

            this._dictionaryFactory = dictionaryFactory ?? DictionaryFactory<TKey>.Default;
            this.Depth = depth;
            this._root = CreateInternalDictionary(0);
        }

        private IDictionary CreateInternalDictionary(int dimension)
        {
            var result = dimension == Depth - 1 ?
                (IDictionary)_dictionaryFactory.Create<TValue>() :
                (IDictionary)_dictionaryFactory.Create<IDictionary>();
            if (result == null) ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_NoValue);
            return result;
        }

        private IDictionary<TKey, TValue> ResolveLeafDictionary(Func<int, TKey> keyAccessor, bool createIfNeeded)
        {
            System.Collections.IDictionary result = _root;
            for (int i = 0; i < Depth - 1; i++)
            {
                var parent = result;
                var key = keyAccessor(i);
                result = (System.Collections.IDictionary)parent[key];
                if (result == null)
                {
                    if (!createIfNeeded) return null;
                    parent[key] = result = CreateInternalDictionary(i + 1);
                }
            }
            return (IDictionary<TKey, TValue>)result;
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<IJaggedIndex<TKey>, TValue>> GetEnumerator()
        {
            return new Enumerator<KeyValuePair<IJaggedIndex<TKey>, TValue>>(this,
                enums => new KeyValuePair<IJaggedIndex<TKey>, TValue>(
                        JaggedIndex.Create<TKey>(enums.Select(e => (TKey)e.Key).ToArray()),
                        (TValue)enums.Last().Value));
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>Determines whether the <see cref="T:Blondin.JaggedDictionary`2" /> contains a specific value.</summary>
        /// <returns>true if the <see cref="T:Blondin.JaggedDictionary`2" /> contains an element with the specified value; otherwise, false.</returns>
        /// <param name="value">The value to locate in the <see cref="T:System.Collections.Generic.Dictionary`2" />. The value can be null for reference types.</param>
        public bool ContainsValue(TValue value)
        {
            var comparer = EqualityComparer<TValue>.Default;
            using (var enumerator = new Enumerator<TValue>(this, ValueExtractor))
            {
                while (enumerator.MoveNext())
                {
                    if ((value == null && enumerator.Current == null) ||
                        comparer.Equals(enumerator.Current, value)) return true;
                }
            }
            return false;
        }

        /// <summary>Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains a specific key.</summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.</returns>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
        public bool ContainsKey(IJaggedIndex<TKey> key)
        {
            var dictionary = ResolveLeafDictionary(i => key[i], false);
            if (dictionary == null) return false;
            return dictionary.ContainsKey(key[key.Depth - 1]);
        }

        /// <summary>Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        public void Add(IJaggedIndex<TKey> key, TValue value)
        {
            var dictionary = ResolveLeafDictionary(i => key[i], true);
            dictionary.Add(key[key.Depth - 1], value);
            _count++;
        }

        /// <summary>Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.</returns>
        public bool Remove(IJaggedIndex<TKey> key)
        {
            var dictionary = ResolveLeafDictionary(i => key[i], false);
            if (dictionary == null || !dictionary.Remove(key[key.Depth - 1])) return false;
            _count--;
            return true;
        }

        /// <summary>Gets the value associated with the specified key.</summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(IJaggedIndex<TKey> key, out TValue value)
        {
            var dictionary = ResolveLeafDictionary(i => key[i], false);
            if (dictionary != null) return dictionary.TryGetValue(key[key.Depth - 1], out value);
            value = default(TValue);
            return false;
        }

        /// <summary>Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        public void Clear()
        {
            _root.Clear();
            _count = 0;
        }

        /// <summary>Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public void Add(KeyValuePair<IJaggedIndex<TKey>, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
        public bool Contains(KeyValuePair<IJaggedIndex<TKey>, TValue> item)
        {
            var dictionary = ResolveLeafDictionary(i => item.Key[i], false);
            if (dictionary == null) return false;
            return dictionary.Contains(new KeyValuePair<TKey, TValue>(item.Key[item.Key.Depth - 1], item.Value));
        }

        /// <summary>Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public bool Remove(KeyValuePair<IJaggedIndex<TKey>, TValue> item)
        {
            return Remove(item.Key);
        }

        /// <summary>Copies to.</summary>
        /// <param name="array">The array.</param>
        /// <param name="index">The index.</param>
        public void CopyTo(KeyValuePair<IJaggedIndex<TKey>, TValue>[] array, int index)
        {
            if (array == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            if (index < 0 || index > array.Length) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            if (array.Length - index < Count) ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
            int i = 0;
            foreach (var kvp in this)
            {
                array[index + (i++)] = kvp;
            }
        }
    }
}
