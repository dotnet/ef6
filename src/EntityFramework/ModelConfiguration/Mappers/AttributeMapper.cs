// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Mappers
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    internal sealed class AttributeMapper
    {
        private readonly AttributeProvider _attributeProvider;

        public AttributeMapper(AttributeProvider attributeProvider)
        {
            Contract.Requires(attributeProvider != null);

            _attributeProvider = attributeProvider;
        }

        public void Map(PropertyInfo propertyInfo, ICollection<DataModelAnnotation> annotations)
        {
            Contract.Requires(propertyInfo != null);
            Contract.Requires(annotations != null);

            annotations.SetClrAttributes(_attributeProvider.GetAttributes(propertyInfo).ToList());
        }

        public void Map(Type type, ICollection<DataModelAnnotation> annotations)
        {
            Contract.Requires(type != null);
            Contract.Requires(annotations != null);

            annotations.SetClrAttributes(_attributeProvider.GetAttributes(type).ToList());
        }
    }
}
