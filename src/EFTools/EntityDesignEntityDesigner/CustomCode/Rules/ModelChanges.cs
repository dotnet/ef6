// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.Rules
{
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VisualStudio.Modeling;

    internal abstract class ViewModelChange : CommonViewModelChange
    {
        internal virtual bool IsDiagramChange
        {
            get { return false; }
        }

        internal abstract void Invoke(CommandProcessorContext cpc);
    }
}
