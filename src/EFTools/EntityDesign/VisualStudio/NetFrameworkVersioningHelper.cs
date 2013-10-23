// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Runtime.Versioning;
    using EnvDTE;

    internal class NetFrameworkVersioningHelper
    {
        private const string NetFrameworkMonikerIdentifier = ".NETFramework";

        public static readonly Version NetFrameworkVersion3_5 = new Version(3, 5);
        public static readonly Version NetFrameworkVersion4 = new Version(4, 0);
        public static readonly Version NetFrameworkVersion4_5 = new Version(4, 5);

        public static Version TargetNetFrameworkVersion(Project project, IServiceProvider serviceProvider)
        {
            var frameworkName = GetFrameworkName(project, serviceProvider);
            return frameworkName != null && frameworkName.Identifier == NetFrameworkMonikerIdentifier
                       ? frameworkName.Version
                       : null;
        }

        private static FrameworkName GetFrameworkName(Project project, IServiceProvider serviceProvider)
        {
            var targetFrameworkMoniker = VsUtils.GetTargetFrameworkMonikerForProject(project, serviceProvider);

            if (!string.IsNullOrWhiteSpace(targetFrameworkMoniker))
            {
                try
                {
                    return new FrameworkName(targetFrameworkMoniker);
                }
                catch (ArgumentException)
                {
                }
            }

            return null;
        }
    }
}
