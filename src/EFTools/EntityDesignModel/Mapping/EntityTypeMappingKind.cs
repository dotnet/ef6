// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Mapping
{
    internal enum EntityTypeMappingKind
    {
        /// <summary>
        ///     The ETM that just references the type name
        /// </summary>
        Default,

        /// <summary>
        ///     The ETM that uses IsTypeOf(type name)
        /// </summary>
        IsTypeOf,

        /// <summary>
        ///     The ETM that holds any function mappings
        /// </summary>
        Function,

        /// <summary>
        ///     Derive the kind based on how the types are referenced
        /// </summary>
        Derive
    };
}
