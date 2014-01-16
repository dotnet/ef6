// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    internal enum ModelGenerationOption
    {
        GenerateFromDatabase = 0,
        EmptyModel = 1,
        GenerateDatabaseScript = 3,
        EmptyModelCodeFirst = 4,
        CodeFirstFromDatabase = 5
    }
}