using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Blondin.LightCollections
{
	internal enum ExceptionArgument
	{
		obj,
		dictionary,
		dictionaryCreationThreshold,
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
		view,
		sourceBytesToCopy
	}

	internal enum ExceptionResource
	{
		Argument_ImplementIComparable,
		Argument_InvalidType,
		Argument_InvalidArgumentForComparison,
		Argument_InvalidRegistryKeyPermissionCheck,
		ArgumentOutOfRange_NeedNonNegNum,
		Arg_ArrayPlusOffTooSmall,
		Arg_NonZeroLowerBound,
		Arg_RankMultiDimNotSupported,
		Arg_RegKeyDelHive,
		Arg_RegKeyStrLenBug,
		Arg_RegSetStrArrNull,
		Arg_RegSetMismatchedKind,
		Arg_RegSubKeyAbsent,
		Arg_RegSubKeyValueAbsent,
		Argument_AddingDuplicate,
		Serialization_InvalidOnDeser,
		Serialization_MissingKeys,
		Serialization_NullKey,
		Argument_InvalidArrayType,
		NotSupported_KeyCollectionSet,
		NotSupported_ValueCollectionSet,
		ArgumentOutOfRange_SmallCapacity,
		ArgumentOutOfRange_Index,
		Argument_InvalidOffLen,
		Argument_ItemNotExist,
		ArgumentOutOfRange_Count,
		ArgumentOutOfRange_InvalidThreshold,
		ArgumentOutOfRange_ListInsert,
		NotSupported_ReadOnlyCollection,
		InvalidOperation_CannotRemoveFromStackOrQueue,
		InvalidOperation_EmptyQueue,
		InvalidOperation_EnumOpCantHappen,
		InvalidOperation_EnumFailedVersion,
		InvalidOperation_EmptyStack,
		ArgumentOutOfRange_BiggerThanCollection,
		InvalidOperation_EnumNotStarted,
		InvalidOperation_EnumEnded,
		NotSupported_SortedListNestedWrite,
		InvalidOperation_NoValue,
		InvalidOperation_RegRemoveSubKey,
		Security_RegistryPermission,
		UnauthorizedAccess_RegistryNoWrite,
		ObjectDisposed_RegKeyClosed,
		NotSupported_InComparableType,
		Argument_InvalidRegistryOptionsCheck,
		Argument_InvalidRegistryViewCheck
	}

    internal static class ThrowHelper
    {
        internal static readonly Action ThrowArgumentOutOfRangeException0;
        internal static readonly Action<ExceptionArgument> ThrowArgumentOutOfRangeException1;
        internal static readonly Action<ExceptionArgument, ExceptionResource> ThrowArgumentOutOfRangeException2;
        internal static readonly Action<ExceptionArgument> ThrowArgumentNullException;
        internal static readonly Action<object, Type> ThrowWrongValueTypeArgumentException;
        internal static readonly Action<object, Type> ThrowWrongKeyTypeArgumentException;
        internal static readonly Action<ExceptionResource> ThrowArgumentException;
        internal static readonly Action<ExceptionResource> ThrowInvalidOperationException;
        internal static readonly Action<ExceptionResource> ThrowNotSupportedException;
        internal static readonly Action<ExceptionResource> ThrowSerializationException;
        internal static readonly Action ThrowKeyNotFoundException;
        internal static void IfNullAndNullsAreIllegalThenThrow<T>(object value, ExceptionArgument argName)
        {
            if (value == null && default(T) != null)
            {
                ThrowArgumentNullException(argName);
            }
        }

        static ThrowHelper()
        {
            var exceptionArg = Expression.Parameter(typeof(ExceptionArgument));
            var exceptionResource = Expression.Parameter(typeof(ExceptionResource));
            var objectArg = Expression.Parameter(typeof(object));
            var typeArg = Expression.Parameter(typeof(Type));

            var realThrowHelperType = typeof(string).Assembly.GetType("System.ThrowHelper");
            var throwArgumentOutOfRangeExceptionTwoArgsMethod = (from m in realThrowHelperType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                                                                 where m.Name.Equals("ThrowArgumentOutOfRangeException") &&
                                                                 m.GetParameters().Length == 2
                                                                 select m).Single();
            var throwArgumentOutOfRangeExceptionTwoArgsParams = throwArgumentOutOfRangeExceptionTwoArgsMethod.GetParameters();
            var throwArgumentOutOfRangeExceptionTwoArgsLambda = Expression.Lambda<Action<ExceptionArgument, ExceptionResource>>(
                Expression.Call(throwArgumentOutOfRangeExceptionTwoArgsMethod,
                    ConvertEnum(exceptionArg, throwArgumentOutOfRangeExceptionTwoArgsParams[0].ParameterType),
                    ConvertEnum(exceptionArg, throwArgumentOutOfRangeExceptionTwoArgsParams[1].ParameterType)),
                exceptionArg, exceptionResource);
            ThrowArgumentOutOfRangeException2 = throwArgumentOutOfRangeExceptionTwoArgsLambda.Compile();

            var throwArgumentOutOfRangeExceptionOneArgMethod = (from m in realThrowHelperType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                                                                where m.Name.Equals("ThrowArgumentOutOfRangeException") &&
                                                                m.GetParameters().Length == 1
                                                                select m).Single();
            var throwArgumentOutOfRangeExceptionOneArgLambda = Expression.Lambda<Action<ExceptionArgument>>(
                Expression.Call(throwArgumentOutOfRangeExceptionOneArgMethod,
                    ConvertEnum(exceptionArg, throwArgumentOutOfRangeExceptionTwoArgsParams[0].ParameterType)),
                exceptionArg);
            ThrowArgumentOutOfRangeException1 = throwArgumentOutOfRangeExceptionOneArgLambda.Compile();

            var throwArgumentOutOfRangeExceptionNoArgMethod = (from m in realThrowHelperType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                                                                where m.Name.Equals("ThrowArgumentOutOfRangeException") &&
                                                                m.GetParameters().Length == 0
                                                                select m).Single();
            var throwArgumentOutOfRangeExceptionNoArgLambda = Expression.Lambda<Action>(
                Expression.Call(throwArgumentOutOfRangeExceptionNoArgMethod));
            ThrowArgumentOutOfRangeException0 = throwArgumentOutOfRangeExceptionNoArgLambda.Compile();

            var throwArgumentExceptionMethod = (from m in realThrowHelperType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                                                where m.Name.Equals("ThrowArgumentException") &&
                                                                 m.GetParameters().Length == 1
                                                select m).Single();
            var throwArgumentExceptionParams = throwArgumentExceptionMethod.GetParameters();
            var throwArgumentExceptionLambda = Expression.Lambda<Action<ExceptionResource>>(
                Expression.Call(throwArgumentExceptionMethod,
                    ConvertEnum(exceptionResource, throwArgumentExceptionParams[0].ParameterType)),
                exceptionResource);
            ThrowArgumentException = throwArgumentExceptionLambda.Compile();

            var throwInvalidOperationExceptionMethod = (from m in realThrowHelperType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                                                where m.Name.Equals("ThrowInvalidOperationException") &&
                                                                 m.GetParameters().Length == 1
                                                select m).Single();
            var throwInvalidOperationExceptionParams = throwInvalidOperationExceptionMethod.GetParameters();
            var throwInvalidOperationExceptionLambda = Expression.Lambda<Action<ExceptionResource>>(
                Expression.Call(throwInvalidOperationExceptionMethod,
                    ConvertEnum(exceptionResource, throwInvalidOperationExceptionParams[0].ParameterType)),
                exceptionResource);
            ThrowInvalidOperationException = throwInvalidOperationExceptionLambda.Compile();

            var throwArgumentNullExceptionLambda = Expression.Lambda<Action<ExceptionArgument>>(
                Expression.Call(realThrowHelperType.GetMethod("ThrowArgumentNullException", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static),
                    ConvertEnum(exceptionArg, throwArgumentOutOfRangeExceptionTwoArgsParams[0].ParameterType)),
                exceptionArg);
            ThrowArgumentNullException = throwArgumentNullExceptionLambda.Compile();

            var throwWrongValueTypeArgumentExceptionLambda = Expression.Lambda<Action<object, Type>>(
                Expression.Call(realThrowHelperType.GetMethod("ThrowWrongValueTypeArgumentException", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static),
                    objectArg, typeArg),
                objectArg, typeArg);
            ThrowWrongValueTypeArgumentException = throwWrongValueTypeArgumentExceptionLambda.Compile();

            var throwWrongKeyTypeArgumentExceptionLambda = Expression.Lambda<Action<object, Type>>(
                Expression.Call(realThrowHelperType.GetMethod("ThrowWrongKeyTypeArgumentException", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static),
                    objectArg, typeArg),
                objectArg, typeArg);
            ThrowWrongKeyTypeArgumentException = throwWrongKeyTypeArgumentExceptionLambda.Compile();

            var throwNotSupportedExceptionMethod = realThrowHelperType.GetMethod("ThrowNotSupportedException", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var throwNotSupportedExceptionLambda = Expression.Lambda<Action<ExceptionResource>>(
                Expression.Call(throwNotSupportedExceptionMethod,
                    ConvertEnum(exceptionResource, throwNotSupportedExceptionMethod.GetParameters()[0].ParameterType)),
                exceptionResource);
            ThrowNotSupportedException = throwNotSupportedExceptionLambda.Compile();

            var throwSerializationExceptionMethod = realThrowHelperType.GetMethod("ThrowSerializationException", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var throwSerializationExceptionLambda = Expression.Lambda<Action<ExceptionResource>>(
                Expression.Call(throwSerializationExceptionMethod,
                    ConvertEnum(exceptionResource, throwSerializationExceptionMethod.GetParameters()[0].ParameterType)),
                exceptionResource);
            ThrowSerializationException = throwSerializationExceptionLambda.Compile();

            var throwKeyNotFoundExceptionLambda = Expression.Lambda<Action>(
                Expression.Call(realThrowHelperType.GetMethod("ThrowKeyNotFoundException", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)));
            ThrowKeyNotFoundException = throwKeyNotFoundExceptionLambda.Compile();
        }

        private static Expression ConvertEnum(Expression value, Type destType)
        {
            var convertEnum = typeof(ThrowHelper).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(m => m.Name == nameof(ConvertEnum) && m.IsGenericMethod);
            return Expression.Call(convertEnum.MakeGenericMethod(value.Type, destType), value);
        }
        private static TDest ConvertEnum<TSource, TDest>(TSource value)
        {
            var name = Enum.GetName(typeof(TSource), value);
            return (TDest)Enum.Parse(typeof(TDest), name);
        }
    }
}
