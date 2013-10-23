// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.Views;
    using Microsoft.Data.Entity.Design.UI.Views.Dialogs;

    internal abstract class InheritanceModelChange : ViewModelChange
    {
        private readonly Inheritance _inheritance;

        protected InheritanceModelChange(Inheritance inheritance)
        {
            _inheritance = inheritance;
        }

        public Inheritance Inheritance
        {
            get { return _inheritance; }
        }
    }

    internal class Inheritance_AddFromDialog : ViewModelChange
    {
        private readonly NewInheritanceDialog _dialog;

        internal Inheritance_AddFromDialog(NewInheritanceDialog dialog)
        {
            _dialog = dialog;
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            ViewUtils.SetBaseEntityType(cpc, _dialog.DerivedEntityType as ConceptualEntityType, _dialog.BaseEntityType);
        }

        internal override int InvokeOrderPriority
        {
            get { return 120; }
        }
    }
}
