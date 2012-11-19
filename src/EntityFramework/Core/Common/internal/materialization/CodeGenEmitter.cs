// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;

    internal static class CodeGenEmitter
    {
        #region Static Reflection info used in emitters

        internal static readonly MethodInfo CodeGenEmitter_BinaryEquals = typeof(CodeGenEmitter).GetMethod(
            "BinaryEquals", BindingFlags.NonPublic | BindingFlags.Static);

        internal static readonly MethodInfo CodeGenEmitter_CheckedConvert = typeof(CodeGenEmitter).GetMethod(
            "CheckedConvert", BindingFlags.NonPublic | BindingFlags.Static);

        internal static readonly MethodInfo CodeGenEmitter_Compile = typeof(CodeGenEmitter).GetMethod(
            "Compile", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(Expression) }, null);

        internal static readonly MethodInfo DbDataReader_GetValue = typeof(DbDataReader).GetMethod("GetValue");
        internal static readonly MethodInfo DbDataReader_GetString = typeof(DbDataReader).GetMethod("GetString");
        internal static readonly MethodInfo DbDataReader_GetInt16 = typeof(DbDataReader).GetMethod("GetInt16");
        internal static readonly MethodInfo DbDataReader_GetInt32 = typeof(DbDataReader).GetMethod("GetInt32");
        internal static readonly MethodInfo DbDataReader_GetInt64 = typeof(DbDataReader).GetMethod("GetInt64");
        internal static readonly MethodInfo DbDataReader_GetBoolean = typeof(DbDataReader).GetMethod("GetBoolean");
        internal static readonly MethodInfo DbDataReader_GetDecimal = typeof(DbDataReader).GetMethod("GetDecimal");
        internal static readonly MethodInfo DbDataReader_GetFloat = typeof(DbDataReader).GetMethod("GetFloat");
        internal static readonly MethodInfo DbDataReader_GetDouble = typeof(DbDataReader).GetMethod("GetDouble");
        internal static readonly MethodInfo DbDataReader_GetDateTime = typeof(DbDataReader).GetMethod("GetDateTime");
        internal static readonly MethodInfo DbDataReader_GetGuid = typeof(DbDataReader).GetMethod("GetGuid");
        internal static readonly MethodInfo DbDataReader_GetByte = typeof(DbDataReader).GetMethod("GetByte");
        internal static readonly MethodInfo DbDataReader_IsDBNull = typeof(DbDataReader).GetMethod("IsDBNull");

        internal static readonly ConstructorInfo EntityKey_ctor_SingleKey =
            typeof(EntityKey).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(EntitySet), typeof(object) }, null);

        internal static readonly ConstructorInfo EntityKey_ctor_CompositeKey =
            typeof(EntityKey).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(EntitySet), typeof(object[]) }, null);

        internal static readonly MethodInfo EntityWrapperFactory_GetEntityWithChangeTrackerStrategyFunc =
            typeof(EntityWrapperFactory).GetMethod("GetEntityWithChangeTrackerStrategyFunc", BindingFlags.NonPublic | BindingFlags.Static);

        internal static readonly MethodInfo EntityWrapperFactory_GetEntityWithKeyStrategyStrategyFunc =
            typeof(EntityWrapperFactory).GetMethod("GetEntityWithKeyStrategyStrategyFunc", BindingFlags.NonPublic | BindingFlags.Static);

        internal static readonly MethodInfo EntityProxyTypeInfo_SetEntityWrapper = typeof(EntityProxyTypeInfo).GetMethod(
            "SetEntityWrapper", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static readonly MethodInfo EntityWrapperFactory_GetNullPropertyAccessorStrategyFunc =
            typeof(EntityWrapperFactory).GetMethod("GetNullPropertyAccessorStrategyFunc", BindingFlags.NonPublic | BindingFlags.Static);

        internal static readonly MethodInfo EntityWrapperFactory_GetPocoEntityKeyStrategyFunc =
            typeof(EntityWrapperFactory).GetMethod("GetPocoEntityKeyStrategyFunc", BindingFlags.NonPublic | BindingFlags.Static);

        internal static readonly MethodInfo EntityWrapperFactory_GetPocoPropertyAccessorStrategyFunc =
            typeof(EntityWrapperFactory).GetMethod("GetPocoPropertyAccessorStrategyFunc", BindingFlags.NonPublic | BindingFlags.Static);

        internal static readonly MethodInfo EntityWrapperFactory_GetSnapshotChangeTrackingStrategyFunc =
            typeof(EntityWrapperFactory).GetMethod("GetSnapshotChangeTrackingStrategyFunc", BindingFlags.NonPublic | BindingFlags.Static);

        internal static readonly PropertyInfo EntityWrapperFactory_NullWrapper = typeof(NullEntityWrapper).GetProperty(
            "NullWrapper", BindingFlags.Static | BindingFlags.NonPublic);

        internal static readonly PropertyInfo IEntityWrapper_Entity = typeof(IEntityWrapper).GetProperty("Entity");

        internal static readonly MethodInfo IEqualityComparerOfString_Equals = typeof(IEqualityComparer<string>).GetMethod(
            "Equals", new[] { typeof(string), typeof(string) });

        internal static readonly ConstructorInfo MaterializedDataRecord_ctor = typeof(MaterializedDataRecord).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null, new[] { typeof(MetadataWorkspace), typeof(TypeUsage), typeof(object[]) },
            null);

        internal static readonly MethodInfo RecordState_GatherData = typeof(RecordState).GetMethod(
            "GatherData", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static readonly MethodInfo RecordState_SetNullRecord = typeof(RecordState).GetMethod(
            "SetNullRecord", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static readonly MethodInfo Shaper_Discriminate = typeof(Shaper).GetMethod("Discriminate");

        internal static readonly MethodInfo Shaper_GetPropertyValueWithErrorHandling =
            typeof(Shaper).GetMethod("GetPropertyValueWithErrorHandling");

        internal static readonly MethodInfo Shaper_GetColumnValueWithErrorHandling =
            typeof(Shaper).GetMethod("GetColumnValueWithErrorHandling");

        internal static readonly MethodInfo Shaper_GetGeographyColumnValue = typeof(Shaper).GetMethod("GetGeographyColumnValue");
        internal static readonly MethodInfo Shaper_GetGeometryColumnValue = typeof(Shaper).GetMethod("GetGeometryColumnValue");

        internal static readonly MethodInfo Shaper_GetSpatialColumnValueWithErrorHandling =
            typeof(Shaper).GetMethod("GetSpatialColumnValueWithErrorHandling");

        internal static readonly MethodInfo Shaper_GetSpatialPropertyValueWithErrorHandling =
            typeof(Shaper).GetMethod("GetSpatialPropertyValueWithErrorHandling");

        internal static readonly MethodInfo Shaper_HandleEntity = typeof(Shaper).GetMethod("HandleEntity");
        internal static readonly MethodInfo Shaper_HandleEntityAppendOnly = typeof(Shaper).GetMethod("HandleEntityAppendOnly");
        internal static readonly MethodInfo Shaper_HandleEntityNoTracking = typeof(Shaper).GetMethod("HandleEntityNoTracking");
        internal static readonly MethodInfo Shaper_HandleFullSpanCollection = typeof(Shaper).GetMethod("HandleFullSpanCollection");
        internal static readonly MethodInfo Shaper_HandleFullSpanElement = typeof(Shaper).GetMethod("HandleFullSpanElement");
        internal static readonly MethodInfo Shaper_HandleIEntityWithKey = typeof(Shaper).GetMethod("HandleIEntityWithKey");
        internal static readonly MethodInfo Shaper_HandleRelationshipSpan = typeof(Shaper).GetMethod("HandleRelationshipSpan");
        internal static readonly MethodInfo Shaper_SetColumnValue = typeof(Shaper).GetMethod("SetColumnValue");
        internal static readonly MethodInfo Shaper_SetEntityRecordInfo = typeof(Shaper).GetMethod("SetEntityRecordInfo");
        internal static readonly MethodInfo Shaper_SetState = typeof(Shaper).GetMethod("SetState");
        internal static readonly MethodInfo Shaper_SetStatePassthrough = typeof(Shaper).GetMethod("SetStatePassthrough");

        #endregion

        #region Static expressions used in emitters

        internal static readonly Expression DBNull_Value = Expression.Constant(DBNull.Value, typeof(object));

        internal static readonly ParameterExpression Shaper_Parameter = Expression.Parameter(typeof(Shaper), "shaper");

        internal static readonly Expression Shaper_Reader = Expression.Field(Shaper_Parameter, typeof(Shaper).GetField("Reader"));
        internal static readonly Expression Shaper_Workspace = Expression.Field(Shaper_Parameter, typeof(Shaper).GetField("Workspace"));
        internal static readonly Expression Shaper_State = Expression.Field(Shaper_Parameter, typeof(Shaper).GetField("State"));
        internal static readonly Expression Shaper_Context = Expression.Field(Shaper_Parameter, typeof(Shaper).GetField("Context"));

        internal static readonly Expression Shaper_Context_Options = Expression.Property(
            Shaper_Context, typeof(ObjectContext).GetProperty("ContextOptions"));

        internal static readonly Expression Shaper_ProxyCreationEnabled = Expression.Property(
            Shaper_Context_Options, typeof(ObjectContextOptions).GetProperty("ProxyCreationEnabled"));

        #endregion

        /// <summary>
        ///     Helper method used in expressions generated by Emit_Equal to perform a
        ///     byte-by-byte comparison of two byte arrays.  There really ought to be
        ///     a way to do this in the framework but I'm unaware of it.
        /// </summary>
        internal static bool BinaryEquals(byte[] left, byte[] right)
        {
            if (null == left)
            {
                return null == right;
            }
            else if (null == right)
            {
                return false;
            }
            if (left.Length
                != right.Length)
            {
                return false;
            }
            for (var i = 0; i < left.Length; i++)
            {
                if (left[i]
                    != right[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///     Compiles a delegate taking a Shaper instance and returning values. Used to compile
        ///     Expressions produced by the emitter.
        ///     Asserts MemberAccess to skip visbility check.
        ///     This means that that security checks are skipped. Before calling this
        ///     method you must ensure that you've done a TestComple on expressions provided
        ///     by the user to ensure the compilation doesn't violate them.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2128")]
        [SecuritySafeCritical]
        [ReflectionPermission(SecurityAction.Assert, MemberAccess = true)]
        internal static Func<Shaper, TResult> Compile<TResult>(Expression body)
        {
            return BuildShaperLambda<TResult>(body).Compile();
        }

        internal static Expression<Func<Shaper, TResult>> BuildShaperLambda<TResult>(Expression body)
        {
            return body == null
                       ? null
                       : Expression.Lambda<Func<Shaper, TResult>>(body, Shaper_Parameter);
        }

        /// <summary>
        ///     Non-generic version of Compile (where the result type is passed in as an argument rather
        ///     than a type parameter)
        /// </summary>
        // The caller might not have the reflection permission, so inlining this method could cause a security exception
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static object Compile(Type resultType, Expression body)
        {
            var compile = CodeGenEmitter_Compile.MakeGenericMethod(resultType);
            return compile.Invoke(null, new object[] { body });
        }

        #region Lightweight CodeGen emitters

        /// <summary>
        ///     Create expression to AndAlso the expressions and return the result.
        /// </summary>
        internal static Expression Emit_AndAlso(IEnumerable<Expression> operands)
        {
            Expression result = null;
            foreach (var operand in operands)
            {
                if (result == null)
                {
                    result = operand;
                }
                else
                {
                    result = Expression.AndAlso(result, operand);
                }
            }
            return result;
        }

        /// <summary>
        ///     Create expression to bitwise-or the expressions and return the result.
        /// </summary>
        internal static Expression Emit_BitwiseOr(IEnumerable<Expression> operands)
        {
            Expression result = null;
            foreach (var operand in operands)
            {
                if (result == null)
                {
                    result = operand;
                }
                else
                {
                    result = Expression.Or(result, operand);
                }
            }
            return result;
        }

        /// <summary>
        ///     Creates an expression with null value. If the given type cannot be assigned
        ///     a null value, we create a value that throws when materializing. We don't throw statically
        ///     because we consistently defer type checks until materialization.
        ///     See SQL BU 588980.
        /// </summary>
        /// <param name="type"> Type of null expression. </param>
        /// <returns> Null expression. </returns>
        internal static Expression Emit_NullConstant(Type type)
        {
            Expression nullConstant;
            DebugCheck.NotNull(type);

            // check if null can be assigned to the type
            if (type.IsClass
                || TypeSystem.IsNullableType(type))
            {
                // create the constant directly if it accepts null
                nullConstant = Expression.Constant(null, type);
            }
            else
            {
                // create (object)null and then cast to the type
                nullConstant = Emit_EnsureType(Expression.Constant(null, typeof(object)), type);
            }
            return nullConstant;
        }

        /// <summary>
        ///     Emits an expression that represnts a NullEntityWrapper instance.
        /// </summary>
        /// <returns> An expression represnting a wrapped null </returns>
        internal static Expression Emit_WrappedNullConstant()
        {
            return Expression.Property(null, EntityWrapperFactory_NullWrapper);
        }

        /// <summary>
        ///     Create expression that guarantees the input expression is of the specified
        ///     type; no Convert is added if the expression is already of the same type.
        ///     Internal because it is called from the TranslatorResult.
        /// </summary>
        internal static Expression Emit_EnsureType(Expression input, Type type)
        {
            var result = input;
            if (input.Type != type
                && !typeof(IEntityWrapper).IsAssignableFrom(input.Type))
            {
                if (type.IsAssignableFrom(input.Type))
                {
                    // simple convert, just to make sure static type checks succeed
                    result = Expression.Convert(input, type);
                }
                else
                {
                    // user is asking for the 'wrong' type... add exception handling
                    // in case of failure
                    var checkedConvertMethod = CodeGenEmitter_CheckedConvert.MakeGenericMethod(input.Type, type);
                    result = Expression.Call(checkedConvertMethod, input);
                }
            }
            return result;
        }

        /// <summary>
        ///     Uses Emit_EnsureType and then wraps the result in an IEntityWrapper instance.
        /// </summary>
        /// <param name="input"> The expression that creates the entity to be wrapped </param>
        /// <param name="keyReader"> Expression to read the entity key </param>
        /// <param name="entitySetReader"> Expression to read the entity set </param>
        /// <param name="requestedType"> The type that was actuall requested by the client--may be object </param>
        /// <param name="identityType"> The type of the identity type of the entity being materialized--never a proxy type </param>
        /// <param name="actualType"> The actual type being materialized--may be a proxy type </param>
        /// <param name="mergeOption"> Either NoTracking or AppendOnly depending on whether the entity is to be tracked </param>
        /// <param name="isProxy"> If true, then a proxy is being created </param>
        /// <returns> An expression representing the IEntityWrapper for the new entity </returns>
        internal static Expression Emit_EnsureTypeAndWrap(
            Expression input, Expression keyReader, Expression entitySetReader, Type requestedType, Type identityType, Type actualType,
            MergeOption mergeOption, bool isProxy)
        {
            var result = Emit_EnsureType(input, requestedType); // Needed to ensure appropriate exception is thrown
            if (!requestedType.IsClass)
            {
                result = Emit_EnsureType(input, typeof(object));
            }
            result = Emit_EnsureType(result, actualType); // Needed to ensure appropriate type for wrapper constructor
            return CreateEntityWrapper(result, keyReader, entitySetReader, actualType, identityType, mergeOption, isProxy);
        }

        /// <summary>
        ///     Returns an expression that creates an IEntityWrapper appropriate for the type of entity being materialized.
        /// </summary>
        internal static Expression CreateEntityWrapper(
            Expression input, Expression keyReader, Expression entitySetReader, Type actualType, Type identityType, MergeOption mergeOption,
            bool isProxy)
        {
            Expression result;
            var isIEntityWithKey = typeof(IEntityWithKey).IsAssignableFrom(actualType);
            var isIEntityWithRelationships = typeof(IEntityWithRelationships).IsAssignableFrom(actualType);
            var isIEntityWithChangeTracker = typeof(IEntityWithChangeTracker).IsAssignableFrom(actualType);
            if (isIEntityWithRelationships
                && isIEntityWithChangeTracker
                && isIEntityWithKey
                && !isProxy)
            {
                // This is the case where all our interfaces are implemented by the entity and we are not creating a proxy.
                // This is the case that absolutely must be kept fast.  It is a simple call to the wrapper constructor.
                var genericType = typeof(LightweightEntityWrapper<>).MakeGenericType(actualType);
                var ci = genericType.GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null,
                    new[] { actualType, typeof(EntityKey), typeof(EntitySet), typeof(ObjectContext), typeof(MergeOption), typeof(Type) },
                    null);
                result = Expression.New(
                    ci, input, keyReader, entitySetReader, Shaper_Context, Expression.Constant(mergeOption, typeof(MergeOption)),
                    Expression.Constant(identityType, typeof(Type)));
            }
            else
            {
                // This is the general case.  We choose various strategy objects based on the interfaces implemented and
                // whether or not we are creating a proxy.
                // We pass in lambdas to create the strategy objects so that they can have the materialized entity as
                // a parameter while still being set in the wrapper constructor.
                Expression propertyAccessorStrategy = !isIEntityWithRelationships || isProxy
                                                          ? Expression.Call(EntityWrapperFactory_GetPocoPropertyAccessorStrategyFunc)
                                                          : Expression.Call(EntityWrapperFactory_GetNullPropertyAccessorStrategyFunc);

                Expression keyStrategy = isIEntityWithKey
                                             ? Expression.Call(EntityWrapperFactory_GetEntityWithKeyStrategyStrategyFunc)
                                             : Expression.Call(EntityWrapperFactory_GetPocoEntityKeyStrategyFunc);

                Expression changeTrackingStrategy = isIEntityWithChangeTracker
                                                        ? Expression.Call(EntityWrapperFactory_GetEntityWithChangeTrackerStrategyFunc)
                                                        : Expression.Call(EntityWrapperFactory_GetSnapshotChangeTrackingStrategyFunc);

                var genericType = isIEntityWithRelationships
                                      ? typeof(EntityWrapperWithRelationships<>).MakeGenericType(actualType)
                                      : typeof(EntityWrapperWithoutRelationships<>).MakeGenericType(actualType);

                var ci = genericType.GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null,
                    new[]
                        {
                            actualType, typeof(EntityKey), typeof(EntitySet), typeof(ObjectContext), typeof(MergeOption), typeof(Type),
                            typeof(Func<object, IPropertyAccessorStrategy>), typeof(Func<object, IChangeTrackingStrategy>),
                            typeof(Func<object, IEntityKeyStrategy>)
                        }, null);
                result = Expression.New(
                    ci, input, keyReader, entitySetReader, Shaper_Context, Expression.Constant(mergeOption, typeof(MergeOption)),
                    Expression.Constant(identityType, typeof(Type)),
                    propertyAccessorStrategy, changeTrackingStrategy, keyStrategy);
            }
            result = Expression.Convert(result, typeof(IEntityWrapper));
            return result;
        }

        /// <summary>
        ///     Takes an expression that represents an IEntityWrapper instance and creates a new
        ///     expression that extracts the raw entity from this.
        /// </summary>
        internal static Expression Emit_UnwrapAndEnsureType(Expression input, Type type)
        {
            return Emit_EnsureType(Expression.Property(input, IEntityWrapper_Entity), type);
        }

        /// <summary>
        ///     Method that the generated expression calls when the types are not
        ///     assignable
        /// </summary>
        internal static TTarget CheckedConvert<TSource, TTarget>(TSource value)
        {
            checked
            {
                try
                {
                    return (TTarget)(object)value;
                }
                catch (InvalidCastException)
                {
                    var valueType = value.GetType();

                    // In the case of CompensatingCollection<T>, simply report IEnumerable<T> in the
                    // exception message because the user has no reason to know what the type represents.
                    if (valueType.IsGenericType
                        && valueType.GetGenericTypeDefinition() == typeof(CompensatingCollection<>))
                    {
                        valueType = typeof(IEnumerable<>).MakeGenericType(valueType.GetGenericArguments());
                    }
                    throw EntityUtil.ValueInvalidCast(valueType, typeof(TTarget));
                }
                catch (NullReferenceException)
                {
                    throw new InvalidOperationException(Strings.Materializer_NullReferenceCast(typeof(TTarget).Name));
                }
            }
        }

        /// <summary>
        ///     Create expression to compare the results of two expressions and return
        ///     whether they are equal.  Note we have special case logic for byte arrays.
        /// </summary>
        internal static Expression Emit_Equal(Expression left, Expression right)
        {
            DebugCheck.NotNull(left);
            DebugCheck.NotNull(right);
            Debug.Assert(left.Type == right.Type);

            Expression result;
            if (typeof(byte[])
                == left.Type)
            {
                result = Expression.Call(CodeGenEmitter_BinaryEquals, left, right);
            }
            else
            {
                result = Expression.Equal(left, right);
            }
            return result;
        }

        /// <summary>
        ///     Create expression that verifies that the entityKey has a value.  Note we just
        ///     presume that if the first key is non-null, all the keys will be valid.
        /// </summary>
        internal static Expression Emit_EntityKey_HasValue(SimpleColumnMap[] keyColumns)
        {
            Debug.Assert(0 < keyColumns.Length);

            // !shaper.Reader.IsDBNull(keyColumn[0].ordinal)
            var result = Emit_Reader_IsDBNull(keyColumns[0]);
            result = Expression.Not(result);
            return result;
        }

        /// <summary>
        ///     Create expression to call the GetValue method of the shaper's source data reader
        /// </summary>
        internal static Expression Emit_Reader_GetValue(int ordinal, Type type)
        {
            // (type)shaper.Reader.GetValue(ordinal)
            var result = Emit_EnsureType(Expression.Call(Shaper_Reader, DbDataReader_GetValue, Expression.Constant(ordinal)), type);
            return result;
        }

        /// <summary>
        ///     Create expression to call the IsDBNull method of the shaper's source data reader
        /// </summary>
        internal static Expression Emit_Reader_IsDBNull(int ordinal)
        {
            // shaper.Reader.IsDBNull(ordinal)
            Expression result = Expression.Call(Shaper_Reader, DbDataReader_IsDBNull, Expression.Constant(ordinal));
            return result;
        }

        /// <summary>
        ///     Create expression to call the IsDBNull method of the shaper's source data reader
        ///     for the scalar column represented by the column map.
        /// </summary>
        internal static Expression Emit_Reader_IsDBNull(ColumnMap columnMap)
        {
            // CONSIDER: I don't care for the derefing columnMap.  Find an alternative.
            var result = Emit_Reader_IsDBNull(((ScalarColumnMap)columnMap).ColumnPos);
            return result;
        }

        internal static Expression Emit_Conditional_NotDBNull(Expression result, int ordinal, Type columnType)
        {
            result = Expression.Condition(
                Emit_Reader_IsDBNull(ordinal),
                Expression.Constant(TypeSystem.GetDefaultValue(columnType), columnType),
                result);
            return result;
        }

        internal static MethodInfo GetReaderMethod(Type type, out bool isNullable)
        {
            DebugCheck.NotNull(type);

            MethodInfo result;
            isNullable = false;

            // determine if this is a Nullable<T>
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (null != underlyingType)
            {
                isNullable = true;
                type = underlyingType;
            }

            var typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.String:
                    result = DbDataReader_GetString;
                    isNullable = true;
                    break;
                case TypeCode.Int16:
                    result = DbDataReader_GetInt16;
                    break;
                case TypeCode.Int32:
                    result = DbDataReader_GetInt32;
                    break;
                case TypeCode.Int64:
                    result = DbDataReader_GetInt64;
                    break;
                case TypeCode.Boolean:
                    result = DbDataReader_GetBoolean;
                    break;
                case TypeCode.Decimal:
                    result = DbDataReader_GetDecimal;
                    break;
                case TypeCode.Double:
                    result = DbDataReader_GetDouble;
                    break;
                case TypeCode.Single:
                    result = DbDataReader_GetFloat;
                    break;
                case TypeCode.DateTime:
                    result = DbDataReader_GetDateTime;
                    break;
                case TypeCode.Byte:
                    result = DbDataReader_GetByte;
                    break;
                default:
                    if (typeof(Guid) == type)
                    {
                        // Guid doesn't have a type code
                        result = DbDataReader_GetGuid;
                    }
                    else if (typeof(TimeSpan) == type
                             || typeof(DateTimeOffset) == type)
                    {
                        // TimeSpan and DateTimeOffset don't have a type code or a specific
                        // GetXXX method
                        result = DbDataReader_GetValue;
                    }
                    else if (typeof(Object) == type)
                    {
                        // We assume that Object means we want DBNull rather than null. I believe this is a bug.
                        result = DbDataReader_GetValue;
                    }
                    else
                    {
                        result = DbDataReader_GetValue;
                        isNullable = true;
                    }
                    break;
            }
            return result;
        }

        /// <summary>
        ///     Create expression to read a property value with error handling
        /// </summary>
        internal static Expression Emit_Shaper_GetPropertyValueWithErrorHandling(
            Type propertyType, int ordinal, string propertyName, string typeName, TypeUsage columnType)
        {
            // // shaper.GetSpatialColumnValueWithErrorHandling(ordinal, propertyName, typeName, primitiveColumnType) OR shaper.GetColumnValueWithErrorHandling(ordinal, propertyName, typeName)
            Expression result;
            PrimitiveTypeKind primitiveColumnType;
            if (Helper.IsSpatialType(columnType, out primitiveColumnType))
            {
                result = Expression.Call(
                    Shaper_Parameter, Shaper_GetSpatialPropertyValueWithErrorHandling.MakeGenericMethod(propertyType),
                    Expression.Constant(ordinal), Expression.Constant(propertyName), Expression.Constant(typeName),
                    Expression.Constant(primitiveColumnType, typeof(PrimitiveTypeKind)));
            }
            else
            {
                result = Expression.Call(
                    Shaper_Parameter, Shaper_GetPropertyValueWithErrorHandling.MakeGenericMethod(propertyType), Expression.Constant(ordinal),
                    Expression.Constant(propertyName), Expression.Constant(typeName));
            }
            return result;
        }

        /// <summary>
        ///     Create expression to read a column value with error handling
        /// </summary>
        internal static Expression Emit_Shaper_GetColumnValueWithErrorHandling(Type resultType, int ordinal, TypeUsage columnType)
        {
            // shaper.GetSpatialColumnValueWithErrorHandling(ordinal, primitiveColumnType) OR shaper.GetColumnValueWithErrorHandling(ordinal)
            Expression result;
            PrimitiveTypeKind primitiveColumnType;
            if (Helper.IsSpatialType(columnType, out primitiveColumnType))
            {
                primitiveColumnType = Helper.IsGeographicType((PrimitiveType)columnType.EdmType)
                                          ? PrimitiveTypeKind.Geography
                                          : PrimitiveTypeKind.Geometry;
                result = Expression.Call(
                    Shaper_Parameter, Shaper_GetSpatialColumnValueWithErrorHandling.MakeGenericMethod(resultType),
                    Expression.Constant(ordinal), Expression.Constant(primitiveColumnType, typeof(PrimitiveTypeKind)));
            }
            else
            {
                result = Expression.Call(
                    Shaper_Parameter, Shaper_GetColumnValueWithErrorHandling.MakeGenericMethod(resultType), Expression.Constant(ordinal));
            }
            return result;
        }

        /// <summary>
        ///     Create expression to read a column value of type System.Data.Entity.Spatial.DbGeography by delegating to the DbSpatialServices implementation of the underlying provider
        /// </summary>
        internal static Expression Emit_Shaper_GetGeographyColumnValue(int ordinal)
        {
            // shaper.GetGeographyColumnValue(ordinal)
            Expression result = Expression.Call(Shaper_Parameter, Shaper_GetGeographyColumnValue, Expression.Constant(ordinal));
            return result;
        }

        /// <summary>
        ///     Create expression to read a column value of type System.Data.Entity.Spatial.DbGeometry by delegating to the DbSpatialServices implementation of the underlying provider
        /// </summary>
        internal static Expression Emit_Shaper_GetGeometryColumnValue(int ordinal)
        {
            // shaper.GetGeometryColumnValue(ordinal)
            Expression result = Expression.Call(Shaper_Parameter, Shaper_GetGeometryColumnValue, Expression.Constant(ordinal));
            return result;
        }

        /// <summary>
        ///     Create expression to read an item from the shaper's state array
        /// </summary>
        internal static Expression Emit_Shaper_GetState(int stateSlotNumber, Type type)
        {
            // (type)shaper.State[stateSlotNumber]
            var result = Emit_EnsureType(Expression.ArrayIndex(Shaper_State, Expression.Constant(stateSlotNumber)), type);
            return result;
        }

        /// <summary>
        ///     Create expression to set an item in the shaper's state array
        /// </summary>
        internal static Expression Emit_Shaper_SetState(int stateSlotNumber, Expression value)
        {
            // shaper.SetState<T>(stateSlotNumber, value)
            Expression result = Expression.Call(
                Shaper_Parameter, Shaper_SetState.MakeGenericMethod(value.Type), Expression.Constant(stateSlotNumber), value);
            return result;
        }

        /// <summary>
        ///     Create expression to set an item in the shaper's state array
        /// </summary>
        internal static Expression Emit_Shaper_SetStatePassthrough(int stateSlotNumber, Expression value)
        {
            // shaper.SetState<T>(stateSlotNumber, value)
            Expression result = Expression.Call(
                Shaper_Parameter, Shaper_SetStatePassthrough.MakeGenericMethod(value.Type), Expression.Constant(stateSlotNumber), value);
            return result;
        }

        #endregion
    }
}
