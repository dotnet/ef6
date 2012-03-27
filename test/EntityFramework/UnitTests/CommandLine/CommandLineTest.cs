namespace CmdLine.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Xunit;

    public class CommandLineTest
    {
        [Fact]
        public void CommandLineTokenizesQuestionMarkSwitches()
        {
            var args = new[] { "/?" };

            CommandLine.CommandEnvironment = new TestCommandEnvironment(args);

            var tokens = CommandLine.Tokenize();

            Assert.Equal(1, CommandLine.Args.Length);
            Assert.Equal(1, CommandLine.GetSwitches(tokens).Count);
            Assert.Equal(0, CommandLine.GetParameters(tokens).Count);
            Assert.True(tokens[0].IsCommand());
            Assert.Equal("?", tokens[0].Command);
        }

        [Fact]
        public void CommandLineTokenizesSwitchesAndParameters()
        {
            var args = new[]
                {
                    "C:\\Foo And Bar\\Some Long File.txt", "/1:Test", "-2:Some Quoted Arg", "/3=Arg with in it", "/Y-", "And another",
                    "Another", "One", "Word", "Args"
                };

            CommandLine.CommandEnvironment = new TestCommandEnvironment(args);

            var tokens = CommandLine.Tokenize();

            Assert.Equal(10, CommandLine.Args.Length);
            Assert.Equal(4, CommandLine.GetSwitches(tokens).Count);
            Assert.Equal(6, CommandLine.GetParameters(tokens).Count);
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
            CommandLine.CommandEnvironment = new TestCommandEnvironment();
            CommandLine.Parse<object>();
        }

        [Fact]
        public void MissingRequiredSwitchArgShouldThrow()
        {
            CommandLine.CommandEnvironment = new TestCommandEnvironment();
            Assert.Equal(new CommandLineRequiredArgumentMissingException(typeof(string), "N", -1).Message, Assert.Throws<CommandLineRequiredArgumentMissingException>(() => CommandLine.Parse<TestArgs>()).Message);
        }

        [Fact]
        public void AttributeWithNoCommandNameShouldUsePropertyName()
        {
            CommandLine.CommandEnvironment = new TestCommandEnvironment("/b1-");
            var actual = CommandLine.Parse<PropWithNoCommandName>();
            Assert.False(actual.b1);
        }

        [Fact]
        public void MissingRequiredPositionArgShouldThrow()
        {
            CommandLine.CommandEnvironment = new TestCommandEnvironment();
            Assert.Equal(new CommandLineRequiredArgumentMissingException(typeof(string), "String 1", 1).Message, Assert.Throws<CommandLineRequiredArgumentMissingException>(() => CommandLine.Parse<ThreeRequiredPositionArgs>()).Message);
        }

        [Fact]
        public void DuplicateArgsShouldThrow()
        {
            CommandLine.CommandEnvironment = new TestCommandEnvironment("/N:123 /N:345");

            var commandArg = new CommandArgument("/N:345", 7);
            commandArg.Command = "N";
            Assert.Equal(new CommandLineArgumentInvalidException(typeof(string), commandArg).Message, Assert.Throws<CommandLineArgumentInvalidException>(() => CommandLine.Parse<TestArgs>()).Message);
        }

        [Fact]
        public void DuplicateArgsWithListShouldNotThrow()
        {
            CommandLine.CommandEnvironment = new TestCommandEnvironment("/N:123 /N:345");
            var actual = CommandLine.Parse<TestArgsWithList>();
            Assert.Equal(2, actual.NList.Count);
            Assert.Equal("123", actual.NList[0]);
            Assert.Equal("345", actual.NList[1]);
        }

        [Fact]
        public void DefaultArgsAreApplied()
        {
            CommandLine.CommandEnvironment = new TestCommandEnvironment("/n:123");
            var target = CommandLine.Parse<TestArgs>();

            Assert.True(target.BoolT);
            Assert.False(target.BoolY);
            Assert.Equal(TestArgs.StringArgDefault, target.StringArg);
        }

        /// <summary>
        ///     Verifies that you can use alternate seperators
        /// </summary>
        [Fact]
        public void CommandLineTokenizesWithAlternateSwitchChars()
        {
            var args = new[]
                {
                    "C:\\Foo And Bar\\Some Long File.txt", "~1|Test", "~2|Some Quoted Arg", "~3|Arg with in it", "_Y-", "And another",
                    "Another", "One", "Word", "Args"
                };

            CommandLine.CommandEnvironment = new TestCommandEnvironment(args);

            // Save the seperators since they are static members
            var oldSep = CommandLine.CommandSeparators;
            var oldValueSep = CommandLine.ValueSeparators;

            CommandLine.CommandSeparators = new List<string> { "~", "_" };
            CommandLine.ValueSeparators = new List<string> { "|" };

            var tokens = CommandLine.Tokenize();

            // Restore the seperators
            CommandLine.CommandSeparators = oldSep;
            CommandLine.ValueSeparators = oldValueSep;

            Assert.Equal(10, CommandLine.Args.Length);
            Assert.Equal(4, CommandLine.GetSwitches(tokens).Count);
            Assert.Equal(6, CommandLine.GetParameters(tokens).Count);
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
                    @"D:\Documents and Settings\MY.USERNAME\My Documents\*", @"E:\MYBACKUP\My Documents\", "/A", @"/EXCLUDE:SomeQuoted String"
                    , "/I", "/D:7-8-2011"
                };

            CommandLine.CommandEnvironment = new TestCommandEnvironment(args);
            var xcopyCommand = CommandLine.Parse<XCopyCommandArgs>();

            Assert.NotNull(xcopyCommand);
            Assert.Equal(args[0], xcopyCommand.Source);
            Assert.Equal(args[1], xcopyCommand.Destination);
            Assert.Equal("SomeQuoted String", xcopyCommand.ExcludeFiles);
            Assert.True(xcopyCommand.ArchivedBit);
            Assert.True(xcopyCommand.InferDirectory);
            Assert.Equal(new DateTime(2011, 7, 8), xcopyCommand.ChangedAfterDate);
        }

        [Fact]
        public void EmbeddedSeparatorsDoNotCountAsSwitch()
        {
            var args = new[] { "/S:Value/With:Separators", "/Y-", "/t", "/N:123" };

            CommandLine.CommandEnvironment = new TestCommandEnvironment(args);

            var actual = CommandLine.Parse<TestArgs>();

            Assert.Equal("Value/With:Separators", actual.StringArg);
            Assert.True(actual.BoolT);
            Assert.False(actual.BoolY);
        }

        [Fact]
        public void CaseSensitiveAllowsUpperAndLowerWithSameSwitch()
        {
            CommandLine.CommandEnvironment = new TestCommandEnvironment("/t:lower /T:UPPER /y /Y-");
            var oldValue = CommandLine.CaseSensitive;
            CommandLine.CaseSensitive = true;

            var actual = CommandLine.Parse<TypeWithUpperAndLower>();

            CommandLine.CaseSensitive = oldValue;

            Assert.Equal("lower", actual.Lower);
            Assert.Equal("UPPER", actual.Upper);
            Assert.True(actual.YLower);
            Assert.False(actual.YUpper);
        }

        [Fact]
        public void NoMatchingPropertyShouldThrow()
        {
            var args = new[] { "/?" };

            CommandLine.CommandEnvironment = new TestCommandEnvironment(args);

            try
            {
                CommandLine.Parse<TestArgs>();
                Assert.True(false, "Parse did not throw an exception");
            }
            catch (CommandLineArgumentInvalidException exception)
            {
                Assert.NotNull(exception.ArgumentHelp);
                Assert.Equal(4, exception.ArgumentHelp.ValidArguments.Count());
            }
        }

        [Fact]
        public void NoMatchingPropertyWithInferredShouldThrow()
        {
            var args = new[] { "/NoMatch" };

            CommandLine.CommandEnvironment = new TestCommandEnvironment(args);

            var commandArg = new CommandArgument("/NoMatch", 0);
            commandArg.Command = "NoMatch";
            Assert.Equal(new CommandLineArgumentInvalidException(typeof(string), commandArg).Message, Assert.Throws<CommandLineArgumentInvalidException>(() => CommandLine.Parse<InferredTestArgs>()).Message);
        }

        [Fact]
        public void TwoPropsWithSameSwitchShouldThrow()
        {
            CommandLine.CommandEnvironment = new TestCommandEnvironment();
            Assert.Equal(new CommandLineException("Duplicate Command \"B\"").Message, Assert.Throws<CommandLineException>(() => CommandLine.Parse<TwoPropsWithSameSwitch>()).Message);
        }

        [Fact]
        public void WhenNoAttributesParseWillUsePropertyNames()
        {
            var args = new[] { "/StringArg:Value/With:Separators", "/BoolY-", "/BoolT", "/Date:12-01-2011", "/Number:23" };

            CommandLine.CommandEnvironment = new TestCommandEnvironment(args);

            var actual = CommandLine.Parse<InferredTestArgs>();

            Assert.Equal("Value/With:Separators", actual.StringArg);
            Assert.True(actual.BoolT);
            Assert.False(actual.BoolY);
            Assert.Equal(new DateTime(2011, 12, 1), actual.Date);
            Assert.Equal(23, actual.Number);
        }

        [Fact]
        public void WhenNoPositionOneShouldThrow()
        {
            CommandLine.CommandEnvironment = new TestCommandEnvironment();
            Assert.Equal(new CommandLineException("Out of order parameter \"source\" should have be at parameter index 1 but was found at 2").Message, Assert.Throws<CommandLineException>(() => CommandLine.Parse<BadPositionArgNoOne>()).Message);
        }

        [Fact]
        public void WhenNoPositionTwoShouldThrow()
        {
            CommandLine.CommandEnvironment = new TestCommandEnvironment();
            Assert.Equal(new CommandLineException("Out of order parameter \"destination\" should have be at parameter index 2 but was found at 3").Message, Assert.Throws<CommandLineException>(() => CommandLine.Parse<BadPositionArgMissingTwo>()).Message);
        }

        [Fact]
        public void WhenDuplicatePositionShouldThrow()
        {
            CommandLine.CommandEnvironment = new TestCommandEnvironment();
            Assert.Equal(new CommandLineException("Duplicate Parameter Index [1] on Property \"S2\"").Message, Assert.Throws<CommandLineException>(() => CommandLine.Parse<TypeWithDuplicateParamIndex>()).Message);
        }

        [Fact]
        public void WhenBadParameterIndexShouldThrow()
        {
            CommandLine.CommandEnvironment = new TestCommandEnvironment();
            Assert.Equal(new CustomAttributeFormatException("'ParameterIndex' property specified was not found.").Message, Assert.Throws<CustomAttributeFormatException>(() => CommandLine.Parse<TypeWithBadParamIndex>()).Message);
        }
    }

    public class TestArgsWithList
    {
        [CommandLineParameter(Command = "N")]
        public List<string> NList { get; set; }
    }

    public class TypeWithUpperAndLower
    {
        [CommandLineParameter("t")]
        public string Lower { get; set; }

        [CommandLineParameter("T")]
        public string Upper { get; set; }

        [CommandLineParameter("y")]
        public bool YLower { get; set; }

        [CommandLineParameter("Y")]
        public bool YUpper { get; set; }
    }

    public class TypeWithBadParamIndex
    {
        [CommandLineParameter(ParameterIndex = -1)]
        public string S1 { get; set; }
    }

    public class TypeWithDuplicateParamIndex
    {
        [CommandLineParameter(ParameterIndex = 1)]
        public string S1 { get; set; }

        [CommandLineParameter(ParameterIndex = 1)]
        public string S2 { get; set; }
    }
}