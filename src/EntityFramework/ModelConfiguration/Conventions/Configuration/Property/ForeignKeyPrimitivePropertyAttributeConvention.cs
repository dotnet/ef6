// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Mappers;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref="ForeignKeyAttribute" /> found on foreign key properties in the model.
    /// </summary>
    public class ForeignKeyPrimitivePropertyAttributeConvention :
        PropertyAttributeConfigurationConvention<ForeignKeyAttribute>
    {
        /// <inheritdoc/>
        public override void Apply(PropertyInfo memberInfo, ConventionTypeConfiguration configuration, ForeignKeyAttribute attribute)
        {
            Check.NotNull(memberInfo, "memberInfo");
            Check.NotNull(configuration, "configuration");
            Check.NotNull(attribute, "attribute");

            if (memberInfo.IsValidEdmScalarProperty())
            {
                var navigationPropertyInfo
                    = (from pi in new PropertyFilter().GetProperties(configuration.ClrType, false)
                       where pi.Name.Equals(attribute.Name, StringComparison.Ordinal)
                       select pi).SingleOrDefault();

                if (navigationPropertyInfo == null)
                {
                    throw Error.ForeignKeyAttributeConvention_InvalidNavigationProperty(
                        memberInfo.Name, configuration.ClrType, attribute.Name);
                }

                var navigationPropertyConfiguration = configuration.NavigationProperty(navigationPropertyInfo);

                navigationPropertyConfiguration.HasConstraint<ForeignKeyConstraintConfiguration>(fk => fk.AddColumn(memberInfo));
            }
        }
    }
}
