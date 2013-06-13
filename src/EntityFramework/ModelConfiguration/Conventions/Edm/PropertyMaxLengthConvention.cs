// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Convention to set a maximum length for properties whose type supports length facets. The default value is 128.
    /// </summary>
    public class PropertyMaxLengthConvention : IModelConvention<EntityType>,
                                               IModelConvention<ComplexType>,
                                               IModelConvention<AssociationType>
    {
        private const int DefaultLength = 128;
        private readonly int _length;

        /// <summary>
        ///     Initializes a new instance of <see cref="PropertyMaxLengthConvention"/> with the default length.
        /// </summary>
        public PropertyMaxLengthConvention()
            : this(DefaultLength)
        {
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="PropertyMaxLengthConvention"/> with the specified length.
        /// </summary>
        public PropertyMaxLengthConvention(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException("length", Strings.InvalidMaxLengthSize);
            }

            _length = length;
        }

        /// <inheritdoc/>
        public void Apply(EntityType edmDataModelItem, EdmModel model)
        {
            Check.NotNull(edmDataModelItem, "edmDataModelItem");
            Check.NotNull(model, "model");

            SetLength(edmDataModelItem.DeclaredProperties, edmDataModelItem.KeyProperties);
        }

        /// <inheritdoc/>
        public void Apply(ComplexType edmDataModelItem, EdmModel model)
        {
            Check.NotNull(edmDataModelItem, "edmDataModelItem");
            Check.NotNull(model, "model");

            SetLength(edmDataModelItem.Properties, new List<EdmProperty>());
        }

        private void SetLength(IEnumerable<EdmProperty> properties, ICollection<EdmProperty> keyProperties)
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

        /// <inheritdoc/>
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

        private void SetStringDefaults(EdmProperty property, bool isKey)
        {
            DebugCheck.NotNull(property);

            if (property.IsUnicode == null)
            {
                property.IsUnicode = true;
            }

            SetBinaryDefaults(property, isKey);
        }

        private void SetBinaryDefaults(EdmProperty property, bool isKey)
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
                    property.MaxLength = _length;
                }
                else
                {
                    property.IsMaxLength = true;
                }
            }
        }
    }
}
