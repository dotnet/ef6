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
    public sealed class PropertyMaxLengthConvention : IEdmConvention<EdmEntityType>,
                                                      IEdmConvention<EdmComplexType>,
                                                      IEdmConvention<EdmAssociationType>
    {
        private const int DefaultLength = 128;

        internal PropertyMaxLengthConvention()
        {
        }

        void IEdmConvention<EdmEntityType>.Apply(EdmEntityType entityType, EdmModel model)
        {
            SetLength(entityType.DeclaredProperties, entityType.DeclaredKeyProperties);
        }

        void IEdmConvention<EdmComplexType>.Apply(EdmComplexType complexType, EdmModel model)
        {
            SetLength(complexType.DeclaredProperties, new List<EdmProperty>());
        }

        private static void SetLength(IEnumerable<EdmProperty> properties, ICollection<EdmProperty> keyProperties)
        {
            foreach (var property in properties)
            {
                if (!property.PropertyType.IsPrimitiveType)
                {
                    continue;
                }

                if (property.PropertyType.PrimitiveType == EdmPrimitiveType.String)
                {
                    SetStringDefaults(property, keyProperties.Contains(property));
                }

                if (property.PropertyType.PrimitiveType == EdmPrimitiveType.Binary)
                {
                    SetBinaryDefaults(property, keyProperties.Contains(property));
                }
            }
        }

        void IEdmConvention<EdmAssociationType>.Apply(EdmAssociationType associationType, EdmModel model)
        {
            if (associationType.Constraint == null)
            {
                return;
            }

            var principalKeyProperties
                = associationType
                    .GetOtherEnd(associationType.Constraint.DependentEnd)
                    .EntityType
                    .KeyProperties();

            if (principalKeyProperties.Count() != associationType.Constraint.DependentProperties.Count)
            {
                return;
            }

            for (var i = 0; i < associationType.Constraint.DependentProperties.Count; i++)
            {
                var dependentProperty = associationType.Constraint.DependentProperties[i];
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