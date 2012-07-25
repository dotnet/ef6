// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace ProductivityApiTests
{
    using System;
    using System.Collections;
    using System.Data.Entity.Core;
    using System.Data;
    using System.Data.Entity;
    using System.Linq;
    using AdvancedPatternsModel;
    using AllTypeKeysModel;
    using SimpleModel;
    using UnSpecifiedOrderingModel;
    using Xunit;

    /// <summary>
    /// Tests for DbSet.Find.
    /// </summary>
    public class FindTests : FunctionalTestBase
    {
        #region Simple positive cases

        [Fact]
        public void Find_returns_null_if_entity_is_not_found()
        {
            Find_returns_null_if_entity_is_not_found_implementation(c => c.Products.Find(-666));
        }

        [Fact]
        public void Non_generic_Find_returns_null_if_entity_is_not_found()
        {
            Find_returns_null_if_entity_is_not_found_implementation(c => c.Set(typeof(Product)).Find(-666));
        }

        private void Find_returns_null_if_entity_is_not_found_implementation(Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                var product = find(context);

                Assert.Null(product);
                Assert.Equal(0, GetStateEntries(context).Count());
            }
        }

        [Fact]
        public void Find_with_int_key_returns_Entity_from_store()
        {
            Find_with_int_key_returns_Entity_from_store_implementation(c => c.Products.Find(1));
        }

        [Fact]
        public void Non_generic_Find_with_int_key_returns_Entity_from_store()
        {
            Find_with_int_key_returns_Entity_from_store_implementation(c => c.Set(typeof(Product)).Find(1));
        }

        private void Find_with_int_key_returns_Entity_from_store_implementation(Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                var product = (Product)find(context);

                Assert.NotNull(product);
                Assert.Equal(1, product.Id);
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product).State);
                Assert.Equal(1, GetStateEntries(context).Count());
            }
        }

        [Fact]
        public void Find_with_int_key_on_Base_Set_returns_derived_entity_from_store()
        {
            Find_with_int_key_on_Base_Set_returns_derived_entity_from_store_implementation(c => c.Products.Find(7));
        }

        [Fact]
        public void Non_generic_Find_with_int_key_on_Base_Set_returns_derived_entity_from_store()
        {
            Find_with_int_key_on_Base_Set_returns_derived_entity_from_store_implementation(
                c => c.Set(typeof(Product)).Find(7));
        }

        private void Find_with_int_key_on_Base_Set_returns_derived_entity_from_store_implementation(
            Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                // Act
                var product = (Product)find(context);

                // Assert
                Assert.NotNull(product);
                Assert.Equal(7, product.Id);
            }
        }

        [Fact]
        public void Find_with_string_key_returns_entity_from_store()
        {
            using (var context = new SimpleModelContext())
            {
                var category = context.Categories.Find("Medications");

                Assert.NotNull(category);
                Assert.Equal("Medications", category.Id);
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, category).State);
                Assert.Equal(1, GetStateEntries(context).Count());
            }
        }

        [Fact]
        public void Find_with_binary_key_returns_entity_from_store()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                // Arrange
                var byteKey = new byte[] { 1, 9, 7, 3 };

                // Act
                var whiteBoard = context.Whiteboards.Find(byteKey);

                // Assert findings
                Assert.NotNull(whiteBoard);
                Assert.True(StructuralComparisons.StructuralEqualityComparer.Equals(whiteBoard.iD, byteKey));
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, whiteBoard).State);
            }
        }

        [Fact]
        public void Find_with_int_key_returns_entity_from_state_manager()
        {
            Find_with_int_key_returns_entity_from_state_manager_implementation(c => c.Products.Find(2));
        }

        [Fact]
        public void Non_generic_Find_with_int_key_returns_entity_from_state_manager()
        {
            Find_with_int_key_returns_entity_from_state_manager_implementation(c => c.Set(typeof(Product)).Find(2));
        }

        private void Find_with_int_key_returns_entity_from_state_manager_implementation(
            Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                var product1 = (Product)find(context);
                var product2 = (Product)find(context);

                Assert.Same(product1, product2);
                Assert.Equal(2, product1.Id);
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product1).State);
                Assert.Equal(1, GetStateEntries(context).Count());
            }
        }

        [Fact]
        public void Find_with_int_key_returns_derived_entity_state_manager()
        {
            Find_with_int_key_returns_derived_entity_state_manager_implementation(c => c.Products.Find(7));
        }

        [Fact]
        public void Non_generic_Find_with_int_key_returns_derived_entity_state_manager()
        {
            Find_with_int_key_returns_derived_entity_state_manager_implementation(c => c.Set(typeof(Product)).Find(7));
        }

        private void Find_with_int_key_returns_derived_entity_state_manager_implementation(
            Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var product = (Product)find(context);

                // Act
                var product2 = (Product)find(context);

                // Assert
                Assert.Same(product, product2);
                Assert.NotNull(product2);
                Assert.Equal(7, product.Id);
                Assert.IsType<FeaturedProduct>(product2);
            }
        }

        [Fact]
        public void Find_with_string_key_returns_entity_from_state_manager()
        {
            using (var context = new SimpleModelContext())
            {
                var category1 = context.Categories.Find("Beverages");
                var category2 = context.Categories.Find("Beverages");

                Assert.Same(category1, category2);
                Assert.Equal("Beverages", category1.Id);
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, category1).State);
                Assert.Equal(1, GetStateEntries(context).Count());
            }
        }

        [Fact]
        public void Find_with_binary_key_returns_entity_from_state_manager()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                // Arrange
                var byteKey = new byte[] { 1, 9, 7, 3 };
                var whiteBoard = context.Whiteboards.Find(byteKey);

                // Act
                var whiteBoard2 = context.Whiteboards.Find(byteKey);

                // Assert Findings
                Assert.Same(whiteBoard2, whiteBoard);
                Assert.NotNull(whiteBoard2);
                Assert.True(StructuralComparisons.StructuralEqualityComparer.Equals(whiteBoard2.iD, byteKey));
            }
        }

        [Fact]
        public void Find_with_int_key_returns_Added_entity_from_state_manager()
        {
            Find_with_int_key_returns_Added_entity_from_state_manager_implementation(c => c.Products.Find(-1));
        }

        [Fact]
        public void Non_generic_Find_with_int_key_returns_Added_entity_from_state_manager()
        {
            Find_with_int_key_returns_Added_entity_from_state_manager_implementation(
                c => c.Set(typeof(Product)).Find(-1));
        }

        private void Find_with_int_key_returns_Added_entity_from_state_manager_implementation(
            Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                var addedProduct = new Product() { Id = -1, Name = "Italian Radicals" };
                context.Products.Add(addedProduct);

                var foundProduct = (Product)find(context);

                Assert.Same(addedProduct, foundProduct);
                Assert.Equal(-1, addedProduct.Id);
                Assert.Equal(EntityState.Added, GetStateEntry(context, addedProduct).State);
                Assert.Equal(1, GetStateEntries(context).Count());
            }
        }

        [Fact]
        public void Find_with_int_key_returns_added_derived_entity_from_state_manager()
        {
            Find_with_int_key_returns_added_derived_entity_from_state_manager_implementation(c => c.Products.Find(7));
        }

        [Fact]
        public void Non_generic_Find_with_int_key_returns_added_derived_entity_from_state_manager()
        {
            Find_with_int_key_returns_added_derived_entity_from_state_manager_implementation(
                c => c.Set(typeof(Product)).Find(7));
        }

        private void Find_with_int_key_returns_added_derived_entity_from_state_manager_implementation(
            Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var product = (Product)find(context);

                // This will transform product to being in Added state
                context.Products.Add(product);

                // Act
                var product2 = (Product)find(context);

                // Assert
                Assert.Same(product, product2);
                Assert.NotNull(product2);
                Assert.Equal(7, product.Id);
                Assert.IsType<FeaturedProduct>(product2);
            }
        }

        [Fact]
        public void Find_with_string_key_returns_Added_entity_from_state_manager()
        {
            using (var context = new SimpleModelContext())
            {
                var addedCategory = new Category() { Id = "NorthStar Center" };
                context.Categories.Add(addedCategory);

                var foundCategory = context.Categories.Find("NorthStar Center");

                Assert.Same(addedCategory, foundCategory);
                Assert.Equal("NorthStar Center", addedCategory.Id);
                Assert.Equal(EntityState.Added, GetStateEntry(context, addedCategory).State);
                Assert.Equal(1, GetStateEntries(context).Count());
            }
        }

        [Fact]
        public void Find_with_binary_key_returns_Added_entity_from_state_manager()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                // Arrange
                var byteKey = new byte[] { 1, 2, 3, 4 };
                var whiteBoard = new Whiteboard { iD = byteKey, AssetTag = "AMZN-PacMed-1012" };
                context.Set<Whiteboard>().Add(whiteBoard);

                // Act
                var foundWhiteBoard = context.Whiteboards.Find(byteKey);

                // Assert Findings
                Assert.Same(foundWhiteBoard, whiteBoard);
                Assert.NotNull(foundWhiteBoard);
                Assert.True(StructuralComparisons.StructuralEqualityComparer.Equals(foundWhiteBoard.iD, byteKey));
                Assert.True(StructuralComparisons.StructuralEqualityComparer.Equals(foundWhiteBoard.AssetTag,
                                                                                    "AMZN-PacMed-1012"));
            }
        }

        [Fact]
        public void Find_returns_Deleted_entity_from_state_manager()
        {
            Find_returns_Deleted_entity_from_state_manager_implementation(c => c.Categories.Find("Xiaohe Tomb complex"));
        }

        [Fact]
        public void Non_generic_Find_returns_Deleted_entity_from_state_manager()
        {
            Find_returns_Deleted_entity_from_state_manager_implementation(
                c => c.Set(typeof(Category)).Find("Xiaohe Tomb complex"));
        }

        private void Find_returns_Deleted_entity_from_state_manager_implementation(Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                var deletedCategory = new Category() { Id = "Xiaohe Tomb complex" };
                context.Categories.Add(deletedCategory);
                GetObjectContext(context).AcceptAllChanges();
                context.Categories.Remove(deletedCategory);

                var foundCategory = (Category)find(context);

                Assert.Same(deletedCategory, foundCategory);
                Assert.Equal("Xiaohe Tomb complex", deletedCategory.Id);
                Assert.Equal(EntityState.Deleted, GetStateEntry(context, deletedCategory).State);
                Assert.Equal(1, GetStateEntries(context).Count());
            }
        }

        [Fact]
        public void Find_returns_Added_entity_from_state_manager_even_if_key_matches_entity_in_store()
        {
            Find_returns_Added_entity_from_state_manager_even_if_key_matches_entity_in_store_implementation(
                c => c.Products.Find(1));
        }

        [Fact]
        public void Non_generic_Find_returns_Added_entity_from_state_manager_even_if_key_matches_entity_in_store()
        {
            Find_returns_Added_entity_from_state_manager_even_if_key_matches_entity_in_store_implementation(
                c => c.Set(typeof(Product)).Find(1));
        }

        private void Find_returns_Added_entity_from_state_manager_even_if_key_matches_entity_in_store_implementation(
            Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                var addedProduct = new Product() { Id = 1, Name = "Vegemite" };
                context.Products.Add(addedProduct);

                var foundProduct = (Product)find(context);

                Assert.Same(addedProduct, foundProduct);
                Assert.Equal(1, addedProduct.Id);
                Assert.Equal("Vegemite", addedProduct.Name);
                Assert.Equal(EntityState.Added, GetStateEntry(context, addedProduct).State);
                Assert.Equal(1, GetStateEntries(context).Count());
            }
        }

        [Fact]
        public void Find_returns_Unchanged_entity_from_state_manager_even_if_key_matches_Added_entity()
        {
            Find_returns_Unchanged_entity_from_state_manager_even_if_key_matches_Added_entity_implementation(
                c => c.Products.Find(1));
        }

        [Fact]
        public void Non_generic_Find_returns_Unchanged_entity_from_state_manager_even_if_key_matches_Added_entity()
        {
            Find_returns_Unchanged_entity_from_state_manager_even_if_key_matches_Added_entity_implementation(
                c => c.Set(typeof(Product)).Find(1));
        }

        private void Find_returns_Unchanged_entity_from_state_manager_even_if_key_matches_Added_entity_implementation(
            Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                var unchangedProduct = context.Products.Find(1);
                var addedProduct = new Product() { Id = 1, Name = "Vegemite" };
                context.Products.Add(addedProduct);

                var foundProduct = (Product)find(context);

                Assert.Same(unchangedProduct, foundProduct);
                Assert.Equal(1, unchangedProduct.Id);
                Assert.Equal(1, addedProduct.Id);
                Assert.Equal("Marmite", unchangedProduct.Name);
                Assert.Equal("Vegemite", addedProduct.Name);
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, unchangedProduct).State);
                Assert.Equal(EntityState.Added, GetStateEntry(context, addedProduct).State);
                Assert.Equal(2, GetStateEntries(context).Count());
            }
        }

        [Fact]
        public void Find_derived_type_from_state_manager_in_unchanged_state_even_though_its_key_matches_Added_derived_entity_Sanity_test()
        {
            Find_returns_derived_entity_in_unchanged_state_in_preference_to_added_from_state_manager_Sanity_test_implementation
                <FeaturedProduct>(useNonGeneric: false);
        }

        [Fact]
        public void Find_Base_type_returns_derived_entity_in_unchanged_state_in_preference_to_added_from_state_manager_Sanity_test()
        {
            Find_returns_derived_entity_in_unchanged_state_in_preference_to_added_from_state_manager_Sanity_test_implementation
                <Product>(useNonGeneric: false);
        }

        [Fact]
        public void Non_generic_Find_derived_type_from_state_manager_in_unchanged_state_even_though_its_key_matches_Added_derived_entity_Sanity_test()
        {
            Find_returns_derived_entity_in_unchanged_state_in_preference_to_added_from_state_manager_Sanity_test_implementation
                <FeaturedProduct>(useNonGeneric: true);
        }

        [Fact]
        public void Non_generic_Find_Base_type_returns_derived_entity_in_unchanged_state_in_preference_to_added_from_state_manager_Sanity_test()
        {
            Find_returns_derived_entity_in_unchanged_state_in_preference_to_added_from_state_manager_Sanity_test_implementation
                <Product>(useNonGeneric: true);
        }

        private void
            Find_returns_derived_entity_in_unchanged_state_in_preference_to_added_from_state_manager_Sanity_test_implementation
            <T>(bool useNonGeneric, int keyToFind = 7)
            where T : class
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var product = context.Set<FeaturedProduct>().Find(7);
                var addedProduct = new FeaturedProduct { Id = 7, Name = "Benz", CategoryId = "Cars" };
                context.Products.Add(addedProduct);

                // Act
                var foundProduct = useNonGeneric
                                       ? (T)context.Set(typeof(T)).Find(keyToFind)
                                       : context.Set<T>().Find(keyToFind);

                // Assert
                Assert.Same(foundProduct, product);
                Assert.NotSame(foundProduct, addedProduct);
                Assert.NotNull(foundProduct);
            }
        }

        [Fact]
        public void Find_on_Derived_set_returns_unchanged_derived_entity_from_state_manager_even_if_key_matches_Added_base_type_entity()
        {
            Find_derived_type_from_state_manager_in_unchanged_state_in_preference_to_Added_base_type_entity
                <FeaturedProduct>(useNonGeneric: false);
        }

        [Fact]
        public void Find_on_Base_set_returns_unchanged_derived_entity_from_state_manager_even_if_key_matches_Added_base_type_entity()
        {
            Find_derived_type_from_state_manager_in_unchanged_state_in_preference_to_Added_base_type_entity<Product>(
                useNonGeneric: false);
        }

        [Fact]
        public void Non_generic_Find_on_Derived_set_returns_unchanged_derived_entity_from_state_manager_even_if_key_matches_Added_base_type_entity()
        {
            Find_derived_type_from_state_manager_in_unchanged_state_in_preference_to_Added_base_type_entity
                <FeaturedProduct>(useNonGeneric: true);
        }

        [Fact]
        public void Non_generic_Find_on_Base_set_returns_unchanged_derived_entity_from_state_manager_even_if_key_matches_Added_base_type_entity()
        {
            Find_derived_type_from_state_manager_in_unchanged_state_in_preference_to_Added_base_type_entity<Product>(
                useNonGeneric: true);
        }

        private void Find_derived_type_from_state_manager_in_unchanged_state_in_preference_to_Added_base_type_entity<T>(
            bool useNonGeneric, int keyToFind = 7)
            where T : class
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var unchangedProduct = context.Set<FeaturedProduct>().Find(7);
                var addedProduct = new Product() { Id = 7, Name = "Nutella" };
                context.Products.Add(addedProduct);

                // Act
                dynamic foundProduct = useNonGeneric
                                           ? (T)context.Set(typeof(T)).Find(keyToFind)
                                           : context.Set<T>().Find(keyToFind);

                // Assert
                Assert.NotNull(foundProduct);
                Assert.Same(unchangedProduct, foundProduct);
                Assert.NotSame(unchangedProduct, addedProduct);
                Assert.Equal("Cadillac", foundProduct.Name);
                Assert.Equal(7, foundProduct.Id);
            }
        }

        [Fact]
        public void Find_added_entity_with_string_key_with_default_value_of_null()
        {
            Find_added_entity_with_string_key_with_default_value_of_null_implementation(c => c.Categories.Find(null));
        }

        [Fact]
        public void Non_generic_Find_added_entity_with_string_key_with_default_value_of_null()
        {
            Find_added_entity_with_string_key_with_default_value_of_null_implementation(
                c => c.Set(typeof(Category)).Find(null));
        }

        private void Find_added_entity_with_string_key_with_default_value_of_null_implementation(
            Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var newCategory = new Category { Id = null, DetailedDescription = "Cakes" };
                context.Categories.Add(newCategory);

                // Act
                var category = (Category)find(context);

                // Assert
                Assert.Same(category, newCategory);
            }
        }

        [Fact]
        public void Find_for_base_type_can_return_derived_type()
        {
            Find_for_base_type_can_return_derived_type_implementation(c => c.Products.Find(7));
        }

        [Fact]
        public void Non_generic_Find_for_base_type_can_return_derived_type()
        {
            Find_for_base_type_can_return_derived_type_implementation(c => c.Set(typeof(Product)).Find(7));
        }

        private void Find_for_base_type_can_return_derived_type_implementation(Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                var product = (Product)find(context);

                Assert.NotNull(product);
                Assert.IsType<FeaturedProduct>(product);
                Assert.Equal(7, product.Id);
                Assert.Equal(EntityState.Unchanged, GetStateEntry(context, product).State);
                Assert.Equal(1, GetStateEntries(context).Count());
            }
        }

        [Fact]
        public void Find_returns_null_for_null_key_values_in_array()
        {
            Find_returns_null_for_null_key_values_in_array_implementation(c => c.Products.Find(new object[] { null }));
        }

        [Fact]
        public void Non_generic_Find_returns_null_for_null_key_values_in_array()
        {
            Find_returns_null_for_null_key_values_in_array_implementation(
                c => c.Set(typeof(Product)).Find(new object[] { null }));
        }

        private void Find_returns_null_for_null_key_values_in_array_implementation(Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                var product = find(context);

                Assert.Null(product);
                Assert.Equal(0, GetStateEntries(context).Count());
            }
        }

        [Fact]
        public void Find_on_Derived_Set_a_derived_entity_that_lives_in_store_Sanity_test()
        {
            using (var context = new SimpleModelContext())
            {
                // Act
                var product = context.Set<FeaturedProduct>().Find(7);

                // Assert
                Assert.NotNull(product);
                Assert.Equal(7, product.Id);
            }
        }

        [Fact]
        public void Find_on_Derived_Set_a_derived_entity_that_lives_in_state_manager_Sanity_test()
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var product = context.Set<FeaturedProduct>().Find(7);

                // Act
                var product2 = context.Set<FeaturedProduct>().Find(7);

                // Assert
                Assert.Same(product2, product);
                Assert.NotNull(product2);
                Assert.Equal(7, product.Id);
            }
        }

        [Fact]
        public void Find_derived_entity_in_added_state_from_state_manager_Sanity_test()
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var newProduct = new FeaturedProduct { Id = 7, Name = "Red Bull", CategoryId = "Beverages" };
                context.Products.Add(newProduct);

                // Act
                var product = context.Set<FeaturedProduct>().Find(7);

                // Assert
                Assert.Same(product, newProduct);
                Assert.NotNull(product);
                Assert.Equal(7, product.Id);
            }
        }

        [Fact]
        public void Find_an_entity_which_was_just_detected_by_Detect_Changes_in_the_Find_Call()
        {
            Find_an_entity_which_was_just_detected_by_Detect_Changes_in_the_Find_Call_implementation(
                c => c.Products.Find(-55));
        }

        [Fact]
        public void Non_generic_Find_an_entity_which_was_just_detected_by_Detect_Changes_in_the_Find_Call()
        {
            Find_an_entity_which_was_just_detected_by_Detect_Changes_in_the_Find_Call_implementation(
                c => c.Set(typeof(Product)).Find(-55));
        }

        private void Find_an_entity_which_was_just_detected_by_Detect_Changes_in_the_Find_Call_implementation(
            Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                var category = new Category { Id = "Cakes", DetailedDescription = "Cakes And Bakes" };
                context.Categories.Attach(category);

                var product = new Product { Id = -55, Name = "Red Velvet" };
                category.Products.Add(product);

                // Act
                var foundProduct = (Product)find(context);

                // Assert
                Assert.Same(foundProduct, product);
                Assert.Equal(foundProduct.Id, -55);
            }
        }

        [Fact]
        public void Find_treats_null_array_as_single_null_value()
        {
            Find_treats_null_array_as_single_null_value_implementation(c => c.Products.Find(null));
        }

        [Fact]
        public void Non_generic_Find_treats_null_array_as_single_null_value()
        {
            Find_treats_null_array_as_single_null_value_implementation(c => c.Set(typeof(Product)).Find(null));
        }

        private void Find_treats_null_array_as_single_null_value_implementation(Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                var product = find(context);

                Assert.Null(product);
                Assert.Equal(0, GetStateEntries(context).Count());
            }
        }

        #endregion

        #region Find entity with all supported Key Types

        // Int, string, binary have been covered earlier, so this covers the other valid primitive types
        [Fact]
        public void Find_entity_with_bool_key_from_store()
        {
            Find_entity_with_key_from_store_implementation<BoolKeyEntity>(true);
        }

        [Fact]
        public void Find_entity_with_float_key_from_store()
        {
            Find_entity_with_key_from_store_implementation<FloatKeyEntity>(33.2F);
        }

        [Fact]
        public void Find_entity_with_decimal_key_from_store()
        {
            Find_entity_with_key_from_store_implementation<DecimalKeyEntity>(300.5m);
        }

        [Fact]
        public void Find_entity_with_double_key_from_store()
        {
            Find_entity_with_key_from_store_implementation<DoubleKeyEntity>(1.7E+3D);
        }

        [Fact]
        public void Find_entity_with_long_key_from_store()
        {
            Find_entity_with_key_from_store_implementation<LongKeyEntity>(4294967296L);
        }

        [Fact]
        public void Find_entity_with_short_key_from_store()
        {
            Find_entity_with_key_from_store_implementation<ShortKeyEntity>((short)32767);
        }

        [Fact]
        public void Find_entity_with_byte_key_from_store()
        {
            Find_entity_with_key_from_store_implementation<ByteKeyEntity>((byte)255);
        }

        [Fact]
        public void Find_entity_with_Guid_key_from_store()
        {
            Find_entity_with_key_from_store_implementation<GuidKeyEntity>(
                Guid.Parse("F9168C5E-CEB2-4faa-B6BF-329BF39FA1E4"));
        }

        [Fact]
        public void Find_entity_with_TimeSpan_key_from_store()
        {
            Find_entity_with_key_from_store_implementation<TimeSpanKeyEntity>(new TimeSpan(2, 14, 18));
        }

        [Fact]
        public void Find_entity_with_DateTime_key_from_store()
        {
            Find_entity_with_key_from_store_implementation<DateTimeKeyEntity>(new DateTime(2008, 5, 1, 8, 30, 52));
        }

        [Fact]
        public void Find_entity_with_DateTimeOffset_key_from_store()
        {
            Find_entity_with_key_from_store_implementation<DateTimeOffSetKeyEntity>(
                new DateTimeOffset(new DateTime(2008, 5, 1, 8, 30, 52)));
        }

        private void Find_entity_with_key_from_store_implementation<T>(object value)
            where T : class
        {
            using (var context = new AllTypeKeysContext())
            {
                // Act
                dynamic found = context.Set<T>().Find(value);

                // Assert
                Assert.NotNull(found);
                Assert.True(StructuralComparisons.StructuralEqualityComparer.Equals(found.key, value));
                Assert.Equal(found.Description, typeof(T).Name);
            }
        }

        #endregion

        #region Composite Keys

        [Fact]
        public void Find_an_entity_with_Composite_Key_from_store()
        {
            Find_an_entity_with_Composite_Key_from_store_implementation(
                c => c.CompositeKeyEntities.Find(new byte[] { 201, 202, 203, 204 }, 4, "TheOneWithBinaryKeyLength4"));
        }

        [Fact]
        public void Non_generic_Find_an_entity_with_Composite_Key_from_store()
        {
            Find_an_entity_with_Composite_Key_from_store_implementation(
                c =>
                c.Set(typeof(CompositeKeyEntity)).Find(new byte[] { 201, 202, 203, 204 }, 4,
                                                       "TheOneWithBinaryKeyLength4"));
        }

        private void Find_an_entity_with_Composite_Key_from_store_implementation(Func<AllTypeKeysContext, object> find)
        {
            using (var context = new AllTypeKeysContext())
            {
                // Act
                var foundEntity = (CompositeKeyEntity)find(context);

                // Assert
                Assert.NotNull(foundEntity);
            }
        }

        [Fact]
        public void Find_an_entity_with_Composite_Key_from_state_manager_unchanged()
        {
            Find_an_entity_with_Composite_Key_from_state_manager_unchanged_implementation(
                c => c.CompositeKeyEntities.Find(new byte[] { 1, 2, 3, 4, 5, 6 }, 6, "TheOneWithBinaryKeyLength6"));
        }

        [Fact]
        public void Non_generic_Find_an_entity_with_Composite_Key_from_state_manager_unchanged()
        {
            Find_an_entity_with_Composite_Key_from_state_manager_unchanged_implementation(
                c =>
                c.Set(typeof(CompositeKeyEntity)).Find(new byte[] { 1, 2, 3, 4, 5, 6 }, 6, "TheOneWithBinaryKeyLength6"));
        }

        private void Find_an_entity_with_Composite_Key_from_state_manager_unchanged_implementation(
            Func<AllTypeKeysContext, object> find)
        {
            using (var context = new AllTypeKeysContext())
            {
                // Arrange
                var foundEntity = context.CompositeKeyEntities.Find(new byte[] { 1, 2, 3, 4, 5, 6 }, 6,
                                                                    "TheOneWithBinaryKeyLength6");

                // Act
                var foundEntity2 = (CompositeKeyEntity)find(context);

                // Assert
                Assert.NotNull(foundEntity2);
                Assert.Same(foundEntity2, foundEntity);
            }
        }

        [Fact]
        public void Find_an_entity_with_Composite_Key_from_state_manager_Added()
        {
            Find_an_entity_with_Composite_Key_from_state_manager_Added_implementation(
                c => c.CompositeKeyEntities.Find(new byte[] { 1, 2, 3, 4, 5, 6 }, 6, "TheOneWithBinaryKeyLength6"));
        }

        [Fact]
        public void Non_generic_Find_an_entity_with_Composite_Key_from_state_manager_Added()
        {
            Find_an_entity_with_Composite_Key_from_state_manager_Added_implementation(
                c =>
                c.Set(typeof(CompositeKeyEntity)).Find(new byte[] { 1, 2, 3, 4, 5, 6 }, 6, "TheOneWithBinaryKeyLength6"));
        }

        private void Find_an_entity_with_Composite_Key_from_state_manager_Added_implementation(
            Func<AllTypeKeysContext, object> find)
        {
            using (var context = new AllTypeKeysContext())
            {
                // Arrange
                var compositeKeyEntity = new CompositeKeyEntity
                                         {
                                             intKey = 6,
                                             binaryKey = new byte[] { 1, 2, 3, 4, 5, 6 },
                                             stringKey = "TheOneWithBinaryKeyLength6"
                                         };
                context.CompositeKeyEntities.Add(compositeKeyEntity);

                // Act
                var foundEntity = (CompositeKeyEntity)find(context);

                // Assert
                Assert.NotNull(foundEntity);
                Assert.Same(foundEntity, compositeKeyEntity);
            }
        }

        [Fact]
        public void Find_an_entity_with_Composite_Key_With_Key_Annotations_With_DataMember_Ordering_from_store()
        {
            using (var context = new AllTypeKeysContext())
            {
                var foundEntity = context.CompositeKeyEntitiesWithOrderingAnnotations.Find(3,
                                                                                           "TheOneWithBinaryKeyLength3",
                                                                                           new byte[] { 230, 231, 232 });

                Assert.NotNull(foundEntity);
                Assert.Equal(3, foundEntity.intKey);
                Assert.Equal("TheOneWithBinaryKeyLength3", foundEntity.stringKey);
                Assert.True(new byte[] { 230, 231, 232 }.SequenceEqual(foundEntity.binaryKey));
            }
        }

        [Fact]
        public void Find_an_entity_with_Composite_Key_With_Key_Annotations_With_DataMember_Ordering_from_state_manager_unchanged()
        {
            using (var context = new AllTypeKeysContext())
            {
                // Arrange
                var foundEntity = context.CompositeKeyEntitiesWithOrderingAnnotations.Find(3,
                                                                                           "TheOneWithBinaryKeyLength3",
                                                                                           new byte[] { 230, 231, 232 });

                // Act
                // The ordering defined in the data annotation is Int, String, Binary
                var foundEntity2 = context.CompositeKeyEntitiesWithOrderingAnnotations.Find(3,
                                                                                            "TheOneWithBinaryKeyLength3",
                                                                                            new byte[] { 230, 231, 232 });

                // Assert
                Assert.NotNull(foundEntity2);
                Assert.Same(foundEntity2, foundEntity);
            }
        }

        [Fact]
        public void
            Find_an_entity_with_Composite_Key_With_Key_Annotations_With_DataMember_Ordering_from_state_manager_added()
        {
            using (var context = new AllTypeKeysContext())
            {
                // Arrange
                var compositeEntity = new CompositeKeyEntityWithOrderingAnnotations
                                      {
                                          intKey = 3,
                                          stringKey = "TheOneWithBinaryKeyLength3",
                                          binaryKey = new byte[] { 230, 231, 232 }
                                      };
                context.CompositeKeyEntitiesWithOrderingAnnotations.Add(compositeEntity);

                // Act, the ordering defined in the data annotation is Int, String, Binary
                var foundEntity = context.CompositeKeyEntitiesWithOrderingAnnotations.Find(3,
                                                                                           "TheOneWithBinaryKeyLength3",
                                                                                           new byte[] { 230, 231, 232 });

                // Assert
                Assert.NotNull(foundEntity);
                Assert.Same(foundEntity, compositeEntity);
            }
        }

        #endregion

        #region Simple negative cases

        [Fact]
        public void Find_throws_for_wrong_number_of_key_values()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Throws<ArgumentException>(() => context.Products.Find(1, 2)).ValidateMessage(
                    "DbSet_WrongNumberOfKeyValuesPassed", "keyValues");
            }
        }

        [Fact]
        public void Non_generic_Find_throws_for_wrong_number_of_key_values()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Throws<ArgumentException>(() => context.Set(typeof(Product)).Find(1, 2)).ValidateMessage(
                    "DbSet_WrongNumberOfKeyValuesPassed", "keyValues");
            }
        }

        [Fact]
        public void Find_throws_for_zero_key_values()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Throws<ArgumentException>(() => context.Products.Find()).ValidateMessage(
                    "DbSet_WrongNumberOfKeyValuesPassed", "keyValues");
            }
        }

        [Fact]
        public void Non_generic_Find_throws_for_zero_key_values()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Throws<ArgumentException>(() => context.Set(typeof(Product)).Find()).ValidateMessage(
                    "DbSet_WrongNumberOfKeyValuesPassed", "keyValues");
            }
        }

        [Fact]
        public void Find_throws_for_key_values_of_wrong_type()
        {
            Find_throws_for_key_values_of_wrong_type_implementation(c => c.Products.Find("Cetina, Aragon"));
        }

        [Fact]
        public void Non_generic_Find_throws_for_key_values_of_wrong_type()
        {
            Find_throws_for_key_values_of_wrong_type_implementation(c => c.Set(typeof(Product)).Find("Cetina, Aragon"));
        }

        private void Find_throws_for_key_values_of_wrong_type_implementation(Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                try
                {
                    find(context);
                    Assert.True(false);
                }
                catch (ArgumentException ex)
                {
                    Assert.Equal("keyValues", ex.ParamName);
                    var withNoParam = ex.Message.Substring(0, ex.Message.LastIndexOf("\r\n"));

                    Assert.IsType<EntitySqlException>(ex.InnerException);
                }
            }
        }

        [Fact]
        public void Find_throws_if_multiple_Added_entities_match_key()
        {
            using (var context = new SimpleModelContext())
            {
                context.Products.Add(new Product() { Id = 1, Name = "Coke" });
                context.Products.Add(new Product() { Id = 1, Name = "Pepsi" });
                Assert.Throws<InvalidOperationException>(() => context.Products.Find(1)).ValidateMessage(
                    "DbSet_MultipleAddedEntitiesFound");
            }
        }

        [Fact]
        public void Find_throws_on_disposed_context()
        {
            Find_throws_on_disposed_context_implementation(c => c.Products.Find(1));
        }

        [Fact]
        public void Non_generic_Find_throws_on_disposed_context()
        {
            Find_throws_on_disposed_context_implementation(c => c.Set(typeof(Product)).Find(1));
        }

        private void Find_throws_on_disposed_context_implementation(Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                context.Products.Add(new Product() { Id = 1, Name = "Coke" });
                context.Dispose();
                Assert.Throws<InvalidOperationException>(() => find(context)).ValidateMessage("DbContext_Disposed");
            }
        }

        private class DummyEntity
        {
        }

        [Fact]
        public void Find_throws_when_type_not_valid_for_current_context()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Throws<InvalidOperationException>(() => context.Set<DummyEntity>().Find(1)).ValidateMessage(
                    "DbSet_EntityTypeNotInModel", "DummyEntity");
            }
        }

        [Fact]
        public void Non_generic_Find_throws_when_type_not_valid_for_current_context()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Throws<InvalidOperationException>(() => context.Set(typeof(DummyEntity)).Find(1)).ValidateMessage
                    ("DbSet_EntityTypeNotInModel", "DummyEntity");
            }
        }

        [Fact]
        public void Find_throws_when_type_is_complex_type_which_is_not_valid_for_current_context()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                Assert.Throws<InvalidOperationException>(() => context.Set<Address>().Find(1)).ValidateMessage(
                    "DbSet_DbSetUsedWithComplexType", "Address");
            }
        }

        [Fact]
        public void Non_generic_Find_throws_when_type_is_complex_type_which_is_not_valid_for_current_context()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                Assert.Throws<InvalidOperationException>(() => context.Set(typeof(Address)).Find(1)).ValidateMessage(
                    "DbSet_DbSetUsedWithComplexType", "Address");
            }
        }

        [Fact]
        public void Find_throws_if_requested_type_does_not_match_actual_type_in_store()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Throws<InvalidOperationException>(() => context.Set<FeaturedProduct>().Find(1)).ValidateMessage(
                    "Materializer_InvalidCastReference", 
                    "SimpleModel.Product", "SimpleModel.FeaturedProduct");
                Assert.Equal(0, GetStateEntries(context).Count());
            }
        }

        [Fact]
        public void Non_generic_Find_throws_if_requested_type_does_not_match_actual_type_in_store()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Throws<InvalidOperationException>(() => context.Set(typeof(FeaturedProduct)).Find(1)).
                    ValidateMessage("Materializer_InvalidCastReference", 
                                    "SimpleModel.Product", "SimpleModel.FeaturedProduct");
                Assert.Equal(0, GetStateEntries(context).Count());
            }
        }

        [Fact]
        public void Find_throws_if_requested_type_does_not_match_actual_type_in_state_manager()
        {
            Find_throws_if_requested_type_does_not_match_actual_type_in_state_manager_implementation(
                c => c.Set<FeaturedProduct>().Find(1));
        }

        [Fact]
        public void Non_generic_Find_throws_if_requested_type_does_not_match_actual_type_in_state_manager()
        {
            Find_throws_if_requested_type_does_not_match_actual_type_in_state_manager_implementation(
                c => c.Set(typeof(FeaturedProduct)).Find(1));
        }

        private void Find_throws_if_requested_type_does_not_match_actual_type_in_state_manager_implementation(
            Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                context.Products.Find(1);

                // Act
                Assert.Throws<InvalidOperationException>(() => find(context)).ValidateMessage(
                    "DbSet_WrongEntityTypeFound", "Product", "FeaturedProduct");
            }
        }

        [Fact]
        public void Find_throws_if_requested_type_does_not_match_actual_Added_type_in_state_manager()
        {
            Find_throws_if_requested_type_does_not_match_actual_Added_type_in_state_manager_implementation(
                c => c.Set<FeaturedProduct>().Find(-1));
        }

        [Fact]
        public void Non_generic_Find_throws_if_requested_type_does_not_match_actual_Added_type_in_state_manager()
        {
            Find_throws_if_requested_type_does_not_match_actual_Added_type_in_state_manager_implementation(
                c => c.Set(typeof(FeaturedProduct)).Find(-1));
        }

        private void Find_throws_if_requested_type_does_not_match_actual_Added_type_in_state_manager_implementation(
            Func<SimpleModelContext, object> find)
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                context.Products.Add(new Product() { Id = -1, Name = "Green Jello" });

                // Act
                Assert.Throws<InvalidOperationException>(() => find(context)).ValidateMessage(
                    "DbSet_WrongEntityTypeFound", "Product", "FeaturedProduct");
            }
        }

        [Fact]
        public void Find_in_derived_set_when_matching_base_type_lives_in_state_manager_in_added_state_and_matching_derived_type_lives_in_store()
        {
            Find_derived_entity_when_matching_base_type_lives_in_state_manager_in_added_unchanged_or_deleted_state_and_matching_derived_type_lives_in_store
                (EntityState.Added);
        }

        [Fact]
        public void Find_in_derived_set_when_matching_base_type_lives_in_state_manager_in_unchanged_state_and_matching_derived_type_lives_in_store()
        {
            Find_derived_entity_when_matching_base_type_lives_in_state_manager_in_added_unchanged_or_deleted_state_and_matching_derived_type_lives_in_store
                (EntityState.Unchanged);
        }

        [Fact]
        public void Find_in_derived_set_when_matching_base_type_lives_in_state_manager_in_deleted_state_and_matching_derived_type_lives_in_store()
        {
            Find_derived_entity_when_matching_base_type_lives_in_state_manager_in_added_unchanged_or_deleted_state_and_matching_derived_type_lives_in_store
                (EntityState.Deleted);
        }

        private void
            Find_derived_entity_when_matching_base_type_lives_in_state_manager_in_added_unchanged_or_deleted_state_and_matching_derived_type_lives_in_store
            (EntityState entityState)
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                // There is a derived Featured Product with Id 7 stored in the DB.
                var product = new Product { Id = 7, Name = "Benz", CategoryId = "Cars" };

                switch (entityState)
                {
                    case EntityState.Added:
                        context.Products.Add(product);
                        break;
                    case EntityState.Deleted:
                        context.Products.Attach(product);
                        context.Products.Remove(product);
                        break;
                    case EntityState.Unchanged:
                        context.Products.Attach(product);
                        break;
                    default:
                        Assert.True(false, "Invalid Entity State for this test" + entityState);
                        break;
                }

                // Assert state of entity is as expected
                Assert.True(GetStateEntry(context, product).State == entityState);

                // Act and Assert
                Assert.Throws<InvalidOperationException>(() => context.Set<FeaturedProduct>().Find(7)).ValidateMessage(
                    "DbSet_WrongEntityTypeFound", "Product", "FeaturedProduct");
            }
        }

        [Fact]
        public void Find_in_derived_set_when_matching_base_type_entity_lives_in_unchanged_and_matching_derived_type_lives_in_added_state_in_state_manager()
        {
            using (var context = new SimpleModelContext())
            {
                // Arrange
                // There is a Featured Product with Id 7 stored in the DB, bring it to the Added state
                var actualProduct = context.Products.Find(7);
                context.Products.Add(actualProduct);

                var product = new Product { Id = 7, Name = "Benz", CategoryId = "Cars" };
                context.Set<Product>().Attach(product);
                
                // Act and Assert
                Assert.Throws<InvalidOperationException>(() => context.Set<FeaturedProduct>().Find(7)).ValidateMessage(
                    "DbSet_WrongEntityTypeFound", "Product", "FeaturedProduct");
            }
        }

        [Fact]
        public void Find_an_entity_with_Composite_Key_with_wrong_order_of_Keys()
        {
            // The right ordering is binary, int, string
            Find_an_entity_with_Composite_Key_with_wrong_order_of_Keys_implementation(
                c => c.CompositeKeyEntities.Find(4, new byte[] { 201, 202, 203, 204 }, "TheOneWithBinaryKeyLength4"),
                findInStateManager: false);
        }

        [Fact]
        public void Non_generic_Find_an_entity_with_Composite_Key_with_wrong_order_of_Keys()
        {
            // The right ordering is binary, int, string
            Find_an_entity_with_Composite_Key_with_wrong_order_of_Keys_implementation(
                c =>
                c.Set(typeof(CompositeKeyEntity)).Find(4, new byte[] { 201, 202, 203, 204 },
                                                       "TheOneWithBinaryKeyLength4"), findInStateManager: false);
        }

        [Fact]
        public void Find_an_entity_already_in_the_state_manager_with_Composite_Key_with_wrong_order_of_Keys()
        {
            // The right ordering is binary, int, string
            Find_an_entity_with_Composite_Key_with_wrong_order_of_Keys_implementation(
                c => c.CompositeKeyEntities.Find(4, new byte[] { 201, 202, 203, 204 }, "TheOneWithBinaryKeyLength4"),
                findInStateManager: true);
        }

        [Fact]
        public void Non_generic_Find_an_entity_already_in_the_state_manager_with_Composite_Key_with_wrong_order_of_Keys()
        {
            // The right ordering is binary, int, string
            Find_an_entity_with_Composite_Key_with_wrong_order_of_Keys_implementation(
                c =>
                c.Set(typeof(CompositeKeyEntity)).Find(4, new byte[] { 201, 202, 203, 204 },
                                                       "TheOneWithBinaryKeyLength4"), findInStateManager: true);
        }

        private void Find_an_entity_with_Composite_Key_with_wrong_order_of_Keys_implementation(
            Func<AllTypeKeysContext, object> find, bool findInStateManager)
        {
            using (var context = new AllTypeKeysContext())
            {
                if (findInStateManager)
                {
                    var actual = context.CompositeKeyEntities.Find(new byte[] { 201, 202, 203, 204 }, 4,
                                                                   "TheOneWithBinaryKeyLength4");
                    Assert.NotNull(actual);
                }
                try
                {
                    find(context);
                    Assert.True(false);
                }
                catch (ArgumentException ex)
                {
                    Assert.Equal("keyValues", ex.ParamName);
                    var withNoParam = ex.Message.Substring(0, ex.Message.LastIndexOf("\r\n"));
                    new StringResourceVerifier(new AssemblyResourceLookup(EntityFrameworkAssembly,
                                                                          "System.Data.Entity.Properties.Resources")).
                        VerifyMatch("DbSet_WrongKeyValueType", withNoParam);

                    Assert.IsType<EntitySqlException>(ex.InnerException);
                }

                Assert.Equal(findInStateManager ? 1 : 0, GetStateEntries(context).Count());
            }
        }

        [Fact]
        public void Find_returns_added_entity_which_contains_partially_null_composite_key()
        {
            using (var context = new AllTypeKeysContext())
            {
                var compositeKeyEntity = new CompositeKeyEntity
                                         { intKey = 6, binaryKey = null, stringKey = "TheOneWithNullBinaryKey" };
                context.CompositeKeyEntities.Add(compositeKeyEntity);

                var foundEntity = context.CompositeKeyEntities.Find(null, 6, "TheOneWithNullBinaryKey");

                Assert.NotNull(foundEntity);
                Assert.Same(foundEntity, compositeKeyEntity);
            }
        }

        [Fact]
        public void Find_returns_null_when_no_added_entity_which_contains_partially_null_composite_key_is_found()
        {
            using (var context = new AllTypeKeysContext())
            {
                Assert.Null(context.CompositeKeyEntities.Find(null, 6, "TheOneWithNullBinaryKey"));
            }
        }

        [Fact]
        public void Find_an_entity_with_Composite_Key_With_Key_Annotations_With_Wrong_Ordering_defined()
        {
            using (var context = new AllTypeKeysContext())
            {
                try
                {
                    // Right ordering is integer, string, binary
                    var foundEntity = context.CompositeKeyEntitiesWithOrderingAnnotations.Find(
                        new byte[] { 1, 2, 3, 4 }, "Composite1", 1);
                    Assert.True(false);
                }
                catch (ArgumentException ex)
                {
                    Assert.Equal("keyValues", ex.ParamName);
                    var withNoParam = ex.Message.Substring(0, ex.Message.LastIndexOf("\r\n"));
                    new StringResourceVerifier(new AssemblyResourceLookup(EntityFrameworkAssembly,
                                                                          "System.Data.Entity.Properties.Resources")).
                        VerifyMatch("DbSet_WrongKeyValueType", withNoParam);

                    Assert.IsType<EntitySqlException>(ex.InnerException);
                }

                Assert.Equal(0, GetStateEntries(context).Count());
            }
        }

        [Fact]
        public void Find_an_entity_with_Composite_Key_With_No_Ordering_Defined()
        {
            using (var context = new NoOrderingContext())
            {
                // context.CompositeKeyEntities.Find(new byte[] { 1, 2, 3, 4 }, 2.3F, 1);
                Assert.Throws<InvalidOperationException>(
                    () => context.CompositeKeyEntities.Find(new byte[] { 1, 2, 3, 4 }, 2.3F, 1)).ValidateMessage(
                        "ModelGeneration_UnableToDetermineKeyOrder",
                        typeof(CompositeKeyEntityWithNoOrdering).ToString());
            }
        }

        [Fact]
        public void Find_composite_entity_by_specifying_some_of_its_keys_in_an_array()
        {
            using (var context = new AllTypeKeysContext())
            {
                Assert.Throws<ArgumentException>(
                    () =>
                    context.CompositeKeyEntitiesWithOrderingAnnotations.Find(
                        new object[] { 2, "TheOneWithBinaryKeyLength2" }, new byte[] { 220, 221 })).ValidateMessage(
                            "DbSet_WrongNumberOfKeyValuesPassed", "keyValues");
            }
        }

        #endregion
    }
}