// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Text;
using Xunit;

namespace System.Collections.Tests
{
    /// <summary>
    /// Contains tests that ensure the correctness of any class that implements the nongeneric
    /// ICollection interface
    /// </summary>
    public abstract class ICollection_NonGeneric_Tests : IEnumerable_NonGeneric_Tests
    {
        #region Helper methods

        /// <summary>
        /// Creates an instance of an ICollection that can be used for testing.
        /// </summary>
        /// <returns>An instance of an ICollection that can be used for testing.</returns>
        protected abstract ICollection NonGenericICollectionFactory();

        /// <summary>
        /// Creates an instance of an ICollection that can be used for testing.
        /// </summary>
        /// <param name="count">The number of unique items that the returned ICollection contains.</param>
        /// <returns>An instance of an ICollection that can be used for testing.</returns>
        protected virtual ICollection NonGenericICollectionFactory(int count)
        {
            ICollection collection = NonGenericICollectionFactory();
            AddToCollection(collection, count);
            return collection;
        }

        protected virtual bool DuplicateValuesAllowed => true;
        protected virtual bool IsReadOnly => false;
        protected virtual bool NullAllowed => true;
        protected virtual bool ExpectedIsSynchronized => false;
        protected virtual IEnumerable<object> InvalidValues => new object[0];

        protected abstract void AddToCollection(ICollection collection, int numberOfItemsToAdd);

        /// <summary>
        /// Used for the ICollection_NonGeneric_CopyTo_ArrayOfEnumType test where we try to call CopyTo
        /// on an Array of Enum values. Some implementations special-case for this and throw an ArgumentException,
        /// while others just throw an InvalidCastExcepton.
        /// </summary>
        protected virtual Type ICollection_NonGeneric_CopyTo_ArrayOfEnumType_ThrowType => typeof(InvalidCastException);

        /// <summary>
        /// Used for the ICollection_NonGeneric_CopyTo_ArrayOfIncorrectReferenceType test where we try to call CopyTo
        /// on an Array of different reference values. Some implementations special-case for this and throw an ArgumentException,
        /// while others just throw an InvalidCastExcepton or an ArrayTypeMismatchException.
        /// </summary>
        protected virtual Type ICollection_NonGeneric_CopyTo_ArrayOfIncorrectReferenceType_ThrowType => typeof(ArgumentException);

        /// <summary>
        /// Used for the ICollection_NonGeneric_CopyTo_ArrayOfIncorrectValueType test where we try to call CopyTo
        /// on an Array of different value values. Some implementations special-case for this and throw an ArgumentException,
        /// while others just throw an InvalidCastExcepton.
        /// </summary>
        protected virtual Type ICollection_NonGeneric_CopyTo_ArrayOfIncorrectValueType_ThrowType => typeof(ArgumentException);

        /// <summary>
        /// Used for the ICollection_NonGeneric_CopyTo_NonZeroLowerBound test where we try to call CopyTo
        /// on an Array of with a non-zero lower bound.
        /// Most implementations throw an ArgumentException, but others (e.g. SortedList) throw
        /// an ArgumentOutOfRangeException.
        /// </summary>
        protected virtual Type ICollection_NonGeneric_CopyTo_NonZeroLowerBound_ThrowType => typeof(ArgumentException);

        /// <summary>
        /// Used for ICollection_NonGeneric_SyncRoot tests. Some implementations (e.g. ConcurrentDictionary)
        /// don't support the SyncRoot property of an ICollection and throw a NotSupportedException.
        /// </summary>
        protected virtual bool ICollection_NonGeneric_SupportsSyncRoot => true;

        /// <summary>
        /// Used for ICollection_NonGeneric_SyncRoot tests. Some implementations (e.g. TempFileCollection)
        /// return null for the SyncRoot property of an ICollection.
        /// </summary>
        protected virtual bool ICollection_NonGeneric_HasNullSyncRoot => false;

        /// <summary>
        /// Used for the ICollection_NonGeneric_SyncRootType_MatchesExcepted test. Most SyncRoots are created
        /// using System.Threading.Interlocked.CompareExchange(ref _syncRoot, new Object(), null)
        /// so we should test that the SyncRoot is the type we expect.
        /// </summary>
        protected virtual Type ICollection_NonGeneric_SyncRootType => typeof(object);

