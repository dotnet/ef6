// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ExtraLazyLoading
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Functionals.Utilities;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Reflection;

    public abstract class CachingCollectionInitializer
    {
        private static readonly ConcurrentDictionary<Type, IList<Tuple<string, Func<DbCollectionEntry, object>>>> _factories
            = new ConcurrentDictionary<Type, IList<Tuple<string, Func<DbCollectionEntry, object>>>>();

        private static readonly MethodInfo _factoryMethodInfo
            = typeof(CachingCollectionInitializer).GetOnlyDeclaredMethod("CreateCollection");

        public virtual Type TryGetElementType(PropertyInfo collectionProperty)
        {
            // We can only replace properties that are declared as ICollection<T> and have a setter.
            var propertyType = collectionProperty.PropertyType;
            if (propertyType.IsGenericType()
                && propertyType.GetGenericTypeDefinition() == typeof(ICollection<>)
                && collectionProperty.Setter() != null)
            {
                return propertyType.GetGenericArguments().Single();
            }
            return null;
        }

        public virtual void InitializeCollections(DbContext context, object entity)
        {
            var factories = _factories.GetOrAdd(
                entity.GetType(),
                t =>
                {
                    var currentDetectChanges = context.Configuration.AutoDetectChangesEnabled;
                    try
                    {
                        context.Configuration.AutoDetectChangesEnabled = false;

                        var list = new List<Tuple<string, Func<DbCollectionEntry, object>>>();

                        foreach (var property in t.GetRuntimeProperties().Where(p => p.IsPublic()))
                        {
                            var collectionEntry = context.Entry(entity).Member(property.Name) as DbCollectionEntry;

                            if (collectionEntry != null)
                            {
                                var elementType = TryGetElementType(property);
                                if (elementType != null)
                                {
                                    list.Add(Tuple.Create(property.Name, CreateCollectionFactory(elementType)));
                                }
                            }
                        }

                        return list;
                    }
                    finally
                    {
                        context.Configuration.AutoDetectChangesEnabled = currentDetectChanges;
                    }
                });

            foreach (var factory in factories)
            {
                var collectionEntry = context.Entry(entity).Collection(factory.Item1);
                collectionEntry.CurrentValue = factory.Item2(collectionEntry);
            }
        }

        public virtual Func<DbCollectionEntry, object> CreateCollectionFactory(Type elementType)
        {
            return (Func<DbCollectionEntry, object>)Delegate.CreateDelegate(
                typeof(Func<DbCollectionEntry, object>), this,
                _factoryMethodInfo.MakeGenericMethod(elementType));
        }

        public abstract object CreateCollection<TElement>(DbCollectionEntry collectionEntry);
    }
}
