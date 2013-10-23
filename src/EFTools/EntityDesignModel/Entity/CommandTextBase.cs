// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Xml.Linq;

    internal abstract class CommandTextBase : EFElement
    {
        internal CommandTextBase(EFElement parent, XElement element)
            : base(parent, element)
        {
        }

        internal string Command
        {
            get { return XElement.Value; }
        }
    }
}
