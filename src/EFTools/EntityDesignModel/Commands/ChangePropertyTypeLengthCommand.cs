// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using Microsoft.Data.Entity.Design.Model.Entity;

    /// <summary>
    ///     Strongly/uniquely-typed command associated with changing the property type's MaxLength
    /// </summary>
    internal class ChangePropertyTypeLengthCommand : UpdateDefaultableValueCommand<StringOrPrimitive<UInt32>>
    {
        public Property Property { get; set; }

        internal uint? Length
        {
            get
            {
                return Value == null
                       || (Value.StringValue != null
                           && (Value.StringValue.Equals(Property.MaxLengthMaxValue, StringComparison.CurrentCulture)))
                           ? (uint?)null
                           : Value.PrimitiveValue;
            }
        }

        internal bool IsMax
        {
            get
            {
                return Value != null && Value.StringValue != null
                       && Value.StringValue.Equals(Property.MaxLengthMaxValue, StringComparison.CurrentCulture)
                           ? true
                           : false;
            }
        }

        public ChangePropertyTypeLengthCommand()
            : base(null, null)
        {
        }

        internal ChangePropertyTypeLengthCommand(Property property, StringOrPrimitive<UInt32> value)
            : base(property.MaxLength, value)
        {
            Property = property;
        }
    }
}
