using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Blondin.LightCollections
{
    internal enum ExceptionArgument
    {
        obj,
        dictionary,
        array,
        info,
        key,
        collection,
        list,
        match,
        converter,
        queue,
        stack,
        capacity,
        index,
        startIndex,
        value,
        count,
        arrayIndex,
        name,
        mode,
        item,
        options,
        view
    }

    internal enum ExceptionResource
    {
        ArgumentOutOfRange_NeedNonNegNum,
        Arg_ArrayPlusOffTooSmall,
        Arg_NonZeroLowerBound,
        Arg_RankMultiDimNotSupported,
        Argument_AddingDuplicate,
        Argument_InvalidArrayType,
        NotSupported_KeyCollectionSet,
        NotSupported_ValueCollectionSet,
        ArgumentOutOfRange_SmallCapacity,
        ArgumentOutOfRange_Index,
        InvalidOperation_EnumFailedVersion,
        InvalidOperation_EnumOpCantHappen,
        InvalidOperation_NoValue,
        Serialization_MissingKeys,
        Serialization_NullKey
    }

    internal static class ThrowHelper
    {
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentNullException(ExceptionArgument argument)
        {
            throw new ArgumentNullException(GetArgumentName(argument));
        }

        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument)
        {
            throw new ArgumentOutOfRangeException(GetArgumentName(argument));
        }

        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument, ExceptionResource resource)
        {
            throw new ArgumentOutOfRangeException(GetArgumentName(argument), GetResourceString(resource));
        }

        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException(ExceptionResource resource)
        {
            throw new ArgumentException(GetResourceString(resource));
        }

        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException(ExceptionResource resource)
        {
            throw new InvalidOperationException(GetResourceString(resource));
        }

        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNotSupportedException(ExceptionResource resource)
        {
            throw new NotSupportedException(GetResourceString(resource));
        }

        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowSerializationException(ExceptionResource resource)
        {
            throw new SerializationException(GetResourceString(resource));
        }

        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowKeyNotFoundException()
        {
            throw new System.Collections.Generic.KeyNotFoundException();
        }

        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowWrongKeyTypeArgumentException<T>(T key, Type targetType)
        {
            throw new ArgumentException($"Wrong type for key. Expected {targetType}, got {typeof(T)}", nameof(key));
        }

        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowWrongValueTypeArgumentException<T>(T value, Type targetType)
        {
            throw new ArgumentException($"Wrong type for value. Expected {targetType}, got {typeof(T)}", nameof(value));
        }

        internal static void IfNullAndNullsAreIllegalThenThrow<T>(object value, ExceptionArgument argName)
        {
            if (value == null && default(T) != null)
            {
                ThrowArgumentNullException(argName);
            }
        }

        private static string GetArgumentName(ExceptionArgument argument)
        {
            switch (argument)
            {
                case ExceptionArgument.obj:
                    return "obj";
                case ExceptionArgument.dictionary:
                    return "dictionary";
                case ExceptionArgument.array:
                    return "array";
                case ExceptionArgument.info:
                    return "info";
                case ExceptionArgument.key:
                    return "key";
                case ExceptionArgument.collection:
                    return "collection";
                case ExceptionArgument.list:
                    return "list";
                case ExceptionArgument.match:
                    return "match";
                case ExceptionArgument.converter:
                    return "converter";
                case ExceptionArgument.queue:
                    return "queue";
                case ExceptionArgument.stack:
                    return "stack";
                case ExceptionArgument.capacity:
                    return "capacity";
                case ExceptionArgument.index:
                    return "index";
                case ExceptionArgument.startIndex:
                    return "startIndex";
                case ExceptionArgument.value:
                    return "value";
                case ExceptionArgument.count:
                    return "count";
                case ExceptionArgument.arrayIndex:
                    return "arrayIndex";
                case ExceptionArgument.name:
                    return "name";
                case ExceptionArgument.mode:
                    return "mode";
                case ExceptionArgument.item:
                    return "item";
                case ExceptionArgument.options:
                    return "options";
                case ExceptionArgument.view:
                    return "view";
                default:
                    return argument.ToString();
            }
        }

        private static string GetResourceString(ExceptionResource resource)
        {
            switch (resource)
            {
                case ExceptionResource.ArgumentOutOfRange_NeedNonNegNum:
                    return "Non-negative number required.";
                case ExceptionResource.Arg_ArrayPlusOffTooSmall:
                    return "Destination array is not long enough to copy all the items in the collection. Check array index and length.";
                case ExceptionResource.Arg_NonZeroLowerBound:
                    return "The lower bound of target array must be zero.";
                case ExceptionResource.Arg_RankMultiDimNotSupported:
                    return "Only single dimensional arrays are supported for the requested action.";
                case ExceptionResource.Argument_AddingDuplicate:
                    return "An item with the same key has already been added.";
                case ExceptionResource.Argument_InvalidArrayType:
                    return "Target array type is not compatible with the type of items in the collection.";
                case ExceptionResource.NotSupported_KeyCollectionSet:
                    return "Mutating a key collection derived from a dictionary is not allowed.";
                case ExceptionResource.NotSupported_ValueCollectionSet:
                    return "Mutating a value collection derived from a dictionary is not allowed.";
                case ExceptionResource.ArgumentOutOfRange_SmallCapacity:
                    return "capacity was less than the current size.";
                case ExceptionResource.ArgumentOutOfRange_Index:
                    return "Index was out of range. Must be non-negative and less than the size of the collection.";
                case ExceptionResource.InvalidOperation_EnumFailedVersion:
                    return "Collection was modified; enumeration operation may not execute.";
                case ExceptionResource.InvalidOperation_EnumOpCantHappen:
                    return "Enumeration has either not started or has already finished.";
                case ExceptionResource.InvalidOperation_NoValue:
                    return "Nullable object must have a value.";
                case ExceptionResource.Serialization_MissingKeys:
                    return "The Keys for this Hashtable are missing.";
                case ExceptionResource.Serialization_NullKey:
                    return "One of the serialized keys is null.";
                default:
                    return resource.ToString();
            }
        }
    }
}