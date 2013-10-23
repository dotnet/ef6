// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    internal class DeleteDefaultableValueCommand<T> : Command
    {
        private readonly DefaultableValue<T> _defaultableValue;

        internal DeleteDefaultableValueCommand(DefaultableValue<T> defaultableValue)
        {
            _defaultableValue = defaultableValue;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            _defaultableValue.Delete();
        }
    }
}
