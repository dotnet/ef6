// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace CmdLine.Tests
{
    extern alias migrate;
    using System;
    using Xunit;

    public class CommandLineParameterAttributeTests
    {
        [Fact]
        public void SettingNameAndNameResourceIdThrows()
        {
            var attribute = new migrate::CmdLine.CommandLineParameterAttribute
                                {
                                    Name = "foo"
                                };

            Assert.Equal(
                migrate::System.Data.Entity.Migrations.Console.Resources.Strings.AmbiguousAttributeValues("Name", "NameResourceId"),
                Assert.Throws<InvalidOperationException>(() => attribute.NameResourceId = "bar").Message);
        }

        [Fact]
        public void SettingNameResourceIdAndNameThrows()
        {
            var attribute = new migrate::CmdLine.CommandLineParameterAttribute
                                {
                                    NameResourceId = "foo"
                                };

            Assert.Equal(
                migrate::System.Data.Entity.Migrations.Console.Resources.Strings.AmbiguousAttributeValues("Name", "NameResourceId"),
                Assert.Throws<InvalidOperationException>(() => attribute.Name = "bar").Message);
        }

        [Fact]
        public void SettingDescriptionAndDescriptionResourceIdThrows()
        {
            var attribute = new migrate::CmdLine.CommandLineParameterAttribute
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
            var attribute = new migrate::CmdLine.CommandLineParameterAttribute
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
