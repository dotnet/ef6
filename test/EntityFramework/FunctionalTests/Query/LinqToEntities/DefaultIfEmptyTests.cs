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
[Extent2].[Number] AS [Number], 
[Extent2].[Comment] AS [Comment], 
[Extent2].[Resolution] AS [Resolution], 
[Extent2].[Failure_Id] AS [Failure_Id], 
[Extent2].[ArubaOwner_Id] AS [ArubaOwner_Id]
FROM  [dbo].[ArubaOwners] AS [Extent1]
LEFT OUTER JOIN [dbo].[ArubaBugs] AS [Extent2] ON [Extent1].[Id] = [Extent2].[ArubaOwner_Id]";

                var query = context.Owners.SelectMany(c => c.Bugs.DefaultIfEmpty());
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that owners that have empty collecion of bugs are projected as nulls
                var results = query.ToList();
                var ownersWithoutBugsCount = context.Owners.Count(o => !o.Bugs.Any());
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
[Extent1].[FirstName] AS [FirstName], 
[Extent1].[LastName] AS [LastName], 
[Extent1].[Alias] AS [Alias], 
[Extent2].[Id] AS [Id1], 
[Extent2].[Number] AS [Number], 
[Extent2].[Comment] AS [Comment], 
[Extent2].[Resolution] AS [Resolution], 
[Extent2].[Failure_Id] AS [Failure_Id], 
[Extent2].[ArubaOwner_Id] AS [ArubaOwner_Id]
FROM  [dbo].[ArubaOwners] AS [Extent1]
LEFT OUTER JOIN [dbo].[ArubaBugs] AS [Extent2] ON ([Extent1].[Id] = [Extent2].[ArubaOwner_Id]) AND (1 = [Extent2].[Id])";

                var query = context.Owners.SelectMany(c => c.Bugs.Where(b => b.Id == 1).DefaultIfEmpty(), (o, b) => new { o, b });
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that all owners are projected
                // verify that the only bug that is non-null has Id = 1
                var results = query.ToList();
                var ownersCount = context.Owners.Count();
                Assert.Equal(ownersCount, results.Select(r => r.o.Id).Distinct().Count());
                for (int i = 0; i < ownersCount; i++)
                {
                    var bug = results[i].b;
                    Assert.True(bug == null || bug.Id == 1);
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
CASE WHEN (CASE WHEN ([Extent2].[Id] IS NULL) THEN CAST(NULL AS tinyint) ELSE cast(1 as tinyint) END IS NULL) THEN 0 ELSE [Extent2].[Id] END AS [C1]
FROM  [dbo].[ArubaOwners] AS [Extent1]
LEFT OUTER JOIN [dbo].[ArubaBugs] AS [Extent2] ON ([Extent1].[Id] = [Extent2].[ArubaOwner_Id]) AND (1 = [Extent2].[Id])";

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
[Extent1].[FirstName] AS [FirstName], 
[Extent1].[LastName] AS [LastName], 
[Extent1].[Alias] AS [Alias], 
CASE WHEN (CASE WHEN ([Extent2].[Id] IS NULL) THEN CAST(NULL AS tinyint) ELSE cast(1 as tinyint) END IS NULL) THEN N'Foo' ELSE [Extent2].[Comment] END AS [C1]
FROM  [dbo].[ArubaOwners] AS [Extent1]
LEFT OUTER JOIN [dbo].[ArubaBugs] AS [Extent2] ON ([Extent1].[Id] = [Extent2].[ArubaOwner_Id]) AND (1 = [Extent2].[Id])";

                var query = context.Owners.SelectMany(o => o.Bugs.Where(b => b.Id == 1).Select(b => b.Comment).DefaultIfEmpty("Foo"), (o, b) => new { o, b });
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that all owners are projected
                // verify that the only bug that is non "Foo" has Id = 1
                var results = query.ToList();
                var bugCommentForIdOne = context.Bugs.Where(b => b.Id == 1).Single().Comment;
                var ownersCount = context.Owners.Count();
                Assert.Equal(ownersCount, results.Select(r => r.o.Id).Distinct().Count());
                for (int i = 0; i < ownersCount; i++)
                {
                    var bugComment = results[i].b;
                    Assert.True(bugComment == "Foo" || bugComment == bugCommentForIdOne);
                }

                Console.WriteLine(results);
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
[Extent1].[FirstName] AS [FirstName], 
[Extent1].[LastName] AS [LastName], 
[Extent1].[Alias] AS [Alias], 
CASE WHEN (CASE WHEN ([Extent2].[Id] IS NULL) THEN CAST(NULL AS tinyint) ELSE cast(1 as tinyint) END IS NULL) THEN -1 ELSE [Extent2].[Id] END AS [C1], 
CASE WHEN (CASE WHEN ([Extent2].[Id] IS NULL) THEN CAST(NULL AS tinyint) ELSE cast(1 as tinyint) END IS NULL) THEN N'Unknown' ELSE [Extent2].[Comment] END AS [C2], 
CASE WHEN (CASE WHEN ([Extent2].[Id] IS NULL) THEN CAST(NULL AS tinyint) ELSE cast(1 as tinyint) END IS NULL) THEN 3 ELSE [Extent2].[Resolution] END AS [C3]
FROM  [dbo].[ArubaOwners] AS [Extent1]
LEFT OUTER JOIN [dbo].[ArubaBugs] AS [Extent2] ON ([Extent1].[Id] = [Extent2].[ArubaOwner_Id]) AND (5 = [Extent2].[Id])";

                var query = context.Owners.SelectMany(o => o.Bugs.Where(b => b.Id == 5).Select(b => new { b.Id, b.Comment, b.Resolution }).DefaultIfEmpty(new { Id = -1, Comment = "Unknown", Resolution = (ArubaBugResolution?)ArubaBugResolution.NoRepro }), (o, b) => new { o, b });
                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                // verify that all owners are projected
                // verify that the only bug that is non default has Id = 5
                var results = query.ToList();
                var bugCommentForIdFive = context.Bugs.Where(b => b.Id == 5).Single().Comment;
                var bugResolutionForIdFive = context.Bugs.Where(b => b.Id == 5).Single().Resolution;
                var ownersCount = context.Owners.Count();
                Assert.Equal(ownersCount, results.Select(r => r.o.Id).Distinct().Count());
                for (int i = 0; i < ownersCount; i++)
                {
                    var bug = results[i].b;
                    Assert.True(bug.Id == -1 || bug.Id == 5);
                    Assert.True(bug.Comment == "Unknown" || bug.Comment == bugCommentForIdFive);
                    Assert.True(bug.Resolution == ArubaBugResolution.NoRepro || bug.Resolution == bugResolutionForIdFive);
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
[Extent2].[Id] AS [Id1], 
[Extent2].[Number] AS [Number], 
[Extent2].[Comment] AS [Comment], 
[Extent2].[Resolution] AS [Resolution], 
[Extent2].[Failure_Id] AS [Failure_Id], 
[Extent2].[ArubaOwner_Id] AS [ArubaOwner_Id]
FROM  [dbo].[ArubaFailures] AS [Extent1]
LEFT OUTER JOIN [dbo].[ArubaBugs] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Failure_Id]
WHERE [Extent1].[Discriminator] IN (N'ArubaBaseline',N'ArubaTestFailure',N'ArubaFailure')";

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
