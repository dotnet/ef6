// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Reflection;
    using AdvancedPatternsModel;
    using ConcurrencyModel;
    using SimpleModel;
    using Xunit;

    /// <summary>
    ///     Tests for the primary methods on DbContext.
    /// </summary>
    public class DbSetTests : FunctionalTestBase
    {
        #region Positive query tests

        [Fact]
        public void DbSet_acts_as_simple_query()
        {
            DbSet_acts_as_simple_query_implementation(c => c.Products);
        }

        [Fact]
        public void Non_generic_DbSet_acts_as_simple_query()
        {
            DbSet_acts_as_simple_query_implementation(c => c.Set(typeof(Product)).Cast<Product>());
        }

        private void DbSet_acts_as_simple_query_implementation(Func<SimpleModelContext, IQueryable<Product>> createQuery)
        {
            using (var context = new SimpleModelContext())
            {
                var query = createQuery(context);
                var results = query.ToList();

                Assert.IsAssignableFrom<IQueryable<Product>>(query);
                Assert.Equal(7, results.Count);
                Assert.True(results.TrueForAll(p => p is Product));
                Assert.True(results.TrueForAll(p => GetStateEntry(context, p).State == EntityState.Unchanged));
                Assert.Equal(7, GetStateEntries(context).Count());
            }
        }

        [Fact]
        public void DbSet_acts_as_simple_query_for_derived_type()
        {
            DbSet_acts_as_simple_query_for_derived_type_implementation(c => c.Set<FeaturedProduct>());
        }

        [Fact]
        public void Non_generic_DbSet_acts_as_simple_query_for_derived_type()
        {
            DbSet_acts_as_simple_query_for_derived_type_implementation(
                c => c.Set(typeof(FeaturedProduct)).Cast<FeaturedProduct>());
        }

        private void DbSet_acts_as_simple_query_for_derived_type_implementation(
            Func<SimpleModelContext, IQueryable<FeaturedProduct>> createQuery)
        {
            using (var context = new SimpleModelContext())
            {
                var query = createQuery(context);
                var results = query.ToList();

                Assert.IsAssignableFrom<IQueryable<FeaturedProduct>>(query);
                Assert.Equal(1, results.Count);
                Assert.True(results.TrueForAll(p => p is FeaturedProduct));
                Assert.True(results.TrueForAll(p => GetStateEntry(context, p).State == EntityState.Unchanged));
                Assert.Equal(1, GetStateEntries(context).Count());
            }
        }

        [Fact]
        public void Non_generic_DbSet_can_be_enumerated_without_using_Cast()
        {
            using (var context = new SimpleModelContext())
            {
                var query = context.Set(typeof(Product));

                Assert.IsAssignableFrom<IQueryable>(query);

                var count = 0;
                foreach (var product in query)
                {
                    count++;
                    Assert.IsAssignableFrom<Product>(product);
                    Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
                }
                Assert.Equal(7, count);
            }
        }

        [Fact]
        public void Load_can_be_used_to_load_DbSet()
        {
            Load_can_be_used_to_load_DbSet_implementation(c => c.Products.Load());
        }

        [Fact]
        public void Load_can_be_used_to_load_non_generic_DbSet()
        {
            Load_can_be_used_to_load_DbSet_implementation(c => c.Set(typeof(Product)).Load());
        }

#if !NET40

        [Fact]
        public void LoadAsync_can_be_used_to_load_DbSet()
        {
            Load_can_be_used_to_load_DbSet_implementation(c => c.Products.LoadAsync().Wait());
        }

        [Fact]
        public void LoadAsync_can_be_used_to_load_non_generic_DbSet()
        {
            Load_can_be_used_to_load_DbSet_implementation(c => c.Set(typeof(Product)).LoadAsync().Wait());
        }

#endif

        private void Load_can_be_used_to_load_DbSet_implementation(Action<SimpleModelContext> loadProducts)
        {
            using (var context = new SimpleModelContext())
            {
                loadProducts(context);

                Assert.Equal(7, context.Products.Local.Count);
                Assert.Equal(7, GetStateEntries(context).Count());
            }
        }

        #endregion

        #region Positive Add tests

        [Fact]
        public void Add_adds_entity_to_context()
        {
            using (var context = new SimpleModelContext())
            {
                var addedProduct = new Product
                                       {
                                           Id = -1,
                                           Name = "Daddies Sauce"
                                       };
                context.Products.Add(addedProduct);

                VerifyProduct(context, addedProduct, EntityState.Added);
            }
        }

        [Fact]
        public void Non_generic_Add_adds_entity_to_context()
        {
            using (var context = new SimpleModelContext())
            {
                var addedProduct = new Product
                                       {
                                           Id = -1,
                                           Name = "Daddies Sauce"
                                       };
                context.Set(typeof(Product)).Add(addedProduct);

                VerifyProduct(context, addedProduct, EntityState.Added);
            }
        }

        [Fact]
        public void Add_on_non_derived_set_adds_derived_entity_to_context()
        {
            using (var context = new SimpleModelContext())
            {
                var addedProduct = new FeaturedProduct
                                       {
                                           Id = -1,
                                           Name = "Salad Cream"
                                       };
                context.Products.Add(addedProduct);

                VerifyProduct(context, addedProduct, EntityState.Added);
            }
        }

        [Fact]
        public void Add_on_non_derived_non_generic_set_adds_derived_entity_to_context()
        {
            using (var context = new SimpleModelContext())
            {
                var addedProduct = new FeaturedProduct
                                       {
                                           Id = -1,
                                           Name = "Salad Cream"
                                       };
                context.Set(typeof(Product)).Add(addedProduct);

                VerifyProduct(context, addedProduct, EntityState.Added);
            }
        }

        [Fact]
        public void Add_on_derived_set_adds_derived_entity_to_context()
        {
            using (var context = new SimpleModelContext())
            {
                var addedProduct = new FeaturedProduct
                                       {
                                           Id = -1,
                                           Name = "Piccalilli"
                                       };
                context.Set<FeaturedProduct>().Add(addedProduct);

                VerifyProduct(context, addedProduct, EntityState.Added);
            }
        }

        [Fact]
        public void Add_on_derived_non_generic_set_adds_derived_entity_to_context()
        {
            using (var context = new SimpleModelContext())
            {
                var addedProduct = new FeaturedProduct
                                       {
                                           Id = -1,
                                           Name = "Piccalilli"
                                       };
                context.Set(typeof(FeaturedProduct)).Add(addedProduct);

                VerifyProduct(context, addedProduct, EntityState.Added);
            }
        }

        [Fact]
        public void Add_is_noop_if_Added_entity_already_exists()
        {
            Add_moves_entity_from_other_state_to_Added(EntityState.Added);
        }

        [Fact]
        public void Add_moves_Deleted_entity_to_Added()
        {
            Add_moves_entity_from_other_state_to_Added(EntityState.Deleted);
        }

        [Fact]
        public void Add_moves_Unchanged_entity_to_Added()
        {
            Add_moves_entity_from_other_state_to_Added(EntityState.Unchanged);
        }

        [Fact]
        public void Add_moves_Modified_entity_to_Added()
        {
            Add_moves_entity_from_other_state_to_Added(EntityState.Modified);
        }

        [Fact]
        public void Non_generic_Add_is_noop_if_Added_entity_already_exists()
        {
            Non_generic_Add_moves_entity_from_other_state_to_Added(EntityState.Added);
        }

        [Fact]
        public void Non_generic_Add_moves_Deleted_entity_to_Added()
        {
            Non_generic_Add_moves_entity_from_other_state_to_Added(EntityState.Deleted);
        }

        [Fact]
        public void Non_generic_Add_moves_Unchanged_entity_to_Added()
        {
            Non_generic_Add_moves_entity_from_other_state_to_Added(EntityState.Unchanged);
        }

        [Fact]
        public void Non_generic_Add_moves_Modified_entity_to_Added()
        {
            Non_generic_Add_moves_entity_from_other_state_to_Added(EntityState.Modified);
        }

        [Fact]
        public void Add_moves_root_to_Added_with_conflicts_in_the_leaf_node()
        {
            using (var context = new SimpleModelContext())
            {
                var product = new Product
                                  {
                                      Id = 0,
                                      Name = "Red Bull"
                                  };
                context.Products.Attach(product);
                var category = new Category
                                   {
                                       Id = "Beverages",
                                       Products = new List<Product>
                                                      {
                                                          product
                                                      }
                                   };

                Assert.Equal(null, product.Category);
                Assert.Equal(null, product.CategoryId);
                context.Categories.Add(category);

                Assert.Equal(EntityState.Modified, GetStateEntry(context, product).State);
                Assert.Equal(EntityState.Added, GetStateEntry(context, category).State);
                var value2 = product.CategoryId;
                Assert.Equal(product.Category, context.Categories.Find("Beverages"));

                // FK Is still null here because we don't use PK values from Added entities during
                // fixup of FKs on Add.
                Assert.Null(product.CategoryId);

                GetObjectContext(context).AcceptAllChanges();
                Assert.Equal("Beverages", product.CategoryId);
            }
        }

        [Fact]
        public void Add_moves_Detached_Evicted_entity_to_Added_Sanity_test()
        {
            Add_moves_Detached_Evicted_entity_to_Added_Sanity_test_implementation((c, p) => c.Products.Add(p));
        }

        [Fact]
        public void Non_generic_Add_moves_Detached_Evicted_entity_to_Added_Sanity_test()
        {
            Add_moves_Detached_Evicted_entity_to_Added_Sanity_test_implementation(
                (c, p) => c.Set(typeof(Product)).Add(p));
        }

        private void Add_moves_Detached_Evicted_entity_to_Added_Sanity_test_implementation(
            Action<SimpleModelContext, Product> performAdd)
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var addedProduct = new Product
                                       {
                                           Id = -1,
                                           Name = "Marmite"
                                       };
                context.Products.Add(addedProduct);
                GetObjectContext(context).Detach(addedProduct);

                // Asserting Detached state
                Assert.Null(context.Products.Find(-1));

                // Act
                context.Products.Add(addedProduct);

                // Assert
                VerifyProduct(context, addedProduct, EntityState.Added);
            }
        }

        [Fact]
        public void Add_Attach_or_Remove_does_not_throw_for_string_key_Sanity_test()
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var category = new Category
                                   {
                                       Id = "Something"
                                   };

                // Act- Assert
                context.Categories.Add(category);
                Assert.Equal(1, GetStateEntries(context).Count());
                Assert.Equal(EntityState.Added, GetStateEntry(context, category).State);

                // Act- Assert
                context.Categories.Attach(category);
                Assert.Equal(1, GetStateEntries(context).Count());
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, category).State);

                // Act- Assert
                context.Categories.Remove(category);
                Assert.Equal(1, GetStateEntries(context).Count());
                Assert.Equal(EntityState.Deleted, GetStateEntry(context, category).State);

                // Act- Assert
                Assert.Equal(1, GetStateEntries(context).Count());
                Assert.Equal(context.Categories.Find("Something"), category);
            }
        }

        [Fact]
        public void Add_Attach_or_Remove_does_not_throw_for_Binary_keys_Sanity_test()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                // Arrange
                var binaryKey = new byte[] { 20, 21, 22, 23, 24 };
                var whiteBoard = new Whiteboard
                                     {
                                         iD = binaryKey,
                                         AssetTag = "First Board in my Office"
                                     };

                // Act- Assert
                context.Whiteboards.Add(whiteBoard);
                Assert.Equal(EntityState.Added, GetStateEntry(context, whiteBoard).State);
                Assert.Equal(1, GetStateEntries(context).Count());

                // Act- Assert
                context.Whiteboards.Attach(whiteBoard);
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, whiteBoard).State);
                Assert.Equal(1, GetStateEntries(context).Count());

                // Act- Assert
                context.Whiteboards.Remove(whiteBoard);
                Assert.Equal(EntityState.Deleted, GetStateEntry(context, whiteBoard).State);
                Assert.Equal(1, GetStateEntries(context).Count());

                // Assert
                Assert.Same(whiteBoard, context.Whiteboards.Find(binaryKey));
                Assert.Equal(1, GetStateEntries(context).Count());
            }
        }

        // Adding conflict free graph into the context
        [Fact]
        public void Add_moves_Detached_FK_graph_to_Added()
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var category = new Category
                                   {
                                       Id = "Beverages"
                                   };
                var product = new Product
                                  {
                                      Id = 0,
                                      Name = "Red Bull"
                                  };
                category.Products.Add(product);

                // Act
                context.Categories.Add(category);

                // Assert
                Assert.Equal(EntityState.Added, GetStateEntry(context, category).State);
                Assert.Equal(EntityState.Added, GetStateEntry(context, product).State);

                // Assert fixup, refernce fixup happens for principal in Added, but not FK fixup.
                Assert.Equal(product.Category, context.Categories.Find("Beverages"));
                Assert.Equal(null, product.CategoryId);
            }
        }

        #endregion

        #region Conficts at root level for FK graph

        [Fact]
        public void Add_is_a_noop_if_FK_graph_is_already_Added_Principal_and_Dependent_Added()
        {
            Add_moves_root_to_Added_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Added,
                EntityState.Added);
        }

        [Fact]
        public void Add_is_a_noop_if_FK_graph_has_Added_Principal_and_Dependent_Unchanged()
        {
            Add_moves_root_to_Added_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Added,
                EntityState.Unchanged);
        }

        [Fact]
        public void Add_is_a_noop_if_FK_graph_is_already_Added_Prinicipal_and_Dependent_Modified()
        {
            Add_moves_root_to_Added_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Added,
                EntityState.Modified);
        }

        [Fact]
        public void Add_is_a_noop_if_FK_graph_is_already_Added_Prinicipal_and_Dependent_Deleted()
        {
            Add_moves_root_to_Added_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Added,
                EntityState.Deleted);
        }

        [Fact]
        public void Add_moves_graph_to_Added_with_Unchanged_Principal_and_Dependent_Added()
        {
            Add_moves_root_to_Added_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged,
                EntityState.Added);
        }

        [Fact]
        public void Add_moves_graph_to_Added_with_Unchanged_Principal_and_Dependent_Unchanged()
        {
            Add_moves_root_to_Added_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged,
                EntityState.Unchanged);
        }

        [Fact]
        public void Add_moves_graph_to_Added_with_Unchanged_Principal_and_Dependent_Modified()
        {
            Add_moves_root_to_Added_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged,
                EntityState.Modified);
        }

        [Fact]
        public void Add_moves_graph_to_Added_with_Unchanged_Principal_and_Dependent_Deleted()
        {
            Add_moves_root_to_Added_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged,
                EntityState.Deleted);
        }

        [Fact]
        public void Add_moves_graph_to_Added_with_Modified_Principal_and_Dependent_Added()
        {
            Add_moves_root_to_Added_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Modified,
                EntityState.Added);
        }

        [Fact]
        public void Add_moves_graph_to_Added_with_Modified_Principal_and_Dependent_Unchanged()
        {
            Add_moves_root_to_Added_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Modified,
                EntityState.Unchanged);
        }

        [Fact]
        public void Add_moves_graph_to_Added_with_Modified_Principal_and_Dependent_Modified()
        {
            Add_moves_root_to_Added_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Modified,
                EntityState.Modified);
        }

        [Fact]
        public void Add_moves_graph_to_Added_with_Modified_Principal_and_Dependent_Deleted()
        {
            Add_moves_root_to_Added_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Modified,
                EntityState.Deleted);
        }

        [Fact]
        public void Add_moves_graph_to_Added_with_Deleted_Principal_and_Dependent_Added()
        {
            Add_moves_root_to_Added_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Deleted,
                EntityState.Added);
        }

        [Fact]
        public void Add_moves_graph_to_Added_with_Deleted_Principal_and_Dependent_Unchanged()
        {
            Add_moves_root_to_Added_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Deleted,
                EntityState.Unchanged);
        }

        [Fact]
        public void Add_moves_graph_to_Added_with_Deleted_Principal_and_Dependent_Modified()
        {
            Add_moves_root_to_Added_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Deleted,
                EntityState.Modified);
        }

        [Fact]
        public void Add_moves_graph_to_Added_with_Deleted_Principal_and_Dependent_Deleted()
        {
            Add_moves_root_to_Added_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Deleted,
                EntityState.Deleted);
        }

        private void Add_moves_root_to_Added_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
            EntityState principalState, EntityState dependentState)
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var category = new Category
                                   {
                                       Id = "Beverages"
                                   };
                var product = new Product
                                  {
                                      Id = 0,
                                      Name = "Red Bull",
                                      CategoryId = "Beverages"
                                  };
                category.Products.Add(product);

                switch (principalState)
                {
                    case EntityState.Added:
                        context.Categories.Add(category);
                        break;
                    case EntityState.Unchanged:
                        context.Categories.Attach(category);
                        break;
                    case EntityState.Modified:
                        context.Categories.Attach(category);
                        category.DetailedDescription += "Non-Alcoholic Beverages";
                        break;
                    case EntityState.Deleted:
                        context.Categories.Attach(category);
                        context.Categories.Remove(category);
                        break;
                    default:
                        Assert.True(false, "Invalid Principal State " + principalState);
                        break;
                }

                switch (dependentState)
                {
                    case EntityState.Added:
                        context.Products.Add(product);
                        break;
                    case EntityState.Unchanged:
                        context.Products.Attach(product);
                        break;
                    case EntityState.Deleted:
                        context.Products.Attach(product);
                        context.Products.Remove(product);
                        break;
                    case EntityState.Modified:
                        context.Products.Attach(product);
                        product.Name += "Caffeine drink";
                        break;
                    default:
                        Assert.True(false, "Invalid Dependent state " + dependentState);
                        break;
                }

                // Assert states
                if (principalState != EntityState.Modified)
                {
                    Assert.Equal(principalState, GetStateEntry(context, category).State);
                }

                if (dependentState != EntityState.Modified)
                {
                    Assert.Equal(dependentState, GetStateEntry(context, product).State);
                }

                // Assert fixup based on principal state
                if (principalState == EntityState.Deleted
                    || dependentState == EntityState.Deleted)
                {
                    Assert.Null(product.Category);
                    Assert.Equal(0, category.Products.Count);
                }
                else
                {
                    Assert.Same(product.Category, context.Categories.Find("Beverages"));
                    Assert.Equal(1, category.Products.Count);
                    Assert.True(category.Products.Contains(product));
                }

                // Act
                context.Categories.Add(category);

                // Assert
                Assert.Equal(EntityState.Added, GetStateEntry(context, category).State);

                if (principalState == EntityState.Modified)
                {
                    Assert.Equal(GetStateEntry(context, category).GetModifiedProperties(), new string[0]);
                }
                else if (principalState == EntityState.Deleted)
                {
                    Assert.Equal("Beverages", GetStateEntry(context, category).CurrentValues["Id"]);
                    Assert.Equal(DBNull.Value, GetStateEntry(context, category).CurrentValues["DetailedDescription"]);
                }

                if (dependentState == EntityState.Modified)
                {
                    Assert.Equal(EntityState.Modified, GetStateEntry(context, product).State);
                }

                // Assert fixup
                if (principalState == EntityState.Deleted
                    || dependentState == EntityState.Deleted)
                {
                    Assert.Null(product.Category);
                    Assert.Equal(0, category.Products.Count);
                }
                else
                {
                    Assert.Same(product.Category, context.Categories.Find("Beverages"));
                    Assert.Equal(1, category.Products.Count);
                    Assert.True(category.Products.Contains(product));
                }
            }
        }

        #endregion

        #region Conflicts at root level for Independent Association Graphs

        [Fact]
        public void Add_moves_independent_graph_to_Added_with_Added_Principal_and_Dependent_Added()
        {
            Add_moves_root_to_Added_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Added, EntityState.Added);
        }

        [Fact]
        public void Add_moves_independent_graph_to_Added_with_Added_Principal_and_Dependent_Unchanged()
        {
            Add_moves_root_to_Added_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Added, EntityState.Unchanged);
        }

        [Fact]
        public void Add_moves_independent_graph_to_Added_with_Added_Principal_and_Dependent_Modified()
        {
            Add_moves_root_to_Added_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Added, EntityState.Modified);
        }

        [Fact]
        public void Add_moves_independent_graph_to_Added_with_Added_Principal_and_Dependent_Deleted()
        {
            Add_moves_root_to_Added_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Added, EntityState.Deleted);
        }

        [Fact]
        public void Add_moves_independent_graph_to_Added_with_Unchanged_Principal_and_Dependent_Added()
        {
            Add_moves_root_to_Added_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged, EntityState.Added);
        }

        [Fact]
        public void Add_moves_independent_graph_to_Added_with_Unchanged_Principal_and_Dependent_Unchanged()
        {
            Add_moves_root_to_Added_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged, EntityState.Unchanged);
        }

        [Fact]
        public void Add_moves_independent_graph_to_Added_with_Unchanged_Principal_and_Dependent_Modified()
        {
            Add_moves_root_to_Added_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged, EntityState.Modified);
        }

        [Fact]
        public void Add_moves_independent_graph_to_Added_with_Unchanged_Principal_and_Dependent_Deleted()
        {
            Add_moves_root_to_Added_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged, EntityState.Deleted);
        }

        [Fact]
        public void Add_moves_independent_graph_to_Added_with_Modified_Principal_and_Dependent_Added()
        {
            Add_moves_root_to_Added_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Modified, EntityState.Added);
        }

        [Fact]
        public void Add_moves_independent_graph_to_Added_with_Modified_Principal_and_Dependent_Unchanged()
        {
            Add_moves_root_to_Added_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Modified, EntityState.Unchanged);
        }

        [Fact]
        public void Add_moves_independent_graph_to_Added_with_Modified_Principal_and_Dependent_Modified()
        {
            Add_moves_root_to_Added_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Modified, EntityState.Modified);
        }

        [Fact]
        public void Add_moves_independent_graph_to_Added_with_Modified_Principal_and_Dependent_Deleted()
        {
            Add_moves_root_to_Added_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Modified, EntityState.Deleted);
        }

        [Fact]
        public void Add_moves_independent_graph_to_Added_with_Deleted_Principal_and_Dependent_Added()
        {
            Add_moves_root_to_Added_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Deleted, EntityState.Added);
        }

        [Fact]
        public void Add_moves_independent_graph_to_Added_with_Deleted_Principal_and_Dependent_Unchanged()
        {
            Add_moves_root_to_Added_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Deleted, EntityState.Unchanged);
        }

        [Fact]
        public void Add_moves_independent_graph_to_Added_with_Deleted_Principal_and_Dependent_Modified()
        {
            Add_moves_root_to_Added_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Deleted, EntityState.Modified);
        }

        [Fact]
        public void Add_moves_independent_graph_to_Added_with_Deleted_Principal_and_Dependent_Deleted()
        {
            Add_moves_root_to_Added_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Deleted, EntityState.Deleted);
        }

        private void
            Add_moves_root_to_Added_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted(
            EntityState principalState, EntityState dependentState)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                // Arrange 
                var office = new Office
                                 {
                                     BuildingId = Guid.NewGuid(),
                                     Number = "18/1111"
                                 };
                var whiteBoard = new Whiteboard
                                     {
                                         iD = new byte[] { 1, 2, 3, 4 },
                                         AssetTag = "ABCDX0009"
                                     };
                office.WhiteBoards.Add(whiteBoard);

                switch (principalState)
                {
                    case EntityState.Added:
                        context.Offices.Add(office);
                        break;
                    case EntityState.Unchanged:
                        context.Offices.Attach(office);
                        break;
                    case EntityState.Deleted:
                        context.Offices.Attach(office);
                        context.Offices.Remove(office);
                        break;
                    case EntityState.Modified:
                        context.Offices.Attach(office);
                        office.Description += "Joe's Room";
                        break;
                    default:
                        Assert.True(false, "Invalid Dependent State " + dependentState);
                        break;
                }

                switch (dependentState)
                {
                    case EntityState.Added:
                        context.Whiteboards.Add(whiteBoard);
                        break;
                    case EntityState.Unchanged:
                        context.Whiteboards.Attach(whiteBoard);
                        break;
                    case EntityState.Deleted:
                        context.Whiteboards.Attach(whiteBoard);
                        context.Whiteboards.Remove(whiteBoard);
                        break;
                    case EntityState.Modified:
                        context.Whiteboards.Attach(whiteBoard);
                        whiteBoard.AssetTag = string.Join("/", "18", whiteBoard.AssetTag);
                        break;
                    default:
                        Assert.True(false, "Invalid dependent state " + dependentState);
                        break;
                }

                // Asserting states to confirm the test prep worked as expected
                if (principalState != EntityState.Modified)
                {
                    Assert.Equal(principalState, GetStateEntry(context, office).State);
                }

                if (dependentState != EntityState.Modified)
                {
                    Assert.Equal(dependentState, GetStateEntry(context, whiteBoard).State);
                }

                // Act
                context.Offices.Add(office);

                // Assert
                if (dependentState == EntityState.Modified)
                {
                    Assert.Equal(dependentState, GetStateEntry(context, whiteBoard).State);
                }

                Assert.Equal(EntityState.Added, GetStateEntry(context, office).State);

                if (principalState == EntityState.Modified)
                {
                    Assert.Equal(new string[0], GetStateEntry(context, office).GetModifiedProperties());
                }
                else if (principalState == EntityState.Deleted)
                {
                    Assert.Equal(office.BuildingId, GetStateEntry(context, office).CurrentValues["BuildingId"]);
                    Assert.Equal(office.Number, GetStateEntry(context, office).CurrentValues["Number"]);
                    Assert.Equal(DBNull.Value, GetStateEntry(context, office).CurrentValues["Description"]);
                }

                // Assert fixup
                if (principalState == EntityState.Deleted
                    || dependentState == EntityState.Deleted)
                {
                    Assert.Null(whiteBoard.Office);
                    Assert.Equal(0, office.WhiteBoards.Count);
                }
                else
                {
                    Assert.Same(whiteBoard.Office, context.Offices.Find(office.Number, office.BuildingId));
                    Assert.Equal(1, office.WhiteBoards.Count);
                    Assert.True(office.WhiteBoards.Contains(whiteBoard));
                }
            }
        }

        #endregion

        #region Conflicts at leaf level

        [Fact]
        public void Add_moves_root_to_Added_with_conflicts_in_the_leaf_node_Added()
        {
            Add_moves_root_to_Added_with_conflicts_in_the_leaf_node_Added_Unchanged_Modified_or_Deleted(
                EntityState.Added);
        }

        public void Add_moves_root_to_Added_with_conflicts_in_the_leaf_node_Added_Unchanged_Modified_or_Deleted(
            EntityState dependentState)
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var product = new Product
                                  {
                                      Id = 0,
                                      Name = "Red Bull"
                                  };
                switch (dependentState)
                {
                    case EntityState.Added:
                        context.Products.Add(product);
                        break;
                    case EntityState.Modified:
                        context.Products.Attach(product);
                        product.Name += "Caffeine Drink";
                        break;
                    case EntityState.Unchanged:
                        context.Products.Attach(product);
                        break;
                    default:
                        Assert.True(false, "Invalid dependent state " + dependentState);
                        break;
                }

                if (dependentState != EntityState.Modified)
                {
                    Assert.Equal(dependentState, GetStateEntry(context, product).State);
                }

                var category = new Category
                                   {
                                       Id = "Beverages"
                                   };
                category.Products.Add(product);

                // Assert fixup
                Assert.Equal(null, product.Category);
                Assert.Equal(null, product.CategoryId);

                // Act
                context.Categories.Add(category);

                // Assert
                Assert.Equal(EntityState.Added, GetStateEntry(context, category).State);
                Assert.Equal("Beverages", GetStateEntry(context, category).CurrentValues["Id"]);
                Assert.Equal(DBNull.Value, GetStateEntry(context, category).CurrentValues["DetailedDescription"]);

                // Assert FK fixup
                Assert.Equal(product.Category, context.Categories.Find("Beverages"));
                Assert.Equal(null, product.CategoryId);
            }
        }

        [Fact]
        public void Add_moves_root_to_Added_with_conflicts_in_the_leaf_node_added()
        {
            using (var context = new SimpleModelContext())
            {
                var product = new Product
                                  {
                                      Id = 0,
                                      Name = "Red Bull"
                                  };
                context.Products.Add(product);

                var category = new Category
                                   {
                                       Id = "Beverages"
                                   };
                category.Products.Add(product);

                Assert.Equal(null, product.Category);
                Assert.Equal(null, product.CategoryId);

                context.Categories.Add(category);

                Assert.Equal(EntityState.Added, GetStateEntry(context, product).State);
                Assert.Equal(EntityState.Added, GetStateEntry(context, category).State);

                Assert.Equal(product.Category, context.Categories.Find("Beverages"));
                Assert.Equal(null, product.CategoryId);
            }
        }

        #endregion

        #region Negative Add tests

        [Fact]
        public void Add_throws_if_entity_is_null()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Equal("entity", Assert.Throws<ArgumentNullException>(() => context.Products.Add(null)).ParamName);
            }
        }

        [Fact]
        public void Non_generic_Add_throws_if_entity_is_null()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Equal(
                    "entity",
                    Assert.Throws<ArgumentNullException>(() => context.Set(typeof(Product)).Add(null)).
                        ParamName);
            }
        }

        [Fact]
        public void Add_throws_when_adding_object_which_has_a_relationship_to_Deleted_object()
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var product = new Product
                                  {
                                      Id = 0,
                                      Name = "Red Bull"
                                  };
                context.Products.Attach(product);
                context.Products.Remove(product);

                var category = new Category
                                   {
                                       Id = "Beverages"
                                   };
                category.Products.Add(product);

                // Act - Assert
                Assert.Throws<InvalidOperationException>(() => context.Categories.Add(category)).ValidateMessage(
                    "RelatedEnd_UnableToAddRelationshipWithDeletedEntity");
            }
        }

        #endregion

        #region Positive Attach tests

        [Fact]
        public void Attach_attaches_entity_to_context()
        {
            using (var context = new SimpleModelContext())
            {
                var product = new Product
                                  {
                                      Id = -1,
                                      Name = "Daddies Sauce"
                                  };
                context.Products.Attach(product);

                VerifyProduct(context, product, EntityState.Unchanged);
            }
        }

        [Fact]
        public void Non_generic_Attach_attaches_entity_to_context()
        {
            using (var context = new SimpleModelContext())
            {
                var product = new Product
                                  {
                                      Id = -1,
                                      Name = "Daddies Sauce"
                                  };
                context.Set(typeof(Product)).Attach(product);

                VerifyProduct(context, product, EntityState.Unchanged);
            }
        }

        [Fact]
        public void Attach_on_non_derived_set_attaches_derived_entity_to_context()
        {
            using (var context = new SimpleModelContext())
            {
                var product = new FeaturedProduct
                                  {
                                      Id = -1,
                                      Name = "Salad Cream"
                                  };
                context.Products.Attach(product);

                VerifyProduct(context, product, EntityState.Unchanged);
            }
        }

        [Fact]
        public void Non_generic_Attach_on_non_derived_set_attaches_derived_entity_to_context()
        {
            using (var context = new SimpleModelContext())
            {
                var product = new FeaturedProduct
                                  {
                                      Id = -1,
                                      Name = "Salad Cream"
                                  };
                context.Set(typeof(Product)).Attach(product);

                VerifyProduct(context, product, EntityState.Unchanged);
            }
        }

        [Fact]
        public void Attach_on_derived_set_attaches_derived_entity_to_context()
        {
            using (var context = new SimpleModelContext())
            {
                var product = new FeaturedProduct
                                  {
                                      Id = -1,
                                      Name = "Piccalilli"
                                  };
                context.Set<FeaturedProduct>().Attach(product);

                VerifyProduct(context, product, EntityState.Unchanged);
            }
        }

        [Fact]
        public void Non_generic_Attach_on_derived_set_attaches_derived_entity_to_context()
        {
            using (var context = new SimpleModelContext())
            {
                var product = new FeaturedProduct
                                  {
                                      Id = -1,
                                      Name = "Piccalilli"
                                  };
                context.Set(typeof(FeaturedProduct)).Attach(product);

                VerifyProduct(context, product, EntityState.Unchanged);
            }
        }

        [Fact]
        public void Attach_is_noop_if_Unchanged_entity_already_exists()
        {
            Attach_moves_entity_from_other_state_to_Unchanged(EntityState.Unchanged);
        }

        [Fact]
        public void Attach_moves_Deleted_entity_to_Unchanged()
        {
            Attach_moves_entity_from_other_state_to_Unchanged(EntityState.Deleted);
        }

        [Fact]
        public void Attach_moves_Added_entity_to_Unchanged()
        {
            Attach_moves_entity_from_other_state_to_Unchanged(EntityState.Added);
        }

        [Fact]
        public void Attach_moves_Modified_entity_to_Unchanged()
        {
            Attach_moves_entity_from_other_state_to_Unchanged(EntityState.Modified);
        }

        [Fact]
        public void Non_generic_Attach_is_noop_if_Unchanged_entity_already_exists()
        {
            Non_generic_Attach_moves_entity_from_other_state_to_Unchanged(EntityState.Unchanged);
        }

        [Fact]
        public void Non_generic_Attach_moves_Deleted_entity_to_Unchanged()
        {
            Non_generic_Attach_moves_entity_from_other_state_to_Unchanged(EntityState.Deleted);
        }

        [Fact]
        public void Non_generic_Attach_moves_Added_entity_to_Unchanged()
        {
            Non_generic_Attach_moves_entity_from_other_state_to_Unchanged(EntityState.Added);
        }

        [Fact]
        public void Non_generic_Attach_moves_Modified_entity_to_Unchanged()
        {
            Non_generic_Attach_moves_entity_from_other_state_to_Unchanged(EntityState.Modified);
        }

        [Fact]
        public void Attach_moves_Detached_evicted_entity_to_Unchanged_Sanity_test()
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var insertedProduct = new FeaturedProduct
                                          {
                                              Id = -1,
                                              Name = "Marmite",
                                              PromotionalCode = "blah"
                                          };
                context.Products.Add(insertedProduct);
                GetObjectContext(context).Detach(insertedProduct);
                Assert.Null(context.Products.Find(-1));

                // Act
                context.Set<FeaturedProduct>().Attach(insertedProduct);

                // Assert
                VerifyProduct(context, insertedProduct, EntityState.Unchanged);
            }
        }

        [Fact]
        public void Attach_moves_Detached_conflict_free_FK_graph_to_Unchanged()
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var category = new Category
                                   {
                                       Id = "Beverages"
                                   };
                var product = new Product
                                  {
                                      Id = 0,
                                      Name = "Red Bull",
                                      CategoryId = "Beverages"
                                  };
                category.Products.Add(product);

                // Act
                context.Categories.Attach(category);

                // Assert
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, category).State);
                Assert.Equal("Beverages", GetStateEntry(context, category).CurrentValues["Id"]);
                Assert.Equal(DBNull.Value, GetStateEntry(context, category).CurrentValues["DetailedDescription"]);
                Assert.Equal("Beverages", GetStateEntry(context, category).OriginalValues["Id"]);
                Assert.Equal(DBNull.Value, GetStateEntry(context, category).OriginalValues["DetailedDescription"]);

                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product).State);
                Assert.Equal("Beverages", GetStateEntry(context, product).CurrentValues["CategoryId"]);
                Assert.Equal("Beverages", GetStateEntry(context, product).OriginalValues["CategoryId"]);

                // Assert fixup
                Assert.Equal(product.Category, context.Categories.Find("Beverages"));
                Assert.True(category.Products.Contains(product));
                Assert.Equal(1, category.Products.Count);
            }
        }

        #endregion

        #region Conflict at root level in FK graph

        [Fact]
        public void Attach_moves_root_of_FK_graph_to_Unchanged_when_it_has_Added_Principal_and_Dependent_Added()
        {
            Attach_moves_root_to_Unchanged_when_FK_graph_root_is_Unchanged_Modified_or_Deleted(
                EntityState.Added,
                EntityState.Added);
        }

        [Fact]
        public void Attach_moves_root_of_FK_graph_to_Unchanged_when_it_has_Added_Principal_and_Dependent_Unchanged()
        {
            Attach_moves_root_to_Unchanged_when_FK_graph_root_is_Unchanged_Modified_or_Deleted(
                EntityState.Added,
                EntityState.Unchanged);
        }

        [Fact]
        public void Attach_moves_root_of_FK_graph_to_Unchanged_when_it_has_Added_Principal_and_Dependent_Modified()
        {
            Attach_moves_root_to_Unchanged_when_FK_graph_root_is_Unchanged_Modified_or_Deleted(
                EntityState.Added,
                EntityState.Modified);
        }

        [Fact]
        public void Attach_moves_root_of_FK_graph_to_Unchanged_when_it_has_Added_Principal_and_Dependent_Deleted()
        {
            Attach_moves_root_to_Unchanged_when_FK_graph_root_is_Unchanged_Modified_or_Deleted(
                EntityState.Added,
                EntityState.Deleted);
        }

        [Fact]
        public void Attach_is_a_noop_if_FK_graph_has_Unchanged_Principal_and_Dependent_Added()
        {
            Attach_moves_root_to_Unchanged_when_FK_graph_root_is_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged,
                EntityState.Added);
        }

        [Fact]
        public void Attach_is_a_noop_if_FK_graph_has_Unchanged_Principal_and_Dependent_Unchanged()
        {
            Attach_moves_root_to_Unchanged_when_FK_graph_root_is_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged,
                EntityState.Added);
        }

        [Fact]
        public void Attach_is_a_noop_if_FK_graph_has_Unchanged_Principal_and_Dependent_Modified()
        {
            Attach_moves_root_to_Unchanged_when_FK_graph_root_is_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged,
                EntityState.Added);
        }

        [Fact]
        public void Attach_is_a_noop_if_FK_graph_has_Unchanged_Principal_and_Dependent_Deleted()
        {
            Attach_moves_root_to_Unchanged_when_FK_graph_root_is_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged,
                EntityState.Added);
        }

        [Fact]
        public void Attach_moves_root_of_FK_graph_to_Unchanged_when_it_has_Modified_Principal_and_Dependent_Added()
        {
            Attach_moves_root_to_Unchanged_when_FK_graph_root_is_Unchanged_Modified_or_Deleted(
                EntityState.Modified,
                EntityState.Added);
        }

        [Fact]
        public void Attach_moves_root_of_FK_graph_to_Unchanged_when_it_has_Modified_Principal_and_Dependent_Unchanged()
        {
            Attach_moves_root_to_Unchanged_when_FK_graph_root_is_Unchanged_Modified_or_Deleted(
                EntityState.Modified,
                EntityState.Unchanged);
        }

        [Fact]
        public void Attach_moves_root_of_FK_graph_to_Unchanged_when_it_has_Modified_Principal_and_Dependent_Modified()
        {
            Attach_moves_root_to_Unchanged_when_FK_graph_root_is_Unchanged_Modified_or_Deleted(
                EntityState.Modified,
                EntityState.Modified);
        }

        [Fact]
        public void Attach_moves_root_of_FK_graph_to_Unchanged_when_it_has_Modified_Principal_and_Dependent_Deleted()
        {
            Attach_moves_root_to_Unchanged_when_FK_graph_root_is_Unchanged_Modified_or_Deleted(
                EntityState.Modified,
                EntityState.Deleted);
        }

        [Fact]
        public void Attach_moves_root_of_FK_graph_to_Unchanged_when_it_has_Deleted_Principal_and_Dependent_Added()
        {
            Attach_moves_root_to_Unchanged_when_FK_graph_root_is_Unchanged_Modified_or_Deleted(
                EntityState.Deleted,
                EntityState.Added);
        }

        [Fact]
        public void Attach_moves_root_of_FK_graph_to_Unchanged_when_it_has_Deleted_Principal_and_Dependent_Unchanged()
        {
            Attach_moves_root_to_Unchanged_when_FK_graph_root_is_Unchanged_Modified_or_Deleted(
                EntityState.Deleted,
                EntityState.Unchanged);
        }

        [Fact]
        public void Attach_moves_root_of_FK_graph_to_Unchanged_when_it_has_Deleted_Principal_and_Dependent_Modified()
        {
            Attach_moves_root_to_Unchanged_when_FK_graph_root_is_Unchanged_Modified_or_Deleted(
                EntityState.Deleted,
                EntityState.Modified);
        }

        [Fact]
        public void Attach_moves_root_of_FK_graph_to_Unchanged_when_it_has_Deleted_Principal_and_Dependent_Deleted()
        {
            Attach_moves_root_to_Unchanged_when_FK_graph_root_is_Unchanged_Modified_or_Deleted(
                EntityState.Deleted,
                EntityState.Deleted);
        }

        private void Attach_moves_root_to_Unchanged_when_FK_graph_root_is_Unchanged_Modified_or_Deleted(
            EntityState principalState, EntityState dependentState)
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var category = new Category
                                   {
                                       Id = "Beverages"
                                   };
                var product = new Product
                                  {
                                      Id = 0,
                                      Name = "Red Bull",
                                      CategoryId = "Beverages"
                                  };
                category.Products.Add(product);

                switch (principalState)
                {
                    case EntityState.Added:
                        context.Categories.Add(category);
                        break;
                    case EntityState.Unchanged:
                        context.Categories.Attach(category);
                        break;
                    case EntityState.Deleted:
                        context.Categories.Attach(category);
                        context.Categories.Remove(category);
                        break;
                    case EntityState.Modified:
                        context.Categories.Attach(category);
                        category.DetailedDescription += "Non-Alcoholic Beverages";
                        break;
                    default:
                        Assert.True(false, "Invalid Principal State " + principalState);
                        break;
                }

                switch (dependentState)
                {
                    case EntityState.Added:
                        context.Products.Add(product);
                        break;
                    case EntityState.Unchanged:
                        context.Products.Attach(product);
                        break;
                    case EntityState.Deleted:
                        context.Products.Attach(product);
                        context.Products.Remove(product);
                        break;
                    case EntityState.Modified:
                        context.Products.Attach(product);
                        product.Name += "Caffeine drink";
                        break;
                    default:
                        Assert.True(false, "Invalid Dependent state " + dependentState);
                        break;
                }

                // Asserting states to confirm the test prep worked as expected
                if (principalState != EntityState.Modified)
                {
                    Assert.Equal(principalState, GetStateEntry(context, category).State);
                }

                if (dependentState != EntityState.Modified)
                {
                    Assert.Equal(dependentState, GetStateEntry(context, product).State);
                }

                // Assert fixup based on principal state
                if (principalState == EntityState.Deleted
                    || dependentState == EntityState.Deleted)
                {
                    Assert.Null(product.Category);
                    Assert.Equal(0, category.Products.Count);
                }
                else
                {
                    // Assert fixup, as Attach will fixup references and FK's
                    Assert.Same(product.Category, context.Categories.Find("Beverages"));
                    Assert.Equal(1, category.Products.Count);
                    Assert.True(category.Products.Contains(product));
                }

                // Act
                context.Categories.Attach(category);

                // Assert
                // Its a no-op for initial principal state of Unchanged
                if (principalState != EntityState.Unchanged)
                {
                    Assert.Equal(EntityState.Unchanged, GetStateEntry(context, category).State);

                    if (principalState == EntityState.Added)
                    {
                        Assert.Equal(category.Id, GetStateEntry(context, category).OriginalValues["Id"]);
                        Assert.Equal(DBNull.Value, GetStateEntry(context, category).OriginalValues["DetailedDescription"]);
                    }
                    else if (principalState == EntityState.Modified)
                    {
                        Assert.Equal(new string[0], GetStateEntry(context, category).GetModifiedProperties());
                        Assert.Equal("Non-Alcoholic Beverages", GetStateEntry(context, category).OriginalValues["DetailedDescription"]);
                    }
                    else if (principalState == EntityState.Deleted)
                    {
                        Assert.Equal("Beverages", GetStateEntry(context, category).CurrentValues["Id"]);
                        Assert.Equal(DBNull.Value, GetStateEntry(context, category).CurrentValues["DetailedDescription"]);
                    }

                    if (dependentState == EntityState.Modified)
                    {
                        Assert.Equal(EntityState.Modified, GetStateEntry(context, product).State);
                        Assert.Equal(new[] { "Name" }, GetStateEntry(context, product).GetModifiedProperties());
                    }
                }

                // Assert fixup
                if (principalState == EntityState.Deleted
                    || dependentState == EntityState.Deleted)
                {
                    Assert.Null(product.Category);
                    Assert.Equal(0, category.Products.Count);
                }
                else
                {
                    Assert.Same(product.Category, context.Categories.Find("Beverages"));
                    Assert.Equal(1, category.Products.Count);
                    Assert.True(category.Products.Contains(product));
                }
            }
        }

        #endregion

        #region Conflict at root level in Independent Associations graph

        [Fact]
        public void Attach_moves_root_of_Independent_graph_to_Unchanged_when_it_has_Added_Principal_and_Dependent_Added()
        {
            Attach_moves_root_to_Unchanged_when_Independent_graph_root_is_Unchanged_or_Modified_or_Deleted(
                EntityState.Added, EntityState.Added);
        }

        [Fact]
        public void
            Attach_moves_root_of_Independent_graph_to_Unchanged_when_it_has_Added_Principal_and_Dependent_Unchanged()
        {
            Attach_moves_root_to_Unchanged_when_Independent_graph_root_is_Unchanged_or_Modified_or_Deleted(
                EntityState.Added, EntityState.Unchanged);
        }

        [Fact]
        public void
            Attach_moves_root_of_Independent_graph_to_Unchanged_when_it_has_Added_Principal_and_Dependent_Modified()
        {
            Attach_moves_root_to_Unchanged_when_Independent_graph_root_is_Unchanged_or_Modified_or_Deleted(
                EntityState.Added, EntityState.Modified);
        }

        [Fact]
        public void
            Attach_moves_root_of_Independent_graph_to_Unchanged_when_it_has_Added_Principal_and_Dependent_Deleted()
        {
            Attach_moves_root_to_Unchanged_when_Independent_graph_root_is_Unchanged_or_Modified_or_Deleted(
                EntityState.Added, EntityState.Deleted);
        }

        [Fact]
        public void Attach_is_a_noop_if_Independent_graph_has_Unchanged_Principal_and_Dependent_Added()
        {
            Attach_moves_root_to_Unchanged_when_Independent_graph_root_is_Unchanged_or_Modified_or_Deleted(
                EntityState.Added, EntityState.Added);
        }

        [Fact]
        public void Attach_is_a_noop_if_Independent_graph_has_Unchanged_Principal_and_Dependent_Unchanged()
        {
            Attach_moves_root_to_Unchanged_when_Independent_graph_root_is_Unchanged_or_Modified_or_Deleted(
                EntityState.Added, EntityState.Added);
        }

        [Fact]
        public void Attach_is_a_noop_if_Independent_graph_has_Unchanged_Principal_and_Dependent_Modified()
        {
            Attach_moves_root_to_Unchanged_when_Independent_graph_root_is_Unchanged_or_Modified_or_Deleted(
                EntityState.Added, EntityState.Added);
        }

        [Fact]
        public void Attach_is_a_noop_if_Independent_graph_has_Unchanged_Principal_and_Dependent_Deleted()
        {
            Attach_moves_root_to_Unchanged_when_Independent_graph_root_is_Unchanged_or_Modified_or_Deleted(
                EntityState.Added, EntityState.Added);
        }

        [Fact]
        public void
            Attach_moves_root_of_Independent_graph_to_Unchanged_when_it_has_Modified_Principal_and_Dependent_Added()
        {
            Attach_moves_root_to_Unchanged_when_Independent_graph_root_is_Unchanged_or_Modified_or_Deleted(
                EntityState.Modified, EntityState.Added);
        }

        [Fact]
        public void
            Attach_moves_root_of_Independent_graph_to_Unchanged_when_it_has_Modified_Principal_and_Dependent_Unchanged()
        {
            Attach_moves_root_to_Unchanged_when_Independent_graph_root_is_Unchanged_or_Modified_or_Deleted(
                EntityState.Modified, EntityState.Unchanged);
        }

        [Fact]
        public void
            Attach_moves_root_of_Independent_graph_to_Unchanged_when_it_has_Modified_Principal_and_Dependent_Modified()
        {
            Attach_moves_root_to_Unchanged_when_Independent_graph_root_is_Unchanged_or_Modified_or_Deleted(
                EntityState.Modified, EntityState.Modified);
        }

        [Fact]
        public void
            Attach_moves_root_of_Independent_graph_to_Unchanged_when_it_has_Modified_Principal_and_Dependent_Deleted()
        {
            Attach_moves_root_to_Unchanged_when_Independent_graph_root_is_Unchanged_or_Modified_or_Deleted(
                EntityState.Modified, EntityState.Deleted);
        }

        [Fact]
        public void
            Attach_moves_root_of_Independent_graph_to_Unchanged_when_it_has_Deleted_Principal_and_Dependent_Added()
        {
            Attach_moves_root_to_Unchanged_when_Independent_graph_root_is_Unchanged_or_Modified_or_Deleted(
                EntityState.Deleted, EntityState.Added);
        }

        [Fact]
        public void
            Attach_moves_root_of_Independent_graph_to_Unchanged_when_it_has_Deleted_Principal_and_Dependent_Unchanged()
        {
            Attach_moves_root_to_Unchanged_when_Independent_graph_root_is_Unchanged_or_Modified_or_Deleted(
                EntityState.Deleted, EntityState.Unchanged);
        }

        [Fact]
        public void
            Attach_moves_root_of_Independent_graph_to_Unchanged_when_it_has_Deleted_Principal_and_Dependent_Modified()
        {
            Attach_moves_root_to_Unchanged_when_Independent_graph_root_is_Unchanged_or_Modified_or_Deleted(
                EntityState.Deleted, EntityState.Modified);
        }

        [Fact]
        public void
            Attach_moves_root_of_Independent_graph_to_Unchanged_when_it_has_Deleted_Principal_and_Dependent_Deleted()
        {
            Attach_moves_root_to_Unchanged_when_Independent_graph_root_is_Unchanged_or_Modified_or_Deleted(
                EntityState.Deleted, EntityState.Deleted);
        }

        private void Attach_moves_root_to_Unchanged_when_Independent_graph_root_is_Unchanged_or_Modified_or_Deleted(
            EntityState principalState, EntityState dependentState)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                // Arrange 
                var office = new Office
                                 {
                                     BuildingId = Guid.NewGuid(),
                                     Number = "18/1111",
                                     Description = DBNull.Value.ToString()
                                 };
                var whiteBoard = new Whiteboard
                                     {
                                         iD = new byte[] { 1, 2, 3, 4 },
                                         AssetTag = "ABCDX0009"
                                     };
                office.WhiteBoards.Add(whiteBoard);

                switch (principalState)
                {
                    case EntityState.Added:
                        context.Offices.Add(office);
                        break;
                    case EntityState.Unchanged:
                        context.Offices.Attach(office);
                        break;
                    case EntityState.Deleted:
                        context.Offices.Attach(office);
                        context.Offices.Remove(office);
                        break;
                    case EntityState.Modified:
                        context.Offices.Attach(office);
                        office.Description += "Joe's Room";
                        break;
                    default:
                        Assert.True(false, "Invalid Dependent State " + dependentState);
                        break;
                }

                switch (dependentState)
                {
                    case EntityState.Added:
                        context.Whiteboards.Add(whiteBoard);
                        break;
                    case EntityState.Unchanged:
                        context.Whiteboards.Attach(whiteBoard);
                        break;
                    case EntityState.Deleted:
                        context.Whiteboards.Attach(whiteBoard);
                        context.Whiteboards.Remove(whiteBoard);
                        break;
                    case EntityState.Modified:
                        context.Whiteboards.Attach(whiteBoard);
                        whiteBoard.AssetTag = string.Join("/", "18", whiteBoard.AssetTag);
                        break;
                    default:
                        Assert.True(false, "Invalid dependent state " + dependentState);
                        break;
                }

                // Asserting states to confirm the test prep worked as expected
                if (principalState != EntityState.Modified)
                {
                    Assert.Equal(principalState, GetStateEntry(context, office).State);
                }

                if (dependentState != EntityState.Modified)
                {
                    Assert.Equal(dependentState, GetStateEntry(context, whiteBoard).State);
                }

                // Act
                context.Offices.Attach(office);

                // Assert
                if (dependentState == EntityState.Modified)
                {
                    Assert.Equal(dependentState, GetStateEntry(context, whiteBoard).State);
                }

                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, office).State);

                if (principalState == EntityState.Added)
                {
                    Assert.Equal(office.BuildingId, GetStateEntry(context, office).OriginalValues["BuildingId"]);
                    Assert.Equal(office.Number, GetStateEntry(context, office).OriginalValues["Number"]);
                    Assert.Equal(office.Description, GetStateEntry(context, office).OriginalValues["Description"]);
                }
                else if (principalState == EntityState.Modified)
                {
                    Assert.Equal(new string[0], GetStateEntry(context, office).GetModifiedProperties());
                    Assert.Equal(office.Description, GetStateEntry(context, office).OriginalValues["Description"]);
                }
                else if (principalState == EntityState.Deleted)
                {
                    Assert.Equal(office.BuildingId, GetStateEntry(context, office).CurrentValues["BuildingId"]);
                    Assert.Equal(office.Number, GetStateEntry(context, office).CurrentValues["Number"]);
                    Assert.Equal(office.Description, GetStateEntry(context, office).CurrentValues["Description"]);
                }

                // Assert fixup
                if (principalState == EntityState.Deleted
                    || dependentState == EntityState.Deleted)
                {
                    Assert.Null(whiteBoard.Office);
                    Assert.Equal(0, office.WhiteBoards.Count);
                }
                else
                {
                    Assert.Same(whiteBoard.Office, context.Offices.Find(office.Number, office.BuildingId));
                    Assert.Equal(1, office.WhiteBoards.Count);
                    Assert.True(office.WhiteBoards.Contains(whiteBoard));
                }
            }
        }

        #endregion

        #region Conflicts at leaf level

        [Fact]
        public void Attach_puts_root_in_Unchanged_with_conflicts_in_the_leaf_node_Added()
        {
            Attach_puts_root_in_Unchanged_with_conflicts_in_the_leaf_node_Added_Unchanged_Modified(EntityState.Added);
        }

        [Fact]
        public void Attach_puts_root_in_Unchanged_with_conflicts_in_the_leaf_node_Unchanged()
        {
            Attach_puts_root_in_Unchanged_with_conflicts_in_the_leaf_node_Added_Unchanged_Modified(EntityState.Unchanged);
        }

        [Fact]
        public void Attach_puts_root_in_Unchanged_with_conflicts_in_the_leaf_node_Modified()
        {
            Attach_puts_root_in_Unchanged_with_conflicts_in_the_leaf_node_Added_Unchanged_Modified(EntityState.Modified);
        }

        private void Attach_puts_root_in_Unchanged_with_conflicts_in_the_leaf_node_Added_Unchanged_Modified(
            EntityState dependentState)
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var product = new Product
                                  {
                                      Id = 0,
                                      Name = "Red Bull",
                                      CategoryId = "Beverages"
                                  };
                switch (dependentState)
                {
                    case EntityState.Added:
                        context.Products.Add(product);
                        break;
                    case EntityState.Modified:
                        context.Products.Attach(product);
                        product.Name += "Caffeine Drink";
                        break;
                    case EntityState.Unchanged:
                        context.Products.Attach(product);
                        break;
                    default:
                        Assert.True(false, "Invalid dependent state " + dependentState);
                        break;
                }

                if (dependentState != EntityState.Modified)
                {
                    Assert.Equal(dependentState, GetStateEntry(context, product).State);
                }

                var category = new Category
                                   {
                                       Id = "Beverages"
                                   };
                category.Products.Add(product);

                // Assert fixup
                Assert.Equal(null, product.Category);

                // Act
                context.Categories.Attach(category);

                // Assert
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, category).State);
                Assert.Equal("Beverages", GetStateEntry(context, category).CurrentValues["Id"]);
                Assert.Equal("Beverages", GetStateEntry(context, category).OriginalValues["Id"]);
                Assert.Equal(DBNull.Value, GetStateEntry(context, category).CurrentValues["DetailedDescription"]);
                Assert.Equal(DBNull.Value, GetStateEntry(context, category).OriginalValues["DetailedDescription"]);

                if (dependentState == EntityState.Modified)
                {
                    Assert.Equal(EntityState.Modified, GetStateEntry(context, product).State);
                    Assert.Equal(new[] { "Name" }, GetStateEntry(context, product).GetModifiedProperties());
                }

                // Assert FK fixup
                Assert.Equal(product.Category, context.Categories.Find("Beverages"));
                Assert.Equal("Beverages", product.CategoryId);
            }
        }

        #endregion

        #region Negative Attach tests

        [Fact]
        public void Attach_throws_if_entity_is_null()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Equal(
                    "entity",
                    Assert.Throws<ArgumentNullException>(() => context.Products.Attach(null)).ParamName);
            }
        }

        [Fact]
        public void Non_generic_Attach_throws_if_entity_is_null()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Equal(
                    "entity",
                    Assert.Throws<ArgumentNullException>(() => context.Set(typeof(Product)).Attach(null)).
                        ParamName);
            }
        }

        [Fact]
        public void Attach_throws_when_changing_state_from_Added_entity_with_null_key()
        {
            using (var context = new SimpleModelContext())
            {
                var badCategory = new Category(null);
                context.Categories.Add(badCategory);
                Assert.Throws<ArgumentException>(() => context.Categories.Attach(badCategory)).ValidateMessage(
                    "ObjectStateManager_ChangeStateFromAddedWithNullKeyIsInvalid");
            }
        }

        [Fact]
        public void Attach_throws_when_instance_has_null_key()
        {
            using (var context = new SimpleModelContext())
            {
                var category = new Category();
                Assert.Throws<InvalidOperationException>(() => context.Categories.Attach(category)).ValidateMessage(
                    "EntityKey_NullKeyValue", "Id", typeof(Category).Name);
            }
        }

        [Fact]
        public void Attach_throws_when_referential_constraint_properties_are_not_consistent()
        {
            using (var context = new SimpleModelContext())
            {
                var category = new Category
                                   {
                                       Id = "Spreads"
                                   };

                // Note that the FK has not been set on the dependent, thereby creating an inconsistency 
                var product = new Product
                                  {
                                      Id = 0,
                                      Name = "Vegemite"
                                  };
                category.Products.Add(product);

                Assert.Throws<InvalidOperationException>(() => context.Categories.Attach(category)).ValidateMessage(
                    "RelationshipManager_InconsistentReferentialConstraintProperties");

                Assert.Null(context.Categories.Find("Spreads"));
                Assert.Null(context.Products.Find(0));
                Assert.Null(product.Category);
                Assert.Null(product.CategoryId);
                Assert.True(category.Products.Contains(product));
            }
        }

        #endregion

        #region Positive Remove tests

        [Fact]
        public void Remove_is_noop_if_Deleted_entity_already_exists()
        {
            Remove_moves_entity_from_other_state_to_Deleted(EntityState.Deleted);
        }

        [Fact]
        public void Remove_moves_Unchanged_entity_to_Deleted()
        {
            Remove_moves_entity_from_other_state_to_Deleted(EntityState.Unchanged);
        }

        [Fact]
        public void Non_generic_Remove_is_noop_if_Deleted_entity_already_exists()
        {
            Non_generic_Remove_moves_entity_from_other_state_to_Deleted(EntityState.Deleted);
        }

        [Fact]
        public void Non_generic_Remove_moves_Unchanged_entity_to_Deleted()
        {
            Non_generic_Remove_moves_entity_from_other_state_to_Deleted(EntityState.Unchanged);
        }

        [Fact]
        public void Remove_moves_Added_entity_to_Detached()
        {
            using (var context = new SimpleModelContext())
            {
                var product = new Product
                                  {
                                      Id = -1,
                                      Name = "Haggis"
                                  };
                context.Products.Add(product);

                context.Products.Remove(product);

                Assert.Null(context.Products.Find(-1));
            }
        }

        [Fact]
        public void Remove_moves_Modified_entity_to_Deleted()
        {
            Remove_moves_entity_from_other_state_to_Deleted(EntityState.Modified);
        }

        [Fact]
        public void Remove_derived_entity_from_Base_Set()
        {
            using (var context = new SimpleModelContext())
            {
                var product = new FeaturedProduct
                                  {
                                      Id = -1,
                                      Name = "Haggis"
                                  };
                context.Products.Attach(product);

                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product).State);
                context.Products.Remove(product);
                Assert.Equal(EntityState.Deleted, GetStateEntry(context, product).State);
            }
        }

        [Fact]
        public void Remove_derived_entity_from_derived_Set()
        {
            using (var context = new SimpleModelContext())
            {
                var product = new FeaturedProduct
                                  {
                                      Id = -1,
                                      Name = "Haggis"
                                  };
                context.Set<FeaturedProduct>().Attach(product);

                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product).State);
                context.Set<FeaturedProduct>().Remove(product);
                Assert.Equal(EntityState.Deleted, GetStateEntry(context, product).State);
            }
        }

        [Fact]
        public void Remove_can_work_in_base_or_derived_set()
        {
            using (var context = new SimpleModelContext())
            {
                var product = new FeaturedProduct
                                  {
                                      Id = -1,
                                      Name = "Ginger Cookies"
                                  };

                //Insert into base set and delete from derived set
                context.Products.Attach(product);
                context.Set<FeaturedProduct>().Remove(product);
                Assert.Equal(EntityState.Deleted, GetStateEntry(context, product).State);
            }

            using (var context = new SimpleModelContext())
            {
                var product = new FeaturedProduct
                                  {
                                      Id = -1,
                                      Name = "Ginger Cookies"
                                  };

                //Insert into derived set and delete from base set
                context.Set<FeaturedProduct>().Attach(product);
                context.Products.Remove(product);
                Assert.Equal(EntityState.Deleted, GetStateEntry(context, product).State);
            }
        }

        [Fact]
        public void Remove_will_mark_a_non_nullable_FK_entity_as_Deleted()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = new Building
                                   {
                                       BuildingId = Guid.NewGuid(),
                                       Name = "Building 35"
                                   };
                var mailroom = new MailRoom
                                   {
                                       id = 1,
                                       BuildingId = building.BuildingId
                                   };

                building.MailRooms.Add(mailroom);

                context.Buildings.Attach(building);

                // Assert fixup
                Assert.Equal(building, mailroom.Building);

                // Act
                context.Buildings.Remove(building);

                // Assert
                Assert.Equal(EntityState.Deleted, GetStateEntry(context, building).State);
                Assert.Equal(EntityState.Deleted, GetStateEntry(context, mailroom).State);
                Assert.Equal(building.BuildingId, mailroom.BuildingId);
            }
        }

        [Fact]
        public void Remove_will_mark_a_nullable_FK_property_null()
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var category = new Category
                                   {
                                       Id = "Beverages"
                                   };
                var product = new Product
                                  {
                                      Id = 0,
                                      Name = "RedBull",
                                      CategoryId = "Beverages"
                                  };
                category.Products.Add(product);

                context.Categories.Attach(category);

                // Assert fixup
                Assert.True(category.Products.Contains(product));
                Assert.Same(product.Category, category);

                context.Categories.Remove(category);

                // Assert the lack of fixup
                Assert.Equal(0, category.Products.Count);
                Assert.Null(product.Category);
                Assert.Null(product.CategoryId);
            }
        }

        #endregion

        #region Conflicts at the root level for FK graphs

        [Fact]
        public void Remove_moves_FK_graph_root_to_Deleted_when_it_has_Added_Principal_Added_Dependent()
        {
            Remove_moves_root_to_Deleted_when_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Added,
                EntityState.Added);
        }

        [Fact]
        public void Remove_moves_FK_graph_root_to_Deleted_when_it_has_Added_Principal_Unchanged_Dependent()
        {
            Remove_moves_root_to_Deleted_when_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Added,
                EntityState.Unchanged);
        }

        [Fact]
        public void Remove_moves_FK_graph_root_to_Deleted_when_it_has_Added_Principal_Modified_Dependent()
        {
            Remove_moves_root_to_Deleted_when_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Added,
                EntityState.Modified);
        }

        [Fact]
        public void Remove_moves_FK_graph_root_to_Deleted_when_it_has_Added_Principal_Deleted_Dependent()
        {
            Remove_moves_root_to_Deleted_when_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Added,
                EntityState.Deleted);
        }

        [Fact]
        public void Remove_moves_FK_graph_root_to_Deleted_when_it_has_Unchanged_Principal_Added_Dependent()
        {
            Remove_moves_root_to_Deleted_when_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged,
                EntityState.Added);
        }

        [Fact]
        public void Remove_moves_FK_graph_root_to_Deleted_when_it_has_Unchanged_Principal_Unchanged_Dependent()
        {
            Remove_moves_root_to_Deleted_when_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged,
                EntityState.Unchanged);
        }

        [Fact]
        public void Remove_moves_FK_graph_root_to_Deleted_when_it_has_Unchanged_Principal_Modified_Dependent()
        {
            Remove_moves_root_to_Deleted_when_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged,
                EntityState.Modified);
        }

        [Fact]
        public void Remove_moves_FK_graph_root_to_Deleted_when_it_has_Unchanged_Principal_Deleted_Dependent()
        {
            Remove_moves_root_to_Deleted_when_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged,
                EntityState.Deleted);
        }

        [Fact]
        public void Remove_moves_FK_graph_root_to_Deleted_when_it_has_Modified_Principal_Added_Dependent()
        {
            Remove_moves_root_to_Deleted_when_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Modified,
                EntityState.Added);
        }

        [Fact]
        public void Remove_moves_FK_graph_root_to_Deleted_when_it_has_Modified_Principal_Unchanged_Dependent()
        {
            Remove_moves_root_to_Deleted_when_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Modified,
                EntityState.Unchanged);
        }

        [Fact]
        public void Remove_moves_FK_graph_root_to_Deleted_when_it_has_Modified_Principal_Modified_Dependent()
        {
            Remove_moves_root_to_Deleted_when_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Modified,
                EntityState.Modified);
        }

        [Fact]
        public void Remove_moves_FK_graph_root_to_Deleted_when_it_has_Modified_Principal_Deleted_Dependent()
        {
            Remove_moves_root_to_Deleted_when_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Modified,
                EntityState.Deleted);
        }

        [Fact]
        public void Remove_moves_FK_graph_root_to_Deleted_when_it_has_Deleted_Principal_Added_Dependent()
        {
            Remove_moves_root_to_Deleted_when_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Deleted,
                EntityState.Added);
        }

        [Fact]
        public void Remove_moves_FK_graph_root_to_Deleted_when_it_has_Deleted_Principal_Unchanged_Dependent()
        {
            Remove_moves_root_to_Deleted_when_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Deleted,
                EntityState.Unchanged);
        }

        [Fact]
        public void Remove_moves_FK_graph_root_to_Deleted_when_it_has_Deleted_Principal_Modified_Dependent()
        {
            Remove_moves_root_to_Deleted_when_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Deleted,
                EntityState.Modified);
        }

        [Fact]
        public void Remove_moves_FK_graph_root_to_Deleted_when_it_has_Deleted_Principal_Deleted_Dependent()
        {
            Remove_moves_root_to_Deleted_when_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Deleted,
                EntityState.Deleted);
        }

        private void Remove_moves_root_to_Deleted_when_graph_root_is_Added_Unchanged_Modified_or_Deleted(
            EntityState principalState, EntityState dependentState)
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var category = new Category
                                   {
                                       Id = "Beverages"
                                   };
                var product = new Product
                                  {
                                      Id = 0,
                                      Name = "Red Bull",
                                      CategoryId = "Beverages"
                                  };
                category.Products.Add(product);

                // Arranging principal object in the required state
                switch (principalState)
                {
                    case EntityState.Added:
                        context.Categories.Add(category);
                        break;
                    case EntityState.Unchanged:
                        context.Categories.Attach(category);
                        break;
                    case EntityState.Deleted:
                        context.Categories.Attach(category);
                        context.Categories.Remove(category);
                        break;
                    case EntityState.Modified:
                        context.Categories.Attach(category);
                        category.DetailedDescription += "Non-Alcoholic Beverages";
                        break;
                    default:
                        Assert.True(false, "Invalid Principal State " + principalState);
                        break;
                }

                // Arranging dependent object ine th erequired state
                switch (dependentState)
                {
                    case EntityState.Added:
                        context.Products.Add(product);
                        break;
                    case EntityState.Unchanged:
                        context.Products.Attach(product);
                        break;
                    case EntityState.Deleted:
                        context.Products.Attach(product);
                        context.Products.Remove(product);
                        break;
                    case EntityState.Modified:
                        context.Products.Attach(product);
                        product.Name += "Caffeine drink";
                        break;
                    default:
                        Assert.True(false, "Invalid Dependent state " + dependentState);
                        break;
                }

                // Asserting states to confirm the test prep worked as expected for all states other modified
                // as modified cannot be detected without a DetectChanges call
                if (principalState != EntityState.Modified)
                {
                    Assert.Equal(principalState, GetStateEntry(context, category).State);
                }
                if (dependentState != EntityState.Modified)
                {
                    Assert.Equal(dependentState, GetStateEntry(context, product).State);
                }

                // Assert fixup based on principal state
                if (principalState == EntityState.Deleted
                    || dependentState == EntityState.Deleted)
                {
                    Assert.Null(product.Category);
                    Assert.Equal(0, category.Products.Count);
                }
                else
                {
                    Assert.Same(product.Category, context.Categories.Find("Beverages"));
                    Assert.Equal(1, category.Products.Count);
                    Assert.True(category.Products.Contains(product));
                }

                // Act
                context.Categories.Remove(category);

                if (principalState == EntityState.Deleted)
                {
                    if (dependentState == EntityState.Modified)
                    {
                        Assert.Equal(EntityState.Modified, GetStateEntry(context, product).State);
                        Assert.Equal(new[] { "Name" }, GetStateEntry(context, product).GetModifiedProperties());
                    }
                }
                else
                {
                    // Delete will null out the nullable FK 
                    if (dependentState != EntityState.Deleted)
                    {
                        Assert.Equal(DBNull.Value, GetStateEntry(context, product).CurrentValues["CategoryId"]);

                        // For unchanged, modified dependents for which the principal was not deleted, Remove
                        // would mark these as modified. 
                        if (dependentState != EntityState.Added)
                        {
                            Assert.Equal(EntityState.Modified, GetStateEntry(context, product).State);
                            if (dependentState == EntityState.Modified)
                            {
                                Assert.Equal(new[] { "CategoryId", "Name" }, GetStateEntry(context, product).GetModifiedProperties());
                            }
                            else
                            {
                                Assert.Equal(new[] { "CategoryId" }, GetStateEntry(context, product).GetModifiedProperties());
                            }
                        }
                    }

                    if (principalState == EntityState.Added)
                    {
                        AssertNoStateEntry(context, category);
                    }
                    else
                    {
                        Assert.Equal(GetStateEntry(context, category).State, EntityState.Deleted);

                        if (principalState == EntityState.Modified)
                        {
                            Assert.Equal(DBNull.Value, GetStateEntry(context, category).OriginalValues["DetailedDescription"]);
                        }
                        else if (principalState == EntityState.Deleted)
                        {
                            Assert.Equal(GetStateEntry(context, category).CurrentValues["Id"], "Beverages");
                            Assert.Equal(
                                GetStateEntry(context, category).CurrentValues["DetailedDescription"],
                                DBNull.Value);
                        }
                    }
                }

                // Assert fixup
                Assert.Null(product.Category);
                Assert.Null(product.CategoryId);
                Assert.Equal(0, category.Products.Count);
            }
        }

        #endregion

        #region Negative Remove tests

        [Fact]
        public void Remove_throws_if_entity_is_null()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Equal(
                    "entity",
                    Assert.Throws<ArgumentNullException>(() => context.Products.Remove(null)).ParamName);
            }
        }

        [Fact]
        public void Non_generic_Remove_throws_if_entity_is_null()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Equal(
                    "entity",
                    Assert.Throws<ArgumentNullException>(() => context.Set(typeof(Product)).Remove(null)).
                        ParamName);
            }
        }

        [Fact]
        public void Remove_throws_if_entity_is_not_in_context()
        {
            using (var context = new SimpleModelContext())
            {
                var product = new Product
                                  {
                                      Name = "Digestive Biscuits"
                                  };
                Assert.Throws<InvalidOperationException>(() => context.Products.Remove(product)).ValidateMessage(
                    "ObjectContext_CannotDeleteEntityNotInObjectStateManager");
            }
        }

        [Fact]
        public void Non_generic_Remove_throws_if_entity_is_not_in_context()
        {
            using (var context = new SimpleModelContext())
            {
                var product = new Product
                                  {
                                      Name = "Digestive Biscuits"
                                  };
                Assert.Throws<InvalidOperationException>(() => context.Set(typeof(Product)).Remove(product)).
                    ValidateMessage("ObjectContext_CannotDeleteEntityNotInObjectStateManager");
            }
        }

        #endregion

        #region Detach Root of Independent Association Graph which will leave a Stub behind

        [Fact]
        public void Detach_IA_Graph_root_when_principal_is_Added_and_Dependent_is_Added()
        {
            Detach_moves_root_to_Detached_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted
                (EntityState.Added, EntityState.Added);
        }

        [Fact]
        public void Detach_IA_Graph_root_when_principal_is_Added_and_Dependent_is_Unchanged()
        {
            Detach_moves_root_to_Detached_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted
                (EntityState.Added, EntityState.Unchanged);
        }

        [Fact]
        public void Detach_IA_Graph_root_when_principal_is_Added_and_Dependent_is_Modified()
        {
            Detach_moves_root_to_Detached_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted
                (EntityState.Added, EntityState.Modified);
        }

        [Fact]
        public void Detach_IA_Graph_root_when_principal_is_Unchanged_and_Dependent_is_Added()
        {
            Detach_moves_root_to_Detached_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted
                (EntityState.Unchanged, EntityState.Added);
        }

        [Fact]
        public void Detach_IA_Graph_root_when_principal_is_Unchanged_and_Dependent_is_Unchanged()
        {
            Detach_moves_root_to_Detached_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted
                (EntityState.Unchanged, EntityState.Unchanged);
        }

        [Fact]
        public void Detach_IA_Graph_root_when_principal_is_Unchanged_and_Dependent_is_Modified()
        {
            Detach_moves_root_to_Detached_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted
                (EntityState.Unchanged, EntityState.Modified);
        }

        [Fact]
        public void Detach_IA_Graph_root_when_principal_is_Unchanged_and_Dependent_is_Deleted()
        {
            Detach_moves_root_to_Detached_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted
                (EntityState.Unchanged, EntityState.Deleted);
        }

        [Fact]
        public void Detach_IA_Graph_root_when_principal_is_Deleted_and_Dependent_is_Unchanged()
        {
            Detach_moves_root_to_Detached_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted
                (EntityState.Deleted, EntityState.Unchanged);
        }

        [Fact]
        public void Detach_IA_Graph_root_when_principal_is_Deleted_and_Dependent_is_Modified()
        {
            Detach_moves_root_to_Detached_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted
                (EntityState.Deleted, EntityState.Modified);
        }

        [Fact]
        public void Detach_IA_Graph_root_when_principal_is_Deleted_and_Dependent_is_Deleted()
        {
            Detach_moves_root_to_Detached_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted
                (EntityState.Deleted, EntityState.Deleted);
        }

        private void
            Detach_moves_root_to_Detached_when_independent_association_graph_root_is_Added_Unchanged_Modified_or_Deleted
            (EntityState principalState, EntityState dependentState)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                // Arrange 
                var office = new Office
                                 {
                                     BuildingId = Guid.NewGuid(),
                                     Number = "18/1111"
                                 };
                var whiteBoard = new Whiteboard
                                     {
                                         iD = new byte[] { 1, 2, 3, 4 },
                                         AssetTag = "ABCDX0009"
                                     };
                office.WhiteBoards.Add(whiteBoard);

                switch (principalState)
                {
                    case EntityState.Added:
                        context.Offices.Add(office);
                        break;
                    case EntityState.Unchanged:
                        context.Offices.Attach(office);
                        break;
                    case EntityState.Deleted:
                        context.Offices.Attach(office);
                        context.Offices.Remove(office);
                        break;
                    case EntityState.Modified:
                        context.Offices.Attach(office);
                        office.Description += "Joe's Room";
                        break;
                    default:
                        Assert.True(false, "Invalid Dependent State " + dependentState);
                        break;
                }

                switch (dependentState)
                {
                    case EntityState.Added:
                        context.Whiteboards.Add(whiteBoard);
                        break;
                    case EntityState.Unchanged:
                        context.Whiteboards.Attach(whiteBoard);
                        break;
                    case EntityState.Deleted:
                        context.Whiteboards.Attach(whiteBoard);
                        context.Whiteboards.Remove(whiteBoard);
                        break;
                    case EntityState.Modified:
                        context.Whiteboards.Attach(whiteBoard);
                        whiteBoard.AssetTag = string.Join("/", "18", whiteBoard.AssetTag);
                        break;
                    default:
                        Assert.True(false, "Invalid dependent state " + dependentState);
                        break;
                }

                // Asserting principal and dependent states to confirm the test prep worked as expected.
                if (principalState != EntityState.Modified)
                {
                    Assert.Equal(principalState, GetStateEntry(context, office).State);
                }

                if (dependentState != EntityState.Modified)
                {
                    Assert.Equal(dependentState, GetStateEntry(context, whiteBoard).State);
                }

                // Act
                GetObjectContext(context).DetectChanges();
                GetObjectContext(context).Detach(office);

                // Assert dependent state as DetectChanges call would have happened by now
                if (dependentState == EntityState.Modified)
                {
                    Assert.Equal(dependentState, GetStateEntry(context, whiteBoard).State);
                    Assert.Equal(GetStateEntry(context, whiteBoard).GetModifiedProperties(), new[] { "AssetTag" });
                }

                AssertNoStateEntry(context, office);

                // Assert lack of fixup
                Assert.Null(whiteBoard.Office);
                Assert.Equal(0, office.WhiteBoards.Count);
            }
        }

        #endregion

        #region Detach Root of FK Graph

        [Fact]
        public void Detach_FK_Graph_root_when_principal_is_Added_and_Dependent_is_Added()
        {
            Detach_detaches_root_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Added,
                EntityState.Added);
        }

        [Fact]
        public void Detach_FK_Graph_root_when_principal_is_Added_and_Dependent_is_Unchanged()
        {
            Detach_detaches_root_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Added,
                EntityState.Unchanged);
        }

        [Fact]
        public void Detach_FK_Graph_root_when_principal_is_Added_and_Dependent_is_Modified()
        {
            Detach_detaches_root_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Added,
                EntityState.Modified);
        }

        [Fact]
        public void Detach_FK_Graph_root_when_principal_is_Unchanged_and_Dependent_is_Added()
        {
            Detach_detaches_root_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged,
                EntityState.Added);
        }

        [Fact]
        public void Detach_FK_Graph_root_when_principal_is_Unchanged_and_Dependent_is_Unchanged()
        {
            Detach_detaches_root_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged,
                EntityState.Unchanged);
        }

        [Fact]
        public void Detach_FK_Graph_root_when_principal_is_Unchanged_and_Dependent_is_Modified()
        {
            Detach_detaches_root_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged,
                EntityState.Modified);
        }

        [Fact]
        public void Detach_FK_Graph_root_when_principal_is_Unchanged_and_Dependent_is_Deleted()
        {
            Detach_detaches_root_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Unchanged,
                EntityState.Deleted);
        }

        [Fact]
        public void Detach_FK_Graph_root_when_principal_is_Deleted_and_Dependent_is_Unchanged()
        {
            Detach_detaches_root_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Deleted,
                EntityState.Unchanged);
        }

        [Fact]
        public void Detach_FK_Graph_root_when_principal_is_Deleted_and_Dependent_is_Modified()
        {
            Detach_detaches_root_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Deleted,
                EntityState.Modified);
        }

        [Fact]
        public void Detach_FK_Graph_root_when_principal_is_Deleted_and_Dependent_is_Deleted()
        {
            Detach_detaches_root_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
                EntityState.Deleted,
                EntityState.Deleted);
        }

        private void Detach_detaches_root_when_FK_graph_root_is_Added_Unchanged_Modified_or_Deleted(
            EntityState principalState, EntityState dependentState)
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var category = new Category
                                   {
                                       Id = "Beverages"
                                   };
                var product = new Product
                                  {
                                      Id = 0,
                                      Name = "Red Bull"
                                  };
                category.Products.Add(product);

                switch (principalState)
                {
                    case EntityState.Added:
                        context.Categories.Add(category);
                        break;
                    case EntityState.Unchanged:
                        product.CategoryId = "Beverages";
                        context.Categories.Attach(category);
                        break;
                    case EntityState.Deleted:
                        product.CategoryId = "Beverages";
                        context.Categories.Attach(category);
                        context.Categories.Remove(category);
                        break;
                    case EntityState.Modified:
                        product.CategoryId = "Beverages";
                        context.Categories.Attach(category);
                        category.DetailedDescription += "Non-Alcoholic Beverages";
                        break;
                    default:
                        Assert.True(false, "Invalid Principal State " + principalState);
                        break;
                }

                switch (dependentState)
                {
                    case EntityState.Added:
                        context.Products.Add(product);
                        break;
                    case EntityState.Unchanged:
                        context.Products.Attach(product);
                        break;
                    case EntityState.Deleted:
                        context.Products.Attach(product);
                        context.Products.Remove(product);
                        break;
                    case EntityState.Modified:
                        context.Products.Attach(product);
                        product.Name += "Caffeine drink";
                        break;
                    default:
                        Assert.True(false, "Invalid Dependent state " + dependentState);
                        break;
                }

                // Asserting states to confirm the test prep worked as expected
                if (principalState != EntityState.Modified)
                {
                    Assert.Equal(principalState, GetStateEntry(context, category).State);
                }

                if (dependentState != EntityState.Modified)
                {
                    Assert.Equal(dependentState, GetStateEntry(context, product).State);
                }

                // Assert fixup is as expected after test prep
                if (principalState == EntityState.Deleted
                    || dependentState == EntityState.Deleted)
                {
                    Assert.Null(product.Category);
                    Assert.Equal(0, category.Products.Count);
                }
                else
                {
                    Assert.Same(product.Category, context.Categories.Find("Beverages"));
                    Assert.Equal(1, category.Products.Count);
                    Assert.True(category.Products.Contains(product));
                }

                // Act
                GetObjectContext(context).DetectChanges();
                GetObjectContext(context).Detach(category);

                // Assert
                AssertNoStateEntry(context, category);

                if (dependentState == EntityState.Modified)
                {
                    Assert.Equal(GetStateEntry(context, product).State, EntityState.Modified);
                    Assert.Equal(GetStateEntry(context, product).GetModifiedProperties(), new[] { "Name" });
                }

                // Assert lack of fixup after Detach
                Assert.Null(product.Category);
                Assert.Equal(0, category.Products.Count);
            }
        }

        #endregion

        #region DbSet.Create tests

        [Fact]
        public void DbSet_Create_creates_proxy_instance_for_proxyable_class()
        {
            DbSet_Create_creates_proxy_instance_for_proxyable_class_implementation(c => c.Drivers.Create());
        }

        [Fact]
        public void Non_generic_DbSet_Create_creates_proxy_instance_for_proxyable_class()
        {
            DbSet_Create_creates_proxy_instance_for_proxyable_class_implementation(c => c.Set(typeof(Driver)).Create());
        }

        private void DbSet_Create_creates_proxy_instance_for_proxyable_class_implementation(
            Func<F1Context, object> create)
        {
            using (var context = new F1Context())
            {
                var driver = create(context);

                Assert.IsAssignableFrom<Driver>(driver);
                Assert.NotSame(typeof(Driver), driver.GetType());
            }
        }

        [Fact]
        public void DbSet_Create_creates_non_proxy_instance_for_non_proxyable_class()
        {
            DbSet_Create_creates_non_proxy_instance_for_non_proxyable_class_implementation(c => c.Products.Create());
        }

        [Fact]
        public void Non_generic_DbSet_Create_creates_non_proxy_instance_for_non_proxyable_class()
        {
            DbSet_Create_creates_non_proxy_instance_for_non_proxyable_class_implementation(
                c => c.Set(typeof(Product)).Create());
        }

        private void DbSet_Create_creates_non_proxy_instance_for_non_proxyable_class_implementation(
            Func<SimpleModelContext, object> create)
        {
            using (var context = new SimpleModelContext())
            {
                var product = create(context);

                Assert.IsType<Product>(product);
                Assert.Same(typeof(Product), product.GetType());
            }
        }

        [Fact]
        public void DbSet_Create_creates_proxy_instance_of_derived_type_for_proxyable_class()
        {
            DbSet_Create_creates_proxy_instance_of_derived_type_for_proxyable_class_implementation(
                c => c.Drivers.Create<TestDriver>());
        }

        [Fact]
        public void Non_generic_DbSet_Create_creates_proxy_instance_of_derived_type_for_proxyable_class()
        {
            DbSet_Create_creates_proxy_instance_of_derived_type_for_proxyable_class_implementation(
                c => c.Set(typeof(Driver)).Create(typeof(TestDriver)));
        }

        private void DbSet_Create_creates_proxy_instance_of_derived_type_for_proxyable_class_implementation(
            Func<F1Context, object> create)
        {
            using (var context = new F1Context())
            {
                var driver = create(context);

                Assert.IsAssignableFrom<TestDriver>(driver);
                Assert.NotSame(typeof(TestDriver), driver.GetType());
            }
        }

        [Fact]
        public void DbSet_Create_creates_non_proxy_instance_of_derived_type_for_non_proxyable_class()
        {
            DbSet_Create_creates_non_proxy_instance_of_derived_type_for_non_proxyable_class_implementation(
                c => c.Products.Create<FeaturedProduct>());
        }

        [Fact]
        public void Non_generic_DbSet_Create_creates_non_proxy_instance_of_derived_type_for_non_proxyable_class()
        {
            DbSet_Create_creates_non_proxy_instance_of_derived_type_for_non_proxyable_class_implementation(
                c => c.Set(typeof(Product)).Create(typeof(FeaturedProduct)));
        }

        private void DbSet_Create_creates_non_proxy_instance_of_derived_type_for_non_proxyable_class_implementation(
            Func<SimpleModelContext, object> create)
        {
            using (var context = new SimpleModelContext())
            {
                var product = create(context);

                Assert.IsType<FeaturedProduct>(product);
                Assert.Same(typeof(FeaturedProduct), product.GetType());
            }
        }

        #endregion

        #region Converstion between generic and non-generic sets

        [Fact]
        public void Non_generic_DbSet_can_be_converted_to_generic_DbSet_with_Cast()
        {
            using (var context = new SimpleModelContext())
            {
                var nonGenericSet = context.Set(typeof(Product));

                var genericSet = nonGenericSet.Cast<Product>();

                Assert.IsType<DbSet<Product>>(genericSet);
                Assert.Same(genericSet, context.Products);
            }
        }

        [Fact]
        public void Generic_DbSet_can_be_implicitly_converted_to_non_generic_DbSet()
        {
            using (var context = new SimpleModelContext())
            {
                var genericSet = context.Products;

                DbSet nonGenericSet = genericSet;

                Assert.IsAssignableFrom<DbSet>(nonGenericSet);
                Assert.Same(nonGenericSet, context.Set(typeof(Product)));
            }
        }

        [Fact]
        public void Non_generic_DbQuery_can_be_converted_to_generic_DbQuery_with_Cast()
        {
            using (var context = new SimpleModelContext())
            {
                var products = context.Products.Where(p => p.Name == "Marmite");
                var query =
                    (DbQuery)((IQueryable)context.Set(typeof(Product))).Provider.CreateQuery(products.Expression);

                var genericQuery = query.Cast<Product>();

                Assert.IsType<DbQuery<Product>>(genericQuery);
                Assert.Same(genericQuery.First(), products.First());
            }
        }

        [Fact]
        public void Generic_DbQuery_can_be_implicitly_converted_to_non_generic_DbQuery()
        {
            using (var context = new SimpleModelContext())
            {
                var query = (DbQuery<Product>)context.Products.Where(p => p.Name == "Marmite");

                DbQuery nonGenericQuery = query;

                var enumerator = ((IEnumerable)nonGenericQuery).GetEnumerator();

                Assert.True(enumerator.MoveNext());

                Assert.Same(query.First(), enumerator.Current);

#if !NET40
                var asyncEnumerator = ((IDbAsyncEnumerable)nonGenericQuery).GetAsyncEnumerator();

                Assert.True(asyncEnumerator.MoveNextAsync().Result);

                Assert.Same(query.First(), asyncEnumerator.Current);
#endif
            }
        }

        #endregion

        #region Entity set names with special characters

        [Fact]
        public void DbSet_for_an_entity_set_with_special_characters_can_be_created()
        {
            using (var context = new SpecialCharacters())
            {
                var countries = context.Länder.ToList();

                Assert.Equal(2, countries.Count);
                Assert.Equal(1, countries.Count(l => l.Näme == "A"));
                Assert.Equal(1, countries.Count(l => l.Näme == "B"));
            }
        }

        [Fact]
        public void Find_from_context_works_for_entity_set_with_special_characters()
        {
            using (var context = new SpecialCharacters())
            {
                context.Länder.Load();

                var lander = context.Länder.Find(1);
                Assert.NotNull(lander);
                Assert.Equal("A", lander.Näme);
            }
        }

        [Fact]
        public void Find_from_database_works_for_entity_set_with_special_characters()
        {
            using (var context = new SpecialCharacters())
            {
                var lander = context.Länder.Find(1);
                Assert.NotNull(lander);
                Assert.Equal("A", lander.Näme);
            }
        }

        [Fact]
        public void DbSet_Add_works_for_entity_set_with_special_characters()
        {
            using (var context = new SpecialCharacters())
            {
                var lander = context.Länder.Add(
                    new Länder
                        {
                            Id = 3,
                            Näme = "C"
                        });

                Assert.Equal(EntityState.Added, context.Entry(lander).State);
            }
        }

        [Fact]
        public void DbSet_Attach_works_for_entity_set_with_special_characters()
        {
            using (var context = new SpecialCharacters())
            {
                var lander = context.Länder.Attach(
                    new Länder
                        {
                            Id = 3,
                            Näme = "C"
                        });

                Assert.Equal(EntityState.Unchanged, context.Entry(lander).State);
            }
        }

        [Fact]
        public void SqlQuery_works_for_entity_set_with_special_characters()
        {
            using (var context = new SpecialCharacters())
            {
                var countries = context.Länder.SqlQuery("select * from Länder").ToList();

                Assert.Equal(2, countries.Count);
                Assert.Equal(1, countries.Count(l => l.Näme == "A"));
                Assert.Equal(1, countries.Count(l => l.Näme == "B"));
            }
        }

        #endregion

        #region Using Set, etc with proxy types

        [Fact]
        public void Calling_the_non_generic_Set_method_with_a_proxy_type_returns_the_same_set_as_when_called_with_the_real_entity_type()
        {
            using (var context = new F1Context())
            {
                var driverProxy = context.Drivers.Create();
                Assert.NotSame(typeof(Driver), driverProxy.GetType());

                var set = context.Set(driverProxy.GetType());

                Assert.Same(set, context.Set(typeof(Driver)));
            }
        }

        [Fact]
        public void Calling_the_generic_Set_method_with_a_proxy_type_throws()
        {
            using (var context = new F1Context())
            {
                var driverProxy = context.Drivers.Create();
                Assert.NotSame(typeof(Driver), driverProxy.GetType());

                var setMethod = typeof(DbContext)
                    .GetMethods()
                    .Where(m => m.Name == "Set" && m.IsGenericMethodDefinition)
                    .Single()
                    .MakeGenericMethod(driverProxy.GetType());

                // This throws because Set always returns the same Set instance every time
                // it is called and we cannot cast the generic Set for the actual entity type
                // to the generic Set for the proxy type.
                Assert.Throws<InvalidOperationException>(
                    () =>
                        {
                            try
                            {
                                setMethod.Invoke(context, null);
                            }
                            catch (TargetInvocationException ex)
                            {
                                throw ex.InnerException;
                            }
                            ;
                        }).ValidateMessage("CannotCallGenericSetWithProxyType");
            }
        }

        [Fact]
        public void Calling_the_non_generic_Create_method_with_a_proxy_type_works()
        {
            using (var context = new F1Context())
            {
                var testDriverProxy1 = context.Set<TestDriver>().Create();
                Assert.NotSame(typeof(TestDriver), testDriverProxy1.GetType());

                var testDriverProxy2 = context.Set(typeof(Driver)).Create(testDriverProxy1.GetType());

                Assert.Same(testDriverProxy1.GetType(), testDriverProxy2.GetType());
            }
        }

        [Fact]
        public void Calling_the_generic_Create_method_with_a_proxy_type_works()
        {
            using (var context = new F1Context())
            {
                var testDriverProxy1 = context.Set<TestDriver>().Create();
                Assert.NotSame(typeof(TestDriver), testDriverProxy1.GetType());

                var createMethod = typeof(DbSet<Driver>)
                    .GetMethods()
                    .Where(m => m.Name == "Create" && m.IsGenericMethodDefinition)
                    .Single()
                    .MakeGenericMethod(testDriverProxy1.GetType());

                var testDriverProxy2 = createMethod.Invoke(context.Drivers, null);

                Assert.Same(testDriverProxy1.GetType(), testDriverProxy2.GetType());
            }
        }

        [Fact]
        public void Calling_the_non_generic_Entry_method_with_a_proxy_type_works()
        {
            using (var context = new F1Context())
            {
                var driverProxy = context.Drivers.Create();
                Assert.NotSame(typeof(Driver), driverProxy.GetType());

                var entry = context.Entry(driverProxy);

                Assert.Same(driverProxy, entry.Entity);
            }
        }

        [Fact]
        public void Calling_the_generic_Entry_method_with_a_proxy_type_works()
        {
            using (var context = new F1Context())
            {
                var driverProxy = context.Drivers.Create();
                Assert.NotSame(typeof(Driver), driverProxy.GetType());

                var entryMethod = typeof(DbContext)
                    .GetMethods()
                    .Where(m => m.Name == "Entry" && m.IsGenericMethodDefinition)
                    .Single()
                    .MakeGenericMethod(driverProxy.GetType());

                var entry = entryMethod.Invoke(context, new object[] { driverProxy });

                Assert.Same(driverProxy, entry.GetType().GetProperty("Entity").GetValue(entry, null));
            }
        }

        [Fact]
        public void Calling_the_generic_Entries_method_with_a_proxy_type_works()
        {
            using (var context = new F1Context())
            {
                var driverProxy = context.Drivers.Create();
                Assert.NotSame(typeof(Driver), driverProxy.GetType());

                var entriesMethod = typeof(DbChangeTracker)
                    .GetMethods()
                    .Where(m => m.Name == "Entries" && m.IsGenericMethodDefinition)
                    .Single()
                    .MakeGenericMethod(driverProxy.GetType());

                var entries = entriesMethod.Invoke(context.ChangeTracker, null);
            }
        }

        #endregion

        #region Helpers

        private void Add_moves_entity_from_other_state_to_Added(EntityState initialState)
        {
            Operation_moves_entity_from_one_state_to_another((s, p) => s.Add(p), initialState, EntityState.Added);
        }

        private void Attach_moves_entity_from_other_state_to_Unchanged(EntityState initialState)
        {
            Operation_moves_entity_from_one_state_to_another((s, p) => s.Attach(p), initialState, EntityState.Unchanged);
        }

        private void Remove_moves_entity_from_other_state_to_Deleted(EntityState initialState)
        {
            Operation_moves_entity_from_one_state_to_another((s, p) => s.Remove(p), initialState, EntityState.Deleted);
        }

        private void Operation_moves_entity_from_one_state_to_another(
            Action<IDbSet<Product>, Product> operation,
            EntityState initialState, EntityState finalState)
        {
            using (var context = new SimpleModelContext())
            {
                var product = new Product
                                  {
                                      Id = -1,
                                      Name = "Haggis"
                                  };
                context.Products.Add(product);
                GetObjectContext(context).ObjectStateManager.ChangeObjectState(product, initialState);

                operation(context.Products, product);

                VerifyProduct(context, product, finalState);
            }
        }

        private void Non_generic_Add_moves_entity_from_other_state_to_Added(EntityState initialState)
        {
            Non_generic_Operation_moves_entity_from_one_state_to_another(
                (s, p) => s.Add(p), initialState,
                EntityState.Added);
        }

        private void Non_generic_Attach_moves_entity_from_other_state_to_Unchanged(EntityState initialState)
        {
            Non_generic_Operation_moves_entity_from_one_state_to_another(
                (s, p) => s.Attach(p), initialState,
                EntityState.Unchanged);
        }

        private void Non_generic_Remove_moves_entity_from_other_state_to_Deleted(EntityState initialState)
        {
            Non_generic_Operation_moves_entity_from_one_state_to_another(
                (s, p) => s.Remove(p), initialState,
                EntityState.Deleted);
        }

        private void Non_generic_Operation_moves_entity_from_one_state_to_another(
            Action<DbSet, Product> operation,
            EntityState initialState,
            EntityState finalState)
        {
            using (var context = new SimpleModelContext())
            {
                var product = new Product
                                  {
                                      Id = -1,
                                      Name = "Haggis"
                                  };
                context.Set(typeof(Product)).Add(product);
                GetObjectContext(context).ObjectStateManager.ChangeObjectState(product, initialState);

                operation(context.Set(typeof(Product)), product);

                VerifyProduct(context, product, finalState);
            }
        }

        private void VerifyProduct(SimpleModelContext context, Product product, EntityState expectedState)
        {
            var foundProduct = context.Products.Find(-1);

            Assert.Same(product, foundProduct);
            Assert.Equal(-1, product.Id);
            Assert.Equal(expectedState, GetStateEntry(context, product).State);
            Assert.Equal(1, GetStateEntries(context).Count());
        }

        #endregion
    }

    #region Model with special characters

    public class SpecialCharacters : DbContext
    {
        public SpecialCharacters()
        {
            Database.SetInitializer(new SpecialCharactersInitializer());
        }

        public DbSet<Länder> Länder { get; set; }
    }

    public class Länder
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string Näme { get; set; }
    }

    public class SpecialCharactersInitializer : DropCreateDatabaseAlways<SpecialCharacters>
    {
        protected override void Seed(SpecialCharacters context)
        {
            context.Länder.Add(
                new Länder
                    {
                        Id = 1,
                        Näme = "A"
                    });
            context.Länder.Add(
                new Länder
                    {
                        Id = 2,
                        Näme = "B"
                    });
        }
    }

    #endregion
}
