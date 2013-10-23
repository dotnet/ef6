// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class ChangePropertyTypePrecisionCommand : UpdateDefaultableValueCommand<StringOrPrimitive<UInt32>>
    {
        public Property Property { get; set; }

        internal uint? Precision
        {
            get { return Value == null ? (uint?)null : Value.PrimitiveValue; }
        }

        public ChangePropertyTypePrecisionCommand()
            : base(null, null)
        {
        }

        internal ChangePropertyTypePrecisionCommand(Property property, StringOrPrimitive<UInt32> value)
            : base(property.Precision, value)
        {
            Property = property;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // TODO 625010: EF does not support precision on floats even though T-SQL does. By default EF will hide the precision
            // of a float column when importing a DB so we'll only update precision if this property is not a float. We also should
            // ignore double types in the CSDL since that is what floats map to.
            // We can remove this hack once 625010 is fixed.
            if (Property == null
                || (Property.TypeName != "float" && Property.TypeName != "Double"))
            {
                base.InvokeInternal(cpc);
            }
        }
    }
}
