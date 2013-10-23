// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Commands;

    internal class PropertyDelete : ViewModelChange
    {
        internal Property Property { get; private set; }
        internal EntityType EntityType { get; private set; }

        internal PropertyDelete(Property property)
        {
            Property = property;
            EntityType = property.EntityType;
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            var viewModel = Property.GetRootViewModel();
            Debug.Assert(viewModel != null, "Unable to find root view model from property:" + Property.Name);

            if (viewModel != null)
            {
                var property = viewModel.ModelXRef.GetExisting(Property) as Model.Entity.Property;
                Debug.Assert(property != null);
                DeleteEFElementCommand.DeleteInTransaction(cpc, property);
                viewModel.ModelXRef.Remove(property, Property);
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 30; }
        }
    }
}
