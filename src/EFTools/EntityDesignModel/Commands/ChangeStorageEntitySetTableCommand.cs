// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class ChangeStorageEntitySetTableCommand : Command
    {
        internal ChangeStorageEntitySetTableCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        internal string NewTableName { get; set; }

        internal string OldTableName { get; private set; }

        internal StorageEntitySet StorageEntitySet { get; set; }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            StorageEntitySet.Table.Value = NewTableName;
        }

        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            base.PreInvoke(cpc);
            OldTableName = StorageEntitySet.Table.Value;
        }
    }
}
