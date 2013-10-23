// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;

    internal class DocDataDirtyDelegateCommand : Command, IDocDataDirtyCommand
    {
        private readonly Action<DocDataDirtyDelegateCommand> _delegateCommandCallback;
        public bool IsDocDataDirty { get; internal set; }

        internal DocDataDirtyDelegateCommand(Action<DocDataDirtyDelegateCommand> delegateCommandCallback)
        {
            _delegateCommandCallback = delegateCommandCallback;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            _delegateCommandCallback(this);
        }
    }
}
