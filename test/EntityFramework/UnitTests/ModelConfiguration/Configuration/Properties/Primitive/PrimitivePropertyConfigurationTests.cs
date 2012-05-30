namespace System.Data.Entity.ModelConfiguration.Configuration.UnitTests
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using Xunit;
    using Strings = System.Data.Entity.Resources.Strings;

    public abstract class PrimitivePropertyConfigurationTests : TestBase
    {
        [Fact]
        public void HasColumnOrder_should_throw_when_argument_out_of_range()
        {
            var configuration = new Configuration.PrimitivePropertyConfiguration(new PrimitivePropertyConfiguration());

            Assert.Equal(new ArgumentOutOfRangeException("columnOrder").Message, Assert.Throws<ArgumentOutOfRangeException>(() => configuration.HasColumnOrder(-1)).Message);
        }

        [Fact]
        public void HasDatabaseGeneratedOption_should_throw_when_argument_out_of_range()
        {
            var configuration = new Configuration.PrimitivePropertyConfiguration(new PrimitivePropertyConfiguration());

            Assert.Equal(new ArgumentOutOfRangeException("databaseGeneratedOption").Message, Assert.Throws<ArgumentOutOfRangeException>(() => configuration.HasDatabaseGeneratedOption((DatabaseGeneratedOption?)(-1))).Message);
        }

        [Fact]
        public void OverridableConfigurationParts_is_set_by_constructor()
        {
            var configurationA = CreateConfiguration();

            Assert.Equal(OverridableConfigurationParts.OverridableInCSpace | OverridableConfigurationParts.OverridableInSSpace, configurationA.OverridableConfigurationParts);
        }

        [Fact]
        public void Configure_should_set_CSpace_configuration_annotation()
        {
            var configuration = CreateConfiguration();
            var property = new EdmProperty().AsPrimitive();

            Assert.Null(property.GetConfiguration());

            configuration.Configure(property);

            Assert.Same(configuration, property.GetConfiguration());
        }

        [Fact]
        public void Configure_should_merge_CSpace_configurations()
        {
            var configurationA = CreateConfiguration();
            configurationA.ConcurrencyMode = EdmConcurrencyMode.Fixed;
            var configurationB = CreateConfiguration();
            configurationB.IsNullable = false;

            var property = new EdmProperty().AsPrimitive();

            Assert.Null(property.GetConfiguration());

            configurationA.Configure(property);

            Assert.Equal(EdmConcurrencyMode.Fixed, ((PrimitivePropertyConfiguration)property.GetConfiguration()).ConcurrencyMode);

            configurationB.Configure(property);

            Assert.Equal(EdmConcurrencyMode.Fixed, ((PrimitivePropertyConfiguration)property.GetConfiguration()).ConcurrencyMode);
            Assert.Equal(false, ((PrimitivePropertyConfiguration)property.GetConfiguration()).IsNullable);
        }

        [Fact]
        public void Configure_should_preserve_the_most_derived_configuration()
        {
            var configurationA = CreateConfiguration();
            configurationA.ConcurrencyMode = EdmConcurrencyMode.Fixed;
            var configurationB = new PrimitivePropertyConfiguration();
            configurationB.IsNullable = false;

            var property = new EdmProperty().AsPrimitive();

            Assert.Null(property.GetConfiguration());

            configurationA.Configure(property);

            Assert.Equal(EdmConcurrencyMode.Fixed, ((PrimitivePropertyConfiguration)property.GetConfiguration()).ConcurrencyMode);

            configurationB.Configure(property);

            Assert.Equal(EdmConcurrencyMode.Fixed, ((PrimitivePropertyConfiguration)property.GetConfiguration()).ConcurrencyMode);
            Assert.Equal(false, ((PrimitivePropertyConfiguration)property.GetConfiguration()).IsNullable);
            Assert.Equal(GetConfigurationType(), property.GetConfiguration().GetType());
        }

        [Fact]
        public void Configure_should_update_model_nullability()
        {
            var configuration = CreateConfiguration();
            configuration.IsNullable = true;
            var property = new EdmProperty().AsPrimitive();

            configuration.Configure(property);

            Assert.Equal(true, property.PropertyType.IsNullable);
        }

        [Fact]
        public void Configure_should_update_model_property_concurrency_mode()
        {
            var configuration = CreateConfiguration();
            configuration.ConcurrencyMode = EdmConcurrencyMode.Fixed;

            var property = new EdmProperty().AsPrimitive();

            configuration.Configure(property);

            Assert.Equal(EdmConcurrencyMode.Fixed, property.ConcurrencyMode);
        }

        [Fact]
        public void Configure_should_update_model_property_store_generated_pattern()
        {
            var configuration = CreateConfiguration();
            configuration.DatabaseGeneratedOption = DatabaseGeneratedOption.Identity;

            var property = new EdmProperty().AsPrimitive();

            configuration.Configure(property);

            Assert.Equal(DbStoreGeneratedPattern.Identity, property.GetStoreGeneratedPattern());
            Assert.Equal(false, property.PropertyType.IsNullable);
        }

        [Fact]
        public void Configure_should_set_SSpace_configuration_annotation()
        {
            var configuration = CreateConfiguration();

            var edmPropertyMapping = new DbEdmPropertyMapping { Column = new DbTableColumnMetadata() };

            Assert.Null(edmPropertyMapping.Column.GetConfiguration());

            configuration.Configure(new[] { Tuple.Create(edmPropertyMapping, new DbTableMetadata()) }, ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Same(configuration, edmPropertyMapping.Column.GetConfiguration());
        }

        [Fact]
        public void Configure_should_merge_SSpace_configurations()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnName = "foo";
            var configurationB = CreateConfiguration();
            configurationB.ColumnType = "nvarchar";

            var edmPropertyMapping = new DbEdmPropertyMapping { Column = new DbTableColumnMetadata { Facets = new DbPrimitiveTypeFacets() } };

            configurationA.Configure(new[] { Tuple.Create(edmPropertyMapping, new DbTableMetadata()) }, ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal("foo", ((PrimitivePropertyConfiguration)edmPropertyMapping.Column.GetConfiguration()).ColumnName);

            configurationB.Configure(new[] { Tuple.Create(edmPropertyMapping, new DbTableMetadata()) }, ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal("foo", ((PrimitivePropertyConfiguration)edmPropertyMapping.Column.GetConfiguration()).ColumnName);
            Assert.Equal("nvarchar", ((PrimitivePropertyConfiguration)edmPropertyMapping.Column.GetConfiguration()).ColumnType);
        }

        [Fact]
        public void Configure_should_update_mapped_column_name()
        {
            var configuration = CreateConfiguration();
            configuration.ColumnName = "Foo";

            var edmPropertyMapping = new DbEdmPropertyMapping { Column = new DbTableColumnMetadata() };

            configuration.Configure(new[] { Tuple.Create(edmPropertyMapping, new DbTableMetadata()) }, ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal("Foo", edmPropertyMapping.Column.Name);
        }

        [Fact]
        public void Configure_should_update_mapped_column_order()
        {
            var configuration = CreateConfiguration();
            configuration.ColumnOrder = 2;
            configuration.ColumnType = "nvarchar";

            var edmPropertyMapping = new DbEdmPropertyMapping { Column = new DbTableColumnMetadata { Facets = new DbPrimitiveTypeFacets() } };

            configuration.Configure(new[] { Tuple.Create(edmPropertyMapping, new DbTableMetadata()) }, ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal(2, edmPropertyMapping.Column.GetOrder());
        }

        [Fact]
        public void Configure_should_update_mapped_column_type()
        {
            var configuration = CreateConfiguration();
            configuration.ColumnType = "Foo";

            var edmPropertyMapping = new DbEdmPropertyMapping { Column = new DbTableColumnMetadata() };

            configuration.Configure(new[] { Tuple.Create(edmPropertyMapping, new DbTableMetadata()) }, ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal("Foo", edmPropertyMapping.Column.TypeName);
        }

        [Fact]
        public void Configure_should_throw_on_incompatible_configurations()
        {
            var configurationNullable = CreateConfiguration();
            configurationNullable.IsNullable = true;
            configurationNullable.OverridableConfigurationParts = OverridableConfigurationParts.None;

            var configurationRequired = CreateConfiguration();
            configurationRequired.IsNullable = false;

            var property = new EdmProperty().AsPrimitive();

            configurationNullable.Configure(property);
            Assert.Equal(Strings.ConflictingPropertyConfiguration(string.Empty, string.Empty, Environment.NewLine + "\tIsNullable = True conflicts with IsNullable = False"), Assert.Throws<InvalidOperationException>(() =>
                                                                                                                                                                                                                             configurationRequired.Configure(property)).Message);
        }

        [Fact]
        public void CopyFrom_overwrites_null_ColumnName()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.ColumnName = "foo";

            configurationA.CopyFrom(configurationB);

            Assert.Equal("foo", configurationA.ColumnName);
        }

        [Fact]
        public void CopyFrom_overwrites_non_null_ColumnName()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnName = "bar";
            var configurationB = CreateConfiguration();
            configurationB.ColumnName = "foo";

            configurationA.CopyFrom(configurationB);

            Assert.Equal("foo", configurationA.ColumnName);
        }

        [Fact]
        public void CopyFrom_overwrites_null_ColumnOrder()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.ColumnOrder = 2;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(2, configurationA.ColumnOrder);
        }

        [Fact]
        public void CopyFrom_overwrites_non_null_ColumnOrder()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnOrder = 1;
            var configurationB = CreateConfiguration();
            configurationB.ColumnOrder = 2;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(2, configurationA.ColumnOrder);
        }

        [Fact]
        public void CopyFrom_overwrites_null_ColumnType()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.ColumnType = "foo";

            configurationA.CopyFrom(configurationB);

            Assert.Equal("foo", configurationA.ColumnType);
        }

        [Fact]
        public void CopyFrom_overwrites_non_null_ColumnType()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnType = "bar";
            var configurationB = CreateConfiguration();
            configurationB.ColumnType = "foo";

            configurationA.CopyFrom(configurationB);

            Assert.Equal("foo", configurationA.ColumnType);
        }

        [Fact]
        public void CopyFrom_overwrites_null_ConcurrencyMode()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.ConcurrencyMode = EdmConcurrencyMode.Fixed;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(EdmConcurrencyMode.Fixed, configurationA.ConcurrencyMode);
        }

        [Fact]
        public void CopyFrom_overwrites_non_null_ConcurrencyMode()
        {
            var configurationA = CreateConfiguration();
            configurationA.ConcurrencyMode = EdmConcurrencyMode.None;
            var configurationB = CreateConfiguration();
            configurationB.ConcurrencyMode = EdmConcurrencyMode.Fixed;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(EdmConcurrencyMode.Fixed, configurationA.ConcurrencyMode);
        }

        [Fact]
        public void CopyFrom_overwrites_null_DatabaseGeneratedOption()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.DatabaseGeneratedOption = DatabaseGeneratedOption.Identity;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(DatabaseGeneratedOption.Identity, configurationA.DatabaseGeneratedOption);
        }

        [Fact]
        public void CopyFrom_overwrites_non_null_DatabaseGeneratedOption()
        {
            var configurationA = CreateConfiguration();
            configurationA.DatabaseGeneratedOption = DatabaseGeneratedOption.Computed;
            var configurationB = CreateConfiguration();
            configurationB.DatabaseGeneratedOption = DatabaseGeneratedOption.Identity;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(DatabaseGeneratedOption.Identity, configurationA.DatabaseGeneratedOption);
        }

        [Fact]
        public void CopyFrom_overwrites_null_Nullable()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsNullable = false;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(false, configurationA.IsNullable);
        }

        [Fact]
        public void CopyFrom_overwrites_non_null_Nullable()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsNullable = true;
            var configurationB = CreateConfiguration();
            configurationB.IsNullable = false;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(false, configurationA.IsNullable);
        }

        [Fact]
        public void CopyFrom_overwrites_IsConfigurationOverridable_default()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.OverridableConfigurationParts = OverridableConfigurationParts.None;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(OverridableConfigurationParts.None, configurationA.OverridableConfigurationParts);
        }

        [Fact]
        public void CopyFrom_overwrites_IsConfigurationOverridable_set()
        {
            var configurationA = CreateConfiguration();
            configurationA.OverridableConfigurationParts = OverridableConfigurationParts.None;
            var configurationB = CreateConfiguration();
            configurationB.OverridableConfigurationParts = OverridableConfigurationParts.OverridableInCSpace | OverridableConfigurationParts.OverridableInSSpace;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(OverridableConfigurationParts.OverridableInCSpace | OverridableConfigurationParts.OverridableInSSpace, configurationA.OverridableConfigurationParts);
        }

        [Fact]
        public void FillFrom_overwrites_null_ColumnName()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.ColumnName = "foo";

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal("foo", configurationA.ColumnName);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_ColumnName()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnName = "bar";
            var configurationB = CreateConfiguration();
            configurationB.ColumnName = "foo";

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal("bar", configurationA.ColumnName);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_null_ColumnName_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.ColumnName = "foo";

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(null, configurationA.ColumnName);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_ColumnName_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnName = "bar";
            var configurationB = CreateConfiguration();
            configurationB.ColumnName = "foo";

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal("bar", configurationA.ColumnName);
        }

        [Fact]
        public void FillFrom_overwrites_null_ColumnOrder()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.ColumnOrder = 2;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(2, configurationA.ColumnOrder);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_ColumnOrder()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnOrder = 1;
            var configurationB = CreateConfiguration();
            configurationB.ColumnOrder = 2;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(1, configurationA.ColumnOrder);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_null_ColumnOrder_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.ColumnOrder = 2;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(null, configurationA.ColumnOrder);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_ColumnOrder_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnOrder = 1;
            var configurationB = CreateConfiguration();
            configurationB.ColumnOrder = 2;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(1, configurationA.ColumnOrder);
        }

        [Fact]
        public void FillFrom_overwrites_null_ColumnType()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.ColumnType = "foo";

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal("foo", configurationA.ColumnType);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_ColumnType()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnType = "bar";
            var configurationB = CreateConfiguration();
            configurationB.ColumnType = "foo";

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal("bar", configurationA.ColumnType);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_null_ColumnType_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.ColumnType = "foo";

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(null, configurationA.ColumnType);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_ColumnType_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnType = "bar";
            var configurationB = CreateConfiguration();
            configurationB.ColumnType = "foo";

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal("bar", configurationA.ColumnType);
        }

        [Fact]
        public void FillFrom_overwrites_null_ConcurrencyMode()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.ConcurrencyMode = EdmConcurrencyMode.Fixed;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(EdmConcurrencyMode.Fixed, configurationA.ConcurrencyMode);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_ConcurrencyMode()
        {
            var configurationA = CreateConfiguration();
            configurationA.ConcurrencyMode = EdmConcurrencyMode.None;
            var configurationB = CreateConfiguration();
            configurationB.ConcurrencyMode = EdmConcurrencyMode.Fixed;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(EdmConcurrencyMode.None, configurationA.ConcurrencyMode);
        }

        [Fact]
        public void FillFrom_overwrites_null_ConcurrencyMode_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.ConcurrencyMode = EdmConcurrencyMode.Fixed;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(EdmConcurrencyMode.Fixed, configurationA.ConcurrencyMode);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_ConcurrencyMode_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            configurationA.ConcurrencyMode = EdmConcurrencyMode.None;
            var configurationB = CreateConfiguration();
            configurationB.ConcurrencyMode = EdmConcurrencyMode.Fixed;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(EdmConcurrencyMode.None, configurationA.ConcurrencyMode);
        }

        [Fact]
        public void FillFrom_overwrites_null_DatabaseGeneratedOption()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.DatabaseGeneratedOption = DatabaseGeneratedOption.Identity;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(DatabaseGeneratedOption.Identity, configurationA.DatabaseGeneratedOption);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_DatabaseGeneratedOption()
        {
            var configurationA = CreateConfiguration();
            configurationA.DatabaseGeneratedOption = DatabaseGeneratedOption.Computed;
            var configurationB = CreateConfiguration();
            configurationB.DatabaseGeneratedOption = DatabaseGeneratedOption.Identity;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(DatabaseGeneratedOption.Computed, configurationA.DatabaseGeneratedOption);
        }

        [Fact]
        public void FillFrom_overwrites_null_DatabaseGeneratedOption_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.DatabaseGeneratedOption = DatabaseGeneratedOption.Identity;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(DatabaseGeneratedOption.Identity, configurationA.DatabaseGeneratedOption);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_DatabaseGeneratedOption_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            configurationA.DatabaseGeneratedOption = DatabaseGeneratedOption.Computed;
            var configurationB = CreateConfiguration();
            configurationB.DatabaseGeneratedOption = DatabaseGeneratedOption.Identity;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(DatabaseGeneratedOption.Computed, configurationA.DatabaseGeneratedOption);
        }

        [Fact]
        public void FillFrom_overwrites_null_Nullable()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsNullable = false;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(false, configurationA.IsNullable);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_Nullable()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsNullable = true;
            var configurationB = CreateConfiguration();
            configurationB.IsNullable = false;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(true, configurationA.IsNullable);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_Nullable_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsNullable = true;
            var configurationB = CreateConfiguration();
            configurationB.IsNullable = false;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(true, configurationA.IsNullable);
        }

        [Fact]
        public void FillFrom_overwrites_OverridableConfigurationParts_set_to_both()
        {
            var configurationA = CreateConfiguration();
            configurationA.OverridableConfigurationParts = OverridableConfigurationParts.OverridableInCSpace | OverridableConfigurationParts.OverridableInSSpace;
            var configurationB = CreateConfiguration();
            configurationB.OverridableConfigurationParts = OverridableConfigurationParts.None;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(OverridableConfigurationParts.None, configurationA.OverridableConfigurationParts);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_OverridableConfigurationParts_set_to_None()
        {
            var configurationA = CreateConfiguration();
            configurationA.OverridableConfigurationParts = OverridableConfigurationParts.None;
            var configurationB = CreateConfiguration();
            configurationB.OverridableConfigurationParts = OverridableConfigurationParts.OverridableInCSpace | OverridableConfigurationParts.OverridableInSSpace;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(OverridableConfigurationParts.None, configurationA.OverridableConfigurationParts);
        }

        [Fact]
        public void FillFrom_overwrites_OverridableConfigurationParts_set_to_both_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            configurationA.OverridableConfigurationParts = OverridableConfigurationParts.OverridableInCSpace | OverridableConfigurationParts.OverridableInSSpace;
            var configurationB = CreateConfiguration();
            configurationB.OverridableConfigurationParts = OverridableConfigurationParts.None;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(OverridableConfigurationParts.None, configurationA.OverridableConfigurationParts);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_OverridableConfigurationParts_set_to_None_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            configurationA.OverridableConfigurationParts = OverridableConfigurationParts.None;
            var configurationB = CreateConfiguration();
            configurationB.OverridableConfigurationParts = OverridableConfigurationParts.OverridableInCSpace | OverridableConfigurationParts.OverridableInSSpace;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(OverridableConfigurationParts.None, configurationA.OverridableConfigurationParts);
        }

        [Fact]
        public void IsCompatible_returns_true_for_null()
        {
            var configurationA = CreateConfiguration();

            string errorMessage;
            Assert.True(configurationA.IsCompatible(null, true, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_errors_for_all_mismatched_properties()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnName = "bar";
            configurationA.ColumnType = "bar";
            configurationA.ColumnOrder = 1;
            configurationA.IsNullable = true;
            configurationA.ConcurrencyMode = EdmConcurrencyMode.None;
            configurationA.DatabaseGeneratedOption = DatabaseGeneratedOption.Computed;

            var configurationB = CreateConfiguration();
            configurationB.ColumnName = "foo";
            configurationB.ColumnType = "foo";
            configurationB.ColumnOrder = 2;
            configurationB.IsNullable = false;
            configurationB.ConcurrencyMode = EdmConcurrencyMode.Fixed;
            configurationB.DatabaseGeneratedOption = DatabaseGeneratedOption.Identity;

            var expectedMessageCSpace = Environment.NewLine + "\t" +
                Strings.ConflictingConfigurationValue(
                    "IsNullable", true, "IsNullable", false);

            expectedMessageCSpace += Environment.NewLine + "\t" +
                Strings.ConflictingConfigurationValue(
                    "ConcurrencyMode", EdmConcurrencyMode.None, "ConcurrencyMode", EdmConcurrencyMode.Fixed);

            expectedMessageCSpace += Environment.NewLine + "\t" +
                Strings.ConflictingConfigurationValue(
                    "DatabaseGeneratedOption", DatabaseGeneratedOption.Computed, "DatabaseGeneratedOption", DatabaseGeneratedOption.Identity);

            var expectedMessage = Environment.NewLine + "\t" +
                Strings.ConflictingConfigurationValue(
                    "ColumnName", "bar", "ColumnName", "foo");

            expectedMessage += Environment.NewLine + "\t" +
                Strings.ConflictingConfigurationValue(
                    "ColumnOrder", 1, "ColumnOrder", 2);

            expectedMessage += Environment.NewLine + "\t" +
                Strings.ConflictingConfigurationValue(
                    "ColumnType", "bar", "ColumnType", "foo");

            string errorMessage;
            Assert.False(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);
            Assert.False(configurationA.IsCompatible(configurationB, true, out errorMessage));
            Assert.Equal(expectedMessageCSpace, errorMessage);
        }

        [Fact]
        public void IsCompatible_returns_true_for_matching_ColumnName()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnName = "foo";
            var configurationB = CreateConfiguration();
            configurationB.ColumnName = "foo";

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_false_for_mismatched_ColumnName()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnName = "bar";
            var configurationB = CreateConfiguration();
            configurationB.ColumnName = "foo";

            var expectedMessage = Environment.NewLine + "\t" +
                Strings.ConflictingConfigurationValue(
                    "ColumnName", "bar", "ColumnName", "foo");

            string errorMessage;
            Assert.False(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);

            Assert.True(configurationA.IsCompatible(configurationB, true, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_true_for_null_ColumnName()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.ColumnName = "foo";

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));

            Assert.True(configurationB.IsCompatible(configurationA, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_true_for_matching_ColumnType()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnType = "foo";
            var configurationB = CreateConfiguration();
            configurationB.ColumnType = "foo";

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_false_for_mismatched_ColumnType()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnType = "bar";
            var configurationB = CreateConfiguration();
            configurationB.ColumnType = "foo";

            var expectedMessage = Environment.NewLine + "\t" +
                Strings.ConflictingConfigurationValue(
                    "ColumnType", "bar", "ColumnType", "foo");

            string errorMessage;
            Assert.False(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);

            Assert.True(configurationA.IsCompatible(configurationB, true, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_true_for_null_ColumnType()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.ColumnType = "foo";

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));

            Assert.True(configurationB.IsCompatible(configurationA, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_true_for_matching_ColumnOrder()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnOrder = 2;
            var configurationB = CreateConfiguration();
            configurationB.ColumnOrder = 2;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_false_for_mismatched_ColumnOrder()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnOrder = 1;
            var configurationB = CreateConfiguration();
            configurationB.ColumnOrder = 2;

            var expectedMessage = Environment.NewLine + "\t" +
                Strings.ConflictingConfigurationValue(
                    "ColumnOrder", 1, "ColumnOrder", 2);

            string errorMessage;
            Assert.False(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);

            Assert.True(configurationA.IsCompatible(configurationB, true, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_true_for_null_ColumnOrder()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.ColumnOrder = 2;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));

            Assert.True(configurationB.IsCompatible(configurationA, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_true_for_matching_Nullable()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsNullable = false;
            var configurationB = CreateConfiguration();
            configurationB.IsNullable = false;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_false_for_mismatched_Nullable()
        {
            var configurationA = CreateConfiguration();
            configurationA.IsNullable = true;
            var configurationB = CreateConfiguration();
            configurationB.IsNullable = false;

            var expectedMessage = Environment.NewLine + "\t" +
                Strings.ConflictingConfigurationValue(
                    "IsNullable", true, "IsNullable", false);

            string errorMessage;
            Assert.False(configurationA.IsCompatible(configurationB, true, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);

            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_true_for_null_Nullable()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.IsNullable = false;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));

            Assert.True(configurationB.IsCompatible(configurationA, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_true_for_matching_ConcurrencyMode()
        {
            var configurationA = CreateConfiguration();
            configurationA.ConcurrencyMode = EdmConcurrencyMode.Fixed;
            var configurationB = CreateConfiguration();
            configurationB.ConcurrencyMode = EdmConcurrencyMode.Fixed;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_false_for_mismatched_ConcurrencyMode()
        {
            var configurationA = CreateConfiguration();
            configurationA.ConcurrencyMode = EdmConcurrencyMode.None;
            var configurationB = CreateConfiguration();
            configurationB.ConcurrencyMode = EdmConcurrencyMode.Fixed;

            var expectedMessage = Environment.NewLine + "\t" +
                Strings.ConflictingConfigurationValue(
                    "ConcurrencyMode", EdmConcurrencyMode.None, "ConcurrencyMode", EdmConcurrencyMode.Fixed);

            string errorMessage;
            Assert.False(configurationA.IsCompatible(configurationB, true, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);

            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_true_for_null_ConcurrencyMode()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.ConcurrencyMode = EdmConcurrencyMode.Fixed;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));

            Assert.True(configurationB.IsCompatible(configurationA, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_true_for_matching_DatabaseGeneratedOption()
        {
            var configurationA = CreateConfiguration();
            configurationA.DatabaseGeneratedOption = DatabaseGeneratedOption.Identity;
            var configurationB = CreateConfiguration();
            configurationB.DatabaseGeneratedOption = DatabaseGeneratedOption.Identity;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_false_for_mismatched_DatabaseGeneratedOption()
        {
            var configurationA = CreateConfiguration();
            configurationA.DatabaseGeneratedOption = DatabaseGeneratedOption.Computed;
            var configurationB = CreateConfiguration();
            configurationB.DatabaseGeneratedOption = DatabaseGeneratedOption.Identity;

            var expectedMessage = Environment.NewLine + "\t" +
                Strings.ConflictingConfigurationValue(
                    "DatabaseGeneratedOption", DatabaseGeneratedOption.Computed, "DatabaseGeneratedOption", DatabaseGeneratedOption.Identity);

            string errorMessage;
            Assert.False(configurationA.IsCompatible(configurationB, true, out errorMessage));
            Assert.Equal(expectedMessage, errorMessage);

            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_true_for_null_DatabaseGeneratedOption()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.DatabaseGeneratedOption = DatabaseGeneratedOption.Identity;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));

            Assert.True(configurationB.IsCompatible(configurationA, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        internal PrimitivePropertyConfiguration CreateConfiguration()
        {
            return (PrimitivePropertyConfiguration)Activator.CreateInstance(GetConfigurationType());
        }

        internal virtual Type GetConfigurationType()
        {
            return typeof(PrimitivePropertyConfiguration);
        }
    }
}