// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact.SqlGen
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.SqlServerCompact.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// A SqlSelectStatement represents a canonical SQL SELECT statement.
    /// It has fields for the 5 main clauses
    /// <list type="number">
    ///     <item>SELECT</item>
    ///     <item>FROM</item>
    ///     <item>WHERE</item>
    ///     <item>GROUP BY</item>
    ///     <item>ORDER BY</item>
    /// </list>
    /// We do not have HAVING, since the CQT does not have such a node.
    /// Each of the fields is a SqlBuilder, so we can keep appending SQL strings
    /// or other fragments to build up the clause.
    /// We have a IsDistinct property to indicate that we want distict columns.
    /// This is given out of band, since the input expression to the select clause
    /// may already have some columns projected out, and we use append-only SqlBuilders.
    /// The DISTINCT is inserted when we finally write the object into a string.
    /// Also, we have a Top property, which is non-null if the number of results should
    /// be limited to certain number. It is given out of band for the same reasons as DISTINCT.
    /// The FromExtents contains the list of inputs in use for the select statement.
    /// There is usually just one element in this - Select statements for joins may
    /// temporarily have more than one.
    /// If the select statement is created by a Join node, we maintain a list of
    /// all the extents that have been flattened in the join in AllJoinExtents
    /// <example>
    ///     in J(j1= J(a,b), c)
    ///     FromExtents has 2 nodes JoinSymbol(name=j1, ...) and Symbol(name=c)
    ///     AllJoinExtents has 3 nodes Symbol(name=a), Symbol(name=b), Symbol(name=c)
    /// </example>
    /// If any expression in the non-FROM clause refers to an extent in a higher scope,
    /// we add that extent to the OuterExtents list.  This list denotes the list
    /// of extent aliases that may collide with the aliases used in this select statement.
    /// It is set by <see cref="SqlGenerator.Visit(DbVariableReferenceExpression)" />.
    /// An extent is an outer extent if it is not one of the FromExtents.
    /// </summary>
    internal sealed class SqlSelectStatement : ISqlFragment
    {
        /// <summary>
        /// Whether the columns ouput by this sql statement were renamed from what given in the command tree.
        /// </summary>
        internal bool OutputColumnsRenamed { get; set; }

        /// <summary>
        /// A dictionary of output columns, relevant only if output columns are renamed.
        /// </summary>
        internal Dictionary<string, Symbol> OutputColumns { get; set; }

        /// <summary>
        /// Do we need to add a DISTINCT at the beginning of the SELECT
        /// </summary>
        internal bool IsDistinct { get; set; }

        internal List<Symbol> AllJoinExtents { get; // We have a setter as well, even though this is a list,
            // since we use this field only in special cases.
            set; }

        private List<Symbol> fromExtents;

        internal List<Symbol> FromExtents
        {
            get
            {
                if (null == fromExtents)
                {
                    fromExtents = new List<Symbol>();
                }
                return fromExtents;
            }
        }

        private Dictionary<Symbol, bool> outerExtents;

        internal Dictionary<Symbol, bool> OuterExtents
        {
            get
            {
                if (null == outerExtents)
                {
                    outerExtents = new Dictionary<Symbol, bool>();
                }
                return outerExtents;
            }
        }

        private TopClause top;

        internal TopClause Top
        {
            get { return top; }
            set
            {
                Debug.Assert(top == null, "SqlSelectStatement.Top has already been set");
                top = value;
            }
        }

        private SkipClause skip;

        internal SkipClause Skip
        {
            get { return skip; }
            set
            {
                Debug.Assert(skip == null, "SqlSelectStatement.Skip has already been set");
                skip = value;
            }
        }

        private readonly SqlBuilder select = new SqlBuilder();

        internal SqlBuilder Select
        {
            get { return select; }
        }

        private readonly SqlBuilder from = new SqlBuilder();

        internal SqlBuilder From
        {
            get { return from; }
        }

        private SqlBuilder where;

        internal SqlBuilder Where
        {
            get
            {
                if (null == where)
                {
                    where = new SqlBuilder();
                }
                return where;
            }
        }

        private SqlBuilder groupBy;

        internal SqlBuilder GroupBy
        {
            get
            {
                if (null == groupBy)
                {
                    groupBy = new SqlBuilder();
                }
                return groupBy;
            }
        }

        private SqlBuilder orderBy;

        public SqlBuilder OrderBy
        {
            get
            {
                if (null == orderBy)
                {
                    orderBy = new SqlBuilder();
                }
                return orderBy;
            }
        }

        //indicates whether it is the top most select statement, 
        // if not Order By should be omitted unless there is a corresponding TOP
        internal bool IsTopMost { get; set; }

        #region ISqlFragment Members

        /// <summary>
        /// Write out a SQL select statement as a string.
        /// We have to
        /// <list type="number">
        ///     <item>
        ///         Check whether the aliases extents we use in this statement have
        ///         to be renamed.
        ///         We first create a list of all the aliases used by the outer extents.
        ///         For each of the FromExtents( or AllJoinExtents if it is non-null),
        ///         rename it if it collides with the previous list.
        ///     </item>
        ///     <item>Write each of the clauses (if it exists) as a string</item>
        /// </list>
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
        {
            #region Check if FROM aliases need to be renamed

            // Create a list of the aliases used by the outer extents
            // JoinSymbols have to be treated specially.
            List<string> outerExtentAliases = null;
            if ((null != outerExtents)
                && (0 < outerExtents.Count))
            {
                foreach (var outerExtent in outerExtents.Keys)
                {
                    var joinSymbol = outerExtent as JoinSymbol;
                    if (joinSymbol != null)
                    {
                        foreach (var symbol in joinSymbol.FlattenedExtentList)
                        {
                            if (null == outerExtentAliases)
                            {
                                outerExtentAliases = new List<string>();
                            }
                            outerExtentAliases.Add(symbol.NewName);
                        }
                    }
                    else
                    {
                        if (null == outerExtentAliases)
                        {
                            outerExtentAliases = new List<string>();
                        }
                        outerExtentAliases.Add(outerExtent.NewName);
                    }
                }
            }

            // An then rename each of the FromExtents we have
            // If AllJoinExtents is non-null - it has precedence.
            // The new name is derived from the old name - we append an increasing int.
            var extentList = AllJoinExtents ?? fromExtents;
            if (null != extentList)
            {
                foreach (var fromAlias in extentList)
                {
                    if ((null != outerExtentAliases)
                        && outerExtentAliases.Contains(fromAlias.Name))
                    {
                        var i = sqlGenerator.AllExtentNames[fromAlias.Name];
                        string newName;
                        do
                        {
                            ++i;
                            newName = fromAlias.Name + i.ToString(CultureInfo.InvariantCulture);
                        }
                        while (sqlGenerator.AllExtentNames.ContainsKey(newName));
                        sqlGenerator.AllExtentNames[fromAlias.Name] = i;
                        fromAlias.NewName = newName;

                        // Add extent to list of known names (although i is always incrementing, "prefix11" can
                        // eventually collide with "prefix1" when it is extended)
                        sqlGenerator.AllExtentNames[newName] = 0;
                    }

                    // Add the current alias to the list, so that the extents
                    // that follow do not collide with me.
                    if (null == outerExtentAliases)
                    {
                        outerExtentAliases = new List<string>();
                    }
                    outerExtentAliases.Add(fromAlias.NewName);
                }
            }

            #endregion

            // Increase the indent, so that the Sql statement is nested by one tab.
            writer.Indent += 1; // ++ can be confusing in this context

            writer.Write("SELECT ");
            if (IsDistinct)
            {
                writer.Write("DISTINCT ");
            }

            if (Top != null
                && Skip == null) // This is simple TOP case. There is no OFFSET.
            {
                Top.WriteSql(writer, sqlGenerator);
            }

            if ((null == @select)
                || Select.IsEmpty)
            {
                Debug.Assert(false); // we have removed all possibilities of SELECT *.
                writer.Write("*");
            }
            else
            {
                Select.WriteSql(writer, sqlGenerator);
            }

            writer.WriteLine();
            writer.Write("FROM ");
            From.WriteSql(writer, sqlGenerator);

            if ((null != @where)
                && !Where.IsEmpty)
            {
                writer.WriteLine();
                writer.Write("WHERE ");
                Where.WriteSql(writer, sqlGenerator);
            }

            if ((null != groupBy)
                && !GroupBy.IsEmpty)
            {
                writer.WriteLine();
                writer.Write("GROUP BY ");
                GroupBy.WriteSql(writer, sqlGenerator);
            }

            if ((null != orderBy)
                && !OrderBy.IsEmpty
                && (IsTopMost || Top != null || Skip != null))
            {
                writer.WriteLine();
                writer.Write("ORDER BY ");
                OrderBy.WriteSql(writer, sqlGenerator);
            }

            if (null != Skip)
            {
                writer.WriteLine();
                WriteOffsetFetch(writer, Top, Skip, sqlGenerator); // Write OFFSET, FETCH clause.
            }

            --writer.Indent;
        }

        #endregion

        // This function generates OFFSET, FETCH clause from Top and Skip information.
        // Note that this should be used only when Skip is present. For only Top, TopClause.WriteSql()
        //
        private static void WriteOffsetFetch(SqlWriter writer, TopClause top, SkipClause skip, SqlGenerator sqlGenerator)
        {
            DebugCheck.NotNull(skip);
            skip.WriteSql(writer, sqlGenerator);
            if (top != null)
            {
                writer.Write("FETCH NEXT ");

                top.TopCount.WriteSql(writer, sqlGenerator);

                writer.Write(" ROWS ONLY");

                writer.Write(" ");
            }
        }
    }
}