        /// <summary>
        /// Used for the ICollection_NonGeneric_CopyTo_IndexLargerThanArrayCount_ThrowsArgumentException tests. Some
        /// implementations throw a different exception type (e.g. ArgumentOutOfRangeException).
        /// </summary>
        protected virtual Type ICollection_NonGeneric_CopyTo_IndexLargerThanArrayCount_ThrowType => typeof(ArgumentException);

        /// <summary>
        /// Used for the ICollection_NonGeneric_CopyTo_TwoDimensionArray_ThrowsException test. Some implementations
        /// throw a different exception type (e.g. RankException by ImmutableArray)
        /// </summary>
        protected virtual Type ICollection_NonGeneric_CopyTo_TwoDimensionArray_ThrowType => typeof(ArgumentException);

        #endregion

        #region IEnumerable Helper Methods

        protected override IEnumerable<ModifyEnumerable> ModifyEnumerables => new List<ModifyEnumerable>();

        protected override IEnumerable NonGenericIEnumerableFactory(int count) => NonGenericICollectionFactory(count);

        #endregion

        #region Count

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_NonGeneric_Count_Validity(int count)
        {
            ICollection collection = NonGenericICollectionFactory(count);
            Assert.Equal(count, collection.Count);
        }

        #endregion

        #region IsSynchronized

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_NonGeneric_IsSynchronized(int count)
        {
            ICollection collection = NonGenericICollectionFactory(count);
            Assert.Equal(ExpectedIsSynchronized, collection.IsSynchronized);
        }

        #endregion

        #region SyncRoot

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_NonGeneric_SyncRoot(int count)
        {
            ICollection collection = NonGenericICollectionFactory(count);
            if (ICollection_NonGeneric_SupportsSyncRoot)
            {
                Assert.Equal(ICollection_NonGeneric_HasNullSyncRoot, collection.SyncRoot == null);
                Assert.Same(collection.SyncRoot, collection.SyncRoot);

                if (!ICollection_NonGeneric_HasNullSyncRoot)
                {
                    Assert.IsType(ICollection_NonGeneric_SyncRootType, collection.SyncRoot);

                    if (ICollection_NonGeneric_SyncRootType == collection.GetType())
                    {
                        // If we expect the SyncRoot to be the same type as the collection, 
                        // the SyncRoot should be the same as the collection (e.g. HybridDictionary)
                        Assert.Same(collection, collection.SyncRoot);
                    }
                    else
                    {
                        Assert.NotSame(collection, collection.SyncRoot);
                    }
                }
            }
            else
            {
                Assert.Throws<NotSupportedException>(() => collection.SyncRoot);
            }
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_NonGeneric_SyncRootUnique(int count)
        {
            if (ICollection_NonGeneric_SupportsSyncRoot && !ICollection_NonGeneric_HasNullSyncRoot)
            {
                ICollection collection1 = NonGenericICollectionFactory(count);
                ICollection collection2 = NonGenericICollectionFactory(count);
                Assert.NotSame(collection1.SyncRoot, collection2.SyncRoot);
            }
        }

        #endregion

        #region CopyTo

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_NonGeneric_CopyTo_ExactlyEnoughSpaceInArray(int count)
        {
            ICollection collection = NonGenericICollectionFactory(count);
            object[] array = new object[count];
            collection.CopyTo(array, 0);
            int i = 0;
            foreach (object obj in collection)
                Assert.Equal(array[i++], obj);
        }

        [Theory]
        [MemberData(nameof(ValidCollectionSizes))]
        public void ICollection_NonGeneric_CopyTo_ArrayIsLargerThanCollection(int count)
        {
            ICollection collection = NonGenericICollectionFactory(count);
            object[] array = new object[count * 3 / 2];
            collection.CopyTo(array, 0);
            int i = 0;
            foreach (object obj in collection)
                Assert.Equal(array[i++], obj);
        }

        #endregion
    }
}
