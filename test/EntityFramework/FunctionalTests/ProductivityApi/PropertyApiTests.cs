// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.TestHelpers;
    using System.Linq;
    using AdvancedPatternsModel;
    using ConcurrencyModel;
    using SimpleModel;
    using Xunit;
    using Xunit.Extensions;

    /// <summary>
    /// Functional tests for the Property, Reference, and Collection methods on DbEntityEntry.
    /// Unit tests also exist in the unit tests project.
    /// </summary>
    public class PropertyApiTests : FunctionalTestBase
    {
        #region Tests for loading navigation properties

        [Fact]
        public void Generic_reference_navigation_property_can_be_loaded_and_IsLoaded_is_set()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var driver = context.Drivers.Single(d => d.Name == "Jenson Button");
                var teamReference = context.Entry(driver).Reference(d => d.Team);

                Assert.False(teamReference.IsLoaded);
                teamReference.Load();
                Assert.True(teamReference.IsLoaded);
                Assert.Equal(Team.McLaren, driver.Team.Id);
            }
        }

        [Fact]
        public void Generic_collection_navigation_property_can_be_loaded_and_IsLoaded_is_set()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var team = context.Teams.Find(Team.McLaren);
                var driversCollection = context.Entry(team).Collection(t => t.Drivers);

                Assert.False(driversCollection.IsLoaded);
                driversCollection.Load();
                Assert.True(driversCollection.IsLoaded);
                Assert.Equal(3, team.Drivers.Count);
            }
        }

        [Fact]
        public void Non_generic_reference_navigation_property_can_be_loaded_and_IsLoaded_is_set()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var driver = context.Drivers.Single(d => d.Name == "Jenson Button");
                var teamReference = context.Entry((object)driver).Reference("Team");

                Assert.False(teamReference.IsLoaded);
                teamReference.Load();
                Assert.True(teamReference.IsLoaded);
                Assert.Equal(Team.McLaren, driver.Team.Id);
            }
        }

        [Fact]
        public void Non_generic_collection_navigation_property_can_be_loaded_and_IsLoaded_is_set()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var team = context.Teams.Find(Team.McLaren);
                var driversCollection = context.Entry((object)team).Collection("Drivers");

                Assert.False(driversCollection.IsLoaded);
                driversCollection.Load();
                Assert.True(driversCollection.IsLoaded);
                Assert.Equal(3, team.Drivers.Count);
            }
        }

        [Fact]
        public void Collection_navigation_property_for_many_to_many_relationship_can_be_loaded()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var team = context.Teams.Find(Team.McLaren);
                var sponsorsCollection = context.Entry(team).Collection(t => t.Sponsors);

                Assert.False(sponsorsCollection.IsLoaded);
                sponsorsCollection.Load();
                Assert.True(sponsorsCollection.IsLoaded);
                Assert.Equal(3, team.Sponsors.Count);
            }
        }

        [Fact]
        public void Reference_navigation_property_can_be_reloaded_with_AppendOnly_semantics()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var driver = context.Drivers.Single(d => d.Name == "Jenson Button");
                var teamReference = context.Entry(driver).Reference(d => d.Team);

                teamReference.Load();
                Assert.True(teamReference.IsLoaded);

                driver.Team.Principal = "Larry David";

                Assert.True(teamReference.IsLoaded);
                teamReference.Load();

                Assert.Equal("Larry David", driver.Team.Principal);
            }
        }

        [Fact]
        public void Collection_navigation_property_can_be_reloaded_with_AppendOnly_semantics()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var team = context.Teams.Find(Team.McLaren);
                var driversCollection = context.Entry(team).Collection(t => t.Drivers);

                // Load drivers for the first time
                driversCollection.Load();

                Assert.True(driversCollection.IsLoaded);
                Assert.Equal(3, team.Drivers.Count);

                // Now detach one driver from the collection and modify another one; the collection becomes unloaded
                context.Entry(context.Drivers.Local.Single(d => d.Name == "Jenson Button")).State = EntityState.Detached;
                context.Drivers.Local.Single(d => d.Name == "Lewis Hamilton").Wins = -1;

                // Check the collection has become unloaded because of the detach.  Reload it.
                Assert.False(driversCollection.IsLoaded);
                Assert.Equal(2, team.Drivers.Count);

                driversCollection.Load();

                // The detached driver should be back and the modified driver should not have been touched
                Assert.True(driversCollection.IsLoaded);
                Assert.Equal(3, team.Drivers.Count);
                Assert.Equal(-1, context.Drivers.Local.Single(d => d.Name == "Lewis Hamilton").Wins);
            }
        }

        [Fact]
        [AutoRollback, UseDefaultExecutionStrategy]
        public void Collection_navigation_property_can_be_reloaded_even_if_marked_as_loaded()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var team = context.Teams.Find(Team.McLaren);
                var driversCollection = context.Entry(team).Collection(t => t.Drivers);

                // Load drivers for the first time
                driversCollection.Load();

                Assert.True(driversCollection.IsLoaded);
                Assert.Equal(3, team.Drivers.Count);

                // Add a new driver to the database
                using (var innerContext = new F1Context())
                {
                    innerContext.Drivers.Add(
                        new Driver
                            {
                                Name = "Larry David",
                                TeamId = Team.McLaren
                            });
                    innerContext.SaveChanges();
                }

                // Now force load again
                Assert.True(driversCollection.IsLoaded);
                driversCollection.Load();

                Assert.True(driversCollection.IsLoaded);
                Assert.Equal(4, team.Drivers.Count);
            }
        }

        [Fact]
        public void Reference_navigation_property_can_be_reloaded_after_changing_foreign_key()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var driver = context.Drivers.Single(d => d.Name == "Jenson Button");
                var teamReference = context.Entry(driver).Reference(d => d.Team);

                teamReference.Load();
                Assert.True(teamReference.IsLoaded);

                driver.TeamId = Team.Ferrari;

                Assert.True(teamReference.IsLoaded); // Because changes have not been detected yet

                teamReference = context.Entry(driver).Reference(d => d.Team); // Calls DetectChanges
                Assert.False(teamReference.IsLoaded);
                teamReference.Load();
                Assert.True(teamReference.IsLoaded);
                Assert.Equal(Team.Ferrari, driver.Team.Id);
            }
        }

        [Fact]
        public void Related_reference_is_not_lazy_loaded_when_IsLoaded_is_set_to_true()
        {
            using (var context = new F1Context())
            {
                var driver = context.Drivers.Single(d => d.Name == "Jenson Button");
                var teamReference = context.Entry(driver).Reference(d => d.Team);
                
                teamReference.IsLoaded = true;

                Assert.True(teamReference.IsLoaded);
                Assert.Null(driver.Team);
                Assert.Null(teamReference.CurrentValue);
            }
        }

        [Fact]
        public void Related_reference_can_still_be_explicitly_loaded_when_IsLoaded_is_set_to_true()
        {
            using (var context = new F1Context())
            {
                var driver = context.Drivers.Single(d => d.Name == "Jenson Button");
                var teamReference = context.Entry(driver).Reference(d => d.Team);
                
                teamReference.IsLoaded = true;

                Assert.True(teamReference.IsLoaded);
                Assert.Null(driver.Team);
                Assert.Null(teamReference.CurrentValue);

                teamReference.Load();

                Assert.True(teamReference.IsLoaded);
                Assert.Equal(Team.McLaren, driver.Team.Id);
                Assert.Equal(Team.McLaren, teamReference.CurrentValue.Id);
            }
        }

        [Fact]
        public void Related_reference_is_lazy_loaded_again_when_IsLoaded_is_changed_back_to_false()
        {
            using (var context = new F1Context())
            {
                var driver = context.Drivers.Single(d => d.Name == "Jenson Button");
                var teamReference = context.Entry(driver).Reference(d => d.Team);

                teamReference.IsLoaded = true;

                Assert.True(teamReference.IsLoaded);
                Assert.Null(driver.Team);
                Assert.Null(teamReference.CurrentValue);

                teamReference.IsLoaded = false;

                Assert.Equal(Team.McLaren, driver.Team.Id);
                Assert.Equal(Team.McLaren, teamReference.CurrentValue.Id);
                Assert.True(teamReference.IsLoaded);
            }
        }

        [Fact]
        public void Related_collection_is_not_lazy_loaded_when_IsLoaded_is_set_to_true()
        {
            using (var context = new F1Context())
            {
                var team = context.Teams.Find(Team.McLaren);
                var driversCollection = context.Entry(team).Collection(t => t.Drivers);

                driversCollection.IsLoaded = true;

                Assert.True(driversCollection.IsLoaded);
                Assert.Empty(team.Drivers);
                Assert.Empty(driversCollection.CurrentValue);
            }
        }

        [Fact]
        public void Related_collection_can_still_be_explicitly_loaded_when_IsLoaded_is_set_to_true()
        {
            using (var context = new F1Context())
            {
                var team = context.Teams.Find(Team.McLaren);
                var driversCollection = context.Entry(team).Collection(t => t.Drivers);

                driversCollection.IsLoaded = true;

                Assert.True(driversCollection.IsLoaded);
                Assert.Empty(team.Drivers);
                Assert.Empty(driversCollection.CurrentValue);

                driversCollection.Load();

                Assert.True(driversCollection.IsLoaded);
                Assert.Equal(3, team.Drivers.Count);
            }
        }

        [Fact]
        public void Related_collection_is_lazy_loaded_again_when_IsLoaded_is_changed_back_to_false()
        {
            using (var context = new F1Context())
            {
                var team = context.Teams.Find(Team.McLaren);
                var driversCollection = context.Entry(team).Collection(t => t.Drivers);

                driversCollection.IsLoaded = true;

                Assert.True(driversCollection.IsLoaded);
                Assert.Empty(team.Drivers);
                Assert.Empty(driversCollection.CurrentValue);

                driversCollection.IsLoaded = false;

                Assert.Equal(3, team.Drivers.Count);
                Assert.Equal(3, driversCollection.CurrentValue.Count);
                Assert.True(driversCollection.IsLoaded);
            }
        }

        [Fact]
        public void Related_reference_IsLoaded_is_reset_when_foreign_key_is_changed()
        {
            using (var context = new F1Context())
            {
                var driver = context.Drivers.Single(d => d.Name == "Jenson Button");
                var teamReference = context.Entry(driver).Reference(d => d.Team);

                teamReference.IsLoaded = true;

                Assert.True(teamReference.IsLoaded);
                Assert.Null(driver.Team);
                Assert.Null(teamReference.CurrentValue);

                driver.TeamId = Team.Ferrari;

                Assert.True(teamReference.IsLoaded); // Because changes have not been detected yet
                teamReference = context.Entry(driver).Reference(d => d.Team); // Calls DetectChanges
                Assert.False(teamReference.IsLoaded);

                Assert.Equal(Team.Ferrari, driver.Team.Id);
                Assert.Equal(Team.Ferrari, teamReference.CurrentValue.Id);
                Assert.True(teamReference.IsLoaded);
            }
        }

        [Fact]
        public void Related_reference_IsLoaded_is_reset_when_related_entity_is_detached()
        {
            using (var context = new F1Context())
            {
                var driver = context.Drivers.Single(d => d.Name == "Jenson Button");
                var teamReference = context.Entry(driver).Reference(d => d.Team);
                var originalTeam = driver.Team;

                Assert.True(teamReference.IsLoaded);

                context.Entry(originalTeam).State = EntityState.Detached;

                Assert.False(teamReference.IsLoaded);

                var newTeam = driver.Team;
                Assert.True(teamReference.IsLoaded);
                Assert.Equal(originalTeam.Id, newTeam.Id);
                Assert.NotSame(originalTeam, newTeam);
            }
        }

        [Fact]
        public void Related_collection_IsLoaded_is_reset_when_one_of_the_related_entities_is_detached()
        {
            using (var context = new F1Context())
            {
                var team = context.Teams.Find(Team.McLaren);
                var driversCollection = context.Entry(team).Collection(t => t.Drivers);

                Assert.Equal(3, team.Drivers.Count);
                Assert.True(driversCollection.IsLoaded);

                var originalDriver = team.Drivers.OrderBy(d => d.Id).First();
                context.Entry(originalDriver).State = EntityState.Detached;

                Assert.False(driversCollection.IsLoaded);
                Assert.Equal(3, team.Drivers.Count);
                Assert.True(driversCollection.IsLoaded);

                var newDriver = team.Drivers.OrderBy(d => d.Id).First();
                Assert.Equal(originalDriver.Id, newDriver.Id);
                Assert.NotSame(originalDriver, newDriver);
            }
        }

        [Fact]
        public void Related_collection_reload_after_detach_can_be_avoided_by_setting_IsLoaded_to_true()
        {
            using (var context = new F1Context())
            {
                var team = context.Teams.Find(Team.McLaren);
                var driversCollection = context.Entry(team).Collection(t => t.Drivers);

                Assert.Equal(3, team.Drivers.Count);
                Assert.True(driversCollection.IsLoaded);

                context.Entry(team.Drivers.First()).State = EntityState.Detached;

                Assert.False(driversCollection.IsLoaded);

                driversCollection.IsLoaded = true;
                Assert.Equal(2, team.Drivers.Count);
            }
        }

        #endregion

        #region Tests for loading navigation properties asynchronously

#if !NET40

        [Fact]
        public void Generic_reference_navigation_property_can_be_loaded_asynchronously_and_IsLoaded_is_set()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var driver = context.Drivers.Single(d => d.Name == "Jenson Button");
                var teamReference = context.Entry(driver).Reference(d => d.Team);

                Assert.False(teamReference.IsLoaded);
                teamReference.LoadAsync().Wait();
                Assert.True(teamReference.IsLoaded);
                Assert.Equal(Team.McLaren, driver.Team.Id);
            }
        }

        [Fact]
        public void Generic_collection_navigation_property_can_be_loaded_asynchronously_and_IsLoaded_is_set()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var team = context.Teams.Find(Team.McLaren);
                var driversCollection = context.Entry(team).Collection(t => t.Drivers);

                Assert.False(driversCollection.IsLoaded);
                driversCollection.LoadAsync().Wait();
                Assert.True(driversCollection.IsLoaded);
                Assert.Equal(3, team.Drivers.Count);
            }
        }

        [Fact]
        public void Non_generic_reference_navigation_property_can_be_loaded_asynchronously_and_IsLoaded_is_set()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var driver = context.Drivers.Single(d => d.Name == "Jenson Button");
                var teamReference = context.Entry((object)driver).Reference("Team");

                Assert.False(teamReference.IsLoaded);
                teamReference.LoadAsync().Wait();
                Assert.True(teamReference.IsLoaded);
                Assert.Equal(Team.McLaren, driver.Team.Id);
            }
        }

        [Fact]
        public void Non_generic_collection_navigation_property_can_be_loaded_asynchronously_and_IsLoaded_is_set()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var team = context.Teams.Find(Team.McLaren);
                var driversCollection = context.Entry((object)team).Collection("Drivers");

                Assert.False(driversCollection.IsLoaded);
                driversCollection.LoadAsync().Wait();
                Assert.True(driversCollection.IsLoaded);
                Assert.Equal(3, team.Drivers.Count);
            }
        }

        [Fact]
        public void Collection_navigation_property_for_many_to_many_relationship_can_be_loaded_asynchronously()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var team = context.Teams.Find(Team.McLaren);
                var sponsorsCollection = context.Entry(team).Collection(t => t.Sponsors);

                Assert.False(sponsorsCollection.IsLoaded);
                sponsorsCollection.LoadAsync().Wait();
                Assert.True(sponsorsCollection.IsLoaded);
                Assert.Equal(3, team.Sponsors.Count);
            }
        }

        [Fact]
        public void Reference_navigation_property_can_be_reloaded_asynchronously_with_AppendOnly_semantics()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var driver = context.Drivers.Single(d => d.Name == "Jenson Button");
                var teamReference = context.Entry(driver).Reference(d => d.Team);

                teamReference.LoadAsync().Wait();
                Assert.True(teamReference.IsLoaded);

                driver.Team.Principal = "Larry David";

                Assert.True(teamReference.IsLoaded);
                teamReference.LoadAsync().Wait();

                Assert.Equal("Larry David", driver.Team.Principal);
            }
        }

        [Fact]
        public void Collection_navigation_property_can_be_reloaded_asynchronously_with_AppendOnly_semantics()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var team = context.Teams.Find(Team.McLaren);
                var driversCollection = context.Entry(team).Collection(t => t.Drivers);

                // Load drivers for the first time
                driversCollection.LoadAsync().Wait();

                Assert.True(driversCollection.IsLoaded);
                Assert.Equal(3, team.Drivers.Count);

                // Now detach one driver from the collection and modify another one; the collection becomes unloaded
                context.Entry(context.Drivers.Local.Single(d => d.Name == "Jenson Button")).State = EntityState.Detached;
                context.Drivers.Local.Single(d => d.Name == "Lewis Hamilton").Wins = -1;

                // Check the collection has become unloaded because of the detach.  Reload it.
                Assert.False(driversCollection.IsLoaded);
                Assert.Equal(2, team.Drivers.Count);

                driversCollection.LoadAsync().Wait();

                // The detached driver should be back and the modified driver should not have been touched
                Assert.True(driversCollection.IsLoaded);
                Assert.Equal(3, team.Drivers.Count);
                Assert.Equal(-1, context.Drivers.Local.Single(d => d.Name == "Lewis Hamilton").Wins);
            }
        }

        [Fact]
        [AutoRollback, UseDefaultExecutionStrategy]
        public void Collection_navigation_property_can_be_reloaded_even_if_marked_as_loaded_asynchronously()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var team = context.Teams.Find(Team.McLaren);
                var driversCollection = context.Entry(team).Collection(t => t.Drivers);

                // Load drivers for the first time
                driversCollection.LoadAsync().Wait();

                Assert.True(driversCollection.IsLoaded);
                Assert.Equal(3, team.Drivers.Count);

                // Add a new driver to the database
                using (var innerContext = new F1Context())
                {
                    innerContext.Drivers.Add(
                        new Driver
                            {
                                Name = "Larry David",
                                TeamId = Team.McLaren
                            });
                    innerContext.SaveChanges();
                }

                // Now force load again
                Assert.True(driversCollection.IsLoaded);
                driversCollection.LoadAsync().Wait();

                Assert.True(driversCollection.IsLoaded);
                Assert.Equal(4, team.Drivers.Count);
            }
        }

        [Fact]
        public void Reference_navigation_property_can_be_reloaded_asynchronously_after_changing_foreign_key()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var driver = context.Drivers.Single(d => d.Name == "Jenson Button");
                var teamReference = context.Entry(driver).Reference(d => d.Team);

                teamReference.LoadAsync().Wait();
                Assert.True(teamReference.IsLoaded);

                driver.TeamId = Team.Ferrari;

                Assert.True(teamReference.IsLoaded); // Because changes have not been detected yet

                teamReference = context.Entry(driver).Reference(d => d.Team); // Calls DetectChanges
                Assert.False(teamReference.IsLoaded);
                teamReference.LoadAsync().Wait();
                Assert.True(teamReference.IsLoaded);
                Assert.Equal(Team.Ferrari, driver.Team.Id);
            }
        }

