// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.ProductivityApi.TemplateModels.CsAdvancedPatterns
{
    internal partial class AdvancedPatternsModelFirstContext
    {
        public AdvancedPatternsModelFirstContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            Configuration.LazyLoadingEnabled = false;
        }
    }
}
