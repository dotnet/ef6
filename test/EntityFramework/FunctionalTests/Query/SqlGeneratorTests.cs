// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.TestModels.ArubaModel;
    using System.Linq;
    using Xunit;

    public class SqlGeneratorTests : FunctionalTestBase
    {
        public class Top : FunctionalTestBase
        {
            [Fact]
            public void Select_top_two()
            {
                var query = @"
SELECT TOP (2) o.Id FROM ArubaContext.Owners as o";
                var expectedSql = @"
SELECT TOP (2) 
[c].[Id] AS [Id]
FROM [dbo].[ArubaOwners] AS [c]";

                // verifying that only two results are returned and an Int is projected as a result
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        VerifyTypeAndCount(reader, 2, "Int32");
                    }
                }
            }

            [Fact]
            public void Select_top_order_by_desc()
            {
                var query = @"
SELECT TOP (3) o.Id FROM ArubaContext.Owners as o ORDER BY o.Id DESC";

                var expectedSql = @"
SELECT TOP (3) 
[Extent1].[Id] AS [Id]
FROM [dbo].[ArubaOwners] AS [Extent1]
ORDER BY [Extent1].[Id] DESC";

                // verifying that there are three integer results returned and that they are sorted in descending order
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        VerifySortDescAndCountInt(reader, 3);
                    }
                }
            }

            [Fact]
            public void Select_top_nested_two_order_by()
            {
                var query = @"
SELECT TOP (3) C.ID
FROM ( SELECT TOP (14) o.Id AS ID
    FROM ArubaContext.Owners as o
    ORDER BY o.Id ASC
) AS C
ORDER BY C.ID DESC";

                var expectedSql = @"
SELECT TOP (3) 
[Limit1].[Id] AS [Id]
FROM ( SELECT TOP (14) [Extent1].[Id] AS [Id]
	FROM [dbo].[ArubaOwners] AS [Extent1]
	ORDER BY [Extent1].[Id] ASC
)  AS [Limit1]
ORDER BY [Limit1].[Id] DESC";

                // verifying that there are 3 integer results returned and they are sorted in descending order
                // using nesting TOP statements and ORDER BY statements
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        VerifySortDescAndCountInt(reader, 3);
                    }
                }
            }

            [Fact]
            public void Select_top_nested_order_by()
            {
                var query = @"
SELECT TOP (4) C.Id
FROM ( SELECT TOP (14) o.Id AS Id
    FROM ArubaContext.Owners as o
    ORDER BY o.Id
) AS C
ORDER BY C.Id DESC";

                var expectedSql = @"
SELECT TOP (4) 
[Limit1].[Id] AS [Id]
FROM ( SELECT TOP (14) [Extent1].[Id] AS [Id]
	FROM [dbo].[ArubaOwners] AS [Extent1]
	ORDER BY [Extent1].[Id] ASC
)  AS [Limit1]
ORDER BY [Limit1].[Id] DESC";

                // verifying that there are 4 results returned and they are sorted in descending order 
                // with nested TOP statements
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        VerifySortDescAndCountInt(reader, 4);
                    }
                }
            }

            [Fact]
            public void Select_top_distinct()
            {
                var query = @"
SELECT DISTINCT TOP (2) o.FirstName AS name
FROM ArubaContext.Owners as o
ORDER BY name DESC";

                var expectedSql = @"
SELECT TOP (2) 
[Distinct1].[C1] AS [C1], 
[Distinct1].[FirstName] AS [FirstName]
FROM ( SELECT DISTINCT 
	[Extent1].[FirstName] AS [FirstName], 
	1 AS [C1]
	FROM [dbo].[ArubaOwners] AS [Extent1]
)  AS [Distinct1]
ORDER BY [Distinct1].[FirstName] DESC";

                // verifying that there are 2 results returned, that they are sorted in descending order
                // and they are distinct
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        VerifySortDescAndCountString(reader, 2, distinct: true);
                    }
                }
            }

            [Fact]
            public void Select_top_nested_with_inheritance()
            {
                var query = @"
SELECT  TOP(3) C.m.id, C.m.address 
FROM ( SELECT TOP(5) m
	FROM OFTYPE (ArubaContext.Configs, CodeFirstNamespace.ArubaMachineConfig) AS m
) AS C
ORDER BY C.m.Id DESC";

                var expectedSql = @"
SELECT TOP (3) 
[top].[Id] AS [Id], 
[top].[Address] AS [Address]
FROM ( SELECT TOP (5) [Extent1].[Id] AS [Id], [Extent1].[Address] AS [Address]
	FROM [dbo].[ArubaConfigs] AS [Extent1]
	WHERE [Extent1].[Discriminator] = N'ArubaMachineConfig'
	ORDER BY [Extent1].[Id] DESC
)  AS [top]";

                // verifying that there are 3 results returned and they are sorted in descending order
                // using nested TOP statements
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        VerifySortDescAndCountInt(reader, 3);
                    }
                }
            }

            [Fact]
            public void Select_top_order_by_with_inheritance()
            {
                var query = @"
SELECT  TOP (3) C.Id, C.Address
FROM OFTYPE (ArubaContext.Configs, CodeFirstNamespace.ArubaMachineConfig) AS C 
ORDER BY C.Id DESC";

                var expectedSql = @"
SELECT TOP (3) 
[Extent1].[Id] AS [Id], 
[Extent1].[Address] AS [Address]
FROM [dbo].[ArubaConfigs] AS [Extent1]
WHERE [Extent1].[Discriminator] = N'ArubaMachineConfig'
ORDER BY [Extent1].[Id] DESC";

                // verifying that there are 3 results returned and they are sorted in descending order
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        VerifySortDescAndCountInt(reader, 3);
                    }
                }
            }

            [Fact]
            public void Select_top_order_by_reduced_with_inheritance()
            {
                var query = @"
SELECT  TOP (3) C.Id, C.Address
FROM OFTYPE (ArubaContext.Configs, CodeFirstNamespace.ArubaMachineConfig) AS C 
ORDER BY C.Id DESC";

                var expectedSql = @"
SELECT TOP (3) 
[Extent1].[Id] AS [Id], 
[Extent1].[Address] AS [Address]
FROM [dbo].[ArubaConfigs] AS [Extent1]
WHERE [Extent1].[Discriminator] = N'ArubaMachineConfig'
ORDER BY [Extent1].[Id] DESC";

                // verifying that there are 3 results returned and they are sorted in descending order
                // using a model with inheritance
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        VerifySortDescAndCountInt(reader, 3);
                    }
                }
            }

            [Fact]
            public void Select_top_nested_asc_and_desc_with_params()
            {
                var query = @"
SELECT TOP (@pInt64) C.OwnerId
FROM (
        SELECT TOP (@pInt16) o.Id AS OwnerId
        FROM ArubaContext.Owners as o
        ORDER BY o.Id ASC
    ) AS C
ORDER BY C.OwnerId DESC";

                string expectedSql = @"
SELECT TOP (@pInt64) 
[Limit1].[Id] AS [Id]
FROM ( SELECT TOP (@pInt16) [Extent1].[Id] AS [Id]
	FROM [dbo].[ArubaOwners] AS [Extent1]
	ORDER BY [Extent1].[Id] ASC
)  AS [Limit1]
ORDER BY [Limit1].[Id] DESC";
                var prm1 = new EntityParameter("pInt16", DbType.Int16);
                var prm2 = new EntityParameter("pInt64", DbType.Int64);
                prm1.Value = 5;
                prm2.Value = 3;

                // verifying that there are 3 results returned and they are sorted in descending order
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql, prm1, prm2))
                    {
                        VerifySortDescAndCountInt(reader, 3);
                    }
                }
            }

            [Fact]
            public void Select_top_nested_with_params()
            {
                var query = @"
SELECT TOP (@pInt64) C.OwnerId
FROM (
        SELECT TOP (@pInt16) o.Id AS OwnerId
        FROM ArubaContext.Owners as o
        ORDER BY o.Id
    ) AS C
ORDER BY C.OwnerId DESC";
                var prm1 = new EntityParameter("pInt16", DbType.Int16);
                var prm2 = new EntityParameter("pInt64", DbType.Int64);
                prm1.Value = 5;
                prm2.Value = 2;

                // verifying that there are 2 results returned and they are sorted in descending order
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, null, prm1, prm2))
                    {
                        VerifySortDescAndCountInt(reader, 2);
                    }
                }
            }

            [Fact]
            public void Select_desc_skip_limit_with_params()
            {
                var query = @"
select o.Id
from ArubaContext.Owners as o 
order by o.Id desc skip @pInt16 LIMIT @pInt64";
                var prm1 = new EntityParameter("pInt16", DbType.Int16);
                var prm2 = new EntityParameter("pInt64", DbType.Int64);
                prm1.Value = 5;
                prm2.Value = 2;

                // verifying that there are 2 results returned and they are sorted in descending order
                using (var db = new ArubaContext())
                {
                    using (var db2 = new ArubaContext())
                    {
                        using (var reader = QueryTestHelpers.EntityCommandSetup(db2, query, null, prm1, prm2))
                        {
                            var expectedResults = db.Owners.ToList().OrderByDescending(o => o.Id).Skip(5).Take(2).Select(o => o.Id).ToList();

                            Assert.Equal(expectedResults.Count, 2);
                            VerifyAgainstBaselineResults(reader, expectedResults);
                        }
                    }
                }
            }

            [Fact]
            public void Select_desc_skip_limit_with_params_and_literal()
            {
                var query = @"
select o.Id
from ArubaContext.Owners as o
order by o.Id desc skip @pInt16 LIMIT 5";

                var expectedSql = @"
SELECT TOP (5) 
[Extent1].[Id] AS [Id]
FROM ( SELECT [Extent1].[Id] AS [Id], row_number() OVER (ORDER BY [Extent1].[Id] DESC) AS [row_number]
	FROM [dbo].[ArubaOwners] AS [Extent1]
)  AS [Extent1]
WHERE [Extent1].[row_number] > @pInt16
ORDER BY [Extent1].[Id] DESC";
                var prm1 = new EntityParameter("pInt16", DbType.Int16);
                prm1.Value = 5;

                // verifying that there are 5 results returned and they are sorted in descending order
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql, prm1))
                    {
                        VerifySortDescAndCountInt(reader, 5);
                    }
                }
            }
        }

        public class SkipLimit : FunctionalTestBase
        {
            [Fact]
            public void Nested_limit()
            {
                var query = @"
SELECT C.Id
FROM (
        SELECT o.Id AS Id
        FROM ArubaContext.Owners as o
        ORDER BY o.Id LIMIT 5
    ) AS C
ORDER BY C.Id DESC LIMIT 3";

                var expectedSql = @"
SELECT TOP (3) 
[Limit1].[Id] AS [Id]
FROM ( SELECT TOP (5) [Extent1].[Id] AS [Id]
	FROM [dbo].[ArubaOwners] AS [Extent1]
	ORDER BY [Extent1].[Id] ASC
)  AS [Limit1]
ORDER BY [Limit1].[Id] DESC";

                // verifying that there are 3 results returned and they are sorted in descending order
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        VerifySortDescAndCountInt(reader, 3);
                    }
                }
            }

            [Fact]
            public void Basic_skip_limit()
            {
                var query = @"
SELECT C.Id, C.Address 
FROM OFTYPE (ArubaContext.Configs, CodeFirstNamespace.ArubaMachineConfig) AS C 
ORDER BY C.Id DESC SKIP 3 LIMIT 2";

                var expectedSql = @"
SELECT TOP (2) 
[Filter1].[Id] AS [Id], 
[Filter1].[Address] AS [Address]
FROM ( SELECT [Extent1].[Id] AS [Id], [Extent1].[Address] AS [Address], row_number() OVER (ORDER BY [Extent1].[Id] DESC) AS [row_number]
	FROM [dbo].[ArubaConfigs] AS [Extent1]
	WHERE [Extent1].[Discriminator] = N'ArubaMachineConfig'
)  AS [Filter1]
WHERE [Filter1].[row_number] > 3
ORDER BY [Filter1].[Id] DESC";

                // verifying that there are 2 results returned and they are sorted in descending order
                using (var db = new ArubaContext())
                {
                    using (var db2 = new ArubaContext())
                    {
                        using (var reader = QueryTestHelpers.EntityCommandSetup(db2, query, expectedSql))
                        {
                            var expectedResults =
                                db.Configs.ToList().OfType<ArubaMachineConfig>().OrderByDescending(o => o.Id).Skip(3).Take(2)
                                  .Select(o => o.Id).ToList();
                            Assert.Equal(expectedResults.Count(), 2);
                            VerifyAgainstBaselineResults(reader, expectedResults);
                        }
                    }
                }
            }

            [Fact]
            public void Skip_limit_group_by()
            {
                var query = @"
SELECT o.FirstName as c
FROM ArubaContext.Owners as o
GROUP BY o.FirstName
ORDER BY c DESC SKIP 1 LIMIT 2";

                var expectedSql = @"
SELECT TOP (2) 
[Project2].[C1] AS [C1], 
[Project2].[FirstName] AS [FirstName]
FROM ( SELECT [Project2].[FirstName] AS [FirstName], [Project2].[C1] AS [C1], row_number() OVER (ORDER BY [Project2].[FirstName] DESC) AS [row_number]
	FROM ( SELECT 
		[Distinct1].[FirstName] AS [FirstName], 
		1 AS [C1]
		FROM ( SELECT DISTINCT 
			[Extent1].[FirstName] AS [FirstName]
			FROM [dbo].[ArubaOwners] AS [Extent1]
		)  AS [Distinct1]
	)  AS [Project2]
)  AS [Project2]
WHERE [Project2].[row_number] > 1
ORDER BY [Project2].[FirstName] DESC";

                // verifying that there are 2 results returned and they are sorted in Descending order
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        VerifySortDescAndCountString(reader, 2);
                    }
                }
            }

            [Fact]
            public void Skip_no_limit_with_inheritance()
            {
                var query = @"
SELECT Config.Id, Config.Address
FROM OFTYPE (ArubaContext.Configs, CodeFirstNamespace.ArubaMachineConfig) AS Config 
ORDER BY Config.Id DESC SKIP 1";

                var expectedSql = @"
SELECT 
[Filter1].[Id] AS [Id], 
[Filter1].[Address] AS [Address]
FROM ( SELECT [Extent1].[Id] AS [Id], [Extent1].[Address] AS [Address], row_number() OVER (ORDER BY [Extent1].[Id] DESC) AS [row_number]
	FROM [dbo].[ArubaConfigs] AS [Extent1]
	WHERE [Extent1].[Discriminator] = N'ArubaMachineConfig'
)  AS [Filter1]
WHERE [Filter1].[row_number] > 1
ORDER BY [Filter1].[Id] DESC";

                // verify that the first 1 is skipped and that the results are sorted in descending order
                using (var db = new ArubaContext())
                {
                    using (var db2 = new ArubaContext())
                    {
                        using (var reader = QueryTestHelpers.EntityCommandSetup(db2, query, expectedSql))
                        {
                            var expectedResults = db.Configs.OfType<ArubaMachineConfig>().ToList().OrderByDescending(c => c.Id).Skip(1)
                                                    .Select(c => c.Id);
                            VerifyAgainstBaselineResults(reader, expectedResults);
                        }
                    }
                }
            }

            [Fact]
            public void Skip_limit_distinct()
            {
                var query = @"
SELECT DISTINCT o.FirstName as a
FROM ArubaContext.Owners as o
ORDER BY a SKIP 1 LIMIT 1";
                
                var expectedSql = @"
SELECT TOP (1) 
[Distinct1].[C1] AS [C1], 
[Distinct1].[FirstName] AS [FirstName]
FROM ( SELECT [Distinct1].[FirstName] AS [FirstName], [Distinct1].[C1] AS [C1], row_number() OVER (ORDER BY [Distinct1].[FirstName] ASC) AS [row_number]
	FROM ( SELECT DISTINCT 
		[Extent1].[FirstName] AS [FirstName], 
		1 AS [C1]
		FROM [dbo].[ArubaOwners] AS [Extent1]
	)  AS [Distinct1]
)  AS [Distinct1]
WHERE [Distinct1].[row_number] > 1
ORDER BY [Distinct1].[FirstName] ASC";

                // verifying that there is 1 result returned
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        VerifySortDescAndCountString(reader, 1);
                    }
                }
            }

            [Fact]
            public void Multiple_sort_keys()
            {
                var query = @"
 SELECT o.id, o.FirstName, o.LastName
 FROM ArubaContext.Owners as o
 ORDER BY o.FirstName ASC, o.LastName DESC SKIP 3 LIMIT 4";

                var expectedSql = @"
SELECT TOP (4) 
[Extent1].[Id] AS [Id], 
[Extent1].[FirstName] AS [FirstName], 
[Extent1].[LastName] AS [LastName]
FROM ( SELECT [Extent1].[Id] AS [Id], [Extent1].[FirstName] AS [FirstName], [Extent1].[LastName] AS [LastName], row_number() OVER (ORDER BY [Extent1].[FirstName] ASC, [Extent1].[LastName] DESC) AS [row_number]
	FROM [dbo].[ArubaOwners] AS [Extent1]
)  AS [Extent1]
WHERE [Extent1].[row_number] > 3
ORDER BY [Extent1].[FirstName] ASC, [Extent1].[LastName] DESC";

                // verifying that there are 2 results returned
                using (var db = new ArubaContext())
                {
                    using (var db2 = new ArubaContext())
                    {
                        using (var reader = QueryTestHelpers.EntityCommandSetup(db2, query, expectedSql))
                        {
                            var expectedResults = db.Owners.ToList().OrderBy(o => o.FirstName).ThenByDescending(o => o.LastName)
                                                    .Skip(3).Take(4).Select(o => o.Id);
                            VerifyAgainstBaselineResults(reader, expectedResults);
                        }
                    }
                }
            }

            [Fact]
            public void Nested_skip_limits_in_select()
            {
                var query = @"
SELECT (
        SELECT o.Id 
        FROM ArubaContext.Owners AS o 
        ORDER BY o.Id SKIP 1 LIMIT 2) AS TOP, oo.Id
FROM ArubaContext.Owners as oo 
WHERE oo.Id > 3
ORDER BY oo.Id SKIP 2 LIMIT 3";

                var expectedSql = @"
SELECT 
[Limit1].[Id] AS [Id], 
[Project1].[C1] AS [C1], 
[Project1].[Id] AS [Id1]
FROM   (SELECT TOP (3) [Filter1].[Id] AS [Id]
	FROM ( SELECT [Extent1].[Id] AS [Id], row_number() OVER (ORDER BY [Extent1].[Id] ASC) AS [row_number]
		FROM [dbo].[ArubaOwners] AS [Extent1]
		WHERE [Extent1].[Id] > 3
	)  AS [Filter1]
	WHERE [Filter1].[row_number] > 2
	ORDER BY [Filter1].[Id] ASC ) AS [Limit1]
LEFT OUTER JOIN  (SELECT TOP (2) 
	[Extent2].[Id] AS [Id], 
	1 AS [C1]
	FROM ( SELECT [Extent2].[Id] AS [Id], row_number() OVER (ORDER BY [Extent2].[Id] ASC) AS [row_number]
		FROM [dbo].[ArubaOwners] AS [Extent2]
	)  AS [Extent2]
	WHERE [Extent2].[row_number] > 1
	ORDER BY [Extent2].[Id] ASC ) AS [Project1] ON 1 = 1
ORDER BY [Limit1].[Id] ASC, [Project1].[C1] ASC";

                // verifying that there are 3 results returned
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        var count = 0;
                        while (reader.Read())
                        {
                            var nestedReader = (DbDataReader)reader.GetValue(0);
                            while (nestedReader.Read())
                            {
                                count++;
                            }
                        }
                 
                        Assert.Equal(3 * 2, count);
                    }
                }
            }

            [Fact]
            public void Nested_skip_limits_in_from()
            {
                var query = @"
SELECT TOP.Id
FROM  (  
        SELECT o.Alias, o.Id 
        FROM ArubaContext.Owners AS o
        ORDER BY o.Id desc SKIP 2 LIMIT 5
) AS TOP
ORDER BY TOP.Id SKIP 1 LIMIT 2";

                var expectedSql = @"
SELECT TOP (2) 
[Limit1].[Id] AS [Id]
FROM ( SELECT [Limit1].[Id] AS [Id], row_number() OVER (ORDER BY [Limit1].[Id] ASC) AS [row_number]
	FROM ( SELECT TOP (5) [Extent1].[Id] AS [Id]
		FROM ( SELECT [Extent1].[Id] AS [Id], row_number() OVER (ORDER BY [Extent1].[Id] DESC) AS [row_number]
			FROM [dbo].[ArubaOwners] AS [Extent1]
		)  AS [Extent1]
		WHERE [Extent1].[row_number] > 2
		ORDER BY [Extent1].[Id] DESC
	)  AS [Limit1]
)  AS [Limit1]
WHERE [Limit1].[row_number] > 1
ORDER BY [Limit1].[Id] ASC";

                // verifying that there are 2 integer results returned
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        VerifyTypeAndCount(reader, 2, "Int32");
                    }
                }
            }

            [Fact]
            public void Intersect_with_split_limit()
            {
                var query = @"
(SELECT o.Id 
FROM ArubaContext.Owners as o
ORDER BY o.Id DESC, o.Alias DESC SKIP 3L LIMIT 7L) 
    INTERSECT 
(SELECT o.Id
FROM ArubaContext.Owners as o
ORDER BY o.Id ASC, o.Alias ASC SKIP 4 LIMIT 6)";

                var expectedSql = @"
SELECT 
[Intersect1].[C1] AS [C1], 
[Intersect1].[Id] AS [C2]
FROM  (SELECT TOP (7) 
	[Project1].[C1] AS [C1], 
	[Project1].[Id] AS [Id]
	FROM ( SELECT [Project1].[Id] AS [Id], [Project1].[Alias] AS [Alias], [Project1].[C1] AS [C1], row_number() OVER (ORDER BY [Project1].[Id] DESC, [Project1].[Alias] DESC) AS [row_number]
		FROM ( SELECT 
			[Extent1].[Id] AS [Id], 
			[Extent1].[Alias] AS [Alias], 
			1 AS [C1]
			FROM [dbo].[ArubaOwners] AS [Extent1]
		)  AS [Project1]
	)  AS [Project1]
	WHERE [Project1].[row_number] > 3
	ORDER BY [Project1].[Id] DESC, [Project1].[Alias] DESC
INTERSECT
	SELECT TOP (6) 
	[Project3].[C1] AS [C1], 
	[Project3].[Id] AS [Id]
	FROM ( SELECT [Project3].[Id] AS [Id], [Project3].[Alias] AS [Alias], [Project3].[C1] AS [C1], row_number() OVER (ORDER BY [Project3].[Id] ASC, [Project3].[Alias] ASC) AS [row_number]
		FROM ( SELECT 
			[Extent2].[Id] AS [Id], 
			[Extent2].[Alias] AS [Alias], 
			1 AS [C1]
			FROM [dbo].[ArubaOwners] AS [Extent2]
		)  AS [Project3]
	)  AS [Project3]
	WHERE [Project3].[row_number] > 4
	ORDER BY [Project3].[Id] ASC, [Project3].[Alias] ASC) AS [Intersect1]";

                // verifying that the results returned match the results of the Linq baseline
                using (var db = new ArubaContext())
                {
                    using (var db2 = new ArubaContext())
                    {
                        using (var reader = QueryTestHelpers.EntityCommandSetup(db2, query, expectedSql))
                        {
                            //building expected results
                            var query1 = db.Owners.ToList().OrderByDescending(o => o.Id).ThenByDescending(o => o.Alias).Skip(3).Take(7)
                                           .Select(o => o.Id);
                            var query2 = db.Owners.ToList().OrderBy(o => o.Id).ThenBy(o => o.Alias).Skip(4).Take(6).Select(o => o.Id);
                            var intersect = query1.Intersect(query2);

                            VerifyAgainstBaselineResults(reader, intersect);
                        }
                    }
                }
            }

            [Fact]
            public void Skip_limit_desc_with_params()
            {
                var query = @"
select o.Id
from ArubaContext.Owners as o
order by o.Id desc skip @pInt16 Limit @pInt64";
                var prm1 = new EntityParameter("pInt16", DbType.Int16);
                var prm2 = new EntityParameter("pInt64", DbType.Int64);
                prm1.Value = 5;
                prm2.Value = 2;

                // verifying that there are 2 results returned and they are sorted in descending order
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, null, prm1, prm2))
                    {
                        VerifySortDescAndCountInt(reader, 2);
                    }
                }
            }

            [Fact]
            public void Skip_limit_with_params()
            {
                var query = @"
select o.Id
from ArubaContext.owners as o
order by o.Id desc skip @pInt16 LIMIT @pInt64";
                var prm1 = new EntityParameter("pInt16", DbType.Int16);
                var prm2 = new EntityParameter("pInt64", DbType.Int64);
                prm1.Value = 5;
                prm2.Value = 2;

                // verifying that there are 2 results returned and they are sorted in descending order
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, null, prm1, prm2))
                    {
                        VerifySortDescAndCountInt(reader, 2);
                    }
                }
            }

            [Fact]
            public void Skip_with_params_and_literal()
            {
                var query = @"
select o.Id
from ArubaContext.Owners as o
order by o.Id desc skip @pInt16 LIMIT 5";
                var prm1 = new EntityParameter("pInt16", DbType.Int16);
                prm1.Value = 5;

                // verifying that there are 5 results returned and they are sorted in descending order
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, null, prm1))
                    {
                        VerifySortDescAndCountInt(reader, 5);
                    }
                }
            }

            [Fact]
            public void Nested_projections_list()
            {
                var query = @"
SELECT VALUE (
       SELECT VALUE TOP (2) d.Id
       FROM  ArubaContext.Owners AS d
       WHERE d.Id > c.Id
       ORDER BY d.Id 
       ) AS top 
FROM ArubaContext.Owners AS c 
ORDER BY c.Id skip 5 limit 2";

                var expectedSql = @"
SELECT 
[Project2].[Id] AS [Id], 
[Project2].[C1] AS [C1], 
[Project2].[Id1] AS [Id1]
FROM ( SELECT 
	[Limit1].[Id] AS [Id], 
	[Limit2].[Id] AS [Id1], 
	CASE WHEN ([Limit2].[Id] IS NULL) THEN CAST(NULL AS int) ELSE 1 END AS [C1]
	FROM   (SELECT TOP (2) [Extent1].[Id] AS [Id]
		FROM ( SELECT [Extent1].[Id] AS [Id], row_number() OVER (ORDER BY [Extent1].[Id] ASC) AS [row_number]
			FROM [dbo].[ArubaOwners] AS [Extent1]
		)  AS [Extent1]
		WHERE [Extent1].[row_number] > 5
		ORDER BY [Extent1].[Id] ASC ) AS [Limit1]
	OUTER APPLY  (SELECT TOP (2) [Project1].[Id] AS [Id]
		FROM ( SELECT 
			[Extent2].[Id] AS [Id]
			FROM [dbo].[ArubaOwners] AS [Extent2]
			WHERE [Extent2].[Id] > [Limit1].[Id]
		)  AS [Project1]
		ORDER BY [Project1].[Id] ASC ) AS [Limit2]
)  AS [Project2]
ORDER BY [Project2].[Id] ASC, [Project2].[C1] ASC";

                // verifying that there are 4 results returned and they are integers
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        var count = 0;

                        while (reader.Read())
                        {
                            var nestedReader = (DbDataReader)reader.GetValue(0);
                            while (nestedReader.Read())
                            {
                                Assert.Equal("Int32", ((CollectionType)reader.DataRecordInfo.RecordType.EdmType).TypeUsage.EdmType.Name);
                                count++;
                            }
                        }
                 
                        Assert.Equal(4, count);
                    }
                }
            }

            [Fact]
            public void Anyelement_over_skip_limit()
            {
                var query = @"
ANYELEMENT (
    SELECT C.Id
    FROM ArubaContext.Owners AS C
    ORDER BY C.Id SKIP 3 LIMIT 2
)";

                var expectedSql = @"
SELECT 
[Element1].[Id] AS [Id]
FROM   ( SELECT 1 AS X ) AS [SingleRowTable1]
LEFT OUTER JOIN  (SELECT TOP (1) [element].[Id] AS [Id]
	FROM ( SELECT TOP (2) 
		[Extent1].[Id] AS [Id]
		FROM ( SELECT [Extent1].[Id] AS [Id], row_number() OVER (ORDER BY [Extent1].[Id] ASC) AS [row_number]
			FROM [dbo].[ArubaOwners] AS [Extent1]
		)  AS [Extent1]
		WHERE [Extent1].[row_number] > 3
		ORDER BY [Extent1].[Id] ASC
	)  AS [element] ) AS [Element1] ON 1 = 1";

                // verifying that there is 1 result returned and that it is an integer
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        VerifyTypeAndCount(reader, 1, "Int32");
                    }
                }
            }

            [Fact]
            public void Complicated_order_by()
            {
                var query = @"
SELECT 1, d.Id 
FROM ArubaContext.Owners as c, ArubaContext.Owners as d 
WHERE c.Id == d.Id 
ORDER BY anyelement(select value c.Id * (-1) from {d} as c) SKIP 4 LIMIT 3";

                var expectedSql = @"
SELECT TOP (3) 
[Project1].[Id] AS [Id], 
[Project1].[C2] AS [C1]
FROM ( SELECT [Project1].[C1] AS [C1], [Project1].[Id] AS [Id], [Project1].[C2] AS [C2], row_number() OVER (ORDER BY [Project1].[C1] ASC) AS [row_number]
	FROM ( SELECT 
		[Extent1].[Id] * -1 AS [C1], 
		[Extent1].[Id] AS [Id], 
		1 AS [C2]
		FROM [dbo].[ArubaOwners] AS [Extent1]
	)  AS [Project1]
)  AS [Project1]
WHERE [Project1].[row_number] > 4
ORDER BY [Project1].[C1] ASC";

                // verifying that there are 3 results returned and they are sorted in descending order
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        VerifySortDescAndCountInt(reader, 3);
                    }
                }
            }

            [Fact]
            public void Skip_with_no_limit_and_multiset()
            {
                var query = @"
SELECT c.Id
FROM ArubaContext.Owners AS c
ORDER BY c.Id DESC SKIP 2";

                var expectedSql = @"
SELECT 
[Extent1].[Id] AS [Id]
FROM ( SELECT [Extent1].[Id] AS [Id], row_number() OVER (ORDER BY [Extent1].[Id] DESC) AS [row_number]
	FROM [dbo].[ArubaOwners] AS [Extent1]
)  AS [Extent1]
WHERE [Extent1].[row_number] > 2
ORDER BY [Extent1].[Id] DESC";

                using (var db = new ArubaContext())
                {
                    using (var db2 = new ArubaContext())
                    {
                        using (var reader = QueryTestHelpers.EntityCommandSetup(db2, query, expectedSql))
                        {
                            var expectedResults = db.Owners.ToList().OrderByDescending(o => o.Id).Skip(2).Select(o => o.Id);
                            VerifyAgainstBaselineResults(reader, expectedResults);
                        }
                    }
                }
            }

            [Fact]
            public void Edge_case_column_name()
            {
                var query = @"
SELECT c.Id as [row_number]
FROM ArubaContext.Owners as c
ORDER BY [row_number] skip 2 limit 3";

                var expectedSql = @"
SELECT TOP (3) 
[Extent1].[Id] AS [Id]
FROM ( SELECT [Extent1].[Id] AS [Id], row_number() OVER (ORDER BY [Extent1].[Id] ASC) AS [row_number]
	FROM [dbo].[ArubaOwners] AS [Extent1]
)  AS [Extent1]
WHERE [Extent1].[row_number] > 2
ORDER BY [Extent1].[Id] ASC";

                // verifying that there are 3 results returned and they are sorted in ascending order
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        VerifySortAscAndCountInt(reader, 3);
                    }
                }
            }

            [Fact]
            public void Skip_limit_over_skip_limit_intersect()
            {
                var query = @"
SELECT VALUE d
FROM (
	(SELECT c.Id, c.Alias
	FROM ArubaContext.Owners as c
	ORDER BY c.Id DESC, c.Alias DESC SKIP 3L LIMIT 4L) 
		INTERSECT 
	(SELECT c.Id, c.Alias 
	FROM ArubaContext.Owners as c
	ORDER BY c.Id ASC, c.Alias ASC SKIP 5 LIMIT 2)
    ) AS d
ORDER BY d.Id SKIP 1 LIMIT 2";

                var expectedSql = @"
SELECT TOP (2) 
[Intersect1].[C1] AS [C1], 
[Intersect1].[Id] AS [C2], 
[Intersect1].[Alias] AS [C3]
FROM ( SELECT [Intersect1].[C1] AS [C1], [Intersect1].[Id] AS [Id], [Intersect1].[Alias] AS [Alias], row_number() OVER (ORDER BY [Intersect1].[Id] ASC) AS [row_number]
	FROM  (SELECT TOP (4) 
		[Project1].[C1] AS [C1], 
		[Project1].[Id] AS [Id], 
		[Project1].[Alias] AS [Alias]
		FROM ( SELECT [Project1].[Id] AS [Id], [Project1].[Alias] AS [Alias], [Project1].[C1] AS [C1], row_number() OVER (ORDER BY [Project1].[Id] DESC, [Project1].[Alias] DESC) AS [row_number]
			FROM ( SELECT 
				[Extent1].[Id] AS [Id], 
				[Extent1].[Alias] AS [Alias], 
				1 AS [C1]
				FROM [dbo].[ArubaOwners] AS [Extent1]
			)  AS [Project1]
		)  AS [Project1]
		WHERE [Project1].[row_number] > 3
		ORDER BY [Project1].[Id] DESC, [Project1].[Alias] DESC
	INTERSECT
		SELECT TOP (2) 
		[Project3].[C1] AS [C1], 
		[Project3].[Id] AS [Id], 
		[Project3].[Alias] AS [Alias]
		FROM ( SELECT [Project3].[Id] AS [Id], [Project3].[Alias] AS [Alias], [Project3].[C1] AS [C1], row_number() OVER (ORDER BY [Project3].[Id] ASC, [Project3].[Alias] ASC) AS [row_number]
			FROM ( SELECT 
				[Extent2].[Id] AS [Id], 
				[Extent2].[Alias] AS [Alias], 
				1 AS [C1]
				FROM [dbo].[ArubaOwners] AS [Extent2]
			)  AS [Project3]
		)  AS [Project3]
		WHERE [Project3].[row_number] > 5
		ORDER BY [Project3].[Id] ASC, [Project3].[Alias] ASC) AS [Intersect1]
)  AS [Intersect1]
WHERE [Intersect1].[row_number] > 1
ORDER BY [Intersect1].[Id] ASC";

                // verifying that there is 1 result returned and it is an integer
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        VerifyTypeAndCount(reader, 1, "Int32");
                    }
                }
            }

            [Fact]
            public void Skip_limit_with_duplicates()
            {
                var query = @"
SELECT i
FROM  {1,1,1,2,2} as i
ORDER BY i SKIP 2 LIMIT 4";

                var expectedSql = @"
SELECT TOP (4) 
    [UnionAll4].[C1] AS [C1]
    FROM ( SELECT [UnionAll4].[C1] AS [C1], row_number() OVER (ORDER BY [UnionAll4].[C1] ASC) AS [row_number]
        FROM  (SELECT 
            1 AS [C1]
            FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]
        UNION ALL
            SELECT 
            1 AS [C1]
            FROM  ( SELECT 1 AS X ) AS [SingleRowTable2]
        UNION ALL
            SELECT 
            1 AS [C1]
            FROM  ( SELECT 1 AS X ) AS [SingleRowTable3]
        UNION ALL
            SELECT 
            2 AS [C1]
            FROM  ( SELECT 1 AS X ) AS [SingleRowTable4]
        UNION ALL
            SELECT 
            2 AS [C1]
            FROM  ( SELECT 1 AS X ) AS [SingleRowTable5]) AS [UnionAll4]
    )  AS [UnionAll4]
    WHERE [UnionAll4].[row_number] > 2
    ORDER BY [UnionAll4].[C1] ASC";

                // verifying that there are 3 results returned and that they match the expected output
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        var values = new List<int> { 1, 2, 2 };
                        VerifyAgainstBaselineResults(reader, values);
                    }
                }
            }

            [Fact]
            public void Skip_limit_with_nulls()
            {
                var query = @"
SELECT i
FROM  {null, 2,2} as i
ORDER BY i SKIP 2 LIMIT 4";

                var expectedSql = @"
SELECT TOP (4) 
    [Project4].[C2] AS [C1], 
    [Project4].[C1] AS [C2]
    FROM ( SELECT [Project4].[C1] AS [C1], [Project4].[C2] AS [C2], row_number() OVER (ORDER BY [Project4].[C1] ASC) AS [row_number]
        FROM ( SELECT 
            [UnionAll2].[C1] AS [C1], 
            1 AS [C2]
            FROM  (SELECT 
                CAST(NULL AS int) AS [C1]
                FROM  ( SELECT 1 AS X ) AS [SingleRowTable1]
            UNION ALL
                SELECT 
                2 AS [C1]
                FROM  ( SELECT 1 AS X ) AS [SingleRowTable2]
            UNION ALL
                SELECT 
                2 AS [C1]
                FROM  ( SELECT 1 AS X ) AS [SingleRowTable3]) AS [UnionAll2]
        )  AS [Project4]
    )  AS [Project4]
    WHERE [Project4].[row_number] > 2
    ORDER BY [Project4].[C1] ASC";

                // verifying that there is 1 result returned and it is an int
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query, expectedSql))
                    {
                        VerifyTypeAndCount(reader, 1, "Int32");
                    }
                }
            }
        }

        public class IntersectAndExcept : FunctionalTestBase
        {
            [Fact]
            public void Intersect_with_except_and_sql_verification()
            {
                var query = @"
{0,5,-7} intersect {5, 7, 0} except {7, -4, 5}";

                // verifying that there is { 0 } is returned
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query))
                    {
                        VerifyAgainstBaselineResults(reader, new List<int> { 0 });
                    }
                }
            }

            [Fact]
            public void Intersect_with_nulls()
            {
                var query = @"
{1, null, 5} intersect {5, null, 7}";

                // verifying that { DbNull, 5 } is returned
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query))
                    {
                        VerifyAgainstBaselineResults(reader, new List<object> { DBNull.Value, 5 });
                    }
                }
            }

            [Fact]
            public void Except_with_nulls()
            {
                var query = @"
{1, null, null, 5} except {5, null, 7}";

                // verifying that { 1 } is returned
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query))
                    {
                        VerifyAgainstBaselineResults(reader, new List<int> { 1 });
                    }
                }
            }

            [Fact]
            public void Large_union_all_should_not_give_query_nested_too_deeply()
            {
                var query = "{" + string.Join(", ", Enumerable.Range(1, 100)) + "}";

                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query))
                    {
                        VerifyAgainstBaselineResults(reader, Enumerable.Range(1, 100));
                    }
                }
            }

            [Fact]
            public void Large_intersect_should_not_give_query_nested_too_deeply()
            {
                var query = string.Join(" intersect ", Enumerable.Range(1, 100).Select(i => "{-1, " + i + "}"));

                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query))
                    {
                        VerifyAgainstBaselineResults(reader, new[] { -1 });
                    }
                }
            }
            
            // verifies that we are not flattening EXCEPT trees, since they are order-dependent. Also serves as verification that our
            // queries for Large_intersect_should_not_give_query_nested_too_deeply and Large_union_all_should_not_give_query_nested_too_deeply
            // are large enough such that they would generate the error without flattening
            [Fact]
            public void Large_except_should_give_query_nested_too_deeply()
            {
                var query = string.Join(" except ", Enumerable.Range(1, 100).Select(i => "{-1, " + i + "}"));

                using (var db = new ArubaContext())
                {
                    var exception = Assert.Throws<System.Data.Entity.Core.EntityCommandExecutionException>(() => QueryTestHelpers.EntityCommandSetup(db, query));
                    var sqlException = exception.InnerException as System.Data.SqlClient.SqlException;
                    Assert.NotNull(sqlException);
                    // error message number for "Some part of your SQL statement is nested too deeply..." see http://technet.microsoft.com/en-us/library/cc917589.aspx
                    Assert.Equal(191, sqlException.Number);
                }
            }

            [Fact]
            public void Test_union_all_flattening_works_with_table_per_type()
            {
                var guids = Enumerable.Range(0, 3).Select(i => Guid.NewGuid()).ToArray();
                using (var db = new TablePerTypeContext())
                {
                    db.Base.Add(new TablePerTypeDerived1 { Id = guids[0] });
                    db.Base.Add(new TablePerTypeDerived2 { Id = guids[1] });
                    db.Base.Add(new TablePerTypeDerived3 { Id = guids[2] });
                    db.SaveChanges();
                }

                using (var db = new TablePerTypeContext())
                {
                    var query = db.Base.Take(5);
                    
                    const string expectedSql = @"
SELECT 
    [Limit1].[C2] AS [C1], 
    [Limit1].[C1] AS [C2], 
    [Limit1].[C3] AS [C3], 
    [Limit1].[C4] AS [C4], 
    [Limit1].[C5] AS [C5]
    FROM ( SELECT TOP (5) 
        [UnionAll2].[Id] AS [C1], 
        CASE WHEN ([UnionAll2].[C3] = 1) THEN '0X0X' WHEN ([UnionAll2].[C4] = 1) THEN '0X1X' ELSE '0X2X' END AS [C2], 
        CASE WHEN ([UnionAll2].[C3] = 1) THEN [UnionAll2].[C1] WHEN ([UnionAll2].[C4] = 1) THEN CAST(NULL AS int) END AS [C3], 
        CASE WHEN ([UnionAll2].[C3] = 1) THEN CAST(NULL AS int) WHEN ([UnionAll2].[C4] = 1) THEN [UnionAll2].[Value2] END AS [C4], 
        CASE WHEN ([UnionAll2].[C3] = 1) THEN CAST(NULL AS int) WHEN ([UnionAll2].[C4] = 1) THEN CAST(NULL AS int) ELSE [UnionAll2].[C2] END AS [C5]
        FROM   (SELECT 
            [Extent1].[Id] AS [Id], 
            CAST(NULL AS int) AS [C1], 
            [Extent1].[Value2] AS [Value2], 
            CAST(NULL AS int) AS [C2], 
            cast(0 as bit) AS [C3], 
            cast(1 as bit) AS [C4]
            FROM [dbo].[TablePerTypeDerived2] AS [Extent1]
        UNION ALL
            SELECT 
            [Extent2].[Id] AS [Id], 
            [Extent2].[Value1] AS [Value1], 
            CAST(NULL AS int) AS [C1], 
            CAST(NULL AS int) AS [C2], 
            cast(1 as bit) AS [C3], 
            cast(0 as bit) AS [C4]
            FROM [dbo].[TablePerTypeDerived1] AS [Extent2]
        UNION ALL
            SELECT 
            [Extent3].[Id] AS [Id], 
            CAST(NULL AS int) AS [C1], 
            CAST(NULL AS int) AS [C2], 
            [Extent3].[Value3] AS [Value3], 
            cast(0 as bit) AS [C3], 
            cast(0 as bit) AS [C4]
            FROM [dbo].[TablePerTypeDerived3] AS [Extent3]) AS [UnionAll2]
        INNER JOIN [dbo].[TablePerTypeBase] AS [Extent4] ON [UnionAll2].[Id] = [Extent4].[Id]
    )  AS [Limit1]";

                    // test an actual read
                    var values = db.Base.Where(e => guids.Contains(e.Id))
                        .ToArray();
                    Assert.Equal(3, values.Select(v => v.GetType()).Distinct().Count());

                    QueryTestHelpers.VerifyQuery(query, expectedSql);
                }
            }

            [Fact]
            public void Table_self_referential_except()
            {
                var query = @"
ArubaContext.Owners
EXCEPT
ArubaContext.Owners";

                // verifying that there are no results returned
                using (var db = new ArubaContext())
                {
                    using (var reader = QueryTestHelpers.EntityCommandSetup(db, query))
                    {
                        VerifySortAscAndCountInt(reader, 0);
                    }
                }
            }
        }

        #region helpers
        private static void VerifySortDescAndCountInt(EntityDataReader reader, int expectedCount)
        {
            var count = 0;
            int id = int.MaxValue;
            while (reader.Read())
            {
                var newId = reader.GetInt32(0);
                Assert.True(id >= newId);
                id = newId;
                count++;
            }
            Assert.Equal(count, expectedCount);
        }

        private static void VerifySortAscAndCountInt(EntityDataReader reader, int expectedCount)
        {
            var count = 0;
            int id = int.MinValue;
            while (reader.Read())
            {
                var newId = reader.GetInt32(0);
                Assert.True(id <= newId);
                id = newId;
                count++;
            }
            Assert.Equal(count, expectedCount);
        }

        private static void VerifyAgainstBaselineResults(EntityDataReader reader, IEnumerable<int> expectedResults)
        {
            VerifyAgainstBaselineResults(reader, expectedResults.Cast<object>());
        }

        private static void VerifyAgainstBaselineResults(EntityDataReader reader, IEnumerable<object> expectedResults)
        {
            var actualResults = new List<object>();
            while (reader.Read())
            {
                actualResults.Add(reader.GetValue(0));
            }

            Assert.True(expectedResults.SequenceEqual(actualResults));
        }

        private static void VerifyTypeAndCount(EntityDataReader reader, int expectedCount, string type)
        {
            var count = 0;
            while (reader.Read())
            {                
                count++;
            }
            Assert.Equal(type, reader.DataRecordInfo.FieldMetadata[0].FieldType.TypeUsage.EdmType.Name);
            if (expectedCount >= 0)
            {
                Assert.Equal(count, expectedCount);   
            }            
        }

        private static void VerifySortDescAndCountString(EntityDataReader reader, int expectedCount, bool distinct = false)
        {
            string name = null;
            var count = 0;
            var items = new HashSet<string>();

            while (reader.Read())
            {
                var newName = reader.GetString(0);
                if (name != null)
                {
                    Assert.True(name.CompareTo(newName) >= 0);
                }
                if (distinct)
                {
                    Assert.False(items.Contains(newName));
                    items.Add(newName);
                }
                name = newName;
                count++;
            }
            Assert.Equal(expectedCount, count);
        }
        #endregion
    }

    #region Table per type context
    public sealed class TablePerTypeContext : DbContext
    {
        public TablePerTypeContext()
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<TablePerTypeContext>());
        }

        public DbSet<TablePerTypeBase> Base { get; set; }
        public DbSet<TablePerTypeDerived1> Derived1 { get; set; }
        public DbSet<TablePerTypeDerived2> Derived2 { get; set; }
        public DbSet<TablePerTypeDerived3> Derived3 { get; set; }
    }

    [Table("TablePerTypeBase")]
    public abstract class TablePerTypeBase
    {
        [Key] public Guid Id { get; set; }
    }

    [Table("TablePerTypeDerived1")]
    public class TablePerTypeDerived1 : TablePerTypeBase
    {
        public int Value1 { get; set; }
    }

    [Table("TablePerTypeDerived2")]
    public class TablePerTypeDerived2 : TablePerTypeBase
    {
        public int Value2 { get; set; }
    }

    [Table("TablePerTypeDerived3")]
    public class TablePerTypeDerived3 : TablePerTypeBase
    {
        public int Value3 { get; set; }
    }
    #endregion
}