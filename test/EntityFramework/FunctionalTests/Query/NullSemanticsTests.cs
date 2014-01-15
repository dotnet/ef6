// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.TestHelpers;
    using System.Data.SqlClient;
    using System.Linq;
    using SimpleModel;
    using Xunit;
    using Xunit.Extensions;
    using System.Data.Entity.Core.Common.CommandTrees;

    public class NullSemanticsTests : FunctionalTestBase
    {
        [Fact]
        [AutoRollback]
        [UseDefaultExecutionStrategy]
        public void Query_string_and_results_are_valid_for_column_equals_constant()
        {
            using (var context = new SimpleModelContext())
            {
                SetupContext(context);

                var query1 = context.Products.Where(p => p.Category.Id == "Fruit" && p.Name == "Grapes");
                var query2 = context.Products.Where(p => p.Category.Id == "Fruit" && "Grapes" == p.Name);
                var expectedSql =
@"SELECT 
    [Extent1].[Discriminator] AS [Discriminator], 
    [Extent1].[Id] AS [Id], 
    [Extent1].[CategoryId] AS [CategoryId], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[PromotionalCode] AS [PromotionalCode]
    FROM [dbo].[Products] AS [Extent1]
    WHERE ([Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND (N'Fruit' = [Extent1].[CategoryId]) AND (N'Grapes' = [Extent1].[Name])";

                QueryTestHelpers.VerifyDbQuery(query1, expectedSql);
                QueryTestHelpers.VerifyDbQuery(query2, expectedSql);
                Assert.Equal(1, query1.Count());
                Assert.Equal(1, query2.Count());
            }
        }

        [Fact]
        [AutoRollback]
        [UseDefaultExecutionStrategy]
        public void Query_string_and_results_are_valid_for_column_not_equal_constant()
        {
            using (var context = new SimpleModelContext())
            {
                SetupContext(context);

                var query1 = context.Products.Where(p => p.Category.Id == "Fruit" && p.Name != "Grapes");
                var query2 = context.Products.Where(p => p.Category.Id == "Fruit" && "Grapes" != p.Name);
                var query3 = context.Products.Where(p => p.Category.Id == "Fruit" && !("Grapes" == p.Name));
                var expectedSql =
@"SELECT 
    [Extent1].[Discriminator] AS [Discriminator], 
    [Extent1].[Id] AS [Id], 
    [Extent1].[CategoryId] AS [CategoryId], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[PromotionalCode] AS [PromotionalCode]
    FROM [dbo].[Products] AS [Extent1]
    WHERE ([Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND (N'Fruit' = [Extent1].[CategoryId]) AND ( NOT ((N'Grapes' = [Extent1].[Name]) AND ([Extent1].[Name] IS NOT NULL)))";

                QueryTestHelpers.VerifyDbQuery(query1, expectedSql);
                QueryTestHelpers.VerifyDbQuery(query2, expectedSql);
                QueryTestHelpers.VerifyDbQuery(query3, expectedSql);
                Assert.Equal(2, query1.Count());
            }
        }

        [Fact]
        [AutoRollback]
        [UseDefaultExecutionStrategy]
        public void Query_string_and_results_are_valid_for_column_equals_null()
        {
            using (var context = new SimpleModelContext())
            {
                SetupContext(context);

                var query1 = context.Products.Where(p => p.Category.Id == "Fruit" && p.Name == null);
                var query2 = context.Products.Where(p => p.Category.Id == "Fruit" && null == p.Name);
                var expectedSql =
@"SELECT 
    [Extent1].[Discriminator] AS [Discriminator], 
    [Extent1].[Id] AS [Id], 
    [Extent1].[CategoryId] AS [CategoryId], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[PromotionalCode] AS [PromotionalCode]
    FROM [dbo].[Products] AS [Extent1]
    WHERE ([Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND (N'Fruit' = [Extent1].[CategoryId]) AND ([Extent1].[Name] IS NULL)";

                QueryTestHelpers.VerifyDbQuery(query1, expectedSql);
                QueryTestHelpers.VerifyDbQuery(query2, expectedSql);
                Assert.Equal(1, query1.Count());
                Assert.Equal(1, query2.Count());
            }
        }

        [Fact]
        [AutoRollback]
        [UseDefaultExecutionStrategy]
        public void Query_string_and_results_are_valid_for_column_not_equal_null()
        {
            using (var context = new SimpleModelContext())
            {
                SetupContext(context);

                var query1 = context.Products.Where(p => p.Category.Id == "Fruit" && p.Name != null);
                var query2 = context.Products.Where(p => p.Category.Id == "Fruit" && null != p.Name);
                var query3 = context.Products.Where(p => p.Category.Id == "Fruit" && !(null == p.Name));
                var expectedSql =
@"SELECT 
    [Extent1].[Discriminator] AS [Discriminator], 
    [Extent1].[Id] AS [Id], 
    [Extent1].[CategoryId] AS [CategoryId], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[PromotionalCode] AS [PromotionalCode]
    FROM [dbo].[Products] AS [Extent1]
    WHERE ([Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND (N'Fruit' = [Extent1].[CategoryId]) AND ([Extent1].[Name] IS NOT NULL)";

                QueryTestHelpers.VerifyDbQuery(query1, expectedSql);
                QueryTestHelpers.VerifyDbQuery(query2, expectedSql);
                QueryTestHelpers.VerifyDbQuery(query3, expectedSql);
                Assert.Equal(2, query1.Count());
            }
        }

        [Fact]
        [AutoRollback]
        [UseDefaultExecutionStrategy]
        public void Query_string_and_results_are_valid_for_column_equals_parameter()
        {
            using (var context = new SimpleModelContext())
            {
                SetupContext(context);

                var parameter = "Bananas";
                var query1 = context.Products.Where(p => p.Category.Id == "Fruit" && p.Name == parameter);
                var query2 = context.Products.Where(p => p.Category.Id == "Fruit" && parameter == p.Name);
                var expectedSql1 =
@"SELECT 
    [Extent1].[Discriminator] AS [Discriminator], 
    [Extent1].[Id] AS [Id], 
    [Extent1].[CategoryId] AS [CategoryId], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[PromotionalCode] AS [PromotionalCode]
    FROM [dbo].[Products] AS [Extent1]
    WHERE ([Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND (N'Fruit' = [Extent1].[CategoryId]) AND (([Extent1].[Name] = @p__linq__0) OR (([Extent1].[Name] IS NULL) AND (@p__linq__0 IS NULL)))";
                var expectedSql2 =
@"SELECT 
    [Extent1].[Discriminator] AS [Discriminator], 
    [Extent1].[Id] AS [Id], 
    [Extent1].[CategoryId] AS [CategoryId], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[PromotionalCode] AS [PromotionalCode]
    FROM [dbo].[Products] AS [Extent1]
    WHERE ([Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND (N'Fruit' = [Extent1].[CategoryId]) AND ((@p__linq__0 = [Extent1].[Name]) OR ((@p__linq__0 IS NULL) AND ([Extent1].[Name] IS NULL)))";

                QueryTestHelpers.VerifyDbQuery(query1, expectedSql1);
                QueryTestHelpers.VerifyDbQuery(query2, expectedSql2);
                Assert.Equal(1, query1.Count());
                Assert.Equal(1, query2.Count());
            }
        }

        [Fact]
        [AutoRollback]
        [UseDefaultExecutionStrategy]
        public void Query_string_and_results_are_valid_for_column_not_equal_parameter()
        {
            using (var context = new SimpleModelContext())
            {
                SetupContext(context);

                var parameter = "Bananas";
                var query1 = context.Products.Where(p => p.Category.Id == "Fruit" && p.Name != parameter);
                var query2 = context.Products.Where(p => p.Category.Id == "Fruit" && parameter != p.Name);
                var query3 = context.Products.Where(p => p.Category.Id == "Fruit" && !(p.Name == parameter));

                parameter = null;
                var query4 = context.Products.Where(p => p.Category.Id == "Fruit" && p.Name != parameter);
                var query5 = context.Products.Where(p => p.Category.Id == "Fruit" && parameter != p.Name);
                var query6 = context.Products.Where(p => p.Category.Id == "Fruit" && !(p.Name == parameter));

                var expectedSql1 =
@"SELECT 
    [Extent1].[Discriminator] AS [Discriminator], 
    [Extent1].[Id] AS [Id], 
    [Extent1].[CategoryId] AS [CategoryId], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[PromotionalCode] AS [PromotionalCode]
    FROM [dbo].[Products] AS [Extent1]
    WHERE ([Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND (N'Fruit' = [Extent1].[CategoryId]) AND ( NOT (([Extent1].[Name] = @p__linq__0) AND ((CASE WHEN ([Extent1].[Name] IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END) = (CASE WHEN (@p__linq__0 IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END))))";
                var expectedSql2 =
@"SELECT 
    [Extent1].[Discriminator] AS [Discriminator], 
    [Extent1].[Id] AS [Id], 
    [Extent1].[CategoryId] AS [CategoryId], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[PromotionalCode] AS [PromotionalCode]
    FROM [dbo].[Products] AS [Extent1]
    WHERE ([Extent1].[Discriminator] IN (N'FeaturedProduct',N'Product')) AND (N'Fruit' = [Extent1].[CategoryId]) AND ( NOT ((@p__linq__0 = [Extent1].[Name]) AND ((CASE WHEN (@p__linq__0 IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END) = (CASE WHEN ([Extent1].[Name] IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END))))";

                QueryTestHelpers.VerifyDbQuery(query1, expectedSql1);
                QueryTestHelpers.VerifyDbQuery(query2, expectedSql2);
                QueryTestHelpers.VerifyDbQuery(query3, expectedSql1);
                QueryTestHelpers.VerifyDbQuery(query4, expectedSql1);
                QueryTestHelpers.VerifyDbQuery(query5, expectedSql2);
                QueryTestHelpers.VerifyDbQuery(query6, expectedSql1);
                Assert.Equal(2, query1.Count());
                Assert.Equal(2, query2.Count());
                Assert.Equal(2, query4.Count());
                Assert.Equal(2, query5.Count());
            }
        }

        private void SetupContext(SimpleModelContext context)
        {
            context.Configuration.UseDatabaseNullSemantics = false;

            context.Categories.Add(new Category { Id = "Fruit" });

            context.Products.Add(new Product { Name = "Grapes", CategoryId = "Fruit" });
            context.Products.Add(new Product { Name = null, CategoryId = "Fruit" });
            context.Products.Add(new Product { Name = "Bananas", CategoryId = "Fruit" });

            context.SaveChanges();
        }

        public class A
        {
            public int Id { get; set; }
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
        public void Null_checks_for_non_nullable_parameters_are_eliminated()
        {
            using (var context = new ABContext())
            {
                var aId = 1;
                var query = context.As.Where(a => a.Id != aId);
                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Name] AS [Name]
    FROM [dbo].[A] AS [Extent1]
    WHERE [Extent1].[Id] <> @p__linq__0";

                QueryTestHelpers.VerifyDbQuery(query, expectedSql);
            }
        }

        [Fact]
        public void Null_checks_for_nullable_parameters_are_not_eliminated()
        {
            using (var context = new ABContext())
            {
                int? aId = 1;
                var query = context.As.Where(a => a.Id != aId);
                var expectedSql =
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Name] AS [Name]
    FROM [dbo].[A] AS [Extent1]
    WHERE  NOT (([Extent1].[Id] = @p__linq__0) AND (0 = (CASE WHEN (@p__linq__0 IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END)))";

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
    [Extent1].[Name] AS [Name]
    FROM  [dbo].[A] AS [Extent1]
    INNER JOIN  (SELECT TOP (1) [c].[Name] AS [Name]
        FROM [dbo].[B] AS [c] ) AS [Limit1] ON [Extent1].[Name] = [Limit1].[Name]";

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
    [Extent1].[Name] AS [Name]
    FROM [dbo].[A] AS [Extent1]
    WHERE  NOT (
        (([Extent1].[Name] = @p__linq__0) AND ((CASE WHEN ([Extent1].[Name] IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END) = (CASE WHEN (@p__linq__0 IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END))) OR 
        ( NOT (([Extent1].[Name] = @p__linq__1) OR (([Extent1].[Name] IS NULL) AND (@p__linq__1 IS NULL)))) OR 
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
    WHEN ( NOT (([Extent1].[Name] = @p__linq__0) AND ((CASE WHEN ([Extent1].[Name] IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END) = (CASE WHEN (@p__linq__0 IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END)))) 
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
    CASE 
    WHEN ([Extent1].[Name] = @p__linq__0) 
        THEN cast(1 as bit) 
    WHEN ( NOT (([Extent1].[Name] = @p__linq__0) AND (0 = (CASE WHEN (@p__linq__0 IS NULL) THEN cast(1 as bit) ELSE cast(0 as bit) END))))
        THEN cast(0 as bit)
    END AS [C1]
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
    }
}
