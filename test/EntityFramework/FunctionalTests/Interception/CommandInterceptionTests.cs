// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Interception
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.TestHelpers;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class CommandInterceptionTests : FunctionalTestBase
    {
        public CommandInterceptionTests()
        {
            using (var context = new BlogContextNoInit())
            {
                context.Database.Initialize(force: false);
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Initialization_and_simple_query_and_update_commands_can_be_logged()
        {
            CommandInterceptionTest<BlogContextLogAll>();
            CommandInterceptionTest<BlogContextDateTime2LogAll>();
            CommandInterceptionTest<BlogContextDateTimeLogAll>();
        }

        private void CommandInterceptionTest<TContextType>()
            where TContextType : BlogContext, new()
        {
            var logger = new CommandLogger();
            DbInterception.Add(logger);

            try
            {
                using (var context = new TContextType())
                {
                    BlogContext.DoStuff(context);
                }
            }
            finally
            {
                DbInterception.Remove(logger);
            }

            var commandsUsed = new bool[Enum.GetValues(typeof(CommandMethod)).Length];

            for (var i = 0; i < logger.Log.Count; i++)
            {
                var method = logger.Log[i].Method;
                commandsUsed[(int)method] = true;

                if (method.ToString().EndsWith("Executing"))
                {
                    Assert.Equal(method + 1, logger.Log[i + 1].Method);
                    Assert.Same(logger.Log[i].Command, logger.Log[i + 1].Command);
                    
                    var parameters = logger.Log[i].Command.Parameters;

                    var expectedDbType = typeof(TContextType) == typeof(BlogContextDateTimeLogAll)
                        ? DbType.DateTime
                        : DbType.DateTime2;
                    
                    foreach (DbParameter parameter in parameters)
                    {
                        if (parameter.Value is DateTime)
                        {
                            Assert.Equal(expectedDbType, parameter.DbType);
                        }
                    }
                }
            }

            // Check that every type of command used has log entries
            Assert.True(commandsUsed[(int)CommandMethod.NonQueryExecuting]);
            Assert.True(commandsUsed[(int)CommandMethod.NonQueryExecuted]);
            Assert.True(commandsUsed[(int)CommandMethod.ReaderExecuting]);
            Assert.True(commandsUsed[(int)CommandMethod.ReaderExecuted]);
            Assert.True(commandsUsed[(int)CommandMethod.ScalarExecuting]);
            Assert.True(commandsUsed[(int)CommandMethod.ScalarExecuted]);

            // Sanity check on command text
            var commandTexts = logger.Log.Select(l => l.CommandText.ToLowerInvariant());
            Assert.True(commandTexts.Any(c => c.StartsWith("select")));
            Assert.True(commandTexts.Any(c => c.StartsWith("create")));
            Assert.True(commandTexts.Any(c => c.StartsWith("alter")));
            Assert.True(commandTexts.Any(c => c.StartsWith("insert")));
        }

        public class BlogContextLogAll : BlogContext
        {
            static BlogContextLogAll()
            {
                Database.SetInitializer<BlogContextLogAll>(new BlogInitializer());
            }
        }
        [ForceDateTimeType(DbType.DateTime)]
        public class BlogContextDateTimeLogAll : BlogContext
        {
            static BlogContextDateTimeLogAll()
            {
                Database.SetInitializer<BlogContextDateTimeLogAll>(new BlogInitializer());
            }
        }
        
        [ForceDateTimeType(DbType.DateTime2)]
        public class BlogContextDateTime2LogAll : BlogContext
        {
            static BlogContextDateTime2LogAll()
            {
                Database.SetInitializer<BlogContextDateTime2LogAll>(new BlogInitializer());
            }
        }

        [Fact]
        public void Commands_that_result_in_exceptions_are_still_intercepted()
        {
            var logger = new CommandLogger();
            DbInterception.Add(logger);

            Exception exception;
            try
            {
                using (var context = new BlogContextNoInit())
                {
                    exception = Assert.Throws<SqlException>(() => context.Blogs.SqlQuery("select * from No.Chance").ToList());
                }
            }
            finally
            {
                DbInterception.Remove(logger);
            }

            Assert.Equal(2, logger.Log.Count);

            var executingLog = logger.Log[0];
            Assert.Equal(CommandMethod.ReaderExecuting, executingLog.Method);
            Assert.False(executingLog.IsAsync);
            Assert.Null(executingLog.Result);
            Assert.Null(executingLog.Exception);

            var executedLog = logger.Log[1];
            Assert.Equal(CommandMethod.ReaderExecuted, executedLog.Method);
            Assert.False(executedLog.IsAsync);
            Assert.Null(executedLog.Result);
            Assert.Same(exception, executedLog.Exception);
        }

#if !NET40
        [Fact]
        public void Async_commands_that_result_in_exceptions_are_still_intercepted()
        {
            var logger = new CommandLogger();
            DbInterception.Add(logger);

            try
            {
                using (var context = new BlogContextNoInit())
                {
                    var query = context.Blogs.SqlQuery("select * from No.Chance").ToListAsync();

                    Assert.Throws<AggregateException>(() => query.Wait());

                    Assert.True(query.IsFaulted);
                }
            }
            finally
            {
                DbInterception.Remove(logger);
            }

            Assert.Equal(2, logger.Log.Count);

            var executingLog = logger.Log[0];
            Assert.Equal(CommandMethod.ReaderExecuting, executingLog.Method);
            Assert.True(executingLog.IsAsync);
            Assert.Null(executingLog.Result);
            Assert.Null(executingLog.Exception);

            var executedLog = logger.Log[1];
            Assert.Equal(CommandMethod.ReaderExecuted, executedLog.Method);
            Assert.True(executedLog.IsAsync);
            Assert.Null(executedLog.Result);
            Assert.IsType<SqlException>(executedLog.Exception);
            Assert.True(executedLog.TaskStatus.HasFlag(TaskStatus.Faulted));
        }

#endif

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Multiple_contexts_running_concurrently_can_use_interception()
        {
            var loggers = new ConcurrentBag<CommandLogger>();

            const int executionCount = 5;
            ExecuteInParallel(
                () =>
                    {
                        using (var context = new BlogContextNoInit())
                        {
                            var logger = new CommandLogger(context);
                            DbInterception.Add(logger);
                            loggers.Add(logger);

                            try
                            {
                                BlogContext.DoStuff(context);
                            }
                            finally
                            {
                                DbInterception.Remove(logger);
                            }

                            var commandsUsed = new bool[Enum.GetValues(typeof(CommandMethod)).Length];

                            for (var i = 0; i < logger.Log.Count; i++)
                            {
                                var method = logger.Log[i].Method;
                                commandsUsed[(int)method] = true;

                                if (method.ToString().EndsWith("Executing"))
                                {
                                    Assert.Equal(method + 1, logger.Log[i + 1].Method);
                                    Assert.Same(logger.Log[i].Command, logger.Log[i + 1].Command);
                                }
                            }

                            // Check that expected command have log entries
                            Assert.True(commandsUsed[(int)CommandMethod.ReaderExecuting]);
                            Assert.True(commandsUsed[(int)CommandMethod.ReaderExecuted]);
                            Assert.True(commandsUsed[(int)CommandMethod.ReaderExecuting]);
                            Assert.True(commandsUsed[(int)CommandMethod.ReaderExecuted]);
#if !NET40
                            Assert.True(commandsUsed[(int)CommandMethod.NonQueryExecuting]);
                            Assert.True(commandsUsed[(int)CommandMethod.NonQueryExecuted]);
#endif

                            // Sanity check on command text
                            var commandTexts = logger.Log.Select(l => l.CommandText.ToLowerInvariant());
                            Assert.True(commandTexts.Any(c => c.StartsWith("select")));
                            Assert.True(commandTexts.Any(c => c.StartsWith("insert")));
#if !NET40
                            Assert.True(commandTexts.Any(c => c.StartsWith("update")));
#endif

                            // Sanity check on results
                            Assert.True(logger.Log.Where(l => l.Method == CommandMethod.NonQueryExecuted).All(l => l.Result != null));
                            Assert.True(logger.Log.Where(l => l.Method == CommandMethod.ReaderExecuted).All(l => l.Result != null));
                        }
                    }, executionCount);

            // Check that each execution logged exactly the same commands.

            Assert.Equal(executionCount, loggers.Count);

            var firstLog = loggers.First().Log;
            foreach (var log in loggers.Select(l => l.Log).Skip(1))
            {
                Assert.Equal(firstLog.Count, log.Count);

                for (var i = 0; i < log.Count; i++)
                {
                    Assert.Equal(firstLog[i].Method, log[i].Method);
                    Assert.Equal(firstLog[i].CommandText, log[i].CommandText);

                    if (firstLog[i].Result == null)
                    {
                        Assert.Null(log[i].Result);
                    }
                    else
                    {
                        Assert.Same(firstLog[i].Result.GetType(), log[i].Result.GetType());
                    }
                }
            }
        }

        public enum CommandMethod
        {
            NonQueryExecuting = 0,
            NonQueryExecuted,
            ReaderExecuting,
            ReaderExecuted,
            ScalarExecuting,
            ScalarExecuted
        }

        public class CommandLogger : IDbCommandInterceptor
        {
            private readonly DbContext _context;
            private readonly IList<CommandLogItem> _log = new List<CommandLogItem>();

            public CommandLogger(DbContext context = null)
            {
                _context = context;
            }

            public IList<CommandLogItem> Log
            {
                get { return _log; }
            }

            public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(
                        new CommandLogItem(
                            CommandMethod.NonQueryExecuting, command, interceptionContext.Exception, interceptionContext.TaskStatus,
                            interceptionContext.IsAsync));
                }
            }

            public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(
                        new CommandLogItem(
                            CommandMethod.NonQueryExecuted, command, interceptionContext.Exception, interceptionContext.TaskStatus,
                            interceptionContext.IsAsync, interceptionContext.Result));
                }
            }

            public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(
                        new CommandLogItem(
                            CommandMethod.ReaderExecuting, command, interceptionContext.Exception, interceptionContext.TaskStatus,
                            interceptionContext.IsAsync));
                }
            }

            public void ReaderExecuted(
                DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(
                        new CommandLogItem(
                            CommandMethod.ReaderExecuted, command, interceptionContext.Exception, interceptionContext.TaskStatus,
                            interceptionContext.IsAsync, interceptionContext.Result));
                }
            }

            public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(
                        new CommandLogItem(
                            CommandMethod.ScalarExecuting, command, interceptionContext.Exception, interceptionContext.TaskStatus,
                            interceptionContext.IsAsync));
                }
            }

            public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
            {
                if (ShouldLog(interceptionContext))
                {
                    _log.Add(
                        new CommandLogItem(
                            CommandMethod.ScalarExecuted, command, interceptionContext.Exception, interceptionContext.TaskStatus,
                            interceptionContext.IsAsync, interceptionContext.Result));
                }
            }

            private bool ShouldLog(DbInterceptionContext interceptionContext)
            {
                return _context == null || interceptionContext.DbContexts.Contains(_context);
            }
        }

        public class CommandLogItem
        {
            public CommandLogItem(
                CommandMethod method,
                DbCommand command,
                Exception exception,
                TaskStatus taskStatus,
                bool isAsync,
                object result = null)
            {
                Method = method;
                CommandText = command.CommandText;
                Command = command;
                Exception = exception;
                TaskStatus = taskStatus;
                IsAsync = isAsync;
                Result = result;
            }

            public CommandMethod Method { get; set; }
            public string CommandText { get; set; }
            public DbCommand Command { get; set; }
            public Exception Exception { get; set; }
            public bool IsAsync { get; set; }
            public TaskStatus TaskStatus { get; set; }
            public object Result { get; set; }
        }
    }
}
