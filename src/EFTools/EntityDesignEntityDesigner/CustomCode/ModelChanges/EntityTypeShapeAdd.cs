// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using EntityType = Microsoft.Data.Entity.Design.Model.Entity.EntityType;
    using EntityTypeShape = Microsoft.Data.Entity.Design.EntityDesigner.View.EntityTypeShape;

    internal class EntityTypeShapeAdd : EntityTypeShapeModelChange
    {
        internal EntityTypeShapeAdd(EntityTypeShape entityShape)
            : base(entityShape)
        {
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            StaticInvoke(cpc, EntityTypeShape);
        }

        internal static void StaticInvoke(CommandProcessorContext cpc, EntityTypeShape entityTypeShape)
        {
            var viewModel = entityTypeShape.GetRootViewModel();

            Debug.Assert(viewModel != null, "Unable to find root view model from entity-type-shape:" + entityTypeShape.AccessibleName);
            if (viewModel != null)
            {
                var entityType = viewModel.ModelXRef.GetExisting(entityTypeShape.ModelElement) as EntityType;
                var diagram = viewModel.ModelXRef.GetExisting(entityTypeShape.Diagram) as Diagram;
                Debug.Assert(entityType != null && diagram != null);
                if (entityType != null
                    && diagram != null)
                {
                    var cmd = new CreateEntityTypeShapeCommand(diagram, entityType);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    var modelEntityShape = cmd.EntityTypeShape;
                    Debug.Assert(modelEntityShape != null);
                    viewModel.ModelXRef.Add(modelEntityShape, entityTypeShape, viewModel.EditingContext);
                }
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 150; }
        }
    }
}
