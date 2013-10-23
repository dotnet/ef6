// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    /// <summary>
    ///     Represents the relationship between an EntityType and its ancestors as regards
    ///     the mapping strategy used. See ModelHelper.DetermineCurrentInheritanceStrategy()
    ///     for the full rules on determining this.
    /// </summary>
    internal enum InheritanceMappingStrategy
    {
        NoInheritance, // EntityType has no ancestors
        TablePerHierarchy,
        TablePerType,
        Mixed // both an IsTypeOf ETM and a non-IsTypeOf ETM for a single EntityType exist (only available through hand-editing the MSL)
    }
}
