// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact.SqlGen
{
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    ///     TopClause represents the a TOP expression in a SqlSelectStatement.
    ///     It has a count property, which indicates how many TOP rows should be selected and a
    ///     boolen WithTies property.
    /// </summary>
    internal class TopClause : ISqlFragment
    {
        private readonly ISqlFragment topCount;
        private readonly bool withTies;

        /// <summary>
        ///     Do we need to add a WITH_TIES to the top statement
        /// </summary>
        internal bool WithTies
        {
            get { return withTies; }
        }

        /// <summary>
        ///     How many top rows should be selected.
        /// </summary>
        internal ISqlFragment TopCount
        {
            get { return topCount; }
        }

        /// <summary>
        ///     Creates a TopClause with the given topCount and withTies.
        /// </summary>
        /// <param name="topCount"> </param>
        /// <param name="withTies"> </param>
        internal TopClause(ISqlFragment topCount, bool withTies)
        {
            this.topCount = topCount;
            this.withTies = withTies;
        }

        /// <summary>
        ///     Creates a TopClause with the given topCount and withTies.
        ///     This function is not called if we have both TOP and SKIP. In that case SqlSelectStatment.WriteOffsetFetch is used.
        /// </summary>
        /// <param name="topCount"> </param>
        /// <param name="withTies"> </param>
        internal TopClause(int topCount, bool withTies)
        {
            Debug.Assert(!withTies, "WITH TIES is not supported in Top clause");
            var sqlBuilder = new SqlBuilder();
            sqlBuilder.Append(topCount.ToString(CultureInfo.InvariantCulture));
            this.topCount = sqlBuilder;
            this.withTies = withTies;
        }

        #region ISqlFragment Members

        /// <summary>
        ///     Write out the TOP part of sql select statement
        ///     It basically writes TOP (X) [WITH TIES].
        ///     The brackets around X are ommited for Sql8.
        /// </summary>
        /// <param name="writer"> </param>
        /// <param name="sqlGenerator"> </param>
        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            writer.Write("TOP ");

            writer.Write("(");

            TopCount.WriteSql(writer, sqlGenerator);

            writer.Write(")");

            writer.Write(" ");

            Debug.Assert(!WithTies, "WITH TIES cannot be true for Top clause");
        }

        #endregion
    }
}
