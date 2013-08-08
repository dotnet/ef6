// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Data.Entity.Validation;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// This is an abstract base class use to represent a scalar or complex property, or a navigation property
    /// of an entity.  Scalar and complex properties use the derived class <see cref="DbPropertyEntry" />,
    /// reference navigation properties use the derived class <see cref="DbReferenceEntry" />, and collection
    /// navigation properties use the derived class <see cref="DbCollectionEntry" />.
    /// </summary>
    public abstract class DbMemberEntry
    {
        #region  Factory methods

        /// <summary>
        /// Creates a <see cref="DbMemberEntry" /> from information in the given <see cref="InternalMemberEntry" />.
        /// This method will create an instance of the appropriate subclass depending on the metadata contained
        /// in the InternalMemberEntry instance.
        /// </summary>
        /// <param name="internalMemberEntry"> The internal member entry. </param>
        /// <returns> The new entry. </returns>
        internal static DbMemberEntry Create(InternalMemberEntry internalMemberEntry)
        {
            DebugCheck.NotNull(internalMemberEntry);

            return internalMemberEntry.CreateDbMemberEntry();
        }

        #endregion

        #region Name

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value> The property name. </value>
        public abstract string Name { get; }

        #endregion

        #region Current values

        /// <summary>
        /// Gets or sets the current value of this property.
        /// </summary>
        /// <value> The current value. </value>
        public abstract object CurrentValue { get; set; }

        #endregion

        #region Back references

        /// <summary>
        /// The <see cref="DbEntityEntry" /> to which this member belongs.
        /// </summary>
        /// <value> An entry for the entity that owns this member. </value>
        public abstract DbEntityEntry EntityEntry { get; }

        #endregion

        #region Validation

        /// <summary>
        /// Validates this property.
        /// </summary>
        /// <returns>
        /// Collection of <see cref="DbValidationError" /> objects. Never null. If the entity is valid the collection will be empty.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public ICollection<DbValidationError> GetValidationErrors()
        {
            return InternalMemberEntry.GetValidationErrors().ToList();
        }

        #endregion

        #region Hidden Object methods

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Gets the <see cref="Type" /> of the current instance.
        /// </summary>
        /// <returns>The exact runtime type of the current instance.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }

        #endregion

        #region InternalMemberEntry access

        /// <summary>
        /// Gets the <see cref="InternalMemberEntry" /> backing this object.
        /// </summary>
        /// <value> The internal member entry. </value>
        internal abstract InternalMemberEntry InternalMemberEntry { get; }

        #endregion

        #region Conversion to generic

        /// <summary>
        /// Returns the equivalent generic <see cref="DbMemberEntry{TEntity,TProperty}" /> object.
        /// </summary>
        /// <typeparam name="TEntity"> The type of entity on which the member is declared. </typeparam>
        /// <typeparam name="TProperty"> The type of the property. </typeparam>
        /// <returns> The equivalent generic object. </returns>
        public DbMemberEntry<TEntity, TProperty> Cast<TEntity, TProperty>() where TEntity : class
        {
            var metadata = InternalMemberEntry.EntryMetadata;
            if (!typeof(TEntity).IsAssignableFrom(metadata.DeclaringType)
                || !typeof(TProperty).IsAssignableFrom(metadata.MemberType))
            {
                throw Error.DbMember_BadTypeForCast(
                    typeof(DbMemberEntry).Name,
                    typeof(TEntity).Name,
                    typeof(TProperty).Name,
                    metadata.DeclaringType.Name,
                    metadata.MemberType.Name);
            }

            return DbMemberEntry<TEntity, TProperty>.Create(InternalMemberEntry);
        }

        #endregion
    }
}
