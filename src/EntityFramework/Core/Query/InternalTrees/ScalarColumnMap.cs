// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// Column map for a scalar column - maps 1-1 with a column from a 
    /// row of the underlying reader
    /// </summary>
    internal class ScalarColumnMap : SimpleColumnMap
    {
        private readonly int m_commandId;
        private readonly int m_columnPos;

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="type">datatype for this column</param>
        /// <param name="name">column name</param>
        /// <param name="commandId">Underlying command to locate this column</param>
        /// <param name="columnPos">Position in underlying reader</param>
        internal ScalarColumnMap(TypeUsage type, string name, int commandId, int columnPos)
            : base(type, name)
        {
            Debug.Assert(commandId >= 0, "invalid command id");
            Debug.Assert(columnPos >= 0, "invalid column position");
            m_commandId = commandId;
            m_columnPos = columnPos;
        }

        /// <summary>
        /// The command (reader, really) to get this column value from
        /// </summary>
        internal int CommandId
        {
            get { return m_commandId; }
        }

        /// <summary>
        /// Column position within the reader of the command
        /// </summary>
        internal int ColumnPos
        {
            get { return m_columnPos; }
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

        /// <summary>
        /// Debugging support
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "S({0},{1})", CommandId, ColumnPos);
        }
    }
}
