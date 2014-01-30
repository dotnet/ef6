// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.IO;
    using Microsoft.VisualStudio.TextTemplating;
    using Microsoft.VisualStudio.TextTemplating.VSHost;

    internal class CustomGenerator : IContextGenerator, IEntityTypeGenerator
    {
        private readonly ITextTemplating _textTemplating;
        private readonly string _templatePath;

        public CustomGenerator(IServiceProvider serviceProvider, string templatePath)
        {
            Debug.Assert(serviceProvider != null, "serviceProvider is null.");
            Debug.Assert(!string.IsNullOrEmpty(templatePath), "templatePath is null or empty.");

            _textTemplating = (ITextTemplating)serviceProvider.GetService(typeof(STextTemplating));
            _templatePath = templatePath;
        }

        public string Generate(DbModel model, string codeNamespace, string contextClassName, string connectionStringName)
        {
            Debug.Assert(model != null, "model is null.");
            Debug.Assert(!string.IsNullOrWhiteSpace(codeNamespace), "invalid namespace");
            Debug.Assert(!string.IsNullOrWhiteSpace(contextClassName), "contextClassName");
            Debug.Assert(!string.IsNullOrWhiteSpace(connectionStringName), "connectionStringName");

            var sessionHost = (ITextTemplatingSessionHost)_textTemplating;
            sessionHost.Session = sessionHost.CreateSession();
            sessionHost.Session.Add("Model", model);
            sessionHost.Session.Add("Namespace", codeNamespace);
            sessionHost.Session.Add("ContextClassName", contextClassName);
            sessionHost.Session.Add("ConnectionStringName", connectionStringName);

            return _textTemplating.ProcessTemplate(_templatePath, File.ReadAllText(_templatePath));
        }

        public string Generate(EntitySet entitySet, DbModel model, string codeNamespace)
        {
            Debug.Assert(entitySet != null, "entitySet is null.");
            Debug.Assert(model != null, "model is null.");

            var sessionHost = (ITextTemplatingSessionHost)_textTemplating;
            sessionHost.Session = sessionHost.CreateSession();
            sessionHost.Session.Add("EntitySet", entitySet);
            sessionHost.Session.Add("Model", model);
            sessionHost.Session.Add("Namespace", codeNamespace);

            return _textTemplating.ProcessTemplate(_templatePath, File.ReadAllText(_templatePath));
        }
    }
}
