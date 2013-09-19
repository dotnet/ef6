// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents a "simple" collection map.
    /// </summary>
    internal class SimpleCollectionColumnMap : CollectionColumnMap
    {
        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="type"> Column datatype </param>
        /// <param name="name"> column name </param>
        /// <param name="elementMap"> column map for the element of the collection </param>
        /// <param name="keys"> list of key columns </param>
        /// <param name="foreignKeys"> list of foreign key columns </param>
        internal SimpleCollectionColumnMap(
            TypeUsage type, string name,
            ColumnMap elementMap,
            SimpleColumnMap[] keys,
            SimpleColumnMap[] foreignKeys)
            : base(type, name, elementMap, keys, foreignKeys)
        {
        }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        [DebuggerNonUserCode]
        internal override void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg)
        {
            visitor.Visit(this, arg);
        }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType, TArgType>(
            ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg)
        {
            return visitor.Visit(this, arg);
        }
    }
}
