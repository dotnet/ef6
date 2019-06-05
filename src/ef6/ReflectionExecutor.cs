// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace System.Data.Entity.Tools
{
    internal class ReflectionExecutor : ExecutorBase
    {
        private readonly string _applicationBase;
        private readonly Assembly _assembly;
        private readonly object _executor;
        private readonly Type _resultHandlerType;

        // TODO: Use configurationFile. Blocked by dotnet/corefx#32095
        public ReflectionExecutor(
            string assembly,
            string dataDirectory,
            string configurationFile,
            string rootNamespace,
            string language)
        {
            _applicationBase = Path.GetFullPath(
                    Path.Combine(Directory.GetCurrentDirectory(), Path.GetDirectoryName(assembly)));

            if (dataDirectory != null)
            {
                AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);
            }

            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

            _assembly = Assembly.Load(new AssemblyName { Name = "EntityFramework" });
            var reportHandlerType = _assembly.GetType(
                "System.Data.Entity.Infrastructure.Design.ReportHandler",
                throwOnError: true,
                ignoreCase: false);

            var reportHandler = Activator.CreateInstance(
                reportHandlerType,
                (Action<string>)Reporter.WriteError,
                (Action<string>)Reporter.WriteWarning,
                (Action<string>)Reporter.WriteInformation,
                (Action<string>)Reporter.WriteVerbose);

            _executor = Activator.CreateInstance(
                _assembly.GetType(ExecutorTypeName, throwOnError: true, ignoreCase: false),
                Path.GetFileName(assembly),
                new Dictionary<string, object>
                {
                    { "reportHandler", reportHandler },
                    { "language", language },
                    { "rootNamespace", rootNamespace }
                });

            _resultHandlerType = _assembly.GetType(
                "System.Data.Entity.Infrastructure.Design.ResultHandler",
                throwOnError: true,
                ignoreCase: false);
        }

        protected override dynamic CreateResultHandler()
            => Activator.CreateInstance(_resultHandlerType);

        protected override void Execute(string operation, object resultHandler, IDictionary args)
            => Activator.CreateInstance(
                _assembly.GetType(ExecutorTypeName + "+" + operation, throwOnError: true, ignoreCase: false),
                _executor,
                resultHandler,
                args);

        private Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);

            foreach (var extension in new[] { ".dll", ".exe" })
            {
                var path = Path.Combine(_applicationBase, assemblyName.Name + extension);
                if (File.Exists(path))
                {
                    try
                    {
                        return Assembly.LoadFrom(path);
                    }
                    catch
                    {
                    }
                }
            }

            return null;
        }
    }
}
