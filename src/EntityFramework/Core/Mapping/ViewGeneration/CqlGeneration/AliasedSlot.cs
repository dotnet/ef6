// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Encapsulates a slot in a particular cql block.
    /// </summary>
    internal sealed class QualifiedSlot : ProjectedSlot
    {
        #region Constructor

        /// <summary>
        /// Creates a qualified slot "block_alias.slot_alias"
        /// </summary>
        internal QualifiedSlot(CqlBlock block, ProjectedSlot slot)
        {
            Debug.Assert(block != null && slot != null, "Null input to QualifiedSlot constructor");
            m_block = block;
            m_slot = slot; // Note: slot can be another qualified slot.
        }

        #endregion

        #region Fields

        private readonly CqlBlock m_block;
        private readonly ProjectedSlot m_slot;

        #endregion

        #region Methods

        /// <summary>
        /// Creates new <see cref="ProjectedSlot"/> that is qualified with <paramref name="block"/>.CqlAlias.
        /// If current slot is composite (such as <see cref="CaseStatementProjectedSlot"/>, then this method recursively qualifies all parts
        /// and returns a new deeply qualified slot (as opposed to <see cref="CqlBlock.QualifySlotWithBlockAlias"/>).
        /// </summary>
        internal override ProjectedSlot DeepQualify(CqlBlock block)
        {
            // We take the slot inside this and change the block
            var result = new QualifiedSlot(block, m_slot);
            return result;
        }

        /// <summary>
        /// Delegates alias generation to the leaf slot in the qualified chain.
        /// </summary>
        internal override string GetCqlFieldAlias(MemberPath outputMember)
        {
            // Keep looking inside the chain of qualified slots till we find a non-qualified slot and then get the alias name for it.
            var result = GetOriginalSlot().GetCqlFieldAlias(outputMember);
            return result;
        }

        /// <summary>
        /// Walks the chain of <see cref="QualifiedSlot"/>s starting from the current one and returns the original slot.
        /// </summary>
        internal ProjectedSlot GetOriginalSlot()
        {
            var slot = m_slot;
            while (true)
            {
                var qualifiedSlot = slot as QualifiedSlot;
                if (qualifiedSlot == null)
                {
                    break;
                }
                slot = qualifiedSlot.m_slot;
            }
            return slot;
        }

        internal string GetQualifiedCqlName(MemberPath outputMember)
        {
            return CqlWriter.GetQualifiedName(m_block.CqlAlias, GetCqlFieldAlias(outputMember));
        }

        internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias, int indentLevel)
        {
            Debug.Assert(blockAlias == null || m_block.CqlAlias == blockAlias, "QualifiedSlot: blockAlias mismatch");
            builder.Append(GetQualifiedCqlName(outputMember));
            return builder;
        }

        internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
        {
            return m_block.GetInput(row).Property(GetCqlFieldAlias(outputMember));
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            StringUtil.FormatStringBuilder(builder, "{0} ", m_block.CqlAlias);
            m_slot.ToCompactString(builder);
        }

        #endregion
    }
}
