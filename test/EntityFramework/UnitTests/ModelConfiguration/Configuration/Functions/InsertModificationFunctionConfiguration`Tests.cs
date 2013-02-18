// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Functions
{
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class InsertModificationFunctionConfigurationTTests : ModificationFunctionConfigurationTTests
    {
        [Fact]
        public void HasName_should_set_name_on_underlying_configuration()
        {
            var configuration = new InsertModificationFunctionConfiguration<Entity>();

            configuration.HasName("Foo");

            Assert.Equal("Foo", configuration.Configuration.Name);
        }

        [Fact]
        public void HasName_when_schema_should_set_name_and_schema_on_underlying_configuration()
        {
            var configuration = new InsertModificationFunctionConfiguration<Entity>();

            configuration.HasName("Foo", "Bar");

            Assert.Equal("Foo", configuration.Configuration.Name);
            Assert.Equal("Bar", configuration.Configuration.Schema);
        }

        [Fact]
        public void Parameter_should_return_configuration_for_valid_property_expressions()
        {
            var configuration = new InsertModificationFunctionConfiguration<Entity>();

            Assert.Same(configuration, configuration.Parameter(e => e.Int, "Foo"));
            Assert.Same(configuration, configuration.Parameter(e => e.Nullable, "Foo"));
            Assert.Same(configuration, configuration.Parameter(e => e.String, "Foo"));
            Assert.Same(configuration, configuration.Parameter(e => e.Bytes, "Foo"));
            Assert.Same(configuration, configuration.Parameter(e => e.Geography, "Foo"));
            Assert.Same(configuration, configuration.Parameter(e => e.Geometry, "Foo"));
            Assert.Same(configuration, configuration.Parameter(e => e.ComplexType.Int, "Foo"));
        }

        [Fact]
        public void Parameter_should_set_parameter_name_for_valid_property_expressions()
        {
            Assert.Equal(
                "Foo",
                new InsertModificationFunctionConfiguration<Entity>()
                    .Parameter(e => e.Int, "Foo").Configuration.ParameterNames.Single().Value.Item1);

            Assert.Equal(
                "Foo",
                new InsertModificationFunctionConfiguration<Entity>()
                    .Parameter(e => e.Nullable, "Foo").Configuration.ParameterNames.Single().Value.Item1);

            Assert.Equal(
                "Foo",
                new InsertModificationFunctionConfiguration<Entity>()
                    .Parameter(e => e.String, "Foo").Configuration.ParameterNames.Single().Value.Item1);

            Assert.Equal(
                "Foo",
                new InsertModificationFunctionConfiguration<Entity>()
                    .Parameter(e => e.Bytes, "Foo").Configuration.ParameterNames.Single().Value.Item1);

            Assert.Equal(
                "Foo",
                new InsertModificationFunctionConfiguration<Entity>()
                    .Parameter(e => e.Geography, "Foo").Configuration.ParameterNames.Single().Value.Item1);

            Assert.Equal(
                "Foo",
                new InsertModificationFunctionConfiguration<Entity>()
                    .Parameter(e => e.Geometry, "Foo").Configuration.ParameterNames.Single().Value.Item1);

            Assert.Equal(
                "Foo",
                new InsertModificationFunctionConfiguration<Entity>()
                    .Parameter(e => e.ComplexType.Int, "Foo").Configuration.ParameterNames.Single().Value.Item1);
        }

        [Fact]
        public void Result_should_return_configuration_for_valid_property_expressions()
        {
            var configuration = new InsertModificationFunctionConfiguration<Entity>();

            Assert.Same(configuration, configuration.Result(e => e.Int, "Foo"));
            Assert.Same(configuration, configuration.Result(e => e.Nullable, "Foo"));
            Assert.Same(configuration, configuration.Result(e => e.String, "Foo"));
            Assert.Same(configuration, configuration.Result(e => e.Bytes, "Foo"));
            Assert.Same(configuration, configuration.Result(e => e.Geography, "Foo"));
            Assert.Same(configuration, configuration.Result(e => e.Geometry, "Foo"));
        }

        [Fact]
        public void Result_should_set_column_name_for_valid_property_expressions()
        {
            Assert.Equal(
                "Foo",
                new InsertModificationFunctionConfiguration<Entity>()
                    .Result(e => e.Int, "Foo").Configuration.ResultBindings.Single().Value);

            Assert.Equal(
                "Foo",
                new InsertModificationFunctionConfiguration<Entity>()
                    .Result(e => e.Nullable, "Foo").Configuration.ResultBindings.Single().Value);

            Assert.Equal(
                "Foo",
                new InsertModificationFunctionConfiguration<Entity>()
                    .Result(e => e.String, "Foo").Configuration.ResultBindings.Single().Value);

            Assert.Equal(
                "Foo",
                new InsertModificationFunctionConfiguration<Entity>()
                    .Result(e => e.Bytes, "Foo").Configuration.ResultBindings.Single().Value);

            Assert.Equal(
                "Foo",
                new InsertModificationFunctionConfiguration<Entity>()
                    .Result(e => e.Geography, "Foo").Configuration.ResultBindings.Single().Value);

            Assert.Equal(
                "Foo",
                new InsertModificationFunctionConfiguration<Entity>()
                    .Result(e => e.Geometry, "Foo").Configuration.ResultBindings.Single().Value);
        }

        [Fact]
        public void Result_should_throw_when_complex_property_expressions()
        {
            var configuration = new InsertModificationFunctionConfiguration<Entity>();

            Assert.Throws<InvalidOperationException>(() => configuration.Result(e => e.ComplexType.Int, "Foo"));
        }

        [Fact]
        public void Association_when_collection_should_invoke_action_with_configuration()
        {
            var configuration = new InsertModificationFunctionConfiguration<Entity>();

            AssociationModificationFunctionConfiguration<Entity> associationConfiguration = null;

            configuration.Association<Entity>(e => e.Children, c => { associationConfiguration = c; });

            Assert.NotNull(associationConfiguration);
        }

        [Fact]
        public void Association_when_reference_should_invoke_action_with_configuration()
        {
            var configuration = new InsertModificationFunctionConfiguration<Entity>();

            AssociationModificationFunctionConfiguration<Entity> associationConfiguration = null;

            configuration.Association<Entity>(e => e.Parent, c => { associationConfiguration = c; });

            Assert.NotNull(associationConfiguration);
        }

        [Fact]
        public void Association_when_collection_should_throw_when_complex_property_expression()
        {
            var configuration = new InsertModificationFunctionConfiguration<Entity>();

            Assert.Equal(
                Strings.InvalidPropertyExpression("e => e.Parent.Children"),
                Assert.Throws<InvalidOperationException>(
                    () => configuration.Association<Entity>(e => e.Parent.Children, c => { })).Message);
        }

        [Fact]
        public void Association_when_reference_should_throw_when_complex_property_expression()
        {
            var configuration = new InsertModificationFunctionConfiguration<Entity>();

            Assert.Equal(
                Strings.InvalidPropertyExpression("e => e.Parent.Parent"),
                Assert.Throws<InvalidOperationException>(
                    () => configuration.Association<Entity>(e => e.Parent.Parent, c => { })).Message);
        }
    }
}
