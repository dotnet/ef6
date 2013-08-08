// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// A "Rel" property is best thought of as a collocated reference (aka foreign key).
    /// Any entity may have zero or more rel-properties carried along with it (purely
    /// as a means to optimize for common relationship traversal scenarios)
    /// Although the definition is lax here, we only deal with RelProperties that
    /// are one-ended (ie) the target multiplicity is at most One.
    /// Consider for example, an Order entity with a (N:1) Order-Customer relationship. The Customer ref
    /// will be treated as a rel property for the Order entity.
    /// Similarly, the OrderLine entity may have an Order ref rel property (assuming that there was
    /// a N:1 relationship between OrderLine and Order)
    /// </summary>
    internal sealed class RelProperty
    {
        #region private state

        private readonly RelationshipType m_relationshipType;
        private readonly RelationshipEndMember m_fromEnd;
        private readonly RelationshipEndMember m_toEnd;

        #endregion

        #region constructors

        internal RelProperty(RelationshipType relationshipType, RelationshipEndMember fromEnd, RelationshipEndMember toEnd)
        {
            m_relationshipType = relationshipType;
            m_fromEnd = fromEnd;
            m_toEnd = toEnd;
        }

        #endregion

        #region public APIs

        /// <summary>
        /// The relationship
        /// </summary>
        public RelationshipType Relationship
        {
            get { return m_relationshipType; }
        }

        /// <summary>
        /// The source end of the relationship
        /// </summary>
        public RelationshipEndMember FromEnd
        {
            get { return m_fromEnd; }
        }

        /// <summary>
        /// the target end of the relationship
        /// </summary>
        public RelationshipEndMember ToEnd
        {
            get { return m_toEnd; }
        }

        /// <summary>
        /// Our definition of equality
        /// </summary>
        public override bool Equals(object obj)
        {
            var other = obj as RelProperty;
            return (other != null &&
                    Relationship.EdmEquals(other.Relationship) &&
                    FromEnd.EdmEquals(other.FromEnd) &&
                    ToEnd.EdmEquals(other.ToEnd));
        }

        /// <summary>
        /// our hash code
        /// </summary>
        public override int GetHashCode()
        {
            return ToEnd.Identity.GetHashCode();
        }

        /// <summary>
        /// String form
        /// </summary>
        [DebuggerNonUserCode]
        public override string ToString()
        {
            return m_relationshipType + ":" +
                   m_fromEnd + ":" +
                   m_toEnd;
        }

        #endregion
    }
}
