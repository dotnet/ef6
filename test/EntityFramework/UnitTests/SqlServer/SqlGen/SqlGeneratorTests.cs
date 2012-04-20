namespace System.Data.Entity.SqlServer.SqlGen
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using Moq;
    using Xunit;

    public class SqlGeneratorTests
    {
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
            public void MatchTargetPatternForForcingNonUnicode_returns_false_for_matching_three_arg_canonical_function_with_non_matching_arg()
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
            var mockEdmFunction = new Mock<EdmFunction>();
            mockEdmFunction.Setup(m => m.FullName).Returns(functionName);
            mockEdmFunction.Setup(m => m.NamespaceName).Returns(functionName.Split('.')[0]);
            mockEdmFunction.Setup(m => m.Name).Returns(functionName.Split('.')[1]);
            mockEdmFunction.Setup(m => m.DataSpace).Returns(DataSpace.CSpace);
            mockEdmFunction.Setup(m => m.BuiltInAttribute).Returns(builtInAttribute);

            return mockEdmFunction;
        }
    }
}