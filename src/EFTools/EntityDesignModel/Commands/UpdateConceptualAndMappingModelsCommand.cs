// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.Model.UpdateFromDatabase;
    using Microsoft.Data.Entity.Design.Model.Validation;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal class UpdateConceptualAndMappingModelsCommand : Command
    {
        private readonly ExistingModelSummary _preExistingModel;
        private readonly UpdatedModelSummary _modelRepresentingDatabase;

        internal UpdateConceptualAndMappingModelsCommand(
            ExistingModelSummary preExistingModel, UpdatedModelSummary modelRepresentingDatabase)
        {
            Debug.Assert(null != preExistingModel, "null preExistingModel");
            Debug.Assert(null != modelRepresentingDatabase, "null modelRepresentingDatabase");
            _preExistingModel = preExistingModel;
            _modelRepresentingDatabase = modelRepresentingDatabase;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var service = cpc.EditingContext.GetEFArtifactService();
            var existingArtifact = service.Artifact;
            Debug.Assert(existingArtifact != null, "Null Artifact");
            if (null == existingArtifact)
            {
                return;
            }

            // from the model for (potentially) updated database determine which 
            // C-side objects need to be added/updated and then update the
            // C- and M- side models appropriately

            // Find the list of C-side EntityTypes (in the temp model) which 
            // are new compared to those in the existing model
            HashSet<EntityType> entityTypesToBeAdded;
            HashSet<EntityType> entityTypesToBeUpdated;
            FindNewAndExistingEntityTypes(
                out entityTypesToBeAdded, out entityTypesToBeUpdated);

            // For each C-side EntityType in the temp model determine 
            // if it maps to a table/view which existed before the
            // update and only add new ones (with their EntitySets and mappings)
            Dictionary<EntityType, EntityType> tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact;
            AddNewConceptualEntityTypesFromArtifact(
                cpc, entityTypesToBeAdded,
                out tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact);

            // Next update the properties on the entityTypesToBeUpdated
            UpdateConceptualPropertiesFromArtifact(cpc, entityTypesToBeUpdated);

            // find a list of all new Associations (compared to the existing model)
            HashSet<Association> associationsToBeAdded;
            FindNewAssociations(
                tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact,
                out associationsToBeAdded);

            // Next add appropriate C-side Associations from the temp model
            // to the existing model (with their AssociationSets and mappings)
            AddNewConceptualAssociationsFromArtifact(
                cpc, associationsToBeAdded,
                tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact);

            // push any changes that the user has made to the corresponding view definition in the SSDL
            PropagateViewKeysToStorageModel.AddRule(
                cpc, _preExistingModel.Artifact.ConceptualModel().EntityTypes().OfType<ConceptualEntityType>());
        }

        private void AddNewConceptualEntityTypesFromArtifact(
            CommandProcessorContext cpc, HashSet<EntityType> entityTypesFromTempArtifactToBeAdded,
            out Dictionary<EntityType, EntityType> tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact)
        {
            tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact = new Dictionary<EntityType, EntityType>();

            // add in all new C-side Entity Types (creating clones because the
            // EntityType objects in entityTypesToBeAdded are from the temporary DB artifact)
            foreach (var etFromTempArtifact in entityTypesFromTempArtifactToBeAdded)
            {
                var etcf = new EntityTypeClipboardFormat(etFromTempArtifact);
                var cmd = new CopyEntityCommand(etcf, ModelSpace.Conceptual);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
                var newEntityTypeInExistingArtifact = cmd.EntityType;

                // add an EntitySetMapping for the new EntityType
                if (null != newEntityTypeInExistingArtifact)
                {
                    // add mapping from EntityType in the temp artifact to the newly-created
                    // EntityType in the existing artifact
                    tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact.Add(
                        etFromTempArtifact, newEntityTypeInExistingArtifact);

                    var esmInTempArtifact = ModelHelper.FindEntitySetMappingForEntityType(etFromTempArtifact);
                    var newEntitySet = newEntityTypeInExistingArtifact.EntitySet as ConceptualEntitySet;
                    if (null == esmInTempArtifact)
                    {
                        Debug.Fail("null esmInTempArtifact");
                    }
                    else if (null == newEntitySet)
                    {
                        Debug.Fail("null newEntitySet");
                    }
                    else
                    {
                        CloneEntitySetMapping(
                            cpc, esmInTempArtifact,
                            _preExistingModel.Artifact.MappingModel().FirstEntityContainerMapping, newEntitySet,
                            tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact);
                    }
                }
                else
                {
                    throw new UpdateModelFromDatabaseException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.UpdateFromDatabaseCannotCreateEntityType,
                            etFromTempArtifact.ToPrettyString()));
                }
            }
        }

        /// <summary>
        ///     This method loops over the C-side EntityTypes (from the temp
        ///     artifact) which are to be updated (as passed in in the argument),
        ///     finds the underlying DatabaseObject identities, finds the root
        ///     C-side EntityTypes in the existing artifact which match those
        ///     identities, finds the new properties (if any) in the DatabaseObject
        ///     and adds new C-side properties to those root EntityTypes plus
        ///     mappings between those newly-added C-side properties and the S-side
        ///     properties which underlie them
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="cSideEntityTypesToBeUpdatedFromTempArtifact"></param>
        private void UpdateConceptualPropertiesFromArtifact(
            CommandProcessorContext cpc,
            HashSet<EntityType> cSideEntityTypesToBeUpdatedFromTempArtifact)
        {
            // construct list of all tables/views underlying the entityTypesToBeUpdated
            var entityTypesToBeUpdatedDatabaseObjects =
                new HashSet<DatabaseObject>();
            foreach (var tempArtifactEntityType in cSideEntityTypesToBeUpdatedFromTempArtifact)
            {
                var tempArtifactEntityTypeId = _modelRepresentingDatabase.
                    GetEntityTypeIdentityForEntityType(tempArtifactEntityType);
                entityTypesToBeUpdatedDatabaseObjects.UnionWith(tempArtifactEntityTypeId.TablesAndViews);
            }

            // foreach table/view in entityTypesToBeUpdatedDatabaseObjects find
            // the EntityTypes in the ExistingModel which need to be updated
            foreach (var dbObj in entityTypesToBeUpdatedDatabaseObjects)
            {
                // find list of new S-side properties for this DatabaseObject
                HashSet<Property> newStoragePropertiesForDbObj;
                FindNewProperties(dbObj, out newStoragePropertiesForDbObj);

                // if no new properties then just move on to next DatabaseObject
                if (newStoragePropertiesForDbObj.Count > 0)
                {
                    // find existing root EntityTypes that map to this DatabaseObject
                    var existingEntityTypesForDbObj =
                        _preExistingModel.GetConceptualEntityTypesForDatabaseObject(dbObj);
                    if (null == existingEntityTypesForDbObj
                        || 0 == existingEntityTypesForDbObj.Count)
                    {
                        // a column has been added to a table on the database
                        // but the EntityType that was mapped to that table has
                        // been deleted by the user. So ignore this DatabaseObject 
                        // and move on to the next.
                        continue;
                    }

                    var rootExistingEntityTypesForDbObj = new HashSet<ConceptualEntityType>();
                    foreach (var et in existingEntityTypesForDbObj)
                    {
                        if (!_preExistingModel.HasAncestorTypeThatMapsToDbObject(et, dbObj))
                        {
                            rootExistingEntityTypesForDbObj.Add(et);
                        }
                    }
                    if (0 == rootExistingEntityTypesForDbObj.Count)
                    {
                        throw new UpdateModelFromDatabaseException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.UpdateFromDatabaseCannotFindRootEntityTypeForProperty,
                                newStoragePropertiesForDbObj.Count,
                                dbObj.ToString()));
                    }

                    // for each new S-side Property find the mapped C-side 
                    // Property in the tempArtifact and copy that C-side Property 
                    // into each root EntityType in the existing artifact
                    foreach (var sSidePropInTempArtifact in newStoragePropertiesForDbObj)
                    {
                        var cSidePropInTempArtifact = FindMappedCSidePropertyForSSideProperty(sSidePropInTempArtifact);

                        // if the S-side property is unmapped even in the _temp_ artifact 
                        // then it's a foreign key column - so we do not need a matching 
                        // property on the C-side
                        if (null != cSidePropInTempArtifact)
                        {
                            // loop over all the root existing EntityTypes adding this new property
                            foreach (EntityType existingEntityType in rootExistingEntityTypesForDbObj)
                            {
                                CreateNewConceptualPropertyAndMapping(
                                    cpc, cSidePropInTempArtifact, sSidePropInTempArtifact,
                                    existingEntityType);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     In the existing artifact create a new C-side Property
        ///     in the existing EntityType  plus a mapping based on the
        ///     C-side and S-side Properties in the temp artifact
        /// </summary>
        private Property CreateNewConceptualPropertyAndMapping(
            CommandProcessorContext cpc,
            Property cSidePropInTempArtifact, Property sSidePropInTempArtifact,
            EntityType existingEntityType)
        {
            Debug.Assert(cSidePropInTempArtifact != null, "Null C-side Property");
            Debug.Assert(cSidePropInTempArtifact.EntityType.EntityModel.IsCSDL, "cSidePropInTempArtifact must be C-side");
            Debug.Assert(sSidePropInTempArtifact != null, "Null S-side Property");
            Debug.Assert(!sSidePropInTempArtifact.EntityType.EntityModel.IsCSDL, "sSidePropInTempArtifact must be S-side");
            Debug.Assert(existingEntityType != null, "Null existing EntityType");
            Debug.Assert(existingEntityType.EntityModel.IsCSDL, "Existing EntityType must be C-side");

            var pcf = new PropertyClipboardFormat(cSidePropInTempArtifact);
            Debug.Assert(
                pcf != null, "Could not construct PropertyClipboardFormat for C-side Property " + cSidePropInTempArtifact.ToPrettyString());

            if (null == pcf)
            {
                return null;
            }
            else
            {
                // store off matching S-side property in the existing 
                // artifact for mapping below
                var sSidePropInExistingArtifact =
                    FindSSidePropInExistingArtifact(sSidePropInTempArtifact);
                Debug.Assert(
                    null != sSidePropInExistingArtifact,
                    "Cannot find S-side Property matching the one in the temp artifact " + sSidePropInTempArtifact.ToPrettyString());

                // create the C-side Property in the existing artifact
                var cmd = new CopyPropertyCommand(pcf, existingEntityType);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
                var cSidePropInExistingArtifact = cmd.Property;

                // now create the mapping for the C-side Property just created
                if (null != cSidePropInExistingArtifact
                    && null != sSidePropInExistingArtifact)
                {
                    var cmd2 =
                        new CreateFragmentScalarPropertyCommand(
                            existingEntityType,
                            cSidePropInExistingArtifact, sSidePropInExistingArtifact);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd2);
                }

                return cSidePropInExistingArtifact;
            }
        }

        private void AddNewConceptualAssociationsFromArtifact(
            CommandProcessorContext cpc, HashSet<Association> associationsToBeAdded,
            Dictionary<EntityType, EntityType> tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact)
        {
            var existingArtifact = _preExistingModel.Artifact;

            // add in all new C-side Associations (creating clones because the
            // Association objects in associationsToBeAdded are from the temporary DB artifact)
            foreach (var assocInTempArtifact in associationsToBeAdded)
            {
                if (assocInTempArtifact.AssociationEnds().Count != 2)
                {
                    throw new UpdateModelFromDatabaseException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.UpdateFromDatabaseAssociationWrongNumberEnds,
                            assocInTempArtifact.ToPrettyString(),
                            assocInTempArtifact.AssociationEnds().Count));
                }

                // find the EntityType targets of the AssociationEnds in the temp artifact
                var end1InTempArtifact = assocInTempArtifact.AssociationEnds()[0];
                var end1EntityTypeInTempArtifact = end1InTempArtifact.Type.Target as ConceptualEntityType;
                if (null == end1EntityTypeInTempArtifact)
                {
                    throw new UpdateModelFromDatabaseException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.UpdateFromDatabaseAssociationEndNoTarget,
                            end1InTempArtifact.ToPrettyString()));
                }

                var end2InTempArtifact = assocInTempArtifact.AssociationEnds()[1];
                var end2EntityTypeInTempArtifact = end2InTempArtifact.Type.Target as ConceptualEntityType;
                if (null == end2EntityTypeInTempArtifact)
                {
                    throw new UpdateModelFromDatabaseException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.UpdateFromDatabaseAssociationEndNoTarget,
                            end2InTempArtifact.ToPrettyString()));
                }

                // locate the NavigationProperties for each AssociationEnd in the temp artifact
                var navProp1InTempArtifact =
                    ModelHelper.FindNavigationPropertyForAssociationEnd(end1EntityTypeInTempArtifact, end1InTempArtifact);
                if (null == navProp1InTempArtifact)
                {
                    throw new UpdateModelFromDatabaseException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.UpdateFromDatabaseAssociationNoMatchingNavProp,
                            end1EntityTypeInTempArtifact.ToPrettyString(),
                            end1InTempArtifact.ToPrettyString()));
                }

                var navProp2InTempArtifact =
                    ModelHelper.FindNavigationPropertyForAssociationEnd(end2EntityTypeInTempArtifact, end2InTempArtifact);
                if (null == navProp2InTempArtifact)
                {
                    throw new UpdateModelFromDatabaseException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.UpdateFromDatabaseAssociationNoMatchingNavProp,
                            end2EntityTypeInTempArtifact.ToPrettyString(),
                            end2InTempArtifact.ToPrettyString()));
                }

                // find the EntityTypes in the existing model which match end1EntityType & end2EntityType
                var end1EntityTypeInExistingArtifact =
                    FindMatchingConceptualEntityTypeInExistingArtifact(
                        end1EntityTypeInTempArtifact, tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact);
                var end2EntityTypeInExistingArtifact =
                    FindMatchingConceptualEntityTypeInExistingArtifact(
                        end2EntityTypeInTempArtifact, tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact);
                if (null == end1EntityTypeInExistingArtifact
                    || null == end2EntityTypeInExistingArtifact)
                {
                    // if there is no matching EntityType in the existing model
                    // this means the user has deleted that EntityType in the
                    // existing artifact in which case skip the attempt to 
                    // create the C-Side Association and its mapping
                    continue;
                }

                // attempt to clone the Association, ReferentialConstraint (if available), 
                // AssociationSet and AssociationSetMapping from the temp artifact
                CloneAssociation(
                    cpc, existingArtifact, assocInTempArtifact,
                    end1InTempArtifact, end2InTempArtifact, navProp1InTempArtifact, navProp2InTempArtifact,
                    end1EntityTypeInExistingArtifact, end2EntityTypeInExistingArtifact,
                    tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact);
            } // end of loop over associationsToBeAdded
        }

        /// <summary>
        ///     Construct the list of those EntityTypes in tempArtifactModel
        ///     which are "new" compared to the existing model where "new"
        ///     is defined by mapping to a set of DatabaseObjects (representing
        ///     tables/views on the database) which did not exist in the
        ///     existing model
        /// </summary>
        private void FindNewAndExistingEntityTypes(
            out HashSet<EntityType> newEntityTypes, out HashSet<EntityType> existingEntityTypes)
        {
            newEntityTypes = new HashSet<EntityType>();
            existingEntityTypes = new HashSet<EntityType>();

            if (null == _modelRepresentingDatabase
                || null == _modelRepresentingDatabase.Artifact
                || null == _modelRepresentingDatabase.Artifact.ConceptualModel())
            {
                return;
            }

            // find all tables/views in the pre-existing model
            var allPreExistingTablesAndViews =
                _preExistingModel.AllTablesAndViews;

            // loop over each EntityType in the temp model from DB, lookup
            // its mapped S-side EntitySet (if any) and compare to pre-existing
            // EntitySets from existing model. Assign the EntityType to either
            // newEntityTypes or existingEntityTypes based on this
            foreach (var et in _modelRepresentingDatabase.Artifact.ConceptualModel().EntityTypes())
            {
                var entityTypeIdentity =
                    _modelRepresentingDatabase.GetEntityTypeIdentityForEntityType(et);
                if (null == entityTypeIdentity
                    || entityTypeIdentity.Count == 0)
                {
                    newEntityTypes.Add(et);
                }
                else
                {
                    var matchesPreExistingTableOrView = false;
                    foreach (var tableOrView in entityTypeIdentity.TablesAndViews)
                    {
                        if (allPreExistingTablesAndViews.Contains(tableOrView))
                        {
                            matchesPreExistingTableOrView = true;
                            break;
                        }
                    }

                    if (matchesPreExistingTableOrView)
                    {
                        existingEntityTypes.Add(et);
                    }
                    else
                    {
                        newEntityTypes.Add(et);
                    }
                }
            }
        }

        /// <summary>
        ///     For the passed in DatabaseObject, construct the set of
        ///     columns which are new in the temp artifact compared to
        ///     the existing model and the S-side properties (in the
        ///     temp artifact) to which these map.
        /// </summary>
        private void FindNewProperties(DatabaseObject dbObj, out HashSet<Property> newProperties)
        {
            newProperties = new HashSet<Property>();

            var existingColumnNamesForDbObj =
                _preExistingModel.GetColumnsForDatabaseObject(dbObj);

            var tempArtifactPropertiesForDbObj =
                _modelRepresentingDatabase.GetPropertiesForDatabaseObject(dbObj);

            foreach (var tempArtifactColumn in tempArtifactPropertiesForDbObj)
            {
                var tempArtifactColumnName = tempArtifactColumn.LocalName.Value;
                Debug.Assert(null != tempArtifactColumnName, "Property " + tempArtifactColumn.ToPrettyString() + "has null LocalName");
                if (null != tempArtifactColumnName
                    && !existingColumnNamesForDbObj.Contains(tempArtifactColumnName))
                {
                    newProperties.Add(tempArtifactColumn);
                }
            }
        }

        /// <summary>
        ///     Construct the list of those C-side Associations which are "new"
        ///     compared to the existing model where "new" is defined by
        ///     mapping to an S-side EntitySet which did not exist under the
        ///     same name in the existing model
        ///     Note: it is assumed that no C-side Associations for the temporary
        ///     DB-based artifact are unmapped (which is true in the CSDL generated
        ///     by the runtime). If any are unmapped they will appear as "existing".
        /// </summary>
        private void FindNewAssociations(
            Dictionary<EntityType, EntityType> tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact,
            out HashSet<Association> associationsToBeAdded)
        {
            associationsToBeAdded = new HashSet<Association>();

            if (null == _modelRepresentingDatabase
                || null == _modelRepresentingDatabase.Artifact
                || null == _modelRepresentingDatabase.Artifact.ConceptualModel())
            {
                return;
            }

            // loop over each Association in the temp model from DB, lookup
            // its AssociationIdentity (if any) and compare to pre-existing
            // AssociationIdentities from existing model. Assign the Association to
            // associationsToBeAdded if pre-existing model contained no matching
            // identity and if the Association has not been replaced in the existing
            // model by an inheritance relationship (see below for details)
            foreach (var assoc in _modelRepresentingDatabase.Artifact.ConceptualModel().Associations())
            {
                var assocIdentity =
                    _modelRepresentingDatabase.GetAssociationIdentityForAssociation(assoc);
                Debug.Assert(null != assocIdentity, "Null AssociationIdentity for Association " + assoc.ToPrettyString());
                if (null != assocIdentity)
                {
                    // if identical association already exists then skip
                    if (_preExistingModel.AssociationSummary.Contains(assocIdentity))
                    {
                        continue;
                    }

                    // if association has been replaced by an inheritance
                    // relationship then skip, otherwise include 
                    if (
                        !HasBeenReplacedByInheritanceOrSplitEntity(
                            assoc, assocIdentity, tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact))
                    {
                        associationsToBeAdded.Add(assoc);
                    }
                }
            }
        }

        /// <summary>
        ///     Checks whether this association has been replaced by an inheritance
        ///     relationship between the 2 ends. Requires:
        ///     1) a 1:1 or 1:0..1 association
        ///     2) it is not a self-association
        ///     3) there exists an inheritance relation between the two different C-side EntityTypes
        ///     Also checks whether this association has been replaced by a single split-entity
        ///     which represents both ends of the association. Requires:
        ///     1) a 1:1 or 1:0..1 association
        ///     2) it is not a self-association
        ///     3) there is a single EntityType which is mapped to both tables
        ///     4) the referential constraint’s principal and dependent elements reference the full primary keys of both tables
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private bool HasBeenReplacedByInheritanceOrSplitEntity(
            Association assoc, AssociationIdentity assocId,
            Dictionary<EntityType, EntityType> tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact)
        {
            if (2 != assoc.AssociationEnds().Count)
            {
                Debug.Fail(
                    "Received incorrect number of AssociationEnds (" + assoc.AssociationEnds().Count + ") for Association "
                    + assoc.ToPrettyString() + " should be 2.");
                return false;
            }

            // check 1:1 or 1:0..1
            var assocEnds = assoc.AssociationEnds();
            var assocEnd1 = assocEnds[0];
            var assocEnd2 = assocEnds[1];
            if (!(ModelConstants.Multiplicity_One == assocEnd1.Multiplicity.Value
                  && (ModelConstants.Multiplicity_One == assocEnd2.Multiplicity.Value ||
                      ModelConstants.Multiplicity_ZeroOrOne == assocEnd2.Multiplicity.Value))
                ||
                (ModelConstants.Multiplicity_One == assocEnd2.Multiplicity.Value
                 && (ModelConstants.Multiplicity_One == assocEnd1.Multiplicity.Value ||
                     ModelConstants.Multiplicity_ZeroOrOne == assocEnd1.Multiplicity.Value)))
            {
                return false;
            }

            // get C-side EntityTypes for each AssociationEnd
            var et1 = assocEnd1.Type.Target as ConceptualEntityType;
            var et2 = assocEnd2.Type.Target as ConceptualEntityType;
            if (null == et1)
            {
                Debug.Fail("EntityType et1 is not a ConceptualEntityType");
                return false;
            }
            else if (null == et2)
            {
                Debug.Fail("EntityType et2 is not a ConceptualEntityType");
                return false;
            }

            // check not both pointing to same entity type (i.e. self-association)
            if (et1.Equals(et2))
            {
                return false;
            }

            // check inheritance relationship

            // First identify the association table and which end 
            // contains that table
            var et1Id = _modelRepresentingDatabase.GetEntityTypeIdentityForEntityType(et1);
            var et2Id = _modelRepresentingDatabase.GetEntityTypeIdentityForEntityType(et2);
            if (null == et1Id)
            {
                Debug.Fail("Could not find EntityTypeIdentity for et1 " + et1.ToPrettyString());
                return false;
            }
            else if (null == et2Id)
            {
                Debug.Fail("Could not find EntityTypeIdentity for et2 " + et2.ToPrettyString());
                return false;
            }

            EntityTypeIdentity etIdNotContainingAssocTable = null;

            var tables = assocId.AssociationTables.GetEnumerator();
            tables.MoveNext();
            var assocTable = tables.Current;

            if (tables.MoveNext())
            {
                //
                // If we get here, the properties involved in a Ref Constraint were mapped to multiple tables.  This implies inheritance 
                // or horizontal partitioning on the database (ie, the dependent end of the RC is an entity mapped to more than one table, 
                // so the RC spans tables).  
                //
                // We don't ever expect this to be the case for a model generated from model-gen APIs, so assert.  
                //
                Debug.Fail(
                    "An association is mapped to more than one table on the dependent via a referential constraint.  We didn't expect this to happen for a model generated from model gen APIs");
                // just return false here.  It should be OK for us to include this association, and the user can make the call on what to do.
                return false;
            }

            if (et1Id.ContainsDatabaseObject(assocTable))
            {
                etIdNotContainingAssocTable = et2Id;
            }
            else if (et2Id.ContainsDatabaseObject(assocTable))
            {
                etIdNotContainingAssocTable = et1Id;
            }
            else
            {
                // Neither end of a 1:1 or 1:0..1 Association contains the association table.
                // This is an error.
                Debug.Fail(
                    "Neither end of the Association " + assoc.ToPrettyString() + " contains the association table " + assocTable.ToString());
                return false;
            }

            // now find the C-side EntityTypes in the existing artifact
            // for that association table
            var existingEntityTypesForAssocTable =
                _preExistingModel.GetConceptualEntityTypesForDatabaseObject(assocTable);

            // now if any of these EntityTypes has an ancestor which maps 
            // to any of the tables/views in the EntityTypeIdentity of 
            // the _other_ AssociationEnd then we have found the inheritance
            // relationship which replaced this Association
            // Note: this covers the case where one or the other end is a new table/view:
            // if assocTable is "new" then existingEntityTypesForAssocTable will be null,
            // if the other end of the association is "new" then no existing EntityType
            // will have an ancestor type that maps to the tables/views in the other end.
            if (null != existingEntityTypesForAssocTable)
            {
                foreach (var cet in existingEntityTypesForAssocTable)
                {
                    foreach (var tableOrView in etIdNotContainingAssocTable.TablesAndViews)
                    {
                        if (_preExistingModel.HasAncestorTypeThatMapsToDbObject(cet, tableOrView))
                        {
                            return true;
                        }
                    }
                }
            }

            // check split-entity

            // find the C-Side EntityTypes in the existing artifact for both ends
            var existingEntityType1 = FindMatchingConceptualEntityTypeInExistingArtifact(
                et1,
                tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact);
            var existingEntityType2 = FindMatchingConceptualEntityTypeInExistingArtifact(
                et2,
                tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact);

            // if either there are no matching EntityTypes or the 2 matching EntityTypes
            // are different in the existing artifact then this is not split-entity
            if (null == existingEntityType1
                || null == existingEntityType2
                || !existingEntityType1.Equals(existingEntityType2))
            {
                return false;
            }

            // check referential constraint
            var refConstraint = assoc.ReferentialConstraint;
            if (null == refConstraint)
            {
                return false;
            }

            return (HasIdenticalKeyToEntityType(refConstraint.Principal)
                    && HasIdenticalKeyToEntityType(refConstraint.Dependent));
        }

        private static bool HasIdenticalKeyToEntityType(ReferentialConstraintRole refConstraintRole)
        {
            if (null == refConstraintRole
                || null == refConstraintRole.Role
                || null == refConstraintRole.Role.Target
                || null == refConstraintRole.Role.Target.Type
                || null == refConstraintRole.Role.Target.Type.Target)
            {
                // no resolved EntityType - cannot match keys
                return false;
            }

            // find list of properties representing PK of EntityType
            var et = refConstraintRole.Role.Target.Type.Target;
            var cet = et as ConceptualEntityType;

            Debug.Assert(
                cet != null,
                "expected EntityType of type ConceptualEntityType, instead type is " + (et == null ? "null" : et.GetType().FullName));

            var etKeyProps = new HashSet<Property>();
            foreach (var p in cet.ResolvableTopMostBaseType.ResolvableKeys)
            {
                etKeyProps.Add(p);
            }

            // find list of properties referenced in the ReferentialConstraintRole
            var refConstraintProps = new HashSet<Property>();
            foreach (var propRef in refConstraintRole.PropertyRefs)
            {
                if (null != propRef.Name.Target)
                {
                    refConstraintProps.Add(propRef.Name.Target);
                }
            }

            // if the counts of properties in refConstraintProps and etKeyProps
            // are not the same then the keys cannot be identical
            if (etKeyProps.Count != refConstraintProps.Count)
            {
                return false;
            }

            // remove all the refConstraintProps properties from etKeyProps,
            // if any are left then the key has not been covered
            etKeyProps.ExceptWith(refConstraintProps);
            return (etKeyProps.Count == 0);
        }

        private static Property FindMappedCSidePropertyForSSideProperty(Property sSideProp)
        {
            Debug.Assert(
                !sSideProp.EntityModel.IsCSDL,
                "sSideProp with name " + sSideProp.NormalizedNameExternal + " is from CSDL, it must be from SSDL");

            // just return the first C-side Property we find that maps to
            // the sSideProp argument - just need type etc information from
            // it in order to clone a C-side Property in the existing artifact
            var scalarProps = sSideProp.GetAntiDependenciesOfType<ScalarProperty>();
            foreach (var scalarProp in scalarProps)
            {
                // if the scalarProp is not from a MappingFragment then ignore
                // (we want the column mapping, not e.g. an EndProperty mapping)
                if (!(scalarProp.Parent is MappingFragment))
                {
                    continue;
                }

                var cSideProp = scalarProp.Name.Target;
                if (null != cSideProp)
                {
                    return cSideProp;
                }
            }

            return null;
        }

        private static ICollection<Property> FindAllMappedSSidePropertiesForCSideProperty(Property cSideProp)
        {
            Debug.Assert(
                cSideProp.EntityModel.IsCSDL,
                "cSideProp with name " + cSideProp.NormalizedNameExternal + " is from SSDL, it must be from CSDL");

            ICollection<Property> sSideProps = new List<Property>();
            // return all the S-side Properties we find that map to
            // the cSideProp argument
            var scalarProps = cSideProp.GetAntiDependenciesOfType<ScalarProperty>();
            foreach (var scalarProp in scalarProps)
            {
                // if the scalarProp is not from a MappingFragment then ignore
                // (we want the column mapping, not e.g. an EndProperty mapping)
                if (!(scalarProp.Parent is MappingFragment))
                {
                    continue;
                }

                var sSideProp = scalarProp.ColumnName.Target;
                if (null != sSideProp)
                {
                    sSideProps.Add(sSideProp);
                }
            }

            return sSideProps;
        }

        private Property FindSSidePropInExistingArtifact(Property sSidePropInTempArtifact)
        {
            if (null == sSidePropInTempArtifact)
            {
                Debug.Fail("Null sSidePropInTempArtifact");
                return null;
            }

            var existingArtifactSet = _preExistingModel.Artifact.ArtifactSet;
            if (null == existingArtifactSet)
            {
                Debug.Fail("Null ArtifactSet");
                return null;
            }

            var sSidePropSymbolInTempArtifactSet = sSidePropInTempArtifact.NormalizedName;
            var sSidePropInExistingArtifact = existingArtifactSet.LookupSymbol(sSidePropSymbolInTempArtifactSet) as Property;
            if (null == sSidePropInTempArtifact)
            {
                Debug.Fail(
                    "Cannot find matching Property for sSidePropInTempArtifact " + sSidePropInTempArtifact.ToPrettyString()
                    + " in existing artifact");
                return null;
            }

            return sSidePropInExistingArtifact;
        }

        private static StorageEntityType FindMatchingStorageEntityTypeInExistingArtifact(
            StorageEntityType entityTypeFromTempArtifact)
        {
            if (null == entityTypeFromTempArtifact)
            {
                Debug.Fail("Null entityTypeFromTempArtifact");
                return null;
            }

            // if the EntityType is S-side then (since we have already replaced the SSDL)
            // it is safe to match just by name
            Debug.Assert(entityTypeFromTempArtifact.EntityModel is BaseEntityModel, "EntityModel should be a BaseEntityModel");
            Debug.Assert(!entityTypeFromTempArtifact.EntityModel.IsCSDL, "StorageEntityType is from CSDL?  This is wrong!");
            var et = ModelHelper.FindEntityType(entityTypeFromTempArtifact.EntityModel, entityTypeFromTempArtifact.LocalName.Value);
            var set = et as StorageEntityType;
            Debug.Assert(null != set, "Matching EntityType from temp artifact is not a StorageEntityType");
            return set;
        }

        private ConceptualEntityType FindMatchingConceptualEntityTypeInExistingArtifact(
            ConceptualEntityType entityTypeFromTempArtifact,
            Dictionary<EntityType, EntityType> tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact)
        {
            if (null == entityTypeFromTempArtifact)
            {
                Debug.Fail("null entityTypeFromTempArtifact");
                return null;
            }

            Debug.Assert(entityTypeFromTempArtifact.EntityModel.IsCSDL, "ConceptualEntityType is not from CSDL?  This is wrong!");

            // if the EntityType passed in matches an EntityType which was
            // added in AddNewConceptualEntityTypesFromArtifact() then return
            // that one
            if (null != tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact)
            {
                EntityType etInExistingArtifact;
                tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact.TryGetValue(entityTypeFromTempArtifact, out etInExistingArtifact);
                if (null != etInExistingArtifact)
                {
                    var cet = etInExistingArtifact as ConceptualEntityType;
                    Debug.Assert(cet != null, "Matching EntityType in existing artifact is not a ConceptualEntityType");
                    return cet;
                }
            }

            var tempEntityTypeId =
                _modelRepresentingDatabase.GetEntityTypeIdentityForEntityType(entityTypeFromTempArtifact);
            if (null == tempEntityTypeId)
            {
                Debug.Fail("Null EntityTypeIdentity for temp artifact entity type " + entityTypeFromTempArtifact.ToPrettyString());
                return null;
            }

            foreach (var dbObj in tempEntityTypeId.TablesAndViews)
            {
                var existingEntityTypes =
                    _preExistingModel.GetConceptualEntityTypesForDatabaseObject(dbObj);
                if (null != existingEntityTypes)
                {
                    // return first matching EntityType
                    foreach (var existingEntityType in existingEntityTypes)
                    {
                        // if the existingEntityType has a root type which maps to this
                        // DatabaseObject then return that instead
                        var rootExistingType = _preExistingModel.FindRootAncestorTypeThatMapsToDbObject(existingEntityType, dbObj);
                        if (null != rootExistingType)
                        {
                            var cet = rootExistingType as ConceptualEntityType;
                            Debug.Assert(cet != null, "discovered rootExistingType is not a ConceptualEntityType");
                            return cet;
                        }
                        else
                        {
                            var cet = existingEntityType;
                            Debug.Assert(cet != null, "discovered existingEntityType is not a ConceptualEntityType");
                            return cet;
                        }
                    }
                }
            }

            return null;
        }

        // Find a C-Side Property in the passed in EntityType in the existing artifact
        // which matches the identity of the passed in C-side Property from the temp artifact
        private static Property FindMatchingPropertyInExistingArtifactEntityType(
            Property propInTempArtifact, EntityType entityTypeInExistingArtifact)
        {
            if (null == propInTempArtifact)
            {
                Debug.Fail("Null Property for temp artifact");
                return null;
            }

            if (null == entityTypeInExistingArtifact)
            {
                Debug.Fail("Null EntityType for existing artifact");
                return null;
            }

            var entityIsCSide = entityTypeInExistingArtifact.EntityModel.IsCSDL;
            var propIsCSide = propInTempArtifact.EntityType.EntityModel.IsCSDL;

            if (entityIsCSide != propIsCSide)
            {
                Debug.Fail(
                    " Mismatched Property and EntityType passed to FindMatchingPropertyInExistingArtifactEntityType. " +
                    "Property has IsCSDL = " + propIsCSide + ", EntityType has IsCSDL = " + entityIsCSide);
                return null;
            }

            // Find list of possible S-side properties from which to construct identity
            ICollection<Property> sSidePropsInTempArtifact;
            if (propIsCSide)
            {
                sSidePropsInTempArtifact =
                    FindAllMappedSSidePropertiesForCSideProperty(propInTempArtifact);
            }
            else
            {
                sSidePropsInTempArtifact = new List<Property>();
                sSidePropsInTempArtifact.Add(propInTempArtifact);
            }

            // loop over them comparing each one (for runtime generated files there
            // will be only 1 Property in list)
            var cet = entityTypeInExistingArtifact as ConceptualEntityType;
            foreach (var sSidePropInTempArtifact in sSidePropsInTempArtifact)
            {
                var propertyIdFromTempArtifact =
                    DatabaseColumn.CreateFromProperty(sSidePropInTempArtifact);

                IEnumerable<Property> props;

                if (cet != null)
                {
                    props = cet.SafeInheritedAndDeclaredProperties;
                }
                else
                {
                    props = entityTypeInExistingArtifact.Properties();
                }

                // now find matching property within the existing EntityType 
                // or its inherited types
                foreach (var prop in props)
                {
                    ICollection<Property> sSidePropsInExistingArtifact;
                    if (entityIsCSide)
                    {
                        sSidePropsInExistingArtifact =
                            FindAllMappedSSidePropertiesForCSideProperty(prop);
                    }
                    else
                    {
                        sSidePropsInExistingArtifact = new List<Property>();
                        sSidePropsInExistingArtifact.Add(prop);
                    }

                    // now loop over all mapped S-side props to see if we have a match
                    // (Note: we need to look at all of them for e.g. 1 C-side EntityType
                    // which is mapped to 2+ S-Side EntityTypes with 1:0..1 relationships;
                    // under those conditions the key columns can be mapped to multiple tables
                    // and we need to compare to all of the mappings to get the right identity)
                    foreach (var sSidePropInExistingArtifact in sSidePropsInExistingArtifact)
                    {
                        var propertyIdFromExistingArtifact =
                            DatabaseColumn.CreateFromProperty(sSidePropInExistingArtifact);
                        if (propertyIdFromExistingArtifact.Equals(propertyIdFromTempArtifact))
                        {
                            return prop;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Creates a new EntitySetMapping in the existing EntityContainerMapping
        ///     based on another EntitySetMapping (esmToClone) in a different artifact.
        ///     Uses the FindMatchingEntityTypeInExistingArtifact() to match EntityTypes
        ///     within the EntitySetMapping.
        /// </summary>
        private EntitySetMapping CloneEntitySetMapping(
            CommandProcessorContext cpc, EntitySetMapping esmToClone,
            EntityContainerMapping existingEntityContainerMapping, ConceptualEntitySet existingEntitySet,
            Dictionary<EntityType, EntityType> tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact)
        {
            var createESM = new CreateEntitySetMappingCommand(existingEntityContainerMapping, existingEntitySet);
            CommandProcessor.InvokeSingleCommand(cpc, createESM);
            var esm = createESM.EntitySetMapping;

            foreach (var etmToBeCloned in esmToClone.EntityTypeMappings())
            {
                var bindings = etmToBeCloned.TypeName.Bindings;
                if (bindings.Count != 1)
                {
                    Debug.Fail(
                        "EntityTypeMapping to be cloned " + etmToBeCloned.ToPrettyString() +
                        " has bindings count = " + bindings.Count + ". We only support 1 binding.");
                }
                else
                {
                    var b = bindings.First();
                    var etToBeCloned = b.Target as ConceptualEntityType;
                    Debug.Assert(etToBeCloned != null, "EntityType target of binding is not ConceptualEntityType");
                    var etInExistingArtifact =
                        FindMatchingConceptualEntityTypeInExistingArtifact(
                            etToBeCloned,
                            tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact);
                    if (null != etInExistingArtifact)
                    {
                        CreateEntityTypeMappingCommand.CloneEntityTypeMapping(
                            cpc,
                            etmToBeCloned,
                            esm,
                            etInExistingArtifact,
                            etmToBeCloned.Kind);
                    }
                    else
                    {
                        throw new UpdateModelFromDatabaseException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.UpdateFromDatabaseEntitySetMappingCannotFindEntityType,
                                etToBeCloned.ToPrettyString()));
                    }
                }
            }

            return esm;
        }

        /// <summary>
        ///     Clone the Association with its ReferentialConstraint (if available), AssociationSet
        ///     and AssociationSetMapping. If the ReferentialConstraint is available in the temp artifact
        ///     but we cannot find matching properties in the existing artifact then the whole Association
        ///     (and AssociationSet and AssociationSetMapping) will not be cloned, a warning message will
        ///     be issued but otherwise the process is not stopped.
        ///     But if the Association cannot be created for any other reason that's an error and an
        ///     UpdateModelFromDatabaseException will be thrown.
        /// </summary>
        /// <param name="cpc">CommandProcessorContext for the commands to be issued</param>
        /// <param name="existingArtifact">the existing artifact in which to make these changes</param>
        /// <param name="assocInTempArtifact">the Association in the temp artifact to be cloned</param>
        /// <param name="end1InTempArtifact">the end of the Association in the temp artifact to be cloned to be treated as End1</param>
        /// <param name="end2InTempArtifact">the end of the Association in the temp artifact to be cloned to be treated as End2</param>
        /// <param name="navProp1InTempArtifact">the NavigationProperty for End1 in the temp artifact to be cloned</param>
        /// <param name="navProp2InTempArtifact">the NavigationProperty for End2 in the temp artifact to be cloned</param>
        /// <param name="end1EntityTypeInExistingArtifact">the EntityType in the existing artifact matching the End1 target in the temp artifact</param>
        /// <param name="end2EntityTypeInExistingArtifact">the EntityType in the existing artifact matching the End2 target in the temp artifact</param>
        /// <param name="tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact">
        ///     a Dictionary mapping temp artifact EntityTypes which have
        ///     been identified as new to their equivalent in the existing artifact
        /// </param>
        private void CloneAssociation(
            CommandProcessorContext cpc, EFArtifact existingArtifact, Association assocInTempArtifact,
            AssociationEnd end1InTempArtifact, AssociationEnd end2InTempArtifact,
            NavigationProperty navProp1InTempArtifact, NavigationProperty navProp2InTempArtifact,
            ConceptualEntityType end1EntityTypeInExistingArtifact, ConceptualEntityType end2EntityTypeInExistingArtifact,
            Dictionary<EntityType, EntityType> tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact)
        {
            Association newAssocInExistingArtifact = null;
            AssociationEnd newAssocEnd1 = null;
            AssociationEnd newAssocEnd2 = null;
            if (assocInTempArtifact.ReferentialConstraint == null)
            {
                // if there is no ReferentialConstraint to clone then just try to 
                // create the Association and AssociationSet
                var cmd = new CreateConceptualAssociationCommand(
                    assocInTempArtifact.LocalName.Value,
                    end1EntityTypeInExistingArtifact, end1InTempArtifact.Multiplicity.Value, navProp1InTempArtifact.LocalName.Value,
                    end2EntityTypeInExistingArtifact, end2InTempArtifact.Multiplicity.Value, navProp2InTempArtifact.LocalName.Value,
                    true, false);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
                newAssocInExistingArtifact = cmd.CreatedAssociation;
                newAssocEnd1 = cmd.End1;
                newAssocEnd2 = cmd.End2;
            }
            else
            {
                // There is a ReferentialConstraint - so we need to check whether we can find matching properties
                // for the Principal and Dependent roles of the ReferentialConstraint. If we can't find
                // matching properties then log a warning message and return. Otherwise attempt to create
                // the Association (and its AssociationSet) followed by a matching ReferentialConstraint.
                //
                // Note: ShouldCreateAssociationGivenReferentialConstraint() produces 2 sets of information:
                // (1) the sets of matching properties to use if we can find matching properties for the Principal
                // and Dependent roles, and (2) the sets of property names to include in the error message if 
                // we cannot find matching properties for the Principal and Dependent roles.
                // If ShouldCreateAssociationGivenReferentialConstraint() returns true we use the first set of
                // information to create the ReferentialConstraint after having created the Association and
                // AssociationSet. If it returns false we use the second set of information to construct the
                // warning message.
                var refConstraintInTempArtifact = assocInTempArtifact.ReferentialConstraint;
                bool end1IsPrincipalEnd;
                List<Property> principalPropertiesInExistingArtifact;
                List<Property> dependentPropertiesInExistingArtifact;
                List<string> unfoundPrincipalProperties;
                List<string> unfoundDependentProperties;
                if (ShouldCreateAssociationGivenReferentialConstraint(
                    refConstraintInTempArtifact, end1InTempArtifact,
                    end2InTempArtifact, end1EntityTypeInExistingArtifact, end2EntityTypeInExistingArtifact, out end1IsPrincipalEnd,
                    out principalPropertiesInExistingArtifact, out dependentPropertiesInExistingArtifact,
                    out unfoundPrincipalProperties, out unfoundDependentProperties))
                {
                    // create the new Association and AssociationSet
                    var cmd = new CreateConceptualAssociationCommand(
                        assocInTempArtifact.LocalName.Value,
                        end1EntityTypeInExistingArtifact, end1InTempArtifact.Multiplicity.Value, navProp1InTempArtifact.LocalName.Value,
                        end2EntityTypeInExistingArtifact, end2InTempArtifact.Multiplicity.Value, navProp2InTempArtifact.LocalName.Value,
                        true, false);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    newAssocInExistingArtifact = cmd.CreatedAssociation;
                    newAssocEnd1 = cmd.End1;
                    newAssocEnd2 = cmd.End2;

                    // Add in the ReferentialConstraint for the new Association
                    if (null != newAssocInExistingArtifact)
                    {
                        AddReferentialConstraintForAssociation(
                            cpc, cmd, end1IsPrincipalEnd, principalPropertiesInExistingArtifact, dependentPropertiesInExistingArtifact);
                    }
                }
                else
                {
                    // Unable to find matching properties for the Principal and Dependent roles of 
                    // the ReferentialConstraint. So log a warning message.
                    newAssocInExistingArtifact = null;
                    EntityType principalEndEntityType = end1IsPrincipalEnd
                                                            ? end1EntityTypeInExistingArtifact
                                                            : end2EntityTypeInExistingArtifact;
                    EntityType dependentEndEntityType = end1IsPrincipalEnd
                                                            ? end2EntityTypeInExistingArtifact
                                                            : end1EntityTypeInExistingArtifact;
                    LogWarningMessageForReferentialConstraintProperties(
                        assocInTempArtifact.LocalName.Value,
                        principalEndEntityType, dependentEndEntityType, unfoundPrincipalProperties, unfoundDependentProperties);

                    // having logged the warning message we do not need to attempt to create
                    // the AssociationSetMapping nor throw an exception due to the lack of the
                    // created Association - so just return
                    return;
                }
            }

            // if we have failed to create an Association at this stage that's a serious error
            // so throw an exception to indicate the failure
            if (null == newAssocInExistingArtifact)
            {
                throw new UpdateModelFromDatabaseException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.UpdateFromDatabaseCannotCreateAssociation,
                        assocInTempArtifact.ToPrettyString()));
            }

            // update OnDeleteActions to match temp artifact
            UpdateOnDeleteAction(cpc, newAssocEnd1, end1InTempArtifact);
            UpdateOnDeleteAction(cpc, newAssocEnd2, end2InTempArtifact);

            // add a new AssociationSetMapping for the new Association
            AddAssociationSetMappingForConceptualAssociation(
                cpc, existingArtifact, assocInTempArtifact,
                newAssocInExistingArtifact, tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact);
        }

        private static void UpdateOnDeleteAction(CommandProcessorContext cpc, AssociationEnd newAssocEnd, AssociationEnd tempArtifactAssocEnd)
        {
            Debug.Assert(newAssocEnd != null, "newAssoc should not be null");
            Debug.Assert(tempArtifactAssocEnd != null, "tempArtifactAssocEnd should not be null");
            if (newAssocEnd == null
                || tempArtifactAssocEnd == null)
            {
                return;
            }

            // ensure the OnDeleteAction for the existing artifact is the same as that for the temp artifact
            if (null == tempArtifactAssocEnd.OnDeleteAction
                && null != newAssocEnd.OnDeleteAction)
            {
                // temp artifact has no OnDeleteAction - so delete the one in the existing artifact
                newAssocEnd.OnDeleteAction.Delete();
            }
            else if (null != tempArtifactAssocEnd.OnDeleteAction
                     && null != tempArtifactAssocEnd.OnDeleteAction.Action)
            {
                var tempArtifactOnDeleteAction = tempArtifactAssocEnd.OnDeleteAction.Action.Value;
                if (null == newAssocEnd.OnDeleteAction)
                {
                    // existing artifact has no OnDeleteAction - so create a new one and assign the value from the temp artifact
                    var cmd = new CreateOnDeleteActionCommand(newAssocEnd, tempArtifactOnDeleteAction);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
                    // use ordinal comparison as possible values for this attribute are fixed regardless of locale
                else if (false == newAssocEnd.OnDeleteAction.Action.Value.Equals(tempArtifactOnDeleteAction, StringComparison.Ordinal))
                {
                    // existing artifact has an OnDeleteAction but the value does not match - so assign the value from the temp artifact
                    var cmd =
                        new UpdateDefaultableValueCommand<string>(newAssocEnd.OnDeleteAction.Action, tempArtifactOnDeleteAction);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
            }
        }

        private void
            AddAssociationSetMappingForConceptualAssociation(
            CommandProcessorContext cpc, EFArtifact existingArtifact,
            Association assocInTempArtifact, Association assocInExistingArtifact,
            Dictionary<EntityType, EntityType> tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact)
        {
            // first find the AssociationSetMapping in the tempArtifact for this association
            var asmInTempArtifact =
                ModelHelper.FindAssociationSetMappingForConceptualAssociation(assocInTempArtifact);
            if (asmInTempArtifact == null)
            {
                if (!EdmFeatureManager.GetForeignKeysInModelFeatureState(existingArtifact.SchemaVersion).IsEnabled()
                    || assocInTempArtifact.IsManyToMany)
                {
                    // this is an error condition - we should have an association set mapping in this case, so assert and throw an exception
                    throw new UpdateModelFromDatabaseException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.UpdateFromDatabaseAssociationSetMappingCannotFind,
                            assocInTempArtifact.ToPrettyString()));
                }
                else
                {
                    // we don't expect an association set mapping here
                    return;
                }
            }

            // next find the S-side EntitySet in the tempArtifact for this AssociationSetMapping
            var storeEntitySetInTempArtifact = asmInTempArtifact.StoreEntitySet.Target;
            if (storeEntitySetInTempArtifact == null)
            {
                throw new UpdateModelFromDatabaseException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.UpdateFromDatabaseAssociationSetMappingCannotFindTempSSideEntitySet,
                        asmInTempArtifact.ToPrettyString()));
            }

            // now find the S-side EntitySet in the existingArtifact which matches this
            // Note: LocalName's will be the same as the SSDL has been replaced
            StorageEntitySet storeEntitySetInExistingArtifact = null;
            foreach (var es in existingArtifact.StorageModel().FirstEntityContainer.EntitySets())
            {
                if (es.LocalName.Value == storeEntitySetInTempArtifact.LocalName.Value)
                {
                    storeEntitySetInExistingArtifact = es as StorageEntitySet;
                    break;
                }
            }

            if (storeEntitySetInExistingArtifact == null)
            {
                throw new UpdateModelFromDatabaseException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.UpdateFromDatabaseAssociationSetMappingCannotFindMatchingSSideEntitySet,
                        storeEntitySetInTempArtifact.LocalName.Value));
            }

            // now create a new AssociationSetMapping in the existingArtifact using the data
            // accumulated above
            CloneAssociationSetMapping(
                cpc,
                asmInTempArtifact,
                existingArtifact.MappingModel().FirstEntityContainerMapping,
                assocInExistingArtifact.AssociationSet,
                assocInExistingArtifact,
                storeEntitySetInExistingArtifact,
                tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact
                );
        }

        /// <summary>
        ///     Creates a new AssociationSetMapping in the existing EntityContainerMapping
        ///     based on another AssociationSetMapping (asmToClone) in a different artifact.
        ///     All the other parameters are presumed to already exist in the same artifact
        ///     as the EntityContainerMapping.
        /// </summary>
        private AssociationSetMapping CloneAssociationSetMapping(
            CommandProcessorContext cpc, AssociationSetMapping asmToClone,
            EntityContainerMapping existingEntityContainerMapping, AssociationSet existingAssociationSet,
            Association existingAssociation, StorageEntitySet existingStorageEntitySet,
            Dictionary<EntityType, EntityType> tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact)
        {
            var createASM = new CreateAssociationSetMappingCommand(
                existingEntityContainerMapping, existingAssociationSet, existingAssociation, existingStorageEntitySet);
            CommandProcessor.InvokeSingleCommand(cpc, createASM);
            var asmInExistingArtifact = createASM.AssociationSetMapping;

            if (null == asmInExistingArtifact)
            {
                throw new UpdateModelFromDatabaseException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.UpdateFromDatabaseCannotCreateAssociationSetMapping,
                        existingAssociationSet.ToPrettyString()));
            }

            // cannot just look for an AssociationSetEnd with the same Role name in
            // the existing artifact as the role may have changed when the Association was
            // copied into the existing artifact - but we do know the ends were created in 
            // the same order - so simply match them up
            var existingAssocSetEnds = existingAssociationSet.AssociationSetEnds().ToArray();
            if (2 != existingAssocSetEnds.Length)
            {
                throw new UpdateModelFromDatabaseException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.UpdateFromDatabaseAssociationSetMappingWrongNumberAssociationSetEnds,
                        existingAssociationSet.ToPrettyString(),
                        existingAssocSetEnds.Length));
            }

            var endsToClone = asmToClone.EndProperties().ToArray();
            if (2 != endsToClone.Length)
            {
                throw new UpdateModelFromDatabaseException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.UpdateFromDatabaseAssociationSetMappingWrongNumberAssociationSetEnds,
                        existingAssociationSet.ToPrettyString(),
                        existingAssocSetEnds.Length));
            }

            for (var i = 0; i < 2; i++)
            {
                var aseInExistingArtifact = existingAssocSetEnds[i];
                var endToClone = endsToClone[i];
                CloneEndProperty(
                    cpc, endToClone, asmInExistingArtifact, aseInExistingArtifact, tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact);
            }

            return asmInExistingArtifact;
        }

        /// <summary>
        ///     Determines whether we should create an Association in the existing artifact given
        ///     the ReferentialConstraint and AssociationEnd info from the temp artifact
        /// </summary>
        private static bool ShouldCreateAssociationGivenReferentialConstraint(
            ReferentialConstraint refConstraintInTempArtifact,
            AssociationEnd end1InTempArtifact, AssociationEnd end2InTempArtifact,
            ConceptualEntityType end1EntityTypeInExistingArtifact, ConceptualEntityType end2EntityTypeInExistingArtifact,
            out bool end1IsPrincipalEnd, out List<Property> principalPropertiesInExistingArtifact,
            out List<Property> dependentPropertiesInExistingArtifact, out List<string> unfoundPrincipalProperties,
            out List<string> unfoundDependentProperties)
        {
            end1IsPrincipalEnd = true;
            principalPropertiesInExistingArtifact = new List<Property>();
            unfoundPrincipalProperties = new List<string>();
            dependentPropertiesInExistingArtifact = new List<Property>();
            unfoundDependentProperties = new List<string>();
            ConceptualEntityType principalEndEntityType = null;
            ConceptualEntityType dependentEndEntityType = null;

            if (refConstraintInTempArtifact == null)
            {
                Debug.Fail("Should not have null ReferentialConstraint");
                // no ReferentialConstraint in temp artifact to clone
                return false;
            }

            // determine which end is principal and which dependent
            if (refConstraintInTempArtifact.Principal.Role.Target == end1InTempArtifact)
            {
                Debug.Assert(
                    refConstraintInTempArtifact.Dependent.Role.Target == end2InTempArtifact,
                    "Unexpected end value for Dependent Role of ReferentialConstraint when Principal Role matches end1 from temp artifact");
                end1IsPrincipalEnd = true;
                principalEndEntityType = end1EntityTypeInExistingArtifact;
                dependentEndEntityType = end2EntityTypeInExistingArtifact;
            }
            else if (refConstraintInTempArtifact.Principal.Role.Target == end2InTempArtifact)
            {
                Debug.Assert(
                    refConstraintInTempArtifact.Dependent.Role.Target == end1InTempArtifact,
                    "Unexpected end value for Dependent Role of ReferentialConstraint when Principal Role matches end2 from temp artifact");
                end1IsPrincipalEnd = false;
                principalEndEntityType = end2EntityTypeInExistingArtifact;
                dependentEndEntityType = end1EntityTypeInExistingArtifact;
            }
            else
            {
                Debug.Fail("Couldn't identify principal & dependent end");
                return false;
            }

            // find the principal properties in the existing doc to include in the referential constraint
            foreach (var tempProp in refConstraintInTempArtifact.Principal.Properties)
            {
                var prop = FindMatchingPropertyInExistingArtifactEntityType(tempProp, principalEndEntityType);
                if (prop != null)
                {
                    principalPropertiesInExistingArtifact.Add(prop);
                }
                else
                {
                    unfoundPrincipalProperties.Add(tempProp.LocalName.Value);
                }
            }

            // find the dependent properties in the existing doc to include in the referential constraint
            foreach (var tempProp in refConstraintInTempArtifact.Dependent.Properties)
            {
                var prop = FindMatchingPropertyInExistingArtifactEntityType(tempProp, dependentEndEntityType);
                if (prop != null)
                {
                    dependentPropertiesInExistingArtifact.Add(prop);
                }
                else
                {
                    unfoundDependentProperties.Add(tempProp.LocalName.Value);
                }
            }

            // return true if we found matching properties for all the properties in
            // both the principal and dependent ends, false otherwise
            if (unfoundDependentProperties.Count == 0
                && unfoundPrincipalProperties.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///     Adds a ReferentialConstraint on the assocInTempArtifact to the newAssocInExistingArtifact
        /// </summary>
        private static ReferentialConstraint
            AddReferentialConstraintForAssociation(
            CommandProcessorContext cpc, CreateConceptualAssociationCommand cmd, bool end1IsPrincipalEnd,
            List<Property> principalPropertiesInExistingArtifact, List<Property> dependentPropertiesInExistingArtifact)
        {
            if (cmd.End1 == null)
            {
                Debug.Fail("Should not have null AssociationEnd End1 from CreateConceptualAssociationCommand");
                return null;
            }
            else if (cmd.End2 == null)
            {
                Debug.Fail("Should not have null AssociationEnd End2 from CreateConceptualAssociationCommand");
                return null;
            }

            //assign principal and dependent ends
            var principalEndInExistingArtifact = end1IsPrincipalEnd ? cmd.End1 : cmd.End2;
            var dependentEndInExistingArtifact = end1IsPrincipalEnd ? cmd.End2 : cmd.End1;

            // now create the referential constraint
            var rcCmd = new CreateReferentialConstraintCommand(
                principalEndInExistingArtifact, dependentEndInExistingArtifact,
                principalPropertiesInExistingArtifact, dependentPropertiesInExistingArtifact);
            CommandProcessor.InvokeSingleCommand(cpc, rcCmd);
            var newRC = rcCmd.ReferentialConstraint;

            return newRC;
        }

        /// <summary>
        ///     Helper method to log the VS warning message given the principal and dependent properties
        ///     for which we could not find matches in the existing artifact
        /// </summary>
        private static void LogWarningMessageForReferentialConstraintProperties(
            string associationName,
            EntityType principalEntityType, EntityType dependentEntityType, List<string> unfoundPrincipalProperties,
            List<string> unfoundDependentProperties)
        {
            if (unfoundDependentProperties.Count > 0
                || unfoundPrincipalProperties.Count > 0)
            {
                // log a warning to the VS error list.
                var propertyList = new StringBuilder();
                var isFirst = true;
                foreach (var p in unfoundPrincipalProperties)
                {
                    if (false == isFirst)
                    {
                        propertyList.Append(Resources.SeparatorCharacterForMultipleItemsInAnErrorMessage);
                    }
                    propertyList.Append(p);
                    isFirst = false;
                }
                foreach (var p in unfoundDependentProperties)
                {
                    if (false == isFirst)
                    {
                        propertyList.Append(Resources.SeparatorCharacterForMultipleItemsInAnErrorMessage);
                    }
                    propertyList.Append(p);
                    isFirst = false;
                }

                var principalEntityTypeName = principalEntityType.LocalName.Value;
                var dependentEntityTypeName = dependentEntityType.LocalName.Value;
                var s = String.Format(
                    CultureInfo.CurrentCulture, Resources.UpdateFromDatabaseUnableToBringRefConstraint, associationName,
                    principalEntityTypeName, dependentEntityTypeName, propertyList);
                var errorMessageTarget = unfoundPrincipalProperties.Count > 0 ? principalEntityType : dependentEntityType;
                var errorInfo = new ErrorInfo(
                    ErrorInfo.Severity.WARNING, s, errorMessageTarget, ErrorCodes.UPDATE_MODEL_FROM_DB_CANT_INCLUDE_REF_CONSTRAINT,
                    ErrorClass.Escher_UpdateModelFromDB);
                HostContext.Instance.LogUpdateModelWizardError(errorInfo, errorMessageTarget.Uri.LocalPath);
            }
        }

        /// <summary>
        ///     Creates a new EndProperty in the existing AssociationSetMapping
        ///     based on another EndProperty (endToClone) in a different artifact.
        ///     All the other parameters are presumed to already exist in the same artifact
        ///     as the AssociationSetMapping.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private EndProperty CloneEndProperty(
            CommandProcessorContext cpc,
            EndProperty endToClone, AssociationSetMapping asmInExistingArtifact, AssociationSetEnd aseInExistingArtifact,
            Dictionary<EntityType, EntityType> tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact)
        {
            var createEnd = new CreateEndPropertyCommand(asmInExistingArtifact, aseInExistingArtifact);
            CommandProcessor.InvokeSingleCommand(cpc, createEnd);
            var endInExistingArtifact = createEnd.EndProperty;

            if (null == endInExistingArtifact)
            {
                throw new UpdateModelFromDatabaseException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.UpdateFromDatabaseCannotCreateAssociationSetMappingEndProperty,
                        aseInExistingArtifact.ToPrettyString()));
            }

            var existingArtifact = cpc.Artifact;
            Debug.Assert(existingArtifact != null, "existingArtifact is null for endToClone " + endToClone.ToPrettyString());

            foreach (var sp in endToClone.ScalarProperties())
            {
                if (null == sp.Name.Target)
                {
                    throw new UpdateModelFromDatabaseException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.UpdateFromDatabaseScalarPropertyNoNameTarget,
                            sp.ToPrettyString()));
                }
                if (null == sp.ColumnName.Target)
                {
                    throw new UpdateModelFromDatabaseException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.UpdateFromDatabaseScalarPropertyNoColumnNameTarget,
                            sp.ToPrettyString()));
                }

                var spCSideEntityTypeInTempArtifact = sp.Name.Target.EntityType as ConceptualEntityType;
                if (null == spCSideEntityTypeInTempArtifact)
                {
                    throw new UpdateModelFromDatabaseException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.UpdateFromDatabaseAssociationSetMappingCannotFindEntityTypeForProperty,
                            sp.Name.Target.ToPrettyString()));
                }

                var spSSideEntityTypeInTempArtifact = sp.ColumnName.Target.EntityType as StorageEntityType;
                if (null == spSSideEntityTypeInTempArtifact)
                {
                    throw new UpdateModelFromDatabaseException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.UpdateFromDatabaseAssociationSetMappingCannotFindEntityTypeForProperty,
                            sp.ColumnName.Target.ToPrettyString()));
                }

                var csdlEntityTypeInExistingArtifact =
                    FindMatchingConceptualEntityTypeInExistingArtifact(
                        spCSideEntityTypeInTempArtifact,
                        tempArtifactCEntityTypeToNewCEntityTypeInExistingArtifact);
                if (null == csdlEntityTypeInExistingArtifact)
                {
                    throw new UpdateModelFromDatabaseException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.UpdateFromDatabaseAssociationSetMappingCannotFindMatchingEntityType,
                            sp.ToPrettyString(),
                            spCSideEntityTypeInTempArtifact.ToPrettyString()));
                }

                var ssdlEntityTypeInExistingArtifact =
                    FindMatchingStorageEntityTypeInExistingArtifact(spSSideEntityTypeInTempArtifact);
                if (null == ssdlEntityTypeInExistingArtifact)
                {
                    throw new UpdateModelFromDatabaseException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.UpdateFromDatabaseAssociationSetMappingCannotFindMatchingEntityType,
                            sp.ToPrettyString(),
                            spSSideEntityTypeInTempArtifact.ToPrettyString()));
                }

                var entityProperty = FindMatchingPropertyInExistingArtifactEntityType(sp.Name.Target, csdlEntityTypeInExistingArtifact);
                if (null == entityProperty)
                {
                    // Cannot find matching property - it must have been unmapped. 
                    // So try to create a new mapped property to which to attach 
                    // this association.

                    // First find S-side Property in temp artifact to which the C-side
                    // Property identified in the AssociationSetMapping is mapped
                    // (Note: cannot use just sp.ColumnName.Target as the S-side Property
                    // used in the AssociationSetMapping can be different from what is used
                    // for the EntitySetMapping in the temp artifact and it is this latter
                    // we need to replicate here).
                    Property sSidePropertyToBeMappedInTempArtifact = null;
                    foreach (var spInTempArtifact in sp.Name.Target.GetAntiDependenciesOfType<ScalarProperty>())
                    {
                        // Ensure that S-side ScalarProperty is from an EntitySetMapping (and not 
                        // an AssociationSetMapping) in the temp artifact.
                        // Can use first one as in temp artifact there is 1:1 mapping.
                        if (null != spInTempArtifact.GetParentOfType(typeof(EntitySetMapping)))
                        {
                            if (null != spInTempArtifact.ColumnName
                                && null != spInTempArtifact.ColumnName.Target)
                            {
                                if (null == sSidePropertyToBeMappedInTempArtifact)
                                {
                                    sSidePropertyToBeMappedInTempArtifact = spInTempArtifact.ColumnName.Target;
                                }
                                else
                                {
                                    // error in temp artifact - there's more than 1 EntitySetMapping ScalarProperty
                                    // mapped to the C-side Property
                                    Debug.Fail(
                                        "C-side Property " + sp.Name.Target.ToPrettyString() +
                                        " has more than 1 ScalarProperty anti-dep with an EntitySetMapping parent. Should be at most 1.");
                                    break;
                                }
                            }
                        }
                    }

                    if (null == sSidePropertyToBeMappedInTempArtifact)
                    {
                        throw new UpdateModelFromDatabaseException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.UpdateFromDatabaseAssociationSetMappingCannotFindSSideForCSideProperty,
                                sp.ToPrettyString(),
                                sp.Name.Target.ToPrettyString()));
                    }

                    // Now find the matching S-side Property in the existing artifact
                    var sSidePropertyToBeMappedInExistingArtifact =
                        FindSSidePropInExistingArtifact(sSidePropertyToBeMappedInTempArtifact);
                    if (null == sSidePropertyToBeMappedInExistingArtifact)
                    {
                        throw new UpdateModelFromDatabaseException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.UpdateFromDatabaseAssociationSetMappingCannotFindMatchingSSideProperty,
                                sp.ToPrettyString(),
                                sSidePropertyToBeMappedInTempArtifact.ToPrettyString()));
                    }

                    // Now create a new C-side Property in the existing artifact mapped 
                    // to the S-side Property we just found
                    entityProperty = CreateNewConceptualPropertyAndMapping(
                        cpc, sp.Name.Target, sSidePropertyToBeMappedInTempArtifact,
                        csdlEntityTypeInExistingArtifact);
                    if (null == entityProperty)
                    {
                        throw new UpdateModelFromDatabaseException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.UpdateFromDatabaseAssociationSetMappingCannotFindOrCreateMatchingProperty,
                                sp.ToPrettyString(),
                                sp.Name.Target.ToPrettyString(),
                                csdlEntityTypeInExistingArtifact.ToPrettyString()));
                    }
                }

                var tableColumn = FindMatchingPropertyInExistingArtifactEntityType(sp.ColumnName.Target, ssdlEntityTypeInExistingArtifact);
                if (null == tableColumn)
                {
                    throw new UpdateModelFromDatabaseException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.UpdateFromDatabaseAssociationSetMappingCannotFindMatchingProperty,
                            sp.ToPrettyString(),
                            sp.ColumnName.Target.ToPrettyString(),
                            ssdlEntityTypeInExistingArtifact.ToPrettyString()));
                }

                var createScalar = new CreateEndScalarPropertyCommand(endInExistingArtifact, entityProperty, tableColumn);
                CommandProcessor.InvokeSingleCommand(cpc, createScalar);
                var existingScalarProp = createScalar.ScalarProperty;
                if (null == existingScalarProp)
                {
                    throw new UpdateModelFromDatabaseException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.UpdateFromDatabaseCannotCreateAssociationSetMappingScalarProperty,
                            entityProperty.ToPrettyString(),
                            tableColumn.ToPrettyString()));
                }
            }

            return endInExistingArtifact;
        }
    }
}
