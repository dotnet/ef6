// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Editors
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors;
    using Microsoft.Data.Entity.Design.UI.Views.Dialogs;

    internal class ReferentialConstraintEditor : ObjectSelectorEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (context == null
                || context.Instance == null)
            {
                return value;
            }

            var desc = context.Instance as EFAssociationDescriptor;
            if (desc != null)
            {
                var assoc = desc.WrappedItem as Association;

                if (assoc != null)
                {
                    var commands = ReferentialConstraintDialog.LaunchReferentialConstraintDialog(assoc);
                    var cpc = new CommandProcessorContext(
                        desc.EditingContext,
                        EfiTransactionOriginator.PropertyWindowOriginatorId,
                        Resources.Tx_ReferentialContraint);
                    var cp = new CommandProcessor(cpc);
                    foreach (var c in commands)
                    {
                        cp.EnqueueCommand(c);
                    }
                    cp.Invoke();
                }
            }

            return value;
        }
    }
}
