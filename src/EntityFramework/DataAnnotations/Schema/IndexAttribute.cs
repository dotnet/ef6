// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.ComponentModel.DataAnnotations.Schema
{
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Text;

    /// <summary>
    /// When this attribute is placed on a property it indicates that the database column to which the
    /// property is mapped has an index.
    /// </summary>
    /// <remarks>
    /// This attribute is used by Entity Framework Migrations to create indexes on mapped database columns.
    /// Multi-column indexes are created by using the same index name in multiple attributes. The information
    /// in these attributes is then merged together to specify the actual database index.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes")]
    public class IndexAttribute : Attribute
    {
        private readonly string _name;
        private int _order = -1;
        private bool? _isClustered;
        private bool? _isUnique;

        /// <summary>
        /// Creates a <see cref="IndexAttribute" /> instance for an index that will be named by convention and
        /// has no column order, clustering, or uniqueness specified.
        /// </summary>
        public IndexAttribute()
        {
        }

        /// <summary>
        /// Creates a <see cref="IndexAttribute" /> instance for an index with the given name and
        /// has no column order, clustering, or uniqueness specified.
        /// </summary>
        /// <param name="name">The index name.</param>
        public IndexAttribute(string name)
        {
            Check.NotEmpty(name, "name");

            // This makes handling the index names easier and doesn't seem too restrictive. If necessary this
            // could be relaxed in the future, but then proper handling of strange names must happen when serializing
            if (!name.IsValidUndottedName())
            {
                throw new ArgumentException(Strings.BadIndexName(name));
            }

            _name = name;
        }

        /// <summary>
        /// Creates a <see cref="IndexAttribute" /> instance for an index with the given name and column order, 
        /// but with no clustering or uniqueness specified.
        /// </summary>
        /// <remarks>
        /// Multi-column indexes are created by using the same index name in multiple attributes. The information
        /// in these attributes is then merged together to specify the actual database index.
        /// </remarks>
        /// <param name="name">The index name.</param>
        /// <param name="order">A number which will be used to determine column ordering for multi-column indexes.</param>
        public IndexAttribute(string name, int order)
        {
            Check.NotEmpty(name, "name");

            // This makes handling the index names easier and doesn't seem too restrictive. If necessary this
            // could be relaxed in the future, but then proper handling of strange names must happen when serializing
            if (!name.IsValidUndottedName())
            {
                throw new ArgumentException(Strings.BadIndexName(name));
            }

            if (order < 0)
            {
                throw new ArgumentOutOfRangeException("order");
            }

            _name = name;
            _order = order;
        }

        private IndexAttribute(string name, int order, bool? isClustered, bool? isUnique)
        {
            _name = name;
            _order = order;
            _isClustered = isClustered;
            _isUnique = isUnique;
        }

        /// <summary>
        /// The index name.
        /// </summary>
        /// <remarks>
        /// Multi-column indexes are created by using the same index name in multiple attributes. The information
        /// in these attributes is then merged together to specify the actual database index.
        /// </remarks>
        public virtual string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// A number which will be used to determine column ordering for multi-column indexes. This will be -1 if no
        /// column order has been specified.
        /// </summary>
        /// <remarks>
        /// Multi-column indexes are created by using the same index name in multiple attributes. The information
        /// in these attributes is then merged together to specify the actual database index.
        /// </remarks>
        public virtual int Order
        {
            get { return _order; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _order = value;
            }
        }

        /// <summary>
        /// Set this property to true to define a clustered index. Set this property to false to define a 
        /// non-clustered index. Do not get the value of this property; use <see cref="ClusteredConfiguration"/> instead.
        /// </summary>
        /// <remarks>
        /// This property only has a getter because this is required for use in the C# language
        /// syntax for attributes since that syntax does not support nullable types or write-only properties.
        /// </remarks>
        public virtual bool IsClustered
        {
            get
            {
                throw new NotSupportedException(Strings.IndexAttributeNonNullableProperty("IsClustered", "ClusteredConfiguration"));
            }
            set { _isClustered = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the index is clustered.
        /// </summary>
        /// <remarks>
        /// A value of null indicates that the index has not been explicitly configured as either clustered
        /// or non-clustered and this will therefore be decided at a later time when processing the complete
        /// configuration. The value ultimately used may come from another attribute that is merged with this
        /// one or may be determined by convention if no value is specified in any attribute.
        /// </remarks>
        public virtual bool? ClusteredConfiguration
        {
            get { return _isClustered; }
            set { _isClustered = value; }
        }

        /// <summary>
        /// Set this property to true to define a unique index. Set this property to false to define a 
        /// non-unique index. Do not get the value of this property; use <see cref="UniqueConfiguration"/> instead.
        /// </summary>
        /// <remarks>
        /// This property only has a getter because this is required for use in the C# language
        /// syntax for attributes since that syntax does not support nullable types or write-only properties.
        /// </remarks>
        public virtual bool IsUnique
        {
            get
            {
                throw new NotSupportedException(Strings.IndexAttributeNonNullableProperty("IsUnique", "UniqueConfiguration"));
            }
            set { _isUnique = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the index is unique.
        /// </summary>
        /// <remarks>
        /// A value of null indicates that the index has not been explicitly configured as either unique
        /// or non-unique and this will therefore be decided at a later time when processing the complete
        /// configuration. The value ultimately used may come from another attribute that is merged with this
        /// one or may be determined by convention if no value is specified in any attribute.
        /// </remarks>
        public virtual bool? UniqueConfiguration
        {
            get { return _isUnique; }
            set { _isUnique = value; }
        }

        /// <summary>
        /// Returns a different ID for each object instance such that type descriptors won't
        /// attempt to combine all IndexAttribute instances into a single instance.
        /// </summary>
        public override object TypeId
        {
            get
            {
                return RuntimeHelpers.GetHashCode(this);
            }
        }

        /// <summary>
        /// Returns true if this attribute does not conflict with the given attribute such that
        /// the two can be combined together using the <see cref="MergeWith"/> method.
        /// </summary>
        /// <remarks>
        /// Two attributes are considered compatible if they have the same name and all other properties
        /// (<see cref="IsUnique"/>, <see cref="IsClustered"/>, and <see cref="Order"/>) are either not specified
        /// on this attribute or the other attribute or are specified with the same value on both.
        /// </remarks>
        /// <param name="other">The attribute to compare.</param>
        /// <returns>A CompatibilityResult indicating whether or not this attribute is compatible with the other.</returns>
        public virtual CompatibilityResult IsCompatibleWith(IndexAttribute other)
        {
            if (ReferenceEquals(this, other)
                || other == null)
            {
                return new CompatibilityResult(true, null);
            }

            string errorMessage = null;

            if (_name != other._name)
            {
                errorMessage = Strings.ConflictingIndexAttributeProperty("Name", _name, other._name);
            }

            if (_order != -1
                && other._order != -1
                && _order != other._order)
            {
                errorMessage = errorMessage == null ? "" : errorMessage + (Environment.NewLine + "\t");
                errorMessage += Strings.ConflictingIndexAttributeProperty("Order", _order, other._order);
            }

            if (_isClustered != null
                && other._isClustered != null
                && _isClustered != other._isClustered)
            {
                errorMessage = errorMessage == null ? "" : errorMessage + (Environment.NewLine + "\t");
                errorMessage += Strings.ConflictingIndexAttributeProperty("IsClustered", _isClustered, other._isClustered);
            }

            if (_isUnique != null
                && other._isUnique != null
                && _isUnique != other._isUnique)
            {
                errorMessage = errorMessage == null ? "" : errorMessage + (Environment.NewLine + "\t");
                errorMessage += Strings.ConflictingIndexAttributeProperty("IsUnique", _isUnique, other._isUnique);
            }

            return new CompatibilityResult(errorMessage == null, errorMessage);
        }

        /// <summary>
        /// Merges this attribute with the given attribute and returns a new attribute containing the merged properties.
        /// </summary>
        /// <remarks>
        /// The other attribute must have the same name as this attribute. For other properties, if neither attribute
        /// specifies a value, then the property on the merged attribute is also unspecified. If one
        /// attribute but not the other specifies a value, then the property on the merged attribute gets that value.
        /// If both properties specify a value, then those values must match and the property on the merged
        /// attribute gets that value.
        /// </remarks>
        /// <param name="other">The attribute to merge with this one.</param>
        /// <returns>A new attribute with properties merged.</returns>
        /// <exception cref="InvalidOperationException">
        /// The other attribute is not compatible with this attribute as determined by the <see cref="IsCompatibleWith"/> method.
        /// </exception>
        public virtual IndexAttribute MergeWith(IndexAttribute other)
        {
            if (ReferenceEquals(this, other)
                || other == null)
            {
                return this;
            }

            var isCompatible = IsCompatibleWith(other);
            if (!isCompatible)
            {
                throw new InvalidOperationException(
                    Strings.ConflictingIndexAttribute(_name, Environment.NewLine + "\t" + isCompatible.ErrorMessage));
            }

            return new IndexAttribute(
                _name ?? other._name,
                _order != -1 ? _order : other._order,
                _isClustered ?? other._isClustered,
                _isUnique ?? other._isUnique);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "IndexAttribute: " + DetailsToString();
        }

        // For example: "{ Name: 'Foo', Order: 1, IsClustered: True, IsUnique: False }"
        internal virtual string DetailsToString()
        {
            var builder = new StringBuilder();

            var hasContent = false;
            builder.Append("{ ");

            if (_name != null)
            {
                builder.Append("Name: '").Append(_name).Append("'");
                hasContent = true;
            }

            if (_order != -1)
            {
                builder.Append(hasContent ? ", " : "").Append("Order: ").Append(_order);
                hasContent = true;
            }

            if (_isClustered.HasValue)
            {
                builder.Append(hasContent ? ", " : "").Append("IsClustered: ").Append(_isClustered);
                hasContent = true;
            }

            if (_isUnique.HasValue)
            {
                builder.Append(hasContent ? ", " : "").Append("IsUnique: ").Append(_isUnique);
                hasContent = true;
            }

            builder.Append(hasContent ? " }" : "}");

            return builder.ToString();
        }
    }
}
