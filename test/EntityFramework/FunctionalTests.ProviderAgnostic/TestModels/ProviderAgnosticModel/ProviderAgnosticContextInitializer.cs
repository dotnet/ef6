// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ProviderAgnosticModel
{
    using System.Collections.Generic;
    using System.Linq;

    public class ProviderAgnosticContextInitializer : DropCreateDatabaseIfModelChanges<ProviderAgnosticContext>
    {
        private int _entitiesCount = 10;

        protected override void Seed(ProviderAgnosticContext context)
        {
            var allTypes = InitializeAllTypes();
            var bugs = InitializeBugs();
            var configs = InitializeConfigs();
            var failures = InitializeFailures();
            var owners = InitializeOwners();
            var runs = InitializeRuns();
            var tasks = InitializeTasks();

            for (var i = 0; i < _entitiesCount; i++)
            {
                bugs[i].Failure = failures[i % 4];
                failures[i % 4].Bugs.Add(bugs[i]);
            }

            for (var i = 0; i < 10; i++)
            {
                for (var j = 0; j < i % 3; j++)
                {
                    configs[i].Failures.Add(failures[(i + j) % 10]);
                }
            }

            for (var i = 0; i < _entitiesCount; i++)
            {
                for (var j = 0; j < i % 2; j++)
                {
                    failures[i].Configs.Add(configs[(i + j) % _entitiesCount]);
                }
            }

            for (var i = 0; i < _entitiesCount; i++)
            {
                runs[i].RunOwner = owners[i];
                for (var j = 0; j < 3; j++)
                {
                    runs[i].Tasks.Add(tasks[(i + j) % _entitiesCount]);
                }
            }

            context.AllTypes.AddRange(allTypes);
            context.Bugs.AddRange(bugs);
            context.Configs.AddRange(configs);
            context.Failures.AddRange(failures);
            context.Owners.AddRange(owners);
            context.Runs.AddRange(runs);
            context.Tasks.AddRange(tasks);
            context.SaveChanges();

            var lancer = new StandardWeapon
            {
                Name = "Lancer",
                Specs = new WeaponSpecification
                {
                    AmmoPerClip = 60,
                    ClipsCount = 8,
                }
            };

            var gnasher = new StandardWeapon
            {
                Name = "Gnasher",
                Specs = new WeaponSpecification
                {
                    AmmoPerClip = 8,
                    ClipsCount = 6,
                },

                SynergyWith = lancer,
            };

            var hammerburst = new StandardWeapon
            {
                Name = "Hammerburst",
                Specs = new WeaponSpecification
                {
                    AmmoPerClip = 20,
                    ClipsCount = 7,
                }
            };

            var markza = new StandardWeapon
            {
                Name = "Markza",
                Specs = new WeaponSpecification
                {
                    AmmoPerClip = 10,
                    ClipsCount = 12,
                },

                SynergyWith = gnasher,
            };

            var mulcher = new HeavyWeapon
            {
                Name = "Mulcher",
                Overheats = true,
            };

            context.Weapons.AddRange(new List<Weapon> { lancer, gnasher, hammerburst, markza, mulcher, });

            var deltaSquad = new Squad
            {
                Id = 1,
                Name = "Delta",
            };

            var kiloSquad = new Squad
            {
                Id = 2,
                Name = "Kilo",
            };

            context.Squads.AddRange(new[] { deltaSquad, kiloSquad });

            var jacinto = new City
            {
                Name = "Jacinto",
            };

            var ephyra = new City
            {
                Name = "Ephyra",
            };

            var hanover = new City
            {
                Name = "Hanover",
            };

            context.Cities.AddRange(new[] { jacinto, ephyra, hanover });

            var marcusTag = new CogTag
            {
                Id = Guid.NewGuid(),
                Note = "Marcus's Tag",
            };

            var domsTag = new CogTag
            {
                Id = Guid.NewGuid(),
                Note = "Dom's Tag",
            };

            var colesTag = new CogTag
            {
                Id = Guid.NewGuid(),
                Note = "Cole's Tag",
            };

            var bairdsTag = new CogTag
            {
                Id = Guid.NewGuid(),
                Note = "Bairds's Tag",
            };

            var paduksTag = new CogTag
            {
                Id = Guid.NewGuid(),
                Note = "Paduk's Tag",
            };

            var kiaTag = new CogTag
            {
                Id = Guid.NewGuid(),
                Note = "K.I.A.",
            };

            context.Tags.AddRange(
                new[] { marcusTag, domsTag, colesTag, bairdsTag, paduksTag, kiaTag });

            var marcus = new Gear
            {
                Nickname = "Marcus",
                FullName = "Marcus Fenix",
                Squad = deltaSquad,
                Rank = MilitaryRank.Sergeant,
                Tag = marcusTag,
                CityOfBirth = jacinto,
                Weapons = new List<Weapon> { lancer, gnasher },
            };

            var dom = new Gear
            {
                Nickname = "Dom",
                FullName = "Dominic Santiago",
                Squad = deltaSquad,
                Rank = MilitaryRank.Corporal,
                Tag = domsTag,
                CityOfBirth = ephyra,
                Weapons = new List<Weapon> { hammerburst, gnasher }
            };

            var cole = new Gear
            {
                Nickname = "Cole Train",
                FullName = "Augustus Cole",
                Squad = deltaSquad,
                Rank = MilitaryRank.Private,
                Tag = colesTag,
                CityOfBirth = hanover,
                Weapons = new List<Weapon> { gnasher, mulcher }
            };

            var baird = new Gear
            {
                Nickname = "Baird",
                FullName = "Damon Baird",
                Squad = deltaSquad,
                Rank = MilitaryRank.Corporal,
                Tag = bairdsTag,
                Weapons = new List<Weapon> { lancer, gnasher }
            };

            var paduk = new Gear
            {
                Nickname = "Paduk",
                FullName = "Garron Paduk",
                Squad = kiloSquad,
                Rank = MilitaryRank.Private,
                Tag = paduksTag,
                Weapons = new List<Weapon> { markza },
            };

            marcus.Reports = new List<Gear> { dom, cole, baird };
            baird.Reports = new List<Gear> { paduk };

            context.Gears.AddRange(new[] { marcus, dom, cole, baird, paduk });
            context.SaveChanges();
        }

        private AllTypes[] InitializeAllTypes()
        {
            var allTypesList = new AllTypes[_entitiesCount];
            for (var i = 0; i < _entitiesCount; i++)
            {
                var allTypes = new AllTypes
                {
                    ByteProperty = (byte)i,
                    BooleanProperty = i % 2 == 0,
                    DateTimeProperty = new DateTime(1990, i % 12 + 1, i % 28 + 1, i % 12, i % 60, i % 60),
                    DecimalProperty = 10 + (decimal)((double)i / 4),
                    DoubleProperty = i + (double)i / 3,
                    FixedLengthBinaryProperty = Enumerable.Repeat<byte>((byte)i, 255).ToArray(),
                    FixedLengthStringProperty = new string((char)(i + 'a'), 255),
                    FixedLengthUnicodeStringProperty = new string((char)(i + 'a'), 255),
                    FloatProperty = (float)i / 3,
                    GuidProperty = new Guid(new string((char)((i % 5) + '0'), 32)),
                    Int16Property = (short)i,
                    Int32Property = i,
                    Int64Property = (long)i * 10,
                    MaxLengthBinaryProperty = Enumerable.Repeat<byte>((byte)i, 15 + i % 8).ToArray(),
                    MaxLengthStringProperty = new string((char)(i + 'A'), 15 + i % 4),
                    MaxLengthUnicodeStringProperty = new string((char)(i + 'A'), 15 + i % 7),
                    TimeSpanProperty = new TimeSpan(10, i % 60 + 1, i % 60 + i),
                    VariableLengthBinaryProperty = Enumerable.Repeat<byte>((byte)i, 1 + i % 7).ToArray(),
                    VariableLengthStringProperty = new string((char)(i + 'a'), i % 8),
                    VariableLengthUnicodeStringProperty = new string((char)(i + 'a'), i),
                };

                allTypesList[i] = allTypes;
            }

            return allTypesList;
        }

        private Bug[] InitializeBugs()
        {
            var bugs = new Bug[_entitiesCount];
            for (var i = 0; i < _entitiesCount; i++)
            {
                var bug = new Bug
                {
                    Comment = "Bug Comment " + i,
                    Number = i,
                    Resolution = (ArubaBugResolution)(i % 5),
                };
                bugs[i] = bug;
            }

            return bugs;
        }

        private Config[] InitializeConfigs()
        {
            var configs = new Config[_entitiesCount];
            for (var i = 0; i < _entitiesCount; i++)
            {
                if (i % 2 == 0)
                {
                    var config = new Config
                    {
                        Arch = "Config Architecture " + i % 3,
                        Lang = "Config Language" + i,
                        OS = "Config Operating System" + i,
                        Failures = new List<Failure>(),
                    };
                    configs[i] = config;
                }
                else
                {
                    var machineConfig = new MachineConfig
                    {
                        Address = new Guid(new string((char)((i % 8) + '0'), 32)),
                        Arch = "Machine Config Architecture " + i,
                        Host = "Machine Config Host " + i,
                        Lang = "Machine Config Language " + i,
                        OS = "Machine Config Operating System " + i % 5,
                        Failures = new List<Failure>(),
                    };
                    configs[i] = machineConfig;
                }
            }

            return configs;
        }

        private Failure[] InitializeFailures()
        {
            var failures = new Failure[_entitiesCount];
            for (var i = 0; i < _entitiesCount; i++)
            {
                var failure = new Failure
                {
                    Changed = new DateTime(2000, i % 12 + 1, i % 28 + 1),
                    Log = "Failure Log " + i,
                    TestCase = "Failure Test Case " + i,
                    TestId = i,
                    Variation = i % 4,
                    Configs = new List<Config>(),
                    Bugs = new List<Bug>(),
                };

                failures[i] = failure;
            }

            return failures;
        }

        private Owner[] InitializeOwners()
        {
            var owners = new Owner[_entitiesCount];
            for (var i = 0; i < _entitiesCount; i++)
            {
                var owner = new Owner
                {
                    Id = i,
                    Alias = "Owner Alias " + i,
                    FirstName = "First Name " + i % 3,
                    LastName = "Last Name " + i,
                };

                owners[i] = owner;
            }

            return owners;
        }

        private Run[] InitializeRuns()
        {
            var runs = new Run[_entitiesCount];
            for (var i = 0; i < _entitiesCount; i++)
            {
                var run = new Run
                {
                    Id = i,
                    Name = "Run Name" + i,
                    Purpose = i + 10,
                    Tasks = new List<Task>(),
                };

                runs[i] = run;
            }

            return runs;
        }

        private Task[] InitializeTasks()
        {
            var tasks = new Task[_entitiesCount];
            for (int i = 0; i < _entitiesCount; i++)
            {
                var task = new Task
                {
                    Id = i / 2,
                    Name = i % 2 == 0 ? "Foo" : "Bar",
                    Deleted = i % 3 == 0,
                };
            
                tasks[i] = task;
            }

            return tasks;
        }
    }
}
