// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using md = System.Data.Entity.Core.Metadata.Edm;

//using System.Diagnostics; // Please use PlanCompiler.Assert instead of Debug.Assert in this class...

// It is fine to use Debug.Assert in cases where you assert an obvious thing that is supposed
// to prevent from simple mistakes during development (e.g. method argument validation 
// in cases where it was you who created the variables or the variables had already been validated or 
// in "else" clauses where due to code changes (e.g. adding a new value to an enum type) the default 
// "else" block is chosen why the new condition should be treated separately). This kind of asserts are 
// (can be) helpful when developing new code to avoid simple mistakes but have no or little value in 
// the shipped product. 
// PlanCompiler.Assert *MUST* be used to verify conditions in the trees. These would be assumptions 
// about how the tree was built etc. - in these cases we probably want to throw an exception (this is
// what PlanCompiler.Assert does when the condition is not met) if either the assumption is not correct 
// or the tree was built/rewritten not the way we thought it was.
// Use your judgment - if you rather remove an assert than ship it use Debug.Assert otherwise use
// PlanCompiler.Assert.

//
// The ConstraintManager module manages foreign key constraints for a query. It reshapes
// referential constraints supplied by metadata into a more useful form.
//

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;

    /// <summary>
    /// Keeps track of all foreign key relationships
    /// </summary>
    internal class ConstraintManager
    {
        #region public methods

        /// <summary>
        /// Is there a parent child relationship between table1 and table2 ?
        /// </summary>
        /// <param name="table1"> parent table ? </param>
        /// <param name="table2"> child table ? </param>
        /// <param name="constraints"> list of constraints ? </param>
        /// <returns> true if there is at least one constraint </returns>
        internal bool IsParentChildRelationship(
            md.EntitySetBase table1, md.EntitySetBase table2,
            out List<ForeignKeyConstraint> constraints)
        {
            LoadRelationships(table1.EntityContainer);
            LoadRelationships(table2.EntityContainer);

            var extentPair = new ExtentPair(table1, table2);
            return m_parentChildRelationships.TryGetValue(extentPair, out constraints);
        }

        /// <summary>
        /// Load all relationships in this entity container
        /// </summary>
        internal void LoadRelationships(md.EntityContainer entityContainer)
        {
            // Check to see if I've already loaded information for this entity container
            if (m_entityContainerMap.ContainsKey(entityContainer))
            {
                return;
            }

            // Load all relationships from this entitycontainer
            foreach (var e in entityContainer.BaseEntitySets)
            {
                var relationshipSet = e as md.RelationshipSet;
                if (relationshipSet == null)
                {
                    continue;
                }

                // Relationship sets can only contain relationships
                var relationshipType = relationshipSet.ElementType;
                var assocType = relationshipType as md.AssociationType;

                //
                // Handle only binary Association relationships for now
                //
                if (null == assocType
                    || !IsBinary(relationshipType))
                {
                    continue;
                }

                foreach (var constraint in assocType.ReferentialConstraints)
                {
                    List<ForeignKeyConstraint> fkConstraintList;
                    var fkConstraint = new ForeignKeyConstraint(relationshipSet, constraint);
                    if (!m_parentChildRelationships.TryGetValue(fkConstraint.Pair, out fkConstraintList))
                    {
                        fkConstraintList = new List<ForeignKeyConstraint>();
                        m_parentChildRelationships[fkConstraint.Pair] = fkConstraintList;
                    }
                    //
                    // Theoretically, we can have more than one fk constraint between
                    // the 2 tables (though, it is unlikely)
                    //
                    fkConstraintList.Add(fkConstraint);
                }
            }

            // Mark this entity container as already loaded
            m_entityContainerMap[entityContainer] = entityContainer;
        }

        #endregion

        #region constructors

        internal ConstraintManager()
        {
            m_entityContainerMap = new Dictionary<md.EntityContainer, md.EntityContainer>();
            m_parentChildRelationships = new Dictionary<ExtentPair, List<ForeignKeyConstraint>>();
        }

        #endregion

        #region private state

        private readonly Dictionary<md.EntityContainer, md.EntityContainer> m_entityContainerMap;
        private readonly Dictionary<ExtentPair, List<ForeignKeyConstraint>> m_parentChildRelationships;

        #endregion

        #region private methods

        /// <summary>
        /// Is this relationship a binary relationship (ie) does it have exactly 2 end points?
        /// This should ideally be a method supported by RelationType itself
        /// </summary>
        /// <returns> true, if this is a binary relationship </returns>
        private static bool IsBinary(md.RelationshipType relationshipType)
        {
            var endCount = 0;
            foreach (var member in relationshipType.Members)
            {
                if (member is md.RelationshipEndMember)
                {
                    endCount++;
                    if (endCount > 2)
                    {
                        return false;
                    }
                }
            }
            return (endCount == 2);
        }

        #endregion
    }
}
