// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Design
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Utilities;
    using System.Linq;
    using Moq;
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

        [Fact]
        public void GetNamespaces_includes_generic_collations_if_any_annotations_are_present()
        {
            var generator = new DummyCodeGenerator();
            generator.AnnotationGenerators["A1"] = () => new TestGenerator("A1");
            
            Assert.Equal(
                new[]
                {
                    "System",
                    "System.Collections.Generic",
                    "System.Data.Entity.Migrations"
                },
                generator.GetNamespaces(new MigrationOperation[0]));
        }

        [Fact]
        public void GetNamespaces_includes_namespaces_from_any_registered_annotation_code_generators()
        {
            var generator = new DummyCodeGenerator();
            generator.AnnotationGenerators["A1"] = () => new TestGenerator("A1", "A1.B", "A1.C");
            generator.AnnotationGenerators["A2"] = () => new TestGenerator("A2", "A2.B", "A2.C");

            Assert.Equal(
                new[]
                {
                    "A1.B",
                    "A1.C",
                    "A2.B",
                    "A2.C",
                    "System",
                    "System.Collections.Generic",
                    "System.Data.Entity.Migrations"
                },
                generator.GetNamespaces(new MigrationOperation[0]));
        }

        [Fact]
        public void Annotation_code_generators_are_registered_for_any_annotations_in_the_operations_to_be_generated()
        {
            _annotationCount = 0;

            var mockResolver = new Mock<IDbDependencyResolver>();
            mockResolver.Setup(m => m.GetService(typeof(Func<AnnotationCodeGenerator>), It.IsAny<string>()))
                .Returns<Type, object>((s, k) => (Func<AnnotationCodeGenerator>)(() => new TestGenerator((string)k)));

            var generator = new DummyCodeGenerator(() => mockResolver.Object);

            generator.RegisterAnnotationGenerators(
                new MigrationOperation[]
                {
                    new AddColumnOperation("Foo", CreateColumnModel()),
                    new DropColumnOperation("Foo", "Bar", CreateAnnotations(), new AddColumnOperation("Foo", CreateColumnModel())),
                    new AlterColumnOperation("Foo", CreateColumnModel(), false),
                    CreateTableOperation(),
                    new DropTableOperation("Foo", CreateAnnotations(), CreateColumnAnnotations(), CreateTableOperation()),
                    AlterTableAnnotationsOperation(),
                });

            for (var i = 0; i < _annotationCount; i++)
            {
                var name = "A" + i;
                Assert.Equal(name, ((TestGenerator)generator.AnnotationGenerators[name]()).Name);
            }
        }

        private AlterTableAnnotationsOperation AlterTableAnnotationsOperation()
        {
            var operation = new AlterTableAnnotationsOperation("Foo", CreateAnnotationPairs());
            operation.Columns.Add(CreateColumnModel());
            return operation;
        }

        private CreateTableOperation CreateTableOperation()
        {
            var operation = new CreateTableOperation("Foo", CreateAnnotations());
            operation.Columns.Add(CreateColumnModel());
            return operation;
        }

        private int _annotationCount;

        private ColumnModel CreateColumnModel()
        {
            return new ColumnModel(PrimitiveTypeKind.Int32)
            {
                Name = "N",
                Annotations = CreateAnnotationPairs()
            };
        }

        private Dictionary<string, AnnotationPair> CreateAnnotationPairs()
        {
            return new Dictionary<string, AnnotationPair>
            {
                { "A" + _annotationCount++, new AnnotationPair("V1", "V2") },
                { "A" + _annotationCount++, new AnnotationPair("V1", "V2") }
            };
        }

        private Dictionary<string, object> CreateAnnotations()
        {
            return new Dictionary<string, object>
            {
                { "A" + _annotationCount++, "V" },
                { "A" + _annotationCount++, "V" }
            };
        }

        private Dictionary<string, IDictionary<string, object>> CreateColumnAnnotations()
        {
            return new Dictionary<string, IDictionary<string, object>>
            {
                { "C1", CreateAnnotations() },
                { "C2", CreateAnnotations() }
            };
        }

        [Fact]
        public void RegisterAnnotationGenerators_checks_arguments()
        {
            Assert.Equal(
                "operations",
                Assert.Throws<ArgumentNullException>(() => new DummyCodeGenerator().RegisterAnnotationGenerators(null)).ParamName);
        }

        /// <summary>
        /// Exposes protected methods for unit testing.
        /// </summary>
        public class DummyCodeGenerator : MigrationCodeGenerator
        {
            public DummyCodeGenerator(Func<IDbDependencyResolver> resolver = null)
                : base(resolver)
            {
            }

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

            public new IDictionary<string, Func<AnnotationCodeGenerator>> AnnotationGenerators
            {
                get { return base.AnnotationGenerators; }
            }

        }

        public class TestGenerator : AnnotationCodeGenerator
        {
            private readonly IEnumerable<string> _namespaces;
            private readonly string _name;

            public TestGenerator(string name, params string[] namespaces)
            {
                _name = name;
                _namespaces = namespaces ?? new string[0];
            }

            public string Name
            {
                get { return _name; }
            }

            public override void Generate(string annotationName, object annotation, IndentedTextWriter writer)
            {
            }

            public override IEnumerable<string> GetExtraNamespaces(IEnumerable<string> annotationNames)
            {
                return _namespaces;
            }
        }
    }
}
