// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Editors
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using Microsoft.Data.Entity.Design.UI.Views.Dialogs;

    internal class DebugViewerEditor : ObjectSelectorEditor
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

            using (var dlg = new DebugViewerDialog(context.PropertyDescriptor.Name, value as string))
            {
                dlg.ShowDialog();

                return value;
            }
        }
    }
}
