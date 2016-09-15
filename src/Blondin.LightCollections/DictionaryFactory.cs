using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blondin.LightCollections
{
    public interface IDictionaryFactory<TKey>
    {
        IDictionary<TKey, TValue> Create<TValue>();
    }

    public class DictionaryFactory<TKey> : IDictionaryFactory<TKey>
    {
        private static readonly DictionaryFactory<TKey> _default = new DictionaryFactory<TKey>();
        internal static DictionaryFactory<TKey> Default { get { return _default; } }

        private readonly IEqualityComparer<TKey> _comparer;

        public DictionaryFactory(IEqualityComparer<TKey> comparer = null)
        {
            this._comparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        public IDictionary<TKey, TValue> Create<TValue>()
        {
            return new Dictionary<TKey, TValue>(_comparer);
        }
    }

    public class SortedDictionaryFactory<TKey> : IDictionaryFactory<TKey>
    {
        private static readonly SortedDictionaryFactory<TKey> _default = new SortedDictionaryFactory<TKey>();
        internal static SortedDictionaryFactory<TKey> Default { get { return _default; } }

        private readonly IComparer<TKey> _comparer;

        public SortedDictionaryFactory(IComparer<TKey> comparer = null)
        {
            this._comparer = comparer ?? Comparer<TKey>.Default;
        }

        public IDictionary<TKey, TValue> Create<TValue>()
        {
            return new SortedDictionary<TKey, TValue>(_comparer);
        }
    }
}
