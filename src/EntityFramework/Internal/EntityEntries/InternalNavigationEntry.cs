// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Resources;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Base class for <see cref="InternalCollectionEntry" /> and <see cref="InternalReferenceEntry" />
    ///     containing common code for collection and reference navigation property entries.
    /// </summary>
    internal abstract class InternalNavigationEntry : InternalMemberEntry
    {
        #region Fields and constructors

        private IRelatedEnd _relatedEnd;

        private Func<object, object> _getter;
        private bool _triedToGetGetter;

        private Action<object, object> _setter;
        private bool _triedToGetSetter;

        /// <summary>
        ///     Initializes a new instance of the <see cref="InternalNavigationEntry" /> class.
        /// </summary>
        /// <param name="internalEntityEntry"> The internal entity entry. </param>
        /// <param name="navigationMetadata"> The navigation metadata. </param>
        protected InternalNavigationEntry(
            InternalEntityEntry internalEntityEntry, NavigationEntryMetadata navigationMetadata)
            : base(internalEntityEntry, navigationMetadata)
        {
        }

        #endregion

        #region Loading

        /// <summary>
        ///     Calls Load on the underlying <see cref="IRelatedEnd" />.
        /// </summary>
        public virtual void Load()
        {
            ValidateNotDetached("Load");

            _relatedEnd.Load();
        }

#if !NET40

        /// <summary>
        ///     Calls LoadAsync on the underlying <see cref="IRelatedEnd" />.
        /// </summary>
        public virtual Task LoadAsync(CancellationToken cancellationToken)
        {
            ValidateNotDetached("LoadAsync");

            return _relatedEnd.LoadAsync(cancellationToken);
        }

#endif

        /// <summary>
        ///     Calls IsLoaded on the underlying <see cref="IRelatedEnd" />.
        /// </summary>
        public virtual bool IsLoaded
        {
            get
            {
                ValidateNotDetached("IsLoaded");

                return _relatedEnd.IsLoaded;
            }
        }

        /// <summary>
        ///     Uses CreateSourceQuery on the underlying <see cref="RelatedEnd" /> to create a query for this
        ///     navigation property.
        /// </summary>
        public virtual IQueryable Query()
        {
            ValidateNotDetached("Query");

            return (IQueryable)_relatedEnd.CreateSourceQuery();
        }

        #endregion

        #region Accessors

        /// <summary>
        ///     Gets the related end, which will be null if the entity is not being tracked.
        /// </summary>
        /// <value> The related end. </value>
        protected IRelatedEnd RelatedEnd
        {
            get
            {
                if (_relatedEnd == null
                    && !InternalEntityEntry.IsDetached)
                {
                    _relatedEnd = InternalEntityEntry.GetRelatedEnd(Name);
                }
                return _relatedEnd;
            }
        }

        #endregion

        #region Current values

        /// <summary>
        ///     Gets or sets the current value of the navigation property.  The current value is
        ///     the entity that the navigation property references or the collection of references
        ///     for a collection property.
        ///     This property is virtual so that it can be mocked.
        /// </summary>
        /// <value> The current value. </value>
        public override object CurrentValue
        {
            get
            {
                // Try to get the value directly from the entity and only try to get using the related end
                // if the entity has no getter that we can use.
                // This means we will always force lazy loading if available/enabled.
                if (Getter == null)
                {
                    ValidateNotDetached("CurrentValue");

                    return GetNavigationPropertyFromRelatedEnd(InternalEntityEntry.Entity);
                }
                return Getter(InternalEntityEntry.Entity);
            }
        }

        /// <summary>
        ///     Gets a delegate that can be used to get the value of the property directly from the entity.
        ///     Returns null if the property does not have an accessible getter.
        /// </summary>
        /// <value> The getter delegate, or null. </value>
        protected Func<object, object> Getter
        {
            get
            {
                if (!_triedToGetGetter)
                {
                    DbHelpers.GetPropertyGetters(InternalEntityEntry.EntityType).TryGetValue(Name, out _getter);
                    _triedToGetGetter = true;
                }
                return _getter;
            }
        }

        /// <summary>
        ///     Gets a delegate that can be used to set the value of the property directly on the entity.
        ///     Returns null if the property does not have an accessible setter.
        /// </summary>
        /// <value> The setter delegate, or null. </value>
        protected Action<object, object> Setter
        {
            get
            {
                if (!_triedToGetSetter)
                {
                    DbHelpers.GetPropertySetters(InternalEntityEntry.EntityType).TryGetValue(Name, out _setter);
                    _triedToGetSetter = true;
                }
                return _setter;
            }
        }

        /// <summary>
        ///     Gets the navigation property value from the <see cref="IRelatedEnd" /> object.
        /// </summary>
        /// <param name="entity"> The entity. </param>
        /// <returns> The navigation property value. </returns>
        protected abstract object GetNavigationPropertyFromRelatedEnd(object entity);

        #endregion

        #region Handling entries for detached entities

        /// <summary>
        ///     Validates that the owning entity entry is associated with an underlying <see
        ///      cref="System.Data.Entity.Core.Objects.ObjectStateEntry" /> and
        ///     is not just wrapping a non-attached entity.
        ///     If the entity is not detached, then the RelatedEnd for this navigation property is obtained.
        /// </summary>
        private void ValidateNotDetached(string method)
        {
            if (_relatedEnd == null)
            {
                if (InternalEntityEntry.IsDetached)
                {
                    throw Error.DbPropertyEntry_NotSupportedForDetached(
                        method, Name, InternalEntityEntry.EntityType.Name);
                }

                _relatedEnd = InternalEntityEntry.GetRelatedEnd(Name);
            }
        }

        #endregion
    }
}
