// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.UpdateFromDatabase
{
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Tools.XmlDesignerBase.Common.Diagnostics;

    /// <summary>
    ///     The identity of a given C-side AssociationEnd consists of the
    ///     set of DatabaseColumns to which it maps.
    /// </summary>
    internal class AssociationEndIdentity
    {
        private readonly SortedListAllowDupes<AssociationPropertyIdentity> _propertyIdentities =
            new SortedListAllowDupes<AssociationPropertyIdentity>(AssociationPropertyIdentityComparer.Instance);

        internal AssociationEndIdentity(EndProperty endProp)
        {
            _propertyIdentities = AssociationPropertyIdentity.CreateIdentitiesFromAssociationEndProperty(endProp);
        }

        internal string TraceString()
        {
            var sb = new StringBuilder("[AssociationEndIdentity");
            sb.Append(
                EFToolsTraceUtils.FormatNamedEnumerable(
                    "propertyIdentities", _propertyIdentities,
                    delegate(AssociationPropertyIdentity assocPropId) { return assocPropId.TraceString(); }));
            return sb.ToString();
        }

        internal int ColumnCount
        {
            get { return _propertyIdentities.Count; }
        }

        /// <summary>
        ///     Returns true if this end is for the "principal" end of the association.
        ///     This is determined if none of the columns mapped as part of the association
        ///     set mapping are included as part of the key's mapped columns.
        /// </summary>
        /// <returns></returns>
        internal bool IsPrincipalEnd()
        {
            foreach (var pmi in _propertyIdentities)
            {
                foreach (var associationMappedColumn in pmi.DependentColumns)
                {
                    if (pmi.PrincipalColumns.Contains(associationMappedColumn))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal IEnumerable<AssociationPropertyIdentity> GetPropertyMappingIdentities()
        {
            return _propertyIdentities;
        }

        public override bool Equals(object obj)
        {
            if (null == obj)
            {
                return false;
            }

            var objAsAssocEndIdentity = obj as AssociationEndIdentity;
            if (null == objAsAssocEndIdentity)
            {
                return false;
            }

            if (ColumnCount != objAsAssocEndIdentity.ColumnCount)
            {
                return false;
            }

            if (ColumnCount == 0)
            {
                return true;
            }

            var thisPMI = _propertyIdentities.GetEnumerator();
            var thatPMI = objAsAssocEndIdentity._propertyIdentities.GetEnumerator();

            while (thisPMI.MoveNext()
                   && thatPMI.MoveNext())
            {
                if (thisPMI.Current.Equals(thatPMI.Current) == false)
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            foreach (var pmi in _propertyIdentities)
            {
                hashCode ^= pmi.GetHashCode();
            }
            return hashCode;
        }

        /// <summary>
        ///     See whether the AssociationEndIdentity otherEndId contains a "covering" for this
        ///     i.e. for each AssociationPropertyIdentity in this._propertyIdentities see whether
        ///     otherEndId._propertyIdentities contains a principal->dependent mapping which has
        ///     at least the same principal DatabaseColumns (but possibly more) _and_ at least
        ///     the same dependent DatabaseColumns (but possibly more).
        ///     This allows for treating these AssociationEndIdentity's as identical for the purposes
        ///     of Update Model even if a given C-side property has been mapped to more than 1 S-side property.
        /// </summary>
        internal bool IsCoveredBy(AssociationEndIdentity otherEndId)
        {
            foreach (var thisApi in _propertyIdentities)
            {
                var foundCoveringApi = false;
                if (null != otherEndId)
                {
                    foreach (var otherApi in otherEndId._propertyIdentities)
                    {
                        if (thisApi.IsCoveredBy(otherApi))
                        {
                            // have found an AssociationPropertyIdentity in otherEndId._propertyIdentities which covers thisApi
                            foundCoveringApi = true;
                            break;
                        }
                    }
                }

                if (false == foundCoveringApi)
                {
                    // no covering AssociationPropertyIdentity was found for thisApi in 
                    // otherEndId._propertyIdentities
                    return false;
                }
            }

            // all AssociationPropertyIdentity's in this._propertyIdentities were covered by
            // AssociationPropertyIdentity's in other._propertyIdentities
            return true;
        }
    }
}
