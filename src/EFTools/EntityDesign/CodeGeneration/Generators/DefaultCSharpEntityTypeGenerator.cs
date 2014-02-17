// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;

    internal partial class DefaultCSharpEntityTypeGenerator : IEntityTypeGenerator
    {
        public string Generate(EntitySet entitySet, DbModel model, string codeNamespace)
        {
            Debug.Assert(entitySet != null, "entitySet is null.");
            Debug.Assert(model != null, "model is null.");
            Debug.Assert(!string.IsNullOrEmpty(codeNamespace), "codeNamespace is null or empty.");

            Session = new Dictionary<string, object>
                    {
                        { "EntitySet", entitySet },
                        { "Model", model },
                        { "Namespace", codeNamespace }
                    };
            Initialize();

            return TransformText();
        }
    }
}
