// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using Xunit;

    public sealed class ColumnOrderingConventionTests
    {
        [Fact]
        public void Apply_should_order_by_annotation_if_given()
        {
            var table = new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace);
            var columnA = new EdmProperty(
                "C",
                ProviderRegistry.Sql2008_ProviderManifest.GetStoreType(
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))));
            columnA.SetOrder(2);
            table.AddColumn(columnA);
            var columnB = new EdmProperty(
                "Id",
                ProviderRegistry.Sql2008_ProviderManifest.GetStoreType(
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))));
            columnB.SetOrder(1);
            table.AddColumn(columnB);

            var database = new EdmModel(DataSpace.CSpace);
            database.AddEntitySet("ES", table);

            (new ColumnOrderingConvention()).Apply(table, new DbModel(null, database));

            Assert.Equal(2, table.Properties.Count);
            Assert.Equal("Id", table.Properties.First().Name);
        }

        [Fact]
        public void Apply_should_sort_annotated_before_unannotated()
        {
            var table = new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace);
            var columnA = new EdmProperty(
                "C",
                ProviderRegistry.Sql2008_ProviderManifest.GetStoreType(
                    TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))));
            columnA.SetOrder(2);
            table.AddColumn(columnA);
            table.AddColumn(
                new EdmProperty(
                    "Id",
                    ProviderRegistry.Sql2008_ProviderManifest.GetStoreType(
                        TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)))));

            var database = new EdmModel(DataSpace.CSpace);
            database.AddEntitySet("ES", table);

            (new ColumnOrderingConvention()).Apply(table, new DbModel(null, database));

            Assert.Equal(2, table.Properties.Count);
            Assert.Equal("C", table.Properties.First().Name);
        }

        [Fact]
        public void Apply_should_sort_unannotated_in_given_order()
        {
            var table = new EntityType("T", XmlConstants.TargetNamespace_3, DataSpace.SSpace);
            table.AddColumn(
                new EdmProperty(
                    "C",
                    ProviderRegistry.Sql2008_ProviderManifest.GetStoreType(
                        TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)))));
            table.AddColumn(
                new EdmProperty(
                    "Id",
                    ProviderRegistry.Sql2008_ProviderManifest.GetStoreType(
                        TypeUsage.Create(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)))));

            var database = new EdmModel(DataSpace.CSpace);
            database.AddEntitySet("ES", table);

            (new ColumnOrderingConvention()).Apply(table, new DbModel(null, database));

            Assert.Equal(2, table.Properties.Count);
            Assert.Equal("C", table.Properties.First().Name);
        }
    }
}
