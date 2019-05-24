// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace PlanCompilerTests
{
    using System.Data.Entity;
    using System.Linq;
    using AdvancedPatternsModel;
    using Xunit;
    using System.IO;
    using System.Data.Entity.TestModels.ArubaModel;
    using System.Data.Entity.Query;
    using System.Data;
    using System;
    using System.Data.Entity.TestHelpers;

    /// <summary>
    /// Tests for DbLimitExpression nodes in the CQT tree generation process.
    /// </summary>
    public class LimitExpressionTests : FunctionalTestBase, IClassFixture<LimitExpressionFixture>
    {
        #region Infrastructure/setup

        public LimitExpressionTests(LimitExpressionFixture data)
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                context.Database.Initialize(false);
            }
            using (var context = new ArubaContext())
            {
                context.Database.Initialize(false);
            }

            _limit_SimpleModel_OrderBy_Skip_First_expectedSql = data.Limit_SimpleModel_OrderBy_Skip_First_expectedSql;
            _limit_SimpleModel_OrderBy_Skip_FirstOrDefault_expectedSql = data.Limit_SimpleModel_OrderBy_Skip_FirstOrDefault_expectedSql;
            _limit_SimpleModel_OrderBy_Skip_Single_expectedSql = data.Limit_SimpleModel_OrderBy_Skip_Single_expectedSql;
            _limit_SimpleModel_OrderBy_Skip_SingleOrDefault_expectedSql = data.Limit_SimpleModel_OrderBy_Skip_SingleOrDefault_expectedSql;
            _limit_SimpleModel_OrderBy_Skip_Take_expectedSql = data.Limit_SimpleModel_OrderBy_Skip_Take_expectedSql;

            _limit_ComplexModel_OrderBy_Skip_First_expectedSql = data.Limit_ComplexModel_OrderBy_Skip_First_expectedSql;
            _limit_ComplexModel_OrderBy_Skip_FirstOrDefault_expectedSql = data.Limit_ComplexModel_OrderBy_Skip_FirstOrDefault_expectedSql;
            _limit_ComplexModel_OrderBy_Skip_Single_expectedSql = data.Limit_ComplexModel_OrderBy_Skip_Single_expectedSql;
            _limit_ComplexModel_OrderBy_Skip_SingleOrDefault_expectedSql = data.Limit_ComplexModel_OrderBy_Skip_SingleOrDefault_expectedSql;
            _limit_ComplexModel_OrderBy_Skip_Take_expectedSql = data.Limit_ComplexModel_OrderBy_Skip_Take_expectedSql;
        }

        #endregion

        #region Tests that generate a limit on a simple model

        private string _limit_SimpleModel_OrderBy_Skip_First_expectedSql;
        private string _limit_SimpleModel_OrderBy_Skip_FirstOrDefault_expectedSql;
        private string _limit_SimpleModel_OrderBy_Skip_Single_expectedSql;
        private string _limit_SimpleModel_OrderBy_Skip_SingleOrDefault_expectedSql;
        private string _limit_SimpleModel_OrderBy_Skip_Take_expectedSql;

        private string _limit_ComplexModel_OrderBy_Skip_First_expectedSql;
        private string _limit_ComplexModel_OrderBy_Skip_FirstOrDefault_expectedSql;
        private string _limit_ComplexModel_OrderBy_Skip_Single_expectedSql;
        private string _limit_ComplexModel_OrderBy_Skip_SingleOrDefault_expectedSql;
        private string _limit_ComplexModel_OrderBy_Skip_Take_expectedSql;

