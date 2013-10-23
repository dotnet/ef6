// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Commands;

    internal class EntityTypeChange : ViewModelChange
    {
        private readonly EntityType _entityType;

        internal EntityTypeChange(EntityType entityType)
        {
            _entityType = entityType;
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            var viewModel = _entityType.GetRootViewModel();
            Debug.Assert(viewModel != null, "Unable to find root view model from entity type: " + _entityType.Name);

            if (viewModel != null)
            {
                var entityType = viewModel.ModelXRef.GetExisting(_entityType) as Model.Entity.EntityType;
                Debug.Assert(entityType != null);
                Command c = new EntityDesignRenameCommand(entityType, _entityType.Name, true);
                var cp = new CommandProcessor(cpc, c);
                cp.Invoke();
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 200; }
        }
    }
}
