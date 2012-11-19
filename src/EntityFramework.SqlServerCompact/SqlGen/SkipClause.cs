// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact.SqlGen
{
    using System.Globalization;

    /// <summary>
    ///     SkipClause represents the a SKIP expression in a SqlSelectStatement.
    ///     It has a count property, which indicates how many rows should be discarded.
    /// </summary>
    internal class SkipClause : ISqlFragment
    {
        private readonly ISqlFragment skipCount;

        /// <summary>
        ///     How many top rows should be discarded.
        /// </summary>
        internal ISqlFragment SkipCount
        {
            get { return skipCount; }
        }

        /// <summary>
        ///     Creates a SkipClause with the given skipCount.
        /// </summary>
        /// <param name="topCount"> </param>
        /// <param name="withTies"> </param>
        internal SkipClause(ISqlFragment skipCount)
        {
            this.skipCount = skipCount;
        }

        /// <summary>
        ///     Creates a SkipClause with the given skipCount.
        /// </summary>
        /// <param name="skipCount"> </param>
        /// <param name="withTies"> </param>
        internal SkipClause(int skipCount)
        {
            var sqlBuilder = new SqlBuilder();
            sqlBuilder.Append(skipCount.ToString(CultureInfo.InvariantCulture));
            this.skipCount = sqlBuilder;
        }

        #region ISqlFragment Members

        /// <summary>
        ///     Write out the OFFSET part of sql select statement
        ///     It basically writes OFFSET X ROWS.
        /// </summary>
        /// <param name="writer"> </param>
        /// <param name="sqlGenerator"> </param>
        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            writer.Write("OFFSET ");

            SkipCount.WriteSql(writer, sqlGenerator);

            writer.Write(" ROWS");

            writer.Write(" ");
        }

        #endregion
    }
}
