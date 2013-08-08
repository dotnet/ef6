// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Denotes the fact that the key of the current tuple comes from a specific extent, or association role.
    /// </summary>
    internal sealed class RoleBoolean : TrueFalseLiteral
    {
        internal RoleBoolean(EntitySetBase extent)
        {
            m_metadataItem = extent;
        }

        internal RoleBoolean(AssociationSetEnd end)
        {
            m_metadataItem = end;
        }

        private readonly MetadataItem m_metadataItem;

        /// <summary>
        /// Not supported in this class.
        /// </summary>
        internal override StringBuilder AsEsql(StringBuilder builder, string blockAlias, bool skipIsNotNull)
        {
            Debug.Fail("Should not be called.");
            return null; // To keep the compiler happy
        }

        /// <summary>
        /// Not supported in this class.
        /// </summary>
        internal override DbExpression AsCqt(DbExpression row, bool skipIsNotNull)
        {
            Debug.Fail("Should not be called.");
            return null; // To keep the compiler happy
        }

        internal override StringBuilder AsUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
        {
            var end = m_metadataItem as AssociationSetEnd;
            if (end != null)
            {
                builder.Append(Strings.ViewGen_AssociationSet_AsUserString(blockAlias, end.Name, end.ParentAssociationSet));
            }
            else
            {
                builder.Append(Strings.ViewGen_EntitySet_AsUserString(blockAlias, m_metadataItem.ToString()));
            }
            return builder;
        }

        internal override StringBuilder AsNegatedUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
        {
            var end = m_metadataItem as AssociationSetEnd;
            if (end != null)
            {
                builder.Append(Strings.ViewGen_AssociationSet_AsUserString_Negated(blockAlias, end.Name, end.ParentAssociationSet));
            }
            else
            {
                builder.Append(Strings.ViewGen_EntitySet_AsUserString_Negated(blockAlias, m_metadataItem.ToString()));
            }
            return builder;
        }

        internal override void GetRequiredSlots(MemberProjectionIndex projectedSlotMap, bool[] requiredSlots)
        {
            throw new NotImplementedException();
        }

        protected override bool IsEqualTo(BoolLiteral right)
        {
            var rightBoolean = right as RoleBoolean;
            if (rightBoolean == null)
            {
                return false;
            }
            return m_metadataItem == rightBoolean.m_metadataItem;
        }

        public override int GetHashCode()
        {
            return m_metadataItem.GetHashCode();
        }

        internal override BoolLiteral RemapBool(Dictionary<MemberPath, MemberPath> remap)
        {
            return this;
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            var end = m_metadataItem as AssociationSetEnd;
            if (end != null)
            {
                builder.Append("InEnd:" + end.ParentAssociationSet + "_" + end.Name);
            }
            else
            {
                builder.Append("InSet:" + m_metadataItem);
            }
        }
    }
}
