// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections;
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;
    using Xunit;

    public class DbCommandLoggerTests
    {
        public class Constructors : TestBase
        {
            [Fact]
            public void Constructors_validate_arguments()
            {
                Assert.Equal(
                    "context",
                    Assert.Throws<ArgumentNullException>(() => new DbCommandLogger(null, new StringWriter())).ParamName);
                Assert.Equal(
                    "writer",
                    Assert.Throws<ArgumentNullException>(() => new DbCommandLogger(new Mock<DbContext>().Object, null)).ParamName);
                Assert.Equal(
                    "writer",
                    Assert.Throws<ArgumentNullException>(() => new DbCommandLogger(null)).ParamName);
            }
        }

        public class Context : TestBase
        {
            [Fact]
            public void Context_returns_configured_context()
            {
                var context = new Mock<DbContext>().Object;
                Assert.Same(context, new DbCommandLogger(context, new StringWriter()).Context);
            }
        }

        public class Writer : TestBase
        {
            [Fact]
            public void Writer_returns_configured_writer()
            {
                var writer = new StringWriter();
                Assert.Same(writer, new DbCommandLogger(new Mock<DbContext>().Object, writer).Writer);
            }
        }

        public class NonQueryExecuting : TestBase
        {
            [Fact]
            public void NonQueryExecuting_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter());

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.NonQueryExecuting(null, new DbCommandInterceptionContext())).ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.NonQueryExecuting(new Mock<DbCommand>().Object, null)).ParamName);
            }

            [Fact]
            public void NonQueryExecuting_logs()
            {
                var logger = new DbCommandLogger(new StringWriter());
                logger.NonQueryExecuting(CreateCommand("I am Sam"), new DbCommandInterceptionContext());

                Assert.Equal("I am Sam", GetSingleLine(logger.Writer));
            }
        }

        public class ReaderExecuting : TestBase
        {
            [Fact]
            public void ReaderExecuting_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter());

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.ReaderExecuting(null, new DbCommandInterceptionContext())).ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.ReaderExecuting(new Mock<DbCommand>().Object, null)).ParamName);
            }

            [Fact]
            public void ReaderExecuting_logs()
            {
                var logger = new DbCommandLogger(new StringWriter());
                logger.ReaderExecuting(CreateCommand("I am Sam"), new DbCommandInterceptionContext());

                Assert.Equal("I am Sam", GetSingleLine(logger.Writer));
            }
        }

        public class ScalarExecuting : TestBase
        {
            [Fact]
            public void ScalarExecuting_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter());

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.ScalarExecuting(null, new DbCommandInterceptionContext())).ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.ScalarExecuting(new Mock<DbCommand>().Object, null)).ParamName);
            }

            [Fact]
            public void ScalarExecuting_logs()
            {
                var logger = new DbCommandLogger(new StringWriter());
                logger.ScalarExecuting(CreateCommand("Sam I am"), new DbCommandInterceptionContext());

                Assert.Equal("Sam I am", GetSingleLine(logger.Writer));
            }
        }

        public class NonQueryExecuted : TestBase
        {
            [Fact]
            public void NonQueryExecuted_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter());

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.NonQueryExecuted(null, 1, new DbCommandInterceptionContext())).ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.NonQueryExecuted(new Mock<DbCommand>().Object, 1, null)).ParamName);
            }

            [Fact]
            public void NonQueryExecuted_logs()
            {
                var logger = new DbCommandLogger(new StringWriter());
                logger.NonQueryExecuted(CreateCommand(""), 88, new DbCommandInterceptionContext());

                Assert.Equal(Strings.CommandLogComplete("88"), GetSingleLine(logger.Writer));
            }
        }

        public class ReaderExecuted : TestBase
        {
            [Fact]
            public void ReaderExecuted_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter());

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.ReaderExecuted(null, null, new DbCommandInterceptionContext())).ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.ReaderExecuted(new Mock<DbCommand>().Object, null, null)).ParamName);
            }

            [Fact]
            public void ReaderExecuted_logs()
            {
                var dataReader = new Mock<DbDataReader>().Object;
                var logger = new DbCommandLogger(new StringWriter());
                logger.ReaderExecuted(CreateCommand(""), dataReader, new DbCommandInterceptionContext());

                Assert.Equal(Strings.CommandLogComplete(dataReader.GetType().Name), GetSingleLine(logger.Writer));
            }
        }

        public class ScalarExecuted : TestBase
        {
            [Fact]
            public void ScalarExecuted_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter());

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.ScalarExecuted(null, null, new DbCommandInterceptionContext())).ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.ScalarExecuted(new Mock<DbCommand>().Object, null, null)).ParamName);
            }

            [Fact]
            public void ScalarExecuted_logs()
            {
                var logger = new DbCommandLogger(new StringWriter());
                logger.ScalarExecuted(CreateCommand(""), "That Sam-I-am", new DbCommandInterceptionContext());

                Assert.Equal(Strings.CommandLogComplete("That Sam-I-am"), GetSingleLine(logger.Writer));
            }
        }

        public class Executing : TestBase
        {
            [Fact]
            public void Executing_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter());

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.Executing(null, new DbCommandInterceptionContext())).ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.Executing(new Mock<DbCommand>().Object, null)).ParamName);
            }

            [Fact]
            public void Executing_filters_by_context_when_set()
            {
                var context1 = new Mock<DbContext>().Object;
                var context2 = new Mock<DbContext>().Object;

                var logger = new DbCommandLogger(context1, new StringWriter());
                logger.Executing(CreateCommand("That Sam-I-am!"), new DbCommandInterceptionContext().WithDbContext(context1));
                logger.Executing(CreateCommand("I do not like"), new DbCommandInterceptionContext().WithDbContext(context2));
                logger.Executing(CreateCommand("that Sam-I-am"), new DbCommandInterceptionContext());

                Assert.Equal("That Sam-I-am!", GetSingleLine(logger.Writer));
            }

            [Fact]
            public void Executing_logs_every_command_when_context_not_set()
            {
                var context1 = new Mock<DbContext>().Object;
                var context2 = new Mock<DbContext>().Object;

                var logger = new DbCommandLogger(new StringWriter());
                logger.Executing(CreateCommand("Do you like"), new DbCommandInterceptionContext());
                logger.Executing(CreateCommand("Green eggs and ham?"), new DbCommandInterceptionContext().WithDbContext(context1));
                logger.Executing(CreateCommand("I do not like them"), new DbCommandInterceptionContext().WithDbContext(context2));

                var lines = GetLines(logger.Writer);
                Assert.Equal("Do you like", lines[0]);
                Assert.Equal("Green eggs and ham?", lines[1]);
                Assert.Equal("I do not like them", lines[2]);
            }
        }

        public class Executed : TestBase
        {
            [Fact]
            public void Executed_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter());

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.Executed(null, null, new DbCommandInterceptionContext())).ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.Executed(new Mock<DbCommand>().Object, null, null)).ParamName);
            }

            [Fact]
            public void Executed_filters_by_context_when_set()
            {
                var context1 = new Mock<DbContext>().Object;
                var context2 = new Mock<DbContext>().Object;

                var logger = new DbCommandLogger(context1, new StringWriter());
                logger.Executed(CreateCommand(""), "Sam-I-am", new DbCommandInterceptionContext().WithDbContext(context1));
                logger.Executed(CreateCommand(""), "I do not like", new DbCommandInterceptionContext().WithDbContext(context2));
                logger.Executed(CreateCommand(""), "Green eggs and ham", new DbCommandInterceptionContext());

                Assert.Equal(Strings.CommandLogComplete("Sam-I-am"), GetSingleLine(logger.Writer));
            }

            [Fact]
            public void Executed_logs_every_command_when_context_not_set()
            {
                var context1 = new Mock<DbContext>().Object;
                var context2 = new Mock<DbContext>().Object;

                var logger = new DbCommandLogger(new StringWriter());
                logger.Executed(CreateCommand(""), "Would you like them", new DbCommandInterceptionContext());
                logger.Executed(CreateCommand(""), "Here or there?", new DbCommandInterceptionContext().WithDbContext(context1));
                logger.Executed(CreateCommand(""), "I would not like them", new DbCommandInterceptionContext().WithDbContext(context2));

                var lines = GetLines(logger.Writer);
                Assert.Equal(Strings.CommandLogComplete("Would you like them"), lines[0]);
                Assert.Equal(Strings.CommandLogComplete("Here or there?"), lines[2]);
                Assert.Equal(Strings.CommandLogComplete("I would not like them"), lines[4]);
            }
        }

        public class LogCommand : TestBase
        {
            [Fact]
            public void LogCommand_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter());

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.LogCommand(null, new DbCommandInterceptionContext())).ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.LogCommand(new Mock<DbCommand>().Object, null)).ParamName);
            }

            [Fact]
            public void LogCommand_logs_command_text_and_parameters()
            {
                var parameter1 = CreateParameter("Param1", ParameterDirection.Input, true, DbType.String, 4000, 0, 0, "value");
                var parameter2 = CreateParameter("Param2", ParameterDirection.InputOutput, false, DbType.Decimal, -1, 18, 2, 7.7m);

                var logger = new DbCommandLogger(new StringWriter());
                logger.LogCommand(CreateCommand("here or there", parameter1, parameter2), new DbCommandInterceptionContext());

                var lines = GetLines(logger.Writer);
                Assert.Equal("here or there", lines[0]);
                Assert.Equal("-- Param1: Input Nullable String (Size = 4000; Precision = 0; Scale = 0) [value]", lines[1]);
                Assert.Equal("-- Param2: InputOutput Decimal (Size = -1; Precision = 18; Scale = 2) [7.7]", lines[2]);
            }

            [Fact]
            public void LogCommand_adds_log_line_for_async_commands()
            {
                var logger = new DbCommandLogger(new StringWriter());
                logger.LogCommand(CreateCommand("I would not like them"), new DbCommandInterceptionContext().AsAsync());

                var lines = GetLines(logger.Writer);
                Assert.Equal("I would not like them", lines[0]);
                Assert.Equal(Strings.CommandLogAsync, lines[1]);
            }
        }

        public class LogParameter : TestBase
        {
            [Fact]
            public void LogParameter_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter());

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.LogParameter(
                        null, new DbCommandInterceptionContext(), new Mock<DbParameter>().Object)).ParamName);
                
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.LogParameter(
                        new Mock<DbCommand>().Object, null, new Mock<DbParameter>().Object)).ParamName);
                
                Assert.Equal(
                    "parameter",
                    Assert.Throws<ArgumentNullException>(() => logger.LogParameter(
                        new Mock<DbCommand>().Object, new DbCommandInterceptionContext(), null)).ParamName);
            }

            [Fact]
            public void LogParameter_handles_all_normal_properties()
            {
                var parameter = CreateParameter("Param1", ParameterDirection.InputOutput, false, DbType.Decimal, 4, 18, 2, 2013m);

                var logger = new DbCommandLogger(new StringWriter());
                logger.LogCommand(CreateCommand("", parameter), new DbCommandInterceptionContext());

                Assert.Equal("-- Param1: InputOutput Decimal (Size = 4; Precision = 18; Scale = 2) [2013]", GetLines(logger.Writer)[1]);
            }

            [Fact]
            public void LogParameter_handles_nullable_and_non_nullable_properties()
            {
                var parameter1 = CreateParameter("Param1", ParameterDirection.Input, true, DbType.String, 4000, 0, 0, "value");
                var parameter2 = CreateParameter("Param2", ParameterDirection.Input, false, DbType.String, 4000, 0, 0, "value");

                var logger = new DbCommandLogger(new StringWriter());
                logger.LogCommand(CreateCommand("", parameter1, parameter2), new DbCommandInterceptionContext());

                var lines = GetLines(logger.Writer);
                Assert.Equal("-- Param1: Input Nullable String (Size = 4000; Precision = 0; Scale = 0) [value]", lines[1]);
                Assert.Equal("-- Param2: Input String (Size = 4000; Precision = 0; Scale = 0) [value]", lines[2]);
            }


            [Fact]
            public void LogParameter_handles_different_kinds_of_null_and_non_null_values()
            {
                var parameter1 = CreateParameter("Param1", ParameterDirection.Input, true, DbType.String, 4000, 0, 0, null);
                var parameter2 = CreateParameter("Param2", ParameterDirection.Input, false, DbType.String, 4000, 0, 0, DBNull.Value);
                var parameter3 = CreateParameter("Param3", ParameterDirection.Input, false, DbType.String, 4000, 0, 0, "Not Null");

                var logger = new DbCommandLogger(new StringWriter());
                logger.LogCommand(CreateCommand("", parameter1, parameter2, parameter3), new DbCommandInterceptionContext());

                var lines = GetLines(logger.Writer);
                Assert.Equal("-- Param1: Input Nullable String (Size = 4000; Precision = 0; Scale = 0) [null]", lines[1]);
                Assert.Equal("-- Param2: Input String (Size = 4000; Precision = 0; Scale = 0) [null]", lines[2]);
                Assert.Equal("-- Param3: Input String (Size = 4000; Precision = 0; Scale = 0) [Not Null]", lines[3]);
            }
        }

        public class LogResult : TestBase
        {
            [Fact]
            public void LogResult_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter());

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.LogResult(null, null, new DbCommandInterceptionContext())).ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.LogResult(new Mock<DbCommand>().Object, null, null)).ParamName);
            }

            [Fact]
            public void LogResult_handles_completed_commands_with_DbDataReader_results()
            {
                var dataReader = new Mock<DbDataReader>().Object;
                var logger = new DbCommandLogger(new StringWriter());
                logger.LogResult(new Mock<DbCommand>().Object, dataReader, new DbCommandInterceptionContext());

                Assert.Equal(Strings.CommandLogComplete(dataReader.GetType().Name), GetSingleLine(logger.Writer));
            }
            [Fact]
            public void LogResult_handles_completed_commands_with_int_results()
            {
                var logger = new DbCommandLogger(new StringWriter());
                logger.LogResult(new Mock<DbCommand>().Object, 77, new DbCommandInterceptionContext());

                Assert.Equal(Strings.CommandLogComplete("77"), GetSingleLine(logger.Writer));
            }

            [Fact]
            public void LogResult_handles_completed_commands_with_some_object_results()
            {
                var logger = new DbCommandLogger(new StringWriter());
                logger.LogResult(new Mock<DbCommand>().Object, "Green Eggs and Ham", new DbCommandInterceptionContext());

                Assert.Equal(Strings.CommandLogComplete("Green Eggs and Ham"), GetSingleLine(logger.Writer));
            }

            [Fact]
            public void LogResult_handles_completed_commands_with_null_results()
            {
                var logger = new DbCommandLogger(new StringWriter());
                logger.LogResult(new Mock<DbCommand>().Object, null, new DbCommandInterceptionContext());

                Assert.Equal(Strings.CommandLogComplete("null"), GetSingleLine(logger.Writer));
            }

            [Fact]
            public void LogResult_handles_failed_commands()
            {
                var logger = new DbCommandLogger(new StringWriter());
                logger.LogResult(
                    new Mock<DbCommand>().Object, 
                    null, 
                    new DbCommandInterceptionContext().WithException(new Exception("I do not like them!")));

                Assert.Equal(Strings.CommandLogFailed("I do not like them!"), GetSingleLine(logger.Writer));
            }

            [Fact]
            public void LogResult_handles_canceled_commands()
            {
                var logger = new DbCommandLogger(new StringWriter());
                logger.LogResult(
                    new Mock<DbCommand>().Object,
                    null,
                    new DbCommandInterceptionContext().WithTaskStatus(TaskStatus.Canceled));

                Assert.Equal(Strings.CommandLogCanceled, GetSingleLine(logger.Writer));
            }
        }

        private static DbCommand CreateCommand(string commandText, params DbParameter[] parameters)
        {
            var mockParameters = new Mock<DbParameterCollection>();
            mockParameters.As<IEnumerable>().Setup(m => m.GetEnumerator()).Returns(parameters.GetEnumerator());

            var mockCommand = new Mock<DbCommand>();
            mockCommand.Setup(m => m.CommandText).Returns(commandText);
            mockCommand.Protected().Setup<DbParameterCollection>("DbParameterCollection").Returns(mockParameters.Object);

            return mockCommand.Object;
        }

        private static DbParameter CreateParameter(
            string name,
            ParameterDirection direction,
            bool isNullable,
            DbType type,
            int size,
            byte precision,
            byte scale,
            object value)
        {
            var parameter = new Mock<DbParameter>();
            parameter.Setup(m => m.ParameterName).Returns(name);
            parameter.Setup(m => m.Direction).Returns(direction);
            parameter.Setup(m => m.IsNullable).Returns(isNullable);
            parameter.Setup(m => m.DbType).Returns(type);
            parameter.Setup(m => m.Size).Returns(size);
            parameter.As<IDbDataParameter>().Setup(m => m.Precision).Returns(precision);
            parameter.As<IDbDataParameter>().Setup(m => m.Scale).Returns(scale);
            parameter.Setup(m => m.Value).Returns(value);

            return parameter.Object;
        }

        private static string GetSingleLine(TextWriter writer)
        {
            return GetLines(writer).First();
        }

        private static string[] GetLines(TextWriter writer)
        {
            return writer.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        }
    }
}
