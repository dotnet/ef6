namespace System.Data.Entity.Internal
{
    using System.Collections.Concurrent;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     The internal class used to implement <see cref = "System.Data.Entity.Infrastructure.DbReferenceEntry" />,
    ///     and <see cref = "System.Data.Entity.Infrastructure.DbReferenceEntry{TEntity, TProperty}" />.
    ///     This internal class contains all the common implementation between the generic and non-generic
    ///     entry classes and also allows for a clean internal factoring without compromising the public API.
    /// </summary>
    internal class InternalReferenceEntry : InternalNavigationEntry
    {
        #region Fields and constructors

        private static readonly ConcurrentDictionary<Type, Action<IRelatedEnd, object>> _entityReferenceValueSetters =
            new ConcurrentDictionary<Type, Action<IRelatedEnd, object>>();

        private static readonly MethodInfo _setValueOnEntityReferenceMethod =
            typeof(InternalReferenceEntry).GetMethod(
                "SetValueOnEntityReference", BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        ///     Initializes a new instance of the <see cref = "InternalReferenceEntry" /> class.
        /// </summary>
        /// <param name = "internalEntityEntry">The internal entity entry.</param>
        /// <param name = "navigationMetadata">The navigation metadata.</param>
        public InternalReferenceEntry(
            InternalEntityEntry internalEntityEntry, NavigationEntryMetadata navigationMetadata)
            : base(internalEntityEntry, navigationMetadata)
        {
        }

        #endregion

        #region Current values

        /// <summary>
        ///     Gets the navigation property value from the <see cref = "IRelatedEnd" /> object.
        ///     For reference navigation properties, this means getting the value from the
        ///     <see cref = "EntityReference{T}" /> object.
        /// </summary>
        /// <param name = "entity">The entity.</param>
        /// <returns>The navigation property value.</returns>
        protected override object GetNavigationPropertyFromRelatedEnd(object entity)
        {
            Contract.Assert(!(RelatedEnd is IDisposable), "RelatedEnd is not expected to be disposable.");

            // To avoid needing to access the generic EntityReference<T> class we instead
            // treat the RelatedEnd as an IEnumerable and get the single value that way.
            var enumerator = RelatedEnd.GetEnumerator();
            return enumerator.MoveNext() ? enumerator.Current : null;
        }

        /// <summary>
        ///     Sets the navigation property value onto the <see cref = "IRelatedEnd" /> object.
        ///     For reference navigation properties, this means setting the value onto the
        ///     <see cref = "EntityReference{T}" /> object.
        /// </summary>
        /// <param name = "entity">The entity.</param>
        /// <param name = "value">The value.</param>
        protected virtual void SetNavigationPropertyOnRelatedEnd(object value)
        {
            var entityRefType = RelatedEnd.GetType();
            Action<IRelatedEnd, object> setter;
            if (!_entityReferenceValueSetters.TryGetValue(entityRefType, out setter))
            {
                var setMethod =
                    _setValueOnEntityReferenceMethod.MakeGenericMethod(entityRefType.GetGenericArguments().Single());
                setter =
                    (Action<IRelatedEnd, object>)Delegate.CreateDelegate(typeof(Action<IRelatedEnd, object>), setMethod);
                _entityReferenceValueSetters.TryAdd(entityRefType, setter);
            }
            setter(RelatedEnd, value);
        }

        /// <summary>
        ///     Sets the given value on the given <see cref = "IRelatedEnd" /> which must be an
        ///     <see cref = "EntityReference{TRelatedEntity}" />.
        ///     This method is setup in such a way that it can easily be used by CreateDelegate without any
        ///     dynamic code generation needed.
        /// </summary>
        /// <typeparam name = "TRelatedEntity">The type of the related entity.</typeparam>
        /// <param name = "entityReference">The entity reference.</param>
        /// <param name = "value">The value.</param>
        private static void SetValueOnEntityReference<TRelatedEntity>(IRelatedEnd entityReference, object value)
            where TRelatedEntity : class
        {
            Contract.Assert(entityReference is EntityReference<TRelatedEntity>);
            Contract.Assert(value == null || value is TRelatedEntity);

            ((EntityReference<TRelatedEntity>)entityReference).Value = (TRelatedEntity)value;
        }

        /// <summary>
        ///     Gets or sets the current value of the navigation property.  The current value is
        ///     the entity that the navigation property references or the collection of references
        ///     for a collection property.
        /// </summary>
        /// <value>The current value.</value>
        public override object CurrentValue
        {
            get
            {
                // Needed for Moq
                return base.CurrentValue;
            }
            set
            {
                // Always try to set using the related end if we can since it doesn't require a call to
                // DetectChanges for the change to be tracked.
                if (RelatedEnd != null
                    && InternalEntityEntry.State != EntityState.Deleted)
                {
                    SetNavigationPropertyOnRelatedEnd(value);
                }
                else
                {
                    if (Setter != null)
                    {
                        Setter(InternalEntityEntry.Entity, value);
                    }
                    else
                    {
                        Contract.Assert(
                            InternalEntityEntry.State == EntityState.Detached
                            || InternalEntityEntry.State == EntityState.Deleted);

                        throw Error.DbPropertyEntry_SettingEntityRefNotSupported(
                            Name, InternalEntityEntry.EntityType.Name, InternalEntityEntry.State);
                    }
                }
            }
        }

        #endregion

        #region DbMemberEntry factory methods

        /// <summary>
        ///     Creates a new non-generic <see cref = "DbMemberEntry" /> backed by this internal entry.
        ///     The runtime type of the DbMemberEntry created will be <see cref = "DbReferenceEntry" /> or a subtype of it.
        /// </summary>
        /// <returns>The new entry.</returns>
        public override DbMemberEntry CreateDbMemberEntry()
        {
            return new DbReferenceEntry(this);
        }

        /// <summary>
        ///     Creates a new generic <see cref = "DbMemberEntry{TEntity,TProperty}" /> backed by this internal entry.
        ///     The runtime type of the DbMemberEntry created will be <see cref = "DbReferenceEntry{TEntity,TProperty}" /> or a subtype of it.
        /// </summary>
        /// <typeparam name = "TEntity">The type of the entity.</typeparam>
        /// <typeparam name = "TProperty">The type of the property.</typeparam>
        /// <returns>The new entry.</returns>
        public override DbMemberEntry<TEntity, TProperty> CreateDbMemberEntry<TEntity, TProperty>()
        {
            return new DbReferenceEntry<TEntity, TProperty>(this);
        }

        #endregion
    }
}
