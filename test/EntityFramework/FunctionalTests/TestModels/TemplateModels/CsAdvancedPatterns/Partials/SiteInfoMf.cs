// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.ProductivityApi.TemplateModels.CsAdvancedPatterns
{
    internal partial class SiteInfoMf
    {
        public SiteInfoMf()
        {
        }

        public SiteInfoMf(int? zone, string environment)
        {
            Zone = zone;
            Environment = environment;
        }
    }
}