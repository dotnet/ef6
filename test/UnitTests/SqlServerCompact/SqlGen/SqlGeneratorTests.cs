// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact.SqlGen
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
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
