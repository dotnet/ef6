// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.QueryCache;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;

    /// <summary>
    ///     Translates query ColumnMap into ShaperFactory. Basically, we interpret the 
    ///     ColumnMap and compile delegates used to materialize results.
    /// </summary>
    internal class Translator
    {
        private static readonly MethodInfo GenericTranslateColumnMap = typeof(Translator).GetMethod(
            "TranslateColumnMap", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        ///     The main entry point for the translation process. Given a ColumnMap, returns 
        ///     a ShaperFactory which can be used to materialize results for a query.
        /// </summary>
        internal virtual ShaperFactory<T> TranslateColumnMap<T>(
            QueryCacheManager queryCacheManager, ColumnMap columnMap, MetadataWorkspace workspace, SpanIndex spanIndex,
            MergeOption mergeOption, bool valueLayer)
        {
            Contract.Requires(queryCacheManager != null);
            Contract.Requires(columnMap != null);
            Contract.Requires(workspace != null);

            Contract.Ensures(Contract.Result<ShaperFactory>() != null);

            Contract.Assert(columnMap is CollectionColumnMap, "root column map must be a collection for a query");

            // If the query cache already contains a plan, then we're done
            ShaperFactory<T> result;
            var columnMapKey = ColumnMapKeyBuilder.GetColumnMapKey(columnMap, spanIndex);
            var cacheKey = new ShaperFactoryQueryCacheKey<T>(columnMapKey, mergeOption, valueLayer);

            if (queryCacheManager.TryCacheLookup(cacheKey, out result))
            {
                return result;
            }

            // Didn't find it in the cache, so we have to do the translation;  First create
            // the translator visitor that recursively tranforms ColumnMaps into Expressions
            // stored on the CoordinatorScratchpads it also constructs.  We'll compile those
            // expressions into delegates later.
            var translatorVisitor = new TranslatorVisitor(workspace, spanIndex, mergeOption, valueLayer);
            columnMap.Accept(translatorVisitor, new TranslatorArg(typeof(IEnumerable<>).MakeGenericType(typeof(T))));

            Contract.Assert(
                null != translatorVisitor.RootCoordinatorScratchpad,
                "translating the root of the query must populate RootCoordinatorScratchpad");

            // We're good. Go ahead and recursively compile the CoordinatorScratchpads we
            // created in the vistor into CoordinatorFactories which contain compiled
            // delegates for the expressions we generated.
            var coordinatorFactory = (CoordinatorFactory<T>)translatorVisitor.RootCoordinatorScratchpad.Compile();

            // Along the way we constructed a nice delegate to perform runtime permission 
            // checks (e.g. for LinkDemand and non-public members).  We need that now.
            var checkPermissionsDelegate = translatorVisitor.GetCheckPermissionsDelegate();

            // Finally, take everything we've produced, and create the ShaperFactory to
            // contain it all, then add it to the query cache so we don't need to do this
            // for this query again.
            result = new ShaperFactory<T>(
                translatorVisitor.StateSlotCount, coordinatorFactory, checkPermissionsDelegate, mergeOption);
            var cacheEntry = new QueryCacheEntry(cacheKey, result);
            if (queryCacheManager.TryLookupAndAdd(cacheEntry, out cacheEntry))
            {
                // Someone beat us to it. Use their result instead.
                result = (ShaperFactory<T>)cacheEntry.GetTarget();
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static ShaperFactory TranslateColumnMap(
            Translator translator,
            Type elementType,
            QueryCacheManager queryCacheManager,
            ColumnMap columnMap,
            MetadataWorkspace workspace,
            SpanIndex spanIndex,
            MergeOption mergeOption,
            bool valueLayer)
        {
            Contract.Requires(elementType != null);
            Contract.Requires(queryCacheManager != null);
            Contract.Requires(columnMap != null);
            Contract.Requires(workspace != null);

            Contract.Ensures(Contract.Result<ShaperFactory>() != null);

            var typedCreateMethod = GenericTranslateColumnMap.MakeGenericMethod(elementType);

            return (ShaperFactory)typedCreateMethod.Invoke(
                translator, new object[] { queryCacheManager, columnMap, workspace, spanIndex, mergeOption, valueLayer });
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private class TranslatorVisitor : ColumnMapVisitorWithResults<TranslatorResult, TranslatorArg>
        {
            #region Private state

            /// <summary>
            ///     Gets the O-Space Metadata workspace.
            /// </summary>
            private readonly MetadataWorkspace _workspace;

            /// <summary>
            ///     Gets structure telling us how to interpret 'span' rows (includes implicit
            ///     relationship span and explicit full span via ObjectQuery.Include().
            /// </summary>
            private readonly SpanIndex _spanIndex;

            /// <summary>
            ///     Gets the MergeOption for the current query (influences our handling of 
            ///     entities when they are materialized).
            /// </summary>
            private readonly MergeOption _mergeOption;

            /// <summary>
            ///     When true, indicates we're processing for the value layer (BridgeDataReader)
            ///     and not the ObjectMaterializer
            /// </summary>
            private readonly bool IsValueLayer;

            /// <summary>
            ///     Gets scratchpad for the coordinator builder for the nested reader currently
            ///     being translated or emitted.
            /// </summary>
            private CoordinatorScratchpad _currentCoordinatorScratchpad;

            /// <summary>
            ///     Set to true if any Entity/Complex type/property for which we're emitting a
            ///     handler is non-public. Used to determine which security checks are necessary 
            ///     when invoking the delegate.
            /// </summary>
            private bool _hasNonPublicMembers;

            /// <summary>
            ///     Local cache of ObjectTypeMappings for EdmTypes (to prevent expensive lookups).
            /// </summary>
            private readonly Dictionary<EdmType, ObjectTypeMapping> _objectTypeMappings = new Dictionary<EdmType, ObjectTypeMapping>();

            #endregion

            private static readonly MethodInfo Translator_MultipleDiscriminatorPolymorphicColumnMapHelper =
                typeof(TranslatorVisitor).GetMethod(
                    "MultipleDiscriminatorPolymorphicColumnMapHelper", BindingFlags.NonPublic | BindingFlags.Instance);

            private static readonly MethodInfo Translator_TypedCreateInlineDelegate = typeof(TranslatorVisitor).GetMethod(
                "TypedCreateInlineDelegate", BindingFlags.NonPublic | BindingFlags.Instance);

            public TranslatorVisitor(MetadataWorkspace workspace, SpanIndex spanIndex, MergeOption mergeOption, bool valueLayer)
            {
                Contract.Requires(workspace != null);

                _workspace = workspace;
                _spanIndex = spanIndex;
                _mergeOption = mergeOption;
                IsValueLayer = valueLayer;
            }

            #region "Public" surface

            /// <summary>
            ///     Scratchpad for topmost nested reader coordinator.
            /// </summary>
            public CoordinatorScratchpad RootCoordinatorScratchpad { get; private set; }

            /// <summary>
            ///     Gets number of 'Shaper.State' slots allocated (used to hold onto intermediate
            ///     values during materialization)
            /// </summary>
            public int StateSlotCount { get; private set; }

            /// <summary>
            ///     Returns a delegate performing necessary permission checks identified
            ///     by this translator.  This delegate must be called every time a row is 
            ///     read from the ObjectResult enumerator, since the enumerator can be 
            ///     passed across security contexts.
            /// </summary>
            public Action GetCheckPermissionsDelegate()
            {
                // Emit an action to check runtime permissions.
                return _hasNonPublicMembers ? (Action)DemandMemberAccess : null;
            }

            // utility accept that looks up CLR type
            private static TranslatorResult AcceptWithMappedType(TranslatorVisitor translatorVisitor, ColumnMap columnMap)
            {
                var type = translatorVisitor.DetermineClrType(columnMap.Type);
                var result = columnMap.Accept(translatorVisitor, new TranslatorArg(type));
                return result;
            }

            #endregion

            #region Structured columns

            /// <summary>
            ///     Visit(ComplexTypeColumnMap)
            /// </summary>
            internal override TranslatorResult Visit(ComplexTypeColumnMap columnMap, TranslatorArg arg)
            {
                Expression result = null;
                Expression nullSentinelCheck = null;

                if (null != columnMap.NullSentinel)
                {
                    nullSentinelCheck = CodeGenEmitter.Emit_Reader_IsDBNull(columnMap.NullSentinel);
                }

                if (IsValueLayer)
                {
                    result = BuildExpressionToGetRecordState(columnMap, null, null, nullSentinelCheck);
                }
                else
                {
                    var complexType = (ComplexType)columnMap.Type.EdmType;
                    var clrType = DetermineClrType(complexType);
                    var constructor = GetConstructor(clrType);

                    // Build expressions to read the property values from the source data 
                    // reader and bind them to their target properties
                    var propertyBindings = CreatePropertyBindings(columnMap, complexType.Properties);

                    // We have all the property bindings now; go ahead and build the expression to
                    // construct the type and store the property values.
                    result = Expression.MemberInit(Expression.New(constructor), propertyBindings);

                    // If there's a null sentinel, then everything above is gated upon whether 
                    // it's value is DBNull.Value.
                    if (null != nullSentinelCheck)
                    {
                        // shaper.Reader.IsDBNull(nullsentinelOridinal) ? (type)null : result
                        result = Expression.Condition(nullSentinelCheck, CodeGenEmitter.Emit_NullConstant(result.Type), result);
                    }
                }
                return new TranslatorResult(result, arg.RequestedType);
            }

            /// <summary>
            ///     Visit(EntityColumnMap)
            /// </summary>
            internal override TranslatorResult Visit(EntityColumnMap columnMap, TranslatorArg arg)
            {
                Expression result;

                // Build expressions to read the entityKey and determine the entitySet. Note
                // that we attempt to optimize things such that we won't construct anything 
                // that isn't needed, depending upon the interfaces the clrType derives from 
                // and the MergeOption that was requested.
                //
                // We always need the entitySet, except when MergeOption.NoTracking
                //
                // We always need the entityKey, except when MergeOption.NoTracking and the
                // clrType doesn't derive from IEntityWithKey
                var entityIdentity = columnMap.EntityIdentity;
                Expression entitySetReader = null;
                var entityKeyReader = Emit_EntityKey_ctor(this, entityIdentity, false, out entitySetReader);

                if (IsValueLayer)
                {
                    Expression nullCheckExpression = Expression.Not(CodeGenEmitter.Emit_EntityKey_HasValue(entityIdentity.Keys));
                    //Expression nullCheckExpression = Emit_EntityKey_HasValue(entityIdentity.Keys);
                    result = BuildExpressionToGetRecordState(columnMap, entityKeyReader, entitySetReader, nullCheckExpression);
                }
                else
                {
                    Expression constructEntity = null;

                    var cSpaceType = (EntityType)columnMap.Type.EdmType;
                    Debug.Assert(cSpaceType.BuiltInTypeKind == BuiltInTypeKind.EntityType, "Type was " + cSpaceType.BuiltInTypeKind);
                    var oSpaceType = (ClrEntityType)LookupObjectMapping(cSpaceType).ClrType;
                    var clrType = oSpaceType.ClrType;

                    // Build expressions to read the property values from the source data 
                    // reader and bind them to their target properties
                    var propertyBindings = CreatePropertyBindings(columnMap, cSpaceType.Properties);

                    // We have all the property bindings now; go ahead and build the expression to
                    // construct the entity or proxy and store the property values.  We'll wrap it with more
                    // stuff that needs to happen (or not) below.
                    var proxyTypeInfo = EntityProxyFactory.GetProxyType(oSpaceType);

                    // If no proxy type exists for the entity, construct the regular entity object.
                    // If a proxy type does exist, examine the ObjectContext.ContextOptions.ProxyCreationEnabled flag
                    // to determine whether to create a regular or proxy entity object.

                    var constructNonProxyEntity = Emit_ConstructEntity(
                        oSpaceType, propertyBindings, entityKeyReader, entitySetReader, arg, null);
                    if (proxyTypeInfo == null)
                    {
                        constructEntity = constructNonProxyEntity;
                    }
                    else
                    {
                        var constructProxyEntity = Emit_ConstructEntity(
                            oSpaceType, propertyBindings, entityKeyReader, entitySetReader, arg, proxyTypeInfo);

                        constructEntity = Expression.Condition(
                            CodeGenEmitter.Shaper_ProxyCreationEnabled,
                            constructProxyEntity,
                            constructNonProxyEntity);
                    }

                    // If we're tracking, call HandleEntity (or HandleIEntityWithKey or 
                    // HandleEntityAppendOnly) as appropriate
                    if (MergeOption.NoTracking != _mergeOption)
                    {
                        var actualType = proxyTypeInfo == null ? clrType : proxyTypeInfo.ProxyType;
                        if (typeof(IEntityWithKey).IsAssignableFrom(actualType)
                            && MergeOption.AppendOnly != _mergeOption)
                        {
                            constructEntity = Expression.Call(
                                CodeGenEmitter.Shaper_Parameter, CodeGenEmitter.Shaper_HandleIEntityWithKey.MakeGenericMethod(clrType),
                                constructEntity,
                                entitySetReader
                                );
                        }
                        else
                        {
                            if (MergeOption.AppendOnly == _mergeOption)
                            {
                                // pass through a delegate creating the entity rather than the actual entity, so we can avoid
                                // the cost of materialization when the entity is already in the state manager

                                //Func<Shaper, TEntity> entityDelegate = shaper => constructEntity(shaper);
                                var entityDelegate = CreateInlineDelegate(constructEntity);
                                constructEntity = Expression.Call(
                                    CodeGenEmitter.Shaper_Parameter, CodeGenEmitter.Shaper_HandleEntityAppendOnly.MakeGenericMethod(clrType),
                                    entityDelegate,
                                    entityKeyReader,
                                    entitySetReader
                                    );
                            }
                            else
                            {
                                constructEntity = Expression.Call(
                                    CodeGenEmitter.Shaper_Parameter, CodeGenEmitter.Shaper_HandleEntity.MakeGenericMethod(clrType),
                                    constructEntity,
                                    entityKeyReader,
                                    entitySetReader
                                    );
                            }
                        }
                    }
                    else
                    {
                        constructEntity = Expression.Call(
                            CodeGenEmitter.Shaper_Parameter, CodeGenEmitter.Shaper_HandleEntityNoTracking.MakeGenericMethod(clrType),
                            constructEntity
                            );
                    }

                    // All the above is gated upon whether there really is an entity value; 
                    // we won't bother executing anything unless there is an entityKey value,
                    // otherwise we'll just return a typed null.
                    result = Expression.Condition(
                        CodeGenEmitter.Emit_EntityKey_HasValue(entityIdentity.Keys),
                        constructEntity,
                        CodeGenEmitter.Emit_WrappedNullConstant()
                        );
                }

                return new TranslatorResult(result, arg.RequestedType);
            }

            private Expression Emit_ConstructEntity(
                EntityType oSpaceType, IEnumerable<MemberBinding> propertyBindings, Expression entityKeyReader, Expression entitySetReader,
                TranslatorArg arg, EntityProxyTypeInfo proxyTypeInfo)
            {
                var isProxy = proxyTypeInfo != null;
                var clrType = oSpaceType.ClrType;
                Type actualType;

                Expression constructEntity;

                if (isProxy)
                {
                    constructEntity = Expression.MemberInit(Expression.New(proxyTypeInfo.ProxyType), propertyBindings);
                    actualType = proxyTypeInfo.ProxyType;
                }
                else
                {
                    var constructor = GetConstructor(clrType);
                    constructEntity = Expression.MemberInit(Expression.New(constructor), propertyBindings);
                    actualType = clrType;
                }

                // After calling the constructor, immediately create an IEntityWrapper instance for the entity.
                constructEntity = CodeGenEmitter.Emit_EnsureTypeAndWrap(
                    constructEntity, entityKeyReader, entitySetReader, arg.RequestedType, clrType, actualType,
                    _mergeOption == MergeOption.NoTracking ? MergeOption.NoTracking : MergeOption.AppendOnly, isProxy);

                if (isProxy)
                {
                    // Since we created a proxy, we now need to give it a reference to the wrapper that we just created.
                    constructEntity = Expression.Call(
                        Expression.Constant(proxyTypeInfo), CodeGenEmitter.EntityProxyTypeInfo_SetEntityWrapper, constructEntity);

                    if (proxyTypeInfo.InitializeEntityCollections != null)
                    {
                        constructEntity = Expression.Call(proxyTypeInfo.InitializeEntityCollections, constructEntity);
                    }
                }

                return constructEntity;
            }

            /// <summary>
            ///     Prepare a list of PropertyBindings for each item in the specified property 
            ///     collection such that the mapped property of the specified clrType has its
            ///     value set from the source data reader.  
            /// 
            ///     Along the way we'll keep track of non-public properties and properties that
            ///     have link demands, so we can ensure enforce them.
            /// </summary>
            private List<MemberBinding> CreatePropertyBindings(
                StructuredColumnMap columnMap, ReadOnlyMetadataCollection<EdmProperty> properties)
            {
                var result = new List<MemberBinding>(columnMap.Properties.Length);

                var mapping = LookupObjectMapping(columnMap.Type.EdmType);

                for (var i = 0; i < columnMap.Properties.Length; i++)
                {
                    var edmProperty = mapping.GetPropertyMap(properties[i].Name).ClrProperty;

                    LightweightCodeGenerator.ValidateSetterProperty(edmProperty.PropertyInfo);
                    var propertyAccessor = edmProperty.PropertyInfo.GetSetMethod(nonPublic: true);
                    var propertyType = edmProperty.PropertyInfo.PropertyType;

                    // determine if any security checks are required
                    if (!IsPublic(propertyAccessor))
                    {
                        _hasNonPublicMembers = true;
                    }

                    // get translation of property value
                    var valueReader = columnMap.Properties[i].Accept(this, new TranslatorArg(propertyType)).Expression;

                    var scalarColumnMap = columnMap.Properties[i] as ScalarColumnMap;
                    if (null != scalarColumnMap)
                    {
                        var propertyName = propertyAccessor.Name.Substring(4); // substring to strip "set_"

                        // create a value reader with error handling
                        var valueReaderWithErrorHandling = CodeGenEmitter.Emit_Shaper_GetPropertyValueWithErrorHandling(
                            propertyType, scalarColumnMap.ColumnPos, propertyName, propertyAccessor.DeclaringType.Name, scalarColumnMap.Type);
                        _currentCoordinatorScratchpad.AddExpressionWithErrorHandling(valueReader, valueReaderWithErrorHandling);
                    }

                    var binding = Expression.Bind(GetProperty(propertyAccessor, edmProperty.EntityDeclaringType), valueReader);
                    result.Add(binding);
                }
                return result;
            }

            private static bool IsPublic(MethodBase method)
            {
                return (method.IsPublic && IsPublic(method.DeclaringType));
            }

            private static bool IsPublic(Type type)
            {
                return ((null == type) || (type.IsPublic && IsPublic(type.DeclaringType)));
            }

            /// <summary>
            ///     Gets the PropertyInfo representing the property with which the given setter method is associated.
            ///     This code is taken from Expression.Bind(MethodInfo) but adapted to take a type such that it
            ///     will work in cases in which the property was declared on a generic base class.  In such cases,
            ///     the declaringType needs to be the actual entity type, rather than the base class type.  Note that
            ///     declaringType can be null, in which case the setterMethod.DeclaringType is used.
            /// </summary>
            private static PropertyInfo GetProperty(MethodInfo setterMethod, Type declaringType)
            {
                if (declaringType == null)
                {
                    declaringType = setterMethod.DeclaringType;
                }
                var bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                foreach (var propertyInfo in declaringType.GetProperties(bindingAttr))
                {
                    if (propertyInfo.GetSetMethod(nonPublic: true) == setterMethod)
                    {
                        return propertyInfo;
                    }
                }
                Contract.Assert(
                    false,
                    "Should always find a property for the setterMethod since we got the setter method from a property in the first place.");
                return null;
            }

            /// <summary>
            ///     Visit(SimplePolymorphicColumnMap)
            /// </summary>
            internal override TranslatorResult Visit(SimplePolymorphicColumnMap columnMap, TranslatorArg arg)
            {
                Expression result;

                // We're building a conditional ladder, where we'll compare each 
                // discriminator value with the one from the source data reader, and 
                // we'll pick that type if they match.
                var discriminatorReader = AcceptWithMappedType(this, columnMap.TypeDiscriminator).Expression;

                if (IsValueLayer)
                {
                    result = CodeGenEmitter.Emit_EnsureType(
                        BuildExpressionToGetRecordState(columnMap, null, null, Expression.Constant(true)),
                        arg.RequestedType);
                }
                else
                {
                    result = CodeGenEmitter.Emit_WrappedNullConstant(); // the default
                }

                foreach (var typeChoice in columnMap.TypeChoices)
                {
                    // determine CLR type for the type choice, and don't bother adding 
                    // this choice if it can't produce a result
                    var type = DetermineClrType(typeChoice.Value.Type);

                    if (type.IsAbstract)
                    {
                        continue;
                    }

                    Expression discriminatorConstant = Expression.Constant(typeChoice.Key, discriminatorReader.Type);
                    Expression discriminatorMatches;

                    // For string types, we have to use a specific comparison that handles
                    // trailing spaces properly, not just the general equality test we use 
                    // elsewhere.
                    if (discriminatorReader.Type
                        == typeof(string))
                    {
                        discriminatorMatches = Expression.Call(
                            Expression.Constant(TrailingSpaceStringComparer.Instance), CodeGenEmitter.IEqualityComparerOfString_Equals,
                            discriminatorConstant,
                            discriminatorReader);
                    }
                    else
                    {
                        discriminatorMatches = CodeGenEmitter.Emit_Equal(discriminatorConstant, discriminatorReader);
                    }

                    result = Expression.Condition(
                        discriminatorMatches,
                        typeChoice.Value.Accept(this, arg).Expression,
                        result);
                }
                return new TranslatorResult(result, arg.RequestedType);
            }

            /// <summary>
            ///     Visit(MultipleDiscriminatorPolymorphicColumnMap)
            /// </summary>
            // The caller might not have the reflection permission, so inlining this method could cause a security exception
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            internal override TranslatorResult Visit(MultipleDiscriminatorPolymorphicColumnMap columnMap, TranslatorArg arg)
            {
                var multipleDiscriminatorPolymorphicColumnMapHelper =
                    Translator_MultipleDiscriminatorPolymorphicColumnMapHelper.MakeGenericMethod(arg.RequestedType);
                var result = (Expression)multipleDiscriminatorPolymorphicColumnMapHelper.Invoke(this, new object[] { columnMap, arg });
                return new TranslatorResult(result, arg.RequestedType);
            }

            /// <summary>
            ///     Helper method to simplify the construction of the types
            /// </summary>
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                Justification = "Called via reflection by the Visit method")]
            private Expression MultipleDiscriminatorPolymorphicColumnMapHelper<TElement>(
                MultipleDiscriminatorPolymorphicColumnMap columnMap)
            {
                // construct an array of discriminator values
                var discriminatorReaders = new Expression[columnMap.TypeDiscriminators.Length];
                for (var i = 0; i < discriminatorReaders.Length; i++)
                {
                    discriminatorReaders[i] = columnMap.TypeDiscriminators[i].Accept(this, new TranslatorArg(typeof(object))).Expression;
                }
                Expression discriminatorValues = Expression.NewArrayInit(typeof(object), discriminatorReaders);

                // Next build the expressions that will construct the type choices. An array of KeyValuePair<EntityType, Func<Shaper, TElement>>
                var elementDelegates = new List<Expression>();
                var typeDelegatePairType = typeof(KeyValuePair<EntityType, Func<Shaper, TElement>>);
                var typeDelegatePairConstructor =
                    typeDelegatePairType.GetConstructor(new[] { typeof(EntityType), typeof(Func<Shaper, TElement>) });
                foreach (var typeChoice in columnMap.TypeChoices)
                {
                    var typeReader = CodeGenEmitter.Emit_EnsureType(
                        AcceptWithMappedType(this, typeChoice.Value).UnwrappedExpression, typeof(TElement));
                    var typeReaderDelegate = CreateInlineDelegate(typeReader);
                    Expression typeDelegatePair = Expression.New(
                        typeDelegatePairConstructor,
                        Expression.Constant(typeChoice.Key),
                        typeReaderDelegate
                        );
                    elementDelegates.Add(typeDelegatePair);
                }

                // invoke shaper.Discrimate({ discriminatorValue1...discriminatorValueN }, discriminateDelegate, elementDelegates)
                var shaperDiscriminateOfT = CodeGenEmitter.Shaper_Discriminate.MakeGenericMethod(typeof(TElement));
                Expression result = Expression.Call(
                    CodeGenEmitter.Shaper_Parameter, shaperDiscriminateOfT,
                    discriminatorValues,
                    Expression.Constant(columnMap.Discriminate),
                    Expression.NewArrayInit(typeDelegatePairType, elementDelegates)
                    );
                return result;
            }

            /// <summary>
            ///     Visit(RecordColumnMap)
            /// </summary>
            internal override TranslatorResult Visit(RecordColumnMap columnMap, TranslatorArg arg)
            {
                Expression result = null;
                Expression nullSentinelCheck = null;

                if (null != columnMap.NullSentinel)
                {
                    nullSentinelCheck = CodeGenEmitter.Emit_Reader_IsDBNull(columnMap.NullSentinel);
                }

                if (IsValueLayer)
                {
                    result = BuildExpressionToGetRecordState(columnMap, null, null, nullSentinelCheck);
                }
                else
                {
                    Debug.Assert(columnMap.Type.EdmType.BuiltInTypeKind == BuiltInTypeKind.RowType, "RecordColumnMap without RowType?");
                    // we kind of depend upon this 
                    Expression nullConstant;

                    // There are (at least) three different reasons we have a RecordColumnMap
                    // so pick the method that handles the reason we have for this one.
                    InitializerMetadata initializerMetadata;
                    if (InitializerMetadata.TryGetInitializerMetadata(columnMap.Type, out initializerMetadata))
                    {
                        result = HandleLinqRecord(columnMap, initializerMetadata);
                        nullConstant = CodeGenEmitter.Emit_NullConstant(result.Type);
                    }
                    else
                    {
                        var spanRowType = (RowType)columnMap.Type.EdmType;

                        if (null != _spanIndex
                            && _spanIndex.HasSpanMap(spanRowType))
                        {
                            result = HandleSpandexRecord(columnMap, arg, spanRowType);
                            nullConstant = CodeGenEmitter.Emit_WrappedNullConstant();
                        }
                        else
                        {
                            result = HandleRegularRecord(columnMap, arg, spanRowType);
                            nullConstant = CodeGenEmitter.Emit_NullConstant(result.Type);
                        }
                    }

                    // If there is a null sentinel process it accordingly.
                    if (null != nullSentinelCheck)
                    {
                        // shaper.Reader.IsDBNull(nullsentinelOridinal) ? (type)null : result
                        result = Expression.Condition(nullSentinelCheck, nullConstant, result);
                    }
                }
                return new TranslatorResult(result, arg.RequestedType);
            }

            private Expression BuildExpressionToGetRecordState(
                StructuredColumnMap columnMap, Expression entityKeyReader, Expression entitySetReader, Expression nullCheckExpression)
            {
                var recordStateScratchpad = _currentCoordinatorScratchpad.CreateRecordStateScratchpad();

                var stateSlotNumber = AllocateStateSlot();
                recordStateScratchpad.StateSlotNumber = stateSlotNumber;

                var propertyCount = columnMap.Properties.Length;
                var readerCount = (null != entityKeyReader) ? propertyCount + 1 : propertyCount;

                recordStateScratchpad.ColumnCount = propertyCount;

                // We can have an entity here, even though it's a RecordResultColumn, because
                // it may be a polymorphic type; eg: TREAT(Product AS DiscontinuedProduct); we
                // construct an EntityRecordInfo with a sentinel EntityNotValidKey as it's Key
                EntityType entityTypeMetadata = null;
                if (TypeHelpers.TryGetEdmType(columnMap.Type, out entityTypeMetadata))
                {
                    recordStateScratchpad.DataRecordInfo = new EntityRecordInfo(entityTypeMetadata, EntityKey.EntityNotValidKey, null);
                }
                else
                {
                    var edmType = Helper.GetModelTypeUsage(columnMap.Type);
                    recordStateScratchpad.DataRecordInfo = new DataRecordInfo(edmType);
                }

                var propertyReaders = new Expression[readerCount];
                var propertyNames = new string[recordStateScratchpad.ColumnCount];
                var typeUsages = new TypeUsage[recordStateScratchpad.ColumnCount];

                for (var ordinal = 0; ordinal < propertyCount; ordinal++)
                {
                    var propertyReader = columnMap.Properties[ordinal].Accept(this, new TranslatorArg(typeof(Object))).Expression;

                    // recordState.SetColumnValue(i, propertyReader ?? DBNull.Value)
                    propertyReaders[ordinal] = Expression.Call(
                        CodeGenEmitter.Shaper_Parameter, CodeGenEmitter.Shaper_SetColumnValue,
                        Expression.Constant(stateSlotNumber),
                        Expression.Constant(ordinal),
                        Expression.Coalesce(propertyReader, CodeGenEmitter.DBNull_Value)
                        );

                    propertyNames[ordinal] = columnMap.Properties[ordinal].Name;
                    typeUsages[ordinal] = columnMap.Properties[ordinal].Type;
                }

                if (null != entityKeyReader)
                {
                    propertyReaders[readerCount - 1] = Expression.Call(
                        CodeGenEmitter.Shaper_Parameter, CodeGenEmitter.Shaper_SetEntityRecordInfo,
                        Expression.Constant(stateSlotNumber),
                        entityKeyReader,
                        entitySetReader);
                }

                recordStateScratchpad.GatherData = CodeGenEmitter.Emit_BitwiseOr(propertyReaders);
                recordStateScratchpad.PropertyNames = propertyNames;
                recordStateScratchpad.TypeUsages = typeUsages;

                // Finally, build the expression to read the recordState from the shaper state

                // (RecordState)shaperState.State[stateSlotNumber].GatherData(shaper)           
                Expression result = Expression.Call(
                    CodeGenEmitter.Emit_Shaper_GetState(stateSlotNumber, typeof(RecordState)), CodeGenEmitter.RecordState_GatherData,
                    CodeGenEmitter.Shaper_Parameter);

                // If there's a null check, then everything above is gated upon whether 
                // it's value is DBNull.Value.
                if (null != nullCheckExpression)
                {
                    Expression nullResult = Expression.Call(
                        CodeGenEmitter.Emit_Shaper_GetState(stateSlotNumber, typeof(RecordState)), CodeGenEmitter.RecordState_SetNullRecord);
                    // nullCheckExpression ? (type)null : result
                    result = Expression.Condition(nullCheckExpression, nullResult, result);
                }
                return result;
            }

            /// <summary>
            ///     Build expression to materialize LINQ initialization types (anonymous 
            ///     types, IGrouping, EntityCollection)
            /// </summary>
            private Expression HandleLinqRecord(RecordColumnMap columnMap, InitializerMetadata initializerMetadata)
            {
                var propertyReaders = new List<TranslatorResult>(columnMap.Properties.Length);

                foreach (var pair in columnMap.Properties.Zip(initializerMetadata.GetChildTypes()))
                {
                    var propertyColumnMap = pair.Key;
                    var type = pair.Value;

                    // Note that we're not just blindly using the type from the column map
                    // because we need to match the type thatthe initializer says it needs; 
                    // that's why were not using AcceptWithMappedType;
                    if (null == type)
                    {
                        type = DetermineClrType(propertyColumnMap.Type);
                    }

                    var propertyReader = propertyColumnMap.Accept(this, new TranslatorArg(type));
                    propertyReaders.Add(propertyReader);
                }

                var result = initializerMetadata.Emit(propertyReaders);
                return result;
            }

            /// <summary>
            ///     Build expression to materialize a data record.
            /// </summary>
            private Expression HandleRegularRecord(RecordColumnMap columnMap, TranslatorArg arg, RowType spanRowType)
            {
                // handle regular records

                // Build an array of expressions that read the individual values from the 
                // source data reader.
                var columnReaders = new Expression[columnMap.Properties.Length];
                for (var i = 0; i < columnReaders.Length; i++)
                {
                    var columnReader = AcceptWithMappedType(this, columnMap.Properties[i]).UnwrappedExpression;

                    // ((object)columnReader) ?? DBNull.Value
                    columnReaders[i] = Expression.Coalesce(
                        CodeGenEmitter.Emit_EnsureType(columnReader, typeof(object)), CodeGenEmitter.DBNull_Value);
                }
                // new object[] {columnReader0..columnReaderN}
                Expression columnReaderArray = Expression.NewArrayInit(typeof(object), columnReaders);

                // Get an expression representing the TypeUsage of the MaterializedDataRecord 
                // we're about to construct; we need to remove the span information from it, 
                // though, since we don't want to surface that...
                var type = columnMap.Type;
                if (null != _spanIndex)
                {
                    type = _spanIndex.GetSpannedRowType(spanRowType) ?? type;
                }
                Expression typeUsage = Expression.Constant(type, typeof(TypeUsage));

                // new MaterializedDataRecord(Shaper.Workspace, typeUsage, values)
                var result = CodeGenEmitter.Emit_EnsureType(
                    Expression.New(
                        CodeGenEmitter.MaterializedDataRecord_ctor, CodeGenEmitter.Shaper_Workspace, typeUsage, columnReaderArray),
                    arg.RequestedType);
                return result;
            }

            /// <summary>
            ///     Build expression to materialize the spanned information
            /// </summary>
            private Expression HandleSpandexRecord(RecordColumnMap columnMap, TranslatorArg arg, RowType spanRowType)
            {
                var spanMap = _spanIndex.GetSpanMap(spanRowType);

                // First, build the expression to materialize the root item.
                var result = columnMap.Properties[0].Accept(this, arg).Expression;

                // Now build expressions that call into the appropriate shaper method
                // for the type of span for each spanned item.
                for (var i = 1; i < columnMap.Properties.Length; i++)
                {
                    var targetMember = spanMap[i];
                    var propertyTranslatorResult = AcceptWithMappedType(this, columnMap.Properties[i]);
                    var spannedResultReader = propertyTranslatorResult.Expression;

                    // figure out the flavor of the span
                    var collectionTranslatorResult = propertyTranslatorResult as CollectionTranslatorResult;
                    if (null != collectionTranslatorResult)
                    {
                        var expressionToGetCoordinator = collectionTranslatorResult.ExpressionToGetCoordinator;

                        // full span collection
                        var elementType = spannedResultReader.Type.GetGenericArguments()[0];

                        var handleFullSpanCollectionMethod =
                            CodeGenEmitter.Shaper_HandleFullSpanCollection.MakeGenericMethod(arg.RequestedType, elementType);
                        result = Expression.Call(
                            CodeGenEmitter.Shaper_Parameter, handleFullSpanCollectionMethod, result, expressionToGetCoordinator,
                            Expression.Constant(targetMember));
                    }
                    else
                    {
                        if (typeof(EntityKey)
                            == spannedResultReader.Type)
                        {
                            // relationship span
                            var handleRelationshipSpanMethod =
                                CodeGenEmitter.Shaper_HandleRelationshipSpan.MakeGenericMethod(arg.RequestedType);
                            result = Expression.Call(
                                CodeGenEmitter.Shaper_Parameter, handleRelationshipSpanMethod, result, spannedResultReader,
                                Expression.Constant(targetMember));
                        }
                        else
                        {
                            // full span element
                            var handleFullSpanElementMethod = CodeGenEmitter.Shaper_HandleFullSpanElement.MakeGenericMethod(
                                arg.RequestedType, spannedResultReader.Type);
                            result = Expression.Call(
                                CodeGenEmitter.Shaper_Parameter, handleFullSpanElementMethod, result, spannedResultReader,
                                Expression.Constant(targetMember));
                        }
                    }
                }
                return result;
            }

            #endregion

            #region Collection columns

            /// <summary>
            ///     Visit(SimpleCollectionColumnMap)
            /// </summary>
            internal override TranslatorResult Visit(SimpleCollectionColumnMap columnMap, TranslatorArg arg)
            {
                return ProcessCollectionColumnMap(columnMap, arg);
            }

            /// <summary>
            ///     Visit(DiscriminatedCollectionColumnMap)
            /// </summary>
            internal override TranslatorResult Visit(DiscriminatedCollectionColumnMap columnMap, TranslatorArg arg)
            {
                return ProcessCollectionColumnMap(columnMap, arg, columnMap.Discriminator, columnMap.DiscriminatorValue);
            }

            /// <summary>
            ///     Common code for both Simple and Discrminated Column Maps.
            /// </summary>
            private TranslatorResult ProcessCollectionColumnMap(CollectionColumnMap columnMap, TranslatorArg arg)
            {
                return ProcessCollectionColumnMap(columnMap, arg, null, null);
            }

            /// <summary>
            ///     Common code for both Simple and Discriminated Column Maps.
            /// </summary>
            private TranslatorResult ProcessCollectionColumnMap(
                CollectionColumnMap columnMap, TranslatorArg arg, ColumnMap discriminatorColumnMap, object discriminatorValue)
            {
                var elementType = DetermineElementType(arg.RequestedType, columnMap);

                // CoordinatorScratchpad aggregates information about the current nested
                // result (represented by the given CollectionColumnMap)
                var coordinatorScratchpad = new CoordinatorScratchpad(elementType);

                // enter scope for current coordinator when translating children, etc.
                EnterCoordinatorTranslateScope(coordinatorScratchpad);

                var elementColumnMap = columnMap.Element;

                if (IsValueLayer)
                {
                    var structuredElement = elementColumnMap as StructuredColumnMap;

                    // If we have a collection of non-structured types we have to put 
                    // a structure around it, because we don't have data readers of 
                    // scalars, only structures.  We don't need a null sentinel because
                    // this structure can't ever be null.
                    if (null == structuredElement)
                    {
                        var columnMaps = new ColumnMap[1] { columnMap.Element };
                        elementColumnMap = new RecordColumnMap(columnMap.Element.Type, columnMap.Element.Name, columnMaps, null);
                    }
                }

                // Build the expression that will construct the element of the collection
                // from the source data reader.
                // We use UnconvertedExpression here so we can defer doing type checking in case
                // we need to translate to a POCO collection later in the process.
                var elementReader = elementColumnMap.Accept(this, new TranslatorArg(elementType)).UnconvertedExpression;

                // Build the expression(s) that read the collection's keys from the source
                // data reader; note that the top level collection may not have keys if there
                // are no children.
                Expression[] keyReaders;

                if (null != columnMap.Keys)
                {
                    keyReaders = new Expression[columnMap.Keys.Length];
                    for (var i = 0; i < keyReaders.Length; i++)
                    {
                        var keyReader = AcceptWithMappedType(this, columnMap.Keys[i]).Expression;
                        keyReaders[i] = keyReader;
                    }
                }
                else
                {
                    keyReaders = new Expression[] { };
                }

                // Build the expression that reads the discriminator value from the source
                // data reader.
                Expression discriminatorReader = null;
                if (null != discriminatorColumnMap)
                {
                    discriminatorReader = AcceptWithMappedType(this, discriminatorColumnMap).Expression;
                }

                // get expression retrieving the coordinator
                var expressionToGetCoordinator = BuildExpressionToGetCoordinator(
                    elementType, elementReader, keyReaders, discriminatorReader, discriminatorValue, coordinatorScratchpad);
                var getElementsExpression = typeof(Coordinator<>).MakeGenericType(elementType).GetMethod(
                    "GetElements", BindingFlags.NonPublic | BindingFlags.Instance);

                Expression result;
                if (IsValueLayer)
                {
                    result = expressionToGetCoordinator;
                }
                else
                {
                    // coordinator.GetElements()
                    result = Expression.Call(expressionToGetCoordinator, getElementsExpression);

                    // Perform the type check that was previously deferred so we could process POCO collections.
                    coordinatorScratchpad.Element = CodeGenEmitter.Emit_EnsureType(coordinatorScratchpad.Element, elementType);

                    // When materializing specifically requested collection types, we need
                    // to transfer the results from the Enumerable to the requested collection.
                    Type innerElementType;
                    if (EntityUtil.TryGetICollectionElementType(arg.RequestedType, out innerElementType))
                    {
                        // Given we have some type that implements ICollection<T>, we need to decide what concrete
                        // collection type to instantiate--See EntityUtil.DetermineCollectionType for details.
                        var typeToInstantiate = EntityUtil.DetermineCollectionType(arg.RequestedType);

                        if (typeToInstantiate == null)
                        {
                            throw new InvalidOperationException(
                                Strings.ObjectQuery_UnableToMaterializeArbitaryProjectionType(arg.RequestedType));
                        }

                        var listOfElementType = typeof(List<>).MakeGenericType(innerElementType);
                        if (typeToInstantiate != listOfElementType)
                        {
                            coordinatorScratchpad.InitializeCollection = CodeGenEmitter.Emit_EnsureType(
                                Expression.New(GetConstructor(typeToInstantiate)),
                                typeof(ICollection<>).MakeGenericType(innerElementType));
                        }
                        result = CodeGenEmitter.Emit_EnsureType(result, arg.RequestedType);
                    }
                    else
                    {
                        // If any compensation is required (returning IOrderedEnumerable<T>, not 
                        // just vanilla IEnumerable<T> we must wrap the result with a static class
                        // that is of the type expected.
                        if (!arg.RequestedType.IsAssignableFrom(result.Type))
                        {
                            // new CompensatingCollection<TElement>(_collectionReader)
                            var compensatingCollectionType = typeof(CompensatingCollection<>).MakeGenericType(elementType);
                            var constructorInfo = compensatingCollectionType.GetConstructors()[0];
                            result = CodeGenEmitter.Emit_EnsureType(Expression.New(constructorInfo, result), compensatingCollectionType);
                        }
                    }
                }
                ExitCoordinatorTranslateScope();
                return new CollectionTranslatorResult(result, arg.RequestedType, expressionToGetCoordinator);
            }

            /// <summary>
            ///     Returns the CLR Type of the element of the collection
            /// </summary>
            private Type DetermineElementType(Type collectionType, CollectionColumnMap columnMap)
            {
                Type result = null;

                if (IsValueLayer)
                {
                    result = typeof(RecordState);
                }
                else
                {
                    result = TypeSystem.GetElementType(collectionType);

                    // GetElementType returns the input type if it is not a collection.
                    if (result == collectionType)
                    {
                        // if the user isn't asking for a CLR collection type (e.g. ObjectQuery<object>("{{1, 2}}")), we choose for them
                        var edmElementType = ((CollectionType)columnMap.Type.EdmType).TypeUsage;
                        // the TypeUsage of the Element of the collection.
                        result = DetermineClrType(edmElementType);
                    }
                }
                return result;
            }

            /// <summary>
            ///     Build up the coordinator graph using Enter/ExitCoordinatorTranslateScope.
            /// </summary>
            private void EnterCoordinatorTranslateScope(CoordinatorScratchpad coordinatorScratchpad)
            {
                if (null == RootCoordinatorScratchpad)
                {
                    coordinatorScratchpad.Depth = 0;
                    RootCoordinatorScratchpad = coordinatorScratchpad;
                    _currentCoordinatorScratchpad = coordinatorScratchpad;
                }
                else
                {
                    coordinatorScratchpad.Depth = _currentCoordinatorScratchpad.Depth + 1;
                    _currentCoordinatorScratchpad.AddNestedCoordinator(coordinatorScratchpad);
                    _currentCoordinatorScratchpad = coordinatorScratchpad;
                }
            }

            private void ExitCoordinatorTranslateScope()
            {
                _currentCoordinatorScratchpad = _currentCoordinatorScratchpad.Parent;
            }

            /// <summary>
            ///     Return an expression to read the coordinator from a state slot at 
            ///     runtime.  This is the method where we store the expressions we've
            ///     been building into the CoordinatorScratchpad, which we'll compile
            ///     later, once we've left the visitor.
            /// </summary>
            private Expression BuildExpressionToGetCoordinator(
                Type elementType, Expression element, Expression[] keyReaders, Expression discriminator, object discriminatorValue,
                CoordinatorScratchpad coordinatorScratchpad)
            {
                var stateSlotNumber = AllocateStateSlot();
                coordinatorScratchpad.StateSlotNumber = stateSlotNumber;

                // Ensure that the element type of the collec element translator
                coordinatorScratchpad.Element = element;

                // Build expressions to set the key values into their state slots, and
                // to compare the current values from the source reader with the values
                // in the slots.
                var setKeyTerms = new List<Expression>(keyReaders.Length);
                var checkKeyTerms = new List<Expression>(keyReaders.Length);

                foreach (var keyReader in keyReaders)
                {
                    // allocate space for the key value in the reader state
                    var keyStateSlotNumber = AllocateStateSlot();

                    // SetKey: readerState.SetState<T>(stateSlot, keyReader)
                    setKeyTerms.Add(CodeGenEmitter.Emit_Shaper_SetState(keyStateSlotNumber, keyReader));

                    // CheckKey: ((T)readerState.State[ordinal]).Equals(keyValue)
                    checkKeyTerms.Add(
                        CodeGenEmitter.Emit_Equal(
                            CodeGenEmitter.Emit_Shaper_GetState(keyStateSlotNumber, keyReader.Type),
                            keyReader
                            )
                        );
                }

                // For setting keys, we use BitwiseOr so that we don't short-circuit (all  
                // key terms are set)
                coordinatorScratchpad.SetKeys = CodeGenEmitter.Emit_BitwiseOr(setKeyTerms);

                // When checking for equality, we use AndAlso so that we short-circuit (return
                // as soon as key values don't match)
                coordinatorScratchpad.CheckKeys = CodeGenEmitter.Emit_AndAlso(checkKeyTerms);

                if (null != discriminator)
                {
                    // discriminatorValue == discriminator
                    coordinatorScratchpad.HasData = CodeGenEmitter.Emit_Equal(
                        Expression.Constant(discriminatorValue, discriminator.Type),
                        discriminator
                        );
                }

                // Finally, build the expression to read the coordinator from the state
                // (Coordinator<elementType>)readerState.State[stateOrdinal]
                var result = CodeGenEmitter.Emit_Shaper_GetState(stateSlotNumber, typeof(Coordinator<>).MakeGenericType(elementType));
                return result;
            }

            #endregion

            #region Scalar columns

            /// <summary>
            ///     Visit(RefColumnMap)
            /// 
            ///     If the entityKey has a value, then return it otherwise return a null 
            ///     valued EntityKey.  The EntityKey construction is the tricky part.
            /// </summary>
            internal override TranslatorResult Visit(RefColumnMap columnMap, TranslatorArg arg)
            {
                var entityIdentity = columnMap.EntityIdentity;
                Expression entitySetReader; // Ignored here; used when constructing Entities

                // hasValue ? entityKey : (EntityKey)null
                Expression result = Expression.Condition(
                    CodeGenEmitter.Emit_EntityKey_HasValue(entityIdentity.Keys),
                    Emit_EntityKey_ctor(this, entityIdentity, true, out entitySetReader),
                    Expression.Constant(null, typeof(EntityKey))
                    );
                return new TranslatorResult(result, arg.RequestedType);
            }

            /// <summary>
            ///     Visit(ScalarColumnMap)
            /// 
            ///     Pretty basic stuff here; we just call the method that matches the
            ///     type of the column.  Of course we have to handle nullable/non-nullable
            ///     types, and non-value types.
            /// </summary>
            internal override TranslatorResult Visit(ScalarColumnMap columnMap, TranslatorArg arg)
            {
                var type = arg.RequestedType;
                var columnType = columnMap.Type;
                var ordinal = columnMap.ColumnPos;
                Expression result;

                // 1. Create an expression to access the column value as an instance of the correct type. For non-spatial types this requires a call to one of the
                //    DbDataReader GetXXX methods; spatial values must be read using the provider's spatial services implementation.
                // 2. If the type was nullable (strings, byte[], Nullable<T>), wrap the expression with a check for the DBNull value and produce the correct typed null instead.
                //    Since the base spatial types (DbGeography/DbGeometry) are reference types, this is always required for spatial columns.
                // 3. Also create a version of the expression with error handling so that we can throw better exception messages when needed
                //
                PrimitiveTypeKind typeKind;
                if (Helper.IsSpatialType(columnType, out typeKind))
                {
                    Debug.Assert(
                        Helper.IsGeographicType((PrimitiveType)columnType.EdmType)
                        || Helper.IsGeometricType((PrimitiveType)columnType.EdmType),
                        "Spatial primitive type is neither Geometry or Geography?");
                    result =
                        CodeGenEmitter.Emit_Conditional_NotDBNull(
                            Helper.IsGeographicType((PrimitiveType)columnType.EdmType)
                                ? CodeGenEmitter.Emit_EnsureType(CodeGenEmitter.Emit_Shaper_GetGeographyColumnValue(ordinal), type)
                                : CodeGenEmitter.Emit_EnsureType(CodeGenEmitter.Emit_Shaper_GetGeometryColumnValue(ordinal), type),
                            ordinal, type);
                }
                else
                {
                    bool needsNullableCheck;
                    var readerMethod = CodeGenEmitter.GetReaderMethod(type, out needsNullableCheck);

                    result = Expression.Call(CodeGenEmitter.Shaper_Reader, readerMethod, Expression.Constant(ordinal));

                    // if the requested type is a nullable enum we need to cast it first to the non-nullable enum type to avoid InvalidCastException.
                    // Note that we guard against null values by wrapping the expression with DbNullCheck later. Also we don't actually 
                    // look at the type of the value returned by reader. If the value is not castable to enum we will fail with cast exception.
                    var nonNullableType = TypeSystem.GetNonNullableType(type);
                    if (nonNullableType.IsEnum
                        && nonNullableType != type)
                    {
                        Debug.Assert(
                            needsNullableCheck,
                            "This is a nullable enum so needsNullableCheck should be true to emit code that handles null values read from the reader.");

                        result = Expression.Convert(result, nonNullableType);
                    }
                    else if (type == typeof(object))
                    {
                        Debug.Assert(
                            !needsNullableCheck,
                            "If the requested type is object there is no special handling for null values returned from the reader.");

                        // special case for an OSpace query where the requested type is object but the column type is of an enum type. In this case
                        // we want to return a boxed value of enum type instead a boxed value of the enum underlying type. We also need to handle null
                        // values to return DBNull to be consistent with behavior for primitive types (e.g. int)
                        if (!IsValueLayer
                            && TypeSemantics.IsEnumerationType(columnType))
                        {
                            result = Expression.Condition(
                                CodeGenEmitter.Emit_Reader_IsDBNull(ordinal),
                                result,
                                Expression.Convert(
                                    Expression.Convert(result, TypeSystem.GetNonNullableType(DetermineClrType(columnType.EdmType))),
                                    typeof(object)));
                        }
                    }

                    // (type)shaper.Reader.Get???(ordinal)
                    result = CodeGenEmitter.Emit_EnsureType(result, type);

                    if (needsNullableCheck)
                    {
                        result = CodeGenEmitter.Emit_Conditional_NotDBNull(result, ordinal, type);
                    }
                }

                var resultWithErrorHandling = CodeGenEmitter.Emit_Shaper_GetColumnValueWithErrorHandling(
                    arg.RequestedType, ordinal, columnType);
                _currentCoordinatorScratchpad.AddExpressionWithErrorHandling(result, resultWithErrorHandling);
                return new TranslatorResult(result, type);
            }

            /// <summary>
            ///     Visit(VarRefColumnMap)
            /// 
            ///     This should throw; VarRefColumnMaps should be removed by the PlanCompiler.
            /// </summary>
            internal override TranslatorResult Visit(VarRefColumnMap columnMap, TranslatorArg arg)
            {
                Debug.Fail("VarRefColumnMap should be substituted at this point");
                throw new InvalidOperationException(String.Empty);
            }

            #endregion

            #region Helper methods

            /// <summary>
            ///     Allocates a slot in 'Shaper.State' which can be used as storage for 
            ///     materialization tasks (e.g. remembering key values for a nested collection)
            /// </summary>
            private int AllocateStateSlot()
            {
                return StateSlotCount++;
            }

            [SuppressMessage("Microsoft.Security", "CA2143:TransparentMethodsShouldNotDemandFxCopRule")]
            private static void DemandMemberAccess()
            {
                new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Demand();
            }

            /// <summary>
            ///     Return the CLR type we're supposed to materialize for the TypeUsage
            /// </summary>
            private Type DetermineClrType(TypeUsage typeUsage)
            {
                return DetermineClrType(typeUsage.EdmType);
            }

            /// <summary>
            ///     Return the CLR type we're supposed to materialize for the EdmType
            /// </summary>
            private Type DetermineClrType(EdmType edmType)
            {
                Type result = null;
                // Normalize for spandex
                edmType = ResolveSpanType(edmType);

                switch (edmType.BuiltInTypeKind)
                {
                    case BuiltInTypeKind.EntityType:
                    case BuiltInTypeKind.ComplexType:
                        if (IsValueLayer)
                        {
                            result = typeof(RecordState);
                        }
                        else
                        {
                            result = LookupObjectMapping(edmType).ClrType.ClrType;
                        }
                        break;

                    case BuiltInTypeKind.RefType:
                        result = typeof(EntityKey);
                        break;

                    case BuiltInTypeKind.CollectionType:
                        if (IsValueLayer)
                        {
                            result = typeof(Coordinator<RecordState>);
                        }
                        else
                        {
                            var edmElementType = ((CollectionType)edmType).TypeUsage.EdmType;
                            result = DetermineClrType(edmElementType);
                            result = typeof(IEnumerable<>).MakeGenericType(result);
                        }
                        break;

                    case BuiltInTypeKind.EnumType:
                        if (IsValueLayer)
                        {
                            result = DetermineClrType(((EnumType)edmType).UnderlyingType);
                        }
                        else
                        {
                            result = LookupObjectMapping(edmType).ClrType.ClrType;
                            result = typeof(Nullable<>).MakeGenericType(result);
                        }
                        break;

                    case BuiltInTypeKind.PrimitiveType:
                        result = ((PrimitiveType)edmType).ClrEquivalentType;
                        if (result.IsValueType)
                        {
                            result = typeof(Nullable<>).MakeGenericType(result);
                        }
                        break;

                    case BuiltInTypeKind.RowType:
                        if (IsValueLayer)
                        {
                            result = typeof(RecordState);
                        }
                        else
                        {
                            // LINQ has anonymous types that aren't going to show up in our
                            // metadata workspace, and we don't want to hydrate a record when
                            // we need an anonymous type.  ELINQ solves this by annotating the
                            // edmType with some additional information, which we'll pick up 
                            // here.
                            var initializerMetadata = ((RowType)edmType).InitializerMetadata;
                            if (null != initializerMetadata)
                            {
                                result = initializerMetadata.ClrType;
                            }
                            else
                            {
                                // Otherwise, by default, we'll give DbDataRecord results (the 
                                // user can also cast to IExtendedDataRecord)
                                result = typeof(DbDataRecord);
                            }
                        }
                        break;

                    default:
                        Debug.Fail(
                            String.Format(
                                CultureInfo.CurrentCulture,
                                "The type {0} was not the expected scalar, enumeration, collection, structural, nominal, or reference type.",
                                edmType.GetType()));
                        break;
                }
                Debug.Assert(null != result, "no result?"); // just making sure we cover this in the switch statement.

                return result;
            }

            /// <summary>
            ///     Get the ConstructorInfo for the type specified, and ensure we keep track
            ///     of any security requirements that the type has.
            /// </summary>
            private ConstructorInfo GetConstructor(Type type)
            {
                ConstructorInfo result = null;
                if (!type.IsAbstract)
                {
                    result = LightweightCodeGenerator.GetConstructorForType(type);

                    // remember security requirements for this constructor
                    if (!IsPublic(result))
                    {
                        _hasNonPublicMembers = true;
                    }
                }
                return result;
            }

            /// <summary>
            ///     Retrieves object mapping metadata for the given type. The first time a type 
            ///     is encountered, we cache the metadata to avoid repeating the work for every 
            ///     row in result. 
            /// 
            ///     Caching at the materializer rather than workspace/metadata cache level optimizes
            ///     for transient types (including row types produced for span, LINQ initializations, 
            ///     collections and projections).
            /// </summary>
            private ObjectTypeMapping LookupObjectMapping(EdmType edmType)
            {
                Contract.Requires(null != edmType);

                ObjectTypeMapping result;

                var resolvedType = ResolveSpanType(edmType);
                if (null == resolvedType)
                {
                    resolvedType = edmType;
                }

                if (!_objectTypeMappings.TryGetValue(resolvedType, out result))
                {
                    result = Util.GetObjectMapping(resolvedType, _workspace);
                    _objectTypeMappings.Add(resolvedType, result);
                }
                return result;
            }

            /// <summary>
            ///     Remove spanned info from the edmType
            /// </summary>
            /// <param name="edmType"> </param>
            /// <returns> </returns>
            private EdmType ResolveSpanType(EdmType edmType)
            {
                var result = edmType;

                switch (result.BuiltInTypeKind)
                {
                    case BuiltInTypeKind.CollectionType:
                        // For collections, we have to edmType from the (potentially) spanned
                        // element of the collection, then build a new Collection around it.
                        result = ResolveSpanType(((CollectionType)result).TypeUsage.EdmType);
                        if (null != result)
                        {
                            result = new CollectionType(result);
                        }
                        break;

                    case BuiltInTypeKind.RowType:
                        // If there is a SpanMap, pick up the EdmType from the first column
                        // in the record, otherwise it's just the type we already have.
                        var rowType = (RowType)result;
                        if (null != _spanIndex
                            && _spanIndex.HasSpanMap(rowType))
                        {
                            result = rowType.Members[0].TypeUsage.EdmType;
                        }
                        break;
                }
                return result;
            }

            /// <summary>
            ///     Creates an expression representing an inline delegate of type Func{Shaper, body.Type};
            /// </summary>
            // The caller might not have the reflection permission, so inlining this method could cause a security exception
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
            private LambdaExpression CreateInlineDelegate(Expression body)
            {
                // Note that we call through to a typed method so that we can call Expression.Lambda<Func> instead
                // of the straightforward Expression.Lambda. The latter requires FullTrust.
                var delegateReturnType = body.Type;
                var createMethod = Translator_TypedCreateInlineDelegate.MakeGenericMethod(delegateReturnType);
                var result = (LambdaExpression)createMethod.Invoke(this, new object[] { body });
                return result;
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
                Justification = "Called via reflection by the non-generic overload")]
            private Expression<Func<Shaper, T>> TypedCreateInlineDelegate<T>(Expression body)
            {
                var result = Expression.Lambda<Func<Shaper, T>>(body, CodeGenEmitter.Shaper_Parameter);
                _currentCoordinatorScratchpad.AddInlineDelegate(result);
                return result;
            }

            /// <summary>
            ///     Creates expression to construct an EntityKey. Assumes that both the key has 
            ///     a value (Emit_EntityKey_HasValue == true) and that the EntitySet has value 
            ///     (EntitySet != null).
            /// </summary>
            private static Expression Emit_EntityKey_ctor(
                TranslatorVisitor translatorVisitor, EntityIdentity entityIdentity, bool isForColumnValue, out Expression entitySetReader)
            {
                Expression result;
                Expression setEntitySetStateSlotValue = null;

                // First build the expressions that read each value that comprises the EntityKey
                var keyReaders = new List<Expression>(entityIdentity.Keys.Length);
                for (var i = 0; i < entityIdentity.Keys.Length; i++)
                {
                    var keyReader = entityIdentity.Keys[i].Accept(translatorVisitor, new TranslatorArg(typeof(object))).Expression;
                    keyReaders.Add(keyReader);
                }

                // Next build the expression that determines us the entitySet; how we do this differs 
                // depending on whether we have a simple or discriminated identity.

                var simpleEntityIdentity = entityIdentity as SimpleEntityIdentity;
                if (null != simpleEntityIdentity)
                {
                    if (simpleEntityIdentity.EntitySet == null)
                    {
                        // 'Free-floating' entities do not have entity keys.
                        entitySetReader = Expression.Constant(null, typeof(EntitySet));
                        return Expression.Constant(null, typeof(EntityKey));
                    }
                    // For SimpleEntityIdentities, the entitySet expression is a constant
                    entitySetReader = Expression.Constant(simpleEntityIdentity.EntitySet, typeof(EntitySet));
                }
                else
                {
                    // For DiscriminatedEntityIdentities, the we have to search the EntitySetMap 
                    // for the matching discriminator value; we'll get the discriminator first, 
                    // the compare them all in sequence.                
                    var discriminatedEntityIdentity = (DiscriminatedEntityIdentity)entityIdentity;

                    var discriminator =
                        discriminatedEntityIdentity.EntitySetColumnMap.Accept(translatorVisitor, new TranslatorArg(typeof(int?))).Expression;
                    var entitySets = discriminatedEntityIdentity.EntitySetMap;

                    // CONSIDER: We could just do an index lookup here instead of a series of 
                    //         comparisons, however this is MEST, and they get what they asked for.

                    // (_discriminator == 0 ? entitySets[0] : (_discriminator == 1 ? entitySets[1] ... : null)
                    entitySetReader = Expression.Constant(null, typeof(EntitySet));
                    for (var i = 0; i < entitySets.Length; i++)
                    {
                        entitySetReader = Expression.Condition(
                            Expression.Equal(discriminator, Expression.Constant(i, typeof(int?))),
                            Expression.Constant(entitySets[i], typeof(EntitySet)),
                            entitySetReader
                            );
                    }

                    // Allocate a stateSlot to contain the entitySet we determine, and ensure we
                    // store it there on the way to constructing the key.
                    var entitySetStateSlotNumber = translatorVisitor.AllocateStateSlot();
                    setEntitySetStateSlotValue = CodeGenEmitter.Emit_Shaper_SetStatePassthrough(entitySetStateSlotNumber, entitySetReader);
                    entitySetReader = CodeGenEmitter.Emit_Shaper_GetState(entitySetStateSlotNumber, typeof(EntitySet));
                }

                // And now that we have all the pieces, construct the EntityKey using the appropriate
                // constructor (there's an optimized constructor for the single key case)
                if (1 == entityIdentity.Keys.Length)
                {
                    // new EntityKey(entitySet, keyReaders[0])
                    result = Expression.New(
                        CodeGenEmitter.EntityKey_ctor_SingleKey,
                        entitySetReader,
                        keyReaders[0]);
                }
                else
                {
                    // new EntityKey(entitySet, { keyReaders[0], ... keyReaders[n] })
                    result = Expression.New(
                        CodeGenEmitter.EntityKey_ctor_CompositeKey,
                        entitySetReader,
                        Expression.NewArrayInit(typeof(object), keyReaders));
                }

                // In the case where we've had to store the entitySetReader value in a 
                // state slot, we test the value for non-null before we construct the 
                // entityKey.  We use this opportunity to stuff the value into the state
                // slot, so the code above that attempts to read it from there will find
                // it.
                if (null != setEntitySetStateSlotValue)
                {
                    Expression noEntityKeyExpression;
                    if (translatorVisitor.IsValueLayer
                        && !isForColumnValue)
                    {
                        noEntityKeyExpression = Expression.Constant(EntityKey.NoEntitySetKey, typeof(EntityKey));
                    }
                    else
                    {
                        noEntityKeyExpression = Expression.Constant(null, typeof(EntityKey));
                    }
                    result = Expression.Condition(
                        Expression.Equal(setEntitySetStateSlotValue, Expression.Constant(null, typeof(EntitySet))),
                        noEntityKeyExpression,
                        result
                        );
                }
                return result;
            }

            #endregion
        }
    }
}
