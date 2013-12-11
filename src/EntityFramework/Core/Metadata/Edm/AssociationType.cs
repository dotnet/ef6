// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Describes an association/relationship between two entities in the conceptual model or a foreign key relationship 
    /// between two tables in the store model. In the conceptual model the dependant class may or may not define a foreign key property.
    /// If a foreign key is defined the <see cref="IsForeignKey"/> property will be true and the <see cref="Constraint"/> property will contain details of the foreign keys
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    public class AssociationType : RelationshipType
    {
        // <summary>
        // Initializes a new instance of Association Type with the given name, namespace, version and ends
        // </summary>
        // <param name="name"> name of the association type </param>
        // <param name="namespaceName"> namespace of the association type </param>
        // <param name="foreignKey"> is this a foreign key (FK) relationship? </param>
        // <param name="dataSpace"> dataSpace in which this AssociationType belongs to </param>
        // <exception cref="System.ArgumentNullException">Thrown if either the name, namespace or version attributes are null</exception>
        internal AssociationType(
            string name,
            string namespaceName,
            bool foreignKey,
            DataSpace dataSpace)
            : base(name, namespaceName, dataSpace)
        {
            _referentialConstraints
                = new ReadOnlyMetadataCollection<ReferentialConstraint>(
                    new MetadataCollection<ReferentialConstraint>());

            _isForeignKey = foreignKey;
        }

        private readonly ReadOnlyMetadataCollection<ReferentialConstraint> _referentialConstraints;
        private FilteredReadOnlyMetadataCollection<AssociationEndMember, EdmMember> _associationEndMembers;
        private bool _isForeignKey;

        /// <summary>
        /// Gets the built-in type kind for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationType" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.BuiltInTypeKind" /> object that represents the built-in type kind for this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationType" />
        /// .
        /// </returns>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.AssociationType; }
        }

        /// <summary>
        /// Gets the list of ends for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationType" />.
        /// </summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Data.Entity.Core.Metadata.Edm.ReadOnlyMetadataCollection`1" /> that contains the list of ends for this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationType" />
        /// .
        /// </returns>
        public ReadOnlyMetadataCollection<AssociationEndMember> AssociationEndMembers
        {
            get
            {
                Debug.Assert(
                    IsReadOnly,
                    "this is a wrapper around this.Members, don't call it during metadata loading, only call it after the metadata is set to read-only");

                if (null == _associationEndMembers)
                {
                    Interlocked.CompareExchange(
                        ref _associationEndMembers,
                        new FilteredReadOnlyMetadataCollection<AssociationEndMember, EdmMember>(
                            KeyMembers, Helper.IsAssociationEndMember), null);
                }
                return _associationEndMembers;
            }
        }

        /// <summary>Gets or sets the referential constraint.</summary>
        /// <returns>The referential constraint.</returns>
        public ReferentialConstraint Constraint
        {
            get { return ReferentialConstraints.SingleOrDefault(); }
            set
            {
                Check.NotNull(value, "value");
                Util.ThrowIfReadOnly(this);

                var constraint = Constraint;

                if (constraint != null)
                {
                    ReferentialConstraints.Source.Remove(constraint);
                }

                AddReferentialConstraint(value);

                _isForeignKey = true;
            }
        }

        internal AssociationEndMember SourceEnd
        {
            get { return KeyMembers.FirstOrDefault() as AssociationEndMember; }
            set
            {
                DebugCheck.NotNull(value);
                Util.ThrowIfReadOnly(this);

                if (KeyMembers.Count == 0)
                {
                    AddKeyMember(value);
                }
                else
                {
                    SetKeyMember(0, value);
                }
            }
        }

        internal AssociationEndMember TargetEnd
        {
            get { return KeyMembers.ElementAtOrDefault(1) as AssociationEndMember; }
            set
            {
                DebugCheck.NotNull(value);
                Util.ThrowIfReadOnly(this);
                Debug.Assert(KeyMembers.Any());

                if (KeyMembers.Count == 1)
                {
                    AddKeyMember(value);
                }
                else
                {
                    SetKeyMember(1, value);
                }
            }
        }

        private void SetKeyMember(int index, AssociationEndMember member)
        {
            Debug.Assert(index < KeyMembers.Count);
            DebugCheck.NotNull(member);
            Debug.Assert(!IsReadOnly);

            var keyMember = KeyMembers.Source[index];
            var memberIndex = Members.IndexOf(keyMember);

            if (memberIndex >= 0)
            {
                Members.Source[memberIndex] = member;
            }
            else
            {
                Debug.Fail("KeyMembers and Members are out of sync.");
            }

            KeyMembers.Source[index] = member;
        }

        /// <summary>
        /// Gets the list of constraints for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationType" />.
        /// </summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Data.Entity.Core.Metadata.Edm.ReadOnlyMetadataCollection`1" /> that contains the list of constraints for this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.AssociationType" />
        /// .
        /// </returns>
        [MetadataProperty(BuiltInTypeKind.ReferentialConstraint, true)]
        public ReadOnlyMetadataCollection<ReferentialConstraint> ReferentialConstraints
        {
            get { return _referentialConstraints; }
        }

        /// <summary>Gets the Boolean property value that specifies whether the column is a foreign key.</summary>
        /// <returns>A Boolean value that specifies whether the column is a foreign key. If true, the column is a foreign key. If false (default), the column is not a foreign key.</returns>
        [MetadataProperty(PrimitiveTypeKind.Boolean, false)]
        public bool IsForeignKey
        {
            get { return _isForeignKey; }
        }

        // <summary>
        // Validates a EdmMember object to determine if it can be added to this type's
        // Members collection. If this method returns without throwing, it is assumed
        // the member is valid.
        // </summary>
        // <param name="member"> The member to validate </param>
        // <exception cref="System.ArgumentException">Thrown if the member is not an AssociationEndMember</exception>
        internal override void ValidateMemberForAdd(EdmMember member)
        {
            Debug.Assert(
                (member is AssociationEndMember),
                "Only members of type AssociationEndMember may be added to Association definitions.");
        }

        // <summary>
        // Sets this item to be read-only, once this is set, the item will never be writable again.
        // </summary>
        internal override void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                base.SetReadOnly();
                ReferentialConstraints.Source.SetReadOnly();
            }
        }

        // <summary>
        // Add the given referential constraint to the collection of referential constraints
        // </summary>
        internal void AddReferentialConstraint(ReferentialConstraint referentialConstraint)
        {
            ReferentialConstraints.Source.Add(referentialConstraint);
        }

        /// <summary>
        /// Creates a read-only AssociationType instance from the specified parameters.
        /// </summary>
        /// <param name="name">The name of the association type.</param>
        /// <param name="namespaceName">The namespace of the association type.</param>
        /// <param name="foreignKey">Flag that indicates a foreign key (FK) relationship.</param>
        /// <param name="dataSpace">The data space for the association type.</param>
        /// <param name="sourceEnd">The source association end member.</param>
        /// <param name="targetEnd">The target association end member.</param>
        /// <param name="constraint">A referential constraint.</param>
        /// <param name="metadataProperties">Metadata properties to be associated with the instance.</param>
        /// <returns>The newly created AssociationType instance.</returns>
        /// <exception cref="System.ArgumentException">The specified name is null or empty.</exception>
        /// <exception cref="System.ArgumentException">The specified namespace is null or empty.</exception>
        public static AssociationType Create(
            string name,
            string namespaceName,
            bool foreignKey,
            DataSpace dataSpace,
            AssociationEndMember sourceEnd,
            AssociationEndMember targetEnd,
            ReferentialConstraint constraint,
            IEnumerable<MetadataProperty> metadataProperties)
        {
            Check.NotEmpty(name, "name");
            Check.NotEmpty(namespaceName, "namespaceName");

            var instance = new AssociationType(name, namespaceName, foreignKey, dataSpace);

            if (sourceEnd != null)
            {
                instance.SourceEnd = sourceEnd;
            }

            if (targetEnd != null)
            {
                instance.TargetEnd = targetEnd;
            }

            if (constraint != null)
            {
                instance.AddReferentialConstraint(constraint);
            }

            if (metadataProperties != null)
            {
                instance.AddMetadataProperties(metadataProperties.ToList());
            }

            instance.SetReadOnly();

            return instance;
        }
    }
}
