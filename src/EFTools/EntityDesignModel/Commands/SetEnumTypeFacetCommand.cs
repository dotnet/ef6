// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class SetEnumTypeFacetCommand : Command
    {
        private readonly bool _isFlag;
        private readonly string _underlyingType;
        private readonly string _externalTypeName;
        private readonly string _enumTypeName;

        private readonly EnumType _enumType;

        internal SetEnumTypeFacetCommand(
            EnumType enumType, string enumTypeName, string underlyingType, string externalTypeName, bool isFlag)
        {
            CommandValidation.ValidateEnumType(enumType);

            _enumType = enumType;
            _enumTypeName = enumTypeName;
            _underlyingType = underlyingType;
            _externalTypeName = externalTypeName;
            _isFlag = isFlag;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            if (string.Compare(_enumTypeName, _enumType.Name.Value, StringComparison.CurrentCulture) != 0)
            {
                CommandProcessor.InvokeSingleCommand(
                    cpc, _enumType.Artifact.ModelManager.CreateRenameCommand(_enumType, _enumTypeName, true));
            }

            if (_enumType.IsFlags.Value != _isFlag)
            {
                CommandProcessor.InvokeSingleCommand(cpc, new SetEnumFlagAttributeCommand(_enumType, _isFlag));
            }

            if (String.CompareOrdinal(_enumType.UnderlyingType.Value, _underlyingType) != 0)
            {
                CommandProcessor.InvokeSingleCommand(cpc, new ChangeEnumUnderlyingTypeCommand(_enumType, _underlyingType));
            }

            if (String.CompareOrdinal(_enumType.ExternalTypeName.Value, _externalTypeName) != 0)
            {
                _enumType.ExternalTypeName.Value = _externalTypeName;
            }
        }
    }
}
