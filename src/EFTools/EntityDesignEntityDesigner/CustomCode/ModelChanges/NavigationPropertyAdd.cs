// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using NavigationProperty = Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.NavigationProperty;

    internal class NavigationPropertyAdd : ViewModelChange
    {
        private readonly NavigationProperty _property;

        internal NavigationPropertyAdd(NavigationProperty property)
        {
            _property = property;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal override void Invoke(CommandProcessorContext cpc)
        {
            var viewModel = _property.GetRootViewModel();
            Debug.Assert(viewModel != null, "Unable to find root view model from property: " + _property.Name);

            if (viewModel != null)
            {
                var entityType = viewModel.ModelXRef.GetExisting(_property.EntityType) as Model.Entity.EntityType;
                var cet = entityType as ConceptualEntityType;
                Debug.Assert(entityType != null ? cet != null : true, "EntityType is not ConceptualEntityType");
                Debug.Assert(entityType != null);

                if (cet != null)
                {
                    var property = CreateNavigationPropertyCommand.CreateDefaultProperty(cpc, _property.Name, cet);
                    viewModel.ModelXRef.Add(property, _property, viewModel.EditingContext);
                }
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 110; }
        }
    }
}
