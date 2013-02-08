// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Linq;
    using Moq;
    using ProductivityApiTests;
    using Xunit;

    public class CommandTracerTests
    {
        [Fact]
        public void Can_enable_tracing_and_return_commands()
        {
            var mockCommandInterceptor = new Mock<IDbCommandInterceptor>();

            var commandTracer = new CommandTracer(mockCommandInterceptor.Object);

            mockCommandInterceptor.VerifySet(i => i.IsEnabled = true);

            var interceptedCommands = new List<InterceptedCommand>();
            mockCommandInterceptor.SetupGet(i => i.Commands).Returns(interceptedCommands);

            Assert.Same(interceptedCommands, commandTracer.Commands);
        }

        [Fact]
        public void Dispose_should_disable_tracing()
        {
            var mockCommandInterceptor = new Mock<IDbCommandInterceptor>();

            var commandTracer = new CommandTracer(mockCommandInterceptor.Object);

            commandTracer.Dispose();

            mockCommandInterceptor.VerifySet(i => i.IsEnabled = false);
        }

        [Fact]
        public void Can_trace_commands()
        {
            using (var context = new YummyContext())
            {
                context.Products.Add(
                    new YummyProduct
                        {
                            Name = "Pineapple Lumps"
                        });

                using (var commandTracer = new CommandTracer())
                {
                    context.SaveChanges();

                    var interceptedCommand = commandTracer.Commands.Single();

                    Assert.Equal(
                        "insert [dbo].[YummyProducts]([Id], [Name])\r\nvalues (@0, @1)\r\n",
                        interceptedCommand.CommandText);
                    Assert.Equal(2, interceptedCommand.Parameters.Count());
                }
            }
        }
    }
}
