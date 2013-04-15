// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.SqlGen
{
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Infrastructure.FunctionsModel;
    using System.Linq;
    using Xunit;

    public class DmlFunctionSqlGeneratorTests
    {
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
                    .ToList();

            var functionSqlGenerator
                = new DmlFunctionSqlGenerator(ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal(
                @"insert [dbo].[Customers]([Name])
values (@Name)

declare @CustomerId int
select @CustomerId = [CustomerId]
from [dbo].[Customers]
where @@ROWCOUNT > 0 and [CustomerId] = scope_identity()

select t0.[CustomerId]
from [dbo].[Customers] as t0
where @@ROWCOUNT > 0 and t0.[CustomerId] = @CustomerId",
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
                    .ToList();

            var functionSqlGenerator
                = new DmlFunctionSqlGenerator(ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal(
                @"declare @generated_keys table([order_id] int, [Key] uniqueidentifier, [Code] nvarchar(128), [Signature] varbinary(128))
insert [dbo].[Orders]([Code], [Signature], [Name], [Address_Street], [Address_City], [Address_Country_Name], [OrderGroupId], [Customer_CustomerId])
output inserted.[order_id], inserted.[Key], inserted.[Code], inserted.[Signature] into @generated_keys
values (@teh_codez, @Signature, @the_name, @Address_Street, @Address_City, @Address_Country_Name, @OrderGroupId, @Customer_CustomerId)

declare @order_id int, @Key uniqueidentifier
select @order_id = t.[order_id], @Key = t.[Key]
from @generated_keys as g join [dbo].[Orders] as t on g.[order_id] = t.[order_id] and g.[Key] = t.[Key] and g.[Code] = t.[Code] and g.[Signature] = t.[Signature]
where @@ROWCOUNT > 0

insert [dbo].[special_orders]([order_id], [so_key], [Code], [Signature], [OtherCustomer_CustomerId], [OtherAddress_Street], [OtherAddress_City], [OtherAddress_Country_Name])
values (@order_id, @Key, @teh_codez, @Signature, @OtherCustomer_CustomerId, @OtherAddress_Street, @OtherAddress_City, @OtherAddress_Country_Name)

insert [dbo].[xspecial_orders]([xid], [so_key], [Code], [Signature], [TheSpecialist])
values (@order_id, @Key, @teh_codez, @Signature, @TheSpecialist)

select t0.[order_id] as xid, t0.[Key] as key_result, t0.[OrderNo], t0.[RowVersion], t1.[MagicOrderToken], t2.[FairyDust]
from [dbo].[Orders] as t0
join [dbo].[special_orders] as t1 on t1.[order_id] = t0.[order_id] and t1.[so_key] = t0.[Key] and t1.[Code] = t0.[Code] and t1.[Signature] = t0.[Signature]
join [dbo].[xspecial_orders] as t2 on t2.[xid] = t0.[order_id] and t2.[so_key] = t0.[Key] and t2.[Code] = t0.[Code] and t2.[Signature] = t0.[Signature]
where @@ROWCOUNT > 0 and t0.[order_id] = @order_id and t0.[Key] = @Key and t0.[Code] = @teh_codez and t0.[Signature] = @Signature",
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
                    .ToList();

            var functionSqlGenerator
                = new DmlFunctionSqlGenerator(ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal(
                @"update [dbo].[Customers]
set [Name] = @Name
where ([CustomerId] = @CustomerId)",
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
                    .ToList();

            var functionSqlGenerator
                = new DmlFunctionSqlGenerator(ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal(
                @"update [dbo].[Orders]
set [Name] = @Name, [Address_Street] = @Address_Street, [Address_City] = @Address_City, [Address_Country_Name] = @Address_Country_Name, [OrderGroupId] = @OrderGroupId, [Customer_CustomerId] = @Customer_CustomerId
where ((((((([order_id] = @xid) and ([Key] = @key_for_update)) and ([Code] = @Code)) and ([Signature] = @Signature)) and (([Name] = @Name_Original) or ([Name] is null and @Name_Original is null))) and (([RowVersion] = @RowVersion_Original) or ([RowVersion] is null and @RowVersion_Original is null))) and (([Customer_CustomerId] = @Customer_CustomerId) or ([Customer_CustomerId] is null and @Customer_CustomerId is null)))

update [dbo].[special_orders]
set [OtherCustomer_CustomerId] = @OtherCustomer_CustomerId, [OtherAddress_Street] = @OtherAddress_Street, [OtherAddress_City] = @OtherAddress_City, [OtherAddress_Country_Name] = @OtherAddress_Country_Name
where ((((([order_id] = @xid) and ([so_key] = @key_for_update)) and ([Code] = @Code)) and ([Signature] = @Signature)) and (([OtherCustomer_CustomerId] = @OtherCustomer_CustomerId) or ([OtherCustomer_CustomerId] is null and @OtherCustomer_CustomerId is null)))
and @@ROWCOUNT > 0

update [dbo].[xspecial_orders]
set [TheSpecialist] = @TheSpecialist
where (((([xid] = @xid) and ([so_key] = @key_for_update)) and ([Code] = @Code)) and ([Signature] = @Signature))
and @@ROWCOUNT > 0

select t0.[OrderNo] as order_fu, t0.[RowVersion], t1.[MagicOrderToken], t2.[FairyDust]
from [dbo].[Orders] as t0
join [dbo].[special_orders] as t1 on t1.[order_id] = t0.[order_id] and t1.[so_key] = t0.[Key] and t1.[Code] = t0.[Code] and t1.[Signature] = t0.[Signature]
join [dbo].[xspecial_orders] as t2 on t2.[xid] = t0.[order_id] and t2.[so_key] = t0.[Key] and t2.[Code] = t0.[Code] and t2.[Signature] = t0.[Signature]
where @@ROWCOUNT > 0 and t0.[order_id] = @xid and t0.[Key] = @key_for_update and t0.[Code] = @Code and t0.[Signature] = @Signature

set @rows_affected = @@ROWCOUNT",
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
                    .ToList();

            var functionSqlGenerator
                = new DmlFunctionSqlGenerator(ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal(
                @"delete [dbo].[Customers]
where ([CustomerId] = @CustomerId)",
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
                    .ToList();

            var functionSqlGenerator
                = new DmlFunctionSqlGenerator(ProviderRegistry.Sql2008_ProviderManifest);

            Assert.Equal(
                @"delete [dbo].[xspecial_orders]
where (((([xid] = @xid) and ([so_key] = @key_for_delete)) and ([Code] = @Code)) and ([Signature] = @Signature))

delete [dbo].[special_orders]
where ((((([order_id] = @xid) and ([so_key] = @key_for_delete)) and ([Code] = @Code)) and ([Signature] = @Signature)) and (([OtherCustomer_CustomerId] = @OtherCustomer_CustomerId) or ([OtherCustomer_CustomerId] is null and @OtherCustomer_CustomerId is null)))
and @@ROWCOUNT > 0

delete [dbo].[Orders]
where ((((((([order_id] = @xid) and ([Key] = @key_for_delete)) and ([Code] = @Code)) and ([Signature] = @Signature)) and (([Name] = @Name_Original) or ([Name] is null and @Name_Original is null))) and (([RowVersion] = @RowVersion_Original) or ([RowVersion] is null and @RowVersion_Original is null))) and (([Customer_CustomerId] = @Customer_CustomerId) or ([Customer_CustomerId] is null and @Customer_CustomerId is null)))
and @@ROWCOUNT > 0

set @rows_affected = @@ROWCOUNT",
                functionSqlGenerator.GenerateDelete(convertedTrees, "rows_affected"));
        }
    }
}
