// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections;
    using System.Data.Common;
    using System.Data.Entity.Resources;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
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
                    Assert.Throws<ArgumentNullException>(() => new DbCommandLogger(null, new StringWriter().Write)).ParamName);
                Assert.Equal(
                    "sink",
                    Assert.Throws<ArgumentNullException>(() => new DbCommandLogger(new Mock<DbContext>().Object, null)).ParamName);
                Assert.Equal(
                    "sink",
                    Assert.Throws<ArgumentNullException>(() => new DbCommandLogger(null)).ParamName);
            }
        }

        public class Context : TestBase
        {
            [Fact]
            public void Context_returns_configured_context()
            {
                var context = new Mock<DbContext>().Object;
                Assert.Same(context, new DbCommandLogger(context, new StringWriter().Write).Context);
            }
        }

        public class Writer : TestBase
        {
            [Fact]
            public void Writer_returns_configured_writer()
            {
                Action<string> writer = new StringWriter().Write;
                Assert.Same(writer, new DbCommandLogger(new Mock<DbContext>().Object, writer).Sink);
            }
        }

        public class NonQueryExecuting : TestBase
        {
            [Fact]
            public void NonQueryExecuting_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter().Write);

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.NonQueryExecuting(null, new DbCommandInterceptionContext<int>()))
                        .ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.NonQueryExecuting(new Mock<DbCommand>().Object, null)).ParamName);
            }

            [Fact]
            public void NonQueryExecuting_logs()
            {
                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).NonQueryExecuting(CreateCommand("I am Sam"), new DbCommandInterceptionContext<int>());

                Assert.Equal("I am Sam", GetSingleLine(writer));
            }
        }

        public class ReaderExecuting : TestBase
        {
            [Fact]
            public void ReaderExecuting_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter().Write);

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () => logger.ReaderExecuting(null, new DbCommandInterceptionContext<DbDataReader>())).ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.ReaderExecuting(new Mock<DbCommand>().Object, null)).ParamName);
            }

            [Fact]
            public void ReaderExecuting_logs()
            {
                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).ReaderExecuting(
                    CreateCommand("I am Sam"), new DbCommandInterceptionContext<DbDataReader>());

                Assert.Equal("I am Sam", GetSingleLine(writer));
            }
        }

        public class ScalarExecuting : TestBase
        {
            [Fact]
            public void ScalarExecuting_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter().Write);

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.ScalarExecuting(null, new DbCommandInterceptionContext<object>()))
                        .ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.ScalarExecuting(new Mock<DbCommand>().Object, null)).ParamName);
            }

            [Fact]
            public void ScalarExecuting_logs()
            {
                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).ScalarExecuting(CreateCommand("Sam I am"), new DbCommandInterceptionContext<object>());

                Assert.Equal("Sam I am", GetSingleLine(writer));
            }
        }

        public class NonQueryExecuted : TestBase
        {
            [Fact]
            public void NonQueryExecuted_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter().Write);

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.NonQueryExecuted(null, new DbCommandInterceptionContext<int>()))
                        .ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.NonQueryExecuted(new Mock<DbCommand>().Object, null)).ParamName);
            }

            [Fact]
            public void NonQueryExecuted_logs()
            {
                var interceptionContext = new DbCommandInterceptionContext<int>();
                interceptionContext.Result = 88;
                var writer = new StringWriter();

                new DbCommandLogger(writer.Write).NonQueryExecuted(CreateCommand(""), interceptionContext);

                Assert.Equal(Strings.CommandLogComplete(0, "88", ""), GetSingleLine(writer));
            }
        }

        public class ReaderExecuted : TestBase
        {
            [Fact]
            public void ReaderExecuted_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter().Write);

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () => logger.ReaderExecuted(null, new DbCommandInterceptionContext<DbDataReader>()))
                        .ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.ReaderExecuted(new Mock<DbCommand>().Object, null)).ParamName);
            }

            [Fact]
            public void ReaderExecuted_logs()
            {
                var dataReader = new Mock<DbDataReader>().Object;
                var interceptionContext = new DbCommandInterceptionContext<DbDataReader>();

                interceptionContext.Result = dataReader;
                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).ReaderExecuted(CreateCommand(""), interceptionContext);

                Assert.Equal(Strings.CommandLogComplete(0, dataReader.GetType().Name, ""), GetSingleLine(writer));
            }
        }

        public class ScalarExecuted : TestBase
        {
            [Fact]
            public void ScalarExecuted_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter().Write);

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.ScalarExecuted(null, new DbCommandInterceptionContext<object>()))
                        .ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.ScalarExecuted(new Mock<DbCommand>().Object, null)).ParamName);
            }

            [Fact]
            public void ScalarExecuted_logs()
            {
                var interceptionContext = new DbCommandInterceptionContext<object>();
                interceptionContext.Result = "That Sam-I-am";

                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).ScalarExecuted(CreateCommand(""), interceptionContext);

                Assert.Equal(Strings.CommandLogComplete(0, "That Sam-I-am", ""), GetSingleLine(writer));
            }
        }

        public class Executing : TestBase
        {
            [Fact]
            public void Executing_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter().Write);

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.Executing(null, new DbCommandInterceptionContext<int>())).ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.Executing(new Mock<DbCommand>().Object, null)).ParamName);
            }

            [Fact]
            public void Executing_filters_by_context_when_set()
            {
                var context1 = new Mock<DbContext>().Object;
                var context2 = new Mock<DbContext>().Object;

                var writer = new StringWriter();
                var logger = new DbCommandLogger(writer.Write);
                logger.Executing(CreateCommand("That Sam-I-am!"), new DbCommandInterceptionContext<int>().WithDbContext(context1));
                logger.Executing(CreateCommand("I do not like"), new DbCommandInterceptionContext<int>().WithDbContext(context2));
                logger.Executing(CreateCommand("that Sam-I-am"), new DbCommandInterceptionContext<int>());

                Assert.Equal("That Sam-I-am!", GetSingleLine(writer));
            }

            [Fact]
            public void Executing_logs_every_command_when_context_not_set()
            {
                var context1 = new Mock<DbContext>().Object;
                var context2 = new Mock<DbContext>().Object;

                var writer = new StringWriter();
                var logger = new DbCommandLogger(writer.Write);
                logger.Executing(CreateCommand("Do you like"), new DbCommandInterceptionContext<int>());
                logger.Executing(CreateCommand("Green eggs and ham?"), new DbCommandInterceptionContext<int>().WithDbContext(context1));
                logger.Executing(CreateCommand("I do not like them"), new DbCommandInterceptionContext<int>().WithDbContext(context2));

                var lines = GetLines(writer);
                Assert.Equal("Do you like", lines[0]);
                Assert.Equal("Green eggs and ham?", lines[2]);
                Assert.Equal("I do not like them", lines[4]);
            }
        }

        public class Executed : TestBase
        {
            [Fact]
            public void Executed_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter().Write);

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.Executed(null, null, new DbCommandInterceptionContext<int>()))
                        .ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.Executed(new Mock<DbCommand>().Object, null, null)).ParamName);
            }

            [Fact]
            public void Executed_filters_by_context_when_set()
            {
                var context1 = new Mock<DbContext>().Object;
                var context2 = new Mock<DbContext>().Object;

                var writer = new StringWriter();
                var logger = new DbCommandLogger(writer.Write);
                logger.Executed(CreateCommand(""), "Sam-I-am", new DbCommandInterceptionContext<int>().WithDbContext(context1));
                logger.Executed(CreateCommand(""), "I do not like", new DbCommandInterceptionContext<int>().WithDbContext(context2));
                logger.Executed(CreateCommand(""), "Green eggs and ham", new DbCommandInterceptionContext<int>());

                Assert.Equal(Strings.CommandLogComplete(0, "Sam-I-am", ""), GetSingleLine(writer));
            }

            [Fact]
            public void Executed_logs_every_command_when_context_not_set()
            {
                var context1 = new Mock<DbContext>().Object;
                var context2 = new Mock<DbContext>().Object;

                var writer = new StringWriter();
                var logger = new DbCommandLogger(writer.Write);
                logger.Executed(CreateCommand(""), "Would you like them", new DbCommandInterceptionContext<int>());
                logger.Executed(CreateCommand(""), "Here or there?", new DbCommandInterceptionContext<int>().WithDbContext(context1));
                logger.Executed(CreateCommand(""), "I would not like them", new DbCommandInterceptionContext<int>().WithDbContext(context2));

                var lines = GetLines(writer);
                Assert.Equal(Strings.CommandLogComplete(0, "Would you like them", ""), lines[0]);
                Assert.Equal(Strings.CommandLogComplete(0, "Here or there?", ""), lines[2]);
                Assert.Equal(Strings.CommandLogComplete(0, "I would not like them", ""), lines[4]);
            }
        }

        public class LogCommand : TestBase
        {
            [Fact]
            public void LogCommand_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter().Write);

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.LogCommand(null, new DbCommandInterceptionContext<int>())).ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.LogCommand(new Mock<DbCommand>().Object, null)).ParamName);
            }

            [Fact]
            public void LogCommand_can_handle_commands_with_null_text_and_parameters()
            {
                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).LogCommand(new Mock<DbCommand>().Object, new DbCommandInterceptionContext<int>());

                Assert.Equal("<null>", GetLines(writer)[0]);
            }

            [Fact]
            public void LogCommand_logs_command_text_and_parameters()
            {
                var parameter1 = CreateParameter("Param1", ParameterDirection.Input, true, DbType.String, 4000, 0, 0, "value");
                var parameter2 = CreateParameter("Param2", ParameterDirection.InputOutput, false, DbType.Decimal, -1, 18, 2, 7.7m);

                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).LogCommand(
                    CreateCommand("here or there", parameter1, parameter2), new DbCommandInterceptionContext<int>());

                var lines = GetLines(writer);
                Assert.Equal("here or there", lines[0]);
                Assert.Equal("-- Param1: 'value' (Type = String, Size = 4000)", lines[1]);

                var expected = string.Format(
                    CultureInfo.CurrentCulture,
                    "-- Param2: '{0}' (Type = Decimal, Direction = InputOutput, IsNullable = false, Size = -1, Precision = 18, Scale = 2)",
                    7.7m);
                Assert.Equal(expected, lines[2]);
            }

            [Fact]
            public void LogCommand_adds_timestamp_log_line_for_async_commands()
            {
                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).LogCommand(
                    CreateCommand("I would not like them"), new DbCommandInterceptionContext<int>().AsAsync());

                var lines = GetLines(writer);
                Assert.Equal("I would not like them", lines[0]);
                Assert.Contains(Strings.CommandLogAsync("", ""), lines[1]);

                var timestamp = DateTime.Parse(lines[1].Substring(Strings.CommandLogAsync("", "").Length));
                Assert.Equal(0d, Math.Abs((timestamp - DateTime.Now).TotalMinutes), 0);
            }

            [Fact]
            public void LogCommand_adds_timestamp_log_line_for_sync_commands()
            {
                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).LogCommand(
                    CreateCommand("I would not like them"), new DbCommandInterceptionContext<int>());

                var lines = GetLines(writer);
                Assert.Equal("I would not like them", lines[0]);
                Assert.Contains(Strings.CommandLogNonAsync("", ""), lines[1]);

                var timestamp = DateTime.Parse(lines[1].Substring(Strings.CommandLogNonAsync("", "").Length));
                Assert.Equal(0d, Math.Abs((timestamp - DateTime.Now).TotalMinutes), 0);
            }
        }

        public class LogParameter : TestBase
        {
            [Fact]
            public void LogParameter_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter().Write);

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(
                        () => logger.LogParameter(
                            null, new DbCommandInterceptionContext<int>(), new Mock<DbParameter>().Object)).ParamName);

                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(
                        () => logger.LogParameter(
                            new Mock<DbCommand>().Object, null, new Mock<DbParameter>().Object)).ParamName);

                Assert.Equal(
                    "parameter",
                    Assert.Throws<ArgumentNullException>(
                        () => logger.LogParameter(
                            new Mock<DbCommand>().Object, new DbCommandInterceptionContext<int>(), null)).ParamName);
            }

            [Fact]
            public void LogParameter_handles_all_normal_properties()
            {
                var parameter = CreateParameter("Param1", ParameterDirection.InputOutput, false, DbType.Decimal, 4, 18, 2, 2013m);

                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).LogCommand(CreateCommand("", parameter), new DbCommandInterceptionContext<int>());

                Assert.Equal(
                    "-- Param1: '2013' (Type = Decimal, Direction = InputOutput, IsNullable = false, Size = 4, Precision = 18, Scale = 2)",
                    GetLines(writer)[1]);
            }

            [Fact]
            public void LogParameter_handles_nullable_and_non_nullable_properties()
            {
                var parameter1 = CreateParameter("Param1", ParameterDirection.Input, true, DbType.String, 4000, 0, 0, "value");
                var parameter2 = CreateParameter("Param2", ParameterDirection.Input, false, DbType.String, 4000, 0, 0, "value");

                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).LogCommand(
                    CreateCommand("", parameter1, parameter2), new DbCommandInterceptionContext<int>());

                var lines = GetLines(writer);
                Assert.Equal("-- Param1: 'value' (Type = String, Size = 4000)", lines[1]);
                Assert.Equal("-- Param2: 'value' (Type = String, IsNullable = false, Size = 4000)", lines[2]);
            }

            [Fact]
            public void LogParameter_handles_different_kinds_of_null_and_non_null_values()
            {
                var parameter1 = CreateParameter("Param1", ParameterDirection.Input, true, DbType.String, 4000, 0, 0, null);
                var parameter2 = CreateParameter("Param2", ParameterDirection.Input, false, DbType.String, 4000, 0, 0, DBNull.Value);
                var parameter3 = CreateParameter("Param3", ParameterDirection.Input, false, DbType.String, 4000, 0, 0, "Not Null");

                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).LogCommand(
                    CreateCommand("", parameter1, parameter2, parameter3), new DbCommandInterceptionContext<int>());

                var lines = GetLines(writer);
                Assert.Equal("-- Param1: 'null' (Type = String, Size = 4000)", lines[1]);
                Assert.Equal("-- Param2: 'null' (Type = String, IsNullable = false, Size = 4000)", lines[2]);
                Assert.Equal("-- Param3: 'Not Null' (Type = String, IsNullable = false, Size = 4000)", lines[3]);
            }

            [Fact]
            public void LogParameter_includes_Scale_only_if_set()
            {
                var parameter1 = CreateParameter("Param1", ParameterDirection.Input, true, DbType.String, 0, 0, 2, "value");
                var parameter2 = CreateParameter("Param2", ParameterDirection.Input, true, DbType.String, 0, 0, 0, "value");

                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).LogCommand(
                    CreateCommand("", parameter1, parameter2), new DbCommandInterceptionContext<int>());

                var lines = GetLines(writer);
                Assert.Equal("-- Param1: 'value' (Type = String, Scale = 2)", lines[1]);
                Assert.Equal("-- Param2: 'value' (Type = String)", lines[2]);
            }

            [Fact]
            public void LogParameter_includes_Precision_only_if_set()
            {
                var parameter1 = CreateParameter("Param1", ParameterDirection.Input, true, DbType.String, 0, 18, 0, "value");
                var parameter2 = CreateParameter("Param2", ParameterDirection.Input, true, DbType.String, 0, 0, 0, "value");

                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).LogCommand(
                    CreateCommand("", parameter1, parameter2), new DbCommandInterceptionContext<int>());

                var lines = GetLines(writer);
                Assert.Equal("-- Param1: 'value' (Type = String, Precision = 18)", lines[1]);
                Assert.Equal("-- Param2: 'value' (Type = String)", lines[2]);
            }

            [Fact]
            public void LogParameter_includes_Size_only_if_set()
            {
                var parameter1 = CreateParameter("Param1", ParameterDirection.Input, true, DbType.String, -1, 0, 0, "value");
                var parameter2 = CreateParameter("Param2", ParameterDirection.Input, true, DbType.String, 0, 0, 0, "value");

                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).LogCommand(
                    CreateCommand("", parameter1, parameter2), new DbCommandInterceptionContext<int>());

                var lines = GetLines(writer);
                Assert.Equal("-- Param1: 'value' (Type = String, Size = -1)", lines[1]);
                Assert.Equal("-- Param2: 'value' (Type = String)", lines[2]);
            }

            [Fact]
            public void LogParameter_includes_Direction_only_if_set()
            {
                var parameter1 = CreateParameter("Param1", ParameterDirection.ReturnValue, true, DbType.String, 0, 0, 0, "value");
                var parameter2 = CreateParameter("Param2", ParameterDirection.Input, true, DbType.String, 0, 0, 0, "value");

                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).LogCommand(
                    CreateCommand("", parameter1, parameter2), new DbCommandInterceptionContext<int>());

                var lines = GetLines(writer);
                Assert.Equal("-- Param1: 'value' (Type = String, Direction = ReturnValue)", lines[1]);
                Assert.Equal("-- Param2: 'value' (Type = String)", lines[2]);
            }
        }

        public class LogResult : TestBase
        {
            [Fact]
            public void LogResult_validates_arguments()
            {
                var logger = new DbCommandLogger(new StringWriter().Write);

                Assert.Equal(
                    "command",
                    Assert.Throws<ArgumentNullException>(() => logger.LogResult(null, null, new DbCommandInterceptionContext<int>()))
                        .ParamName);
                Assert.Equal(
                    "interceptionContext",
                    Assert.Throws<ArgumentNullException>(() => logger.LogResult(new Mock<DbCommand>().Object, null, null)).ParamName);
            }

            [Fact]
            public void LogResult_handles_completed_commands_with_DbDataReader_results()
            {
                var dataReader = new Mock<DbDataReader>().Object;
                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).LogResult(
                    new Mock<DbCommand>().Object, dataReader, new DbCommandInterceptionContext<int>());

                Assert.Equal(Strings.CommandLogComplete(0, dataReader.GetType().Name, ""), GetSingleLine(writer));
            }

            [Fact]
            public void LogResult_handles_completed_commands_with_int_results()
            {
                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).LogResult(new Mock<DbCommand>().Object, 77, new DbCommandInterceptionContext<int>());

                Assert.Equal(Strings.CommandLogComplete(0, "77", ""), GetSingleLine(writer));
            }

            [Fact]
            public void LogResult_handles_completed_commands_with_some_object_results()
            {
                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).LogResult(
                    new Mock<DbCommand>().Object, "Green Eggs and Ham", new DbCommandInterceptionContext<int>());

                Assert.Equal(Strings.CommandLogComplete(0, "Green Eggs and Ham", ""), GetSingleLine(writer));
            }

            [Fact]
            public void LogResult_handles_completed_commands_with_null_results()
            {
                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).LogResult(new Mock<DbCommand>().Object, null, new DbCommandInterceptionContext<int>());

                Assert.Equal(Strings.CommandLogComplete(0, "null", ""), GetSingleLine(writer));
            }

            [Fact]
            public void LogResult_handles_failed_commands()
            {
                var writer = new StringWriter();
                new DbCommandLogger(writer.Write).LogResult(
                    new Mock<DbCommand>().Object,
                    null,
                    new DbCommandInterceptionContext<DbDataReader> { Exception = new Exception("I do not like them!") });

                Assert.Equal(Strings.CommandLogFailed(0, "I do not like them!", ""), GetSingleLine(writer));
            }

            [Fact]
            public void LogResult_handles_canceled_commands()
            {
                var writer = new StringWriter();

                var interceptionContext = new DbCommandInterceptionContext<DbDataReader>();
                interceptionContext.MutableData.TaskStatus = TaskStatus.Canceled;

                new DbCommandLogger(writer.Write).LogResult(
                    new Mock<DbCommand>().Object,
                    null,
                    interceptionContext);

                Assert.Equal(Strings.CommandLogCanceled(0, ""), GetSingleLine(writer));
            }

            [Fact]
            public void LogResult_logs_elapsed_time_for_completed_commands()
            {
                var writer = new StringWriter();
                var logger = new DbCommandLogger(writer.Write);
                var elapsed = GetElapsed(logger);

                logger.LogResult(new Mock<DbCommand>().Object, 77, new DbCommandInterceptionContext<int>());

                Assert.Equal(Strings.CommandLogComplete(elapsed, "77", ""), GetSingleLine(writer));
            }

            [Fact]
            public void LogResult_logs_elapsed_time_for_failed_commands()
            {
                var writer = new StringWriter();
                var logger = new DbCommandLogger(writer.Write);
                var elapsed = GetElapsed(logger);

                logger.LogResult(
                    new Mock<DbCommand>().Object,
                    77,
                    new DbCommandInterceptionContext<int> { Exception = new Exception("I do not like them!") } );

                Assert.Equal(Strings.CommandLogFailed(elapsed, "I do not like them!", ""), GetSingleLine(writer));
            }

            [Fact]
            public void LogResult_logs_elapsed_time_for_canceled_commands()
            {
                var writer = new StringWriter();
                var logger = new DbCommandLogger(writer.Write);
                var elapsed = GetElapsed(logger);

                var interceptionContext = new DbCommandInterceptionContext<int>();
                interceptionContext.MutableData.TaskStatus = TaskStatus.Canceled;

                logger.LogResult(
                    new Mock<DbCommand>().Object, 77, interceptionContext);

                Assert.Equal(Strings.CommandLogCanceled(elapsed, ""), GetSingleLine(writer));
            }

            private static long GetElapsed(DbCommandLogger logger)
            {
                logger.Stopwatch.Restart();
                Thread.Sleep(10);
                logger.Stopwatch.Stop();
                return logger.Stopwatch.ElapsedMilliseconds;
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
