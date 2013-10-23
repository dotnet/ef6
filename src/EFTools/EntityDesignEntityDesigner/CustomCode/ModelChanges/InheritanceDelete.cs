// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class InheritanceDelete : InheritanceModelChange
    {
        internal InheritanceDelete(Inheritance inheritance)
            : base(inheritance)
        {
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            var viewModel = Inheritance.GetRootViewModel();
            Debug.Assert(viewModel != null, "Unable to find root view model from inheritance: " + Inheritance);
            if (viewModel != null)
            {
                var derivedEntity = viewModel.ModelXRef.GetExisting(Inheritance.TargetEntityType) as ConceptualEntityType;
                Debug.Assert(derivedEntity != null);
                if (derivedEntity != null)
                {
                    viewModel.ModelXRef.Remove(derivedEntity.BaseType, Inheritance);
                    var cmd = new DeleteInheritanceCommand(derivedEntity);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 10; }
        }
    }
}
