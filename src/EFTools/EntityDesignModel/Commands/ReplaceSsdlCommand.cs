// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class ReplaceSsdlCommand : Command
    {
        private readonly StorageEntityModel _newArtifactStorageEntityModel;

        internal ReplaceSsdlCommand(StorageEntityModel newArtifactStorageEntityModel)
        {
            Debug.Assert(null != newArtifactStorageEntityModel, "received null newArtifactStorageEntityModel in constructor");
            _newArtifactStorageEntityModel = newArtifactStorageEntityModel;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // check that we have an existing artifact
            var service = cpc.EditingContext.GetEFArtifactService();
            var existingArtifact = service.Artifact as EntityDesignArtifact;
            Debug.Assert(existingArtifact != null, "Null Artifact");
            if (null == existingArtifact)
            {
                return;
            }

            // check that the _new_ artifact's StorageEntityModel exists and has an underlying XObject
            if (null == _newArtifactStorageEntityModel
                || null == _newArtifactStorageEntityModel.XObject)
            {
                Debug.Fail("new artifact StorageModel was null or has null XObject");
                return;
            }

            // save off the old storage model namespace, the old storage Entity Model 
            // and the old storage Entity Container name
            string previousStorageModelNamespace = null;
            StorageEntityModel previousStorageEntityModel = null;
            string previousStorageEntityContainerName = null;
            if (null != existingArtifact.StorageModel)
            {
                previousStorageEntityModel = existingArtifact.StorageModel;
                if (null != previousStorageEntityModel.Namespace
                    && !string.IsNullOrEmpty(previousStorageEntityModel.Namespace.Value))
                {
                    previousStorageModelNamespace = previousStorageEntityModel.Namespace.Value;
                }

                if (null != previousStorageEntityModel.FirstEntityContainer
                    && null != previousStorageEntityModel.FirstEntityContainer.LocalName
                    && !string.IsNullOrEmpty(previousStorageEntityModel.FirstEntityContainer.LocalName.Value))
                {
                    previousStorageEntityContainerName = previousStorageEntityModel.FirstEntityContainer.LocalName.Value;
                }
            }

            // for any S-side objects which have been renamed, re-target the MSL 
            // which targets them in preparation for their replacement below
            RetargetMappingsForRenamedStorageObjects(previousStorageEntityModel, _newArtifactStorageEntityModel);

            // Recurse through the MappingModel updating any references to the old S-side Namespace
            // if different from the new one (must be done before SSDL is replaced which will 
            // unbind all the references)
            if (existingArtifact.MappingModel != null
                && _newArtifactStorageEntityModel != null
                && _newArtifactStorageEntityModel.Namespace != null
                && !string.IsNullOrEmpty(_newArtifactStorageEntityModel.Namespace.Value)
                && !_newArtifactStorageEntityModel.Namespace.Value.Equals(previousStorageModelNamespace, StringComparison.CurrentCulture))
            {
                RecursivelyReplaceStorageNamespaceRefs(
                    existingArtifact.MappingModel, previousStorageModelNamespace, _newArtifactStorageEntityModel.Namespace.Value);
            }

            // replace the old SSDL with the new
            ReplaceSsdl(cpc, existingArtifact, _newArtifactStorageEntityModel.XElement);

            // update the Mapping Model to reference the new SSDL
            UpdateMappingModel(existingArtifact, previousStorageEntityContainerName);
        }

        /// <summary>
        ///     Regenerating the SSDL can sometimes cause existing S-side objects to be renamed.
        ///     In this case, ensure that the references to those old S-side objects from
        ///     the MSL are updated to the new names.
        /// </summary>
        private static void RetargetMappingsForRenamedStorageObjects(
            StorageEntityModel preExistingStorageEntityModel,
            StorageEntityModel newStorageEntityModel)
        {
            Debug.Assert(
                null != preExistingStorageEntityModel,
                "Null preExistingStorageEntityModel in ReplaceSsdlCommand.RetargetMappingsForRenamedStorageObjects()");
            Debug.Assert(
                null != newStorageEntityModel, "Null newStorageEntityModel in ReplaceSsdlCommand.RetargetMappingsForRenamedStorageObjects()");
            if (null == preExistingStorageEntityModel
                || null == newStorageEntityModel)
            {
                return;
            }

            // re-target the mappings which reference Functions
            RetargetMappingsForRenamedStorageFunctions(preExistingStorageEntityModel, newStorageEntityModel);

            // re-target the mappings which reference EntitySets
            var preExistingStorageEntityContainer = preExistingStorageEntityModel.FirstEntityContainer as StorageEntityContainer;
            var newStorageEntityEntityContainer = newStorageEntityModel.FirstEntityContainer as StorageEntityContainer;
            RetargetMappingsForRenamedStorageEntitySets(preExistingStorageEntityContainer, newStorageEntityEntityContainer);

            // Note: DO NOT normalize and resolve the Mapping Model yet - this needs to happen
            // once the SSDL has been replaced.
        }

        private static void RetargetMappingsForRenamedStorageFunctions(
            StorageEntityModel preExistingStorageEntityModel,
            StorageEntityModel newStorageEntityModel)
        {
            if (null == preExistingStorageEntityModel
                || null == newStorageEntityModel)
            {
                return;
            }

            // find all mappings from database identity to S-side Functions for the new artifact
            var newArtifactSSideMappings =
                ConstructDatabaseObjectToFunctionMappings(newStorageEntityModel);

            // find all mappings from database identity to S-side Functions for the existing artifact
            var existingArtifactSSideMappings =
                ConstructDatabaseObjectToFunctionMappings(preExistingStorageEntityModel);

            // find all Functions whose name has changed (but whose identity has stayed the same)
            // and update the name part of any ItemBinding which references the old name
            UpdateOldDatabaseObjectReferencesToNewNames(existingArtifactSSideMappings, newArtifactSSideMappings);
        }

        private static void RetargetMappingsForRenamedStorageEntitySets(
            StorageEntityContainer preExistingStorageEntityContainer,
            StorageEntityContainer newStorageEntityEntityContainer)
        {
            if (null == preExistingStorageEntityContainer
                || null == newStorageEntityEntityContainer)
            {
                return;
            }

            // find all mappings from database identity to S-side EntitySet for the new artifact
            var newArtifactSSideMappings =
                ConstructDatabaseObjectToEntitySetMappings(newStorageEntityEntityContainer);

            // find all mappings from database identity to S-side EntitySet for the existing artifact
            var existingArtifactSSideMappings =
                ConstructDatabaseObjectToEntitySetMappings(preExistingStorageEntityContainer);

            // find all EntitySets whose name has changed (but whose identity has stayed the same)
            // and update the name part of any ItemBinding which references the old name
            UpdateOldDatabaseObjectReferencesToNewNames(existingArtifactSSideMappings, newArtifactSSideMappings);
        }

        private static void ReplaceSsdl(CommandProcessorContext cpc, EntityDesignArtifact existingArtifact, XElement newSsdl)
        {
            // find the XObject representing the existing StorageModel Schema element
            var existingStorageModelNode = existingArtifact.StorageModel.XObject as XElement;
            Debug.Assert(existingStorageModelNode != null, "existingStorageModelNode is null");

            // find the parent of the existing StorageModel Schema element
            var existingStorageModelParentNode = existingStorageModelNode.Parent;

            // delete the old StorageModel but do not delete its anti-dependencies
            if (null != existingArtifact.StorageModel)
            {
                var deleteStorageModelCommand = new DeleteEFElementCommand(existingArtifact.StorageModel, true, false);
                DeleteEFElementCommand.DeleteInTransaction(cpc, deleteStorageModelCommand);
            }

            // this will clone the source element
            var ssdlSchemaElement = new XElement(newSsdl);
            // add ssdlSchemaElement to the parent of the previously existing Storage node
            existingStorageModelParentNode.Add(ssdlSchemaElement);

            // create a new StorageModel object, add it back into the artifact and re-parse
            existingArtifact.StorageModel = new StorageEntityModel(existingArtifact, ssdlSchemaElement);
            existingArtifact.StorageModel.Parse(new HashSet<XName>());
            Debug.Assert(
                EFElementState.Parsed == existingArtifact.StorageModel.State,
                "StorageModel State should be Parsed, instead it is " + existingArtifact.StorageModel.State);

            // normalize and resolve the StorageModel
            XmlModelHelper.NormalizeAndResolve(existingArtifact.StorageModel);
            Debug.Assert(
                EFElementState.Resolved == existingArtifact.StorageModel.State,
                "StorageModel State should be Resolved, instead it is " + existingArtifact.StorageModel.State);
        }

        private static void UpdateMappingModel(EFArtifact existingArtifact, string oldStorageEntityContainerName)
        {
            // replaces the possible reference to the old S-side EntityContainer name
            ReplaceMappingContainerRef(existingArtifact, oldStorageEntityContainerName);

            // normalize and resolve the changes we just made above
            XmlModelHelper.NormalizeAndResolve(existingArtifact.MappingModel());
        }

        private static void RecursivelyReplaceStorageNamespaceRefs(
            EFContainer target, string oldStorageNamespace, string newStorageNamespace)
        {
            foreach (var child in target.Children)
            {
                var childAsEFContainer = child as EFContainer;
                var childAsItemBinding = child as ItemBinding;
                if (null != childAsEFContainer)
                {
                    RecursivelyReplaceStorageNamespaceRefs(childAsEFContainer, oldStorageNamespace, newStorageNamespace);
                }
                else if (null != childAsItemBinding)
                {
                    // if childAsItemBinding startsWith oldStorageNamespace then will update
                    // (Note: need to check this because SingleItemBinding<Parameter> will have
                    // a NormalizedName which could have this namespace but the reference is solely
                    // to the parameterName and so no replacement needs to be made)
                    var refName = childAsItemBinding.RefName;
                    if (null != refName
                        && refName.StartsWith(oldStorageNamespace, StringComparison.CurrentCulture))
                    {
                        childAsItemBinding.UpdateRefNameNamespaces(oldStorageNamespace, newStorageNamespace);
                    }
                }
            }
        }

        private static void ReplaceMappingContainerRef(EFArtifact existingArtifact, string oldStorageEntityContainerName)
        {
            Debug.Assert(existingArtifact != null, "ReplaceMappingContainerRef(): received null existingArtifact");

            if (!string.IsNullOrEmpty(oldStorageEntityContainerName)
                && existingArtifact.StorageModel() != null
                && existingArtifact.StorageModel().FirstEntityContainer != null
                && existingArtifact.MappingModel() != null
                && existingArtifact.MappingModel().FirstEntityContainerMapping != null
                && existingArtifact.MappingModel().FirstEntityContainerMapping.StorageEntityContainer != null
                && existingArtifact.MappingModel().FirstEntityContainerMapping.StorageEntityContainer.RefName == oldStorageEntityContainerName)
            {
                existingArtifact.MappingModel().FirstEntityContainerMapping.StorageEntityContainer.
                    SetRefName(existingArtifact.StorageModel().FirstEntityContainer);
            }
        }

        /// <summary>
        ///     Compares the two Dictionaries passed in and constructs a Dictionary mapping
        ///     any old DatabaseObjects which exist in both artifacts but whose name has changed
        ///     in the new artifact. It then iterates over that Dictionary finding
        ///     anti-dependencies of each S-side object and updating those references to target
        ///     the updated name of the DatabaseObject in the new artifact.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="existingArtifactSSideMappings">Dictionary mapping S-side database objects to their names in the existing artifact</param>
        /// <param name="newArtifactSSideMappings">Dictionary mapping S-side database objects to their names in the new artifact</param>
        private static void UpdateOldDatabaseObjectReferencesToNewNames<T>(
            Dictionary<DatabaseObject, T> existingArtifactSSideMappings,
            Dictionary<DatabaseObject, T> newArtifactSSideMappings) where T : EFNameableItem
        {
            var existingDatabaseObjectToNewNameMappings = new Dictionary<T, string>();
            foreach (var databaseIdNamePair in existingArtifactSSideMappings)
            {
                var dbIdentity = databaseIdNamePair.Key;
                if (null != databaseIdNamePair.Value
                    && null != databaseIdNamePair.Value.LocalName
                    && !string.IsNullOrEmpty(databaseIdNamePair.Value.LocalName.Value))
                {
                    var oldName = databaseIdNamePair.Value.LocalName.Value;
                    T matchingNewDatabaseObject;
                    var newArtifactContainsDBIdentity =
                        newArtifactSSideMappings.TryGetValue(dbIdentity, out matchingNewDatabaseObject);
                    if (newArtifactContainsDBIdentity
                        && null != matchingNewDatabaseObject
                        && null != matchingNewDatabaseObject.LocalName
                        && !string.IsNullOrEmpty(matchingNewDatabaseObject.LocalName.Value)
                        && matchingNewDatabaseObject.LocalName.Value != oldName)
                        // Note: byte-by-byte (Ordinal) compare is correct here - if name has changed ordinally want to change references too
                    {
                        existingDatabaseObjectToNewNameMappings.Add(databaseIdNamePair.Value, matchingNewDatabaseObject.LocalName.Value);
                    }
                }
            }

            // Now update the name part of any ItemBinding which references the old name
            foreach (var existingDatabaseObjectNewNamePair in existingDatabaseObjectToNewNameMappings)
            {
                var existingDatabaseObject = existingDatabaseObjectNewNamePair.Key;
                var newName = existingDatabaseObjectNewNamePair.Value;

                // Note: have to convert to array first to prevent exceptions due to
                // editing the collection while iterating over it
                var antiDepsArray = existingDatabaseObject.GetAntiDependencies().ToArray();
                foreach (var antiDep in antiDepsArray)
                {
                    var antiDepAsItemBinding = antiDep as ItemBinding;
                    if (null != antiDepAsItemBinding
                        && null != existingDatabaseObject.LocalName
                        && !string.IsNullOrEmpty(existingDatabaseObject.LocalName.Value))
                    {
                        antiDepAsItemBinding.UpdateRefNameNamePart(existingDatabaseObject.LocalName.Value, newName);
                    }
                }
            }
        }

        private static Dictionary<DatabaseObject, StorageEntitySet> ConstructDatabaseObjectToEntitySetMappings(StorageEntityContainer sec)
        {
            Debug.Assert(null != sec, "was passed a null StorageEntityContainer");
            var mappings = new Dictionary<DatabaseObject, StorageEntitySet>();
            if (null != sec)
            {
                foreach (var es in sec.EntitySets())
                {
                    var ses = es as StorageEntitySet;
                    if (null != ses)
                    {
                        var dbObj = DatabaseObject.CreateFromEntitySet(ses);
                        mappings.Add(dbObj, ses);
                    }
                }
            }

            return mappings;
        }

        private static Dictionary<DatabaseObject, Function> ConstructDatabaseObjectToFunctionMappings(StorageEntityModel sem)
        {
            Debug.Assert(null != sem, "was passed a null StorageEntityModel");
            var mappings = new Dictionary<DatabaseObject, Function>();
            if (null != sem)
            {
                foreach (var func in sem.Functions())
                {
                    var dbObj = DatabaseObject.CreateFromFunction(func);
                    mappings.Add(dbObj, func);
                }
            }

            return mappings;
        }
    }
}
