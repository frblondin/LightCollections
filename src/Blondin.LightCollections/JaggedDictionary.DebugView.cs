using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blondin.LightCollections
{
    internal sealed class JaggedDictionaryDebugView<TKey, TValue>
    {
        private JaggedDictionary<TKey, TValue> dict;

        public JaggedDictionaryDebugView(JaggedDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);

            this.dict = dictionary;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<IJaggedIndex<TKey>, TValue>[] Items
        {
            get
            {
                return dict.ToArray();
            }
        }
    }

    internal sealed class JaggedDictionaryKeyCollectionDebugView<TKey, TValue>
    {
        private ICollection<IJaggedIndex<TKey>> _collection;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IJaggedIndex<TKey>[] Items
        {
            get
            {
                return _collection.ToArray();
            }
        }

        public JaggedDictionaryKeyCollectionDebugView(ICollection<IJaggedIndex<TKey>> collection)
        {
            if (collection == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);

            this._collection = collection;
        }
    }

    internal sealed class JaggedDictionaryValueCollectionDebugView<TKey, TValue>
    {
        private ICollection<TValue> _collection;

        public JaggedDictionaryValueCollectionDebugView(ICollection<TValue> collection)
        {
            if (collection == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);

            this._collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TValue[] Items
        {
            get
            {
                return _collection.ToArray();
            }
        }
    }
}
