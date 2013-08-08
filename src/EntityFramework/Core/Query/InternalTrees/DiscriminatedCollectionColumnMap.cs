// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// Represents a "discriminated" collection column.
    /// This represents a scenario when multiple collections are represented
    /// at the same level of the container row, and there is a need to distinguish
    /// between these collections
    /// </summary>
    internal class DiscriminatedCollectionColumnMap : CollectionColumnMap
    {
        private readonly SimpleColumnMap m_discriminator;
        private readonly object m_discriminatorValue;

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="type"> Column datatype </param>
        /// <param name="name"> column name </param>
        /// <param name="elementMap"> column map for collection element </param>
        /// <param name="keys"> Keys for the collection </param>
        /// <param name="foreignKeys"> Foreign keys for the collection </param>
        /// <param name="discriminator"> Discriminator column map </param>
        /// <param name="discriminatorValue"> Discriminator value </param>
        internal DiscriminatedCollectionColumnMap(
            TypeUsage type, string name,
            ColumnMap elementMap,
            SimpleColumnMap[] keys,
            SimpleColumnMap[] foreignKeys,
            SimpleColumnMap discriminator,
            object discriminatorValue)
            : base(type, name, elementMap, keys, foreignKeys)
        {
            DebugCheck.NotNull(discriminator);
            DebugCheck.NotNull(discriminatorValue);
            m_discriminator = discriminator;
            m_discriminatorValue = discriminatorValue;
        }

        /// <summary>
        /// Get the column that describes the discriminator
        /// </summary>
        internal SimpleColumnMap Discriminator
        {
            get { return m_discriminator; }
        }

        /// <summary>
        /// Get the discriminator value
        /// </summary>
        internal object DiscriminatorValue
        {
            get { return m_discriminatorValue; }
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

        /// <summary>
        /// Debugging support
        /// </summary>
        public override string ToString()
        {
            var str = String.Format(CultureInfo.InvariantCulture, "M{{{0}}}", Element);
            return str;
        }
    }
}
