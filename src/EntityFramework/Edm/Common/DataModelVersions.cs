// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Common
{
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Csdl;
    using System.Data.Entity.Edm.Parsing.Xml.Internal.Ssdl;
    using System.Diagnostics.Contracts;

    internal static class DataModelVersions
    {
        public const double Version1 = 1.0;
        public const double Version1_1 = 1.1;
        public const double Version2 = 2.0;
        public const double Version3 = 3.0;

        public static string GetCsdlNamespace(double edmVersion)
        {
            if (edmVersion == Version1)
            {
                return CsdlConstants.Version1Namespace;
            }

            if (edmVersion == Version1_1)
            {
                return CsdlConstants.Version1_1Namespace;
            }

            if (edmVersion == Version2)
            {
                return CsdlConstants.Version2Namespace;
            }

            Contract.Assert(edmVersion == Version3, "Added a new version?");

            return CsdlConstants.Version3Namespace;
        }

        public static string GetSsdlNamespace(double edmVersion)
{
            if (edmVersion == Version1)
            {
                return SsdlConstants.Version1Namespace;
            }

            if (edmVersion == Version2)
            {
                return SsdlConstants.Version2Namespace;
            }

            Contract.Assert(edmVersion == Version3, "Added a new version?");

            return SsdlConstants.Version3Namespace;
        }
    }
}
