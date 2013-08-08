// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Design
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Migrations.Model;
    using System.Linq;
    using Xunit;

    public class MigrationCodeGeneratorTests
    {
        [Fact]
        public void GetDefaultNamespaces_with_designer_false_returns_Migrations_namespace()
        {
            Assert.True(
                new DummyCodeGenerator().GetDefaultNamespaces()
                    .SequenceEqual(
                        new[]
                            {
                                "System",
                                "System.Data.Entity.Migrations"
                            }));
        }

        [Fact]
        public void GetDefaultNamespaces_with_designer_true_returns_Migrations_and_Infrastructure_namespace()
        {
            Assert.True(
                new DummyCodeGenerator()
                    .GetDefaultNamespaces(designer: true)
                    .SequenceEqual(
                        new[]
                            {
                                "System.CodeDom.Compiler",
                                "System.Data.Entity.Migrations",
                                "System.Data.Entity.Migrations.Infrastructure",
                                "System.Resources"
                            }));
        }

        [Fact]
        public void GetNamespaces_includes_spatial_namespace_when_geography_Add_column_operation_is_present()
        {
            Assert.True(
                new DummyCodeGenerator()
                    .GetNamespaces(
                        new[]
                            {
                                new AddColumnOperation(
                                    "T",
                                    new ColumnModel(PrimitiveTypeKind.Geography))
                            })
                    .SequenceEqual(
                        new[]
                            {
                                "System",
                                "System.Data.Entity.Migrations",
                                "System.Data.Entity.Spatial"
                            }));
        }

        [Fact]
        public void GetNamespaces_includes_spatial_namespace_when_geometry_Add_column_operation_is_present()
        {
            Assert.True(
                new DummyCodeGenerator()
                    .GetNamespaces(
                        new[]
                            {
                                new AddColumnOperation(
                                    "T",
                                    new ColumnModel(PrimitiveTypeKind.Geometry))
                            })
                    .SequenceEqual(
                        new[]
                            {
                                "System",
                                "System.Data.Entity.Migrations",
                                "System.Data.Entity.Spatial"
                            }));
        }

        [Fact]
        public void GetNamespaces_does_not_include_spatial_namespace_when_spatial_Add_column_operation_is_not_present()
        {
            Assert.True(
                new DummyCodeGenerator()
                    .GetNamespaces(
                        new[]
                            {
                                new AddColumnOperation(
                                    "T",
                                    new ColumnModel(PrimitiveTypeKind.Int32))
                            })
                    .SequenceEqual(
                        new[]
                            {
                                "System",
                                "System.Data.Entity.Migrations"
                            }));
        }

        /// <summary>
        /// Exposes protected methods for unit testing.
        /// </summary>
        public class DummyCodeGenerator : MigrationCodeGenerator
        {
            public new IEnumerable<string> GetNamespaces(IEnumerable<MigrationOperation> operations)
            {
                return base.GetNamespaces(operations);
            }

            public new IEnumerable<string> GetDefaultNamespaces(bool designer = false)
            {
                return base.GetDefaultNamespaces(designer);
            }

            public override ScaffoldedMigration Generate(
                string migrationId, IEnumerable<MigrationOperation> operations,
                string sourceModel, string targetModel, string @namespace,
                string className)
            {
                throw new NotImplementedException();
            }
        }
    }
}
