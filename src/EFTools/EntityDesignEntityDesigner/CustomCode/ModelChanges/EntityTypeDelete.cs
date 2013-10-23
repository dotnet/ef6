// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Commands;

    internal class EntityTypeDelete : ViewModelChange
    {
        internal EntityType EntityType { get; private set; }

        internal EntityTypeDelete(EntityType entityType)
        {
            EntityType = entityType;
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            var viewModel = EntityType.GetRootViewModel();
            Debug.Assert(viewModel != null, "Unable to find root view model from entity-type:" + EntityType.Name);
            if (viewModel != null)
            {
                var entityType = viewModel.ModelXRef.GetExisting(EntityType) as Model.Entity.EntityType;
                // Escher EntityType can be null. 
                // Scenario: When a user adds an entity-type using toolbox, the code in EntityType_AddRule class will delete the DSL EntityType before Model's EntityType is created.
                // In that scenario, the xref between DSL's Entity-Type and Model's Entity-Type has not been established yet.
                if (entityType != null)
                {
                    DeleteEFElementCommand.DeleteInTransaction(cpc, entityType);
                    viewModel.ModelXRef.Remove(entityType, EntityType);
                }
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 40; }
        }
    }
}
