// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
    using BasicSchemaConstraints = SchemaConstraints<BasicKeyConstraint>;

    // Class representing a key constraint on the basic cell relations
    internal class BasicKeyConstraint : KeyConstraint<BasicCellRelation, MemberProjectedSlot>
    {
        #region Constructor

        //  Constructs a key constraint for the given relation and keyslots
        internal BasicKeyConstraint(BasicCellRelation relation, IEnumerable<MemberProjectedSlot> keySlots)
            : base(relation, keySlots, ProjectedSlot.EqualityComparer)
        {
        }

        #endregion

        #region Methods

        // effects: Propagates this constraint from the basic cell relation
        // to the corresponding view cell relation and returns the new constraint
        // If all the key slots are not being projected, returns null
        internal ViewKeyConstraint Propagate()
        {
            var viewCellRelation = CellRelation.ViewCellRelation;
            // If all slots appear in the projection, propagate key constraint
            var viewSlots = new List<ViewCellSlot>();
            foreach (var keySlot in KeySlots)
            {
                var viewCellSlot = viewCellRelation.LookupViewSlot(keySlot);
                if (viewCellSlot == null)
                {
                    // Slot is missing -- no key constraint on the view relation
                    return null;
                }
                viewSlots.Add(viewCellSlot);
            }

            // Create a key on view relation
            var viewKeyConstraint = new ViewKeyConstraint(viewCellRelation, viewSlots);
            return viewKeyConstraint;
        }

        #endregion
    }
}
