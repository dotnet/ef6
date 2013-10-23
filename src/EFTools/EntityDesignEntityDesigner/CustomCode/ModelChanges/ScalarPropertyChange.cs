// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Property = Microsoft.Data.Entity.Design.Model.Entity.Property;

    internal class ScalarPropertyKeyChange : ViewModelChange
    {
        private readonly ScalarProperty _property;

        internal ScalarPropertyKeyChange(ScalarProperty property)
        {
            _property = property;
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            var viewModel = _property.GetRootViewModel();
            Debug.Assert(viewModel != null, "Unable to find root view model from property: " + _property.Name);

            if (viewModel != null)
            {
                var property = viewModel.ModelXRef.GetExisting(_property) as Property;
                Debug.Assert(property != null);
                var cmd = new SetKeyPropertyCommand(property, _property.EntityKey);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 210; }
        }
    }
}
