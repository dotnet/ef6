// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.UpdateFromDatabase
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Tools.XmlDesignerBase.Common.Diagnostics;

    internal class ExistingModelSummary
    {
        // the artifact from which this was created
        private readonly EFArtifact _artifact;

        // a map of the C-side EntityType NormalizedNames to their EntityTypeIdentity
        // (Note: cannot use actual EntityTypes because the ReplaceSsdlCommand invokes
        // EFArtifact.ReloadArtifact() which invalidates the EntityTypes - however the 
        // names remain the same and so can be used)
        private readonly Dictionary<Symbol, EntityTypeIdentity> _cEntityTypeNameToEntityTypeIdentity =
            new Dictionary<Symbol, EntityTypeIdentity>();

        // records all tables and views referred to by the S-side EntitySets in this model
        // mapping their identity to their local name
        private readonly Dictionary<DatabaseObject, string> _allTablesAndViews = new Dictionary<DatabaseObject, string>();

        // a map of the DatabaseObjects (from the S-side EntitySets) to their column names
        private readonly Dictionary<DatabaseObject, HashSet<string>> _databaseObjectColumns =
            new Dictionary<DatabaseObject, HashSet<string>>();

        // records all tables and views referred to by the S-side Functions in this model
        // mapping their identity to their local name
        private readonly Dictionary<DatabaseObject, string> _allFunctions = new Dictionary<DatabaseObject, string>();

        // records the AssociationIdentities for all the Associations in this model
        private readonly AssociationSummary _associationSummary;

        // for each C-side EntityType with a baseType, this maps every DatabaseObject 
        // in the EntityTypeIdentity for the child EntityType to a HashSet of DatabaseObjects
        // for the base EntityType
        private readonly Dictionary<DatabaseObject, HashSet<DatabaseObject>> _cAncestorTypeDatabaseObjectMap =
            new Dictionary<DatabaseObject, HashSet<DatabaseObject>>();

        // maps a DatabaseObject to the HashSet of NormalizedNames of the 
        // C-side EntityTypes whose EntityTypeIdentity includes that DatabaseObject
        // (Note: cannot use actual EntityTypes because the ReplaceSsdlCommand invokes
        // EFArtifact.ReloadArtifact() which invalidates the EntityTypes - however the 
        // names remain the same and so can be used)
        private readonly Dictionary<DatabaseObject, HashSet<Symbol>> _databaseObjectToCEntityTypeNamesMap =
            new Dictionary<DatabaseObject, HashSet<Symbol>>();

        // Dictionary used for lazy-loading of the EntityTypes represented by 
        // _databaseObjectToCEntityTypeNamesMap above
        private readonly Dictionary<DatabaseObject, HashSet<ConceptualEntityType>> _lazyLoadDatabaseObjectToCEntityTypesMap =
            new Dictionary<DatabaseObject, HashSet<ConceptualEntityType>>();

        internal ExistingModelSummary(EFArtifact artifact)
        {
            _artifact = artifact;
            if (null == artifact)
            {
                Debug.Fail("Null artifact");
            }
            else
            {
                if (null != artifact.MappingModel()
                    && null != artifact.MappingModel().FirstEntityContainerMapping)
                {
                    RecordEntityTypeIdentities(
                        artifact.MappingModel().FirstEntityContainerMapping);

                    // build the association summary.
                    _associationSummary = AssociationSummary.ConstructAssociationSummary(artifact);
                }

                if (null != artifact.ConceptualModel())
                {
                    RecordInheritanceAndEntityTypeMappings(artifact.ConceptualModel());
                }

                if (null != artifact.StorageModel())
                {
                    RecordFunctions(artifact.StorageModel());

                    if (null != artifact.StorageModel().FirstEntityContainer)
                    {
                        var sec = artifact.StorageModel().FirstEntityContainer as StorageEntityContainer;
                        if (sec != null)
                        {
                            RecordStorageEntitySetsAndProperties(sec);
                        }
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal string TraceString()
        {
            var sb = new StringBuilder("[ExistingModelSummary");
            sb.AppendLine(" artifactUri=" + (_artifact == null ? "null" : _artifact.Uri.ToString()));

            sb.Append(
                " " + EFToolsTraceUtils.FormatNamedDictionary(
                    "cEntityTypeNameToEntityTypeIdentity", _cEntityTypeNameToEntityTypeIdentity,
                    delegate(Symbol symbol) { return symbol.ToExternalString(); },
                    delegate(EntityTypeIdentity etId) { return etId.TraceString(); },
                    true,
                    " "
                          ));

            sb.Append(
                " " + EFToolsTraceUtils.FormatNamedDictionary(
                    "allTablesAndViews", _allTablesAndViews,
                    delegate(DatabaseObject dbObj) { return dbObj.ToString(); },
                    delegate(string s) { return s; },
                    true,
                    " "
                          ));

            sb.Append(
                " " + EFToolsTraceUtils.FormatNamedDictionary(
                    "databaseObjectColumns", _databaseObjectColumns,
                    delegate(DatabaseObject dbObj) { return dbObj.ToString(); },
                    delegate(HashSet<string> hashOfColumnNames)
                        { return EFToolsTraceUtils.FormatEnumerable(hashOfColumnNames, delegate(string s) { return s; }); },
                    true,
                    " "
                          ));

            sb.Append(
                " " + EFToolsTraceUtils.FormatNamedDictionary(
                    "allFunctions", _allFunctions,
                    delegate(DatabaseObject dbObj) { return dbObj.ToString(); },
                    delegate(string s) { return s; },
                    true,
                    " "
                          ));

            sb.AppendLine(" associationSummary=" + (_associationSummary == null ? "null" : _associationSummary.TraceString()));

            sb.Append(
                " " + EFToolsTraceUtils.FormatNamedDictionary(
                    "cAncestorTypeDatabaseObjectMap", _cAncestorTypeDatabaseObjectMap,
                    delegate(DatabaseObject dbObj) { return dbObj.ToString(); },
                    delegate(HashSet<DatabaseObject> hashOfBaseTypeDatabaseObjects)
                        {
                            return EFToolsTraceUtils.FormatEnumerable(
                                hashOfBaseTypeDatabaseObjects, delegate(DatabaseObject dbObj) { return dbObj.ToString(); });
                        },
                    true,
                    " "
                          ));

            sb.Append(
                " " + EFToolsTraceUtils.FormatNamedDictionary(
                    "databaseObjectToCEntityTypeNamesMap", _databaseObjectToCEntityTypeNamesMap,
                    delegate(DatabaseObject dbObj) { return dbObj.ToString(); },
                    delegate(HashSet<Symbol> hashOfNormalizedNames)
                        {
                            return EFToolsTraceUtils.FormatEnumerable(
                                hashOfNormalizedNames, delegate(Symbol symbol) { return symbol.ToExternalString(); });
                        },
                    true,
                    " "
                          ));

            sb.Append("]");

            return sb.ToString();
        }

        internal EFArtifact Artifact
        {
            get { return _artifact; }
        }

        internal EntityTypeIdentity GetEntityTypeIdentityForEntityType(EntityType et)
        {
            var normalizedName = et.NormalizedName;
            if (normalizedName == null)
            {
                Debug.Fail("null or empty Normalized Name for " + et.ToPrettyString());
                return null;
            }

            EntityTypeIdentity results;
            _cEntityTypeNameToEntityTypeIdentity.TryGetValue(normalizedName, out results);
            return results;
        }

        internal HashSet<DatabaseObject> AllTablesAndViews
        {
            get
            {
                // return just the keys
                return new HashSet<DatabaseObject>(_allTablesAndViews.Keys);
            }
        }

        internal Dictionary<DatabaseObject, string> AllTablesAndViewsDictionary
        {
            get { return _allTablesAndViews; }
        }

        internal Dictionary<DatabaseObject, string> AllFunctionsDictionary
        {
            get { return _allFunctions; }
        }

        internal HashSet<string> GetColumnsForDatabaseObject(DatabaseObject dbObj)
        {
            HashSet<string> results;
            _databaseObjectColumns.TryGetValue(dbObj, out results);
            return results;
        }

        internal AssociationSummary AssociationSummary
        {
            get { return _associationSummary; }
        }

        internal HashSet<DatabaseObject> GetAncestorTypeTablesAndViews(DatabaseObject tableOrView)
        {
            HashSet<DatabaseObject> results;
            _cAncestorTypeDatabaseObjectMap.TryGetValue(tableOrView, out results);
            return results;
        }

        // Returns a list of C-side EntityTypes from the current artifact which
        // match the list of EntityType NormalizedNames stored off against the
        // passed in argument tableOrView in this artifact at this 
        // ExistingModelSummary's creation-time
        // (Note: cannot use the actual EntityTypes from the creation-time because 
        // the ReplaceSsdlCommand invokes EFArtifact.ReloadArtifact() which 
        // invalidates them - however the names remain the same and so can be used)
        internal HashSet<ConceptualEntityType> GetConceptualEntityTypesForDatabaseObject(DatabaseObject tableOrView)
        {
            // First try the lazy load map to see if we've already calculated
            // the HashSet for this DatabaseObject
            HashSet<ConceptualEntityType> entityTypesLazyLoad;
            _lazyLoadDatabaseObjectToCEntityTypesMap.TryGetValue(tableOrView, out entityTypesLazyLoad);
            if (null != entityTypesLazyLoad)
            {
                return entityTypesLazyLoad;
            }

            // if not then consult the NormalizedNames map 
            HashSet<Symbol> entityTypeNormalizedNamesForDbObj;
            _databaseObjectToCEntityTypeNamesMap.TryGetValue(tableOrView, out entityTypeNormalizedNamesForDbObj);
            if (null != entityTypeNormalizedNamesForDbObj)
            {
                var artifactSet = Artifact.ArtifactSet;
                var entityTypes = new HashSet<ConceptualEntityType>();
                foreach (var entityTypeName in entityTypeNormalizedNamesForDbObj)
                {
                    var element = artifactSet.LookupSymbol(entityTypeName);
                    if (null == element)
                    {
                        Debug.Fail("no match for symbol " + entityTypeName);
                    }
                    else
                    {
                        var et = element as EntityType;
                        var cet = element as ConceptualEntityType;

                        if (null == cet)
                        {
                            Debug.Assert(
                                et != null, "symbol " + entityTypeName +
                                            " matches non-EntityTypeelement " + element.ToPrettyString());
                            Debug.Assert(et != null ? cet != null : true, "EntityType is not a ConceptualEntityType");
                        }
                        else
                        {
                            entityTypes.Add(cet);
                        }
                    }
                }

                // construct the lazy load map in case we're called again 
                // for the same DatabaseObject
                _lazyLoadDatabaseObjectToCEntityTypesMap.Add(tableOrView, entityTypes);
                return entityTypes;
            }
            else
            {
                return null;
            }
        }

        private void RecordEntityTypeIdentities(
            EntityContainerMapping entityContainerMapping)
        {
            // construct mapping from EntityType to EntityTypeIdentity which
            // is its identity
            UpdateModelFromDatabaseUtils.ConstructEntityMappings(
                entityContainerMapping, AddCEntityTypeNameToEntityTypeIdentityMapping);
        }

        private void RecordStorageEntitySetsAndProperties(StorageEntityContainer sec)
        {
            foreach (var es in sec.EntitySets())
            {
                var ses = es as StorageEntitySet;
                if (null != ses)
                {
                    var dbObj = DatabaseObject.CreateFromEntitySet(ses);
                    _allTablesAndViews.Add(dbObj, ses.LocalName.Value);
                    var et = ses.EntityType.Target;
                    if (null == et)
                    {
                        Debug.Fail("Null EntityType");
                    }
                    else
                    {
                        foreach (var prop in et.Properties())
                        {
                            AddDbObjToColumnMapping(dbObj, prop);
                        }
                    }
                }
            }
        }

        private void RecordFunctions(StorageEntityModel sem)
        {
            foreach (var f in sem.Functions())
            {
                var dbObj = DatabaseObject.CreateFromFunction(f);
                _allFunctions.Add(dbObj, f.LocalName.Value);
            }
        }

        private void RecordInheritanceAndEntityTypeMappings(ConceptualEntityModel cem)
        {
            // ensure _cEntityTypeToEntityTypeIdentity has been constructed
            Debug.Assert(null != _cEntityTypeNameToEntityTypeIdentity, "Null _cEntityTypeToEntityTypeIdentity");

            // now loop over EntityTypes. For each EntityType that has a base
            // type, map each DatabaseObject in the EntityTypeIdentity which is the
            // child type's identity to all TablesAndViews in the base type (and to 
            // all TablesAndViews in its base-type and so on up the chain).
            //
            // Note: although the normal case is a mapping 1 DatabaseObject -> 1 DatabaseObject
            // it is possible for mappings to overlap e.g. consider C-Side type C1 
            // which maps to EntityTypeIdentity {T1, T2} with a base type C2 which maps to
            // EntityTypeIdentity {T3, T4) and C-side type C3 which maps to
            // EntityTypeIdentity {T2, T5} with base type C4 which maps to {T4, T6}
            // Then this map will contain mappings:
            //   T1 -> {T3, T4}
            //   T2 -> {T3, T4, T6}
            //   T5 -> {T4, T6}
            // In the above {T3, T4, T6} is not the identity of any given C-side
            // entity type. But it allows us to know that something which maps to 
            // T2 (amongst others), has a base type which maps to T6 (amongst others).
            //
            // Also record the DatabaseObject->EntityType mappings.
            foreach (var et in cem.EntityTypes())
            {
                // Note: etEntityTypeId below will be null for unmapped EntityTypes
                var etEntityTypeId = GetEntityTypeIdentityForEntityType(et);
                if (null != etEntityTypeId)
                {
                    // record DatabaseObject->EntityType mappings
                    foreach (var etTableOrView in etEntityTypeId.TablesAndViews)
                    {
                        AddDbObjToEntityTypeNameMap(etTableOrView, et);
                    }

                    // record inheritance mappings
                    foreach (var etTableOrView in etEntityTypeId.TablesAndViews)
                    {
                        AddDbObjToEntityTypeNameMap(etTableOrView, et);

                        // loop over all ancestor types (i.e. this EntityType's base-type
                        // and its base-type etc.)
                        var baseType = et as ConceptualEntityType;
                        Debug.Assert(baseType != null, "EntityType is not a ConceptualEntityType");

                        while ((baseType = baseType.BaseType.Target) != null)
                        {
                            var baseTypeEntityTypeId = GetEntityTypeIdentityForEntityType(baseType);
                            if (null != etEntityTypeId
                                && null != baseTypeEntityTypeId)
                            {
                                foreach (var baseTypeTableOrView in baseTypeEntityTypeId.TablesAndViews)
                                {
                                    AddTableOrViewToBaseTypeMappings(etTableOrView, baseTypeTableOrView);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddTableOrViewToBaseTypeMappings(
            DatabaseObject key, DatabaseObject databaseObjectInAncestor)
        {
            HashSet<DatabaseObject> tablesAndViewsHashSet = null;
            _cAncestorTypeDatabaseObjectMap.TryGetValue(key, out tablesAndViewsHashSet);
            if (null == tablesAndViewsHashSet)
            {
                tablesAndViewsHashSet = _cAncestorTypeDatabaseObjectMap[key] = new HashSet<DatabaseObject>();
            }

            tablesAndViewsHashSet.Add(databaseObjectInAncestor);
        }

        private void AddCEntityTypeNameToEntityTypeIdentityMapping(
            EntityType key, DatabaseObject dbObj)
        {
            var normalizedName = key.NormalizedName;
            if (normalizedName == null)
            {
                Debug.Fail("null or empty Normalized Name for " + key.ToPrettyString());
                return;
            }

            EntityTypeIdentity etId = null;
            _cEntityTypeNameToEntityTypeIdentity.TryGetValue(normalizedName, out etId);
            if (null == etId)
            {
                etId = _cEntityTypeNameToEntityTypeIdentity[normalizedName] = new EntityTypeIdentity();
            }

            etId.AddTableOrView(dbObj);
        }

        private void AddDbObjToEntityTypeNameMap(DatabaseObject key, EntityType et)
        {
            var normalizedName = et.NormalizedName;
            if (normalizedName == null)
            {
                Debug.Fail("null or empty Normalized Name for " + et.ToPrettyString());
                return;
            }

            HashSet<Symbol> entityTypeNamesHashSet = null;
            _databaseObjectToCEntityTypeNamesMap.TryGetValue(key, out entityTypeNamesHashSet);
            if (null == entityTypeNamesHashSet)
            {
                entityTypeNamesHashSet = _databaseObjectToCEntityTypeNamesMap[key] = new HashSet<Symbol>();
            }

            entityTypeNamesHashSet.Add(normalizedName);
        }

        private void AddDbObjToColumnMapping(DatabaseObject key, Property prop)
        {
            var propName = prop.LocalName.Value;
            if (null == propName)
            {
                Debug.Fail("Null property name in AddDbObjToColumnMapping");
                return;
            }

            HashSet<string> columnsHashSet = null;
            _databaseObjectColumns.TryGetValue(key, out columnsHashSet);
            if (null == columnsHashSet)
            {
                columnsHashSet = _databaseObjectColumns[key] = new HashSet<string>();
            }

            columnsHashSet.Add(propName);
        }

        internal bool HasAncestorTypeThatMapsToDbObject(ConceptualEntityType et, DatabaseObject dbObj)
        {
            return (FindClosestAncestorTypeThatMapsToDbObject(et, dbObj) == null ? false : true);
        }

        internal ConceptualEntityType FindClosestAncestorTypeThatMapsToDbObject(ConceptualEntityType et, DatabaseObject dbObj)
        {
            Debug.Assert(
                null != _cEntityTypeNameToEntityTypeIdentity, "requires that _cEntityTypeToEntityTypeIdentity " +
                                                              "is initialized for GetEntityTypeIdentityForEntityType call");

            if (null == et)
            {
                Debug.Fail("Null EntityType");
                return null;
            }

            var baseType = et;
            while ((baseType = baseType.BaseType.Target) != null)
            {
                var baseTypeId =
                    GetEntityTypeIdentityForEntityType(baseType);
                if (null != baseTypeId)
                {
                    foreach (var baseTypeTableOrView in baseTypeId.TablesAndViews)
                    {
                        if (dbObj.Equals(baseTypeTableOrView))
                        {
                            return baseType;
                        }
                    }
                }
            }

            return null;
        }

        internal EntityType FindRootAncestorTypeThatMapsToDbObject(ConceptualEntityType et, DatabaseObject dbObj)
        {
            ConceptualEntityType rootAncestor = null;
            var nextAncestor = et;
            while ((nextAncestor = FindClosestAncestorTypeThatMapsToDbObject(nextAncestor, dbObj)) != null)
            {
                rootAncestor = nextAncestor;
            }

            return rootAncestor;
        }
    }
}
