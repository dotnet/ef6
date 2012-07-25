// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    /// A helper class for all rel-properties
    /// </summary>
    internal sealed class RelPropertyHelper
    {
        #region private state

        private readonly Dictionary<EntityTypeBase, List<RelProperty>> _relPropertyMap;
        private readonly HashSet<RelProperty> _interestingRelProperties;

        #endregion

        #region private methods

        /// <summary>
        /// Add the rel property induced by the specified relationship, (if the target
        /// end has a multiplicity of one)
        /// We only keep track of rel-properties that are "interesting" 
        /// </summary>
        /// <param name="associationType">the association relationship</param>
        /// <param name="fromEnd">source end of the relationship traversal</param>
        /// <param name="toEnd">target end of the traversal</param>
        private void AddRelProperty(
            AssociationType associationType,
            AssociationEndMember fromEnd, AssociationEndMember toEnd)
        {
            if (toEnd.RelationshipMultiplicity
                == RelationshipMultiplicity.Many)
            {
                return;
            }
            var prop = new RelProperty(associationType, fromEnd, toEnd);
            if (_interestingRelProperties == null
                ||
                !_interestingRelProperties.Contains(prop))
            {
                return;
            }

            var entityType = ((RefType)fromEnd.TypeUsage.EdmType).ElementType;
            List<RelProperty> propList;
            if (!_relPropertyMap.TryGetValue(entityType, out propList))
            {
                propList = new List<RelProperty>();
                _relPropertyMap[entityType] = propList;
            }
            propList.Add(prop);
        }

        /// <summary>
        /// Add any rel properties that are induced by the supplied relationship
        /// </summary>
        /// <param name="relationshipType">the relationship</param>
        private void ProcessRelationship(RelationshipType relationshipType)
        {
            var associationType = relationshipType as AssociationType;
            if (associationType == null)
            {
                return;
            }

            // Handle only binary associations
            if (associationType.AssociationEndMembers.Count != 2)
            {
                return;
            }

            var end0 = associationType.AssociationEndMembers[0];
            var end1 = associationType.AssociationEndMembers[1];

            AddRelProperty(associationType, end0, end1);
            AddRelProperty(associationType, end1, end0);
        }

        #endregion

        #region constructors

        internal RelPropertyHelper(MetadataWorkspace ws, HashSet<RelProperty> interestingRelProperties)
        {
            _relPropertyMap = new Dictionary<EntityTypeBase, List<RelProperty>>();
            _interestingRelProperties = interestingRelProperties;

            foreach (var relationshipType in ws.GetItems<RelationshipType>(DataSpace.CSpace))
            {
                ProcessRelationship(relationshipType);
            }
        }

        #endregion

        #region public APIs

        /// <summary>
        /// Get the rel properties declared by this type (and *not* by any of its subtypes)
        /// </summary>
        /// <param name="entityType">the entity type</param>
        /// <returns>set of rel properties declared for this type</returns>
        internal IEnumerable<RelProperty> GetDeclaredOnlyRelProperties(EntityTypeBase entityType)
        {
            List<RelProperty> relProperties;
            if (_relPropertyMap.TryGetValue(entityType, out relProperties))
            {
                foreach (var p in relProperties)
                {
                    yield return p;
                }
            }
            yield break;
        }

        /// <summary>
        /// Get the rel-properties of this entity and its supertypes (starting from the root)
        /// </summary>
        /// <param name="entityType">the entity type</param>
        /// <returns>set of rel-properties for this entity type (and its supertypes)</returns>
        internal IEnumerable<RelProperty> GetRelProperties(EntityTypeBase entityType)
        {
            if (entityType.BaseType != null)
            {
                foreach (var p in GetRelProperties(entityType.BaseType as EntityTypeBase))
                {
                    yield return p;
                }
            }

            foreach (var p in GetDeclaredOnlyRelProperties(entityType))
            {
                yield return p;
            }
        }

        #endregion
    }
}
