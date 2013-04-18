// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Entity.Internal.Linq;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    ///     A set of extension methods for <see cref="IDbSet{TEntity}" />
    /// </summary>
    public static class DbSetMigrationsExtensions
    {
        private const BindingFlags KeyPropertyBindingFlags
            = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        ///     Adds or updates entities by key when SaveChanges is called. Equivalent to an "upsert" operation
        ///     from database terminology.
        ///     This method can useful when seeding data using Migrations.
        /// </summary>
        /// <param name="entities"> The entities to add or update. </param>
        /// <remarks>
        ///     When the
        ///     <param name="set" />
        ///     parameter is a custom or fake IDbSet implementation, this method will
        ///     attempt to locate and invoke a public, instance method with the same signature as this extension method.
        /// </remarks>
        public static void AddOrUpdate<TEntity>(
            this IDbSet<TEntity> set, params TEntity[] entities)
            where TEntity : class
        {
            Check.NotNull(set, "set");
            Check.NotNull(entities, "entities");

            var dbSet = set as DbSet<TEntity>;

            if (dbSet != null)
            {
                var internalSet = (InternalSet<TEntity>)((IInternalSetAdapter)dbSet).InternalSet;

                dbSet.AddOrUpdate(GetKeyProperties(typeof(TEntity), internalSet), entities);
            }
            else
            {
                var targetType = set.GetType();

                var method = targetType.GetMethod("AddOrUpdate", new[] { typeof(TEntity[]) });

                if (method == null)
                {
                    throw Error.UnableToDispatchAddOrUpdate(targetType);
                }

                method.Invoke(set, new[] { entities });
            }
        }

        /// <summary>
        ///     Adds or updates entities by a custom identification expression when SaveChanges is called.
        ///     Equivalent to an "upsert" operation from database terminology.
        ///     This method can useful when seeding data using Migrations.
        /// </summary>
        /// <param name="identifierExpression"> An expression specifying the properties that should be used when determining whether an Add or Update operation should be performed. </param>
        /// <param name="entities"> The entities to add or update. </param>
        /// <remarks>
        ///     When the
        ///     <param name="set" />
        ///     parameter is a custom or fake IDbSet implementation, this method will
        ///     attempt to locate and invoke a public, instance method with the same signature as this extension method.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static void AddOrUpdate<TEntity>(
            this IDbSet<TEntity> set, Expression<Func<TEntity, object>> identifierExpression, params TEntity[] entities)
            where TEntity : class
        {
            Check.NotNull(set, "set");
            Check.NotNull(identifierExpression, "identifierExpression");
            Check.NotNull(entities, "entities");

            var dbSet = set as DbSet<TEntity>;

            if (dbSet != null)
            {
                var identifyingProperties = identifierExpression.GetPropertyAccessList();

                dbSet.AddOrUpdate(identifyingProperties, entities);
            }
            else
            {
                var targetType = set.GetType();

                var method
                    = targetType.GetMethod(
                        "AddOrUpdate",
                        new[] { typeof(Expression<Func<TEntity, object>>), typeof(TEntity[]) });

                if (method == null)
                {
                    throw Error.UnableToDispatchAddOrUpdate(targetType);
                }

                method.Invoke(set, new object[] { identifierExpression, entities });
            }
        }

        private static void AddOrUpdate<TEntity>(
            this DbSet<TEntity> set, IEnumerable<PropertyPath> identifyingProperties, params TEntity[] entities)
            where TEntity : class
        {
            DebugCheck.NotNull(set);
            DebugCheck.NotNull(identifyingProperties);
            DebugCheck.NotNull(entities);

            var internalSet = (InternalSet<TEntity>)((IInternalSetAdapter)set).InternalSet;
            var keyProperties = GetKeyProperties(typeof(TEntity), internalSet);
            var parameter = Expression.Parameter(typeof(TEntity));

            foreach (var entity in entities)
            {
                var matchExpression
                    = identifyingProperties.Select(
                        pi => Expression.Equal(
                            Expression.Property(parameter, pi.Last()),
                            Expression.Constant(pi.Last().GetValue(entity, null))))
                                           .Aggregate<BinaryExpression, Expression>(
                                               null,
                                               (current, predicate)
                                               => (current == null)
                                                      ? predicate
                                                      : Expression.AndAlso(current, predicate));

                var existing
                    = set.SingleOrDefault(Expression.Lambda<Func<TEntity, bool>>(matchExpression, new[] { parameter }));

                if (existing != null)
                {
                    foreach (var keyProperty in keyProperties)
                    {
                        keyProperty.Single().GetPropertyInfoForSet().SetValue(entity, keyProperty.Single().GetValue(existing, null), null);
                    }

                    internalSet.InternalContext.Owner.Entry(existing).CurrentValues.SetValues(entity);
                }

                else
                {
                    internalSet.Add(entity);
                }
            }
        }

        private static IEnumerable<PropertyPath> GetKeyProperties<TEntity>(
            Type entityType, InternalSet<TEntity> internalSet)
            where TEntity : class
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(internalSet);

            return internalSet.InternalContext
                              .GetEntitySetAndBaseTypeForType(typeof(TEntity))
                              .EntitySet.ElementType.KeyMembers
                              .Select(km => new PropertyPath(entityType.GetProperty(km.Name, KeyPropertyBindingFlags)));
        }
    }
}
