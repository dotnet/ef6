namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents a record (an untyped structured column)
    /// </summary>
    internal class RecordColumnMap : StructuredColumnMap
    {
        private readonly SimpleColumnMap m_nullSentinel;

        /// <summary>
        /// Constructor for a record column map
        /// </summary>
        /// <param name="type">Datatype of this column</param>
        /// <param name="name">column name</param>
        /// <param name="properties">List of ColumnMaps - one for each property</param>
        internal RecordColumnMap(TypeUsage type, string name, ColumnMap[] properties, SimpleColumnMap nullSentinel)
            : base(type, name, properties)
        {
            m_nullSentinel = nullSentinel;
        }

        /// <summary>
        /// Get the type Nullability column
        /// </summary>
        internal override SimpleColumnMap NullSentinel
        {
            get { return m_nullSentinel; }
        }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg)
        {
            visitor.Visit(this, arg);
        }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TResultType"></typeparam>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType, TArgType>(
            ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg)
        {
            return visitor.Visit(this, arg);
        }
    }
}