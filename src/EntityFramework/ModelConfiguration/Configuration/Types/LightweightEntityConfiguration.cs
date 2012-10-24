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
        private readonly Type _type;
        private readonly Func<EntityTypeConfiguration> _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightweightEntityConfiguration" /> class.
        /// </summary>
        /// <param name="type">The <see cref="Type" /> of this entity type.</param>
        /// <param name="configuration">The configuration object that this instance wraps.</param>
        public LightweightEntityConfiguration(Type type, Func<EntityTypeConfiguration> configuration)
        {
            Contract.Requires(type != null);
            Contract.Requires(configuration != null);

            _type = type;
            _configuration = configuration;
        }

        /// <summary>
        /// Gets the <see cref="Type" /> of this entity type.
        /// </summary>
        public Type ClrType
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets or sets the entity set name to be used for this entity type.
        /// </summary>
        /// <remarks>
        /// Setting this will have no effect once it has been configured.
        /// </remarks>
        public string EntitySetName
        {
            get { return _configuration().EntitySetName; }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));

                if (_configuration().EntitySetName == null)
                {
                    _configuration().EntitySetName = value;
                }
            }
        }

        /// <summary>
        /// Gets the name of the table that this entity type is mapped to.
        /// </summary>
        public string TableName
        {
            get { return _configuration().TableName; }
        }

        /// <summary>
        /// Gets the database schema of the table that this entity type is mapped to.
        /// </summary>
        public string SchemaName
        {
            get { return _configuration().SchemaName; }
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
        /// Configures the primary key property(s) for this entity type.
        /// </summary>
        /// <param name="propertyInfo">
        /// The property to be used as the primary key. If the primary key is made up of
        /// multiple properties, call this method once for each of them.
        /// </param>
        public void HasKey(PropertyInfo propertyInfo)
        {
            Contract.Requires(propertyInfo != null);

            _configuration().Key(propertyInfo);
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
