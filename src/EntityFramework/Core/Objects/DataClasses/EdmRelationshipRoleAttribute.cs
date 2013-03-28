// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class EdmRelationshipAttribute : Attribute
    {
        private readonly string _relationshipNamespaceName;
        private readonly string _relationshipName;
        private readonly string _role1Name;
        private readonly string _role2Name;
        private readonly RelationshipMultiplicity _role1Multiplicity;
        private readonly RelationshipMultiplicity _role2Multiplicity;
        private readonly Type _role1Type;
        private readonly Type _role2Type;
        private readonly bool _isForeignKey;

        /// <summary>
        ///     Creates an instance of the <see cref="T:System.Data.Entity.Core.Objects.DataClasses.EdmRelationshipAttribute" /> class.
        /// </summary>
        /// <param name="relationshipNamespaceName">The name of the namespace for the association in which this entity participates.</param>
        /// <param name="relationshipName">The name of a relationship in which this entity participates. </param>
        /// <param name="role1Name">Name of the role for the type at one end of the association.</param>
        /// <param name="role1Multiplicity">
        ///     A value of <see cref="T:System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity" /> that indicates the multiplicity at one end of the association, such as one or many.
        /// </param>
        /// <param name="role1Type">The type of the entity at one end of the association.</param>
        /// <param name="role2Name">Name of the role for the type at the other end of the association.</param>
        /// <param name="role2Multiplicity">
        ///     A value of <see cref="T:System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity" /> that indicates the multiplicity at the other end of the association, such as one or many.
        /// </param>
        /// <param name="role2Type">The type of the entity at the other end of the association.</param>
        public EdmRelationshipAttribute(
            string relationshipNamespaceName,
            string relationshipName,
            string role1Name,
            RelationshipMultiplicity role1Multiplicity,
            Type role1Type,
            string role2Name,
            RelationshipMultiplicity role2Multiplicity,
            Type role2Type)
        {
            _relationshipNamespaceName = relationshipNamespaceName;
            _relationshipName = relationshipName;

            _role1Name = role1Name;
            _role1Multiplicity = role1Multiplicity;
            _role1Type = role1Type;

            _role2Name = role2Name;
            _role2Multiplicity = role2Multiplicity;
            _role2Type = role2Type;
        }

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Objects.DataClasses.EdmRelationshipAttribute" />
        ///     class.
        /// </summary>
        /// <param name="relationshipNamespaceName">The name of the namespace for the association in which this entity participates.</param>
        /// <param name="relationshipName">The name of a relationship in which this entity participates.</param>
        /// <param name="role1Name">Name of the role for the type at one end of the association.</param>
        /// <param name="role1Multiplicity">
        ///     A value of <see cref="T:System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity" /> that indicates the multiplicity at one end of the association, such as one or many.
        /// </param>
        /// <param name="role1Type">The type of the entity at one end of the association.</param>
        /// <param name="role2Name">Name of the role for the type at the other end of the association.</param>
        /// <param name="role2Multiplicity">
        ///     A value of <see cref="T:System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity" /> that indicates the multiplicity at the other end of the association, such as one or many.
        /// </param>
        /// <param name="role2Type">The type of the entity at the other end of the association.</param>
        /// <param name="isForeignKey">A value that indicates whether the relationship is based on the foreign key value.</param>
        public EdmRelationshipAttribute(
            string relationshipNamespaceName,
            string relationshipName,
            string role1Name,
            RelationshipMultiplicity role1Multiplicity,
            Type role1Type,
            string role2Name,
            RelationshipMultiplicity role2Multiplicity,
            Type role2Type,
            bool isForeignKey)
        {
            _relationshipNamespaceName = relationshipNamespaceName;
            _relationshipName = relationshipName;

            _role1Name = role1Name;
            _role1Multiplicity = role1Multiplicity;
            _role1Type = role1Type;

            _role2Name = role2Name;
            _role2Multiplicity = role2Multiplicity;
            _role2Type = role2Type;

            _isForeignKey = isForeignKey;
        }

        /// <summary>The namespace for the relationship.</summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> that is the namespace for the relationship.
        /// </returns>
        public string RelationshipNamespaceName
        {
            get { return _relationshipNamespaceName; }
        }

        /// <summary>Name of the relationship. </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> that is the name of a relationship that is defined by this
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Objects.DataClasses.EdmRelationshipAttribute" />
        ///     .
        /// </returns>
        public string RelationshipName
        {
            get { return _relationshipName; }
        }

        /// <summary>Name of the role at one end of the relationship.</summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> that is the name of the role.
        /// </returns>
        public string Role1Name
        {
            get { return _role1Name; }
        }

        /// <summary>Multiplicity at one end of the relationship.</summary>
        /// <returns>
        ///     A <see cref="T:System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity" /> value that indicates the multiplicity.
        /// </returns>
        public RelationshipMultiplicity Role1Multiplicity
        {
            get { return _role1Multiplicity; }
        }

        /// <summary>Type of the entity at one end of the relationship.</summary>
        /// <returns>
        ///     A <see cref="T:System.Type" /> that is the type of the object at this end of the association.
        /// </returns>
        public Type Role1Type
        {
            get { return _role1Type; }
        }

        /// <summary>Name of the role at the other end of the relationship.</summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> that is the name of the role.
        /// </returns>
        public string Role2Name
        {
            get { return _role2Name; }
        }

        /// <summary>Multiplicity at the other end of the relationship.</summary>
        /// <returns>
        ///     A <see cref="T:System.Data.Entity.Core.Metadata.Edm.RelationshipMultiplicity" /> value that indicates the multiplicity.
        /// </returns>
        public RelationshipMultiplicity Role2Multiplicity
        {
            get { return _role2Multiplicity; }
        }

        /// <summary>Type of the entity at the other end of the relationship.</summary>
        /// <returns>
        ///     A <see cref="T:System.Type" /> that is the type of the object t the other end of the association.
        /// </returns>
        public Type Role2Type
        {
            get { return _role2Type; }
        }

        /// <summary>Gets a Boolean value that indicates whether the relationship is based on the foreign key value.</summary>
        /// <returns>true if the relationship is based on the foreign key value; otherwise false.</returns>
        public bool IsForeignKey
        {
            get { return _isForeignKey; }
        }
    }
}
