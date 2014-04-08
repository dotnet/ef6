// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Reflection;

    // <summary>
    // The internal class used to implement <see cref="DbCollectionEntry" /> and
    // <see cref="DbCollectionEntry{TEntity,TElement}" />.
    // This internal class contains all the common implementation between the generic and non-generic
    // entry classes and also allows for a clean internal factoring without compromising the public API.
    // </summary>
    internal class InternalCollectionEntry : InternalNavigationEntry
    {
        #region Fields and constructors

        private static readonly ConcurrentDictionary<Type, Func<InternalCollectionEntry, object>> _entryFactories =
            new ConcurrentDictionary<Type, Func<InternalCollectionEntry, object>>();

        // <summary>
        // Initializes a new instance of the <see cref="InternalCollectionEntry" /> class.
        // </summary>
        // <param name="internalEntityEntry"> The internal entity entry. </param>
        // <param name="navigationMetadata"> The navigation metadata. </param>
        public InternalCollectionEntry(
            InternalEntityEntry internalEntityEntry, NavigationEntryMetadata navigationMetadata)
            : base(internalEntityEntry, navigationMetadata)
        {
        }

        #endregion

        #region Current values

        // <summary>
        // Gets the navigation property value from the <see cref="IRelatedEnd" /> object.
        // Since for a collection the related end is an <see cref="EntityCollection{T}" />, it means
        // that the internal representation of the navigation property is just the related end.
        // </summary>
        // <param name="entity"> The entity. </param>
        // <returns> The navigation property value. </returns>
        protected override object GetNavigationPropertyFromRelatedEnd(object entity)
        {
            return RelatedEnd;
        }

        // <summary>
        // Gets or sets the current value of the navigation property.  The current value is
        // the entity that the navigation property references or the collection of references
        // for a collection property.
        // </summary>
        // <value> The current value. </value>
        public override object CurrentValue
        {
            get
            {
                // Needed for Moq
                return base.CurrentValue;
            }
            set
            {
                if (Setter != null)
                {
                    Setter(InternalEntityEntry.Entity, value);
                }
                else if (InternalEntityEntry.IsDetached
                         || !ReferenceEquals(RelatedEnd, value))
                {
                    throw Error.DbCollectionEntry_CannotSetCollectionProp(
                        Name, InternalEntityEntry.Entity.GetType().ToString());
                }
            }
        }

        #endregion

        #region DbMemberEntry factory methods

        // <summary>
        // Creates a new non-generic <see cref="DbMemberEntry" /> backed by this internal entry.
        // The runtime type of the DbMemberEntry created will be <see cref="DbCollectionEntry" /> or a subtype of it.
        // </summary>
        // <returns> The new entry. </returns>
        public override DbMemberEntry CreateDbMemberEntry()
        {
            return new DbCollectionEntry(this);
        }

        // <summary>
        // Creates a new generic <see cref="DbMemberEntry{TEntity,TProperty}" /> backed by this internal entry.
        // The runtime type of the DbMemberEntry created will be <see cref="DbCollectionEntry{TEntity,TElement}" /> or a subtype of it.
        // </summary>
        // <typeparam name="TEntity"> The type of the entity. </typeparam>
        // <typeparam name="TProperty"> The type of the property. </typeparam>
        // <returns> The new entry. </returns>
        public override DbMemberEntry<TEntity, TProperty> CreateDbMemberEntry<TEntity, TProperty>()
        {
            // The challenge here is that DbMemberEntry is defined in terms of the property type
            // (e.g. ICollection<Xyz>) while DbCollectionEntry is defined in terms of the element
            // type (e.g. Xyz).  We therefore need to dynamically create a DbCollectionEntry of
            // the correct type using reflection compiled to a delegate.
            return CreateDbCollectionEntry<TEntity, TProperty>(EntryMetadata.ElementType);
        }

        // <summary>
        // Creates a new generic <see cref="DbMemberEntry{TEntity,TProperty}" /> backed by this internal entry.
        // The actual subtype of the DbCollectionEntry created depends on the metadata of this internal entry.
        // </summary>
        // <typeparam name="TEntity"> The type of the entity. </typeparam>
        // <typeparam name="TElement"> The type of the element. </typeparam>
        // <returns> The new entry. </returns>
        public virtual DbCollectionEntry<TEntity, TElement> CreateDbCollectionEntry<TEntity, TElement>()
            where TEntity : class
        {
            return new DbCollectionEntry<TEntity, TElement>(this);
        }

        // <summary>
        // Creates a <see cref="DbCollectionEntry{TEntity,TElement}" /> object for the given entity type
        // and collection element type.
        // </summary>
        // <typeparam name="TEntity"> The type of the entity. </typeparam>
        // <typeparam name="TProperty"> The type of the property. </typeparam>
        // <param name="elementType"> Type of the element. </param>
        // <returns> The set. </returns>
        private DbMemberEntry<TEntity, TProperty> CreateDbCollectionEntry<TEntity, TProperty>(Type elementType)
            where TEntity : class
        {
            var targetType = typeof(DbMemberEntry<TEntity, TProperty>);

            Func<InternalCollectionEntry, object> factory;
            if (!_entryFactories.TryGetValue(targetType, out factory))
            {
                var genericType = typeof(DbCollectionEntry<,>).MakeGenericType(typeof(TEntity), elementType);

                if (!targetType.IsAssignableFrom(genericType))
                {
                    throw Error.DbEntityEntry_WrongGenericForCollectionNavProp(
                        typeof(TProperty),
                        Name,
                        EntryMetadata.DeclaringType,
                        typeof(ICollection<>).MakeGenericType(elementType));
                }

                var factoryMethod = genericType.GetDeclaredMethod("Create", typeof(InternalCollectionEntry));
                factory =
                    (Func<InternalCollectionEntry, object>)
                    Delegate.CreateDelegate(typeof(Func<InternalCollectionEntry, object>), factoryMethod);
                _entryFactories.TryAdd(targetType, factory);
            }
            return (DbMemberEntry<TEntity, TProperty>)factory(this);
        }

        #endregion
    }
}
