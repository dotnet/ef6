// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Used to configure a property in a mapping fragment.
    ///     This configuration functionality is available via the Code First Fluent API, see <see cref="DbModelBuilder" />.
    /// </summary>
    public class PropertyMappingConfiguration
    {
        private readonly Properties.Primitive.PrimitivePropertyConfiguration _configuration;

        internal PropertyMappingConfiguration(Properties.Primitive.PrimitivePropertyConfiguration configuration)
        {
            DebugCheck.NotNull(configuration);

            _configuration = configuration;
        }

        internal Properties.Primitive.PrimitivePropertyConfiguration Configuration
        {
            get { return _configuration; }
        }

        /// <summary>
        ///     Configures the name of the database column used to store the property, in a mapping fragment.
        /// </summary>
        /// <param name="columnName"> The name of the column. </param>
        /// <returns> The same PropertyMappingConfiguration instance so that multiple calls can be chained. </returns>
        public PropertyMappingConfiguration HasColumnName(string columnName)
        {
            Configuration.ColumnName = columnName;

            return this;
        }
    }
}
