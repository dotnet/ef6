// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    internal abstract class ScanTableBaseOp : RelOp
    {
        #region private state

        private readonly Table m_table;

        #endregion

        #region constructors

        protected ScanTableBaseOp(OpType opType, Table table)
            : base(opType)
        {
            m_table = table;
        }

        protected ScanTableBaseOp(OpType opType)
            : base(opType)
        {
        }

        #endregion

        #region public methods

        /// <summary>
        /// Get the table instance produced by this Op
        /// </summary>
        internal Table Table
        {
            get { return m_table; }
        }

        #endregion
    }
}
