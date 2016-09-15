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
    public partial class JaggedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<IJaggedIndex<TKey>, TValue>>
    {
        #region Enumerator
        internal struct Enumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
        {
            internal delegate T Extractor(IDictionaryEnumerator[] enums);

            private readonly JaggedDictionary<TKey, TValue> _dictionary;
            private readonly IDictionaryEnumerator[] _enumerators;
            private readonly Extractor _extractor;

            public Enumerator(JaggedDictionary<TKey, TValue> dictionary, Extractor extractor)
            {
                this._dictionary = dictionary;
                this._enumerators = new IDictionaryEnumerator[dictionary.Depth];
                this._extractor = extractor;
            }

            public T Current
            {
                get
                {
                    if (_enumerators[_dictionary.Depth - 1] == null)
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
                int i = 0;
                while (i >= 0 && i < _dictionary.Depth)
                {
                    if (_enumerators[i] == null)
                    {
                        if (i == 0)
                        {
                            _enumerators[i] = _dictionary._root.GetEnumerator();
                        }
                        else
                        {
                            if (!_enumerators[i - 1].MoveNext())
                            {
                                if (i == 1) return false;
                                _enumerators[i] = null;
                                _enumerators[i - 1] = null;
                                i--;
                                continue;
                            }
                            var upperDictionary = (IDictionary)_enumerators[i - 1].Value;
                            _enumerators[i] = upperDictionary.GetEnumerator();
                        }
                    }
                    if (_dictionary.Depth > 1 && i == _dictionary.Depth - 1)
                    {
                        if (!_enumerators[i].MoveNext())
                        {
                            if (i == 1) return false;
                            _enumerators[i] = null;
                            _enumerators[i - 1] = null;
                            i--;
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
                for (int i = 0; i < _dictionary.Depth; i++) _enumerators[i] = null;
            }
        }
        #endregion

        private static readonly Enumerator<TValue>.Extractor ValueExtractor = (enums => (TValue)enums.Last().Value);
        private static readonly Enumerator<IJaggedIndex<TKey>>.Extractor KeyExtractor = (enums => JaggedIndex.Create<TKey>(enums.Select(e => (TKey)e.Key).ToArray()));

        private readonly IDictionaryFactory<TKey> _dictionaryFactory;
        private readonly System.Collections.IDictionary _root;
        public int Depth { get; private set; }
        private int _count;

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

        /// <summary>Initializes a new instance of the <see cref="T:Blondin.JaggedDictionary`2" />
        /// class that is empty, has the default initial capacity, and uses the default equality
        /// comparer for the key type.</summary>
        public JaggedDictionary(int depth, IDictionaryFactory<TKey> dictionaryFactory = null)
        {
            if (depth < 1) ThrowHelper.ThrowArgumentOutOfRangeException1(ExceptionArgument.capacity);

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
    }
}
