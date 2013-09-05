// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.SqlGen
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.SqlServer.Utilities;
    using System.Linq;
    using Moq;
    using Xunit;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Text;

    public class SqlGeneratorTests
    {
        public class IntegerType
        {
            [Fact]
            public void IntegerType_returns_64_bit_integer_type()
            {
                var mockType = new Mock<PrimitiveType>();
                mockType.Setup(m => m.PrimitiveTypeKind).Returns(PrimitiveTypeKind.Int64);

                var mockItemCollection = new Mock<StoreItemCollection>();
                mockItemCollection.Setup(m => m.GetPrimitiveTypes()).Returns(
                    new ReadOnlyCollection<PrimitiveType>(new[] { mockType.Object }));

                var mockSqlGenerator = new Mock<SqlGenerator>
                                           {
                                               CallBase = true
                                           };
                mockSqlGenerator.Setup(m => m.StoreItemCollection).Returns(mockItemCollection.Object);

                Assert.Same(mockType.Object, mockSqlGenerator.Object.IntegerType.EdmType);
            }
        }

        public class GetTargetTSql
        {
            [Fact]
            public void GetTargetTSql_uses_defining_query_if_set()
            {
                var mockProperty = new Mock<MetadataProperty>();
                mockProperty.Setup(m => m.Name).Returns("DefiningQuery");
                mockProperty.Setup(m => m.Identity).Returns("DefiningQuery");
                mockProperty.Setup(m => m.Value).Returns("Run Run Run");

                var mockSet = new Mock<EntitySetBase>();
                mockSet.Setup(m => m.MetadataProperties).Returns(
                    new ReadOnlyMetadataCollection<MetadataProperty>(new[] { mockProperty.Object }));

                Assert.Equal("(Run Run Run)", SqlGenerator.GetTargetTSql(mockSet.Object));
            }

            [Fact]
            public void GetTargetTSql_uses_schema_and_table_name_if_defining_query_not_set()
            {
                var mockSchemaProperty = new Mock<MetadataProperty>();
                mockSchemaProperty.Setup(m => m.Name).Returns("Schema");
                mockSchemaProperty.Setup(m => m.Identity).Returns("Schema");
                mockSchemaProperty.Setup(m => m.Value).Returns("Velvet");

                var mockTableProperty = new Mock<MetadataProperty>();
                mockTableProperty.Setup(m => m.Name).Returns("Table");
                mockTableProperty.Setup(m => m.Identity).Returns("Table");
                mockTableProperty.Setup(m => m.Value).Returns("Underground");

                var mockSet = new Mock<EntitySetBase>();
                mockSet.Setup(m => m.MetadataProperties).Returns(
                    new ReadOnlyMetadataCollection<MetadataProperty>(new[] { mockSchemaProperty.Object, mockTableProperty.Object }));

                Assert.Equal("[Velvet].[Underground]", SqlGenerator.GetTargetTSql(mockSet.Object));
            }

            [Fact]
            public void GetTargetTSql_uses_container_and_set_name_if_schema_table_and_defining_query_not_set()
            {
                var mockContainer = new Mock<EntityContainer>("C", DataSpace.CSpace);
                mockContainer.Setup(m => m.Name).Returns("Boots");

                var mockSet = new Mock<EntitySetBase>();
                mockSet.Setup(m => m.MetadataProperties).Returns(
                    new ReadOnlyMetadataCollection<MetadataProperty>(new MetadataProperty[0]));
                mockSet.Setup(m => m.Name).Returns("Leather");
                mockSet.Setup(m => m.EntityContainer).Returns(mockContainer.Object);

                Assert.Equal("[Boots].[Leather]", SqlGenerator.GetTargetTSql(mockSet.Object));
            }
        }

        public class KeyFieldExpressionComparer
        {
            [Fact]
            public void Equals_returns_false_for_different_expression_types()
            {
                var mockLeft = new Mock<DbExpression>();
                mockLeft.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Null);

                var mockRight = new Mock<DbExpression>();
                mockRight.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Not);

                Assert.False(new SqlGenerator.KeyFieldExpressionComparer().Equals(mockLeft.Object, mockRight.Object));
            }

            [Fact]
            public void Equals_returns_true_for_VariableReference_expressions_if_references_are_same()
            {
                var mock = new Mock<DbExpression>();
                mock.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.VariableReference);

                Assert.True(new SqlGenerator.KeyFieldExpressionComparer().Equals(mock.Object, mock.Object));
            }

            [Fact]
            public void Equals_returns_true_for_VariableReference_expressions_if_references_are_different()
            {
                var mockLeft = new Mock<DbExpression>();
                mockLeft.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.VariableReference);

                var mockRight = new Mock<DbExpression>();
                mockRight.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.VariableReference);

                Assert.False(new SqlGenerator.KeyFieldExpressionComparer().Equals(mockLeft.Object, mockRight.Object));
            }

            [Fact]
            public void Equals_returns_true_for_Property_expressions_if_property_and_instance_are_same()
            {
                var mockProperty = new Mock<EdmMember>();

                var mockInstance = new Mock<DbExpression>();
                mockInstance.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.VariableReference);

                var mockLeft = new Mock<DbPropertyExpression>();
                mockLeft.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Property);
                mockLeft.Setup(m => m.Property).Returns(mockProperty.Object);
                mockLeft.Setup(m => m.Instance).Returns(mockInstance.Object);

                var mockRight = new Mock<DbPropertyExpression>();
                mockRight.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Property);
                mockRight.Setup(m => m.Property).Returns(mockProperty.Object);
                mockRight.Setup(m => m.Instance).Returns(mockInstance.Object);

                Assert.True(new SqlGenerator.KeyFieldExpressionComparer().Equals(mockLeft.Object, mockRight.Object));
            }

            [Fact]
            public void Equals_returns_false_for_Property_expressions_if_properties_are_not_same()
            {
                var mockInstance = new Mock<DbExpression>();
                mockInstance.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.VariableReference);

                var mockLeft = new Mock<DbPropertyExpression>();
                mockLeft.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Property);
                mockLeft.Setup(m => m.Property).Returns(new Mock<EdmMember>().Object);
                mockLeft.Setup(m => m.Instance).Returns(mockInstance.Object);

                var mockRight = new Mock<DbPropertyExpression>();
                mockRight.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Property);
                mockRight.Setup(m => m.Property).Returns(new Mock<EdmMember>().Object);
                mockRight.Setup(m => m.Instance).Returns(mockInstance.Object);

                Assert.False(new SqlGenerator.KeyFieldExpressionComparer().Equals(mockLeft.Object, mockRight.Object));
            }

            [Fact]
            public void Equals_returns_false_for_Property_expressions_if_instances_are_not_equal()
            {
                var mockProperty = new Mock<EdmMember>();

                var mockLeftInstance = new Mock<DbExpression>();
                mockLeftInstance.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.VariableReference);

                var mockRightInstance = new Mock<DbExpression>();
                mockRightInstance.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.VariableReference);

                var mockLeft = new Mock<DbPropertyExpression>();
                mockLeft.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Property);
                mockLeft.Setup(m => m.Property).Returns(mockProperty.Object);
                mockLeft.Setup(m => m.Instance).Returns(mockLeftInstance.Object);

                var mockRight = new Mock<DbPropertyExpression>();
                mockRight.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Property);
                mockRight.Setup(m => m.Property).Returns(mockProperty.Object);
                mockRight.Setup(m => m.Instance).Returns(mockRightInstance.Object);

                Assert.False(new SqlGenerator.KeyFieldExpressionComparer().Equals(mockLeft.Object, mockRight.Object));
            }

            [Fact]
            public void Equals_returns_true_for_Cast_expressions_if_result_type_and_argument_are_same()
            {
                var mockResultType = new Mock<TypeUsage>();

                var mockArgument = new Mock<DbExpression>();
                mockArgument.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.VariableReference);

                var mockLeft = new Mock<DbCastExpression>();
                mockLeft.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Cast);
                mockLeft.Setup(m => m.ResultType).Returns(mockResultType.Object);
                mockLeft.Setup(m => m.Argument).Returns(mockArgument.Object);

                var mockRight = new Mock<DbCastExpression>();
                mockRight.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Cast);
                mockRight.Setup(m => m.ResultType).Returns(mockResultType.Object);
                mockRight.Setup(m => m.Argument).Returns(mockArgument.Object);

                Assert.True(new SqlGenerator.KeyFieldExpressionComparer().Equals(mockLeft.Object, mockRight.Object));
            }

            [Fact]
            public void Equals_returns_false_for_Cast_expressions_if_result_types_are_not_same()
            {
                var mockArgument = new Mock<DbExpression>();
                mockArgument.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.VariableReference);

                var mockLeft = new Mock<DbCastExpression>();
                mockLeft.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Cast);
                mockLeft.Setup(m => m.ResultType).Returns(new Mock<TypeUsage>().Object);
                mockLeft.Setup(m => m.Argument).Returns(mockArgument.Object);

                var mockRight = new Mock<DbCastExpression>();
                mockRight.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Cast);
                mockRight.Setup(m => m.ResultType).Returns(new Mock<TypeUsage>().Object);
                mockRight.Setup(m => m.Argument).Returns(mockArgument.Object);

                Assert.False(new SqlGenerator.KeyFieldExpressionComparer().Equals(mockLeft.Object, mockRight.Object));
            }

            [Fact]
            public void Equals_returns_false_for_Cast_expressions_if_arguments_are_not_equal()
            {
                var mockResultType = new Mock<TypeUsage>();

                var mockLeftArgument = new Mock<DbExpression>();
                mockLeftArgument.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.VariableReference);

                var mockRightArgument = new Mock<DbExpression>();
                mockRightArgument.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.VariableReference);

                var mockLeft = new Mock<DbCastExpression>();
                mockLeft.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Cast);
                mockLeft.Setup(m => m.ResultType).Returns(mockResultType.Object);
                mockLeft.Setup(m => m.Argument).Returns(mockLeftArgument.Object);

                var mockRight = new Mock<DbCastExpression>();
                mockRight.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Cast);
                mockRight.Setup(m => m.ResultType).Returns(mockResultType.Object);
                mockRight.Setup(m => m.Argument).Returns(mockRightArgument.Object);

                Assert.False(new SqlGenerator.KeyFieldExpressionComparer().Equals(mockLeft.Object, mockRight.Object));
            }

            [Fact]
            public void Equals_always_returns_false_for_unknown_expression_types()
            {
                var mock = new Mock<DbCastExpression>();
                mock.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Not);

                Assert.False(new SqlGenerator.KeyFieldExpressionComparer().Equals(mock.Object, mock.Object));
            }

            [Fact]
            public void GetHashCode_for_Property_expression_returns_hash_code_of_property()
            {
                var mockProperty = new Mock<EdmMember>();

                var mockExpression = new Mock<DbPropertyExpression>();
                mockExpression.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Property);
                mockExpression.Setup(m => m.Property).Returns(mockProperty.Object);

                Assert.Equal(
                    mockProperty.Object.GetHashCode(),
                    new SqlGenerator.KeyFieldExpressionComparer().GetHashCode(mockExpression.Object));
            }

            [Fact]
            public void GetHashCode_for_ParameterReference_expression_returns_modified_hash_code_of_parameter_name()
            {
                var mockExpression = new Mock<DbParameterReferenceExpression>();
                mockExpression.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.ParameterReference);
                mockExpression.Setup(m => m.ParameterName).Returns("Bing!");

                Assert.Equal(
                    "Bing!".GetHashCode() ^ Int32.MaxValue,
                    new SqlGenerator.KeyFieldExpressionComparer().GetHashCode(mockExpression.Object));
            }

            [Fact]
            public void GetHashCode_for_VariableReference_expression_returns_hash_code_of_variable_name()
            {
                var mockExpression = new Mock<DbVariableReferenceExpression>();
                mockExpression.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.VariableReference);
                mockExpression.Setup(m => m.VariableName).Returns("Bing!");

                Assert.Equal("Bing!".GetHashCode(), new SqlGenerator.KeyFieldExpressionComparer().GetHashCode(mockExpression.Object));
            }

            [Fact]
            public void GetHashCode_for_Cast_expression_returns_hash_code_of_argument()
            {
                var mockArgumenExpression = new Mock<DbVariableReferenceExpression>();
                mockArgumenExpression.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.VariableReference);
                mockArgumenExpression.Setup(m => m.VariableName).Returns("Bing!");

                var mockExpression = new Mock<DbCastExpression>();
                mockExpression.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Cast);
                mockExpression.Setup(m => m.Argument).Returns(mockArgumenExpression.Object);

                Assert.Equal("Bing!".GetHashCode(), new SqlGenerator.KeyFieldExpressionComparer().GetHashCode(mockExpression.Object));
            }

            [Fact]
            public void GetHashCode_for_unknown_expression_returns_hash_code_of_expression_object()
            {
                var mockExpression = new Mock<DbPropertyExpression>();

                Assert.Equal(
                    mockExpression.Object.GetHashCode(),
                    new SqlGenerator.KeyFieldExpressionComparer().GetHashCode(mockExpression.Object));
            }
        }

        public class IsKeyForIn
        {
            [Fact]
            public void IsKeyForIn_returns_true_for_properties_and_references_only()
            {
                var expressionKinds =
                    new[]
                        {
                            DbExpressionKind.Property,
                            DbExpressionKind.VariableReference,
                            DbExpressionKind.ParameterReference,
                        };

                Enum.GetValues(typeof(DbExpressionKind))
                    .OfType<DbExpressionKind>()
                    .Each(
                        e =>
                            {
                                var mockDbExpression = new Mock<DbExpression>();
                                mockDbExpression.Setup(m => m.ExpressionKind).Returns(e);
                                Assert.Equal(expressionKinds.Contains(e), SqlGenerator.IsKeyForIn(mockDbExpression.Object));
                            });
            }
        }

        public class TryAddExpressionForIn
        {
            [Fact]
            public void TryAddExpressionForIn_adds_left_and_right_values_and_returns_true_if_only_left_expression_matches()
            {
                var mockLeftExpression = CreateMockArgumentExpression(DbExpressionKind.Property);
                var mockRightExpression = CreateMockArgumentExpression(DbExpressionKind.Not);
                var mockBinaryExpression = CreateMockBinaryExpression(mockLeftExpression, mockRightExpression, DbExpressionKind.Or);

                var map = new Dictionary<DbExpression, IList<DbExpression>>(new SqlGenerator.KeyFieldExpressionComparer());

                Assert.True(SqlGenerator.TryAddExpressionForIn(mockBinaryExpression.Object, map));

                Assert.Equal(1, map.Keys.Count());
                var values = map[mockLeftExpression.Object];
                Assert.Same(mockRightExpression.Object, values.Single());
            }

            [Fact]
            public void TryAddExpressionForIn_adds_left_and_right_values_and_returns_true_if_both_expressions_match()
            {
                var mockLeftExpression = CreateMockArgumentExpression(DbExpressionKind.Property);
                var mockRightExpression = CreateMockArgumentExpression(DbExpressionKind.Property);
                var mockBinaryExpression = CreateMockBinaryExpression(mockLeftExpression, mockRightExpression, DbExpressionKind.Or);

                var map = new Dictionary<DbExpression, IList<DbExpression>>(new SqlGenerator.KeyFieldExpressionComparer());

                Assert.True(SqlGenerator.TryAddExpressionForIn(mockBinaryExpression.Object, map));

                Assert.Equal(1, map.Keys.Count());
                var values = map[mockLeftExpression.Object];
                Assert.Same(mockRightExpression.Object, values.Single());
            }

            [Fact]
            public void TryAddExpressionForIn_adds_right_and_left_values_and_returns_true_if_only_right_expression_matches()
            {
                var mockLeftExpression = CreateMockArgumentExpression(DbExpressionKind.Not);
                var mockRightExpression = CreateMockArgumentExpression(DbExpressionKind.Property);
                var mockBinaryExpression = CreateMockBinaryExpression(mockLeftExpression, mockRightExpression, DbExpressionKind.Or);

                var map = new Dictionary<DbExpression, IList<DbExpression>>(new SqlGenerator.KeyFieldExpressionComparer());

                Assert.True(SqlGenerator.TryAddExpressionForIn(mockBinaryExpression.Object, map));

                Assert.Equal(1, map.Keys.Count());
                var values = map[mockRightExpression.Object];
                Assert.Same(mockLeftExpression.Object, values.Single());
            }

            [Fact]
            public void TryAddExpressionForIn_adds_nothing_and_returns_false_if_neither_left_or_right_match()
            {
                var mockBinaryExpression = CreateMockBinaryExpression(
                    CreateMockArgumentExpression(DbExpressionKind.Not),
                    CreateMockArgumentExpression(DbExpressionKind.Not), DbExpressionKind.Or);

                var map = new Dictionary<DbExpression, IList<DbExpression>>(new SqlGenerator.KeyFieldExpressionComparer());

                Assert.False(SqlGenerator.TryAddExpressionForIn(mockBinaryExpression.Object, map));

                Assert.Equal(0, map.Keys.Count());
            }
        }

        public class HasBuiltMapForIn
        {
            [Fact]
            public void HasBuiltMapForIn_for_equals_returns_true_and_gets_values_from_TryAddExpressionForIn_when_arguments_match()
            {
                var mockLeftExpression = CreateMockArgumentExpression(DbExpressionKind.Not);
                var mockRightExpression = CreateMockArgumentExpression(DbExpressionKind.Property);
                var mockBinaryExpression = CreateMockBinaryExpression(mockLeftExpression, mockRightExpression, DbExpressionKind.Equals);

                var map = new Dictionary<DbExpression, IList<DbExpression>>(new SqlGenerator.KeyFieldExpressionComparer());

                Assert.True(SqlGenerator.HasBuiltMapForIn(mockBinaryExpression.Object, map));

                Assert.Equal(1, map.Keys.Count());
                var values = map[mockRightExpression.Object];
                Assert.Same(mockLeftExpression.Object, values.Single());
            }

            [Fact]
            public void HasBuiltMapForIn_for_equals_returns_false_when_arguments_dont_match()
            {
                var mockBinaryExpression = CreateMockBinaryExpression(
                    CreateMockArgumentExpression(DbExpressionKind.Not),
                    CreateMockArgumentExpression(DbExpressionKind.Not), DbExpressionKind.Or);

                Assert.False(
                    SqlGenerator.HasBuiltMapForIn(
                        mockBinaryExpression.Object,
                        new Dictionary<DbExpression, IList<DbExpression>>(new SqlGenerator.KeyFieldExpressionComparer())));
            }

            [Fact]
            public void HasBuiltMapForIn_for_null_returns_true_and_gets_value_when_argument_matches()
            {
                var mockArgumentExpression = CreateMockArgumentExpression(DbExpressionKind.Property);
                SetupMockArguments(mockArgumentExpression);

                var mockNullExppression = new Mock<DbIsNullExpression>();
                mockNullExppression.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.IsNull);
                mockNullExppression.Setup(m => m.Argument).Returns(mockArgumentExpression.Object);

                var map = new Dictionary<DbExpression, IList<DbExpression>>(new SqlGenerator.KeyFieldExpressionComparer());

                Assert.True(SqlGenerator.HasBuiltMapForIn(mockNullExppression.Object, map));

                Assert.Equal(1, map.Keys.Count());
                var values = map[mockArgumentExpression.Object];
                Assert.Same(mockNullExppression.Object, values.Single());
            }

            [Fact]
            public void HasBuiltMapForIn_for_is_null_returns_false_when_argument_does_not_match()
            {
                var mockNullExppression = new Mock<DbIsNullExpression>();
                mockNullExppression.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.IsNull);
                mockNullExppression.Setup(m => m.Argument).Returns(CreateMockArgumentExpression(DbExpressionKind.Not).Object);

                var map = new Dictionary<DbExpression, IList<DbExpression>>(new SqlGenerator.KeyFieldExpressionComparer());

                Assert.False(SqlGenerator.HasBuiltMapForIn(mockNullExppression.Object, map));
            }

            [Fact]
            public void HasBuiltMapForIn_for_Or_returns_true_and_gets_both_values_when_left_and_right_both_match()
            {
                var mockLeftArgumentExpression = CreateArgumentForNullExpression();
                var mockLeftExpression = CreatNullExpression(mockLeftArgumentExpression, DbExpressionKind.IsNull);

                var mockRightArgumentExpression = CreateArgumentForNullExpression();
                var mockRightExpression = CreatNullExpression(mockRightArgumentExpression, DbExpressionKind.IsNull);

                var mockBinaryExpression = new Mock<DbBinaryExpression>();
                mockBinaryExpression.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Or);
                mockBinaryExpression.Setup(m => m.Left).Returns(mockLeftExpression.Object);
                mockBinaryExpression.Setup(m => m.Right).Returns(mockRightExpression.Object);

                var map = new Dictionary<DbExpression, IList<DbExpression>>(new SqlGenerator.KeyFieldExpressionComparer());

                Assert.True(SqlGenerator.HasBuiltMapForIn(mockBinaryExpression.Object, map));

                Assert.Equal(2, map.Keys.Count());

                var leftValues = map[mockLeftArgumentExpression.Object];
                Assert.Same(mockLeftExpression.Object, leftValues.Single());

                var rightValues = map[mockRightArgumentExpression.Object];
                Assert.Same(mockRightExpression.Object, rightValues.Single());
            }

            [Fact]
            public void HasBuiltMapForIn_for_Or_returns_false_if_left_does_not_match()
            {
                var mockBinaryExpression = new Mock<DbBinaryExpression>();
                mockBinaryExpression.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Or);

                mockBinaryExpression.Setup(m => m.Left).Returns(
                    CreatNullExpression(
                        CreateArgumentForNullExpression(),
                        DbExpressionKind.Not).Object);

                mockBinaryExpression.Setup(m => m.Right).Returns(
                    CreatNullExpression(
                        CreateArgumentForNullExpression(),
                        DbExpressionKind.IsNull).Object);

                var map = new Dictionary<DbExpression, IList<DbExpression>>(new SqlGenerator.KeyFieldExpressionComparer());

                Assert.False(SqlGenerator.HasBuiltMapForIn(mockBinaryExpression.Object, map));
            }

            [Fact]
            public void HasBuiltMapForIn_for_Or_returns_false_if_right_does_not_match()
            {
                var mockBinaryExpression = new Mock<DbBinaryExpression>();
                mockBinaryExpression.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Or);

                mockBinaryExpression.Setup(m => m.Left).Returns(
                    CreatNullExpression(CreateArgumentForNullExpression(), DbExpressionKind.IsNull).Object);

                mockBinaryExpression.Setup(m => m.Right).Returns(
                    CreatNullExpression(CreateArgumentForNullExpression(), DbExpressionKind.Not).Object);

                var map = new Dictionary<DbExpression, IList<DbExpression>>(new SqlGenerator.KeyFieldExpressionComparer());

                Assert.False(SqlGenerator.HasBuiltMapForIn(mockBinaryExpression.Object, map));
            }

            [Fact]
            public void Top_level_HasBuiltMapForIn_returns_non_null_map_if_recursive_HasBuiltMapForIn_returns_true()
            {
                var mockLeftArgumentExpression = CreateArgumentForNullExpression();
                var mockLeftExpression = CreatNullExpression(mockLeftArgumentExpression, DbExpressionKind.IsNull);

                var mockRightArgumentExpression = CreateArgumentForNullExpression();
                var mockRightExpression = CreatNullExpression(mockRightArgumentExpression, DbExpressionKind.IsNull);

                var mockBinaryExpression = new Mock<DbOrExpression>();
                mockBinaryExpression.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Or);
                mockBinaryExpression.Setup(m => m.Left).Returns(mockLeftExpression.Object);
                mockBinaryExpression.Setup(m => m.Right).Returns(mockRightExpression.Object);

                var map = SqlGenerator.HasBuiltMapForIn(mockBinaryExpression.Object);

                Assert.Equal(2, map.Keys.Count());

                var leftValues = map[mockLeftArgumentExpression.Object];
                Assert.Same(mockLeftExpression.Object, leftValues.Single());

                var rightValues = map[mockRightArgumentExpression.Object];
                Assert.Same(mockRightExpression.Object, rightValues.Single());
            }

            [Fact]
            public void Top_level_HasBuiltMapForIn_returns_null_map_if_recursive_HasBuiltMapForIn_returns_false()
            {
                var mockBinaryExpression = new Mock<DbOrExpression>();
                mockBinaryExpression.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Or);

                mockBinaryExpression.Setup(m => m.Left).Returns(
                    CreatNullExpression(CreateArgumentForNullExpression(), DbExpressionKind.Not).Object);

                mockBinaryExpression.Setup(m => m.Right).Returns(
                    CreatNullExpression(CreateArgumentForNullExpression(), DbExpressionKind.IsNull).Object);

                Assert.Null(SqlGenerator.HasBuiltMapForIn(mockBinaryExpression.Object));
            }

            private static Mock<DbPropertyExpression> CreateArgumentForNullExpression()
            {
                var mockLeftArgumentExpression = CreateMockArgumentExpression(DbExpressionKind.Property);
                SetupMockArguments(mockLeftArgumentExpression);
                return mockLeftArgumentExpression;
            }

            private static Mock<DbIsNullExpression> CreatNullExpression(
                Mock<DbPropertyExpression> mockLeftArgumentExpression,
                DbExpressionKind kind)
            {
                var mockLeftExpression = new Mock<DbIsNullExpression>();
                mockLeftExpression.Setup(m => m.ExpressionKind).Returns(kind);
                mockLeftExpression.Setup(m => m.Argument).Returns(mockLeftArgumentExpression.Object);

                return mockLeftExpression;
            }
        }

        public class ListForKey
        {
            [Fact]
            public void ListForKey_returns_values_for_a_given_key()
            {
                var mockKey1 = CreateMockArgumentExpression(DbExpressionKind.Property);
                SetupMockArguments(mockKey1);
                var mockKey2 = CreateMockArgumentExpression(DbExpressionKind.Property);
                SetupMockArguments(mockKey2);

                var values1 = new[] { new Mock<DbExpression>().Object, new Mock<DbExpression>().Object };
                var values2 = new[] { new Mock<DbExpression>().Object, new Mock<DbExpression>().Object };

                var map = new Dictionary<DbExpression, IList<DbExpression>>(new SqlGenerator.KeyFieldExpressionComparer());
                map.Add(mockKey1.Object, values1[0]);
                map.Add(mockKey1.Object, values1[1]);
                map.Add(mockKey2.Object, values2[0]);
                map.Add(mockKey2.Object, values2[1]);

                Assert.Equal(values1, map[mockKey1.Object].ToArray());
                Assert.Equal(values2, map[mockKey2.Object].ToArray());
            }
        }

        private static Mock<DbPropertyExpression> CreateMockArgumentExpression(DbExpressionKind kind)
        {
            var mockLeftExpression = new Mock<DbPropertyExpression>();
            mockLeftExpression.Setup(m => m.ExpressionKind).Returns(kind);

            return mockLeftExpression;
        }

        private static Mock<DbBinaryExpression> CreateMockBinaryExpression(
            Mock<DbPropertyExpression> mockLeftExpression,
            Mock<DbPropertyExpression> mockRightExpression,
            DbExpressionKind kind)
        {
            SetupMockArguments(mockLeftExpression, mockRightExpression);

            var mockBinaryExpression = new Mock<DbBinaryExpression>();
            mockBinaryExpression.Setup(m => m.ExpressionKind).Returns(kind);
            mockBinaryExpression.Setup(m => m.Left).Returns(mockLeftExpression.Object);
            mockBinaryExpression.Setup(m => m.Right).Returns(mockRightExpression.Object);

            return mockBinaryExpression;
        }

        private static void SetupMockArguments(
            Mock<DbPropertyExpression> mockLeftExpression, Mock<DbPropertyExpression> mockRightExpression = null)
        {
            var mockInstanceExpression = new Mock<DbExpression>();
            mockInstanceExpression.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.VariableReference);

            var mockEdmMember = new Mock<EdmMember>();
            //mockEdmMember.Setup(m => m.GetHashCode()).Returns(1);

            mockLeftExpression.Setup(m => m.Property).Returns(mockEdmMember.Object);
            mockLeftExpression.Setup(m => m.Instance).Returns(mockInstanceExpression.Object);

            if (mockRightExpression != null)
            {
                mockRightExpression.Setup(m => m.Property).Returns(mockEdmMember.Object);
                mockRightExpression.Setup(m => m.Instance).Returns(mockInstanceExpression.Object);
            }
        }

        public class IsConstParamOrNullExpressionUnicodeNotSpecified
        {
            [Fact]
            public void IsConstParamOrNullExpressionUnicodeNotSpecified_returns_true_for_string_constant_with_undefined_character_set()
            {
                Assert.True(
                    SqlGenerator.IsConstParamOrNullExpressionUnicodeNotSpecified(
                        CreateMockDbArgumentExpression(
                            PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object));
            }

            [Fact]
            public void IsConstParamOrNullExpressionUnicodeNotSpecified_returns_true_for_null_string_with_undefined_character_set()
            {
                Assert.True(
                    SqlGenerator.IsConstParamOrNullExpressionUnicodeNotSpecified(
                        CreateMockDbArgumentExpression(
                            PrimitiveTypeKind.String, DbExpressionKind.Null, isUnicode: null).Object));
            }

            [Fact]
            public void IsConstParamOrNullExpressionUnicodeNotSpecified_returns_true_for_string_parameter_with_undefined_character_set()
            {
                Assert.True(
                    SqlGenerator.IsConstParamOrNullExpressionUnicodeNotSpecified(
                        CreateMockDbArgumentExpression(
                            PrimitiveTypeKind.String, DbExpressionKind.ParameterReference, isUnicode: null).Object));
            }

            [Fact]
            public void IsConstParamOrNullExpressionUnicodeNotSpecified_returns_false_for_unicode_string_constant_null_or_parameter()
            {
                Assert.False(
                    SqlGenerator.IsConstParamOrNullExpressionUnicodeNotSpecified(
                        CreateMockDbArgumentExpression(
                            PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: true).Object));
            }

            [Fact]
            public void IsConstParamOrNullExpressionUnicodeNotSpecified_returns_false_for_non_unicode_string_constant_null_or_parameter()
            {
                Assert.False(
                    SqlGenerator.IsConstParamOrNullExpressionUnicodeNotSpecified(
                        CreateMockDbArgumentExpression(
                            PrimitiveTypeKind.String, DbExpressionKind.Null, isUnicode: false).Object));
            }

            [Fact]
            public void IsConstParamOrNullExpressionUnicodeNotSpecified_returns_false_for_non_string()
            {
                Assert.False(
                    SqlGenerator.IsConstParamOrNullExpressionUnicodeNotSpecified(
                        CreateMockDbArgumentExpression(
                            PrimitiveTypeKind.Int16, DbExpressionKind.Constant, isUnicode: null).Object));
            }
        }

        public class MatchTargetPatternForForcingNonUnicode
        {
            [Fact]
            public void MatchTargetPatternForForcingNonUnicode_returns_true_for_const_param_or_null_string_with_unkown_coding()
            {
                Assert.True(
                    new SqlGenerator().MatchTargetPatternForForcingNonUnicode(
                        CreateMockDbArgumentExpression(
                            PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object));
            }

            [Fact]
            public void MatchTargetPatternForForcingNonUnicode_returns_false_for_non_function()
            {
                Assert.False(
                    new SqlGenerator().MatchTargetPatternForForcingNonUnicode(
                        CreateMockDbArgumentExpression(
                            PrimitiveTypeKind.String, DbExpressionKind.Skip, isUnicode: null).Object));
            }

            [Fact]
            public void MatchTargetPatternForForcingNonUnicode_returns_false_for_non_canonical_and_non_store_function()
            {
                var mockFunctionExpression = CreateMockDbFunctionExpression("My.Function", builtInAttribute: false);

                Assert.False(new SqlGenerator().MatchTargetPatternForForcingNonUnicode(mockFunctionExpression.Object));
            }

            [Fact]
            public void MatchTargetPatternForForcingNonUnicode_returns_false_for_non_matching_one_arg_function()
            {
                Assert.False(
                    new SqlGenerator().MatchTargetPatternForForcingNonUnicode(
                        CreateMockDbFunctionExpression(
                            "Edm.NoMatch", builtInAttribute: false, arguments: CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object).Object));
            }

            [Fact]
            public void MatchTargetPatternForForcingNonUnicode_returns_true_for_matching_one_arg_canonical_function()
            {
                Assert.True(
                    new SqlGenerator().MatchTargetPatternForForcingNonUnicode(
                        CreateMockDbFunctionExpression(
                            "Edm.Left", builtInAttribute: false, arguments: CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object).Object));
            }

            [Fact]
            public void MatchTargetPatternForForcingNonUnicode_returns_false_for_matching_one_arg_canonical_function_with_non_matching_arg()
            {
                Assert.False(
                    new SqlGenerator().MatchTargetPatternForForcingNonUnicode(
                        CreateMockDbFunctionExpression(
                            "Edm.Left", builtInAttribute: false, arguments: CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.Int32, DbExpressionKind.Constant, isUnicode: null).Object).Object));
            }

            [Fact]
            public void MatchTargetPatternForForcingNonUnicode_returns_true_for_matching_one_arg_store_function()
            {
                Assert.True(
                    new SqlGenerator().MatchTargetPatternForForcingNonUnicode(
                        CreateMockDbFunctionExpression(
                            "SqlServer.LEFT", builtInAttribute: true, arguments: CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object).Object));
            }

            [Fact]
            public void MatchTargetPatternForForcingNonUnicode_returns_false_for_matching_one_arg_store_function_with_non_matching_arg()
            {
                Assert.False(
                    new SqlGenerator().MatchTargetPatternForForcingNonUnicode(
                        CreateMockDbFunctionExpression(
                            "SqlServer.LEFT", builtInAttribute: true, arguments: CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.Int32, DbExpressionKind.Constant, isUnicode: null).Object).Object));
            }

            [Fact]
            public void MatchTargetPatternForForcingNonUnicode_returns_false_for_non_matching_two_arg_function()
            {
                Assert.False(
                    new SqlGenerator().MatchTargetPatternForForcingNonUnicode(
                        CreateMockDbFunctionExpression(
                            "Edm.NoMatch",
                            false,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object).Object));
            }

            [Fact]
            public void MatchTargetPatternForForcingNonUnicode_returns_true_for_matching_two_arg_canonical_function()
            {
                Assert.True(
                    new SqlGenerator().MatchTargetPatternForForcingNonUnicode(
                        CreateMockDbFunctionExpression(
                            "Edm.Concat",
                            false,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object).Object));
            }

            [Fact]
            public void MatchTargetPatternForForcingNonUnicode_returns_false_for_matching_two_arg_canonical_function_with_non_matching_arg()
            {
                Assert.False(
                    new SqlGenerator().MatchTargetPatternForForcingNonUnicode(
                        CreateMockDbFunctionExpression(
                            "Edm.Concat",
                            false,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.Int32, DbExpressionKind.Constant, isUnicode: null).Object).Object));
            }

            [Fact]
            public void MatchTargetPatternForForcingNonUnicode_returns_false_for_non_matching_three_arg_function()
            {
                Assert.False(
                    new SqlGenerator().MatchTargetPatternForForcingNonUnicode(
                        CreateMockDbFunctionExpression(
                            "Edm.NoMatch",
                            false,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object).Object));
            }

            [Fact]
            public void MatchTargetPatternForForcingNonUnicode_returns_true_for_matching_three_arg_canonical_function()
            {
                Assert.True(
                    new SqlGenerator().MatchTargetPatternForForcingNonUnicode(
                        CreateMockDbFunctionExpression(
                            "Edm.Replace",
                            false,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object).Object));
            }

            [Fact]
            public void MatchTargetPatternForForcingNonUnicode_returns_false_for_matching_three_arg_canonical_function_with_non_matching_arg
                ()
            {
                Assert.False(
                    new SqlGenerator().MatchTargetPatternForForcingNonUnicode(
                        CreateMockDbFunctionExpression(
                            "Edm.Replace",
                            false,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.Int32, DbExpressionKind.Constant, isUnicode: null).Object).Object));
            }

            [Fact]
            public void MatchTargetPatternForForcingNonUnicode_returns_true_for_matching_three_arg_store_function()
            {
                Assert.True(
                    new SqlGenerator().MatchTargetPatternForForcingNonUnicode(
                        CreateMockDbFunctionExpression(
                            "SqlServer.REPLACE",
                            true,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object).Object));
            }

            [Fact]
            public void MatchTargetPatternForForcingNonUnicode_returns_false_for_matching_three_arg_store_function_with_non_matching_arg()
            {
                Assert.False(
                    new SqlGenerator().MatchTargetPatternForForcingNonUnicode(
                        CreateMockDbFunctionExpression(
                            "SqlServer.REPLACE",
                            true,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.String, DbExpressionKind.Constant, isUnicode: null).Object,
                            CreateMockDbArgumentExpression(
                                PrimitiveTypeKind.Int32, DbExpressionKind.Constant, isUnicode: null).Object).Object));
            }
        }

        private static Mock<DbFunctionExpression> CreateMockDbFunctionExpression(
            string functionName,
            bool builtInAttribute,
            params DbExpression[] arguments)
        {
            arguments = arguments ?? new DbExpression[0];

            var mockEdmType = new Mock<PrimitiveType>();
            mockEdmType.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.PrimitiveType);
            mockEdmType.Setup(m => m.PrimitiveTypeKind).Returns(PrimitiveTypeKind.String);

            var mockTypeUsage = new Mock<TypeUsage>();
            mockTypeUsage.Setup(m => m.EdmType).Returns(mockEdmType.Object);

            var mockExpression = new Mock<DbFunctionExpression>();
            mockExpression.Setup(m => m.ResultType).Returns(mockTypeUsage.Object);
            mockExpression.Setup(m => m.ExpressionKind).Returns(DbExpressionKind.Function);
            mockExpression.Setup(m => m.Function).Returns(CreateMockEdmFunction(functionName, builtInAttribute).Object);
            mockExpression.Setup(m => m.Arguments).Returns(arguments);

            return mockExpression;
        }

        private static Mock<DbExpression> CreateMockDbArgumentExpression(
            PrimitiveTypeKind typeKind,
            DbExpressionKind expressionKind,
            bool? isUnicode)
        {
            var mockEdmType = new Mock<PrimitiveType>();
            mockEdmType.Setup(m => m.BuiltInTypeKind).Returns(BuiltInTypeKind.PrimitiveType);
            mockEdmType.Setup(m => m.PrimitiveTypeKind).Returns(typeKind);

            var mockTypeUsage = new Mock<TypeUsage>();
            mockTypeUsage.Setup(m => m.EdmType).Returns(mockEdmType.Object);

            if (isUnicode.HasValue)
            {
                var mockFacet = new Mock<Facet>();
                mockFacet.Setup(m => m.Value).Returns(isUnicode.Value);
                mockFacet.Setup(m => m.Identity).Returns(DbProviderManifest.UnicodeFacetName);

                mockTypeUsage.Setup(m => m.Facets).Returns(
                    new ReadOnlyMetadataCollection<Facet>(
                        new MetadataCollection<Facet>
                            {
                                mockFacet.Object
                            }));
            }
            else
            {
                mockTypeUsage.Setup(m => m.Facets).Returns(new ReadOnlyMetadataCollection<Facet>(new MetadataCollection<Facet>()));
            }

            var mockExpression = new Mock<DbExpression>();
            mockExpression.Setup(m => m.ResultType).Returns(mockTypeUsage.Object);
            mockExpression.Setup(m => m.ExpressionKind).Returns(expressionKind);

            return mockExpression;
        }

        private static Mock<EdmFunction> CreateMockEdmFunction(string functionName, bool builtInAttribute)
        {
            var mockProperty = new Mock<MetadataProperty>();
            mockProperty.Setup(m => m.Name).Returns("DataSpace");
            mockProperty.Setup(m => m.Identity).Returns("DataSpace");
            mockProperty.Setup(m => m.Value).Returns(DataSpace.CSpace);

            var mockEdmFunction = new Mock<EdmFunction>("F", "N", DataSpace.SSpace);
            mockEdmFunction.Setup(m => m.FullName).Returns(functionName);
            mockEdmFunction.Setup(m => m.NamespaceName).Returns(functionName.Split('.')[0]);
            mockEdmFunction.Setup(m => m.Name).Returns(functionName.Split('.')[1]);
            mockEdmFunction.Setup(m => m.DataSpace).Returns(DataSpace.CSpace);
            mockEdmFunction.Setup(m => m.BuiltInAttribute).Returns(builtInAttribute);
            mockEdmFunction.Setup(m => m.MetadataProperties).Returns(
                new ReadOnlyMetadataCollection<MetadataProperty>(new[] { mockProperty.Object }));

            return mockEdmFunction;
        }

        [Fact]
        public void Visit_In_expression_with_empty_list_and_Visit_Constant_expression_false_generate_same_sql()
        {            
            var generator = new SqlGenerator();
            var inExpression = DbExpressionBuilder.In(
                DbExpressionBuilder.Constant(5), new List<DbConstantExpression>());
            var builder1 = new StringBuilder();
            var builder2 = new StringBuilder();

            using (var writer = new SqlWriter(builder1))
            {
                generator.Visit(DbExpressionBuilder.False).WriteSql(writer, null);
            }

            using (var writer = new SqlWriter(builder2))
            {
                generator.Visit(inExpression).WriteSql(writer, null);
            }

            Assert.Equal(builder1.ToString(), builder2.ToString());
        }
    }
}
