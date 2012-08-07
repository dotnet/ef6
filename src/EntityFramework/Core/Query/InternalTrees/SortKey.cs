// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    /// <summary>
    ///     A Sortkey
    /// </summary>
    internal class SortKey
    {
        #region private state

        private readonly bool m_asc;
        private readonly string m_collation;

        #endregion

        #region constructors

        internal SortKey(Var v, bool asc, string collation)
        {
            Var = v;
            m_asc = asc;
            m_collation = collation;
        }

        #endregion

        #region public methods

        /// <summary>
        ///     The Var being sorted
        /// </summary>
        internal Var Var { get; set; }

        /// <summary>
        ///     Is this a sort asc, or a sort desc
        /// </summary>
        internal bool AscendingSort
        {
            get { return m_asc; }
        }

        /// <summary>
        ///     An optional collation (only for string types)
        /// </summary>
        internal string Collation
        {
            get { return m_collation; }
        }

        #endregion
    }
}
