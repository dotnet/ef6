// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Internal;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;

    /// <summary>
    ///     Instances of this class are returned from the ComplexProperty method of
    ///     <see cref = "DbEntityEntry{T}" /> and allow access to the state of a complex property.
    /// </summary>
    /// <typeparam name = "TEntity">The type of the entity to which this property belongs.</typeparam>
    /// <typeparam name = "TComplexProperty">The type of the property.</typeparam>
    public class DbComplexPropertyEntry<TEntity, TComplexProperty> : DbPropertyEntry<TEntity, TComplexProperty>
        where TEntity : class
    {
        #region Fields and constructors

        /// <summary>
        ///     Creates a <see cref = "DbComplexPropertyEntry" /> from information in the given <see cref = "InternalPropertyEntry" />.
        ///     Use this method in preference to the constructor since it may potentially create a subclass depending on
        ///     the type of member represented by the InternalCollectionEntry instance.
        /// </summary>
        /// <param name = "internalPropertyEntry">The internal property entry.</param>
        /// <returns>The new entry.</returns>
        internal new static DbComplexPropertyEntry<TEntity, TComplexProperty> Create(
            InternalPropertyEntry internalPropertyEntry)
        {
            Contract.Requires(internalPropertyEntry != null);

            return
                (DbComplexPropertyEntry<TEntity, TComplexProperty>)
                internalPropertyEntry.CreateDbMemberEntry<TEntity, TComplexProperty>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref = "DbComplexPropertyEntry{TEntity, TComplexProperty}" /> class.
        /// </summary>
        /// <param name = "internalPropertyEntry">The internal entry.</param>
        internal DbComplexPropertyEntry(InternalPropertyEntry internalPropertyEntry)
            : base(internalPropertyEntry)
        {
        }

        #endregion

        #region Conversion to non-generic

        /// <summary>
        ///     Returns a new instance of the non-generic <see cref = "DbComplexPropertyEntry" /> class for 
        ///     the property represented by this object.
        /// </summary>
        /// <returns>A non-generic version.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates",
            Justification = "Intentionally just implicit to reduce API clutter.")]
        public static implicit operator DbComplexPropertyEntry(DbComplexPropertyEntry<TEntity, TComplexProperty> entry)
        {
            return DbComplexPropertyEntry.Create(entry.InternalPropertyEntry);
        }

        #endregion

        #region Access to nested properties

        /// <summary>
        ///     Gets an object that represents a nested property of this property.
        ///     This method can be used for both scalar or complex properties.
        /// </summary>
        /// <param name = "propertyName">The name of the nested property.</param>
        /// <returns>An object representing the nested property.</returns>
        public DbPropertyEntry Property(string propertyName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            return DbPropertyEntry.Create(InternalPropertyEntry.Property(propertyName));
        }

        /// <summary>
        ///     Gets an object that represents a nested property of this property.
        ///     This method can be used for both scalar or complex properties.
        /// </summary>
        /// <typeparam name = "TNestedProperty">The type of the nested property.</typeparam>
        /// <param name = "propertyName">The name of the nested property.</param>
        /// <returns>An object representing the nested property.</returns>
        public DbPropertyEntry<TEntity, TNestedProperty> Property<TNestedProperty>(string propertyName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            return
                DbPropertyEntry<TEntity, TNestedProperty>.Create(
                    InternalPropertyEntry.Property(propertyName, typeof(TNestedProperty)));
        }

        /// <summary>
        ///     Gets an object that represents a nested property of this property.
        ///     This method can be used for both scalar or complex properties.
        /// </summary>
        /// <typeparam name = "TNestedProperty">The type of the nested property.</typeparam>
        /// <param name = "navigationProperty">An expression representing the nested property.</param>
        /// <returns>An object representing the nested property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#",
            Justification = "Rule predates more fluent naming conventions.")]
        public DbPropertyEntry<TEntity, TNestedProperty> Property<TNestedProperty>(
            Expression<Func<TComplexProperty, TNestedProperty>> property)
        {
            Contract.Requires(property != null);

            return Property<TNestedProperty>(DbHelpers.ParsePropertySelector(property, "Property", "property"));
        }

        /// <summary>
        ///     Gets an object that represents a nested complex property of this property.
        /// </summary>
        /// <param name = "propertyName">The name of the nested property.</param>
        /// <returns>An object representing the nested property.</returns>
        public DbComplexPropertyEntry ComplexProperty(string propertyName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            return
                DbComplexPropertyEntry.Create(InternalPropertyEntry.Property(propertyName, null, requireComplex: true));
        }

        /// <summary>
        ///     Gets an object that represents a nested complex property of this property.
        /// </summary>
        /// <typeparam name = "TNestedComplexProperty">The type of the nested property.</typeparam>
        /// <param name = "propertyName">The name of the nested property.</param>
        /// <returns>An object representing the nested property.</returns>
        public DbComplexPropertyEntry<TEntity, TNestedComplexProperty> ComplexProperty<TNestedComplexProperty>(
            string propertyName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));

            return
                DbComplexPropertyEntry<TEntity, TNestedComplexProperty>.Create(
                    InternalPropertyEntry.Property(propertyName, typeof(TNestedComplexProperty), requireComplex: true));
        }

        /// <summary>
        ///     Gets an object that represents a nested complex property of this property.
        /// </summary>
        /// <typeparam name = "TNestedComplexProperty">The type of the nested property.</typeparam>
        /// <param name = "navigationProperty">An expression representing the nested property.</param>
        /// <returns>An object representing the nested property.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#",
            Justification = "Rule predates more fluent naming conventions.")]
        public DbComplexPropertyEntry<TEntity, TNestedComplexProperty> ComplexProperty<TNestedComplexProperty>(
            Expression<Func<TComplexProperty, TNestedComplexProperty>> property)
        {
            Contract.Requires(property != null);

            return
                ComplexProperty<TNestedComplexProperty>(
                    DbHelpers.ParsePropertySelector(property, "Property", "property"));
        }

        #endregion
    }
}
