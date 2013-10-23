// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.Metadata
{
    using System;
    using System.Data.Entity.Core.Metadata.Edm;

    internal static class EdmItemCollectionExtensions
    {
        public static Version CsdlVersion(this EdmItemCollection edmItemCollection)
        {
            return EntityFrameworkVersion.DoubleToVersion(edmItemCollection.EdmVersion);
        }
    }
}
