// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Convention to set a default maximum length of 128 for properties whose type supports length facets.
    /// </summary>
    public class PropertyMaxLengthConvention : IEdmConvention<EntityType>,
                                               IEdmConvention<ComplexType>,
                                               IEdmConvention<AssociationType>
    {
        private const int DefaultLength = 128;

        public void Apply(EntityType edmDataModelItem, EdmModel model)
        {
            Check.NotNull(edmDataModelItem, "edmDataModelItem");
            Check.NotNull(model, "model");

            SetLength(edmDataModelItem.DeclaredProperties, edmDataModelItem.KeyProperties);
        }

        public void Apply(ComplexType edmDataModelItem, EdmModel model)
        {
            Check.NotNull(edmDataModelItem, "edmDataModelItem");
            Check.NotNull(model, "model");

            SetLength(edmDataModelItem.Properties, new List<EdmProperty>());
        }

        private static void SetLength(IEnumerable<EdmProperty> properties, ICollection<EdmProperty> keyProperties)
        {
            foreach (var property in properties)
            {
                if (!property.IsPrimitiveType)
                {
                    continue;
                }

                if (property.PrimitiveType
                    == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))
                {
                    SetStringDefaults(property, keyProperties.Contains(property));
                }

                if (property.PrimitiveType
                    == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary))
                {
                    SetBinaryDefaults(property, keyProperties.Contains(property));
                }
            }
        }

        public void Apply(AssociationType edmDataModelItem, EdmModel model)
        {
            Check.NotNull(edmDataModelItem, "edmDataModelItem");
            Check.NotNull(model, "model");

            if (edmDataModelItem.Constraint == null)
            {
                return;
            }

            var principalKeyProperties
                = edmDataModelItem
                    .GetOtherEnd(edmDataModelItem.Constraint.DependentEnd).GetEntityType()
                    .KeyProperties();

            if (principalKeyProperties.Count()
                != edmDataModelItem.Constraint.ToProperties.Count)
            {
                return;
            }

            for (var i = 0; i < edmDataModelItem.Constraint.ToProperties.Count; i++)
            {
                var dependentProperty = edmDataModelItem.Constraint.ToProperties[i];
                var principalProperty = principalKeyProperties.ElementAt(i);

                if ((dependentProperty.PrimitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))
                    || (dependentProperty.PrimitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary)))
                {
                    dependentProperty.IsUnicode = principalProperty.IsUnicode;
                    dependentProperty.IsFixedLength = principalProperty.IsFixedLength;
                    dependentProperty.MaxLength = principalProperty.MaxLength;
                    dependentProperty.IsMaxLength = principalProperty.IsMaxLength;
                }
            }
        }

        private static void SetStringDefaults(EdmProperty property, bool isKey)
        {
            DebugCheck.NotNull(property);

            if (property.IsUnicode == null)
            {
                property.IsUnicode = true;
            }

            SetBinaryDefaults(property, isKey);
        }

        private static void SetBinaryDefaults(EdmProperty property, bool isKey)
        {
            DebugCheck.NotNull(property);

            if (property.IsFixedLength == null)
            {
                property.IsFixedLength = false;
            }

            if ((property.MaxLength == null)
                && (!property.IsMaxLength))
            {
                if (isKey || (property.IsFixedLength == true))
                {
                    property.MaxLength = DefaultLength;
                }
                else
                {
                    property.IsMaxLength = true;
                }
            }
        }
    }
}
