// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ArubaModel
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Spatial;
    using System.Linq;

    public class ArubaInitializer : DropCreateDatabaseIfModelChanges<ArubaContext>
    {
        private const int EntitiesCount = 10;

        protected override void Seed(ArubaContext context)
        {
            var allTypes = InitializeAllTypes();
            var bugs = InitializeBugs();
            var configs = InitializeConfigs();
            var failures = InitializeFailures();
            var owners = InitializeOwners();
            var runs = InitializeRuns();
            var tasks = InitializeTasks();
            InitializeAndPersistPeople(context);

            for (var i = 0; i < EntitiesCount; i++)
            {
                bugs[i].Failure = failures[i % 4];
                failures[i % 4].Bugs.Add(bugs[i]);
            }

            for (var i = 0; i < EntitiesCount; i++)
            {
                for (var j = 0; j < i % 3; j++)
                {
                    configs[i].Failures.Add(failures[(i + j) % EntitiesCount]);
                }
            }

            for (var i = 0; i < EntitiesCount; i++)
            {
                for (var j = 0; j < i % 2; j++)
                {
                    failures[i].Configs.Add(configs[(i + j) % EntitiesCount]);
                }
            }

            for (var i = 0; i < EntitiesCount; i++)
            {
                owners[i].OwnedRun = runs[i];
                for (var j = 0; j < 5; j++)
                {
                    owners[i].Bugs.Add(bugs[(i + j) % EntitiesCount]);
                }
            }

            for (var i = 0; i < EntitiesCount; i++)
            {
                runs[i].RunOwner = owners[i];
                for (var j = 0; j < 3; j++)
                {
                    runs[i].Tasks.Add(tasks[(i + j) % EntitiesCount]);
                }
            }

            for (int i = 0; i < EntitiesCount; i++)
            {
                context.AllTypes.Add(allTypes[i]);
                context.Bugs.Add(bugs[i]);
                context.Configs.Add(configs[i]);
                context.Failures.Add(failures[i]);
                context.Owners.Add(owners[i]);
                context.Runs.Add(runs[i]);
                context.Tasks.Add(tasks[i]);
            }

            context.SaveChanges();

            base.Seed(context);
        }

        private ArubaAllTypes[] InitializeAllTypes()
        {
            var allTypesList = new ArubaAllTypes[EntitiesCount];
            for (var i = 0; i < EntitiesCount; i++)
            {
                var allTypes = new ArubaAllTypes
                    {
                        c2_smallint = (short)i,
                        c3_tinyint = (byte)i,
                        c4_bit = i % 2 == 0,
                        c5_datetime = new DateTime(1990, i % 12 + 1, i % 28 + 1, i % 12, i % 60, i % 60),
                        c6_smalldatetime = new DateTime(2010, i % 12 + 1, i % 28 + 1, i % 12, 0, 0),
                        c7_decimal_28_4 = 10 + (decimal)((double)i / 4),
                        c8_numeric_28_4 = -5 + (decimal)((double)i / 8),
                        c9_real = (float)i / 3,
                        c10_float = i + (double)i / 3,
                        c11_money = i + (decimal)((double)i / 5),
                        c12_smallmoney = i + (decimal)((double)i / 2),
                        c13_varchar_512_ = new string((char)(i + 'a'), i % 8),
                        c14_char_512_= new string((char)(i + 'a'), 512),
                        c15_text = new string((char)(i + 'a'), 20 + i),
                        c16_binary_512_ = Enumerable.Repeat<byte>((byte)i, 512).ToArray(),
                        c17_varbinary_512_ = Enumerable.Repeat<byte>((byte)i, 1 + i % 7).ToArray(),
                        c18_image = Enumerable.Repeat<byte>((byte)i, i + 10).ToArray(),
                        c19_nvarchar_512_ = new string((char)(i + 'a'), i),
                        c20_nchar_512_ = new string((char)(i + 'a'), 512),
                        c21_ntext = new string((char)(i + 'a'), 20 + i),
                        c22_uniqueidentifier = new Guid(new string((char)((i % 5) + '0'), 32)),
                        c23_bigint = (long)i * 10,
                        c24_varchar_max_ = new string((char)(i + 'A'), 15 + i % 4),
                        c25_nvarchar_max_ = new string((char)(i + 'A'), 15 + i % 7),
                        c26_varbinary_max_ = Enumerable.Repeat<byte>((byte)i, 15 + i % 8).ToArray(),
                        c27_time = new TimeSpan(10, i % 60 + 1, i % 60 + i),
                        c28_date = new DateTime(2000, i % 8 + 1, i % 8 + 1),
                        c29_datetime2 = new DateTime(2012, i % 5 + 1, i % 5 + 1, 1, 2, 3),
                        c30_datetimeoffset = new DateTimeOffset(new DateTime(2030 + i, 1, 2), new TimeSpan(i % 12, i % 60, 0)),
                        c31_geography = DbGeography.FromText(string.Format("POINT ({0}.0 {0}.0)", i % 8), 4326),
                        c32_geometry = DbGeometry.FromText(string.Format("POINT (1{0}.0 2{0}.0)", i % 8), 32768),
                        c33_enum = (ArubaEnum)(i % 4),
                        c34_byteenum = (ArubaByteEnum)(i % 3),
                        //c35_timestamp
                        c36_geometry_linestring = DbGeometry.FromText(string.Format("LINESTRING (1{0} 2{0}, 1{1} 2{0}, 1{1} 2{1}, 1{0} 2{1}, 1{0} 2{0})", i % 5 + 2, i % 5 + 4), 32768),
                        c37_geometry_polygon = DbGeometry.FromText(string.Format("POLYGON ((1{1} 2{0}, 1{0} 2{0}, 1{0} 2{1}, 1{1} 2{0}))", i % 5+ 3, i % 5 + 4), 32768),
                    };

                allTypesList[i] = allTypes;
            }

            return allTypesList;
        }

        private ArubaBug[] InitializeBugs()
        {
            var bugs = new ArubaBug[EntitiesCount];
            for (var i = 0; i < EntitiesCount; i++)
            {
                var bug = new ArubaBug
                {
                    Comment = "Bug Comment " + i,
                    Number = i,
                    Resolution = (ArubaBugResolution)(i % 5),
                };
                bugs[i] = bug;
            }

            return bugs;
        }

        private ArubaConfig[] InitializeConfigs()
        {
            var configs = new ArubaConfig[EntitiesCount];
            for (var i = 0; i < EntitiesCount; i++)
            {
                if (i % 2 == 0)
                {
                    var config = new ArubaConfig
                    {
                        Arch = "Config Architecture " + i % 3,
                        Lang = "Config Language" + i,
                        OS = "Config Operating System" + i,
                        Failures = new List<ArubaFailure>(),
                    };
                    configs[i] = config;
                }
                else
                {
                    var machineConfig = new ArubaMachineConfig
                    {
                        Address = new Guid(new string((char)((i % 8) + '0'), 32)),
                        Arch = "Machine Config Architecture " + i,
                        Host = "Machine Config Host " + i,
                        Lang = "Machine Config Language " + i,
                        Location = DbGeography.FromText(string.Format("POINT ({0}.0 {0}.0)", i), 4326),
                        OS = "Machine Config Operating System " + i % 5,
                        Failures = new List<ArubaFailure>(),
                    };
                    configs[i] = machineConfig;
                }
            }

            return configs;
        }

        private ArubaFailure[] InitializeFailures()
        {
            var failures = new ArubaFailure[EntitiesCount];
            for (var i = 0; i < EntitiesCount; i++)
            {
                if (i % 3 == 0)
                {
                    var failure = new ArubaFailure
                    {
                        Changed = new DateTime(2000, i % 12 + 1, i % 28 + 1),
                        Log = "Failure Log " + i,
                        TestCase = "Failure Test Case " + i,
                        TestId = i,
                        Variation = i % 4,
                        Configs = new List<ArubaConfig>(),
                        Bugs = new List<ArubaBug>(),
                    };
                    failures[i] = failure;
                }
                else if (i % 3 == 1)
                {
                    var baseline = new ArubaBaseline
                    {
                        Changed = new DateTime(2000, i % 12 + 1, i % 28 + 1),
                        Comment = "Baseline Comment " + i,
                        Log = "Baseline Log " + i,
                        TestCase = "Baseline Test Case " + i,
                        TestId = i % 4,
                        Variation = i,
                        Configs = new List<ArubaConfig>(),
                        Bugs = new List<ArubaBug>(),
                    };
                    failures[i] = baseline;
                }
                else
                {
                    var testFailure = new ArubaTestFailure
                    {
                        Changed = new DateTime(2000, i % 12 + 1, i % 28 + 1),
                        Comment = "Test Failure Comment " + i % 2,
                        Log = "Test Failure Log " + i,
                        TestCase = "Test Failure Test Case " + i,
                        TestId = i % 4,
                        Variation = i,
                        Configs = new List<ArubaConfig>(),
                        Bugs = new List<ArubaBug>(),
                    };
                    failures[i] = testFailure;
                }
            }

            return failures;
        }

        private ArubaOwner[] InitializeOwners()
        {
            var owners = new ArubaOwner[EntitiesCount];
            for (var i = 0; i < EntitiesCount; i++)
            {
                var owner = new ArubaOwner
                {
                    Id = i,
                    Alias = "Owner Alias " + i,
                    FirstName = "First Name " + i % 3,
                    LastName = "Last Name " + i,
                    Bugs = new List<ArubaBug>(),
                };
                owners[i] = owner;
            }

            return owners;
        }

        private ArubaRun[] InitializeRuns()
        {
            var runs = new ArubaRun[EntitiesCount];
            for (var i = 0; i < EntitiesCount; i++)
            {
                var run = new ArubaRun
                {
                    Id = i,
                    Name = "Run Name" + i,
                    Purpose = i + 10,
                    Tasks = new List<ArubaTask>(),
                    Geometry = DbGeometry.FromText(string.Format("POINT (1{0}.0 2{1}.0)", i % 8, 8 - i % 8), 32768),
                };
                runs[i] = run;
            }

            return runs;
        }

        private ArubaTask[] InitializeTasks()
        {
            var tasks = new ArubaTask[EntitiesCount];
            for (int i = 0; i < EntitiesCount; i++)
            {
                var task = new ArubaTask
                {
                    Id = i / 2,
                    Name = i % 2 == 0 ? "Foo" : "Bar",
                    Deleted = i % 3 == 0,
                    TaskInfo = new ArubaTaskInfo
                    {
                        Failed = i,
                        Improvements = i * 2,
                        Investigates = i % 4,
                        Passed = i % 2,
                    },
                };
                tasks[i] = task;
            }
            return tasks;
        }

        private void InitializeAndPersistPeople(ArubaContext context)
        {
            var grandFather = new ArubaPerson
                {
                    Name = "GrandFather",
                    Children = new List<ArubaPerson>(),
                };

            var mother = new ArubaPerson
                {
                    Name = "Mother",
                    Children = new List<ArubaPerson>(),
                    Parents = new List<ArubaPerson> { grandFather },
                };

            grandFather.Children.Add(mother);

            var father = new ArubaPerson
                {
                    Name = "Father",
                    Children = new List<ArubaPerson>(),
                };
            mother.Partner = father;

            var child = new ArubaPerson
                {
                    Name = "Child",
                    Parents = new List<ArubaPerson> { mother, father }
                };

            mother.Children.Add(child);
            father.Children.Add(child);

            var childOfSingleMother = new ArubaPerson
            {
                Name = "Child",
            };

            var singleMother = new ArubaPerson
                {
                    Name = "Single Mother",
                    Children = new List<ArubaPerson> { childOfSingleMother },
                };

            childOfSingleMother.Parents = new List<ArubaPerson> { singleMother };

            var childOfDivorcedParents = new ArubaPerson
                {
                    Name = "Child",
                };

            var divorcedFather = new ArubaPerson
                {
                    Name = "Divorced Father",
                    Children = new List<ArubaPerson> { childOfDivorcedParents },
                };

            var divorcedMother = new ArubaPerson
            {
                Name = "Divorced Mother",
                Children = new List<ArubaPerson> { childOfDivorcedParents },
            };

            childOfDivorcedParents.Parents = new List<ArubaPerson> { divorcedFather, divorcedMother };

            var bachelor = new ArubaPerson
                {
                    Name = "Bachelor",
                };

            context.People.Add(bachelor);
            context.People.Add(divorcedFather);
            context.People.Add(divorcedMother);
            context.People.Add(singleMother);
            context.People.Add(grandFather);
            context.SaveChanges();
        }
    }
}
