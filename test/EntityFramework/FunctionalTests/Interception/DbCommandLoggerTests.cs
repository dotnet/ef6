// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Interception
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Xunit;

    public class DbCommandLoggerTests : FunctionalTestBase
    {
        private readonly StringResourceVerifier _resourceVerifier = new StringResourceVerifier(
            new AssemblyResourceLookup(EntityFrameworkAssembly, "System.Data.Entity.Properties.Resources"));

        public DbCommandLoggerTests()
        {
            using (var context = new BlogContextNoInit())
            {
                context.Database.Initialize(force: false);
            }
        }

        [Fact]
        public void Simple_query_and_update_commands_can_be_logged()
        {
            var log = new StringWriter();
            using (var context = new BlogContextNoInit())
            {
                context.Database.Log = log;
                BlogContext.DoStuff(context);
            }

            var logLines = log.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

#if NET40
            const int selectCount = 4;
            const int updateCount = 0;
            const int asyncCount = 0;
            const int paramCount = 2;
            const int imALoggerCount = 0;
#else
            const int selectCount = 5;
            const int updateCount = 1;
            const int asyncCount = 2;
            const int paramCount = 4;
            const int imALoggerCount = 1;
#endif
            Assert.Equal(selectCount, logLines.Count(l => l.ToUpperInvariant().StartsWith("SELECT")));
            Assert.Equal(1, logLines.Count(l => l.ToUpperInvariant().StartsWith("INSERT")));
            Assert.Equal(updateCount, logLines.Count(l => l.ToUpperInvariant().StartsWith("UPDATE")));

            Assert.Equal(asyncCount, logLines.Count(l => _resourceVerifier.IsMatch("CommandLogAsync", l)));

            Assert.Equal(paramCount, logLines.Count(l => l.StartsWith("-- @")));
            Assert.Equal(1, logLines.Count(l => l.StartsWith("-- @") && l.EndsWith("[Throw it away...]")));
            Assert.Equal(imALoggerCount, logLines.Count(l => l.StartsWith("-- @") && l.EndsWith("[I'm a logger and I'm okay...]")));
            Assert.Equal(paramCount / 2, logLines.Count(l => l.StartsWith("-- @") && l.EndsWith("[1]")));

            Assert.Equal(selectCount, logLines.Count(l => _resourceVerifier.IsMatch("CommandLogComplete", l, "SqlDataReader")));
            Assert.Equal(updateCount, logLines.Count(l => _resourceVerifier.IsMatch("CommandLogComplete", l, "1")));
        }

        [Fact]
        public void Commands_that_result_in_exceptions_are_still_logged()
        {
            Exception exception;
            var log = new StringWriter();
            using (var context = new BlogContextNoInit())
            {
                context.Database.Log = log;
                exception = Assert.Throws<SqlException>(() => context.Blogs.SqlQuery("select * from No.Chance").ToList());
            }

            var logLines = log.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            Assert.Equal(4, logLines.Length);
            Assert.Equal("select * from No.Chance", logLines[0]);
            _resourceVerifier.VerifyMatch("CommandLogFailed", logLines[1], exception.Message);
        }

#if !NET40
        [Fact]
        public void Async_commands_that_result_in_exceptions_are_still_logged()
        {
            Exception exception;
            var log = new StringWriter();
            using (var context = new BlogContextNoInit())
            {
                context.Database.Log = log;
                var query = context.Blogs.SqlQuery("select * from No.Chance").ToListAsync();

                exception = Assert.Throws<AggregateException>(() => query.Wait()).InnerException;
            }

            var logLines = log.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            Assert.Equal(5, logLines.Length);
            Assert.Equal("select * from No.Chance", logLines[0]);
            _resourceVerifier.VerifyMatch("CommandLogAsync", logLines[1]);
            _resourceVerifier.VerifyMatch("CommandLogFailed", logLines[2], exception.Message);
        }

        [Fact]
        public void Async_commands_that_are_canceled_are_still_logged()
        {
            var log = new StringWriter();
            using (var context = new BlogContextNoInit())
            {
                context.Database.Log = log;

                context.Database.Connection.Open();

                var cancellation = new CancellationTokenSource();
                cancellation.Cancel();
                var command = context.Database.ExecuteSqlCommandAsync("update Blogs set Title = 'No' where Id = -1", cancellation.Token);

                Assert.Throws<AggregateException>(() => command.Wait());
                Assert.True(command.IsCanceled);

                context.Database.Connection.Close();
            }

            var logLines = log.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            Assert.Equal(5, logLines.Length);
            Assert.Equal("update Blogs set Title = 'No' where Id = -1", logLines[0]);
            _resourceVerifier.VerifyMatch("CommandLogAsync", logLines[1]);
            _resourceVerifier.VerifyMatch("CommandLogCanceled", logLines[2]);
        }
#endif

        [Fact]
        public void The_command_logger_to_use_can_be_changed()
        {
            var log = new StringWriter();
            try
            {
                MutableResolver.AddResolver<DbCommandLoggerFactory>(
                    k => (DbCommandLoggerFactory)((c, w) => new TestDbCommandLogger(c, w)));

                using (var context = new BlogContextNoInit())
                {
                    context.Database.Log = log;
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
                "Context 'BlogContextNoInit' is executing command 'SELECT TOP (2) [c].[Id] AS [Id], [c].[Title] AS [Title]FROM [dbo].[Blogs] AS [c]'",
                logLines[0]);

            Assert.Equal(
                "Context 'BlogContextNoInit' finished executing command",
                logLines[1]);
        }

        public class TestDbCommandLogger : DbCommandLogger
        {
            public TestDbCommandLogger(DbContext context, TextWriter writer)
                : base(context, writer)
            {
            }

            public override void LogCommand(DbCommand command, DbCommandInterceptionContext interceptionContext)
            {
                Writer.WriteLine(
                    "Context '{0}' is executing command '{1}'",
                    Context.GetType().Name,
                    command.CommandText.Replace(Environment.NewLine, ""));
            }

            public override void LogResult(DbCommand command, object result, DbCommandInterceptionContext interceptionContext)
            {
                Writer.WriteLine("Context '{0}' finished executing command", Context.GetType().Name);
            }
        }
    }
}
