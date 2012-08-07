// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Extensions
{
    using System.Diagnostics.Contracts;
    using EnvDTE;

    /// <summary>
    ///     Extension methods for the Visual Studio ProjectItem interface.
    /// </summary>
    internal static class ProjectItemExtensions
    {
        /// <summary>
        ///     Returns true if the project item is named either "app.config" or "web.config".
        /// </summary>
        public static bool IsConfig(this ProjectItem item)
        {
            Contract.Requires(item != null);

            return IsNamed(item, "app.config") || IsNamed(item, "web.config");
        }

        /// <summary>
        ///     Returns true if the project item has the given name, with case ignored.
        /// </summary>
        public static bool IsNamed(this ProjectItem item, string name)
        {
            Contract.Requires(item != null);

            return item.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
