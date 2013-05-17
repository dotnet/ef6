// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Update
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.TestModels.GearsOfWarModel;
    using System.Linq;
    using Xunit;
    using Xunit.Extensions;

    public class UpdateTests : FunctionalTestBase
    {
        static UpdateTests()
        {
            // force database initialization, we don't want this to happen inside transaction
            using (var context = new GearsOfWarContext())
            {
                context.Weapons.Count();
            }
        }
        
        [Fact]
        [AutoRollback]
        public void Verify_that_deletes_precede_inserts()
        {
            using (var context = new GearsOfWarContext())
            {
                var squad1 = new Squad
                    {
                        Id = 3,
                        Name = "Alpha",
                    };

                context.Squads.Add(squad1);
                context.SaveChanges();

                var squad2 = new Squad
                    {
                        Id = 3,
                        Name = "Bravo",
                    };

                context.Squads.Add(squad2);
                context.Squads.Remove(squad1);
                context.SaveChanges();

                Assert.Equal(
                    "Bravo", context.Squads.Where(o => o.Id == 3).Select(s => s.Name).Single());
            }
        }

        [Fact]
        [AutoRollback]
        public void Verify_that_order_of_insert_is_based_on_key_values_and_not_order_of_adding_to_collection()
        {
            using (var context = new GearsOfWarContext())
            {
                var weapon1 = new HeavyWeapon
                    {
                        Id = 10,
                        Name = "Mortar",
                        Overheats = false,
                    };

                var weapon2 = new HeavyWeapon
                    {
                        Id = 11,
                        Name = "Oneshot",
                        Overheats = false,
                    };

                var weapon3 = new StandardWeapon
                    {
                        Id = 12,
                        Name = "Boltok",
                        Specs = new WeaponSpecification
                            {
                                AmmoPerClip = 6,
                                ClipsCount = 9,
                            },
                    };

                context.Weapons.Add(weapon3);
                context.Weapons.Add(weapon1);
                context.Weapons.Add(weapon2);
                context.SaveChanges();

                var newWeapons = context.Weapons.OrderByDescending(t => t.Id).Take(3).ToList();

                Assert.Equal("Boltok", newWeapons[0].Name);
                Assert.Equal("Oneshot", newWeapons[1].Name);
                Assert.Equal("Mortar", newWeapons[2].Name);
            }
        }

        [Fact]
        [AutoRollback]
        public void Insert_resulting_in_data_truncation_throws_exception()
        {
            using (var context = new GearsOfWarContext())
            {
                var cogTagNoteMaxLength = 40;
                var cogTag = new CogTag
                    {
                        Id = Guid.NewGuid(),
                        Note = new string('A', cogTagNoteMaxLength + 1),
                    };

                context.Tags.Add(cogTag);

                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }
        }

        [Fact]
        [AutoRollback]
        public void Update_resulting_in_data_truncation_throws_exception()
        {
            using (var context = new GearsOfWarContext())
            {
                var cogTagNoteMaxLength = 40;
                var cogTag = new CogTag
                    {
                        Id = Guid.NewGuid(),
                        Note = new string('A', cogTagNoteMaxLength),
                    };

                context.Tags.Add(cogTag);
                context.SaveChanges();

                cogTag.Note = new string('A', cogTagNoteMaxLength + 1);
                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }
        }

        [Fact]
        [AutoRollback]
        public void Inserting_entity_that_references_itself_in_one_to_one_relationship_throws()
        {
            using (var context = new GearsOfWarContext())
            {
                var hammerOfDawn = new HeavyWeapon
                    {
                        Name = "Hammer of Dawn",
                        Overheats = false,
                    };

                hammerOfDawn.SynergyWith = hammerOfDawn;
                context.Weapons.Add(hammerOfDawn);

                Assert.Throws<DbUpdateException>(() => context.SaveChanges())
                      .InnerException.ValidateMessage(typeof(DbContext).Assembly, "Update_ConstraintCycle", null);
            }
        }

        [Fact]
        [AutoRollback]
        public void Inserting_entity_that_references_itself_in_one_to_many_relationship_works()
        {
            using (var context = new GearsOfWarContext())
            {
                var squad = new Squad
                    {
                        Name = "One Man Squad",
                    };

                var tag = new CogTag
                    {
                        Id = Guid.NewGuid(),
                        Note = "Tag",
                    };

                var oneManArmy = new Gear
                    {
                        FullName = "One Man Army",
                        Nickname = "OMA",
                        Rank = MilitaryRank.Private,
                        Squad = squad,
                        Tag = tag,
                    };

                oneManArmy.Reports = new List<Gear> { oneManArmy };

                context.Gears.Add(oneManArmy);
                context.SaveChanges();

                var oma = context.Gears.Where(g => g.Nickname == "OMA").Include(g => g.Reports).Single();
                Assert.Same(oma, oma.Reports.Single());
            }
        }

        [Fact]
        [AutoRollback]
        public void Inserting_entities_that_both_reference_each_other_throws()
        {
            using (var context = new GearsOfWarContext())
            {
                var snub = new StandardWeapon
                    {
                        Name = "Snub",
                        Specs = new WeaponSpecification
                            {
                                AmmoPerClip = 12,
                                ClipsCount = 11,
                            }
                    };

                var sawedoff = new StandardWeapon
                    {
                        Name = "Sawed-Off Shotgun",
                        Specs = new WeaponSpecification
                            {
                                AmmoPerClip = 1,
                                ClipsCount = 6,
                            }
                    };

                snub.SynergyWith = sawedoff;
                sawedoff.SynergyWith = snub;

                context.Weapons.Add(snub);

                Assert.Throws<DbUpdateException>(() => context.SaveChanges())
                      .InnerException.ValidateMessage(typeof(DbContext).Assembly, "Update_ConstraintCycle", null);
            }
        }

        [Fact]
        [AutoRollback]
        public void Inserting_entities_that_reference_themselves_in_a_cycle_throws()
        {
            using (var context = new GearsOfWarContext())
            {
                var snub = new StandardWeapon
                    {
                        Name = "Snub",
                        Specs = new WeaponSpecification
                            {
                                AmmoPerClip = 12,
                                ClipsCount = 11,
                            }
                    };

                var sawedoff = new StandardWeapon
                    {
                        Name = "Sawed-Off Shotgun",
                        Specs = new WeaponSpecification
                            {
                                AmmoPerClip = 1,
                                ClipsCount = 6,
                            }
                    };

                var longshot = new StandardWeapon
                    {
                        Name = "Longshot",
                        Specs = new WeaponSpecification
                            {
                                AmmoPerClip = 1,
                                ClipsCount = 24,
                            }
                    };

                snub.SynergyWith = sawedoff;
                sawedoff.SynergyWith = longshot;
                longshot.SynergyWith = snub;

                context.Weapons.Add(snub);

                Assert.Throws<DbUpdateException>(() => context.SaveChanges())
                      .InnerException.ValidateMessage(typeof(DbContext).Assembly, "Update_ConstraintCycle", null);
            }
        }

        [Fact]
        [AutoRollback]
        public void Insert_dependant_entity_without_required_principal_throws()
        {
            using (var context = new GearsOfWarContext())
            {
                var gearWithoutTag = new Gear
                    {
                        Nickname = "Stranded",
                        FullName = "John Doe",
                        Rank = MilitaryRank.Private,
                        Squad = context.Squads.First(),
                    };

                context.Gears.Add(gearWithoutTag);

                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }
        }

        [Fact]
        [AutoRollback]
        public void Insert_principal_entity_without_required_dependant_does_not_throw()
        {
            using (var context = new GearsOfWarContext())
            {
                var nobodysTag = new CogTag
                    {
                        Id = Guid.NewGuid(),
                        Note = "Owner Unknown",
                    };

                context.Tags.Add(nobodysTag);
                context.SaveChanges();

                Assert.True(context.Tags.Any(t => t.Note == "Owner Unknown"));
            }
        }

        [Fact]
        [AutoRollback]
        public void Same_entity_with_one_to_one_relationship_attached_to_two_different_entities_throw_on_insert()
        {
            using (var context = new GearsOfWarContext())
            {
                var tag = new CogTag
                    {
                        Id = Guid.NewGuid(),
                        Note = "Who's tag is it?",
                    };

                var gear1 = new Gear
                    {
                        Nickname = "Gear1",
                        FullName = "Gear1",
                        Rank = MilitaryRank.Private,
                        Squad = context.Squads.First(),
                        Tag = tag,
                    };

                var gear2 = new Gear
                    {
                        Nickname = "Gear2",
                        FullName = "Gear2",
                        Rank = MilitaryRank.Private,
                        Squad = context.Squads.First(),
                        Tag = tag,
                    };

                context.Gears.AddRange(new[] { gear1, gear2 });
                Assert.Throws<DbUpdateException>(() => context.SaveChanges());
            }
        }

        [Fact]
        [AutoRollback]
        public void Cascade_delete_works_properly_on_one_to_many_relationship()
        {
            using (var context = new GearsOfWarContext())
            {
                var gearsBefore = context.Gears.Count();
                var gear1 = new Gear
                    {
                        FullName = "Gear1",
                        Nickname = "Gear1",
                        Tag = new CogTag
                            {
                                Id = Guid.NewGuid(),
                                Note = "Tag1",
                            },
                    };

                var gear2 = new Gear
                    {
                        FullName = "Gear2",
                        Nickname = "Gear2",
                        Tag = new CogTag
                            {
                                Id = Guid.NewGuid(),
                                Note = "Tag2",
                            },
                    };

                var squad = new Squad
                    {
                        Name = "Charlie",
                        Members = new List<Gear> { gear1, gear2 },
                    };

                context.Squads.Add(squad);
                context.SaveChanges();

                var gearsAfterAdd = context.Gears.Count();

                context.Squads.Remove(squad);
                context.SaveChanges();

                var gearsAfterRemove = context.Gears.Count();

                Assert.Equal(gearsBefore, gearsAfterRemove);
                Assert.Equal(gearsBefore + 2, gearsAfterAdd);
            }
        }

        [Fact]
        [AutoRollback]
        public void Saving_null_compex_type_property_throws()
        {
            using (var context = new GearsOfWarContext())
            {
                var secretWeapon = new StandardWeapon
                    {
                        Name = "Top Secret",
                        Specs = null,
                    };

                context.Weapons.Add(secretWeapon);

                Assert.Throws<DbUpdateException>(() => context.SaveChanges())
                      .ValidateMessage(typeof(DbContext).Assembly, "Update_NullValue", null, "Specs");
            }
        }

        [Fact]
        [AutoRollback]
        public void Modifying_identity_generated_key_throws()
        {
            using (var context = new GearsOfWarContext())
            {
                var tag = new CogTag
                    {
                        Id = Guid.NewGuid(),
                        Note = "Some Note",
                    };

                context.Tags.Add(tag);
                context.SaveChanges();
                tag.Id = Guid.NewGuid();

                Assert.Throws<InvalidOperationException>(() => context.SaveChanges())
                      .ValidateMessage(typeof(DbContext).Assembly, "ObjectStateEntry_CannotModifyKeyProperty", null, "Id");
            }
        }

        [Fact]
        [AutoRollback]
        public void Modifying_non_generated_key_throws()
        {
            using (var context = new GearsOfWarContext())
            {
                var squad = new Squad
                    {
                        Id = 10,
                        Name = "Lima",
                    };

                context.Squads.Add(squad);
                context.SaveChanges();
                squad.Id = 20;

                Assert.Throws<InvalidOperationException>(() => context.SaveChanges())
                      .ValidateMessage(typeof(DbContext).Assembly, "ObjectStateEntry_CannotModifyKeyProperty", null, "Id");
            }
        }

        [Fact]
        [AutoRollback]
        public void Modifying_identity_non_key_throws()
        {
            using (var context = new GearsOfWarContext())
            {
                var squad = new Squad
                    {
                        Id = 10,
                        Name = "Lima",
                    };

                context.Squads.Add(squad);
                context.SaveChanges();
                var squadInternalNumber = squad.InternalNumber;
                squad.InternalNumber = squadInternalNumber + 1;

                Assert.Throws<DbUpdateException>(() => context.SaveChanges())
                      .InnerException.InnerException.ValidateMessage(
                          typeof(DbContext).Assembly,
                          "Update_ModifyingIdentityColumn",
                          null,
                          "Identity",
                          "InternalNumber",
                          "CodeFirstDatabaseSchema.Squad");
            }
        }

        [Fact]
        [AutoRollback]
        public void Optimistic_concurrency_error_on_property_update_previously_modified_entity_when_using_timestamp()
        {
            using (var context = new GearsOfWarContext())
            {
                using (var context2 = new GearsOfWarContext())
                {
                    var gnasher = context.Weapons.Where(w => w.Name == "Gnasher").Single();
                    var gnasher2 = context2.Weapons.Where(w => w.Name == "Gnasher").Single();

                    gnasher.Name = "Gnasher Mk 2";
                    context.SaveChanges();

                    gnasher2.Name = "Sawed-off";
                    Assert.Throws<DbUpdateConcurrencyException>(() => context2.SaveChanges())
                          .ValidateMessage(typeof(DbContext).Assembly, "Update_ConcurrencyError", null);
                }
            }
        }

        [Fact]
        [AutoRollback]
        public void Optimistic_concurrency_error_attempting_to_delete_previously_modified_entity_when_using_timestamp()
        {
            using (var context = new GearsOfWarContext())
            {
                using (var context2 = new GearsOfWarContext())
                {
                    var troika = new HeavyWeapon
                        {
                            Name = "Troika",
                            Overheats = true,
                        };

                    context.Weapons.Add(troika);
                    context.SaveChanges();
                    var troika2 = context2.Weapons.Where(w => w.Name == "Troika").Single();

                    troika.Overheats = false;
                    context.SaveChanges();

                    context2.Weapons.Remove(troika2);
                    Assert.Throws<DbUpdateConcurrencyException>(() => context2.SaveChanges())
                          .ValidateMessage(typeof(DbContext).Assembly, "Update_ConcurrencyError", null);
                }
            }
        }

        [Fact]
        [AutoRollback]
        public void Optimistic_concurrency_error_attempting_to_modify_previously_deleted_entity_when_using_timestamp()
        {
            using (var context = new GearsOfWarContext())
            {
                using (var context2 = new GearsOfWarContext())
                {
                    var troika = new HeavyWeapon
                        {
                            Name = "Troika",
                            Overheats = true,
                        };

                    context.Weapons.Add(troika);
                    context.SaveChanges();
                    var troika2 = context2.Weapons.OfType<HeavyWeapon>().Where(w => w.Name == "Troika").Single();

                    context.Weapons.Remove(troika);
                    context.SaveChanges();

                    troika2.Overheats = false;
                    Assert.Throws<DbUpdateConcurrencyException>(() => context2.SaveChanges())
                          .ValidateMessage(typeof(DbContext).Assembly, "Update_ConcurrencyError", null);
                }
            }
        }

        [Fact]
        [AutoRollback]
        public void Optimistic_concurrency_error_attempting_to_update_previously_modified_entity_when_using_property_as_concurrency_token()
        {
            using (var context = new GearsOfWarContext())
            {
                using (var context2 = new GearsOfWarContext())
                {
                    var baird = context.Gears.Where(g => g.Nickname == "Baird").Single();
                    var baird2 = context2.Gears.Where(g => g.Nickname == "Baird").Single();

                    baird.Rank = MilitaryRank.Lieutenant;
                    context.SaveChanges();

                    baird2.Rank = MilitaryRank.Private;
                    Assert.Throws<DbUpdateConcurrencyException>(() => context2.SaveChanges())
                          .ValidateMessage(typeof(DbContext).Assembly, "Update_ConcurrencyError", null);
                }
            }
        }

        [Fact]
        [AutoRollback]
        public void Optimistic_concurrency_error_attempting_to_delete_previously_modified_entity_when_using_property_as_concurrency_token()
        {
            using (var context = new GearsOfWarContext())
            {
                using (var context2 = new GearsOfWarContext())
                {
                    var baird = context.Gears.Where(g => g.Nickname == "Baird").Single();
                    var baird2 = context2.Gears.Where(g => g.Nickname == "Baird").Single();

                    baird.Rank = MilitaryRank.Lieutenant;
                    context.SaveChanges();

                    context2.Gears.Remove(baird2);
                    Assert.Throws<DbUpdateException>(() => context2.SaveChanges()).InnerException
                          .ValidateMessage(typeof(DbContext).Assembly, "Update_ConcurrencyError", null);
                }
            }
        }

        [Fact]
        [AutoRollback]
        public void Optimistic_concurrency_error_when_updating_previously_modified_reference()
        {
            using (var context = new GearsOfWarContext())
            {
                using (var context2 = new GearsOfWarContext())
                {
                    var jacinto = context.Cities.Where(c => c.Name == "Jacinto").Single();
                    var ephyra2 = context2.Cities.Where(c => c.Name == "Ephyra").Single();

                    var cole = context.Gears.Where(g => g.Nickname == "Cole Train").Single();
                    var cole2 = context2.Gears.Where(g => g.Nickname == "Cole Train").Single();

                    cole.CityOfBirth = jacinto;
                    context.SaveChanges();

                    cole2.CityOfBirth = ephyra2;

                    Assert.Throws<DbUpdateException>(() => context2.SaveChanges())
                          .InnerException.ValidateMessage(typeof(DbContext).Assembly, "Update_ConcurrencyError", null);
                }
            }
        }

        [Fact]
        [AutoRollback]
        public void Optimisic_concurrency_error_when_deleting_previously_modified_reference()
        {
            using (var context = new GearsOfWarContext())
            {
                using (var context2 = new GearsOfWarContext())
                {
                    var jacinto = context.Cities.Where(c => c.Name == "Jacinto").Single();

                    var cole = context.Gears.Where(g => g.Nickname == "Cole Train").Include(g => g.CityOfBirth).Single();
                    var cole2 = context2.Gears.Where(g => g.Nickname == "Cole Train").Include(g => g.CityOfBirth).Single();

                    cole.CityOfBirth = jacinto;
                    context.SaveChanges();

                    cole2.CityOfBirth = null;

                    Assert.Throws<DbUpdateException>(() => context2.SaveChanges())
                          .InnerException.ValidateMessage(typeof(DbContext).Assembly, "Update_ConcurrencyError", null);
                }
            }
        }

        [Fact]
        [AutoRollback]
        public void Insert_update_and_delete_entity_with_enum_property()
        {
            using (var context = new GearsOfWarContext())
            {
                var taisTag = new CogTag
                    {
                        Id = Guid.NewGuid(),
                        Note = "Tai's tag",
                    };

                var tai = new Gear
                    {
                        FullName = "Tai Kaliso",
                        Nickname = "Tai",
                        Squad = context.Squads.First(),
                        Rank = MilitaryRank.Corporal,
                        Tag = taisTag,
                    };

                context.Gears.Add(tai);
                context.SaveChanges();
                Assert.True(context.Gears.Where(g => g.Nickname == "Tai" && g.Rank == MilitaryRank.Corporal).Any());

                tai.Rank = MilitaryRank.Sergeant;
                context.SaveChanges();
                var taisNewRank = context.Gears.Where(g => g.Nickname == "Tai").Select(g => g.Rank).Single();
                Assert.Equal(MilitaryRank.Sergeant, taisNewRank);

                context.Gears.Remove(tai);
                context.SaveChanges();
                Assert.False(context.Gears.Where(g => g.Nickname == "Tai").Any());
            }
        }

        [Fact]
        [AutoRollback]
        public void Insert_update_delete_entity_with_spatial_property()
        {
            using (var context = new GearsOfWarContext())
            {
                var GeographySrid = 4326;
                var halvoBay = new City
                    {
                        Name = "Halvo Bay",
                        Location = DbGeography.FromText("POINT(10 10)", GeographySrid),
                    };

                context.Cities.Add(halvoBay);
                context.SaveChanges();
                Assert.True(context.Cities.Where(c => c.Name == "Halvo Bay").Any());

                halvoBay.Location = DbGeography.FromText("POINT(20 20)", GeographySrid);
                context.SaveChanges();
                var halvoBaysNewLocation = context.Cities.Where(g => g.Name == "Halvo Bay").Select(g => g.Location).Single();
                Assert.True(halvoBaysNewLocation.SpatialEquals(DbGeography.FromText("POINT(20 20)", GeographySrid)));

                context.Cities.Remove(halvoBay);
                context.SaveChanges();
                Assert.False(context.Cities.Where(g => g.Name == "Halvo Bay").Any());
            }
        }
    }
}
