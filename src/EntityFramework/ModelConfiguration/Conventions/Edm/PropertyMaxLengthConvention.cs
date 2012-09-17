// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     Convention to set a default maximum length of 128 for properties whose type supports length facets.
    /// </summary>
    public class PropertyMaxLengthConvention : IEdmConvention<EdmEntityType>,
                                               IEdmConvention<EdmComplexType>,
                                               IEdmConvention<EdmAssociationType>
    {
        private const int DefaultLength = 128;

        public void Apply(EdmEntityType edmDataModelItem, EdmModel model)
        {
            SetLength(edmDataModelItem.DeclaredProperties, edmDataModelItem.DeclaredKeyProperties);
        }

        public void Apply(EdmComplexType edmDataModelItem, EdmModel model)
        {
            SetLength(edmDataModelItem.DeclaredProperties, new List<EdmProperty>());
        }

        private static void SetLength(IEnumerable<EdmProperty> properties, ICollection<EdmProperty> keyProperties)
        {
            foreach (var property in properties)
            {
                if (!property.PropertyType.IsPrimitiveType)
                {
                    continue;
                }

                if (property.PropertyType.PrimitiveType
                    == EdmPrimitiveType.String)
                {
                    SetStringDefaults(property, keyProperties.Contains(property));
                }

                if (property.PropertyType.PrimitiveType
                    == EdmPrimitiveType.Binary)
                {
                    SetBinaryDefaults(property, keyProperties.Contains(property));
                }
            }
        }

        public void Apply(EdmAssociationType edmDataModelItem, EdmModel model)
        {
            if (edmDataModelItem.Constraint == null)
            {
                return;
            }

            var principalKeyProperties
                = edmDataModelItem
                    .GetOtherEnd(edmDataModelItem.Constraint.DependentEnd)
                    .EntityType
                    .KeyProperties();

            if (principalKeyProperties.Count()
                != edmDataModelItem.Constraint.DependentProperties.Count)
            {
                return;
            }

            for (var i = 0; i < edmDataModelItem.Constraint.DependentProperties.Count; i++)
            {
                var dependentProperty = edmDataModelItem.Constraint.DependentProperties[i];
                var principalProperty = principalKeyProperties.ElementAt(i);

                if ((dependentProperty.PropertyType.PrimitiveType == EdmPrimitiveType.String)
                    || (dependentProperty.PropertyType.PrimitiveType == EdmPrimitiveType.Binary))
                {
                    var dependentTypeFacets = dependentProperty.PropertyType.PrimitiveTypeFacets;
                    var principalTypeFacets = principalProperty.PropertyType.PrimitiveTypeFacets;

                    dependentTypeFacets.IsUnicode = principalTypeFacets.IsUnicode;
                    dependentTypeFacets.IsFixedLength = principalTypeFacets.IsFixedLength;
                    dependentTypeFacets.MaxLength = principalTypeFacets.MaxLength;
                    dependentTypeFacets.IsMaxLength = principalTypeFacets.IsMaxLength;
                }
            }
        }

        private static void SetStringDefaults(EdmProperty property, bool isKey)
        {
            Contract.Requires(property != null);

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            if (primitiveTypeFacets.IsUnicode == null)
            {
                primitiveTypeFacets.IsUnicode = true;
            }

            SetBinaryDefaults(property, isKey);
        }

        private static void SetBinaryDefaults(EdmProperty property, bool isKey)
        {
            Contract.Requires(property != null);

            var primitiveTypeFacets = property.PropertyType.PrimitiveTypeFacets;

            if (primitiveTypeFacets.IsFixedLength == null)
            {
                primitiveTypeFacets.IsFixedLength = false;
            }

            if ((primitiveTypeFacets.MaxLength == null)
                && (primitiveTypeFacets.IsMaxLength == null))
            {
                if (isKey || (primitiveTypeFacets.IsFixedLength == true))
                {
                    primitiveTypeFacets.MaxLength = DefaultLength;
                }
                else
                {
                    primitiveTypeFacets.IsMaxLength = true;
                }
            }
        }
    }
}
