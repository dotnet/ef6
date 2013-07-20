// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Design
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Infrastructure.Design;
    using System.Data.Entity.SqlServer;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CSharp;
    using Xunit;

    /// <summary>
    /// This test is in the "transitional" project for now since the new executor classes are internal for EF6.
    /// </summary>
    public class BasicDesignTimeScenarios : TestBase
    {
        private const string Source = @"
namespace ConsoleApplication1
{
    internal class Program
    {
        private static void Main()
        {
        }
    }
}
";
        private const string Config = @"
<configuration>
  <configSections>
    <section name='entityFramework' type='System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework' />
  </configSections>
  <entityFramework>
    <providers>
      <provider invariantName='System.Data.SqlClient' type='System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer' />
    </providers>
  </entityFramework>
</configuration>
";

        [Fact]
        public void Can_invoke_operations_accross_boundaries()
        {
            var outputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(outputDirectory);
            try
            {
                var pathToEFAssembly = Path.Combine(outputDirectory, "EntityFramework.dll");
                File.Copy(new Uri(typeof(DbContext).Assembly.CodeBase).LocalPath, pathToEFAssembly);
                File.Copy(
                    new Uri(typeof(SqlProviderServices).Assembly.CodeBase).LocalPath,
                    Path.Combine(outputDirectory, "EntityFramework.SqlServer.dll"));
                var pathToConfig = Path.Combine(outputDirectory, "ConsoleApplication1.exe.config");
                File.WriteAllText(pathToConfig, Config);

                using (var compiler = new CSharpCodeProvider())
                {
                    var results = compiler.CompileAssemblyFromSource(
                        new CompilerParameters(
                            new[] { pathToEFAssembly },
                            Path.Combine(outputDirectory, "ConsoleApplication1.exe")),
                        Source);
                    if (results.Errors.HasErrors)
                    {
                        Debug.Fail(results.Errors.Cast<CompilerError>().FirstOrDefault(e => !e.IsWarning).ToString());
                    }

                    var domain = AppDomain.CreateDomain(
                        "BasicDesignTimeScenarios",
                        null,
                        new AppDomainSetup
                        {
                            ApplicationBase = Path.GetDirectoryName(results.PathToAssembly),
                            ConfigurationFile = pathToConfig,
                            ShadowCopyFiles = "true"
                        });
                    try
                    {
                        var executor = domain.CreateInstanceAndUnwrap(
                            typeof(Infrastructure.Design.Executor).Assembly.GetName().Name,
                            typeof(Infrastructure.Design.Executor).FullName,
                            false,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
                            null,
                            new[] { results.PathToAssembly, null },
                            null,
                            null);
                        var handler = new ResultHandler();
                        domain.CreateInstance(
                            typeof(Infrastructure.Design.Executor.GetProviderServices).Assembly.GetName().Name,
                            typeof(Infrastructure.Design.Executor.GetProviderServices).FullName,
                            false,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
                            null,
                            new[] { executor, handler, "System.Data.SqlClient", null },
                            null,
                            null);

                        Assert.Equal(typeof(SqlProviderServices).AssemblyQualifiedName, handler.Result);
                    }
                    finally
                    {
                        AppDomain.Unload(domain);
                    }
                }
            }
            finally
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
        }

        private class ResultHandler : HandlerBase, IResultHandler
        {
            private object _result;

            public object Result
            {
                get { return _result; }
            }

            void IResultHandler.SetResult(object value)
            {
                _result = value;
            }
        }
    }
}
