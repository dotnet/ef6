// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace CmdLine
{
    extern alias migrate;
    using System;
    using System.Data.Entity;
    using Xunit;

    public class CommandLineExceptionTests
    {
        [Fact]
        public void Constructors_allow_for_nulls_message_and_inner_exception()
        {
            Assert.True(new migrate::CmdLine.CommandLineException((string)null).Message.Contains("'CmdLine.CommandLineException'"));
            Assert.Null(
                new migrate::CmdLine.CommandLineException(
                    new migrate::CmdLine.CommandArgumentHelp(typeof(SomeCommandLineClass)), null).InnerException);
        }

        [Fact]
        public void Constructors_throw_when_given_null_CommandArgumentHelp()
        {
            Assert.Equal(
                "argumentHelp",
                Assert.Throws<ArgumentNullException>(
                    () => new migrate::CmdLine.CommandLineException((migrate::CmdLine.CommandArgumentHelp)null)).ParamName);

            Assert.Equal(
                "argumentHelp",
                Assert.Throws<ArgumentNullException>(
                    () => new migrate::CmdLine.CommandLineException(null, new Exception())).ParamName);
        }

        [Fact]
        public void Constructor_uses_given_message_and_sets_up_serialization()
        {
            var exception = new migrate::CmdLine.CommandLineException("I'm a DOS prompt.");

            Assert.Equal("I'm a DOS prompt.", exception.Message);
            Assert.Null(exception.ArgumentHelp);

            exception = ExceptionHelpers.SerializeAndDeserialize(exception);

            Assert.Equal("I'm a DOS prompt.", exception.Message);
            Assert.Null(exception.ArgumentHelp);
        }

        [Fact]
        public void Constructor_uses_given_ArgumentHelp_and_sets_up_serialization()
        {
            var exception =
                new migrate::CmdLine.CommandLineException(
                    new migrate::CmdLine.CommandArgumentHelp(typeof(SomeCommandLineClass), "CLI"));

            Assert.Equal("CLI", exception.Message);
            Assert.Equal("Code First Migrations Command Line Utility", exception.ArgumentHelp.Title);

            exception = ExceptionHelpers.SerializeAndDeserialize(exception);

            Assert.Equal("CLI", exception.Message);
            Assert.Equal("Code First Migrations Command Line Utility", exception.ArgumentHelp.Title);
        }

        [Fact]
        public void Constructor_uses_given_ArgumentHelp_and_inner_exception_and_sets_up_serialization()
        {
            var innerException = new Exception("You are so exceptional!");
            var exception =
                new migrate::CmdLine.CommandLineException(
                    new migrate::CmdLine.CommandArgumentHelp(typeof(SomeCommandLineClass), "Look inside."), innerException);

            Assert.Equal("Look inside.", exception.Message);
            Assert.Same(innerException, exception.InnerException);
            Assert.Equal("Code First Migrations Command Line Utility", exception.ArgumentHelp.Title);

            exception = ExceptionHelpers.SerializeAndDeserialize(exception);

            Assert.Equal("Look inside.", exception.Message);
            Assert.Equal(innerException.Message, exception.InnerException.Message);
            Assert.Equal("Code First Migrations Command Line Utility", exception.ArgumentHelp.Title);
        }

        [Fact]
        public void ArgumentHelp_can_be_read_and_set()
        {
            var argHelp = new migrate::CmdLine.CommandArgumentHelp(typeof(SomeCommandLineClass));
            Assert.Same(
                argHelp, new migrate::CmdLine.CommandLineException("")
                {
                    ArgumentHelp = argHelp
                }.ArgumentHelp);
        }

        [migrate::CmdLine.CommandLineArgumentsAttribute(
            Program = "migrate",
            TitleResourceId = migrate::System.Data.Entity.Migrations.Console.Resources.EntityRes.MigrateTitle,
            DescriptionResourceId = migrate::System.Data.Entity.Migrations.Console.Resources.EntityRes.MigrateDescription)]
        public class SomeCommandLineClass
        {
        }
    }
}
