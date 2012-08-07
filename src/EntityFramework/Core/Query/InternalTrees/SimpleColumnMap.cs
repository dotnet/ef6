// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    ///     Base class for simple column maps; can be either a VarRefColumnMap or 
    ///     ScalarColumnMap; the former is used pretty much throughout the PlanCompiler,
    ///     while the latter will only be used once we generate the final Plan.
    /// </summary>
    internal abstract class SimpleColumnMap : ColumnMap
    {
        /// <summary>
        ///     Basic constructor
        /// </summary>
        /// <param name="type"> datatype for this column </param>
        /// <param name="name"> column name </param>
        internal SimpleColumnMap(TypeUsage type, string name)
            : base(type, name)
        {
        }
    }
}
