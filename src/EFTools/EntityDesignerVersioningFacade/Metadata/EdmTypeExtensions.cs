// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.Metadata
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Reflection;

    internal static class EdmTypeExtensions
    {
        public static DataSpace GetDataSpace(this EdmType edmType)
        {
            Debug.Assert(edmType != null, "edmType != null");

            return
                (DataSpace)typeof(EdmType)
                               .GetProperty("DataSpace", BindingFlags.Instance | BindingFlags.NonPublic)
                               .GetValue(edmType);
        }
    }
}
