// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class Bug952621 : FunctionalTestBase
    {
        [Fact]
        public void Unnecessary_joins_are_eliminated_test1()
        {
            using (var ctx = new Entities())
            {
                var query
                    = ctx.QueryDerivedEntities
                        .OrderBy(x => x.Id)
                        .Select(x => new
                        {
                            // QueryDerivedEntity
                            x.QueryData,
                            // DerivedEntityA
                            CodeA = x.DerivedEntityA.Code,
                            NameA = x.DerivedEntityA.Name,
                            DescriptionA = x.DerivedEntityA.Description,
                            // DerivedEntityB
                            CodeB = x.DerivedEntityB == null ? null : x.DerivedEntityB.Code,
                            NameB = x.DerivedEntityB == null ? null : x.DerivedEntityB.Name,
                            DescriptionB = x.DerivedEntityB == null ? null : x.DerivedEntityB.Description,
                            // DerivedEntityC
                            CodeC = x.DerivedEntityC.Code,
                            NameC = x.DerivedEntityC.Name,
                            DescriptionC = x.DerivedEntityC.Description,
                            AdditionalDataC = x.DerivedEntityC == null ? null : x.DerivedEntityC.AdditionalData,
                            // DerivedEntityD
                            CodeD = x.DerivedEntityD == null ? null : x.DerivedEntityD.Code,
                            NameD = x.DerivedEntityD == null ? null : x.DerivedEntityD.Name,
                            DescriptionD = x.DerivedEntityD == null ? null : x.DerivedEntityD.Description,
                            AdditionalDataD = x.DerivedEntityD == null ? null : x.DerivedEntityD.AdditionalData
                        })
                        .Take(10);

                QueryTestHelpers.VerifyQuery(
                    query,
@"SELECT TOP (10) 
    [Project1].[Id] AS [Id], 
    [Project1].[QueryData] AS [QueryData], 
    [Project1].[Code] AS [Code], 
    [Project1].[Name] AS [Name], 
    [Project1].[Description] AS [Description], 
    [Project1].[C1] AS [C1], 
    [Project1].[C2] AS [C2], 
    [Project1].[C3] AS [C3], 
    [Project1].[Code1] AS [Code1], 
    [Project1].[Name1] AS [Name1], 
    [Project1].[Description1] AS [Description1], 
    [Project1].[C4] AS [C4], 
    [Project1].[C5] AS [C5], 
    [Project1].[C6] AS [C6], 
    [Project1].[C7] AS [C7], 
    [Project1].[C8] AS [C8]
    FROM ( SELECT 
        [Extent1].[Id] AS [Id], 
        [Extent1].[QueryData] AS [QueryData], 
        [Extent2].[Code] AS [Code], 
        [Extent2].[Name] AS [Name], 
        [Extent2].[Description] AS [Description], 
        [Join3].[Code] AS [Code1], 
        [Join3].[Name] AS [Name1], 
        [Join3].[Description] AS [Description1], 
        CASE WHEN ([Extent3].[Id] IS NULL) THEN CAST(NULL AS varchar(1)) ELSE [Extent3].[Code] END AS [C1], 
        CASE WHEN ([Extent3].[Id] IS NULL) THEN CAST(NULL AS varchar(1)) ELSE [Extent3].[Name] END AS [C2], 
        CASE WHEN ([Extent3].[Id] IS NULL) THEN CAST(NULL AS varchar(1)) ELSE [Extent3].[Description] END AS [C3], 
        CASE WHEN ([Join3].[Id1] IS NULL) THEN CAST(NULL AS varchar(1)) ELSE [Join3].[AdditionalData] END AS [C4], 
        CASE WHEN ([Join5].[Id2] IS NULL) THEN CAST(NULL AS varchar(1)) ELSE [Join5].[Code] END AS [C5], 
        CASE WHEN ([Join5].[Id2] IS NULL) THEN CAST(NULL AS varchar(1)) ELSE [Join5].[Name] END AS [C6], 
        CASE WHEN ([Join5].[Id2] IS NULL) THEN CAST(NULL AS varchar(1)) ELSE [Join5].[Description] END AS [C7], 
        CASE WHEN ([Join5].[Id2] IS NULL) THEN CAST(NULL AS varchar(1)) ELSE [Join5].[AdditionalData] END AS [C8]
        FROM     [dbo].[QueryDerivedEntities] AS [Extent1]
        LEFT OUTER JOIN [dbo].[BaseEntities] AS [Extent2] ON ([Extent2].[Discriminator] = N'DerivedEntityA') AND ([Extent1].[DerivedEntityAId] = [Extent2].[Id])
        LEFT OUTER JOIN [dbo].[BaseEntities] AS [Extent3] ON ([Extent3].[Discriminator] = N'DerivedEntityB') AND ([Extent1].[DerivedEntityBId] = [Extent3].[Id])
        LEFT OUTER JOIN  (SELECT [Extent4].[Id] AS [Id1], [Extent4].[AdditionalData] AS [AdditionalData], [Extent5].[Code] AS [Code], [Extent5].[Name] AS [Name], [Extent5].[Description] AS [Description], [Extent5].[Discriminator] AS [Discriminator]
            FROM  [dbo].[AbstractEntities] AS [Extent4]
            INNER JOIN [dbo].[BaseEntities] AS [Extent5] ON [Extent4].[Id] = [Extent5].[Id] ) AS [Join3] ON ([Join3].[Discriminator] = N'DerivedEntityC') AND ([Extent1].[DerivedEntityCId] = [Join3].[Id1])
        LEFT OUTER JOIN  (SELECT [Extent6].[Id] AS [Id2], [Extent6].[AdditionalData] AS [AdditionalData], [Extent7].[Code] AS [Code], [Extent7].[Name] AS [Name], [Extent7].[Description] AS [Description], [Extent7].[Discriminator] AS [Discriminator]
            FROM  [dbo].[AbstractEntities] AS [Extent6]
            INNER JOIN [dbo].[BaseEntities] AS [Extent7] ON [Extent6].[Id] = [Extent7].[Id] ) AS [Join5] ON ([Join5].[Discriminator] = N'DerivedEntityD') AND ([Extent1].[DerivedEntityDId] = [Join5].[Id2])
    )  AS [Project1]
    ORDER BY [Project1].[Id] ASC");
            }
        }

        [Fact]
        public void Unnecessary_joins_are_eliminated_test2()
        {
            using (var ctx = new Entities())
            {
                var query
                    = ctx.QueryTestEntities
                        .OrderBy(x => x.Id)
                        .Select(
                            x => new
                                 {
                                     // QueryTestEntity
                                     x.QueryData,
                                     // TestEntity
                                     x.TestEntity.Code,
                                     x.TestEntity.Name,
                                     x.TestEntity.Description,
                                     // TestProperty
                                     x.TestEntity.TestProperty.PropertyCode,
                                     x.TestEntity.TestProperty.PropertyName,
                                     x.TestEntity.TestProperty.PropertyDescription
                                 })
                        .Take(10);

                QueryTestHelpers.VerifyQuery(
                    query,
@"SELECT TOP (10) 
    [Extent1].[Id] AS [Id], 
    [Extent1].[QueryData] AS [QueryData], 
    [Extent2].[Code] AS [Code], 
    [Extent2].[Name] AS [Name], 
    [Extent2].[Description] AS [Description], 
    [Join2].[PropertyCode] AS [PropertyCode], 
    [Join2].[PropertyName] AS [PropertyName], 
    [Join2].[PropertyDescription] AS [PropertyDescription]
    FROM   [dbo].[QueryTestEntities] AS [Extent1]
    INNER JOIN [dbo].[TestEntities] AS [Extent2] ON [Extent1].[TestEntityId] = [Extent2].[Id]
    LEFT OUTER JOIN  (SELECT [Extent3].[PropertyCode] AS [PropertyCode], [Extent3].[PropertyName] AS [PropertyName], [Extent3].[PropertyDescription] AS [PropertyDescription], [Extent4].[Id] AS [Id1]
        FROM  [dbo].[TestProperties] AS [Extent3]
        LEFT OUTER JOIN [dbo].[TestEntities] AS [Extent4] ON [Extent3].[Id] = [Extent4].[Id] ) AS [Join2] ON [Join2].[Id1] = [Extent1].[TestEntityId]
    ORDER BY [Extent1].[Id] ASC");
            }
        }

        public class Entities : DbContext
        {
            static Entities()
            {
                Database.SetInitializer<Entities>(null);
            }

            public virtual DbSet<BaseEntity> BaseEntities { get; set; }
            public virtual DbSet<QueryDerivedEntity> QueryDerivedEntities { get; set; }
            public virtual DbSet<QueryTestEntity> QueryTestEntities { get; set; }
            public virtual DbSet<TestEntity> TestEntities { get; set; }
            public virtual DbSet<TestNavigation> TestNavigations { get; set; }
            public virtual DbSet<TestProperty> TestProperties { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<TestEntity>().HasRequired(e => e.TestProperty).WithOptional(e => e.TestEntity);
                modelBuilder.Entity<BaseEntity>().ToTable("BaseEntities");
                modelBuilder.Entity<AbstractEntity>().ToTable("AbstractEntities");
            }
        }

        public abstract class BaseEntity
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTimeOffset RecordDate { get; set; }
        }

        public abstract class AbstractEntity : BaseEntity
        {
            public string AdditionalData { get; set; }
        }

        public class DerivedEntityA : BaseEntity
        {
        }

        public class DerivedEntityB : BaseEntity
        {
        }

        public class DerivedEntityC : AbstractEntity
        {
        }

        public class DerivedEntityD : AbstractEntity
        {
        }

        public class QueryDerivedEntity
        {
            public int Id { get; set; }
            public int DerivedEntityAId { get; set; }
            public int? DerivedEntityBId { get; set; }
            public int DerivedEntityCId { get; set; }
            public int? DerivedEntityDId { get; set; }
            public string QueryData { get; set; }

            public virtual DerivedEntityA DerivedEntityA { get; set; }
            public virtual DerivedEntityB DerivedEntityB { get; set; }
            public virtual DerivedEntityC DerivedEntityC { get; set; }
            public virtual DerivedEntityD DerivedEntityD { get; set; }
        }

        public class QueryTestEntity
        {
            public int Id { get; set; }
            public string QueryData { get; set; }
            public int TestEntityId { get; set; }

            public virtual TestEntity TestEntity { get; set; }
        }

        public class TestEntity
        {
            public TestEntity()
            {
                QueryTestEntities = new HashSet<QueryTestEntity>();
            }

            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTimeOffset RecordDate { get; set; }
            public int NavigationId { get; set; }

            public virtual ICollection<QueryTestEntity> QueryTestEntities { get; set; }
            public virtual TestNavigation TestNavigation { get; set; }
            public virtual TestProperty TestProperty { get; set; }
        }

        public class TestNavigation
        {
            public TestNavigation()
            {
                TestEntities = new HashSet<TestEntity>();
            }

            public int Id { get; set; }
            public string NavigationCode { get; set; }
            public string NavigationName { get; set; }
            public string NavigationDescription { get; set; }

            public virtual ICollection<TestEntity> TestEntities { get; set; }
        }

        public class TestProperty
        {
            public int Id { get; set; }
            public string PropertyCode { get; set; }
            public string PropertyName { get; set; }
            public string PropertyDescription { get; set; }

            public virtual TestEntity TestEntity { get; set; }
        }
    }
}
