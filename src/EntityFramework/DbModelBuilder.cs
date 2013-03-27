// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.ModelConfiguration.Conventions.Sets;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Mappers;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     DbModelBuilder is used to map CLR classes to a database schema.
    ///     This code centric approach to building an Entity Data Model (EDM) model is known as 'Code First'.
    /// </summary>
    /// <remarks>
    ///     DbModelBuilder is typically used to configure a model by overriding
    ///     <see
    ///         cref="DbContext.OnModelCreating(DbModelBuilder)" />
    ///     .
    ///     You can also use DbModelBuilder independently of DbContext to build a model and then construct a
    ///     <see cref="DbContext" /> or <see cref="T:System.Data.Objects.ObjectContext" />.
    ///     The recommended approach, however, is to use OnModelCreating in <see cref="DbContext" /> as
    ///     the workflow is more intuitive and takes care of common tasks, such as caching the created model.
    ///     Types that form your model are registered with DbModelBuilder and optional configuration can be
    ///     performed by applying data annotations to your classes and/or using the fluent style DbModelBuilder
    ///     API.
    ///     When the Build method is called a set of conventions are run to discover the initial model.
    ///     These conventions will automatically discover aspects of the model, such as primary keys, and
    ///     will also process any data annotations that were specified on your classes. Finally
    ///     any configuration that was performed using the DbModelBuilder API is applied.
    ///     Configuration done via the DbModelBuilder API takes precedence over data annotations which
    ///     in turn take precedence over the default conventions.
    /// </remarks>
    public class DbModelBuilder
    {
        private readonly ModelConfiguration.Configuration.ModelConfiguration _modelConfiguration;
        private readonly ConventionsConfiguration _conventionsConfiguration;
        private readonly DbModelBuilderVersion _modelBuilderVersion;
        private readonly object _lock = new object();

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbModelBuilder" /> class.
        ///     The process of discovering the initial model will use the set of conventions included
        ///     in the most recent version of the Entity Framework installed on your machine.
        /// </summary>
        /// <remarks>
        ///     Upgrading to newer versions of the Entity Framework may cause breaking changes
        ///     in your application because new conventions may cause the initial model to be
        ///     configured differently. There is an alternate constructor that allows a specific
        ///     version of conventions to be specified.
        /// </remarks>
        public DbModelBuilder()
            : this(new ModelConfiguration.Configuration.ModelConfiguration())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbModelBuilder" /> class that will use
        ///     a specific set of conventions to discover the initial model.
        /// </summary>
        /// <param name="modelBuilderVersion"> The version of conventions to be used. </param>
        public DbModelBuilder(DbModelBuilderVersion modelBuilderVersion)
            : this(new ModelConfiguration.Configuration.ModelConfiguration(), modelBuilderVersion)
        {
            if (!(Enum.IsDefined(typeof(DbModelBuilderVersion), modelBuilderVersion)))
            {
                throw new ArgumentOutOfRangeException("modelBuilderVersion");
            }
        }

        internal DbModelBuilder(
            ModelConfiguration.Configuration.ModelConfiguration modelConfiguration,
            DbModelBuilderVersion modelBuilderVersion = DbModelBuilderVersion.Latest)
            : this(
                modelConfiguration, new ConventionsConfiguration(SelectConventionSet(modelBuilderVersion)),
                modelBuilderVersion)
        {
        }

        private static IEnumerable<IConvention> SelectConventionSet(DbModelBuilderVersion modelBuilderVersion)
        {
            switch (modelBuilderVersion)
            {
                case DbModelBuilderVersion.V4_1:
                    return V1ConventionSet.Conventions;
                case DbModelBuilderVersion.V5_0:
                case DbModelBuilderVersion.V6_0:
                case DbModelBuilderVersion.Latest:
                    return V2ConventionSet.Conventions;
                default:
                    throw new ArgumentOutOfRangeException("modelBuilderVersion");
            }
        }

        private DbModelBuilder(
            ModelConfiguration.Configuration.ModelConfiguration modelConfiguration,
            ConventionsConfiguration conventionsConfiguration,
            DbModelBuilderVersion modelBuilderVersion = DbModelBuilderVersion.Latest)
        {
            DebugCheck.NotNull(modelConfiguration);
            DebugCheck.NotNull(conventionsConfiguration);
            if (!(Enum.IsDefined(typeof(DbModelBuilderVersion), modelBuilderVersion)))
            {
                throw new ArgumentOutOfRangeException("modelBuilderVersion");
            }

            _modelConfiguration = modelConfiguration;
            _conventionsConfiguration = conventionsConfiguration;
            _modelBuilderVersion = modelBuilderVersion;
        }

        private DbModelBuilder(DbModelBuilder source)
        {
            DebugCheck.NotNull(source);

            _modelConfiguration = source._modelConfiguration.Clone();
            _conventionsConfiguration = source._conventionsConfiguration.Clone();
            _modelBuilderVersion = source._modelBuilderVersion;
        }

        internal virtual DbModelBuilder Clone()
        {
            lock (_lock)
            {
                return new DbModelBuilder(this);
            }
        }

        /// <summary>
        ///     Excludes a type from the model. This is used to remove types from the model that were added
        ///     by convention during initial model discovery.
        /// </summary>
        /// <typeparam name="T"> The type to be excluded. </typeparam>
        /// <returns> The same DbModelBuilder instance so that multiple calls can be chained. </returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public virtual DbModelBuilder Ignore<T>()
            where T : class
        {
            _modelConfiguration.Ignore(typeof(T));

            return this;
        }

        /// <summary>
        ///     Configures the default database schema name. The default database schema name is used
        ///     when resolving database objects that do not have an explicitly configured schema name.
        /// </summary>
        /// <param name="schema"> The name of the default database schema. </param>
        public virtual DbModelBuilder HasDefaultSchema(string schema)
        {
            _modelConfiguration.DefaultSchema = schema;

            return this;
        }

        /// <summary>
        ///     Excludes the specified type(s) from the model. This is used to remove types from the model that were added
        ///     by convention during initial model discovery.
        /// </summary>
        /// <param name="types"> The types to be excluded from the model. </param>
        /// <returns> The same DbModelBuilder instance so that multiple calls can be chained. </returns>
        public virtual DbModelBuilder Ignore(IEnumerable<Type> types)
        {
            Check.NotNull(types, "types");

            foreach (var type in types)
            {
                _modelConfiguration.Ignore(type);
            }

            return this;
        }

        /// <summary>
        ///     Registers an entity type as part of the model and returns an object that can be used to
        ///     configure the entity. This method can be called multiple times for the same entity to
        ///     perform multiple lines of configuration.
        /// </summary>
        /// <typeparam name="TEntityType"> The type to be registered or configured. </typeparam>
        /// <returns> The configuration object for the specified entity type. </returns>
        public virtual EntityTypeConfiguration<TEntityType> Entity<TEntityType>()
            where TEntityType : class
        {
            return
                new EntityTypeConfiguration<TEntityType>(
                    _modelConfiguration.Entity(typeof(TEntityType), explicitEntity: true));
        }

        /// <summary>
        ///     Registers a type as an entity in the model and returns an object that can be used to
        ///     configure the entity. This method can be called multiple times for the same type to
        ///     perform multiple lines of configuration.
        /// </summary>
        /// <param name="entityType"> The type to be registered or configured. </param>
        /// <returns> The configuration object for the specified entity type. </returns>
        internal virtual EntityTypeConfiguration Entity(Type entityType)
        {
            DebugCheck.NotNull(entityType);

            var config = _modelConfiguration.Entity(entityType);
            config.IsReplaceable = true;
            return config;
        }

        /// <summary>
        ///     Registers a type as a complex type in the model and returns an object that can be used to
        ///     configure the complex type. This method can be called multiple times for the same type to
        ///     perform multiple lines of configuration.
        /// </summary>
        /// <typeparam name="TComplexType"> The type to be registered or configured. </typeparam>
        /// <returns> The configuration object for the specified complex type. </returns>
        public virtual ComplexTypeConfiguration<TComplexType> ComplexType<TComplexType>()
            where TComplexType : class
        {
            return new ComplexTypeConfiguration<TComplexType>(_modelConfiguration.ComplexType(typeof(TComplexType)));
        }

        /// <summary>
        ///     Begins configuration of a lightweight convention that applies to all entities in
        ///     the model.
        /// </summary>
        /// <returns> A configuration object for the convention. </returns>
        public EntityConventionConfiguration Entities()
        {
            return new EntityConventionConfiguration(_conventionsConfiguration);
        }

        /// <summary>
        ///     Begins configuration of a lightweight convention that applies to all entities of
        ///     the specified type in the model. This method does not register entity types as
        ///     part of the model.
        /// </summary>
        /// <typeparam name="T"> The type of the entities that this convention will apply to. </typeparam>
        /// <returns> A configuration object for the convention. </returns>
        public EntityConventionOfTypeConfiguration<T> Entities<T>()
            where T : class
        {
            return new EntityConventionOfTypeConfiguration<T>(_conventionsConfiguration);
        }

        /// <summary>
        ///     Begins configuration of a lightweight convention that applies to all properties
        ///     in the model.
        /// </summary>
        /// <returns> A configuration object for the convention. </returns>
        public PropertyConventionConfiguration Properties()
        {
            return new PropertyConventionConfiguration(_conventionsConfiguration);
        }

        /// <summary>
        ///     Begins configuration of a lightweight convention that applies to all primitive
        ///     properties of the specified type in the model.
        /// </summary>
        /// <typeparam name="T"> The type of the properties that the convention will apply to. </typeparam>
        /// <returns> A configuration object for the convention. </returns>
        /// <remarks>
        ///     The convention will apply to both nullable and non-nullable properties of the
        ///     specified type.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public PropertyConventionConfiguration Properties<T>()
        {
            if (!typeof(T).IsValidEdmScalarType())
            {
                throw Error.ModelBuilder_PropertyFilterTypeMustBePrimitive(typeof(T));
            }

            var config = new PropertyConventionConfiguration(_conventionsConfiguration);

            return config.Where(
                p =>
                    {
                        Type propertyType;
                        p.PropertyType.TryUnwrapNullableType(out propertyType);

                        return propertyType == typeof(T);
                    });
        }

        /// <summary>
        ///     Provides access to the settings of this DbModelBuilder that deal with conventions.
        /// </summary>
        public virtual ConventionsConfiguration Conventions
        {
            get { return _conventionsConfiguration; }
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationRegistrar" /> for this DbModelBuilder.
        ///     The registrar allows derived entity and complex type configurations to be registered with this builder.
        /// </summary>
        public virtual ConfigurationRegistrar Configurations
        {
            get { return new ConfigurationRegistrar(_modelConfiguration); }
        }

        /// <summary>
        ///     Creates a <see cref="DbModel" /> based on the configuration performed using this builder.
        ///     The connection is used to determine the database provider being used as this
        ///     affects the database layer of the generated model.
        /// </summary>
        /// <param name="providerConnection"> Connection to use to determine provider information. </param>
        /// <returns> The model that was built. </returns>
        public virtual DbModel Build(DbConnection providerConnection)
        {
            Check.NotNull(providerConnection, "providerConnection");

            DbProviderManifest providerManifest;
            var providerInfo = providerConnection.GetProviderInfo(out providerManifest);

            return Build(providerManifest, providerInfo);
        }

        /// <summary>
        ///     Creates a <see cref="DbModel" /> based on the configuration performed using this builder.
        ///     Provider information must be specified because this affects the database layer of the generated model.
        ///     For SqlClient the invariant name is 'System.Data.SqlClient' and the manifest token is the version year (i.e. '2005', '2008' etc.)
        /// </summary>
        /// <param name="providerInfo"> The database provider that the model will be used with. </param>
        /// <returns> The model that was built. </returns>
        public virtual DbModel Build(DbProviderInfo providerInfo)
        {
            Check.NotNull(providerInfo, "providerInfo");

            var providerManifest = GetProviderManifest(providerInfo);

            return Build(providerManifest, providerInfo);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        internal DbModelBuilderVersion Version
        {
            get { return _modelBuilderVersion; }
        }

        private DbModel Build(DbProviderManifest providerManifest, DbProviderInfo providerInfo)
        {
            DebugCheck.NotNull(providerManifest);
            DebugCheck.NotNull(providerInfo);

            var model = new EdmModel(DataSpace.CSpace, _modelBuilderVersion.GetEdmVersion());

            model.ProviderInfo = providerInfo;

            _conventionsConfiguration.ApplyModelConfiguration(_modelConfiguration);

            _modelConfiguration.NormalizeConfigurations();

            MapTypes(model);

            _modelConfiguration.Configure(model);
            _conventionsConfiguration.ApplyModel(model);

            model.Validate();

            var databaseMapping = model.GenerateDatabaseMapping(providerManifest);

            // Run the PluralizingTableNameConvention first so that the new table name is available for configuration
            _conventionsConfiguration.ApplyPluralizingTableNameConvention(databaseMapping.Database);

            _modelConfiguration.Configure(databaseMapping, providerManifest);

            _conventionsConfiguration.ApplyDatabase(databaseMapping.Database);
            _conventionsConfiguration.ApplyMapping(databaseMapping);

            databaseMapping.Database.ProviderManifest = providerManifest;
            databaseMapping.Database.ProviderInfo = providerInfo;

            databaseMapping.Database.Validate();

            return new DbModel(databaseMapping, Clone());
        }

        private static DbProviderManifest GetProviderManifest(DbProviderInfo providerInfo)
        {
            DebugCheck.NotNull(providerInfo);

            var providerFactory = DbConfiguration.GetService<DbProviderFactory>(providerInfo.ProviderInvariantName);
            var providerServices = providerFactory.GetProviderServices();
            var providerManifest = providerServices.GetProviderManifest(providerInfo.ProviderManifestToken);

            return providerManifest;
        }

        private void MapTypes(EdmModel model)
        {
            DebugCheck.NotNull(model);

            var typeMapper = new TypeMapper(
                new MappingContext(
                    _modelConfiguration,
                    _conventionsConfiguration,
                    model,
                    _modelBuilderVersion,
                    DbConfiguration.GetService<AttributeProvider>()));

            _modelConfiguration.Entities
                               .Where(type => typeMapper.MapEntityType(type) == null)
                               .Each(t => { throw Error.InvalidEntityType(t); });

            _modelConfiguration.ComplexTypes
                               .Where(type => typeMapper.MapComplexType(type) == null)
                               .Each(t => { throw Error.CodeFirstInvalidComplexType(t); });
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        internal ModelConfiguration.Configuration.ModelConfiguration ModelConfiguration
        {
            get { return _modelConfiguration; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
