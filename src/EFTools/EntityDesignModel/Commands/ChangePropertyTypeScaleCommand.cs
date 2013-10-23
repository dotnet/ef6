// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class ChangePropertyTypeScaleCommand : UpdateDefaultableValueCommand<StringOrPrimitive<UInt32>>
    {
        public Property Property { get; set; }

        internal uint? Scale
        {
            get { return Value == null ? (uint?)null : Value.PrimitiveValue; }
        }

        public ChangePropertyTypeScaleCommand()
            : base(null, null)
        {
        }

        internal ChangePropertyTypeScaleCommand(Property property, StringOrPrimitive<UInt32> value)
            : base(property.Scale, value)
        {
            Property = property;
        }
    }
}
