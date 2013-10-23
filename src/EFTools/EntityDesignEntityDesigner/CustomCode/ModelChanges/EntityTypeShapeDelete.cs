// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Commands;

    internal class EntityTypeShapeDelete : EntityTypeShapeModelChange
    {
        internal EntityTypeShapeDelete(EntityTypeShape entityShape)
            : base(entityShape)
        {
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            var viewModel = EntityTypeShape.GetRootViewModel();
            Debug.Assert(viewModel != null, "Unable to find root view model from entity type shape: " + EntityTypeShape.AccessibleName);
            if (viewModel != null)
            {
                var modelEntityShape = viewModel.ModelXRef.GetExisting(EntityTypeShape) as Model.Designer.EntityTypeShape;
                if (modelEntityShape != null)
                {
                    DeleteEFElementCommand.DeleteInTransaction(cpc, modelEntityShape);
                    viewModel.ModelXRef.Remove(modelEntityShape, EntityTypeShape);
                }
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 0; }
        }
    }
}
