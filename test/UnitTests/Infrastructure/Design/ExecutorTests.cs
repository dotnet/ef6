// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Design
{
    using System.Data.Entity.SqlServer;
    using System.Data.Entity.SqlServer.Utilities;
    using System.IO;
    using System.Reflection;
    using Moq;
    using Xunit;

    public class ExecutorTests
    {
        private static readonly string AssemblyFile
            = Path.GetFileName(new Uri(typeof(ExecutorTests).Assembly().CodeBase).LocalPath);

        [Fact]
        public void GetProviderServicesInternal_returns_type_name()
        {
            var executor = new Executor(AssemblyFile, null);

            var providerServicesTypeName = executor.GetProviderServicesInternal("System.Data.SqlClient");

            Assert.Equal(typeof(SqlProviderServices).AssemblyQualifiedName, providerServicesTypeName);
        }

        [Fact]
        public void GetProviderServicesInternal_returns_null_when_none()
        {
            var executor = new Executor(AssemblyFile, null);

            var providerServicesTypeName = executor.GetProviderServicesInternal("My.Fake.Provider");

            Assert.Null(providerServicesTypeName);
        }

        [Fact]
        public void GetProviderServices_invokes_internal_implementation()
        {
            var executor = new Mock<Executor>(AssemblyFile, null);
            executor.Setup(e => e.GetProviderServicesInternal(It.IsAny<string>())).Returns("Type1");

            var handler = new Mock<HandlerBase>().As<IResultHandler>();

            new Executor.GetProviderServices(executor.Object, handler.Object, "Provider1", null);

            executor.Verify(e => e.GetProviderServicesInternal("Provider1"));
            handler.Verify(h => h.SetResult("Type1"));
        }
    }
}
