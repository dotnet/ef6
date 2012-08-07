// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Edm.Common;

    internal static class DbModelBuilderVersionExtensions
    {
        public static double GetEdmVersion(this DbModelBuilderVersion modelBuilderVersion)
        {
            switch (modelBuilderVersion)
            {
                case DbModelBuilderVersion.V4_1:
                    return DataModelVersions.Version2;
                case DbModelBuilderVersion.V5_0:
                case DbModelBuilderVersion.Latest:
                    return DataModelVersions.Version3;
                default:
                    throw new ArgumentOutOfRangeException("modelBuilderVersion");
            }
        }
    }
}
