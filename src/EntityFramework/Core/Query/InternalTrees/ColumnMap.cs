using md = System.Data.Entity.Core.Metadata.Edm;
using mp = System.Data.Entity.Core.Mapping;

// A ColumnMap is a data structure that maps columns from the C space to
// the corresponding columns from one or more underlying readers.
//
// ColumnMaps are used by the ResultAssembly phase to assemble results in the
// desired shape (as requested by the caller) from a set of underlying
// (usually) flat readers. ColumnMaps are produced as part of the PlanCompiler
// module of the bridge, and are consumed by the Execution phase of the bridge.
//
// * Simple (scalar) columns (and UDTs) are represented by a SimpleColumnMap
// * Record type columns are represented by a RecordColumnMap
// * A nominal type instance (that supports inheritance) is usually represented
//     by a PolymorphicColumnMap - this polymorphicColumnMap contains information
//     about the type discriminator (assumed to be a simple column), and a mapping
//     from type-discriminator value to the column map for the specific type
// * The specific type for nominal types is represented by ComplexTypeColumnMap
//     for complextype columns, and EntityColumnMap for entity type columns.
//     EntityColumnMaps additionally have an EntityIdentity that describes
//     the entity identity. The entity identity is logically just a set of keys
//     (and the column maps), plus a column map that helps to identify the
//     the appropriate entity set for the entity instance
// * Refs are represented by a RefColumnMap. The RefColumnMap simply contains an
//   EntityIdentity
// * Collections are represented by either a SimpleCollectionColumnMap or a
//     DiscriminatedCollectionColumnMap. Both of these contain a column map for the
//     element type, and an optional list of simple columns (the keys) that help
//     demarcate the elements of a specific collection instance.
//     The DiscriminatedCollectionColumnMap is used in scenarios when the containing
//     row has multiple collections, and the different collection properties must be
//     differentiated. This differentiation is achieved via a Discriminator column
//     (a simple column), and a Discriminator value. The value of the Discriminator
//     column is read and compared with the DiscriminatorValue stored in this map
//     to determine if we're dealing with the current collection.
//
// NOTE:
//  * Key columns are assumed to be SimpleColumns. There may be more than one key
//      column (applies to EntityColumnMap and *CollectionColumnMap)
//  * TypeDiscriminator and Discriminator columns are also considered to be
//      SimpleColumns. There are singleton columns.
//
// It is the responsibility of the PlanCompiler phase to produce the right column
// maps.
//
// The result of a query is always assumed to be a collection. The ColumnMap that we
// return as part of plan compilation refers to the element type of this collection
// - the element type is usually a structured type, but may also be a simple type
//   or another collection type. How does the DbRecord framework handle these cases?
//
//

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Diagnostics;

    /// <summary>
    /// Represents a column
    /// </summary>
    internal abstract class ColumnMap
    {
        private readonly md.TypeUsage m_type; // column datatype
        private string m_name; // name of the column

        /// <summary>
        /// Default Column Name; should not be set until CodeGen once we're done 
        /// with all our transformations that might give us a good name, but put 
        /// here for ease of finding it.
        /// </summary>
        internal const string DefaultColumnName = "Value";

        /// <summary>
        /// Simple constructor - just needs the name and type of the column
        /// </summary>
        /// <param name="type">column type</param>
        /// <param name="name">column name</param>
        internal ColumnMap(md.TypeUsage type, string name)
        {
            Debug.Assert(type != null, "Unspecified type");
            m_type = type;
            m_name = name;
        }

        /// <summary>
        /// Get the column's datatype
        /// </summary>
        internal md.TypeUsage Type
        {
            get { return m_type; }
        }

        /// <summary>
        /// Get the column name
        /// </summary>
        internal string Name
        {
            get { return m_name; }
            set
            {
                Debug.Assert(!String.IsNullOrEmpty(value), "invalid name?");
                m_name = value;
            }
        }

        /// <summary>
        /// Returns whether the column already has a name;
        /// </summary>
        internal bool IsNamed
        {
            get { return m_name != null; }
        }

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        internal abstract void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg);

        /// <summary>
        /// Visitor Design Pattern
        /// </summary>
        /// <typeparam name="TResultType"></typeparam>
        /// <typeparam name="TArgType"></typeparam>
        /// <param name="visitor"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        [DebuggerNonUserCode]
        internal abstract TResultType Accept<TResultType, TArgType>(
            ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg);
    }
}
