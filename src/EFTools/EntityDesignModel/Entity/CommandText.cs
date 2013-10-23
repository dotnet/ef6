// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Xml.Linq;

    internal class CommandText : CommandTextBase
    {
        internal static readonly string ElementName = "CommandText";

        internal CommandText(EFElement parent, XElement element)
            : base(parent, element)
        {
        }
    }
}
