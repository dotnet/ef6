// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.UpdateFromDatabase
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     The identity of a given C-side Association consists of the table
    ///     on which the Association is stored plus the set of AssociationEndIdentity
    ///     objects which represent the ends of the Association.
    /// </summary>
    internal class AssociationIdentityForAssociationSetMapping : AssociationIdentity
    {
        private DatabaseObject _assocTable;
        private readonly AssociationEndIdentity[] _ends = new AssociationEndIdentity[2];

        private AssociationIdentityForAssociationSetMapping()
        {
        }

        internal static AssociationIdentity CreateAssociationIdentity(AssociationSetMapping asm)
        {
            var sSideEntitySet = asm.StoreEntitySet.Target as StorageEntitySet;
            if (null == sSideEntitySet)
            {
                // a null sSideEntitySet indicates an unresolved AssociationSetMapping
                // we treat this as equivalent to the AssociationSet being unmapped
                return null;
            }

            var assocId = new AssociationIdentityForAssociationSetMapping();
            assocId._assocTable = DatabaseObject.CreateFromEntitySet(sSideEntitySet);
            foreach (var endProp in asm.EndProperties())
            {
                var assocEndId = new AssociationEndIdentity(endProp);
                assocId.AddAssociationEndIdentity(assocEndId);
            }

            return assocId;
        }

        internal override string TraceString()
        {
            var sb = new StringBuilder(
                "[AssociationIdentityForAssociationSetMapping assocTable=" + _assocTable.ToString());
            sb.Append(", ends[0]=" + (_ends[0] == null ? "null" : _ends[0].TraceString()));
            sb.Append(", ends[1]=" + (_ends[1] == null ? "null" : _ends[1].TraceString()));
            sb.Append("]");

            return sb.ToString();
        }

        internal DatabaseObject AssociationTable
        {
            get { return _assocTable; }
        }

        internal override IEnumerable<DatabaseObject> AssociationTables
        {
            get { yield return AssociationTable; }
        }

        private int EndCount
        {
            get
            {
                if (_ends[0] != null
                    && _ends[1] != null)
                {
                    return 2;
                }
                else if (_ends[0] == null)
                {
                    Debug.Assert(_ends[1] == null, "ends[0] should always be filled before ends[1]");
                    return 0;
                }
                else
                {
                    Debug.Assert(_ends[1] == null, "ends[1] should be null");
                    return 1;
                }
            }
        }

        internal IEnumerable<AssociationEndIdentity> Ends
        {
            get
            {
                foreach (var assocEndIdentity in _ends)
                {
                    yield return assocEndIdentity;
                }
            }
        }

        private void AddAssociationEndIdentity(AssociationEndIdentity assocEndIdentity)
        {
            if (_ends[0] == null)
            {
                _ends[0] = assocEndIdentity;
            }
            else
            {
                Debug.Assert(_ends[1] == null, "attempted to add more than two association ends!");
                if (_ends[1] == null)
                {
                    _ends[1] = assocEndIdentity;
                }
            }
        }

        /// <summary>
        ///     Returns the "principal" end of the association.  This is the end whose
        ///     association set mapping columns are not the same as the ID
        /// </summary>
        /// <returns></returns>
        internal AssociationEndIdentity GetPrincipalEnd()
        {
            foreach (var e in _ends)
            {
                if (e != null)
                {
                    if (e.IsPrincipalEnd())
                    {
                        return e;
                    }
                }
            }
            return null;
        }

        public override bool Equals(object obj)
        {
            var objAsAssocIdentity = obj as AssociationIdentityForAssociationSetMapping;
            if (null == objAsAssocIdentity)
            {
                return false;
            }

            if (!AssociationTable.Equals(objAsAssocIdentity.AssociationTable))
            {
                return false;
            }

            // see if both ends match
            var thisEndCount = EndCount;
            if (thisEndCount != objAsAssocIdentity.EndCount)
            {
                return false;
            }
            else if (thisEndCount == 2)
            {
                if (_ends[0].Equals(objAsAssocIdentity._ends[0])
                    && _ends[1].Equals(objAsAssocIdentity._ends[1]))
                {
                    return true;
                }
                else if (_ends[0].Equals(objAsAssocIdentity._ends[1])
                         && _ends[1].Equals(objAsAssocIdentity._ends[0]))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (thisEndCount == 1)
            {
                if (_ends[0].Equals(objAsAssocIdentity._ends[0]))
                {
                    return true;
                }
            }
            else if (thisEndCount == 0)
            {
                return true;
            }
            else
            {
                Debug.Assert(_ends[1] == null, "unexpected end count number " + thisEndCount + " for " + TraceString());
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = 0;

            foreach (var table in AssociationTables)
            {
                hashCode ^= table.GetHashCode();
            }

            foreach (var assocEndIdentity in _ends)
            {
                if (assocEndIdentity != null)
                {
                    hashCode ^= assocEndIdentity.GetHashCode();
                }
            }

            return hashCode;
        }

        /// <summary>
        ///     See whether the AssociationIdentityForAssociationSetMapping other contains a "covering" for this
        ///     i.e. check that the two AssociationIdentityForAssociationSetMapping objects have the same
        ///     AssociationTable and that all AssociationEndIdentity's in this.Ends are covered by
        ///     AssociationEndIdentity's in other.Ends (for definition of "covering" see
        ///     method AssociationEndIdentity.IsCoveredBy(AssociationEndIdentity))
        /// </summary>
        internal bool IsCoveredBy(AssociationIdentityForAssociationSetMapping other)
        {
            if (false == AssociationTable.Equals(other.AssociationTable))
            {
                return false;
            }

            foreach (var thisEndId in Ends)
            {
                var foundCoveringEndId = false;
                foreach (var otherEndId in other.Ends)
                {
                    // check whether thisEndId is covered by otherEndId
                    if (thisEndId.IsCoveredBy(otherEndId))
                    {
                        // have found an AssociationEndIdentity in other.Ends which covers thisEndId
                        foundCoveringEndId = true;
                        break;
                    }
                }

                if (false == foundCoveringEndId)
                {
                    // no covering AssociationEndIdentity was found for thisEndId in other.Ends
                    return false;
                }
            }

            // all AssociationEndIdentity's in this.Ends were covered by an AssociationEndIdentity in other.Ends
            return true;
        }
    }
}
