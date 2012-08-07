// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    using System.Transactions;
    using AdvancedPatternsModel;
    using ConcurrencyModel;
    using SimpleModel;
    using Xunit;

    /// <summary>
    ///     General functional tests for DbEntityEntry and related classes/methods.
    ///     Unit tests also exist in the unit tests project.
    /// </summary>
    public class DbEntityEntryTests : FunctionalTestBase
    {
        #region Tests for getting entity entries from a context

        private void FindEntryTest(EntityState state)
        {
            using (var context = new SimpleModelContext())
            {
                object product = context.Products.Find(1);
                ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager.ChangeObjectState(product, state);

                var entry = context.Entry(product);

                Assert.NotNull(entry);
                Assert.Same(product, entry.Entity);
                Assert.Equal(state, entry.State);
            }
        }

        [Fact]
        public void DbContext_Entry_can_find_entry_for_Unchanged_entity()
        {
            FindEntryTest(EntityState.Unchanged);
        }

        [Fact]
        public void DbContext_Entry_can_find_entry_for_Modified_entity()
        {
            FindEntryTest(EntityState.Modified);
        }

        [Fact]
        public void DbContext_Entry_can_find_entry_for_Added_entity()
        {
            FindEntryTest(EntityState.Added);
        }

        [Fact]
        public void DbContext_Entry_can_find_entry_for_Deleted_entity()
        {
            FindEntryTest(EntityState.Deleted);
        }

        [Fact]
        public void DbContext_Entry_can_find_entry_for_Detached_entity()
        {
            FindEntryTest(EntityState.Detached);
        }

        private void GenericFindEntryTest(EntityState state)
        {
            using (var context = new SimpleModelContext())
            {
                var product = context.Products.Find(1);
                ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager.ChangeObjectState(product, state);

                var entry = context.Entry(product);

                Assert.NotNull(entry);
                Assert.Same(product, entry.Entity);
                Assert.Equal(state, entry.State);
            }
        }

        [Fact]
        public void Generic_DbContext_Entry_can_find_entry_for_Unchanged_entity()
        {
            GenericFindEntryTest(EntityState.Unchanged);
        }

        [Fact]
        public void Generic_DbContext_Entry_can_find_entry_for_Modified_entity()
        {
            GenericFindEntryTest(EntityState.Modified);
        }

        [Fact]
        public void Generic_DbContext_Entry_can_find_entry_for_Added_entity()
        {
            GenericFindEntryTest(EntityState.Added);
        }

        [Fact]
        public void Generic_DbContext_Entry_can_find_entry_for_Deleted_entity()
        {
            GenericFindEntryTest(EntityState.Deleted);
        }

        [Fact]
        public void Generic_DbContext_Entry_can_find_entry_for_Detached_entity()
        {
            GenericFindEntryTest(EntityState.Detached);
        }

        [Fact]
        public void DbContext_Entries_returns_entries_for_all_types_of_tracked_entity_in_all_states()
        {
            using (var context = new SimpleModelContext())
            {
                context.Products.Include(p => p.Category).Load();

                var unchanged = context.Products.Find(1);

                var deleted = context.Products.Find(2);
                context.Products.Remove(deleted);

                var modified = context.Products.Find(3);
                modified.Name = "Bovril";

                var added = new Product();
                context.Products.Add(added);

                var totalEntities = context.Products.Local.Count + context.Categories.Local.Count + 1;

                var entries = context.ChangeTracker.Entries().ToList();

                Assert.Equal(totalEntities, entries.Count);
                Assert.True(entries.Any(e => e.Entity is Product));
                Assert.True(entries.Any(e => e.Entity is FeaturedProduct));
                Assert.True(entries.Any(e => e.Entity is Category));

                Assert.Equal(1, entries.Count(e => e.State == EntityState.Unchanged && e.Entity == unchanged));
                Assert.Equal(1, entries.Count(e => e.State == EntityState.Deleted && e.Entity == deleted));
                Assert.Equal(1, entries.Count(e => e.State == EntityState.Modified && e.Entity == modified));
                Assert.Equal(1, entries.Count(e => e.State == EntityState.Added && e.Entity == added));
            }
        }

        [Fact]
        public void Generic_DbContext_Entries_returns_entries_for_tracked_entities_of_the_given_type_in_all_states()
        {
            using (var context = new SimpleModelContext())
            {
                context.Products.Include(p => p.Category).Load();

                var unchanged = context.Products.Find(1);

                var deleted = context.Products.Find(2);
                context.Products.Remove(deleted);

                var modified = context.Products.Find(3);
                modified.Name = "Bovril";

                var added = new Product();
                context.Products.Add(added);

                var totalEntities = context.Products.Local.Count + 1;

                var entries = context.ChangeTracker.Entries<Product>().ToList();

                Assert.Equal(totalEntities, entries.Count);
                Assert.Equal(1, entries.Count(e => e.State == EntityState.Unchanged && e.Entity == unchanged));
                Assert.Equal(1, entries.Count(e => e.State == EntityState.Deleted && e.Entity == deleted));
                Assert.Equal(1, entries.Count(e => e.State == EntityState.Modified && e.Entity == modified));
                Assert.Equal(1, entries.Count(e => e.State == EntityState.Added && e.Entity == added));
            }
        }

        [Fact]
        public void Generic_DbContext_Entries_can_be_used_with_an_unmapped_base_type()
        {
            using (var context = new SimpleModelContext())
            {
                context.Products.Include(p => p.Category).Load();

                var entries = context.ChangeTracker.Entries<ProductBase>().ToList();

                Assert.NotEqual(0, entries.Count);
                Assert.Equal(context.Products.Local.Count, entries.Count);
            }
        }

        [Fact]
        public void Generic_DbContext_Entries_can_be_used_with_a_derived_type()
        {
            using (var context = new SimpleModelContext())
            {
                context.Products.Include(p => p.Category).Load();

                var entries = context.ChangeTracker.Entries<FeaturedProduct>().ToList();

                Assert.NotEqual(0, entries.Count);
                Assert.Equal(context.Set<FeaturedProduct>().Local.Count, entries.Count);
            }
        }

        [Fact]
        public void DbContext_Entries_ignores_stubs_and_relationship_entries()
        {
            using (var context = new SimpleModelForLinq())
            {
                var nonStubEntityEntryCount = PopulateContextAndGetNonStubEntityEntryCount(context);

                var entries = context.ChangeTracker.Entries().ToList();

                Assert.True(entries.All(e => e.Entity != null));
                Assert.Equal(nonStubEntityEntryCount, entries.Count);
            }
        }

        [Fact]
        public void Generic_DbContext_Entries_ignores_stubs_and_relationship_entries()
        {
            using (var context = new SimpleModelForLinq())
            {
                var nonStubEntityEntryCount = PopulateContextAndGetNonStubEntityEntryCount(context);

                var entries = context.ChangeTracker.Entries<OrderForLinq>().ToList();

                Assert.True(entries.All(e => e.Entity != null));
                Assert.Equal(nonStubEntityEntryCount, entries.Count);
            }
        }

        private int PopulateContextAndGetNonStubEntityEntryCount(SimpleModelForLinq context)
        {
            context.Orders.Load();

            var objectStateEntries =
                ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager.GetObjectStateEntries(
                    ~EntityState.Detached);
            var relationshipEntryCount = objectStateEntries.Count(e => e.IsRelationship);
            var stubEntryCount = objectStateEntries.Count(e => !e.IsRelationship && e.Entity == null);

            // Sanity check that we actually have stubs and relationship entries
            Assert.NotEqual(0, relationshipEntryCount);
            Assert.NotEqual(0, stubEntryCount);

            return objectStateEntries.Count() - relationshipEntryCount - stubEntryCount;
        }

        #endregion

        #region Tests for changing entity state of a tracked entity using DbEntityEntry

        [Fact]
        public void DbEntityEntry_State_calls_DetectChanges_for_detached_modified_entities()
        {
            using (var context = new SimpleModelContext())
            {
                using (new TransactionScope())
                {
                    var entry = context.Entry((object)new Product());
                    context.Products.Add((Product)entry.Entity);
                    context.SaveChanges();

                    // Ensure that the entity doesn't have a change tracking proxy
                    Assert.Equal(
                        entry.Entity.GetType(),
                        ObjectContext.GetObjectType(entry.Entity.GetType()));

                    ((Product)entry.Entity).Name = "foo";

                    // DetectChanges is called the first time the state is queried for a detached entity
                    Assert.Equal(EntityState.Modified, entry.State);
                }
            }
        }

        [Fact]
        public void DbEntityEntry_State_calls_DetectChanges_for_detached_unchanged_entities()
        {
            using (var context = new SimpleModelContext())
            {
                using (new TransactionScope())
                {
                    var entry = context.Entry((object)new Product());
                    context.Products.Add((Product)entry.Entity);
                    context.SaveChanges();

                    // Ensure that the entity doesn't have a change tracking proxy
                    Assert.Equal(
                        entry.Entity.GetType(),
                        ObjectContext.GetObjectType(entry.Entity.GetType()));

                    // DetectChanges is called the first time the state is queried for a detached entity
                    Assert.Equal(EntityState.Unchanged, entry.State);

                    ((Product)entry.Entity).Name = "foo";

                    Assert.Equal(EntityState.Unchanged, entry.State);
                }
            }
        }

        [Fact]
        public void Generic_DbEntityEntry_State_calls_DetectChanges_for_detached_modified_entities()
        {
            using (var context = new SimpleModelContext())
            {
                using (new TransactionScope())
                {
                    var entry = context.Entry(new Product());
                    context.Products.Add(entry.Entity);
                    context.SaveChanges();

                    // Ensure that the entity doesn't have a change tracking proxy
                    Assert.Equal(
                        entry.Entity.GetType(),
                        ObjectContext.GetObjectType(entry.Entity.GetType()));

                    entry.Entity.Name = "foo";

                    // DetectChanges is called the first time the state is queried for a detached entity
                    Assert.Equal(EntityState.Modified, entry.State);
                }
            }
        }

        [Fact]
        public void Generic_DbEntityEntry_State_calls_DetectChanges_for_detached_unchanged_entities()
        {
            using (var context = new SimpleModelContext())
            {
                using (new TransactionScope())
                {
                    var entry = context.Entry(new Product());
                    context.Products.Add(entry.Entity);
                    context.SaveChanges();

                    // Ensure that the entity doesn't have a change tracking proxy
                    Assert.Equal(
                        entry.Entity.GetType(),
                        ObjectContext.GetObjectType(entry.Entity.GetType()));

                    // DetectChanges is called the first time the state is queried for a detached entity
                    Assert.Equal(EntityState.Unchanged, entry.State);

                    entry.Entity.Name = "foo";

                    Assert.Equal(EntityState.Unchanged, entry.State);
                }
            }
        }

        [Fact]
        public void DbEntityEntry_can_be_used_to_change_entity_state()
        {
            using (var context = new SimpleModelContext())
            {
                object product = context.Products.Find(1);
                var ose = GetStateEntry(context, product);

                Assert.Equal(EntityState.Unchanged, ose.State);

                context.Entry(product).State = EntityState.Modified;

                Assert.Equal(EntityState.Modified, ose.State);
            }
        }

        [Fact]
        public void Generic_DbEntityEntry_can_be_used_to_change_entity_state()
        {
            using (var context = new SimpleModelContext())
            {
                var product = context.Products.Find(1);
                var ose = GetStateEntry(context, product);

                Assert.Equal(EntityState.Unchanged, ose.State);

                context.Entry(product).State = EntityState.Modified;

                Assert.Equal(EntityState.Modified, ose.State);
            }
        }

        [Fact]
        public void Changing_state_from_Modified_to_Unchanged_does_RejectChanges()
        {
            using (var context = new SimpleModelContext())
            {
                // Setup a modified entity, including a modified FK
                var product = context.Products
                    .Where(p => p.Name == "Marmite")
                    .Include(p => p.Category)
                    .Single();
                var foods = product.Category;
                var beverages = context.Categories.Find("Beverages");

                var ose = GetStateEntry(context, product);

                var nameProp = context.Entry(product).Property(p => p.Name);
                var catIdProp = context.Entry(product).Property(p => p.CategoryId);

                nameProp.CurrentValue = "Magic Unicorn Brew";
                catIdProp.CurrentValue = "Beverages";

                Assert.True(nameProp.IsModified);
                Assert.True(catIdProp.IsModified);
                Assert.Same(beverages, product.Category);
                Assert.Equal(EntityState.Modified, ose.State);

                // Change the state back to Unchanged to reject changes
                context.Entry(product).State = EntityState.Unchanged;

                // Verify the changes have been rejected
                Assert.Equal(EntityState.Unchanged, ose.State);
                Assert.False(nameProp.IsModified);
                Assert.False(catIdProp.IsModified);
                Assert.Equal("Marmite", product.Name);
                Assert.Equal("Foods", product.CategoryId);
                Assert.Same(foods, product.Category);
            }
        }

        #endregion

        #region Tests using entries for detached entities

        [Fact]
        public void Stand_alone_DbEntityEntry_can_be_used_to_Add_an_entity()
        {
            Stand_alone_DbEntityEntry_can_be_used_to_attach_an_entity_to_the_context_and_set_its_state(EntityState.Added);
        }

        [Fact]
        public void Stand_alone_DbEntityEntry_can_be_used_to_Attach_an_entity()
        {
            Stand_alone_DbEntityEntry_can_be_used_to_attach_an_entity_to_the_context_and_set_its_state(
                EntityState.Unchanged);
        }

        [Fact]
        public void Stand_alone_DbEntityEntry_can_be_used_to_attach_an_entity_as_Modified()
        {
            Stand_alone_DbEntityEntry_can_be_used_to_attach_an_entity_to_the_context_and_set_its_state(
                EntityState.Modified);
        }

        [Fact]
        public void Stand_alone_DbEntityEntry_can_be_used_to_delete_an_entity()
        {
            Stand_alone_DbEntityEntry_can_be_used_to_attach_an_entity_to_the_context_and_set_its_state(
                EntityState.Deleted);
        }

        [Fact]
        public void Stand_alone_DbEntityEntry_can_be_used_to_Add_an_entity_when_entity_is_a_proxy()
        {
            Stand_alone_DbEntityEntry_can_be_used_to_attach_an_entity_to_the_context_and_set_its_state(
                EntityState.Added, useProxy: true);
        }

        [Fact]
        public void Stand_alone_DbEntityEntry_can_be_used_to_Attach_an_entity_when_entity_is_a_proxy()
        {
            Stand_alone_DbEntityEntry_can_be_used_to_attach_an_entity_to_the_context_and_set_its_state(
                EntityState.Unchanged, useProxy: true);
        }

        [Fact]
        public void Stand_alone_DbEntityEntry_can_be_used_to_attach_an_entity_as_Modified_when_entity_is_a_proxy()
        {
            Stand_alone_DbEntityEntry_can_be_used_to_attach_an_entity_to_the_context_and_set_its_state(
                EntityState.Modified, useProxy: true);
        }

        [Fact]
        public void Stand_alone_DbEntityEntry_can_be_used_to_delete_an_entity_when_entity_is_a_proxy()
        {
            Stand_alone_DbEntityEntry_can_be_used_to_attach_an_entity_to_the_context_and_set_its_state(
                EntityState.Deleted, useProxy: true);
        }

        private void Stand_alone_DbEntityEntry_can_be_used_to_attach_an_entity_to_the_context_and_set_its_state(
            EntityState state, bool useProxy = false)
        {
            using (var context = new F1Context())
            {
                var larry = useProxy ? context.Drivers.Create() : new Driver();
                larry.Name = "Larry David";
                larry.TeamId = Team.Ferrari;

                var entry = context.Entry(larry);

                entry.State = state;

                Assert.Equal(state, entry.State);
                var objectStateEntry = GetStateEntry(context, larry);
                Assert.Equal(state, objectStateEntry.State);
            }
        }

        [Fact]
        public void Setting_state_of_stand_alone_DbEntityEntry_to_Detached_does_nothing()
        {
            using (var context = new F1Context())
            {
                var larry = new Driver
                                {
                                    Name = "Larry David",
                                    TeamId = Team.Ferrari
                                };
                var entry = context.Entry(larry);

                entry.State = EntityState.Detached;

                Assert.Equal(EntityState.Detached, entry.State);
                ObjectStateEntry objectStateEntry;
                Assert.False(
                    GetObjectContext(context).ObjectStateManager.TryGetObjectStateEntry(
                        larry,
                        out objectStateEntry));
            }
        }

        [Fact]
        public void Creating_DbEntityEntry_for_entity_type_that_is_not_in_the_model_throws()
        {
            using (var context = new F1Context())
            {
                Assert.Throws<InvalidOperationException>(() => context.Entry(new Category())).ValidateMessage(
                    "DbSet_EntityTypeNotInModel", typeof(Category).Name);
            }
        }

        [Fact]
        public void Creating_DbEntityEntry_for_complex_type_throws()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                Assert.Throws<InvalidOperationException>(() => context.Entry(new Address())).ValidateMessage(
                    "DbSet_DbSetUsedWithComplexType", typeof(Address).Name);
            }
        }

        [Fact]
        public void Attempting_to_Attach_by_DbEntityEntry_change_state_throws_if_Attach_not_allowed()
        {
            Attempting_to_Attach_by_DbEntityEntry_change_state_throws_if_Attach_not_allowed_implementation(
                EntityState.Unchanged);
        }

        [Fact]
        public void Attempting_to_attach_as_modified_by_DbEntityEntry_change_state_throws_if_Attach_not_allowed()
        {
            Attempting_to_Attach_by_DbEntityEntry_change_state_throws_if_Attach_not_allowed_implementation(
                EntityState.Modified);
        }

        [Fact]
        public void Attempting_to_delete_by_DbEntityEntry_change_state_throws_if_Attach_not_allowed()
        {
            Attempting_to_Attach_by_DbEntityEntry_change_state_throws_if_Attach_not_allowed_implementation(
                EntityState.Deleted);
        }

        private void Attempting_to_Attach_by_DbEntityEntry_change_state_throws_if_Attach_not_allowed_implementation(
            EntityState state)
        {
            using (var context = new SimpleModelContext())
            {
                context.Categories.Find("Foods");
                var newCategory = new Category("Foods");
                var entry = context.Entry(newCategory);

                Assert.Throws<InvalidOperationException>(() => entry.State = state).ValidateMessage(
                    "ObjectStateManager_ObjectStateManagerContainsThisEntityKey");
            }
        }

        [Fact]
        public void DbEntityEntry_Reference_can_be_used_on_previously_stand_alone_entry_after_the_entity_becomes_tracked()
        {
            DbEntityEntry_method_can_be_used_on_previously_stand_alone_entry_after_the_entity_becomes_tracked(
                e => e.Reference(t => t.Chassis));
        }

        [Fact]
        public void
            DbEntityEntry_Collection_can_be_used_on_previously_stand_alone_entry_after_the_entity_becomes_tracked()
        {
            DbEntityEntry_method_can_be_used_on_previously_stand_alone_entry_after_the_entity_becomes_tracked(
                e => e.Collection(t => t.Drivers));
        }

        [Fact]
        public void DbEntityEntry_Property_can_be_used_on_previously_stand_alone_entry_after_the_entity_becomes_tracked()
        {
            DbEntityEntry_method_can_be_used_on_previously_stand_alone_entry_after_the_entity_becomes_tracked(
                e => e.Property(t => t.FastestLaps));
        }

        [Fact]
        public void
            DbEntityEntry_CurrentValues_can_be_used_on_previously_stand_alone_entry_after_the_entity_becomes_tracked()
        {
            DbEntityEntry_method_can_be_used_on_previously_stand_alone_entry_after_the_entity_becomes_tracked(
                e => { var _ = e.CurrentValues; });
        }

        [Fact]
        public void
            DbEntityEntry_OriginalValues_can_be_used_on_previously_stand_alone_entry_after_the_entity_becomes_tracked()
        {
            DbEntityEntry_method_can_be_used_on_previously_stand_alone_entry_after_the_entity_becomes_tracked(
                e => { var _ = e.OriginalValues; });
        }

        [Fact]
        public void DbEntityEntry_GetDatabaseValues_can_be_used_on_previously_stand_alone_entry_after_the_entity_becomes_tracked()
        {
            DbEntityEntry_method_can_be_used_on_previously_stand_alone_entry_after_the_entity_becomes_tracked(
                e => e.GetDatabaseValues());
        }

        [Fact]
        public void DbEntityEntry_Reload_can_be_used_on_previously_stand_alone_entry_after_the_entity_becomes_tracked()
        {
            DbEntityEntry_method_can_be_used_on_previously_stand_alone_entry_after_the_entity_becomes_tracked(
                e => e.Reload());
        }

        private void DbEntityEntry_method_can_be_used_on_previously_stand_alone_entry_after_the_entity_becomes_tracked(
            Action<DbEntityEntry<Team>> test)
        {
            using (var context = new F1Context())
            {
                var team = new Team
                               {
                                   Id = -1,
                                   Name = "Wubbsy Racing",
                                   Chassis = new Chassis
                                                 {
                                                     TeamId = -1,
                                                     Name = "Wubbsy"
                                                 }
                               };
                var entry = context.Entry(team);

                context.Teams.Attach(team);

                test(entry); // Just testing here that this doesn't throw
            }
        }

        #endregion

        #region Tests for implicit conversion of generic to non-generic

        [Fact]
        public void Generic_DbEntityEntry_can_be_implicitly_converted_to_non_generic_version()
        {
            using (var context = new SimpleModelContext())
            {
                var product = context.Products.Find(1);

                var genericEntry = context.Entry(product);

                NonGenericTestMethod(genericEntry, genericEntry.Entity);
            }
        }

        private void NonGenericTestMethod(DbEntityEntry nonGenericEntry, object wrappedEntity)
        {
            Assert.Same(wrappedEntity, nonGenericEntry.Entity);
        }

        #endregion

        #region Attaching and detaching mixed IA/FK relationships (Dev11 264780)

        [Fact]
        public void Attaching_previously_detached_entity_should_not_throw_exception()
        {
            using (var db = new DetachmentContext())
            {
                var login = new DeLogin
                                {
                                    Id = 14
                                };
                var order = new DeOrder
                                {
                                    Id = 19
                                };

                login.Orders.Add(order);
                order.Login = login;

                db.Logins.Attach(login);
                ((IObjectContextAdapter)db).ObjectContext.Detach(login);

                Assert.Null(order.Login);

                db.Logins.Attach(login);

                Assert.Same(login, order.Login);
                Assert.True(login.Orders.Contains(order));
                Assert.Equal(EntityState.Unchanged, db.Entry(login).State);
                Assert.Equal(EntityState.Unchanged, db.Entry(order).State);
            }
        }

        [Fact]
        public void Querying_for_previously_detached_entity_should_not_throw_exception()
        {
            using (var db = new DetachmentContext())
            {
                var login = db.Logins.Include(l => l.Orders).First();
                var order = login.Orders.First();

                ((IObjectContextAdapter)db).ObjectContext.Detach(login);

                Assert.Null(order.Login);

                login = db.Logins.First();

                Assert.Same(login, order.Login);
                Assert.True(login.Orders.Contains(order));
                Assert.Equal(EntityState.Unchanged, db.Entry(login).State);
                Assert.Equal(EntityState.Unchanged, db.Entry(order).State);
            }
        }

        #endregion

        #region Refresh with primary key (Dev11 212562)

        [Fact]
        public void Setting_primary_key_to_same_value_on_Modified_entity_as_part_of_Refresh_with_conceptual_null_should_not_throw()
        {
            using (var context = new RefreshContext())
            {
                var product = context.Products.Include(p => p.Supplier).Single(p => p.Id == "ALFKI");
                var supplier = product.Supplier;

                context.Entry(supplier).State = EntityState.Deleted;
                context.Entry(product).State = EntityState.Modified;

                // product now has a conceptual null since its Supplier is deleted.
                // This line would previously throw when we try to set the PK (ProductId) to the same value
                GetObjectContext(context).Refresh(RefreshMode.StoreWins, product);

                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
                Assert.Equal(EntityState.Deleted, context.Entry(supplier).State);

                // Should not throw because the conceptual null should have been cleared.
                GetObjectContext(context).AcceptAllChanges();
            }
        }

        #endregion

        #region Removing demoted state entries from dangling FKs index (Dev11 322801)

        [Fact]
        public void State_entries_demoted_to_stubs_should_be_removed_from_dangling_foreign_keys_index()
        {
            using (var context = new MixedIAAndFKContext())
            {
                var principal = context.Principals.Include(p => p.FKDependents.Select(d => d.FKDependents)).Single();
                var both = principal.FKDependents.Single();

                context.Entry(principal).State = EntityState.Detached;
                context.Entry(both).State = EntityState.Detached;

                // At this point there was previously a stub entry in the dangling keys index
                // Querying for the principal will cause that entry to be accessed which would previously throw
                context.Principals.Single();
            }
        }

        [Fact]
        public void State_entries_demoted_to_stub_and_then_detached_should_be_removed_from_dangling_foreign_keys_index()
        {
            using (var context = new MixedIAAndFKContext())
            {
                var principal = context.Principals.Include(p => p.FKDependents.Select(d => d.FKDependents)).Single();
                var both = principal.FKDependents.Single();
                var dependent = both.FKDependents.Single();

                context.Entry(principal).State = EntityState.Detached;
                context.Entry(both).State = EntityState.Detached;
                context.Entry(dependent).State = EntityState.Detached;

                // At this point there was previously a detached entry in the dangling keys index
                // Querying for the principal will cause that entry to be accessed which would previously throw
                context.Principals.Single();
            }
        }

        #endregion
    }

    #region Repro models

    public class DetachmentContext : DbContext
    {
        public DetachmentContext()
        {
            Database.SetInitializer(new DetachmentInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<DeLogin>()
                .HasMany(l => l.Orders)
                .WithOptional(o => o.Login);

            modelBuilder
                .Entity<DeCustomer>()
                .HasMany(c => c.Logins)
                .WithOptional(l => l.Customer)
                .HasForeignKey(l => l.CustomerId);

            modelBuilder
                .Entity<DeLogin>()
                .Property(l => l.CustomerId)
                .IsOptional();
        }

        public DbSet<DeLogin> Logins { get; set; }
        public DbSet<DeOrder> Orders { get; set; }
        public DbSet<DeCustomer> Customers { get; set; }
    }

    public class DetachmentInitializer : DropCreateDatabaseAlways<DetachmentContext>
    {
        protected override void Seed(DetachmentContext context)
        {
            var login = new DeLogin
                            {
                                Id = 14,
                                CustomerId = 21
                            };
            var order = new DeOrder
                            {
                                Id = 19
                            };
            var customer = new DeCustomer
                               {
                                   DeCustomerId = 21
                               };

            login.Orders.Add(order);
            order.Login = login;
            login.Customer = customer;

            context.Logins.Add(login);
        }
    }

    public class DeLogin
    {
        public DeLogin()
        {
            Orders = new List<DeOrder>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public ICollection<DeOrder> Orders { get; set; }
        public int CustomerId { get; set; }
        public DeCustomer Customer { get; set; }
    }

    public class DeOrder
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public DeLogin Login { get; set; }
    }

    public class DeCustomer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DeCustomerId { get; set; }

        public ICollection<DeLogin> Logins { get; set; }
    }

    public class RefreshContext : DbContext
    {
        public RefreshContext()
        {
            Database.SetInitializer(new RefreshInitializer());
        }

        public DbSet<ReProduct> Products { get; set; }
    }

    public class RefreshInitializer : DropCreateDatabaseAlways<RefreshContext>
    {
        protected override void Seed(RefreshContext context)
        {
            context.Products.Add(
                new ReProduct
                    {
                        Id = "ALFKI",
                        Supplier = new ReSupplier
                                       {
                                           Id = 14,
                                           Name = "Initial"
                                       }
                    });
        }
    }

    public class ReProduct
    {
        public string Id { get; set; }
        public int SupplierId { get; set; }
        public ReSupplier Supplier { get; set; }
    }

    public class ReSupplier
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class MixedIAAndFKContext : DbContext
    {
        public MixedIAAndFKContext()
        {
            Database.SetInitializer(new MixedIAAndFKInitializer());
        }

        public DbSet<MixedPrincipal> Principals { get; set; }
        public DbSet<MixedBoth> Boths { get; set; }
        public DbSet<MixedDependent> Dependents { get; set; }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            builder.Entity<MixedPrincipal>().HasMany(p => p.IADependents).WithOptional(d => d.IAPrincipal);
            builder.Entity<MixedPrincipal>().HasMany(p => p.FKDependents).WithOptional(d => d.FKPrincipal).HasForeignKey
                (d => d.FK);
            builder.Entity<MixedBoth>().HasMany(p => p.IADependents).WithOptional(d => d.IAPrincipal);
            builder.Entity<MixedBoth>().HasMany(p => p.FKDependents).WithOptional(d => d.FKPrincipal).HasForeignKey(
                d => d.FK);
        }
    }

    public class MixedPrincipal
    {
        public int Id { get; set; }

        public virtual ICollection<MixedBoth> IADependents { get; set; }
        public virtual ICollection<MixedBoth> FKDependents { get; set; }
    }

    public class MixedBoth
    {
        public int Id { get; set; }
        public int? FK { get; set; }

        public virtual MixedPrincipal IAPrincipal { get; set; }
        public virtual MixedPrincipal FKPrincipal { get; set; }

        public virtual ICollection<MixedDependent> IADependents { get; set; }
        public virtual ICollection<MixedDependent> FKDependents { get; set; }
    }

    public class MixedDependent
    {
        public int Id { get; set; }
        public int? FK { get; set; }

        public virtual MixedBoth IAPrincipal { get; set; }
        public virtual MixedBoth FKPrincipal { get; set; }
    }

    public class MixedIAAndFKInitializer : DropCreateDatabaseIfModelChanges<MixedIAAndFKContext>
    {
        protected override void Seed(MixedIAAndFKContext context)
        {
            var principal = new MixedPrincipal();
            var both = new MixedBoth
                           {
                               IAPrincipal = principal,
                               FKPrincipal = principal
                           };
            context.Dependents.Add(
                new MixedDependent
                    {
                        IAPrincipal = both,
                        FKPrincipal = both
                    });
        }
    }

    #endregion
}
