// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Diagnostics;
    using System.Text;

    // Class representing a key constraint for particular cellrelation
    internal class KeyConstraint<TCellRelation, TSlot> : InternalBase
        where TCellRelation : CellRelation
    {
        //  Constructs a key constraint for the given relation and keyslots
        //  with comparer being the comparison operator for comparing various
        //  keyslots in Implies, etc
        internal KeyConstraint(TCellRelation relation, IEnumerable<TSlot> keySlots, IEqualityComparer<TSlot> comparer)
        {
            m_relation = relation;
            m_keySlots = new Set<TSlot>(keySlots, comparer).MakeReadOnly();
            Debug.Assert(m_keySlots.Count > 0, "Key constraint being created without any keyslots?");
        }

        private readonly TCellRelation m_relation;
        private readonly Set<TSlot> m_keySlots;

        protected TCellRelation CellRelation
        {
            get { return m_relation; }
        }

        protected Set<TSlot> KeySlots
        {
            get { return m_keySlots; }
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            StringUtil.FormatStringBuilder(builder, "Key (V{0}) - ", m_relation.CellNumber);
            StringUtil.ToSeparatedStringSorted(builder, KeySlots, ", ");
            // The slots contain the name of the relation: So we skip
            // printing the CellRelation
        }
    }
}
