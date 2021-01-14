// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    using System.Linq;
    using Xunit;

    public class Bug2612 : FunctionalTestBase
    {
        [Fact]
        public void Outer_applies_are_eliminated_simple_case()
        {
            using (var context = new BugContext())
            {
                var query =
                    from c in context.As.OrderBy(c => c.Id).Take(100)
                    let rel1 = c.B1
                    select new
                    {
                        c.Id,
                        Rel1 = new
                        {
                            c.B1.Id,
                            c.B1.Code,
                            Id2 = rel1.Id
                        }
                    };

                QueryTestHelpers.VerifyQuery(
                    query,
@"SELECT TOP (100) 
    [Extent1].[Id] AS [Id], 
    [Extent2].[Id] AS [Id1], 
    [Extent2].[Code] AS [Code]
    FROM  [dbo].[A] AS [Extent1]
    LEFT OUTER JOIN [dbo].[B] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
    ORDER BY [Extent1].[Id] ASC");
            }
        }

        [Fact]
        public void Outer_applies_are_eliminated_complex_case()
        {
            using (var context = new BugContext())
            {
                var query =
                    from c in context.As.OrderBy(c => c.Id).Take(100)
                    let rel1 = c.B1
                    let rel2 = c.B2
                    select new
                    {
                        c.Id,
                        Rel1 = new
                        {
                            rel1.Id,
                            rel1.Code,
                        },
                        Rel2 = new
                        {
                            c.B2.Id,
                            c.B2.Code,
                        }
                    };

                QueryTestHelpers.VerifyQuery(
                    query,
@"SELECT TOP (100) 
    [Extent1].[Id] AS [Id], 
    [Extent2].[Id] AS [Id1], 
    [Extent2].[Code] AS [Code]
    FROM  [dbo].[A] AS [Extent1]
    LEFT OUTER JOIN [dbo].[B] AS [Extent2] ON [Extent1].[Id] = [Extent2].[Id]
    ORDER BY [Extent1].[Id] ASC");
            }
        }

        public class BugContext : DbContext
        {
            static BugContext()
            {
                Database.SetInitializer<BugContext>(null);
            }
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<A>()
                    .HasRequired(c => c.B1)
                    .WithRequiredPrincipal();
                modelBuilder.Entity<A>()
                    .HasRequired(c => c.B2)
                    .WithRequiredPrincipal();
            }

            public DbSet<A> As { get; set; }
            public DbSet<B> Bs { get; set; }
        }

        public class A
        {
            public int Id { get; set; }
            public virtual B B1 { get; set; }
            public virtual B B2 { get; set; }
        }

        public class B
        {
            public int Id { get; set; }
            public string Code { get; set; }
        }
    }
}
