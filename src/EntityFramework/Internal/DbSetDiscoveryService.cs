// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Service used to search for instance properties on a DbContext class that can
    /// be assigned a DbSet instance.  Also, if the the property has a public setter,
    /// then a delegate is compiled to set the property to a new instance of DbSet.
    /// All of this information is cached per app domain.
    /// </summary>
    internal class DbSetDiscoveryService
    {
        #region Fields and constructors

        // AppDomain cache collection initializers for a known type.
        private static readonly ConcurrentDictionary<Type, DbContextTypesInitializersPair> _objectSetInitializers =
            new ConcurrentDictionary<Type, DbContextTypesInitializersPair>();

        // Used by the code below to create DbSet instances
        private static readonly MethodInfo _setMethod = typeof(DbContext).GetMethod("Set", Type.EmptyTypes);

        private readonly DbContext _context;

        /// <summary>
        /// Creates a set discovery service for the given derived context.
        /// </summary>
        public DbSetDiscoveryService(DbContext context)
        {
            DebugCheck.NotNull(context);

            _context = context;
        }

        #endregion

        #region Set discovery/processing

        /// <summary>
        /// Processes the given context type to determine the DbSet or IDbSet
        /// properties and collect root entity types from those properties.  Also, delegates are
        /// created to initialize any of these properties that have public setters.
        /// If the type has been processed previously in the app domain, then all this information
        /// is returned from a cache.
        /// </summary>
        /// <returns> A dictionary of potential entity type to the list of the names of the properties that used the type. </returns>
        private Dictionary<Type, List<string>> GetSets()
        {
            DbContextTypesInitializersPair setsInfo;
            if (!_objectSetInitializers.TryGetValue(_context.GetType(), out setsInfo))
            {
                // It is possible that multiple threads will enter this code and create the list
                // and the delegates.  However, the result will always be the same so we may, in
                // the rare cases in which this happens, do some work twice, but functionally the
                // outcome will be correct.

                var dbContextParam = Expression.Parameter(typeof(DbContext), "dbContext");
                var initDelegates = new List<Action<DbContext>>();

                var entityTypes = new Dictionary<Type, List<string>>();

                // Properties declared directly on DbContext such as Database are skipped
                const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                foreach (var propertyInfo in _context.GetType().GetProperties(bindingFlags)
                                                     .Where(
                                                         p => p.GetIndexParameters().Length == 0 &&
                                                              p.DeclaringType != typeof(DbContext))
                                                     .OrderBy(p => p.Name))
                {
                    var entityType = GetSetType(propertyInfo.PropertyType);
                    if (entityType != null)
                    {
                        // We validate immediately because a DbSet/IDbSet must be of
                        // a valid entity type since otherwise you could never use an instance.
                        if (!entityType.IsValidStructuralType())
                        {
                            throw Error.InvalidEntityType(entityType);
                        }

                        List<string> properties;
                        if (!entityTypes.TryGetValue(entityType, out properties))
                        {
                            properties = new List<string>();
                            entityTypes[entityType] = properties;
                        }
                        properties.Add(propertyInfo.Name);

                        if (DbSetPropertyShouldBeInitialized(propertyInfo))
                        {
                            var setter = propertyInfo.GetSetMethod(nonPublic: false);
                            if (setter != null)
                            {
                                var setMethod = _setMethod.MakeGenericMethod(entityType);

                                var newExpression = Expression.Call(dbContextParam, setMethod);
                                var setExpression = Expression.Call(
                                    Expression.Convert(dbContextParam, _context.GetType()), setter, newExpression);
                                initDelegates.Add(
                                    Expression.Lambda<Action<DbContext>>(setExpression, dbContextParam).Compile());
                            }
                        }
                    }
                }

                Action<DbContext> initializer = dbContext =>
                    {
                        foreach (var initer in initDelegates)
                        {
                            initer(dbContext);
                        }
                    };

                setsInfo = new DbContextTypesInitializersPair(entityTypes, initializer);

                // If TryAdd fails it just means some other thread got here first, which is okay
                // since the end result is the same info anyway.
                _objectSetInitializers.TryAdd(_context.GetType(), setsInfo);
            }
            return setsInfo.EntityTypeToPropertyNameMap;
        }

        /// <summary>
        /// Calls the public setter on any property found to initialize it to a new instance of DbSet.
        /// </summary>
        public void InitializeSets()
        {
            GetSets(); // Ensures sets have been discovered
            _objectSetInitializers[_context.GetType()].SetsInitializer(_context);
        }

        /// <summary>
        /// Registers the entities and their entity set name hints with the given <see cref="DbModelBuilder" />.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        public void RegisterSets(DbModelBuilder modelBuilder)
        {
            foreach (var set in GetSets())
            {
                if (set.Value.Count > 1)
                {
                    throw Error.Mapping_MESTNotSupported(set.Value[0], set.Value[1], set.Key);
                }

                modelBuilder.Entity(set.Key).EntitySetName = set.Value[0];
            }
        }

        /// <summary>
        /// Returns false if SuppressDbSetInitializationAttribute is found on the property or the class, otherwise
        /// returns true.
        /// </summary>
        private static bool DbSetPropertyShouldBeInitialized(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttributes(typeof(SuppressDbSetInitializationAttribute), inherit: false).Length
                   == 0 &&
                   propertyInfo.DeclaringType.GetCustomAttributes(
                       typeof(SuppressDbSetInitializationAttribute), inherit: false).Length == 0;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Determines whether or not an instance of DbSet/ObjectSet can be assigned to a property of the given type.
        /// </summary>
        /// <param name="declaredType"> The type to check. </param>
        /// <returns> The entity type of the DbSet/ObjectSet that can be assigned, or null if no set type can be assigned. </returns>
        private static Type GetSetType(Type declaredType)
        {
            if (!declaredType.IsArray)
            {
                var entityType = GetSetElementType(declaredType);
                if (entityType != null)
                {
                    var setOfT = typeof(DbSet<>).MakeGenericType(entityType);
                    if (declaredType.IsAssignableFrom(setOfT))
                    {
                        return entityType;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Given a type that might be an IDbSet\IObjectSet, determine if the type implements IDbSet&lt;&gt;\IObjectSet&lt;&gt;, and if
        /// so return the element type of the IDbSet\IObjectSet.  Currently, if the collection implements IDbSet&lt;&gt;\IObjectSet&lt;&gt;
        /// multiple times with different types, then we will return false since this is not supported.
        /// </summary>
        /// <param name="setType"> The type to check. </param>
        /// <returns> The element type of the IDbSet\IObjectSet, or null if the type does not match. </returns>
        private static Type GetSetElementType(Type setType)
        {
            // We have to check if the type actually is the interface, or if it implements the interface:
            try
            {
                var setInterface =
                    (setType.IsGenericType && typeof(IDbSet<>).IsAssignableFrom(setType.GetGenericTypeDefinition()))
                        ? setType
                        : setType.GetInterface(typeof(IDbSet<>).FullName);

                // We need to make sure the type is fully specified otherwise we won't be able to add element to it.
                if (setInterface != null
                    && !setInterface.ContainsGenericParameters)
                {
                    return setInterface.GetGenericArguments()[0];
                }
            }
            catch (AmbiguousMatchException)
            {
                // Thrown if collection type implements IDbSet or IObjectSet<> more than once
            }
            return null;
        }

        #endregion
    }
}
