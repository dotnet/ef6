// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.UpdateFromDatabase
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Tools.XmlDesignerBase.Common.Diagnostics;

    /// <summary>
    ///     This class defines the "Identity" of a property that participates in an association.
    ///     This can be either through an AssociationSetMapping, or through a C-side ReferentialConstratint.
    ///     Since a ReferentialConstraint on the c-side can be defined over properties that are mapped to multiple
    ///     columns in the database (eg, in TPC scenarios), we define the "principal" & "dependent" ends of the
    ///     Association property via a list of columns.
    ///     For AssociationSetMappings, we look at the C-side column referenced in the ASM - the "Principal" end identity is the
    ///     set of columns that this c-side column is mapped to.  For the Dependent end, we use the columns that the ASM has mapped.
    /// </summary>
    internal class AssociationPropertyIdentity
    {
        private SortedListAllowDupes<DatabaseColumn> _principalColumns =
            new SortedListAllowDupes<DatabaseColumn>(new DatabaseColumnComparer());

        private SortedListAllowDupes<DatabaseColumn> _dependentColumns =
            new SortedListAllowDupes<DatabaseColumn>(new DatabaseColumnComparer());

        internal static SortedListAllowDupes<AssociationPropertyIdentity> CreateIdentitiesFromReferentialConstraint(
            ReferentialConstraint referentialConstraint)
        {
            var principal = referentialConstraint.Principal;
            var dependent = referentialConstraint.Dependent;

            var identities = new SortedListAllowDupes<AssociationPropertyIdentity>(AssociationPropertyIdentityComparer.Instance);

            if (principal != null
                && dependent != null)
            {
                // TODO:  deal with case where counts differ. 
                if (principal.PropertyRefs.Count == dependent.PropertyRefs.Count)
                {
                    var pPropRefs = principal.PropertyRefs.GetEnumerator();
                    var dPropRefs = dependent.PropertyRefs.GetEnumerator();

                    while (pPropRefs.MoveNext()
                           && dPropRefs.MoveNext())
                    {
                        var pProp = pPropRefs.Current.Name.Target as ConceptualProperty;
                        var dProp = dPropRefs.Current.Name.Target as ConceptualProperty;

                        if (pProp != null
                            && dProp != null)
                        {
                            var id = new AssociationPropertyIdentity();
                            id.PrincipalColumns = GetMappedColumnsForConceptualProperty(pProp);
                            id.DependentColumns = GetMappedColumnsForConceptualProperty(dProp);
                            identities.Add(id);
                        }
                    }
                }
            }
            return identities;
        }

        internal static SortedListAllowDupes<AssociationPropertyIdentity> CreateIdentitiesFromAssociationEndProperty(EndProperty endProp)
        {
            var props = new SortedListAllowDupes<AssociationPropertyIdentity>(AssociationPropertyIdentityComparer.Instance);

            foreach (var sProp in endProp.ScalarProperties())
            {
                var id = new AssociationPropertyIdentity();
                var keyProperty = sProp.Name.Target as ConceptualProperty;

                id.PrincipalColumns = GetMappedColumnsForConceptualProperty(keyProperty);
                id.DependentColumns = new SortedListAllowDupes<DatabaseColumn>(new DatabaseColumnComparer());

                var sSideProperty = sProp.ColumnName.Target;
                if (null != sSideProperty)
                {
                    var stc = DatabaseColumn.CreateFromProperty(sSideProperty);
                    id.DependentColumns.Add(stc);
                    props.Add(id);
                }
            }
            return props;
        }

        internal string TraceString()
        {
            var sb = new StringBuilder("[AssociationPropertyIdentity");
            sb.Append(
                " " + EFToolsTraceUtils.FormatNamedEnumerable(
                    "principalColumns", _principalColumns, delegate(DatabaseColumn dc) { return dc.ToString(); }));
            sb.Append(
                ", " + EFToolsTraceUtils.FormatNamedEnumerable(
                    "dependentColumns", _dependentColumns, delegate(DatabaseColumn dc) { return dc.ToString(); }));
            sb.Append("]");

            return sb.ToString();
        }

        private static SortedListAllowDupes<DatabaseColumn> GetMappedColumnsForConceptualProperty(ConceptualProperty property)
        {
            var columns = new SortedListAllowDupes<DatabaseColumn>(new DatabaseColumnComparer());
            if (property != null)
            {
                foreach (var sp in property.GetAntiDependenciesOfType<ScalarProperty>())
                {
                    // only want scalar props for a mapping fragment, not for an association set mapping
                    if (sp.Parent is MappingFragment)
                    {
                        if (sp.ColumnName.Target == null)
                        {
                            Debug.Fail("Null target for " + sp.ToPrettyString());
                        }
                        else
                        {
                            // in the situation where property is in multiple EntityTypeMappings (an inheritance 
                            // hierarchy) there can be multiple ScalarProperty anti-dependencies of property 
                            // (one for each EntityTypeMapping) all of which map to the same DatabaseColumn - 
                            // in this case only insert the first of these
                            var dbCol = DatabaseColumn.CreateFromProperty(sp.ColumnName.Target);
                            if (!columns.Contains(dbCol))
                            {
                                columns.Add(dbCol);
                            }
                        }
                    }
                }
            }
            return columns;
        }

        private AssociationPropertyIdentity()
        {
        }

        internal SortedListAllowDupes<DatabaseColumn> PrincipalColumns
        {
            get { return _principalColumns; }
            set { _principalColumns = value; }
        }

        internal SortedListAllowDupes<DatabaseColumn> DependentColumns
        {
            get { return _dependentColumns; }
            set { _dependentColumns = value; }
        }

        public override bool Equals(object obj)
        {
            var that = obj as AssociationPropertyIdentity;
            if (that == null)
            {
                return false;
            }

            if ((SortedListAllowDupes<DatabaseColumn>.CompareListContents(PrincipalColumns, that.PrincipalColumns) == 0)
                && (SortedListAllowDupes<DatabaseColumn>.CompareListContents(DependentColumns, that.DependentColumns) == 0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            foreach (var dc in _principalColumns)
            {
                hashCode ^= dc.GetHashCode();
            }
            foreach (var dc in _dependentColumns)
            {
                hashCode ^= dc.GetHashCode();
            }
            return hashCode;
        }

        /// <summary>
        ///     See whether the AssociationPropertyIdentity otherApi contains a "covering" for this
        ///     AssociationPropertyIdentity i.e. see whether otherApi contains at least the same
        ///     principal DatabaseColumns (but possibly more) _and_ at least the same dependent
        ///     DatabaseColumns (but possibly more).
        ///     This allows for treating these AssociationPropertyIdentity's as identical for the purposes
        ///     of Update Model even if a given C-side property has been mapped to more than 1 S-side property.
        /// </summary>
        internal bool IsCoveredBy(AssociationPropertyIdentity otherApi)
        {
            // check whether all this.PrincipalColumns are contained in otherApi.PrincipalColumns
            var thisPrincipalColumns = PrincipalColumns;
            var otherPrincipalColumns = otherApi.PrincipalColumns;
            if (false == otherPrincipalColumns.ContainsAll(thisPrincipalColumns))
            {
                // thisPrincipalColumns has at least one DatabaseColumn which is not
                // in otherPrincipalColumns, so otherApi does not cover thisApi
                return false;
            }

            // check whether all this.DependentColumns are contained in otherApi.DependentColumns
            var thisDependentColumns = DependentColumns;
            var otherDependentColumns = otherApi.DependentColumns;
            if (false == otherDependentColumns.ContainsAll(thisDependentColumns))
            {
                // thisDependentColumns has at least one DatabaseColumn which is not
                // in otherDependentColumns, so otherApi does not cover thisApi
                return false;
            }

            // the Principal and Dependent DatabaseColumns for this are covered by the Principal 
            // and Dependent DatabaseColumns in otherApi
            return true;
        }
    }

    internal class AssociationPropertyIdentityComparer : IComparer<AssociationPropertyIdentity>
    {
        private static readonly AssociationPropertyIdentityComparer _instance = new AssociationPropertyIdentityComparer();

        internal static AssociationPropertyIdentityComparer Instance
        {
            get { return _instance; }
        }

        private AssociationPropertyIdentityComparer()
        {
        }

        public int Compare(AssociationPropertyIdentity x, AssociationPropertyIdentity y)
        {
            var compVal = SortedListAllowDupes<DatabaseColumn>.CompareListContents(x.PrincipalColumns, y.PrincipalColumns);
            if (compVal == 0)
            {
                // left columns are equal, compare right columns
                compVal = SortedListAllowDupes<DatabaseColumn>.CompareListContents(x.DependentColumns, y.DependentColumns);
            }
            return compVal;
        }
    }
}
