// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Blondin.LightCollections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace System.Collections.Tests
{
    /// <summary>
    /// Contains tests that ensure the correctness of the Dictionary class.
    /// </summary>
    public abstract partial class LightDictionary_Generic_Tests<TKey, TValue> : IDictionary_Generic_Tests<TKey, TValue>
    {
        #region Remove(TKey)

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void LightDictionary_Generic_RemoveKey_ValidKeyNotContainedInDictionary(int count)
        {
            LightDictionary<TKey, TValue> dictionary = (LightDictionary<TKey, TValue>)GenericIDictionaryFactory(count);
            TKey missingKey = GetNewKey(dictionary);
            Assert.False(dictionary.Remove(missingKey));
            Assert.Equal(count, dictionary.Count);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void LightDictionary_Generic_RemoveKey_ValidKeyContainedInDictionary(int count)
        {
            LightDictionary<TKey, TValue> dictionary = (LightDictionary<TKey, TValue>)GenericIDictionaryFactory(count);
            TKey missingKey = GetNewKey(dictionary);
            TValue inValue = CreateTValue(count);
            dictionary.Add(missingKey, inValue);
            Assert.True(dictionary.Remove(missingKey));
            Assert.Equal(count, dictionary.Count);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void LightDictionary_Generic_RemoveKey_DefaultKeyNotContainedInDictionary(int count)
        {
            LightDictionary<TKey, TValue> dictionary = (LightDictionary<TKey, TValue>)GenericIDictionaryFactory(count);
            if (DefaultValueAllowed)
            {
                TKey missingKey = default(TKey);
                while (dictionary.ContainsKey(missingKey))
                dictionary.Remove(missingKey);
                Assert.False(dictionary.Remove(missingKey));
            }
                    }

        #endregion
    }
}
