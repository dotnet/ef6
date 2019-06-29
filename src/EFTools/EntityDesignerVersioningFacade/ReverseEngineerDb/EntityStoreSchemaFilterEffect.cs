// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb
{
    /// <summary>
    ///     The effect that the filter entry should have on the results
    ///     When a database object matches the pattern for both an allow and exclude EntityStoreSchemaFilterEntry,
    ///     the database object will be excluded.
    /// </summary>
    internal enum EntityStoreSchemaFilterEffect
    {
        /// <summary>Allow the entries that match the specified pattern.</summary>
        Allow = 0,

        /// <summary>Exclude the entries that match the specified pattern.</summary>
        Exclude = 1,
    }
}
