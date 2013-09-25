// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Represent the edm navigation property class
    /// </summary>
    public sealed class NavigationProperty : EdmMember
    {
        // <summary>
        // Initializes a new instance of the navigation property class
        // </summary>
        // <param name="name"> name of the navigation property </param>
        // <param name="typeUsage"> TypeUsage object containing the navigation property type and its facets </param>
        // <exception cref="System.ArgumentNullException">Thrown if name or typeUsage arguments are null</exception>
        // <exception cref="System.ArgumentException">Thrown if name argument is empty string</exception>
        internal NavigationProperty(string name, TypeUsage typeUsage)
            : base(name, typeUsage)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(typeUsage, "typeUsage");

            _accessor = new NavigationPropertyAccessor(name);
        }

        /// <summary>
        /// Gets the built-in type kind for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.NavigationProperty" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.BuiltInTypeKind" /> object that represents the built-in type kind for this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.NavigationProperty" />
        /// .
        /// </returns>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.NavigationProperty; }
        }

        internal const string RelationshipTypeNamePropertyName = "RelationshipType";
        internal const string ToEndMemberNamePropertyName = "ToEndMember";

        // <summary>
        // cached dynamic methods to access the property values from a CLR instance
        // </summary>
        private readonly NavigationPropertyAccessor _accessor;

        /// <summary>Gets the relationship type that this navigation property operates on.</summary>
        /// <returns>The relationship type that this navigation property operates on.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the NavigationProperty instance is in ReadOnly state</exception>
        [MetadataProperty(BuiltInTypeKind.RelationshipType, false)]
        public RelationshipType RelationshipType { get; internal set; }

        /// <summary>Gets the "to" relationship end member of this navigation.</summary>
        /// <returns>The "to" relationship end member of this navigation.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the NavigationProperty instance is in ReadOnly state</exception>
        [MetadataProperty(BuiltInTypeKind.RelationshipEndMember, false)]
        public RelationshipEndMember ToEndMember { get; internal set; }

        /// <summary>Gets the "from" relationship end member in this navigation.</summary>
        /// <returns>The "from" relationship end member in this navigation.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the NavigationProperty instance is in ReadOnly state</exception>
        [MetadataProperty(BuiltInTypeKind.RelationshipEndMember, false)]
        public RelationshipEndMember FromEndMember { get; internal set; }

        internal AssociationType Association
        {
            get { return (AssociationType)RelationshipType; }
        }

        internal AssociationEndMember ResultEnd
        {
            get { return (AssociationEndMember)ToEndMember; }
        }

        internal NavigationPropertyAccessor Accessor
        {
            get { return _accessor; }
        }

        /// <summary>
        /// Where the given navigation property is on the dependent end of a referential constraint,
        /// returns the foreign key properties. Otherwise, returns an empty set. We will return the members in the order
        /// of the principal end key properties.
        /// </summary>
        /// <returns>A collection of the foreign key properties.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IEnumerable<EdmProperty> GetDependentProperties()
        {
            // Get the declared type
            var associationType = (AssociationType)RelationshipType;
            Debug.Assert(
                associationType.ReferentialConstraints != null,
                "ReferenceConstraints cannot be null");

            if (associationType.ReferentialConstraints.Count > 0)
            {
                var rc = associationType.ReferentialConstraints[0];
                var dependentEndMember = rc.ToRole;

                if (dependentEndMember.EdmEquals(FromEndMember))
                {
                    //Order the dependant properties in the order of principal end's key members.
                    var keyMembers = rc.FromRole.GetEntityType().KeyMembers;
                    var dependantProperties = new List<EdmProperty>(keyMembers.Count);
                    for (var i = 0; i < keyMembers.Count; i++)
                    {
                        dependantProperties.Add(rc.ToProperties[rc.FromProperties.IndexOf(((EdmProperty)keyMembers[i]))]);
                    }
                    return new ReadOnlyCollection<EdmProperty>(dependantProperties);
                }
            }

            return Enumerable.Empty<EdmProperty>();
        }

        internal override void SetReadOnly()
        {
            if (!IsReadOnly
                && (ToEndMember != null)
                && (ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.One))
            {
                // Correct our nullability if the multiplicity of the target end has changed. 
                TypeUsage = TypeUsage.ShallowCopy(Facet.Create(NullableFacetDescription, false));
            }

            base.SetReadOnly();
        }

        /// <summary>
        /// Creates a NavigationProperty instance from the specified parameters.
        /// </summary>
        /// <param name="name">The name of the navigation property.</param>
        /// <param name="typeUsage">Specifies the navigation property type and its facets.</param>
        /// <param name="relationshipType">The relationship type for the navigation.</param>
        /// <param name="from">The source end member in the navigation.</param>
        /// <param name="to">The target end member in the navigation.</param>
        /// <param name="metadataProperties">The metadata properties of the navigation property.</param>
        /// <returns>The newly created NavigationProperty instance.</returns>
        public static NavigationProperty Create(
            string name,
            TypeUsage typeUsage,
            RelationshipType relationshipType,
            RelationshipEndMember from,
            RelationshipEndMember to,
            IEnumerable<MetadataProperty> metadataProperties)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(typeUsage, "typeUsage");

            var instance = new NavigationProperty(name, typeUsage);

            instance.RelationshipType = relationshipType;
            instance.FromEndMember = from;
            instance.ToEndMember = to;

            if (metadataProperties != null)
            {
                instance.AddMetadataProperties(metadataProperties.ToList());
            }

            instance.SetReadOnly();

            return instance;
        }
    }
}
