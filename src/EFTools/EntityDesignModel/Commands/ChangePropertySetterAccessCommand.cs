// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using Microsoft.Data.Entity.Design.Model.Entity;

    /// <summary>
    ///     Strongly/uniquely-typed command associated with changing the property's SetterAccess
    /// </summary>
    internal class ChangePropertySetterAccessCommand : UpdateDefaultableValueCommand<string>
    {
        public Property Property { get; set; }

        internal string SetterAccess
        {
            get { return Value; }
        }

        public ChangePropertySetterAccessCommand()
            : base(null, null)
        {
        }

        internal ChangePropertySetterAccessCommand(Property property, string value)
            : base(property.Setter, value)
        {
            Property = property;
        }
    }
}
