// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using Microsoft.Data.Entity.Design.Model.Entity;

    /// <summary>
    ///     Strongly/uniquely-typed command associated with changing the property's GetterAccess
    /// </summary>
    internal class ChangePropertyGetterAccessCommand : UpdateDefaultableValueCommand<string>
    {
        public Property Property { get; set; }

        internal string GetterAccess
        {
            get { return Value; }
        }

        public ChangePropertyGetterAccessCommand()
            : base(null, null)
        {
        }

        internal ChangePropertyGetterAccessCommand(Property property, string value)
            : base(property.Getter, value)
        {
            Property = property;
        }
    }
}
