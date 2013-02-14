
namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class EdmSchemaErrorTests
    {
        [Fact]
        public void EdmSchemaError_stores_passed_arguments()
        {
            var schemaError = new EdmSchemaError("message", 100, EdmSchemaErrorSeverity.Error);

            Assert.Equal("message", schemaError.Message);
            Assert.Equal(100, schemaError.ErrorCode);
            Assert.Equal(EdmSchemaErrorSeverity.Error, schemaError.Severity);
        }
    }
}
