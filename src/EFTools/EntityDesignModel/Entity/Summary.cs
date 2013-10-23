// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Xml.Linq;

    internal class Summary : TextNode
    {
        internal static readonly string ElementName = "Summary";

        internal Summary(EFContainer parent, XElement element)
            : base(parent, element)
        {
        }
    }
}
