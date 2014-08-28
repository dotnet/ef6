// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Xunit;

    public class JoinEliminationTests : FunctionalTestBase
    {
        public class Codeplex655Context : DbContext
        {
            public DbSet<Customer> Customers { get; set; }
            public DbSet<Document> Documents { get; set; }
            public DbSet<DocumentDetail> DocumentDetails { get; set; }

            public class Customer
            {
                [Key]
                public int? CustomerId { get; set; }
                public int PersonId { get; set; }
            }

            public class Document
            {
                [Key]
                public int? DocumentId { get; set; }
                public int? CustomerId { get; set; }
                [ForeignKey("CustomerId")]
                public Customer Customer { get; set; }
            }

            public class DocumentDetail
            {
                [Key]
                public int? DocumentDetailId { get; set; }
                public int? DocumentId { get; set; }
                [ForeignKey("DocumentId")]
                public Document Document { get; set; }
            }
        }

        [Fact] // Codeplex #655
        public static void LeftOuterJoin_duplicates_are_eliminated()
        {
            const string expectedSql =
@"SELECT 
[Extent1].[DocumentDetailId] AS [DocumentDetailId], 
[Extent1].[DocumentId] AS [DocumentId] 
FROM [dbo].[DocumentDetails] AS [Extent1] 
LEFT OUTER JOIN [dbo].[Documents] AS [Extent2] ON [Extent1].[DocumentId] = [Extent2].[DocumentId] 
LEFT OUTER JOIN [dbo].[Customers] AS [Extent3] ON [Extent2].[CustomerId] = [Extent3].[CustomerId] 
WHERE [Extent3].[PersonId] IN (1,2)";

            Database.SetInitializer<Codeplex655Context>(null);

            using (var context = new Codeplex655Context())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                var query = context.DocumentDetails.Where(x => x.Document.Customer.PersonId == 1 || x.Document.Customer.PersonId == 2);

                QueryTestHelpers.VerifyQuery(query, expectedSql);
            }
        }

        public class Codeplex960Context : DbContext
        {
            public DbSet<Blog> Blogs { get; set; }
            public DbSet<Post> Posts { get; set; }

            public class Blog
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }

            public class Post
            {
                public int Id { get; set; }
                public int BlogId { get; set; }
                public int? ParentPostId { get; set; }
                public string Text { get; set; }

                [ForeignKey("BlogId")]
                public virtual Blog Blog { get; set; }
                [ForeignKey("ParentPostId")]
                public virtual Post ParentPost { get; set; }
            }
        }

        [Fact] // Codeplex #960
        public static void LeftOuterJoin_is_not_turned_into_inner_join_if_nullable_foreign_key()
        {
            const string expectedSql = 
@"SELECT
[Extent1].[Id] AS [Id],
[Extent1].[Text] AS [Text],
[Extent3].[Name] AS [Name]
FROM   [dbo].[Posts] AS [Extent1]
LEFT OUTER JOIN [dbo].[Posts] AS [Extent2] ON [Extent1].[ParentPostId] = [Extent2].[Id]
LEFT OUTER JOIN [dbo].[Blogs] AS [Extent3] ON [Extent2].[BlogId] = [Extent3].[Id]
WHERE 1 = [Extent1].[Id]";

            Database.SetInitializer<Codeplex960Context>(null);

            using (var context = new Codeplex960Context())
            {
                context.Configuration.UseDatabaseNullSemantics = true;

                var query = from p in context.Posts where p.Id == 1 select new { p.Id, p.Text, p.ParentPost.Blog.Name };

                QueryTestHelpers.VerifyQuery(query, expectedSql);
            }
        }

        public partial class Codeplex199Context : DbContext
        {
            public DbSet<Product> Products { get; set; }
            public DbSet<ProductModel> ProductModels { get; set; }
            public DbSet<String> Strings { get; set; }
            public DbSet<StringInstrument> StringInstruments { get; set; }

            public partial class Product
            {
                public int ProductID { get; set; }
                public string Name { get; set; }
                public Nullable<int> ProductModelID { get; set; }
                public virtual ProductModel ProductModel { get; set; }
                public Nullable<int> StringInstrumentId { get; set; }
                public virtual StringInstrument StringInstrument { get; set; }
            }

            public partial class ProductModel
            {
                public ProductModel()
                {
                    this.Products = new List<Product>();
                }

                public int ProductModelID { get; set; }
                public DateTime ModifiedDate { get; set; }
                public virtual ICollection<Product> Products { get; set; }
            }

            public partial class String
            {
                public int StringId { get; set; }
                public string Name { get; set; }
                public int StringInstrumentId { get; set; }
                public virtual StringInstrument StringInstrument { get; set; }
            }

            public partial class StringInstrument
            {
                public StringInstrument()
                {
                    this.Strings = new List<String>();
                }

                public int StringInstrumentId { get; set; }
                public DateTime ProductionDate { get; set; }
                public virtual ICollection<String> Strings { get; set; }
            }
        }

        [Fact]
        public static void Making_use_of_variable_multiple_times_doesnt_cause_redundant_joins()
        {
            const string expectedSqlProducts =
@"SELECT
    [Extent1].[Name] AS [Name]
    FROM   [dbo].[Products] AS [Extent1]
    LEFT OUTER JOIN [dbo].[ProductModels] AS [Extent2] ON [Extent1].[ProductModelID] = [Extent2].[ProductModelID]
    INNER JOIN [dbo].[StringInstruments] AS [Extent3] ON [Extent1].[StringInstrumentId] = [Extent3].[StringInstrumentId]
    WHERE ([Extent2].[ModifiedDate] >= @p__linq__0) AND ([Extent3].[ProductionDate]=@p__linq__1) AND ([Extent2].[ModifiedDate] <= @p__linq__2)";

            const string expectedSqlStrings = 
@"SELECT
    [Extent1].[Name] AS [Name]
    FROM  [dbo].[Strings] AS [Extent1]
    INNER JOIN [dbo].[StringInstruments] AS [Extent2] ON [Extent1].[StringInstrumentId] = [Extent2].[StringInstrumentId]
    WHERE ([Extent2].[ProductionDate] >= @p__linq__0) AND ([Extent2].[ProductionDate] <= @p__linq__1)";

            Database.SetInitializer<Codeplex199Context>(null);

            using (var context = new Codeplex199Context())
            {
                context.Configuration.UseDatabaseNullSemantics = true;
                context.Configuration.LazyLoadingEnabled = false;

                var MinDate = new DateTime(2011, 02, 03);
                var MaxDate = new DateTime(2011, 03, 04);
                var ProductionDate = new DateTime(2011, 03, 04);

                var query = context.Products
                                   .Where(p => p.ProductModel.ModifiedDate >= MinDate
                                               && p.StringInstrument.ProductionDate == ProductionDate
                                               && p.ProductModel.ModifiedDate <= MaxDate)
                                   .Select(p => p.Name);

                QueryTestHelpers.VerifyQuery(query, expectedSqlProducts);

                query = context.Strings
                               .Where(s => s.StringInstrument.ProductionDate >= MinDate
                                           && s.StringInstrument.ProductionDate <= MaxDate)
                               .Select(s => s.Name);

                QueryTestHelpers.VerifyQuery(query, expectedSqlStrings);
            }
        }

        public class CodePlex2296 : FunctionalTestBase
        {
            public class MyBase
            {
                public int Id { get; set; }
            }

            public class MyDerived1 : MyBase
            {
                public string Name { get; set; }
                public MyDerived2 Derived2 { get; set; }
            }

            public class MyDerived2 : MyBase
            {
                public string Name { get; set; }
                public MyDerived1 Derived1 { get; set; }
            }

            public class MyContext : DbContext
            {
                static MyContext()
                {
                    Database.SetInitializer<MyContext>(null);
                }

                public DbSet<MyBase> Bases { get; set; }

                protected override void OnModelCreating(DbModelBuilder builder)
                {
                    builder.Entity<MyBase>().ToTable("MyBase");
                    builder.Entity<MyDerived2>().HasOptional(e => e.Derived1).WithOptionalDependent(e => e.Derived2);
                    builder.Entity<MyDerived1>().ToTable("Derived1");
                    builder.Entity<MyDerived2>().ToTable("Derived2");
                }
            }

            [Fact]
            public void Joins_are_eliminated_and_expression_is_simplified()
            {
                using (var context = new MyContext())
                {
                    var query = context.Bases.OfType<MyDerived2>().Where(e => e.Derived1.Derived2.Name == "Foo");

                    QueryTestHelpers.VerifyQuery(
                        query,
@"SELECT 
    [Extent1].[Id] AS [Id], 
    '0X0X' AS [C1], 
    [Extent1].[Name] AS [Name], 
    CAST(NULL AS varchar(1)) AS [C2], 
    [Extent1].[Derived1_Id] AS [Derived1_Id]
    FROM  [dbo].[Derived2] AS [Extent1]
    INNER JOIN [dbo].[Derived2] AS [Extent2] ON ([Extent2].[Derived1_Id] = [Extent1].[Derived1_Id]) AND ([Extent1].[Derived1_Id] IS NOT NULL)
    WHERE ([Extent2].[Derived1_Id] IS NOT NULL) AND (N'Foo' = [Extent2].[Name])");
                }
            }
        }

        public class CodePlex2256 : FunctionalTestBase
        {
            public class A
            {
                public int Id { get; set; }

                public B B { get; set; }
            }

            public class B
            {
                public B()
                {
                    As = new List<A>();
                }

                public int Id { get; set; }

                public C C { get; set; }
                public ICollection<A> As { get; set; } 
            }

            public class C
            {
                public C()
                {
                    Bs = new List<B>();
                }

                public int Id { get; set; }

                public D D { get; set; }
                public ICollection<B> Bs { get; set; }
            }

            public class D
            {
                public D()
                {
                    Cs = new List<C>();
                }

                public int Id { get; set; }

                public E E { get; set; }
                public ICollection<C> Cs { get; set; }
            }

            public class E
            {
                public E()
                {
                    Ds = new List<D>();
                }

                public int Id { get; set; }

                public ICollection<D> Ds { get; set; }
            }

            public class MyContext : DbContext
            {
                static MyContext()
                {
                    Database.SetInitializer<MyContext>(null);
                }

                public DbSet<A> As { get; set; }

                protected override void OnModelCreating(DbModelBuilder modelBuilder)
                {
                    modelBuilder.Entity<A>().HasRequired(e => e.B).WithMany(e => e.As);
                    modelBuilder.Entity<B>().HasRequired(e => e.C).WithMany(e => e.Bs);
                    modelBuilder.Entity<C>().HasRequired(e => e.D).WithMany(e => e.Cs);
                    modelBuilder.Entity<D>().HasRequired(e => e.E).WithMany(e => e.Ds);
                }
            }

            [Fact]
            public void Joins_are_eliminated_and_expression_is_simplified()
            {
                using (var context = new MyContext())
                {
                    var query = context.As.Include(a => a.B.C.D.E);

                    QueryTestHelpers.VerifyQuery(
                        query,
@"SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent2].[Id] AS [Id1], 
    [Extent3].[Id] AS [Id2], 
    [Extent4].[Id] AS [Id3], 
    [Extent4].[E_Id] AS [E_Id]
    FROM    [dbo].[A] AS [Extent1]
    INNER JOIN [dbo].[B] AS [Extent2] ON [Extent1].[B_Id] = [Extent2].[Id]
    INNER JOIN [dbo].[C] AS [Extent3] ON [Extent2].[C_Id] = [Extent3].[Id]
    INNER JOIN [dbo].[D] AS [Extent4] ON [Extent3].[D_Id] = [Extent4].[Id]");
                }
            }
        }

        public class CodePlex2196 : FunctionalTestBase
        {
            public class Context : DbContext
            {
                static Context()
                {
                    Database.SetInitializer<Context>(null);
                }

                public DbSet<BaseType> BaseTypes { get; set; }
                public DbSet<QueryType> QueryTypes { get; set; }
            }

            public abstract class BaseType
            {
                public int Id { get; set; }
                public byte Type { get; set; }
                public string Code { get; set; }
                public string Name { get; set; }
                public string Description { get; set; }
                public DateTimeOffset RecordDate { get; set; }
            }

            public class QueryType
            {
                public int Id { get; set; }
                public string Code { get; set; }
                public string Name { get; set; }
                public string Description { get; set; }
                public DateTimeOffset RecordDate { get; set; }

                public int RelatedTypeId { get; set; }

                public virtual BaseType RelatedType { get; set; }
            }

            public class DerivedTypeA : BaseType
            {
                public string PropertyA { get; set; }
            }

            public class DerivedTypeB : BaseType
            {
                public string PropertyB { get; set; }
            }

            public class DerivedTypeC : BaseType
            {
                public string PropertyC { get; set; }
            }

            public class DerivedTypeD : BaseType
            {
                public string PropertyD { get; set; }
            }

            public class DerivedTypeE : BaseType
            {
                public string PropertyE { get; set; }
            }

            [Fact]
            public void Duplicate_joins_with_non_equi_join_predicates_are_eliminated()
            {
                using (var context = new Context())
                {
                    var query
                        = context.QueryTypes
                            .OrderBy(x => x.Id)
                            .Select(
                                x =>
                                    new
                                    {
                                        x.RelatedType.Id,
                                        x.RelatedType.RecordDate,
                                        x.RelatedType.Name,
                                        x.RelatedType.Type,
                                        x.RelatedType.Code
                                    })
                            .Take(10);

                    QueryTestHelpers.VerifyQuery(
                        query,
@"SELECT TOP (10) 
    [Extent1].[Id] AS [Id], 
    [Extent1].[RelatedTypeId] AS [RelatedTypeId], 
    [Extent2].[RecordDate] AS [RecordDate], 
    [Extent2].[Name] AS [Name], 
    [Extent2].[Type] AS [Type], 
    [Extent2].[Code] AS [Code]
    FROM  [dbo].[QueryTypes] AS [Extent1]
    LEFT OUTER JOIN [dbo].[BaseTypes] AS [Extent2] ON ([Extent2].[Discriminator] IN (N'DerivedTypeA',N'DerivedTypeB',N'DerivedTypeC',N'DerivedTypeD',N'DerivedTypeE')) AND ([Extent1].[RelatedTypeId] = [Extent2].[Id])
    ORDER BY [Extent1].[Id] ASC");
                }
            }
        }

        public class CodePlex2369 : FunctionalTestBase
        {
            public class A
            {
                public int Id { get; set; }

                public B B { get; set; }
            }

            public class B
            {
                public int Id { get; set; }
                public string Name { get; set; }

                public A A { get; set; }
            }

            public class Context : DbContext
            {
                static Context()
                {
                    Database.SetInitializer<Context>(null);
                }

                public DbSet<A> As { get; set; }

                protected override void OnModelCreating(DbModelBuilder builder)
                {
                    builder.Entity<B>().HasRequired(b => b.A).WithRequiredPrincipal(a => a.B);
                }
            }

            [Fact]
            public void Unnecessary_joins_are_eliminated_and_query_is_simplified()
            {
                using (var context = new Context())
                {
                    var query = from a in context.As select a.B.Name;

                    QueryTestHelpers.VerifyQuery(
                        query,
@"SELECT
    [Extent2].[Name] AS [Name]
    FROM  [dbo].[A] AS [Extent1]
    INNER JOIN [dbo].[B] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]");
                }
            }
        }
    }
}
