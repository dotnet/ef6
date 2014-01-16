// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Used to configure a property in a mapping fragment.
    /// This configuration functionality is available via the Code First Fluent API, see <see cref="DbModelBuilder" />.
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
        /// Configures the name of the database column used to store the property, in a mapping fragment.
        /// </summary>
        /// <param name="columnName"> The name of the column. </param>
        /// <returns> The same PropertyMappingConfiguration instance so that multiple calls can be chained. </returns>
        public PropertyMappingConfiguration HasColumnName(string columnName)
        {
            Configuration.ColumnName = columnName;

            return this;
        }

        /// <summary>
        /// Sets an annotation in the model for the database column used to store the property. The annotation
        /// value can later be used when processing the column such as when creating migrations.
        /// </summary>
        /// <remarks>
        /// It will likely be necessary to register a <see cref="IMetadataAnnotationSerializer"/> if the type of
        /// the annotation value is anything other than a string. Passing a null value clears any annotation with
        /// the given name on the column that had been previously set.
        /// </remarks>
        /// <param name="name">The annotation name, which must be a valid C#/EDM identifier.</param>
        /// <param name="value">The annotation value, which may be a string or some other type that
        /// can be serialized with an <see cref="IMetadataAnnotationSerializer"/></param>.
        /// <returns>The same PropertyMappingConfiguration instance so that multiple calls can be chained.</returns>
        public PropertyMappingConfiguration HasColumnAnnotation(string name, object value)
        {
            Check.NotEmpty(name, "name");

            Configuration.SetAnnotation(name, value);

            return this;
        }
    }
}
