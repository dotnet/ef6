// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

# if NET40 || NET45

using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Design;
using System.IO;
using System.Reflection;

namespace System.Data.Entity.Tools
{
    internal class AppDomainExecutor : ExecutorBase
    {
        private readonly AppDomain _domain;
        private readonly object _executor;

        public AppDomainExecutor(
            string assembly,
            string dataDirectory,
            string configurationFile,
            string rootNamespace,
            string language)
        {
            var friendlyName = "MigrationsToolingFacade" + Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            var info = new AppDomainSetup
            {
                ApplicationBase = Path.GetFullPath(
                    Path.Combine(Directory.GetCurrentDirectory(), Path.GetDirectoryName(assembly)))
            };

            if (string.IsNullOrEmpty(configurationFile))
            {
                configurationFile = Path.GetFileNameWithoutExtension(assembly) + ".config";
            }
            if (File.Exists(configurationFile))
            {
                info.ConfigurationFile = configurationFile;
            }

            _domain = AppDomain.CreateDomain(friendlyName, securityInfo: null, info);

            if (dataDirectory != null)
            {
                _domain.SetData("DataDirectory", dataDirectory);
            }

            var reportHandler = new ReportHandler(
                Reporter.WriteError,
                Reporter.WriteWarning,
                Reporter.WriteInformation,
                Reporter.WriteVerbose);

            _executor = _domain.CreateInstanceAndUnwrap(
                "EntityFramework",
                ExecutorTypeName,
                ignoreCase: false,
                BindingFlags.Default,
                binder: null,
                args: new object[]
                {
                    Path.GetFileName(assembly),
                    new Dictionary<string, object>
                    {
                        { "reportHandler", reportHandler },
                        { "language", language },
                        { "rootNamespace", rootNamespace }
                    }
                },
                culture: null,
                activationAttributes: null);
        }

        protected override dynamic CreateResultHandler()
            => new ResultHandler();

        protected override void Execute(string operation, object resultHandler, IDictionary args)
            => _domain.CreateInstance(
                "EntityFramework",
                ExecutorTypeName + "+" + operation,
                ignoreCase: false,
                BindingFlags.Default,
                binder: null,
                new[] { _executor, resultHandler, args },
                culture: null,
                activationAttributes: null);
    }
}

#elif !NETCOREAPP
#error Unexpected target framework
#endif
