// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Mappers
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    internal sealed class AttributeMapper
    {
        private readonly AttributeProvider _attributeProvider;

        public AttributeMapper(AttributeProvider attributeProvider)
        {
            DebugCheck.NotNull(attributeProvider);

            _attributeProvider = attributeProvider;
        }

        public void Map(PropertyInfo propertyInfo, ICollection<MetadataProperty> annotations)
        {
            DebugCheck.NotNull(propertyInfo);
            DebugCheck.NotNull(annotations);

            annotations.SetClrAttributes(_attributeProvider.GetAttributes(propertyInfo).ToList());
        }

        public void Map(Type type, ICollection<MetadataProperty> annotations)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(annotations);

            annotations.SetClrAttributes(_attributeProvider.GetAttributes(type).ToList());
        }
    }
}
