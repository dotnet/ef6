// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Services.UnitTests
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Mapping.Update.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class FunctionParameterMappingGeneratorTests
    {
        [Fact]
        public void Can_generate_scalar_and_complex_properties_when_insert()
        {
            var functionParameterMappingGenerator
                = new FunctionParameterMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest);

            var property0 = new EdmProperty("P0");
            var property1 = new EdmProperty("P1");
            var property2 = new EdmProperty("P2");

            property2.SetStoreGeneratedPattern(StoreGeneratedPattern.Computed);
            property2.ConcurrencyMode = ConcurrencyMode.Fixed;

            var complexType = new ComplexType("CT", "N", DataSpace.CSpace);

            complexType.AddMember(property1);

            var complexProperty = EdmProperty.Complex("C", complexType);

            var parameterBindings
                = functionParameterMappingGenerator
                    .Generate(
                        ModificationOperator.Insert,
                        new[] { property0, complexProperty, property2 },
                        new List<EdmProperty>(),
                        useOriginalValues: true)
                    .ToList();

            Assert.Equal(2, parameterBindings.Count());

            var parameterBinding = parameterBindings.First();

            Assert.Equal("P0", parameterBinding.Parameter.Name);
            Assert.Same(property0, parameterBinding.MemberPath.Members.Single());
            Assert.Equal("nvarchar(max)", parameterBinding.Parameter.TypeName);
            Assert.Equal(ParameterMode.In, parameterBinding.Parameter.Mode);
            Assert.False(parameterBinding.IsCurrent);

            parameterBinding = parameterBindings.Last();

            Assert.Equal("C_P1", parameterBinding.Parameter.Name);
            Assert.Same(complexProperty, parameterBinding.MemberPath.Members.First());
            Assert.Same(property1, parameterBinding.MemberPath.Members.Last());
            Assert.Equal("nvarchar(max)", parameterBinding.Parameter.TypeName);
            Assert.Equal(ParameterMode.In, parameterBinding.Parameter.Mode);
            Assert.False(parameterBinding.IsCurrent);
        }

        [Fact]
        public void Can_generate_scalar_and_complex_properties_when_update()
        {
            var functionParameterMappingGenerator
                = new FunctionParameterMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest);

            var property0 = new EdmProperty("P0");
            var property1 = new EdmProperty("P1");
            var property2 = new EdmProperty("P2");

            property2.SetStoreGeneratedPattern(StoreGeneratedPattern.Computed);
            property2.ConcurrencyMode = ConcurrencyMode.Fixed;

            var complexType = new ComplexType("CT", "N", DataSpace.CSpace);

            complexType.AddMember(property1);

            var complexProperty = EdmProperty.Complex("C", complexType);

            var parameterBindings
                = functionParameterMappingGenerator
                    .Generate(
                        ModificationOperator.Update,
                        new[] { property0, complexProperty, property2 },
                        new List<EdmProperty>(),
                        useOriginalValues: true)
                    .ToList();

            Assert.Equal(3, parameterBindings.Count());

            var parameterBinding = parameterBindings.First();

            Assert.Equal("P0", parameterBinding.Parameter.Name);
            Assert.Same(property0, parameterBinding.MemberPath.Members.Single());
            Assert.Equal("nvarchar(max)", parameterBinding.Parameter.TypeName);
            Assert.Equal(ParameterMode.In, parameterBinding.Parameter.Mode);
            Assert.False(parameterBinding.IsCurrent);

            parameterBinding = parameterBindings.ElementAt(1);

            Assert.Equal("C_P1", parameterBinding.Parameter.Name);
            Assert.Same(complexProperty, parameterBinding.MemberPath.Members.First());
            Assert.Same(property1, parameterBinding.MemberPath.Members.Last());
            Assert.Equal("nvarchar(max)", parameterBinding.Parameter.TypeName);
            Assert.Equal(ParameterMode.In, parameterBinding.Parameter.Mode);
            Assert.False(parameterBinding.IsCurrent);

            parameterBinding = parameterBindings.Last();

            Assert.Equal("P2_Original", parameterBinding.Parameter.Name);
            Assert.Same(property2, parameterBinding.MemberPath.Members.Single());
            Assert.Equal("nvarchar(max)", parameterBinding.Parameter.TypeName);
            Assert.Equal(ParameterMode.In, parameterBinding.Parameter.Mode);
            Assert.False(parameterBinding.IsCurrent);
        }

        [Fact]
        public void Can_generate_scalar_and_complex_properties_when_delete()
        {
            var functionParameterMappingGenerator
                            = new FunctionParameterMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest);

            var property0 = new EdmProperty("P0");
            var property1 = new EdmProperty("P1");
            var property2 = new EdmProperty("P2");

            property2.SetStoreGeneratedPattern(StoreGeneratedPattern.Computed);
            property2.ConcurrencyMode = ConcurrencyMode.Fixed;

            var complexType = new ComplexType("CT", "N", DataSpace.CSpace);

            complexType.AddMember(property1);

            var complexProperty = EdmProperty.Complex("C", complexType);

            new EntityType().AddKeyMember(property0);

            var parameterBindings
                = functionParameterMappingGenerator
                    .Generate(
                        ModificationOperator.Delete,
                        new[] { property0, complexProperty, property2 },
                        new List<EdmProperty>(),
                        useOriginalValues: true)
                    .ToList();

            Assert.Equal(2, parameterBindings.Count());

            var parameterBinding = parameterBindings.First();

            Assert.Equal("P0", parameterBinding.Parameter.Name);
            Assert.Same(property0, parameterBinding.MemberPath.Members.Single());
            Assert.Equal("nvarchar(max)", parameterBinding.Parameter.TypeName);
            Assert.Equal(ParameterMode.In, parameterBinding.Parameter.Mode);
            Assert.False(parameterBinding.IsCurrent);

            parameterBinding = parameterBindings.Last();

            Assert.Equal("P2_Original", parameterBinding.Parameter.Name);
            Assert.Same(property2, parameterBinding.MemberPath.Members.Single());
            Assert.Equal("nvarchar(max)", parameterBinding.Parameter.TypeName);
            Assert.Equal(ParameterMode.In, parameterBinding.Parameter.Mode);
            Assert.False(parameterBinding.IsCurrent);
        }

        [Fact]
        public void Can_generate_ia_fk_parameters()
        {
            var functionParameterMappingGenerator
                = new FunctionParameterMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest);

            var associationType
                = new AssociationType
                      {
                          SourceEnd = new AssociationEndMember("S", new EntityType()),
                          TargetEnd = new AssociationEndMember("T", new EntityType())
                      };

            var associationSet
                = new AssociationSet("AS", associationType)
                      {
                          SourceSet = new EntitySet(),
                          TargetSet = new EntitySet()
                      };

            var memberPath
                = new StorageModificationFunctionMemberPath(
                    new EdmMember[] { new EdmProperty("P"), associationType.TargetEnd },
                    associationSet);

            var parameterBindings
                = functionParameterMappingGenerator
                    .Generate(
                        new[] { Tuple.Create(memberPath, "param") },
                        useOriginalValues: true)
                    .ToList();

            var parameterBinding = parameterBindings.Single();

            Assert.Equal("param", parameterBinding.Parameter.Name);
            Assert.Same(memberPath, parameterBinding.MemberPath);
            Assert.Equal("nvarchar(max)", parameterBinding.Parameter.TypeName);
            Assert.Equal(ParameterMode.In, parameterBinding.Parameter.Mode);
            Assert.False(parameterBinding.IsCurrent);
        }

        [Fact]
        public void Generate_should_throw_when_circular_complex_property()
        {
            var functionParameterMappingGenerator
                = new FunctionParameterMappingGenerator(ProviderRegistry.Sql2008_ProviderManifest);

            var complexType = new ComplexType("CT", "N", DataSpace.CSpace);
            complexType.AddMember(EdmProperty.Complex("C1", complexType));

            Assert.Equal(
                Strings.CircularComplexTypeHierarchy,
                Assert.Throws<InvalidOperationException>(
                    () => functionParameterMappingGenerator
                              .Generate(
                                  ModificationOperator.Insert,
                                  new[] { EdmProperty.Complex("C0", complexType) },
                                  new List<EdmProperty>())
                              .ToList()).Message);
        }
    }
}
