// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class ReplaceSsdlAndMslCommand : Command
    {
        private readonly XmlReader _newSsdlReader;
        private readonly XmlReader _newMslReader;

        private delegate void CreateModelRootCallback(XElement newModelRootNode);

        internal ReplaceSsdlAndMslCommand(XmlReader newSsdlReader, XmlReader newMslReader)
        {
            Debug.Assert(newSsdlReader != null, "Received null SSDL XmlReader");
            Debug.Assert(newMslReader != null, "Received null MSL XmlReader");
            _newSsdlReader = newSsdlReader;
            _newMslReader = newMslReader;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // check that we have an existing artifact
            var service = cpc.EditingContext.GetEFArtifactService();
            var existingArtifact = service.Artifact as EntityDesignArtifact;
            if (null == existingArtifact)
            {
                Debug.Fail("Null Artifact in ReplaceSsdlCommand.InvokeInternal()");
                return;
            }

            // replace the old SSDL with the new
            ReplaceSsdl(cpc, existingArtifact, _newSsdlReader);

            // replace the old MSL with the new
            ReplaceMsl(cpc, existingArtifact, _newMslReader);

            // normalize and resolve the StorageModel
            XmlModelHelper.NormalizeAndResolve(existingArtifact.StorageModel);
            Debug.Assert(EFElementState.Resolved == existingArtifact.StorageModel.State, "StorageModel State should be Resolved");

            // normalize and resolve the MappingModel
            XmlModelHelper.NormalizeAndResolve(existingArtifact.MappingModel);
            Debug.Assert(EFElementState.Resolved == existingArtifact.MappingModel.State, "MappingModel State should be Resolved");
        }

        private static void ReplaceSsdl(CommandProcessorContext cpc, EntityDesignArtifact existingArtifact, XmlReader newSsdl)
        {
            if (null == existingArtifact)
            {
                Debug.Fail("Existing artifact is null");
                return;
            }

            ReplaceModelRoot(
                cpc, existingArtifact.StorageModel, newSsdl,
                newSsdlElement =>
                    {
                        // create a new StorageModel object, add it back into the artifact and re-parse
                        existingArtifact.StorageModel = new StorageEntityModel(existingArtifact, newSsdlElement);
                        existingArtifact.StorageModel.Parse(new HashSet<XName>());
                        Debug.Assert(EFElementState.Parsed == existingArtifact.StorageModel.State, "StorageModel State should be Parsed");
                    });
        }

        private static void ReplaceMsl(CommandProcessorContext cpc, EntityDesignArtifact existingArtifact, XmlReader newMsl)
        {
            Debug.Assert(null != existingArtifact, "Existing artifact is null");
            if (null == existingArtifact)
            {
                return;
            }

            ReplaceModelRoot(
                cpc, existingArtifact.MappingModel, newMsl,
                newMslElement =>
                    {
                        // create a new MappingModel object, add it back into the artifact and re-parse
                        existingArtifact.MappingModel = new MappingModel(existingArtifact, newMslElement);
                        existingArtifact.MappingModel.Parse(new HashSet<XName>());
                        Debug.Assert(EFElementState.Parsed == existingArtifact.MappingModel.State, "MappingModel State should be Parsed");
                    });
        }

        private static void ReplaceModelRoot(
            CommandProcessorContext cpc, EFRuntimeModelRoot existingModelRoot, XmlReader newSchemaReader,
            CreateModelRootCallback createModelRootCallback)
        {
            var newModelRootNode = XElement.Load(newSchemaReader);

            // find the XObject representing the existing EFRuntimeModelRoot EFObject
            var existingModelRootNode = existingModelRoot.XObject as XElement;
            Debug.Assert(existingModelRootNode != null, "existingRootXElement is null in ReplaceModelRoot()");

            // find the parent of the existing XObject tied to the EFRuntimeModelRoot
            var existingModelRootParentNode = existingModelRootNode.Parent;

            // delete the old EFRuntimeModelRoot EFObject but do not delete its anti-dependencies
            if (null != existingModelRoot)
            {
                var deleteStorageModelCommand = new DeleteEFElementCommand(existingModelRoot, true, false);
                DeleteEFElementCommand.DeleteInTransaction(cpc, deleteStorageModelCommand);
            }

            existingModelRootParentNode.Add(newModelRootNode);

            // Callback to create the EFRuntimeModelRoot EFObject
            createModelRootCallback(newModelRootNode);
        }
    }
}
