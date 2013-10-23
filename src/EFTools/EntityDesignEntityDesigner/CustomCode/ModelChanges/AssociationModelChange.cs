// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.UI.Views.Dialogs;

    internal class Association_AddFromDialog : ViewModelChange
    {
        private readonly NewAssociationDialog _dialog;

        internal Association_AddFromDialog(NewAssociationDialog dialog)
        {
            _dialog = dialog;
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            var cmd = new CreateConceptualAssociationCommand(
                _dialog.AssociationName,
                _dialog.End1Entity,
                _dialog.End1Multiplicity,
                _dialog.End1NavigationPropertyName,
                _dialog.End2Entity,
                _dialog.End2Multiplicity,
                _dialog.End2NavigationPropertyName,
                false, // uniquify names
                _dialog.CreateForeignKeyProperties);
            CommandProcessor.InvokeSingleCommand(cpc, cmd);
            Debug.Assert(cmd.CreatedAssociation != null);
        }

        internal override int InvokeOrderPriority
        {
            get { return 130; }
        }
    }
}
