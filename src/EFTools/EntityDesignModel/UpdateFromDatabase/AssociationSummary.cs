// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.UpdateFromDatabase
{
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Tools.XmlDesignerBase.Common.Diagnostics;

    internal class AssociationSummary
    {
        // private ctor.  Use CreateAssociationSummary(...)
        private AssociationSummary()
        {
        }

        // map from association to its AssociationIdentity
        private readonly Dictionary<Association, AssociationIdentity> _associationToAssociationIdentity =
            new Dictionary<Association, AssociationIdentity>();

        // map from a table in the database, to all association identities whose dependent end references a column in this table
        private readonly Dictionary<DatabaseObject, List<AssociationIdentity>> _dependentEndTablesToAssociationIdentities =
            new Dictionary<DatabaseObject, List<AssociationIdentity>>();

        internal string TraceString()
        {
            var sb = new StringBuilder("[AssociationSummary");
            sb.Append(
                " " +
                EFToolsTraceUtils.FormatNamedDictionary(
                    "associationToAssociationIdentity", _associationToAssociationIdentity,
                    delegate(Association assoc) { return assoc.NormalizedNameExternal; },
                    delegate(AssociationIdentity assocId) { return assocId.TraceString(); }));

            sb.Append(
                " " +
                EFToolsTraceUtils.FormatNamedDictionary(
                    "dependentEndTablesToAssociationIdentities", _dependentEndTablesToAssociationIdentities,
                    delegate(DatabaseObject dbObj) { return dbObj.ToString(); },
                    delegate(List<AssociationIdentity> assocIdList)
                        {
                            return EFToolsTraceUtils.FormatEnumerable(
                                assocIdList, delegate(AssociationIdentity assocId) { return "  " + assocId.TraceString(); });
                        },
                    true,
                    "  "
                    ));

            sb.Append("]");

            return sb.ToString();
        }

        internal AssociationIdentity GetAssociationIdentity(Association a)
        {
            AssociationIdentity id = null;
            _associationToAssociationIdentity.TryGetValue(a, out id);
            return id;
        }

        internal bool Contains(AssociationIdentity id)
        {
            // Since we can’t compare an association with an RC to an association with an ASM for equality, we can’t use a HashSet<AssociationIdentity>
            // to do the “contains” lookup.  So, we use the dependent end tables to narrow down the search, and then scan each 
            // candidate AssociationIdentity to see if it covers the passed in id.
            foreach (var table in id.AssociationTables)
            {
                List<AssociationIdentity> otherIDs = null;
                if (_dependentEndTablesToAssociationIdentities.TryGetValue(table, out otherIDs))
                {
                    foreach (var otherID in otherIDs)
                    {
                        if (id.IsCoveredBy(otherID))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal void Add(Association a, AssociationIdentity aID)
        {
            _associationToAssociationIdentity.Add(a, aID);
            foreach (var table in aID.AssociationTables)
            {
                List<AssociationIdentity> l = null;
                if (false == _dependentEndTablesToAssociationIdentities.TryGetValue(table, out l))
                {
                    l = new List<AssociationIdentity>();
                    _dependentEndTablesToAssociationIdentities.Add(table, l);
                }
                l.Add(aID);
            }
        }

        internal static AssociationSummary ConstructAssociationSummary(EFArtifact artifact)
        {
            var ecm = artifact.MappingModel().FirstEntityContainerMapping;

            var summary = new AssociationSummary();

            if (!EdmFeatureManager.GetForeignKeysInModelFeatureState(artifact.SchemaVersion).IsEnabled())
            {
                if (ecm != null)
                {
                    // Foreign keys in the model are not supported for this EDMX version.
                    foreach (var asm in ecm.AssociationSetMappings())
                    {
                        var cSideAssociation = asm.TypeName.Target;

                        if (null != cSideAssociation)
                        {
                            var assocId = AssociationIdentityForAssociationSetMapping.CreateAssociationIdentity(asm);
                            if (null != assocId)
                            {
                                summary.Add(cSideAssociation, assocId);
                            }
                        }
                    }
                }
            }
            else
            {
                // Foreign keys in the model are supported for this EDMX version.
                foreach (var a in artifact.ConceptualModel().Associations())
                {
                    AssociationIdentity assocId = null;
                    if (a.IsManyToMany == false
                        && a.ReferentialConstraint != null)
                    {
                        assocId = AssociationIdentityForReferentialConstraint.CreateAssociationIdentity(a.ReferentialConstraint);
                    }
                    else
                    {
                        var asm = ModelHelper.FindAssociationSetMappingForConceptualAssociation(a);
                        if (asm != null)
                        {
                            assocId = AssociationIdentityForAssociationSetMapping.CreateAssociationIdentity(asm);
                        }
                    }
                    if (null != assocId)
                    {
                        summary.Add(a, assocId);
                    }
                }
            }
            return summary;
        }
    }
}
