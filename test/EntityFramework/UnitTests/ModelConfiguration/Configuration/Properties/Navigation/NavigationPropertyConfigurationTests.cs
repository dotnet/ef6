// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public sealed class NavigationPropertyConfigurationTests
    {
        [Fact]
        public void Inverse_navigation_property_should_throw_when_self_inverse()
        {
            var mockPropertyInfo = new MockPropertyInfo(new MockType(), "N");
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(mockPropertyInfo);

            Assert.Equal(
                Strings.NavigationInverseItself("N", typeof(object)),
                Assert.Throws<InvalidOperationException>(() => navigationPropertyConfiguration.InverseNavigationProperty = mockPropertyInfo)
                      .Message);
        }

        [Fact]
        public void Configure_should_set_configuration_annotations()
        {
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N"));
            var navigationProperty = new NavigationProperty("N", TypeUsage.Create(new EntityType("E", "N", DataSpace.CSpace)))
                                         {
                                             RelationshipType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)
                                         };

            navigationProperty.Association.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            navigationProperty.Association.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));

            navigationPropertyConfiguration.Configure(
                navigationProperty, new EdmModel(DataSpace.CSpace), new EntityTypeConfiguration(typeof(object)));

            Assert.NotNull(navigationProperty.GetConfiguration());
            Assert.NotNull(navigationProperty.Association.GetConfiguration());
        }

        [Fact]
        public void Configure_should_configure_ends()
        {
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N"))
                                                      {
                                                          RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne,
                                                          InverseEndKind = RelationshipMultiplicity.Many
                                                      };
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));

            navigationPropertyConfiguration.Configure(
                new NavigationProperty("N", TypeUsage.Create(associationType.TargetEnd.GetEntityType()))
                    {
                        RelationshipType = associationType
                    },
                new EdmModel(DataSpace.CSpace), new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(RelationshipMultiplicity.Many, associationType.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationType.TargetEnd.RelationshipMultiplicity);
        }

        [Fact]
        public void Configure_should_configure_inverse()
        {
            var inverseMockPropertyInfo = new MockPropertyInfo();
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N"))
                                                      {
                                                          InverseNavigationProperty = inverseMockPropertyInfo
                                                      };
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));
            var inverseAssociationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            inverseAssociationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            inverseAssociationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));
            var model = new EdmModel(DataSpace.CSpace);
            model.AddAssociationType(inverseAssociationType);
            var inverseNavigationProperty
                = model.AddEntityType("T")
                       .AddNavigationProperty("N", inverseAssociationType);
            inverseNavigationProperty.SetClrPropertyInfo(inverseMockPropertyInfo);

            navigationPropertyConfiguration.Configure(
                new NavigationProperty("N", TypeUsage.Create(associationType.TargetEnd.GetEntityType()))
                    {
                        RelationshipType = associationType
                    }, model, new EntityTypeConfiguration(typeof(object)));

            Assert.Same(associationType, inverseNavigationProperty.Association);
            Assert.Same(associationType.SourceEnd, inverseNavigationProperty.ResultEnd);
            Assert.Same(associationType.TargetEnd, inverseNavigationProperty.FromEndMember);
            Assert.Equal(0, model.AssociationTypes.Count());
        }

        [Fact]
        public void Configure_should_configure_constraint()
        {
            var mockType = new MockType();
            var mockPropertyInfo = new MockPropertyInfo(new MockType(), "P");
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N"))
                                                      {
                                                          Constraint =
                                                              new ForeignKeyConstraintConfiguration(new[] { mockPropertyInfo.Object })
                                                      };
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));

            associationType.SourceEnd.GetEntityType().Annotations.SetClrType(mockType);
            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.Many;
            var property1 = EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            associationType.SourceEnd.GetEntityType().AddMember(property1);
            var property = property1;
            property.SetClrPropertyInfo(mockPropertyInfo);

            navigationPropertyConfiguration.Configure(
                new NavigationProperty("N", TypeUsage.Create(associationType.TargetEnd.GetEntityType()))
                    {
                        RelationshipType = associationType
                    },
                new EdmModel(DataSpace.CSpace), new EntityTypeConfiguration(typeof(object)));

            Assert.NotNull(associationType.Constraint);
            Assert.Same(associationType.SourceEnd, associationType.Constraint.ToRole);
            Assert.Same(associationType.TargetEnd, associationType.Constraint.FromRole);
            Assert.True(associationType.Constraint.ToProperties.Any());
        }

        [Fact]
        public void Configure_should_configure_delete_action()
        {
            var mockType = new MockType();
            var navigationPropertyConfiguration = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N"))
                                                      {
                                                          DeleteAction = OperationAction.Cascade,
                                                      };
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));

            associationType.SourceEnd.GetEntityType().Annotations.SetClrType(mockType);
            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.Many; // make this the principal
            associationType.TargetEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;

            navigationPropertyConfiguration.Configure(
                new NavigationProperty("N", TypeUsage.Create(associationType.TargetEnd.GetEntityType()))
                    {
                        RelationshipType = associationType
                    },
                new EdmModel(DataSpace.CSpace), new EntityTypeConfiguration(typeof(object)));

            Assert.Equal(OperationAction.Cascade, associationType.TargetEnd.DeleteBehavior);
        }

        [Fact]
        public void Configure_should_configure_mapping()
        {
            var manyToManyAssociationMappingConfiguration = new ManyToManyAssociationMappingConfiguration();
            manyToManyAssociationMappingConfiguration.ToTable("Foo");

            var mockPropertyInfo = new MockPropertyInfo(new MockType(), "N");

            var navigationPropertyConfiguration
                = new NavigationPropertyConfiguration(mockPropertyInfo)
                      {
                          AssociationMappingConfiguration = manyToManyAssociationMappingConfiguration
                      };

            var databaseMapping
                = new DbDatabaseMapping()
                    .Initialize(new EdmModel(DataSpace.CSpace), new EdmModel(DataSpace.SSpace));

            var associationSetMapping = databaseMapping.AddAssociationSetMapping(
                new AssociationSet("AS", new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)), new EntitySet());

            var dependentTable = databaseMapping.Database.AddTable("T");

            associationSetMapping.StoreEntitySet = databaseMapping.Database.GetEntitySet(dependentTable);
            associationSetMapping.AssociationSet.ElementType.SetConfiguration(navigationPropertyConfiguration);

            associationSetMapping.SourceEndMapping.EndMember = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationSetMapping.SourceEndMapping.EndMember.SetClrPropertyInfo(mockPropertyInfo);

            navigationPropertyConfiguration.Configure(associationSetMapping, databaseMapping, ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal("Foo", associationSetMapping.Table.GetTableName().Name);
        }

        [Fact]
        public void Configure_should_configure_function_mapping()
        {
            var functionsConfiguration = new ModificationFunctionsConfiguration();
            var functionConfiguration = new ModificationFunctionConfiguration();
            functionConfiguration.HasName("Func");
            functionsConfiguration.Insert(functionConfiguration);

            var mockPropertyInfo = new MockPropertyInfo(new MockType(), "N");

            var navigationPropertyConfiguration
                = new NavigationPropertyConfiguration(mockPropertyInfo)
                      {
                          ModificationFunctionsConfiguration = functionsConfiguration
                      };

            var databaseMapping
                = new DbDatabaseMapping()
                    .Initialize(new EdmModel(DataSpace.CSpace), new EdmModel(DataSpace.SSpace));

            var associationType
                = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace)
                      {
                          SourceEnd =
                              new AssociationEndMember(
                              "S",
                              new EntityType(
                              "E1", "N", DataSpace.CSpace)),
                          TargetEnd =
                              new AssociationEndMember(
                              "T",
                              new EntityType(
                              "E2", "N", DataSpace.CSpace))
                      };

            associationType.SourceEnd.SetClrPropertyInfo(mockPropertyInfo);

            var associationSetMapping
                = databaseMapping.AddAssociationSetMapping(new AssociationSet("AS", associationType), new EntitySet());

            var dependentTable = databaseMapping.Database.AddTable("T");

            associationSetMapping.StoreEntitySet = databaseMapping.Database.GetEntitySet(dependentTable);
            associationSetMapping.AssociationSet.ElementType.SetConfiguration(navigationPropertyConfiguration);

            associationSetMapping.SourceEndMapping.EndMember = associationType.SourceEnd;

            navigationPropertyConfiguration.Configure(associationSetMapping, databaseMapping, ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal("Func", associationSetMapping.ModificationFunctionMapping.InsertFunctionMapping.Function.StoreFunctionNameAttribute);
        }

        [Fact]
        public void Configure_should_validate_consistency_of_end_kind_when_already_configured()
        {
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));
            var navigationPropertyConfigurationA
                = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N1"))
                      {
                          InverseEndKind = RelationshipMultiplicity.ZeroOrOne
                      };
            associationType.SetConfiguration(navigationPropertyConfigurationA);
            var navigationPropertyConfigurationB
                = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N2"))
                      {
                          RelationshipMultiplicity = RelationshipMultiplicity.Many
                      };

            Assert.Equal(
                Strings.ConflictingMultiplicities("N2", typeof(object)),
                Assert.Throws<InvalidOperationException>(
                    () => navigationPropertyConfigurationB.Configure(
                        new NavigationProperty("N", TypeUsage.Create(associationType.TargetEnd.GetEntityType()))
                            {
                                RelationshipType = associationType
                            },
                        new EdmModel(DataSpace.CSpace), new EntityTypeConfiguration(typeof(object)))).Message);
        }

        [Fact]
        public void Configure_should_validate_consistency_of_inverse_end_kind_when_already_configured()
        {
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));
            var mockPropertyInfo = new MockPropertyInfo(new MockType(), "N1");
            var navigationPropertyConfigurationA
                = new NavigationPropertyConfiguration(mockPropertyInfo)
                      {
                          RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne
                      };
            associationType.SetConfiguration(navigationPropertyConfigurationA);
            var navigationPropertyConfigurationB
                = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N2"))
                      {
                          InverseEndKind = RelationshipMultiplicity.Many,
                          InverseNavigationProperty = mockPropertyInfo
                      };

            Assert.Equal(
                Strings.ConflictingMultiplicities("N1", typeof(object)),
                Assert.Throws<InvalidOperationException>(
                    () => navigationPropertyConfigurationB.Configure(
                        new NavigationProperty("N", TypeUsage.Create(associationType.TargetEnd.GetEntityType()))
                            {
                                RelationshipType = associationType
                            },
                        new EdmModel(DataSpace.CSpace), new EntityTypeConfiguration(typeof(object)))).Message);
        }

        [Fact]
        public void Configure_should_validate_consistency_of_delete_action_when_already_configured()
        {
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));
            var navigationPropertyConfigurationA
                = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N1"))
                      {
                          DeleteAction = OperationAction.None
                      };
            associationType.SetConfiguration(navigationPropertyConfigurationA);
            var navigationPropertyConfigurationB
                = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N2"))
                      {
                          DeleteAction = OperationAction.Cascade
                      };

            Assert.Equal(
                Strings.ConflictingCascadeDeleteOperation("N2", typeof(object)),
                Assert.Throws<InvalidOperationException>(
                    () => navigationPropertyConfigurationB.Configure(
                        new NavigationProperty("N", TypeUsage.Create(associationType.TargetEnd.GetEntityType()))
                            {
                                RelationshipType = associationType
                            },
                        new EdmModel(DataSpace.CSpace), new EntityTypeConfiguration(typeof(object)))).Message);
        }

        [Fact]
        public void Configure_should_validate_consistency_of_constraint_when_already_configured()
        {
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));
            var navigationPropertyConfigurationA
                = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N1"))
                      {
                          Constraint = new ForeignKeyConstraintConfiguration(
                              new[]
                                  {
                                      new MockPropertyInfo(typeof(int), "P1").Object
                                  })
                      };
            associationType.SetConfiguration(navigationPropertyConfigurationA);
            var navigationPropertyConfigurationB
                = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N2"))
                      {
                          Constraint = new ForeignKeyConstraintConfiguration(
                              new[]
                                  {
                                      new MockPropertyInfo(typeof(int), "P2").Object
                                  })
                      };

            Assert.Equal(
                Strings.ConflictingConstraint("N2", typeof(object)),
                Assert.Throws<InvalidOperationException>(
                    () => navigationPropertyConfigurationB.Configure(
                        new NavigationProperty("N", TypeUsage.Create(associationType.TargetEnd.GetEntityType()))
                            {
                                RelationshipType = associationType
                            },
                        new EdmModel(DataSpace.CSpace), new EntityTypeConfiguration(typeof(object)))).Message);
        }

        [Fact]
        public void Configure_should_validate_consistency_of_mapping_when_already_configured()
        {
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));
            var manyToManyAssociationMappingConfiguration1 = new ManyToManyAssociationMappingConfiguration();
            manyToManyAssociationMappingConfiguration1.ToTable("A");
            var navigationPropertyConfigurationA
                = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N1"))
                      {
                          AssociationMappingConfiguration = manyToManyAssociationMappingConfiguration1
                      };
            associationType.SetConfiguration(navigationPropertyConfigurationA);
            var manyToManyAssociationMappingConfiguration2 = new ManyToManyAssociationMappingConfiguration();
            manyToManyAssociationMappingConfiguration1.ToTable("B");
            var navigationPropertyConfigurationB
                = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N2"))
                      {
                          AssociationMappingConfiguration = manyToManyAssociationMappingConfiguration2
                      };

            Assert.Equal(
                Strings.ConflictingMapping("N2", typeof(object)),
                Assert.Throws<InvalidOperationException>(
                    () => navigationPropertyConfigurationB.Configure(
                        new NavigationProperty("N", TypeUsage.Create(associationType.TargetEnd.GetEntityType()))
                            {
                                RelationshipType = associationType
                            },
                        new EdmModel(DataSpace.CSpace), new EntityTypeConfiguration(typeof(object)))).Message);
        }

        [Fact]
        public void Configure_should_validate_consistency_of_function_configuration_when_already_configured()
        {
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));

            var functionConfiguration1 = new ModificationFunctionConfiguration();
            functionConfiguration1.HasName("Foo");

            var functionConfiguration2 = new ModificationFunctionConfiguration();
            functionConfiguration2.HasName("Bar");

            var functionsConfiguration1 = new ModificationFunctionsConfiguration();

            functionsConfiguration1.Insert(functionConfiguration1);

            var functionsConfiguration2 = new ModificationFunctionsConfiguration();

            functionsConfiguration2.Insert(functionConfiguration2);

            var navigationPropertyConfigurationA
                = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N1"))
                      {
                          ModificationFunctionsConfiguration = functionsConfiguration1
                      };

            associationType.SetConfiguration(navigationPropertyConfigurationA);

            var navigationPropertyConfigurationB
                = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N2"))
                      {
                          ModificationFunctionsConfiguration = functionsConfiguration2
                      };

            Assert.Equal(
                Strings.ConflictingFunctionsMapping("N2", typeof(object)),
                Assert.Throws<InvalidOperationException>(
                    () => navigationPropertyConfigurationB.Configure(
                        new NavigationProperty("N", TypeUsage.Create(associationType.TargetEnd.GetEntityType()))
                            {
                                RelationshipType = associationType
                            }, new EdmModel(DataSpace.CSpace), new EntityTypeConfiguration(typeof(object)))).Message);
        }

        [Fact]
        public void Configure_should_not_validate_consistency_of_dependent_end_when_both_false()
        {
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));
            var navigationPropertyConfigurationA
                = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N"));
            associationType.SetConfiguration(navigationPropertyConfigurationA);
            var navigationPropertyConfigurationB
                = new NavigationPropertyConfiguration(new MockPropertyInfo(new MockType(), "N"));

            navigationPropertyConfigurationB.Configure(
                new NavigationProperty("N", TypeUsage.Create(associationType.TargetEnd.GetEntityType()))
                    {
                        RelationshipType = associationType
                    },
                new EdmModel(DataSpace.CSpace), new EntityTypeConfiguration(typeof(object)));
        }
    }
}
