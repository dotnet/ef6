// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.CodeFirst
{
    using FunctionalTests.Model;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public class PropertyConfigurationScenarioTests : TestBase
    {
        #region ConfigureKey

        [Fact]
        public void Configure_configure_key_on_property_index_named()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .ConfigureKey()
                .HasName("PK_Foo_Bar");

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttribute = ConfiguredPrimaryKeyIndexAttribute(model, "Customer");

                indexAttribute.AssertConfiguration("PK_Foo_Bar", null, null, null);
            }
        }

        [Fact]
        public void Configure_configure_key_on_property_index_non_clustered()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .ConfigureKey()
                .IsClustered(false);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttribute = ConfiguredPrimaryKeyIndexAttribute(model, "Customer");

                indexAttribute.AssertConfiguration(null, null, null, false);
            }
        }

        [Fact]
        public void Configure_configure_key_on_property_index_clustered()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .ConfigureKey()
                .IsClustered(true);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttribute = ConfiguredPrimaryKeyIndexAttribute(model, "Customer");

                indexAttribute.AssertConfiguration(null, null, null, true);
            }
        }

        #endregion

        #region HasKey

        #region Single Property

        [Fact]
        public void Configure_has_key_on_property_creates_index()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasKey(e => e.CustomerID);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttribute = ConfiguredPrimaryKeyIndexAttribute(model, "Customer");

                indexAttribute.AssertConfiguration(null, null, null, null);
            }
        }

        [Fact]
        public void Configure_has_key_on_property_index_named()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasKey(e => e.CustomerID)
                .ConfigureKey()
                .HasName("PK_Foo_Bar");

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttribute = ConfiguredPrimaryKeyIndexAttribute(model, "Customer");

                indexAttribute.AssertConfiguration("PK_Foo_Bar", null, null, null);
            }
        }

        [Fact]
        public void Configure_has_key_on_property_index_non_clustered()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasKey(e => e.CustomerID)
                .ConfigureKey()
                .IsClustered(false);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttribute = ConfiguredPrimaryKeyIndexAttribute(model, "Customer");

                indexAttribute.AssertConfiguration(null, null, null, false);
            }
        }

        [Fact]
        public void Configure_has_key_on_property_index_clustered()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasKey(e => e.CustomerID)
                .ConfigureKey()
                .IsClustered(true);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttribute = ConfiguredPrimaryKeyIndexAttribute(model, "Customer");

                indexAttribute.AssertConfiguration(null, null, null, true);
            }
        }

        #endregion

        #region Multiple Properties

        [Fact]
        public void Configure_has_key_on_multiple_properties_creates_index()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasKey(e => new { e.CustomerID, e.AccountNumber });

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttribute = ConfiguredPrimaryKeyIndexAttribute(model, "Customer");

                indexAttribute.AssertConfiguration(null, null, null, null);
            }
        }

        [Fact]
        public void Configure_has_key_on_multiple_properties_index_named()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasKey(e => new { e.AccountNumber, e.CustomerID })
                .ConfigureKey()
                .HasName("PK_Foo_Bar");

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttribute = ConfiguredPrimaryKeyIndexAttribute(model, "Customer");

                indexAttribute.AssertConfiguration("PK_Foo_Bar", null, null, null);
            }
        }
        

        [Fact]
        public void Configure_has_key_on_multiple_properties_index_non_clustered()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasKey(e => new { e.AccountNumber, e.CustomerID })
                .ConfigureKey()
                .IsClustered(false);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttribute = ConfiguredPrimaryKeyIndexAttribute(model, "Customer");

                indexAttribute.AssertConfiguration(null, null, null, false);
            }
        }

        [Fact]
        public void Configure_has_key_on_multiple_properties_index_clustered()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasKey(e => new { e.AccountNumber, e.CustomerID })
                .ConfigureKey()
                .IsClustered(true);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttribute = ConfiguredPrimaryKeyIndexAttribute(model, "Customer");

                indexAttribute.AssertConfiguration(null, null, null, true);
            }
        }

        #endregion

        #endregion

        #region HasIndex

        #region Single Property

        [Fact]
        public void Configure_has_index_on_property_creates_index()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasIndex(e => e.CustomerID);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerID");
                Assert.Single(indexAttributes);
                
                indexAttributes.Single().AssertConfiguration(null, null, null, null);
            }
        }

        [Fact]
        public void Configure_has_index_on_property_index_named()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasIndex(e => e.CustomerID)
                    .HasName("IX_Foo_Bar");

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerID");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration("IX_Foo_Bar", null, null, null);
            }
        }

        [Fact]
        public void Configure_has_index_on_property_index_ordered()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasIndex(e => e.CustomerID);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerID");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, -1, null, null);
            }
        }

        [Fact]
        public void Configure_has_index_on_property_index_non_unique()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasIndex(e => e.CustomerID)
                    .IsUnique(false);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerID");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, null, false, null);
            }
        }

        [Fact]
        public void Configure_has_index_on_property_index_unique()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasIndex(e => e.CustomerID)
                    .IsUnique(true);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerID");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, null, true, null);
            }
        }

        [Fact]
        public void Configure_has_index_on_property_index_non_clustered()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasIndex(e => e.CustomerID)
                    .IsClustered(false);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerID");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, null, null, false);
            }
        }

        [Fact]
        public void Configure_has_index_on_property_index_clustered()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasIndex(e => e.CustomerID)
                    .IsClustered(true);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerID");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, null, null, true);
            }
        }

        #endregion

        #region Multiple Properties
        
        [Fact]
        public void Configure_has_index_on_multiple_properties_creates_index()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasIndex(e => new { e.CustomerID, e.AccountNumber });

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerID");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, null, null, null);
            }

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "AccountNumber");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, null, null, null);
            }
        }

        [Fact]
        public void Configure_has_index_on_multiple_properties_index_named()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasIndex(e => new { e.CustomerID, e.AccountNumber })
                .HasName("IX_Foo_Bar");

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerID");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration("IX_Foo_Bar", null, null, null);
            }

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "AccountNumber");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration("IX_Foo_Bar", null, null, null);
            }
        }

        [Fact]
        public void Configure_has_index_on_multiple_properties_index_ordered()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasIndex(e => new { e.AccountNumber, e.CustomerID, e.CustomerType });

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerID");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, 1, null, null);
            }

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "AccountNumber");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, 0, null, null);
            }

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerType");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, 2, null, null);
            }
        }


        [Fact]
        public void Configure_has_index_on_multiple_properties_index_non_unique()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasIndex(e => new { e.AccountNumber, e.CustomerID })
                .IsUnique(false);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerID");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, null, false, null);
            }

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "AccountNumber");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, null, false, null);
            }
        }

        [Fact]
        public void Configure_has_index_on_multiple_properties_index_unique()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasIndex(e => new { e.AccountNumber, e.CustomerID })
                .IsUnique(true);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerID");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, null, true, null);
            }

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "AccountNumber");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, null, true, null);
            }
        }

        [Fact]
        public void Configure_has_index_on_multiple_properties_index_non_clustered()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasIndex(e => new { e.AccountNumber, e.CustomerID })
                .IsClustered(false);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerID");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, null, null, false);
            }

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "AccountNumber");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, null, null, false);
            }
        }

        [Fact]
        public void Configure_has_index_on_multiple_properties_index_clustered()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasIndex(e => new { e.AccountNumber, e.CustomerID })
                .IsClustered(true);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerID");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, null, null, true);
            }

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "AccountNumber");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, null, null, true);
            }
        }

        #endregion

        #endregion

        #region Multiple Indexes

        [Fact]
        public void Configure_has_key_has_index_creates_indexes()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasKey(e => e.CustomerID)
                .HasIndex(e => e.CustomerType);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            Assert.NotNull(
                ConfiguredPrimaryKeyIndexAttribute(model, "Customer"));

            var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerType");
            Assert.Single(indexAttributes);

            indexAttributes.Single().AssertConfiguration(null, null, null, null);
        }

        [Fact]
        public void Configure_multiple_has_index_creates_indexes()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasIndex(e => e.CustomerID);

            modelBuilder.Entity<Customer>()
                .HasIndex(e => e.CustomerType);

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerID");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, null, null, null);
            }

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerType");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, null, null, null);
            }
        }

        [Fact]
        public void Configure_has_key_has_index_creates_indexes_complex()
        {
            var modelBuilder = new AdventureWorksModelBuilder();

            modelBuilder.Entity<Customer>()
                .HasKey(e => new { e.CustomerID, e.AccountNumber })
                .ConfigureKey()
                .IsClustered(false);

            modelBuilder.Entity<Customer>()
                .HasIndex(e => new { e.CustomerType, e.rowguid })
                .IsUnique()
                .IsClustered();

            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            {
                var indexAttribute = ConfiguredPrimaryKeyIndexAttribute(model, "Customer");
                
                indexAttribute.AssertConfiguration(null, null, null, false);
            }

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "CustomerType");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, 0, true, true);
            }

            {
                var indexAttributes = ConfiguredIndexAttributes(model, "Customer", "rowguid");
                Assert.Single(indexAttributes);

                indexAttributes.Single().AssertConfiguration(null, 1, true, true);
            }
        }

        #endregion

        private IEnumerable<IndexAttribute> ConfiguredIndexAttributes(DbModel model, string typeName, string propertyName)
        {
            Assert.NotNull(model);
            Assert.NotNull(typeName);
            Assert.NotNull(propertyName);

            var entityType = model.StoreModel.EntityTypes.SingleOrDefault(et => et.Name == typeName);
            Assert.NotNull(entityType);

            var customerProperty = entityType.Properties.SingleOrDefault(p => p.Name == propertyName);
            Assert.NotNull(customerProperty);

            var indexAnnotationMetadataProperty = customerProperty.MetadataProperties
                .SingleOrDefault(mp => mp.IsAnnotation && mp.Name.EndsWith(":" + IndexAnnotation.AnnotationName));

            Assert.NotNull(indexAnnotationMetadataProperty);

            Assert.NotNull(indexAnnotationMetadataProperty.Value);
            Assert.IsType<IndexAnnotation>(indexAnnotationMetadataProperty.Value);

            var indexAnnotation = (IndexAnnotation)indexAnnotationMetadataProperty.Value;

            return indexAnnotation.Indexes;
        }

        private IndexAttribute ConfiguredPrimaryKeyIndexAttribute(DbModel model, string typeName)
        {
            Assert.NotNull(model);
            Assert.NotNull(typeName);

            var entityType = model.StoreModel.EntityTypes.SingleOrDefault(et => et.Name == typeName);
            Assert.NotNull(entityType);

            var indexAnnotationMetadataProperty = entityType.MetadataProperties
                .SingleOrDefault(mp => mp.IsAnnotation && mp.Name.EndsWith(":" + IndexAnnotation.AnnotationName));

            Assert.NotNull(indexAnnotationMetadataProperty);

            Assert.NotNull(indexAnnotationMetadataProperty.Value);
            Assert.IsType<IndexAnnotation>(indexAnnotationMetadataProperty.Value);

            var indexAnnotation = (IndexAnnotation) indexAnnotationMetadataProperty.Value;
            return Assert.Single(indexAnnotation.Indexes);
        }

    }


    public static class IndexAttributeTestExtensions
    {
        public static void AssertConfiguration(this IndexAttribute indexAttribute, string name, int? order, bool? isUnique, bool? isClustered)
        {
            Assert.Equal(name, indexAttribute.Name);
            

            if (order.HasValue)
            {
                Assert.Equal(order.Value, indexAttribute.Order);
            }
            

            if (isClustered.HasValue)
            {
                Assert.Equal(isClustered.Value, indexAttribute.IsClustered);
            }
            else 
            {
                Assert.False(indexAttribute.IsClusteredConfigured);
            }


            if (isUnique.HasValue)
            {
                Assert.Equal(isUnique.Value, indexAttribute.IsUnique);
            }
            else 
            {
                Assert.False(indexAttribute.IsUniqueConfigured);
            }

        }
    }
}
