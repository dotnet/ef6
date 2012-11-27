// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     A non-generic version of the <see cref="DbComplexPropertyEntry{TEntity, TProperty}" /> class.
    /// </summary>
    public class DbComplexPropertyEntry : DbPropertyEntry
    {
        #region Fields and constructors

        /// <summary>
        ///     Creates a <see cref="DbComplexPropertyEntry{TEntity,TComplexProperty}" /> from information in the given
        ///     <see
        ///         cref="InternalPropertyEntry" />
        ///     .
        ///     Use this method in preference to the constructor since it may potentially create a subclass depending on
        ///     the type of member represented by the InternalCollectionEntry instance.
        /// </summary>
        /// <param name="internalPropertyEntry"> The internal property entry. </param>
        /// <returns> The new entry. </returns>
        internal new static DbComplexPropertyEntry Create(InternalPropertyEntry internalPropertyEntry)
        {
            DebugCheck.NotNull(internalPropertyEntry);

            return (DbComplexPropertyEntry)internalPropertyEntry.CreateDbMemberEntry();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbPropertyEntry" /> class.
        /// </summary>
        /// <param name="internalPropertyEntry"> The internal entry. </param>
        internal DbComplexPropertyEntry(InternalPropertyEntry internalPropertyEntry)
            : base(internalPropertyEntry)
        {
        }

        #endregion

        #region Access to nested properties

        /// <summary>
        ///     Gets an object that represents a nested property of this property.
        ///     This method can be used for both scalar or complex properties.
        /// </summary>
        /// <param name="propertyName"> The name of the nested property. </param>
        /// <returns> An object representing the nested property. </returns>
        public DbPropertyEntry Property(string propertyName)
        {
            Check.NotEmpty(propertyName, "propertyName");

            return DbPropertyEntry.Create(((InternalPropertyEntry)InternalMemberEntry).Property(propertyName));
        }

        /// <summary>
        ///     Gets an object that represents a nested complex property of this property.
        /// </summary>
        /// <param name="propertyName"> The name of the nested property. </param>
        /// <returns> An object representing the nested property. </returns>
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#",
            Justification = "Rule predates more fluent naming conventions.")]
        public DbComplexPropertyEntry ComplexProperty(string propertyName)
        {
            Check.NotEmpty(propertyName, "propertyName");

            return
                Create(((InternalPropertyEntry)InternalMemberEntry).Property(propertyName, null, requireComplex: true));
        }

        #endregion

        #region Conversion to generic

        /// <summary>
        ///     Returns the equivalent generic <see cref="DbComplexPropertyEntry{TEntity,TComplexProperty}" /> object.
        /// </summary>
        /// <typeparam name="TEntity"> The type of entity on which the member is declared. </typeparam>
        /// <typeparam name="TComplexProperty"> The type of the complex property. </typeparam>
        /// <returns> The equivalent generic object. </returns>
        public new DbComplexPropertyEntry<TEntity, TComplexProperty> Cast<TEntity, TComplexProperty>()
            where TEntity : class
        {
            var metadata = InternalMemberEntry.EntryMetadata;
            if (!typeof(TEntity).IsAssignableFrom(metadata.DeclaringType)
                || !typeof(TComplexProperty).IsAssignableFrom(metadata.ElementType))
            {
                throw Error.DbMember_BadTypeForCast(
                    typeof(DbComplexPropertyEntry).Name,
                    typeof(TEntity).Name,
                    typeof(TComplexProperty).Name,
                    metadata.DeclaringType.Name,
                    metadata.MemberType.Name);
            }

            return DbComplexPropertyEntry<TEntity, TComplexProperty>.Create((InternalPropertyEntry)InternalMemberEntry);
        }

        #endregion
    }
}
