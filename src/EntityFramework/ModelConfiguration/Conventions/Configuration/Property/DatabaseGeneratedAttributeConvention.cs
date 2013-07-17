// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Convention to process instances of <see cref="DatabaseGeneratedAttribute" /> found on properties in the model.
    /// </summary>
    public class DatabaseGeneratedAttributeConvention
        : PrimitivePropertyAttributeConfigurationConvention<DatabaseGeneratedAttribute>
    {
        /// <inheritdoc/>
        public override void Apply(ConventionPrimitivePropertyConfiguration configuration, DatabaseGeneratedAttribute attribute)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(attribute, "attribute");

            configuration.HasDatabaseGeneratedOption(attribute.DatabaseGeneratedOption);
        }
    }
}
