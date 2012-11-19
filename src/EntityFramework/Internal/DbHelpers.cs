// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Data.Entity.Validation;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    ///     Static helper methods only.
    /// </summary>
    internal static class DbHelpers
    {
        #region Null/empty/type checking

        /// <summary>
        ///     Checks whether the given value is null and throws ArgumentNullException if it is.
        ///     This method should only be used in places where Code Contracts are compiled out in the
        ///     release build but we still need public surface null-checking, such as where a public
        ///     abstract class is implemented by an internal concrete class.
        /// </summary>
        public static void ThrowIfNull<T>(T value, string parameterName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        /// <summary>
        ///     Checks whether the given string is null, empty, or just whitespace, and throws appropriately
        ///     if the check fails.
        ///     This method should only be used in places where Code Contracts are compiled out in the
        ///     release build but we still need public surface checking, such as where a public
        ///     abstract class is implemented by an internal concrete class.
        /// </summary>
        public static void ThrowIfNullOrWhitespace(string value, string parameterName)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                throw Error.ArgumentIsNullOrWhitespace(parameterName);
            }
        }

        #endregion

        #region Binary key values

        /// <summary>
        ///     Given two key values that may or may not be byte arrays, this method determines
        ///     whether or not they are equal.  For non-binary key values, this is equivalent
        ///     to Object.Equals.  For binary keys, it is by comparison of every byte in the
        ///     arrays.
        /// </summary>
        public static bool KeyValuesEqual(object x, object y)
        {
            if (x is DBNull)
            {
                x = null;
            }

            if (y is DBNull)
            {
                y = null;
            }

            if (Equals(x, y))
            {
                return true;
            }

            var xBytes = x as byte[];
            var yBytes = y as byte[];
            if (xBytes == null
                || yBytes == null
                || xBytes.Length != yBytes.Length)
            {
                return false;
            }

            for (var i = 0; i < xBytes.Length; i++)
            {
                if (xBytes[i]
                    != yBytes[i])
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Identifier quoting

        /// <summary>
        ///     Provides a standard helper method for quoting identifiers
        /// </summary>
        /// <param name="identifier"> Identifier to be quoted. Does not validate that this identifier is valid. </param>
        /// <returns> Quoted string </returns>
        public static string QuoteIdentifier(string identifier)
        {
            DebugCheck.NotNull(identifier);

            return "[" + identifier.Replace("]", "]]") + "]";
        }

        #endregion

        #region Connection string detection

        /// <summary>
        ///     Checks the given string which might be a database name or a connection string and determines
        ///     whether it should be treated as a name or connection string.  Currently, the test is simply
        ///     whether or not the string contains an '=' character--if it does, then it should be treated
        ///     as a connection string.
        /// </summary>
        /// <param name="nameOrConnectionString"> The name or connection string. </param>
        /// <returns>
        ///     <c>true</c> if the string should be treated as a connection string; <c>false</c> if it should be treated as a name.
        /// </returns>
        public static bool TreatAsConnectionString(string nameOrConnectionString)
        {
            DebugCheck.NotNull(nameOrConnectionString);

            return nameOrConnectionString.IndexOf('=') >= 0;
        }

        /// <summary>
        ///     Determines whether the given string should be treated as a database name directly (it contains no '='),
        ///     is in the form name=foo, or is some other connection string.  If it is a direct name or has name=, then
        ///     the name is extracted and the method returns true.
        /// </summary>
        /// <param name="nameOrConnectionString"> The name or connection string. </param>
        /// <param name="name"> The name. </param>
        /// <returns> True if a name is found; false otherwise. </returns>
        public static bool TryGetConnectionName(string nameOrConnectionString, out string name)
        {
            DebugCheck.NotNull(nameOrConnectionString);

            // No '=' at all means just treat the whole string as a name
            var firstEquals = nameOrConnectionString.IndexOf('=');
            if (firstEquals < 0)
            {
                name = nameOrConnectionString;
                return true;
            }

            // More than one equals means treat the whole thing as a connection string
            if (nameOrConnectionString.IndexOf('=', firstEquals + 1) >= 0)
            {
                name = null;
                return false;
            }

            // If the keyword before the single '=' is "name" then return the name value
            if (nameOrConnectionString.Substring(0, firstEquals).Trim().Equals(
                "name", StringComparison.OrdinalIgnoreCase))
            {
                name = nameOrConnectionString.Substring(firstEquals + 1).Trim();
                return true;
            }

            // Otherwise it is just a connection string.
            name = null;
            return false;
        }

        /// <summary>
        ///     Determines whether the given string is a full EF connection string with provider, provider connection string,
        ///     and metadata parts, or is is instead some other form of connection string.
        /// </summary>
        /// <param name="nameOrConnectionString"> The name or connection string. </param>
        /// <returns>
        ///     <c>true</c> if the given string is an EF connection string; otherwise, <c>false</c> .
        /// </returns>
        public static bool IsFullEFConnectionString(string nameOrConnectionString)
        {
            DebugCheck.NotNull(nameOrConnectionString);

            var tokens = nameOrConnectionString.ToUpperInvariant().Split('=', ';').Select(t => t.Trim());
            return tokens.Contains("PROVIDER") && tokens.Contains("PROVIDER CONNECTION STRING")
                   && tokens.Contains("METADATA");
        }

        #endregion

        #region Parsing selector expressions

        /// <summary>
        ///     Parses a property selector expression used for the expression-based versions of the Property, Collection, Reference,
        ///     etc methods on <see cref="System.Data.Entity.Infrastructure.DbEntityEntry" /> and
        ///     <see cref="System.Data.Entity.Infrastructure.DbEntityEntry{T}" /> classes.
        /// </summary>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <typeparam name="TProperty"> The type of the property. </typeparam>
        /// <param name="property"> The property. </param>
        /// <param name="methodName"> Name of the method. </param>
        /// <param name="paramName"> Name of the param. </param>
        /// <returns> The property name. </returns>
        public static string ParsePropertySelector<TEntity, TProperty>(
            Expression<Func<TEntity, TProperty>> property, string methodName, string paramName)
        {
            DebugCheck.NotNull(property);

            string path;
            if (!TryParsePath(property.Body, out path)
                || path == null)
            {
                throw new ArgumentException(
                    Strings.DbEntityEntry_BadPropertyExpression(methodName, typeof(TEntity).Name), paramName);
            }
            return path;
        }

        /// <summary>
        ///     Called recursively to parse an expression tree representing a property path such
        ///     as can be passed to Include or the Reference/Collection/Property methods of <see cref="InternalEntityEntry" />.
        ///     This involves parsing simple property accesses like o =&gt; o.Products as well as calls to Select like
        ///     o =&gt; o.Products.Select(p =&gt; p.OrderLines).
        /// </summary>
        /// <param name="expression"> The expression to parse. </param>
        /// <param name="path"> The expression parsed into an include path, or null if the expression did not match. </param>
        /// <returns> True if matching succeeded; false if the expression could not be parsed. </returns>
        public static bool TryParsePath(Expression expression, out string path)
        {
            DebugCheck.NotNull(expression);

            path = null;
            var withoutConvert = expression.RemoveConvert(); // Removes boxing
            var memberExpression = withoutConvert as MemberExpression;
            var callExpression = withoutConvert as MethodCallExpression;

            if (memberExpression != null)
            {
                var thisPart = memberExpression.Member.Name;
                string parentPart;
                if (!TryParsePath(memberExpression.Expression, out parentPart))
                {
                    return false;
                }
                path = parentPart == null ? thisPart : (parentPart + "." + thisPart);
            }
            else if (callExpression != null)
            {
                if (callExpression.Method.Name == "Select"
                    && callExpression.Arguments.Count == 2)
                {
                    string parentPart;
                    if (!TryParsePath(callExpression.Arguments[0], out parentPart))
                    {
                        return false;
                    }
                    if (parentPart != null)
                    {
                        var subExpression = callExpression.Arguments[1] as LambdaExpression;
                        if (subExpression != null)
                        {
                            string thisPart;
                            if (!TryParsePath(subExpression.Body, out thisPart))
                            {
                                return false;
                            }
                            if (thisPart != null)
                            {
                                path = parentPart + "." + thisPart;
                                return true;
                            }
                        }
                    }
                }
                return false;
            }

            return true;
        }

        #endregion

        #region Compiled delegates for accessing property getters and setters

        private const BindingFlags PropertyBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly MethodInfo _convertAndSetMethod = typeof(DbHelpers).GetMethod(
            "ConvertAndSet", BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly ConcurrentDictionary<Type, IDictionary<string, Type>> _propertyTypes =
            new ConcurrentDictionary<Type, IDictionary<string, Type>>();

        private static readonly ConcurrentDictionary<Type, IDictionary<string, Action<object, object>>> _propertySetters
            =
            new ConcurrentDictionary<Type, IDictionary<string, Action<object, object>>>();

        private static readonly ConcurrentDictionary<Type, IDictionary<string, Func<object, object>>> _propertyGetters =
            new ConcurrentDictionary<Type, IDictionary<string, Func<object, object>>>();

        /// <summary>
        ///     Gets a cached dictionary mapping property names to property types for all the properties
        ///     in the given type.
        /// </summary>
        public static IDictionary<string, Type> GetPropertyTypes(Type type)
        {
            DebugCheck.NotNull(type);

            IDictionary<string, Type> types;
            if (!_propertyTypes.TryGetValue(type, out types))
            {
                var properties = type.GetProperties(PropertyBindingFlags).Where(p => p.GetIndexParameters().Length == 0);
                types = new Dictionary<string, Type>(properties.Count());
                foreach (var property in properties)
                {
                    types[property.Name] = property.PropertyType;
                }
                _propertyTypes.TryAdd(type, types);
            }
            return types;
        }

        /// <summary>
        ///     Gets a dictionary of compiled property setter delegates for the underlying types.
        ///     The dictionary is cached for the type in the app domain.
        /// </summary>
        public static IDictionary<string, Action<object, object>> GetPropertySetters(Type type)
        {
            DebugCheck.NotNull(type);

            IDictionary<string, Action<object, object>> setters;
            if (!_propertySetters.TryGetValue(type, out setters))
            {
                var properties = type.GetProperties(PropertyBindingFlags).Where(p => p.GetIndexParameters().Length == 0);
                setters = new Dictionary<string, Action<object, object>>(properties.Count());
                foreach (var property in properties)
                {
                    // Only create delegates for properties that are found and have a setter.
                    var setMethod = property.GetSetMethod(nonPublic: true);
                    if (setMethod != null)
                    {
                        // First create a dynamic delegate that will call the setter on the object instance.
                        // This does not access anything internal to us so it will only throw in partial trust
                        // if the caller doesn't have access to the actual property setter itself.
                        var valueParam = Expression.Parameter(typeof(object), "value");
                        var instanceParam = Expression.Parameter(typeof(object), "instance");
                        var setterExpression = Expression.Call(
                            Expression.Convert(instanceParam, type), setMethod,
                            Expression.Convert(valueParam, property.PropertyType));
                        var setter =
                            Expression.Lambda<Action<object, object>>(setterExpression, instanceParam, valueParam).
                                       Compile();

                        // Next create a delegate with CreateDelegate that calls the internal ConvertAndSet method below.
                        // This works in partial trust because it is using CreateDelegate to avoid creating any dynamic code.
                        var convertMethod = _convertAndSetMethod.MakeGenericMethod(property.PropertyType);
                        var convertAndSet = (Action<object, object, Action<object, object>, string, string>)
                                            Delegate.CreateDelegate(
                                                typeof(Action<object, object, Action<object, object>, string, string>),
                                                convertMethod);

                        // Finally create a closure around the ConvertAndSet call to pass in things specific to this property
                        // instance, including the actual dynamic setter delegate that we created above.
                        var propertyName = property.Name;
                        setters[property.Name] = (i, v) => convertAndSet(i, v, setter, propertyName, type.Name);
                    }
                }
                _propertySetters.TryAdd(type, setters);
            }
            return setters;
        }

        /// <summary>
        ///     Used by the property setter delegates to throw for attempts to set null onto
        ///     non-nullable properties or otherwise go ahead and set the property.
        /// </summary>
        private static void ConvertAndSet<T>(
            object instance, object value, Action<object, object> setter, string propertyName, string typeName)
        {
            if (value == null
                && typeof(T).IsValueType
                && Nullable.GetUnderlyingType(typeof(T)) == null)
            {
                throw Error.DbPropertyValues_CannotSetNullValue(propertyName, typeof(T).Name, typeName);
            }
            setter(instance, (T)value);
        }

        /// <summary>
        ///     Gets a dictionary of compiled property getter delegates for the underlying types.
        ///     The dictionary is cached for the type in the app domain.
        /// </summary>
        public static IDictionary<string, Func<object, object>> GetPropertyGetters(Type type)
        {
            DebugCheck.NotNull(type);

            IDictionary<string, Func<object, object>> getters;
            if (!_propertyGetters.TryGetValue(type, out getters))
            {
                var properties = type.GetProperties(PropertyBindingFlags).Where(p => p.GetIndexParameters().Length == 0);
                getters = new Dictionary<string, Func<object, object>>(properties.Count());
                foreach (var property in properties)
                {
                    var getMethod = property.GetGetMethod(nonPublic: true);
                    if (getMethod != null)
                    {
                        var instanceParam = Expression.Parameter(typeof(object), "instance");
                        var getterExpression = Expression.Convert(
                            Expression.Call(Expression.Convert(instanceParam, type), getMethod), typeof(object));
                        getters[property.Name] =
                            Expression.Lambda<Func<object, object>>(getterExpression, instanceParam).Compile();
                    }
                }
                _propertyGetters.TryAdd(type, getters);
            }
            return getters;
        }

        #endregion

        #region Creating NoTracking queries

        /// <summary>
        ///     Creates a new <see cref="ObjectQuery" /> with the NoTracking merge option applied.
        ///     The query object passed in is not changed.
        /// </summary>
        /// <param name="query"> The query. </param>
        /// <returns> A new query with NoTracking applied. </returns>
        public static IQueryable CreateNoTrackingQuery(ObjectQuery query)
        {
            DebugCheck.NotNull(query);

            var asIQueryable = (IQueryable)query;
            var newQuery = (ObjectQuery)asIQueryable.Provider.CreateQuery(asIQueryable.Expression);
            newQuery.MergeOption = MergeOption.NoTracking;
            return newQuery;
        }

        #endregion

        #region Splitting ValidationResult to multiple DbValidationErrors

        /// <summary>
        ///     Converts <see cref="IEnumerable{ValidationResult}" /> to <see cref="IEnumerable{DbValidationError}" />
        /// </summary>
        /// <param name="propertyName"> Name of the property being validated with ValidationAttributes. Null for type-level validation. </param>
        /// <param name="validationResults">
        ///     ValidationResults instances to be converted to <see cref="DbValidationError" /> instances.
        /// </param>
        /// <returns>
        ///     An <see cref="IEnumerable{DbValidationError}" /> created based on the <paramref name="validationResults" /> .
        /// </returns>
        /// <remarks>
        ///     <see cref="ValidationResult" /> class contains a property with names of properties the error applies to.
        ///     On the other hand each <see cref="DbValidationError" /> applies at most to a single property. As a result for
        ///     each name in ValidationResult.MemberNames one <see cref="DbValidationError" /> will be created (with some
        ///     exceptions for special cases like null or empty .MemberNames or null names in the .MemberNames).
        /// </remarks>
        public static IEnumerable<DbValidationError> SplitValidationResults(
            string propertyName, IEnumerable<ValidationResult> validationResults)
        {
            DebugCheck.NotNull(validationResults);

            foreach (var validationResult in validationResults)
            {
                if (validationResult == null)
                {
                    continue;
                }
                // let's treat null or empty .MemberNames the same way as one undefined (null) memberName
                var memberNames = validationResult.MemberNames == null || !validationResult.MemberNames.Any()
                                      ? new string[] { null }
                                      : validationResult.MemberNames;

                foreach (var memberName in memberNames)
                {
                    yield return new DbValidationError(memberName ?? propertyName, validationResult.ErrorMessage);
                }
            }
        }

        #endregion

        #region Calculating a dot separated "path" to a property

        /// <summary>
        ///     Calculates a "path" to a property. For primitive properties on an entity type it is just the
        ///     name of the property. Otherwise it is a dot separated list of names of the property and all
        ///     its ancestor properties starting from the entity.
        /// </summary>
        /// <param name="property"> Property for which to calculate the path. </param>
        /// <returns> Dot separated path to the property. </returns>
        public static string GetPropertyPath(InternalMemberEntry property)
        {
            DebugCheck.NotNull(property);

            return string.Join(".", GetPropertyPathSegments(property).Reverse());
        }

        /// <summary>
        ///     Gets names of the property and its ancestor properties as enumerable walking "bottom-up".
        /// </summary>
        /// <param name="property"> Property for which to get the segments. </param>
        /// <returns> Names of the property and its ancestor properties. </returns>
        private static IEnumerable<string> GetPropertyPathSegments(InternalMemberEntry property)
        {
            DebugCheck.NotNull(property);

            do
            {
                yield return property.Name;
                property = (property is InternalNestedPropertyEntry)
                               ? ((InternalNestedPropertyEntry)property).ParentPropertyEntry
                               : null;
            }
            while (property != null);
        }

        #endregion

        #region Collection types for element types

        private static readonly ConcurrentDictionary<Type, Type> _collectionTypes =
            new ConcurrentDictionary<Type, Type>();

        /// <summary>
        ///     Gets an <see cref="ICollection{T}" /> type for the given element type.
        /// </summary>
        /// <param name="elementType"> Type of the element. </param>
        /// <returns> The collection type. </returns>
        public static Type CollectionType(Type elementType)
        {
            return _collectionTypes.GetOrAdd(elementType, t => typeof(ICollection<>).MakeGenericType(t));
        }

        #endregion

        #region Creating a database name from a context name

        /// <summary>
        ///     Creates a database name given a type derived from DbContext.  This handles nested and
        ///     generic classes.  No attempt is made to ensure that the name is not too long since this
        ///     is provider specific.  If a too long name is generated then the provider will throw and
        ///     the user must correct by specifying their own name in the DbContext constructor.
        /// </summary>
        /// <param name="contextType"> Type of the context. </param>
        /// <returns> The database name to use. </returns>
        public static string DatabaseName(this Type contextType)
        {
            DebugCheck.NotNull(contextType);
            Debug.Assert(typeof(DbContext).IsAssignableFrom(contextType));

            // ToString seems to give us what we need.
            return contextType.ToString();
        }

        #endregion
    }
}
