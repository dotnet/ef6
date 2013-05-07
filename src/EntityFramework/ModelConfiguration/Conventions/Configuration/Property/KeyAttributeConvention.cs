// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Config;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref="KeyAttribute" /> found on properties in the model.
    /// </summary>
    public class KeyAttributeConvention : Convention
    {
        private readonly AttributeProvider _attributeProvider = DbConfiguration.GetService<AttributeProvider>();

        // Not using the public API to avoid including the property in the model if it wasn't in before
        internal override void ApplyPropertyTypeConfiguration<TStructuralTypeConfiguration>(
            PropertyInfo propertyInfo, Func<TStructuralTypeConfiguration> structuralTypeConfiguration, ModelConfiguration modelConfiguration)
        {
            DebugCheck.NotNull(propertyInfo);
            DebugCheck.NotNull(structuralTypeConfiguration);
            DebugCheck.NotNull(modelConfiguration);

            if (typeof(TStructuralTypeConfiguration) == typeof(EntityTypeConfiguration)
                && _attributeProvider.GetAttributes(propertyInfo).OfType<KeyAttribute>().Any())
            {
                var entityTypeConfiguration = (EntityTypeConfiguration)(object)structuralTypeConfiguration();

                if (propertyInfo.IsValidEdmScalarProperty())
                {
                    entityTypeConfiguration.Key(propertyInfo);
                }
            }
        }
    }
}
