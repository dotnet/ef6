// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal class InitialModelContentsFactory : IInitialModelContentsFactory
    {
        public string GetInitialModelContents(Version targetSchemaVersion)
        {
            Debug.Assert(
                EntityFrameworkVersion.IsValidVersion(targetSchemaVersion),
                "invalid schema version");

            return EdmUtils.CreateEdmxString(targetSchemaVersion, string.Empty, string.Empty, string.Empty);
        }
    }
}
