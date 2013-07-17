// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.SqlGen
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Infrastructure.FunctionsModel;
    using System.Linq;
    using Xunit;

    public class DmlFunctionSqlGeneratorTests
    {
        [Fact]
        public void Insert_simple_tph_entity_base()
        {
            var modificationFunctionMapping
                = TestContext.GetModificationFunctionMapping("Vehicle");

            var commandTrees
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel())
                    .GenerateInsert(modificationFunctionMapping.Item1.EntityType.FullName);

            var convertedTrees
                = new DynamicToFunctionModificationCommandConverter(
                    modificationFunctionMapping.Item1, modificationFunctionMapping.Item2)
                    .Convert(commandTrees)
                    .OfType<DbInsertCommandTree>()
                    .ToList();

            var functionSqlGenerator
                = new DmlFunctionSqlGenerator(new SqlGenerator());

            Assert.Equal(
                @"INSERT [dbo].[Vehicles]([Name], [Discriminator])
VALUES (NULL, N'Vehicle')

DECLARE @Id int
SELECT @Id = [Id]
FROM [dbo].[Vehicles]
WHERE @@ROWCOUNT > 0 AND [Id] = scope_identity()

SELECT t0.[Id]
FROM [dbo].[Vehicles] AS t0
WHERE @@ROWCOUNT > 0 AND t0.[Id] = @Id",
                functionSqlGenerator.GenerateInsert(convertedTrees));
        }

        [Fact]
        public void Insert_simple_tph_entity_derived()
        {
            var modificationFunctionMapping
                = TestContext.GetModificationFunctionMapping("Car");

            var commandTrees
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel())
                    .GenerateInsert(modificationFunctionMapping.Item1.EntityType.FullName);

            var convertedTrees
                = new DynamicToFunctionModificationCommandConverter(
                    modificationFunctionMapping.Item1, modificationFunctionMapping.Item2)
                    .Convert(commandTrees)
                    .OfType<DbInsertCommandTree>()
                    .ToList();

            var functionSqlGenerator
                = new DmlFunctionSqlGenerator(new SqlGenerator());

            Assert.Equal(
                @"INSERT [dbo].[Vehicles]([Name], [Discriminator])
VALUES (@Name, N'Car')

DECLARE @Id int
SELECT @Id = [Id]
FROM [dbo].[Vehicles]
WHERE @@ROWCOUNT > 0 AND [Id] = scope_identity()

SELECT t0.[Id]
FROM [dbo].[Vehicles] AS t0
WHERE @@ROWCOUNT > 0 AND t0.[Id] = @Id",
                functionSqlGenerator.GenerateInsert(convertedTrees));
        }

        [Fact]
        public void Update_simple_entity_with_tph_base()
        {
            var modificationFunctionMapping
                = TestContext.GetModificationFunctionMapping("Vehicle");

            var commandTrees
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel())
                    .GenerateUpdate(modificationFunctionMapping.Item1.EntityType.FullName);

            var convertedTrees
                = new DynamicToFunctionModificationCommandConverter(
                    modificationFunctionMapping.Item1, modificationFunctionMapping.Item2)
                    .Convert(commandTrees)
                    .OfType<DbUpdateCommandTree>()
                    .ToList();

            var functionSqlGenerator
                = new DmlFunctionSqlGenerator(new SqlGenerator());

            Assert.Null(functionSqlGenerator.GenerateUpdate(convertedTrees, null));
        }

        [Fact]
        public void Update_simple_entity_with_tph_derived()
        {
            var modificationFunctionMapping
                = TestContext.GetModificationFunctionMapping("Car");

            var commandTrees
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel())
                    .GenerateUpdate(modificationFunctionMapping.Item1.EntityType.FullName);

            var convertedTrees
                = new DynamicToFunctionModificationCommandConverter(
                    modificationFunctionMapping.Item1, modificationFunctionMapping.Item2)
                    .Convert(commandTrees)
                    .OfType<DbUpdateCommandTree>()
                    .ToList();

            var functionSqlGenerator
                = new DmlFunctionSqlGenerator(new SqlGenerator());

            Assert.Equal(
                @"UPDATE [dbo].[Vehicles]
SET [Name] = @Name
WHERE ([Id] = @Id)",
                functionSqlGenerator.GenerateUpdate(convertedTrees, null));
        }

        [Fact]
        public void Delete_simple_entity_with_tph_base()
        {
            var modificationFunctionMapping
                = TestContext.GetModificationFunctionMapping("Vehicle");

            var commandTrees
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel())
                    .GenerateDelete(modificationFunctionMapping.Item1.EntityType.FullName);

            var convertedTrees
                = new DynamicToFunctionModificationCommandConverter(
                    modificationFunctionMapping.Item1, modificationFunctionMapping.Item2)
                    .Convert(commandTrees)
                    .OfType<DbDeleteCommandTree>()
                    .ToList();

            var functionSqlGenerator
                = new DmlFunctionSqlGenerator(new SqlGenerator());

            Assert.Equal(
                @"DELETE [dbo].[Vehicles]
WHERE ([Id] = @Id)",
                functionSqlGenerator.GenerateDelete(convertedTrees, null));
        }

        [Fact]
        public void Delete_simple_entity_with_tph_derived()
        {
            var modificationFunctionMapping
                = TestContext.GetModificationFunctionMapping("Car");

            var commandTrees
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel())
                    .GenerateDelete(modificationFunctionMapping.Item1.EntityType.FullName);

            var convertedTrees
                = new DynamicToFunctionModificationCommandConverter(
                    modificationFunctionMapping.Item1, modificationFunctionMapping.Item2)
                    .Convert(commandTrees)
                    .OfType<DbDeleteCommandTree>()
                    .ToList();

            var functionSqlGenerator
                = new DmlFunctionSqlGenerator(new SqlGenerator());

            Assert.Equal(
                @"DELETE [dbo].[Vehicles]
WHERE ([Id] = @Id)",
                functionSqlGenerator.GenerateDelete(convertedTrees, null));
        }

        [Fact]
        public void Insert_simple_entity()
        {
            var modificationFunctionMapping
                = TestContext.GetModificationFunctionMapping("Customer");

            var commandTrees
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel())
                    .GenerateInsert(modificationFunctionMapping.Item1.EntityType.FullName);

            var convertedTrees
                = new DynamicToFunctionModificationCommandConverter(
                    modificationFunctionMapping.Item1, modificationFunctionMapping.Item2)
                    .Convert(commandTrees)
                    .OfType<DbInsertCommandTree>()
                    .ToList();

            var functionSqlGenerator
                = new DmlFunctionSqlGenerator(new SqlGenerator());

            Assert.Equal(
                @"INSERT [dbo].[Customers]([Name])
VALUES (@Name)

DECLARE @CustomerId int
SELECT @CustomerId = [CustomerId]
FROM [dbo].[Customers]
WHERE @@ROWCOUNT > 0 AND [CustomerId] = scope_identity()

SELECT t0.[CustomerId]
FROM [dbo].[Customers] AS t0
WHERE @@ROWCOUNT > 0 AND t0.[CustomerId] = @CustomerId",
                functionSqlGenerator.GenerateInsert(convertedTrees));
        }

        [Fact]
        public void Insert_tpt_entity_with_concurrency_and_store_generated_values()
        {
            var modificationFunctionMapping
                = TestContext.GetModificationFunctionMapping("ExtraSpecialOrder");

            var commandTrees
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel())
                    .GenerateInsert(modificationFunctionMapping.Item1.EntityType.FullName);

            var convertedTrees
                = new DynamicToFunctionModificationCommandConverter(
                    modificationFunctionMapping.Item1, modificationFunctionMapping.Item2)
                    .Convert(commandTrees)
                    .OfType<DbInsertCommandTree>()
                    .ToList();

            var functionSqlGenerator
                = new DmlFunctionSqlGenerator(new SqlGenerator());

            Assert.Equal(
                @"DECLARE @generated_keys table([order_id] int, [Key] uniqueidentifier, [Code] nvarchar(128), [Signature] varbinary(128))
INSERT [dbo].[Orders]([Code], [Signature], [Name], [Address_Street], [Address_City], [Address_Country_Name], [OrderGroupId], [Customer_CustomerId])
OUTPUT inserted.[order_id], inserted.[Key], inserted.[Code], inserted.[Signature] INTO @generated_keys
VALUES (@teh_codez, @Signature, @the_name, @Address_Street, @Address_City, @Address_Country_Name, @OrderGroupId, @Customer_CustomerId)

DECLARE @order_id int, @Key uniqueidentifier
SELECT @order_id = t.[order_id], @Key = t.[Key]
FROM @generated_keys AS g JOIN [dbo].[Orders] AS t ON g.[order_id] = t.[order_id] AND g.[Key] = t.[Key] AND g.[Code] = t.[Code] AND g.[Signature] = t.[Signature]
WHERE @@ROWCOUNT > 0

INSERT [dbo].[special_orders]([order_id], [so_key], [Code], [Signature], [OtherCustomer_CustomerId], [OtherAddress_Street], [OtherAddress_City], [OtherAddress_Country_Name])
VALUES (@order_id, @Key, @teh_codez, @Signature, @OtherCustomer_CustomerId, @OtherAddress_Street, @OtherAddress_City, @OtherAddress_Country_Name)

INSERT [dbo].[xspecial_orders]([xid], [so_key], [Code], [Signature], [TheSpecialist])
VALUES (@order_id, @Key, @teh_codez, @Signature, @TheSpecialist)

SELECT t0.[order_id] AS xid, t0.[Key] AS key_result, t0.[OrderNo], t0.[RowVersion], t1.[MagicOrderToken], t2.[FairyDust]
FROM [dbo].[Orders] AS t0
JOIN [dbo].[special_orders] AS t1 ON t1.[order_id] = t0.[order_id] AND t1.[so_key] = t0.[Key] AND t1.[Code] = t0.[Code] AND t1.[Signature] = t0.[Signature]
JOIN [dbo].[xspecial_orders] AS t2 ON t2.[xid] = t0.[order_id] AND t2.[so_key] = t0.[Key] AND t2.[Code] = t0.[Code] AND t2.[Signature] = t0.[Signature]
WHERE @@ROWCOUNT > 0 AND t0.[order_id] = @order_id AND t0.[Key] = @Key AND t0.[Code] = @teh_codez AND t0.[Signature] = @Signature",
                functionSqlGenerator.GenerateInsert(convertedTrees));
        }

        [Fact]
        public void Update_simple_entity()
        {
            var modificationFunctionMapping
                = TestContext.GetModificationFunctionMapping("Customer");

            var commandTrees
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel())
                    .GenerateUpdate(modificationFunctionMapping.Item1.EntityType.FullName);

            var convertedTrees
                = new DynamicToFunctionModificationCommandConverter(
                    modificationFunctionMapping.Item1, modificationFunctionMapping.Item2)
                    .Convert(commandTrees)
                    .OfType<DbUpdateCommandTree>()
                    .ToList();

            var functionSqlGenerator
                = new DmlFunctionSqlGenerator(new SqlGenerator());

            Assert.Equal(
                @"UPDATE [dbo].[Customers]
SET [Name] = @Name
WHERE ([CustomerId] = @CustomerId)",
                functionSqlGenerator.GenerateUpdate(convertedTrees, null));
        }

        [Fact]
        public void Update_tpt_entity_with_concurrency_and_store_generated_values()
        {
            var modificationFunctionMapping
                = TestContext.GetModificationFunctionMapping("ExtraSpecialOrder");

            var commandTrees
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel())
                    .GenerateUpdate(modificationFunctionMapping.Item1.EntityType.FullName);

            var convertedTrees
                = new DynamicToFunctionModificationCommandConverter(
                    modificationFunctionMapping.Item1, modificationFunctionMapping.Item2)
                    .Convert(commandTrees)
                    .OfType<DbUpdateCommandTree>()
                    .ToList();

            var functionSqlGenerator
                = new DmlFunctionSqlGenerator(new SqlGenerator());

            Assert.Equal(
                @"UPDATE [dbo].[Orders]
SET [Name] = @Name, [Address_Street] = @Address_Street, [Address_City] = @Address_City, [Address_Country_Name] = @Address_Country_Name, [OrderGroupId] = @OrderGroupId, [Customer_CustomerId] = @Customer_CustomerId
WHERE (((((([order_id] = @xid) AND ([Key] = @key_for_update)) AND ([Code] = @Code)) AND ([Signature] = @Signature)) AND (([Name] = @Name_Original) OR ([Name] IS NULL AND @Name_Original IS NULL))) AND (([RowVersion] = @RowVersion_Original) OR ([RowVersion] IS NULL AND @RowVersion_Original IS NULL)))

UPDATE [dbo].[special_orders]
SET [OtherCustomer_CustomerId] = @OtherCustomer_CustomerId, [OtherAddress_Street] = @OtherAddress_Street, [OtherAddress_City] = @OtherAddress_City, [OtherAddress_Country_Name] = @OtherAddress_Country_Name
WHERE (((([order_id] = @xid) AND ([so_key] = @key_for_update)) AND ([Code] = @Code)) AND ([Signature] = @Signature))
AND @@ROWCOUNT > 0

UPDATE [dbo].[xspecial_orders]
SET [TheSpecialist] = @TheSpecialist
WHERE (((([xid] = @xid) AND ([so_key] = @key_for_update)) AND ([Code] = @Code)) AND ([Signature] = @Signature))
AND @@ROWCOUNT > 0

SELECT t0.[OrderNo] AS order_fu, t0.[RowVersion], t1.[MagicOrderToken], t2.[FairyDust]
FROM [dbo].[Orders] AS t0
JOIN [dbo].[special_orders] AS t1 ON t1.[order_id] = t0.[order_id] AND t1.[so_key] = t0.[Key] AND t1.[Code] = t0.[Code] AND t1.[Signature] = t0.[Signature]
JOIN [dbo].[xspecial_orders] AS t2 ON t2.[xid] = t0.[order_id] AND t2.[so_key] = t0.[Key] AND t2.[Code] = t0.[Code] AND t2.[Signature] = t0.[Signature]
WHERE @@ROWCOUNT > 0 AND t0.[order_id] = @xid AND t0.[Key] = @key_for_update AND t0.[Code] = @Code AND t0.[Signature] = @Signature

SET @rows_affected = @@ROWCOUNT",
                functionSqlGenerator.GenerateUpdate(convertedTrees, "rows_affected"));
        }

        [Fact]
        public void Delete_simple_entity()
        {
            var modificationFunctionMapping
                = TestContext.GetModificationFunctionMapping("Customer");

            var commandTrees
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel())
                    .GenerateDelete(modificationFunctionMapping.Item1.EntityType.FullName);

            var convertedTrees
                = new DynamicToFunctionModificationCommandConverter(
                    modificationFunctionMapping.Item1, modificationFunctionMapping.Item2)
                    .Convert(commandTrees)
                    .OfType<DbDeleteCommandTree>()
                    .ToList();

            var functionSqlGenerator
                = new DmlFunctionSqlGenerator(new SqlGenerator());

            Assert.Equal(
                @"DELETE [dbo].[Customers]
WHERE ([CustomerId] = @CustomerId)",
                functionSqlGenerator.GenerateDelete(convertedTrees, null));
        }

        [Fact]
        public void Delete_tpt_entity_with_concurrency_and_store_generated_values()
        {
            var modificationFunctionMapping
                = TestContext.GetModificationFunctionMapping("ExtraSpecialOrder");

            var commandTrees
                = new ModificationCommandTreeGenerator(TestContext.CreateDynamicUpdateModel())
                    .GenerateDelete(modificationFunctionMapping.Item1.EntityType.FullName);

            var convertedTrees
                = new DynamicToFunctionModificationCommandConverter(
                    modificationFunctionMapping.Item1, modificationFunctionMapping.Item2)
                    .Convert(commandTrees)
                    .OfType<DbDeleteCommandTree>()
                    .ToList();

            var functionSqlGenerator
                = new DmlFunctionSqlGenerator(new SqlGenerator());

            Assert.Equal(
                @"DELETE [dbo].[xspecial_orders]
WHERE (((([xid] = @xid) AND ([so_key] = @key_for_delete)) AND ([Code] = @Code)) AND ([Signature] = @Signature))

DELETE [dbo].[special_orders]
WHERE ((((([order_id] = @xid) AND ([so_key] = @key_for_delete)) AND ([Code] = @Code)) AND ([Signature] = @Signature)) AND (([OtherCustomer_CustomerId] = @OtherCustomer_CustomerId) OR ([OtherCustomer_CustomerId] IS NULL AND @OtherCustomer_CustomerId IS NULL)))
AND @@ROWCOUNT > 0

DELETE [dbo].[Orders]
WHERE ((((((([order_id] = @xid) AND ([Key] = @key_for_delete)) AND ([Code] = @Code)) AND ([Signature] = @Signature)) AND (([Name] = @Name_Original) OR ([Name] IS NULL AND @Name_Original IS NULL))) AND (([RowVersion] = @RowVersion_Original) OR ([RowVersion] IS NULL AND @RowVersion_Original IS NULL))) AND (([Customer_CustomerId] = @Customer_CustomerId) OR ([Customer_CustomerId] IS NULL AND @Customer_CustomerId IS NULL)))
AND @@ROWCOUNT > 0

SET @rows_affected = @@ROWCOUNT",
                functionSqlGenerator.GenerateDelete(convertedTrees, "rows_affected"));
        }
    }
}
