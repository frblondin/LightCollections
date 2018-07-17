// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Blondin.LightCollections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xunit;

namespace System.Collections.Tests
{
    public class LightDictionary_IDictionary_NonGeneric_Tests : IDictionary_NonGeneric_Tests
    {
        protected override IDictionary NonGenericIDictionaryFactory()
        {
            return new LightDictionary<string, string>();
        }

        /// <summary>
        /// Creates an object that is dependent on the seed given. The object may be either
        /// a value type or a reference type, chosen based on the value of the seed.
        /// </summary>
        protected override object CreateTKey(int seed)
        {
            int stringLength = seed % 10 + 5;
            Random rand = new Random(seed);
            byte[] bytes = new byte[stringLength];
            rand.NextBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Creates an object that is dependent on the seed given. The object may be either
        /// a value type or a reference type, chosen based on the value of the seed.
        /// </summary>
        protected override object CreateTValue(int seed) => CreateTKey(seed);

        protected override Type ICollection_NonGeneric_CopyTo_IndexLargerThanArrayCount_ThrowType => typeof(ArgumentOutOfRangeException);

        #region IDictionary tests

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IDictionary_NonGeneric_ItemSet_NullValueWhenDefaultValueIsNonNull(int count)
        {
            IDictionary dictionary = new LightDictionary<string, int>();
            Assert.Throws<ArgumentNullException>(() => dictionary[GetNewKey(dictionary)] = null);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IDictionary_NonGeneric_ItemSet_KeyOfWrongType(int count)
        {
            if (!IsReadOnly)
            {
                IDictionary dictionary = new LightDictionary<string, string>();
                Assert.Throws<ArgumentException>("key", () => dictionary[23] = CreateTValue(12345));
                Assert.Empty(dictionary);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IDictionary_NonGeneric_ItemSet_ValueOfWrongType(int count)
        {
            if (!IsReadOnly)
            {
                IDictionary dictionary = new LightDictionary<string, string>();
                object missingKey = GetNewKey(dictionary);
                Assert.Throws<ArgumentException>("value", () => dictionary[missingKey] = 324);
                Assert.Empty(dictionary);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IDictionary_NonGeneric_Add_KeyOfWrongType(int count)
        {
            if (!IsReadOnly)
            {
                IDictionary dictionary = new LightDictionary<string, string>();
                object missingKey = 23;
                Assert.Throws<ArgumentException>("key", () => dictionary.Add(missingKey, CreateTValue(12345)));
                Assert.Empty(dictionary);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IDictionary_NonGeneric_Add_ValueOfWrongType(int count)
        {
            if (!IsReadOnly)
            {
                IDictionary dictionary = new LightDictionary<string, string>();
                object missingKey = GetNewKey(dictionary);
                Assert.Throws<ArgumentException>("value", () => dictionary.Add(missingKey, 324));
                Assert.Empty(dictionary);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IDictionary_NonGeneric_Add_NullValueWhenDefaultTValueIsNonNull(int count)
        {
            if (!IsReadOnly)
            {
                IDictionary dictionary = new LightDictionary<string, int>();
                object missingKey = GetNewKey(dictionary);
                Assert.Throws<ArgumentNullException>(() => dictionary.Add(missingKey, null));
                Assert.Empty(dictionary);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void IDictionary_NonGeneric_Contains_KeyOfWrongType(int count)
        {
            if (!IsReadOnly)
            {
                IDictionary dictionary = new LightDictionary<string, int>();
                Assert.False(dictionary.Contains(1));
            }
        }

        #endregion

        #region ICollection tests

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_NonGeneric_CopyTo_ArrayOfIncorrectKeyValuePairType(int count)
        {
            ICollection collection = NonGenericICollectionFactory(count);
            KeyValuePair<string, int>[] array = new KeyValuePair<string, int>[count * 3 / 2];
            Assert.Throws<ArgumentException>(null, () => collection.CopyTo(array, 0));
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_NonGeneric_CopyTo_ArrayOfCorrectKeyValuePairType(int count)
        {
            ICollection collection = NonGenericICollectionFactory(count);
            KeyValuePair<string, string>[] array = new KeyValuePair<string, string>[count];
            collection.CopyTo(array, 0);
            int i = 0;
            foreach (object obj in collection)
                Assert.Equal(array[i++], obj);
        }

        #endregion
    }

    public class LightDictionary_Tests
    {
        [Fact]
        public void CopyConstructorExceptions()
        {
            Assert.Throws<ArgumentNullException>("dictionary", () => new LightDictionary<int, int>((IDictionary<int, int>)null));
            Assert.Throws<ArgumentNullException>("dictionary", () => new LightDictionary<int, int>((IDictionary<int, int>)null, null));
            Assert.Throws<ArgumentNullException>("dictionary", () => new LightDictionary<int, int>((IDictionary<int, int>)null, EqualityComparer<int>.Default));

            Assert.Throws<ArgumentOutOfRangeException>("capacity", () => new LightDictionary<int, int>(new NegativeCountLightDictionary<int, int>()));
            Assert.Throws<ArgumentOutOfRangeException>("capacity", () => new LightDictionary<int, int>(new NegativeCountLightDictionary<int, int>(), null));
            Assert.Throws<ArgumentOutOfRangeException>("capacity", () => new LightDictionary<int, int>(new NegativeCountLightDictionary<int, int>(), EqualityComparer<int>.Default));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(101)]
        public void ICollection_NonGeneric_CopyTo_NonContiguousDictionary(int count)
        {
            ICollection collection = (ICollection)CreateLightDictionary(count, k => k.ToString());
            KeyValuePair<string, string>[] array = new KeyValuePair<string, string>[count];
            collection.CopyTo(array, 0);
            int i = 0;
            foreach (object obj in collection)
                Assert.Equal(array[i++], obj);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(101)]
        public void ICollection_Generic_CopyTo_NonContiguousDictionary(int count)
        {
            ICollection<KeyValuePair<string, string>> collection = CreateLightDictionary(count, k => k.ToString());
            KeyValuePair<string, string>[] array = new KeyValuePair<string, string>[count];
            collection.CopyTo(array, 0);
            int i = 0;
            foreach (KeyValuePair<string, string> obj in collection)
                Assert.Equal(array[i++], obj);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(101)]
        public void IDictionary_Generic_CopyTo_NonContiguousDictionary(int count)
        {
            IDictionary<string, string> collection = CreateLightDictionary(count, k => k.ToString());
            KeyValuePair<string, string>[] array = new KeyValuePair<string, string>[count];
            collection.CopyTo(array, 0);
            int i = 0;
            foreach (KeyValuePair<string, string> obj in collection)
                Assert.Equal(array[i++], obj);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(101)]
        public void CopyTo_NonContiguousDictionary(int count)
        {
            LightDictionary<string, string> collection = (LightDictionary<string, string>)CreateLightDictionary(count, k => k.ToString());
            string[] array = new string[count];
            collection.Keys.CopyTo(array, 0);
            int i = 0;
            foreach (KeyValuePair<string, string> obj in collection)
                Assert.Equal(array[i++], obj.Key);

            collection.Values.CopyTo(array, 0);
            i = 0;
            foreach (KeyValuePair<string, string> obj in collection)
                Assert.Equal(array[i++], obj.Key);
        }

        [Fact]
        public void Remove_NonExistentEntries_DoesNotPreventEnumeration()
        {
            const string SubKey = "-sub-key";
            var dictionary = new LightDictionary<string, string>();
            dictionary.Add("a", "b");
            dictionary.Add("c", "d");
            foreach (string key in dictionary.Keys)
            {
                if (dictionary.Remove(key + SubKey))
                    break;
            }

            dictionary.Add("c" + SubKey, "d");
            foreach (string key in dictionary.Keys)
            {
                if (dictionary.Remove(key + SubKey))
                    break;
            }
        }

        [Theory]
        [MemberData(nameof(CopyConstructorInt32Data))]
        public void CopyConstructorInt32(int size, Func<int, int> keyValueSelector, Func<IDictionary<int, int>, IDictionary<int, int>> dictionarySelector)
        {
            TestCopyConstructor(size, keyValueSelector, dictionarySelector);
        }

        public static IEnumerable<object[]> CopyConstructorInt32Data
        {
            get { return GetCopyConstructorData(i => i); }
        }

        [Theory]
        [MemberData(nameof(CopyConstructorStringData))]
        public void CopyConstructorString(int size, Func<int, string> keyValueSelector, Func<IDictionary<string, string>, IDictionary<string, string>> dictionarySelector)
        {
            TestCopyConstructor(size, keyValueSelector, dictionarySelector);
        }

        public static IEnumerable<object[]> CopyConstructorStringData
        {
            get { return GetCopyConstructorData(i => i.ToString()); }
        }

        private static void TestCopyConstructor<T>(int size, Func<int, T> keyValueSelector, Func<IDictionary<T, T>, IDictionary<T, T>> dictionarySelector)
        {
            IDictionary<T, T> expected = CreateLightDictionary(size, keyValueSelector);
            IDictionary<T, T> input = dictionarySelector(CreateLightDictionary(size, keyValueSelector));

            Assert.Equal(expected, new LightDictionary<T, T>(input));
        }

        [Theory]
        [MemberData(nameof(CopyConstructorInt32ComparerData))]
        public void CopyConstructorInt32Comparer(int size, Func<int, int> keyValueSelector, Func<IDictionary<int, int>, IDictionary<int, int>> dictionarySelector, IEqualityComparer<int> comparer)
        {
            TestCopyConstructor(size, keyValueSelector, dictionarySelector, comparer);
        }

        public static IEnumerable<object[]> CopyConstructorInt32ComparerData
        {
            get
            {
                var comparers = new IEqualityComparer<int>[]
                {
                    null,
                    EqualityComparer<int>.Default
                };

                return GetCopyConstructorData(i => i, comparers);
            }
        }

        [Theory]
        [MemberData(nameof(CopyConstructorStringComparerData))]
        public void CopyConstructorStringComparer(int size, Func<int, string> keyValueSelector, Func<IDictionary<string, string>, IDictionary<string, string>> dictionarySelector, IEqualityComparer<string> comparer)
        {
            TestCopyConstructor(size, keyValueSelector, dictionarySelector, comparer);
        }

        [Fact]
        public void CantAcceptDuplicateKeysFromSourceDictionary()
        {
            LightDictionary<string, int> source = new LightDictionary<string, int> { { "a", 1 }, { "A", 1 } };
            Assert.Throws<ArgumentException>(null, () => new LightDictionary<string, int>(source, StringComparer.OrdinalIgnoreCase));
        }

        public static IEnumerable<object[]> CopyConstructorStringComparerData
        {
            get
            {
                var comparers = new IEqualityComparer<string>[]
                {
                    null,
                    EqualityComparer<string>.Default,
                    StringComparer.Ordinal,
                    StringComparer.OrdinalIgnoreCase
                };

                return GetCopyConstructorData(i => i.ToString(), comparers);
            }
        }

        private static void TestCopyConstructor<T>(int size, Func<int, T> keyValueSelector, Func<IDictionary<T, T>, IDictionary<T, T>> dictionarySelector, IEqualityComparer<T> comparer)
        {
            IDictionary<T, T> expected = CreateLightDictionary(size, keyValueSelector, comparer);
            IDictionary<T, T> input = dictionarySelector(CreateLightDictionary(size, keyValueSelector, comparer));

            Assert.Equal(expected, new LightDictionary<T, T>(input, comparer));
        }

        private static IEnumerable<object[]> GetCopyConstructorData<T>(Func<int, T> keyValueSelector, IEqualityComparer<T>[] comparers = null)
        {
            var dictionarySelectors = new Func<IDictionary<T, T>, IDictionary<T, T>>[]
            {
                d => d,
                d => new LightDictionarySubclass<T, T>(d),
                d => new ReadOnlyDictionary<T, T>(d)
            };

            var sizes = new int[] { 0, 1, 2, 3 };

            foreach (Func<IDictionary<T, T>, IDictionary<T, T>> dictionarySelector in dictionarySelectors)
            {
                foreach (int size in sizes)
                {
                    if (comparers != null)
                    {
                        foreach (IEqualityComparer<T> comparer in comparers)
                        {
                            yield return new object[] { size, keyValueSelector, dictionarySelector, comparer };
                        }
                    }
                    else
                    {
                        yield return new object[] { size, keyValueSelector, dictionarySelector };
                    }
                }
            }
        }

        private static IDictionary<T, T> CreateLightDictionary<T>(int size, Func<int, T> keyValueSelector, IEqualityComparer<T> comparer = null)
        {
            Dictionary<T, T> dict = Enumerable.Range(0, size + 1).ToDictionary(keyValueSelector, keyValueSelector, comparer);
            // Remove first item to reduce Count to size and alter the contiguity of the dictionary
            dict.Remove(keyValueSelector(0));
            LightDictionary<T, T> lightDict = new LightDictionary<T, T>(dict);
            return lightDict;
        }

        private sealed class LightDictionarySubclass<TKey, TValue> : LightDictionary<TKey, TValue>
        {
            public LightDictionarySubclass(IDictionary<TKey, TValue> dictionary)
            {
                foreach (var pair in dictionary)
                {
                    Add(pair.Key, pair.Value);
                }
            }
        }

        /// <summary>
        /// An incorrectly implemented dictionary that returns -1 from Count.
        /// </summary>
        private sealed class NegativeCountLightDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        {
            public int Count { get { return -1; } }

            public TValue this[TKey key] { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
            public bool IsReadOnly { get { throw new NotImplementedException(); } }
            public ICollection<TKey> Keys { get { throw new NotImplementedException(); } }
            public ICollection<TValue> Values { get { throw new NotImplementedException(); } }
            public void Add(KeyValuePair<TKey, TValue> item) { throw new NotImplementedException(); }
            public void Add(TKey key, TValue value) { throw new NotImplementedException(); }
            public void Clear() { throw new NotImplementedException(); }
            public bool Contains(KeyValuePair<TKey, TValue> item) { throw new NotImplementedException(); }
            public bool ContainsKey(TKey key) { throw new NotImplementedException(); }
            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { throw new NotImplementedException(); }
            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { throw new NotImplementedException(); }
            public bool Remove(KeyValuePair<TKey, TValue> item) { throw new NotImplementedException(); }
            public bool Remove(TKey key) { throw new NotImplementedException(); }
            public bool TryGetValue(TKey key, out TValue value) { throw new NotImplementedException(); }
            IEnumerator IEnumerable.GetEnumerator() { throw new NotImplementedException(); }
        }
    }
}
