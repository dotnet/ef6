// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.UpdateFromDatabase
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Database;

    internal abstract class AssociationIdentity
    {
        /// <summary>
        ///     Returns a string representation of this object for Tracing
        /// </summary>
        internal abstract string TraceString();

        /// <summary>
        ///     For an association mapped with an AssociationSetMapping, this will return the table that the ASM is mapped to
        ///     For an association mapped with a ReferentialConstraing, this will return the Dependent end tables of the RC.
        /// </summary>
        internal abstract IEnumerable<DatabaseObject> AssociationTables { get; }

        /// <summary>
        ///     see whether this AssociationIdentity is "covered" by the one in id2 where "covered" means
        ///     is equivalent to (but allowing for one side potentially being an AssociationIdentityForAssociationSetMapping
        ///     whereas the other is an AssociationIdentityForReferentialConstraint and allowing for mapping of
        ///     multiple S-side properties to a single C-side property)
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        internal bool IsCoveredBy(AssociationIdentity id2)
        {
            var id1 = this;
            if (id1 is AssociationIdentityForAssociationSetMapping
                && id2 is AssociationIdentityForAssociationSetMapping)
            {
                var id1AsAIFASM = id1 as AssociationIdentityForAssociationSetMapping;
                var id2AsAIFASM = id2 as AssociationIdentityForAssociationSetMapping;
                return id1AsAIFASM.IsCoveredBy(id2AsAIFASM);
            }
            else if (id1 is AssociationIdentityForReferentialConstraint
                     && id2 is AssociationIdentityForReferentialConstraint)
            {
                var id1AsAIFRC = id1 as AssociationIdentityForReferentialConstraint;
                var id2AsAIFRC = id2 as AssociationIdentityForReferentialConstraint;
                return id1AsAIFRC.IsCoveredBy(id2AsAIFRC);
            }
            else if (id1 is AssociationIdentityForAssociationSetMapping
                     && id2 is AssociationIdentityForReferentialConstraint)
            {
                var rcid = id2 as AssociationIdentityForReferentialConstraint;
                var asmid = id1 as AssociationIdentityForAssociationSetMapping;
                return IsCoveredBy(rcid, asmid);
            }
            else if (id1 is AssociationIdentityForReferentialConstraint
                     && id2 is AssociationIdentityForAssociationSetMapping)
            {
                var rcid = id1 as AssociationIdentityForReferentialConstraint;
                var asmid = id2 as AssociationIdentityForAssociationSetMapping;
                return IsCoveredBy(rcid, asmid);
            }
            else
            {
                Debug.Fail("unexpected type of AssociationIdentity found id1=" + id1.TraceString() + ", id2=" + id2.TraceString());
            }

            return false;
        }

        /// <summary>
        ///     returns true if the AssociationSetMapping is covered by the referential constraint.
        ///     "covers" means that the
        ///     1.  The columns mapped to the RC's principal keys match the columns mapped to the
        ///     association's principal keys.
        ///     2.  For each "pair" of properties that match between the RC & the ASM, the
        ///     ASM's mapped column is contained in the dependent columns of the RC.
        /// </summary>
        private static bool IsCoveredBy(AssociationIdentityForReferentialConstraint assRC, AssociationIdentityForAssociationSetMapping asmid)
        {
            // get the principal end of the association
            var principalEnd = asmid.GetPrincipalEnd();
            if (principalEnd == null)
            {
                return false;
            }

            foreach (var p1 in principalEnd.GetPropertyMappingIdentities())
            {
                // find the "equivalent" property mapping identity in the referential constraint
                // this is the property mapping identity whose Principal side columns match the
                // Principal side columns on this property of the association end.
                AssociationPropertyIdentity rcEquivalent = null;
                foreach (var p2 in assRC.ReferentialConstraintIdentity.PropertyIdentities)
                {
                    if (AssociationPropertyIdentityComparer.Instance.Compare(p1, p2) == 0)
                    {
                        rcEquivalent = p2;
                        break;
                    }
                }

                // if we couldn't find an identity in the RC whose "Principal" columns matched those
                // in the principal association end, return false
                if (rcEquivalent == null)
                {
                    return false;
                }

                //
                // now check that for each database column in "Dependent" part of the association end,
                // (ie, each column mapped as part of the association set mapping) 
                // it exists in the referential constraint's right columns (ie, the RC's dependent columns)
                //
                //  Note that we don't do an equality check here, as the conceptual property in an RC 
                //  may be mapped to multiple tables, while the association set mapping will only ever be 
                //  mapped to one table. 
                //
                foreach (var dc in p1.DependentColumns)
                {
                    if (rcEquivalent.DependentColumns.Contains(dc) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
