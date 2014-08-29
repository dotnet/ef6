// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Interception
{
    using System.Data.Entity.Infrastructure.Interception;
    using System.IO;
    using System.Linq;
    using System.Text;
    using SimpleModel;
    using Xunit;

    public class DatabaseLoggerTests : TestBase
    {
        [Fact]
        public void DatabaseLogger_can_log_to_the_console()
        {
            Assert.Contains(
                "FROM [dbo].[Products]",
                CaptureConsoleOutput(
                    () =>
                    {
                        using (var logger = new DatabaseLogger())
                        {
                            using (var context = new SimpleModelContext())
                            {
                                logger.StartLogging();

                                context.Products.ToArray();
                            }
                        }
                    }));
        }

        [Fact]
        public void DatabaseLogger_can_log_to_a_file()
        {
            Assert.Contains(
                "FROM [dbo].[Products]",
                CaptureFileOutput(
                    f =>
                    {
                        using (var logger = new DatabaseLogger(f))
                        {
                            using (var context = new SimpleModelContext())
                            {
                                logger.StartLogging();

                                context.Products.ToArray();
                            }
                        }
                    }));
        }

        [Fact]
        public void DatabaseLogger_can_append_to_a_file()
        {
            using (var context = new SimpleModelContext())
            {
                var output = CaptureFileOutput(
                    f =>
                    {
                        using (var logger = new DatabaseLogger(f, append: false))
                        {
                            logger.StartLogging();

                            context.Categories.ToArray();
                        }

                        using (var logger = new DatabaseLogger(f, append: true))
                        {
                            logger.StartLogging();

                            context.Products.ToArray();
                        }
                    });

                Assert.Contains("FROM [dbo].[Categories]", output);
                Assert.Contains("FROM [dbo].[Products]", output);
            }
        }

        [Fact]
        public void Logging_can_be_started_and_stopped()
        {
            using (var context = new SimpleModelContext())
            {
                var output = CaptureConsoleOutput(
                    () =>
                    {
                        using (var logger = new DatabaseLogger())
                        {
                            context.Products.ToArray();

                            logger.StartLogging();
                            logger.StopLogging();

                            context.Products.ToArray();

                            logger.StartLogging();

                            context.Categories.ToArray();

                            logger.StopLogging();

                            context.Products.ToArray();
                        }
                    });

                var foundIndex = output.IndexOf("FROM [dbo].[Categories]");
                Assert.True(foundIndex > 0);
                foundIndex = output.IndexOf("FROM [dbo].[Categories]", foundIndex + 1);
                Assert.Equal(-1, foundIndex);

                Assert.DoesNotContain("FROM [dbo].[Products]", output);
            }
        }

        [Fact]
        public void Starting_is_no_op_if_already_started_and_likewise_for_stopping()
        {
            using (var context = new SimpleModelContext())
            {
                var output = CaptureConsoleOutput(
                    () =>
                    {
                        using (var logger = new DatabaseLogger())
                        {
                            logger.StopLogging();

                            context.Products.ToArray();

                            logger.StartLogging();
                            logger.StartLogging();

                            context.Categories.ToArray();

                            logger.StopLogging();
                            logger.StopLogging();

                            context.Products.ToArray();
                        }
                    });

                Assert.Contains("FROM [dbo].[Categories]", output);
                Assert.DoesNotContain("FROM [dbo].[Products]", output);
            }
        }

        [Fact]
        public void Dispose_stops_logging()
        {
            using (var context = new SimpleModelContext())
            {
                var output = CaptureConsoleOutput(
                    () =>
                    {
                        using (var logger = new DatabaseLogger())
                        {
                            logger.StartLogging();

                            context.Categories.ToArray();
                        }

                        context.Products.ToArray();
                    });

                Assert.Contains("FROM [dbo].[Categories]", output);
                Assert.DoesNotContain("FROM [dbo].[Products]", output);
            }
        }

        /// <summary>
        /// This test makes calls from multiple threads such that we have at least some chance of finding threading
        /// issues. As with any test of this type just because the test passes does not mean that the code is
        /// correct. On the other hand if this test ever fails (EVEN ONCE) then we know there is a problem to
        /// be investigated. DON'T just re-run and think things are okay if the test then passes.
        /// </summary>
        [Fact] // CodePlex 2568
        public void DatabaseLogger_can_be_used_concurrently()
        {
            CaptureFileOutput(
                f =>
                {
                    using (var logger = new DatabaseLogger(f))
                    {
                        logger.StartLogging();

                        ExecuteInParallel(
                            () =>
                            {
                                using (var context = new SimpleModelContext())
                                {
                                    for (var i = 0; i < 200; i++)
                                    {
                                        context.Products.AsNoTracking().Load();
                                    }
                                }
                            }, 30);
                    }
                });
        }

        private
            string CaptureConsoleOutput(Action test)
        {
            var consoleOut = Console.Out;
            try
            {
                var output = new StringBuilder();

                using (var writer = new StringWriter(output))
                {
                    Console.SetOut(writer);

                    test();
                }

                return output.ToString();
            }
            finally
            {
                Console.SetOut(consoleOut);
            }
        }

        private string CaptureFileOutput(Action<string> test)
        {
            var tempFileName = Path.GetTempFileName();
            try
            {
                test(tempFileName);

                return File.ReadAllText(tempFileName);
            }
            finally
            {
                try
                {
                    File.SetAttributes(tempFileName, File.GetAttributes(tempFileName) & ~FileAttributes.ReadOnly);
                    File.Delete(tempFileName);
                }
                catch (FileNotFoundException)
                {
                }
            }
        }
    }
}
