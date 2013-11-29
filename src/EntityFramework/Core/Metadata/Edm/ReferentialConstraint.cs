// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class represents a referential constraint between two entities specifying the "to" and "from" ends of the relationship.
    /// </summary>
    public sealed class ReferentialConstraint : MetadataItem
    {
        /// <summary>
        /// Constructs a new constraint on the relationship
        /// </summary>
        /// <param name="fromRole"> role from which the relationship originates </param>
        /// <param name="toRole"> role to which the relationship is linked/targeted to </param>
        /// <param name="fromProperties"> properties on entity type of to role which take part in the constraint </param>
        /// <param name="toProperties"> properties on entity type of from role which take part in the constraint </param>
        /// <exception cref="ArgumentNullException">Argument Null exception if any of the arguments is null</exception>
        public ReferentialConstraint(
            RelationshipEndMember fromRole,
            RelationshipEndMember toRole,
            IEnumerable<EdmProperty> fromProperties,
            IEnumerable<EdmProperty> toProperties)
        {
            Check.NotNull(fromRole, "fromRole");
            Check.NotNull(toRole, "toRole");
            Check.NotNull(fromProperties, "fromProperties");
            Check.NotNull(toProperties, "toProperties");

            _fromRole = fromRole;
            _toRole = toRole;

            _fromProperties
                = new ReadOnlyMetadataCollection<EdmProperty>(
                    new MetadataCollection<EdmProperty>(fromProperties));

            _toProperties
                = new ReadOnlyMetadataCollection<EdmProperty>(
                    new MetadataCollection<EdmProperty>(toProperties));
        }

        private RelationshipEndMember _fromRole;
        private RelationshipEndMember _toRole;

        private readonly ReadOnlyMetadataCollection<EdmProperty> _fromProperties;
        private readonly ReadOnlyMetadataCollection<EdmProperty> _toProperties;

        /// <summary>
        /// Gets the built-in type kind for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.BuiltInTypeKind" /> object that represents the built-in type kind for this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint" />
        /// .
        /// </returns>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.ReferentialConstraint; }
        }

        // <summary>
        // Returns the identity for this constraint
        // </summary>
        internal override string Identity
        {
            get { return FromRole.Name + "_" + ToRole.Name; }
        }

        /// <summary>
        /// Gets the "from role" that takes part in this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint" />
        /// .
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.RelationshipEndMember" /> object that represents the "from role" that takes part in this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint" />
        /// .
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if value passed into setter is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the ReferentialConstraint instance is in ReadOnly state</exception>
        [MetadataProperty(BuiltInTypeKind.RelationshipEndMember, false)]
        public RelationshipEndMember FromRole
        {
            get { return _fromRole; }
            set
            {
                DebugCheck.NotNull(value);
                Util.ThrowIfReadOnly(this);

                _fromRole = value;
            }
        }

        /// <summary>
        /// Gets the "to role" that takes part in this <see cref="T:System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.RelationshipEndMember" /> object that represents the "to role" that takes part in this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint" />
        /// .
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if value passed into setter is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the ReferentialConstraint instance is in ReadOnly state</exception>
        [MetadataProperty(BuiltInTypeKind.RelationshipEndMember, false)]
        public RelationshipEndMember ToRole
        {
            get { return _toRole; }
            set
            {
                DebugCheck.NotNull(value);
                Util.ThrowIfReadOnly(this);

                _toRole = value;
            }
        }

        internal AssociationEndMember DependentEnd
        {
            get { return (AssociationEndMember)ToRole; }
        }

        /// <summary>
        /// Gets the list of properties for the "from role" on which this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint" />
        /// is defined.
        /// </summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Data.Entity.Core.Metadata.Edm.ReadOnlyMetadataCollection`1" /> that contains the list of properties for "from role" on which this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint" />
        /// is defined.
        /// </returns>
        [MetadataProperty(BuiltInTypeKind.EdmProperty, true)]
        public ReadOnlyMetadataCollection<EdmProperty> FromProperties
        {
            get
            {
                if (!IsReadOnly
                    && _fromProperties.Count == 0)
                {
                    _fromRole.GetEntityType().KeyMembers
                             .Each(p => _fromProperties.Source.Add((EdmProperty)p));
                }

                return _fromProperties;
            }
        }

        /// <summary>
        /// Gets the list of properties for the "to role" on which this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint" />
        /// is defined.
        /// </summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Data.Entity.Core.Metadata.Edm.ReadOnlyMetadataCollection`1" /> that contains the list of properties for the "to role" on which this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint" />
        /// is defined.
        /// </returns>
        [MetadataProperty(BuiltInTypeKind.EdmProperty, true)]
        public ReadOnlyMetadataCollection<EdmProperty> ToProperties
        {
            get { return _toProperties; }
        }

        /// <summary>
        /// Returns the combination of the names of the
        /// <see
        ///     cref="P:System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint.FromRole" />
        /// and the
        /// <see
        ///     cref="P:System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint.ToRole" />
        /// .
        /// </summary>
        /// <returns>
        /// The combination of the names of the
        /// <see
        ///     cref="P:System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint.FromRole" />
        /// and the
        /// <see
        ///     cref="P:System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint.ToRole" />
        /// .
        /// </returns>
        public override string ToString()
        {
            return FromRole.Name + "_" + ToRole.Name;
        }

        // <summary>
        // Sets this item to be read-only, once this is set, the item will never be writable again.
        // </summary>
        internal override void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                FromProperties.Source.SetReadOnly();
                ToProperties.Source.SetReadOnly();

                base.SetReadOnly();

                var fromRole = FromRole;
                if (fromRole != null)
                {
                    fromRole.SetReadOnly();
                }

                var toRole = ToRole;
                if (toRole != null)
                {
                    toRole.SetReadOnly();
                }
            }
        }

        internal string BuildConstraintExceptionMessage()
        {
            var fromType = FromProperties.First().DeclaringType.Name;
            var toType = ToProperties.First().DeclaringType.Name;

            var fromProps = new StringBuilder();
            var toProps = new StringBuilder();
            for (var i = 0; i < FromProperties.Count; ++i)
            {
                if (i > 0)
                {
                    fromProps.Append(", ");
                    toProps.Append(", ");
                }

                fromProps.Append(fromType).Append('.').Append(FromProperties[i]);
                toProps.Append(toType).Append('.').Append(ToProperties[i]);
            }

            return Strings.RelationshipManager_InconsistentReferentialConstraintProperties(
                fromProps.ToString(), toProps.ToString());
        }
    }
}
