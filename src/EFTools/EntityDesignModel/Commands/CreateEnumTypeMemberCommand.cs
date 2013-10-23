// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class CreateEnumTypeMemberCommand : Command
    {
        internal static readonly string PrereqId = "CreateEnumTypeMemberCommand";

        internal EnumType EnumType { get; set; }
        internal string Name { get; set; }
        internal string Value { get; set; }

        internal CreateEnumTypeMemberCommand(EnumType enumType, string name, string value)
            : base(PrereqId)
        {
            ValidateString(name);

            EnumType = enumType;
            Name = name;
            Value = value;
        }

        internal CreateEnumTypeMemberCommand(CreateEnumTypeCommand cmd, string name, string value)
            : base(PrereqId)
        {
            ValidateString(name);

            EnumType = null;
            Name = name;
            Value = value;

            AddPreReqCommand(cmd);
        }

        protected override void ProcessPreReqCommands()
        {
            if (EnumType == null)
            {
                var prereq = GetPreReqCommand(CreateEnumTypeCommand.PrereqId) as CreateEnumTypeCommand;
                if (prereq != null)
                {
                    EnumType = prereq.EnumType;
                }
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            string errorMessage;

            // Check if the EnumTypeMember is unique among its sibling.
            // Note that member names are case sensitive so it is perfectly legal to have members with the same names but differs in case (for e.g. member vs. MEMBER)
            if (ModelHelper.IsUniqueName(typeof(EnumTypeMember), EnumType, Name, true, out errorMessage) == false)
            {
                throw new CommandValidationFailedException(errorMessage);
            }

            var type = ModelHelper.UnderlyingEnumTypes.FirstOrDefault(
                t => String.CompareOrdinal(t.Name, EnumType.UnderlyingType.Value) == 0);

            Debug.Assert(type != null, "Type:" + EnumType.UnderlyingType.Value + " is not valid underlying type for an enum.");

            if (type != null)
            {
                // Check if the EnumTypeMember value is valid.
                if (String.IsNullOrWhiteSpace(Value) == false
                    && ModelHelper.IsValidValueForType(type, Value) == false)
                {
                    throw new CommandValidationFailedException(
                        String.Format(CultureInfo.CurrentCulture, Resources.BadEnumTypeMemberValue, Value));
                }

                var member = new EnumTypeMember(EnumType, null);
                member.LocalName.Value = Name;

                if (String.IsNullOrWhiteSpace(Value) == false)
                {
                    member.Value.Value = Value;
                }
                EnumType.AddMember(member);

                XmlModelHelper.NormalizeAndResolve(member);
            }
        }
    }
}
