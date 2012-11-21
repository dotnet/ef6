// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Text;

    // Class representing a key constraint on the view cell relations
    internal class ViewKeyConstraint : KeyConstraint<ViewCellRelation, ViewCellSlot>
    {
        //  effects: Constructs a key constraint for the given relation and keyslots
        internal ViewKeyConstraint(ViewCellRelation relation, IEnumerable<ViewCellSlot> keySlots)
            :
                base(relation, keySlots, ProjectedSlot.EqualityComparer)
        {
        }

        // effects: Returns the cell corresponding to this constraint
        internal Cell Cell
        {
            get { return CellRelation.Cell; }
        }

        internal bool Implies(ViewKeyConstraint second)
        {
            if (false == ReferenceEquals(CellRelation, second.CellRelation))
            {
                return false;
            }
            // Check if the slots in this key are a subset of slots in
            // second. If it is a key in this e.g., <A.pid> then <A.pid,
            // A.foo> is certainly a key as well

            if (KeySlots.IsSubsetOf(second.KeySlots))
            {
                return true;
            }

            // Now check for subsetting taking referential constraints into account
            // Check that each slot in KeySlots can be found in second.KeySlots if we take
            // slot equivalence into account

            var secondKeySlots = new Set<ViewCellSlot>(second.KeySlots);

            foreach (var firstSlot in KeySlots)
            {
                var found = false; // Need to find a match for firstSlot

                foreach (var secondSlot in secondKeySlots)
                {
                    if (ProjectedSlot.EqualityComparer.Equals(firstSlot.SSlot, secondSlot.SSlot))
                    {
                        // S-side is the same. Check if C-side is the same as well. If so, remove it
                        // from secondKeySlots
                        // We have to check for C-side equivalence in terms of actual equality
                        // and equivalence via ref constraints. The former is needed since the
                        // S-side key slots would typically be mapped to the same C-side slot.
                        // The latter is needed since the same S-side key slot could be mapped
                        // into two slots on the C-side that are connected via a ref constraint
                        var path1 = firstSlot.CSlot.MemberPath;
                        var path2 = secondSlot.CSlot.MemberPath;
                        if (MemberPath.EqualityComparer.Equals(path1, path2)
                            || path1.IsEquivalentViaRefConstraint(path2))
                        {
                            secondKeySlots.Remove(secondSlot);
                            found = true;
                            break;
                        }
                    }
                }
                if (found == false)
                {
                    return false;
                }
            }

            // The subsetting holds when referential constraints are taken into account
            return true;
        }

        // effects: Given the fact that rightKeyConstraint is not implied by a
        // leftSide key constraint, return a useful error message -- some S
        // was not implied by the C key constraints
        internal static ErrorLog.Record GetErrorRecord(ViewKeyConstraint rightKeyConstraint)
        {
            var keySlots = new List<ViewCellSlot>(rightKeyConstraint.KeySlots);
            var table = keySlots[0].SSlot.MemberPath.Extent;
            var cSet = keySlots[0].CSlot.MemberPath.Extent;

            var tablePrefix = new MemberPath(table);
            var cSetPrefix = new MemberPath(cSet);

            var tableKey = ExtentKey.GetPrimaryKeyForEntityType(tablePrefix, (EntityType)table.ElementType);
            ExtentKey cSetKey = null;
            if (cSet is EntitySet)
            {
                cSetKey = ExtentKey.GetPrimaryKeyForEntityType(cSetPrefix, (EntityType)cSet.ElementType);
            }
            else
            {
                cSetKey = ExtentKey.GetKeyForRelationType(cSetPrefix, (AssociationType)cSet.ElementType);
            }

            var message = Strings.ViewGen_KeyConstraint_Violation(
                table.Name,
                ViewCellSlot.SlotsToUserString(rightKeyConstraint.KeySlots, false /*isFromCside*/),
                tableKey.ToUserString(),
                cSet.Name,
                ViewCellSlot.SlotsToUserString(rightKeyConstraint.KeySlots, true /*isFromCside*/),
                cSetKey.ToUserString());

            var debugMessage = StringUtil.FormatInvariant("PROBLEM: Not implied {0}", rightKeyConstraint);
            return new ErrorLog.Record(ViewGenErrorCode.KeyConstraintViolation, message, rightKeyConstraint.CellRelation.Cell, debugMessage);
        }

        // effects: Given the fact that none of the rightKeyConstraint are not implied by a
        // leftSide key constraint, return a useful error message (used for
        // the Update requirement 
        internal static ErrorLog.Record GetErrorRecord(IEnumerable<ViewKeyConstraint> rightKeyConstraints)
        {
            ViewKeyConstraint rightKeyConstraint = null;
            var keyBuilder = new StringBuilder();
            var isFirst = true;
            foreach (var rightConstraint in rightKeyConstraints)
            {
                var keyMsg = ViewCellSlot.SlotsToUserString(rightConstraint.KeySlots, true /*isFromCside*/);
                if (isFirst == false)
                {
                    keyBuilder.Append("; ");
                }
                isFirst = false;
                keyBuilder.Append(keyMsg);
                rightKeyConstraint = rightConstraint;
            }

            var keySlots = new List<ViewCellSlot>(rightKeyConstraint.KeySlots);
            var table = keySlots[0].SSlot.MemberPath.Extent;
            var cSet = keySlots[0].CSlot.MemberPath.Extent;

            var tablePrefix = new MemberPath(table);
            var tableKey = ExtentKey.GetPrimaryKeyForEntityType(tablePrefix, (EntityType)table.ElementType);

            string message;
            if (cSet is EntitySet)
            {
                message = Strings.ViewGen_KeyConstraint_Update_Violation_EntitySet(
                    keyBuilder.ToString(), cSet.Name,
                    tableKey.ToUserString(), table.Name);
            }
            else
            {
                //For a 1:* or 0..1:* association, the * side has to be mapped to the
                //key properties of the table. Fior this specific case, we give out a specific message
                //that is specific for this case.
                var associationSet = (AssociationSet)cSet;
                var endMember = Helper.GetEndThatShouldBeMappedToKey(associationSet.ElementType);
                if (endMember != null)
                {
                    message = Strings.ViewGen_AssociationEndShouldBeMappedToKey(
                        endMember.Name,
                        table.Name);
                }
                else
                {
                    message = Strings.ViewGen_KeyConstraint_Update_Violation_AssociationSet(
                        cSet.Name,
                        tableKey.ToUserString(), table.Name);
                }
            }

            var debugMessage = StringUtil.FormatInvariant("PROBLEM: Not implied {0}", rightKeyConstraint);
            return new ErrorLog.Record(
                ViewGenErrorCode.KeyConstraintUpdateViolation, message, rightKeyConstraint.CellRelation.Cell, debugMessage);
        }
    }
}
