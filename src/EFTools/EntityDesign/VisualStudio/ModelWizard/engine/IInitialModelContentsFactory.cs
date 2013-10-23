// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;

    internal interface IInitialModelContentsFactory
    {
        string GetInitialModelContents(Version targetSchemaVersion);
    }
}
