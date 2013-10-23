// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    internal class ReplaceCsdlAndMslCommand : Command
    {
        private StringReader _newCsdlStringReader;
        private StringReader _newMslStringReader;
        private XmlReader _newCsdlReader;
        private XmlReader _newMslReader;

        private delegate void CreateModelRootCallback(XElement newModelRootNode);

        internal ReplaceCsdlAndMslCommand(XmlReader newCsdlReader, XmlReader newMslReader)
        {
            Debug.Assert(newCsdlReader != null, "Received null CSDL XmlReader");
            Debug.Assert(newMslReader != null, "Received null MSL XmlReader");
            _newCsdlReader = newCsdlReader;
            _newMslReader = newMslReader;
        }

        internal ReplaceCsdlAndMslCommand(string csdl, string msl)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(csdl), "Received null CSDL");
            Debug.Assert(!String.IsNullOrWhiteSpace(msl), "Received null MSL");
            _newCsdlStringReader = new StringReader(csdl);
            _newCsdlReader = XmlReader.Create(_newCsdlStringReader);
            _newMslStringReader = new StringReader(msl);
            _newMslReader = XmlReader.Create(_newMslStringReader);
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            try
            {
                // check that we have an existing artifact
                var service = cpc.EditingContext.GetEFArtifactService();
                var existingArtifact = service.Artifact as EntityDesignArtifact;
                Debug.Assert(existingArtifact != null, "Null Artifact in ReplaceCsdlAndMslCommand.InvokeInternal()");
                if (null == existingArtifact)
                {
                    return;
                }

                // replace the old CSDL with the new
                ReplaceCsdl(cpc, existingArtifact, _newCsdlReader);

                // replace the old MSL with the new
                ReplaceMsl(cpc, existingArtifact, _newMslReader);

                // normalize and resolve the ConceptualModel
                XmlModelHelper.NormalizeAndResolve(existingArtifact.ConceptualModel);
                Debug.Assert(EFElementState.Resolved == existingArtifact.ConceptualModel.State, "ConceptualModel State should be Resolved");

                // normalize and resolve the MappingModel
                XmlModelHelper.NormalizeAndResolve(existingArtifact.MappingModel);
                Debug.Assert(EFElementState.Resolved == existingArtifact.MappingModel.State, "MappingModel State should be Resolved");
            }
            finally
            {
                Cleanup();
            }
        }

        private void Cleanup()
        {
            if (_newMslReader != null)
            {
                _newMslReader.Close();
                _newMslReader = null;
            }
            if (_newCsdlReader != null)
            {
                _newCsdlReader.Close();
                _newCsdlReader = null;
            }
            if (_newMslStringReader != null)
            {
                _newMslStringReader.Close();
                _newMslStringReader = null;
            }
            if (_newCsdlStringReader != null)
            {
                _newCsdlStringReader.Close();
                _newCsdlStringReader = null;
            }
        }

        private static void ReplaceCsdl(CommandProcessorContext cpc, EntityDesignArtifact existingArtifact, XmlReader newCsdl)
        {
            Debug.Assert(null != existingArtifact, "Existing artifact is null");
            if (null == existingArtifact)
            {
                return;
            }

            ReplaceModelRoot(
                cpc, existingArtifact.ConceptualModel, newCsdl,
                newCsdlElement =>
                    {
                        // create a new ConceptualModel object, add it back into the artifact and re-parse
                        existingArtifact.ConceptualModel = new ConceptualEntityModel(existingArtifact, newCsdlElement);
                        existingArtifact.ConceptualModel.Parse(new HashSet<XName>());
                        Debug.Assert(
                            EFElementState.Parsed == existingArtifact.ConceptualModel.State, "ConceptualModel State should be Parsed");
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
                var deleteConceptualModelCommand = new DeleteEFElementCommand(existingModelRoot, true, false);
                DeleteEFElementCommand.DeleteInTransaction(cpc, deleteConceptualModelCommand);
            }

            existingModelRootParentNode.Add(newModelRootNode);

            // Callback to create the EFRuntimeModelRoot EFObject
            createModelRootCallback(newModelRootNode);
        }
    }
}
