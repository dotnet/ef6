// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;

    /// <summary>
    ///     Create a copy of an EntityType
    ///     If the entities are copied to a diagram, the behavior as follow:
    ///     --------------------------------------------------------------------------------------------------------------------------------------------------------------------
    ///     | Object in Model   | Object in Diagram     |           Expected behavior                                                                                          |
    ///     --------------------------------------------------------------------------------------------------------------------------------------------------------------------
    ///     | Yes               |      Yes              |   A new object copy is added to the Entity Model and the corresponding diagram item is created in the diagram model  |
    ///     | Yes               |      No               |   Only diagram item is added into diagram                                                                            |
    ///     | No                |      No               |   A new object copy is added to the Entity Model and the corresponding diagram item is created in the diagram model  |
    ///     --------------------------------------------------------------------------------------------------------------------------------------------------------------------
    /// </summary>
    internal class CopyEntityCommand : CopyAnnotatableElementCommand
    {
        private readonly Diagram _diagram;
        private readonly EntityTypeClipboardFormat _clipboardEntity;
        private readonly ModelSpace _modelSpace;
        private EntityType _createdEntity;

        /// <summary>
        ///     Creates a copy of EntityType from clipboard format
        /// </summary>
        /// <param name="clipboardEntity"></param>
        /// <param name="modelSpace"></param>
        /// <returns></returns>
        internal CopyEntityCommand(EntityTypeClipboardFormat clipboardEntity, ModelSpace modelSpace)
            : this(null, clipboardEntity, modelSpace)
        {
        }

        /// <summary>
        ///     The behavior is as follow:
        ///     Creates a copy of an EntityType from clipboard format in Entity Model if:
        ///     - The entity-type does not exist in the model.
        ///     - The passed in diagram parameter is null.
        ///     OR
        ///     Create the shape for the entity-type in the Diagram Model if:
        ///     - The diagram is not null AND the entity-type exists in the model AND the corresponding entity-type-shape does not exist in the diagram.
        /// </summary>
        /// <param name="diagram"></param>
        /// <param name="clipboardEntity"></param>
        /// <param name="modelSpace"></param>
        internal CopyEntityCommand(Diagram diagram, EntityTypeClipboardFormat clipboardEntity, ModelSpace modelSpace)
        {
            _clipboardEntity = clipboardEntity;
            _modelSpace = modelSpace;
            _diagram = diagram;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            if (_diagram != null)
            {
                // Add diagram id information in the transaction context.
                // This is to ensure the diagram objects are created correctly.
                if (cpc.EfiTransaction.GetContextValue<DiagramContextItem>(EfiTransactionOriginator.TransactionOriginatorDiagramId) == null)
                {
                    cpc.EfiTransaction.AddContextValue(
                        EfiTransactionOriginator.TransactionOriginatorDiagramId, new DiagramContextItem(_diagram.Id.Value));
                }
            }

            var service = cpc.EditingContext.GetEFArtifactService();
            var artifact = service.Artifact;

            // check if entity is in the model
            _createdEntity = artifact.ArtifactSet.LookupSymbol(_clipboardEntity.NormalizedName) as EntityType;
            if (_diagram != null
                && _createdEntity != null
                && _createdEntity is ConceptualEntityType)
            {
                if (_createdEntity.GetAntiDependenciesOfType<EntityTypeShape>().Count(ets => ets.Diagram.Id == _diagram.Id.Value) == 0)
                {
                    // CreateEntityTypeShapeAndConnectorsInDiagram method will check if the shape for the entity-type has been created; 
                    // and it will not create one if the shape already exists in the diagram.
                    // Also, VerifyDiagramModelIntegrityVisitor will assert if there are duplicate diagram shapes (shapes that point to the same model element)
                    // every-time a command transaction is committed. So adding another check to do the same thing here is redundant.
                    CreateEntityTypeShapeCommand.CreateEntityTypeShapeAndConnectorsInDiagram(
                        cpc, _diagram, _createdEntity as ConceptualEntityType, _clipboardEntity.EntityTypeShapeFillColor, false);
                    return;
                }
            }
            CreateEntityCopyInModel(cpc);
        }

        private void CreateEntityCopyInModel(CommandProcessorContext cpc)
        {
            // get unique names for Entity and EntitySet
            var entityName = _clipboardEntity.EntityName;
            var setName = _clipboardEntity.EntitySetName;

            _createdEntity = CreateEntityTypeCommand.CreateEntityTypeAndEntitySetAndProperty(
                cpc, entityName, setName, false, null, null, null, _modelSpace, true);

            foreach (var clipboardProperty in _clipboardEntity.Properties)
            {
                var cmd = new CopyPropertyCommand(clipboardProperty, _createdEntity);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }

            // Create the entity-type-shape since we need to set the fill color.
            if (_diagram != null)
            {
                CommandProcessor.InvokeSingleCommand(
                    cpc, new CreateEntityTypeShapeCommand(_diagram, _createdEntity, _clipboardEntity.EntityTypeShapeFillColor));
            }

            AddAnnotations(_clipboardEntity, _createdEntity);
        }

        internal EntityType EntityType
        {
            get { return _createdEntity; }
        }
    }
}
