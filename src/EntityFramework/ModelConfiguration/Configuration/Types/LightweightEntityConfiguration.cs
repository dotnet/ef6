// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Types
{
    using System.Diagnostics.Contracts;
    using System.Reflection;

    /// <summary>
    /// Allows configuration to be performed for an entity type in a model.
    /// This configuration functionality is available via lightweight conventions.
    /// </summary>
    public class LightweightEntityConfiguration
    {
        private readonly Func<EntityTypeConfiguration> _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightweightEntityConfiguration" /> class.
        /// </summary>
        /// <param name="configuration">The configuration object that this instance wraps.</param>
        public LightweightEntityConfiguration(Func<EntityTypeConfiguration> configuration)
        {
            Contract.Requires(configuration != null);

            _configuration = configuration;
        }

        /// <summary>
        /// Excludes a property from the model so that it will not be mapped to the database.
        /// </summary>
        /// <param name="propertyInfo">The property to be configured.</param>
        public void Ignore(PropertyInfo propertyInfo)
        {
            Contract.Requires(propertyInfo != null);

            _configuration().Ignore(propertyInfo);
        }

        /// <summary>
        /// Configures the table name that this entity type is mapped to.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <remarks>Calling this will have no effect once it has been configured.</remarks>
        public void ToTable(string tableName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(tableName));

            if (!_configuration().IsTableNameConfigured)
            {
                _configuration().ToTable(tableName);
            }
        }

        /// <summary>
        /// Configures the table name that this entity type is mapped to.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="schemaName">The database schema of the table.</param>
        /// <remarks>Calling this will have no effect once it has been configured.</remarks>
        public void ToTable(string tableName, string schemaName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(tableName));

            if (!_configuration().IsTableNameConfigured)
            {
                _configuration().ToTable(tableName, schemaName);
            }
        }
    }
}
