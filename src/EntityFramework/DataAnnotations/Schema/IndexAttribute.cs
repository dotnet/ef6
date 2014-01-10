// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.ComponentModel.DataAnnotations.Schema
{
    using System.Data.Entity.Infrastructure.Annotations;
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
        private string _name;
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
            internal set
            {
                DebugCheck.NotEmpty(value);
                
                _name = value;
        }
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
        /// non-clustered index.
        /// </summary>
        /// <remarks>
        /// The value of this property is only relevant if <see cref="IsClusteredConfigured"/> returns true.
        /// If <see cref="IsClusteredConfigured"/> returns false, then the value of this property is meaningless.
        /// </remarks>
        public virtual bool IsClustered
        {
            get { return _isClustered.HasValue && _isClustered.Value; }
            set { _isClustered = value; }
        }

        /// <summary>
        /// Returns true if <see cref="IsClustered"/> has been set to a value.
        /// </summary>
        public virtual bool IsClusteredConfigured
        {
            get { return _isClustered.HasValue; }
        }

        /// <summary>
        /// Set this property to true to define a unique index. Set this property to false to define a 
        /// non-unique index.
        /// </summary>
        /// <remarks>
        /// The value of this property is only relevant if <see cref="IsUniqueConfigured"/> returns true.
        /// If <see cref="IsUniqueConfigured"/> returns false, then the value of this property is meaningless.
        /// </remarks>
        public virtual bool IsUnique
        {
            get { return _isUnique.HasValue && _isUnique.Value; }
            set { _isUnique = value; }
        }

        /// <summary>
        /// Returns true if <see cref="IsUnique"/> has been set to a value.
        /// </summary>
        public virtual bool IsUniqueConfigured
        {
            get { return _isUnique.HasValue; }
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
        /// Returns true if this attribute specifies the same name and configuration as the given attribute.
        /// </summary>
        /// <param name="other">The attribute to compare.</param>
        /// <returns>True if the other object is equal to this object; otherwise false.</returns>
        protected virtual bool Equals(IndexAttribute other)
        {
            return _name == other._name
                && _order == other._order
                && _isClustered.Equals(other._isClustered)
                && _isUnique.Equals(other._isUnique);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return IndexAnnotationSerializer.SerializeIndexAttribute(this);
        }

        /// <summary>
        /// Returns true if this attribute specifies the same name and configuration as the given attribute.
        /// </summary>
        /// <param name="obj">The attribute to compare.</param>
        /// <returns>True if the other object is equal to this object; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((IndexAttribute)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_name != null ? _name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _order;
                hashCode = (hashCode * 397) ^ _isClustered.GetHashCode();
                hashCode = (hashCode * 397) ^ _isUnique.GetHashCode();
                return hashCode;
            }
        }
    }
}
