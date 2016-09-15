using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blondin.LightCollections
{
    public partial class JaggedDictionary<TKey, TValue>
    {
        internal abstract class AbstractCollection<T> : ICollection<T>, ICollection, IReadOnlyCollection<T>
        {
            internal readonly JaggedDictionary<TKey, TValue> _dictionary;
            internal readonly Enumerator<T>.Extractor _extractor;

            public int Depth
            {
                get { return _dictionary.Depth; }
            }

            public int Count
            {
                get { return _dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            bool ICollection.IsSynchronized
            {
                get { return false; }
            }

            object ICollection.SyncRoot
            {
                get { return _dictionary.SyncRoot; }
            }

            internal AbstractCollection(JaggedDictionary<TKey, TValue> dictionary, Enumerator<T>.Extractor extractor)
            {
                this._dictionary = dictionary;
                this._extractor = extractor;
            }

            void ICollection<T>.Add(T item)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
            }

            void ICollection<T>.Clear()
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
            }

            public bool Remove(T item)
            {
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
                return false;
            }

            public abstract bool Contains(T item);

            public void CopyTo(T[] array, int arrayIndex)
            {
                var index = arrayIndex;
                foreach (var value in (IEnumerable<T>)this)
                    array[arrayIndex++] = value;
            }

            void ICollection.CopyTo(Array array, int index)
            {
                CopyTo((T[])array, index);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new Enumerator<T>(_dictionary, _extractor);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        /// <summary>Represents the collection of keys in a <see cref="T:Blondin.JaggedDictionary`2" />.
        /// This class cannot be inherited.</summary>
        [DebuggerDisplay("Depth = {Depth}, Count = {Count}")]
        [DebuggerTypeProxy(typeof(JaggedDictionaryKeyCollectionDebugView<,>))]
        internal sealed class KeyCollection : AbstractCollection<IJaggedIndex<TKey>>
        {
            internal KeyCollection(JaggedDictionary<TKey, TValue> dictionary)
                : base(dictionary, enums => JaggedIndex.Create<TKey>(enums.Select(e => (TKey)e.Key).ToArray()))
            {
            }

            public override bool Contains(IJaggedIndex<TKey> keys)
            {
                var dictionary = _dictionary.ResolveLeafDictionary(i => keys[i], false);
                if (dictionary == null) throw new KeyNotFoundException();
                return dictionary.ContainsKey(keys[keys.Depth - 1]);
            }
        }

        /// <summary>Represents the collection of values in a <see cref="T:Blondin.JaggedDictionary`2" />.
        /// This class cannot be inherited. </summary>
        [DebuggerDisplay("Depth = {Depth}, Count = {Count}")]
        [DebuggerTypeProxy(typeof(JaggedDictionaryValueCollectionDebugView<,>))]
        internal sealed class ValueCollection : AbstractCollection<TValue>
        {
            internal ValueCollection(JaggedDictionary<TKey, TValue> dictionary)
                : base(dictionary, enums => (TValue)enums.Last().Value)
            {
            }

            public override bool Contains(TValue item)
            {
                return _dictionary.ContainsValue(item);
            }
        }

        private KeyCollection _keys;
        /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
        public ICollection<IJaggedIndex<TKey>> Keys
        {
            get
            {
                if (_keys == null) _keys = new KeyCollection(this);
                return _keys;
            }
        }

        private ValueCollection _values;
        /// <summary>Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</summary>
        public ICollection<TValue> Values
        {
            get
            {
                if (_values == null) _values = new ValueCollection(this);
                return _values;
            }
        }
    }
}
