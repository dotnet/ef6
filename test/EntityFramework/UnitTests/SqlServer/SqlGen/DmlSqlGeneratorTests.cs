// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.SqlGen
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.SqlServer.Resources;
    using System.Text;
    using Moq;
    using Xunit;

    public class DmlSqlGeneratorTests
    {
        [Fact]
        public void GetParameterName_returns_correct_parameter_for_given_index()
        {
            Assert.Equal("@0", DmlSqlGenerator.ExpressionTranslator.GetParameterName(0));
            Assert.Equal("@0", DmlSqlGenerator.ExpressionTranslator.GetParameterName(0));
            Assert.Equal("@2000", DmlSqlGenerator.ExpressionTranslator.GetParameterName(2000));
            Assert.Equal("@2000", DmlSqlGenerator.ExpressionTranslator.GetParameterName(2000));
        }

        [Fact]
        public void GenerateMemberTSql_returns_the_quoted_member_name()
        {
            Assert.Equal("[Magic]", DmlSqlGenerator.GenerateMemberTSql(CreateMockMember("Magic").Object));
            Assert.Equal("[Magic]]Unicorn]", DmlSqlGenerator.GenerateMemberTSql(CreateMockMember("Magic]Unicorn").Object));
            Assert.Equal("[[Magic]]]", DmlSqlGenerator.GenerateMemberTSql(CreateMockMember("[Magic]").Object));
            Assert.Equal("[Magic[Unicorn]", DmlSqlGenerator.GenerateMemberTSql(CreateMockMember("Magic[Unicorn").Object));
        }

        private static Mock<EdmMember> CreateMockMember(string name)
        {
            var mockMember = new Mock<EdmMember>();
            mockMember.Setup(m => m.Name).Returns(name);
            mockMember.Setup(m => m.DeclaringType).Returns(new Mock<EntityType>("E", "N", DataSpace.CSpace).Object);

            return mockMember;
        }

        [Fact]
        public void Visit_DbScanExpression_throws_if_a_defining_query_is_set()
        {
            DbScanExpressionThrowsTest("UpdateFunction", new Mock<DbUpdateCommandTree>().Object);
            DbScanExpressionThrowsTest("DeleteFunction", new Mock<DbDeleteCommandTree>().Object);
            DbScanExpressionThrowsTest("InsertFunction", new Mock<DbInsertCommandTree>().Object);
        }

        private void DbScanExpressionThrowsTest(string functionName, DbModificationCommandTree commandTree)
        {
            Assert.Equal(
                Strings.Update_SqlEntitySetWithoutDmlFunctions("Binky", functionName, "ModificationFunctionMapping"),
                Assert.Throws<UpdateException>(
                    () => new DmlSqlGenerator.ExpressionTranslator(new StringBuilder(), commandTree, true, new SqlGenerator(SqlVersion.Sql10))
                              .Visit(CreateMockScanExpression("I am defined.").Object)).Message);
        }

        [Fact]
        public void Visit_DbScanExpression_appends_SQL_if_defining_query_is_not_set()
        {
            var builder = new StringBuilder();
            new DmlSqlGenerator.ExpressionTranslator(builder, new Mock<DbInsertCommandTree>().Object, true, new SqlGenerator(SqlVersion.Sql10))
                .Visit(CreateMockScanExpression(null).Object);

            Assert.Equal("[Kontainer].[Binky]", builder.ToString());
        }

        private static Mock<DbScanExpression> CreateMockScanExpression(string definingQuery)
        {
            var mockProperty = new Mock<MetadataProperty>();
            mockProperty.Setup(m => m.Name).Returns("DefiningQuery");
            mockProperty.Setup(m => m.Identity).Returns("DefiningQuery");
            mockProperty.Setup(m => m.Value).Returns(definingQuery);

            var mockContainer = new Mock<EntityContainer>("C", DataSpace.CSpace);
            mockContainer.Setup(m => m.Name).Returns("Kontainer");

            var mockSet = new Mock<EntitySetBase>();
            mockSet.Setup(m => m.MetadataProperties).Returns(
                new ReadOnlyMetadataCollection<MetadataProperty>(new[] { mockProperty.Object }));
            mockSet.Setup(m => m.Name).Returns("Binky");
            mockSet.Setup(m => m.EntityContainer).Returns(mockContainer.Object);

            var mockExpression = new Mock<DbScanExpression>();
            mockExpression.Setup(m => m.Target).Returns(mockSet.Object);
            return mockExpression;
        }
    }
}
