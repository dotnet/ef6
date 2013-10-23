// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.VisualStudio.Modeling.Diagrams;
    using Diagram = Microsoft.Data.Entity.Design.Model.Designer.Diagram;
    using EntityType = Microsoft.Data.Entity.Design.Model.Entity.EntityType;
    using EntityTypeShape = Microsoft.Data.Entity.Design.EntityDesigner.View.EntityTypeShape;

    internal class EntityTypeShapeChange : EntityTypeShapeModelChange
    {
        private readonly Guid _domainPropertyId;

        internal EntityTypeShapeChange(EntityTypeShape entityTypeShape, Guid domainPropertyId)
            : base(entityTypeShape)
        {
            _domainPropertyId = domainPropertyId;
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            StaticInvoke(cpc, EntityTypeShape, _domainPropertyId);
        }

        internal static void StaticInvoke(CommandProcessorContext cpc, EntityTypeShape entityTypeShape, Guid domainPropertyId)
        {
            var viewModel = entityTypeShape.GetRootViewModel();
            Debug.Assert(viewModel != null, "Unable to find root view model from entity type shape:" + entityTypeShape.AccessibleName);

            if (viewModel != null)
            {
                var modelEntityShape = viewModel.ModelXRef.GetExisting(entityTypeShape) as Model.Designer.EntityTypeShape;

                // If ModelXRef does not contain about model EntityTypeShape,try to get the information through DSL Model Element
                if (modelEntityShape == null)
                {
                    var modelDiagram = viewModel.ModelXRef.GetExisting(viewModel.GetDiagram()) as Diagram;
                    var entityType = viewModel.ModelXRef.GetExisting(entityTypeShape.ModelElement) as EntityType;
                    Debug.Assert(modelDiagram != null, "Why Escher Diagram is null?");
                    Debug.Assert(entityType != null, "Why there is no XRef between Escher EntityType and DSL EntityT?");
                    if (modelDiagram != null
                        && entityType != null)
                    {
                        modelEntityShape =
                            entityType.GetAntiDependenciesOfType<Model.Designer.EntityTypeShape>()
                            .FirstOrDefault(ets => ets.Diagram.Id == modelDiagram.Id.Value);
                    }

                    if (modelEntityShape != null)
                    {
                        viewModel.ModelXRef.Add(modelEntityShape, entityTypeShape, cpc.EditingContext);
                    }
                }

                // if modelentityshape is still null, create one
                if (modelEntityShape == null)
                {
                    EntityTypeShapeAdd.StaticInvoke(cpc, entityTypeShape);
                    modelEntityShape = viewModel.ModelXRef.GetExisting(entityTypeShape) as Model.Designer.EntityTypeShape;
                }
                Debug.Assert(modelEntityShape != null);
                if (modelEntityShape != null)
                {
                    if (domainPropertyId == NodeShape.AbsoluteBoundsDomainPropertyId)
                    {
                        var cp = new CommandProcessor(cpc);
                        cp.EnqueueCommand(
                            new UpdateDefaultableValueCommand<double>(modelEntityShape.PointX, entityTypeShape.AbsoluteBounds.X));
                        cp.EnqueueCommand(
                            new UpdateDefaultableValueCommand<double>(modelEntityShape.PointY, entityTypeShape.AbsoluteBounds.Y));
                        cp.EnqueueCommand(
                            new UpdateDefaultableValueCommand<double>(modelEntityShape.Width, entityTypeShape.AbsoluteBounds.Width));
                        cp.Invoke();
                    }
                    else if (domainPropertyId == NodeShape.IsExpandedDomainPropertyId)
                    {
                        var cmd = new UpdateDefaultableValueCommand<bool>(modelEntityShape.IsExpanded, entityTypeShape.IsExpanded);
                        CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    }
                }
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 240; }
        }
    }
}
