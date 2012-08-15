// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using Moq;
    using Xunit;

    public class DefaultCommandInterceptorTests : TestBase
    {
        [Fact]
        public void Can_enable_and_intercept_commands()
        {
            var commandInterceptor = new DefaultCommandInterceptor();

            Assert.False(commandInterceptor.IsEnabled);
            Assert.Empty(commandInterceptor.Commands);

            var command
                = new Mock<DbCommand>
                      {
                          DefaultValue = DefaultValue.Mock
                      }
                    .Object;

            Assert.True(commandInterceptor.Intercept(command));

            Assert.False(commandInterceptor.IsEnabled);
            Assert.Empty(commandInterceptor.Commands);

            commandInterceptor.IsEnabled = true;

            Assert.True(commandInterceptor.IsEnabled);
            Assert.Empty(commandInterceptor.Commands);

            Assert.False(commandInterceptor.Intercept(command));

            Assert.NotEmpty(commandInterceptor.Commands);
        }
    }
}
