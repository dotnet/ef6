namespace System.Data.Entity.ModelConfiguration.Edm.UnitTests
{
    using System.Data.Entity.Edm;
    using Xunit;

    public sealed class EdmEnumTypeExtensionsTests
    {
        [Fact]
        public void AddMember_should_create_and_add_to_members_list()
        {
            var enumType = new EdmEnumType();

            var member = enumType.AddMember("Foo", 12);

            Assert.NotNull(member);
            Assert.Equal("Foo", member.Name);
            Assert.True(enumType.Members.Contains(member));
        }

        [Fact]
        public void Should_be_able_to_get_and_set_clr_type()
        {
            var enumType = new EdmEnumType();

            Assert.Null(enumType.GetClrType());

            enumType.SetClrType(typeof(object));

            Assert.Equal(typeof(object), enumType.GetClrType());
        }
    }
}