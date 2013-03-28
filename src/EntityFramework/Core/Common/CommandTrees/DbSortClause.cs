// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Specifies a sort key that can be used as part of the sort order in a
    ///     <see
    ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbSortExpression" />
    ///     . This class cannot be inherited.
    /// </summary>
    public sealed class DbSortClause
    {
        private readonly DbExpression _expr;
        private readonly bool _asc;
        private readonly string _coll;

        internal DbSortClause(DbExpression key, bool asc, string collation)
        {
            DebugCheck.NotNull(key);

            _expr = key;
            _asc = asc;
            _coll = collation;
        }

        /// <summary>Gets a Boolean value indicating whether or not this sort key uses an ascending sort order.</summary>
        /// <returns>true if this sort key uses an ascending sort order; otherwise, false.</returns>
        public bool Ascending
        {
            get { return _asc; }
        }

        /// <summary>Gets a string value that specifies the collation for this sort key.</summary>
        /// <returns>A string value that specifies the collation for this sort key.</returns>
        public string Collation
        {
            get { return _coll; }
        }

        /// <summary>
        ///     Gets or sets the <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that provides the value for this sort key.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that provides the value for this sort key.
        /// </returns>
        public DbExpression Expression
        {
            get { return _expr; }
        }
    }
}
