// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;

    /// <summary>
    ///     Represents a polymorphic typed column - either an entity or
    ///     a complex type.
    /// </summary>
    internal class SimplePolymorphicColumnMap : TypedColumnMap
    {
        private readonly SimpleColumnMap m_typeDiscriminator;
        private readonly Dictionary<object, TypedColumnMap> m_typedColumnMap;

        /// <summary>
        ///     Internal constructor
        /// </summary>
        /// <param name="type"> datatype of the column </param>
        /// <param name="name"> column name </param>
        /// <param name="baseTypeColumns"> base list of fields common to all types </param>
        /// <param name="typeDiscriminator"> column map for type discriminator column </param>
        /// <param name="typeChoices"> map from type discriminator value->columnMap </param>
        internal SimplePolymorphicColumnMap(
            TypeUsage type,
            string name,
            ColumnMap[] baseTypeColumns,
            SimpleColumnMap typeDiscriminator,
            Dictionary<object, TypedColumnMap> typeChoices)
            : base(type, name, baseTypeColumns)
        {
            DebugCheck.NotNull(typeDiscriminator);
            DebugCheck.NotNull(typeChoices);
            m_typedColumnMap = typeChoices;
            m_typeDiscriminator = typeDiscriminator;
        }

        /// <summary>
        ///     Get the type discriminator column
        /// </summary>
        internal SimpleColumnMap TypeDiscriminator
        {
            get { return m_typeDiscriminator; }
        }

        /// <summary>
        ///     Get the type mapping
        /// </summary>
        internal Dictionary<object, TypedColumnMap> TypeChoices
        {
            get { return m_typedColumnMap; }
        }

        /// <summary>
        ///     Visitor Design Pattern
        /// </summary>
        [DebuggerNonUserCode]
        internal override void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg)
        {
            visitor.Visit(this, arg);
        }

        /// <summary>
        ///     Visitor Design Pattern
        /// </summary>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType, TArgType>(
            ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg)
        {
            return visitor.Visit(this, arg);
        }

        /// <summary>
        ///     Debugging support
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            var separator = String.Empty;

            sb.AppendFormat(CultureInfo.InvariantCulture, "P{{TypeId={0}, ", TypeDiscriminator);
            foreach (var kv in TypeChoices)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}({1},{2})", separator, kv.Key, kv.Value);
                separator = ",";
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}
