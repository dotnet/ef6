// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.SqlClient;
    using System.Linq;
    using Xunit;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.ComponentModel.DataAnnotations.Schema;

    public class NullSemanticsTests : FunctionalTestBase
    {
        [Fact]
        public void Query_string_and_results_are_valid_for_column_equals_constant()
        {
            using (var context = new NullSemanticsContext())
            {
                var query1 = context.Entities.Where(e => e.Foo == "Foo" && e.Bar == "Bar");
                var query2 = context.Entities.Where(e => e.Foo == "Foo" && "Bar" == e.Bar);

                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Foo] AS [Foo], 
    [Extent1].[Bar] AS [Bar]
    FROM [dbo].[NullSemanticsEntities] AS [Extent1]
    WHERE (N'Foo' = [Extent1].[Foo]) AND (N'Bar' = [Extent1].[Bar])";

                QueryTestHelpers.VerifyDbQuery(query1, expectedSql);
                QueryTestHelpers.VerifyDbQuery(query2, expectedSql);

                var expected = context.Entities.ToList().Where(e => e.Foo == "Foo" && e.Bar == "Bar").ToList();

                Assert.Equal(expected.Count, query1.Count());
                Assert.Equal(expected.Count, query2.Count());
            }
        }

        [Fact]
        public void Query_string_and_results_are_valid_for_column_not_equal_constant()
        {
            using (var context = new NullSemanticsContext())
            {
                var query1 = context.Entities.Where(e => e.Foo == "Foo" && e.Bar != "Bar");
                var query2 = context.Entities.Where(e => e.Foo == "Foo" && "Bar" != e.Bar);
                var query3 = context.Entities.Where(e => e.Foo == "Foo" && !("Bar" == e.Bar));

                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Foo] AS [Foo], 
    [Extent1].[Bar] AS [Bar] 
    FROM [dbo].[NullSemanticsEntities] AS [Extent1]
    WHERE (N'Foo' = [Extent1].[Foo]) AND ( NOT ((N'Bar' = [Extent1].[Bar]) AND ([Extent1].[Bar] IS NOT NULL)))";

                QueryTestHelpers.VerifyDbQuery(query1, expectedSql);
                QueryTestHelpers.VerifyDbQuery(query2, expectedSql);
                QueryTestHelpers.VerifyDbQuery(query3, expectedSql);

                var expected = context.Entities.ToList().Where(e => e.Foo == "Foo" && e.Bar != "Bar");

                Assert.Equal(expected.Count(), query1.Count());
                Assert.Equal(expected.Count(), query2.Count());
                Assert.Equal(expected.Count(), query3.Count());
            }
        }

        [Fact]
        public void Query_string_and_results_are_valid_for_column_equals_null()
        {
            using (var context = new NullSemanticsContext())
            {
                var query1 = context.Entities.Where(e => e.Foo == "Foo" && e.Bar == null);
                var query2 = context.Entities.Where(e => e.Foo == "Foo" && null == e.Bar);
                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Foo] AS [Foo], 
    [Extent1].[Bar] AS [Bar]
    FROM [dbo].[NullSemanticsEntities] AS [Extent1]
    WHERE (N'Foo' = [Extent1].[Foo]) AND ([Extent1].[Bar] IS NULL)";

                QueryTestHelpers.VerifyDbQuery(query1, expectedSql);
                QueryTestHelpers.VerifyDbQuery(query2, expectedSql);

                var expected = context.Entities.ToList().Where(e => e.Foo == "Foo" && e.Bar == null);
                
                Assert.Equal(expected.Count(), query1.Count());
                Assert.Equal(expected.Count(), query2.Count());
            }
        }

        [Fact]
        public void Query_string_and_results_are_valid_for_column_not_equal_null()
        {
            using (var context = new NullSemanticsContext())
            {
                var query1 = context.Entities.Where(e => e.Foo == "Foo" && e.Bar != null);
                var query2 = context.Entities.Where(e => e.Foo == "Foo" && null != e.Bar);
                var query3 = context.Entities.Where(e => e.Foo == "Foo" && !(null == e.Bar));
                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Foo] AS [Foo], 
    [Extent1].[Bar] AS [Bar]
    FROM [dbo].[NullSemanticsEntities] AS [Extent1]
    WHERE (N'Foo' = [Extent1].[Foo]) AND ([Extent1].[Bar] IS NOT NULL)";

                QueryTestHelpers.VerifyDbQuery(query1, expectedSql);
                QueryTestHelpers.VerifyDbQuery(query2, expectedSql);
                QueryTestHelpers.VerifyDbQuery(query3, expectedSql);

                var expected = context.Entities.ToList().Where(e => e.Foo == "Foo" && e.Bar != null);
                
                Assert.Equal(expected.Count(), query1.Count());
            }
        }

        [Fact]
        public void Query_string_and_results_are_valid_for_column_equals_parameter()
        {
            using (var context = new NullSemanticsContext())
            {
                var parameter = "Bar";
                var query1 = context.Entities.Where(e => e.Foo == "Foo" && e.Bar == parameter);
                var query2 = context.Entities.Where(e => e.Foo == "Foo" && parameter == e.Bar);
                var expectedSql1 =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Foo] AS [Foo], 
    [Extent1].[Bar] AS [Bar]
    FROM [dbo].[NullSemanticsEntities] AS [Extent1]
    WHERE (N'Foo' = [Extent1].[Foo]) AND (([Extent1].[Bar] = @p__linq__0) OR (([Extent1].[Bar] IS NULL) AND (@p__linq__0 IS NULL)))";
                var expectedSql2 =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Foo] AS [Foo], 
    [Extent1].[Bar] AS [Bar]
    FROM [dbo].[NullSemanticsEntities] AS [Extent1]
    WHERE (N'Foo' = [Extent1].[Foo]) AND ((@p__linq__0 = [Extent1].[Bar]) OR ((@p__linq__0 IS NULL) AND ([Extent1].[Bar] IS NULL)))";

                QueryTestHelpers.VerifyDbQuery(query1, expectedSql1);
                QueryTestHelpers.VerifyDbQuery(query2, expectedSql2);

                var expected = context.Entities.Where(e => e.Foo == "Foo" && e.Bar == parameter);

                Assert.Equal(expected.Count(), query1.Count());
                Assert.Equal(expected.Count(), query2.Count());
            }
        }

        [Fact]
        public void Query_string_and_results_are_valid_for_column_not_equal_parameter()
        {
            using (var context = new NullSemanticsContext())
            {
                var parameter = "Bar";
                var query1 = context.Entities.Where(e => e.Foo == "Foo" && e.Bar != parameter);
                var query2 = context.Entities.Where(e => e.Foo == "Foo" && parameter != e.Bar);
                var query3 = context.Entities.Where(e => e.Foo == "Foo" && !(e.Bar == parameter));
                var expectedSql1 =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Foo] AS [Foo], 
    [Extent1].[Bar] AS [Bar]
    FROM [dbo].[NullSemanticsEntities] AS [Extent1]
    WHERE (N'Foo' = [Extent1].[Foo]) AND ( NOT (([Extent1].[Bar] = @p__linq__0) AND ((CASE WHEN ([Extent1].[Bar] IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END) = (CASE WHEN (@p__linq__0 IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END))))";
                var expectedSql2 =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Foo] AS [Foo], 
    [Extent1].[Bar] AS [Bar]
    FROM [dbo].[NullSemanticsEntities] AS [Extent1]
    WHERE (N'Foo' = [Extent1].[Foo]) AND ( NOT ((@p__linq__0 = [Extent1].[Bar]) AND ((CASE WHEN (@p__linq__0 IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END) = (CASE WHEN ([Extent1].[Bar] IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END))))";

                QueryTestHelpers.VerifyDbQuery(query1, expectedSql1);
                QueryTestHelpers.VerifyDbQuery(query2, expectedSql2);
                QueryTestHelpers.VerifyDbQuery(query3, expectedSql1);

                var expected = context.Entities.ToList().Where(e => e.Foo == "Foo" && e.Bar != parameter);

                Assert.Equal(expected.Count(), query1.Count());
                Assert.Equal(expected.Count(), query2.Count());
            }
        }

        [Fact]
        public void Query_string_and_results_are_valid_for_column_compared_with_other_column()
        {
            using (var context = new NullSemanticsContext())
            {
                var query1 = context.Entities.Where(e => e.Foo == e.Bar);
                var query2 = context.Entities.Where(e => e.Foo != e.Bar);
                var query3 = context.Entities.Where(e => !(e.Foo == e.Bar));

                var expectedSql1 =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Foo] AS [Foo], 
    [Extent1].[Bar] AS [Bar]
    FROM [dbo].[NullSemanticsEntities] AS [Extent1]
    WHERE ([Extent1].[Foo] = [Extent1].[Bar]) OR (([Extent1].[Foo] IS NULL) AND ([Extent1].[Bar] IS NULL))";

                var expectedSql2 =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Foo] AS [Foo], 
    [Extent1].[Bar] AS [Bar]
    FROM [dbo].[NullSemanticsEntities] AS [Extent1]
    WHERE  NOT (([Extent1].[Foo] = [Extent1].[Bar]) AND ((CASE WHEN ([Extent1].[Foo] IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END) = (CASE WHEN ([Extent1].[Bar] IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END)))";

                QueryTestHelpers.VerifyDbQuery(query1, expectedSql1);
                QueryTestHelpers.VerifyDbQuery(query2, expectedSql2);
                QueryTestHelpers.VerifyDbQuery(query3, expectedSql2);

                var expected1 = context.Entities.ToList().Where(e => e.Foo == e.Bar);
                var expected2 = context.Entities.ToList().Where(e => e.Foo != e.Bar);

                Assert.Equal(expected1.Count(), query1.Count());
                Assert.Equal(expected2.Count(), query2.Count());
                Assert.Equal(expected2.Count(), query3.Count());
            }
        }

        [Fact]
        public void Query_with_comparison_in_projection_works_with_clr_semantics()
        {
            using (var context = new NullSemanticsContext())
            {
                var query = context.Entities.Select(e => e.Foo == e.Bar);
                var expectedSql =
@"SELECT 
    CASE WHEN (([Extent1].[Foo] = [Extent1].[Bar]) OR (([Extent1].[Foo] IS NULL) AND ([Extent1].[Bar] IS NULL))) THEN cast(1 as bit) WHEN ( NOT (([Extent1].[Foo] = [Extent1].[Bar]) AND ((CASE WHEN ([Extent1].[Foo] IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END) = (CASE WHEN ([Extent1].[Bar] IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END)))) THEN cast(0 as bit) END AS [C1]
    FROM [dbo].[NullSemanticsEntities] AS [Extent1]";

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var expected = context.Entities.ToList().Select(e => e.Foo == e.Bar).ToList();

                QueryTestHelpers.VerifyQueryResult(expected, query.ToList(), (o, i) => o == i);
            }
        }

        [Fact]
        public void Query_with_comparison_of_function_result_with_nullable_parameters_works()
        {
            using (var context = new NullSemanticsContext())
            {
                var query = context.Entities.Where(e => e.Foo.Length == e.Bar.Length);

                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Foo] AS [Foo], 
    [Extent1].[Bar] AS [Bar]
    FROM [dbo].[NullSemanticsEntities] AS [Extent1]
    WHERE (( CAST(LEN([Extent1].[Foo]) AS int)) = ( CAST(LEN([Extent1].[Bar]) AS int))) OR (([Extent1].[Foo] IS NULL) AND ([Extent1].[Bar] IS NULL))";

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Query_with_Count_with_predicate_works_with_clr_semantics()
        {
            using (var context = new NullSemanticsContext())
            {
                var query = context.Entities.Count(e => e.Foo == e.Bar);
                var expected = context.Entities.ToList().Count(e => e.Foo == e.Bar);

                Assert.Equal(query, expected);
            }
        }

        [Fact]
        public void Query_with_All_with_predicate_works_with_clr_semantics()
        {
            using (var context = new NullSemanticsContext())
            {
                var query1 = context.Entities.Where(e => e.Id == 1 || e.Id == 5).All(e => e.Foo == e.Bar);
                var query2 = context.Entities.Where(e => e.Id == 5).All(e => e.Foo != e.Bar);
                var expected1 = context.Entities.ToList().Where(e => e.Id == 1 || e.Id == 5).All(e => e.Foo == e.Bar);
                var expected2 = context.Entities.ToList().Where(e => e.Id == 5).All(e => e.Foo != e.Bar);

                Assert.Equal(query1, expected1);
                Assert.Equal(query2, expected2);
            }
        }

        [Fact]
        public void Query_with_Any_with_predicate_works_with_clr_semantics()
        {
            using (var context = new NullSemanticsContext())
            {
                var query1 = context.Entities.Where(e => e.Id != 1).Any(e => e.Foo == e.Bar);
                var query2 = context.Entities.Where(e => e.Id == 5).Any(e => e.Foo != e.Bar);
                var expected1 = context.Entities.ToList().Where(e => e.Id != 1).Any(e => e.Foo == e.Bar);
                var expected2 = context.Entities.ToList().Where(e => e.Id == 5).Any(e => e.Foo != e.Bar);

                Assert.Equal(query1, expected1);
                Assert.Equal(query2, expected2);
            }
        }

        [Fact]
        public void Negation_counting_does_not_get_propagated_over_unnecessary_operators()
        {
            using (var context = new NullSemanticsContext())
            {
                var query1 = context.Entities.Where(e => (e.Foo == e.Bar ? 3 : 4) + (!(e.Foo == e.Bar) ? 4 : 3) != 6).Count();
                var expected1 = context.Entities.ToList().Where(e => (e.Foo == e.Bar ? 3 : 4) + (!(e.Foo == e.Bar) ? 4 : 3) != 6).Count();

                Assert.Equal(query1, expected1);
            }
        }

        [Fact]
        public void Query_with_comparison_in_subquery_works_with_clr_semantics()
        {
            using (var context = new NullSemanticsContext())
            {
                var query = context.Entities.Where(c => context.Entities.Where(e => e.Foo != e.Bar).Count() == context.Entities.Where(e => e.Foo != e.Bar).FirstOrDefault().Id);

                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Foo] AS [Foo], 
    [Extent1].[Bar] AS [Bar]
    FROM   [dbo].[NullSemanticsEntities] AS [Extent1]
    LEFT OUTER JOIN  (SELECT TOP (1) [Extent2].[Id] AS [Id]
        FROM [dbo].[NullSemanticsEntities] AS [Extent2]
        WHERE  NOT (([Extent2].[Foo] = [Extent2].[Bar]) AND ((CASE WHEN ([Extent2].[Foo] IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END) = (CASE WHEN ([Extent2].[Bar] IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END))) ) AS [Limit1] ON 1 = 1
    INNER JOIN  (SELECT 
        COUNT(1) AS [A1]
        FROM [dbo].[NullSemanticsEntities] AS [Extent3]
        WHERE  NOT (([Extent3].[Foo] = [Extent3].[Bar]) AND ((CASE WHEN ([Extent3].[Foo] IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END) = (CASE WHEN ([Extent3].[Bar] IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END))) ) AS [GroupBy1] ON ([GroupBy1].[A1] = [Limit1].[Id]) OR (([GroupBy1].[A1] IS NULL) AND ([Limit1].[Id] IS NULL))";

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);

                var expected = context.Entities.ToList().Where(c => context.Entities.ToList().Where(e => e.Foo == e.Bar).Count() != context.Entities.ToList().Where(e => e.Foo != e.Bar).FirstOrDefault().Id).ToList();

                QueryTestHelpers.VerifyQueryResult(expected, query.ToList(), (o, i) => o == i);
            }
        }

        private class NullSemanticsContext : DbContext
        {
            public NullSemanticsContext()
            {
                Database.SetInitializer(new NullSemanticsContextInitializer());
            }

            public DbSet<NullSemanticsEntity> Entities { get; set; }
        }

        private class NullSemanticsEntity
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        private class NullSemanticsContextInitializer : DropCreateDatabaseIfModelChanges<NullSemanticsContext>
        {
            protected override void Seed(NullSemanticsContext context)
            {
                var e1 = new NullSemanticsEntity { Id = 1, Foo = "Foo", Bar = "Foo" };
                var e2 = new NullSemanticsEntity { Id = 2, Foo = "Foo", Bar = "Bar" };
                var e3 = new NullSemanticsEntity { Id = 3, Foo = "Foo", Bar = null };
                var e4 = new NullSemanticsEntity { Id = 4, Foo = null, Bar = "Bar" };
                var e5 = new NullSemanticsEntity { Id = 5, Foo = null, Bar = null };

                context.Entities.AddRange(new[] { e1, e2, e3, e4, e5 });
                context.SaveChanges();
            }
        }

        public class A
        {
            public int Id { get; set; }
            public int? NullableId { get; set; }
            public string Name { get; set; }
        }

        public class B
        {
            public int Id { get; set; }
            [Required]
            public string Name { get; set; }
        }

        public class ABContext : DbContext
        {
            static ABContext()
            {
                Database.SetInitializer<ABContext>(null);
            }

            public ABContext()
            {
                Configuration.UseDatabaseNullSemantics = false;
            }

            public DbSet<A> As { get; set; }
            public DbSet<B> Bs { get; set; }
        }

        [Fact]
        public void Null_checks_for_non_nullable_parameters_are_eliminated_when_other_operand_is_not_nullable()
        {
            using (var context = new ABContext())
            {
                var aId = 1;
                var query = context.As.Where(a => a.Id == aId);
                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[NullableId] AS [NullableId], 
    [Extent1].[Name] AS [Name]
    FROM [dbo].[A] AS [Extent1]
    WHERE [Extent1].[Id] = @p__linq__0";

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Null_checks_for_non_nullable_parameters_are_eliminated_when_other_operand_is_nullable()
        {
            using (var context = new ABContext())
            {
                var aId = 1;
                var query = context.As.Where(a => a.NullableId == aId);
                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[NullableId] AS [NullableId], 
    [Extent1].[Name] AS [Name]
    FROM [dbo].[A] AS [Extent1]
    WHERE [Extent1].[NullableId] = @p__linq__0";

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Null_checks_for_nullable_parameters_are_eliminated_when_other_operand_is_not_nullable()
        {
            using (var context = new ABContext())
            {
                int? aId = 1;
                var query = context.As.Where(a => a.Id == aId);
                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[NullableId] AS [NullableId], 
    [Extent1].[Name] AS [Name]
    FROM [dbo].[A] AS [Extent1]
    WHERE [Extent1].[Id] = @p__linq__0";

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Null_checks_for_nullable_parameters_are_not_eliminated_when_other_operand_is_nullable()
        {
            using (var context = new ABContext())
            {
                int? aId = 1;
                var query = context.As.Where(a => a.NullableId == aId);
                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[NullableId] AS [NullableId], 
    [Extent1].[Name] AS [Name]
    FROM [dbo].[A] AS [Extent1]
    WHERE ([Extent1].[NullableId] = @p__linq__0) OR (([Extent1].[NullableId] IS NULL) AND (@p__linq__0 IS NULL))";

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Duplicate_joins_are_not_created()
        {
            using (var context = new ABContext())
            {
                var query =
                    from a in context.As
                    where a.Name == context.Bs.FirstOrDefault().Name
                    select a;

                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[NullableId] AS [NullableId], 
    [Extent1].[Name] AS [Name]
    FROM  [dbo].[A] AS [Extent1]
    LEFT OUTER JOIN  (SELECT TOP (1) [c].[Name] AS [Name]
        FROM [dbo].[B] AS [c] ) AS [Limit1] ON 1 = 1
    WHERE ([Extent1].[Name] = [Limit1].[Name]) OR (([Extent1].[Name] IS NULL) AND ([Limit1].[Name] IS NULL))";

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Inner_equality_comparisons_are_expanded_correctly()
        {
            using (var context = new ABContext())
            {
                var name1 = "ab1";
                var name2 = "ab2";
                var name3 = "ab3";
                var query =
                    from a in context.As
                    where !(a.Name == name1 || a.Name != name2 || !(a.Name == name3))
                    select a;
                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[NullableId] AS [NullableId], 
    [Extent1].[Name] AS [Name]
    FROM [dbo].[A] AS [Extent1]
    WHERE  NOT (
        (([Extent1].[Name] = @p__linq__0) AND 
        ((CASE 
            WHEN ([Extent1].[Name] IS NULL) 
            THEN cast(1 as bit) 
            ELSE cast(0 as bit) 
        END) = 
        (CASE 
            WHEN (@p__linq__0 IS NULL) 
            THEN cast(1 as bit) 
            ELSE cast(0 as bit) 
        END))) OR 
        ( NOT (([Extent1].[Name] = @p__linq__1) OR 
        (([Extent1].[Name] IS NULL) AND (@p__linq__1 IS NULL)))) OR 
        ( NOT (([Extent1].[Name] = @p__linq__2) OR (([Extent1].[Name] IS NULL) AND (@p__linq__2 IS NULL)))))";

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Equality_comparison_is_expanded_correctly_for_case_statement()
        {
            using (var context = new ABContext())
            {
                var name = "ab";
                var query =
                    from a in context.As
                    select a.Name == name;
                var expectedSql =
@"SELECT 
    CASE 
    WHEN (([Extent1].[Name] = @p__linq__0) OR (([Extent1].[Name] IS NULL) AND (@p__linq__0 IS NULL))) 
        THEN cast(1 as bit) 
    WHEN ( NOT (([Extent1].[Name] = @p__linq__0) AND ((
        CASE 
        WHEN ([Extent1].[Name] IS NULL) 
            THEN cast(1 as bit) 
        ELSE cast(0 as bit) END) = 
        (CASE 
        WHEN (@p__linq__0 IS NULL) 
            THEN cast(1 as bit) 
        ELSE cast(0 as bit) 
        END)))) 
        THEN cast(0 as bit) 
    END AS [C1]
    FROM [dbo].[A] AS [Extent1]";

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Equality_comparison_is_expanded_correctly_for_comparison_case_statement()
        {
            using (var context = new ABContext())
            {
                var aId = 1;
                var query =
                    from a in context.As
                    select a.NullableId > aId;
                var expectedSql =
@"SELECT 
    CASE 
    WHEN ([Extent1].[NullableId] > @p__linq__0) 
        THEN cast(1 as bit) 
    WHEN ( NOT (([Extent1].[NullableId] > @p__linq__0) AND ((
            CASE 
            WHEN ([Extent1].[NullableId] IS NULL) 
                THEN cast(1 as bit)
            ELSE 
                cast(0 as bit) 
            END) = 0)))
        THEN cast(0 as bit) 
    END AS [C1]
    FROM [dbo].[A] AS [Extent1]";

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Null_checks_for_non_nullable_columns_are_eliminated_from_case_statement()
        {
            using (var context = new ABContext())
            {
                var name = "ab";
                var query =
                    from b in context.Bs
                    select b.Name == name;
                var expectedSql =
@"SELECT 
    CASE WHEN ([Extent1].[Name] = @p__linq__0) THEN cast(1 as bit) WHEN ( NOT (([Extent1].[Name] = @p__linq__0) AND (0 = (CASE WHEN (@p__linq__0 IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END)))) THEN cast(0 as bit) END AS [C1]
    FROM [dbo].[B] AS [Extent1]";

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }            
        }

        [Fact]
        public void Query_containing_NE_expression_is_expanded_correctly()
        {
            var workspace = QueryTestHelpers.CreateMetadataWorkspace(
                ProductModel.Csdl, ProductModel.Ssdl, ProductModel.Msl);

            var entitySet = workspace
                .GetEntityContainer("ProductContainer", DataSpace.CSpace)
                .GetEntitySetByName("Products", false);

            var query = entitySet.Scan().Where(e => e.Property("ProductName").NotEqual("P"));

            var expectedSql =
@"SELECT 
    [Extent1].[Discontinued] AS [Discontinued], 
    [Extent1].[ProductID] AS [ProductID], 
    [Extent1].[ProductName] AS [ProductName], 
    [Extent1].[ReorderLevel] AS [ReorderLevel]
    FROM [dbo].[Products] AS [Extent1]
    WHERE ([Extent1].[Discontinued] IN (0,1)) AND ([Extent1].[ProductName] <> N'P')";

            var providerServices =
                (DbProviderServices)((IServiceProvider)EntityProviderFactory.Instance)
                    .GetService(typeof(DbProviderServices));
            var connection = new EntityConnection(workspace, new SqlConnection());
            var commandTree = new DbQueryCommandTree(workspace, DataSpace.CSpace, query, true, false);

            var entityCommand = (EntityCommand)providerServices.CreateCommandDefinition(commandTree).CreateCommand();
            entityCommand.Connection = connection;

            Assert.Equal(
                QueryTestHelpers.StripFormatting(expectedSql), 
                QueryTestHelpers.StripFormatting(entityCommand.ToTraceString()));
        }

        public class CodePlex2187 : FunctionalTestBase
        {
            public class Order
            {
                public int Id { get; set; }
                public int Group { get; set; }
                public Invoice Invoice { get; set; }
            }

            public class Invoice
            {
                public int Id { get; set; }
                public Order Order { get; set; }
            }

            public class Context : DbContext
            {
                static Context()
                {
                    Database.SetInitializer(new Initializer());
                }

                public DbSet<Order> Orders { get; set; }
                public DbSet<Invoice> Invoices { get; set; }

                protected override void OnModelCreating(DbModelBuilder builder)
                {
                    builder.Entity<Order>().HasOptional(e => e.Invoice).WithRequired(e => e.Order);
                }
            }

            public class Initializer : DropCreateDatabaseIfModelChanges<Context>
            {
                protected override void Seed(Context context)
                {
                    context.Orders.Add(new Order() { Group = 5 });
                    context.Orders.Add(new Order() { Group = 5 });
                    context.Orders.Add(new Order() { Group = 5 });
                }
            }

            [Fact]
            public void EQ_is_not_expanded_when_rewriting_navigation_properties()
            {
                using (var context = new Context())
                {
                    var query = context.Orders.Where(order => order.Invoice.Order.Group < 10);

                    QueryTestHelpers.VerifyDbQuery(
                        query,
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Group] AS [Group]
    FROM  [dbo].[Orders] AS [Extent1]
    INNER JOIN  (SELECT [Extent2].[Group] AS [Group], [Extent4].[Id] AS [Id1]
        FROM   [dbo].[Orders] AS [Extent2]
        LEFT OUTER JOIN [dbo].[Invoices] AS [Extent3] ON [Extent2].[Id] = [Extent3].[Id]
        INNER JOIN [dbo].[Invoices] AS [Extent4] ON [Extent4].[Id] = [Extent3].[Id] ) AS [Join2] ON [Extent1].[Id] = [Join2].[Id1]
    WHERE [Join2].[Group] < 10");

                    Assert.Equal(0, query.Count());
                }
            }
        }
    }
}
