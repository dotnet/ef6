// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    internal class EnumFlagAttributeDefaultableValue : DefaultableValue<bool>
    {
        internal static readonly string AttributeIsFlags = "IsFlags";

        internal EnumFlagAttributeDefaultableValue(EnumType parent)
            : base(parent, AttributeIsFlags)
        {
        }

        internal override string AttributeName
        {
            get { return AttributeIsFlags; }
        }

        public override bool DefaultValue
        {
            get { return false; }
        }
    }
}
