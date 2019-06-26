// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Interception
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.TestHelpers;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Xunit;

    internal static partial class TestHelper
    {
        internal static string CollapseWhitespace(this string value)
        {
            value = value.Replace(Environment.NewLine, " ").Replace("\t", " ");

            while (value.Contains("  "))
                value = value.Replace("  ", " ");

            return value;
        }
    }

    public class DatabaseLogFormatterTests : FunctionalTestBase
    {
        private readonly StringResourceVerifier _resourceVerifier = new StringResourceVerifier(
            new AssemblyResourceLookup(EntityFrameworkAssembly, "System.Data.Entity.Properties.Resources"));

        public DatabaseLogFormatterTests()
        {
            using (var context = new BlogContextNoInit())
            {
                context.Database.Initialize(force: false);
            }
        }

        [Fact]
        [UseDefaultExecutionStrategy]
        public void Simple_query_and_update_commands_can_be_logged()
        {
            var log = new StringWriter();
            using (var context = new BlogContextNoInit())
            {
                context.Database.Log = log.Write;
                BlogContext.DoStuff(context);
            }

            var logLines = log.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

#if NET40
            const int selectCount = 4;
            const int updateCount = 0;
            const int asyncCount = 0;
            const int paramCount = 2;
            const int imALoggerCount = 0;
            const int transactionCount = 1;
            const int connectionCount = 3;
#else
            const int selectCount = 5;
            const int updateCount = 1;
            const int asyncCount = 2;
            const int paramCount = 4;
            const int imALoggerCount = 1;
            const int transactionCount = 2;
            const int connectionCount = 4;
#endif

            Assert.Equal(selectCount, logLines.Count(l => l.ToUpperInvariant().StartsWith("SELECT")));
            Assert.Equal(1, logLines.Count(l => l.ToUpperInvariant().StartsWith("INSERT")));
            Assert.Equal(updateCount, logLines.Count(l => l.ToUpperInvariant().StartsWith("UPDATE")));

            Assert.Equal(asyncCount, logLines.Count(l => _resourceVerifier.IsMatch("CommandLogAsync", l)));

            Assert.Equal(paramCount, logLines.Count(l => l.StartsWith("-- @")));
            Assert.Equal(1, logLines.Count(l => l.StartsWith("-- @") && l.Contains("'Throw it away...'")));
            Assert.Equal(imALoggerCount, logLines.Count(l => l.StartsWith("-- @") && l.Contains("'I'm a logger and I'm okay...'")));
            Assert.Equal(paramCount / 2, logLines.Count(l => l.StartsWith("-- @") && l.Contains("'1'")));

            Assert.Equal(selectCount, logLines.Count(l => _resourceVerifier.IsMatch("CommandLogComplete", l, new AnyValueParameter(), "SqlDataReader", "")));
            Assert.Equal(updateCount, logLines.Count(l => _resourceVerifier.IsMatch("CommandLogComplete", l, new AnyValueParameter(), "1", "")));

            Assert.Equal(transactionCount, logLines.Count(l => _resourceVerifier.IsMatch("TransactionStartedLog", l, new AnyValueParameter(), "")));
            Assert.Equal(transactionCount, logLines.Count(l => _resourceVerifier.IsMatch("TransactionDisposedLog", l, new AnyValueParameter(), "")));

            Assert.Equal(connectionCount, logLines.Count(l => _resourceVerifier.IsMatch("ConnectionOpenedLog", l, new AnyValueParameter(), "")));
            Assert.Equal(connectionCount, logLines.Count(l => _resourceVerifier.IsMatch("ConnectionClosedLog", l, new AnyValueParameter(), "")));
        }

        [Fact]
        public void Commands_that_result_in_exceptions_are_still_logged()
        {
            Exception exception;
            var log = new StringWriter();
            using (var context = new BlogContextNoInit())
            {
                context.Database.Log = log.Write;
                exception = Assert.Throws<SqlException>(() => context.Blogs.SqlQuery("select * from No.Chance").ToList());
            }

            var logLines = log.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            Assert.Equal(7, logLines.Length);
            Assert.Equal("select * from No.Chance", logLines[1]);
            _resourceVerifier.VerifyMatch("CommandLogFailed", logLines[3], new AnyValueParameter(), exception.Message, "");
        }

#if NET452
        [Fact]
        public void DatabaseLogFormatter_is_disposed_even_if_the_context_is_not()
        {
            CreateContext(out var weakDbContext, out var weakStringWriter);

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.False(weakDbContext.IsAlive);
            DbDispatchersHelpers.AssertNoInterceptors();

            // Need a second pass as the DatabaseLogFormatter is removed from the interceptors in the InternalContext finalizer
            GC.Collect();

            Assert.False(weakStringWriter.IsAlive);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void CreateContext(out WeakReference weakDbContext, out WeakReference weakStringWriter)
        {
            var log = new StringWriter();
            var context = new BlogContextNoInit();

            context.Database.Log = log.Write;
            weakDbContext = new WeakReference(context);
            weakStringWriter = new WeakReference(log);
        }
#endif

#if !NET40
        [Fact]
        public void Async_commands_that_result_in_exceptions_are_still_logged()
        {
            Exception exception;
            var log = new StringWriter();
            using (var context = new BlogContextNoInit())
            {
                context.Database.Log = log.Write;
                var query = context.Blogs.SqlQuery("select * from No.Chance").ToListAsync();

                exception = Assert.Throws<AggregateException>(() => query.Wait()).InnerException;
            }

            var logLines = log.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            Assert.Equal(7, logLines.Length);
            Assert.Equal("select * from No.Chance", logLines[1]);
            _resourceVerifier.VerifyMatch("CommandLogAsync", logLines[2], new AnyValueParameter(), "");
            _resourceVerifier.VerifyMatch("CommandLogFailed", logLines[3], new AnyValueParameter(), exception.Message, "");
        }
#endif

        [Fact]
        public void The_command_formatter_to_use_can_be_changed()
        {
            var log = new StringWriter();
            try
            {
                MutableResolver.AddResolver<Func<DbContext, Action<string>, DatabaseLogFormatter>>(
                    k => (Func<DbContext, Action<string>, DatabaseLogFormatter>)((c, w) => new TestDatabaseLogFormatter(c, w)));

                using (var context = new BlogContextNoInit())
                {
                    context.Database.Log = log.Write;
                    var blog = context.Blogs.Single();
                    Assert.Equal("Half a Unicorn", blog.Title);
                }
            }
            finally
            {
                MutableResolver.ClearResolvers();
            }

            var logLines = log.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            Assert.Equal(3, logLines.Length);

            Assert.Equal(
                "Context 'BlogContextNoInit' is executing command 'SELECT TOP (2) [c].[Id] AS [Id], [c].[Title] AS [Title] FROM [dbo].[Blogs] AS [c]'",
                logLines[0]);

            Assert.Equal(
                "Context 'BlogContextNoInit' finished executing command",
                logLines[1]);
        }

        public class TestDatabaseLogFormatter : DatabaseLogFormatter
        {
            public TestDatabaseLogFormatter(DbContext context, Action<string> writeAction)
                : base(context, writeAction)
            {
            }

            public override void LogCommand<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
            {
                Write(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Context '{0}' is executing command '{1}'{2}",
                        Context.GetType().Name,
                        command.CommandText.CollapseWhitespace(), Environment.NewLine));
            }

            public override void LogResult<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
            {
                Write(
                    string.Format(
                        CultureInfo.CurrentCulture, "Context '{0}' finished executing command{1}", Context.GetType().Name,
                        Environment.NewLine));
            }

            public override void Opened(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
            }

            public override void Closed(DbConnection connection, DbConnectionInterceptionContext interceptionContext)
            {
            }
        }
    }
}
