namespace System.Data.Entity.Core
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.Internal;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.History;
    using System.Data.Entity.Resources;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal static class EntityUtil
    {
        internal const int AssemblyQualifiedNameIndex = 3;
        internal const int InvariantNameIndex = 2;

        internal const string Parameter = "Parameter";

        internal const CompareOptions StringCompareOptions =
            CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase;

        internal static bool? ThreeValuedNot(bool? operand)
        {
            // three-valued logic 'not' (T = true, F = false, U = unknown)
            //      !T = F
            //      !F = T
            //      !U = U
            return operand.HasValue ? !operand.Value : (bool?)null;
        }

        internal static bool? ThreeValuedAnd(bool? left, bool? right)
        {
            // three-valued logic 'and' (T = true, F = false, U = unknown)
            //
            //      T & T = T
            //      T & F = F
            //      F & F = F
            //      F & T = F
            //      F & U = F
            //      U & F = F
            //      T & U = U
            //      U & T = U
            //      U & U = U
            bool? result;
            if (left.HasValue
                && right.HasValue)
            {
                result = left.Value && right.Value;
            }
            else if (!left.HasValue
                     && !right.HasValue)
            {
                result = null; // unknown
            }
            else if (left.HasValue)
            {
                result = left.Value
                             ? (bool?)null
                             : // unknown
                         false;
            }
            else
            {
                result = right.Value
                             ? (bool?)null
                             : false;
            }
            return result;
        }

        internal static bool? ThreeValuedOr(bool? left, bool? right)
        {
            // three-valued logic 'or' (T = true, F = false, U = unknown)
            //
            //      T | T = T
            //      T | F = T
            //      F | F = F
            //      F | T = T
            //      F | U = U
            //      U | F = U
            //      T | U = T
            //      U | T = T
            //      U | U = U
            bool? result;
            if (left.HasValue
                && right.HasValue)
            {
                result = left.Value || right.Value;
            }
            else if (!left.HasValue
                     && !right.HasValue)
            {
                result = null; // unknown
            }
            else if (left.HasValue)
            {
                result = left.Value
                             ? true
                             : (bool?)null; // unknown
            }
            else
            {
                result = right.Value
                             ? true
                             : (bool?)null; // unknown
            }
            return result;
        }

        /// <summary>
        /// Zips two enumerables together (e.g., given {1, 3, 5} and {2, 4, 6} returns {{1, 2}, {3, 4}, {5, 6}})
        /// </summary>
        internal static IEnumerable<KeyValuePair<T1, T2>> Zip<T1, T2>(this IEnumerable<T1> first, IEnumerable<T2> second)
        {
            if (null == first
                || null == second)
            {
                yield break;
            }
            using (var firstEnumerator = first.GetEnumerator())
            {
                using (var secondEnumerator = second.GetEnumerator())
                {
                    while (firstEnumerator.MoveNext()
                           && secondEnumerator.MoveNext())
                    {
                        yield return new KeyValuePair<T1, T2>(firstEnumerator.Current, secondEnumerator.Current);
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the type implements ICollection<>
        /// </summary>
        internal static bool IsAnICollection(Type type)
        {
            return typeof(ICollection<>).IsAssignableFrom(type.GetGenericTypeDefinition()) ||
                   type.GetInterface(typeof(ICollection<>).FullName) != null;
        }

        /// <summary>
        /// Given a type that represents a collection, determine if the type implements ICollection&lt&gt, and if
        /// so return the element type of the collection.  Currently, if the collection implements ICollection&lt&gt
        /// multiple times with different types, then we will return false since this is not supported.
        /// </summary>
        /// <param name="collectionType">the collection type to examine</param>
        /// <param name="elementType">the type of element</param>
        /// <returns>true if the collection implement ICollection&lt&gt; false otherwise</returns>
        internal static bool TryGetICollectionElementType(Type collectionType, out Type elementType)
        {
            elementType = null;
            // We have to check if the type actually is the interface, or if it implements the interface:
            try
            {
                var collectionInterface =
                    (collectionType.IsGenericType && typeof(ICollection<>).IsAssignableFrom(collectionType.GetGenericTypeDefinition()))
                        ? collectionType
                        : collectionType.GetInterface(typeof(ICollection<>).FullName);

                // We need to make sure the type is fully specified otherwise we won't be able to add element to it.
                if (collectionInterface != null
                    && !collectionInterface.ContainsGenericParameters)
                {
                    elementType = collectionInterface.GetGenericArguments()[0];
                    return true;
                }
            }
            catch (AmbiguousMatchException)
            {
                // Thrown if collection type implements ICollection<> more than once
            }
            return false;
        }

        /// <summary>
        /// Helper method to determine the element type of the collection contained by the given property.
        /// If an unambiguous element type cannot be found, then an InvalidOperationException is thrown.
        /// </summary>
        internal static Type GetCollectionElementType(Type propertyType)
        {
            Type elementType;
            if (!TryGetICollectionElementType(propertyType, out elementType))
            {
                throw new InvalidOperationException(Strings.PocoEntityWrapper_UnexpectedTypeForNavigationProperty(
                    propertyType.FullName,
                    typeof(ICollection<>)));
            }
            return elementType;
        }

        /// <summary>
        /// This is used when we need to determine a concrete collection type given some type that may be
        /// abstract or an interface.
        /// </summary>
        /// <remarks>
        /// The rules are:
        /// If the collection is defined as a concrete type with a publicly accessible parameterless constructor, then create an instance of that type
        /// Else, if HashSet<T> can be assigned to the type, then use HashSet<T>
        /// Else, if List<T> can be assigned to the type, then use List<T>
        /// Else, throw a nice exception.
        /// </remarks>
        /// <param name="requestedType">The type of collection that was requested</param>
        /// <returns>The type to instantiate, or null if we cannot find a supported type to instantiate</returns>
        internal static Type DetermineCollectionType(Type requestedType)
        {
            const BindingFlags constructorBinding = BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance;

            var elementType = GetCollectionElementType(requestedType);

            if (requestedType.IsArray)
            {
                throw new InvalidOperationException(Strings.ObjectQuery_UnableToMaterializeArray(
                    requestedType, typeof(List<>).MakeGenericType(elementType)));
            }

            if (!requestedType.IsAbstract
                &&
                requestedType.GetConstructor(constructorBinding, null, Type.EmptyTypes, null) != null)
            {
                return requestedType;
            }

            var hashSetOfT = typeof(HashSet<>).MakeGenericType(elementType);
            if (requestedType.IsAssignableFrom(hashSetOfT))
            {
                return hashSetOfT;
            }

            var listOfT = typeof(List<>).MakeGenericType(elementType);
            if (requestedType.IsAssignableFrom(listOfT))
            {
                return listOfT;
            }

            return null;
        }

        /// <summary>
        /// Returns the Type object that should be used to identify the type in the o-space
        /// metadata.  This is normally just the type that is passed in, but if the type
        /// is a proxy that we have generated, then its base type is returned instead.
        /// This ensures that both proxy entities and normal entities are treated as the
        /// same kind of entity in the metadata and places where the metadata is used.
        /// </summary>
        internal static Type GetEntityIdentityType(Type entityType)
        {
            return EntityProxyFactory.IsProxyType(entityType) ? entityType.BaseType : entityType;
        }

        /// <summary>
        /// Provides a standard helper method for quoting identifiers
        /// </summary>
        /// <param name="identifier">Identifier to be quoted. Does not validate that this identifier is valid.</param>
        /// <returns>Quoted string</returns>
        internal static string QuoteIdentifier(string identifier)
        {
            Debug.Assert(identifier != null, "identifier should not be null");
            return "[" + identifier.Replace("]", "]]") + "]";
        }

        #region Metadata Errors

        internal static MetadataException InvalidSchemaEncountered(string errors)
        {
            // EntityRes.GetString implementation truncates the string arguments to a max length of 1024. 
            // Since csdl, ssdl, providermanifest can have bunch of errors in them and we want to
            // show all of them, we are using String.Format to form the error message.
            // Using CurrentCulture since that's what EntityRes.GetString uses.
            return new MetadataException(String.Format(CultureInfo.CurrentCulture, EntityRes.GetString(EntityRes.InvalidSchemaEncountered), errors));
        }

        #endregion //Metadata Errors

        #region Internal Errors

        // Internal error code to use with the InternalError exception.
        //
        // error numbers end up being hard coded in test cases; they can be removed, but should not be changed.
        // reusing error numbers is probably OK, but not recommended.
        //
        // The acceptable range for this enum is
        // 1000 - 1999
        //
        // The Range 10,000-15,000 is reserved for tools
        //
        /// You must never renumber these, because we rely upon them when
        /// we get an exception report once we release the bits.
        internal enum InternalErrorCode
        {
            WrongNumberOfKeys = 1000,
            UnknownColumnMapKind = 1001,
            NestOverNest = 1002,
            ColumnCountMismatch = 1003,

            /// <summary>
            /// Some assertion failed
            /// </summary>
            AssertionFailed = 1004,

            UnknownVar = 1005,
            WrongVarType = 1006,
            ExtentWithoutEntity = 1007,
            UnnestWithoutInput = 1008,
            UnnestMultipleCollections = 1009,
            CodeGen_NoSuchProperty = 1011,
            JoinOverSingleStreamNest = 1012,
            InvalidInternalTree = 1013,
            NameValuePairNext = 1014,
            InvalidParserState1 = 1015,
            InvalidParserState2 = 1016,

            /// <summary>
            /// Thrown when SQL gen produces parameters for anything other than a 
            /// modification command tree.
            /// </summary>
            SqlGenParametersNotPermitted = 1017,
            EntityKeyMissingKeyValue = 1018,

            /// <summary>
            /// Thrown when an invalid data request is presented to a PropagatorResult in
            /// the update pipeline (confusing simple/complex values, missing key values, etc.).
            /// </summary>
            UpdatePipelineResultRequestInvalid = 1019,
            InvalidStateEntry = 1020,

            /// <summary>
            /// Thrown when the update pipeline encounters an invalid PrimitiveTypeKind
            /// during a cast.
            /// </summary>
            InvalidPrimitiveTypeKind = 1021,

            /// <summary>
            /// Thrown when an unknown node type is encountered in ELinq expression translation.
            /// </summary>
            UnknownLinqNodeType = 1023,

            /// <summary>
            /// Thrown by result assembly upon encountering a collection column that does not use any columns
            /// nor has a descriminated nested collection.
            /// </summary>
            CollectionWithNoColumns = 1024,

            /// <summary>
            /// Thrown when a lambda expression argument has an unexpected node type.
            /// </summary>
            UnexpectedLinqLambdaExpressionFormat = 1025,

            /// <summary>
            /// Thrown when a CommandTree is defined on a stored procedure EntityCommand instance.
            /// </summary>
            CommandTreeOnStoredProcedureEntityCommand = 1026,

            /// <summary>
            /// Thrown when an operation in the BoolExpr library is exceeding anticipated complexity.
            /// </summary>
            BoolExprAssert = 1027,
            // AttemptToGenerateDefinitionForFunctionWithoutDef = 1028,
            /// <summary>
            /// Thrown when type A is promotable to type B, but ranking algorithm fails to rank the promotion.
            /// </summary>
            FailedToGeneratePromotionRank = 1029,
        }

        internal static Exception InternalError(InternalErrorCode internalError, int location, object additionalInfo)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0}, {1}", (int)internalError, location);
            if (null != additionalInfo)
            {
                sb.AppendFormat(", {0}", additionalInfo);
            }
            return new InvalidOperationException(Strings.ADP_InternalProviderError(sb.ToString()));
        }

        #endregion

        #region ObjectStateManager errors

        internal static void CheckValidStateForChangeEntityState(EntityState state)
        {
            switch (state)
            {
                case EntityState.Added:
                case EntityState.Unchanged:
                case EntityState.Modified:
                case EntityState.Deleted:
                case EntityState.Detached:
                    break;
                default:
                    throw new ArgumentException(Strings.ObjectContext_InvalidEntityState, "state");
            }
        }

        internal static void CheckValidStateForChangeRelationshipState(EntityState state, string paramName)
        {
            switch (state)
            {
                case EntityState.Added:
                case EntityState.Unchanged:
                case EntityState.Deleted:
                case EntityState.Detached:
                    break;
                default:
                    throw new ArgumentException(Strings.ObjectContext_InvalidRelationshipState, paramName);
            }
        }

        #endregion

        #region ObjectMaterializer errors

        internal static void ThrowPropertyIsNotNullable(string propertyName)
        {
            if (String.IsNullOrEmpty(propertyName))
            {
                throw new ConstraintException(Strings.Materializer_PropertyIsNotNullable);
            }
            else
            {
                throw new PropertyConstraintException(Strings.Materializer_PropertyIsNotNullableWithName(propertyName), propertyName);
            }
        }

        internal static void ThrowSetInvalidValue(object value, Type destinationType, string className, string propertyName)
        {
            if (null == value)
            {
                throw new ConstraintException(Strings.Materializer_SetInvalidValue(
                    (Nullable.GetUnderlyingType(destinationType) ?? destinationType).Name,
                    className, propertyName, "null"));
            }
            else
            {
                throw new InvalidOperationException(Strings.Materializer_SetInvalidValue(
                    (Nullable.GetUnderlyingType(destinationType) ?? destinationType).Name,
                    className, propertyName, value.GetType().Name));
            }
        }

        internal static InvalidOperationException ValueInvalidCast(Type valueType, Type destinationType)
        {
            Debug.Assert(null != valueType, "null valueType");
            Debug.Assert(null != destinationType, "null destinationType");
            if (destinationType.IsValueType && destinationType.IsGenericType
                && (typeof(Nullable<>) == destinationType.GetGenericTypeDefinition()))
            {
                return new InvalidOperationException(Strings.Materializer_InvalidCastNullable(
                    valueType, destinationType.GetGenericArguments()[0]));
            }
            else
            {
                return new InvalidOperationException(Strings.Materializer_InvalidCastReference(
                    valueType, destinationType));
            }
        }

        #endregion

        #region ObjectView errors

        #endregion

        #region EntityCollection Errors

        internal static void CheckArgumentMergeOption(MergeOption mergeOption)
        {
            switch (mergeOption)
            {
                case MergeOption.NoTracking:
                case MergeOption.AppendOnly:
                case MergeOption.OverwriteChanges:
                case MergeOption.PreserveChanges:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(typeof(MergeOption).Name, Strings.ADP_InvalidEnumerationValue(typeof(MergeOption).Name, ((int)mergeOption).ToString(CultureInfo.InvariantCulture)));
            }
        }

        internal static void CheckArgumentRefreshMode(RefreshMode refreshMode)
        {
            if (refreshMode != RefreshMode.ClientWins && refreshMode != RefreshMode.StoreWins)
            {
                throw new ArgumentOutOfRangeException(typeof(RefreshMode).Name, Strings.ADP_InvalidEnumerationValue(typeof(RefreshMode).Name, ((int)refreshMode).ToString(CultureInfo.InvariantCulture)));
            }
        }

        #endregion

        #region ObjectContext errors

        internal static InvalidOperationException ExecuteFunctionCalledWithNonReaderFunction(EdmFunction functionImport)
        {
            // report ExecuteNonQuery return type if no explicit return type is given
            string message;
            if (null == functionImport.ReturnParameter)
            {
                message = Strings.ObjectContext_ExecuteFunctionCalledWithNonQueryFunction(
                    functionImport.Name);
            }
            else
            {
                message = Strings.ObjectContext_ExecuteFunctionCalledWithScalarFunction(
                    functionImport.ReturnParameter.TypeUsage.EdmType.FullName, functionImport.Name);
            }
            return new InvalidOperationException(message);
        }

        #endregion

        #region Complex Types Errors

        // Complex types exceptions

        #endregion

        internal static void ValidateEntitySetInKey(EntityKey key, EntitySet entitySet)
        {
            ValidateEntitySetInKey(key, entitySet, null);
        }

        internal static void ValidateEntitySetInKey(EntityKey key, EntitySet entitySet, string argument)
        {
            Debug.Assert(null != (object)key, "Null entity key");
            Debug.Assert(null != entitySet, "Null entity set");
            Debug.Assert(null != entitySet.EntityContainer, "Null entity container in the entity set");

            var containerName1 = key.EntityContainerName;
            var setName1 = key.EntitySetName;
            var containerName2 = entitySet.EntityContainer.Name;
            var setName2 = entitySet.Name;

            if (!StringComparer.Ordinal.Equals(containerName1, containerName2)
                ||
                !StringComparer.Ordinal.Equals(setName1, setName2))
            {
                if (String.IsNullOrEmpty(argument))
                {
                    throw new InvalidOperationException(Strings.ObjectContext_InvalidEntitySetInKey(containerName1, setName1, containerName2, setName2));
                }
                throw new InvalidOperationException(Strings.ObjectContext_InvalidEntitySetInKeyFromName(
                    containerName1, setName1, containerName2, setName2, argument));
            }
        }

        internal static void ValidateNecessaryModificationFunctionMapping(
            StorageModificationFunctionMapping mapping, string currentState,
            IEntityStateEntry stateEntry, string type, string typeName)
        {
            if (null == mapping)
            {
                throw new UpdateException(
                    Strings.Update_MissingFunctionMapping(currentState, type, typeName),
                    null,
                    new List<IEntityStateEntry>
                        {
                            stateEntry
                        }.Cast<ObjectStateEntry>().Distinct());
            }
        }

        internal static UpdateException Update(string message, Exception innerException, params IEntityStateEntry[] stateEntries)
        {
            return new UpdateException(message, innerException, stateEntries.Cast<ObjectStateEntry>().Distinct());
        }

        internal static UpdateException UpdateRelationshipCardinalityConstraintViolation(
            string relationshipSetName,
            int minimumCount, int? maximumCount, string entitySetName, int actualCount, string otherEndPluralName,
            IEntityStateEntry stateEntry)
        {
            var minimumCountString = ConvertCardinalityToString(minimumCount);
            var maximumCountString = ConvertCardinalityToString(maximumCount);
            var actualCountString = ConvertCardinalityToString(actualCount);
            if (minimumCount == 1
                && (minimumCountString == maximumCountString))
            {
                // Just one acceptable value and itis value is 1
                return Update(
                    Strings.Update_RelationshipCardinalityConstraintViolationSingleValue(
                        entitySetName, relationshipSetName, actualCountString, otherEndPluralName,
                        minimumCountString), null, stateEntry);
            }
            // Range of acceptable values
            return Update(
                Strings.Update_RelationshipCardinalityConstraintViolation(
                    entitySetName, relationshipSetName, actualCountString, otherEndPluralName,
                    minimumCountString, maximumCountString), null, stateEntry);
        }

        private static string ConvertCardinalityToString(int? cardinality)
        {
            return !cardinality.HasValue ? "*" : cardinality.Value.ToString(CultureInfo.CurrentCulture);
        }

        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        //
        // Helper Functions
        //

        internal static T CheckArgumentOutOfRange<T>(T[] values, int index, string parameterName)
        {
            Debug.Assert(null != values, "null values"); // use a different method if values can be null
            if (unchecked((uint)values.Length <= (uint)index))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
            return values[index];
        }

        internal static T CheckArgumentNull<T>(T value, string parameterName) where T : class
        {
            if (null == value)
            {
                throw new ArgumentNullException(parameterName);
            }
            return value;
        }

        internal static IEnumerable<T> CheckArgumentContainsNull<T>(ref IEnumerable<T> enumerableArgument, string argumentName)
            where T : class
        {
            GetCheapestSafeEnumerableAsCollection(ref enumerableArgument);
            foreach (var item in enumerableArgument)
            {
                if (item == null)
                {
                    throw new ArgumentException(Strings.CheckArgumentContainsNullFailed(argumentName));
                }
            }
            return enumerableArgument;
        }

        internal static IEnumerable<T> CheckArgumentEmpty<T>(
            ref IEnumerable<T> enumerableArgument, Func<string, string> errorMessage, string argumentName)
        {
            int count;
            GetCheapestSafeCountOfEnumerable(ref enumerableArgument, out count);
            if (count <= 0)
            {
                throw new ArgumentException(errorMessage(argumentName));
            }
            return enumerableArgument;
        }

        private static void GetCheapestSafeCountOfEnumerable<T>(ref IEnumerable<T> enumerable, out int count)
        {
            var collection = GetCheapestSafeEnumerableAsCollection(ref enumerable);
            count = collection.Count;
        }

        private static ICollection<T> GetCheapestSafeEnumerableAsCollection<T>(ref IEnumerable<T> enumerable)
        {
            var collection = enumerable as ICollection<T>;
            if (collection != null)
            {
                // cheap way
                return collection;
            }

            // expensive way, but we don't know if the enumeration is rewindable so...
            enumerable = new List<T>(enumerable);
            return enumerable as ICollection<T>;
        }

        internal static T GenericCheckArgumentNull<T>(T value, string parameterName) where T : class
        {
            return CheckArgumentNull(value, parameterName);
        }

        // EntityConnectionStringBuilder

        // Invalid Enumeration

        /// <summary>
        /// Given a provider factory, this returns the provider invariant name for the provider. 
        /// </summary>
        internal static bool TryGetProviderInvariantName(DbProviderFactory providerFactory, out string invariantName)
        {
            Debug.Assert(providerFactory != null);

            var providerFactoryType = providerFactory.GetType();
            var providerFactoryAssemblyName = new AssemblyName(providerFactoryType.Assembly.FullName);

            var infoTable = DbProviderFactories.GetFactoryClasses();

            Debug.Assert(infoTable.Rows != null);

            foreach (DataRow infoRow in infoTable.Rows)
            {
                var infoRowAssemblyQualifiedTypeName = infoRow[AssemblyQualifiedNameIndex] as string;

                if (string.IsNullOrWhiteSpace(infoRowAssemblyQualifiedTypeName))
                {
                    continue;
                }

                var firstCommaIndex = infoRowAssemblyQualifiedTypeName.IndexOf(',');

                if (firstCommaIndex < 0)
                {
                    continue;
                }

                var infoRowProviderFactoryTypeFullName = infoRowAssemblyQualifiedTypeName.Substring(0, firstCommaIndex).Trim();

                // Match the provider type name
                if (!string.Equals(infoRowProviderFactoryTypeFullName, providerFactoryType.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var infoRowProviderAssemblyName = infoRowAssemblyQualifiedTypeName.Substring(firstCommaIndex + 1).Trim();
                if (AssemblyNamesMatch(infoRowProviderAssemblyName, providerFactoryAssemblyName))
                {
                    invariantName = (string)infoRow[InvariantNameIndex];
                    return true;
                }
            }

            invariantName = null;
            return false;
        }

        private static bool AssemblyNamesMatch(string infoRowProviderAssemblyName, AssemblyName targetAssemblyName)
        {
            if (string.IsNullOrWhiteSpace(infoRowProviderAssemblyName))
            {
                return false;
            }

            AssemblyName assemblyName;
            try
            {
                assemblyName = new AssemblyName(infoRowProviderAssemblyName);
            }
            catch (Exception e)
            {
                // Ignore broken provider entries
                if (!IsCatchableExceptionType(e))
                {
                    throw;
                }
                return false;
            }

            // Match the provider assembly details
            if (!string.Equals(targetAssemblyName.Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (targetAssemblyName.Version == null
                || assemblyName.Version == null)
            {
                return false;
            }

            if (targetAssemblyName.Version.Major != assemblyName.Version.Major
                || targetAssemblyName.Version.Minor != assemblyName.Version.Minor)
            {
                return false;
            }

            var targetPublicKeyToken = targetAssemblyName.GetPublicKeyToken();
            return targetPublicKeyToken != null && targetPublicKeyToken.SequenceEqual(assemblyName.GetPublicKeyToken());
        }

        // Invalid string argument
        internal static void CheckStringArgument(string value, string parameterName)
        {
            // Throw ArgumentNullException when string is null
            CheckArgumentNull(value, parameterName);

            // Throw ArgumentException when string is empty
            if (value.Length == 0)
            {
                throw new ArgumentException(Strings.InvalidStringArgument(parameterName), parameterName);
            }
        }

        internal static bool IsCatchableExceptionType(Exception e)
        {
            // a 'catchable' exception is defined by what it is not.
            Debug.Assert(e != null, "Unexpected null exception!");
            var type = e.GetType();

            return ((type != typeof(StackOverflowException)) &&
                    (type != typeof(OutOfMemoryException)) &&
                    (type != typeof(ThreadAbortException)) &&
                    (type != typeof(NullReferenceException)) &&
                    (type != typeof(AccessViolationException)) &&
                    !typeof(SecurityException).IsAssignableFrom(type));
        }

        internal static bool IsCatchableEntityExceptionType(Exception e)
        {
            Debug.Assert(e != null, "Unexpected null exception!");
            var type = e.GetType();

            return IsCatchableExceptionType(e) &&
                   type != typeof(EntityCommandExecutionException) &&
                   type != typeof(EntityCommandCompilationException) &&
                   type != typeof(EntitySqlException);
        }

        internal static bool IsNull(object value)
        {
            if ((null == value) || (DBNull.Value == value))
            {
                return true;
            }
            var nullable = (value as INullable);
            return ((null != nullable) && nullable.IsNull);
        }

        internal static PropertyInfo GetTopProperty(Type t, string propertyName)
        {
            return GetTopProperty(ref t, propertyName);
        }

        /// <summary>
        /// Returns the PropertyInfo and Type where a given property is defined
        /// This is done by traversing the type hierarchy to find the type match.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        internal static PropertyInfo GetTopProperty(ref Type t, string propertyName)
        {
            PropertyInfo propertyInfo = null;
            while (propertyInfo == null
                   && t != null)
            {
                propertyInfo = t.GetProperty(
                    propertyName, BindingFlags.Instance |
                                  BindingFlags.Public |
                                  BindingFlags.NonPublic |
                                  BindingFlags.DeclaredOnly);
                t = t.BaseType;
            }
            t = propertyInfo.DeclaringType;
            return propertyInfo;
        }

        internal static int SrcCompare(string strA, string strB)
        {
            return ((strA == strB) ? 0 : 1);
        }

        internal static int DstCompare(string strA, string strB)
        {
            return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, StringCompareOptions);
        }

        internal static Dictionary<string, string> COMPILER_VERSION = new Dictionary<string, string>
                                                                          {
                                                                              { "CompilerVersion", "V3.5" }
                                                                          }; //v3.5 required for compiling model files with partial methods.

        [FileIOPermission(SecurityAction.Assert, AllFiles = FileIOPermissionAccess.PathDiscovery)]
        [SecuritySafeCritical]
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static string GetFullPath(string filename)
        {
            // MDAC 77686
            return Path.GetFullPath(filename);
        }

        public static Type[] GetTypesSpecial(Assembly assembly)
        {
            // TODO: SDE Merge - Check if perf issue that required this code is still needed
            return ReferenceEquals(assembly, typeof(ObjectContext).Assembly)
#pragma warning disable 612,618
                       ? new[] { typeof(HistoryRow), typeof(EdmMetadata) }
#pragma warning restore 612,618
                       : assembly.GetTypes();
        }
    }
}
