// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.UpdateFromDatabase
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class AssociationIdentityForReferentialConstraint : AssociationIdentity
    {
        private readonly ReferentialConstraintIdentity _referentialConstraintIdentity;

        private AssociationIdentityForReferentialConstraint(ReferentialConstraint rc)
        {
            _referentialConstraintIdentity = ReferentialConstraintIdentity.CreateReferentialConstraintIdentity(rc);
        }

        internal override string TraceString()
        {
            return "[" + typeof(AssociationIdentityForReferentialConstraint).Name +
                   " referentialConstraintIdentity=" + _referentialConstraintIdentity.TraceString() + "]";
        }

        internal ReferentialConstraintIdentity ReferentialConstraintIdentity
        {
            get { return _referentialConstraintIdentity; }
        }

        /// <summary>
        ///     returns all the tables mapped to columns on the "Dependent" end of the referential constraing
        /// </summary>
        internal override IEnumerable<DatabaseObject> AssociationTables
        {
            get
            {
                var allTables = new HashSet<DatabaseObject>();
                foreach (var pmi in _referentialConstraintIdentity.PropertyIdentities)
                {
                    foreach (var dc in pmi.DependentColumns)
                    {
                        allTables.Add(dc.Table);
                    }
                }
                return allTables;
            }
        }

        public override bool Equals(object obj)
        {
            var that = obj as AssociationIdentityForReferentialConstraint;
            if (that != null)
            {
                return _referentialConstraintIdentity.Equals(that._referentialConstraintIdentity);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _referentialConstraintIdentity.GetHashCode();
        }

        /// <summary>
        ///     see whether the ReferentialConstraintIdentity in other contains a "covering" for
        ///     this._referentialConstraintIdentity (for definition of "covering" see
        ///     method ReferentialConstraintIdentity.IsCoveredBy(ReferentialConstraintIdentity))
        /// </summary>
        internal bool IsCoveredBy(AssociationIdentityForReferentialConstraint other)
        {
            if (_referentialConstraintIdentity == null)
            {
                // if _referentialConstraintIdentity is null then anything covers it
                return true;
            }

            return _referentialConstraintIdentity.IsCoveredBy(other._referentialConstraintIdentity);
        }

        internal static AssociationIdentity CreateAssociationIdentity(ReferentialConstraint rc)
        {
            if (rc == null)
            {
                Debug.Fail("You passed a null Referential Constraint to CreateAssociationIdentity!");
            }
            else
            {
                var id = new AssociationIdentityForReferentialConstraint(rc);
                return id;
            }
            return null;
        }
    }
}
