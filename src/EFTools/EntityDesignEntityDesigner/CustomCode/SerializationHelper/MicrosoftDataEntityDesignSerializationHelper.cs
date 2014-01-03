// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using DesignerModel = Microsoft.Data.Entity.Design.Model.Designer;

namespace Microsoft.Data.Entity.Design.EntityDesigner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using Microsoft.Data.Entity.Design.EntityDesigner.CustomSerializer;
    using Microsoft.Data.Entity.Design.EntityDesigner.Utils;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Diagrams;
    using Microsoft.VisualStudio.Modeling.Validation;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.XmlEditor;

    public sealed partial class MicrosoftDataEntityDesignSerializationHelper
    {
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "diagram")]
        internal override EntityDesignerViewModel LoadModelAndDiagram(
            SerializationResult serializationResult, Partition modelPartition, string modelFileName, Partition diagramPartition,
            string diagramFileName, ISchemaResolver schemaResolver, ValidationController validationController,
            ISerializerLocator serializerLocator)
        {
            EntityDesignerViewModel evm = null;
            using (new VsUtils.HourglassHelper())
            {
                evm = LoadModel(serializationResult, modelPartition, modelFileName, schemaResolver, validationController, serializerLocator);
                var diagram = CreateDiagramHelper(diagramPartition, evm);
            }
            return evm;
        }

        /// <summary>
        ///     Helper method to create and initialize a new EntityDesignerDiagram.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal override EntityDesignerDiagram CreateDiagramHelper(Partition diagramPartition, ModelElement modelRoot)
        {
            var evm = modelRoot as EntityDesignerViewModel;
            var diagram = new EntityDesignerDiagram(diagramPartition);
            diagram.ModelElement = evm;
            return diagram;
        }

        internal override EntityDesignerViewModel LoadModel(
            SerializationResult serializationResult, Partition partition, string fileName, ISchemaResolver schemaResolver,
            ValidationController validationController, ISerializerLocator serializerLocator)
        {
            var docData = VSHelpers.GetDocData(PackageManager.Package, fileName) as IEntityDesignDocData;
            docData.CreateAndLoadBuffer();

            EntityDesignerViewModel evm = null;

            var serializationContext = new SerializationContext(GetDirectory(partition.Store), fileName, serializationResult);
            var transactionContext = new TransactionContext();
            transactionContext.Add(SerializationContext.TransactionContextKey, serializationContext);

            using (var t = partition.Store.TransactionManager.BeginTransaction("Load Model from " + fileName, true, transactionContext))
            {
                var uri = Tools.XmlDesignerBase.Base.Util.Utils.FileName2Uri(fileName);
                var context = PackageManager.Package.DocumentFrameMgr.EditingContextManager.GetNewOrExistingContext(uri);
                evm =
                    ModelTranslatorContextItem.GetEntityModelTranslator(context).TranslateModelToDslModel(null, partition) as
                    EntityDesignerViewModel;

                if (evm == null)
                {
                    serializationResult.Failed = true;
                }
                else
                {
                    if (t.IsActive)
                    {
                        t.Commit();
                    }
                }
            }

            // Validate imported model
            if (!serializationResult.Failed
                && (validationController != null))
            {
                validationController.Validate(partition, ValidationCategories.Load);
            }
            return evm;
        }

        internal override MemoryStream InternalSaveModel(
            SerializationResult serializationResult, EntityDesignerViewModel modelRoot, string fileName, Encoding encoding,
            bool writeOptionalPropertiesWithDefaultValue)
        {
            IEntityDesignDocData docData = null;
            MemoryStream stream = null;

            Debug.Assert(modelRoot.EditingContext != null, "Designer model root has a null EditingContext");
            if (modelRoot.EditingContext != null)
            {
                // find our doc data; don't use the passed in fileName as this will be the new name
                // during a SaveAs operation
                var artifact = EditingContextManager.GetArtifact(modelRoot.EditingContext);
                if (artifact != null)
                {
                    docData = VSHelpers.GetDocData(PackageManager.Package, artifact.Uri.LocalPath) as IEntityDesignDocData;
                }

                Debug.Assert(docData != null, "Couldn't locate our DocData");
                if (docData != null)
                {
                    var text = docData.GetBufferTextForSaving();
                    if (!string.IsNullOrEmpty(text))
                    {
                        stream = FileUtils.StringToStream(text, encoding) as MemoryStream;
                    }
                }
            }

            // if we don't have a stream, then we couldn't serialize for some reason
            if (stream == null)
            {
                serializationResult.Failed = true;
            }

            return stream;
        }

        internal override void SaveDiagram(
            SerializationResult serializationResult, EntityDesignerDiagram diagram, string diagramFileName, Encoding encoding,
            bool writeOptionalPropertiesWithDefaultValue)
        {
            // don't save the .diagram file
            return;
        }

        internal override void SaveModelAndDiagram(
            SerializationResult serializationResult, EntityDesignerViewModel modelRoot, string modelFileName, EntityDesignerDiagram diagram,
            string diagramFileName, Encoding encoding, bool writeOptionalPropertiesWithDefaultValue)
        {
            // only save the model
            base.SaveModel(serializationResult, modelRoot, modelFileName, encoding, writeOptionalPropertiesWithDefaultValue);

            if (!serializationResult.Failed)
            {
                // flip our dirty bit (as long as we aren't trying to save the auto-recovery backup file)
                var artifact = EditingContextManager.GetArtifact(modelRoot.EditingContext);
                Debug.Assert(artifact != null, "Failed to get a valid EFArtifact from the context");

                IEntityDesignDocData docData = null;
                var fileName = String.Empty;
                if (artifact != null)
                {
                    fileName = artifact.Uri.LocalPath;
                }

                docData = VSHelpers.GetDocData(PackageManager.Package, fileName) as IEntityDesignDocData;
                Debug.Assert(docData != null, "Couldn't locate our DocData");
                if (artifact != null
                    && docData != null
                    && !string.Equals(docData.BackupFileName, modelFileName, StringComparison.OrdinalIgnoreCase))
                {
                    artifact.IsDirty = false;
                }

                // SaveDiagram file if the file exists
                // TODO: What happened if saving diagram file failed? Should we rollback the model file?
                var diagramDocData =
                    VSHelpers.GetDocData(PackageManager.Package, fileName + EntityDesignArtifact.ExtensionDiagram) as XmlModelDocData;
                if (diagramDocData != null)
                {
                    int saveIsCancelled;
                    diagramDocData.SaveDocData(VSSAVEFLAGS.VSSAVE_SilentSave, out diagramFileName, out saveIsCancelled);
                }
            }
        }

        /// <summary>
        ///     This method removes all of the child elements of the EntityDesignerViewModel.
        /// </summary>
        internal static void ClearModel(EntityDesignerViewModel viewModel)
        {
            // The assumption is that if ModelXRef is null or we cannot get the diagram, DSL Model is empty
            if (viewModel.ModelXRef == null
                || viewModel.GetDiagram() == null)
            {
                return;
            }

            using (var t = viewModel.Store.TransactionManager.BeginTransaction("ClearModel", true))
            {
                // delete inheritance first
                foreach (var melem in viewModel.ModelXRef.ReferencedViewElements)
                {
                    if (melem is Inheritance)
                    {
                        melem.Delete();
                    }
                }

                // delete associations next
                foreach (var melem in viewModel.ModelXRef.ReferencedViewElements)
                {
                    if (melem is Association)
                    {
                        melem.Delete();
                    }
                }

                // delete entities last
                foreach (var melem in viewModel.ModelXRef.ReferencedViewElements)
                {
                    if (melem is EntityType)
                    {
                        melem.Delete();
                    }
                }

                // clear out the XRef
                viewModel.ModelXRef.Clear();

                if (t.IsActive)
                {
                    t.Commit();
                }
            }
        }

        /// <summary>
        ///     This method will remove all child shapes of the EntityDesignerDiagram.
        /// </summary>
        internal static void ClearDiagram(EntityDesignerViewModel viewModel)
        {
            var diagram = viewModel.GetDiagram();
            if (diagram != null)
            {
                using (var t = viewModel.Store.TransactionManager.BeginTransaction("ClearDiagram", true))
                {
                    // make copy of AllElements so that we don't modify the collection while we are iterating over it.
                    // We are only interested to the model elements that belong to the viewModel; so look at the Partition's ElementDirectory
                    // instead of Store's ElementDirectory which is shared across diagram.
                    Debug.Assert(viewModel.Partition != null, "ViewModel's Partition should never be null.");
                    if (viewModel.Partition != null)
                    {
                        var elementDirectory = viewModel.Partition.ElementDirectory;
                        Debug.Assert(
                            elementDirectory != null, "ElementDirectory in partition for view model :" + diagram.Title + " is null.");
                        if (elementDirectory != null)
                        {
                            var allElements = new List<ModelElement>(elementDirectory.AllElements.Count);
                            allElements.AddRange(elementDirectory.AllElements);

                            foreach (var melem in allElements)
                            {
                                // don't delete our diagram, but remove every other presentation element
                                if (melem is PresentationElement
                                    && (melem is EntityDesignerDiagram) == false)
                                {
                                    melem.Delete();
                                }
                            }
                        }
                    }

                    if (t.IsActive)
                    {
                        t.Commit();
                    }
                }
            }
        }

        /// <summary>
        ///     This method loads the DSL view model with the items in the artifact's C-Model.
        /// </summary>
        internal void ReloadModel(EntityDesignerViewModel viewModel)
        {
            var diagram = viewModel.GetDiagram();
            if (diagram == null)
            {
                // empty DSL diagram
                return;
            }

            // get our artifact
            var artifact = EditingContextManager.GetArtifact(viewModel.EditingContext) as EntityDesignArtifact;
            Debug.Assert(artifact != null);

            var serializationResult = new SerializationResult();

            var serializationContext = new SerializationContext(GetDirectory(viewModel.Store), artifact.Uri.LocalPath, serializationResult);
            var transactionContext = new TransactionContext();
            transactionContext.Add(SerializationContext.TransactionContextKey, serializationContext);

            var workaroundFixSerializationTransactionValue = false;
            if (viewModel.Store.PropertyBag.ContainsKey("WorkaroundFixSerializationTransaction"))
            {
                workaroundFixSerializationTransactionValue = (bool)viewModel.Store.PropertyBag["WorkaroundFixSerializationTransaction"];
            }

            try
            {
                // To fix performance issue during reload, we turn-off layout during "serialization".
                viewModel.Store.PropertyBag["WorkaroundFixSerializationTransaction"] = true;

                using (var t = viewModel.Store.TransactionManager.BeginTransaction("ReloadModel", true, transactionContext))
                {
                    if (artifact.ConceptualModel() == null)
                    {
                        return;
                    }

                    DesignerModel.Diagram diagramModel = null;

                    // If DiagramId is not string empty, try to get the diagram from the artifact. 
                    // There is a situation where we could not find the diagram given an ID (for example: EDMX Model's Diagram that is created by VS before SQL 11; 
                    // In that case, we assign temporary ID to the diagram and a new ID will be generated every time the model is reloaded.)
                    // We could safely choose the first diagram since multiple diagrams feature didn't exist in VS prior to SQL11 release.
                    if (!string.IsNullOrEmpty(diagram.DiagramId))
                    {
                        diagramModel = artifact.DesignerInfo.Diagrams.GetDiagram(diagram.DiagramId);
                    }

                    if (diagramModel == null)
                    {
                        diagramModel = artifact.DesignerInfo.Diagrams.FirstDiagram;
                    }

                    if (diagramModel != null)
                    {
                        // Re-establish the xref between Escher conceptual model and DSL root model.
                        // and between Escher Diagram model and DSL diagram model.

                        Debug.Assert(viewModel.ModelXRef != null, "Why ModelXRef is null?");
                        if (viewModel.ModelXRef != null)
                        {
                            viewModel.ModelXRef.Add(artifact.ConceptualModel(), viewModel, viewModel.EditingContext);
                            viewModel.ModelXRef.Add(diagramModel, diagram, viewModel.EditingContext);
                            ModelTranslatorContextItem.GetEntityModelTranslator(viewModel.EditingContext)
                                .TranslateModelToDslModel(diagramModel, viewModel.Partition);
                        }
                    }

                    if (t.IsActive)
                    {
                        t.Commit();
                    }
                }
            }
            finally
            {
                viewModel.Store.PropertyBag["WorkaroundFixSerializationTransaction"] = workaroundFixSerializationTransactionValue;
            }
        }

        /// <summary>
        ///     This method reads the .diagram file and makes sure that shapes exist for every item
        ///     in the .diagram file.
        /// </summary>
        internal static void ReloadDiagram(EntityDesignerViewModel viewModel)
        {
            var diagram = viewModel.GetDiagram();
            if (diagram == null)
            {
                // Empty DSL diagram
                return;
            }

            diagram.ResetWatermark(diagram.ActiveDiagramView);

            // get our artifact
            var artifact = EditingContextManager.GetArtifact(viewModel.EditingContext);
            Debug.Assert(artifact != null);
            if (!artifact.IsDesignerSafe)
            {
                return;
            }

            var diagramModel = viewModel.ModelXRef.GetExisting(diagram) as DesignerModel.Diagram;

            if (diagramModel != null)
            {
                // ensure that we still have all of the parts we need to re-translate from the Model
                EntityModelToDslModelTranslatorStrategy.TranslateDiagram(diagram, diagramModel);
            }
            else
            {
                // this path will usually only happen if an UMFDB extension has deleted the diagram node
                // since we don't have a diagram anymore, lay it all out and create a new one
                if (diagram.ModelElement.EntityTypes.Count < EntityDesignerDiagram.IMPLICIT_AUTO_LAYOUT_CEILING)
                {
                    diagram.AutoLayoutDiagram();
                }
                EntityModelToDslModelTranslatorStrategy.CreateDefaultDiagram(viewModel.EditingContext, diagram);
            }

            // remove "Select All" selection
            if (diagram.ActiveDiagramView != null)
            {
                diagram.ActiveDiagramView.Selection.Set(new DiagramItem(diagram));
            }
        }
    }
}
