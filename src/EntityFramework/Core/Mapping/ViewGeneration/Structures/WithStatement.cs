// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// A class to denote a part of the WITH RELATIONSHIP clause.
    /// </summary>
    internal sealed class WithRelationship : InternalBase
    {
        internal WithRelationship(
            AssociationSet associationSet,
            AssociationEndMember fromEnd,
            EntityType fromEndEntityType,
            AssociationEndMember toEnd,
            EntityType toEndEntityType,
            IEnumerable<MemberPath> toEndEntityKeyMemberPaths)
        {
            m_associationSet = associationSet;
            m_fromEnd = fromEnd;
            m_fromEndEntityType = fromEndEntityType;
            m_toEnd = toEnd;
            m_toEndEntityType = toEndEntityType;
            m_toEndEntitySet = MetadataHelper.GetEntitySetAtEnd(associationSet, toEnd);
            m_toEndEntityKeyMemberPaths = toEndEntityKeyMemberPaths;
        }

        private readonly AssociationSet m_associationSet;
        private readonly RelationshipEndMember m_fromEnd;
        private readonly EntityType m_fromEndEntityType;
        private readonly RelationshipEndMember m_toEnd;
        private readonly EntityType m_toEndEntityType;
        private readonly EntitySet m_toEndEntitySet;
        private readonly IEnumerable<MemberPath> m_toEndEntityKeyMemberPaths;

        internal EntityType FromEndEntityType
        {
            get { return m_fromEndEntityType; }
        }

        internal StringBuilder AsEsql(StringBuilder builder, string blockAlias, int indentLevel)
        {
            StringUtil.IndentNewLine(builder, indentLevel + 1);
            builder.Append("RELATIONSHIP(");
            var fields = new List<string>();
            // If the variable is a relation end, we will gets it scope Extent, e.g., CPerson1 for the CPerson end of CPersonAddress1.
            builder.Append("CREATEREF(");
            CqlWriter.AppendEscapedQualifiedName(builder, m_toEndEntitySet.EntityContainer.Name, m_toEndEntitySet.Name);
            builder.Append(", ROW(");
            foreach (var memberPath in m_toEndEntityKeyMemberPaths)
            {
                var fullFieldAlias = CqlWriter.GetQualifiedName(blockAlias, memberPath.CqlFieldAlias);
                fields.Add(fullFieldAlias);
            }
            StringUtil.ToSeparatedString(builder, fields, ", ", null);
            builder.Append(')');
            builder.Append(",");
            CqlWriter.AppendEscapedTypeName(builder, m_toEndEntityType);
            builder.Append(')');

            builder.Append(',');
            CqlWriter.AppendEscapedTypeName(builder, m_associationSet.ElementType);
            builder.Append(',');
            CqlWriter.AppendEscapedName(builder, m_fromEnd.Name);
            builder.Append(',');
            CqlWriter.AppendEscapedName(builder, m_toEnd.Name);
            builder.Append(')');
            builder.Append(' ');
            return builder;
        }

        internal DbRelatedEntityRef AsCqt(DbExpression row)
        {
            return DbExpressionBuilder.CreateRelatedEntityRef(
                m_fromEnd,
                m_toEnd,
                m_toEndEntitySet.CreateRef(
                    m_toEndEntityType, m_toEndEntityKeyMemberPaths.Select(keyMember => row.Property(keyMember.CqlFieldAlias))));
        }

        /// <summary>
        /// Not supported in this class.
        /// </summary>
        internal override void ToCompactString(StringBuilder builder)
        {
            Debug.Fail("Should not be called.");
        }
    }
}
