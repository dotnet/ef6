// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    internal class UnderlyingEnumTypeDefaultableValue : DefaultableValue<string>
    {
        internal static readonly string AttributeUnderlyingType = "UnderlyingType";

        internal UnderlyingEnumTypeDefaultableValue(EFElement parent)
            : base(parent, AttributeUnderlyingType)
        {
        }

        internal override string AttributeName
        {
            get { return AttributeUnderlyingType; }
        }

        public override string DefaultValue
        {
            get { return ModelConstants.Int32PropertyType; }
        }
    }
}
