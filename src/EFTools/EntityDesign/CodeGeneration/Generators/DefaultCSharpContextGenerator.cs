// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;

    internal partial class DefaultCSharpContextGenerator : IContextGenerator
    {
        public string Generate(DbModel model, string codeNamespace, string contextClassName, string connectionStringName)
        {
            Debug.Assert(model != null, "model is null.");
            Debug.Assert(!string.IsNullOrEmpty(codeNamespace), "codeNamespace is null or empty.");
            Debug.Assert(!string.IsNullOrEmpty(contextClassName), "contextClassName is null or empty.");
            Debug.Assert(!string.IsNullOrEmpty(connectionStringName), "connectionStringName is null or empty.");

            Session = new Dictionary<string, object>
                    {
                        { "Model", model },
                        { "Namespace", codeNamespace },
                        { "ContextClassName", contextClassName },
                        { "ConnectionStringName", connectionStringName }
                    };
            Initialize();

            return TransformText();
        }
    }
}