#if NET452
        [Fact]
        public void Limit_SimpleModel_First()
        {
            var log = new StringWriter();
            using (var context = new ArubaContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = context.Owners.Include(o => o.OwnedRun)
                                              .Where(o => o.FirstName == "Diego")
                                              .Select(o => o);
                    var result = query.First();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql = 
@"SELECT TOP (1) 
    [Extent1].[Id] AS [Id], 
    [Extent1].[FirstName] AS [FirstName], 
    [Extent1].[LastName] AS [LastName], 
    [Extent1].[Alias] AS [Alias], 
    [Extent2].[Id] AS [Id1], 
    [Extent2].[Name] AS [Name], 
    [Extent2].[Purpose] AS [Purpose], 
    [Extent2].[Geometry] AS [Geometry]
    FROM  [dbo].[ArubaOwners] AS [Extent1]
    LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
    WHERE N'Diego' = [Extent1].[FirstName]";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_First()
        {
            var log = new StringWriter();
            using (var context = new ArubaContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = context.Owners.Include(o => o.OwnedRun)
                                              .Where(o => o.FirstName == "Diego")
                                              .OrderBy(o => o.LastName)
                                              .Select(o => o);
                    var result = query.First();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
@"SELECT TOP (1) 
    [Extent1].[Id] AS [Id], 
    [Extent1].[FirstName] AS [FirstName], 
    [Extent1].[LastName] AS [LastName], 
    [Extent1].[Alias] AS [Alias], 
    [Extent2].[Id] AS [Id1], 
    [Extent2].[Name] AS [Name], 
    [Extent2].[Purpose] AS [Purpose], 
    [Extent2].[Geometry] AS [Geometry]
    FROM  [dbo].[ArubaOwners] AS [Extent1]
    LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
    WHERE N'Diego' = [Extent1].[FirstName]
    ORDER BY [Extent1].[LastName] ASC";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_Skip_First()
        {
            var log = new StringWriter();
            using (var context = new ArubaContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = context.Owners.Include(o => o.OwnedRun)
                                              .Where(o => o.FirstName == "Diego")
                                              .OrderBy(o => o.LastName)
                                              .Select(o => o);
                    var result = query.Skip(2).First();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(_limit_SimpleModel_OrderBy_Skip_First_expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_FirstOrDefault()
        {
            var log = new StringWriter();
            using (var context = new ArubaContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = context.Owners.Include(o => o.OwnedRun)
                                              .Where(o => o.FirstName == "Diego")
                                              .Select(o => o);
                    var result = query.FirstOrDefault();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
@"SELECT TOP (1) 
    [Extent1].[Id] AS [Id], 
    [Extent1].[FirstName] AS [FirstName], 
    [Extent1].[LastName] AS [LastName], 
    [Extent1].[Alias] AS [Alias], 
    [Extent2].[Id] AS [Id1], 
    [Extent2].[Name] AS [Name], 
    [Extent2].[Purpose] AS [Purpose], 
    [Extent2].[Geometry] AS [Geometry]
    FROM  [dbo].[ArubaOwners] AS [Extent1]
    LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
    WHERE N'Diego' = [Extent1].[FirstName]";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_FirstOrDefault()
        {
            var log = new StringWriter();
            using (var context = new ArubaContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = context.Owners.Include(o => o.OwnedRun)
                                              .Where(o => o.FirstName == "Diego")
                                              .OrderBy(o => o.LastName)
                                              .Select(o => o);
                    var result = query.FirstOrDefault();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
@"SELECT TOP (1) 
    [Extent1].[Id] AS [Id], 
    [Extent1].[FirstName] AS [FirstName], 
    [Extent1].[LastName] AS [LastName], 
    [Extent1].[Alias] AS [Alias], 
    [Extent2].[Id] AS [Id1], 
    [Extent2].[Name] AS [Name], 
    [Extent2].[Purpose] AS [Purpose], 
    [Extent2].[Geometry] AS [Geometry]
    FROM  [dbo].[ArubaOwners] AS [Extent1]
    LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
    WHERE N'Diego' = [Extent1].[FirstName]
    ORDER BY [Extent1].[LastName] ASC";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_Skip_FirstOrDefault()
        {
            var log = new StringWriter();
            using (var context = new ArubaContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = context.Owners.Include(o => o.OwnedRun)
                                              .Where(o => o.FirstName == "Diego")
                                              .OrderBy(o => o.LastName)
                                              .Select(o => o);
                    var result = query.Skip(2).FirstOrDefault();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(_limit_SimpleModel_OrderBy_Skip_FirstOrDefault_expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_Single()
        {
            var log = new StringWriter();
            using (var context = new ArubaContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = context.Owners.Include(o => o.OwnedRun)
                                              .Where(o => o.FirstName == "Diego")
                                              .Select(o => o);
                    var result = query.Single();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
@"SELECT TOP (2) 
    [Extent1].[Id] AS [Id], 
    [Extent1].[FirstName] AS [FirstName], 
    [Extent1].[LastName] AS [LastName], 
    [Extent1].[Alias] AS [Alias], 
    [Extent2].[Id] AS [Id1], 
    [Extent2].[Name] AS [Name], 
    [Extent2].[Purpose] AS [Purpose], 
    [Extent2].[Geometry] AS [Geometry]
    FROM  [dbo].[ArubaOwners] AS [Extent1]
    LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
    WHERE N'Diego' = [Extent1].[FirstName]";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_Single()
        {
            var log = new StringWriter();
            using (var context = new ArubaContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = context.Owners.Include(o => o.OwnedRun)
                                              .Where(o => o.FirstName == "Diego")
                                              .OrderBy(o => o.LastName)
                                              .Select(o => o);
                    var result = query.Single();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
@"SELECT TOP (2) 
    [Extent1].[Id] AS [Id], 
    [Extent1].[FirstName] AS [FirstName], 
    [Extent1].[LastName] AS [LastName], 
    [Extent1].[Alias] AS [Alias], 
    [Extent2].[Id] AS [Id1], 
    [Extent2].[Name] AS [Name], 
    [Extent2].[Purpose] AS [Purpose], 
    [Extent2].[Geometry] AS [Geometry]
    FROM  [dbo].[ArubaOwners] AS [Extent1]
    LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
    WHERE N'Diego' = [Extent1].[FirstName]
    ORDER BY [Extent1].[LastName] ASC";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_Skip_Single()
        {
            var log = new StringWriter();
            using (var context = new ArubaContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = context.Owners.Include(o => o.OwnedRun)
                                              .Where(o => o.FirstName == "Diego")
                                              .OrderBy(o => o.LastName)
                                              .Select(o => o);
                    var result = query.Skip(2).Single();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(_limit_SimpleModel_OrderBy_Skip_Single_expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_SingleOrDefault()
        {
            var log = new StringWriter();
            using (var context = new ArubaContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = context.Owners.Include(o => o.OwnedRun)
                                              .Where(o => o.FirstName == "Diego")
                                              .Select(o => o);
                    var result = query.SingleOrDefault();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
@"SELECT TOP (2) 
    [Extent1].[Id] AS [Id], 
    [Extent1].[FirstName] AS [FirstName], 
    [Extent1].[LastName] AS [LastName], 
    [Extent1].[Alias] AS [Alias], 
    [Extent2].[Id] AS [Id1], 
    [Extent2].[Name] AS [Name], 
    [Extent2].[Purpose] AS [Purpose], 
    [Extent2].[Geometry] AS [Geometry]
    FROM  [dbo].[ArubaOwners] AS [Extent1]
    LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
    WHERE N'Diego' = [Extent1].[FirstName]";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_SingleOrDefault()
        {
            var log = new StringWriter();
            using (var context = new ArubaContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = context.Owners.Include(o => o.OwnedRun)
                                              .Where(o => o.FirstName == "Diego")
                                              .OrderBy(o => o.LastName)
                                              .Select(o => o);
                    var result = query.SingleOrDefault();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
@"SELECT TOP (2) 
    [Extent1].[Id] AS [Id], 
    [Extent1].[FirstName] AS [FirstName], 
    [Extent1].[LastName] AS [LastName], 
    [Extent1].[Alias] AS [Alias], 
    [Extent2].[Id] AS [Id1], 
    [Extent2].[Name] AS [Name], 
    [Extent2].[Purpose] AS [Purpose], 
    [Extent2].[Geometry] AS [Geometry]
    FROM  [dbo].[ArubaOwners] AS [Extent1]
    LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
    WHERE N'Diego' = [Extent1].[FirstName]
    ORDER BY [Extent1].[LastName] ASC";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_Skip_SingleOrDefault()
        {
            var log = new StringWriter();
            using (var context = new ArubaContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = context.Owners.Include(o => o.OwnedRun)
                                              .Where(o => o.FirstName == "Diego")
                                              .OrderBy(o => o.LastName)
                                              .Select(o => o);
                    var result = query.Skip(2).SingleOrDefault();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(_limit_SimpleModel_OrderBy_Skip_SingleOrDefault_expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_Take()
        {
            var log = new StringWriter();
            using (var context = new ArubaContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = context.Owners.Include(o => o.OwnedRun)
                                              .Where(o => o.FirstName == "Diego")
                                              .Select(o => o);
                    var result = query.Take(1).ToList();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
@"SELECT TOP (1) 
    [Extent1].[Id] AS [Id], 
    [Extent1].[FirstName] AS [FirstName], 
    [Extent1].[LastName] AS [LastName], 
    [Extent1].[Alias] AS [Alias], 
    [Extent2].[Id] AS [Id1], 
    [Extent2].[Name] AS [Name], 
    [Extent2].[Purpose] AS [Purpose], 
    [Extent2].[Geometry] AS [Geometry]
    FROM  [dbo].[ArubaOwners] AS [Extent1]
    LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
    WHERE N'Diego' = [Extent1].[FirstName]";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_Take_Zero()
        {
            using (var context = new ArubaContext())
            {
                var query =
                    context.Owners.Include(o => o.OwnedRun)
                        .Where(o => o.FirstName == "Diego")
                        .Select(o => o)
                        .Take(0);

                Assert.Equal(0, query.Count());
                Assert.Equal(
@"SELECT 
    1 AS [C1], 
    CAST(NULL AS int) AS [C2], 
    CAST(NULL AS varchar(1)) AS [C3], 
    CAST(NULL AS varchar(1)) AS [C4], 
    CAST(NULL AS varchar(1)) AS [C5], 
    CAST(NULL AS int) AS [C6], 
    CAST(NULL AS varchar(1)) AS [C7], 
    CAST(NULL AS int) AS [C8], 
    CAST(NULL AS geometry) AS [C9]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]
    WHERE 1 = 0",
                    query.ToString());

            }
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_Take()
        {
            var log = new StringWriter();
            using (var context = new ArubaContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = context.Owners.Include(o => o.OwnedRun)
                                              .Where(o => o.FirstName == "Diego")
                                              .OrderBy(o => o.LastName)
                                              .Select(o => o);
                    var result = query.Take(1).ToList();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
@"SELECT TOP (1) 
    [Extent1].[Id] AS [Id], 
    [Extent1].[FirstName] AS [FirstName], 
    [Extent1].[LastName] AS [LastName], 
    [Extent1].[Alias] AS [Alias], 
    [Extent2].[Id] AS [Id1], 
    [Extent2].[Name] AS [Name], 
    [Extent2].[Purpose] AS [Purpose], 
    [Extent2].[Geometry] AS [Geometry]
    FROM  [dbo].[ArubaOwners] AS [Extent1]
    LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
    WHERE N'Diego' = [Extent1].[FirstName]
    ORDER BY [Extent1].[LastName] ASC";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_Take_Zero()
        {
            using (var context = new ArubaContext())
            {
                var query =
                    context.Owners.Include(o => o.OwnedRun)
                        .Where(o => o.FirstName == "Diego")
                        .OrderBy(o => o.LastName)
                        .Select(o => o)
                        .Take(0);

                Assert.Equal(0, query.Count());
                Assert.Equal(
@"SELECT 
    1 AS [C1], 
    CAST(NULL AS int) AS [C2], 
    CAST(NULL AS varchar(1)) AS [C3], 
    CAST(NULL AS varchar(1)) AS [C4], 
    CAST(NULL AS varchar(1)) AS [C5], 
    CAST(NULL AS int) AS [C6], 
    CAST(NULL AS varchar(1)) AS [C7], 
    CAST(NULL AS int) AS [C8], 
    CAST(NULL AS geometry) AS [C9]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]
    WHERE 1 = 0",
                    query.ToString());

            }
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_Skip_Take()
        {
            var log = new StringWriter();
            using (var context = new ArubaContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = context.Owners.Include(o => o.OwnedRun)
                                              .Where(o => o.FirstName == "Diego")
                                              .OrderBy(o => o.LastName)
                                              .Select(o => o);
                    var result = query.Skip(2).Take(1).ToList();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(_limit_SimpleModel_OrderBy_Skip_Take_expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_Skip_Take_Zero()
        {
            using (var context = new ArubaContext())
            {
                var query =
                    context.Owners.Include(o => o.OwnedRun)
                        .Where(o => o.FirstName == "Diego")
                        .OrderBy(o => o.LastName)
                        .Select(o => o)
                        .Skip(2)
                        .Take(0);

                Assert.Equal(0, query.Count());
                Assert.Equal(
@"SELECT 
    1 AS [C1], 
    CAST(NULL AS int) AS [C2], 
    CAST(NULL AS varchar(1)) AS [C3], 
    CAST(NULL AS varchar(1)) AS [C4], 
    CAST(NULL AS varchar(1)) AS [C5], 
    CAST(NULL AS int) AS [C6], 
    CAST(NULL AS varchar(1)) AS [C7], 
    CAST(NULL AS int) AS [C8], 
    CAST(NULL AS geometry) AS [C9]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]
    WHERE 1 = 0",
                    query.ToString());

            }
        }
#endif

        #endregion

        #region Tests that generate a limit on a complex model

        [Fact]
        public void Limit_ComplexModel_First()
        {
            var log = new StringWriter();
            using (var context = new AdvancedPatternsMasterContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = from building in context.Buildings.Include(b => b.PrincipalMailRoom)
                                where building.Name == "Building One"
                                select building;
                    var result = query.First();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
@"SELECT 
    [Limit1].[C1] AS [C1],
    [Limit1].[BuildingId] AS [BuildingId], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[Value] AS [Value], 
    [Limit1].[Address_Street] AS [Address_Street], 
    [Limit1].[Address_City] AS [Address_City], 
    [Limit1].[Address_State] AS [Address_State], 
    [Limit1].[Address_ZipCode] AS [Address_ZipCode], 
    [Limit1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Limit1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Limit1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Limit1].[id] AS [id], 
    [Limit1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT TOP (1) 
        [Extent1].[BuildingId] AS [BuildingId], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[Value] AS [Value], 
        [Extent1].[Address_Street] AS [Address_Street], 
        [Extent1].[Address_City] AS [Address_City], 
        [Extent1].[Address_State] AS [Address_State], 
        [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
        [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
        [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
        [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
        1 AS [C1], 
        [Extent2].[id] AS [id], 
        [Extent2].[BuildingId] AS [BuildingId1]
        FROM  [dbo].[Buildings] AS [Extent1]
        LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
        WHERE N'Building One' = [Extent1].[Name]
    )  AS [Limit1]";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_First()
        {
            var log = new StringWriter();
            using (var context = new AdvancedPatternsMasterContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = from building in context.Buildings.Include(b => b.PrincipalMailRoom)
                                where building.Name == "Building One"
                                orderby building.Address.ZipCode
                                select building;
                    var result = query.First();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
 @"SELECT TOP (1) 
    [Project1].[C1] AS [C1], 
    [Project1].[BuildingId] AS [BuildingId], 
    [Project1].[Name] AS [Name], 
    [Project1].[Value] AS [Value], 
    [Project1].[Address_Street] AS [Address_Street], 
    [Project1].[Address_City] AS [Address_City], 
    [Project1].[Address_State] AS [Address_State], 
    [Project1].[Address_ZipCode] AS [Address_ZipCode], 
    [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Project1].[id] AS [id], 
    [Project1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT 
        [Extent1].[BuildingId] AS [BuildingId], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[Value] AS [Value], 
        [Extent1].[Address_Street] AS [Address_Street], 
        [Extent1].[Address_City] AS [Address_City], 
        [Extent1].[Address_State] AS [Address_State], 
        [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
        [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
        [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
        [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
        1 AS [C1], 
        [Extent2].[id] AS [id], 
        [Extent2].[BuildingId] AS [BuildingId1]
        FROM  [dbo].[Buildings] AS [Extent1]
        LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
        WHERE N'Building One' = [Extent1].[Name]
    )  AS [Project1]
    ORDER BY [Project1].[Address_ZipCode] ASC";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_Skip_First()
        {
            var log = new StringWriter();
            using (var context = new AdvancedPatternsMasterContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = from building in context.Buildings.Include(b => b.PrincipalMailRoom)
                                where building.Name == "Building One"
                                orderby building.Address.ZipCode
                                select building;
                    var result = query.Skip(2).First();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(_limit_ComplexModel_OrderBy_Skip_First_expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_FirstOrDefault()
        {
            var log = new StringWriter();
            using (var context = new AdvancedPatternsMasterContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = from building in context.Buildings.Include(b => b.PrincipalMailRoom)
                                where building.Name == "Building One"
                                select building;
                    var result = query.FirstOrDefault();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
@"SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[BuildingId] AS [BuildingId], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[Value] AS [Value], 
    [Limit1].[Address_Street] AS [Address_Street], 
    [Limit1].[Address_City] AS [Address_City], 
    [Limit1].[Address_State] AS [Address_State], 
    [Limit1].[Address_ZipCode] AS [Address_ZipCode], 
    [Limit1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Limit1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Limit1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Limit1].[id] AS [id], 
    [Limit1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT TOP (1) 
        [Extent1].[BuildingId] AS [BuildingId], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[Value] AS [Value], 
        [Extent1].[Address_Street] AS [Address_Street], 
        [Extent1].[Address_City] AS [Address_City], 
        [Extent1].[Address_State] AS [Address_State], 
        [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
        [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
        [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
        [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
        1 AS [C1], 
        [Extent2].[id] AS [id], 
        [Extent2].[BuildingId] AS [BuildingId1]
        FROM  [dbo].[Buildings] AS [Extent1]
        LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
        WHERE N'Building One' = [Extent1].[Name]
    )  AS [Limit1]";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_FirstOrDefault()
        {
            var log = new StringWriter();
            using (var context = new AdvancedPatternsMasterContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = from building in context.Buildings.Include(b => b.PrincipalMailRoom)
                                where building.Name == "Building One"
                                orderby building.Address.ZipCode
                                select building;
                    var result = query.FirstOrDefault();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
@"SELECT TOP (1) 
    [Project1].[C1] AS [C1], 
    [Project1].[BuildingId] AS [BuildingId], 
    [Project1].[Name] AS [Name], 
    [Project1].[Value] AS [Value], 
    [Project1].[Address_Street] AS [Address_Street], 
    [Project1].[Address_City] AS [Address_City], 
    [Project1].[Address_State] AS [Address_State], 
    [Project1].[Address_ZipCode] AS [Address_ZipCode], 
    [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Project1].[id] AS [id], 
    [Project1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT 
        [Extent1].[BuildingId] AS [BuildingId], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[Value] AS [Value], 
        [Extent1].[Address_Street] AS [Address_Street], 
        [Extent1].[Address_City] AS [Address_City], 
        [Extent1].[Address_State] AS [Address_State], 
        [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
        [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
        [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
        [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
        1 AS [C1], 
        [Extent2].[id] AS [id], 
        [Extent2].[BuildingId] AS [BuildingId1]
        FROM  [dbo].[Buildings] AS [Extent1]
        LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
        WHERE N'Building One' = [Extent1].[Name]
    )  AS [Project1]
    ORDER BY [Project1].[Address_ZipCode] ASC";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_Skip_FirstOrDefault()
        {
            var log = new StringWriter();
            using (var context = new AdvancedPatternsMasterContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = from building in context.Buildings.Include(b => b.PrincipalMailRoom)
                                where building.Name == "Building One"
                                orderby building.Address.ZipCode
                                select building;
                    var result = query.Skip(2).FirstOrDefault();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(_limit_ComplexModel_OrderBy_Skip_FirstOrDefault_expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_Single()
        {
            var log = new StringWriter();
            using (var context = new AdvancedPatternsMasterContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = from building in context.Buildings.Include(b => b.PrincipalMailRoom)
                                where building.Name == "Building One"
                                select building;
                    var result = query.Single();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
@"SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[BuildingId] AS [BuildingId], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[Value] AS [Value], 
    [Limit1].[Address_Street] AS [Address_Street], 
    [Limit1].[Address_City] AS [Address_City], 
    [Limit1].[Address_State] AS [Address_State], 
    [Limit1].[Address_ZipCode] AS [Address_ZipCode], 
    [Limit1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Limit1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Limit1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Limit1].[id] AS [id], 
    [Limit1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT TOP (2) 
        [Extent1].[BuildingId] AS [BuildingId], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[Value] AS [Value], 
        [Extent1].[Address_Street] AS [Address_Street], 
        [Extent1].[Address_City] AS [Address_City], 
        [Extent1].[Address_State] AS [Address_State], 
        [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
        [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
        [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
        [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
        1 AS [C1], 
        [Extent2].[id] AS [id], 
        [Extent2].[BuildingId] AS [BuildingId1]
        FROM  [dbo].[Buildings] AS [Extent1]
        LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
        WHERE N'Building One' = [Extent1].[Name]
    )  AS [Limit1]";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_Single()
        {
            var log = new StringWriter();
            using (var context = new AdvancedPatternsMasterContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = from building in context.Buildings.Include(b => b.PrincipalMailRoom)
                                where building.Name == "Building One"
                                orderby building.Address.ZipCode
                                select building;
                    var result = query.Single();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
@"SELECT TOP (2) 
    [Project1].[C1] AS [C1], 
    [Project1].[BuildingId] AS [BuildingId], 
    [Project1].[Name] AS [Name], 
    [Project1].[Value] AS [Value], 
    [Project1].[Address_Street] AS [Address_Street], 
    [Project1].[Address_City] AS [Address_City], 
    [Project1].[Address_State] AS [Address_State], 
    [Project1].[Address_ZipCode] AS [Address_ZipCode], 
    [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Project1].[id] AS [id], 
    [Project1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT 
        [Extent1].[BuildingId] AS [BuildingId], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[Value] AS [Value], 
        [Extent1].[Address_Street] AS [Address_Street], 
        [Extent1].[Address_City] AS [Address_City], 
        [Extent1].[Address_State] AS [Address_State], 
        [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
        [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
        [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
        [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
        1 AS [C1], 
        [Extent2].[id] AS [id], 
        [Extent2].[BuildingId] AS [BuildingId1]
        FROM  [dbo].[Buildings] AS [Extent1]
        LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
        WHERE N'Building One' = [Extent1].[Name]
    )  AS [Project1]
    ORDER BY [Project1].[Address_ZipCode] ASC";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_Skip_Single()
        {
            var log = new StringWriter();
            using (var context = new AdvancedPatternsMasterContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = from building in context.Buildings.Include(b => b.PrincipalMailRoom)
                                where building.Name == "Building One"
                                orderby building.Address.ZipCode
                                select building;
                    var result = query.Skip(2).Single();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(_limit_ComplexModel_OrderBy_Skip_Single_expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_SingleOrDefault()
        {
            var log = new StringWriter();
            using (var context = new AdvancedPatternsMasterContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = from building in context.Buildings.Include(b => b.PrincipalMailRoom)
                                where building.Name == "Building One"
                                select building;
                    var result = query.SingleOrDefault();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
@"SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[BuildingId] AS [BuildingId], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[Value] AS [Value], 
    [Limit1].[Address_Street] AS [Address_Street], 
    [Limit1].[Address_City] AS [Address_City], 
    [Limit1].[Address_State] AS [Address_State], 
    [Limit1].[Address_ZipCode] AS [Address_ZipCode], 
    [Limit1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Limit1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Limit1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Limit1].[id] AS [id], 
    [Limit1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT TOP (2) 
        [Extent1].[BuildingId] AS [BuildingId], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[Value] AS [Value], 
        [Extent1].[Address_Street] AS [Address_Street], 
        [Extent1].[Address_City] AS [Address_City], 
        [Extent1].[Address_State] AS [Address_State], 
        [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
        [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
        [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
        [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
        1 AS [C1], 
        [Extent2].[id] AS [id], 
        [Extent2].[BuildingId] AS [BuildingId1]
        FROM  [dbo].[Buildings] AS [Extent1]
        LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
        WHERE N'Building One' = [Extent1].[Name]
    )  AS [Limit1]";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_SingleOrDefault()
        {
            var log = new StringWriter();
            using (var context = new AdvancedPatternsMasterContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = from building in context.Buildings.Include(b => b.PrincipalMailRoom)
                                where building.Name == "Building One"
                                orderby building.Address.ZipCode
                                select building;
                    var result = query.SingleOrDefault();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
@"SELECT TOP (2) 
    [Project1].[C1] AS [C1], 
    [Project1].[BuildingId] AS [BuildingId], 
    [Project1].[Name] AS [Name], 
    [Project1].[Value] AS [Value], 
    [Project1].[Address_Street] AS [Address_Street], 
    [Project1].[Address_City] AS [Address_City], 
    [Project1].[Address_State] AS [Address_State], 
    [Project1].[Address_ZipCode] AS [Address_ZipCode], 
    [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Project1].[id] AS [id], 
    [Project1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT 
        [Extent1].[BuildingId] AS [BuildingId], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[Value] AS [Value], 
        [Extent1].[Address_Street] AS [Address_Street], 
        [Extent1].[Address_City] AS [Address_City], 
        [Extent1].[Address_State] AS [Address_State], 
        [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
        [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
        [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
        [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
        1 AS [C1], 
        [Extent2].[id] AS [id], 
        [Extent2].[BuildingId] AS [BuildingId1]
        FROM  [dbo].[Buildings] AS [Extent1]
        LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
        WHERE N'Building One' = [Extent1].[Name]
    )  AS [Project1]
    ORDER BY [Project1].[Address_ZipCode] ASC";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_Skip_SingleOrDefault()
        {
            var log = new StringWriter();
            using (var context = new AdvancedPatternsMasterContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = from building in context.Buildings.Include(b => b.PrincipalMailRoom)
                                where building.Name == "Building One"
                                orderby building.Address.ZipCode
                                select building;
                    var result = query.Skip(2).SingleOrDefault();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(_limit_ComplexModel_OrderBy_Skip_SingleOrDefault_expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_Take()
        {
            var log = new StringWriter();
            using (var context = new AdvancedPatternsMasterContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = from building in context.Buildings.Include(b => b.PrincipalMailRoom)
                                where building.Name == "Building One"
                                select building;
                    var result = query.Take(1).ToList();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
@"SELECT 
    [Limit1].[C1] AS [C1], 
    [Limit1].[BuildingId] AS [BuildingId], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[Value] AS [Value], 
    [Limit1].[Address_Street] AS [Address_Street], 
    [Limit1].[Address_City] AS [Address_City], 
    [Limit1].[Address_State] AS [Address_State], 
    [Limit1].[Address_ZipCode] AS [Address_ZipCode], 
    [Limit1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Limit1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Limit1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Limit1].[id] AS [id], 
    [Limit1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT TOP (1) 
        [Extent1].[BuildingId] AS [BuildingId], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[Value] AS [Value], 
        [Extent1].[Address_Street] AS [Address_Street], 
        [Extent1].[Address_City] AS [Address_City], 
        [Extent1].[Address_State] AS [Address_State], 
        [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
        [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
        [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
        [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
        1 AS [C1], 
        [Extent2].[id] AS [id], 
        [Extent2].[BuildingId] AS [BuildingId1]
        FROM  [dbo].[Buildings] AS [Extent1]
        LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
        WHERE N'Building One' = [Extent1].[Name]
    )  AS [Limit1]";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_Take_Zero()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var query =
                    (from building in context.Buildings.Include(b => b.PrincipalMailRoom)
                        where building.Name == "Building One"
                        select building)
                        .Take(0);

                Assert.Equal(0, query.Count());
                Assert.Equal(
@"SELECT 
    1 AS [C1], 
    CAST(NULL AS uniqueidentifier) AS [C2], 
    CAST(NULL AS varchar(1)) AS [C3], 
    CAST(NULL AS decimal(18,2)) AS [C4], 
    CAST(NULL AS int) AS [C5], 
    CAST(NULL AS varchar(1)) AS [C6], 
    CAST(NULL AS varchar(1)) AS [C7], 
    CAST(NULL AS varchar(1)) AS [C8], 
    CAST(NULL AS varchar(1)) AS [C9], 
    CAST(NULL AS int) AS [C10], 
    CAST(NULL AS varchar(1)) AS [C11], 
    CAST(NULL AS int) AS [C12], 
    CAST(NULL AS int) AS [C13], 
    CAST(NULL AS uniqueidentifier) AS [C14]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]
    WHERE 1 = 0",
                    query.ToString());

            }
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_Take()
        {
            var log = new StringWriter();
            using (var context = new AdvancedPatternsMasterContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = from building in context.Buildings.Include(b => b.PrincipalMailRoom)
                                where building.Name == "Building One"
                                orderby building.Address.ZipCode
                                select building;
                    var result = query.Take(1).ToList();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }

            var expectedSql =
@"SELECT TOP (1) 
    [Project1].[C1] AS [C1], 
    [Project1].[BuildingId] AS [BuildingId], 
    [Project1].[Name] AS [Name], 
    [Project1].[Value] AS [Value], 
    [Project1].[Address_Street] AS [Address_Street], 
    [Project1].[Address_City] AS [Address_City], 
    [Project1].[Address_State] AS [Address_State], 
    [Project1].[Address_ZipCode] AS [Address_ZipCode], 
    [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Project1].[id] AS [id], 
    [Project1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT 
        [Extent1].[BuildingId] AS [BuildingId], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[Value] AS [Value], 
        [Extent1].[Address_Street] AS [Address_Street], 
        [Extent1].[Address_City] AS [Address_City], 
        [Extent1].[Address_State] AS [Address_State], 
        [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
        [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
        [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
        [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
        1 AS [C1], 
        [Extent2].[id] AS [id], 
        [Extent2].[BuildingId] AS [BuildingId1]
        FROM  [dbo].[Buildings] AS [Extent1]
        LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
        WHERE N'Building One' = [Extent1].[Name]
    )  AS [Project1]
    ORDER BY [Project1].[Address_ZipCode] ASC";

            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_Take_Zero()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var query =
                    (from building in context.Buildings.Include(b => b.PrincipalMailRoom)
                        where building.Name == "Building One"
                        orderby building.Address.ZipCode
                        select building)
                        .Take(0);

                Assert.Equal(0, query.Count());
                Assert.Equal(
@"SELECT 
    1 AS [C1], 
    CAST(NULL AS uniqueidentifier) AS [C2], 
    CAST(NULL AS varchar(1)) AS [C3], 
    CAST(NULL AS decimal(18,2)) AS [C4], 
    CAST(NULL AS int) AS [C5], 
    CAST(NULL AS varchar(1)) AS [C6], 
    CAST(NULL AS varchar(1)) AS [C7], 
    CAST(NULL AS varchar(1)) AS [C8], 
    CAST(NULL AS varchar(1)) AS [C9], 
    CAST(NULL AS int) AS [C10], 
    CAST(NULL AS varchar(1)) AS [C11], 
    CAST(NULL AS int) AS [C12], 
    CAST(NULL AS int) AS [C13], 
    CAST(NULL AS uniqueidentifier) AS [C14]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]
    WHERE 1 = 0",
                    query.ToString());

            }
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_Skip_Take()
        {
            var log = new StringWriter();
            using (var context = new AdvancedPatternsMasterContext())
            {
                context.Database.Log = log.Write;
                try
                {
                    var query = from building in context.Buildings.Include(b => b.PrincipalMailRoom)
                                where building.Name == "Building One"
                                orderby building.Address.ZipCode
                                select building;
                    var result = query.Skip(2).Take(1).ToList();
                }
                catch
                {
                    //we are only trying to capture the query, the result is not important for this test
                }
            }
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString())
                    .Contains(QueryTestHelpers.StripFormatting(_limit_ComplexModel_OrderBy_Skip_Take_expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_Skip_Take_Zero()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var query =
                    (from building in context.Buildings.Include(b => b.PrincipalMailRoom)
                        where building.Name == "Building One"
                        orderby building.Address.ZipCode
                        select building)
                        .Skip(2)
                        .Take(0);

                Assert.Equal(0, query.Count());
                Assert.Equal(
@"SELECT 
    1 AS [C1], 
    CAST(NULL AS uniqueidentifier) AS [C2], 
    CAST(NULL AS varchar(1)) AS [C3], 
    CAST(NULL AS decimal(18,2)) AS [C4], 
    CAST(NULL AS int) AS [C5], 
    CAST(NULL AS varchar(1)) AS [C6], 
    CAST(NULL AS varchar(1)) AS [C7], 
    CAST(NULL AS varchar(1)) AS [C8], 
    CAST(NULL AS varchar(1)) AS [C9], 
    CAST(NULL AS int) AS [C10], 
    CAST(NULL AS varchar(1)) AS [C11], 
    CAST(NULL AS int) AS [C12], 
    CAST(NULL AS int) AS [C13], 
    CAST(NULL AS uniqueidentifier) AS [C14]
    FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]
    WHERE 1 = 0",
                    query.ToString());

            }
        }

        #endregion
    }

    public class LimitExpressionFixture
    {
        public string Limit_SimpleModel_OrderBy_Skip_First_expectedSql { get; private set; }
        public string Limit_SimpleModel_OrderBy_Skip_FirstOrDefault_expectedSql { get; private set; }
        public string Limit_SimpleModel_OrderBy_Skip_Single_expectedSql { get; private set; }
        public string Limit_SimpleModel_OrderBy_Skip_SingleOrDefault_expectedSql { get; private set; }
        public string Limit_SimpleModel_OrderBy_Skip_Take_expectedSql { get; private set; }

        public string Limit_ComplexModel_OrderBy_Skip_First_expectedSql { get; private set; }
        public string Limit_ComplexModel_OrderBy_Skip_FirstOrDefault_expectedSql { get; private set; }
        public string Limit_ComplexModel_OrderBy_Skip_Single_expectedSql { get; private set; }
        public string Limit_ComplexModel_OrderBy_Skip_SingleOrDefault_expectedSql { get; private set; }
        public string Limit_ComplexModel_OrderBy_Skip_Take_expectedSql { get; private set; }

        public LimitExpressionFixture()
        {
            int sqlVersion = DatabaseTestHelpers.GetSqlDatabaseVersion<ArubaContext>(() => new ArubaContext());
            if (sqlVersion >= 11)
            {
                SetExpectedSqlForSql2012();
            }
            else
            {
                SetExpectedSqlForLegacySql();
            }
        }

        private void SetExpectedSqlForSql2012()
        {
            Limit_SimpleModel_OrderBy_Skip_First_expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[FirstName] AS [FirstName], 
    [Extent1].[LastName] AS [LastName], 
    [Extent1].[Alias] AS [Alias], 
    [Extent2].[Id] AS [Id1], 
    [Extent2].[Name] AS [Name], 
    [Extent2].[Purpose] AS [Purpose], 
    [Extent2].[Geometry] AS [Geometry]
    FROM  [dbo].[ArubaOwners] AS [Extent1]
    LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
    WHERE N'Diego' = [Extent1].[FirstName]
    ORDER BY row_number() OVER (ORDER BY [Extent1].[LastName] ASC)
    OFFSET 2 ROWS FETCH NEXT 1 ROWS ONLY ";

            Limit_SimpleModel_OrderBy_Skip_FirstOrDefault_expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[FirstName] AS [FirstName], 
    [Extent1].[LastName] AS [LastName], 
    [Extent1].[Alias] AS [Alias], 
    [Extent2].[Id] AS [Id1], 
    [Extent2].[Name] AS [Name], 
    [Extent2].[Purpose] AS [Purpose], 
    [Extent2].[Geometry] AS [Geometry]
    FROM  [dbo].[ArubaOwners] AS [Extent1]
    LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
    WHERE N'Diego' = [Extent1].[FirstName]
    ORDER BY row_number() OVER (ORDER BY [Extent1].[LastName] ASC)
    OFFSET 2 ROWS FETCH NEXT 1 ROWS ONLY ";

            Limit_SimpleModel_OrderBy_Skip_Single_expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[FirstName] AS [FirstName], 
    [Extent1].[LastName] AS [LastName], 
    [Extent1].[Alias] AS [Alias], 
    [Extent2].[Id] AS [Id1], 
    [Extent2].[Name] AS [Name], 
    [Extent2].[Purpose] AS [Purpose], 
    [Extent2].[Geometry] AS [Geometry]
    FROM  [dbo].[ArubaOwners] AS [Extent1]
    LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
    WHERE N'Diego' = [Extent1].[FirstName]
    ORDER BY row_number() OVER (ORDER BY [Extent1].[LastName] ASC)
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY ";

            Limit_SimpleModel_OrderBy_Skip_SingleOrDefault_expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[FirstName] AS [FirstName], 
    [Extent1].[LastName] AS [LastName], 
    [Extent1].[Alias] AS [Alias], 
    [Extent2].[Id] AS [Id1], 
    [Extent2].[Name] AS [Name], 
    [Extent2].[Purpose] AS [Purpose], 
    [Extent2].[Geometry] AS [Geometry]
    FROM  [dbo].[ArubaOwners] AS [Extent1]
    LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
    WHERE N'Diego' = [Extent1].[FirstName]
    ORDER BY row_number() OVER (ORDER BY [Extent1].[LastName] ASC)
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY ";

            Limit_SimpleModel_OrderBy_Skip_Take_expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[FirstName] AS [FirstName], 
    [Extent1].[LastName] AS [LastName], 
    [Extent1].[Alias] AS [Alias], 
    [Extent2].[Id] AS [Id1], 
    [Extent2].[Name] AS [Name], 
    [Extent2].[Purpose] AS [Purpose], 
    [Extent2].[Geometry] AS [Geometry]
    FROM  [dbo].[ArubaOwners] AS [Extent1]
    LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
    WHERE N'Diego' = [Extent1].[FirstName]
    ORDER BY row_number() OVER (ORDER BY [Extent1].[LastName] ASC)
    OFFSET 2 ROWS FETCH NEXT 1 ROWS ONLY ";

            Limit_ComplexModel_OrderBy_Skip_First_expectedSql =
@"SELECT 
    [Project1].[C1] AS [C1], 
    [Project1].[BuildingId] AS [BuildingId], 
    [Project1].[Name] AS [Name], 
    [Project1].[Value] AS [Value], 
    [Project1].[Address_Street] AS [Address_Street], 
    [Project1].[Address_City] AS [Address_City], 
    [Project1].[Address_State] AS [Address_State], 
    [Project1].[Address_ZipCode] AS [Address_ZipCode], 
    [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Project1].[id] AS [id], 
    [Project1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT 
        [Extent1].[BuildingId] AS [BuildingId], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[Value] AS [Value], 
        [Extent1].[Address_Street] AS [Address_Street], 
        [Extent1].[Address_City] AS [Address_City], 
        [Extent1].[Address_State] AS [Address_State], 
        [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
        [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
        [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
        [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
        1 AS [C1], 
        [Extent2].[id] AS [id], 
        [Extent2].[BuildingId] AS [BuildingId1]
        FROM  [dbo].[Buildings] AS [Extent1]
        LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
        WHERE N'Building One' = [Extent1].[Name]
    )  AS [Project1]
    ORDER BY row_number() OVER (ORDER BY [Project1].[Address_ZipCode] ASC)
    OFFSET 2 ROWS FETCH NEXT 1 ROWS ONLY ";

            Limit_ComplexModel_OrderBy_Skip_FirstOrDefault_expectedSql =
@"SELECT 
    [Project1].[C1] AS [C1], 
    [Project1].[BuildingId] AS [BuildingId], 
    [Project1].[Name] AS [Name], 
    [Project1].[Value] AS [Value], 
    [Project1].[Address_Street] AS [Address_Street], 
    [Project1].[Address_City] AS [Address_City], 
    [Project1].[Address_State] AS [Address_State], 
    [Project1].[Address_ZipCode] AS [Address_ZipCode], 
    [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Project1].[id] AS [id], 
    [Project1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT 
        [Extent1].[BuildingId] AS [BuildingId], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[Value] AS [Value], 
        [Extent1].[Address_Street] AS [Address_Street], 
        [Extent1].[Address_City] AS [Address_City], 
        [Extent1].[Address_State] AS [Address_State], 
        [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
        [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
        [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
        [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
        1 AS [C1], 
        [Extent2].[id] AS [id], 
        [Extent2].[BuildingId] AS [BuildingId1]
        FROM  [dbo].[Buildings] AS [Extent1]
        LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
        WHERE N'Building One' = [Extent1].[Name]
    )  AS [Project1]
    ORDER BY row_number() OVER (ORDER BY [Project1].[Address_ZipCode] ASC)
    OFFSET 2 ROWS FETCH NEXT 1 ROWS ONLY ";

            Limit_ComplexModel_OrderBy_Skip_Single_expectedSql =
@"SELECT 
    [Project1].[C1] AS [C1], 
    [Project1].[BuildingId] AS [BuildingId], 
    [Project1].[Name] AS [Name], 
    [Project1].[Value] AS [Value], 
    [Project1].[Address_Street] AS [Address_Street], 
    [Project1].[Address_City] AS [Address_City], 
    [Project1].[Address_State] AS [Address_State], 
    [Project1].[Address_ZipCode] AS [Address_ZipCode], 
    [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Project1].[id] AS [id], 
    [Project1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT 
        [Extent1].[BuildingId] AS [BuildingId], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[Value] AS [Value], 
        [Extent1].[Address_Street] AS [Address_Street], 
        [Extent1].[Address_City] AS [Address_City], 
        [Extent1].[Address_State] AS [Address_State], 
        [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
        [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
        [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
        [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
        1 AS [C1], 
        [Extent2].[id] AS [id], 
        [Extent2].[BuildingId] AS [BuildingId1]
        FROM  [dbo].[Buildings] AS [Extent1]
        LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
        WHERE N'Building One' = [Extent1].[Name]
    )  AS [Project1]
    ORDER BY row_number() OVER (ORDER BY [Project1].[Address_ZipCode] ASC)
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY ";

            Limit_ComplexModel_OrderBy_Skip_SingleOrDefault_expectedSql =
@"SELECT 
    [Project1].[C1] AS [C1], 
    [Project1].[BuildingId] AS [BuildingId], 
    [Project1].[Name] AS [Name], 
    [Project1].[Value] AS [Value], 
    [Project1].[Address_Street] AS [Address_Street], 
    [Project1].[Address_City] AS [Address_City], 
    [Project1].[Address_State] AS [Address_State], 
    [Project1].[Address_ZipCode] AS [Address_ZipCode], 
    [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Project1].[id] AS [id], 
    [Project1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT 
        [Extent1].[BuildingId] AS [BuildingId], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[Value] AS [Value], 
        [Extent1].[Address_Street] AS [Address_Street], 
        [Extent1].[Address_City] AS [Address_City], 
        [Extent1].[Address_State] AS [Address_State], 
        [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
        [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
        [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
        [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
        1 AS [C1], 
        [Extent2].[id] AS [id], 
        [Extent2].[BuildingId] AS [BuildingId1]
        FROM  [dbo].[Buildings] AS [Extent1]
        LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
        WHERE N'Building One' = [Extent1].[Name]
    )  AS [Project1]
    ORDER BY row_number() OVER (ORDER BY [Project1].[Address_ZipCode] ASC)
    OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY ";

            Limit_ComplexModel_OrderBy_Skip_Take_expectedSql =
@"SELECT 
    [Project1].[C1] AS [C1], 
    [Project1].[BuildingId] AS [BuildingId], 
    [Project1].[Name] AS [Name], 
    [Project1].[Value] AS [Value], 
    [Project1].[Address_Street] AS [Address_Street], 
    [Project1].[Address_City] AS [Address_City], 
    [Project1].[Address_State] AS [Address_State], 
    [Project1].[Address_ZipCode] AS [Address_ZipCode], 
    [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Project1].[id] AS [id], 
    [Project1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT 
        [Extent1].[BuildingId] AS [BuildingId], 
        [Extent1].[Name] AS [Name], 
        [Extent1].[Value] AS [Value], 
        [Extent1].[Address_Street] AS [Address_Street], 
        [Extent1].[Address_City] AS [Address_City], 
        [Extent1].[Address_State] AS [Address_State], 
        [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
        [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
        [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
        [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
        1 AS [C1], 
        [Extent2].[id] AS [id], 
        [Extent2].[BuildingId] AS [BuildingId1]
        FROM  [dbo].[Buildings] AS [Extent1]
        LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
        WHERE N'Building One' = [Extent1].[Name]
    )  AS [Project1]
    ORDER BY row_number() OVER (ORDER BY [Project1].[Address_ZipCode] ASC)
    OFFSET 2 ROWS FETCH NEXT 1 ROWS ONLY ";
        }

        private void SetExpectedSqlForLegacySql()
        {
            Limit_SimpleModel_OrderBy_Skip_First_expectedSql =
@"SELECT TOP (1) 
    [Filter1].[Id1] AS [Id], 
    [Filter1].[FirstName] AS [FirstName], 
    [Filter1].[LastName] AS [LastName], 
    [Filter1].[Alias] AS [Alias], 
    [Filter1].[Id2] AS [Id1], 
    [Filter1].[Name] AS [Name], 
    [Filter1].[Purpose] AS [Purpose], 
    [Filter1].[Geometry] AS [Geometry]
    FROM ( SELECT [Extent1].[Id] AS [Id1], [Extent1].[FirstName] AS [FirstName], [Extent1].[LastName] AS [LastName], [Extent1].[Alias] AS [Alias], [Extent2].[Id] AS [Id2], [Extent2].[Name] AS [Name], [Extent2].[Purpose] AS [Purpose], [Extent2].[Geometry] AS [Geometry], row_number() OVER (ORDER BY [Extent1].[LastName] ASC) AS [row_number]
        FROM  [dbo].[ArubaOwners] AS [Extent1]
        LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
        WHERE N'Diego' = [Extent1].[FirstName]
    )  AS [Filter1]
    WHERE [Filter1].[row_number] > 2
    ORDER BY [Filter1].[LastName] ASC";

            Limit_SimpleModel_OrderBy_Skip_FirstOrDefault_expectedSql =
@"SELECT TOP (1) 
    [Filter1].[Id1] AS [Id], 
    [Filter1].[FirstName] AS [FirstName], 
    [Filter1].[LastName] AS [LastName], 
    [Filter1].[Alias] AS [Alias], 
    [Filter1].[Id2] AS [Id1], 
    [Filter1].[Name] AS [Name], 
    [Filter1].[Purpose] AS [Purpose], 
    [Filter1].[Geometry] AS [Geometry]
    FROM ( SELECT [Extent1].[Id] AS [Id1], [Extent1].[FirstName] AS [FirstName], [Extent1].[LastName] AS [LastName], [Extent1].[Alias] AS [Alias], [Extent2].[Id] AS [Id2], [Extent2].[Name] AS [Name], [Extent2].[Purpose] AS [Purpose], [Extent2].[Geometry] AS [Geometry], row_number() OVER (ORDER BY [Extent1].[LastName] ASC) AS [row_number]
        FROM  [dbo].[ArubaOwners] AS [Extent1]
        LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
        WHERE N'Diego' = [Extent1].[FirstName]
    )  AS [Filter1]
    WHERE [Filter1].[row_number] > 2
    ORDER BY [Filter1].[LastName] ASC";

            Limit_SimpleModel_OrderBy_Skip_Single_expectedSql =
@"SELECT TOP (2) 
    [Filter1].[Id1] AS [Id], 
    [Filter1].[FirstName] AS [FirstName], 
    [Filter1].[LastName] AS [LastName], 
    [Filter1].[Alias] AS [Alias], 
    [Filter1].[Id2] AS [Id1], 
    [Filter1].[Name] AS [Name], 
    [Filter1].[Purpose] AS [Purpose], 
    [Filter1].[Geometry] AS [Geometry]
    FROM ( SELECT [Extent1].[Id] AS [Id1], [Extent1].[FirstName] AS [FirstName], [Extent1].[LastName] AS [LastName], [Extent1].[Alias] AS [Alias], [Extent2].[Id] AS [Id2], [Extent2].[Name] AS [Name], [Extent2].[Purpose] AS [Purpose], [Extent2].[Geometry] AS [Geometry], row_number() OVER (ORDER BY [Extent1].[LastName] ASC) AS [row_number]
        FROM  [dbo].[ArubaOwners] AS [Extent1]
        LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
        WHERE N'Diego' = [Extent1].[FirstName]
    )  AS [Filter1]
    WHERE [Filter1].[row_number] > 2
    ORDER BY [Filter1].[LastName] ASC";

            Limit_SimpleModel_OrderBy_Skip_SingleOrDefault_expectedSql =
@"SELECT TOP (2) 
    [Filter1].[Id1] AS [Id], 
    [Filter1].[FirstName] AS [FirstName], 
    [Filter1].[LastName] AS [LastName], 
    [Filter1].[Alias] AS [Alias], 
    [Filter1].[Id2] AS [Id1], 
    [Filter1].[Name] AS [Name], 
    [Filter1].[Purpose] AS [Purpose], 
    [Filter1].[Geometry] AS [Geometry]
    FROM ( SELECT [Extent1].[Id] AS [Id1], [Extent1].[FirstName] AS [FirstName], [Extent1].[LastName] AS [LastName], [Extent1].[Alias] AS [Alias], [Extent2].[Id] AS [Id2], [Extent2].[Name] AS [Name], [Extent2].[Purpose] AS [Purpose], [Extent2].[Geometry] AS [Geometry], row_number() OVER (ORDER BY [Extent1].[LastName] ASC) AS [row_number]
        FROM  [dbo].[ArubaOwners] AS [Extent1]
        LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
        WHERE N'Diego' = [Extent1].[FirstName]
    )  AS [Filter1]
    WHERE [Filter1].[row_number] > 2
    ORDER BY [Filter1].[LastName] ASC";

            Limit_SimpleModel_OrderBy_Skip_Take_expectedSql =
@"SELECT TOP (1) 
    [Filter1].[Id1] AS [Id], 
    [Filter1].[FirstName] AS [FirstName], 
    [Filter1].[LastName] AS [LastName], 
    [Filter1].[Alias] AS [Alias], 
    [Filter1].[Id2] AS [Id1], 
    [Filter1].[Name] AS [Name], 
    [Filter1].[Purpose] AS [Purpose], 
    [Filter1].[Geometry] AS [Geometry]
    FROM ( SELECT [Extent1].[Id] AS [Id1], [Extent1].[FirstName] AS [FirstName], [Extent1].[LastName] AS [LastName], [Extent1].[Alias] AS [Alias], [Extent2].[Id] AS [Id2], [Extent2].[Name] AS [Name], [Extent2].[Purpose] AS [Purpose], [Extent2].[Geometry] AS [Geometry], row_number() OVER (ORDER BY [Extent1].[LastName] ASC) AS [row_number]
        FROM  [dbo].[ArubaOwners] AS [Extent1]
        LEFT OUTER JOIN [dbo].[ArubaRuns] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
        WHERE N'Diego' = [Extent1].[FirstName]
    )  AS [Filter1]
    WHERE [Filter1].[row_number] > 2
    ORDER BY [Filter1].[LastName] ASC";

            Limit_ComplexModel_OrderBy_Skip_First_expectedSql =
@"SELECT TOP (1) 
    [Project1].[C1] AS [C1], 
    [Project1].[BuildingId] AS [BuildingId], 
    [Project1].[Name] AS [Name], 
    [Project1].[Value] AS [Value], 
    [Project1].[Address_Street] AS [Address_Street], 
    [Project1].[Address_City] AS [Address_City], 
    [Project1].[Address_State] AS [Address_State], 
    [Project1].[Address_ZipCode] AS [Address_ZipCode], 
    [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Project1].[id] AS [id], 
    [Project1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT [Project1].[BuildingId] AS [BuildingId], [Project1].[Name] AS [Name], [Project1].[Value] AS [Value], [Project1].[Address_Street] AS [Address_Street], [Project1].[Address_City] AS [Address_City], [Project1].[Address_State] AS [Address_State], [Project1].[Address_ZipCode] AS [Address_ZipCode], [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], [Project1].[C1] AS [C1], [Project1].[id] AS [id], [Project1].[BuildingId1] AS [BuildingId1], row_number() OVER (ORDER BY [Project1].[Address_ZipCode] ASC) AS [row_number]
        FROM ( SELECT 
            [Extent1].[BuildingId] AS [BuildingId], 
            [Extent1].[Name] AS [Name], 
            [Extent1].[Value] AS [Value], 
            [Extent1].[Address_Street] AS [Address_Street], 
            [Extent1].[Address_City] AS [Address_City], 
            [Extent1].[Address_State] AS [Address_State], 
            [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
            [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
            [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
            [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
            1 AS [C1], 
            [Extent2].[id] AS [id], 
            [Extent2].[BuildingId] AS [BuildingId1]
            FROM  [dbo].[Buildings] AS [Extent1]
            LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
            WHERE N'Building One' = [Extent1].[Name]
        )  AS [Project1]
    )  AS [Project1]
    WHERE [Project1].[row_number] > 2
    ORDER BY [Project1].[Address_ZipCode] ASC";

            Limit_ComplexModel_OrderBy_Skip_FirstOrDefault_expectedSql =
@"SELECT TOP (1) 
    [Project1].[C1] AS [C1], 
    [Project1].[BuildingId] AS [BuildingId], 
    [Project1].[Name] AS [Name], 
    [Project1].[Value] AS [Value], 
    [Project1].[Address_Street] AS [Address_Street], 
    [Project1].[Address_City] AS [Address_City], 
    [Project1].[Address_State] AS [Address_State], 
    [Project1].[Address_ZipCode] AS [Address_ZipCode], 
    [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Project1].[id] AS [id], 
    [Project1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT [Project1].[BuildingId] AS [BuildingId], [Project1].[Name] AS [Name], [Project1].[Value] AS [Value], [Project1].[Address_Street] AS [Address_Street], [Project1].[Address_City] AS [Address_City], [Project1].[Address_State] AS [Address_State], [Project1].[Address_ZipCode] AS [Address_ZipCode], [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], [Project1].[C1] AS [C1], [Project1].[id] AS [id], [Project1].[BuildingId1] AS [BuildingId1], row_number() OVER (ORDER BY [Project1].[Address_ZipCode] ASC) AS [row_number]
        FROM ( SELECT 
            [Extent1].[BuildingId] AS [BuildingId], 
            [Extent1].[Name] AS [Name], 
            [Extent1].[Value] AS [Value], 
            [Extent1].[Address_Street] AS [Address_Street], 
            [Extent1].[Address_City] AS [Address_City], 
            [Extent1].[Address_State] AS [Address_State], 
            [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
            [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
            [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
            [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
            1 AS [C1], 
            [Extent2].[id] AS [id], 
            [Extent2].[BuildingId] AS [BuildingId1]
            FROM  [dbo].[Buildings] AS [Extent1]
            LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
            WHERE N'Building One' = [Extent1].[Name]
        )  AS [Project1]
    )  AS [Project1]
    WHERE [Project1].[row_number] > 2
    ORDER BY [Project1].[Address_ZipCode] ASC";

            Limit_ComplexModel_OrderBy_Skip_Single_expectedSql =
@"SELECT TOP (2) 
    [Project1].[C1] AS [C1], 
    [Project1].[BuildingId] AS [BuildingId], 
    [Project1].[Name] AS [Name], 
    [Project1].[Value] AS [Value], 
    [Project1].[Address_Street] AS [Address_Street], 
    [Project1].[Address_City] AS [Address_City], 
    [Project1].[Address_State] AS [Address_State], 
    [Project1].[Address_ZipCode] AS [Address_ZipCode], 
    [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Project1].[id] AS [id], 
    [Project1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT [Project1].[BuildingId] AS [BuildingId], [Project1].[Name] AS [Name], [Project1].[Value] AS [Value], [Project1].[Address_Street] AS [Address_Street], [Project1].[Address_City] AS [Address_City], [Project1].[Address_State] AS [Address_State], [Project1].[Address_ZipCode] AS [Address_ZipCode], [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], [Project1].[C1] AS [C1], [Project1].[id] AS [id], [Project1].[BuildingId1] AS [BuildingId1], row_number() OVER (ORDER BY [Project1].[Address_ZipCode] ASC) AS [row_number]
        FROM ( SELECT 
            [Extent1].[BuildingId] AS [BuildingId], 
            [Extent1].[Name] AS [Name], 
            [Extent1].[Value] AS [Value], 
            [Extent1].[Address_Street] AS [Address_Street], 
            [Extent1].[Address_City] AS [Address_City], 
            [Extent1].[Address_State] AS [Address_State], 
            [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
            [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
            [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
            [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
            1 AS [C1], 
            [Extent2].[id] AS [id], 
            [Extent2].[BuildingId] AS [BuildingId1]
            FROM  [dbo].[Buildings] AS [Extent1]
            LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
            WHERE N'Building One' = [Extent1].[Name]
        )  AS [Project1]
    )  AS [Project1]
    WHERE [Project1].[row_number] > 2
    ORDER BY [Project1].[Address_ZipCode] ASC";

            Limit_ComplexModel_OrderBy_Skip_SingleOrDefault_expectedSql =
@"SELECT TOP (2) 
    [Project1].[C1] AS [C1], 
    [Project1].[BuildingId] AS [BuildingId], 
    [Project1].[Name] AS [Name], 
    [Project1].[Value] AS [Value], 
    [Project1].[Address_Street] AS [Address_Street], 
    [Project1].[Address_City] AS [Address_City], 
    [Project1].[Address_State] AS [Address_State], 
    [Project1].[Address_ZipCode] AS [Address_ZipCode], 
    [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Project1].[id] AS [id], 
    [Project1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT [Project1].[BuildingId] AS [BuildingId], [Project1].[Name] AS [Name], [Project1].[Value] AS [Value], [Project1].[Address_Street] AS [Address_Street], [Project1].[Address_City] AS [Address_City], [Project1].[Address_State] AS [Address_State], [Project1].[Address_ZipCode] AS [Address_ZipCode], [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], [Project1].[C1] AS [C1], [Project1].[id] AS [id], [Project1].[BuildingId1] AS [BuildingId1], row_number() OVER (ORDER BY [Project1].[Address_ZipCode] ASC) AS [row_number]
        FROM ( SELECT 
            [Extent1].[BuildingId] AS [BuildingId], 
            [Extent1].[Name] AS [Name], 
            [Extent1].[Value] AS [Value], 
            [Extent1].[Address_Street] AS [Address_Street], 
            [Extent1].[Address_City] AS [Address_City], 
            [Extent1].[Address_State] AS [Address_State], 
            [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
            [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
            [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
            [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
            1 AS [C1], 
            [Extent2].[id] AS [id], 
            [Extent2].[BuildingId] AS [BuildingId1]
            FROM  [dbo].[Buildings] AS [Extent1]
            LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
            WHERE N'Building One' = [Extent1].[Name]
        )  AS [Project1]
    )  AS [Project1]
    WHERE [Project1].[row_number] > 2
    ORDER BY [Project1].[Address_ZipCode] ASC";

            Limit_ComplexModel_OrderBy_Skip_Take_expectedSql =
    @"SELECT TOP (1) 
    [Project1].[C1] AS [C1], 
    [Project1].[BuildingId] AS [BuildingId], 
    [Project1].[Name] AS [Name], 
    [Project1].[Value] AS [Value], 
    [Project1].[Address_Street] AS [Address_Street], 
    [Project1].[Address_City] AS [Address_City], 
    [Project1].[Address_State] AS [Address_State], 
    [Project1].[Address_ZipCode] AS [Address_ZipCode], 
    [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
    [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
    [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
    [Project1].[id] AS [id], 
    [Project1].[BuildingId1] AS [BuildingId1]
    FROM ( SELECT [Project1].[BuildingId] AS [BuildingId], [Project1].[Name] AS [Name], [Project1].[Value] AS [Value], [Project1].[Address_Street] AS [Address_Street], [Project1].[Address_City] AS [Address_City], [Project1].[Address_State] AS [Address_State], [Project1].[Address_ZipCode] AS [Address_ZipCode], [Project1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], [Project1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], [Project1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], [Project1].[C1] AS [C1], [Project1].[id] AS [id], [Project1].[BuildingId1] AS [BuildingId1], row_number() OVER (ORDER BY [Project1].[Address_ZipCode] ASC) AS [row_number]
        FROM ( SELECT 
            [Extent1].[BuildingId] AS [BuildingId], 
            [Extent1].[Name] AS [Name], 
            [Extent1].[Value] AS [Value], 
            [Extent1].[Address_Street] AS [Address_Street], 
            [Extent1].[Address_City] AS [Address_City], 
            [Extent1].[Address_State] AS [Address_State], 
            [Extent1].[Address_ZipCode] AS [Address_ZipCode], 
            [Extent1].[Address_SiteInfo_Zone] AS [Address_SiteInfo_Zone], 
            [Extent1].[Address_SiteInfo_Environment] AS [Address_SiteInfo_Environment], 
            [Extent1].[PrincipalMailRoomId] AS [PrincipalMailRoomId], 
            1 AS [C1], 
            [Extent2].[id] AS [id], 
            [Extent2].[BuildingId] AS [BuildingId1]
            FROM  [dbo].[Buildings] AS [Extent1]
            LEFT OUTER JOIN [dbo].[MailRooms] AS [Extent2] ON [Extent1].[PrincipalMailRoomId] = [Extent2].[id]
            WHERE N'Building One' = [Extent1].[Name]
        )  AS [Project1]
    )  AS [Project1]
    WHERE [Project1].[row_number] > 2
    ORDER BY [Project1].[Address_ZipCode] ASC";
        }
    }
}
