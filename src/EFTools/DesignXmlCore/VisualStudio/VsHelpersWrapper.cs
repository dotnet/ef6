// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;

    internal class VsHelpersWrapper : IVsHelpers
    {
        public object GetDocData(IServiceProvider site, string documentPath)
        {
            return VSHelpers.GetDocData(site, documentPath);
        }
    }
}
