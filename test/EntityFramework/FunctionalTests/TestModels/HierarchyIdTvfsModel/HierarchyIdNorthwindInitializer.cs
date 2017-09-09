// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Hierarchy;

    public class HierarchyIdNorthwindInitializer : DropCreateDatabaseAlways<HierarchyIdNorthwindContext>
    {
        protected override void Seed(HierarchyIdNorthwindContext context)
        {
            context.Database.ExecuteSqlCommand(
                @"

                CREATE FUNCTION [dbo].[fx_SuppliersWithinRange]
                (	
	                @path1 hierarchyid, 
	                @path2 hierarchyid
                )
                RETURNS TABLE 
                AS
                RETURN 
                (
	                SELECT [Id],[Name],[Path]
	                FROM [ProductivityApiTests.HierarchyIdNorthwindContext].[dbo].[SupplierWithHierarchyIds] as supplier
	                Where supplier.Path > @path1 AND supplier.Path < @path2
                )"
                );

            context.Database.ExecuteSqlCommand(
                @"

                CREATE FUNCTION [dbo].[fx_SupplierHierarchyIdsWithinRange]
                (	
	                @path1 hierarchyid, 
	                @path2 hierarchyid
                )
                RETURNS TABLE 
                AS
                RETURN 
                (
	                SELECT [Path]
	                FROM [ProductivityApiTests.HierarchyIdNorthwindContext].[dbo].[SupplierWithHierarchyIds] as supplier
	                Where supplier.Path > @path1 AND supplier.Path < @path2
                )"
                );

            new List<SupplierWithHierarchyId>
                {
                    new SupplierWithHierarchyId
                        {
                            Name = "Supplier1",
                            Path = HierarchyId.Parse("/1/")
                        },
                    new SupplierWithHierarchyId
                        {
                            Name = "Supplier2",
                            Path = HierarchyId.Parse("/1/1/")
                        },
                    new SupplierWithHierarchyId
                        {
                            Name = "Supplier3",
                            Path = HierarchyId.Parse("/1/1.1/")
                        },
                    new SupplierWithHierarchyId
                        {
                            Name = "Supplier4",
                            Path = HierarchyId.Parse("/1/2/")
                        },
                    new SupplierWithHierarchyId
                        {
                            Name = "Supplier5",
                            Path = HierarchyId.Parse("/1/1.1/2/")
                        },
                    new SupplierWithHierarchyId
                        {
                            Name = "Supplier6",
                            Path = HierarchyId.Parse("/1/3/")
                        },
                    new SupplierWithHierarchyId
                        {
                            Name = "Supplier7",
                            Path = HierarchyId.Parse("/1/4/")
                        },
                    new SupplierWithHierarchyId
                        {
                            Name = "Supplier8",
                            Path = HierarchyId.Parse("/1/4/1/")
                        },
                    new SupplierWithHierarchyId
                        {
                            Name = "Supplier9",
                            Path = HierarchyId.Parse("/2/")
                        },
                    new SupplierWithHierarchyId
                        {
                            Name = "Supplier10",
                            Path = HierarchyId.Parse("/3/")
                        },
                    new SupplierWithHierarchyId
                        {
                            Name = "Supplier11",
                            Path = HierarchyId.Parse("/3/1/")
                        },
                    new SupplierWithHierarchyId
                        {
                            Name = "Supplier12",
                            Path = HierarchyId.Parse("/3/2/")
                        },
                    new SupplierWithHierarchyId
                        {
                            Name = "Supplier13",
                            Path = HierarchyId.Parse("/3/2.1/")
                        },
                    new SupplierWithHierarchyId
                        {
                            Name = "Supplier14",
                            Path = HierarchyId.Parse("/3/2.1/3/")
                        },
                    new SupplierWithHierarchyId
                        {
                            Name = "Supplier15",
                            Path = HierarchyId.Parse("/4/")
                        },
                    new SupplierWithHierarchyId
                        {
                            Name = "Supplier16",
                            Path = HierarchyId.Parse("/5/")
                        },
                }.ForEach(s => context.Suppliers.Add(s));

            new List<WidgetWithHierarchyId>
                {
                    new WidgetWithHierarchyId
                        {
                            Name = "Widget1",
                            SomeHierarchyId = HierarchyId.Parse("/11/"),
                            Complex = new ComplexWithHierarchyId
                                          {
                                              NotHierarchyId = "1",
                                              SomeMoreHierarchyId = HierarchyId.Parse("/11/1/")
                                          }
                        },
                    new WidgetWithHierarchyId
                        {
                            Name = "Widget2",
                            SomeHierarchyId = HierarchyId.Parse("/12/"),
                            Complex = new ComplexWithHierarchyId
                                          {
                                              NotHierarchyId = "1",
                                              SomeMoreHierarchyId = HierarchyId.Parse("/12/1/")
                                          }
                        },
                    new WidgetWithHierarchyId
                        {
                            Name = "Widget3",
                            SomeHierarchyId = HierarchyId.Parse("/13/"),
                            Complex = new ComplexWithHierarchyId
                                          {
                                              NotHierarchyId = "1",
                                              SomeMoreHierarchyId = HierarchyId.Parse("/13/1/")
                                          }
                        },
                    new WidgetWithHierarchyId
                        {
                            Name = "Widget4",
                            SomeHierarchyId = HierarchyId.Parse("/14/"),
                            Complex = new ComplexWithHierarchyId
                                          {
                                              NotHierarchyId = "1",
                                              SomeMoreHierarchyId = HierarchyId.Parse("/14/1/")
                                          }
                        },
                }.ForEach(w => context.Widgets.Add(w));
        }
    }
}
