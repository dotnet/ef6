// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class CsdlVersion
    {
        public static readonly Version Version1 = new Version(1, 0, 0, 0);
        public static readonly Version Version1_1 = new Version(1, 1, 0, 0);
        public static readonly Version Version2 = new Version(2, 0, 0, 0);
        public static readonly Version Version3 = new Version(3, 0, 0, 0);

        public static IEnumerable<Version> GetAllVersions()
        {
            yield return Version3;
            yield return Version2;
            yield return Version1;
            yield return Version1_1;
        }

        public static bool IsValidVersion(Version version)
        {
            return version != null && GetAllVersions().Contains(version);
        }
    }
}
