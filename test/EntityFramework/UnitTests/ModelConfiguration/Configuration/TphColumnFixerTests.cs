// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.Resources;
    using System.Linq;
    using Moq;
    using Xunit;

    public class TphColumnFixerTests : TestBase
    {
        [Fact] // CodePlex 583
        public void RemoveDuplicateTphColumns_removes_duplicate_TPH_columns()
        {
            var removed = new List<EdmMember>();
            var mockTableType = CreateMockType("Ankh-Morpork");
            mockTableType.Setup(m => m.RemoveMember(It.IsAny<EdmMember>())).Callback<EdmMember>(removed.Add);

            var columns = CreateColumns(mockTableType, "Moist", "Moist");
            var mappings = CreateMappings(columns, "von", "Lipwig");

            new TphColumnFixer(mappings).RemoveDuplicateTphColumns();

            Assert.Equal(1, removed.Select(m => m.Name).Count(n => n == "Moist"));

            AssertDuplicatesRemoved(mappings, "Moist");
        }

        [Fact] // CodePlex 583
        public void RemoveDuplicateTphColumns_removes_multiple_batches_of_duplicate_columns_from_same_table()
        {
            var removed = new List<EdmMember>();
            var mockTableType = CreateMockType("Lancre");
            mockTableType.Setup(m => m.RemoveMember(It.IsAny<EdmMember>())).Callback<EdmMember>(removed.Add);

            var columns = CreateColumns(
                mockTableType,
                "Nanny", "Ogg", "Nanny", "Nanny", "Granny", "Magrat", "Weatherwax", "Weatherwax", "Magrat", "Garlik", "Tiffany", "Tiffany");

            var mappings = CreateMappings(
                columns,
                "Nanny1", "Ogg", "Nanny2", "Nanny2", "Granny", "Magrat1", "Weatherwax", "Weatherwax", "Magrat2", "Garlik", "Tiffany",
                "Tiffany");

            new TphColumnFixer(mappings).RemoveDuplicateTphColumns();

            Assert.Equal(5, removed.Count);
            Assert.Equal(2, removed.Select(m => m.Name).Count(n => n == "Nanny"));
            Assert.Equal(1, removed.Select(m => m.Name).Count(n => n == "Weatherwax"));
            Assert.Equal(1, removed.Select(m => m.Name).Count(n => n == "Magrat"));
            Assert.Equal(1, removed.Select(m => m.Name).Count(n => n == "Tiffany"));

            AssertDuplicatesRemoved(mappings, "Nanny");
            AssertDuplicatesRemoved(mappings, "Weatherwax");
            AssertDuplicatesRemoved(mappings, "Magrat");
            AssertDuplicatesRemoved(mappings, "Tiffany");
        }

        [Fact] // CodePlex 1021
        public void RemoveDuplicateTphColumns_does_not_attempt_to_remove_inherited_columns_twice()
        {
            var removed = new List<EdmMember>();
            var mockTableType = CreateMockType("Lancre");
            mockTableType.Setup(m => m.RemoveMember(It.IsAny<EdmMember>())).Callback<EdmMember>(removed.Add);

            var columns = CreateColumns(mockTableType, "Nanny", "Nanny");
            columns.Add(columns.Last());
            var mappings = CreateMappings(columns, "Nanny", "Nanny", "Nanny");

            mockTableType.Setup(m => m.HasMember(It.IsAny<EdmMember>())).Returns<EdmMember>(m => !removed.Contains(m));

            new TphColumnFixer(mappings).RemoveDuplicateTphColumns();

            Assert.Equal(1, removed.Count);
            Assert.Equal(1, removed.Select(m => m.Name).Count(n => n == "Nanny"));

            AssertDuplicatesRemoved(mappings, "Nanny");
        }

        private static void AssertDuplicatesRemoved(IEnumerable<ColumnMappingBuilder> mappings, string columnName)
        {
            EdmProperty refColumn = null;
            foreach (var column in mappings.Select(m => m.ColumnProperty).Where(c => c.Name == columnName))
            {
                refColumn = refColumn ?? column;
                Assert.Same(refColumn, column);
            }
        }

        [Fact] // CodePlex 583
        public void RemoveDuplicateTphColumns_throws_if_column_types_do_not_match()
        {
            var mockTableType = CreateMockType("Ankh-Morpork");

            var columns = new[]
                {
                    CreateColumn(
                        mockTableType, "Duke", new PrimitivePropertyConfiguration
                            {
                                ColumnType = "int"
                            }),
                    CreateColumn(
                        mockTableType, "Duke", new PrimitivePropertyConfiguration
                            {
                                ColumnType = "nvarchar(max)"
                            })
                };
            var mappings = CreateMappings(columns, "Sam", "Vimes");

            var fixer = new TphColumnFixer(mappings);

            Assert.Equal(
                Strings.BadTphMappingToSharedColumn(
                    "Sam", "SamType", "Vimes", "VimesType", "Duke", "Ankh-Morpork",
                    Environment.NewLine + "\t" + Strings.ConflictingConfigurationValue("ColumnType", "int", "ColumnType", "nvarchar(max)")),
                Assert.Throws<MappingException>(() => fixer.RemoveDuplicateTphColumns()).Message);
        }

        [Fact] // CodePlex 583
        public void RemoveDuplicateTphColumns_throws_if_column_facets_do_not_match()
        {
            var mockTableType = CreateMockType("Ankh-Morpork");

            var columns = new[]
                {
                    CreateColumn(
                        mockTableType, "Duke", new Properties.Primitive.StringPropertyConfiguration
                            {
                                IsUnicode = true
                            }),
                    CreateColumn(
                        mockTableType, "Duke", new Properties.Primitive.StringPropertyConfiguration
                            {
                                IsUnicode = false
                            })
                };
            var mappings = CreateMappings(columns, "Sam", "Vimes");

            var fixer = new TphColumnFixer(mappings);

            Assert.Equal(
                Strings.BadTphMappingToSharedColumn(
                    "Sam", "SamType", "Vimes", "VimesType", "Duke", "Ankh-Morpork",
                    Environment.NewLine + "\t" + Strings.ConflictingConfigurationValue("IsUnicode", "True", "IsUnicode", "False")),
                Assert.Throws<MappingException>(() => fixer.RemoveDuplicateTphColumns()).Message);
        }

        [Fact] // CodePlex 583
        public void RemoveDuplicateTphColumns_combines_non_conflicting_configuration_from_all_properties()
        {
            var mockTableType = CreateMockType("Ankh-Morpork");

            var columns = new[]
                {
                    CreateColumn(
                        mockTableType, "Duke", new Properties.Primitive.StringPropertyConfiguration
                            {
                                IsUnicode = false
                            }),
                    CreateColumn(
                        mockTableType, "Duke", new Properties.Primitive.StringPropertyConfiguration
                            {
                                MaxLength = 256,
                                IsFixedLength = true
                            }),
                    CreateColumn(
                        mockTableType, "Duke", new Properties.Primitive.StringPropertyConfiguration
                            {
                                MaxLength = 256,
                                IsNullable = true
                            })
                };
            var mappings = CreateMappings(columns, "Sam", "Vimes", "Rules");

            new TphColumnFixer(mappings).RemoveDuplicateTphColumns();

            Assert.Equal(256, mappings[0].ColumnProperty.MaxLength);
            Assert.Equal(false, mappings[0].ColumnProperty.IsUnicode);
            Assert.True(mappings[0].ColumnProperty.Nullable);
            Assert.Equal(true, mappings[0].ColumnProperty.IsFixedLength);
        }

        [Fact] // CodePlex 583
        public void RemoveDuplicateTphColumns_does_not_remove_columns_from_same_type()
        {
            var mockTableType = CreateMockType("Ankh-Morpork");
            var columns = CreateColumns(mockTableType, "Duke", "Duke");

            var entityType = CreateMockType("TheType", CreateMockType("Base").Object).Object;
            var mappings = new[]
                {
                    CreateColumnMapping(entityType, "von", columns[0]),
                    CreateColumnMapping(entityType, "Lipwig", columns[1])
                };

            new TphColumnFixer(mappings).RemoveDuplicateTphColumns();

            mockTableType.Verify(m => m.RemoveMember(It.IsAny<EdmMember>()), Times.Never());
        }

        [Fact] // CodePlex 583
        public void RemoveDuplicateTphColumns_does_not_remove_possibly_colliding_columns_from_types_in_different_hierarchies()
        {
            var mockTableType = CreateMockType("Ankh-Morpork");
            var columns = CreateColumns(mockTableType, "Duke", "Duke");

            var mappings = new[]
                {
                    CreateColumnMapping(CreateMockType("TheType1", CreateMockType("Base1").Object).Object, "von", columns[0]),
                    CreateColumnMapping(CreateMockType("TheType2", CreateMockType("Base2").Object).Object, "Lipwig", columns[1])
                };

            new TphColumnFixer(mappings).RemoveDuplicateTphColumns();

            mockTableType.Verify(m => m.RemoveMember(It.IsAny<EdmMember>()), Times.Never());
        }

        private static IList<EdmProperty> CreateColumns(Mock<EntityType> mockTable, params string[] names)
        {
            return names.Select(n => CreateColumn(mockTable, n)).ToList();
        }

        private static IList<ColumnMappingBuilder> CreateMappings(IEnumerable<EdmProperty> columns, params string[] propertyNames)
        {
            var baseType = CreateMockType("Base").Object;
            return columns.Zip(propertyNames, (t, p) => CreateColumnMapping(CreateMockType(p + "Type", baseType).Object, p, t)).ToList();
        }

        private static ColumnMappingBuilder CreateColumnMapping(EntityType baseType, string propertyName, EdmProperty column)
        {
            return new ColumnMappingBuilder(column, new[] { CreateMockMember(baseType, propertyName).Object });
        }

        private static EdmProperty CreateColumn(Mock<EntityType> mockTableType, string name, PrimitivePropertyConfiguration config = null)
        {
            var mockColumn = CreateMockMember(mockTableType.Object, name);
            mockColumn.Setup(m => m.Annotations).Returns(
                config == null
                    ? new DataModelAnnotation[0]
                    : new[]
                        {
                            new DataModelAnnotation
                                {
                                    Name = "Configuration",
                                    Value = config
                                }
                        });
            
            mockColumn.SetupProperty(
                m => m.TypeUsage, TypeUsage.CreateStringTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String), true, false));

            mockTableType.Setup(m => m.HasMember(mockColumn.Object)).Returns(true);

            return mockColumn.Object;
        }

        private static Mock<EdmProperty> CreateMockMember(EntityType declaringType, string name)
        {
            var mockMember = new Mock<EdmProperty>(name);
            mockMember.Setup(m => m.DeclaringType).Returns(declaringType);
            mockMember.Setup(m => m.Name).Returns(name);
            mockMember.Setup(m => m.Identity).Returns(name);
            return mockMember;
        }

        private static Mock<EntityType> CreateMockType(string name, EntityType baseType = null)
        {
            var mockType = new Mock<EntityType>(name, "Namespace", DataSpace.CSpace);
            mockType.Setup(m => m.BaseType).Returns(baseType);
            mockType.Setup(m => m.Name).Returns(name);
            return mockType;
        }
    }
}
