namespace System.Data.Entity.SqlServer.SqlGen
{
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
    }
}