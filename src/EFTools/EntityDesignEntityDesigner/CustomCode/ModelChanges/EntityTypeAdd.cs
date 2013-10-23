// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.Model.Commands;

    internal class EntityTypeAdd : ViewModelChange
    {
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal override void Invoke(CommandProcessorContext cpc)
        {
            CreateEntityTypeCommand.CreateEntityTypeAndEntitySetWithDefaultNames(cpc);
        }

        internal override int InvokeOrderPriority
        {
            get { return 100; }
        }
    }
}
