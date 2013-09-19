// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.FantasyModel
{
    using System.Collections.Generic;
    using System.Data.Entity.Spatial;
    using System.Linq;

    public class FantasyDatabaseSeeder
    {
        public void Seed(FantasyContext context)
        {
            var creatures = InitializeCreatures();
            context.Creatures.AddRange(creatures);
            context.SaveChanges();

            var skills = InitializeSkills();
            context.Skills.AddRange(skills);
            context.SaveChanges();

            var races = InitializeRaces(skills);
            context.Races.AddRange(races);
            context.SaveChanges();

            var illusionPerks = InitializerIllusionPerks(skills.Single(s => s.Name == "Illusion"));
            context.Perks.AddRange(illusionPerks);
            context.SaveChanges();

            var spells = InitializeSpells();
            context.Spells.AddRange(spells);
            context.SaveChanges();

            var provinces = InitializeProvinces();
            InitializeLandmarksAndTowers(context, provinces);
            var cities = InitializeCities(provinces);
            var npcs = InitializeNpcs(races);
            var buildings = InitializeBuildings(cities, npcs);
            context.Provinces.AddRange(provinces);
            context.Cities.AddRange(cities);
            context.Npcs.AddRange(npcs);
            context.Buildings.AddRange(buildings);
            context.SaveChanges();
        }

        private IEnumerable<Creature> InitializeCreatures()
        {
            var rabbit = new Herbivore
                {
                    Id = 1,
                    FavoritePlant = "Carrot",
                    TransmitsDesease = false,
                    Details = new CreatureDetails
                        {
                            Attributes = new Attributes { Health = 5, Stamina = 25, Mana = -25 },
                            Name = "Rabbit",
                        }
                };

            var cow = new Herbivore
                {
                    Id = 2,
                    FavoritePlant = "Grass",
                    TransmitsDesease = false,
                    Details = new CreatureDetails
                        {
                            Attributes = new Attributes { Health = 87, Stamina = 33, Mana = 0 },
                            Name = "Cow",
                        }
                };

            var deer = new Herbivore
                {
                    Id = 3,
                    FavoritePlant = "Acorn",
                    TransmitsDesease = true,
                    Details = new CreatureDetails
                        {
                            Attributes = new Attributes { Health = 25, Stamina = 25, Mana = 0 },
                            Name = "Deer",
                        }
                };

            var wolf = new Carnivore
                {
                    Id = 4,
                    Eats = new[] { rabbit, deer },
                    TransmitsDesease = true,
                    Details = new CreatureDetails
                        {
                            Attributes = new Attributes { Health = 15, Stamina = 0, Mana = 0 },
                            Name = "Wolf",
                        }
                };

            var tiger = new Carnivore
                {
                    Id = 5,
                    Eats = new Animal[] { rabbit, deer, cow, wolf },
                    TransmitsDesease = true,
                    Details = new CreatureDetails
                        {
                            Attributes = new Attributes { Health = 150, Stamina = 225, Mana = 0 },
                            Name = "Tiger",
                        }
                };

            var omnomnomnivore = new Omnivore
                {
                    Id = 6,
                    FavoritePlant = "French Fries",
                    Details = new CreatureDetails
                        {
                            Attributes = new Attributes { Health = 1000, Stamina = 5, Mana = 0 },
                            Name = "Omnomnomnivore",
                        }
                };

            omnomnomnivore.Eats = new Animal[] { rabbit, cow, deer, wolf, tiger, omnomnomnivore };

            var wraith = new Monster
                {
                    Id = 7,
                    Details = new CreatureDetails
                        {
                            Attributes = new Attributes { Health = 193, Stamina = 227, Mana = 50 },
                            Name = "Wraith",
                        }
                };

            var troll = new Troll
                {
                    Id = 8,
                    Discriminator = 22,
                    Details = new CreatureDetails
                        {
                            Attributes = new Attributes { Health = 460, Stamina = 480, Mana = 0 },
                            Name = "Frost Troll",
                        }
                };

            return new Creature[] { rabbit, cow, deer, wolf, tiger, omnomnomnivore, wraith, troll };
        }

        private IEnumerable<Skill> InitializeSkills()
        {
            var illusion = new Skill
            {
                Archetype = SkillArchetype.Mage,
                Ordinal = 0,
                Name = "Illusion",
            };

            var conjuration = new Skill
            {
                Archetype = SkillArchetype.Mage,
                Ordinal = 1,
                Name = "Conjuration",
            };

            var smithing = new Skill
            {
                Archetype = SkillArchetype.Warrior,
                Ordinal = 0,
                Name = "Smithing",
            };

            var heavyArmor = new Skill
            {
                Archetype = SkillArchetype.Warrior,
                Ordinal = 1,
                Name = "Heavy Armor",
            };

            var block = new Skill
            {
                Archetype = SkillArchetype.Warrior,
                Ordinal = 2,
                Name = "Block",
            };

            var archery = new Skill
            {
                Archetype = SkillArchetype.Thief,
                Ordinal = 0,
                Name = "Archery",
            };

            return new[] { illusion, conjuration, smithing, heavyArmor, block, archery };
        }

        private IEnumerable<Race> InitializeRaces(IEnumerable<Skill> skills)
        {
            var illusion = skills.Single(s => s.Name == "Illusion");
            var conjuration = skills.Single(s => s.Name == "Conjuration");
            var archery = skills.Single(s => s.Name == "Archery");
            var heavyArmor = skills.Single(s => s.Name == "Heavy Armor");
            var block = skills.Single(s => s.Name == "Block");

            var elf = new Race
            {
                RaceName = "Elf",
                SkillBonuses = new[] { illusion, conjuration },
            };

            var orc = new Race
            {
                RaceName = "Orc",
                SkillBonuses = new[] { heavyArmor, block },
            };

            var human = new Race
            {
                RaceName = "Human",
                SkillBonuses = new[] { archery, illusion },
            };

            return new[] { elf, orc, human };
        }

        private IEnumerable<Perk> InitializerIllusionPerks(Skill illusion)
        {
            // set via FK
            var perk1 = new Perk
            {
                Name = "Perk1",
                SkillArchetype = illusion.Archetype,
                SkillOrdinal = illusion.Ordinal,
            };

            var perk2 = new Perk
            {
                Name = "Perk2",
                RequiredSkillValue = 20,
                Skill = illusion,
            };

            var perk3 = new Perk
            {
                Name = "Perk3",
                RequiredSkillValue = 20,
                Skill = illusion,
            };

            var perk4 = new Perk
            {
                Name = "Perk4",
                RequiredSkillValue = 25,
                Skill = illusion,
            };

            var perk5 = new Perk
            {
                Name = "Perk5",
                RequiredSkillValue = 100,
                Skill = illusion,
            };

            perk1.RequiredBy = new[] { perk2, perk4, perk3 };
            perk2.RequiredPerks = new[] { perk1 };
            perk4.RequiredPerks = new[] { perk1 };
            perk4.RequiredBy = new[] { perk5 };
            perk3.RequiredPerks = new[] { perk1 };
            perk3.RequiredBy = new[] { perk5 };
            perk5.RequiredPerks = new[] { perk4, perk3 };

            return new[] { perk1, perk2, perk4, perk3, perk5 };
        }

        private IEnumerable<Spell> InitializeSpells()
        {
            var fireball = new CombatSpell
            {
                Damage = 40,
                DamageType = DamageType.Fire,
                MagickaCost = 133,
            };

            var invisibility = new SupportSpell
            {
                Description = "Makes caster invisible",
                MagickaCost = 334,
            };

            var silentStep = new SupportSpell
            {
                Description = "Makes caster's moves silent",
                MagickaCost = 144,
            };

            var detectLife = new SupportSpell
            {
                Description = "Detects living creatures",
                MagickaCost = 100,
            };

            var healing = new SupportSpell
            {
                Description = "Heals caster",
                MagickaCost = 12,
            };

            var manaSyphon = new SupportSpell
            {
                Description = "Converts health to mana",
                MagickaCost = -25,
            };

            // non symetric relationship
            invisibility.SynergyWith = new[] { silentStep, detectLife };
            silentStep.SynergyWith = new[] { invisibility };

            healing.SynergyWith = new[] { manaSyphon };
            manaSyphon.SynergyWith = new[] { healing };

            return new Spell[] { fireball, invisibility, silentStep, detectLife, healing, manaSyphon };
        }

        private IEnumerable<Province> InitializeProvinces()
        {
            var province1 = new Province
                {
                    Id = 1,
                    Name = "Province1",
                    Shape = DbGeometry.FromText("POINT(1 1)", DbGeometry.DefaultCoordinateSystemId),
                };

            var province2 = new Province
                {
                    Id = 2,
                    Name = "Province2",
                    Shape = DbGeometry.FromText("POINT(2 2)", DbGeometry.DefaultCoordinateSystemId),
                };

            return new[] { province1, province2 };
        }

        private void InitializeLandmarksAndTowers(FantasyContext context, IEnumerable<Province> provinces)
        {
            var province1 = provinces.Where(h => h.Name == "Province1").Single();
            var province2 = provinces.Where(h => h.Name == "Province2").Single();

            var pilchukSummit = new Landmark
                {
                    Id = 1,
                    LocatedIn = province1,
                };

            var grayLighthouse = new Landmark
                {
                    Id = 2,
                    LocatedIn = province2,
                };

            var highTower = new Tower
                {
                    Id = 3,
                    LocatedIn = province1,
                };

            var stoneTower = new Tower
                {
                    Id = 4,
                    LocatedIn = province2,
                };

            pilchukSummit.MatchingTower = highTower;
            grayLighthouse.MatchingTower = stoneTower;

            context.Landmarks.AddRange(new[] { pilchukSummit, grayLighthouse });
            context.Towers.AddRange(new[] { highTower, stoneTower });
        }

        private IEnumerable<City> InitializeCities(IEnumerable<Province> provinces)
        {
            var province1 = provinces.Where(h => h.Name == "Province1").Single();
            var province2 = provinces.Where(h => h.Name == "Province2").Single();
            
            // set just nav prop
            var city1 = new City
                {
                    Id = 1,
                    Name = "City1",
                    Province = province1,
                };

            // set just FK
            var city2 = new City
                {
                    Id = 2,
                    Name = "City2",
                    ProvinceId = province1.Id,
                };

            // set both nav prop and FK
            var city3 = new City
                {
                    Id = 3,
                    Name = "City3",
                    Province = province2,
                    ProvinceId = province2.Id,
                };

            return new[] { city1, city2, city3 };
        }

        private IEnumerable<Npc> InitializeNpcs(IEnumerable<Race> races)
        {
            var elf = races.Where(r => r.RaceName == "Elf").Single();
            var orc = races.Where(r => r.RaceName == "Orc").Single();
            var human = races.Where(r => r.RaceName == "Human").Single();
            
            var alice = new Npc
                {
                    Name = "Alice",
                    Race = elf,
                };

            var bob = new Npc
                {
                    Name = "Bob",
                    Race = orc,
                };

            var mallory = new Npc
                {
                    Name = "Mallory",
                    Race = human,
                };

            return new[] { alice, bob, mallory };
        }

        private IEnumerable<Building> InitializeBuildings(IEnumerable<City> cities, IEnumerable<Npc> npcs)
        {
            var city1 = cities.Where(c => c.Name == "City1").Single();
            var city3 = cities.Where(c => c.Name == "City3").Single();
            var alice = npcs.Where(n => n.Name == "Alice").Single();
            var bob = npcs.Where(n => n.Name == "Bob").Single();
            var mallory = npcs.Where(n => n.Name == "Mallory").Single();
            var alicesAndBobsHome = new Home
                {
                    Tenants = new[] { alice, bob },
                };

            var mallorysSpyShoppe = new Store
                {
                    Owner = mallory,
                };

            var warehouse = new Building();
            var foreclosureVictim = new Home();

            city1.Buildings = new Building[] { alicesAndBobsHome, mallorysSpyShoppe };
            city3.Buildings = new[] { warehouse, foreclosureVictim };

            return new[] { alicesAndBobsHome, mallorysSpyShoppe, warehouse, foreclosureVictim };
        }
    }
}
