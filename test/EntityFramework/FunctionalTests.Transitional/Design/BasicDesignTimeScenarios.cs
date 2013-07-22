// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Design
{
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
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
    public class BasicDesignTimeScenarios : TestBase, IDisposable
    {
        private const string Source = @"
namespace ConsoleApplication1
{
    using System.Data.Entity;
    using System.Data.Entity.SqlServer;

    internal class MyConfiguration : DbConfiguration
    {
        public MyConfiguration()
        {
            ProviderServices(""My.New.SqlClient"", SqlProviderServices.Instance);
        }
    }
    
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

        private readonly string _outputDirectory;
        private readonly string _pathToAssembly;
        private readonly AppDomain _domain;

        public BasicDesignTimeScenarios()
        {
            _outputDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_outputDirectory);

            var pathToEFAssembly = Path.Combine(_outputDirectory, "EntityFramework.dll");
            File.Copy(new Uri(typeof(DbContext).Assembly.CodeBase).LocalPath, pathToEFAssembly);
            var pathToEFSqlAssembly = Path.Combine(_outputDirectory, "EntityFramework.SqlServer.dll");
            File.Copy(new Uri(typeof(SqlProviderServices).Assembly.CodeBase).LocalPath, pathToEFSqlAssembly);
            var pathToConfig = Path.Combine(_outputDirectory, "ConsoleApplication1.exe.config");
            File.WriteAllText(pathToConfig, Config);

            using (var compiler = new CSharpCodeProvider())
            {
                var results = compiler.CompileAssemblyFromSource(
                    new CompilerParameters(
                        new[] { pathToEFAssembly, pathToEFSqlAssembly },
                        Path.Combine(_outputDirectory, "ConsoleApplication1.exe")),
                    Source);
                if (results.Errors.HasErrors)
                {
                    Debug.Fail(results.Errors.Cast<CompilerError>().FirstOrDefault(e => !e.IsWarning).ToString());
                }

                _pathToAssembly = results.PathToAssembly;
                _domain = AppDomain.CreateDomain(
                    "BasicDesignTimeScenarios",
                    null,
                    new AppDomainSetup
                    {
                        ApplicationBase = _outputDirectory,
                        ConfigurationFile = pathToConfig,
                        ShadowCopyFiles = "true"
                    });
            }
        }

        [Fact]
        public void Can_invoke_operations_accross_boundaries()
        {
            var handler = new ResultHandler();
            Invoke<Infrastructure.Design.Executor.GetProviderServices>(
                CreateExecutor(),
                handler,
                "System.Data.SqlClient",
                null);

            Assert.Equal(typeof(SqlProviderServices).AssemblyQualifiedName, handler.Result);
        }

        [Fact]
        public void Uses_configuration_from_specified_assembly()
        {
            var handler = new ResultHandler();
            Invoke<Infrastructure.Design.Executor.GetProviderServices>(
                CreateExecutor(),
                handler,
                "My.New.SqlClient",
                null);

            Assert.Equal(typeof(SqlProviderServices).AssemblyQualifiedName, handler.Result);
        }

        public void Dispose()
        {
            AppDomain.Unload(_domain);
            Directory.Delete(_outputDirectory, recursive: true);
        }

        private object CreateExecutor()
        {
            return _domain.CreateInstanceAndUnwrap(
                typeof(Infrastructure.Design.Executor).Assembly.GetName().Name,
                typeof(Infrastructure.Design.Executor).FullName,
                false,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
                null,
                new[] { _pathToAssembly, null },
                null,
                null);
        }

        private void Invoke<TOperation>(object executor, object handler, params object[] args)
        {
            var realArgs = new List<object> { executor, handler };
            realArgs.AddRange(args);

            _domain.CreateInstance(
                typeof(TOperation).Assembly.GetName().Name,
                typeof(TOperation).FullName,
                false,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance,
                null,
                realArgs.ToArray(),
                null,
                null);
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
