// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class SetEnumFlagAttributeCommand : Command
    {
        private readonly bool _isFlag;
        private readonly EnumType _enumType;

        internal SetEnumFlagAttributeCommand(EnumType enumType, bool isFlag)
        {
            CommandValidation.ValidateEnumType(enumType);
            _isFlag = isFlag;
            _enumType = enumType;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            if (_enumType.IsFlags.Value != _isFlag)
            {
                _enumType.IsFlags.Value = _isFlag;
            }
        }
    }
}
