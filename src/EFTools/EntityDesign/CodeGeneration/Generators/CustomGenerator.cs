// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.TextTemplating.VSHost;

    internal class CustomGenerator : IContextGenerator, IEntityTypeGenerator
    {
        private readonly TextTemplatingHost _host;
        private readonly string _templatePath;

        public CustomGenerator(string templatePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(templatePath), "templatePath is null or empty.");

            _templatePath = templatePath;
            _host = new TextTemplatingHost();
        }

        public string Generate(DbModel model, string codeNamespace, string contextClassName, string connectionStringName)
        {
            Debug.Assert(model != null, "model is null.");
            Debug.Assert(!string.IsNullOrWhiteSpace(codeNamespace), "invalid namespace");
            Debug.Assert(!string.IsNullOrWhiteSpace(contextClassName), "contextClassName");
            Debug.Assert(!string.IsNullOrWhiteSpace(connectionStringName), "connectionStringName");

            _host.Session = _host.CreateSession();
            _host.Session.Add("Model", model);
            _host.Session.Add("Namespace", codeNamespace);
            _host.Session.Add("ContextClassName", contextClassName);
            _host.Session.Add("ConnectionStringName", connectionStringName);

            return ProcessTemplate();
        }

        public string Generate(EntitySet entitySet, DbModel model, string codeNamespace)
        {
            Debug.Assert(entitySet != null, "entitySet is null.");
            Debug.Assert(model != null, "model is null.");

            _host.Session = _host.CreateSession();
            _host.Session.Add("EntitySet", entitySet);
            _host.Session.Add("Model", model);
            _host.Session.Add("Namespace", codeNamespace);

            return ProcessTemplate();
        }

        private string ProcessTemplate()
        {
            var callback = new Callback();

            var result = _host.ProcessTemplate(_templatePath, File.ReadAllText(_templatePath), callback);

            if (callback.Errors.Any())
            {
                throw new InvalidOperationException(callback.Errors.First());
            }

            return result;
        }

        private class Callback : ITextTemplatingCallback
        {
            private readonly ICollection<string> _errors = new List<string>();

            public IEnumerable<string> Errors
            {
                get { return _errors; }
            }

            public void ErrorCallback(bool warning, string message, int line, int column)
            {
                Debug.Assert(!string.IsNullOrEmpty(message), "message is null or empty.");

                if (!warning)
                {
                    _errors.Add(message);
                }
            }

            public void SetFileExtension(string extension)
            {
            }

            public void SetOutputEncoding(Encoding encoding, bool fromOutputDirective)
            {
            }
        }
    }
}
