// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Functions
{
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class ModificationFunctionConfigurationTests
    {
        [Fact]
        public void Can_clone_configuration()
        {
            var modificationFunctionConfiguration = new ModificationFunctionConfiguration();

            modificationFunctionConfiguration.HasName("Foo");

            var mockPropertyInfo = new MockPropertyInfo();

            modificationFunctionConfiguration.Parameter(new PropertyPath(mockPropertyInfo));
            modificationFunctionConfiguration.BindResult(new PropertyPath(mockPropertyInfo), "foo");

            var clone = modificationFunctionConfiguration.Clone();

            Assert.NotSame(modificationFunctionConfiguration, clone);
            Assert.Equal("Foo", clone.Name);
            Assert.Equal(1, clone.ParameterConfigurations.Count);
            Assert.Equal(1, clone.ResultBindings.Count);
        }

        [Fact]
        public void Can_set_parameter_name()
        {
            var modificationFunctionConfiguration = new ModificationFunctionConfiguration();

            modificationFunctionConfiguration.HasName("Foo");

            Assert.Equal("Foo", modificationFunctionConfiguration.Name);
        }

        [Fact]
        public void Can_add_parameter_configuration()
        {
            var modificationFunctionConfiguration = new ModificationFunctionConfiguration();

            Assert.Empty(modificationFunctionConfiguration.ParameterConfigurations);

            var mockPropertyInfo = new MockPropertyInfo();

            var parameterConfiguration
                = modificationFunctionConfiguration.Parameter(new PropertyPath(mockPropertyInfo));

            Assert.Same(parameterConfiguration, modificationFunctionConfiguration.ParameterConfigurations.Single().Value);
        }

        [Fact]
        public void Can_set_column_name_for_result_binding()
        {
            var modificationFunctionConfiguration = new ModificationFunctionConfiguration();

            var mockPropertyInfo = new MockPropertyInfo();

            modificationFunctionConfiguration.BindResult(new PropertyPath(mockPropertyInfo), "foo");

            Assert.Same("foo", modificationFunctionConfiguration.ResultBindings.Single().Value);
        }

        [Fact]
        public void Can_configure_function_name_and_parameters()
        {
            var modificationFunctionConfiguration = new ModificationFunctionConfiguration();

            modificationFunctionConfiguration.HasName("Foo");

            var mockPropertyInfo1 = new MockPropertyInfo();
            var mockPropertyInfo2 = new MockPropertyInfo();

            modificationFunctionConfiguration
                .Parameter(new PropertyPath(mockPropertyInfo1))
                .HasName("P1");

            modificationFunctionConfiguration
                .Parameter(new PropertyPath(new[] { mockPropertyInfo1.Object, mockPropertyInfo2.Object }))
                .HasName("P2");

            var entitySet = new EntitySet();
            entitySet.ChangeEntityContainerWithoutCollectionFixup(new EntityContainer());

            var property1 = new EdmProperty("P1");
            property1.SetClrPropertyInfo(mockPropertyInfo1);

            var property2 = new EdmProperty("P1");
            property2.SetClrPropertyInfo(mockPropertyInfo2);

            var function = new EdmFunction();
            var functionParameter1 = new FunctionParameter();
            var functionParameter2 = new FunctionParameter();

            modificationFunctionConfiguration.Configure(
                new StorageModificationFunctionMapping(
                    entitySet,
                    new EntityType(),
                    function,
                    new[]
                        {
                            new StorageModificationFunctionParameterBinding(
                                functionParameter1,
                                new StorageModificationFunctionMemberPath(
                                new[] { property1 },
                                null),
                                false),
                            new StorageModificationFunctionParameterBinding(
                                functionParameter2,
                                new StorageModificationFunctionMemberPath(
                                new[] { property1, property2 },
                                null),
                                false)
                        },
                    null,
                    null));

            Assert.Equal("Foo", function.Name);
            Assert.Equal("P1", functionParameter1.Name);
            Assert.Equal("P2", functionParameter2.Name);
        }

        [Fact]
        public void Configure_should_throw_when_parameter_binding_not_found()
        {
            var modificationFunctionConfiguration = new ModificationFunctionConfiguration();

            var mockPropertyInfo1 = new MockPropertyInfo();

            modificationFunctionConfiguration
                .Parameter(new PropertyPath(mockPropertyInfo1))
                .HasName("P1");

            var entitySet = new EntitySet();
            entitySet.ChangeEntityContainerWithoutCollectionFixup(new EntityContainer());

            Assert.Equal(
                Strings.ModificationFunctionParameterNotFound("P", "F"),
                Assert.Throws<InvalidOperationException>(
                    () => modificationFunctionConfiguration.Configure(
                        new StorageModificationFunctionMapping(
                              entitySet,
                              new EntityType(),
                              new EdmFunction(),
                              new StorageModificationFunctionParameterBinding[0],
                              null,
                              null))).Message);
        }

        [Fact]
        public void Configure_should_throw_when_original_value_parameter_binding_not_found()
        {
            var modificationFunctionConfiguration = new ModificationFunctionConfiguration();

            var mockPropertyInfo1 = new MockPropertyInfo();

            modificationFunctionConfiguration
                .Parameter(new PropertyPath(mockPropertyInfo1), originalValue: true)
                .HasName("P1");

            var entitySet = new EntitySet();
            entitySet.ChangeEntityContainerWithoutCollectionFixup(new EntityContainer());

            Assert.Equal(
                Strings.ModificationFunctionParameterNotFoundOriginal("P", "F"),
                Assert.Throws<InvalidOperationException>(
                    () => modificationFunctionConfiguration.Configure(
                        new StorageModificationFunctionMapping(
                              entitySet,
                              new EntityType(),
                              new EdmFunction(),
                              new StorageModificationFunctionParameterBinding[0],
                              null,
                              null))).Message);
        }

        [Fact]
        public void Configure_should_throw_when_result_binding_not_found()
        {
            var modificationFunctionConfiguration = new ModificationFunctionConfiguration();

            var mockPropertyInfo1 = new MockPropertyInfo();

            modificationFunctionConfiguration
                .BindResult(new PropertyPath(mockPropertyInfo1), "Foo");

            var entitySet = new EntitySet();
            entitySet.ChangeEntityContainerWithoutCollectionFixup(new EntityContainer());

            Assert.Equal(
                Strings.ResultBindingNotFound("P", "F"),
                Assert.Throws<InvalidOperationException>(
                    () => modificationFunctionConfiguration.Configure(
                        new StorageModificationFunctionMapping(
                              entitySet,
                              new EntityType(),
                              new EdmFunction(),
                              new StorageModificationFunctionParameterBinding[0],
                              null,
                              null))).Message);
        }

        [Fact]
        public void Can_configure_result_bindings()
        {
            var modificationFunctionConfiguration = new ModificationFunctionConfiguration();

            var mockPropertyInfo = new MockPropertyInfo();

            modificationFunctionConfiguration
                .BindResult(new PropertyPath(mockPropertyInfo), "Foo");

            var entitySet = new EntitySet();
            entitySet.ChangeEntityContainerWithoutCollectionFixup(new EntityContainer());

            var property = new EdmProperty("P1");
            property.SetClrPropertyInfo(mockPropertyInfo);

            var resultBinding = new StorageModificationFunctionResultBinding("Bar", property);

            modificationFunctionConfiguration.Configure(
                new StorageModificationFunctionMapping(
                    entitySet,
                    new EntityType(),
                    new EdmFunction(),
                    new[]
                        {
                            new StorageModificationFunctionParameterBinding(
                                new FunctionParameter(),
                                new StorageModificationFunctionMemberPath(new[] { property }, null), false)
                        },
                    null,
                    new[] { resultBinding }));

            Assert.Equal("Foo", resultBinding.ColumnName);
        }
    }
}
