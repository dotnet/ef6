// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.CodeDom.Compiler;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public sealed class ConfigurationRegistrarTests
    {
        [Fact]
        public void Add_entity_configuration_should_validate_preconditions()
        {
            Assert.Equal(
                new ArgumentNullException("entityTypeConfiguration").Message,
                Assert.Throws<ArgumentNullException>(
                    () => new ConfigurationRegistrar(new ModelConfiguration()).Add(null as EntityTypeConfiguration<object>)).Message);
        }

        [Fact]
        public void Add_complex_type_should_validate_preconditions()
        {
            Assert.Equal(
                new ArgumentNullException("complexTypeConfiguration").Message,
                Assert.Throws<ArgumentNullException>(
                    () => new ConfigurationRegistrar(new ModelConfiguration()).Add(null as ComplexTypeConfiguration<object>)).Message);
        }

        [Fact]
        public void Add_entity_configuration_should_add_to_model_configuration()
        {
            var modelConfiguration = new ModelConfiguration();
            var entityConfiguration = new EntityTypeConfiguration<object>();

            new ConfigurationRegistrar(modelConfiguration).Add(entityConfiguration);

            Assert.Same(entityConfiguration.Configuration, modelConfiguration.Entity(typeof(object)));
        }

        [Fact]
        public void Add_complex_type_configuration_should_add_to_model_configuration()
        {
            var modelConfiguration = new ModelConfiguration();
            var complexTypeConfiguration = new ComplexTypeConfiguration<object>();

            new ConfigurationRegistrar(modelConfiguration).Add(complexTypeConfiguration);

            Assert.Same(complexTypeConfiguration.Configuration, modelConfiguration.ComplexType(typeof(object)));
        }

        [Fact]
        public void AddFromAssembly_throw_if_assembly_is_null()
        {
            var registrar = new ConfigurationRegistrar(new ModelConfiguration());

            Assert.Equal("assembly", Assert.Throws<ArgumentNullException>(() => registrar.AddFromAssembly(null)).ParamName);
        }

        [Fact]
        public void AddFromAssembly_should_add_all_configuration_to_model_configuration()
        {
            var modelConfiguration = new ModelConfiguration();

            new ConfigurationRegistrar(modelConfiguration).AddFromAssembly(CreateDynamicAssemblyWithStructuralTypeConfigurations());

            Assert.Equal(3, modelConfiguration.ComplexTypes.Count());
            Assert.Equal(3, modelConfiguration.Entities.Count());
            Assert.Equal(1, modelConfiguration.ConfiguredTypes.Count(t => t.Name == "Entity1"));
            Assert.Equal(1, modelConfiguration.ConfiguredTypes.Count(t => t.Name == "Complex1"));
            Assert.Equal(1, modelConfiguration.ConfiguredTypes.Count(t => t.Name == "Entity2"));
            Assert.Equal(1, modelConfiguration.ConfiguredTypes.Count(t => t.Name == "Complex2"));
            Assert.Equal(1, modelConfiguration.ConfiguredTypes.Count(t => t.Name == "Entity3"));
            Assert.Equal(1, modelConfiguration.ConfiguredTypes.Count(t => t.Name == "Complex3"));
        }

        [Fact]
        public void Get_configured_types_should_return_types()
        {
            var modelConfiguration = new ModelConfiguration();
            var configurationRegistrar
                = new ConfigurationRegistrar(modelConfiguration)
                    .Add(new ComplexTypeConfiguration<object>())
                    .Add(new EntityTypeConfiguration<string>());

            Assert.Equal(2, configurationRegistrar.GetConfiguredTypes().Count());
        }

        private Assembly CreateDynamicAssemblyWithStructuralTypeConfigurations()
        {
            var parameters = new CompilerParameters
                                 {
                                     GenerateExecutable = false,
                                     OutputAssembly = "DynamicAssemblyWithStructuralTypeConfigurationsForTests.dll"
                                 };
            parameters.ReferencedAssemblies.Add("EntityFramework.dll");

            return CodeDomProvider
                .CreateProvider("C#")
                .CompileAssemblyFromSource(
                    parameters,
                    @"using System.Data.Entity.ModelConfiguration;
                      namespace Tests
                      {
                          public class Entity1 {}
                          public class Entity2 {}
                          public class Entity3 {}
                          public class Complex1 {} 
                          public class Complex2 {} 
                          public class Complex3 {} 
                          public class Foo {}
                          class CommonEntityConfig<T> : EntityTypeConfiguration<T> where T : class {} 
                          class CommonComplexConfig<T> : ComplexTypeConfiguration<T> where T : class {}
                          class EntityConfig1 : EntityTypeConfiguration<Entity1> {} 
                          class ComplexConfig1 : ComplexTypeConfiguration<Complex1> {}
                          class EntityConfig2 : CommonEntityConfig<Entity2> {} 
                          class ComplexConfig2 : CommonComplexConfig<Complex2> {}
                          abstract class AbstractEntityConfig : EntityTypeConfiguration<Entity3> {} 
                          abstract class AbstractComplexConfig : ComplexTypeConfiguration<Complex3> {}
                          class EntityConfig3 : AbstractEntityConfig {} 
                          class ComplexConfig3 : AbstractComplexConfig {}
                      }")
                .CompiledAssembly;
        }
    }
}
