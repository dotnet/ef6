// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class CopyEnumTypeCommand : CopyAnnotatableElementCommand
    {
        private readonly EnumTypeClipboardFormat _clipboardEnumType;
        private EnumType _createdEnumType;

        /// <summary>
        ///     Creates a copy of EnumType from clipboard format
        /// </summary>
        /// <param name="clipboardEntity"></param>
        /// <returns></returns>
        internal CopyEnumTypeCommand(EnumTypeClipboardFormat clipboardEnumType)
        {
            _clipboardEnumType = clipboardEnumType;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // create copy of the EnumType
            var cmd = new CreateEnumTypeCommand(
                _clipboardEnumType.Name, _clipboardEnumType.UnderlyingType,
                _clipboardEnumType.ExternalTypeName, _clipboardEnumType.IsFlag, true);

            CommandProcessor.InvokeSingleCommand(cpc, cmd);

            // Copy the members
            foreach (var member in _clipboardEnumType.Members.ClipboardMembers)
            {
                CommandProcessor.InvokeSingleCommand(cpc, new CreateEnumTypeMemberCommand(cmd, member.MemberName, member.MemberValue));
            }

            _createdEnumType = cmd.EnumType;
            AddAnnotations(_clipboardEnumType, _createdEnumType);
        }

        internal EnumType EnumType
        {
            get { return _createdEnumType; }
        }
    }
}
