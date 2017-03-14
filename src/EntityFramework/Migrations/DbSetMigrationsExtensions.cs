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

    /// <summary>
    /// A set of extension methods for <see cref="IDbSet{TEntity}" />
    /// </summary>
    public static class DbSetMigrationsExtensions
    {
        /// <summary>
        /// Adds or updates entities by key when SaveChanges is called. Equivalent to an "upsert" operation
        /// from database terminology.
        /// This method can useful when seeding data using Migrations.
        /// </summary>
        /// <typeparam name="TEntity">The type of entities to add or update.</typeparam>
        /// <param name="set">The set to which the entities belong.</param>
        /// <param name="entities"> The entities to add or update. </param>
        /// <remarks>
        /// When the <paramref name="set" /> parameter is a custom or fake IDbSet implementation, this method will
        /// attempt to locate and invoke a public, instance method with the same signature as this extension method.
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

                if (internalSet != null)
                {
                    dbSet.AddOrUpdate(GetKeyProperties(typeof(TEntity), internalSet), internalSet, entities);

                    return;
                }
            }

            var targetType = set.GetType();

            var method = targetType.GetDeclaredMethod("AddOrUpdate", typeof(TEntity[]));

            if (method == null)
            {
                throw Error.UnableToDispatchAddOrUpdate(targetType);
            }

            method.Invoke(set, new[] { entities });
        }

        /// <summary>
        /// Adds or updates entities by a custom identification expression when SaveChanges is called.
        /// Equivalent to an "upsert" operation from database terminology.
        /// This method can useful when seeding data using Migrations.
        /// </summary>
        /// <typeparam name="TEntity">The type of entities to add or update.</typeparam>
        /// <param name="set">The set to which the entities belong.</param>
        /// <param name="identifierExpression"> An expression specifying the properties that should be used when determining whether an Add or Update operation should be performed. </param>
        /// <param name="entities"> The entities to add or update. </param>
        /// <remarks>
        /// When the <paramref name="set" /> parameter is a custom or fake IDbSet implementation, this method will
        /// attempt to locate and invoke a public, instance method with the same signature as this extension method.
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
                var internalSet = (InternalSet<TEntity>)((IInternalSetAdapter)dbSet).InternalSet;

                if (internalSet != null)
                {
                    var identifyingProperties
                        = identifierExpression.GetSimplePropertyAccessList();

                    dbSet.AddOrUpdate(identifyingProperties, internalSet, entities);

                    return;
                }
            }

            var targetType = set.GetType();

            var method
                = targetType.GetDeclaredMethod(
                    "AddOrUpdate",
                    typeof(Expression<Func<TEntity, object>>), typeof(TEntity[]));

            if (method == null)
            {
                throw Error.UnableToDispatchAddOrUpdate(targetType);
            }

            method.Invoke(set, new object[] { identifierExpression, entities });
        }

        /// <summary>
        /// Remove entities, if they exist, by key when SaveChanges is called. 
        /// This method can useful when seeding data using Migrations.
        /// </summary>
        /// <typeparam name="TEntity">The type of entities to remove if exist.</typeparam>
        /// <param name="set">The set to which the entities belong.</param>
        /// <param name="entities"> The entities to remove if exist. </param>
        /// <remarks>
        /// When the <paramref name="set" /> parameter is a custom or fake IDbSet implementation, this method will
        /// attempt to locate and invoke a public, instance method with the same signature as this extension method.
        /// </remarks>
        public static void RemoveIfExist<TEntity>(
            this IDbSet<TEntity> set, params TEntity[] entities)
            where TEntity : class
        {
            Check.NotNull(set, "set");
            Check.NotNull(entities, "entities");

            var dbSet = set as DbSet<TEntity>;

            if (dbSet != null)
            {
                var internalSet = (InternalSet<TEntity>)((IInternalSetAdapter)dbSet).InternalSet;

                if (internalSet != null)
                {
                    dbSet.RemoveIfExist(GetKeyProperties(typeof(TEntity), internalSet), internalSet, entities);

                    return;
                }
            }

            var targetType = set.GetType();

            var method = targetType.GetDeclaredMethod("RemoveIfExist", typeof(TEntity[]));

            if (method == null)
            {
                throw Error.UnableToDispatchRemoveIfExist(targetType);
            }

            method.Invoke(set, new[] { entities });
        }

        /// <summary>
        /// Remove entities, if they exist, by a custom identification expression when SaveChanges is called.
        /// This method can useful when seeding data using Migrations.
        /// </summary>
        /// <typeparam name="TEntity">The type of entities to remove if exist.</typeparam>
        /// <param name="set">The set to which the entities belong.</param>
        /// <param name="identifierExpression"> An expression specifying the properties that should be used when determining if a remove operation should be performed. </param>
        /// <param name="entities"> The entities to remove if exist. </param>
        /// <remarks>
        /// When the <paramref name="set" /> parameter is a custom or fake IDbSet implementation, this method will
        /// attempt to locate and invoke a public, instance method with the same signature as this extension method.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static void RemoveIfExist<TEntity>(
            this IDbSet<TEntity> set, Expression<Func<TEntity, object>> identifierExpression, params TEntity[] entities)
            where TEntity : class
        {
            Check.NotNull(set, "set");
            Check.NotNull(identifierExpression, "identifierExpression");
            Check.NotNull(entities, "entities");

            var dbSet = set as DbSet<TEntity>;

            if (dbSet != null)
            {
                var internalSet = (InternalSet<TEntity>)((IInternalSetAdapter)dbSet).InternalSet;

                if (internalSet != null)
                {
                    var identifyingProperties
                        = identifierExpression.GetSimplePropertyAccessList();

                    dbSet.RemoveIfExist(identifyingProperties, internalSet, entities);

                    return;
                }
            }

            var targetType = set.GetType();

            var method
                = targetType.GetDeclaredMethod(
                    "RemoveIfExist",
                    typeof(Expression<Func<TEntity, object>>), typeof(TEntity[]));

            if (method == null)
            {
                throw Error.UnableToDispatchRemoveIfExist(targetType);
            }

            method.Invoke(set, new object[] { identifierExpression, entities });
        }

        private static void AddOrUpdate<TEntity>(
            this DbSet<TEntity> set, IEnumerable<PropertyPath> identifyingProperties,
            InternalSet<TEntity> internalSet, params TEntity[] entities)
            where TEntity : class
        {
            DebugCheck.NotNull(set);
            DebugCheck.NotNull(identifyingProperties);
            DebugCheck.NotNull(entities);

            var keyProperties = GetKeyProperties(typeof(TEntity), internalSet);
            var parameter = Expression.Parameter(typeof(TEntity));

            foreach (var entity in entities)
            {
                var matchExpression
                    = identifyingProperties.Select(
                        pi => Expression.Equal(
                            Expression.Property(parameter, pi.Single()),
                            Expression.Constant(pi.Last().GetValue(entity, null), pi.Last().PropertyType)))
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

        private static void RemoveIfExist<TEntity>(
            this DbSet<TEntity> set, IEnumerable<PropertyPath> identifyingProperties,
            InternalSet<TEntity> internalSet, params TEntity[] entities)
            where TEntity : class
        {
            DebugCheck.NotNull(set);
            DebugCheck.NotNull(identifyingProperties);
            DebugCheck.NotNull(entities);

            var keyProperties = GetKeyProperties(typeof(TEntity), internalSet);
            var parameter = Expression.Parameter(typeof(TEntity));

            foreach (var entity in entities)
            {
                var matchExpression
                    = identifyingProperties.Select(
                        pi => Expression.Equal(
                            Expression.Property(parameter, pi.Single()),
                            Expression.Constant(pi.Last().GetValue(entity, null), pi.Last().PropertyType)))
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

                    internalSet.Remove(entity);
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
                              .Select(km => new PropertyPath(entityType.GetAnyProperty(km.Name)));
        }
    }
}
