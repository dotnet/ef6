// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Commands;

    internal class AssociationDelete : ViewModelChange
    {
        private readonly Association _association;

        internal AssociationDelete(Association association)
        {
            _association = association;
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            var viewModel = _association.GetRootViewModel();
            Debug.Assert(viewModel != null, "Unable to find root view model from association: " + _association.Name);

            if (viewModel != null)
            {
                var association = viewModel.ModelXRef.GetExisting(_association) as Model.Entity.Association;
                Debug.Assert(association != null);
                DeleteEFElementCommand.DeleteInTransaction(cpc, association);
                viewModel.ModelXRef.Remove(association, _association);
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 20; }
        }
    }
}
