// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.SqlGen
{
    using System.Globalization;

    // <summary>
    // TopClause represents the a TOP expression in a SqlSelectStatement.
    // It has a count property, which indicates how many TOP rows should be selected and a
    // boolen WithTies property.
    // </summary>
    internal class TopClause : ISqlFragment
    {
        private readonly ISqlFragment topCount;
        private readonly bool withTies;

        // <summary>
        // Do we need to add a WITH_TIES to the top statement
        // </summary>
        internal bool WithTies
        {
            get { return withTies; }
        }

        // <summary>
        // How many top rows should be selected.
        // </summary>
        internal ISqlFragment TopCount
        {
            get { return topCount; }
        }

        // <summary>
        // Creates a TopClause with the given topCount and withTies.
        // </summary>
        internal TopClause(ISqlFragment topCount, bool withTies)
        {
            this.topCount = topCount;
            this.withTies = withTies;
        }

        // <summary>
        // Creates a TopClause with the given topCount and withTies.
        // </summary>
        internal TopClause(int topCount, bool withTies)
        {
            var sqlBuilder = new SqlBuilder();
            sqlBuilder.Append(topCount.ToString(CultureInfo.InvariantCulture));
            this.topCount = sqlBuilder;
            this.withTies = withTies;
        }

        #region ISqlFragment Members

        // <summary>
        // Write out the TOP part of sql select statement
        // It basically writes TOP (X) [WITH TIES].
        // The brackets around X are ommited for Sql8.
        // </summary>
        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            writer.Write("TOP ");

            if (sqlGenerator.SqlVersion
                != SqlVersion.Sql8)
            {
                writer.Write("(");
            }

            TopCount.WriteSql(writer, sqlGenerator);

            if (sqlGenerator.SqlVersion
                != SqlVersion.Sql8)
            {
                writer.Write(")");
            }

            writer.Write(" ");

            if (WithTies)
            {
                writer.Write("WITH TIES ");
            }
        }

        #endregion
    }
}
