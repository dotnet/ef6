// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class ChangePropertyTypeNullableCommand : UpdateDefaultableValueCommand<BoolOrNone>
    {
        public Property Property { get; set; }

        internal bool? Nullable
        {
            get { return Value == null ? (bool?)null : Value.PrimitiveValue; }
        }

        public ChangePropertyTypeNullableCommand()
            : base(null, null)
        {
        }

        internal ChangePropertyTypeNullableCommand(Property property, BoolOrNone value)
            : base(property.Nullable, value)
        {
            Property = property;
        }

        internal ChangePropertyTypeNullableCommand(Property property, bool? value)
            : this(property, BoolOrNoneConverter.ValueConverterForBool(value))
        {
        }
    }
}
