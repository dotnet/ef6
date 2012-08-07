// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    ///     Represents a column map for a specific complextype
    /// </summary>
    internal class ComplexTypeColumnMap : TypedColumnMap
    {
        private readonly SimpleColumnMap m_nullSentinel;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="type"> column Datatype </param>
        /// <param name="name"> column name </param>
        /// <param name="properties"> list of properties </param>
        internal ComplexTypeColumnMap(TypeUsage type, string name, ColumnMap[] properties, SimpleColumnMap nullSentinel)
            : base(type, name, properties)
        {
            m_nullSentinel = nullSentinel;
        }

        /// <summary>
        ///     Get the type Nullability column
        /// </summary>
        internal override SimpleColumnMap NullSentinel
        {
            get { return m_nullSentinel; }
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

        /// <summary>
        ///     Debugging support
        /// </summary>
        /// <returns> </returns>
        public override string ToString()
        {
            var str = String.Format(CultureInfo.InvariantCulture, "C{0}", base.ToString());
            return str;
        }
    }
}
