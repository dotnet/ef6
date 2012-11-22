// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Core.Metadata.Edm;

    internal static class DbModelBuilderVersionExtensions
    {
        public static double GetEdmVersion(this DbModelBuilderVersion modelBuilderVersion)
        {
            switch (modelBuilderVersion)
            {
                case DbModelBuilderVersion.V4_1:
                    return XmlConstants.EdmVersionForV2;
                case DbModelBuilderVersion.V5_0:
                case DbModelBuilderVersion.Latest:
                    return XmlConstants.EdmVersionForV3;
                default:
                    throw new ArgumentOutOfRangeException("modelBuilderVersion");
            }
        }
    }
}
