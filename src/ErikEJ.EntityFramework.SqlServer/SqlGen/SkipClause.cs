// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.SqlGen
{
    using System.Globalization;

    // <summary>
    // SkipClause represents the a SKIP expression in a SqlSelectStatement.
    // It has a count property, which indicates how many rows should be discarded.
    // </summary>
    internal class SkipClause : ISqlFragment
    {
        private readonly ISqlFragment skipCount;

        // <summary>
        // How many rows should be skipped.
        // </summary>
        internal ISqlFragment SkipCount
        {
            get { return skipCount; }
        }

        // <summary>
        // Creates a SkipClause with the given skipCount.
        // </summary>
        internal SkipClause(ISqlFragment skipCount)
        {
            this.skipCount = skipCount;
        }

        // <summary>
        // Creates a SkipClause with the given skipCount.
        // </summary>
        internal SkipClause(int skipCount)
        {
            var sqlBuilder = new SqlBuilder();
            sqlBuilder.Append(skipCount.ToString(CultureInfo.InvariantCulture));
            this.skipCount = sqlBuilder;
        }

        #region ISqlFragment Members

        // <summary>
        // Write out the OFFSET part of sql select statement
        // It basically writes OFFSET X ROWS.
        // </summary>
        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            writer.Write("OFFSET ");

            SkipCount.WriteSql(writer, sqlGenerator);

            writer.Write(" ROWS ");
        }

        #endregion
    }
}
