// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Threading;

    /// <summary>
    ///     Represents the Edm Row Type
    /// </summary>
    public class RowType : StructuralType
    {
        private ReadOnlyMetadataCollection<EdmProperty> _properties;
        private readonly InitializerMetadata _initializerMetadata;

        internal RowType()
        {
        }

        /// <summary>
        ///     Initializes a new instance of RowType class with the given list of members
        /// </summary>
        /// <param name="properties"> properties for this row type </param>
        /// <exception cref="System.ArgumentException">Thrown if any individual property in the passed in properties argument is null</exception>
        internal RowType(IEnumerable<EdmProperty> properties)
            : this(properties, null)
        {
        }

        /// <summary>
        ///     Initializes a RowType with the given members and initializer metadata
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal RowType(IEnumerable<EdmProperty> properties, InitializerMetadata initializerMetadata)
            : base(
                GetRowTypeIdentityFromProperties(CheckProperties(properties), initializerMetadata), EdmConstants.TransientNamespace,
                (DataSpace)(-1))
        {
            // Initialize the properties. 
            if (null != properties)
            {
                foreach (var property in properties)
                {
                    AddProperty(property);
                }
            }

            _initializerMetadata = initializerMetadata;

            // Row types are immutable, so now that we're done initializing, set it
            // to be read-only.
            SetReadOnly();
        }

        /// <summary>
        ///     Gets LINQ initializer Metadata for this row type. If there is no associated
        ///     initializer type, value is null.
        /// </summary>
        internal InitializerMetadata InitializerMetadata
        {
            get { return _initializerMetadata; }
        }

        /// <summary>
        ///     Gets the built-in type kind for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.RowType" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Data.Entity.Core.Metadata.Edm.BuiltInTypeKind" /> object that represents the built-in type kind for this
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Metadata.Edm.RowType" />
        ///     .
        /// </returns>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.RowType; }
        }

        /// <summary>
        ///     Gets the list of properties on this <see cref="T:System.Data.Entity.Core.Metadata.Edm.RowType" />.
        /// </summary>
        /// <returns>
        ///     A collection of type <see cref="T:System.Data.Entity.Core.Metadata.Edm.ReadOnlyMetadataCollection`1" /> that contains the list of properties on this
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Metadata.Edm.RowType" />
        ///     .
        /// </returns>
        public virtual ReadOnlyMetadataCollection<EdmProperty> Properties
        {
            get
            {
                Debug.Assert(
                    IsReadOnly,
                    "this is a wrapper around this.Members, don't call it during metadata loading, only call it after the metadata is set to readonly");
                if (null == _properties)
                {
                    Interlocked.CompareExchange(
                        ref _properties,
                        new FilteredReadOnlyMetadataCollection<EdmProperty, EdmMember>(
                            Members, Helper.IsEdmProperty), null);
                }
                return _properties;
            }
        }

        public ReadOnlyMetadataCollection<EdmProperty> DeclaredProperties
        {
            get { return GetDeclaredOnlyMembers<EdmProperty>(); }
        }

        /// <summary>
        ///     Adds a property
        /// </summary>
        /// <param name="property"> The property to add </param>
        private void AddProperty(EdmProperty property)
        {
            Check.NotNull(property, "property");
            AddMember(property);
        }

        /// <summary>
        ///     Validates a EdmMember object to determine if it can be added to this type's
        ///     Members collection. If this method returns without throwing, it is assumed
        ///     the member is valid.
        /// </summary>
        /// <param name="member"> The member to validate </param>
        /// <exception cref="System.ArgumentException">Thrown if the member is not a EdmProperty</exception>
        internal override void ValidateMemberForAdd(EdmMember member)
        {
            Debug.Assert(Helper.IsEdmProperty(member), "Only members of type Property may be added to Row types.");
        }

        /// <summary>
        ///     Calculates the row type identity that would result from
        ///     a given set of properties.
        /// </summary>
        /// <param name="properties"> The properties that determine the row type's structure </param>
        /// <param name="initializerMetadata"> Metadata describing materialization of this row type </param>
        /// <returns> A string that identifies the row type </returns>
        private static string GetRowTypeIdentityFromProperties(IEnumerable<EdmProperty> properties, InitializerMetadata initializerMetadata)
        {
            // The row type identity is formed as follows:
            // "rowtype[" + a comma-separated list of property identities + "]"
            var identity = new StringBuilder("rowtype[");

            if (null != properties)
            {
                var i = 0;
                // For each property, append the type name and facets.
                foreach (var property in properties)
                {
                    if (i > 0)
                    {
                        identity.Append(",");
                    }
                    identity.Append("(");
                    identity.Append(property.Name);
                    identity.Append(",");
                    property.TypeUsage.BuildIdentity(identity);
                    identity.Append(")");
                    i++;
                }
            }
            identity.Append("]");

            if (null != initializerMetadata)
            {
                identity.Append(",").Append(initializerMetadata.Identity);
            }

            return identity.ToString();
        }

        private static IEnumerable<EdmProperty> CheckProperties(IEnumerable<EdmProperty> properties)
        {
            if (null != properties)
            {
                var i = 0;
                foreach (var prop in properties)
                {
                    if (prop == null)
                    {
                        throw new ArgumentException(Strings.ADP_CollectionParameterElementIsNull("properties"));
                    }
                    i++;
                }

                /*
                if (i < 1)
                {
                    throw EntityUtil.ArgumentOutOfRange("properties");
                }
                 */
            }
            return properties;
        }

        /// <summary>
        ///     EdmEquals override verifying the equivalence of all members and their type usages.
        /// </summary>
        internal override bool EdmEquals(MetadataItem item)
        {
            // short-circuit if this and other are reference equivalent
            if (ReferenceEquals(this, item))
            {
                return true;
            }

            // check type of item
            if (null == item
                || BuiltInTypeKind.RowType != item.BuiltInTypeKind)
            {
                return false;
            }
            var other = (RowType)item;

            // check each row type has the same number of members
            if (Members.Count
                != other.Members.Count)
            {
                return false;
            }

            // verify all members are equivalent
            for (var ordinal = 0; ordinal < Members.Count; ordinal++)
            {
                var thisMember = Members[ordinal];
                var otherMember = other.Members[ordinal];

                // if members are different, return false
                if (!thisMember.EdmEquals(otherMember)
                    ||
                    !thisMember.TypeUsage.EdmEquals(otherMember.TypeUsage))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     The factory method for constructing the <see cref="RowType" /> object.
        /// </summary>
        /// <param name="properties">Properties of the row type object.</param>
        /// <param name="metadataProperties">Metadata properties that will be added to the function. Can be null.</param>
        /// <returns>
        ///     A new, read-only instance of the <see cref="RowType" /> object.
        /// </returns>
        public static RowType Create(IEnumerable<EdmProperty> properties, IEnumerable<MetadataProperty> metadataProperties)
        {
            Check.NotNull(properties, "properties");

            var rowType = new RowType(properties);

            if (metadataProperties != null)
            {
                rowType.AddMetadataProperties(metadataProperties.ToList());
            }

            rowType.SetReadOnly();

            return rowType;
        }
    }
}
