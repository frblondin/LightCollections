// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Blondin.LightCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Generic.LightDictionary
{
    public class LightDictionaryConcurrentAccessDetectionTests
    {
        private async Task LightDictionaryConcurrentAccessDetection<TKey, TValue>(LightDictionary<TKey, TValue> dictionary, bool isValueType, object comparer, Action<LightDictionary<TKey, TValue>> add, Action<LightDictionary<TKey, TValue>> get, Action<LightDictionary<TKey, TValue>> remove)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                // Get the Dictionary into a corrupted state, as if it had been corrupted by concurrent access.
                // We this deterministically by clearing the _entries array using reflection;
                // this means that every Entry struct has a 'next' field of zero, which causes the infinite loop
                // that we want Dictionary to break out of
                FieldInfo entriesType = dictionary.GetType().GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);
                Array entriesInstance = (Array)entriesType.GetValue(dictionary);
                Array entryArray = (Array)Activator.CreateInstance(entriesInstance.GetType(), new object[] { ((IDictionary)dictionary).Count });
                entriesType.SetValue(dictionary, entryArray);

                Assert.Equal(comparer, dictionary.GetType().GetField("_comparer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dictionary));
                Assert.Equal(isValueType, dictionary.GetType().GetGenericArguments()[0].IsValueType);
                Assert.Equal("ThrowInvalidOperationException_ConcurrentOperationsNotSupported", Assert.Throws<InvalidOperationException>(() => add(dictionary)).TargetSite.Name);
                Assert.Equal("ThrowInvalidOperationException_ConcurrentOperationsNotSupported", Assert.Throws<InvalidOperationException>(() => get(dictionary)).TargetSite.Name);
                Assert.Equal("ThrowInvalidOperationException_ConcurrentOperationsNotSupported", Assert.Throws<InvalidOperationException>(() => remove(dictionary)).TargetSite.Name);
            }, TaskCreationOptions.LongRunning);

            // If Dictionary regresses, we do not want to hang here indefinitely
            Assert.True((await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(60))) == task) && task.IsCompleted);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(typeof(CustomEqualityComparerInt32ValueType))]
        public async Task LightDictionaryConcurrentAccessDetection_ValueTypeKey(Type comparerType)
        {
            IEqualityComparer<int> customComparer = null;

            LightDictionary<int, int> dic = comparerType == null ?
                new LightDictionary<int, int>() :
                new LightDictionary<int, int>((customComparer = (IEqualityComparer<int>)Activator.CreateInstance(comparerType)));

            dic.Add(1, 1);

            await LightDictionaryConcurrentAccessDetection(dic,
                typeof(int).IsValueType,
                customComparer,
                d => d.Add(1, 1),
                d => { var v = d[1]; },
                d => d.Remove(1));
        }

        [Theory]
        [InlineData(null)]
        [InlineData(typeof(CustomEqualityComparerDummyRefType))]
        public async Task LightDictionaryConcurrentAccessDetection_ReferenceTypeKey(Type comparerType)
        {
            IEqualityComparer<DummyRefType> customComparer = null;

            LightDictionary<DummyRefType, DummyRefType> dic = comparerType == null ?
                new LightDictionary<DummyRefType, DummyRefType>() :
                new LightDictionary<DummyRefType, DummyRefType>((customComparer = (IEqualityComparer<DummyRefType>)Activator.CreateInstance(comparerType)));

            var keyValueSample = new DummyRefType() { Value = 1 };

            dic.Add(keyValueSample, keyValueSample);

            await LightDictionaryConcurrentAccessDetection(dic,
                typeof(DummyRefType).IsValueType,
                customComparer,
                d => d.Add(keyValueSample, keyValueSample),
                d => { var v = d[keyValueSample]; },
                d => d.Remove(keyValueSample));
        }
    }

    // We use a custom type instead of string because string use optimized comparer https://github.com/dotnet/coreclr/blob/master/src/System.Private.CoreLib/shared/System/Collections/Generic/Dictionary.cs#L79
    // We want to test case with _comparer = null
    class DummyRefType
    {
        public int Value { get; set; }
        public override bool Equals(object obj)
        {
            return ((DummyRefType)obj).Equals(this.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    class CustomEqualityComparerDummyRefType : EqualityComparer<DummyRefType>
    {
        public override bool Equals(DummyRefType x, DummyRefType y)
        {
            return x.Value == y.Value;
        }

        public override int GetHashCode(DummyRefType obj)
        {
            return obj.GetHashCode();
        }
    }

    class CustomEqualityComparerInt32ValueType : EqualityComparer<int>
    {
        public override bool Equals(int x, int y)
        {
            return EqualityComparer<int>.Default.Equals(x, y);
        }

        public override int GetHashCode(int obj)
        {
            return EqualityComparer<int>.Default.GetHashCode(obj);
        }
    }
}
