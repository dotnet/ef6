// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// A VarRefColumnMap is our intermediate representation of a ColumnMap.
    /// Eventually, this gets translated into a regular ColumnMap - during the CodeGen phase
    /// </summary>
    internal class VarRefColumnMap : SimpleColumnMap
    {
        #region Public Methods

        /// <summary>
        /// Get the Var that produces this column's value
        /// </summary>
        internal Var Var
        {
            get { return m_var; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Simple constructor
        /// </summary>
        /// <param name="type">datatype of this Var</param>
        /// <param name="name">the name of the column</param>
        /// <param name="v">the var producing the value for this column</param>
        internal VarRefColumnMap(TypeUsage type, string name, Var v)
            : base(type, name)
        {
            m_var = v;
        }

        internal VarRefColumnMap(Var v)
            : this(v.Type, null, v)
        {
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
            return IsNamed ? Name : String.Format(CultureInfo.InvariantCulture, "{0}", m_var.Id);
        }

        #endregion

        #region private state

        private readonly Var m_var;

        #endregion
    }
}
