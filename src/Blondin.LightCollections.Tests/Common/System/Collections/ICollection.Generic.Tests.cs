// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace System.Collections.Tests
{
    /// <summary>
    /// Contains tests that ensure the correctness of any class that implements the generic
    /// ICollection interface
    /// </summary>
    public abstract class ICollection_Generic_Tests<T> : IEnumerable_Generic_Tests<T>
    {
        #region ICollection<T> Helper Methods

        /// <summary>
        /// Creates an instance of an ICollection{T} that can be used for testing.
        /// </summary>
        /// <returns>An instance of an ICollection{T} that can be used for testing.</returns>
        protected abstract ICollection<T> GenericICollectionFactory();

        /// <summary>
        /// Creates an instance of an ICollection{T} that can be used for testing.
        /// </summary>
        /// <param name="count">The number of unique items that the returned ICollection{T} contains.</param>
        /// <returns>An instance of an ICollection{T} that can be used for testing.</returns>
        protected virtual ICollection<T> GenericICollectionFactory(int count)
        {
            ICollection<T> collection = GenericICollectionFactory();
            AddToCollection(collection, count);
            return collection;
        }

        protected virtual bool DuplicateValuesAllowed => true;
        protected virtual bool DefaultValueWhenNotAllowed_Throws => true;
        protected virtual bool IsReadOnly => false;
        protected virtual bool IsReadOnly_ValidityValue => IsReadOnly;
        protected virtual bool AddRemoveClear_ThrowsNotSupported => false;
        protected virtual bool DefaultValueAllowed => true;
        protected virtual IEnumerable<T> InvalidValues => new T[0];

        protected virtual void AddToCollection(ICollection<T> collection, int numberOfItemsToAdd)
        {
            int seed = 9600;
            IEqualityComparer<T> comparer = GetIEqualityComparer();
            while (collection.Count < numberOfItemsToAdd)
            {
                T toAdd = CreateT(seed++);
                while (collection.Contains(toAdd, comparer) || InvalidValues.Contains(toAdd, comparer))
                    toAdd = CreateT(seed++);
                collection.Add(toAdd);
            }
        }

        protected virtual Type ICollection_Generic_CopyTo_IndexLargerThanArrayCount_ThrowType => typeof(ArgumentException);

        #endregion

        #region IEnumerable<T> Helper Methods

        protected override IEnumerable<T> GenericIEnumerableFactory(int count)
        {
            return GenericICollectionFactory(count);
        }

        /// <summary>
        /// Returns a set of ModifyEnumerable delegates that modify the enumerable passed to them.
        /// </summary>
        protected override IEnumerable<ModifyEnumerable> ModifyEnumerables
        {
            get
            {
                if (!AddRemoveClear_ThrowsNotSupported)
                {
                    yield return (IEnumerable<T> enumerable) =>
                    {
                        var casted = (ICollection<T>)enumerable;
                        casted.Add(CreateT(2344));
                        return true;
                    };
                }
                if (!AddRemoveClear_ThrowsNotSupported)
                {
                    yield return (IEnumerable<T> enumerable) =>
                    {
                        var casted = (ICollection<T>)enumerable;
                        if (casted.Count() > 0)
                        {
                            casted.Remove(casted.ElementAt(0));
                            return true;
                        }
                        return false;
                    };
                }
                if (!AddRemoveClear_ThrowsNotSupported)
                {
                    yield return (IEnumerable<T> enumerable) =>
                    {
                        var casted = (ICollection<T>)enumerable;
                        if (casted.Count() > 0)
                        {
                            casted.Clear();
                            return true;
                        }
                        return false;
                    };
                }
            }
        }
        #endregion
    }
}
