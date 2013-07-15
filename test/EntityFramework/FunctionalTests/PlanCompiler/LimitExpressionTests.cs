// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace PlanCompilerTests
{
    using System.Data.Entity;
    using System.Linq;
    using AdvancedPatternsModel;
    using Xunit;
    using System.IO;
    using System;
    using System.Data.Entity.TestModels.ArubaModel;
    using System.Data.Entity.Query;

    /// <summary>
    ///     Tests for DbLimitExpression nodes in the CQT tree generation process.
    /// </summary>
    public class LimitExpressionTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        public LimitExpressionTests()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                context.Database.Initialize(false);
            }
            using (var context = new ArubaContext())
            {
                context.Database.Initialize(false);
            }
        }

        #endregion

        #region Tests that generate a limit on a simple model

        [Fact]
        public void Limit_SimpleModel_First()
        {
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
    WHERE (N'Diego' = [Extent1].[FirstName]) AND ([Extent1].[FirstName] IS NOT NULL)";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_First()
        {
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
    WHERE (N'Diego' = [Extent1].[FirstName]) AND ([Extent1].[FirstName] IS NOT NULL)
    ORDER BY [Extent1].[LastName] ASC";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_Skip_First()
        {
            var expectedSql =
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
        WHERE (N'Diego' = [Extent1].[FirstName]) AND ([Extent1].[FirstName] IS NOT NULL)
    )  AS [Filter1]
    WHERE [Filter1].[row_number] > 2
    ORDER BY [Filter1].[LastName] ASC";
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
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_FirstOrDefault()
        {
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
    WHERE (N'Diego' = [Extent1].[FirstName]) AND ([Extent1].[FirstName] IS NOT NULL)";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_FirstOrDefault()
        {
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
    WHERE (N'Diego' = [Extent1].[FirstName]) AND ([Extent1].[FirstName] IS NOT NULL)
    ORDER BY [Extent1].[LastName] ASC";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_Skip_FirstOrDefault()
        {
            var expectedSql =
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
        WHERE (N'Diego' = [Extent1].[FirstName]) AND ([Extent1].[FirstName] IS NOT NULL)
    )  AS [Filter1]
    WHERE [Filter1].[row_number] > 2
    ORDER BY [Filter1].[LastName] ASC";
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
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_Single()
        {
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
    WHERE (N'Diego' = [Extent1].[FirstName]) AND ([Extent1].[FirstName] IS NOT NULL)";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_Single()
        {
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
    WHERE (N'Diego' = [Extent1].[FirstName]) AND ([Extent1].[FirstName] IS NOT NULL)
    ORDER BY [Extent1].[LastName] ASC";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_Skip_Single()
        {
            var expectedSql =
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
        WHERE (N'Diego' = [Extent1].[FirstName]) AND ([Extent1].[FirstName] IS NOT NULL)
    )  AS [Filter1]
    WHERE [Filter1].[row_number] > 2
    ORDER BY [Filter1].[LastName] ASC";
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
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_SingleOrDefault()
        {
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
    WHERE (N'Diego' = [Extent1].[FirstName]) AND ([Extent1].[FirstName] IS NOT NULL)";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_SingleOrDefault()
        {
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
    WHERE (N'Diego' = [Extent1].[FirstName]) AND ([Extent1].[FirstName] IS NOT NULL)
    ORDER BY [Extent1].[LastName] ASC";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_Skip_SingleOrDefault()
        {
            var expectedSql =
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
        WHERE (N'Diego' = [Extent1].[FirstName]) AND ([Extent1].[FirstName] IS NOT NULL)
    )  AS [Filter1]
    WHERE [Filter1].[row_number] > 2
    ORDER BY [Filter1].[LastName] ASC";
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
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_Take()
        {
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
    WHERE (N'Diego' = [Extent1].[FirstName]) AND ([Extent1].[FirstName] IS NOT NULL)";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_Take()
        {
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
    WHERE (N'Diego' = [Extent1].[FirstName]) AND ([Extent1].[FirstName] IS NOT NULL)
    ORDER BY [Extent1].[LastName] ASC";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_SimpleModel_OrderBy_Skip_Take()
        {
            var expectedSql =
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
        WHERE (N'Diego' = [Extent1].[FirstName]) AND ([Extent1].[FirstName] IS NOT NULL)
    )  AS [Filter1]
    WHERE [Filter1].[row_number] > 2
    ORDER BY [Filter1].[LastName] ASC";
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
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        #endregion

        #region Tests that generate a limit on a complex model

        [Fact]
        public void Limit_ComplexModel_First()
        {
            string expectedSql =
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
        WHERE (N'Building One' = [Extent1].[Name]) AND ([Extent1].[Name] IS NOT NULL)
    )  AS [Limit1]";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_First()
        {
            string expectedSql =
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
        WHERE (N'Building One' = [Extent1].[Name]) AND ([Extent1].[Name] IS NOT NULL)
    )  AS [Project1]
    ORDER BY [Project1].[Address_ZipCode] ASC";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_Skip_First()
        {
            string expectedSql =
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
            WHERE (N'Building One' = [Extent1].[Name]) AND ([Extent1].[Name] IS NOT NULL)
        )  AS [Project1]
    )  AS [Project1]
    WHERE [Project1].[row_number] > 2
    ORDER BY [Project1].[Address_ZipCode] ASC";
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
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_FirstOrDefault()
        {
            string expectedSql =
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
        WHERE (N'Building One' = [Extent1].[Name]) AND ([Extent1].[Name] IS NOT NULL)
    )  AS [Limit1]";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_FirstOrDefault()
        {
            string expectedSql =
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
        WHERE (N'Building One' = [Extent1].[Name]) AND ([Extent1].[Name] IS NOT NULL)
    )  AS [Project1]
    ORDER BY [Project1].[Address_ZipCode] ASC";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_Skip_FirstOrDefault()
        {
            string expectedSql =
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
            WHERE (N'Building One' = [Extent1].[Name]) AND ([Extent1].[Name] IS NOT NULL)
        )  AS [Project1]
    )  AS [Project1]
    WHERE [Project1].[row_number] > 2
    ORDER BY [Project1].[Address_ZipCode] ASC";
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
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_Single()
        {
            string expectedSql =
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
        WHERE (N'Building One' = [Extent1].[Name]) AND ([Extent1].[Name] IS NOT NULL)
    )  AS [Limit1]";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_Single()
        {
            string expectedSql =
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
        WHERE (N'Building One' = [Extent1].[Name]) AND ([Extent1].[Name] IS NOT NULL)
    )  AS [Project1]
    ORDER BY [Project1].[Address_ZipCode] ASC";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_Skip_Single()
        {
            string expectedSql =
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
            WHERE (N'Building One' = [Extent1].[Name]) AND ([Extent1].[Name] IS NOT NULL)
        )  AS [Project1]
    )  AS [Project1]
    WHERE [Project1].[row_number] > 2
    ORDER BY [Project1].[Address_ZipCode] ASC";
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
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_SingleOrDefault()
        {
            string expectedSql =
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
        WHERE (N'Building One' = [Extent1].[Name]) AND ([Extent1].[Name] IS NOT NULL)
    )  AS [Limit1]";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_SingleOrDefault()
        {
            string expectedSql =
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
        WHERE (N'Building One' = [Extent1].[Name]) AND ([Extent1].[Name] IS NOT NULL)
    )  AS [Project1]
    ORDER BY [Project1].[Address_ZipCode] ASC";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_Skip_SingleOrDefault()
        {
            string expectedSql =
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
            WHERE (N'Building One' = [Extent1].[Name]) AND ([Extent1].[Name] IS NOT NULL)
        )  AS [Project1]
    )  AS [Project1]
    WHERE [Project1].[row_number] > 2
    ORDER BY [Project1].[Address_ZipCode] ASC";
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
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_Take()
        {
            string expectedSql =
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
        WHERE (N'Building One' = [Extent1].[Name]) AND ([Extent1].[Name] IS NOT NULL)
    )  AS [Limit1]";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_Take()
        {
            string expectedSql =
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
        WHERE (N'Building One' = [Extent1].[Name]) AND ([Extent1].[Name] IS NOT NULL)
    )  AS [Project1]
    ORDER BY [Project1].[Address_ZipCode] ASC";
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
            Assert.True(
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        [Fact]
        public void Limit_ComplexModel_OrderBy_Skip_Take()
        {
            string expectedSql =
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
            WHERE (N'Building One' = [Extent1].[Name]) AND ([Extent1].[Name] IS NOT NULL)
        )  AS [Project1]
    )  AS [Project1]
    WHERE [Project1].[row_number] > 2
    ORDER BY [Project1].[Address_ZipCode] ASC";
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
                QueryTestHelpers.StripFormatting(log.ToString()).StartsWith(QueryTestHelpers.StripFormatting(expectedSql)),
                "The resulting query is different from the expected value");
        }

        #endregion
    }
}
