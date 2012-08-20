// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Data.Entity.Infrastructure;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using AdvancedPatternsModel;
    using ConcurrencyModel;
    using SimpleModel;
    using Xunit;
    using Xunit.Extensions;

    /// <summary>
    ///     Functional tests for concurrency exceptions.
    ///     Unit tests also exist in the unit tests project.
    /// </summary>
    public class ConcurrencyTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        public ConcurrencyTests()
        {
            using (var context = new F1Context())
            {
                context.Database.Initialize(force: false);
            }

            using (var context = new AdvancedPatternsMasterContext())
            {
                context.Database.Initialize(force: false);
            }
        }

        #endregion

        #region Concurrency resolution with FK associations

        [Fact]
        [AutoRollback]
        public void Simple_concurrency_exception_can_be_resolved_with_client_values()
        {
            ConcurrencyTest(
                ClientPodiums, (c, ex) =>
                                   {
                                       var driverEntry = ex.Entries.Single();
                                       driverEntry.OriginalValues.SetValues(driverEntry.GetDatabaseValues());
                                   });
        }

        [Fact]
        [AutoRollback]
        public void Simple_concurrency_exception_can_be_resolved_with_store_values()
        {
            ConcurrencyTest(
                StorePodiums, (c, ex) =>
                                  {
                                      var driverEntry = ex.Entries.Single();
                                      var storeValues = driverEntry.GetDatabaseValues();
                                      driverEntry.CurrentValues.SetValues(storeValues);
                                      driverEntry.OriginalValues.SetValues(storeValues);
                                  });
        }

        [Fact]
        [AutoRollback]
        public void Simple_concurrency_exception_can_be_resolved_with_new_values()
        {
            ConcurrencyTest(
                10, (c, ex) =>
                        {
                            var driverEntry = ex.Entries.Single().Cast<Driver>();
                            driverEntry.OriginalValues.SetValues(driverEntry.GetDatabaseValues());
                            driverEntry.Entity.Podiums = 10;
                        });
        }

        [Fact]
        [AutoRollback]
        public void Simple_concurrency_exception_can_be_resolved_with_client_values_using_objects()
        {
            ConcurrencyTest(
                ClientPodiums, (c, ex) =>
                                   {
                                       var driverEntry = ex.Entries.Single();
                                       driverEntry.OriginalValues.SetValues(
                                           driverEntry.GetDatabaseValues().ToObject());
                                   });
        }

        [Fact]
        [AutoRollback]
        public void Simple_concurrency_exception_can_be_resolved_with_store_values_using_objects()
        {
            ConcurrencyTest(
                StorePodiums, (c, ex) =>
                                  {
                                      var driverEntry = ex.Entries.Single();
                                      var storeObject = driverEntry.GetDatabaseValues().ToObject();
                                      driverEntry.CurrentValues.SetValues(storeObject);
                                      driverEntry.OriginalValues.SetValues(storeObject);
                                  });
        }

        [Fact]
        [AutoRollback]
        public void Simple_concurrency_exception_can_be_resolved_with_new_values_using_objects()
        {
            ConcurrencyTest(
                10, (c, ex) =>
                        {
                            var driverEntry = ex.Entries.Single().Cast<Driver>();
                            driverEntry.OriginalValues.SetValues(driverEntry.GetDatabaseValues().ToObject());
                            driverEntry.Entity.Podiums = 10;
                        });
        }

        [Fact]
        [AutoRollback]
        public void Simple_concurrency_exception_can_be_resolved_with_client_values_using_cloned_property_values()
        {
            ConcurrencyTest(
                ClientPodiums, (c, ex) =>
                                   {
                                       var driverEntry = ex.Entries.Single();
                                       driverEntry.OriginalValues.SetValues(
                                           driverEntry.GetDatabaseValues().Clone());
                                   });
        }

        [Fact]
        [AutoRollback]
        public void Simple_concurrency_exception_can_be_resolved_with_store_values_using_cloned_property_values()
        {
            ConcurrencyTest(
                StorePodiums, (c, ex) =>
                                  {
                                      var driverEntry = ex.Entries.Single();
                                      var storeValues = driverEntry.GetDatabaseValues().Clone();
                                      driverEntry.CurrentValues.SetValues(storeValues);
                                      driverEntry.OriginalValues.SetValues(storeValues);
                                  });
        }

        [Fact]
        [AutoRollback]
        public void Simple_concurrency_exception_can_be_resolved_with_new_values_using_cloned_property_values()
        {
            ConcurrencyTest(
                10, (c, ex) =>
                        {
                            var driverEntry = ex.Entries.Single().Cast<Driver>();
                            driverEntry.OriginalValues.SetValues(driverEntry.GetDatabaseValues().Clone());
                            driverEntry.Entity.Podiums = 10;
                        });
        }

        [Fact]
        [AutoRollback]
        public void Simple_concurrency_exception_can_be_resolved_with_store_values_using_equivalent_of_accept_changes()
        {
            ConcurrencyTest(
                StorePodiums, (c, ex) =>
                                  {
                                      var driverEntry = ex.Entries.Single();
                                      driverEntry.CurrentValues.SetValues(driverEntry.GetDatabaseValues());
                                      driverEntry.State = EntityState.Unchanged;
                                  });
        }

        [Fact]
        [AutoRollback]
        public void Simple_concurrency_exception_can_be_resolved_with_store_values_using_Reload()
        {
            ConcurrencyTest(StorePodiums, (c, ex) => ex.Entries.Single().Reload());
        }

        [Fact]
        [AutoRollback]
        public void Two_concurrency_issues_in_one_to_one_related_entities_can_be_handled_by_dealing_with_dependent_first()
        {
            ConcurrencyTest(
                c =>
                    {
                        var team = c.Teams.Find(Team.McLaren);
                        team.Chassis.Name = "MP4-25b";
                        team.Principal = "Larry David";
                    },
                c =>
                    {
                        var team = c.Teams.Find(Team.McLaren);
                        team.Chassis.Name = "MP4-25c";
                        team.Principal = "Jerry Seinfeld";
                    }, (c, ex) =>
                           {
                               Assert.IsType<DbUpdateConcurrencyException>(ex);

                               var entry = ex.Entries.Single();
                               Assert.IsAssignableFrom<Chassis>(entry.Entity);
                               entry.Reload();

                               try
                               {
                                   c.SaveChanges();
                                   Assert.True(false, "Expected second exception due to conflict in principals.");
                               }
                               catch (DbUpdateConcurrencyException ex2)
                               {
                                   var entry2 = ex2.Entries.Single();
                                   Assert.IsAssignableFrom<Team>(entry2.Entity);
                                   entry2.Reload();
                               }
                           },
                c =>
                    {
                        var team = c.Teams.Find(Team.McLaren);
                        Assert.Equal("MP4-25b", team.Chassis.Name);
                        Assert.Equal("Larry David", team.Principal);
                    });
        }

        [Fact]
        [AutoRollback]
        public void
            Two_concurrency_issues_in_one_to_many_related_entities_can_be_handled_by_dealing_with_dependent_first()
        {
            ConcurrencyTest(
                c =>
                    {
                        var team = c.Teams.Find(Team.McLaren);
                        team.Drivers.Single(d => d.Name == "Jenson Button").Poles = 1;
                        team.Principal = "Larry David";
                    },
                c =>
                    {
                        var team = c.Teams.Find(Team.McLaren);
                        team.Drivers.Single(d => d.Name == "Jenson Button").Poles = 2;
                        team.Principal = "Jerry Seinfeld";
                    }, (c, ex) =>
                           {
                               Assert.IsType<DbUpdateConcurrencyException>(ex);

                               var entry = ex.Entries.Single();
                               Assert.IsAssignableFrom<Driver>(entry.Entity);
                               entry.Reload();

                               try
                               {
                                   c.SaveChanges();
                                   Assert.True(false, "Expected second exception due to conflict in principals.");
                               }
                               catch (DbUpdateConcurrencyException ex2)
                               {
                                   var entry2 = ex2.Entries.Single();
                                   Assert.IsAssignableFrom<Team>(entry2.Entity);
                                   entry2.Reload();
                               }
                           },
                c =>
                    {
                        var team = c.Teams.Find(Team.McLaren);
                        Assert.Equal(1, team.Drivers.Single(d => d.Name == "Jenson Button").Poles);
                        Assert.Equal("Larry David", team.Principal);
                    });
        }

        [Fact]
        [AutoRollback]
        public void Concurrency_issue_where_the_FK_is_the_concurrency_token_can_be_handled()
        {
            ConcurrencyTest(
                c =>
                c.Engines.Single(e => e.Name == "056").EngineSupplierId =
                c.EngineSuppliers.Single(s => s.Name == "Cosworth").Id,
                c =>
                c.Engines.Single(e => e.Name == "056").EngineSupplier =
                c.EngineSuppliers.Single(s => s.Name == "Renault"),
                (c, ex) =>
                    {
                        Assert.IsType<DbUpdateConcurrencyException>(ex);

                        var entry = ex.Entries.Single();
                        Assert.IsAssignableFrom<Engine>(entry.Entity);
                        entry.Reload();
                    },
                c =>
                Assert.Equal(
                    "Cosworth",
                    c.Engines.Include(e => e.EngineSupplier).Single(e => e.Name == "056").EngineSupplier.Name));
        }

        #endregion

        #region Concurrency exceptions with independent associations

        private void VerifyIndependentAssociationUpdateMessage(string message)
        {
            new StringResourceVerifier(
                new AssemblyResourceLookup(
                    EntityFrameworkAssembly,
                    "System.Data.Entity.Properties.Resources")).
                VerifyMatch(
                    "DbContext_IndependentAssociationUpdateException", message);
        }

        [Fact]
        [AutoRollback]
        public void Change_in_independent_association_results_in_independent_association_exception()
        {
            ConcurrencyTest(
                c => c.Teams.Single(t => t.Id == Team.Ferrari).Engine = c.Engines.Single(s => s.Name == "FO 108X"),
                (c, ex) =>
                    {
                        VerifyIndependentAssociationUpdateMessage(ex.Message);
                        Assert.Null(ex.Entries.SingleOrDefault());
                        Assert.IsType<OptimisticConcurrencyException>(ex.InnerException);
                    },
                null);
        }

        [Fact]
        [AutoRollback]
        public void
            Change_in_independent_association_after_change_in_different_concurrency_token_results_in_independent_association_exception()
        {
            ConcurrencyTest(
                c => c.Teams.Single(t => t.Id == Team.Ferrari).FastestLaps = 0,
                c =>
                c.Teams.Single(t => t.Constructor == "Ferrari").Engine =
                c.Engines.Single(s => s.Name == "FO 108X"),
                (c, ex) =>
                    {
                        VerifyIndependentAssociationUpdateMessage(ex.Message);
                        Assert.Null(ex.Entries.SingleOrDefault());
                        Assert.IsType<OptimisticConcurrencyException>(ex.InnerException);
                    },
                null);
        }

        [Fact]
        [AutoRollback]
        public void
            Attempting_to_delete_same_relationship_twice_for_many_to_many_results_in_independent_association_exception()
        {
            ConcurrencyTest(
                c =>
                c.Teams.Single(t => t.Id == Team.McLaren).Sponsors.Add(c.Sponsors.Single(s => s.Name.Contains("Shell"))),
                (c, ex) =>
                    {
                        VerifyIndependentAssociationUpdateMessage(ex.Message);
                        Assert.Null(ex.Entries.SingleOrDefault());
                        Assert.IsType<UpdateException>(ex.InnerException);
                    },
                null);
        }

        [Fact]
        [AutoRollback]
        public void
            Attempting_to_add_same_relationship_twice_for_many_to_many_results_in_independent_association_exception()
        {
            ConcurrencyTest(
                c =>
                c.Teams.Single(t => t.Id == Team.McLaren).Sponsors.Remove(c.Sponsors.Single(s => s.Name.Contains("FIA"))),
                (c, ex) =>
                    {
                        VerifyIndependentAssociationUpdateMessage(ex.Message);
                        Assert.Null(ex.Entries.SingleOrDefault());
                        Assert.IsType<OptimisticConcurrencyException>(ex.InnerException);
                    },
                null);
        }

        #endregion

        #region Concurrency exceptions with complex types

        [Fact]
        [AutoRollback]
        public void Concurrency_issue_where_a_complex_type_nested_member_is_the_concurrency_token_can_be_handled()
        {
            ConcurrencyTest(
                c => c.Engines.Single(s => s.Name == "CA2010").StorageLocation.Latitude = 47.642576,
                (c, ex) =>
                    {
                        Assert.IsType<DbUpdateConcurrencyException>(ex);

                        var entry = ex.Entries.Single();
                        Assert.IsAssignableFrom<Engine>(entry.Entity);
                        entry.Reload();
                    },
                c =>
                Assert.Equal(47.642576, c.Engines.Single(s => s.Name == "CA2010").StorageLocation.Latitude));
        }

        #endregion

        #region Tests for update exceptions involving adding and deleting entities

        [Fact]
        [AutoRollback]
        public void Adding_the_same_entity_twice_results_in_DbUpdateException()
        {
            ConcurrencyTest(
                c =>
                c.Teams.Add(
                    new Team
                        {
                            Id = -1,
                            Name = "Wubbsy Racing",
                            Chassis = new Chassis
                                          {
                                              TeamId = -1,
                                              Name = "Wubbsy"
                                          }
                        }),
                (c, ex) =>
                    {
                        Assert.IsType<DbUpdateException>(ex);
                        Assert.IsType<UpdateException>(ex.InnerException);
                    },
                null);
        }

        [Fact]
        [AutoRollback]
        public void Deleting_the_same_entity_twice_results_in_DbUpdateConcurrencyException()
        {
            ConcurrencyTest(
                c => c.Drivers.Remove(c.Drivers.Single(d => d.Name == "Fernando Alonso")),
                (c, ex) =>
                    {
                        Assert.IsType<DbUpdateConcurrencyException>(ex);
                        Assert.IsType<OptimisticConcurrencyException>(ex.InnerException);

                        var entry = ex.Entries.Single();
                        Assert.IsAssignableFrom<Driver>(entry.Entity);
                        entry.Reload();
                    },
                c => Assert.Null(c.Drivers.SingleOrDefault(d => d.Name == "Fernando Alonso")));
        }

        [Fact]
        [AutoRollback]
        public void
            Deleting_the_same_entity_twice_when_entity_has_independent_association_results_in_DbIndependentAssociationUpdateException()
        {
            ConcurrencyTest(
                c => c.Teams.Remove(c.Teams.Single(t => t.Id == Team.Hispania)),
                (c, ex) =>
                    {
                        VerifyIndependentAssociationUpdateMessage(ex.Message);
                        Assert.Null(ex.Entries.SingleOrDefault());
                        Assert.IsType<OptimisticConcurrencyException>(ex.InnerException);
                    },
                null);
        }

        [Fact]
        [AutoRollback]
        public void Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException()
        {
            ConcurrencyTest(
                c => c.Drivers.Single(d => d.Name == "Fernando Alonso").Wins = 1,
                c => c.Drivers.Remove(c.Drivers.Single(d => d.Name == "Fernando Alonso")),
                (c, ex) =>
                    {
                        Assert.IsType<DbUpdateConcurrencyException>(ex);
                        Assert.IsType<OptimisticConcurrencyException>(ex.InnerException);

                        var entry = ex.Entries.Single();
                        Assert.IsAssignableFrom<Driver>(entry.Entity);
                        entry.Reload();
                    },
                c => Assert.Equal(1, c.Drivers.Single(d => d.Name == "Fernando Alonso").Wins));
        }

        [Fact]
        [AutoRollback]
        public void Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException_which_can_be_resolved_with_store_values()
        {
            ConcurrencyTest(
                c => c.Drivers.Single(d => d.Name == "Fernando Alonso").Wins = 1,
                c => c.Drivers.Remove(c.Drivers.Single(d => d.Name == "Fernando Alonso")),
                (c, ex) =>
                    {
                        Assert.IsType<DbUpdateConcurrencyException>(ex);
                        Assert.IsType<OptimisticConcurrencyException>(ex.InnerException);

                        var entry = ex.Entries.Single();
                        Assert.IsAssignableFrom<Driver>(entry.Entity);

                        entry.State = EntityState.Unchanged;
                        var storeValues = entry.GetDatabaseValues();
                        entry.OriginalValues.SetValues(storeValues);
                        entry.CurrentValues.SetValues(storeValues);
                    },
                c => Assert.Equal(1, c.Drivers.Single(d => d.Name == "Fernando Alonso").Wins));
        }

        [Fact]
        [AutoRollback]
        public void Deleting_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException()
        {
            ConcurrencyTest(
                c => c.Drivers.Remove(c.Drivers.Single(d => d.Name == "Fernando Alonso")),
                c => c.Drivers.Single(d => d.Name == "Fernando Alonso").Wins = 1,
                (c, ex) =>
                    {
                        Assert.IsType<DbUpdateConcurrencyException>(ex);
                        Assert.IsType<OptimisticConcurrencyException>(ex.InnerException);

                        var entry = ex.Entries.Single();
                        Assert.IsAssignableFrom<Driver>(entry.Entity);
                        entry.Reload();
                    },
                c => Assert.Null(c.Drivers.SingleOrDefault(d => d.Name == "Fernando Alonso")));
        }

        [Fact]
        [AutoRollback]
        public void Deleting_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException_which_can_be_resolved_with_store_values()
        {
            ConcurrencyTest(
                c => c.Drivers.Remove(c.Drivers.Single(d => d.Name == "Fernando Alonso")),
                c => c.Drivers.Single(d => d.Name == "Fernando Alonso").Wins = 1,
                (c, ex) =>
                    {
                        Assert.IsType<DbUpdateConcurrencyException>(ex);
                        Assert.IsType<OptimisticConcurrencyException>(ex.InnerException);

                        var entry = ex.Entries.Single();
                        Assert.IsAssignableFrom<Driver>(entry.Entity);
                        var storeValues = entry.GetDatabaseValues();
                        Assert.Null(storeValues);
                        entry.State = EntityState.Detached;
                    },
                c => Assert.Null(c.Drivers.SingleOrDefault(d => d.Name == "Fernando Alonso")));
        }

        #endregion

        #region Tests for calling Reload on an entity in various states

        [Fact]
        public void Calling_Reload_on_an_Added_entity_throws()
        {
            using (var context = new F1Context())
            {
                var entry =
                    context.Entry(
                        context.Drivers.Add(
                            new Driver
                                {
                                    Name = "Larry David",
                                    TeamId = Team.Ferrari
                                }));

                Assert.Throws<InvalidOperationException>(() => entry.Reload()).ValidateMessage(
                    "DbPropertyValues_CannotGetValuesForState", "Reload", "Added");
            }
        }

        [Fact]
        public void Calling_Reload_on_a_detached_entity_throws()
        {
            using (var context = new F1Context())
            {
                var entry =
                    context.Entry(
                        context.Drivers.Add(
                            new Driver
                                {
                                    Name = "Larry David",
                                    TeamId = Team.Ferrari
                                }));
                entry.State = EntityState.Detached;

                Assert.Throws<InvalidOperationException>(() => entry.Reload()).ValidateMessage(
                    "DbEntityEntry_NotSupportedForDetached", "Reload", "Driver");
            }
        }

        [Fact]
        public void Calling_Reload_on_a_Unchanged_entity_makes_the_entity_unchanged()
        {
            TestReloadPositive(EntityState.Unchanged);
        }

        [Fact]
        public void Calling_Reload_on_a_Modified_entity_makes_the_entity_unchanged()
        {
            TestReloadPositive(EntityState.Modified);
        }

        [Fact]
        public void Calling_Reload_on_a_Deleted_entity_makes_the_entity_unchanged()
        {
            TestReloadPositive(EntityState.Deleted);
        }

        private void TestReloadPositive(EntityState state)
        {
            using (var context = new F1Context())
            {
                var larry = context.Drivers.Single(d => d.Name == "Jenson Button");
                var entry = context.Entry(larry);
                entry.State = state;

                entry.Reload();

                Assert.Equal(EntityState.Unchanged, entry.State);
            }
        }

        #endregion

        #region Serialization of exceptions

        [Fact]
        [AutoRollback]
        public void DbUpdateException_can_be_serialized_but_does_not_serialize_entries()
        {
            ConcurrencyTest(
                c =>
                c.Teams.Add(
                    new Team
                        {
                            Id = -1,
                            Name = "Wubbsy Racing",
                            Chassis = new Chassis
                                          {
                                              TeamId = -1,
                                              Name = "Wubbsy"
                                          }
                        }),
                (c, ex) => TestExceptionSerializartion(ex, c),
                null);
        }

        [Fact]
        [AutoRollback]
        public void DbUpdateConcurrencyException_can_be_serialized_but_does_not_serialize_entries()
        {
            ConcurrencyTest(
                c => c.Teams.Find(Team.McLaren).Races = 1,
                (c, ex) => TestExceptionSerializartion((DbUpdateConcurrencyException)ex, c),
                null);
        }

        private void TestExceptionSerializartion<TException>(TException exception, DbContext context)
            where TException : DbUpdateException
        {
            Assert.NotNull(exception.Entries.SingleOrDefault());

            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();

            formatter.Serialize(stream, exception);
            stream.Seek(0, SeekOrigin.Begin);
            var deserializedException = (TException)formatter.Deserialize(stream);

            Assert.Null(deserializedException.Entries.SingleOrDefault());
        }

        [Fact]
        [AutoRollback]
        public void DbUpdateException_for_independent_association_error_can_be_serialized()
        {
            ConcurrencyTest(
                c => c.Teams.Remove(c.Teams.Single(t => t.Id == Team.Hispania)),
                (c, ex) =>
                    {
                        var stream = new MemoryStream();
                        var formatter = new BinaryFormatter();

                        formatter.Serialize(stream, ex);
                        stream.Seek(0, SeekOrigin.Begin);
                        var deserializedException = (DbUpdateException)formatter.Deserialize(stream);

                        VerifyIndependentAssociationUpdateMessage(deserializedException.Message);
                        Assert.Null(deserializedException.Entries.SingleOrDefault());
                    },
                null);
        }

        #endregion

        #region Multiple entities in an update exception

        // See DevDiv2 93724
        public void Multiple_entities_in_excecption_from_update_pipeline_can_be_handled()
        {
            using (new TransactionScope())
            {
                using (var context = new AdvancedPatternsMasterContext())
                {
                    // Create two entities which both have dependencies on each other
                    // to force a dependency ordering exception from the update pipeline.

                    var building = new Building
                                       {
                                           BuildingId = new Guid("14C62AB6-A49C-40BD-BD5C-D374E070D3D7"),
                                           Name = "Building 18",
                                           Value = 1m,
                                           PrincipalMailRoomId = -1,
                                           Address =
                                               new Address
                                                   {
                                                       Street = "100 Work St",
                                                       City = "Redmond",
                                                       State = "WA",
                                                       ZipCode = "98052",
                                                       SiteInfo = new SiteInfo
                                                                      {
                                                                          Zone = 1,
                                                                          Environment = "Clean"
                                                                      }
                                                   },
                                       };

                    var mailRoom = new MailRoom
                                       {
                                           id = (int)building.PrincipalMailRoomId,
                                           BuildingId = building.BuildingId
                                       };

                    context.Buildings.Add(building);
                    context.Set<MailRoom>().Add(mailRoom);

                    try
                    {
                        context.SaveChanges();
                        Assert.True(false);
                    }
                    catch (DbUpdateException ex)
                    {
                        Assert.IsType<UpdateException>(ex.InnerException);

                        var entries = ex.Entries.ToList();
                        Assert.Equal(2, entries.Count());
                        Assert.True(entries.Any(e => ReferenceEquals(e.Entity, building)));
                        Assert.True(entries.Any(e => ReferenceEquals(e.Entity, mailRoom)));
                    }
                }
            }
        }

        #endregion

        #region Not awaited async methods

