﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.CodeFirst
{
    using System.Collections.Generic;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.TestHelpers;
    using System.Data.Entity.TestModels.ArubaModel;
    using System.Data.Entity.TestModels.FantasyModel;
    using System.Data.Entity.TestModels.GearsOfWarModel;
    using System.Linq;
    using System.Transactions;
    using Xunit;

    public class ModificationFunctionsEndToEndTests : FunctionalTestBase
    {
        public ModificationFunctionsEndToEndTests()
        {
            // force database initialization, we don't want this to happen inside transaction
            using (var context = new GearsOfWarStoredProceduresContext())
            {
                context.Database.Delete();
                context.Weapons.Count();
            }

            using (var context = new ArubaStoredProceduresContext())
            {
                context.Database.Delete();
                context.Configs.Count();
            }

            using (var context = new FantasyStoredProceduresContext())
            {
                context.Database.Delete();
                context.Creatures.Count();
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Update_GearsOfWar_entities_using_stored_procedures()
        {
            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    using (new TransactionScope())
                    {
                        using (var context = new GearsOfWarStoredProceduresContext())
                        {
                            var city = context.Cities.OrderBy(c => c.Name).First();
#if NETFRAMEWORK
                            city.Location = DbGeography.FromText("POINT(12 23)", DbGeography.DefaultCoordinateSystemId);
#endif
                            context.SaveChanges();

                            var tag = context.Tags.OrderBy(t => t.Id).First();
                            tag.Note = "Modified Note";
                            context.SaveChanges();

                            var gear = context.Gears.OrderBy(g => g.Nickname).ThenBy(g => g.SquadId).First();
                            gear.Rank = MilitaryRank.General;
                            context.SaveChanges();

                            var squad = context.Squads.OrderBy(s => s.Id).First();
                            squad.Name = "Modified Name";
                            context.SaveChanges();

                            var weapon = context.Weapons.OrderBy(w => w.Id).First();
                            weapon.Name = "Modified Name";
                            context.SaveChanges();
                        }

                        using (var context = new GearsOfWarStoredProceduresContext())
                        {
                            var city = context.Cities.OrderBy(c => c.Name).First();
                            var tag = context.Tags.OrderBy(t => t.Id).First();
                            var gear = context.Gears.OrderBy(g => g.Nickname).ThenBy(g => g.SquadId).First();
                            var squad = context.Squads.OrderBy(s => s.Id).First();
                            var weapon = context.Weapons.OrderBy(w => w.Id).First();

#if NETFRAMEWORK
                            Assert.Equal(12, city.Location.Longitude);
                            Assert.Equal(23, city.Location.Latitude);
#endif
                            Assert.Equal("Modified Note", tag.Note);
                            Assert.Equal(MilitaryRank.General, gear.Rank);
                            Assert.Equal("Modified Name", squad.Name);
                            Assert.Equal("Modified Name", weapon.Name);
                        }
                    }
                });
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Update_Aruba_entities_using_stored_procedures()
        {
            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    using (new TransactionScope())
                    {
                        using (var context = new ArubaStoredProceduresContext())
                        {
                            var allTypes = context.AllTypes.OrderBy(a => a.c1_int).First();
                            var bug = context.Bugs.OrderBy(a => a.Id).First();
                            var config = context.Configs.OrderBy(a => a.Id).First();
                            var owner = context.Owners.OrderBy(o => o.Id).First();
                            var person = context.People.OrderBy(p => p.Id).First();
                            var run = context.Runs.OrderBy(r => r.Id).First();
                            var task = context.Tasks.OrderBy(t => t.Id).ThenBy(t => t.Name).First();

                            allTypes.c10_float = 12.5;
                            allTypes.c13_varchar_512_ = "Wenn ist das Nunstück git und Slotermeyer?";
                            allTypes.c28_date = new DateTime(2012, 12, 22);
                            context.SaveChanges();

                            bug.Number = 42;
                            bug.Resolution = ArubaBugResolution.WontFix;
                            context.SaveChanges();

                            config.Lang = "Hungarian";
                            config.OS = "Win8";
                            context.SaveChanges();

                            owner.FirstName = "Baltazar";
                            owner.LastName = "Gabka";
                            context.SaveChanges();

                            person.Name = "Don Pedro";
                            context.SaveChanges();

                            run.Name = "5K";
                            context.SaveChanges();

                            task.TaskInfo = new ArubaTaskInfo
                            {
                                Failed = 12,
                                Improvements = 34,
                                Investigates = 56,
                                Passed = 78,
                            };

                            context.SaveChanges();
                        }

                        using (var context = new ArubaStoredProceduresContext())
                        {
                            var allTypes = context.AllTypes.OrderBy(a => a.c1_int).First();
                            var bug = context.Bugs.OrderBy(a => a.Id).First();
                            var config = context.Configs.OrderBy(a => a.Id).First();
                            var owner = context.Owners.OrderBy(o => o.Id).First();
                            var person = context.People.OrderBy(p => p.Id).First();
                            var run = context.Runs.OrderBy(r => r.Id).First();
                            var task = context.Tasks.OrderBy(t => t.Id).ThenBy(t => t.Name).First();

                            Assert.Equal(12.5, allTypes.c10_float);
                            Assert.Equal("Wenn ist das Nunstück git und Slotermeyer?", allTypes.c13_varchar_512_);
                            Assert.Equal(new DateTime(2012, 12, 22), allTypes.c28_date);
                            Assert.Equal(42, bug.Number);
                            Assert.Equal(ArubaBugResolution.WontFix, bug.Resolution);
                            Assert.Equal("Hungarian", config.Lang);
                            Assert.Equal("Win8", config.OS);

                            Assert.Equal("Baltazar", owner.FirstName);
                            Assert.Equal("Gabka", owner.LastName);
                            Assert.Equal("Don Pedro", person.Name);
                            Assert.Equal("5K", run.Name);
                            Assert.Equal(12, task.TaskInfo.Failed);
                            Assert.Equal(34, task.TaskInfo.Improvements);
                            Assert.Equal(56, task.TaskInfo.Investigates);
                            Assert.Equal(78, task.TaskInfo.Passed);
                        }
                    }
                });
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Update_Fantasy_entities_using_stored_procedures()
        {
            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    using (new TransactionScope())
                    {
                        using (var context = new FantasyStoredProceduresContext())
                        {
                            var city = context.Cities.OrderBy(c => c.Id).First();
                            var creature = context.Creatures.OrderBy(c => c.Id).First();
                            var province = context.Provinces.OrderBy(h => h.Id).First();
                            var npc = context.Npcs.OrderBy(n => n.Id).First();
                            var spell = context.Spells.OrderBy(s => s.Id).First();

                            city.Name = "Changed City";
                            creature.Details.Attributes.Mana = 123;
#if NETFRAMEWORK
                            province.Shape = DbGeometry.FromText("POINT(23 45)", DbGeometry.DefaultCoordinateSystemId);
#endif
                            npc.Name = "Changed NPC Name";
                            spell.MagickaCost = 166;

                            context.SaveChanges();
                        }

                        using (var context = new FantasyStoredProceduresContext())
                        {
                            var city = context.Cities.OrderBy(c => c.Id).First();
                            var creature = context.Creatures.OrderBy(c => c.Id).First();
                            var province = context.Provinces.OrderBy(h => h.Id).First();
                            var npc = context.Npcs.OrderBy(n => n.Id).First();
                            var spell = context.Spells.OrderBy(s => s.Id).First();

                            Assert.Equal("Changed City", city.Name);
                            Assert.Equal(123, creature.Details.Attributes.Mana);
#if NETFRAMEWORK
                            Assert.Equal(23, province.Shape.XCoordinate);
                            Assert.Equal(45, province.Shape.YCoordinate);
#endif
                            Assert.Equal("Changed NPC Name", npc.Name);
                            Assert.Equal(166, spell.MagickaCost);
                        }
                    }
                });
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Delete_GearsOfWar_entities_using_stored_procedures()
        {
            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    using (new TransactionScope())
                    {
                        using (var context = new GearsOfWarStoredProceduresContext())
                        {
                            var cities = context.Cities.ToList();
                            var gears = context.Gears.ToList();
                            var squads = context.Squads.ToList();
                            var tags = context.Tags.ToList();
                            var weapons = context.Weapons.ToList();

                            context.Cities.RemoveRange(cities);
                            context.Gears.RemoveRange(gears);
                            context.Squads.RemoveRange(squads);
                            context.Tags.RemoveRange(tags);
                            context.Weapons.RemoveRange(weapons);
                            context.SaveChanges();
                        }

                        using (var context = new GearsOfWarStoredProceduresContext())
                        {
                            Assert.Equal(0, context.Cities.Count());
                            Assert.Equal(0, context.Gears.Count());
                            Assert.Equal(0, context.Squads.Count());
                            Assert.Equal(0, context.Tags.Count());
                            Assert.Equal(0, context.Weapons.Count());
                        }
                    }
                });
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Update_Many_to_Many_relationship_using_stored_procedures()
        {
            ExtendedSqlAzureExecutionStrategy.ExecuteNew(
                () =>
                {
                    using (new TransactionScope())
                    {
                        List<int?> usedWeaponIds;
                        List<int?> unusedWeaponIds;
                        using (var context = new GearsOfWarStoredProceduresContext())
                        {
                            var gear = context.Gears.OrderBy(g => g.Nickname).ThenBy(g => g.Squad).First();
                            usedWeaponIds = gear.Weapons.Select(w => w.Id).ToList();

                            var unusedWeapons = context.Weapons
                                .Where(w => !usedWeaponIds.Contains(w.Id)).ToList();

                            unusedWeaponIds = unusedWeapons.Select(w => w.Id).ToList();
                            gear.Weapons = unusedWeapons;
                            context.SaveChanges();
                        }

                        using (var context = new GearsOfWarStoredProceduresContext())
                        {
                            var gear = context.Gears.OrderBy(g => g.Nickname).ThenBy(g => g.Squad).First();
                            Assert.True(gear.Weapons.All(w => unusedWeaponIds.Contains(w.Id)));
                        }
                    }
                });
        }
    }
}
