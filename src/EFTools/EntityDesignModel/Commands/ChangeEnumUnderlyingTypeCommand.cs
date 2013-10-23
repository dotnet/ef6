// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class ChangeEnumUnderlyingTypeCommand : Command
    {
        public EnumType EnumType { get; set; }
        internal string NewTypeName { get; set; }

        internal ChangeEnumUnderlyingTypeCommand(EnumType enumType, string newType)
        {
            CommandValidation.ValidateEnumType(enumType);
            ValidateString(newType);

            EnumType = enumType;
            NewTypeName = newType;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            if (ModelHelper.UnderlyingEnumTypes.Count(t => String.CompareOrdinal(t.Name, NewTypeName) == 0) == 0)
            {
                throw new CommandValidationFailedException(
                    String.Format(CultureInfo.CurrentCulture, Resources.Incorrect_Enum_UnderlyingType, NewTypeName));
            }
            EnumType.UnderlyingType.Value = NewTypeName;
        }
    }
}
