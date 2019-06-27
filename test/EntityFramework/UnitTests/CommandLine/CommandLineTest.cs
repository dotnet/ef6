// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET452

namespace CmdLine.Tests
{
    extern alias migrate;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.TestHelpers;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class CommandLineTest
    {
        [Fact]
        public void CommandLineTokenizesQuestionMarkSwitches()
        {
            var args = new[] { "/?" };

            migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment(args);

            var tokens = migrate::CmdLine.CommandLine.Tokenize();

            Assert.Equal(1, migrate::CmdLine.CommandLine.Args.Length);
            Assert.Equal(1, migrate::CmdLine.CommandLine.GetSwitches(tokens).Count);
            Assert.Equal(0, migrate::CmdLine.CommandLine.GetParameters(tokens).Count);
            Assert.True(tokens[0].IsCommand());
            Assert.Equal("?", tokens[0].Command);
        }

        [Fact]
        public void CommandLineTokenizesSwitchesAndParameters()
        {
            var args = new[]
                           {
                               "C:\\Foo And Bar\\Some Long File.txt", "/1:Test", "-2:Some Quoted Arg", "/3=Arg with in it", "/Y-",
                               "And another",
                               "Another", "One", "Word", "Args"
                           };

            migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment(args);

            var tokens = migrate::CmdLine.CommandLine.Tokenize();

            Assert.Equal(10, migrate::CmdLine.CommandLine.Args.Length);
            Assert.Equal(4, migrate::CmdLine.CommandLine.GetSwitches(tokens).Count);
            Assert.Equal(6, migrate::CmdLine.CommandLine.GetParameters(tokens).Count);
            Assert.True(tokens[0].IsParameter());
            Assert.True(tokens[1].IsCommand());
            Assert.True(tokens[2].IsCommand());
            Assert.True(tokens[3].IsCommand());
            Assert.True(tokens[4].IsCommand());
            Assert.True(tokens[5].IsParameter());
            Assert.True(tokens[6].IsParameter());
            Assert.True(tokens[7].IsParameter());
            Assert.True(tokens[8].IsParameter());
            Assert.True(tokens[9].IsParameter());

            for (var i = 0; i < args.Length; i++)
            {
                Assert.Equal(args[i], tokens[i].Token);
            }
        }

        [Fact]
        public void ParseDoesNotThrowWhenNullCommandLine()
        {
            migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment();
            migrate::CmdLine.CommandLine.Parse<object>();
        }

        [Fact]
        public void MissingRequiredSwitchArgShouldThrow()
        {
            migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment();
            Assert.Equal(
                new migrate::CmdLine.CommandLineRequiredArgumentMissingException(typeof(string), "N", -1).Message,
                Assert.Throws<migrate::CmdLine.CommandLineRequiredArgumentMissingException>(
                    () => migrate::CmdLine.CommandLine.Parse<TestArgs>()).Message);
        }

        [Fact]
        public void AttributeWithNoCommandNameShouldUsePropertyName()
        {
            migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment("/b1-");
            var actual = migrate::CmdLine.CommandLine.Parse<PropWithNoCommandName>();
            Assert.False(actual.b1);
        }

        [Fact]
        public void MissingRequiredPositionArgShouldThrow()
        {
            migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment();
            Assert.Equal(
                new migrate::CmdLine.CommandLineRequiredArgumentMissingException(typeof(string), "String 1", 1).Message,
                Assert.Throws<migrate::CmdLine.CommandLineRequiredArgumentMissingException>(
                    () => migrate::CmdLine.CommandLine.Parse<ThreeRequiredPositionArgs>()).Message);
        }

        [Fact]
        public void DuplicateArgsShouldThrow()
        {
            migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment("/N:123 /N:345");

            var commandArg = new migrate::CmdLine.CommandArgument("/N:345", 7);
            commandArg.Command = "N";
            Assert.Equal(
                new migrate::CmdLine.CommandLineArgumentInvalidException(typeof(string), commandArg).Message,
                Assert.Throws<migrate::CmdLine.CommandLineArgumentInvalidException>(() => migrate::CmdLine.CommandLine.Parse<TestArgs>()).
                    Message);
        }

        [Fact]
        public void DuplicateArgsWithListShouldNotThrow()
        {
            migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment("/N:123 /N:345");
            var actual = migrate::CmdLine.CommandLine.Parse<TestArgsWithList>();
            Assert.Equal(2, actual.NList.Count);
            Assert.Equal("123", actual.NList[0]);
            Assert.Equal("345", actual.NList[1]);
        }

        [Fact]
        public void DefaultArgsAreApplied()
        {
            migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment("/n:123");
            var target = migrate::CmdLine.CommandLine.Parse<TestArgs>();

            Assert.True(target.BoolT);
            Assert.False(target.BoolY);
            Assert.Equal(TestArgs.StringArgDefault, target.StringArg);
        }

        /// <summary>
        /// Verifies that you can use alternate seperators
        /// </summary>
        [Fact]
        public void CommandLineTokenizesWithAlternateSwitchChars()
        {
            var args = new[]
                           {
                               "C:\\Foo And Bar\\Some Long File.txt", "~1|Test", "~2|Some Quoted Arg", "~3|Arg with in it", "_Y-",
                               "And another",
                               "Another", "One", "Word", "Args"
                           };

            migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment(args);

            // Save the seperators since they are static members
            var oldSep = migrate::CmdLine.CommandLine.CommandSeparators;
            var oldValueSep = migrate::CmdLine.CommandLine.ValueSeparators;

            migrate::CmdLine.CommandLine.CommandSeparators = new List<string>
                                                                 {
                                                                     "~",
                                                                     "_"
                                                                 };
            migrate::CmdLine.CommandLine.ValueSeparators = new List<string>
                                                               {
                                                                   "|"
                                                               };

            var tokens = migrate::CmdLine.CommandLine.Tokenize();

            // Restore the seperators
            migrate::CmdLine.CommandLine.CommandSeparators = oldSep;
            migrate::CmdLine.CommandLine.ValueSeparators = oldValueSep;

            Assert.Equal(10, migrate::CmdLine.CommandLine.Args.Length);
            Assert.Equal(4, migrate::CmdLine.CommandLine.GetSwitches(tokens).Count);
            Assert.Equal(6, migrate::CmdLine.CommandLine.GetParameters(tokens).Count);
            Assert.True(tokens[0].IsParameter());
            Assert.True(tokens[1].IsCommand());
            Assert.True(tokens[2].IsCommand());
            Assert.True(tokens[3].IsCommand());
            Assert.True(tokens[4].IsCommand());
            Assert.True(tokens[5].IsParameter());
            Assert.True(tokens[6].IsParameter());
            Assert.True(tokens[7].IsParameter());
            Assert.True(tokens[8].IsParameter());
            Assert.True(tokens[9].IsParameter());

            for (var i = 0; i < args.Length; i++)
            {
                Assert.Equal(args[i], tokens[i].Token);
            }
        }

        ///<summary>
        ///    Verifies positional args work
        ///</summary>
        [Fact]
        public void PositionalArgsAreApplied()
        {
            var args = new[]
                           {
                               @"D:\Documents and Settings\MY.USERNAME\My Documents\*", @"E:\MYBACKUP\My Documents\", "/A",
                               @"/EXCLUDE:SomeQuoted String"
                               , "/I", "/D:7-8-2011"
                           };

            migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment(args);
            var xcopyCommand = migrate::CmdLine.CommandLine.Parse<XCopyCommandArgs>();

            Assert.NotNull(xcopyCommand);
            Assert.Equal(args[0], xcopyCommand.Source);
            Assert.Equal(args[1], xcopyCommand.Destination);
            Assert.Equal("SomeQuoted String", xcopyCommand.ExcludeFiles);
            Assert.True(xcopyCommand.ArchivedBit);
            Assert.True(xcopyCommand.InferDirectory);
            Assert.Equal(DateTime.Parse("7-8-2011"), xcopyCommand.ChangedAfterDate);
        }

        [Fact]
        public void EmbeddedSeparatorsDoNotCountAsSwitch()
        {
            var args = new[] { "/S:Value/With:Separators", "/Y-", "/t", "/N:123" };

            migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment(args);

            var actual = migrate::CmdLine.CommandLine.Parse<TestArgs>();

            Assert.Equal("Value/With:Separators", actual.StringArg);
            Assert.True(actual.BoolT);
            Assert.False(actual.BoolY);
        }

        [Fact]
        public void CaseSensitiveAllowsUpperAndLowerWithSameSwitch()
        {
            migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment("/t:lower /T:UPPER /y /Y-");
            var oldValue = migrate::CmdLine.CommandLine.CaseSensitive;
            migrate::CmdLine.CommandLine.CaseSensitive = true;

            var actual = migrate::CmdLine.CommandLine.Parse<TypeWithUpperAndLower>();

            migrate::CmdLine.CommandLine.CaseSensitive = oldValue;

            Assert.Equal("lower", actual.Lower);
            Assert.Equal("UPPER", actual.Upper);
            Assert.True(actual.YLower);
            Assert.False(actual.YUpper);
        }

        [Fact]
        public void NoMatchingPropertyShouldThrow()
        {
            var args = new[] { "/?" };

            migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment(args);

            try
            {
                migrate::CmdLine.CommandLine.Parse<TestArgs>();
                Assert.True(false, "Parse did not throw an exception");
            }
            catch (migrate::CmdLine.CommandLineArgumentInvalidException exception)
            {
                Assert.NotNull(exception.ArgumentHelp);
                Assert.Equal(4, exception.ArgumentHelp.ValidArguments.Count());
            }
        }

        [Fact]
        public void NoMatchingPropertyWithInferredShouldThrow()
        {
            var args = new[] { "/NoMatch" };

            migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment(args);

            var commandArg = new migrate::CmdLine.CommandArgument("/NoMatch", 0);
            commandArg.Command = "NoMatch";
            Assert.Equal(
                new migrate::CmdLine.CommandLineArgumentInvalidException(typeof(string), commandArg).Message,
                Assert.Throws<migrate::CmdLine.CommandLineArgumentInvalidException>(
                    () => migrate::CmdLine.CommandLine.Parse<InferredTestArgs>()).Message);
        }

        [Fact]
        public void TwoPropsWithSameSwitchShouldThrow()
        {
            migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment();
            Assert.Equal(
                new migrate::CmdLine.CommandLineException("Duplicate Command \"B\"").Message,
                Assert.Throws<migrate::CmdLine.CommandLineException>(() => migrate::CmdLine.CommandLine.Parse<TwoPropsWithSameSwitch>()).
                    Message);
        }

        [Fact]
        public void WhenNoAttributesParseWillUsePropertyNames()
        {
            var args = new[] { "/StringArg:Value/With:Separators", "/BoolY-", "/BoolT", "/Date:12-1-2011", "/Number:23" };

            migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment(args);

            var actual = migrate::CmdLine.CommandLine.Parse<InferredTestArgs>();

            Assert.Equal("Value/With:Separators", actual.StringArg);
            Assert.True(actual.BoolT);
            Assert.False(actual.BoolY);
            Assert.Equal(DateTime.Parse("12-1-2011"), actual.Date);
            Assert.Equal(23, actual.Number);
        }

        [Fact]
        public void WhenNoPositionOneShouldThrow()
        {
            if (LocalizationTestHelpers.IsEnglishLocale())
            {
                migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment();
                Assert.Equal(
                    new migrate::CmdLine.CommandLineException(
                        "Out of order parameter \"source\" should have be at parameter index 1 but was found at 2").Message,
                    Assert.Throws<migrate::CmdLine.CommandLineException>(() => migrate::CmdLine.CommandLine.Parse<BadPositionArgNoOne>()).
                        Message);
            }
        }

        [Fact]
        public void WhenNoPositionTwoShouldThrow()
        {
            if (LocalizationTestHelpers.IsEnglishLocale())
            {
                migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment();
                Assert.Equal(
                    new migrate::CmdLine.CommandLineException(
                        "Out of order parameter \"destination\" should have be at parameter index 2 but was found at 3").
                        Message,
                    Assert.Throws<migrate::CmdLine.CommandLineException>(() => migrate::CmdLine.CommandLine.Parse<BadPositionArgMissingTwo>()).
                        Message);
            }
        }

        [Fact]
        public void WhenDuplicatePositionShouldThrow()
        {
            if (LocalizationTestHelpers.IsEnglishLocale())
            {
                migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment();
                Assert.Equal(
                    new migrate::CmdLine.CommandLineException("Duplicate Parameter Index [1] on Property \"S2\"").Message,
                    Assert.Throws<migrate::CmdLine.CommandLineException>(
                        () => migrate::CmdLine.CommandLine.Parse<TypeWithDuplicateParamIndex>()).Message);
            }
        }

        [Fact]
        public void WhenBadParameterIndexShouldThrow()
        {
            if (LocalizationTestHelpers.IsEnglishLocale())
            {
                migrate::CmdLine.CommandLine.CommandEnvironment = new TestCommandEnvironment();
                Assert.Equal(
                    new CustomAttributeFormatException("'ParameterIndex' property specified was not found.").Message,
                    Assert.Throws<CustomAttributeFormatException>(() => migrate::CmdLine.CommandLine.Parse<TypeWithBadParamIndex>()).Message);
            }
        }
    }

    public class TestArgsWithList
    {
        [migrate::CmdLine.CommandLineParameterAttribute(Command = "N")]
        public List<string> NList { get; set; }
    }

    public class TypeWithUpperAndLower
    {
        [migrate::CmdLine.CommandLineParameterAttribute("t")]
        public string Lower { get; set; }

        [migrate::CmdLine.CommandLineParameterAttribute("T")]
        public string Upper { get; set; }

        [migrate::CmdLine.CommandLineParameterAttribute("y")]
        public bool YLower { get; set; }

        [migrate::CmdLine.CommandLineParameterAttribute("Y")]
        public bool YUpper { get; set; }
    }

    public class TypeWithBadParamIndex
    {
        [migrate::CmdLine.CommandLineParameterAttribute(ParameterIndex = -1)]
        public string S1 { get; set; }
    }

    public class TypeWithDuplicateParamIndex
    {
        [migrate::CmdLine.CommandLineParameterAttribute(ParameterIndex = 1)]
        public string S1 { get; set; }

        [migrate::CmdLine.CommandLineParameterAttribute(ParameterIndex = 1)]
        public string S2 { get; set; }
    }
}

#endif
