// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using Xunit;

    public abstract class PrimitivePropertyConfigurationTests : TestBase
    {
        [Fact]
        public void HasParameterName_should_set_name_on_inner_configuration()
        {
            var innerConfiguration = CreateConfiguration();
            var primitivePropertyConfiguration
                = new PrimitivePropertyConfiguration(innerConfiguration);

            primitivePropertyConfiguration.HasParameterName("Foo");

            Assert.Equal("Foo", innerConfiguration.ParameterName);
        }

        [Fact]
        public void ConfigureFunctionParameters_should_configure_parameter_names()
        {
            var configuration = CreateConfiguration();

            configuration.ParameterName = "Foo";

            var functionParameter1
                = new FunctionParameter(
                    "P1",
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                    ParameterMode.In);

            var functionParameter2
                = new FunctionParameter(
                    "P2",
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                    ParameterMode.In);

            new EdmFunction(
                "F", "N", DataSpace.SSpace,
                new EdmFunctionPayload
                    {
                        Parameters = new[] { functionParameter1, functionParameter2 }
                    });

            configuration.ConfigureFunctionParameters(new[] { functionParameter1, functionParameter2 });

            Assert.Equal("Foo", functionParameter1.Name);
            Assert.Equal("Foo", functionParameter2.Name);
        }

        [Fact]
        public void ConfigureFunctionParameters_should_uniquify_parameter_names()
        {
            var configuration = CreateConfiguration();

            configuration.ParameterName = "Foo";

            var functionParameter1
                = new FunctionParameter(
                    "P1",
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                    ParameterMode.In);

            var functionParameter2
                = new FunctionParameter(
                    "Foo",
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                    ParameterMode.In);

            new EdmFunction(
                "F", "N", DataSpace.SSpace,
                new EdmFunctionPayload
                    {
                        Parameters = new[] { functionParameter1, functionParameter2 }
                    });

            configuration.ConfigureFunctionParameters(new[] { functionParameter1 });

            Assert.Equal("Foo", functionParameter1.Name);
            Assert.Equal("Foo1", functionParameter2.Name);
        }

        [Fact]
        public void HasColumnOrder_should_throw_when_argument_out_of_range()
        {
            var configuration = new PrimitivePropertyConfiguration(new Properties.Primitive.PrimitivePropertyConfiguration());

            Assert.Equal(
                new ArgumentOutOfRangeException("columnOrder").Message,
                Assert.Throws<ArgumentOutOfRangeException>(() => configuration.HasColumnOrder(-1)).Message);
        }

        [Fact]
        public void HasDatabaseGeneratedOption_should_throw_when_argument_out_of_range()
        {
            var configuration = new PrimitivePropertyConfiguration(new Properties.Primitive.PrimitivePropertyConfiguration());

            Assert.Equal(
                new ArgumentOutOfRangeException("databaseGeneratedOption").Message,
                Assert.Throws<ArgumentOutOfRangeException>(() => configuration.HasDatabaseGeneratedOption((DatabaseGeneratedOption?)(-1))).
                    Message);
        }

        [Fact]
        public void OverridableConfigurationParts_is_set_by_constructor()
        {
            var configurationA = CreateConfiguration();

            Assert.Equal(
                OverridableConfigurationParts.OverridableInCSpace | OverridableConfigurationParts.OverridableInSSpace,
                configurationA.OverridableConfigurationParts);
        }

        [Fact]
        public void Configure_should_set_CSpace_configuration_annotation()
        {
            var configuration = CreateConfiguration();
            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            Assert.Null(property.GetConfiguration());

            configuration.Configure(property);

            Assert.Same(configuration, property.GetConfiguration());
        }

        [Fact]
        public void Configure_should_merge_CSpace_configurations()
        {
            var configurationA = CreateConfiguration();
            configurationA.ConcurrencyMode = ConcurrencyMode.Fixed;
            var configurationB = CreateConfiguration();
            configurationB.IsNullable = false;

            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            Assert.Null(property.GetConfiguration());

            configurationA.Configure(property);

            Assert.Equal(
                ConcurrencyMode.Fixed, ((Properties.Primitive.PrimitivePropertyConfiguration)property.GetConfiguration()).ConcurrencyMode);

            configurationB.Configure(property);

            Assert.Equal(
                ConcurrencyMode.Fixed, ((Properties.Primitive.PrimitivePropertyConfiguration)property.GetConfiguration()).ConcurrencyMode);
            Assert.Equal(false, ((Properties.Primitive.PrimitivePropertyConfiguration)property.GetConfiguration()).IsNullable);
        }

        [Fact]
        public void Configure_should_preserve_the_most_derived_configuration()
        {
            var configurationA = CreateConfiguration();
            configurationA.ConcurrencyMode = ConcurrencyMode.Fixed;
            var configurationB = new Properties.Primitive.PrimitivePropertyConfiguration();
            configurationB.IsNullable = false;

            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            Assert.Null(property.GetConfiguration());

            configurationA.Configure(property);

            Assert.Equal(
                ConcurrencyMode.Fixed, ((Properties.Primitive.PrimitivePropertyConfiguration)property.GetConfiguration()).ConcurrencyMode);

            configurationB.Configure(property);

            Assert.Equal(
                ConcurrencyMode.Fixed, ((Properties.Primitive.PrimitivePropertyConfiguration)property.GetConfiguration()).ConcurrencyMode);
            Assert.Equal(false, ((Properties.Primitive.PrimitivePropertyConfiguration)property.GetConfiguration()).IsNullable);
            Assert.Equal(GetConfigurationType(), property.GetConfiguration().GetType());
        }

        [Fact]
        public void Configure_should_update_model_nullability()
        {
            var configuration = CreateConfiguration();
            configuration.IsNullable = true;
            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            configuration.Configure(property);

            Assert.Equal(true, property.Nullable);
        }

        [Fact]
        public void Configure_should_update_model_property_concurrency_mode()
        {
            var configuration = CreateConfiguration();
            configuration.ConcurrencyMode = ConcurrencyMode.Fixed;

            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            configuration.Configure(property);

            Assert.Equal(ConcurrencyMode.Fixed, property.ConcurrencyMode);
        }

        [Fact]
        public void Configure_should_update_model_property_store_generated_pattern()
        {
            var configuration = CreateConfiguration();
            configuration.DatabaseGeneratedOption = DatabaseGeneratedOption.Identity;

            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            configuration.Configure(property);

            Assert.Equal(StoreGeneratedPattern.Identity, property.GetStoreGeneratedPattern());
            Assert.Equal(false, property.Nullable);
        }

        [Fact]
        public void Configure_should_set_SSpace_configuration_annotation()
        {
            var configuration = CreateConfiguration();

            var edmPropertyMapping = new ColumnMappingBuilder(new EdmProperty("C"), new List<EdmProperty>());

            Assert.Null(edmPropertyMapping.ColumnProperty.GetConfiguration());

            configuration.Configure(
                new[]
                    {
                        Tuple.Create(
                            edmPropertyMapping,
                            new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace))
                    },
                ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Same(configuration, edmPropertyMapping.ColumnProperty.GetConfiguration());
        }

        [Fact]
        public void Configure_should_merge_SSpace_configurations()
        {
            var configurationA = CreateConfiguration();
            configurationA.ColumnName = "foo";
            var configurationB = CreateConfiguration();
            configurationB.ColumnType = "nvarchar";

            var edmPropertyMapping = new ColumnMappingBuilder(new EdmProperty("C"), new List<EdmProperty>());

            configurationA.Configure(
                new[] { Tuple.Create(edmPropertyMapping, new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace)) },
                ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal(
                "foo",
                ((Properties.Primitive.PrimitivePropertyConfiguration)edmPropertyMapping.ColumnProperty.GetConfiguration()).ColumnName);

            configurationB.Configure(
                new[] { Tuple.Create(edmPropertyMapping, new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace)) },
                ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal(
                "foo",
                ((Properties.Primitive.PrimitivePropertyConfiguration)edmPropertyMapping.ColumnProperty.GetConfiguration()).ColumnName);
            Assert.Equal(
                "nvarchar",
                ((Properties.Primitive.PrimitivePropertyConfiguration)edmPropertyMapping.ColumnProperty.GetConfiguration()).ColumnType);
        }

        [Fact]
        public void Configure_should_update_mapped_column_name()
        {
            var configuration = CreateConfiguration();
            configuration.ColumnName = "Foo";

            var edmPropertyMapping = new ColumnMappingBuilder(new EdmProperty("C"), new List<EdmProperty>());

            configuration.Configure(
                new[] { Tuple.Create(edmPropertyMapping, new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace)) },
                ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal("Foo", edmPropertyMapping.ColumnProperty.Name);
        }

        [Fact]
        public void Configure_should_update_mapped_column_order()
        {
            var configuration = CreateConfiguration();
            configuration.ColumnOrder = 2;
            configuration.ColumnType = "nvarchar";

            var edmPropertyMapping = new ColumnMappingBuilder(new EdmProperty("C"), new List<EdmProperty>());

            configuration.Configure(
                new[] { Tuple.Create(edmPropertyMapping, new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace)) },
                ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal(2, edmPropertyMapping.ColumnProperty.GetOrder());
        }

        [Fact]
        public void Configure_should_update_mapped_column_type()
        {
            var configuration = CreateConfiguration();
            configuration.ColumnType = "NVarchaR(max)";

            var edmPropertyMapping = new ColumnMappingBuilder(new EdmProperty("C"), new List<EdmProperty>());

            configuration.Configure(
                new[] { Tuple.Create(edmPropertyMapping, new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace)) },
                ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal("nvarchar(max)", edmPropertyMapping.ColumnProperty.TypeName);
        }

        [Fact]
        public void Configure_should_throw_on_incompatible_configurations()
        {
            var configurationNullable = CreateConfiguration();
            configurationNullable.IsNullable = true;
            configurationNullable.OverridableConfigurationParts = OverridableConfigurationParts.None;

            var configurationRequired = CreateConfiguration();
            configurationRequired.IsNullable = false;
            configurationRequired.OverridableConfigurationParts = OverridableConfigurationParts.None;

            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            configurationNullable.Configure(property);

            var message = Assert.Throws<InvalidOperationException>(() => configurationRequired.Configure(property)).Message;

            Assert.True(message.StartsWith(
                Strings.ConflictingPropertyConfiguration("P", string.Empty, string.Empty)));
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
            configurationB.ConcurrencyMode = ConcurrencyMode.Fixed;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(ConcurrencyMode.Fixed, configurationA.ConcurrencyMode);
        }

        [Fact]
        public void CopyFrom_overwrites_non_null_ConcurrencyMode()
        {
            var configurationA = CreateConfiguration();
            configurationA.ConcurrencyMode = ConcurrencyMode.None;
            var configurationB = CreateConfiguration();
            configurationB.ConcurrencyMode = ConcurrencyMode.Fixed;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(ConcurrencyMode.Fixed, configurationA.ConcurrencyMode);
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
            configurationB.OverridableConfigurationParts = OverridableConfigurationParts.OverridableInCSpace
                                                           | OverridableConfigurationParts.OverridableInSSpace;

            configurationA.CopyFrom(configurationB);

            Assert.Equal(
                OverridableConfigurationParts.OverridableInCSpace | OverridableConfigurationParts.OverridableInSSpace,
                configurationA.OverridableConfigurationParts);
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
            configurationB.ConcurrencyMode = ConcurrencyMode.Fixed;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(ConcurrencyMode.Fixed, configurationA.ConcurrencyMode);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_ConcurrencyMode()
        {
            var configurationA = CreateConfiguration();
            configurationA.ConcurrencyMode = ConcurrencyMode.None;
            var configurationB = CreateConfiguration();
            configurationB.ConcurrencyMode = ConcurrencyMode.Fixed;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(ConcurrencyMode.None, configurationA.ConcurrencyMode);
        }

        [Fact]
        public void FillFrom_overwrites_null_ConcurrencyMode_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            var configurationB = CreateConfiguration();
            configurationB.ConcurrencyMode = ConcurrencyMode.Fixed;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(ConcurrencyMode.Fixed, configurationA.ConcurrencyMode);
        }

        [Fact]
        public void FillFrom_does_not_overwrite_non_null_ConcurrencyMode_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            configurationA.ConcurrencyMode = ConcurrencyMode.None;
            var configurationB = CreateConfiguration();
            configurationB.ConcurrencyMode = ConcurrencyMode.Fixed;

            configurationA.FillFrom(configurationB, inCSpace: true);

            Assert.Equal(ConcurrencyMode.None, configurationA.ConcurrencyMode);
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
            configurationA.OverridableConfigurationParts = OverridableConfigurationParts.OverridableInCSpace
                                                           | OverridableConfigurationParts.OverridableInSSpace;
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
            configurationB.OverridableConfigurationParts = OverridableConfigurationParts.OverridableInCSpace
                                                           | OverridableConfigurationParts.OverridableInSSpace;

            configurationA.FillFrom(configurationB, inCSpace: false);

            Assert.Equal(OverridableConfigurationParts.None, configurationA.OverridableConfigurationParts);
        }

        [Fact]
        public void FillFrom_overwrites_OverridableConfigurationParts_set_to_both_in_CSpace()
        {
            var configurationA = CreateConfiguration();
            configurationA.OverridableConfigurationParts = OverridableConfigurationParts.OverridableInCSpace
                                                           | OverridableConfigurationParts.OverridableInSSpace;
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
            configurationB.OverridableConfigurationParts = OverridableConfigurationParts.OverridableInCSpace
                                                           | OverridableConfigurationParts.OverridableInSSpace;

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
            configurationA.ConcurrencyMode = ConcurrencyMode.None;
            configurationA.DatabaseGeneratedOption = DatabaseGeneratedOption.Computed;

            var configurationB = CreateConfiguration();
            configurationB.ColumnName = "foo";
            configurationB.ColumnType = "foo";
            configurationB.ColumnOrder = 2;
            configurationB.IsNullable = false;
            configurationB.ConcurrencyMode = ConcurrencyMode.Fixed;
            configurationB.DatabaseGeneratedOption = DatabaseGeneratedOption.Identity;

            var expectedMessageCSpace = Environment.NewLine + "\t" +
                                        Strings.ConflictingConfigurationValue(
                                            "IsNullable", true, "IsNullable", false);

            expectedMessageCSpace += Environment.NewLine + "\t" +
                                     Strings.ConflictingConfigurationValue(
                                         "ConcurrencyMode", ConcurrencyMode.None, "ConcurrencyMode", ConcurrencyMode.Fixed);

            expectedMessageCSpace += Environment.NewLine + "\t" +
                                     Strings.ConflictingConfigurationValue(
                                         "DatabaseGeneratedOption", DatabaseGeneratedOption.Computed, "DatabaseGeneratedOption",
                                         DatabaseGeneratedOption.Identity);

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
            configurationA.ConcurrencyMode = ConcurrencyMode.Fixed;
            var configurationB = CreateConfiguration();
            configurationB.ConcurrencyMode = ConcurrencyMode.Fixed;

            string errorMessage;
            Assert.True(configurationA.IsCompatible(configurationB, false, out errorMessage));
            Assert.True(string.IsNullOrEmpty(errorMessage));
        }

        [Fact]
        public void IsCompatible_returns_false_for_mismatched_ConcurrencyMode()
        {
            var configurationA = CreateConfiguration();
            configurationA.ConcurrencyMode = ConcurrencyMode.None;
            var configurationB = CreateConfiguration();
            configurationB.ConcurrencyMode = ConcurrencyMode.Fixed;

            var expectedMessage = Environment.NewLine + "\t" +
                                  Strings.ConflictingConfigurationValue(
                                      "ConcurrencyMode", ConcurrencyMode.None, "ConcurrencyMode", ConcurrencyMode.Fixed);

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
            configurationB.ConcurrencyMode = ConcurrencyMode.Fixed;

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
                                      "DatabaseGeneratedOption", DatabaseGeneratedOption.Computed, "DatabaseGeneratedOption",
                                      DatabaseGeneratedOption.Identity);

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

        internal Properties.Primitive.PrimitivePropertyConfiguration CreateConfiguration()
        {
            return (Properties.Primitive.PrimitivePropertyConfiguration)Activator.CreateInstance(GetConfigurationType());
        }

        internal virtual Type GetConfigurationType()
        {
            return typeof(Properties.Primitive.PrimitivePropertyConfiguration);
        }
    }
}
