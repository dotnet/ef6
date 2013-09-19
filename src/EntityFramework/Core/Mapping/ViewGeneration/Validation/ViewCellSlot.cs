// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Represents a slot that is projected by C and S queries in a cell.
    /// </summary>
    internal class ViewCellSlot : ProjectedSlot
    {
        // effects: 
        /// <summary>
        /// Creates a view cell slot that corresponds to <paramref name="slotNum" /> in some cell. The <paramref name="cSlot" /> and
        /// <paramref
        ///     name="sSlot" />
        /// represent the
        /// slots in the left and right queries of the view cell.
        /// </summary>
        internal ViewCellSlot(int slotNum, MemberProjectedSlot cSlot, MemberProjectedSlot sSlot)
        {
            m_slotNum = slotNum;
            m_cSlot = cSlot;
            m_sSlot = sSlot;
        }

        private readonly int m_slotNum;
        private readonly MemberProjectedSlot m_cSlot;
        private readonly MemberProjectedSlot m_sSlot;

        /// <summary>
        /// Returns the slot corresponding to the left cellquery.
        /// </summary>
        internal MemberProjectedSlot CSlot
        {
            get { return m_cSlot; }
        }

        /// <summary>
        /// Returns the slot corresponding to the right cellquery.
        /// </summary>
        internal MemberProjectedSlot SSlot
        {
            get { return m_sSlot; }
        }

        protected override bool IsEqualTo(ProjectedSlot right)
        {
            var rightSlot = right as ViewCellSlot;
            if (rightSlot == null)
            {
                return false;
            }

            return m_slotNum == rightSlot.m_slotNum &&
                   EqualityComparer.Equals(m_cSlot, rightSlot.m_cSlot) &&
                   EqualityComparer.Equals(m_sSlot, rightSlot.m_sSlot);
        }

        protected override int GetHash()
        {
            return EqualityComparer.GetHashCode(m_cSlot) ^
                   EqualityComparer.GetHashCode(m_sSlot) ^
                   m_slotNum;
        }

        /// <summary>
        /// Given a list of <paramref name="slots" />, converts the left/right slots (if left is true/false) to a human-readable string.
        /// </summary>
        internal static string SlotsToUserString(IEnumerable<ViewCellSlot> slots, bool isFromCside)
        {
            var builder = new StringBuilder();
            var first = true;
            foreach (var slot in slots)
            {
                if (false == first)
                {
                    builder.Append(", ");
                }
                builder.Append(SlotToUserString(slot, isFromCside));
                first = false;
            }
            return builder.ToString();
        }

        internal static string SlotToUserString(ViewCellSlot slot, bool isFromCside)
        {
            var actualSlot = isFromCside ? slot.CSlot : slot.SSlot;
            var result = StringUtil.FormatInvariant("{0}", actualSlot);
            return result;
        }

        /// <summary>
        /// Not supported in this class.
        /// </summary>
        internal override string GetCqlFieldAlias(MemberPath outputMember)
        {
            Debug.Fail("Should not be called.");
            return null; // To keep the compiler happy
        }

        /// <summary>
        /// Not supported in this class.
        /// </summary>
        internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias, int indentLevel)
        {
            Debug.Fail("Should not be called.");
            return null; // To keep the compiler happy
        }

        /// <summary>
        /// Not supported in this class.
        /// </summary>
        internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
        {
            Debug.Fail("Should not be called.");
            return null;
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            builder.Append('<');
            StringUtil.FormatStringBuilder(builder, "{0}", m_slotNum);
            builder.Append(':');
            m_cSlot.ToCompactString(builder);
            builder.Append('-');
            m_sSlot.ToCompactString(builder);
            builder.Append('>');
        }
    }
}
