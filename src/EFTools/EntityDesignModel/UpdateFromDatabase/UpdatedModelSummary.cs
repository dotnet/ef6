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

    internal class UpdatedModelSummary
    {
        // the artifact from which this was created
        private readonly EFArtifact _artifact;

        // a map of the C-side EntityTypes to their EntityTypeIdentity
        private readonly Dictionary<EntityType, EntityTypeIdentity> _cEntityTypeToEntityTypeIdentity =
            new Dictionary<EntityType, EntityTypeIdentity>();

        // a map with key the DatabaseObject (from the S-side EntitySet) to 
        // a value which is a HashSet of the underlying S-side Property objects
        private readonly Dictionary<DatabaseObject, HashSet<Property>> _databaseObjectColumns =
            new Dictionary<DatabaseObject, HashSet<Property>>();

        // Summary info about associations in the model.
        private readonly AssociationSummary _associationSummary;

        internal UpdatedModelSummary(EFArtifact artifact)
        {
            _artifact = artifact;

            Debug.Assert(artifact != null, "Null artifact");

            if (artifact != null)
            {
                if (null != artifact.MappingModel()
                    && null != artifact.MappingModel().FirstEntityContainerMapping)
                {
                    RecordEntityTypeIdentities(
                        artifact.MappingModel().FirstEntityContainerMapping);

                    // build the association summary
                    _associationSummary = AssociationSummary.ConstructAssociationSummary(artifact);
                }

                if (null != artifact.StorageModel()
                    && null != artifact.StorageModel().FirstEntityContainer)
                {
                    var sec = artifact.StorageModel().FirstEntityContainer as StorageEntityContainer;
                    if (sec != null)
                    {
                        RecordStorageProperties(sec);
                    }
                }
            }
        }

        internal string TraceString()
        {
            var sb = new StringBuilder("[" + typeof(UpdatedModelSummary).Name);
            sb.AppendLine(" artifactUri=" + (_artifact == null ? "null" : _artifact.Uri.ToString()));

            sb.Append(
                " " + EFToolsTraceUtils.FormatNamedDictionary(
                    "cEntityTypeToEntityTypeIdentity", _cEntityTypeToEntityTypeIdentity,
                    delegate(EntityType et) { return et.NormalizedNameExternal; },
                    delegate(EntityTypeIdentity etId) { return etId.TraceString(); },
                    true,
                    " "
                          ));

            sb.Append(
                " " + EFToolsTraceUtils.FormatNamedDictionary(
                    "databaseObjectColumns", _databaseObjectColumns,
                    delegate(DatabaseObject dbObj) { return dbObj.ToString(); },
                    delegate(HashSet<Property> hashOfProperties)
                        {
                            return EFToolsTraceUtils.FormatEnumerable(
                                hashOfProperties, delegate(Property prop) { return prop.NormalizedNameExternal; });
                        },
                    true,
                    " "
                          ));

            sb.AppendLine(" associationSummary=" + (_associationSummary == null ? "null" : _associationSummary.TraceString()));

            sb.Append("]");

            return sb.ToString();
        }

        internal EFArtifact Artifact
        {
            get { return _artifact; }
        }

        internal EntityTypeIdentity GetEntityTypeIdentityForEntityType(EntityType et)
        {
            EntityTypeIdentity results;
            _cEntityTypeToEntityTypeIdentity.TryGetValue(et, out results);
            return results;
        }

        internal HashSet<Property> GetPropertiesForDatabaseObject(DatabaseObject dbObj)
        {
            HashSet<Property> results;
            _databaseObjectColumns.TryGetValue(dbObj, out results);
            return results;
        }

        internal AssociationIdentity GetAssociationIdentityForAssociation(Association assoc)
        {
            var results = _associationSummary.GetAssociationIdentity(assoc);
            return results;
        }

        private void RecordEntityTypeIdentities(
            EntityContainerMapping entityContainerMapping)
        {
            // construct mapping from EntityType to EntityTypeIdentity which
            // is its identity
            UpdateModelFromDatabaseUtils.ConstructEntityMappings(
                entityContainerMapping, AddCEntityTypeToEntityTypeIdentityMapping);
        }

        private void RecordStorageProperties(StorageEntityContainer sec)
        {
            foreach (var es in sec.EntitySets())
            {
                var ses = es as StorageEntitySet;
                if (null != ses)
                {
                    var dbObj = DatabaseObject.CreateFromEntitySet(ses);
                    var et = ses.EntityType.Target;
                    if (null == et)
                    {
                        Debug.Fail("Null EntityType");
                    }
                    else
                    {
                        foreach (var prop in et.Properties())
                        {
                            AddDbObjToPropertiesMapping(dbObj, prop);
                        }
                    }
                }
            }
        }

        private void AddCEntityTypeToEntityTypeIdentityMapping(
            EntityType key, DatabaseObject dbObj)
        {
            EntityTypeIdentity etId = null;
            _cEntityTypeToEntityTypeIdentity.TryGetValue(key, out etId);
            if (null == etId)
            {
                etId = _cEntityTypeToEntityTypeIdentity[key] = new EntityTypeIdentity();
            }

            etId.AddTableOrView(dbObj);
        }

        private void AddDbObjToPropertiesMapping(DatabaseObject key, Property prop)
        {
            HashSet<Property> propsSet = null;
            _databaseObjectColumns.TryGetValue(key, out propsSet);
            if (null == propsSet)
            {
                propsSet = _databaseObjectColumns[key] = new HashSet<Property>();
            }

            propsSet.Add(prop);
        }
    }
}
