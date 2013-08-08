// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;
    using System.Data.Entity.Validation;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Base class for all internal entries that represent different kinds of properties.
    /// </summary>
    internal abstract class InternalMemberEntry
    {
        #region Constructors and fields

        private readonly InternalEntityEntry _internalEntityEntry;
        private readonly MemberEntryMetadata _memberMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalMemberEntry" /> class.
        /// </summary>
        /// <param name="internalEntityEntry"> The internal entity entry. </param>
        /// <param name="memberMetadata"> The member metadata. </param>
        protected InternalMemberEntry(InternalEntityEntry internalEntityEntry, MemberEntryMetadata memberMetadata)
        {
            DebugCheck.NotNull(internalEntityEntry);
            DebugCheck.NotNull(memberMetadata);

            _internalEntityEntry = internalEntityEntry;
            _memberMetadata = memberMetadata;
        }

        #endregion

        #region Name

        /// <summary>
        /// Gets the property name.
        /// The property is virtual to allow mocking.
        /// </summary>
        /// <value> The property name. </value>
        public virtual string Name
        {
            get { return _memberMetadata.MemberName; }
        }

        #endregion

        #region CurrentValue

        /// <summary>
        /// Gets or sets the current value of the navigation property.
        /// </summary>
        /// <value> The current value. </value>
        public abstract object CurrentValue { get; set; }

        #endregion

        #region Internal entity/metadata access

        /// <summary>
        /// Gets the internal entity entry property belongs to.
        /// This property is virtual to allow mocking.
        /// </summary>
        /// <value> The internal entity entry. </value>
        public virtual InternalEntityEntry InternalEntityEntry
        {
            get { return _internalEntityEntry; }
        }

        /// <summary>
        /// Gets the entry metadata.
        /// </summary>
        /// <value> The entry metadata. </value>
        public virtual MemberEntryMetadata EntryMetadata
        {
            get { return _memberMetadata; }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates this property.
        /// </summary>
        /// <returns> A sequence of validation errors for this property. Empty if no errors. Never null. </returns>
        public virtual IEnumerable<DbValidationError> GetValidationErrors()
        {
            Debug.Assert(
                InternalEntityEntry.InternalContext.ValidationProvider != null,
                "_internalEntityEntry.InternalContext.ValidatorProvider != null");

            var validationProvider = InternalEntityEntry.InternalContext.ValidationProvider;
            var propertyValidator = validationProvider.GetPropertyValidator(_internalEntityEntry, this);

            return propertyValidator != null
                       ? propertyValidator.Validate(
                           validationProvider.GetEntityValidationContext(_internalEntityEntry, null), this)
                       : Enumerable.Empty<DbValidationError>();
        }

        #endregion

        #region DbMemberEntry factory methods

        /// <summary>
        /// Creates a new non-generic <see cref="DbMemberEntry" /> backed by this internal entry.
        /// The actual subtype of the DbMemberEntry created depends on the metadata of this internal entry.
        /// </summary>
        /// <returns> The new entry. </returns>
        public abstract DbMemberEntry CreateDbMemberEntry();

        /// <summary>
        /// Creates a new generic <see cref="DbMemberEntry{TEntity,TProperty}" /> backed by this internal entry.
        /// The actual subtype of the DbMemberEntry created depends on the metadata of this internal entry.
        /// </summary>
        /// <typeparam name="TEntity"> The type of the entity. </typeparam>
        /// <typeparam name="TProperty"> The type of the property. </typeparam>
        /// <returns> The new entry. </returns>
        public abstract DbMemberEntry<TEntity, TProperty> CreateDbMemberEntry<TEntity, TProperty>()
            where TEntity : class;

        #endregion
    }
}
