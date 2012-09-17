// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Reflection;

    /// <summary>
    ///     Convention to process instances of <see cref="NotMappedAttribute" /> found on properties in the model.
    /// </summary>
    public class NotMappedPropertyAttributeConvention
        : AttributeConfigurationConvention<PropertyInfo, StructuralTypeConfiguration, NotMappedAttribute>
    {
        public override void Apply(
            PropertyInfo memberInfo, StructuralTypeConfiguration configuration, NotMappedAttribute attribute)
        {
            configuration.Ignore(memberInfo);
        }
    }
}
