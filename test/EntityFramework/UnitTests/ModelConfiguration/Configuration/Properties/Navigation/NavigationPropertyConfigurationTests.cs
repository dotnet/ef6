// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db.Mapping;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public sealed class NavigationPropertyConfigurationTests
    {
        [Fact]
        public void Inverse_navigation_property_should_throw_when_self_inverse()
        {
            var mockPropertyInfo = new MockPropertyInfo();
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(mockPropertyInfo);

            Assert.Equal(
                Strings.NavigationInverseItself("P", typeof(object)),
                Assert.Throws<InvalidOperationException>(() => navigationPropertyConfiguration.InverseNavigationProperty = mockPropertyInfo)
                    .Message);
        }

        [Fact]
        public void Configure_should_set_configuration_annotations()
        {
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo());
            var navigationProperty = new EdmNavigationProperty
                                         {
                                             Association = new EdmAssociationType().Initialize()
                                         };

            navigationPropertyConfiguration.Configure(navigationProperty, new EdmModel(), new EntityTypeConfiguration(typeof(object)));

            Assert.NotNull(navigationProperty.GetConfiguration());
            Assert.NotNull(navigationProperty.Association.GetConfiguration());
        }

        [Fact]
        public void Configure_should_configure_ends()
        {
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo())
                                                      {
                                                          EndKind = EdmAssociationEndKind.Optional,
                                                          InverseEndKind = EdmAssociationEndKind.Many
                                                      };
            var associationType = new EdmAssociationType().Initialize();

            navigationPropertyConfiguration.Configure(
                new EdmNavigationProperty
                    {
                        Association = associationType
                    },
                new EdmModel(), new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(EdmAssociationEndKind.Many, associationType.SourceEnd.EndKind);
            Assert.Equal(EdmAssociationEndKind.Optional, associationType.TargetEnd.EndKind);
        }

        [Fact]
        public void Configure_should_configure_inverse()
        {
            var inverseMockPropertyInfo = new MockPropertyInfo();
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo())
                                                      {
                                                          InverseNavigationProperty = inverseMockPropertyInfo
                                                      };
            var associationType = new EdmAssociationType().Initialize();
            var inverseAssociationType = new EdmAssociationType().Initialize();
            var model = new EdmModel().Initialize();
            model.AddAssociationType(inverseAssociationType);
            var inverseNavigationProperty
                = model.AddEntityType("T")
                    .AddNavigationProperty("N", inverseAssociationType);
            inverseNavigationProperty.SetClrPropertyInfo(inverseMockPropertyInfo);

            navigationPropertyConfiguration.Configure(
                new EdmNavigationProperty
                    {
                        Association = associationType
                    }, model, new EntityTypeConfiguration(typeof(object)));

            Assert.Same(associationType, inverseNavigationProperty.Association);
            Assert.Same(associationType.SourceEnd, inverseNavigationProperty.ResultEnd);
            Assert.Equal(0, model.GetAssociationTypes().Count());
        }

        [Fact]
        public void Configure_should_configure_constraint()
        {
            var mockType = new MockType();
            var mockPropertyInfo = new MockPropertyInfo(typeof(int), "P");
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo())
                                                      {
                                                          Constraint =
                                                              new ForeignKeyConstraintConfiguration(new[] { mockPropertyInfo.Object })
                                                      };
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EntityType = new EdmEntityType();
            associationType.SourceEnd.EntityType.SetClrType(mockType);
            associationType.SourceEnd.EndKind = EdmAssociationEndKind.Many;
            var property = associationType.SourceEnd.EntityType.AddPrimitiveProperty("P");
            property.PropertyType.EdmType = EdmPrimitiveType.Int32;
            property.SetClrPropertyInfo(mockPropertyInfo);

            navigationPropertyConfiguration.Configure(
                new EdmNavigationProperty
                    {
                        Association = associationType
                    },
                new EdmModel(), new EntityTypeConfiguration(typeof(object)));

            Assert.NotNull(associationType.Constraint);
            Assert.Same(associationType.SourceEnd, associationType.Constraint.DependentEnd);
            Assert.True(associationType.Constraint.DependentProperties.Any());
        }

        [Fact]
        public void Configure_should_configure_delete_action()
        {
            var mockType = new MockType();
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo())
                                                      {
                                                          DeleteAction = EdmOperationAction.Cascade,
                                                      };
            var associationType = new EdmAssociationType().Initialize();
            associationType.SourceEnd.EntityType = new EdmEntityType();
            associationType.SourceEnd.EntityType.SetClrType(mockType);
            associationType.SourceEnd.EndKind = EdmAssociationEndKind.Many; // make this the principal
            associationType.TargetEnd.EndKind = EdmAssociationEndKind.Optional;

            navigationPropertyConfiguration.Configure(
                new EdmNavigationProperty
                    {
                        Association = associationType
                    },
                new EdmModel(), new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(EdmOperationAction.Cascade, associationType.TargetEnd.DeleteAction);
        }

        [Fact]
        public void Configure_should_configure_mapping()
        {
            var manyToManyAssociationMappingConfiguration = new ManyToManyAssociationMappingConfiguration();
            manyToManyAssociationMappingConfiguration.ToTable("Foo");

            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo())
                                                      {
                                                          AssociationMappingConfiguration = manyToManyAssociationMappingConfiguration
                                                      };

            var databaseMapping = new DbDatabaseMapping().Initialize(new EdmModel().Initialize(), new DbDatabaseMetadata());
            var associationSetMapping = databaseMapping.AddAssociationSetMapping(
                new EdmAssociationSet
                    {
                        ElementType = new EdmAssociationType()
                    });
            associationSetMapping.Table = new DbTableMetadata();
            associationSetMapping.AssociationSet.ElementType.SetConfiguration(navigationPropertyConfiguration);

            navigationPropertyConfiguration.Configure(associationSetMapping, databaseMapping);

            Assert.Equal("Foo", associationSetMapping.Table.GetTableName().Name);
        }

        [Fact]
        public void Configure_should_validate_consistency_of_end_kind_when_already_configured()
        {
            var associationType = new EdmAssociationType().Initialize();
            var navigationPropertyConfigurationA
                = new NavigationPropertyConfiguration(new MockPropertyInfo())
                      {
                          InverseEndKind = EdmAssociationEndKind.Optional
                      };
            associationType.SetConfiguration(navigationPropertyConfigurationA);
            var navigationPropertyConfigurationB
                = new NavigationPropertyConfiguration(new MockPropertyInfo())
                      {
                          EndKind = EdmAssociationEndKind.Many
                      };

            Assert.Equal(
                Strings.ConflictingMultiplicities("P", typeof(object)),
                Assert.Throws<InvalidOperationException>(
                    () => navigationPropertyConfigurationB.Configure(
                        new EdmNavigationProperty
                            {
                                Association = associationType
                            },
                        new EdmModel(), new EntityTypeConfiguration(typeof(object)))).Message);
        }

        [Fact]
        public void Configure_should_validate_consistency_of_inverse_end_kind_when_already_configured()
        {
            var associationType = new EdmAssociationType().Initialize();
            var mockPropertyInfo = new MockPropertyInfo();
            var navigationPropertyConfigurationA
                = new NavigationPropertyConfiguration(mockPropertyInfo)
                      {
                          EndKind = EdmAssociationEndKind.Optional
                      };
            associationType.SetConfiguration(navigationPropertyConfigurationA);
            var navigationPropertyConfigurationB
                = new NavigationPropertyConfiguration(new MockPropertyInfo())
                      {
                          InverseEndKind = EdmAssociationEndKind.Many,
                          InverseNavigationProperty = mockPropertyInfo
                      };

            Assert.Equal(
                Strings.ConflictingMultiplicities("P", typeof(object)),
                Assert.Throws<InvalidOperationException>(
                    () => navigationPropertyConfigurationB.Configure(
                        new EdmNavigationProperty
                            {
                                Association = associationType
                            },
                        new EdmModel(), new EntityTypeConfiguration(typeof(object)))).Message);
        }

        [Fact]
        public void Configure_should_validate_consistency_of_delete_action_when_already_configured()
        {
            var associationType = new EdmAssociationType().Initialize();
            var navigationPropertyConfigurationA
                = new NavigationPropertyConfiguration(new MockPropertyInfo())
                      {
                          DeleteAction = EdmOperationAction.None
                      };
            associationType.SetConfiguration(navigationPropertyConfigurationA);
            var navigationPropertyConfigurationB
                = new NavigationPropertyConfiguration(new MockPropertyInfo())
                      {
                          DeleteAction = EdmOperationAction.Cascade
                      };

            Assert.Equal(
                Strings.ConflictingCascadeDeleteOperation("P", typeof(object)),
                Assert.Throws<InvalidOperationException>(
                    () => navigationPropertyConfigurationB.Configure(
                        new EdmNavigationProperty
                            {
                                Association = associationType
                            },
                        new EdmModel(), new EntityTypeConfiguration(typeof(object)))).Message);
        }

        [Fact]
        public void Configure_should_validate_consistency_of_constraint_when_already_configured()
        {
            var associationType = new EdmAssociationType().Initialize();
            var navigationPropertyConfigurationA
                = new NavigationPropertyConfiguration(new MockPropertyInfo())
                      {
                          Constraint = new ForeignKeyConstraintConfiguration(
                              new[]
                                  {
                                      new MockPropertyInfo(typeof(int), "P1").Object
                                  })
                      };
            associationType.SetConfiguration(navigationPropertyConfigurationA);
            var navigationPropertyConfigurationB
                = new NavigationPropertyConfiguration(new MockPropertyInfo())
                      {
                          Constraint = new ForeignKeyConstraintConfiguration(
                              new[]
                                  {
                                      new MockPropertyInfo(typeof(int), "P2").Object
                                  })
                      };

            Assert.Equal(
                Strings.ConflictingConstraint("P", typeof(object)),
                Assert.Throws<InvalidOperationException>(
                    () => navigationPropertyConfigurationB.Configure(
                        new EdmNavigationProperty
                            {
                                Association = associationType
                            },
                        new EdmModel(), new EntityTypeConfiguration(typeof(object)))).Message);
        }

        [Fact]
        public void Configure_should_validate_consistency_of_mapping_when_already_configured()
        {
            var associationType = new EdmAssociationType().Initialize();
            var manyToManyAssociationMappingConfiguration1 = new ManyToManyAssociationMappingConfiguration();
            manyToManyAssociationMappingConfiguration1.ToTable("A");
            var navigationPropertyConfigurationA
                = new NavigationPropertyConfiguration(new MockPropertyInfo())
                      {
                          AssociationMappingConfiguration = manyToManyAssociationMappingConfiguration1
                      };
            associationType.SetConfiguration(navigationPropertyConfigurationA);
            var manyToManyAssociationMappingConfiguration2 = new ManyToManyAssociationMappingConfiguration();
            manyToManyAssociationMappingConfiguration1.ToTable("B");
            var navigationPropertyConfigurationB
                = new NavigationPropertyConfiguration(new MockPropertyInfo())
                      {
                          AssociationMappingConfiguration = manyToManyAssociationMappingConfiguration2
                      };

            Assert.Equal(
                Strings.ConflictingMapping("P", typeof(object)),
                Assert.Throws<InvalidOperationException>(
                    () => navigationPropertyConfigurationB.Configure(
                        new EdmNavigationProperty
                            {
                                Association = associationType
                            },
                        new EdmModel(), new EntityTypeConfiguration(typeof(object)))).Message);
        }

        [Fact]
        public void Configure_should_not_validate_consistency_of_dependent_end_when_both_false()
        {
            var associationType = new EdmAssociationType().Initialize();
            var navigationPropertyConfigurationA
                = new NavigationPropertyConfiguration(new MockPropertyInfo());
            associationType.SetConfiguration(navigationPropertyConfigurationA);
            var navigationPropertyConfigurationB
                = new NavigationPropertyConfiguration(new MockPropertyInfo());

            navigationPropertyConfigurationB.Configure(
                new EdmNavigationProperty
                    {
                        Association = associationType
                    },
                new EdmModel(), new EntityTypeConfiguration(typeof(object)));
        }
    }
}
