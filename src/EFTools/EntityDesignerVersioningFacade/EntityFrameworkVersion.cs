// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    internal static class EntityFrameworkVersion
    {
        public static readonly Version Version1 = new Version(1, 0, 0, 0);
        public static readonly Version Version2 = new Version(2, 0, 0, 0);
        public static readonly Version Version3 = new Version(3, 0, 0, 0);

        public static IEnumerable<Version> GetAllVersions()
        {
            yield return Version3;
            yield return Version2;
            yield return Version1;
        }

        public static bool IsValidVersion(Version version)
        {
            return version != null && GetAllVersions().Contains(version);
        }

        internal static Version DoubleToVersion(double version)
        {
            var v = Version.Parse(version.ToString("F1", CultureInfo.InvariantCulture));
            return new Version(v.Major, v.Minor, 0, 0);
        }

        internal static double VersionToDouble(Version version)
        {
            Debug.Assert(IsValidVersion(version), "invalid EF version");

            return double.Parse(version.ToString(2), CultureInfo.InvariantCulture);
        }

        public static Version Latest
        {
            get { return Version3; }
        }
    }
}
