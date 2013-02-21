// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Data.Entity.TestModels.ArubaModel;
    using System.Linq;
    using Xunit;
    
    public class DefaultIfEmptyTests : FunctionalTestBase
    {
        [Fact]
        public void SelectMany_with_DefaultIfEmpty_translates_into_left_outer_join()
        {
            using (var context = new ArubaContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[Id] AS [Id], 
[Extent2].[Id] AS [Id1], 
[Extent2].[Name] AS [Name], 
[Extent2].[Deleted] AS [Deleted], 
[Extent2].[TaskInfo_Passed] AS [TaskInfo_Passed], 
[Extent2].[TaskInfo_Failed] AS [TaskInfo_Failed], 
[Extent2].[TaskInfo_Investigates] AS [TaskInfo_Investigates], 
[Extent2].[TaskInfo_Improvements] AS [TaskInfo_Improvements], 
[Extent2].[ArubaRun_Id] AS [ArubaRun_Id]
FROM  [dbo].[ArubaRuns] AS [Extent1]
LEFT OUTER JOIN [dbo].[ArubaTasks] AS [Extent2] ON [Extent1].[Id] = [Extent2].[ArubaRun_Id]";

                var query = context.Runs.SelectMany(c => c.Tasks.DefaultIfEmpty());
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that owners that have empty collecion of bugs are projected as nulls
                var results = query.ToList();
                var ownersWithoutBugsCount = context.Runs.Count(o => !o.Tasks.Any());
                Assert.Equal(ownersWithoutBugsCount, results.Count(r => r == null));
            }
        }

        [Fact]
        public void DefaultIfEmpty_with_null_default()
        {
            using (var context = new ArubaContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[Id] AS [Id], 
[Extent1].[Name] AS [Name], 
[Extent1].[Purpose] AS [Purpose], 
[Extent2].[Id] AS [Id1], 
[Extent2].[Name] AS [Name1], 
[Extent2].[Deleted] AS [Deleted], 
[Extent2].[TaskInfo_Passed] AS [TaskInfo_Passed], 
[Extent2].[TaskInfo_Failed] AS [TaskInfo_Failed], 
[Extent2].[TaskInfo_Investigates] AS [TaskInfo_Investigates], 
[Extent2].[TaskInfo_Improvements] AS [TaskInfo_Improvements], 
[Extent2].[ArubaRun_Id] AS [ArubaRun_Id]
FROM  [dbo].[ArubaRuns] AS [Extent1]
LEFT OUTER JOIN [dbo].[ArubaTasks] AS [Extent2] ON ([Extent1].[Id] = [Extent2].[ArubaRun_Id]) AND (1 = [Extent2].[Id])";

                var query = context.Runs.SelectMany(c => c.Tasks.Where(t => t.Id == 1).DefaultIfEmpty(), (r, t) => new { r, t });
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that all runs are projected
                // verify that the only task that is non-null has Id = 1
                var results = query.ToList();
                var runsCount = context.Runs.Count();
                Assert.Equal(runsCount, results.Select(r => r.r.Id).Distinct().Count());
                for (int i = 0; i < runsCount; i++)
                {
                    var task = results[i].t;
                    Assert.True(task == null || task.Id == 1);
                }
            }
        }

        [Fact]
        public void DefaultIfEmpty_with_non_null_default()
        {
            using (var context = new ArubaContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[Id] AS [Id], 
[Extent1].[FirstName] AS [FirstName], 
[Extent1].[LastName] AS [LastName], 
[Extent1].[Alias] AS [Alias], 
CASE WHEN (CASE WHEN ([Join1].[Id1] IS NULL) THEN CAST(NULL AS tinyint) ELSE cast(1 as tinyint) END IS NULL) THEN 0 ELSE [Join1].[Id1] END AS [C1]
FROM  [dbo].[ArubaOwners] AS [Extent1]
LEFT OUTER JOIN  (SELECT [Extent2].[Id] AS [Id1], [Extent2].[ArubaOwner_Id] AS [ArubaOwner_Id]
	FROM  [dbo].[Bugs1] AS [Extent2]
	INNER JOIN [dbo].[Bugs2] AS [Extent3] ON [Extent2].[Id] = [Extent3].[Id] ) AS [Join1] ON ([Extent1].[Id] = [Join1].[ArubaOwner_Id]) AND (1 = [Join1].[Id1])";

                var query = context.Owners.SelectMany(c => c.Bugs.Where(b => b.Id == 1).Select(b => b.Id).DefaultIfEmpty(), (o, b) => new { o, b });
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that all owners are projected
                // verify that the only bug that is non 0 has Id = 1
                var results = query.ToList();
                var ownersCount = context.Owners.Count();
                Assert.Equal(ownersCount, results.Select(r => r.o.Id).Distinct().Count());
                for (int i = 0; i < ownersCount; i++)
                {
                    var bugId = results[i].b;
                    Assert.True(bugId == 0 || bugId == 1);
                }
            }
        }

        [Fact]
        public void DefaultIfEmpty_with_explicit_default()
        {
            using (var context = new ArubaContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[Id] AS [Id], 
[Extent1].[Name] AS [Name], 
[Extent1].[Purpose] AS [Purpose], 
CASE WHEN (CASE WHEN ([Extent2].[Id] IS NULL) THEN CAST(NULL AS tinyint) ELSE cast(1 as tinyint) END IS NULL) THEN N'Default' ELSE [Extent2].[Name] END AS [C1]
FROM  [dbo].[ArubaRuns] AS [Extent1]
LEFT OUTER JOIN [dbo].[ArubaTasks] AS [Extent2] ON ([Extent1].[Id] = [Extent2].[ArubaRun_Id]) AND (1 = [Extent2].[Id])";

                var query = context.Runs.SelectMany(r => r.Tasks.Where(t => t.Id == 1).Select(t => t.Name).DefaultIfEmpty("Default"), (r, tn) => new { r, tn });
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that all owners are projected
                // verify that the only task that is non "Default" has Id = 1
                var results = query.ToList();
                var taskNamesForIdOne = context.Tasks.Where(t => t.Id == 1).Select(t => t.Name).ToList();
                var runsCount = context.Runs.Count();
                Assert.Equal(runsCount, results.Select(r => r.r.Id).Distinct().Count());
                for (int i = 0; i < runsCount; i++)
                {
                    var taskName = results[i].tn;
                    Assert.True(taskName == "Default" || taskNamesForIdOne.Contains(taskName));
                }
            }
        }

        [Fact]
        public void DefaultIfEmpty_with_anonymous_type_default()
        {
            using (var context = new ArubaContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[Id] AS [Id], 
[Extent1].[Name] AS [Name], 
[Extent1].[Purpose] AS [Purpose], 
CASE WHEN (CASE WHEN ([Extent2].[Id] IS NULL) THEN CAST(NULL AS tinyint) ELSE cast(1 as tinyint) END IS NULL) THEN -1 ELSE [Extent2].[Id] END AS [C1], 
CASE WHEN (CASE WHEN ([Extent2].[Id] IS NULL) THEN CAST(NULL AS tinyint) ELSE cast(1 as tinyint) END IS NULL) THEN N'Unknown' ELSE [Extent2].[Name] END AS [C2], 
CASE WHEN (CASE WHEN ([Extent2].[Id] IS NULL) THEN CAST(NULL AS tinyint) ELSE cast(1 as tinyint) END IS NULL) THEN cast(1 as bit) ELSE [Extent2].[Deleted] END AS [C3]
FROM  [dbo].[ArubaRuns] AS [Extent1]
LEFT OUTER JOIN [dbo].[ArubaTasks] AS [Extent2] ON ([Extent1].[Id] = [Extent2].[ArubaRun_Id]) AND (2 = [Extent2].[Id]) AND (N'Foo' = [Extent2].[Name])";

                var query = context.Runs.SelectMany(r => r.Tasks.Where(t => t.Id == 2 && t.Name == "Foo").Select(t => new { t.Id, t.Name, t.Deleted, }).DefaultIfEmpty(new { Id = -1, Name = "Unknown", Deleted = true }), (r, t) => new { r, t });
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that all runs are projected
                // verify that the only task that is non default has Id = 2 and Name = "Foo"
                var results = query.ToList();
                var taskDeletedForIdTwoNameFoo = context.Tasks.Where(t => t.Id == 2 && t.Name == "Foo").Single().Deleted;
                var runsCount = context.Runs.Count();
                Assert.Equal(runsCount, results.Select(r => r.r.Id).Distinct().Count());
                for (int i = 0; i < runsCount; i++)
                {
                    var task = results[i].t;
                    Assert.True(task.Id == -1 || task.Id == 2);
                    Assert.True(task.Name == "Unknown" || task.Name == "Foo");
                    Assert.True(task.Deleted || task.Deleted == taskDeletedForIdTwoNameFoo);
                }
            }
        }

        [Fact]
        public void SelectMany_of_two_entity_sets_withDefaultIfEmpty_translated_to_left_outer_join()
        {
            using (var context = new ArubaContext())
            {
                var expectedSql =
@"SELECT 
[Extent1].[Id] AS [Id], 
[Join1].[Id1] AS [Id1], 
[Join1].[Number] AS [Number], 
[Join1].[Comment] AS [Comment], 
[Join1].[Resolution] AS [Resolution], 
[Join1].[Failure_Id] AS [Failure_Id], 
[Join1].[ArubaOwner_Id] AS [ArubaOwner_Id]
FROM  [dbo].[ArubaFailures] AS [Extent1]
LEFT OUTER JOIN  (SELECT [Extent2].[Id] AS [Id1], [Extent2].[Comment] AS [Comment], [Extent2].[Failure_Id] AS [Failure_Id], [Extent2].[ArubaOwner_Id] AS [ArubaOwner_Id], [Extent3].[Number] AS [Number], [Extent3].[Resolution] AS [Resolution]
	FROM  [dbo].[Bugs1] AS [Extent2]
	INNER JOIN [dbo].[Bugs2] AS [Extent3] ON [Extent2].[Id] = [Extent3].[Id] ) AS [Join1] ON [Extent1].[Id] = [Join1].[Failure_Id]";

                var query = context.Failures.GroupJoin(context.Bugs, f => f.Id, b => b.Failure.Id, (f, b) => new { f, b }).SelectMany(r => r.b.DefaultIfEmpty());
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that failures that have empty collecion of bugs are projected as nulls
                var results = query.ToList();
                var failuresWithoutBugsCount = context.Failures.Count(o => !o.Bugs.Any());
                Assert.Equal(failuresWithoutBugsCount, results.Count(r => r == null));
            }
        }
    }
}
