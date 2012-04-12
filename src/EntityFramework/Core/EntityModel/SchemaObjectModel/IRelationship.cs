namespace System.Data.Entity.Core.EntityModel.SchemaObjectModel
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects.DataClasses;

    /// <summary>
    /// Abstracts the properties of a relationship element
    /// </summary>
    internal interface IRelationship
    {
        /// <summary>
        /// Name of the Relationship
        /// </summary>
        string Name { get; }

        string FQName { get; }

        /// <summary>
        /// The list of ends defined in the Relationship.
        /// </summary>
        IList<IRelationshipEnd> Ends { get; }

        /// <summary>
        /// Returns the list of constraints on this relation
        /// </summary>
        IList<ReferentialConstraint> Constraints { get; }

        /// <summary>
        /// Finds an end given the roleName
        /// </summary>
        /// <param name="roleName">The role name of the end you want to find</param>
        /// <param name="end">The relationship end reference to set if the end is found</param>
        /// <returns>True if the end was found, and the passed in reference was set, False otherwise.</returns>
        bool TryGetEnd(string roleName, out IRelationshipEnd end);

        /// <summary>
        /// Is this an Association, or ...
        /// </summary>
        RelationshipKind RelationshipKind { get; }

        /// <summary>
        /// Is this a foreign key (FK) relationship?
        /// </summary>
        bool IsForeignKey { get; }
    }
}
