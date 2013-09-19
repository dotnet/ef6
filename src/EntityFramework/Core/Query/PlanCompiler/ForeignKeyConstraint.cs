// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Information about a foreign-key constraint
    /// </summary>
    internal class ForeignKeyConstraint
    {
        #region public surface

        /// <summary>
        /// Parent key properties
        /// </summary>
        internal List<string> ParentKeys
        {
            get { return m_parentKeys; }
        }

        /// <summary>
        /// Child key properties
        /// </summary>
        internal List<string> ChildKeys
        {
            get { return m_childKeys; }
        }

        /// <summary>
        /// Get the parent-child pair
        /// </summary>
        internal ExtentPair Pair
        {
            get { return m_extentPair; }
        }

        /// <summary>
        /// Return the child rowcount
        /// </summary>
        internal RelationshipMultiplicity ChildMultiplicity
        {
            get { return m_constraint.ToRole.RelationshipMultiplicity; }
        }

        /// <summary>
        /// Get the corresponding parent (key) property, for a specific child (foreign key) property
        /// </summary>
        /// <param name="childPropertyName"> child (foreign key) property name </param>
        /// <param name="parentPropertyName"> corresponding parent property name </param>
        /// <returns> true, if the parent property was found </returns>
        internal bool GetParentProperty(string childPropertyName, out string parentPropertyName)
        {
            BuildKeyMap();
            return m_keyMap.TryGetValue(childPropertyName, out parentPropertyName);
        }

        #endregion

        #region constructors

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal ForeignKeyConstraint(RelationshipSet relationshipSet, ReferentialConstraint constraint)
        {
            var assocSet = relationshipSet as AssociationSet;
            var fromEnd = constraint.FromRole as AssociationEndMember;
            var toEnd = constraint.ToRole as AssociationEndMember;

            // Currently only Associations are supported
            if (null == assocSet
                || null == fromEnd
                || null == toEnd)
            {
                throw new NotSupportedException();
            }

            m_constraint = constraint;
            var parent = MetadataHelper.GetEntitySetAtEnd(assocSet, fromEnd);
            // relationshipSet.GetRelationshipEndExtent(constraint.FromRole);
            var child = MetadataHelper.GetEntitySetAtEnd(assocSet, toEnd); // relationshipSet.GetRelationshipEndExtent(constraint.ToRole);
            m_extentPair = new ExtentPair(parent, child);
            m_childKeys = new List<string>();
            foreach (var prop in constraint.ToProperties)
            {
                m_childKeys.Add(prop.Name);
            }

            m_parentKeys = new List<string>();
            foreach (var prop in constraint.FromProperties)
            {
                m_parentKeys.Add(prop.Name);
            }

            PlanCompiler.Assert(
                (RelationshipMultiplicity.ZeroOrOne == fromEnd.RelationshipMultiplicity
                 || RelationshipMultiplicity.One == fromEnd.RelationshipMultiplicity),
                "from-end of relationship constraint cannot have multiplicity greater than 1");
        }

        #endregion

        #region private state

        private readonly ExtentPair m_extentPair;
        private readonly List<string> m_parentKeys;
        private readonly List<string> m_childKeys;
        private readonly ReferentialConstraint m_constraint;
        private Dictionary<string, string> m_keyMap;

        #endregion

        #region private methods

        /// <summary>
        /// Build up an equivalence map of primary keys and foreign keys (ie) for each
        /// foreign key column, identify the corresponding primary key property
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private void BuildKeyMap()
        {
            if (m_keyMap != null)
            {
                return;
            }

            m_keyMap = new Dictionary<string, string>();
            IEnumerator<EdmProperty> parentProps = m_constraint.FromProperties.GetEnumerator();
            IEnumerator<EdmProperty> childProps = m_constraint.ToProperties.GetEnumerator();
            while (true)
            {
                var parentOver = !parentProps.MoveNext();
                var childOver = !childProps.MoveNext();
                PlanCompiler.Assert(parentOver == childOver, "key count mismatch");
                if (parentOver)
                {
                    break;
                }
                m_keyMap[childProps.Current.Name] = parentProps.Current.Name;
            }
        }

        #endregion
    }
}
