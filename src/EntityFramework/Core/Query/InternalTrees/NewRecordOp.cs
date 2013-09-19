// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents a new record constructor
    /// </summary>
    internal sealed class NewRecordOp : ScalarOp
    {
        #region private state

        private readonly List<EdmProperty> m_fields; // list of fields with specified values

        #endregion

        #region constructors

        /// <summary>
        /// Basic constructor. All fields have a value specified
        /// </summary>
        internal NewRecordOp(TypeUsage type)
            : base(OpType.NewRecord, type)
        {
            m_fields = new List<EdmProperty>(TypeHelpers.GetEdmType<RowType>(type).Properties);
        }

        /// <summary>
        /// Alternate form of the constructor. Only some fields have a value specified
        /// The arguments to the corresponding Node are exactly 1-1 with the fields
        /// described here.
        /// The missing fields are considered to be "null"
        /// </summary>
        internal NewRecordOp(TypeUsage type, List<EdmProperty> fields)
            : base(OpType.NewRecord, type)
        {
#if DEBUG
            foreach (var p in fields)
            {
                Debug.Assert(ReferenceEquals(p.DeclaringType, Type.EdmType));
            }
#endif
            m_fields = fields;
        }

        private NewRecordOp()
            : base(OpType.NewRecord)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly NewRecordOp Pattern = new NewRecordOp();

        /// <summary>
        /// Determine if a value has been provided for the specified field.
        /// Returns the position of this field (ie) the specific argument in the Node's
        /// children. If no value has been provided for this field, then simply
        /// return false
        /// </summary>
        internal bool GetFieldPosition(EdmProperty field, out int fieldPosition)
        {
            Debug.Assert(
                ReferenceEquals(field.DeclaringType, Type.EdmType),
                "attempt to get invalid field from this record type");

            fieldPosition = 0;
            for (var i = 0; i < m_fields.Count; i++)
            {
                if (m_fields[i] == field)
                {
                    fieldPosition = i;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// List of all properties that have values specified
        /// </summary>
        internal List<EdmProperty> Properties
        {
            get { return m_fields; }
        }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v"> The BasicOpVisitor that is visiting this Op </param>
        /// <param name="n"> The Node that references this Op </param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n)
        {
            v.Visit(this, n);
        }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v"> The visitor </param>
        /// <param name="n"> The node in question </param>
        /// <returns> An instance of TResultType </returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n)
        {
            return v.Visit(this, n);
        }

        #endregion
    }
}
