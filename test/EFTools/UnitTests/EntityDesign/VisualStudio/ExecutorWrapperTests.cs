// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Data.Entity.SqlServer;
    using System.IO;
    using Xunit;

    public class ExecutorWrapperTests
    {
        [Fact]
        public void GetProviderServices_returns_assembly_qualified_type_name()
        {
            var domain = AppDomain.CreateDomain("ExecutorWrapperTests", null, AppDomain.CurrentDomain.SetupInformation);
            try
            {
                var executor = new ExecutorWrapper(
                    domain,
                    Path.GetFileName(GetType().Assembly.CodeBase));

                var typeName = executor.GetProviderServices("System.Data.SqlClient");

                Assert.Equal(typeof(SqlProviderServices).AssemblyQualifiedName, typeName);
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        [Fact]
        public void GetProviderServices_returns_null_when_unknown()
        {
            var domain = AppDomain.CreateDomain("ExecutorWrapperTests", null, AppDomain.CurrentDomain.SetupInformation);
            try
            {
                var executor = new ExecutorWrapper(
                    domain,
                    Path.GetFileName(GetType().Assembly.CodeBase));

                Assert.Null(executor.GetProviderServices("My.Fake.Provider"));
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        [Fact]
        public void GetProviderServices_returns_null_when_no_project_assembly()
        {
            var domain = AppDomain.CreateDomain("ExecutorWrapperTests", null, AppDomain.CurrentDomain.SetupInformation);
            try
            {
                var executor = new ExecutorWrapper(
                    domain,
                    "UnknownProject.dll");

                Assert.Null(executor.GetProviderServices("System.Data.SqlClient"));
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }

        [Fact]
        public void GetProviderServices_returns_null_when_no_EntityFramework_assembly()
        {
            var domain = AppDomain.CreateDomain(
                "ExecutorWrapperTests",
                null,
                new AppDomainSetup
                    {
                        // NOTE: This will cause assembly resolution for EntityFramework to fail
                        ApplicationBase = Path.GetTempPath()
                    });
            try
            {
                var executor = new ExecutorWrapper(
                    domain,
                    Path.GetFileName(GetType().Assembly.CodeBase));

                Assert.Null(executor.GetProviderServices("System.Data.SqlClient"));
            }
            finally
            {
                AppDomain.Unload(domain);
            }
        }
    }
}
