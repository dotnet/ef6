// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact.SqlGen
{
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.SqlServerCompact.SqlGen;
    using System.Text;
    using System.Text.RegularExpressions;
    using Xunit;

    public class SqlGeneratorTests
    {
        [Fact]
        public static void VisitDbIsNullExpression_variable_size_string_parameter_without_max_length_is_cast_to_ntext()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String);
            var typeUsage = TypeUsage.CreateStringTypeUsage(primitiveType, isUnicode: true, isFixedLength: false);
            var isNullExpression = typeUsage.Parameter("parameterName").IsNull();

            var sqlGenerator = new SqlGenerator();
            var sqlFragment = sqlGenerator.Visit(isNullExpression);

            var builder = new StringBuilder();
            using (var writer = new SqlWriter(builder))
            {
                sqlFragment.WriteSql(writer, sqlGenerator);
            }

            Assert.Equal("cast(@parameterName as ntext) IS NULL", builder.ToString());
        }

        [Fact]
        public static void VisitDbIsNullExpression_variable_size_binary_parameter_without_max_length_is_cast_to_image()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary);
            var typeUsage = TypeUsage.CreateBinaryTypeUsage(primitiveType, isFixedLength: false);
            var isNullExpression = typeUsage.Parameter("parameterName").IsNull();

            var sqlGenerator = new SqlGenerator();
            var sqlFragment = sqlGenerator.Visit(isNullExpression);

            var builder = new StringBuilder();
            using (var writer = new SqlWriter(builder))
            {
                sqlFragment.WriteSql(writer, sqlGenerator);
            }

            Assert.Equal("cast(@parameterName as image) IS NULL", builder.ToString());
        }

        [Fact]
        public static void VisitDbIsNullExpression_non_parameter_reference_expression_is_not_cast()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String);
            var typeUsage = TypeUsage.CreateStringTypeUsage(primitiveType, isUnicode: true, isFixedLength: false);
            var isNullExpression = typeUsage.Constant("constant").IsNull();

            var sqlGenerator = new SqlGenerator();
            var sqlFragment = sqlGenerator.Visit(isNullExpression);

            var builder = new StringBuilder();
            using (var writer = new SqlWriter(builder))
            {
                sqlFragment.WriteSql(writer, sqlGenerator);
            }

            Assert.Equal("N'constant' IS NULL", builder.ToString());
        }

        [Fact]
        public static void VisitDbIsNullExpression_variable_size_string_parameter_with_max_length_is_not_cast()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String);
            var typeUsage = TypeUsage.CreateStringTypeUsage(primitiveType, isUnicode: true, isFixedLength: false, maxLength: 100);
            var isNullExpression = typeUsage.Parameter("parameterName").IsNull();

            var sqlGenerator = new SqlGenerator();
            var sqlFragment = sqlGenerator.Visit(isNullExpression);

            var builder = new StringBuilder();
            using (var writer = new SqlWriter(builder))
            {
                sqlFragment.WriteSql(writer, sqlGenerator);
            }

            Assert.Equal("@parameterName IS NULL", builder.ToString());
        }

        [Fact]
        public static void VisitDbIsNullExpression_variable_size_binary_parameter_with_max_length_is_not_cast()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary);
            var typeUsage = TypeUsage.CreateBinaryTypeUsage(primitiveType, isFixedLength: false, maxLength: 100);
            var isNullExpression = typeUsage.Parameter("parameterName").IsNull();

            var sqlGenerator = new SqlGenerator();
            var sqlFragment = sqlGenerator.Visit(isNullExpression);

            var builder = new StringBuilder();
            using (var writer = new SqlWriter(builder))
            {
                sqlFragment.WriteSql(writer, sqlGenerator);
            }

            Assert.Equal("@parameterName IS NULL", builder.ToString());
        }
    }
}
