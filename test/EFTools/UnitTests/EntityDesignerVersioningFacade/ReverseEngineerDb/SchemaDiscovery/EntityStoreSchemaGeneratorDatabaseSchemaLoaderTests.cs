// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity.Core.EntityClient;
    using System.Linq;
    using Moq;
    using Xunit;

    public class EntityStoreSchemaGeneratorDatabaseSchemaLoaderTests
    {
        private readonly EntityClientMockFactory mockDataReaderFactory = new EntityClientMockFactory();

        [Fact]
        public void CreateFilteredCommand_creates_command_and_sets_parameters()
        {
            var command =
                new EntityStoreSchemaGeneratorDatabaseSchemaLoader(
                    new Mock<EntityConnection>().Object,
                    EntityFrameworkVersion.Version3,
                    false)
                    .CreateFilteredCommand(
                        "baseQuery",
                        "orderbyClause",
                        EntityStoreSchemaFilterObjectTypes.Table,
                        new List<EntityStoreSchemaFilterEntry>
                            {
                                new EntityStoreSchemaFilterEntry(
                                    "catalog",
                                    "schema",
                                    "name",
                                    EntityStoreSchemaFilterObjectTypes.Table,
                                    EntityStoreSchemaFilterEffect.Allow)
                            },
                        new[] { "alias" }
                    );

            Assert.NotNull(command);
            Assert.Equal(CommandType.Text, command.CommandType);
            Assert.Equal(
                "baseQuery\r\nWHERE\r\n((alias.CatalogName LIKE @p0 AND alias.SchemaName LIKE @p1 AND alias.Name LIKE @p2))\r\norderbyClause",
                command.CommandText);
            Assert.Equal(3, command.Parameters.Count);
            Assert.Equal(0, command.CommandTimeout);
        }

        [Fact]
        public void CreateFunctionDetailsCommand_sets_correct_query_for_target_schema_version()
        {
            var filters = Enumerable.Empty<EntityStoreSchemaFilterEntry>();

            Func<Version, string> getCommandText = (version) =>
                                                   new EntityStoreSchemaGeneratorDatabaseSchemaLoader(
                                                       new Mock<EntityConnection>().Object, version, false)
                                                       .CreateFunctionDetailsCommand(Enumerable.Empty<EntityStoreSchemaFilterEntry>())
                                                       .CommandText;

            Assert.DoesNotContain("sp.IsTvf", getCommandText(EntityFrameworkVersion.Version1));
            Assert.DoesNotContain("sp.IsTvf", getCommandText(EntityFrameworkVersion.Version2));
            Assert.Contains("sp.IsTvf", getCommandText(EntityFrameworkVersion.Version3));
        }

        [Fact]
        public void LoadTableDetails_returns_sorted_table_details()
        {
            TableBasedLoadDetailsTestRunner(
                loader => loader.LoadTableDetails(
                    new[]
                        {
                            new EntityStoreSchemaFilterEntry(
                              null,
                              null,
                              null,
                              EntityStoreSchemaFilterObjectTypes.Table,
                              EntityStoreSchemaFilterEffect.Allow)
                        }));
        }

        [Fact]
        public void LoadViewDetails_returns_sorted_view_details()
        {
            TableBasedLoadDetailsTestRunner(
                loader => loader.LoadViewDetails(
                    new[]
                        {
                            new EntityStoreSchemaFilterEntry(
                              null,
                              null,
                              null,
                              EntityStoreSchemaFilterObjectTypes.View,
                              EntityStoreSchemaFilterEffect.Allow)
                        }));
        }

        [Fact]
        public void LoadFunctionReturnTableDetails_returns_sorted_return_table_details()
        {
            TableBasedLoadDetailsTestRunner(
                loader => loader.LoadFunctionReturnTableDetails(
                    new[]
                        {
                            new EntityStoreSchemaFilterEntry(
                              null,
                              null,
                              null,
                              EntityStoreSchemaFilterObjectTypes.Function,
                              EntityStoreSchemaFilterEffect.Allow)
                        }));
        }

        [Fact]
        public void LoadRelationships_returns_sorted_relationships_details()
        {
            var input =
                new List<object[]>
                    {
                        new object[] { "c1", "s1", "t1", "Id", "c2", "s2", "t2", "Id", 4, "relationship1", "4", false },
                        new object[] { "c1", "s1", "t1", "Id", "c2", "s2", "t2", "Id", 1, "relationship1", "6", false },
                        new object[] { "c1", "s1", "t1", "Id", "c2", "s2", "t2", "Id", 1, "relationship1", "2", false },
                        new object[] { "c1", "s1", "t1", "Id", "c2", "s2", "t2", "Id", 1, "a-relationship", "2", false }
                    };

            var loader =
                new EntityStoreSchemaGeneratorDatabaseSchemaLoaderFake(
                    mockDataReaderFactory.CreateMockEntityCommand(input).Object);

            var results =
                loader.LoadRelationships(
                    new[]
                        {
                            new EntityStoreSchemaFilterEntry(
                                null,
                                null,
                                null,
                                EntityStoreSchemaFilterObjectTypes.Table,
                                EntityStoreSchemaFilterEffect.Allow),
                        }).ToArray();

            Assert.Equal(4, results.Length);

            Assert.True(
                input
                    .OrderBy(t => t[9])
                    .ThenBy(t => t[10])
                    .ThenBy(t => t[8])
                    .Zip(results, (i, r) => i[9] == r["RelationshipName"] && i[10] == r["RelationshipId"] && (int)i[8] == (int)r["Ordinal"])
                    .All(z => z)
                );
        }

        [Fact]
        public void LoadFunctionDetails_returns_function_details()
        {
            var input =
                new List<object[]>
                    {
                        new object[]
                            {
                                "catalog", "dbo", "f1", "int", 1, 0, 0, 0, 0, DBNull.Value, DBNull.Value, "OUT"
                            },
                        new object[]
                            {
                                DBNull.Value, "dbo", "f2", "int", 0, 1, 1, 1, 1, "Param", "nchar", "IN"
                            }
                    };

            var loader =
                new EntityStoreSchemaGeneratorDatabaseSchemaLoaderFake(
                    mockDataReaderFactory.CreateMockEntityCommand(input).Object);

            var results =
                loader.LoadFunctionDetails(
                    new[]
                        {
                            new EntityStoreSchemaFilterEntry(
                                null,
                                null,
                                null,
                                EntityStoreSchemaFilterObjectTypes.Function,
                                EntityStoreSchemaFilterEffect.Allow)
                        }).ToArray();

            Assert.Equal(2, results.Length);
            Assert.Equal("f1", results[0].ProcedureName);
            Assert.Equal("f2", results[1].ProcedureName);
        }

        [Fact]
        public void LoadStoreSchema_returns_initialized_StoreSchemaInstance()
        {
            var tableCommand =
                mockDataReaderFactory.CreateMockEntityCommand(
                    new List<object[]>
                        {
                            new object[]
                                {
                                    "catalog", "dbo", "table", "Id", 1, false, "int", 4, 0, 0, 0, true, true,
                                    true
                                }
                        });

            var viewCommand =
                mockDataReaderFactory.CreateMockEntityCommand(
                    new List<object[]>
                        {
                            new object[]
                                {
                                    "catalog", "dbo", "view", "Id", 1, false, "int", 4, 0, 0, 0, true, true,
                                    true
                                }
                        });

            var relationshipCommand =
                mockDataReaderFactory.CreateMockEntityCommand(
                    new List<object[]>
                        {
                            new object[]
                                {
                                    "catalog", "dbo", "source", "Id", "catalog", "schema", "target", "Id", 0, "relationship",
                                    "RelationshipId", false
                                }
                        });

            var functionCommand =
                mockDataReaderFactory.CreateMockEntityCommand(
                    new List<object[]>
                        {
                            new object[]
                                {
                                    DBNull.Value, "dbo", "f2", "int", 0, 1, 1, 1, 1, "Param", "nchar", "IN"
                                }
                        });

            var tvfReturnTypeCommand =
                mockDataReaderFactory.CreateMockEntityCommand(
                    new List<object[]>
                        {
                            new object[]
                                {
                                    "catalog", "dbo", "function", "Id", 1, false, "int", 4, 0, 0, 0, true, true, true
                                }
                        });

            var commands = new[] { tableCommand, viewCommand, relationshipCommand, functionCommand, tvfReturnTypeCommand };
            var storeSchemaDetails =
                new EntityStoreSchemaGeneratorDatabaseSchemaLoaderFake(commands.Select(c => c.Object).ToArray())
                    .LoadStoreSchemaDetails(new List<EntityStoreSchemaFilterEntry>());

            Assert.NotNull(storeSchemaDetails);
            Assert.Equal("table", storeSchemaDetails.TableDetails.Single().TableName);
            Assert.Equal("view", storeSchemaDetails.ViewDetails.Single().TableName);
            Assert.Equal("relationship", storeSchemaDetails.RelationshipDetails.Single().RelationshipName);
            Assert.Equal("f2", storeSchemaDetails.FunctionDetails.Single().ProcedureName);
            Assert.Equal("function", storeSchemaDetails.TVFReturnTypeDetails.Single().TableName);
        }

        [Fact]
        public void LoadFunctionReturnTableDetails_not_invoked_for_pre_V3_target_schema()
        {
            var mockEntityConnection = new Mock<EntityConnection>();

            foreach (var version in EntityFrameworkVersion.GetAllVersions())
            {
                var mockLoader =
                    new Mock<EntityStoreSchemaGeneratorDatabaseSchemaLoader>(
                        mockEntityConnection.Object, version)
                        {
                            CallBase = true
                        };

                mockLoader.Setup(l => l.LoadTableDetails(It.IsAny<IEnumerable<EntityStoreSchemaFilterEntry>>()))
                    .Returns(Enumerable.Empty<TableDetailsRow>());

                mockLoader.Setup(l => l.LoadViewDetails(It.IsAny<IEnumerable<EntityStoreSchemaFilterEntry>>()))
                    .Returns(Enumerable.Empty<TableDetailsRow>());

                mockLoader.Setup(l => l.LoadRelationships(It.IsAny<IEnumerable<EntityStoreSchemaFilterEntry>>()))
                    .Returns(Enumerable.Empty<RelationshipDetailsRow>());

                mockLoader.Setup(l => l.LoadFunctionDetails(It.IsAny<IEnumerable<EntityStoreSchemaFilterEntry>>()))
                    .Returns(Enumerable.Empty<FunctionDetailsRowView>());

                mockLoader.Setup(
                    l => l.LoadFunctionReturnTableDetails(It.IsAny<IEnumerable<EntityStoreSchemaFilterEntry>>()))
                    .Returns(Enumerable.Empty<TableDetailsRow>());

                mockLoader.Object.LoadStoreSchemaDetails(new List<EntityStoreSchemaFilterEntry>());

                mockLoader.Verify(
                    l => l.LoadTableDetails(It.IsAny<IEnumerable<EntityStoreSchemaFilterEntry>>()), Times.Once());

                mockLoader.Verify(
                    l => l.LoadViewDetails(It.IsAny<IList<EntityStoreSchemaFilterEntry>>()), Times.Once());

                mockLoader.Verify(
                    l => l.LoadRelationships(It.IsAny<IList<EntityStoreSchemaFilterEntry>>()), Times.Once());

                mockLoader.Verify(
                    l => l.LoadFunctionDetails(It.IsAny<IList<EntityStoreSchemaFilterEntry>>()), Times.Once());

                mockLoader.Verify(
                    l => l.LoadFunctionReturnTableDetails(It.IsAny<IEnumerable<EntityStoreSchemaFilterEntry>>()),
                    version == EntityFrameworkVersion.Version3 ? Times.Once() : Times.Never());
            }
        }

        private void TableBasedLoadDetailsTestRunner(Func<EntityStoreSchemaGeneratorDatabaseSchemaLoader, IEnumerable<DataRow>> loadDetails)
        {
            var input =
                new List<object[]>
                    {
                        new object[]
                            {
                                "catalog", "schema", "Score", "Id", 1, false, "int", 4, 0, 0, 0, true, true, true
                            },
                        new object[]
                            {
                                null, "dbo", "Games", "Id", 5, true, "smallint", 4, 0, 0, 0, false, false, false
                            },
                        new object[]
                            {
                                "catalog", "schema", "Names", "Name", 2, true, "nvarchar(max)", 4, 0, 0, 0, false,
                                false, false
                            },
                        new object[]
                            {
                                "catalog", "schema", "Names", "Id", 1, false, "int", 4, 0, 0, 0, true, true, true
                            }
                    };

            var results = loadDetails(
                new EntityStoreSchemaGeneratorDatabaseSchemaLoaderFake(
                    mockDataReaderFactory.CreateMockEntityCommand(input).Object)).ToArray();

            Assert.Equal(4, results.Length);

            Assert.True(
                input
                    .OrderBy(t => t[1])
                    .ThenBy(t => t[2])
                    .ThenBy(t => t[4])
                    .Zip(results, (i, r) => i[1] == r["SchemaName"] && i[2] == r["TableName"] && i[3] == r["ColumnName"])
                    .All(z => z)
                );
        }

        private class EntityStoreSchemaGeneratorDatabaseSchemaLoaderFake : EntityStoreSchemaGeneratorDatabaseSchemaLoader
        {
            private readonly EntityCommand[] _entityCommands;
            private int _timesCalled;

            public EntityStoreSchemaGeneratorDatabaseSchemaLoaderFake(EntityCommand entityCommand)
                : this(new[] { entityCommand })
            {
            }

            public EntityStoreSchemaGeneratorDatabaseSchemaLoaderFake(EntityCommand[] entityCommands)
                : base(new Mock<EntityConnection>().Object, EntityFrameworkVersion.Version3, false)
            {
                _entityCommands = entityCommands;
            }

            internal override EntityCommand CreateFilteredCommand(
                string sql, string orderByClause, EntityStoreSchemaFilterObjectTypes queryTypes, List<EntityStoreSchemaFilterEntry> filters,
                string[] filterAliases)
            {
                return _entityCommands[_timesCalled++];
            }
        }
    }
}