#endif

        #endregion

        #region Tests for bad property names

        // Note that simple cases such as nulls that don't involve EF metadata are tested in the unit tests

        private DbEntityEntry<Team> GetTeamEntry(F1Context context)
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
            team.Engine = new Engine
                              {
                                  Id = -2,
                                  Name = "WubbsyV8",
                                  Teams = new List<Team>
                                              {
                                                  team
                                              },
                                  Gearboxes = new List<Gearbox>()
                              };
            context.Teams.Attach(team);
            return context.Entry(team);
        }

        private DbEntityEntry<Building> GetBuildingEntry(AdvancedPatternsMasterContext context)
        {
            var building = new Building();
            context.Buildings.Attach(building);
            return context.Entry(building);
        }

        [Fact]
        public void Using_Reference_for_a_collection_nav_prop_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Reference(e => e.Drivers)).ValidateMessage(
                    "DbEntityEntry_UsedReferenceForCollectionProp", "Drivers", "Team");
            }
        }

        [Fact]
        public void Using_Reference_for_a_scalar_prop_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Reference(e => e.Name)).ValidateMessage(
                    "DbEntityEntry_NotANavigationProperty", "Name", "Team");
            }
        }

        [Fact]
        public void Using_Reference_for_a_complex_prop_throws()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entry = GetBuildingEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Reference(e => e.Address)).ValidateMessage(
                    "DbEntityEntry_NotANavigationProperty", "Address", "Building");
            }
        }

        [Fact]
        public void Using_Reference_with_a_missing_property_name_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Reference("Foo")).ValidateMessage(
                    "DbEntityEntry_NotANavigationProperty", "Foo", "Team");
            }
        }

        [Fact]
        public void Using_generic_string_Reference_with_a_missing_property_name_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Reference<string>("Foo")).ValidateMessage(
                    "DbEntityEntry_NotANavigationProperty", "Foo", "Team");
            }
        }

        private DbEntityEntry<Gearbox> GetGearboxEntry(F1Context context)
        {
            var team = GetTeamEntry(context).Entity;
            var gearbox = new Gearbox
                              {
                                  Id = 1,
                                  Name = "WubbsyGears"
                              };
            team.Gearbox = gearbox;
            team.Engine.Gearboxes.Add(gearbox);

            context.Entry(team).State = EntityState.Unchanged;

            return context.Entry(gearbox);
        }

        [Fact]
        public void Using_Reference_with_a_hidden_navigation_property_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetGearboxEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Reference("Engine")).ValidateMessage(
                    "DbEntityEntry_NotANavigationProperty", "Engine", "Gearbox");
            }
        }

        [Fact]
        public void Using_generic_string_Reference_with_a_hidden_navigation_property_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetGearboxEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Reference<Engine>("Engine")).ValidateMessage(
                    "DbEntityEntry_NotANavigationProperty", "Engine", "Gearbox");
            }
        }

        [Fact]
        public void Using_wrong_generic_type_with_string_Reference_method_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Reference<Driver>("Engine")).ValidateMessage(
                    "DbEntityEntry_WrongGenericForNavProp", "Engine", "Team", "Driver", "Engine");
            }
        }

        [Fact]
        public void Using_Collection_for_a_reference_nav_prop_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Collection("Engine")).ValidateMessage(
                    "DbEntityEntry_UsedCollectionForReferenceProp", "Engine", "Team");
            }
        }

        [Fact]
        public void Using_Collection_for_a_scalar_prop_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Collection("Name")).ValidateMessage(
                    "DbEntityEntry_NotANavigationProperty", "Name", "Team");
            }
        }

        [Fact]
        public void Using_Collection_for_a_complex_prop_throws()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entry = GetBuildingEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Collection("Address")).ValidateMessage(
                    "DbEntityEntry_NotANavigationProperty", "Address", "Building");
            }
        }

        [Fact]
        public void Using_Collection_with_a_missing_property_name_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Collection("Foo")).ValidateMessage(
                    "DbEntityEntry_NotANavigationProperty", "Foo", "Team");
            }
        }

        [Fact]
        public void Using_Collection_with_a_hidden_navigation_property_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetGearboxEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Collection("Teams")).ValidateMessage(
                    "DbEntityEntry_NotANavigationProperty", "Teams", "Gearbox");
            }
        }

        [Fact]
        public void Using_wrong_generic_type_with_string_Collection_method_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Collection<EngineSupplier>("Drivers")).ValidateMessage(
                    "DbEntityEntry_WrongGenericForNavProp", "Drivers", "Team", "EngineSupplier", "Driver");
            }
        }

        [Fact]
        public void Using_Property_for_a_reference_nav_prop_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Property(e => e.Engine)).ValidateMessage(
                    "DbEntityEntry_NotAScalarProperty", "Engine", "Team");
            }
        }

        [Fact]
        public void Using_Property_for_a_collection_nav_prop_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Property(e => e.Drivers)).ValidateMessage(
                    "DbEntityEntry_NotAScalarProperty", "Drivers", "Team");
            }
        }

        [Fact]
        public void Using_Property_with_a_missing_property_name_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Property("Foo")).ValidateMessage(
                    "DbEntityEntry_NotAScalarProperty", "Foo", "Team");
            }
        }

        [Fact]
        public void Using_ComplexProperty_for_a_reference_nav_prop_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.ComplexProperty(e => e.Engine)).ValidateMessage(
                    "DbEntityEntry_NotAComplexProperty", "Engine", "Team");
            }
        }

        [Fact]
        public void Using_ComplexProperty_for_a_collection_nav_prop_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.ComplexProperty(e => e.Drivers)).ValidateMessage(
                    "DbEntityEntry_NotAComplexProperty", "Drivers", "Team");
            }
        }

        [Fact]
        public void Using_ComplexProperty_for_a_scalar_property_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.ComplexProperty(e => e.Name)).ValidateMessage(
                    "DbEntityEntry_NotAComplexProperty", "Name", "Team");
            }
        }

        [Fact]
        public void Using_ComplexProperty_with_a_missing_property_name_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.ComplexProperty("Foo")).ValidateMessage(
                    "DbEntityEntry_NotAComplexProperty", "Foo", "Team");
            }
        }

        [Fact]
        public void Using_Member_with_a_missing_property_name_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Member("Foo")).ValidateMessage(
                    "DbEntityEntry_NotAProperty", "Foo", "Team");
            }
        }

        [Fact]
        public void Using_wrong_generic_type_with_string_Property_method_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Property<int>("Name")).ValidateMessage(
                    "DbEntityEntry_WrongGenericForProp", "Name", "Team", "Int32", "String");
            }
        }

        [Fact]
        public void Using_wrong_generic_type_with_complex_string_Property_method_throws()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entry = GetBuildingEntry(context);

                Assert.Throws<ArgumentException>(() => entry.ComplexProperty<string>("Address")).ValidateMessage(
                    "DbEntityEntry_WrongGenericForProp", "Address", "Building", "String", "Address");
            }
        }

        [Fact]
        public void Using_wrong_generic_type_with_nested_scalar_string_Property_method_throws()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var propertyEntry = GetBuildingEntry(context).ComplexProperty(b => b.Address);

                Assert.Throws<ArgumentException>(() => propertyEntry.Property<Random>("City")).ValidateMessage(
                    "DbEntityEntry_WrongGenericForProp", "City", "Address", "Random", "String");
            }
        }

        [Fact]
        public void Using_wrong_generic_type_with_nested_complex_string_Property_method_throws()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var propertyEntry = GetBuildingEntry(context).ComplexProperty(b => b.Address);

                Assert.Throws<ArgumentException>(() => propertyEntry.Property<Building>("SiteInfo")).ValidateMessage(
                    "DbEntityEntry_WrongGenericForProp", "SiteInfo", "Address", "Building", "SiteInfo");
            }
        }

        #endregion

        #region Tests for DbReferenceEntry.Query() and DbCollectionEntry.Query()

        [Fact]
        public void Generic_Query_for_reference_loads_related_end()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var driver = context.Drivers.Single(d => d.Name == "Jenson Button");
                var teamReference = context.Entry(driver).Reference(d => d.Team);

                var query = teamReference.Query();
                query.Where(t => t.Id == Team.Ferrari).Load(); // Should not bring anything in

                Assert.False(teamReference.IsLoaded);

                query.Load();

                Assert.True(teamReference.IsLoaded);
                Assert.Equal(Team.McLaren, driver.Team.Id);
            }
        }

        [Fact]
        public void Generic_Query_for_collection_loads_related_end()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var team = context.Teams.Find(Team.McLaren);
                var driversCollection = context.Entry(team).Collection(t => t.Drivers);

                var query = driversCollection.Query();
                query.Where(d => d.Wins > 0).Load(); // Should only bring in two drivers

                Assert.False(driversCollection.IsLoaded);
                Assert.Equal(2, team.Drivers.Count);

                query.Load();

                Assert.False(driversCollection.IsLoaded);
                Assert.Equal(3, team.Drivers.Count);
            }
        }

        [Fact]
        public void Non_Generic_Query_for_reference_loads_related_end()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var driver = context.Drivers.Single(d => d.Name == "Jenson Button");
                var teamReference = context.Entry(driver).Reference("Team");

                var query = teamReference.Query();
                query.Cast<Team>().Where(t => t.Id == Team.Ferrari).Load(); // Should not bring anything in

                Assert.False(teamReference.IsLoaded);

                query.Load();

                Assert.True(teamReference.IsLoaded);
                Assert.Equal(Team.McLaren, driver.Team.Id);
            }
        }

        [Fact]
        public void Non_Generic_Query_for_collection_loads_related_end()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var team = context.Teams.Find(Team.McLaren);
                var driversCollection = context.Entry(team).Collection("Drivers");

                var query = driversCollection.Query();
                query.Cast<Driver>().Where(d => d.Wins > 0).Load(); // Should only bring in two drivers

                Assert.False(driversCollection.IsLoaded);
                Assert.Equal(2, team.Drivers.Count);

                query.Load();

                Assert.False(driversCollection.IsLoaded);
                Assert.Equal(3, team.Drivers.Count);
            }
        }

        [Fact]
        public void Query_for_collection_can_be_used_to_count_without_loading_entities()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var team = context.Teams.Find(Team.McLaren);
                var driversCollection = context.Entry(team).Collection(t => t.Drivers);

                var count = driversCollection.Query().Count();

                Assert.False(driversCollection.IsLoaded);
                Assert.Equal(0, team.Drivers.Count);
                Assert.Equal(3, count);
            }
        }

        [Fact]
        public void Query_for_many_to_many_doesnt_work_well()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var team = context.Teams.Find(Team.McLaren);
                var sponsorsCollection = context.Entry(team).Collection(t => t.Sponsors);

                var query = sponsorsCollection.Query();
                query.Load();

                Assert.False(sponsorsCollection.IsLoaded);

                // This is due to a bug in CreateSourceQuery in core EF that cannot
                // be fixed in the productivity improvements.
                Assert.Equal(0, team.Sponsors.Count);
            }
        }

        #endregion

        #region Tests for access to current and original values for entities in different states

        [Fact]
        public void Current_value_can_be_read_and_set_for_an_object_in_the_Deleted_state()
        {
            TestPropertyValuePositiveForState(e => e.CurrentValue, (e, v) => e.CurrentValue = v, EntityState.Deleted);
        }

        [Fact]
        public void Original_value_can_be_read_and_set_for_an_object_in_the_Deleted_state()
        {
            TestPropertyValuePositiveForState(e => e.OriginalValue, (e, v) => e.OriginalValue = v, EntityState.Deleted);
        }

        [Fact]
        public void Current_value_can_be_read_and_set_for_an_object_in_the_Unchanged_state()
        {
            TestPropertyValuePositiveForState(e => e.CurrentValue, (e, v) => e.CurrentValue = v, EntityState.Unchanged);
        }

        [Fact]
        public void Original_value_can_be_read_and_set_for_an_object_in_the_Unchanged_state()
        {
            TestPropertyValuePositiveForState(e => e.OriginalValue, (e, v) => e.OriginalValue = v, EntityState.Unchanged);
        }

        [Fact]
        public void Current_value_can_be_read_and_set_for_an_object_in_the_Modified_state()
        {
            TestPropertyValuePositiveForState(e => e.CurrentValue, (e, v) => e.CurrentValue = v, EntityState.Modified);
        }

        [Fact]
        public void Original_value_can_be_read_and_set_for_an_object_in_the_Modified_state()
        {
            TestPropertyValuePositiveForState(e => e.OriginalValue, (e, v) => e.OriginalValue = v, EntityState.Modified);
        }

        [Fact]
        public void Current_value_can_be_read_and_set_for_an_object_in_the_Added_state()
        {
            TestPropertyValuePositiveForState(e => e.CurrentValue, (e, v) => e.CurrentValue = v, EntityState.Added);
        }

        [Fact]
        public void Original_value_cannot_be_read_or_set_for_an_object_in_the_Added_state()
        {
            var state = EntityState.Added;
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var entry = context.Entry(building);
                var propEntry = entry.Property(b => b.Name);

                entry.State = state;

                Assert.Throws<InvalidOperationException>(() => { var _ = propEntry.OriginalValue; }).ValidateMessage(
                    "DbPropertyValues_CannotGetValuesForState", "OriginalValues", state.ToString());
                Assert.Throws<InvalidOperationException>(() => propEntry.OriginalValue = "").ValidateMessage(
                    "DbPropertyValues_CannotGetValuesForState", "OriginalValues", state.ToString());
            }
        }

        [Fact]
        public void Current_value_can_be_read_and_set_for_a_Detached_object()
        {
            TestPropertyValuePositiveForState(e => e.CurrentValue, (e, v) => e.CurrentValue = v, EntityState.Detached);
        }

        private void TestPropertyValuePositiveForState(
            Func<DbPropertyEntry<Building, string>, string> getValue,
            Action<DbPropertyEntry<Building, string>, string> setValue,
            EntityState state)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var entry = context.Entry(building);
                var propEntry = entry.Property(b => b.Name);
                entry.State = state;

                Assert.Equal("Building One", getValue(propEntry));

                setValue(propEntry, "New Building");
                Assert.Equal("New Building", getValue(propEntry));
            }
        }

        #endregion

        #region IsModified

        [Fact]
        public void IsModified_returns_true_only_for_modified_properties()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                building.Name = "New Building";
                building.Address.State = "IA";

                ValidateModifiedProperties(context, building);
            }
        }

        [Fact]
        public void IsModified_can_be_set_to_true_when_it_is_currently_false()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var entry = context.Entry(building);

                entry.Property(b => b.Name).IsModified = true;
                entry.ComplexProperty(b => b.Address).IsModified = true;

                ValidateModifiedProperties(context, building);
            }
        }

        [Fact]
        public void
            IsModified_can_be_set_to_true_on_nested_property_when_it_is_currently_false_and_entire_complex_property_is_marked_modified()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var entry = context.Entry(building);

                entry.Property(b => b.Name).IsModified = true;
                entry.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo).Property(i => i.Zone).IsModified
                    = true;

                ValidateModifiedProperties(context, building);
            }
        }

        [Fact]
        public void IsModified_can_be_set_to_true_when_it_is_currently_true()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var entry = context.Entry(building);
                entry.Property(b => b.Name).IsModified = true;
                entry.ComplexProperty(b => b.Address).IsModified = true;
                entry.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo).Property(i => i.Zone).IsModified
                    = true;

                entry.Property(b => b.Name).IsModified = true;
                entry.ComplexProperty(b => b.Address).IsModified = true;
                entry.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo).Property(i => i.Zone).IsModified
                    = true;

                ValidateModifiedProperties(context, building);
            }
        }

        private void ValidateModifiedProperties(AdvancedPatternsMasterContext context, Building building)
        {
            var entry = context.Entry(building);

            Assert.Equal(EntityState.Modified, entry.State);
            Assert.True(entry.Property(b => b.Name).IsModified);
            Assert.True(entry.ComplexProperty(b => b.Address).IsModified);
            Assert.False(entry.Property(b => b.Value).IsModified);
            Assert.True(entry.ComplexProperty(b => b.Address).Property(a => a.Street).IsModified);
            Assert.True(entry.ComplexProperty(b => b.Address).Property(a => a.City).IsModified);
            Assert.True(entry.ComplexProperty(b => b.Address).Property(a => a.State).IsModified);
            Assert.True(entry.ComplexProperty(b => b.Address).Property(a => a.ZipCode).IsModified);
            Assert.True(entry.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo).IsModified);
            Assert.True(
                entry.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo).Property(i => i.Environment).
                    IsModified);
            Assert.True(
                entry.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo).Property(i => i.Zone).IsModified);

            var objectContext = GetObjectContext(context);
            var modified = objectContext.ObjectStateManager.GetObjectStateEntry(building).GetModifiedProperties();
            Assert.Equal(2, modified.Count());
            Assert.True(modified.Contains("Name"));
            Assert.True(modified.Contains("Address"));
        }

        [Fact]
        public void IsModified_can_be_set_to_false_when_it_is_currently_false()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var entry = context.Entry(building);

                entry.Property(b => b.Name).IsModified = false;
                entry.ComplexProperty(b => b.Address).IsModified = false;
                entry.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo).Property(i => i.Zone).IsModified
                    = false;

                Assert.False(entry.Property(b => b.Name).IsModified);
                Assert.False(entry.ComplexProperty(b => b.Address).IsModified);
                Assert.False(
                    entry.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo).Property(i => i.Zone).
                        IsModified);
                Assert.Equal(EntityState.Unchanged, entry.State);

                var objectContext = GetObjectContext(context);
                Assert.Equal(
                    0,
                    objectContext.ObjectStateManager.GetObjectStateEntry(building).GetModifiedProperties().
                        Count());
            }
        }

        private void ValidateBuildingAndNameNotModified(AdvancedPatternsMasterContext context, Building building)
        {
            var entry = context.Entry(building);

            Assert.False(entry.Property(b => b.Name).IsModified);
            Assert.False(entry.ComplexProperty(b => b.Address).IsModified);
            Assert.False(entry.ComplexProperty(b => b.Address).Property(a => a.Street).IsModified);
            Assert.False(entry.ComplexProperty(b => b.Address).Property(a => a.City).IsModified);
            Assert.False(entry.ComplexProperty(b => b.Address).Property(a => a.State).IsModified);
            Assert.False(entry.ComplexProperty(b => b.Address).Property(a => a.ZipCode).IsModified);
            Assert.False(entry.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo).IsModified);
            Assert.False(
                entry.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo).Property(i => i.Environment).
                    IsModified);
            Assert.False(
                entry.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo).Property(i => i.Zone).IsModified);

            Assert.Equal("Building One", entry.Property(b => b.Name).CurrentValue);
            Assert.Equal("100 Work St", entry.ComplexProperty(b => b.Address).Property(a => a.Street).CurrentValue);
            Assert.Equal("Redmond", entry.ComplexProperty(b => b.Address).Property(a => a.City).CurrentValue);
            Assert.Equal("WA", entry.ComplexProperty(b => b.Address).Property(a => a.State).CurrentValue);
            Assert.Equal("98052", entry.ComplexProperty(b => b.Address).Property(a => a.ZipCode).CurrentValue);
            Assert.Equal(
                "Clean",
                entry.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo).Property(
                    i => i.Environment).CurrentValue);
            Assert.Equal(
                1,
                entry.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo).Property(i => i.Zone).
                    CurrentValue);

            var objectContext = GetObjectContext(context);
            var modified = objectContext.ObjectStateManager.GetObjectStateEntry(building).GetModifiedProperties();
            Assert.False(modified.Contains("Name"));
            Assert.False(modified.Contains("Address"));
        }

        private static Address CreateNewAddress()
        {
            return new Address
                       {
                           Street = "300 Main St",
                           City = "Ames",
                           State = "IA",
                           ZipCode = "50010",
                           SiteInfo = new SiteInfo
                                          {
                                              Zone = 3,
                                              Environment = "Contaminated"
                                          }
                       };
        }

        [Fact]
        public void Setting_IsModified_to_false_for_a_modified_property_rejects_changes_to_that_property()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var entry = context.Entry(building);
                entry.Property(b => b.Name).CurrentValue = "Oops I Did It Again!";
                Assert.True(entry.Property(b => b.Name).IsModified);

                entry.Property(b => b.Name).IsModified = false;

                ValidateBuildingAndNameNotModified(context, building);
            }
        }

        [Fact]
        public void Setting_IsModified_to_false_for_a_modified_complex_property_rejects_changes_to_that_property()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var entry = context.Entry(building);
                entry.ComplexProperty(b => b.Address).CurrentValue = CreateNewAddress();
                Assert.True(entry.Property(b => b.Address).IsModified);

                entry.ComplexProperty(b => b.Address).IsModified = false;

                ValidateBuildingAndNameNotModified(context, building);
            }
        }

        [Fact]
        public void
            Rejecting_changes_to_a_complex_property_creates_a_new_complex_object_which_is_then_not_detected_as_changed_by_future_DetectChanges
            ()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var entry = context.Entry(building);
                var originalAddress = building.Address;
                var originalSiteInfo = originalAddress.SiteInfo;
                var newAddress = CreateNewAddress();
                var newSiteInfo = newAddress.SiteInfo;

                entry.ComplexProperty(b => b.Address).CurrentValue = newAddress;
                Assert.True(entry.Property(b => b.Address).IsModified);

                entry.ComplexProperty(b => b.Address).IsModified = false;

                ValidateBuildingAndNameNotModified(context, building);

                Assert.NotSame(newAddress, building.Address);
                Assert.NotSame(newSiteInfo, building.Address.SiteInfo);

                Assert.NotSame(originalAddress, building.Address);
                Assert.NotSame(originalSiteInfo, building.Address.SiteInfo);

                Assert.Equal("300 Main St", newAddress.Street);
                Assert.Equal("Ames", newAddress.City);
                Assert.Equal("IA", newAddress.State);
                Assert.Equal("50010", newAddress.ZipCode);
                Assert.Equal("Contaminated", newAddress.SiteInfo.Environment);
                Assert.Equal(3, newAddress.SiteInfo.Zone);

                context.ChangeTracker.DetectChanges();
                ValidateBuildingAndNameNotModified(context, building);
            }
        }

        [Fact]
        public void
            Setting_IsModified_to_false_for_a_nested_property_of_a_modified_complex_property_rejects_changes_to_the_top_level_complex_property
            ()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var entry = context.Entry(building);
                entry.ComplexProperty(b => b.Address).Property(a => a.City).CurrentValue = "Madrid";
                Assert.True(entry.Property(b => b.Address).IsModified);

                entry.ComplexProperty(b => b.Address).Property(a => a.City).IsModified = false;

                ValidateBuildingAndNameNotModified(context, building);
            }
        }

        [Fact]
        public void Setting_IsModified_to_false_for_a_modified_property_marks_the_entity_as_Unchanged_if_no_properties_remain_modified()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var entry = context.Entry(building);
                entry.Property(b => b.Value).CurrentValue = 100.0M;
                entry.Property(b => b.Name).CurrentValue = "Oops I Did It Again!";

                entry.Property(b => b.Value).IsModified = false;
                Assert.Equal(EntityState.Modified, entry.State); // Name is still modified

                entry.Property(b => b.Name).IsModified = false;
                Assert.Equal(EntityState.Unchanged, entry.State); // Nothing is modified
            }
        }

        [Fact]
        public void
            Setting_IsModified_to_false_for_a_modified_complex_property_marks_the_entity_as_Unchanged_if_no_properties_remain_modified()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var entry = context.Entry(building);
                entry.Property(b => b.Value).CurrentValue = 100.0M;
                entry.ComplexProperty(b => b.Address).CurrentValue = CreateNewAddress();

                entry.ComplexProperty(b => b.Address).IsModified = false;
                Assert.Equal(EntityState.Modified, entry.State); // Value is still modified

                entry.ComplexProperty(b => b.Address).CurrentValue = CreateNewAddress();
                entry.Property(b => b.Value).IsModified = false;

                entry.ComplexProperty(b => b.Address).IsModified = false;
                Assert.Equal(EntityState.Unchanged, entry.State); // Nothing is modified
            }
        }

        [Fact]
        public void
            Setting_IsModified_to_false_for_a_nested_property_of_a_modified_complex_property_marks_the_entity_as_Unchanged_if_no_properties_remain_modified
            ()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var entry = context.Entry(building);
                entry.Property(b => b.Value).CurrentValue = 100.0M;
                entry.ComplexProperty(b => b.Address).Property(a => a.City).CurrentValue = "Madrid";

                entry.ComplexProperty(b => b.Address).Property(a => a.City).IsModified = false;
                Assert.False(entry.ComplexProperty(b => b.Address).Property(a => a.City).IsModified);
                Assert.Equal("Redmond", building.Address.City);
                Assert.Equal(EntityState.Modified, entry.State); // Value is still modified

                entry.ComplexProperty(b => b.Address).Property(a => a.City).CurrentValue = "Madrid";
                entry.Property(b => b.Value).IsModified = false;

                entry.ComplexProperty(b => b.Address).Property(a => a.City).IsModified = false;
                Assert.False(entry.ComplexProperty(b => b.Address).Property(a => a.City).IsModified);
                Assert.Equal("Redmond", building.Address.City);
                Assert.Equal(EntityState.Unchanged, entry.State); // Nothing is modified
            }
        }

        [Fact]
        public void
            Setting_IsModified_to_false_for_a_modified_property_which_is_a_conceptual_null_clears_that_conceptual_null()
        {
            using (var context = new F1Context())
            {
                var hamilton = context.Drivers.Where(d => d.Name == "Lewis Hamilton").Include(d => d.Team).Single();
                hamilton.Team = null; // Creates conceptual null
                var entry = context.Entry(hamilton);
                Assert.True(entry.Property(p => p.TeamId).IsModified);

                entry.Property(p => p.TeamId).IsModified = false;

                Assert.False(entry.Property(p => p.TeamId).IsModified);
                Assert.Equal(EntityState.Unchanged, entry.State);

                GetObjectContext(context).AcceptAllChanges(); // Will throw if there is a conceptual null
            }
        }

        [Fact]
        public void Setting_IsModified_to_false_for_a_modified_property_on_an_entity_which_also_has_a_conceptual_null_does_not_throw()
        {
            using (var context = new F1Context())
            {
                var hamilton = context.Drivers.Where(d => d.Name == "Lewis Hamilton").Include(d => d.Team).Single();
                hamilton.Team = null; // Creates conceptual null
                hamilton.Races++;
                var entry = context.Entry(hamilton);

                entry.Property(p => p.Races).IsModified = false; // Test that this doesn't throw

                Assert.True(entry.Property(p => p.TeamId).IsModified);
                Assert.Equal(EntityState.Modified, entry.State);

                // Conceptual null is still set so should throw now.
                Assert.Throws<InvalidOperationException>(() => GetObjectContext(context).AcceptAllChanges()).
                    ValidateMessage("ObjectContext_CommitWithConceptualNull");
            }
        }

        [Fact]
        public void
            IsModified_stays_true_for_properties_of_a_complex_property_until_changes_are_rejected_to_all_properties()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");

                building.Address.City = "Grimsby";
                building.Address.State = "UK";
                building.Address.SiteInfo.Environment = "Fishy";

                var addressEntry = context.Entry(building).ComplexProperty(b => b.Address);
                AssertStateOfAddressProperties(addressEntry, "Grimsby", "UK", "Fishy", isModified: true);

                addressEntry.Property(a => a.City).IsModified = false;
                AssertStateOfAddressProperties(addressEntry, "Redmond", "UK", "Fishy", isModified: true);

                addressEntry.Property(a => a.State).IsModified = false;
                AssertStateOfAddressProperties(addressEntry, "Redmond", "WA", "Fishy", isModified: true);

                addressEntry.ComplexProperty(a => a.SiteInfo).Property(s => s.Environment).IsModified = false;
                AssertStateOfAddressProperties(addressEntry, "Redmond", "WA", "Clean", isModified: false);
            }
        }

        private void AssertStateOfAddressProperties(
            DbComplexPropertyEntry<Building, Address> addressEntry, string city,
            string state, string environment, bool isModified)
        {
            Assert.Equal(isModified, addressEntry.IsModified);
            Assert.Equal(isModified, addressEntry.Property(a => a.City).IsModified);
            Assert.Equal(isModified, addressEntry.Property(a => a.State).IsModified);
            Assert.Equal(
                isModified,
                addressEntry.ComplexProperty(a => a.SiteInfo).Property(s => s.Environment).IsModified);

            Assert.Equal(city, addressEntry.Property(a => a.City).CurrentValue);
            Assert.Equal(state, addressEntry.Property(a => a.State).CurrentValue);
            Assert.Equal(
                environment,
                addressEntry.ComplexProperty(a => a.SiteInfo).Property(s => s.Environment).CurrentValue);

            Assert.Equal(isModified ? EntityState.Modified : EntityState.Unchanged, addressEntry.EntityEntry.State);
        }

        [Fact]
        public void
            IsModified_stays_true_for_properties_of_a_complex_property_until_changes_are_rejected_to_all_properties_even_if_the_instance_has_been_changed
            ()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var addressEntry = context.Entry(building).ComplexProperty(b => b.Address);

                var originalAddress = building.Address;
                var newAddress = CloneAddress(originalAddress);
                newAddress.SiteInfo = originalAddress.SiteInfo; // Keep same nested complex instance

                building.Address = newAddress;
                building.Address.City = "Grimsby";

                addressEntry = context.Entry(building).ComplexProperty(b => b.Address);
                AssertStateOfAddressProperties(addressEntry, "Grimsby", "WA", "Clean", isModified: true);

                addressEntry.Property(a => a.City).IsModified = false;
                AssertStateOfAddressProperties(addressEntry, "Redmond", "WA", "Clean", isModified: false);
            }
        }

        [Fact]
        public void RejectPropertyChanges_throws_for_a_null_property_name()
        {
            using (var context = new F1Context())
            {
                var hamilton = context.Drivers.Where(d => d.Name == "Lewis Hamilton").Single();
                var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(hamilton);

                Assert.Throws<ArgumentException>(() => stateEntry.RejectPropertyChanges(null))
                    .ValidateMessage("ArgumentIsNullOrWhitespace", "propertyName");
            }
        }

        [Fact]
        public void RejectPropertyChanges_throws_for_a_bad_property_name()
        {
            using (var context = new F1Context())
            {
                var hamilton = context.Drivers.Where(d => d.Name == "Lewis Hamilton").Single();
                var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(hamilton);

                Assert.Throws<ArgumentException>(() => stateEntry.RejectPropertyChanges("bing")).ValidateMessage(
                    "ObjectStateEntry_SetModifiedOnInvalidProperty", "bing");
            }
        }

        [Fact]
        public void RejectPropertyChanges_throws_for_a_navigation_property_name()
        {
            using (var context = new F1Context())
            {
                var hamilton = context.Drivers.Where(d => d.Name == "Lewis Hamilton").Single();
                var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(hamilton);

                Assert.Throws<ArgumentException>(() => stateEntry.RejectPropertyChanges("Team")).ValidateMessage(
                    "ObjectStateEntry_SetModifiedOnInvalidProperty", "Team");
            }
        }

        [Fact]
        public void RejectPropertyChanges_throws_for_a_detached_entity()
        {
            using (var context = new F1Context())
            {
                var hamilton = context.Drivers.Where(d => d.Name == "Lewis Hamilton").Single();
                var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(hamilton);
                context.Entry(hamilton).State = EntityState.Detached;

                Assert.Throws<InvalidOperationException>(() => stateEntry.RejectPropertyChanges("Name")).ValidateMessage
                    ("ObjectStateEntry_InvalidState");
            }
        }

        [Fact]
        public void RejectPropertyChanges_throws_for_an_added_entity()
        {
            using (var context = new F1Context())
            {
                var hamilton = context.Drivers.Where(d => d.Name == "Lewis Hamilton").Single();
                var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(hamilton);
                context.Entry(hamilton).State = EntityState.Added;

                Assert.Throws<InvalidOperationException>(() => stateEntry.RejectPropertyChanges("Name")).ValidateMessage
                    ("ObjectStateEntry_SetModifiedStates");
            }
        }

        [Fact]
        public void RejectPropertyChanges_throws_for_a_deleted_entity()
        {
            using (var context = new F1Context())
            {
                var hamilton = context.Drivers.Where(d => d.Name == "Lewis Hamilton").Single();
                var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(hamilton);
                context.Entry(hamilton).State = EntityState.Deleted;

                Assert.Throws<InvalidOperationException>(() => stateEntry.RejectPropertyChanges("Name")).ValidateMessage
                    ("ObjectStateEntry_SetModifiedStates");
            }
        }

        [Fact]
        public void RejectPropertyChanges_throws_for_a_stub_entry()
        {
            using (var context = new SimpleModelForLinq())
            {
                context.Orders.Load();
                var stateEntry = GetObjectContext(context)
                    .ObjectStateManager
                    .GetObjectStateEntries(~EntityState.Detached)
                    .Where(s => s.Entity == null && !s.IsRelationship)
                    .First();

                Assert.Throws<InvalidOperationException>(() => stateEntry.RejectPropertyChanges("Name")).ValidateMessage
                    ("ObjectStateEntry_CannotModifyKeyEntryState");
            }
        }

        [Fact]
        public void RejectPropertyChanges_throws_for_a_relationship_entry()
        {
            using (var context = new SimpleModelForLinq())
            {
                context.Orders.Load();
                var stateEntry = GetObjectContext(context)
                    .ObjectStateManager
                    .GetObjectStateEntries(~EntityState.Detached)
                    .Where(s => s.IsRelationship)
                    .First();

                Assert.Throws<InvalidOperationException>(() => stateEntry.RejectPropertyChanges("Name")).ValidateMessage
                    ("ObjectStateEntry_CantModifyRelationState");
            }
        }

        [Fact]
        public void RejectPropertyChanges_is_noop_for_Unchanged_entity()
        {
            using (var context = new F1Context())
            {
                var hamilton = context.Drivers.Where(d => d.Name == "Lewis Hamilton").Single();
                var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(hamilton);

                stateEntry.RejectPropertyChanges("Name");

                Assert.Equal(EntityState.Unchanged, stateEntry.State);
                Assert.Equal(0, stateEntry.GetModifiedProperties().Count());
            }
        }

        [Fact]
        public void RejectPropertyChanges_is_noop_for_a_property_that_is_not_modified()
        {
            using (var context = new F1Context())
            {
                var hamilton = context.Drivers.Where(d => d.Name == "Lewis Hamilton").Single();
                context.Entry(hamilton).Property(d => d.Podiums).CurrentValue = 1000;
                var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(hamilton);

                stateEntry.RejectPropertyChanges("Name");

                Assert.Equal(EntityState.Modified, stateEntry.State);
                Assert.Equal(1, stateEntry.GetModifiedProperties().Count());
                Assert.True(stateEntry.GetModifiedProperties().Contains("Podiums"));
            }
        }

        private void IsPropertyChangedTest(Action<Building, DbEntityEntry<Building>, ObjectStateEntry> test)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var entry = context.Entry(building);
                var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(building);

                test(building, entry, stateEntry);
            }
        }

        [Fact]
        public void IsPropertyChanged_returns_true_for_scalar_property_that_is_changed_and_marked_as_modified()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var building = context.Buildings.Single(b => b.Name == "Building One");
                var entry = context.Entry(building);
                var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(building);

                entry.Property(b => b.Name).CurrentValue = "Oops I Did It Again!";

                Assert.True(entry.Property(b => b.Name).IsModified);
                Assert.True(stateEntry.IsPropertyChanged("Name"));

                building.Name = "Building Two";
                context.ChangeTracker.DetectChanges();
                building.Name = "Building One";
                context.ChangeTracker.DetectChanges();

                Assert.True(entry.Property(b => b.Name).IsModified);
                Assert.False(stateEntry.IsPropertyChanged("Name"));
            }
        }

        [Fact]
        public void IsPropertyChanged_returns_true_for_scalar_property_that_is_changed_but_not_marked_as_modified()
        {
            IsPropertyChangedTest(
                (building, entry, stateEntry) =>
                    {
                        building.Name = "Oops I Did It Again!";

                        Assert.False(entry.Property(b => b.Name).IsModified);
                        Assert.True(stateEntry.IsPropertyChanged("Name"));
                        Assert.False(entry.Property(b => b.Name).IsModified);
                    });
        }

        [Fact]
        public void IsPropertyChanged_returns_false_for_scalar_property_that_is_not_changed_but_marked_as_modified()
        {
            IsPropertyChangedTest(
                (building, entry, stateEntry) =>
                    {
                        entry.Property(b => b.Name).IsModified = true;

                        Assert.False(stateEntry.IsPropertyChanged("Name"));
                    });
        }

        [Fact]
        public void IsPropertyChanged_returns_false_for_scalar_property_that_is_not_changed_and_not_marked_as_modified()
        {
            IsPropertyChangedTest(
                (building, entry, stateEntry) =>
                    {
                        Assert.False(entry.Property(b => b.Name).IsModified);
                        Assert.False(stateEntry.IsPropertyChanged("Name"));
                        Assert.False(entry.Property(b => b.Name).IsModified);
                    });
        }

        [Fact]
        public void IsPropertyChanged_returns_true_for_scalar_property_that_is_changed_to_null()
        {
            IsPropertyChangedTest(
                (building, entry, stateEntry) =>
                    {
                        building.Name = null;

                        Assert.True(stateEntry.IsPropertyChanged("Name"));
                    });
        }

        private void IsPropertyChanged_returns_true_for_complex_property_that_is_changed_and_marked_as_modified(
            Action<Building, DbComplexPropertyEntry<Building, Address>> changeAddress)
        {
            IsPropertyChangedTest(
                (building, entry, stateEntry) =>
                    {
                        changeAddress(building, entry.ComplexProperty(b => b.Address));

                        Assert.True(entry.Property(b => b.Address).IsModified);
                        Assert.True(stateEntry.IsPropertyChanged("Address"));
                    });
        }

        private void IsPropertyChanged_returns_true_for_complex_property_that_is_changed_but_not_marked_as_modified(
            Action<Building, DbComplexPropertyEntry<Building, Address>> changeAddress)
        {
            IsPropertyChangedTest(
                (building, entry, stateEntry) =>
                    {
                        changeAddress(building, entry.ComplexProperty(b => b.Address));

                        Assert.False(entry.Property(b => b.Address).IsModified);
                        Assert.True(stateEntry.IsPropertyChanged("Address"));
                        Assert.False(entry.Property(b => b.Address).IsModified);
                    });
        }

        private void
            IsPropertyChanged_returns_false_for_complex_property_that_is_changed_by_reference_only_and_marked_as_modified_implementation
            (Action<Building, DbComplexPropertyEntry<Building, Address>> changeAddress)
        {
            IsPropertyChangedTest(
                (building, entry, stateEntry) =>
                    {
                        changeAddress(building, entry.ComplexProperty(b => b.Address));

                        Assert.True(entry.Property(b => b.Address).IsModified);
                        Assert.False(stateEntry.IsPropertyChanged("Address"));
                    });
        }

        private void
            IsPropertyChanged_returns_true_for_complex_property_that_is_changed_by_reference_only_but_not_marked_as_modified
            (Action<Building, DbComplexPropertyEntry<Building, Address>> changeAddress)
        {
            IsPropertyChangedTest(
                (building, entry, stateEntry) =>
                    {
                        changeAddress(building, entry.ComplexProperty(b => b.Address));

                        Assert.False(entry.Property(b => b.Address).IsModified);
                        Assert.False(stateEntry.IsPropertyChanged("Address"));
                        Assert.False(entry.Property(b => b.Address).IsModified);
                    });
        }

        [Fact]
        public void IsPropertyChanged_returns_false_for_complex_property_that_is_changed_by_reference_only_and_marked_as_modified()
        {
            IsPropertyChanged_returns_false_for_complex_property_that_is_changed_by_reference_only_and_marked_as_modified_implementation
                (
                    (b, e) =>
                        {
                            var originalAddress = b.Address;
                            var newAddress = CloneAddress(originalAddress);
                            newAddress.SiteInfo = originalAddress.SiteInfo; // Keep same nested complex instance
                            e.CurrentValue = newAddress;
                        });
        }

        [Fact]
        public void IsPropertyChanged_returns_false_for_complex_property_that_is_changed_by_reference_only_but_not_marked_as_modified()
        {
            IsPropertyChanged_returns_true_for_complex_property_that_is_changed_by_reference_only_but_not_marked_as_modified
                (
                    (b, e) =>
                        {
                            var originalAddress = b.Address;
                            var newAddress = CloneAddress(originalAddress);
                            newAddress.SiteInfo = originalAddress.SiteInfo; // Keep same nested complex instance
                            b.Address = newAddress;
                        });
        }

        [Fact]
        public void
            IsPropertyChanged_returns_true_for_complex_property_that_is_changed_by_mutation_and_marked_as_modified()
        {
            IsPropertyChanged_returns_true_for_complex_property_that_is_changed_and_marked_as_modified(
                (b, e) => e.Property(a => a.City).CurrentValue = "Grimsby");
        }

        [Fact]
        public void
            IsPropertyChanged_returns_true_for_complex_property_that_is_changed_by_mutation_but_not_marked_as_modified()
        {
            IsPropertyChanged_returns_true_for_complex_property_that_is_changed_but_not_marked_as_modified(
                (b, e) => b.Address.City = "Grimsby");
        }

        [Fact]
        public void IsPropertyChanged_returns_true_for_complex_property_that_is_changed_by_nested_property_mutation_and_marked_as_modified()
        {
            IsPropertyChanged_returns_true_for_complex_property_that_is_changed_and_marked_as_modified(
                (b, e) => e.ComplexProperty(a => a.SiteInfo).Property(i => i.Environment).CurrentValue = "Fishy");
        }

        [Fact]
        public void
            IsPropertyChanged_returns_true_for_complex_property_that_is_changed_by_nested_property_mutation_but_not_marked_as_modified()
        {
            IsPropertyChanged_returns_true_for_complex_property_that_is_changed_but_not_marked_as_modified(
                (b, e) => b.Address.SiteInfo.Environment = "Fishy");
        }

        [Fact]
        public void IsPropertyChanged_returns_false_for_complex_property_that_is_changed_by_nested_property_reference_and_marked_as_modified
            ()
        {
            IsPropertyChanged_returns_false_for_complex_property_that_is_changed_by_reference_only_and_marked_as_modified_implementation
                (
                    (b, e) =>
                    e.ComplexProperty(a => a.SiteInfo).CurrentValue =
                    new SiteInfo
                        {
                            Environment = b.Address.SiteInfo.Environment,
                            Zone = b.Address.SiteInfo.Zone
                        });
        }

        [Fact]
        public void
            IsPropertyChanged_returns_false_for_complex_property_that_is_changed_by_nested_property_reference_but_not_marked_as_modified()
        {
            IsPropertyChanged_returns_true_for_complex_property_that_is_changed_by_reference_only_but_not_marked_as_modified
                (
                    (b, e) =>
                    b.Address.SiteInfo =
                    new SiteInfo
                        {
                            Environment = b.Address.SiteInfo.Environment,
                            Zone = b.Address.SiteInfo.Zone
                        });
        }

        [Fact]
        public void IsPropertyChanged_returns_false_for_complex_property_that_is_not_changed_but_marked_as_modified()
        {
            IsPropertyChangedTest(
                (building, entry, stateEntry) =>
                    {
                        entry.Property(b => b.Name).IsModified = true;

                        Assert.False(stateEntry.IsPropertyChanged("Address"));
                    });
        }

        [Fact]
        public void IsPropertyChanged_returns_false_for_complex_property_that_is_not_changed_and_not_marked_as_modified()
        {
            IsPropertyChangedTest(
                (building, entry, stateEntry) =>
                    {
                        Assert.False(entry.Property(b => b.Address).IsModified);
                        Assert.False(stateEntry.IsPropertyChanged("Address"));
                        Assert.False(entry.Property(b => b.Address).IsModified);
                    });
        }

        [Fact]
        public void IsPropertyChanged_throws_if_complex_property_is_changed_to_null()
        {
            IsPropertyChangedTest(
                (building, entry, stateEntry) =>
                    {
                        building.Address = null;

                        Assert.Throws<InvalidOperationException>(
                            () => stateEntry.IsPropertyChanged("Address")).ValidateMessage(
                                "ComplexObject_NullableComplexTypesNotSupported", "Address");
                    });
        }

        [Fact]
        public void IsPropertyChanged_throws_for_a_null_property_name()
        {
            using (var context = new F1Context())
            {
                var hamilton = context.Drivers.Where(d => d.Name == "Lewis Hamilton").Single();
                var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(hamilton);

                Assert.Throws<ArgumentException>(() => stateEntry.IsPropertyChanged(null))
                    .ValidateMessage("ArgumentIsNullOrWhitespace", "propertyName");
            }
        }

        [Fact]
        public void IsPropertyChanged_throws_for_a_bad_property_name()
        {
            using (var context = new F1Context())
            {
                var hamilton = context.Drivers.Where(d => d.Name == "Lewis Hamilton").Single();
                var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(hamilton);

                Assert.Throws<ArgumentException>(() => stateEntry.IsPropertyChanged("bing")).ValidateMessage(
                    "ObjectStateEntry_SetModifiedOnInvalidProperty", "bing");
            }
        }

        [Fact]
        public void IsPropertyChanged_throws_for_a_navigation_property_name()
        {
            using (var context = new F1Context())
            {
                var hamilton = context.Drivers.Where(d => d.Name == "Lewis Hamilton").Single();
                var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(hamilton);

                Assert.Throws<ArgumentException>(() => stateEntry.IsPropertyChanged("Team")).ValidateMessage(
                    "ObjectStateEntry_SetModifiedOnInvalidProperty", "Team");
            }
        }

        [Fact]
        public void IsPropertyChanged_throws_for_a_detached_entity()
        {
            using (var context = new F1Context())
            {
                var hamilton = context.Drivers.Where(d => d.Name == "Lewis Hamilton").Single();
                var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(hamilton);
                context.Entry(hamilton).State = EntityState.Detached;

                Assert.Throws<InvalidOperationException>(() => stateEntry.IsPropertyChanged("Name")).ValidateMessage(
                    "ObjectStateEntry_InvalidState");
            }
        }

        [Fact]
        public void IsPropertyChanged_throws_for_an_added_entity()
        {
            using (var context = new F1Context())
            {
                var hamilton = context.Drivers.Where(d => d.Name == "Lewis Hamilton").Single();
                var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(hamilton);
                context.Entry(hamilton).State = EntityState.Added;

                Assert.Throws<InvalidOperationException>(() => stateEntry.IsPropertyChanged("Name")).ValidateMessage(
                    "ObjectStateEntry_SetModifiedStates");
            }
        }

        [Fact]
        public void IsPropertyChanged_throws_for_a_deleted_entity()
        {
            using (var context = new F1Context())
            {
                var hamilton = context.Drivers.Where(d => d.Name == "Lewis Hamilton").Single();
                var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(hamilton);
                context.Entry(hamilton).State = EntityState.Deleted;

                Assert.Throws<InvalidOperationException>(() => stateEntry.IsPropertyChanged("Name")).ValidateMessage(
                    "ObjectStateEntry_SetModifiedStates");
            }
        }

        [Fact]
        public void IsPropertyChanged_throws_for_a_stub_entry()
        {
            using (var context = new SimpleModelForLinq())
            {
                context.Orders.Load();
                var stateEntry = GetObjectContext(context)
                    .ObjectStateManager
                    .GetObjectStateEntries(~EntityState.Detached)
                    .Where(s => s.Entity == null && !s.IsRelationship)
                    .First();

                Assert.Throws<InvalidOperationException>(() => stateEntry.IsPropertyChanged("Name")).ValidateMessage(
                    "ObjectStateEntry_CannotModifyKeyEntryState");
            }
        }

        [Fact]
        public void IsPropertyChanged_throws_for_a_relationship_entry()
        {
            using (var context = new SimpleModelForLinq())
            {
                context.Orders.Load();
                var stateEntry = GetObjectContext(context)
                    .ObjectStateManager
                    .GetObjectStateEntries(~EntityState.Detached)
                    .Where(s => s.IsRelationship)
                    .First();

                Assert.Throws<InvalidOperationException>(() => stateEntry.IsPropertyChanged("Name")).ValidateMessage(
                    "ObjectStateEntry_CantModifyRelationState");
            }
        }

        [Fact]
        public void SetModifiedProperty_should_not_change_the_state_of_the_entity_if_a_bad_property_name_is_used()
        {
            using (var context = new F1Context())
            {
                var hamilton = context.Drivers.Where(d => d.Name == "Lewis Hamilton").Single();
                var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(hamilton);

                Assert.Throws<ArgumentException>(() => stateEntry.SetModifiedProperty("bing")).ValidateMessage(
                    "ObjectStateEntry_SetModifiedOnInvalidProperty", "bing");

                Assert.Equal(EntityState.Unchanged, stateEntry.State);
            }
        }

        [Fact]
        public void SetModifiedProperty_should_not_change_the_state_of_the_entity_if_a_null_property_name_is_used()
        {
            using (var context = new F1Context())
            {
                var hamilton = context.Drivers.Where(d => d.Name == "Lewis Hamilton").Single();
                var stateEntry = GetObjectContext(context).ObjectStateManager.GetObjectStateEntry(hamilton);

                Assert.Throws<ArgumentException>(() => stateEntry.SetModifiedProperty(null))
                    .ValidateMessage("ArgumentIsNullOrWhitespace", "propertyName");

                Assert.Equal(EntityState.Unchanged, stateEntry.State);
            }
        }

        #endregion

        #region Tests for property access including detached entities and properties that are not in the model

        private void TestScalarCurrentValue(
            DbEntityEntry entityEntry, DbPropertyEntry<Building, string> propertyEntry,
            DbPropertyValues currentValues, Func<string> getter, string initialValue)
        {
            var initialState = entityEntry.State;

            Assert.Equal(initialState == EntityState.Modified, propertyEntry.IsModified);
            Assert.Equal(initialValue, propertyEntry.CurrentValue);

            // Set to same value; prop should not get marked as modified
            propertyEntry.CurrentValue = initialValue;
            Assert.Equal(initialState == EntityState.Modified, propertyEntry.IsModified);
            Assert.Equal(initialState, entityEntry.State);

            // Set to new value; prop marked as modified
            propertyEntry.CurrentValue = "New Value";
            Assert.Equal("New Value", propertyEntry.CurrentValue);
            Assert.Equal("New Value", getter());
            CheckPropertyIsModified(entityEntry, propertyEntry, initialState);

            // New value reflected in record
            if (initialState != EntityState.Deleted
                && initialState != EntityState.Detached)
            {
                Assert.Equal("New Value", currentValues[propertyEntry.Name]);

                // Change record; new value reflected in entry
                currentValues[propertyEntry.Name] = "Another Value";
                Assert.Equal("Another Value", propertyEntry.CurrentValue);
                Assert.Equal("Another Value", getter());
            }

            // Set to null
            propertyEntry.CurrentValue = null;
            Assert.Null(propertyEntry.CurrentValue);
            Assert.Null(getter());
        }

        private void TestScalarOriginalValue(
            DbEntityEntry entityEntry, DbPropertyEntry<Building, string> propertyEntry,
            Type objectType, DbPropertyValues originalValues, string initialValue)
        {
            var initialState = entityEntry.State;

            if (initialState == EntityState.Added)
            {
                Assert.Throws<InvalidOperationException>(() => { var _ = propertyEntry.OriginalValue; }).ValidateMessage
                    ("DbPropertyValues_CannotGetValuesForState", "OriginalValues", "Added");
                Assert.Throws<InvalidOperationException>(() => propertyEntry.OriginalValue = "").ValidateMessage(
                    "DbPropertyValues_CannotGetValuesForState", "OriginalValues", "Added");
                return;
            }

            if (initialState == EntityState.Detached)
            {
                Assert.Throws<InvalidOperationException>(() => { var _ = propertyEntry.OriginalValue; }).ValidateMessage
                    (
                        "DbPropertyEntry_NotSupportedForDetached", "OriginalValue", propertyEntry.Name,
                        entityEntry.Entity.GetType().Name);
                Assert.Throws<InvalidOperationException>(() => propertyEntry.OriginalValue = "").ValidateMessage(
                    "DbPropertyEntry_NotSupportedForDetached", "OriginalValue", propertyEntry.Name,
                    entityEntry.Entity.GetType().Name);
                return;
            }

            Assert.Equal(initialState == EntityState.Modified, propertyEntry.IsModified);
            Assert.Equal(initialValue, propertyEntry.OriginalValue);

            // Set to same value; prop should not get marked as modified
            propertyEntry.OriginalValue = initialValue;
            Assert.Equal(initialState == EntityState.Modified, propertyEntry.IsModified);
            Assert.Equal(initialState, entityEntry.State);

            // Set to new value; prop marked as modified
            propertyEntry.OriginalValue = "New Value";
            Assert.Equal("New Value", propertyEntry.OriginalValue);
            CheckPropertyIsModified(entityEntry, propertyEntry, initialState);

            // New value reflected in record
            Assert.Equal("New Value", originalValues[propertyEntry.Name]);

            // Change record; new value reflected in entry
            originalValues[propertyEntry.Name] = "Another Value";
            Assert.Equal("Another Value", propertyEntry.OriginalValue);

            // Set to null
            propertyEntry.OriginalValue = null;
            Assert.Equal(null, propertyEntry.OriginalValue);
        }

        private void CheckPropertyIsModified(
            DbEntityEntry entityEntry, DbPropertyEntry propertyEntry,
            EntityState initialState)
        {
            if (initialState == EntityState.Modified
                || initialState == EntityState.Unchanged)
            {
                Assert.True(propertyEntry.IsModified);
                Assert.Equal(EntityState.Modified, entityEntry.State);
            }
            else
            {
                Assert.False(propertyEntry.IsModified);
                Assert.Equal(initialState, entityEntry.State);
            }
        }

        private void TestComplexOriginalValue(
            DbEntityEntry<Building> entityEntry,
            DbPropertyEntry<Building, Address> propertyEntry)
        {
            var initialState = entityEntry.State;

            if (initialState == EntityState.Added)
            {
                Assert.Throws<InvalidOperationException>(() => { var _ = propertyEntry.OriginalValue; }).ValidateMessage
                    ("DbPropertyValues_CannotGetValuesForState", "OriginalValues", "Added");
                Assert.Throws<InvalidOperationException>(() => propertyEntry.OriginalValue = new Address()).
                    ValidateMessage("DbPropertyValues_CannotGetValuesForState", "OriginalValues", "Added");
                return;
            }

            if (initialState == EntityState.Detached)
            {
                Assert.Throws<InvalidOperationException>(() => { var _ = propertyEntry.OriginalValue; }).ValidateMessage
                    (
                        "DbPropertyEntry_NotSupportedForDetached", "OriginalValue", propertyEntry.Name,
                        entityEntry.Entity.GetType().Name);
                Assert.Throws<InvalidOperationException>(() => propertyEntry.OriginalValue = new Address()).
                    ValidateMessage(
                        "DbPropertyEntry_NotSupportedForDetached", "OriginalValue", propertyEntry.Name,
                        entityEntry.Entity.GetType().Name);
                return;
            }

            Assert.Equal(initialState == EntityState.Modified, propertyEntry.IsModified);
            Assert.Equal("Redmond", propertyEntry.OriginalValue.City);
            Assert.Equal("Clean", propertyEntry.OriginalValue.SiteInfo.Environment);

            // Set to new object with the same values; should not get marked as modified because no values changing
            var sameAddress = CloneAddress(propertyEntry.OriginalValue);
            propertyEntry.OriginalValue = sameAddress;
            Assert.Equal("Redmond", propertyEntry.OriginalValue.City);
            Assert.Equal("Clean", propertyEntry.OriginalValue.SiteInfo.Environment);
            Assert.Equal(initialState == EntityState.Modified, propertyEntry.IsModified);
            Assert.Equal(initialState, entityEntry.State);

            // Set to new value; prop marked as modified
            var newAddress = new Address
                                 {
                                     Street = "300 Main St",
                                     City = "Ames",
                                     State = "IA",
                                     ZipCode = "50010",
                                     SiteInfo = new SiteInfo
                                                    {
                                                        Zone = 2,
                                                        Environment = "Contaminated"
                                                    }
                                 };

            propertyEntry.OriginalValue = newAddress;
            Assert.Equal("Ames", propertyEntry.OriginalValue.City);
            Assert.Equal("Contaminated", propertyEntry.OriginalValue.SiteInfo.Environment);
            CheckPropertyIsModified(entityEntry, propertyEntry, initialState);

            // New value reflected in record
            if (initialState != EntityState.Added
                && initialState != EntityState.Detached)
            {
                var addressValues = (DbPropertyValues)entityEntry.OriginalValues["Address"];
                var siteValues = (DbPropertyValues)addressValues["SiteInfo"];

                Assert.Equal("Ames", addressValues["City"]);
                Assert.Equal("Contaminated", siteValues["Environment"]);

                // Change record; new value reflected in entry
                addressValues["City"] = "Cedar Falls";
                siteValues["Environment"] = "Peachy";

                Assert.Equal("Cedar Falls", propertyEntry.OriginalValue.City);
                Assert.Equal("Peachy", propertyEntry.OriginalValue.SiteInfo.Environment);
            }

            // Set to null
            Assert.Throws<InvalidOperationException>(() => propertyEntry.OriginalValue = null).ValidateMessage(
                "DbPropertyValues_ComplexObjectCannotBeNull", "Address", "Building");

            // Set to new value that has a nested null complex object
            // Should always throw, but Originally only throws if the entity is Added/Modified/Unchanged
            if (initialState != EntityState.Detached
                && initialState != EntityState.Deleted)
            {
                var addressWithNull = new Address
                                          {
                                              Street = "300 Main St",
                                              City = "Ames",
                                              State = "IA",
                                              ZipCode = "50010",
                                              SiteInfo = null
                                          };
                Assert.Throws<InvalidOperationException>(() => propertyEntry.OriginalValue = addressWithNull).
                    ValidateMessage("DbPropertyValues_ComplexObjectCannotBeNull", "SiteInfo", "Address");
            }
        }

        private void TestComplexOriginalValue(
            DbEntityEntry<Building> entityEntry,
            DbPropertyEntry<Building, SiteInfo> propertyEntry)
        {
            var initialState = entityEntry.State;

            if (initialState == EntityState.Added)
            {
                Assert.Throws<InvalidOperationException>(() => { var _ = propertyEntry.OriginalValue; }).ValidateMessage
                    ("DbPropertyValues_CannotGetValuesForState", "OriginalValues", "Added");
                Assert.Throws<InvalidOperationException>(() => propertyEntry.OriginalValue = new SiteInfo()).
                    ValidateMessage("DbPropertyValues_CannotGetValuesForState", "OriginalValues", "Added");
                return;
            }

            if (initialState == EntityState.Detached)
            {
                Assert.Throws<InvalidOperationException>(() => { var _ = propertyEntry.OriginalValue; }).ValidateMessage
                    (
                        "DbPropertyEntry_NotSupportedForDetached", "OriginalValue", propertyEntry.Name,
                        typeof(Building).Name);
                Assert.Throws<InvalidOperationException>(() => propertyEntry.OriginalValue = new SiteInfo()).
                    ValidateMessage(
                        "DbPropertyEntry_NotSupportedForDetached", "OriginalValue", propertyEntry.Name,
                        typeof(Building).Name);
                return;
            }

            Assert.Equal(initialState == EntityState.Modified, propertyEntry.IsModified);
            Assert.Equal("Clean", propertyEntry.OriginalValue.Environment);

            // Set to new object with the same values; should not get marked as modified because no values changing
            var sameInfo = CloneSiteInfo(propertyEntry.OriginalValue);
            propertyEntry.OriginalValue = sameInfo;
            Assert.Equal("Clean", propertyEntry.OriginalValue.Environment);
            Assert.Equal(initialState == EntityState.Modified, propertyEntry.IsModified);
            Assert.Equal(initialState, entityEntry.State);

            // Set to new value; prop marked as modified
            var newInfo = new SiteInfo
                              {
                                  Zone = 2,
                                  Environment = "Contaminated"
                              };

            propertyEntry.OriginalValue = newInfo;
            Assert.Equal("Contaminated", propertyEntry.OriginalValue.Environment);
            CheckPropertyIsModified(entityEntry, propertyEntry, initialState);

            // New value reflected in record
            if (initialState != EntityState.Added
                && initialState != EntityState.Detached)
            {
                var siteValues =
                    entityEntry.OriginalValues.GetValue<DbPropertyValues>("Address").GetValue<DbPropertyValues>(
                        "SiteInfo");

                Assert.Equal("Contaminated", siteValues["Environment"]);

                // Change record; new value reflected in entry
                siteValues["Environment"] = "Peachy";

                Assert.Equal("Peachy", propertyEntry.OriginalValue.Environment);
            }

            // Set to null
            Assert.Throws<InvalidOperationException>(() => propertyEntry.OriginalValue = null).ValidateMessage(
                "DbPropertyValues_ComplexObjectCannotBeNull", "SiteInfo", "Address");
        }

        private Address CloneAddress(Address source)
        {
            return new Address
                       {
                           Street = source.Street,
                           City = source.City,
                           State = source.State,
                           ZipCode = source.ZipCode,
                           SiteInfo = CloneSiteInfo(source.SiteInfo)
                       };
        }

        private SiteInfo CloneSiteInfo(SiteInfo source)
        {
            return new SiteInfo
                       {
                           Zone = source.Zone,
                           Environment = source.Environment
                       };
        }

        private void TestComplexCurentValue(
            DbEntityEntry<Building> entityEntry,
            DbComplexPropertyEntry<Building, Address> propertyEntry,
            Func<Address> getter)
        {
            var initialState = entityEntry.State;

            // Get these nested entries at the beginning and check that they remain in sync
            var cityEntry = propertyEntry.Property(a => a.City);
            var environmentEntry = propertyEntry.ComplexProperty(a => a.SiteInfo).Property(i => i.Environment);

            Assert.Equal(initialState == EntityState.Modified, propertyEntry.IsModified);
            Assert.Equal("Redmond", propertyEntry.CurrentValue.City);
            Assert.Equal("Clean", propertyEntry.CurrentValue.SiteInfo.Environment);
            Assert.Equal("Redmond", cityEntry.CurrentValue);
            Assert.Equal("Clean", environmentEntry.CurrentValue);

            // Getting complex object should return actual object
            Assert.Same(getter(), propertyEntry.CurrentValue);

            // Set to same value; prop should not get marked as modified
            propertyEntry.CurrentValue = getter();
            Assert.Equal(initialState == EntityState.Modified, propertyEntry.IsModified);
            Assert.Equal(initialState, entityEntry.State);

            // Set to new object with the same values; should get marked as modified
            var sameAddress = CloneAddress(getter());
            propertyEntry.CurrentValue = sameAddress;
            Assert.Equal("Redmond", propertyEntry.CurrentValue.City);
            Assert.Equal("Clean", propertyEntry.CurrentValue.SiteInfo.Environment);
            Assert.Equal("Redmond", cityEntry.CurrentValue);
            Assert.Equal("Clean", environmentEntry.CurrentValue);
            CheckPropertyIsModified(entityEntry, (DbComplexPropertyEntry)propertyEntry, initialState);
            Assert.Same(sameAddress, getter());

            // Reset state
            if (initialState == EntityState.Unchanged)
            {
                entityEntry.State = EntityState.Unchanged;
            }

            // Set to new value; prop marked as modified
            var newAddress = new Address
                                 {
                                     Street = "300 Main St",
                                     City = "Ames",
                                     State = "IA",
                                     ZipCode = "50010",
                                     SiteInfo = new SiteInfo
                                                    {
                                                        Zone = 2,
                                                        Environment = "Contaminated"
                                                    }
                                 };

            propertyEntry.CurrentValue = newAddress;
            Assert.Equal("Ames", propertyEntry.CurrentValue.City);
            Assert.Equal("Contaminated", propertyEntry.CurrentValue.SiteInfo.Environment);
            Assert.Equal("Ames", cityEntry.CurrentValue);
            Assert.Equal("Contaminated", environmentEntry.CurrentValue);
            CheckPropertyIsModified(entityEntry, (DbComplexPropertyEntry)propertyEntry, initialState);
            Assert.Same(newAddress, getter());

            // New value reflected in record
            if (initialState != EntityState.Deleted
                && initialState != EntityState.Detached)
            {
                var addressValues = (DbPropertyValues)entityEntry.CurrentValues["Address"];
                var siteValues = (DbPropertyValues)addressValues["SiteInfo"];

                Assert.Equal("Ames", addressValues["City"]);
                Assert.Equal("Contaminated", siteValues["Environment"]);

                // Change record; new value reflected in entry
                addressValues["City"] = "Cedar Falls";
                siteValues["Environment"] = "Peachy";

                Assert.Equal("Cedar Falls", propertyEntry.CurrentValue.City);
                Assert.Equal("Peachy", propertyEntry.CurrentValue.SiteInfo.Environment);
            }

            // Set to null
            Assert.Throws<InvalidOperationException>(() => propertyEntry.CurrentValue = null).ValidateMessage(
                "DbPropertyValues_ComplexObjectCannotBeNull", "Address", "Building");

            // Set to new value that has a nested null complex object
            // Should always throw, but currently only throws if the entity is Added/Modified/Unchanged
            if (initialState != EntityState.Detached
                && initialState != EntityState.Deleted)
            {
                var addressWithNull = new Address
                                          {
                                              Street = "300 Main St",
                                              City = "Ames",
                                              State = "IA",
                                              ZipCode = "50010",
                                              SiteInfo = null
                                          };
                Assert.Throws<InvalidOperationException>(() => propertyEntry.CurrentValue = addressWithNull).
                    ValidateMessage("DbPropertyValues_ComplexObjectCannotBeNull", "SiteInfo", "Address");
            }
        }

        private void TestComplexCurentValue(
            DbEntityEntry<Building> entityEntry,
            DbComplexPropertyEntry<Building, SiteInfo> propertyEntry,
            Func<SiteInfo> getter)
        {
            var initialState = entityEntry.State;

            // Get this nested entry at the beginning and check that they remain in sync
            var environmentEntry = propertyEntry.Property(i => i.Environment);

            Assert.Equal(initialState == EntityState.Modified, propertyEntry.IsModified);
            Assert.Equal("Clean", propertyEntry.CurrentValue.Environment);
            Assert.Equal("Clean", environmentEntry.CurrentValue);

            // Getting complex object should return actual object
            Assert.Same(getter(), propertyEntry.CurrentValue);

            // Set to same value; prop should not get marked as modified
            propertyEntry.CurrentValue = getter();
            Assert.Equal(initialState == EntityState.Modified, propertyEntry.IsModified);
            Assert.Equal(initialState, entityEntry.State);

            // Set to new object with the same values; should get marked as modified
            var sameInfo = CloneSiteInfo(getter());
            propertyEntry.CurrentValue = sameInfo;
            Assert.Equal("Clean", propertyEntry.CurrentValue.Environment);
            Assert.Equal("Clean", environmentEntry.CurrentValue);
            CheckPropertyIsModified(entityEntry, (DbComplexPropertyEntry)propertyEntry, initialState);
            Assert.Same(sameInfo, getter());

            // Reset state
            if (initialState == EntityState.Unchanged)
            {
                entityEntry.State = EntityState.Unchanged;
            }

            // Set to new value; prop marked as modified
            var newInfo = new SiteInfo
                              {
                                  Zone = 2,
                                  Environment = "Contaminated"
                              };

            propertyEntry.CurrentValue = newInfo;
            Assert.Equal("Contaminated", propertyEntry.CurrentValue.Environment);
            Assert.Equal("Contaminated", environmentEntry.CurrentValue);
            CheckPropertyIsModified(entityEntry, (DbComplexPropertyEntry)propertyEntry, initialState);
            Assert.Same(newInfo, getter());

            // New value reflected in record
            if (initialState != EntityState.Deleted
                && initialState != EntityState.Detached)
            {
                var siteValues =
                    entityEntry.CurrentValues.GetValue<DbPropertyValues>("Address").GetValue<DbPropertyValues>(
                        "SiteInfo");

                Assert.Equal("Contaminated", siteValues["Environment"]);

                // Change record; new value reflected in entry
                siteValues["Environment"] = "Peachy";

                Assert.Equal("Peachy", propertyEntry.CurrentValue.Environment);
            }

            // Set to null
            Assert.Throws<InvalidOperationException>(() => propertyEntry.CurrentValue = null).ValidateMessage(
                "DbPropertyValues_ComplexObjectCannotBeNull", "SiteInfo", "Address");
        }

        private void TestIsModified(DbEntityEntry entityEntry, DbPropertyEntry propertyEntry)
        {
            var initialState = entityEntry.State;

            if (initialState == EntityState.Detached)
            {
                Assert.False(propertyEntry.IsModified);
                Assert.Throws<InvalidOperationException>(() => propertyEntry.IsModified = true).ValidateMessage(
                    "DbPropertyEntry_NotSupportedForDetached", "IsModified", propertyEntry.Name,
                    entityEntry.Entity.GetType().Name);
                return;
            }

            if (initialState == EntityState.Added
                || initialState == EntityState.Deleted)
            {
                Assert.False(propertyEntry.IsModified);
                Assert.Throws<InvalidOperationException>(() => propertyEntry.IsModified = true).ValidateMessage(
                    "ObjectStateEntry_SetModifiedStates");
                return;
            }

            propertyEntry.IsModified = true;
            Assert.True(propertyEntry.IsModified);
        }

        private void TestCurrentValueNotInModel(
            DbEntityEntry entityEntry, DbPropertyEntry propertyEntry,
            Type objectType, Func<string> getter, string initialValue,
            bool hasGetter, bool hasSetter)
        {
            Assert.False(propertyEntry.IsModified);
            if (hasGetter)
            {
                Assert.Equal(initialValue, propertyEntry.CurrentValue);
            }
            else
            {
                Assert.Throws<InvalidOperationException>(() => { var _ = propertyEntry.CurrentValue; }).ValidateMessage(
                    "DbPropertyEntry_CannotGetCurrentValue", propertyEntry.Name, objectType.Name);
            }

            // Set to new value; prop still not modified because it is not in the model
            if (hasSetter)
            {
                propertyEntry.CurrentValue = "New Value";
                if (hasGetter)
                {
                    Assert.Equal("New Value", propertyEntry.CurrentValue);
                }
                Assert.Equal("New Value", getter());

                // Set to null
                propertyEntry.CurrentValue = null;
                Assert.Null(getter());
            }
            else
            {
                Assert.Throws<InvalidOperationException>(() => { propertyEntry.CurrentValue = ""; }).ValidateMessage(
                    "DbPropertyEntry_CannotSetCurrentValue", propertyEntry.Name, objectType.Name);
            }
            Assert.False(propertyEntry.IsModified);
        }

        private void TestOriginalValueNotInModel(
            DbEntityEntry entityEntry,
            DbPropertyEntry<Building, string> propertyEntry)
        {
            Assert.Throws<InvalidOperationException>(() => { var _ = propertyEntry.OriginalValue; }).ValidateMessage(
                "DbPropertyEntry_NotSupportedForPropertiesNotInTheModel", "OriginalValue", propertyEntry.Name,
                entityEntry.Entity.GetType().Name);
            Assert.Throws<InvalidOperationException>(() => propertyEntry.OriginalValue = "").ValidateMessage(
                "DbPropertyEntry_NotSupportedForPropertiesNotInTheModel", "OriginalValue", propertyEntry.Name,
                entityEntry.Entity.GetType().Name);
        }

        private void TestIsModifiedNotInModel(DbEntityEntry entityEntry, DbPropertyEntry<Building, string> propertyEntry)
        {
            Assert.False(propertyEntry.IsModified);
            Assert.Throws<InvalidOperationException>(() => propertyEntry.IsModified = true).ValidateMessage(
                "DbPropertyEntry_NotSupportedForPropertiesNotInTheModel", "IsModified", propertyEntry.Name,
                entityEntry.Entity.GetType().Name);
        }

        private void DbPropertyEntryTest(EntityState state, Action<DbEntityEntry<Building>> test)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entry = context.Entry(
                    new Building
                        {
                            Name = "Building 18",
                            Address = new Address
                                          {
                                              Street = "1 Microsoft Way",
                                              City = "Redmond",
                                              State = "WA",
                                              ZipCode = "98052",
                                              County = "KING",
                                              SiteInfo = new SiteInfo
                                                             {
                                                                 Zone = 2,
                                                                 Environment = "Clean"
                                                             }
                                          },
                            NotInModel = "NotInModel",
                        });

                entry.State = state;

                test(entry);
            }
        }

        #region Added

        [Fact]
        public void Scalar_CurrentValue_on_DbPropertyEntry_from_an_added_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                TestScalarCurrentValue(
                    e, e.Property(b => b.Name), e.CurrentValues, () => e.Entity.Name,
                    "Building 18"));
        }

        [Fact]
        public void Scalar_OriginalValue_on_DbPropertyEntry_from_an_added_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                TestScalarOriginalValue(
                    e, e.Property(b => b.Name), typeof(Building), null,
                    "Building 18"));
        }

        [Fact]
        public void Complex_CurrentValue_on_DbPropertyEntry_from_an_added_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                TestComplexCurentValue(e, e.ComplexProperty(b => b.Address), () => e.Entity.Address));
        }

        [Fact]
        public void Complex_OriginalValue_on_DbPropertyEntry_from_an_added_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(EntityState.Added, e => TestComplexOriginalValue(e, e.ComplexProperty(b => b.Address)));
        }

        [Fact]
        public void IsModified_on_DbPropertyEntry_from_an_added_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(EntityState.Added, e => TestIsModified(e, e.Property(b => b.Name)));
        }

        [Fact]
        public void CurrentValue_on_DbPropertyEntry_from_an_added_entity_for_a_property_not_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property(b => b.NotInModel), typeof(Building),
                    () => e.Entity.NotInModel, "NotInModel", hasSetter: true,
                    hasGetter: true));
        }

        [Fact]
        public void OriginalValue_on_DbPropertyEntry_from_an_added_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(EntityState.Added, e => TestOriginalValueNotInModel(e, e.Property(b => b.NotInModel)));
        }

        [Fact]
        public void IsModified_on_DbPropertyEntry_from_an_added_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(EntityState.Added, e => TestIsModifiedNotInModel(e, e.Property(b => b.NotInModel)));
        }

        [Fact]
        public void
            CurrentValue_on_DbPropertyEntry_from_an_added_entity_for_a_property_not_in_the_EDM_without_a_getter_can_be_used_to_write_only()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property("NoGetter"), typeof(Building),
                    () => e.Entity.GetNoGetterValue(), "NotInModel",
                    hasSetter: true, hasGetter: false));
        }

        [Fact]
        public void
            CurrentValue_on_DbPropertyEntry_from_an_added_entity_for_a_property_not_in_the_EDM_without_a_setter_can_be_used_to_read_only()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property(b => b.NoSetter), typeof(Building),
                    () => e.Entity.NoSetter, "NoSetter", hasSetter: false,
                    hasGetter: true));
        }

        [Fact]
        public void
            DbPropertyEntry_from_an_added_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                Assert.Throws<ArgumentException>(() => e.Property("BadProperty")).ValidateMessage(
                    "DbEntityEntry_NotAScalarProperty", "BadProperty", "Building"));
        }

        [Fact]
        public void DbComplexPropertyEntry_from_an_added_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                Assert.Throws<ArgumentException>(() => e.ComplexProperty("BadProperty")).ValidateMessage
                    ("DbEntityEntry_NotAComplexProperty", "BadProperty", "Building"));
        }

        [Fact]
        public void
            Nested_scalar_CurrentValue_on_DbPropertyEntry_from_an_added_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                TestScalarCurrentValue(
                    e, e.ComplexProperty(b => b.Address).Property(a => a.City),
                    e.CurrentValues.GetValue<DbPropertyValues>("Address"),
                    () => e.Entity.Address.City, "Redmond"));
        }

        [Fact]
        public void
            Nested_scalar_OriginalValue_on_DbPropertyEntry_from_an_added_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                TestScalarOriginalValue(
                    e, e.Property(b => b.Address.City), typeof(Address), null,
                    "Redmond"));
        }

        [Fact]
        public void
            Nested_complex_CurrentValue_on_DbPropertyEntry_from_an_added_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                TestComplexCurentValue(
                    e,
                    e.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo),
                    () => e.Entity.Address.SiteInfo));
        }

        [Fact]
        public void
            Nested_complex_OriginalValue_on_DbPropertyEntry_from_an_added_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e => TestComplexOriginalValue(e, e.ComplexProperty(b => b.Address.SiteInfo)));
        }

        [Fact]
        public void Double_nested_scalar_CurrentValue_on_DbPropertyEntry_from_an_added_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                TestScalarCurrentValue(
                    e,
                    e.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo)
                    .Property(s => s.Environment),
                    e.CurrentValues.GetValue<DbPropertyValues>("Address").GetValue
                    <DbPropertyValues>("SiteInfo"),
                    () => e.Entity.Address.SiteInfo.Environment, "Clean"));
        }

        [Fact]
        public void Double_nested_scalar_OriginalValue_on_DbPropertyEntry_from_an_added_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                TestScalarOriginalValue(
                    e, e.Property(b => b.Address.SiteInfo.Environment),
                    typeof(SiteInfo), null, "Clean"));
        }

        [Fact]
        public void IsModified_on_nested_DbPropertyEntry_from_an_added_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e => TestIsModified(e, e.ComplexProperty(b => b.Address).Property(a => a.City)));
        }

        [Fact]
        public void
            IsModified_on_double_nested_DbPropertyEntry_from_an_added_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                TestIsModified(
                    e,
                    e.ComplexProperty(b => b.Address).Property(a => a.SiteInfo.Environment)));
        }

        [Fact]
        public void
            CurrentValue_on_nested_DbPropertyEntry_from_an_added_entity_for_a_property_not_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property(b => b.Address.County), typeof(Address),
                    () => e.Entity.Address.County, "KING", hasSetter: true,
                    hasGetter: true));
        }

        [Fact]
        public void
            OriginalValue_on_nested_DbPropertyEntry_from_an_added_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                TestOriginalValueNotInModel(e, e.ComplexProperty(b => b.Address).Property(b => b.County)));
        }

        [Fact]
        public void
            IsModified_on_nested_DbPropertyEntry_from_an_added_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                TestIsModifiedNotInModel(e, e.ComplexProperty(b => b.Address).Property(b => b.County)));
        }

        [Fact]
        public void
            CurrentValue_on_nested_DbPropertyEntry_from_an_added_entity_for_a_property_not_in_the_EDM_without_a_getter_can_be_used_to_write_only
            ()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property("Address.WriteOnly"), typeof(Address),
                    () => e.Entity.Address.GetWriteOnlyValue(), "WriteOnly",
                    hasSetter: true, hasGetter: false));
        }

        [Fact]
        public void
            CurrentValue_on_nested_DbPropertyEntry_from_an_added_entity_for_a_property_not_in_the_EDM_without_a_setter_can_be_used_to_read_only
            ()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                TestCurrentValueNotInModel(
                    e,
                    e.ComplexProperty(b => b.Address).Property(
                        b => b.FormattedAddress), typeof(Address),
                    () => e.Entity.NoSetter, "1 Microsoft Way, Redmond, WA 98052",
                    hasSetter: false, hasGetter: true));
        }

        [Fact]
        public void Nested_DbPropertyEntry_from_an_added_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                Assert.Throws<ArgumentException>(
                    () => e.ComplexProperty(b => b.Address).Property("BadProperty")).ValidateMessage(
                        "DbEntityEntry_NotAScalarProperty", "BadProperty", "Address"));
        }

        [Fact]
        public void
            Nested_DbComplexPropertyEntry_from_an_added_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                Assert.Throws<ArgumentException>(
                    () => e.ComplexProperty(b => b.Address).ComplexProperty("BadProperty")).
                    ValidateMessage("DbEntityEntry_NotAComplexProperty", "BadProperty", "Address"));
        }

        [Fact]
        public void Nested_DbComplexPropertyEntry_from_an_added_entity_for_a_scalar_property_in_the_EDM_and_in_the_CLR_class_can_not_be_used
            ()
        {
            DbPropertyEntryTest(
                EntityState.Added,
                e =>
                Assert.Throws<ArgumentException>(
                    () => e.ComplexProperty(b => b.Address).ComplexProperty("City")).ValidateMessage(
                        "DbEntityEntry_NotAComplexProperty", "City", "Address"));
        }

        #endregion

        #region Unchanged

        [Fact]
        public void
            Scalar_CurrentValue_on_DbPropertyEntry_from_an_unchanged_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e =>
                TestScalarCurrentValue(
                    e, e.Property(b => b.Name), e.CurrentValues, () => e.Entity.Name,
                    "Building 18"));
        }

        [Fact]
        public void
            Scalar_OriginalValue_on_DbPropertyEntry_from_an_unchanged_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e =>
                TestScalarOriginalValue(
                    e, e.Property(b => b.Name), typeof(Building), e.OriginalValues,
                    "Building 18"));
        }

        [Fact]
        public void
            Complex_CurrentValue_on_DbPropertyEntry_from_an_unchanged_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e =>
                TestComplexCurentValue(e, e.ComplexProperty(b => b.Address), () => e.Entity.Address));
        }

        [Fact]
        public void
            Complex_OriginalValue_on_DbPropertyEntry_from_an_unchanged_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e => TestComplexOriginalValue(e, e.ComplexProperty(b => b.Address)));
        }

        [Fact]
        public void IsModified_on_DbPropertyEntry_from_an_unchanged_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(EntityState.Unchanged, e => TestIsModified(e, e.Property(b => b.Name)));
        }

        [Fact]
        public void CurrentValue_on_DbPropertyEntry_from_an_unchanged_entity_for_a_property_not_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property(b => b.NotInModel), typeof(Building),
                    () => e.Entity.NotInModel, "NotInModel", hasSetter: true,
                    hasGetter: true));
        }

        [Fact]
        public void
            OriginalValue_on_DbPropertyEntry_from_an_unchanged_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e => TestOriginalValueNotInModel(e, e.Property(b => b.NotInModel)));
        }

        [Fact]
        public void IsModified_on_DbPropertyEntry_from_an_unchanged_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(EntityState.Unchanged, e => TestIsModifiedNotInModel(e, e.Property(b => b.NotInModel)));
        }

        [Fact]
        public void
            CurrentValue_on_DbPropertyEntry_from_an_unchanged_entity_for_a_property_not_in_the_EDM_without_a_getter_can_be_used_to_write_only
            ()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property("NoGetter"), typeof(Building),
                    () => e.Entity.GetNoGetterValue(), "NotInModel",
                    hasSetter: true, hasGetter: false));
        }

        [Fact]
        public void
            CurrentValue_on_DbPropertyEntry_from_an_unchanged_entity_for_a_property_not_in_the_EDM_without_a_setter_can_be_used_to_read_only
            ()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property(b => b.NoSetter), typeof(Building),
                    () => e.Entity.NoSetter, "NoSetter", hasSetter: false,
                    hasGetter: true));
        }

        [Fact]
        public void DbPropertyEntry_from_an_unchanged_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e =>
                Assert.Throws<ArgumentException>(() => e.Property("BadProperty")).ValidateMessage(
                    "DbEntityEntry_NotAScalarProperty", "BadProperty", "Building"));
        }

        [Fact]
        public void DbComplexPropertyEntry_from_an_unchanged_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e =>
                Assert.Throws<ArgumentException>(() => e.ComplexProperty("BadProperty")).ValidateMessage
                    ("DbEntityEntry_NotAComplexProperty", "BadProperty", "Building"));
        }

        [Fact]
        public void Nested_scalar_CurrentValue_on_DbPropertyEntry_from_an_unchanged_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged, e => TestScalarCurrentValue(
                    e, e.Property(b => b.Address.City),
                    e.CurrentValues.GetValue
                                                <DbPropertyValues>("Address"),
                    () => e.Entity.Address.City,
                    "Redmond"));
        }

        [Fact]
        public void Nested_scalar_OriginalValue_on_DbPropertyEntry_from_an_unchanged_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e => TestScalarOriginalValue(
                    e, e.Property(b => b.Address.City), typeof(Address),
                    e.OriginalValues.GetValue<DbPropertyValues>("Address"),
                    "Redmond"));
        }

        [Fact]
        public void Nested_complex_CurrentValue_on_DbPropertyEntry_from_an_unchanged_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e => TestComplexCurentValue(
                    e, e.ComplexProperty(b => b.Address.SiteInfo),
                    () => e.Entity.Address.SiteInfo));
        }

        [Fact]
        public void Nested_complex_OriginalValue_on_DbPropertyEntry_from_an_unchanged_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e => TestComplexOriginalValue(e, e.ComplexProperty(b => b.Address.SiteInfo)));
        }

        [Fact]
        public void Double_nested_scalar_CurrentValue_on_DbPropertyEntry_from_an_unchanged_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e => TestScalarCurrentValue(
                    e, e.Property(b => b.Address.SiteInfo.Environment),
                    e.CurrentValues.GetValue<DbPropertyValues>("Address").
                         GetValue<DbPropertyValues>("SiteInfo"),
                    () => e.Entity.Address.SiteInfo.Environment, "Clean"));
        }

        [Fact]
        public void Double_nested_scalar_OriginalValue_on_DbPropertyEntry_from_an_unchanged_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e =>
                TestScalarOriginalValue(
                    e, e.Property(b => b.Address.SiteInfo.Environment),
                    typeof(SiteInfo),
                    e.OriginalValues.GetValue<DbPropertyValues>("Address").GetValue
                    <DbPropertyValues>("SiteInfo"), "Clean"));
        }

        [Fact]
        public void IsModified_on_nested_DbPropertyEntry_from_an_unchanged_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(EntityState.Unchanged, e => TestIsModified(e, e.Property(b => b.Address.City)));
        }

        [Fact]
        public void
            IsModified_on_double_nested_DbPropertyEntry_from_an_unchanged_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e => TestIsModified(e, e.Property(b => b.Address.SiteInfo.Environment)));
        }

        [Fact]
        public void
            CurrentValue_on_nested_DbPropertyEntry_from_an_unchanged_entity_for_a_property_not_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property(b => b.Address.County), typeof(Address),
                    () => e.Entity.Address.County, "KING", hasSetter: true,
                    hasGetter: true));
        }

        [Fact]
        public void OriginalValue_on_nested_DbPropertyEntry_from_an_unchanged_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e => TestOriginalValueNotInModel(e, e.Property(b => b.Address.County)));
        }

        [Fact]
        public void
            IsModified_on_nested_DbPropertyEntry_from_an_unchanged_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e => TestIsModifiedNotInModel(e, e.Property(b => b.Address.County)));
        }

        [Fact]
        public void
            CurrentValue_on_nested_DbPropertyEntry_from_an_unchanged_entity_for_a_property_not_in_the_EDM_without_a_getter_can_be_used_to_write_only
            ()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property("Address.WriteOnly"), typeof(Address),
                    () => e.Entity.Address.GetWriteOnlyValue(), "WriteOnly",
                    hasSetter: true, hasGetter: false));
        }

        [Fact]
        public void
            CurrentValue_on_nested_DbPropertyEntry_from_an_unchanged_entity_for_a_property_not_in_the_EDM_without_a_setter_can_be_used_to_read_only
            ()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property(b => b.Address.FormattedAddress),
                    typeof(Address), () => e.Entity.NoSetter,
                    "1 Microsoft Way, Redmond, WA 98052", hasSetter: false,
                    hasGetter: true));
        }

        [Fact]
        public void Nested_DbPropertyEntry_from_an_unchanged_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e =>
                Assert.Throws<ArgumentException>(() => e.Property<int>("Address.BadProperty")).
                    ValidateMessage("DbEntityEntry_NotAScalarProperty", "BadProperty", "Address"));
        }

        [Fact]
        public void
            Nested_DbComplexPropertyEntry_from_an_unchanged_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e =>
                Assert.Throws<ArgumentException>(() => e.ComplexProperty("Address.BadProperty")).
                    ValidateMessage("DbEntityEntry_NotAComplexProperty", "BadProperty", "Address"));
        }

        [Fact]
        public void
            Nested_DbComplexPropertyEntry_from_an_unchanged_entity_for_a_scalar_property_in_the_EDM_and_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Unchanged,
                e =>
                Assert.Throws<ArgumentException>(() => e.ComplexProperty(b => b.Address.City)).
                    ValidateMessage("DbEntityEntry_NotAComplexProperty", "City", "Address"));
        }

        #endregion

        #region Modified

        [Fact]
        public void Scalar_CurrentValue_on_DbPropertyEntry_from_a_modified_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                TestScalarCurrentValue(
                    e, e.Property(b => b.Name), e.CurrentValues, () => e.Entity.Name,
                    "Building 18"));
        }

        [Fact]
        public void Scalar_OriginalValue_on_DbPropertyEntry_from_a_modified_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                TestScalarOriginalValue(
                    e, e.Property(b => b.Name), typeof(Building), e.OriginalValues,
                    "Building 18"));
        }

        [Fact]
        public void Complex_CurrentValue_on_DbPropertyEntry_from_a_modified_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                TestComplexCurentValue(e, e.ComplexProperty(b => b.Address), () => e.Entity.Address));
        }

        [Fact]
        public void
            Complex_OriginalValue_on_DbPropertyEntry_from_a_modified_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e => TestComplexOriginalValue(e, e.ComplexProperty(b => b.Address)));
        }

        [Fact]
        public void IsModified_on_DbPropertyEntry_from_a_modified_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(EntityState.Modified, e => TestIsModified(e, e.Property(b => b.Name)));
        }

        [Fact]
        public void CurrentValue_on_DbPropertyEntry_from_a_modified_entity_for_a_property_not_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property(b => b.NotInModel), typeof(Building),
                    () => e.Entity.NotInModel, "NotInModel", hasSetter: true,
                    hasGetter: true));
        }

        [Fact]
        public void OriginalValue_on_DbPropertyEntry_from_a_modified_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(EntityState.Modified, e => TestOriginalValueNotInModel(e, e.Property(b => b.NotInModel)));
        }

        [Fact]
        public void IsModified_on_DbPropertyEntry_from_a_modified_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(EntityState.Modified, e => TestIsModifiedNotInModel(e, e.Property(b => b.NotInModel)));
        }

        [Fact]
        public void
            CurrentValue_on_DbPropertyEntry_from_a_modified_entity_for_a_property_not_in_the_EDM_without_a_getter_can_be_used_to_write_only(
            
            )
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property("NoGetter"), typeof(Building),
                    () => e.Entity.GetNoGetterValue(), "NotInModel",
                    hasSetter: true, hasGetter: false));
        }

        [Fact]
        public void
            CurrentValue_on_DbPropertyEntry_from_a_modified_entity_for_a_property_not_in_the_EDM_without_a_setter_can_be_used_to_read_only()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property(b => b.NoSetter), typeof(Building),
                    () => e.Entity.NoSetter, "NoSetter", hasSetter: false,
                    hasGetter: true));
        }

        [Fact]
        public void DbPropertyEntry_from_a_modified_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                Assert.Throws<ArgumentException>(() => e.Property("BadProperty")).ValidateMessage(
                    "DbEntityEntry_NotAScalarProperty", "BadProperty", "Building"));
        }

        [Fact]
        public void DbComplexPropertyEntry_from_a_modified_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                Assert.Throws<ArgumentException>(() => e.ComplexProperty("BadProperty")).ValidateMessage
                    ("DbEntityEntry_NotAComplexProperty", "BadProperty", "Building"));
        }

        [Fact]
        public void
            Nested_scalar_CurrentValue_on_DbPropertyEntry_from_a_modified_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e => TestScalarCurrentValue(
                    e, e.ComplexProperty(b => b.Address).Property(a => a.City),
                    e.CurrentValues.GetValue<DbPropertyValues>("Address"),
                    () => e.Entity.Address.City, "Redmond"));
        }

        [Fact]
        public void
            Nested_scalar_OriginalValue_on_DbPropertyEntry_from_a_modified_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                TestScalarOriginalValue(
                    e, e.ComplexProperty(b => b.Address).Property(a => a.City),
                    typeof(Address),
                    e.OriginalValues.GetValue<DbPropertyValues>("Address"),
                    "Redmond"));
        }

        [Fact]
        public void
            Nested_complex_CurrentValue_on_DbPropertyEntry_from_a_modified_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                TestComplexCurentValue(
                    e,
                    e.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo),
                    () => e.Entity.Address.SiteInfo));
        }

        [Fact]
        public void Nested_complex_OriginalValue_on_DbPropertyEntry_from_a_modified_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                TestComplexOriginalValue(
                    e,
                    e.ComplexProperty(b => b.Address).ComplexProperty(
                        a => a.SiteInfo)));
        }

        [Fact]
        public void Double_nested_scalar_CurrentValue_on_DbPropertyEntry_from_a_modified_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                TestScalarCurrentValue(
                    e,
                    e.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo)
                    .Property(s => s.Environment),
                    e.CurrentValues.GetValue<DbPropertyValues>("Address").GetValue
                    <DbPropertyValues>("SiteInfo"),
                    () => e.Entity.Address.SiteInfo.Environment, "Clean"));
        }

        [Fact]
        public void Double_nested_scalar_OriginalValue_on_DbPropertyEntry_from_a_modified_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                TestScalarOriginalValue(
                    e,
                    e.ComplexProperty(b => b.Address).ComplexProperty(
                        a => a.SiteInfo).Property(s => s.Environment),
                    typeof(SiteInfo),
                    e.OriginalValues.GetValue<DbPropertyValues>("Address").GetValue
                    <DbPropertyValues>("SiteInfo"), "Clean"));
        }

        [Fact]
        public void IsModified_on_nested_DbPropertyEntry_from_a_modified_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e => TestIsModified(e, e.ComplexProperty(b => b.Address).Property(a => a.City)));
        }

        [Fact]
        public void
            IsModified_on_double_nested_DbPropertyEntry_from_a_modified_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                TestIsModified(
                    e,
                    e.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo).
                    Property(s => s.Environment)));
        }

        [Fact]
        public void
            CurrentValue_on_nested_DbPropertyEntry_from_a_modified_entity_for_a_property_not_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                TestCurrentValueNotInModel(
                    e, e.ComplexProperty(b => b.Address).Property(a => a.County),
                    typeof(Address), () => e.Entity.Address.County, "KING",
                    hasSetter: true, hasGetter: true));
        }

        [Fact]
        public void
            OriginalValue_on_nested_DbPropertyEntry_from_a_modified_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                TestOriginalValueNotInModel(e, e.ComplexProperty(b => b.Address).Property(b => b.County)));
        }

        [Fact]
        public void
            IsModified_on_nested_DbPropertyEntry_from_a_modified_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                TestIsModifiedNotInModel(e, e.ComplexProperty(b => b.Address).Property(b => b.County)));
        }

        [Fact]
        public void
            CurrentValue_on_nested_DbPropertyEntry_from_a_modified_entity_for_a_property_not_in_the_EDM_without_a_getter_can_be_used_to_write_only
            ()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                TestCurrentValueNotInModel(
                    e, e.ComplexProperty(b => b.Address).Property("WriteOnly"),
                    typeof(Address), () => e.Entity.Address.GetWriteOnlyValue(),
                    "WriteOnly", hasSetter: true, hasGetter: false));
        }

        [Fact]
        public void
            CurrentValue_on_nested_DbPropertyEntry_from_a_modified_entity_for_a_property_not_in_the_EDM_without_a_setter_can_be_used_to_read_only
            ()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                TestCurrentValueNotInModel(
                    e,
                    e.ComplexProperty(b => b.Address).Property(
                        b => b.FormattedAddress), typeof(Address),
                    () => e.Entity.NoSetter, "1 Microsoft Way, Redmond, WA 98052",
                    hasSetter: false, hasGetter: true));
        }

        [Fact]
        public void Nested_DbPropertyEntry_from_a_modified_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                Assert.Throws<ArgumentException>(
                    () => e.ComplexProperty(b => b.Address).Property<object>("BadProperty")).
                    ValidateMessage("DbEntityEntry_NotAScalarProperty", "BadProperty", "Address"));
        }

        [Fact]
        public void
            Nested_DbComplexPropertyEntry_from_a_modified_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                Assert.Throws<ArgumentException>(
                    () => e.ComplexProperty(b => b.Address).ComplexProperty("BadProperty")).
                    ValidateMessage("DbEntityEntry_NotAComplexProperty", "BadProperty", "Address"));
        }

        [Fact]
        public void
            Nested_DbComplexPropertyEntry_from_a_modified_entity_for_a_scalar_property_in_the_EDM_and_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Modified,
                e =>
                Assert.Throws<ArgumentException>(
                    () => e.ComplexProperty(b => b.Address).ComplexProperty("City")).ValidateMessage(
                        "DbEntityEntry_NotAComplexProperty", "City", "Address"));
        }

        #endregion

        #region Deleted

        [Fact]
        public void Scalar_CurrentValue_on_DbPropertyEntry_from_a_deleted_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                TestScalarCurrentValue(
                    e, e.Property(b => b.Name), null, () => e.Entity.Name,
                    "Building 18"));
        }

        [Fact]
        public void Scalar_OriginalValue_on_DbPropertyEntry_from_a_deleted_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                TestScalarOriginalValue(
                    e, e.Property(b => b.Name), typeof(Building), e.OriginalValues,
                    "Building 18"));
        }

        [Fact]
        public void Complex_CurrentValue_on_DbPropertyEntry_from_a_deleted_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                TestComplexCurentValue(e, e.ComplexProperty(b => b.Address), () => e.Entity.Address));
        }

        [Fact]
        public void Complex_OriginalValue_on_DbPropertyEntry_from_a_deleted_entity_for_a_property_in_the_EDM_can_be_used
            ()
        {
            DbPropertyEntryTest(EntityState.Deleted, e => TestComplexOriginalValue(e, e.ComplexProperty(b => b.Address)));
        }

        [Fact]
        public void IsModified_on_DbPropertyEntry_from_a_deleted_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(EntityState.Deleted, e => TestIsModified(e, e.Property(b => b.Name)));
        }

        [Fact]
        public void CurrentValue_on_DbPropertyEntry_from_a_deleted_entity_for_a_property_not_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property(b => b.NotInModel), typeof(Building),
                    () => e.Entity.NotInModel, "NotInModel", hasSetter: true,
                    hasGetter: true));
        }

        [Fact]
        public void OriginalValue_on_DbPropertyEntry_from_a_deleted_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(EntityState.Deleted, e => TestOriginalValueNotInModel(e, e.Property(b => b.NotInModel)));
        }

        [Fact]
        public void IsModified_on_DbPropertyEntry_from_a_deleted_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(EntityState.Deleted, e => TestIsModifiedNotInModel(e, e.Property(b => b.NotInModel)));
        }

        [Fact]
        public void
            CurrentValue_on_DbPropertyEntry_from_a_deleted_entity_for_a_property_not_in_the_EDM_without_a_getter_can_be_used_to_write_only
            ()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property("NoGetter"), typeof(Building),
                    () => e.Entity.GetNoGetterValue(), "NotInModel",
                    hasSetter: true, hasGetter: false));
        }

        [Fact]
        public void
            CurrentValue_on_DbPropertyEntry_from_a_deleted_entity_for_a_property_not_in_the_EDM_without_a_setter_can_be_used_to_read_only
            ()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property(b => b.NoSetter), typeof(Building),
                    () => e.Entity.NoSetter, "NoSetter", hasSetter: false,
                    hasGetter: true));
        }

        [Fact]
        public void
            DbPropertyEntry_from_a_deleted_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used
            ()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                Assert.Throws<ArgumentException>(() => e.Property("BadProperty")).ValidateMessage(
                    "DbEntityEntry_NotAScalarProperty", "BadProperty", "Building"));
        }

        [Fact]
        public void
            DbComplexPropertyEntry_from_a_deleted_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used
            ()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                Assert.Throws<ArgumentException>(() => e.ComplexProperty("BadProperty")).ValidateMessage
                    ("DbEntityEntry_NotAComplexProperty", "BadProperty", "Building"));
        }

        [Fact]
        public void
            Nested_scalar_CurrentValue_on_DbPropertyEntry_from_a_deleted_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                TestScalarCurrentValue(
                    e, e.ComplexProperty(b => b.Address).Property(a => a.City), null,
                    () => e.Entity.Address.City, "Redmond"));
        }

        [Fact]
        public void
            Nested_scalar_OriginalValue_on_DbPropertyEntry_from_a_deleted_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                TestScalarOriginalValue(
                    e, e.ComplexProperty(b => b.Address).Property(a => a.City),
                    typeof(Address),
                    e.OriginalValues.GetValue<DbPropertyValues>("Address"),
                    "Redmond"));
        }

        [Fact]
        public void
            Nested_complex_CurrentValue_on_DbPropertyEntry_from_a_deleted_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                TestComplexCurentValue(
                    e,
                    e.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo),
                    () => e.Entity.Address.SiteInfo));
        }

        [Fact]
        public void
            Nested_complex_OriginalValue_on_DbPropertyEntry_from_a_deleted_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                TestComplexOriginalValue(
                    e,
                    e.ComplexProperty(b => b.Address).ComplexProperty(
                        a => a.SiteInfo)));
        }

        [Fact]
        public void
            Double_nested_scalar_CurrentValue_on_DbPropertyEntry_from_a_deleted_entity_for_a_property_in_the_EDM_can_be_used
            ()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                TestScalarCurrentValue(
                    e,
                    e.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo)
                    .Property(s => s.Environment), null,
                    () => e.Entity.Address.SiteInfo.Environment, "Clean"));
        }

        [Fact]
        public void
            Double_nested_scalar_OriginalValue_on_DbPropertyEntry_from_a_deleted_entity_for_a_property_in_the_EDM_can_be_used
            ()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                TestScalarOriginalValue(
                    e,
                    e.ComplexProperty(b => b.Address).ComplexProperty(
                        a => a.SiteInfo).Property(s => s.Environment),
                    typeof(SiteInfo),
                    e.OriginalValues.GetValue<DbPropertyValues>("Address").GetValue
                    <DbPropertyValues>("SiteInfo"), "Clean"));
        }

        [Fact]
        public void IsModified_on_nested_DbPropertyEntry_from_a_deleted_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e => TestIsModified(e, e.ComplexProperty(b => b.Address).Property(a => a.City)));
        }

        [Fact]
        public void
            IsModified_on_double_nested_DbPropertyEntry_from_a_deleted_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                TestIsModified(
                    e,
                    e.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo).
                    Property(s => s.Environment)));
        }

        [Fact]
        public void
            CurrentValue_on_nested_DbPropertyEntry_from_a_deleted_entity_for_a_property_not_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                TestCurrentValueNotInModel(
                    e, e.ComplexProperty(b => b.Address).Property(a => a.County),
                    typeof(Address), () => e.Entity.Address.County, "KING",
                    hasSetter: true, hasGetter: true));
        }

        [Fact]
        public void
            OriginalValue_on_nested_DbPropertyEntry_from_a_deleted_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                TestOriginalValueNotInModel(e, e.ComplexProperty(b => b.Address).Property(b => b.County)));
        }

        [Fact]
        public void
            IsModified_on_nested_DbPropertyEntry_from_a_deleted_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                TestIsModifiedNotInModel(e, e.ComplexProperty(b => b.Address).Property(b => b.County)));
        }

        [Fact]
        public void
            CurrentValue_on_nested_DbPropertyEntry_from_a_deleted_entity_for_a_property_not_in_the_EDM_without_a_getter_can_be_used_to_write_only
            ()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                TestCurrentValueNotInModel(
                    e, e.ComplexProperty(b => b.Address).Property("WriteOnly"),
                    typeof(Address), () => e.Entity.Address.GetWriteOnlyValue(),
                    "WriteOnly", hasSetter: true, hasGetter: false));
        }

        [Fact]
        public void
            CurrentValue_on_nested_DbPropertyEntry_from_a_deleted_entity_for_a_property_not_in_the_EDM_without_a_setter_can_be_used_to_read_only
            ()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                TestCurrentValueNotInModel(
                    e,
                    e.ComplexProperty(b => b.Address).Property(
                        b => b.FormattedAddress), typeof(Address),
                    () => e.Entity.NoSetter, "1 Microsoft Way, Redmond, WA 98052",
                    hasSetter: false, hasGetter: true));
        }

        [Fact]
        public void
            Nested_DbPropertyEntry_from_a_deleted_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used
            ()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                Assert.Throws<ArgumentException>(
                    () => e.ComplexProperty(b => b.Address).Property("BadProperty")).ValidateMessage(
                        "DbEntityEntry_NotAScalarProperty", "BadProperty", "Address"));
        }

        [Fact]
        public void
            Nested_DbComplexPropertyEntry_from_a_deleted_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used
            ()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                Assert.Throws<ArgumentException>(
                    () => e.ComplexProperty(b => b.Address).ComplexProperty("BadProperty")).
                    ValidateMessage("DbEntityEntry_NotAComplexProperty", "BadProperty", "Address"));
        }

        [Fact]
        public void
            Nested_DbComplexPropertyEntry_from_a_deleted_entity_for_a_scalar_property_in_the_EDM_and_in_the_CLR_class_can_not_be_used
            ()
        {
            DbPropertyEntryTest(
                EntityState.Deleted,
                e =>
                Assert.Throws<ArgumentException>(
                    () => e.ComplexProperty(b => b.Address).ComplexProperty("City")).ValidateMessage(
                        "DbEntityEntry_NotAComplexProperty", "City", "Address"));
        }

        #endregion

        #region Detached

        [Fact]
        public void Scalar_CurrentValue_on_DbPropertyEntry_from_a_detached_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                TestScalarCurrentValue(
                    e, e.Property(b => b.Name), null, () => e.Entity.Name,
                    "Building 18"));
        }

        [Fact]
        public void Scalar_OriginalValue_on_DbPropertyEntry_from_a_detached_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                TestScalarOriginalValue(
                    e, e.Property(b => b.Name), typeof(Building), null,
                    "Building 18"));
        }

        [Fact]
        public void Complex_CurrentValue_on_DbPropertyEntry_from_a_detached_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                TestComplexCurentValue(e, e.ComplexProperty(b => b.Address), () => e.Entity.Address));
        }

        [Fact]
        public void
            Complex_OriginalValue_on_DbPropertyEntry_from_a_detached_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e => TestComplexOriginalValue(e, e.ComplexProperty(b => b.Address)));
        }

        [Fact]
        public void IsModified_on_DbPropertyEntry_from_a_detached_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(EntityState.Detached, e => TestIsModified(e, e.Property(b => b.Name)));
        }

        [Fact]
        public void CurrentValue_on_DbPropertyEntry_from_a_detached_entity_for_a_property_not_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property(b => b.NotInModel), typeof(Building),
                    () => e.Entity.NotInModel, "NotInModel", hasSetter: true,
                    hasGetter: true));
        }

        [Fact]
        public void OriginalValue_on_DbPropertyEntry_from_a_detached_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(EntityState.Detached, e => TestOriginalValueNotInModel(e, e.Property(b => b.NotInModel)));
        }

        [Fact]
        public void IsModified_on_DbPropertyEntry_from_a_detached_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(EntityState.Detached, e => TestIsModifiedNotInModel(e, e.Property(b => b.NotInModel)));
        }

        [Fact]
        public void
            CurrentValue_on_DbPropertyEntry_from_a_detached_entity_for_a_property_not_in_the_EDM_without_a_getter_can_be_used_to_write_only(
            
            )
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property("NoGetter"), typeof(Building),
                    () => e.Entity.GetNoGetterValue(), "NotInModel",
                    hasSetter: true, hasGetter: false));
        }

        [Fact]
        public void
            CurrentValue_on_DbPropertyEntry_from_a_detached_entity_for_a_property_not_in_the_EDM_without_a_setter_can_be_used_to_read_only()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                TestCurrentValueNotInModel(
                    e, e.Property(b => b.NoSetter), typeof(Building),
                    () => e.Entity.NoSetter, "NoSetter", hasSetter: false,
                    hasGetter: true));
        }

        [Fact]
        public void DbPropertyEntry_from_a_detached_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                Assert.Throws<ArgumentException>(() => e.Property("BadProperty")).ValidateMessage(
                    "DbEntityEntry_NotAScalarProperty", "BadProperty", "Building"));
        }

        [Fact]
        public void DbComplexPropertyEntry_from_a_detached_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                Assert.Throws<ArgumentException>(() => e.ComplexProperty("BadProperty")).ValidateMessage
                    ("DbEntityEntry_NotAComplexProperty", "BadProperty", "Building"));
        }

        [Fact]
        public void
            Nested_scalar_CurrentValue_on_DbPropertyEntry_from_a_detached_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                TestScalarCurrentValue(
                    e, e.ComplexProperty(b => b.Address).Property(a => a.City), null,
                    () => e.Entity.Address.City, "Redmond"));
        }

        [Fact]
        public void
            Nested_scalar_OriginalValue_on_DbPropertyEntry_from_a_detached_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                TestScalarOriginalValue(
                    e, e.ComplexProperty(b => b.Address).Property(a => a.City),
                    typeof(Address), null, "Redmond"));
        }

        [Fact]
        public void
            Nested_complex_CurrentValue_on_DbPropertyEntry_from_a_detached_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                TestComplexCurentValue(
                    e,
                    e.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo),
                    () => e.Entity.Address.SiteInfo));
        }

        [Fact]
        public void Nested_complex_OriginalValue_on_DbPropertyEntry_from_a_detached_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                TestComplexOriginalValue(
                    e,
                    e.ComplexProperty(b => b.Address).ComplexProperty(
                        a => a.SiteInfo)));
        }

        [Fact]
        public void Double_nested_scalar_CurrentValue_on_DbPropertyEntry_from_a_detached_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                TestScalarCurrentValue(
                    e,
                    e.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo)
                    .Property(s => s.Environment), null,
                    () => e.Entity.Address.SiteInfo.Environment, "Clean"));
        }

        [Fact]
        public void Double_nested_scalar_OriginalValue_on_DbPropertyEntry_from_a_detached_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                TestScalarOriginalValue(
                    e,
                    e.ComplexProperty(b => b.Address).ComplexProperty(
                        a => a.SiteInfo).Property(s => s.Environment),
                    typeof(SiteInfo), null, "Clean"));
        }

        [Fact]
        public void IsModified_on_nested_DbPropertyEntry_from_a_detached_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e => TestIsModified(e, e.ComplexProperty(b => b.Address).Property(a => a.City)));
        }

        [Fact]
        public void
            IsModified_on_double_nested_DbPropertyEntry_from_a_detached_entity_for_a_property_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                TestIsModified(
                    e,
                    e.ComplexProperty(b => b.Address).ComplexProperty(a => a.SiteInfo).
                    Property(s => s.Environment)));
        }

        [Fact]
        public void
            CurrentValue_on_nested_DbPropertyEntry_from_a_detached_entity_for_a_property_not_in_the_EDM_can_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                TestCurrentValueNotInModel(
                    e, e.ComplexProperty(b => b.Address).Property(a => a.County),
                    typeof(Address), () => e.Entity.Address.County, "KING",
                    hasSetter: true, hasGetter: true));
        }

        [Fact]
        public void
            OriginalValue_on_nested_DbPropertyEntry_from_a_detached_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                TestOriginalValueNotInModel(e, e.ComplexProperty(b => b.Address).Property(b => b.County)));
        }

        [Fact]
        public void
            IsModified_on_nested_DbPropertyEntry_from_a_detached_entity_for_a_property_not_in_the_EDM_cannot_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                TestIsModifiedNotInModel(e, e.ComplexProperty(b => b.Address).Property(b => b.County)));
        }

        [Fact]
        public void
            CurrentValue_on_nested_DbPropertyEntry_from_a_detached_entity_for_a_property_not_in_the_EDM_without_a_getter_can_be_used_to_write_only
            ()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                TestCurrentValueNotInModel(
                    e, e.ComplexProperty(b => b.Address).Property("WriteOnly"),
                    typeof(Address), () => e.Entity.Address.GetWriteOnlyValue(),
                    "WriteOnly", hasSetter: true, hasGetter: false));
        }

        [Fact]
        public void
            CurrentValue_on_nested_DbPropertyEntry_from_a_detached_entity_for_a_property_not_in_the_EDM_without_a_setter_can_be_used_to_read_only
            ()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                TestCurrentValueNotInModel(
                    e,
                    e.ComplexProperty(b => b.Address).Property(
                        b => b.FormattedAddress), typeof(Address),
                    () => e.Entity.NoSetter, "1 Microsoft Way, Redmond, WA 98052",
                    hasSetter: false, hasGetter: true));
        }

        [Fact]
        public void Nested_DbPropertyEntry_from_a_detached_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                Assert.Throws<ArgumentException>(
                    () => e.ComplexProperty(b => b.Address).Property("BadProperty")).ValidateMessage(
                        "DbEntityEntry_NotAScalarProperty", "BadProperty", "Address"));
        }

        [Fact]
        public void
            Nested_DbComplexPropertyEntry_from_a_detached_entity_for_a_property_not_in_the_EDM_and_not_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                Assert.Throws<ArgumentException>(
                    () => e.ComplexProperty(b => b.Address).ComplexProperty("BadProperty")).
                    ValidateMessage("DbEntityEntry_NotAComplexProperty", "BadProperty", "Address"));
        }

        [Fact]
        public void
            Nested_DbComplexPropertyEntry_from_a_detached_entity_for_a_scalar_property_in_the_EDM_and_in_the_CLR_class_can_not_be_used()
        {
            DbPropertyEntryTest(
                EntityState.Detached,
                e =>
                Assert.Throws<ArgumentException>(
                    () => e.ComplexProperty(b => b.Address).ComplexProperty("City")).ValidateMessage(
                        "DbEntityEntry_NotAComplexProperty", "City", "Address"));
        }

        #endregion

        #endregion

        #region Tests for getting Name for collection and reference nav properties

        [Fact]
        public void Name_can_be_obtained_for_exposed_reference_nav_property()
        {
            Name_can_be_obtained_for_exposed_reference_nav_property_implementation(detached: false);
        }

        [Fact]
        public void Name_can_be_obtained_for_exposed_reference_nav_property_on_detatched_entity()
        {
            Name_can_be_obtained_for_exposed_reference_nav_property_implementation(detached: true);
        }

        private void Name_can_be_obtained_for_exposed_reference_nav_property_implementation(bool detached)
        {
            using (var context = new F1Context())
            {
                var teamEntry = context.Entry(new Team());
                if (!detached)
                {
                    teamEntry.State = EntityState.Added;
                }

                Assert.Equal("Chassis", teamEntry.Reference(t => t.Chassis).Name);
                Assert.Equal("Chassis", teamEntry.Reference("Chassis").Name);
                Assert.Equal("Chassis", teamEntry.Reference<Chassis>("Chassis").Name);
            }
        }

        [Fact]
        public void Name_can_be_obtained_for_exposed_collection_nav_property()
        {
            Name_can_be_obtained_for_exposed_collection_nav_property_implementation(detached: false);
        }

        [Fact]
        public void Name_can_be_obtained_for_exposed_collection_nav_property_on_detached_entity()
        {
            Name_can_be_obtained_for_exposed_collection_nav_property_implementation(detached: false);
        }

        private void Name_can_be_obtained_for_exposed_collection_nav_property_implementation(bool detached)
        {
            using (var context = new F1Context())
            {
                var teamEntry = context.Entry(new Team());
                if (!detached)
                {
                    teamEntry.State = EntityState.Added;
                }

                Assert.Equal("Drivers", teamEntry.Collection(t => t.Drivers).Name);
                Assert.Equal("Drivers", teamEntry.Collection("Drivers").Name);
                Assert.Equal("Drivers", teamEntry.Collection<Driver>("Drivers").Name);
            }
        }

        #endregion

        #region Tests for access to current values from references and collections

        [Fact]
        public void
            Current_reference_value_for_one_to_one_dependent_can_be_read_and_set_for_an_object_in_the_Deleted_state()
        {
            Current_reference_value_for_one_to_one_dependent_can_be_read_and_set(EntityState.Deleted);
        }

        [Fact]
        public void
            Current_reference_value_for_one_to_one_dependent_can_be_read_and_set_for_an_object_in_the_Unchanged_state()
        {
            Current_reference_value_for_one_to_one_dependent_can_be_read_and_set(EntityState.Unchanged);
        }

        [Fact]
        public void
            Current_reference_value_for_one_to_one_dependent_can_be_read_and_set_for_an_object_in_the_Modified_state()
        {
            Current_reference_value_for_one_to_one_dependent_can_be_read_and_set(EntityState.Modified);
        }

        [Fact]
        public void
            Current_reference_value_for_one_to_one_dependent_can_be_read_and_set_for_an_object_in_the_Added_state()
        {
            Current_reference_value_for_one_to_one_dependent_can_be_read_and_set(EntityState.Added);
        }

        [Fact]
        public void
            Current_reference_value_for_one_to_one_dependent_can_be_read_and_set_for_an_object_in_the_Detached_state()
        {
            Current_reference_value_for_one_to_one_dependent_can_be_read_and_set(EntityState.Detached);
        }

        private void Current_reference_value_for_one_to_one_dependent_can_be_read_and_set(EntityState state)
        {
            using (var context = new F1Context())
            {
                var teamEntry = GetTeamEntry(context);
                teamEntry.State = state;
                var refEntry = teamEntry.Reference(t => t.Chassis);

                var value = refEntry.CurrentValue;
                Assert.Same(teamEntry.Entity.Chassis, value);

                value = new Chassis();
                refEntry.CurrentValue = value;
                Assert.Same(teamEntry.Entity.Chassis, value);

                if (state == EntityState.Deleted)
                {
                    Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges()).
                        ValidateMessage("RelatedEnd_UnableToAddRelationshipWithDeletedEntity");
                }
                else
                {
                    context.ChangeTracker.DetectChanges();
                }

                value = refEntry.CurrentValue;
                Assert.Same(teamEntry.Entity.Chassis, value);

                refEntry.CurrentValue = null;
                Assert.Null(refEntry.CurrentValue);
                Assert.Null(teamEntry.Entity.Chassis);
                context.ChangeTracker.DetectChanges();
            }
        }

        [Fact]
        public void
            Current_reference_value_for_one_to_one_principal_can_be_read_and_set_for_an_object_in_the_Deleted_state()
        {
            Current_reference_value_for_one_to_one_principal_can_be_read_and_set(EntityState.Deleted);
        }

        [Fact]
        public void
            Current_reference_value_for_one_to_one_principal_can_be_read_and_set_for_an_object_in_the_Unchanged_state()
        {
            Current_reference_value_for_one_to_one_principal_can_be_read_and_set(EntityState.Unchanged);
        }

        [Fact]
        public void
            Current_reference_value_for_one_to_one_principal_can_be_read_and_set_for_an_object_in_the_Modified_state()
        {
            Current_reference_value_for_one_to_one_principal_can_be_read_and_set(EntityState.Modified);
        }

        [Fact]
        public void
            Current_reference_value_for_one_to_one_principal_can_be_read_and_set_for_an_object_in_the_Added_state()
        {
            Current_reference_value_for_one_to_one_principal_can_be_read_and_set(EntityState.Added);
        }

        [Fact]
        public void
            Current_reference_value_for_one_to_one_principal_can_be_read_and_set_for_an_object_in_the_Detached_state()
        {
            Current_reference_value_for_one_to_one_principal_can_be_read_and_set(EntityState.Detached);
        }

        private void Current_reference_value_for_one_to_one_principal_can_be_read_and_set(EntityState state)
        {
            using (var context = new F1Context())
            {
                var chassisEntry = context.Entry(GetTeamEntry(context).Entity.Chassis);
                chassisEntry.State = state;
                var refEntry = chassisEntry.Reference(c => c.Team);

                var value = refEntry.CurrentValue;
                Assert.Same(chassisEntry.Entity.Team, value);

                if (state == EntityState.Unchanged
                    || state == EntityState.Modified)
                {
                    // Changing the reference to the principal will cause EF to throw a referential integrity exception
                    // because it would need a change in the PK of the dependent.
                    Assert.Throws<InvalidOperationException>(() => refEntry.CurrentValue = new Team()).ValidateMessage(
                        "EntityReference_CannotChangeReferentialConstraintProperty");
                }
                else
                {
                    value = new Team();
                    refEntry.CurrentValue = value;
                    Assert.Same(chassisEntry.Entity.Team, value);

                    if (state == EntityState.Deleted)
                    {
                        Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges()).
                            ValidateMessage("RelatedEnd_UnableToAddRelationshipWithDeletedEntity");
                    }
                    else
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    value = refEntry.CurrentValue;
                    Assert.Same(chassisEntry.Entity.Team, value);
                }

                refEntry.CurrentValue = null;
                Assert.Null(refEntry.CurrentValue);
                Assert.Null(chassisEntry.Entity.Team);
                context.ChangeTracker.DetectChanges();
            }
        }

        [Fact]
        public void
            Current_reference_value_for_one_to_many_principal_can_be_read_and_set_for_an_object_in_the_Deleted_state()
        {
            Current_reference_value_for_one_to_many_principal_can_be_read_and_set(EntityState.Deleted);
        }

        [Fact]
        public void
            Current_reference_value_for_one_to_many_principal_can_be_read_and_set_for_an_object_in_the_Unchanged_state()
        {
            Current_reference_value_for_one_to_many_principal_can_be_read_and_set(EntityState.Unchanged);
        }

        [Fact]
        public void
            Current_reference_value_for_one_to_many_principal_can_be_read_and_set_for_an_object_in_the_Modified_state()
        {
            Current_reference_value_for_one_to_many_principal_can_be_read_and_set(EntityState.Modified);
        }

        [Fact]
        public void
            Current_reference_value_for_one_to_many_principal_can_be_read_and_set_for_an_object_in_the_Added_state()
        {
            Current_reference_value_for_one_to_many_principal_can_be_read_and_set(EntityState.Added);
        }

        [Fact]
        public void
            Current_reference_value_for_one_to_many_principal_can_be_read_and_set_for_an_object_in_the_Detached_state()
        {
            Current_reference_value_for_one_to_many_principal_can_be_read_and_set(EntityState.Detached);
        }

        private void Current_reference_value_for_one_to_many_principal_can_be_read_and_set(EntityState state)
        {
            using (var context = new F1Context())
            {
                var teamEntry = GetTeamEntry(context);
                teamEntry.State = state;
                var refEntry = teamEntry.Reference(t => t.Gearbox);

                var value = refEntry.CurrentValue;
                Assert.Same(teamEntry.Entity.Gearbox, value);

                value = new Gearbox
                            {
                                Id = -7
                            };
                refEntry.CurrentValue = value;
                Assert.Same(teamEntry.Entity.Gearbox, value);

                if (state != EntityState.Detached
                    && state != EntityState.Deleted)
                {
                    // FK is fixed up without a call to DetectChanges
                    Assert.Equal(teamEntry.Entity.GearboxId, value.Id);
                }

                if (state == EntityState.Deleted)
                {
                    Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges()).
                        ValidateMessage("RelatedEnd_UnableToAddRelationshipWithDeletedEntity");
                }
                else
                {
                    context.ChangeTracker.DetectChanges();
                }

                value = refEntry.CurrentValue;
                Assert.Same(teamEntry.Entity.Gearbox, value);

                refEntry.CurrentValue = null;
                Assert.Null(refEntry.CurrentValue);
                Assert.Null(teamEntry.Entity.Gearbox);
                if (state != EntityState.Detached)
                {
                    // FK is fixed up without a call to DetectChanges
                    // For Deleted state the FK should already have been null
                    Assert.Null(teamEntry.Entity.GearboxId);
                }
                context.ChangeTracker.DetectChanges();
            }
        }

        [Fact]
        public void
            Setting_current_value_of_reference_nav_prop_to_null_nulls_the_FK_even_if_the_relationship_is_not_loaded()
        {
            using (var context = new SimpleModelContext())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var product = context.Products.Find(1);
                Assert.NotNull(product.CategoryId);
                Assert.Null(product.Category);

                context.Entry(product).Reference(p => p.Category).CurrentValue = null;

                Assert.Null(product.CategoryId);
                Assert.Null(product.Category);

                context.Entry(product).Reference(p => p.Category).Load();

                Assert.Null(product.CategoryId);
                Assert.Null(product.Category);
            }
        }

        [Fact]
        public void DbContext_switches_UseConsistentNullReferenceBehavior_on_by_default()
        {
            using (var context = new F1Context())
            {
                Assert.True(
                    ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseConsistentNullReferenceBehavior);
            }
        }

        [Fact]
        public void Lazy_loading_no_longer_happens_after_setting_reference_to_null_for_FK_relationship()
        {
            using (var context = new F1Context())
            {
                var driver = context.Drivers.First();
                context.Entry(driver).Reference(p => p.Team).CurrentValue = null;

                Assert.Null(driver.Team); // Accessing the reference does not cause it to be loaded
            }
        }

        [Fact]
        public void
            Setting_current_value_of_reference_nav_prop_to_null_does_nothing_for_an_FK_relationship_if_the_relationship_is_not_loaded_and_legacy_behavior_is_set_on_ObjectContext
            ()
        {
            using (var context = new SimpleModelContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseConsistentNullReferenceBehavior = false;

                var product = context.Products.Find(1);
                Assert.NotNull(product.CategoryId);
                Assert.Null(product.Category);

                context.Entry(product).Reference(p => p.Category).CurrentValue = null;

                Assert.NotNull(product.CategoryId);
                Assert.Null(product.Category);
            }
        }

        [Fact]
        public void Lazy_loading_happens_after_setting_reference_to_null_for_FK_relationship_when_legacy_behavior_is_set_on_ObjectContext()
        {
            using (var context = new F1Context())
            {
                ((IObjectContextAdapter)context).ObjectContext.ContextOptions.UseConsistentNullReferenceBehavior = false;

                var driver = context.Drivers.First();
                context.Entry(driver).Reference(p => p.Team).CurrentValue = null;

                Assert.NotNull(driver.Team); // Reference is lazy loaded
            }
        }

        [Fact]
        public void
            Setting_current_value_of_reference_nav_prop_to_null_for_an_independent_association_clears_the_relationship_even_if_it_is_not_loaded
            ()
        {
            using (var context = new F1Context())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var team = context.Teams.Find(Team.McLaren);
                Assert.Null(team.Engine);
                Assert.Equal(
                    0, ((IObjectContextAdapter)context).ObjectContext
                        .ObjectStateManager
                        .GetObjectStateEntries(EntityState.Deleted)
                        .Where(e => e.IsRelationship)
                        .Count());

                context.Entry(team).Reference(p => p.Engine).CurrentValue = null;

                Assert.Null(team.Engine);
                Assert.Equal(
                    1, ((IObjectContextAdapter)context).ObjectContext
                        .ObjectStateManager
                        .GetObjectStateEntries(EntityState.Deleted)
                        .Where(e => e.IsRelationship)
                        .Count());
            }
        }

        [Fact]
        public void Current_reference_value_for_independent_one_to_many_principal_can_be_read_and_set_for_an_object_in_the_Deleted_state()
        {
            Current_reference_value_for_independent_one_to_many_principal_can_be_read_and_set(EntityState.Deleted);
        }

        [Fact]
        public void Current_reference_value_for_independent_one_to_many_principal_can_be_read_and_set_for_an_object_in_the_Unchanged_state()
        {
            Current_reference_value_for_independent_one_to_many_principal_can_be_read_and_set(EntityState.Unchanged);
        }

        [Fact]
        public void Current_reference_value_for_independent_one_to_many_principal_can_be_read_and_set_for_an_object_in_the_Modified_state()
        {
            Current_reference_value_for_independent_one_to_many_principal_can_be_read_and_set(EntityState.Modified);
        }

        [Fact]
        public void Current_reference_value_for_independent_one_to_many_principal_can_be_read_and_set_for_an_object_in_the_Added_state()
        {
            Current_reference_value_for_independent_one_to_many_principal_can_be_read_and_set(EntityState.Added);
        }

        [Fact]
        public void Current_reference_value_for_independent_one_to_many_principal_can_be_read_and_set_for_an_object_in_the_Detached_state()
        {
            Current_reference_value_for_independent_one_to_many_principal_can_be_read_and_set(EntityState.Detached);
        }

        private void Current_reference_value_for_independent_one_to_many_principal_can_be_read_and_set(EntityState state)
        {
            using (var context = new F1Context())
            {
                var teamEntry = GetTeamEntry(context);
                teamEntry.State = state;
                var refEntry = teamEntry.Reference(t => t.Engine);

                var value = refEntry.CurrentValue;
                Assert.Same(teamEntry.Entity.Engine, value);

                value = new Engine();
                refEntry.CurrentValue = value;
                Assert.Same(teamEntry.Entity.Engine, value);

                if (state == EntityState.Deleted)
                {
                    Assert.Throws<InvalidOperationException>(() => context.ChangeTracker.DetectChanges()).
                        ValidateMessage("RelatedEnd_UnableToAddRelationshipWithDeletedEntity");
                }
                else
                {
                    context.ChangeTracker.DetectChanges();
                }

                value = refEntry.CurrentValue;
                Assert.Same(teamEntry.Entity.Engine, value);

                refEntry.CurrentValue = null;
                Assert.Null(refEntry.CurrentValue);
                Assert.Null(teamEntry.Entity.Engine);
                context.ChangeTracker.DetectChanges();
            }
        }

        [Fact]
        public void Current_collection_value_can_be_read_and_set_for_an_object_in_the_Deleted_state()
        {
            Current_collection_value_can_be_read_and_set(EntityState.Deleted);
        }

        [Fact]
        public void Current_collection_value_can_be_read_and_set_for_an_object_in_the_Unchanged_state()
        {
            Current_collection_value_can_be_read_and_set(EntityState.Unchanged);
        }

        [Fact]
        public void Current_collection_value_can_be_read_and_set_for_an_object_in_the_Modified_state()
        {
            Current_collection_value_can_be_read_and_set(EntityState.Modified);
        }

        [Fact]
        public void Current_collection_value_can_be_read_and_set_for_an_object_in_the_Added_state()
        {
            Current_collection_value_can_be_read_and_set(EntityState.Added);
        }

        [Fact]
        public void Current_collection_value_can_be_read_and_set_for_an_object_in_the_Detached_state()
        {
            Current_collection_value_can_be_read_and_set(EntityState.Detached);
        }

        private void Current_collection_value_can_be_read_and_set(EntityState state)
        {
            using (var context = new F1Context())
            {
                var engineEntry = context.Entry(GetTeamEntry(context).Entity.Engine);
                engineEntry.State = state;
                var collectionEntry = engineEntry.Collection(t => t.Gearboxes);

                var value = collectionEntry.CurrentValue;
                Assert.Same(engineEntry.Entity.Gearboxes, value);

                value = new List<Gearbox>();
                collectionEntry.CurrentValue = value;
                Assert.Same(engineEntry.Entity.Gearboxes, value);
                context.ChangeTracker.DetectChanges();

                value = collectionEntry.CurrentValue;
                Assert.Same(engineEntry.Entity.Gearboxes, value);

                collectionEntry.CurrentValue = null;
                Assert.Null(collectionEntry.CurrentValue);
                context.ChangeTracker.DetectChanges();
            }
        }

        [Fact]
        public void Current_value_for_collection_with_no_setter_can_be_read_but_throws_if_set()
        {
            using (var context = new F1Context())
            {
                var teamEntry = GetTeamEntry(context);
                var collectionEntry = teamEntry.Collection(t => t.Drivers);

                var value = collectionEntry.CurrentValue;

                Assert.Same(teamEntry.Entity.Drivers, value);

                Assert.Throws<NotSupportedException>(() => collectionEntry.CurrentValue = new List<Driver>()).
                    ValidateMessage("DbCollectionEntry_CannotSetCollectionProp", "Drivers", "ConcurrencyModel.Team");
            }
        }

        #endregion

        #region Tests for previously detached entity that then becomes attached

        [Fact]
        public void DbReferenceEntry_Query_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked()
        {
            DbReferenceEntry_method_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked(
                e => e.Query());
        }

        [Fact]
        public void DbReferenceEntry_IsLoaded_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked()
        {
            DbReferenceEntry_method_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked(
                e => { var _ = e.IsLoaded; });
        }

        [Fact]
        public void DbReferenceEntry_Load_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked()
        {
            DbReferenceEntry_method_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked(
                e => e.Load());
        }

        private void DbReferenceEntry_method_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked(
            Action<DbReferenceEntry> test)
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
                var entry = context.Entry(team).Reference(t => t.Chassis);

                context.Teams.Attach(team);

                test(entry); // Just testing here that this doesn't throw
            }
        }

        [Fact]
        public void DbCollectionEntry_Query_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked()
        {
            DbCollectionEntry_method_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked(
                e => e.Query());
        }

        [Fact]
        public void DbCollectionEntry_IsLoaded_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked()
        {
            DbCollectionEntry_method_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked(
                e => { var _ = e.IsLoaded; });
        }

        [Fact]
        public void DbCollectionEntry_Load_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked()
        {
            DbCollectionEntry_method_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked(
                e => e.Load());
        }

        private void DbCollectionEntry_method_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked(
            Action<DbCollectionEntry> test)
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
                var entry = context.Entry(team).Collection(t => t.Drivers);

                context.Teams.Attach(team);

                test(entry); // Just testing here that this doesn't throw
            }
        }

        [Fact]
        public void DbPropertyEntry_OriginalValue_setter_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked()
        {
            DbPropertyEntry_method_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked(
                e => e.OriginalValue = "");
        }

        [Fact]
        public void DbPropertyEntry_OriginalValue_getter_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked()
        {
            DbPropertyEntry_method_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked(
                e => { var _ = e.OriginalValue; });
        }

        [Fact]
        public void
            DbPropertyEntry_IsModified_setter_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked()
        {
            DbPropertyEntry_method_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked(
                e => e.IsModified = true);
        }

        private void DbPropertyEntry_method_can_be_used_on_previously_detached_entry_after_the_entity_becomes_tracked(
            Action<DbPropertyEntry> test)
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
                var entry = context.Entry(team).Property(t => t.Name);

                context.Teams.Attach(team);

                test(entry); // Just testing here that this doesn't throw
            }
        }

        #endregion

        #region Tests for access to entries via Member methods

        [Fact]
        public void Can_get_generic_scalar_entry_using_Member()
        {
            using (var context = new F1Context())
            {
                var entityEntry = GetTeamEntry(context);

                var propEntry = entityEntry.Member<string>("Name");

                Assert.IsType<DbPropertyEntry<Team, string>>(propEntry);
                Assert.Equal(entityEntry.Entity.Name, propEntry.CurrentValue);
            }
        }

        [Fact]
        public void Can_get_non_generic_scalar_entry_using_Member()
        {
            using (var context = new F1Context())
            {
                var entityEntry = (DbEntityEntry)GetTeamEntry(context);

                var propEntry = entityEntry.Member("Name");

                Assert.IsType<DbPropertyEntry>(propEntry);
                Assert.Equal(((Team)entityEntry.Entity).Name, propEntry.CurrentValue);
            }
        }

        [Fact]
        public void Can_get_generic_complex_entry_using_Member()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entityEntry = GetBuildingEntry(context);

                var propEntry = entityEntry.Member<Address>("Address");

                Assert.IsType<DbComplexPropertyEntry<Building, Address>>(propEntry);
                Assert.Same(entityEntry.Entity.Address, propEntry.CurrentValue);
            }
        }

        [Fact]
        public void Can_get_non_complex_entry_using_Member()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entityEntry = (DbEntityEntry)GetBuildingEntry(context);

                var propEntry = entityEntry.Member("Address");

                Assert.IsType<DbComplexPropertyEntry>(propEntry);
                Assert.Same(((Building)entityEntry.Entity).Address, propEntry.CurrentValue);
            }
        }

        [Fact]
        public void Can_get_generic_reference_entry_using_Member()
        {
            using (var context = new F1Context())
            {
                var entityEntry = GetTeamEntry(context);

                var propEntry = entityEntry.Member<Gearbox>("Gearbox");

                Assert.IsType<DbReferenceEntry<Team, Gearbox>>(propEntry);
                Assert.Same(entityEntry.Entity.Gearbox, propEntry.CurrentValue);
            }
        }

        [Fact]
        public void Can_get_non_generic_reference_entry_using_Member()
        {
            using (var context = new F1Context())
            {
                var entityEntry = (DbEntityEntry)GetTeamEntry(context);

                var propEntry = entityEntry.Member("Gearbox");

                Assert.IsType<DbReferenceEntry>(propEntry);
                Assert.Same(((Team)entityEntry.Entity).Gearbox, propEntry.CurrentValue);
            }
        }

        [Fact]
        public void Can_get_generic_collection_entry_using_Member()
        {
            using (var context = new F1Context())
            {
                var entityEntry = GetTeamEntry(context);

                // Note that DbMemberEntry type if in terms of ICollection<Element> while DbCollectionEntry
                // type is in terms of Element only.
                var propEntry = entityEntry.Member<ICollection<Driver>>("Drivers");

                Assert.IsType<DbCollectionEntry<Team, Driver>>(propEntry);
                Assert.Same(entityEntry.Entity.Drivers, propEntry.CurrentValue);
            }
        }

        [Fact]
        public void Can_get_non_generic_collection_entry_using_Member()
        {
            using (var context = new F1Context())
            {
                var entityEntry = (DbEntityEntry)GetTeamEntry(context);

                var propEntry = entityEntry.Member("Drivers");

                Assert.IsType<DbCollectionEntry>(propEntry);
                Assert.Same(((Team)entityEntry.Entity).Drivers, propEntry.CurrentValue);
            }
        }

        // See Dev11 bug 138738
        [Fact]
        public void Can_get_generic_collection_for_navigation_property_derived_from_ICollection_entry_using_Member()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entityEntry = GetBuildingEntry(context);

                var propEntry = entityEntry.Member<ICollection<MailRoom>>("MailRooms");

                Assert.IsType<DbCollectionEntry<Building, MailRoom>>(propEntry);
                Assert.Same(entityEntry.Entity.MailRooms, propEntry.CurrentValue);
            }
        }

        // See Dev11 bug 138738
        [Fact]
        public void Using_generic_type_that_is_not_ICollection_to_get_collection_entry_using_Member_throws()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entityEntry = GetBuildingEntry(context);

                // MailRooms is typed as IList<MailRoom> so at first glance it looks like this should work
                // but it doesn't because the returned type must use ICollection.
                Assert.Throws<ArgumentException>(() => entityEntry.Member<IList<MailRoom>>("MailRooms")).ValidateMessage
                    (
                        "DbEntityEntry_WrongGenericForCollectionNavProp", typeof(IList<MailRoom>).ToString(), "MailRooms",
                        typeof(Building).ToString(), typeof(ICollection<MailRoom>).ToString());
            }
        }

        // See Dev11 bug 138738
        [Fact]
        public void
            Using_base_collection_type_for_navigation_property_derived_from_ICollection_entry_using_Member_throws()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entityEntry = GetBuildingEntry(context);

                // MailRooms is typed as IList<MailRoom> so this should never work
                Assert.Throws<ArgumentException>(() => entityEntry.Member<List<MailRoom>>("MailRooms")).ValidateMessage(
                    "DbEntityEntry_WrongGenericForCollectionNavProp", typeof(List<MailRoom>).ToString(), "MailRooms",
                    typeof(Building).ToString(), typeof(ICollection<MailRoom>).ToString());
            }
        }

        [Fact]
        public void Using_wrong_generic_type_for_scalar_property_with_string_Member_method_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Member<int>("Name")).ValidateMessage(
                    "DbEntityEntry_WrongGenericForProp", "Name", "Team", "Int32", "String");
            }
        }

        [Fact]
        public void Using_wrong_generic_type_for_complex_property_with_string_Member_method_throws()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var entry = GetBuildingEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Member<string>("Address")).ValidateMessage(
                    "DbEntityEntry_WrongGenericForProp", "Address", "Building", "String", "Address");
            }
        }

        [Fact]
        public void Using_wrong_generic_type_for_reference_property_with_string_Member_method_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Member<Engine>("Gearbox")).ValidateMessage(
                    "DbEntityEntry_WrongGenericForNavProp", "Gearbox", "Team", "Engine", "Gearbox");
            }
        }

        [Fact]
        public void Using_wrong_generic_type_for_collection_property_with_string_Member_method_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Member<Engine>("Drivers")).ValidateMessage(
                    "DbEntityEntry_WrongGenericForCollectionNavProp", typeof(Engine).ToString(), "Drivers",
                    typeof(Team).ToString(), typeof(ICollection<Driver>).ToString());
            }
        }

        [Fact]
        public void Using_just_element_type_for_collection_property_with_string_Member_method_throws()
        {
            using (var context = new F1Context())
            {
                var entry = GetTeamEntry(context);

                Assert.Throws<ArgumentException>(() => entry.Member<Driver>("Drivers")).ValidateMessage(
                    "DbEntityEntry_WrongGenericForCollectionNavProp", typeof(Driver).ToString(), "Drivers",
                    typeof(Team).ToString(), typeof(ICollection<Driver>).ToString());
            }
        }

        #endregion
    }
}