#if !NET40

        /// <summary>
        /// An IExecutionStrategy that blocks the execution of async actions until it is signaled
        /// </summary>
        public class BlockingStrategy : IExecutionStrategy
        {
            private readonly Task _signalTask;

            public BlockingStrategy(Task signalTask)
            {
                _signalTask = signalTask;
            }

            public bool RetriesOnFailure
            {
                get { return false; }
            }

            public void Execute(Action action)
            {
                action();
            }

            public TResult Execute<TResult>(Func<TResult> func)
            {
                return func();
            }

            public async Task ExecuteAsync(Func<Task> taskFunc)
            {
                await _signalTask;
                await taskFunc();
            }

            public async Task ExecuteAsync(Func<Task> taskFunc, CancellationToken cancellationToken)
            {
                await _signalTask;
                await taskFunc();
            }

            public async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> taskFunc)
            {
                await _signalTask;
                return await taskFunc();
            }

            public async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> taskFunc, CancellationToken cancellationToken)
            {
                await _signalTask;
                return await taskFunc();
            }
        }

        [Fact]
        public void SaveChanges_throws_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.SaveChangesAsync());
                        context.SaveChanges();
                    }, true);
        }

        [Fact]
        public void SaveChangesAsync_throws_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.SaveChangesAsync());
                        tasks.Add(context.SaveChangesAsync());
                    }, true);
        }

        [Fact]
        public void FindAsync_throws_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.SaveChangesAsync());
                        tasks.Add(context.Products.FindAsync(1));
                    }, true);
        }

        [Fact]
        public void FindAsync_triggers_exception_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.Products.FindAsync(1));
                        tasks.Add(context.SaveChangesAsync());
                    }, true);
        }

        [Fact]
        public void FindAsync_doesnt_trigger_exception_on_concurrent_call_if_already_tracked()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        context.Products.Find(1);
                        tasks.Add(context.Products.FindAsync(1));
                        tasks.Add(context.SaveChangesAsync());
                    }, false);
        }

        [Fact]
        public void Find_throws_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.SaveChangesAsync());
                        context.Products.Find(1);
                    }, true);
        }

        [Fact]
        public void DbQuery_ToListAsync_throws_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.SaveChangesAsync());
                        tasks.Add(context.Products.Where(p => p != null).ToListAsync());
                    }, true);
        }

        [Fact]
        public void DbQuery_ToListAsync_AsNoTracking_throws_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.SaveChangesAsync());
                        tasks.Add(context.Products.Where(p => p != null).AsNoTracking().ToListAsync());
                    }, true);
        }

        [Fact]
        public void DbQuery_ToListAsync_triggers_exception_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.Products.Where(p => p != null).ToListAsync());
                        tasks.Add(context.SaveChangesAsync());
                    }, true);
        }

        [Fact]
        public void DbQuery_ToListAsync_AsNoTracking_doesnt_trigger_exception_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.Products.Where(p => p != null).AsNoTracking().ToListAsync());
                        tasks.Add(context.SaveChangesAsync());
                    }, false);
        }

        [Fact]
        public void DbQuery_ToList_throws_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.SaveChangesAsync());
                        context.Products.Where(p => p != null).ToList();
                    }, true);
        }

        [Fact]
        public void DbSqlQuery_ToListAsync_throws_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.SaveChangesAsync());
                        tasks.Add(context.Products.SqlQuery("select * from Products").ToListAsync());
                    }, true);
        }

        [Fact]
        public void DbSqlQuery_ToListAsync_AsNoTracking_throws_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.SaveChangesAsync());
                        tasks.Add(context.Products.SqlQuery("select * from Products").AsNoTracking().ToListAsync());
                    }, true);
        }

        [Fact]
        public void DbSqlQuery_ToListAsync_triggers_exception_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.Products.SqlQuery("select * from Products").ToListAsync());
                        tasks.Add(context.SaveChangesAsync());
                    }, true);
        }

        [Fact]
        public void DbSqlQuery_ToListAsync_AsNoTracking_doesnt_trigger_exception_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.Products.SqlQuery("select * from Products").AsNoTracking().ToListAsync());
                        tasks.Add(context.SaveChangesAsync());
                    }, false);
        }

        [Fact]
        public void DbSqlQuery_ToList_throws_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.SaveChangesAsync());
                        context.Products.SqlQuery("select * from Products").ToList();
                    }, true);
        }

        [Fact]
        public void ExecuteFunction_throws_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.SaveChangesAsync());
                        ((IObjectContextAdapter)context).ObjectContext.ExecuteFunction("Foo");
                    }, true);
        }

        [Fact]
        public void ExecuteSqlCommand_throws_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.SaveChangesAsync());
                        context.Database.ExecuteSqlCommand("select * from Products");
                    }, true);
        }

        [Fact]
        public void ExecuteSqlCommandAsync_throws_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.SaveChangesAsync());
                        tasks.Add(context.Database.ExecuteSqlCommandAsync("select * from Products"));
                    }, true);
        }

        [Fact]
        public void ExecuteSqlCommandAsync_triggers_exception_on_concurrent_call()
        {
            VerifyConcurrency(
                (context, tasks) =>
                    {
                        context.Products.Add(new Product());
                        tasks.Add(context.Database.ExecuteSqlCommandAsync("select * from Products"));
                        tasks.Add(context.SaveChangesAsync());
                    }, true);
        }

        private void VerifyConcurrency(Action<SimpleModelContext, List<Task>> execute, bool shouldThrow)
        {
            var taskCompletionSource = new TaskCompletionSource<object>();
            MutableResolver.AddResolver<IExecutionStrategy>(k => new BlockingStrategy(taskCompletionSource.Task));

            // The returned tasks need to be awaited on before the test ends in case they are faulted
            var tasks = new List<Task>();
            try
            {
                // Needs MARS enabled for concurrent queries
                using (var context = new SimpleModelContext(
                    @"Data Source=.\SQLEXPRESS;Initial Catalog=SimpleModel.SimpleModelContext;Integrated Security=True;MultipleActiveResultSets=True")
                    )
                {
                    using (context.Database.BeginTransaction())
                    {
                        if (shouldThrow)
                        {
                            Assert.Throws<DbConcurrencyException>(
                                () => execute(context, tasks)).ValidateMessage("ConcurrentMethodInvocation");
                        }
                        else
                        {
                            execute(context, tasks);
                        }

                        taskCompletionSource.SetResult(null);
                        // Need to wait for all readers to close before disposing the transactions
                        // The exception needs to be thrown before this call
                        Task.WaitAll(tasks.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                if (!taskCompletionSource.Task.IsCompleted)
                {
                    taskCompletionSource.SetException(ex);
                }
                Task.WaitAll(tasks.ToArray());
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }
        }

#endif

        #endregion

        #region Helpers

        private const int StorePodiums = 20;
        private const int ClientPodiums = 30;

        private void ConcurrencyTest(int expectedPodiums, Action<F1Context, DbUpdateConcurrencyException> resolver)
        {
            ConcurrencyTest(
                c => c.Drivers.Single(d => d.CarNumber == 1).Podiums = StorePodiums,
                c => c.Drivers.Single(d => d.CarNumber == 1).Podiums = ClientPodiums,
                (c, ex) =>
                    {
                        Assert.IsType<DbUpdateConcurrencyException>(ex);
                        Assert.IsType<OptimisticConcurrencyException>(ex.InnerException);

                        resolver(c, (DbUpdateConcurrencyException)ex);
                    },
                c => Assert.Equal(expectedPodiums, c.Drivers.Single(d => d.CarNumber == 1).Podiums));
        }

        /// <summary>
        ///     Runs the same action twice inside a transaction scope but with two different contexts and calling
        ///     SaveChanges such that first time it will succeed and then the second time it will result in a
        ///     concurrency exception.
        ///     After the exception is caught the resolver action is called, after which SaveChanges is called
        ///     again.  Finally, a new context is created and the validator is called so that the state of
        ///     the database at the end of the process can be validated.
        /// </summary>
        private void ConcurrencyTest(
            Action<F1Context> change, Action<F1Context, DbUpdateException> resolver,
            Action<F1Context> validator)
        {
            ConcurrencyTest(change, change, resolver, validator);
        }

        /// <summary>
        ///     Runs the two actions inside a transaction scope but with two different contexts and calling
        ///     SaveChanges such that storeChange will succeed and the store will reflect this change, and
        ///     then clientChange will result in a concurrency exception.
        ///     After the exception is caught the resolver action is called, after which SaveChanges is called
        ///     again.  Finally, a new context is created and the validator is called so that the state of
        ///     the database at the end of the process can be validated.
        /// </summary>
        private void ConcurrencyTest(
            Action<F1Context> storeChange, Action<F1Context> clientChange,
            Action<F1Context, DbUpdateException> resolver, Action<F1Context> validator)
        {
            using (var context = new F1Context())
            {
                clientChange(context);

                using (var innerContext = new F1Context())
                {
                    storeChange(innerContext);
                    innerContext.SaveChanges();
                }

                try
                {
                    context.SaveChanges();
                    Assert.True(false);
                }
                catch (DbUpdateException ex)
                {
                    Assert.IsAssignableFrom<UpdateException>(ex.InnerException);

                    resolver(context, ex);

                    if (validator != null)
                    {
                        context.SaveChanges();

                        using (var validationContext = new F1Context())
                        {
                            validator(validationContext);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
