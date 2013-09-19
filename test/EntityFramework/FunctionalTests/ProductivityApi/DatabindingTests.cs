// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data.Entity;
    using System.Linq;
    using ConcurrencyModel;
    using SimpleModel;
    using Xunit;

    /// <summary>
    /// Functional tests for data binding and DbSet.Local.  Unit tests also exist in the unit tests project.
    /// </summary>
    public class DatabindingTests : FunctionalTestBase
    {
        #region Helpers for context setup and counts

        private const int DeletedTeam = Team.Hispania;
        private const int DeletedCount = 4;

        private const int ModifedTeam = Team.Lotus;
        private const int ModifiedCount = 3;

        private const int AddedTeam = Team.Sauber;
        private const int AddedCount = 2;

        private const int UnchangedTeam = Team.Mercedes;
        private const int UnchangedCount = 3;

        private void SetupContext(F1Context context)
        {
            var drivers = context.Drivers;
            drivers.Load();
            drivers.Local.Where(d => d.TeamId == DeletedTeam).ToList().ForEach(d => drivers.Remove(d));
            drivers.Local.Where(d => d.TeamId == ModifedTeam).ToList().ForEach(d => d.Races = 5);
            drivers.Add(
                new Driver
                    {
                        Name = "Pedro de la Rosa",
                        TeamId = AddedTeam,
                        CarNumber = 13
                    });
            drivers.Add(
                new Driver
                    {
                        Name = "Kamui Kobayashi",
                        TeamId = AddedTeam,
                        CarNumber = null
                    });
        }

        #endregion

        #region DBSet.Local tests

        [Fact]
        public void DbSet_Local_contains_Unchanged_Modified_and_Added_entities_but_not_Deleted_entities()
        {
            DbSet_Local_contains_Unchanged_Modified_and_Added_entities_but_not_Deleted_entities_implementation(
                c => c.Drivers.Local);
        }

        [Fact]
        public void DbSet_Local_contains_Unchanged_Modified_and_Added_entities_but_not_Deleted_entities_non_generic()
        {
            DbSet_Local_contains_Unchanged_Modified_and_Added_entities_but_not_Deleted_entities_implementation(
                c => (ObservableCollection<Driver>)c.Set(typeof(Driver)).Local);
        }

        private void DbSet_Local_contains_Unchanged_Modified_and_Added_entities_but_not_Deleted_entities_implementation(
            Func<F1Context, ObservableCollection<Driver>> getLocal)
        {
            using (var context = new F1Context())
            {
                SetupContext(context);
                var local = getLocal(context);

                Assert.Equal(0, local.Count(d => d.TeamId == DeletedTeam));
                Assert.Equal(ModifiedCount, local.Count(d => d.TeamId == ModifedTeam));
                Assert.Equal(UnchangedCount, local.Count(d => d.TeamId == UnchangedTeam));
                Assert.Equal(AddedCount, local.Count(d => d.TeamId == AddedTeam));
            }
        }

        [Fact]
        public void Adding_entity_to_context_is_reflected_in_local_view()
        {
            Adding_entity_to_context_is_reflected_in_local_view_implementation(c => c.Drivers.Local);
        }

        [Fact]
        public void Adding_entity_to_context_is_reflected_in_local_view_non_generic()
        {
            Adding_entity_to_context_is_reflected_in_local_view_implementation(c => c.Set(typeof(Driver)).Local);
        }

        private void Adding_entity_to_context_is_reflected_in_local_view_implementation(Func<F1Context, IList> getLocal)
        {
            using (var context = new F1Context())
            {
                var local = getLocal(context);

                var larry = new Driver
                                {
                                    Name = "Larry David",
                                    TeamId = Team.Ferrari,
                                    CarNumber = 13
                                };
                context.Drivers.Add(larry);

                Assert.True(local.Contains(larry));
            }
        }

        [Fact]
        public void Attaching_entity_to_context_is_reflected_in_local_view()
        {
            Attaching_entity_to_context_is_reflected_in_local_view_implementation(c => c.Drivers.Local);
        }

        [Fact]
        public void Attaching_entity_to_context_is_reflected_in_local_view_non_generic()
        {
            Attaching_entity_to_context_is_reflected_in_local_view_implementation(c => c.Set(typeof(Driver)).Local);
        }

        private void Attaching_entity_to_context_is_reflected_in_local_view_implementation(
            Func<F1Context, IList> getLocal)
        {
            using (var context = new F1Context())
            {
                var local = getLocal(context);

                var larry = new Driver
                                {
                                    Name = "Larry David",
                                    TeamId = Team.Ferrari,
                                    CarNumber = 13
                                };
                context.Drivers.Attach(larry);

                Assert.True(local.Contains(larry));
            }
        }

        [Fact]
        public void Entities_materialized_into_context_are_reflected_in_local_view()
        {
            Entities_materialized_into_context_are_reflected_in_local_view_implementation(c => c.Drivers.Local);
        }

        [Fact]
        public void Entities_materialized_into_context_are_reflected_in_local_view_non_generic()
        {
            Entities_materialized_into_context_are_reflected_in_local_view_implementation(
                c => c.Set(typeof(Driver)).Local);
        }

        private void Entities_materialized_into_context_are_reflected_in_local_view_implementation(
            Func<F1Context, IList> getLocal)
        {
            using (var context = new F1Context())
            {
                var local = getLocal(context);

                context.Drivers.Where(d => d.TeamId == UnchangedTeam).Load();

                Assert.Equal(UnchangedCount, local.Count);
            }
        }

        [Fact]
        public void Entities_detached_from_context_are_removed_from_local_view()
        {
            Entities_detached_from_context_are_removed_from_local_view_implementation(c => c.Drivers.Local);
        }

        [Fact]
        public void Entities_detached_from_context_are_removed_from_local_view_non_generic()
        {
            Entities_detached_from_context_are_removed_from_local_view_implementation(c => c.Set(typeof(Driver)).Local);
        }

        private void Entities_detached_from_context_are_removed_from_local_view_implementation(
            Func<F1Context, IList> getLocal)
        {
            using (var context = new F1Context())
            {
                SetupContext(context);
                var local = getLocal(context);

                context.Drivers.Local.Where(d => d.TeamId == UnchangedTeam).ToList().ForEach(
                    GetObjectContext(context).Detach);

                Assert.Equal(0, local.Cast<Driver>().Count(d => d.TeamId == UnchangedTeam));
            }
        }

        [Fact]
        public void Entities_deleted_from_context_are_removed_from_local_view()
        {
            using (var context = new F1Context())
            {
                SetupContext(context);
                var local = context.Drivers.Local;

                context.Drivers.Local.Where(d => d.TeamId == UnchangedTeam).ToList().ForEach(
                    d => context.Drivers.Remove(d));

                Assert.Equal(0, local.Count(d => d.TeamId == UnchangedTeam));
            }
        }

        [Fact]
        public void Entities_with_state_changed_to_deleted_are_removed_from_local_view()
        {
            using (var context = new F1Context())
            {
                SetupContext(context);
                var local = context.Drivers.Local;

                context.Drivers.Local.Where(d => d.TeamId == UnchangedTeam).ToList().ForEach(
                    d => GetObjectContext(context).ObjectStateManager.ChangeObjectState(d, EntityState.Deleted));

                Assert.Equal(0, local.Count(d => d.TeamId == UnchangedTeam));
            }
        }

        [Fact]
        public void Entities_with_state_changed_to_detached_are_removed_from_local_view()
        {
            using (var context = new F1Context())
            {
                SetupContext(context);
                var local = context.Drivers.Local;

                context.Drivers.Local.Where(d => d.TeamId == UnchangedTeam).ToList().ForEach(
                    d => GetObjectContext(context).ObjectStateManager.ChangeObjectState(d, EntityState.Detached));

                Assert.Equal(0, local.Count(d => d.TeamId == UnchangedTeam));
            }
        }

        [Fact]
        public void Entities_with_state_changed_from_deleted_to_added_are_added_to_local_view()
        {
            using (var context = new F1Context())
            {
                SetupContext(context);
                var local = context.Drivers.Local;

                context.Drivers.Where(d => d.TeamId == DeletedTeam).ToList().ForEach(
                    d => GetObjectContext(context).ObjectStateManager.ChangeObjectState(d, EntityState.Added));

                Assert.Equal(DeletedCount, local.Count(d => d.TeamId == DeletedTeam));
            }
        }

        [Fact]
        public void Entities_with_state_changed_from_deleted_to_unchanged_are_added_to_local_view()
        {
            using (var context = new F1Context())
            {
                SetupContext(context);
                var local = context.Drivers.Local;

                context.Drivers.Where(d => d.TeamId == DeletedTeam).ToList().ForEach(
                    d => GetObjectContext(context).ObjectStateManager.ChangeObjectState(d, EntityState.Unchanged));

                Assert.Equal(DeletedCount, local.Count(d => d.TeamId == DeletedTeam));
            }
        }

        [Fact]
        public void Entities_added_to_local_view_are_added_to_state_manager()
        {
            Entities_added_to_local_view_are_added_to_state_manager_implementation(c => c.Drivers.Local);
        }

        [Fact]
        public void Entities_added_to_local_view_are_added_to_state_manager_non_generic()
        {
            Entities_added_to_local_view_are_added_to_state_manager_implementation(c => c.Set(typeof(Driver)).Local);
        }

        private void Entities_added_to_local_view_are_added_to_state_manager_implementation(
            Func<F1Context, IList> getLocal)
        {
            using (var context = new F1Context())
            {
                var local = getLocal(context);

                var larry = new Driver
                                {
                                    Id = -1,
                                    Name = "Larry David",
                                    TeamId = Team.Ferrari,
                                    CarNumber = 13
                                };
                local.Add(larry);

                Assert.Same(larry, context.Drivers.Find(-1));
                Assert.Equal(
                    EntityState.Added,
                    GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(larry).State);
            }
        }

        [Fact]
        public void Entities_removed_from_the_local_view_are_marked_deleted_in_the_state_manager()
        {
            Entities_removed_from_the_local_view_are_marked_deleted_in_the_state_manager_implementation(
                c => c.Drivers.Local);
        }

        [Fact]
        public void Entities_removed_from_the_local_view_are_marked_deleted_in_the_state_manager_non_generic()
        {
            Entities_removed_from_the_local_view_are_marked_deleted_in_the_state_manager_implementation(
                c => c.Set(typeof(Driver)).Local);
        }

        private void Entities_removed_from_the_local_view_are_marked_deleted_in_the_state_manager_implementation(
            Func<F1Context, IList> getLocal)
        {
            using (var context = new F1Context())
            {
                SetupContext(context);
                var local = getLocal(context);

                var alonso = local.Cast<Driver>().Single(d => d.Name == "Fernando Alonso");
                local.Remove(alonso);

                Assert.Equal(
                    EntityState.Deleted,
                    GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(alonso).State);
            }
        }

        [Fact]
        public void Adding_entity_to_local_view_that_is_already_in_the_state_manager_and_not_Deleted_is_noop()
        {
            Adding_entity_to_local_view_that_is_already_in_the_state_manager_and_not_Deleted_is_noop_implementation(
                c => c.Drivers.Local);
        }

        [Fact]
        public void Adding_entity_to_local_view_that_is_already_in_the_state_manager_and_not_Deleted_is_noop_non_generic()
        {
            Adding_entity_to_local_view_that_is_already_in_the_state_manager_and_not_Deleted_is_noop_implementation(
                c => c.Set(typeof(Driver)).Local);
        }

        private void
            Adding_entity_to_local_view_that_is_already_in_the_state_manager_and_not_Deleted_is_noop_implementation(
            Func<F1Context, IList> getLocal)
        {
            using (var context = new F1Context())
            {
                SetupContext(context);
                var local = getLocal(context);

                var alonso = local.Cast<Driver>().Single(d => d.Name == "Fernando Alonso");
                local.Add(alonso);

                Assert.Equal(
                    EntityState.Unchanged,
                    GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(alonso).State);
            }
        }

        [Fact]
        public void Adding_entity_to_local_view_that_is_Deleted_in_the_state_manager_makes_entity_Added()
        {
            using (var context = new F1Context())
            {
                SetupContext(context);
                var local = context.Drivers.Local;

                var deletedDrivers = context.Drivers.Where(d => d.TeamId == DeletedTeam).ToList();
                deletedDrivers.ForEach(local.Add);

                Assert.True(
                    deletedDrivers.TrueForAll(
                        d =>
                        GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(d).State == EntityState.Added));
            }
        }

        [Fact]
        public void Adding_entity_to_state_manager_of_different_type_than_local_view_type_has_no_effect_on_local_view()
        {
            using (var context = new F1Context())
            {
                SetupContext(context);
                var local = context.Drivers.Local;
                var count = local.Count;

                context.Teams.Add(
                    new Team
                        {
                            Id = -1,
                            Name = "Wubbsy Racing"
                        });

                Assert.Equal(count, local.Count);
            }
        }

        [Fact]
        public void Adding_entity_to_state_manager_of_subtype_still_shows_up_in_local_view()
        {
            using (var context = new SimpleModelContext())
            {
                context.Products.Load();
                var local = context.Products.Local;

                var newProduct = new FeaturedProduct();
                context.Products.Add(newProduct);

                Assert.True(local.Contains(newProduct));
            }
        }

        [Fact]
        public void Adding_entity_of_wrong_type_to_non_generic_local_view_throws()
        {
            var team = new Team
                           {
                               Id = -1,
                               Name = "Wubbsy Racing"
                           };
            var expectedException = GenerateException(() => ((IList)new List<Driver>()).Add(team));

            using (var context = new F1Context())
            {
                var local = context.Set(typeof(Driver)).Local;

                Assert.Equal(expectedException.Message, Assert.Throws<ArgumentException>(() => local.Add(team)).Message);
            }
        }

        [Fact]
        public void DbSet_Local_is_cached_on_the_set()
        {
            DbSet_Local_is_cached_on_the_set_implementation(c => c.Drivers.Local);
        }

        [Fact]
        public void DbSet_Local_is_cached_on_the_set_non_generic()
        {
            DbSet_Local_is_cached_on_the_set_implementation(c => c.Set(typeof(Driver)).Local);
        }

        private void DbSet_Local_is_cached_on_the_set_implementation(Func<F1Context, IList> getLocal)
        {
            using (var context = new F1Context())
            {
                var local = getLocal(context);

                Assert.Same(local, getLocal(context));
            }
        }

        [Fact]
        public void DbSet_Local_from_generic_set_is_same_as_from_non_generic_set()
        {
            using (var context = new F1Context())
            {
                Assert.Same(context.Drivers.Local, context.Set(typeof(Driver)).Local);
            }
        }

        [Fact]
        public void DbSet_Local_calls_DetectChanges()
        {
            DbSet_Local_calls_DetectChanges_implementation(c => c.Drivers.Local);
        }

        [Fact]
        public void DbSet_Local_calls_DetectChanges_non_generic()
        {
            DbSet_Local_calls_DetectChanges_implementation(c => c.Set(typeof(Driver)).Local);
        }

        private void DbSet_Local_calls_DetectChanges_implementation(Func<F1Context, IList> getLocal)
        {
            using (var context = new F1Context())
            {
                SetupContext(context);

                var alonso = context.Drivers.Single(d => d.Name == "Fernando Alonso");
                alonso.CarNumber = 13;

                Assert.Equal(
                    EntityState.Unchanged,
                    GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(alonso).State);

                getLocal(context);

                Assert.Equal(
                    EntityState.Modified,
                    GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(alonso).State);
            }
        }

        [Fact]
        public void Local_view_used_in_database_initializer_is_reset_for_use_with_real_context()
        {
            Local_view_used_in_database_initializer_is_reset_for_use_with_real_context_implementation(c => c.Teams.Local);
        }

        [Fact]
        public void Local_view_used_in_database_initializer_is_reset_for_use_with_real_context_non_generic()
        {
            Local_view_used_in_database_initializer_is_reset_for_use_with_real_context_implementation(
                c => c.Set(typeof(Team)).Local);
        }

        private void Local_view_used_in_database_initializer_is_reset_for_use_with_real_context_implementation(
            Func<F1Context, IList> getLocal)
        {
            using (var context = new F1Context())
            {
                // Teams.Local is used in the ConcurrencyModelInitializer class
                var local = getLocal(context);

                Assert.Equal(0, local.Count);

                context.Teams.Add(
                    new Team
                        {
                            Id = -1,
                            Name = "Wubbsy Racing"
                        });

                Assert.Equal(1, local.Count);
            }
        }

        #endregion

        #region DBSet.Local.ToBindingList() tests

        [Fact]
        public void DbSet_Local_ToBindingList_contains_Unchanged_Modified_and_Added_entities_but_not_Deleted_entities()
        {
            using (var context = new F1Context())
            {
                SetupContext(context);
                var bindingList = context.Drivers.Local.ToBindingList();

                Assert.Equal(0, bindingList.Count(d => d.TeamId == DeletedTeam));
                Assert.Equal(ModifiedCount, bindingList.Count(d => d.TeamId == ModifedTeam));
                Assert.Equal(UnchangedCount, bindingList.Count(d => d.TeamId == UnchangedTeam));
                Assert.Equal(AddedCount, bindingList.Count(d => d.TeamId == AddedTeam));
            }
        }

        [Fact]
        public void Adding_entity_to_context_is_reflected_in_local_binding_list()
        {
            using (var context = new F1Context())
            {
                var bindingList = context.Drivers.Local.ToBindingList();

                var larry = new Driver
                                {
                                    Name = "Larry David",
                                    TeamId = Team.Ferrari,
                                    CarNumber = 13
                                };
                context.Drivers.Add(larry);

                Assert.True(bindingList.Contains(larry));
            }
        }

        [Fact]
        public void Entities_materialized_into_context_are_reflected_in_local_binding_list()
        {
            using (var context = new F1Context())
            {
                var bindingList = context.Drivers.Local.ToBindingList();

                context.Drivers.Where(d => d.TeamId == UnchangedTeam).Load();

                Assert.Equal(UnchangedCount, bindingList.Count);
            }
        }

        [Fact]
        public void Entities_detached_from_context_are_removed_from_local_binding_list()
        {
            using (var context = new F1Context())
            {
                SetupContext(context);
                var bindingList = context.Drivers.Local.ToBindingList();

                context.Drivers.Local.Where(d => d.TeamId == UnchangedTeam).ToList().ForEach(
                    GetObjectContext(context).Detach);

                Assert.Equal(0, bindingList.Count(d => d.TeamId == UnchangedTeam));
            }
        }

        [Fact]
        public void Entities_deleted_from_context_are_removed_from_local_binding_list()
        {
            using (var context = new F1Context())
            {
                SetupContext(context);
                var bindingList = context.Drivers.Local.ToBindingList();

                context.Drivers.Local.Where(d => d.TeamId == UnchangedTeam).ToList().ForEach(
                    d => context.Drivers.Remove(d));

                Assert.Equal(0, bindingList.Count(d => d.TeamId == UnchangedTeam));
            }
        }

        [Fact]
        public void Entities_added_to_local_binding_list_are_added_to_state_manager()
        {
            using (var context = new F1Context())
            {
                var bindingList = context.Drivers.Local.ToBindingList();

                var larry = new Driver
                                {
                                    Id = -1,
                                    Name = "Larry David",
                                    TeamId = Team.Ferrari,
                                    CarNumber = 13
                                };
                bindingList.Add(larry);

                Assert.Same(larry, context.Drivers.Find(-1));
                Assert.Equal(
                    EntityState.Added,
                    GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(larry).State);
            }
        }

        [Fact]
        public void Entities_removed_from_the_local_binding_list_are_marked_deleted_in_the_state_manager()
        {
            using (var context = new F1Context())
            {
                SetupContext(context);
                var bindingList = context.Drivers.Local.ToBindingList();

                var alonso = bindingList.Single(d => d.Name == "Fernando Alonso");
                bindingList.Remove(alonso);

                Assert.Equal(
                    EntityState.Deleted,
                    GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(alonso).State);
            }
        }

        [Fact]
        public void Adding_entity_to_local_binding_list_that_is_already_in_the_state_manager_and_not_Deleted_is_noop()
        {
            using (var context = new F1Context())
            {
                SetupContext(context);
                var bindingList = context.Drivers.Local.ToBindingList();

                var alonso = bindingList.Single(d => d.Name == "Fernando Alonso");
                bindingList.Add(alonso);

                Assert.Equal(
                    EntityState.Unchanged,
                    GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(alonso).State);
            }
        }

        [Fact]
        public void Adding_entity_to_local_binding_list_that_is_Deleted_in_the_state_manager_makes_entity_Added()
        {
            using (var context = new F1Context())
            {
                SetupContext(context);
                var bindingList = context.Drivers.Local.ToBindingList();

                var deletedDrivers = context.Drivers.Where(d => d.TeamId == DeletedTeam).ToList();
                deletedDrivers.ForEach(bindingList.Add);

                Assert.True(
                    deletedDrivers.TrueForAll(
                        d =>
                        GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(d).State == EntityState.Added));
            }
        }

        [Fact]
        public void
            Adding_entity_to_state_manager_of_different_type_than_local_view_type_has_no_effect_on_local_binding_list()
        {
            using (var context = new F1Context())
            {
                SetupContext(context);
                var bindingList = context.Drivers.Local.ToBindingList();
                var count = bindingList.Count;

                context.Teams.Add(
                    new Team
                        {
                            Id = -1,
                            Name = "Wubbsy Racing"
                        });

                Assert.Equal(count, bindingList.Count);
            }
        }

        [Fact]
        public void Adding_entity_to_state_manager_of_subtype_still_shows_up_in_local_binding_list()
        {
            using (var context = new SimpleModelContext())
            {
                context.Products.Load();
                var bindingList = context.Products.Local.ToBindingList();

                var newProduct = new FeaturedProduct();
                context.Products.Add(newProduct);

                Assert.True(bindingList.Contains(newProduct));
            }
        }

        [Fact]
        public void Sets_of_subtypes_can_still_be_sorted()
        {
            using (var context = new SimpleModelContext())
            {
                var featuredProducts = context.Set<FeaturedProduct>();
                featuredProducts.Attach(
                    new FeaturedProduct
                        {
                            Id = 3
                        });
                featuredProducts.Attach(
                    new FeaturedProduct
                        {
                            Id = 1
                        });
                featuredProducts.Attach(
                    new FeaturedProduct
                        {
                            Id = 4
                        });

                var bindingList = featuredProducts.Local.ToBindingList();

                ((IBindingList)bindingList).ApplySort(
                    TypeDescriptor.GetProperties(typeof(Product))["Id"],
                    ListSortDirection.Ascending);

                Assert.Equal(1, bindingList[0].Id);
                Assert.Equal(3, bindingList[1].Id);
                Assert.Equal(4, bindingList[2].Id);
            }
        }

        [Fact]
        public void Sets_containing_instances_of_subtypes_can_still_be_sorted()
        {
            using (var context = new SimpleModelContext())
            {
                context.Products.Attach(
                    new FeaturedProduct
                        {
                            Id = 3
                        });
                context.Products.Attach(
                    new FeaturedProduct
                        {
                            Id = 1
                        });
                context.Products.Attach(
                    new FeaturedProduct
                        {
                            Id = 4
                        });

                var bindingList = context.Products.Local.ToBindingList();

                ((IBindingList)bindingList).ApplySort(
                    TypeDescriptor.GetProperties(typeof(Product))["Id"],
                    ListSortDirection.Ascending);

                Assert.Equal(1, bindingList[0].Id);
                Assert.Equal(3, bindingList[1].Id);
                Assert.Equal(4, bindingList[2].Id);
            }
        }

        [Fact]
        public void DbSet_Local_ToBindingList_is_cached_on_the_set()
        {
            using (var context = new F1Context())
            {
                var bindingList = context.Drivers.Local.ToBindingList();

                Assert.Same(bindingList, context.Drivers.Local.ToBindingList());
            }
        }

        #endregion

        #region Load tests

        [Fact]
        public void Load_executes_query_on_DbQuery()
        {
            using (var context = new F1Context())
            {
                context.Drivers.Where(d => d.TeamId == UnchangedTeam).Load();

                Assert.Equal(
                    UnchangedCount,
                    GetObjectContext(context).ObjectStateManager.GetObjectStateEntries(~EntityState.Detached).
                        Count());
            }
        }

        [Fact]
        public void Load_executes_query_on_ObjectQuery()
        {
            using (var context = new F1Context())
            {
                var objectContext = GetObjectContext(context);
                var objectSet = objectContext.CreateObjectSet<Driver>();

                objectSet.Where(d => d.TeamId == UnchangedTeam).Load();

                Assert.Equal(
                    UnchangedCount,
                    objectContext.ObjectStateManager.GetObjectStateEntries(~EntityState.Detached).Count());
            }
        }

        #endregion

        #region ObservableListSource as navigation properties tests

        [Fact]
        public void Entity_added_to_context_is_added_to_navigation_property_binding_list()
        {
            using (var context = new F1Context())
            {
                var ferrari = context.Teams.Include(t => t.Drivers).Single(t => t.Id == Team.Ferrari);
                var navBindingList = ((IListSource)ferrari.Drivers).GetList();

                var larry = new Driver
                                {
                                    Name = "Larry David",
                                    TeamId = Team.Ferrari,
                                    CarNumber = 13
                                };
                context.Drivers.Add(larry);

                Assert.True(navBindingList.Contains(larry));
            }
        }

        [Fact]
        public void Entity_marked_as_deleted_in_context_is_removed_from_navigation_property_binding_list()
        {
            using (var context = new F1Context())
            {
                var ferrari = context.Teams.Include(t => t.Drivers).Single(t => t.Id == Team.Ferrari);
                var navBindingList = ((IListSource)ferrari.Drivers).GetList();

                var alonso = context.Drivers.Local.Single(d => d.Name == "Fernando Alonso");
                context.Drivers.Remove(alonso);

                Assert.False(navBindingList.Contains(alonso));
            }
        }

        [Fact]
        public void Entity_added_to_navigation_property_binding_list_is_added_to_context_after_DetectChanges()
        {
            using (var context = new F1Context())
            {
                var ferrari = context.Teams.Include(t => t.Drivers).Single(t => t.Id == Team.Ferrari);
                var navBindingList = ((IListSource)ferrari.Drivers).GetList();
                var localDrivers = context.Drivers.Local;

                var larry = new Driver
                                {
                                    Id = -1,
                                    Name = "Larry David",
                                    TeamId = Team.Ferrari,
                                    CarNumber = 13
                                };
                navBindingList.Add(larry);

                Assert.False(localDrivers.Contains(larry));

                GetObjectContext(context).DetectChanges();

                Assert.True(localDrivers.Contains(larry));
                Assert.Same(larry, context.Drivers.Find(-1));
            }
        }

        [Fact]
        public void
            Entity_removed_from_navigation_property_binding_list_is_removed_from_nav_property_but_not_marked_Deleted()
        {
            using (var context = new F1Context())
            {
                var ferrari = context.Teams.Include(t => t.Drivers).Single(t => t.Id == Team.Ferrari);
                var navBindingList = ((IListSource)ferrari.Drivers).GetList();
                var localDrivers = context.Drivers.Local;

                var alonso = localDrivers.Single(d => d.Name == "Fernando Alonso");
                navBindingList.Remove(alonso);

                Assert.True(localDrivers.Contains(alonso));

                GetObjectContext(context).DetectChanges();

                Assert.True(localDrivers.Contains(alonso)); // Because it is not marked as Deleted

                Assert.False(ferrari.Drivers.Contains(alonso)); // But has been removed from nav prop
            }
        }

        #endregion
    }
}
