// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    /// <summary>
    ///     Class for reasoning about an entity framework runtime version.
    ///     Definitions of terms used in this class:
    ///     <list type="definition">
    ///         <item>
    ///             <term>Runtime version</term>
    ///             <description>The version of Entity Framework (e.g. 6.0.0.0, 5.0.0.0, 4.3.1.0, etc.)</description>
    ///         </item>
    ///         <item>
    ///             <term>Target .NET Framework version</term>
    ///             <description>The version of the project's target .NET Framework version (e.g. 4.5, 4.0, 3.5)</description>
    ///         </item>
    ///         <item>
    ///             <term>Schema version</term>
    ///             <description>The version of the edmx file's schema. (e.g. 3, 2, 1)</description>
    ///         </item>
    ///     </list>
    /// </summary>
    internal static class RuntimeVersion
    {
        public static readonly Version Version1 = new Version(3, 5, 0, 0);
        public static readonly Version Version4 = new Version(4, 0, 0, 0);
        public static readonly Version Version5Net40 = new Version(4, 4, 0, 0);
        public static readonly Version Version5Net45 = new Version(5, 0, 0, 0);
        public static readonly Version Version6 = new Version(6, 0, 0, 0);

        public static Version Latest
        {
            get { return Version6; }
        }

        public static Version Version5(Version targetNetFrameworkVersion)
        {
            Debug.Assert(targetNetFrameworkVersion != null, "targetNetFrameworkVersion is null.");

            return targetNetFrameworkVersion == NetFrameworkVersioningHelper.NetFrameworkVersion4
                       ? Version5Net40
                       : Version5Net45;
        }

        public static string GetName(Version entityFrameworkVersion, Version targetNetFrameworkVersion)
        {
            Debug.Assert(entityFrameworkVersion != null, "entityFrameworkVersion is null.");

            if (entityFrameworkVersion == Version5Net40
                || (targetNetFrameworkVersion != null
                    && entityFrameworkVersion == Version4
                    && targetNetFrameworkVersion >= NetFrameworkVersioningHelper.NetFrameworkVersion4_5))
            {
                entityFrameworkVersion = Version5Net45;
            }

            return string.Format(CultureInfo.InvariantCulture, 
                Resources.EntityFrameworkVersionName, entityFrameworkVersion.ToString(2));
        }

        public static bool RequiresLegacyProvider(Version entityFrameworkVersion)
        {
            Debug.Assert(entityFrameworkVersion != null, "entityFrameworkVersion is null.");

            return entityFrameworkVersion < Version6;
        }

        /// <summary>
        ///     Returns a target schema version for the <paramref name="entityFrameworkVersion" /> and
        ///     <paramref name="targetNetFrameworkVersion" />
        /// </summary>
        /// <param name="entityFrameworkVersion">The version of the referenced Entity Framework assembly.</param>
        /// <param name="targetNetFrameworkVersion">.NET Framework version tergeted by the project.</param>
        /// <returns>
        ///     A target schema version for the <paramref name="entityFrameworkVersion" /> and
        ///     <paramref name="targetNetFrameworkVersion" />
        /// </returns>
        /// <remarks>
        ///     Note that <paramref name="targetNetFrameworkVersion" /> is an auxiliary parameter used only when the target schema
        ///     version cannot be determined based solely on the <paramref name="entityFrameworkVersion" />. There are currently two cases when this can happen:
        ///     - <paramref name="entityFrameworkVersion" /> is null which means that neither EntityFramework.dll nor System.Data.Entity.dll
        ///     is referenced by the project in which case we returned the latest possible target schema version for the targeted .NET Framework
        ///     - <paramref name="entityFrameworkVersion" /> is 4.0.0.0 which means that the project references only System.Data.Entity.dll in which case
        ///     we need to use the target .NET Framework version to decide if this is EF4 or EF5 since the version of System.Data.Entity.dll is
        ///     4.0.0.0 on both .NET Framework 4 and .NET Framework 4.5 but EF4 supports only v2 schemas while EF5 supports v3 schemas.
        /// </remarks>
        public static Version GetTargetSchemaVersion(Version entityFrameworkVersion, Version targetNetFrameworkVersion)
        {
            Debug.Assert(
                entityFrameworkVersion == null || entityFrameworkVersion >= Version1,
                "entityFrameworkVersion is less than 3.5.");
            Debug.Assert(targetNetFrameworkVersion != null, "targetNetFrameworkVersion is null.");

            if (entityFrameworkVersion == null)
            {
                return GetLatestSchemaVersion(targetNetFrameworkVersion);
            }

            if (entityFrameworkVersion >= Version5Net45
                || (entityFrameworkVersion == Version4 &&
                    targetNetFrameworkVersion >= NetFrameworkVersioningHelper.NetFrameworkVersion4_5))
            {
                return EntityFrameworkVersion.Version3;
            }

            if (entityFrameworkVersion >= Version4)
            {
                return EntityFrameworkVersion.Version2;
            }

            Debug.Assert(entityFrameworkVersion == Version1);

            return EntityFrameworkVersion.Version1;
        }

        private static Version GetLatestSchemaVersion(Version targetNetFrameworkVersion)
        {
            Debug.Assert(targetNetFrameworkVersion != null, "targetNetFrameworkVersion is null.");

            if (targetNetFrameworkVersion >= NetFrameworkVersioningHelper.NetFrameworkVersion4)
            {
                return EntityFrameworkVersion.Version3;
            }

            Debug.Assert(
                targetNetFrameworkVersion >= NetFrameworkVersioningHelper.NetFrameworkVersion3_5,
                "Unexpected target .NET Framework Version");

            return EntityFrameworkVersion.Version1;
        }

        /// <summary>
        ///     Whether the given <paramref name="schemaVersion" /> is the latest version supported by the given
        ///     Entity Framework (i.e. EntityFramework.dll or System.Data.Entity.dll) <paramref name="assemblyVersion" />
        /// </summary>
        /// <param name="schemaVersion">Version of the EF schema.</param>
        /// <param name="assemblyVersion">
        ///     Version of an Entity Framework (i.e. EntityFramework.dll or System.Data.Entity.dll) assembly.
        /// </param>
        /// <param name="targetNetFrameworkVersion">
        ///     Targeted .NET Framework version. Used to distinguish EF5 from EF4 if the assembly version if 4.0.0.0.
        /// </param>
        /// <returns>
        ///     <c>True</c> if the given <paramref name="schemaVersion" /> is the latest version supported by the given
        ///     Entity Framework (i.e. EntityFramework.dll or System.Data.Entity.dll) <paramref name="assemblyVersion" />.
        ///     <c>False</c> otherwise.
        /// </returns>
        public static bool IsSchemaVersionLatestForAssemblyVersion(
            Version schemaVersion, Version assemblyVersion, Version targetNetFrameworkVersion)
        {
            Debug.Assert(EntityFrameworkVersion.IsValidVersion(schemaVersion), "Invalid schema version");
            Debug.Assert(assemblyVersion != null, "assemblyVersion != null");

            if (assemblyVersion == Version4
                && targetNetFrameworkVersion >= NetFrameworkVersioningHelper.NetFrameworkVersion4_5)
            {
                assemblyVersion = Version5Net45;
            }

            return
                (schemaVersion == EntityFrameworkVersion.Version3 && assemblyVersion >= Version5Net45) ||
                (schemaVersion == EntityFrameworkVersion.Version2 && (assemblyVersion >= Version4 && assemblyVersion <= Version5Net40)) ||
                (schemaVersion == EntityFrameworkVersion.Version1 && assemblyVersion == Version1);
        }

        /// <summary>
        ///     Returns schema version for a given .NET Framework version. Note it should only be used
        ///     if there are no references to any of the EF dll (i.e. System.Data.Entity.dll or
        ///     EntityFramework.dll in the project.
        /// </summary>
        /// <param name="netFrameworkVersion">Target .NET Framework version.</param>
        /// <returns>
        ///     EF Schema version as it was shipped with the <paramref name="netFrameworkVersion" />.
        /// </returns>
        /// <remarks>
        ///     Should be only used for non-Misc project that does not have references to either
        ///     System.Data.Entity.dll or EntityFramework.dll
        /// </remarks>
        public static Version GetSchemaVersionForNetFrameworkVersion(Version netFrameworkVersion)
        {
            Debug.Assert(netFrameworkVersion != null, "netFrameworkVersion != null");

            if (netFrameworkVersion >= NetFrameworkVersioningHelper.NetFrameworkVersion4_5)
            {
                return EntityFrameworkVersion.Version3;
            }

            if (netFrameworkVersion == NetFrameworkVersioningHelper.NetFrameworkVersion4)
            {
                return EntityFrameworkVersion.Version2;
            }

            Debug.Assert(
                netFrameworkVersion == NetFrameworkVersioningHelper.NetFrameworkVersion3_5,
                "Unexpected .NET Framework Version");

            return EntityFrameworkVersion.Version1;
        }
    }
}
