// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Linq;

    internal class StorageEntityType : EntityType
    {
        internal StorageEntityType(StorageEntityModel model, XElement element)
            : base(model, element)
        {
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal override bool ParseSingleElement(ICollection<XName> unprocessedElements, XElement elem)
        {
            if (elem.Name.LocalName == Property.ElementName)
            {
                Property prop = null;

                prop = new StorageProperty(this, elem);

                prop.Parse(unprocessedElements);
                AddProperty(prop);
            }
            else
            {
                return base.ParseSingleElement(unprocessedElements, elem);
            }
            return true;
        }
    }
}
