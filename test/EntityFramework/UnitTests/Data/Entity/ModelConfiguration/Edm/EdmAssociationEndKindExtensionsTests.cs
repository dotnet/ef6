namespace System.Data.Entity.ModelConfiguration.Edm.UnitTests
{
    using System.Data.Entity.Edm;
    using Xunit;

    public sealed class EdmAssociationEndKindExtensionsTests
    {
        [Fact]
        public void IsX_should_return_true_when_end_kind_is_X()
        {
            Assert.True(EdmAssociationEndKind.Required.IsRequired());
            Assert.True(EdmAssociationEndKind.Optional.IsOptional());
            Assert.True(EdmAssociationEndKind.Many.IsMany());
        }
    }
}