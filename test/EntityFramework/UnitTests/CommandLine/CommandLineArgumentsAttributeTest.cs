// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace CmdLine.Tests
{
    extern alias migrate;
    using System;
    using Xunit;

    public class CommandLineArgumentsAttributeTest
    {
        [Fact]
        public void GetShouldReturnCommandLineArgumentsAttribute()
        {
            var attribute = migrate::CmdLine.CommandLineArgumentsAttribute.Get(typeof(XCopyCommandArgs));
            Assert.NotNull(attribute);
            Assert.Equal(XCopyCommandArgs.Title, attribute.Title);
            Assert.Equal(XCopyCommandArgs.Description, attribute.Description);
        }

        [Fact]
        public void GetReturnsNullWhenNoAttribute()
        {
            var attribute = migrate::CmdLine.CommandLineArgumentsAttribute.Get(typeof(string));
            Assert.Null(attribute);
        }

        [Fact]
        public void GetThrowsArgumentNullWhenNull()
        {
            Assert.Equal(
                "element", Assert.Throws<ArgumentNullException>(() => migrate::CmdLine.CommandLineArgumentsAttribute.Get(null)).ParamName);
        }

        [Fact]
        public void SettingTitleAndTitleResourceIdThrows()
        {
            var attribute = new migrate::CmdLine.CommandLineArgumentsAttribute
                                {
                                    Title = "foo"
                                };

            Assert.Equal(
                migrate::System.Data.Entity.Migrations.Console.Resources.Strings.AmbiguousAttributeValues("Title", "TitleResourceId"),
                Assert.Throws<InvalidOperationException>(() => attribute.TitleResourceId = "bar").Message);
        }

        [Fact]
        public void SettingTitleResourceIdAndTitleThrows()
        {
            var attribute = new migrate::CmdLine.CommandLineArgumentsAttribute
                                {
                                    TitleResourceId = "foo"
                                };

            Assert.Equal(
                migrate::System.Data.Entity.Migrations.Console.Resources.Strings.AmbiguousAttributeValues("Title", "TitleResourceId"),
                Assert.Throws<InvalidOperationException>(() => attribute.Title = "bar").Message);
        }

        [Fact]
        public void SettingDescriptionAndDescriptionResourceIdThrows()
        {
            var attribute = new migrate::CmdLine.CommandLineArgumentsAttribute
                                {
                                    Description = "foo"
                                };

            Assert.Equal(
                migrate::System.Data.Entity.Migrations.Console.Resources.Strings.AmbiguousAttributeValues(
                    "Description", "DescriptionResourceId"),
                Assert.Throws<InvalidOperationException>(() => attribute.DescriptionResourceId = "bar").Message);
        }

        [Fact]
        public void SettingDescriptionResourceIdAndDescriptionThrows()
        {
            var attribute = new migrate::CmdLine.CommandLineArgumentsAttribute
                                {
                                    DescriptionResourceId = "foo"
                                };

            Assert.Equal(
                migrate::System.Data.Entity.Migrations.Console.Resources.Strings.AmbiguousAttributeValues(
                    "Description", "DescriptionResourceId"),
                Assert.Throws<InvalidOperationException>(() => attribute.Description = "bar").Message);
        }
    }
}
