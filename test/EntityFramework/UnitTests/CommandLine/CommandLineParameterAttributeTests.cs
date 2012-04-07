namespace CmdLine.Tests
{
    extern alias migrate;
    using System;
    using migrate::CmdLine;
    using migrate::System.Data.Entity.Migrations.Console.Resources;
    using Xunit;

    public class CommandLineParameterAttributeTests
    {
        [Fact]
        public void SettingNameAndNameResourceIdThrows()
        {
            var attribute = new CommandLineParameterAttribute()
            {
                Name = "foo"
            };

            Assert.Equal(Strings.AmbiguousAttributeValues("Name", "NameResourceId"), Assert.Throws<InvalidOperationException>(() => attribute.NameResourceId = "bar").Message);
        }

        [Fact]
        public void SettingNameResourceIdAndNameThrows()
        {
            var attribute = new CommandLineParameterAttribute()
            {
                NameResourceId = "foo"
            };

            Assert.Equal(Strings.AmbiguousAttributeValues("Name", "NameResourceId"), Assert.Throws<InvalidOperationException>(() => attribute.Name = "bar").Message);
        }


        [Fact]
        public void SettingDescriptionAndDescriptionResourceIdThrows()
        {
            var attribute = new CommandLineParameterAttribute()
            {
                Description = "foo"
            };

            Assert.Equal(Strings.AmbiguousAttributeValues("Description", "DescriptionResourceId"), Assert.Throws<InvalidOperationException>(() => attribute.DescriptionResourceId = "bar").Message);
        }

        [Fact]
        public void SettingDescriptionResourceIdAndDescriptionThrows()
        {
            var attribute = new CommandLineParameterAttribute()
            {
                DescriptionResourceId = "foo"
            };

            Assert.Equal(Strings.AmbiguousAttributeValues("Description", "DescriptionResourceId"), Assert.Throws<InvalidOperationException>(() => attribute.Description = "bar").Message);
        }
    }
}