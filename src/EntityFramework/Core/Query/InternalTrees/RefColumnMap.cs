// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    ///     A column map that represents a ref column.
    /// </summary>
    internal class RefColumnMap : ColumnMap
    {
        private readonly EntityIdentity m_entityIdentity;

        /// <summary>
        ///     Constructor for a ref column
        /// </summary>
        /// <param name="type"> column datatype </param>
        /// <param name="name"> column name </param>
        /// <param name="entityIdentity"> identity information for this entity </param>
        internal RefColumnMap(
            TypeUsage type, string name,
            EntityIdentity entityIdentity)
            : base(type, name)
        {
            DebugCheck.NotNull(entityIdentity);
            m_entityIdentity = entityIdentity;
        }

        /// <summary>
        ///     Get the entity identity information for this ref
        /// </summary>
        internal EntityIdentity EntityIdentity
        {
            get { return m_entityIdentity; }
        }

        /// <summary>
        ///     Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TArgType"> </typeparam>
        /// <param name="visitor"> </param>
        /// <param name="arg"> </param>
        [DebuggerNonUserCode]
        internal override void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg)
        {
            visitor.Visit(this, arg);
        }

        /// <summary>
        ///     Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TResultType"> </typeparam>
        /// <typeparam name="TArgType"> </typeparam>
        /// <param name="visitor"> </param>
        /// <param name="arg"> </param>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType, TArgType>(
            ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg)
        {
            return visitor.Visit(this, arg);
        }
    }
}
