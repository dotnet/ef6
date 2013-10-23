// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    internal abstract class ModelBuilderEngineHostContext
    {
        internal abstract void LogMessage(string s);
        internal abstract void DispatchToModelGenerationExtensions();
    }
}
