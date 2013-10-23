// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Mapping
{
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class FunctionImportEntityTypeMapping : FunctionImportTypeMapping
    {
        internal static readonly string ElementName = "EntityTypeMapping";

        internal FunctionImportEntityTypeMapping(ResultMapping parent, XElement element)
            : base(parent, element)
        {
        }

        internal override string EFTypeName
        {
            get { return ElementName; }
        }

        internal EntityType EntityType
        {
            get { return TypeName.Target as EntityType; }
        }
    }
}
