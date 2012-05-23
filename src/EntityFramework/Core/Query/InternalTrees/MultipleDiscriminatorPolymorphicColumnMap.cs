namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Represents a function import column map.
    /// </summary>
    internal class MultipleDiscriminatorPolymorphicColumnMap : TypedColumnMap
    {
        private readonly SimpleColumnMap[] m_typeDiscriminators;
        private readonly Dictionary<EntityType, TypedColumnMap> m_typeChoices;
        private readonly Func<object[], EntityType> m_discriminate;

        /// <summary>
        /// Internal constructor
        /// </summary>
        internal MultipleDiscriminatorPolymorphicColumnMap(
            TypeUsage type,
            string name,
            ColumnMap[] baseTypeColumns,
            SimpleColumnMap[] typeDiscriminators,
            Dictionary<EntityType, TypedColumnMap> typeChoices,
            Func<object[], EntityType> discriminate)
            : base(type, name, baseTypeColumns)
        {
            Debug.Assert(typeDiscriminators != null, "Must specify type discriminator columns");
            Debug.Assert(typeChoices != null, "No type choices for polymorphic column");
            Debug.Assert(discriminate != null, "Must specify discriminate");

            m_typeDiscriminators = typeDiscriminators;
            m_typeChoices = typeChoices;
            m_discriminate = discriminate;
        }

        /// <summary>
        /// Get the type discriminator column
        /// </summary>
        internal SimpleColumnMap[] TypeDiscriminators
        {
            get { return m_typeDiscriminators; }
        }

        /// <summary>
        /// Get the type mapping
        /// </summary>
        internal Dictionary<EntityType, TypedColumnMap> TypeChoices
        {
            get { return m_typeChoices; }
        }

        /// <summary>
        /// Gets discriminator delegate
        /// </summary>
        internal Func<object[], EntityType> Discriminate
        {
            get { return m_discriminate; }
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
            var sb = new StringBuilder();
            var separator = String.Empty;

            sb.AppendFormat(CultureInfo.InvariantCulture, "P{{TypeId=<{0}>, ", StringUtil.ToCommaSeparatedString(TypeDiscriminators));
            foreach (var kv in TypeChoices)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}(<{1}>,{2})", separator, kv.Key, kv.Value);
                separator = ",";
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}
