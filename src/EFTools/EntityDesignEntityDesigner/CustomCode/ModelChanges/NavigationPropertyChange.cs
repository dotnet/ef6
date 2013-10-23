// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Commands;

    internal class NavigationPropertyChange : ViewModelChange
    {
        private readonly NavigationProperty _property;

        internal NavigationPropertyChange(NavigationProperty property)
        {
            _property = property;
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            var viewModel = _property.GetRootViewModel();
            Debug.Assert(viewModel != null, "Unable to find root view model from navigation property: " + _property.Name);

            if (viewModel != null)
            {
                var property = viewModel.ModelXRef.GetExisting(_property) as Model.Entity.NavigationProperty;
                Debug.Assert(property != null);

                Command c = new EntityDesignRenameCommand(property, _property.Name, true);
                var cp = new CommandProcessor(cpc, c);
                cp.Invoke();
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 220; }
        }
    }
}
