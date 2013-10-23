// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.Model.UpdateFromDatabase;

    internal class UpdateModelFromDatabaseCommand : Command
    {
        private readonly EFArtifact _newArtifactFromDB;

        internal UpdateModelFromDatabaseCommand(EFArtifact newArtifactFromDB)
        {
            _newArtifactFromDB = newArtifactFromDB;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var service = cpc.EditingContext.GetEFArtifactService();
            var artifact = service.Artifact;
            Debug.Assert(artifact != null, "Null Artifact");
            if (null == artifact)
            {
                return;
            }

            // construct a mapping of the existing model's C-side objects
            // and their S-side identities before anything is updated
            var existingModel = new ExistingModelSummary(artifact);

            // replace the old SSDL with the new and fixup any references 
            // in the MSL that broke because of the replacement of the SSDL
            // (i.e. the S-side Alias and S-side EntityContainer name)
            var replaceSsdlCommand = new ReplaceSsdlCommand(_newArtifactFromDB.StorageModel());
            CommandProcessor.InvokeSingleCommand(cpc, replaceSsdlCommand);

            // remove any mappings with references which no longer work 
            // with the new SSDL
            var deleteUnboundMappingsCommand = new DeleteUnboundMappingsCommand();
            CommandProcessor.InvokeSingleCommand(cpc, deleteUnboundMappingsCommand);

            // remove any mappings which should no longer be mapped with the new SSDL
            // but actually are because a new S-side object with identical name
            // but different identity has been added
            var deleteChangedIdentityMappingsCommand = new DeleteChangedIdentityMappingsCommand(existingModel);
            CommandProcessor.InvokeSingleCommand(cpc, deleteChangedIdentityMappingsCommand);

            // from the temp model for the updated database determine which 
            // C-side objects need to be added/updated and then update the
            // C- and M- side models appropriately
            var modelFromUpdatedDatabase = new UpdatedModelSummary(_newArtifactFromDB);
            var updateCsdlAndMslCommand =
                new UpdateConceptualAndMappingModelsCommand(existingModel, modelFromUpdatedDatabase);
            CommandProcessor.InvokeSingleCommand(cpc, updateCsdlAndMslCommand);

            // fix up Function Import parameters and add integrity checks
            if (artifact.MappingModel() != null
                && artifact.MappingModel().FirstEntityContainerMapping != null)
            {
                // Function Import parameters are now out-of-date compared to the updated Function ones.
                // We need to update them as otherwise there is no way to do so using Escher.
                foreach (var fim in artifact.MappingModel().FirstEntityContainerMapping.FunctionImportMappings())
                {
                    if (null != fim.FunctionImportName
                        && null != fim.FunctionImportName.Target
                        && null != fim.FunctionName
                        && null != fim.FunctionName.Target)
                    {
                        CreateFunctionImportCommand.UpdateFunctionImportParameters(
                            cpc, fim.FunctionImportName.Target, fim.FunctionName.Target);
                    }
                }

                // Add integrity checks to enforce mapping rules
                foreach (var esm in artifact.MappingModel().FirstEntityContainerMapping.EntitySetMappings())
                {
                    EnforceEntitySetMappingRules.AddRule(cpc, esm);
                }

                // add the integrity check to propagate all appropriate StoreGeneratedPattern values to the S-side
                // Note: should not propagate "None"/defaulted values to prevent those C-side values overwriting
                // correctly updated S-side StoreGeneratedPattern values which were just received from the runtime
                PropagateStoreGeneratedPatternToStorageModel.AddRule(cpc, artifact, false);

                // Add integrity check to enforce synchronizing C-side Property facets to S-side values
                var shouldSynchronizePropertyFacets = ModelHelper.GetDesignerPropertyValueFromArtifactAsBool(
                    OptionsDesignerInfo.ElementName,
                    OptionsDesignerInfo.AttributeSynchronizePropertyFacets, OptionsDesignerInfo.SynchronizePropertyFacetsDefault(artifact),
                    artifact);
                if (shouldSynchronizePropertyFacets)
                {
                    PropagateStoragePropertyFacetsToConceptualModel.AddRule(cpc, artifact);
                }
            }
        }
    }

    [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    internal class UpdateModelFromDatabaseException : Exception
    {
        internal UpdateModelFromDatabaseException(string message)
            : base(message)
        {
        }
    }
}
