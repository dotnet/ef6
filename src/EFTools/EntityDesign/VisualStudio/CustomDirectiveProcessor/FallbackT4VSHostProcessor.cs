// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Directives
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TextTemplating;

    // <summary>
    //     This is an empty directive processor only meant to support old versions of the T4
    //     processor that did not include the T4VSHost custom directive processor that
    //     we use in the ttinclude files.
    // </summary>
    internal class FallbackT4VSHostProcessor : DirectiveProcessor
    {
        public override void Initialize(ITextTemplatingEngineHost host)
        {
            //no-op
        }

        public override bool IsDirectiveSupported(string directiveName)
        {
            return true;
        }

        public override void ProcessDirective(string directiveName, IDictionary<string, string> arguments)
        {
            //no-op
        }

        public override void FinishProcessingRun()
        {
            //no-op
        }

        public override string GetClassCodeForProcessingRun()
        {
            return String.Empty;
        }

        public override string GetPreInitializationCodeForProcessingRun()
        {
            return String.Empty;
        }

        public override string GetPostInitializationCodeForProcessingRun()
        {
            return String.Empty;
        }

        public override string[] GetReferencesForProcessingRun()
        {
            return null;
        }

        public override string[] GetImportsForProcessingRun()
        {
            return null;
        }
    }
}
