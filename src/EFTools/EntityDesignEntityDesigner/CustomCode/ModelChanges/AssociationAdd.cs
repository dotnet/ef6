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
    using Association = Microsoft.Data.Entity.Design.EntityDesigner.ViewModel.Association;
    using EntityType = Microsoft.Data.Entity.Design.Model.Entity.EntityType;

    internal class AssociationAdd : ViewModelChange
    {
        private readonly Association _association;

        internal AssociationAdd(Association association)
        {
            _association = association;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal override void Invoke(CommandProcessorContext cpc)
        {
            var viewModel = _association.GetRootViewModel();
            Debug.Assert(viewModel != null, "Unable to find root view model from association: " + _association.Name);

            if (viewModel != null)
            {
                var s = viewModel.ModelXRef.GetExisting(_association.SourceEntityType) as EntityType;
                var t = viewModel.ModelXRef.GetExisting(_association.TargetEntityType) as EntityType;

                var source = s as ConceptualEntityType;
                var target = t as ConceptualEntityType;

                Debug.Assert(s != null ? source != null : true, "EntityType is not ConceptualEntityType");
                Debug.Assert(t != null ? target != null : true, "EntityType is not ConceptualEntityType");

                Debug.Assert(source != null && target != null);
                var modelAssociation = CreateConceptualAssociationCommand.CreateAssociationAndAssociationSetWithDefaultNames(
                    cpc, source, target);
                viewModel.ModelXRef.Add(modelAssociation, _association, viewModel.EditingContext);
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 130; }
        }
    }
}
