// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact.SqlGen
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.SqlServerCompact.Resources;
    using System.Data.Entity.SqlServerCompact.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    // <summary>
    // Translates the command object into a SQL string that can be executed on
    // SSCE
    // </summary>
    // <remarks>
    // The translation is implemented as a visitor <see cref="DbExpressionVisitor{TResultType}" />
    // over the query tree.  It makes a single pass over the tree, collecting the sql
    // fragments for the various nodes in the tree <see cref="ISqlFragment" />.
    // The major operations are
    // <list type="bullet">
    //     <item>
    //         Select statement minimization.  Multiple nodes in the query tree
    //         that can be part of a single SQL select statement are merged. e.g. a
    //         Filter node that is the input of a Project node can typically share the
    //         same SQL statement.
    //     </item>
    //     <item>
    //         Alpha-renaming.  As a result of the statement minimization above, there
    //         could be name collisions when using correlated subqueries
    //         <example>
    //             <code>Filter(
    //                 b = Project( c.x
    //                 c = Extent(xyz)
    //                 )
    //                 exists (
    //                 Filter(
    //                 c = Extent(xyz)
    //                 b.x = c.x
    //                 )
    //                 )
    //                 )</code>
    //             The first Filter, Project and Extent will share the same SQL select statement.
    //             The alias for the Project i.e. b, will be replaced with c.
    //             If the alias c for the Filter within the exists clause is not renamed,
    //             we will get <c>c.x = c.x</c>, which is incorrect.
    //             Instead, the alias c within the second filter should be renamed to c1, to give
    //             <c>c.x = c1.x</c> i.e. b is renamed to c, and c is renamed to c1.
    //         </example>
    //     </item>
    //     <item>
    //         Join flattening.  In the query tree, a list of join nodes is typically
    //         represented as a tree of Join nodes, each with 2 children. e.g.
    //         <example>
    //             <code>a = Join(InnerJoin
    //                 b = Join(CrossJoin
    //                 c = Extent(xyz)
    //                 d = Extent(xyz)
    //                 )
    //                 e = Extent(xyz)
    //                 on b.c.x = e.x
    //                 )</code>
    //             If translated directly, this will be translated to
    //             <code>FROM ( SELECT c.*, d.*
    //                 FROM xyz as c
    //                 CROSS JOIN xyz as d) as b
    //                 INNER JOIN xyz as e on b.x' = e.x</code>
    //             It would be better to translate this as
    //             <code>FROM xyz as c
    //                 CROSS JOIN xyz as d
    //                 INNER JOIN xyz as e on c.x = e.x</code>
    //             This allows the optimizer to choose an appropriate join ordering for evaluation.
    //         </example>
    //     </item>
    //     <item>
    //         Select * and column renaming.  In the example above, we noticed that
    //         in some cases we add
    //         <c>SELECT * FROM ...</c>
    //         to complete the SQL
    //         statement. i.e. there is no explicit PROJECT list.
    //         In this case, we enumerate all the columns available in the FROM clause
    //         This is particularly problematic in the case of Join trees, since the columns
    //         from the extents joined might have the same name - this is illegal.  To solve
    //         this problem, we will have to rename columns if they are part of a SELECT *
    //         for a JOIN node - we do not need renaming in any other situation.
    //         <see cref="SqlGenerator.AddDefaultColumns" />
    //         .
    //     </item>
    // </list>
    // <para> Renaming issues. When rows or columns are renamed, we produce names that are unique globally with respect to the query. The names are derived from the original names, with an integer as a suffix. e.g. CustomerId will be renamed to CustomerId1, CustomerId2 etc. Since the names generated are globally unique, they will not conflict when the columns of a JOIN SELECT statement are joined with another JOIN. </para>
    // <para>
    //     Record flattening. SQL server does not have the concept of records. However, a join statement produces records. We have to flatten the record accesses into a simple <c>alias.column</c> form.
    //     <see
    //         cref="SqlGenerator.Visit(DbPropertyExpression)" />
    // </para>
    // <para>
    //     Building the SQL. There are 2 phases
    //     <list type="numbered">
    //         <item>
    //             Traverse the tree, producing a sql builder
    //             <see cref="SqlBuilder" />
    //         </item>
    //         <item>
    //             Write the SqlBuilder into a string, renaming the aliases and columns
    //             as needed.
    //         </item>
    //     </list>
    //     In the first phase, we traverse the tree. We cannot generate the SQL string right away, since
    //     <list
    //         type="bullet">
    //         <item>The WHERE clause has to be visited before the from clause.</item>
    //         <item>
    //             extent aliases and column aliases need to be renamed.  To minimize
    //             renaming collisions, all the names used must be known, before any renaming
    //             choice is made.
    //         </item>
    //     </list>
    //     To defer the renaming choices, we use symbols
    //     <see
    //         cref="Symbol" />
    //     . These are renamed in the second phase. Since visitor methods cannot transfer information to child nodes through parameters, we use some global stacks,
    //     <list
    //         type="bullet">
    //         <item>
    //             A stack for the current SQL select statement.  This is needed by
    //             <see cref="SqlGenerator.Visit(DbVariableReferenceExpression)" />
    //             to create a
    //             list of free variables used by a select statement.  This is needed for
    //             alias renaming.
    //         </item>
    //         <item>
    //             A stack for the join context.  When visiting an extent,
    //             we need to know whether we are inside a join or not.  If we are inside
    //             a join, we do not create a new SELECT statement.
    //         </item>
    //     </list>
    // </para>
    // <para>
    //     Global state. To enable renaming, we maintain
    //     <list type="bullet">
    //         <item>The set of all extent aliases used.</item>
    //         <item>The set of all parameter names.</item>
    //         <item>The set of all column names that may need to be renamed.</item>
    //     </list>
    //     Finally, we have a symbol table to lookup variable references. All references to the same extent have the same symbol.
    // </para>
    // <para>
    //     Sql select statement sharing. Each of the relational operator nodes
    //     <list type="bullet">
    //         <item>Project</item>
    //         <item>Filter</item>
    //         <item>GroupBy</item>
    //         <item>Sort/OrderBy</item>
    //     </list>
    //     can add its non-input (e.g. project, predicate, sort order etc.) to the SQL statement for the input, or create a new SQL statement. If it chooses to reuse the input's SQL statement, we play the following symbol table trick to accomplish renaming. The symbol table entry for the alias of the current node points to the symbol for the input in the input's SQL statement.
    //     <example>
    //         <code>Project(b.x
    //             b = Filter(
    //             c = Extent(xyz)
    //             c.x = 5)
    //             )</code>
    //         The Extent node creates a new SqlSelectStatement.  This is added to the
    //         symbol table by the Filter as {c, Symbol(c)}.  Thus, <c>c.x</c> is resolved to
    //         <c>Symbol(c).x</c>.
    //         Looking at the project node, we add {b, Symbol(c)} to the symbol table if the
    //         SQL statement is reused, and {b, Symbol(b)}, if there is no reuse.
    //         Thus, <c>b.x</c> is resolved to <c>Symbol(c).x</c> if there is reuse, and to
    //         <c>Symbol(b).x</c> if there is no reuse.
    //     </example>
    // </para>
    // </remarks>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal sealed class SqlGenerator : DbExpressionVisitor<ISqlFragment>
    {
        #region Visitor parameter stacks

        // <summary>
        // Every relational node has to pass its SELECT statement to its children
        // This allows them (DbVariableReferenceExpression eventually) to update the list of
        // outer extents (free variables) used by this select statement.
        // </summary>
        private Stack<SqlSelectStatement> selectStatementStack;

        // <summary>
        // The top of the stack
        // </summary>
        private SqlSelectStatement CurrentSelectStatement
        {
            // There is always something on the stack, so we can always Peek.
            get { return selectStatementStack.Peek(); }
        }

        // <summary>
        // Nested joins and extents need to know whether they should create
        // a new Select statement, or reuse the parent's.  This flag
        // indicates whether the parent is a join or not.
        // </summary>
        private Stack<bool> isParentAJoinStack;

        // <summary>
        // The top of the stack
        // </summary>
        private bool IsParentAJoin
        {
            // There might be no entry on the stack if a Join node has never
            // been seen, so we return false in that case.
            get { return isParentAJoinStack.Count == 0 ? false : isParentAJoinStack.Peek(); }
        }

        #endregion

        #region Global lists and state

        private Dictionary<string, int> allExtentNames;

        internal Dictionary<string, int> AllExtentNames
        {
            get { return allExtentNames; }
        }

        // For each column name, we store the last integer suffix that
        // was added to produce a unique column name.  This speeds up
        // the creation of the next unique name for this column name.
        private Dictionary<string, int> allColumnNames;

        internal Dictionary<string, int> AllColumnNames
        {
            get { return allColumnNames; }
        }

        private readonly SymbolTable symbolTable = new SymbolTable();

        // <summary>
        // VariableReferenceExpressions are allowed only as children of DbPropertyExpression
        // or MethodExpression.  The cheapest way to ensure this is to set the following
        // property in DbVariableReferenceExpression and reset it in the allowed parent expressions.
        // </summary>
        private bool isVarRefSingle;

        #endregion

        #region Statics

        private static readonly Dictionary<string, FunctionHandler> _storeFunctionHandlers = InitializeStoreFunctionHandlers();
        private static readonly Dictionary<string, FunctionHandler> _canonicalFunctionHandlers = InitializeCanonicalFunctionHandlers();
        private static readonly Dictionary<string, string> _functionNameToOperatorDictionary = InitializeFunctionNameToOperatorDictionary();
        private static readonly Dictionary<string, string> _dateAddFunctionNameToDatepartDictionary = InitializeDateAddFunctionNameToDatepartDictionary();
        private static readonly Dictionary<string, string> _dateDiffFunctionNameToDatepartDictionary = InitializeDateDiffFunctionNameToDatepartDictionary();
        private static readonly Dictionary<string, object> _datepartKeywords = InitializeDatepartKeywords();
        private static readonly char[] _hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        private static readonly Queue<string> _functionRequiresReturnTypeCast = new Queue<string>(
            new[]
                {
                    "SqlServer.LEN",
                    "SqlServer.PATINDEX",
                    "SqlServer.CHARINDEX",
                    "SqlServer.DATALENGTH",
                    "Edm.IndexOf",
                    "Edm.Length"
                } //,
            /*StringComparer.Ordinal*/);

        private static readonly ISet<string> _functionRequiresReturnTypeCastToSingle =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "Edm.Abs",
                    "Edm.Round",
                    "Edm.Floor",
                    "Edm.Ceiling"
                };

        // topElementExpression is used to detect the any occurrence of element expression which 
        // is not a child of the top level projectExpression
        private bool topElementExpression;
        // stores the list of all the scalar subquery tables names
        private readonly List<string> listOfScalarSubQueryTables = new List<string>();

        private static readonly Queue<string> _maxTypeNames = new Queue<string>(
            new[]
                {
                    "ntext",
                    "image"
                });

        private const byte defaultDecimalPrecision = 18;

        private delegate ISqlFragment FunctionHandler(SqlGenerator sqlgen, DbFunctionExpression functionExpr);

        // <summary>
        // All special store functions and their handlers
        // </summary>
        private static Dictionary<string, FunctionHandler> InitializeStoreFunctionHandlers()
        {
            var functionHandlers = new Dictionary<string, FunctionHandler>(5, StringComparer.Ordinal);
            functionHandlers.Add("CONCAT", HandleConcatFunction);
            functionHandlers.Add("DATEADD", HandleDatepartDateFunction);
            functionHandlers.Add("DATEDIFF", HandleDatepartDateFunction);
            functionHandlers.Add("DATENAME", HandleDatepartDateFunction);
            functionHandlers.Add("DATEPART", HandleDatepartDateFunction);
            return functionHandlers;
        }

        // <summary>
        // Initializes the mapping from names of canonical function for date/time addition
        // to corresponding dateparts
        // </summary>
        private static Dictionary<string, string> InitializeDateAddFunctionNameToDatepartDictionary()
        {
            var dateAddFunctionNameToDatepartDictionary = new Dictionary<string, string>(7, StringComparer.Ordinal);
            dateAddFunctionNameToDatepartDictionary.Add("AddYears", "year");
            dateAddFunctionNameToDatepartDictionary.Add("AddMonths", "month");
            dateAddFunctionNameToDatepartDictionary.Add("AddDays", "day");
            dateAddFunctionNameToDatepartDictionary.Add("AddHours", "hour");
            dateAddFunctionNameToDatepartDictionary.Add("AddMinutes", "minute");
            dateAddFunctionNameToDatepartDictionary.Add("AddSeconds", "second");
            dateAddFunctionNameToDatepartDictionary.Add("AddMilliseconds", "millisecond");
            return dateAddFunctionNameToDatepartDictionary;
        }

        // <summary>
        // Initializes the mapping from names of canonical function for date/time difference
        // to corresponding dateparts
        // </summary>
        private static Dictionary<string, string> InitializeDateDiffFunctionNameToDatepartDictionary()
        {
            var dateDiffFunctionNameToDatepartDictionary = new Dictionary<string, string>(7, StringComparer.Ordinal);
            dateDiffFunctionNameToDatepartDictionary.Add("DiffYears", "year");
            dateDiffFunctionNameToDatepartDictionary.Add("DiffMonths", "month");
            dateDiffFunctionNameToDatepartDictionary.Add("DiffDays", "day");
            dateDiffFunctionNameToDatepartDictionary.Add("DiffHours", "hour");
            dateDiffFunctionNameToDatepartDictionary.Add("DiffMinutes", "minute");
            dateDiffFunctionNameToDatepartDictionary.Add("DiffSeconds", "second");
            dateDiffFunctionNameToDatepartDictionary.Add("DiffMilliseconds", "millisecond");
            return dateDiffFunctionNameToDatepartDictionary;
        }

        // <summary>
        // All special non-aggregate canonical functions and their handlers
        // </summary>
        private static Dictionary<string, FunctionHandler> InitializeCanonicalFunctionHandlers()
        {
            var functionHandlers = new Dictionary<string, FunctionHandler>(16, StringComparer.Ordinal);
            functionHandlers.Add("Round", HandleCanonicalFunctionRound);
            functionHandlers.Add("Truncate", HandleCanonicalFunctionTruncate);
            functionHandlers.Add("IndexOf", HandleCanonicalFunctionIndexOf);
            functionHandlers.Add("Length", HandleCanonicalFunctionLength);
            functionHandlers.Add("Left", HandleCanonicalFunctionLeft);
            functionHandlers.Add("Right", HandleCanonicalFunctionRight);
            functionHandlers.Add("ToLower", HandleCanonicalFunctionToLower);
            functionHandlers.Add("ToUpper", HandleCanonicalFunctionToUpper);
            functionHandlers.Add("Trim", HandleCanonicalFunctionTrim);
            functionHandlers.Add("Contains", HandleCanonicalFunctionContains);
            functionHandlers.Add("StartsWith", HandleCanonicalFunctionStartsWith);
            functionHandlers.Add("EndsWith", HandleCanonicalFunctionEndsWith);
            functionHandlers.Add("NewGuid", HandleCanonicalFunctionNewGuid);

            //DateTime Functions
            functionHandlers.Add("Year", HandleCanonicalFunctionDatepart);
            functionHandlers.Add("Month", HandleCanonicalFunctionDatepart);
            functionHandlers.Add("Day", HandleCanonicalFunctionDatepart);
            functionHandlers.Add("Hour", HandleCanonicalFunctionDatepart);
            functionHandlers.Add("Minute", HandleCanonicalFunctionDatepart);
            functionHandlers.Add("Second", HandleCanonicalFunctionDatepart);
            functionHandlers.Add("Millisecond", HandleCanonicalFunctionDatepart);
            functionHandlers.Add("DayOfYear", HandleCanonicalFunctionDatepart);
            functionHandlers.Add("CurrentDateTime", HandleCanonicalFunctionCurrentDateTime);
            functionHandlers.Add("AddYears", HandleCanonicalFunctionDateAdd);
            functionHandlers.Add("AddMonths", HandleCanonicalFunctionDateAdd);
            functionHandlers.Add("AddDays", HandleCanonicalFunctionDateAdd);
            functionHandlers.Add("AddHours", HandleCanonicalFunctionDateAdd);
            functionHandlers.Add("AddMinutes", HandleCanonicalFunctionDateAdd);
            functionHandlers.Add("AddSeconds", HandleCanonicalFunctionDateAdd);
            functionHandlers.Add("AddMilliseconds", HandleCanonicalFunctionDateAdd);
            functionHandlers.Add("CreateDateTime", HandleCanonicalFunctionCreateDateTime);
            functionHandlers.Add("TruncateTime", HandleCanonicalFunctionTruncateTime);
            functionHandlers.Add("DiffYears", HandleCanonicalFunctionDateDiff);
            functionHandlers.Add("DiffMonths", HandleCanonicalFunctionDateDiff);
            functionHandlers.Add("DiffDays", HandleCanonicalFunctionDateDiff);
            functionHandlers.Add("DiffHours", HandleCanonicalFunctionDateDiff);
            functionHandlers.Add("DiffMinutes", HandleCanonicalFunctionDateDiff);
            functionHandlers.Add("DiffSeconds", HandleCanonicalFunctionDateDiff);
            functionHandlers.Add("DiffMilliseconds", HandleCanonicalFunctionDateDiff);

            //Functions that translate to operators
            functionHandlers.Add("Concat", HandleConcatFunction);
            functionHandlers.Add("BitwiseAnd", HandleCanonicalFunctionBitwise);
            functionHandlers.Add("BitwiseNot", HandleCanonicalFunctionBitwise);
            functionHandlers.Add("BitwiseOr", HandleCanonicalFunctionBitwise);
            functionHandlers.Add("BitwiseXor", HandleCanonicalFunctionBitwise);

            // Unsupported canonical functions
            functionHandlers.Add("StDev", HandleUnsupportedFunction);
            functionHandlers.Add("StDevP", HandleUnsupportedFunction);
            functionHandlers.Add("Var", HandleUnsupportedFunction);
            functionHandlers.Add("VarP", HandleUnsupportedFunction);
            functionHandlers.Add("Reverse", HandleUnsupportedFunction);
            functionHandlers.Add("CurrentUtcDateTime", HandleUnsupportedFunction);
            functionHandlers.Add("CurrentDateTimeOffset", HandleUnsupportedFunction);
            functionHandlers.Add("GetTotalOffsetMinutes", HandleUnsupportedFunction);
            functionHandlers.Add("AddMicroseconds", HandleUnsupportedFunction);
            functionHandlers.Add("AddNanoseconds", HandleUnsupportedFunction);
            functionHandlers.Add("CreateDateTimeOffset", HandleUnsupportedFunction);
            functionHandlers.Add("CreateTime", HandleUnsupportedFunction);
            functionHandlers.Add("DiffMicroseconds", HandleUnsupportedFunction);
            functionHandlers.Add("DiffNanoseconds", HandleUnsupportedFunction);

            return functionHandlers;
        }

        // <summary>
        // Valid datepart values
        // </summary>
        private static Dictionary<string, object> InitializeDatepartKeywords()
        {
            var datepartKeywords = new Dictionary<string, object>(30, StringComparer.OrdinalIgnoreCase);
            datepartKeywords.Add("d", null);
            datepartKeywords.Add("day", null);
            datepartKeywords.Add("dayofyear", null);
            datepartKeywords.Add("dd", null);
            datepartKeywords.Add("dw", null);
            datepartKeywords.Add("dy", null);
            datepartKeywords.Add("hh", null);
            datepartKeywords.Add("hour", null);
            datepartKeywords.Add("m", null);
            datepartKeywords.Add("mi", null);
            datepartKeywords.Add("millisecond", null);
            datepartKeywords.Add("minute", null);
            datepartKeywords.Add("mm", null);
            datepartKeywords.Add("month", null);
            datepartKeywords.Add("ms", null);
            datepartKeywords.Add("n", null);
            datepartKeywords.Add("q", null);
            datepartKeywords.Add("qq", null);
            datepartKeywords.Add("quarter", null);
            datepartKeywords.Add("s", null);
            datepartKeywords.Add("second", null);
            datepartKeywords.Add("ss", null);
            datepartKeywords.Add("week", null);
            datepartKeywords.Add("weekday", null);
            datepartKeywords.Add("wk", null);
            datepartKeywords.Add("ww", null);
            datepartKeywords.Add("y", null);
            datepartKeywords.Add("year", null);
            datepartKeywords.Add("yy", null);
            datepartKeywords.Add("yyyy", null);
            return datepartKeywords;
        }

        // <summary>
        // Initializes the mapping from functions to TSql operators
        // for all functions that translate to TSql operators
        // </summary>
        private static Dictionary<string, string> InitializeFunctionNameToOperatorDictionary()
        {
            var functionNameToOperatorDictionary = new Dictionary<string, string>(5, StringComparer.Ordinal);
            functionNameToOperatorDictionary.Add("Concat", "+"); //canonical
            functionNameToOperatorDictionary.Add("CONCAT", "+"); //store
            functionNameToOperatorDictionary.Add("BitwiseAnd", "&");
            functionNameToOperatorDictionary.Add("BitwiseNot", "~");
            functionNameToOperatorDictionary.Add("BitwiseOr", "|");
            functionNameToOperatorDictionary.Add("BitwiseXor", "^");
            return functionNameToOperatorDictionary;
        }

        #endregion

        #region Constructor

        // <summary>
        // Basic constructor. 
        // Internal for test purposes only, otherwise should be treated as private. 
        // </summary>
        internal SqlGenerator()
        {
        }

        #endregion

        #region Entry points

        // <summary>
        // General purpose static function that can be called from System.Data assembly
        // </summary>
        // <param name="tree"> command tree </param>
        // <param name="parameters"> Parameters to add to the command tree corresponding to constants in the command tree. Used only in ModificationCommandTrees. </param>
        // <returns> The string representing the SQL to be executed. </returns>
        internal static string[] GenerateSql(
            DbCommandTree tree, out List<DbParameter> parameters, out CommandType commandType, bool isLocalProvider)
        {
            SqlGenerator sqlGen;
            commandType = CommandType.Text;

            var qct = tree as DbQueryCommandTree;
            var ict = tree as DbInsertCommandTree;
            var uct = tree as DbUpdateCommandTree;
            var dct = tree as DbDeleteCommandTree;
            var fct = tree as DbFunctionCommandTree;

            if (qct != null)
            {
                sqlGen = new SqlGenerator();
                parameters = null;
                return sqlGen.GenerateSql(qct);
            }
            else if (ict != null)
            {
                return DmlSqlGenerator.GenerateInsertSql(ict, out parameters, isLocalProvider);
            }
            else if (uct != null)
            {
                return DmlSqlGenerator.GenerateUpdateSql(uct, out parameters, isLocalProvider);
            }
            else if (dct != null)
            {
                return DmlSqlGenerator.GenerateDeleteSql(dct, out parameters, isLocalProvider);
            }
            else if (fct != null)
            {
                throw ADP1.NotSupported(EntityRes.GetString(EntityRes.StoredProceduresNotSupported));
            }
            else
            {
                throw ADP1.NotImplemented(String.Empty);
            }
        }

        #endregion

        #region Driver Methods

        // <summary>
        // Translate a command tree to a SQL string.
        // The input tree could be translated to either a SQL SELECT statement
        // or a SELECT expression.  This choice is made based on the return type
        // of the expression
        // CollectionType => select statement
        // non collection type => select expression
        // </summary>
        // <returns> The string representing the SQL to be executed. </returns>
        private string[] GenerateSql(DbQueryCommandTree tree)
        {
            var targetTree = tree;
            selectStatementStack = new Stack<SqlSelectStatement>();
            isParentAJoinStack = new Stack<bool>();

            allExtentNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            allColumnNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // Literals will not be converted to parameters.

            ISqlFragment result;
            if (TypeSemantics.IsCollectionType(targetTree.Query.ResultType))
            {
                var sqlStatement = VisitExpressionEnsureSqlStatement(targetTree.Query);
                Debug.Assert(sqlStatement != null, "The outer most sql statement is null");
                sqlStatement.IsTopMost = true;
                result = sqlStatement;
            }
            else
            {
                var sqlBuilder = new SqlBuilder();
                sqlBuilder.Append("SELECT ");
                sqlBuilder.Append(targetTree.Query.Accept(this));

                result = sqlBuilder;
            }

            if (isVarRefSingle)
            {
                throw ADP1.NotSupported();
                // A DbVariableReferenceExpression has to be a child of DbPropertyExpression or MethodExpression
            }

            // Check that the parameter stacks are not leaking.
            Debug.Assert(selectStatementStack.Count == 0);
            Debug.Assert(isParentAJoinStack.Count == 0);

            var builder = new StringBuilder(1024);
            using (var writer = new SqlWriter(builder))
            {
                WriteSql(writer, result);
            }

            var commandTexts = new[] { builder.ToString() };
            return commandTexts;
        }

        // <summary>
        // Convert the SQL fragments to a string. Writes a string representing the SQL to be executed
        // into the specified writer.
        // </summary>
        // <param name="sqlStatement">The fragment to be emitted</param>
        // <returns>The writer specified for fluent continuations. </returns>
        public SqlWriter WriteSql(SqlWriter writer, ISqlFragment sqlStatement)
        {
            sqlStatement.WriteSql(writer, this);
            return writer;
        }

        #endregion

        #region IExpressionVisitor Members

        // <summary>
        // Translate(left) AND Translate(right)
        // </summary>
        // <returns>
        // A <see cref="SqlBuilder" /> .
        // </returns>
        public override ISqlFragment Visit(DbAndExpression e)
        {
            Check.NotNull(e, "e");

            return VisitBinaryExpression(" AND ", DbExpressionKind.And, e.Left, e.Right);
        }

        // <summary>
        // An apply is just like a join, so it shares the common join processing
        // in <see cref="VisitJoinExpression" />
        // </summary>
        // <returns>
        // A <see cref="SqlSelectStatement" /> .
        // </returns>
        public override ISqlFragment Visit(DbApplyExpression e)
        {
            Check.NotNull(e, "e");

            var inputs = new List<DbExpressionBinding>();
            inputs.Add(e.Input);
            inputs.Add(e.Apply);

            string joinString;
            switch (e.ExpressionKind)
            {
                case DbExpressionKind.CrossApply:
                    joinString = "CROSS APPLY";
                    break;

                case DbExpressionKind.OuterApply:
                    joinString = "OUTER APPLY";
                    break;

                default:
                    Debug.Assert(false);
                    throw ADP1.InvalidOperation(String.Empty);
            }

            // The join condition does not exist in this case, so we use null.
            // WE do not have a on clause, so we use JoinType.CrossJoin.
            return VisitJoinExpression(inputs, DbExpressionKind.CrossJoin, joinString, null);
        }

        // <summary>
        // For binary expressions, we delegate to <see cref="VisitBinaryExpression" />.
        // We handle the other expressions directly.
        // </summary>
        // <returns>
        // A <see cref="SqlBuilder" />
        // </returns>
        public override ISqlFragment Visit(DbArithmeticExpression e)
        {
            Check.NotNull(e, "e");

            SqlBuilder result;

            switch (e.ExpressionKind)
            {
                case DbExpressionKind.Divide:
                    result = VisitBinaryExpression(" / ", e.ExpressionKind, e.Arguments[0], e.Arguments[1]);
                    break;
                case DbExpressionKind.Minus:
                    result = VisitBinaryExpression(" - ", e.ExpressionKind, e.Arguments[0], e.Arguments[1]);
                    break;
                case DbExpressionKind.Modulo:
                    result = VisitBinaryExpression(" % ", e.ExpressionKind, e.Arguments[0], e.Arguments[1]);
                    break;
                case DbExpressionKind.Multiply:
                    result = VisitBinaryExpression(" * ", e.ExpressionKind, e.Arguments[0], e.Arguments[1]);
                    break;
                case DbExpressionKind.Plus:
                    result = VisitBinaryExpression(" + ", e.ExpressionKind, e.Arguments[0], e.Arguments[1]);
                    break;

                case DbExpressionKind.UnaryMinus:
                    result = new SqlBuilder();
                    result.Append(" -(");
                    result.Append(e.Arguments[0].Accept(this));
                    result.Append(")");
                    break;

                default:
                    Debug.Assert(false);
                    throw ADP1.InvalidOperation(String.Empty);
            }

            return result;
        }

        // <summary>
        // If the ELSE clause is null, we do not write it out.
        // </summary>
        // <returns>
        // A <see cref="SqlBuilder" />
        // </returns>
        public override ISqlFragment Visit(DbCaseExpression e)
        {
            Check.NotNull(e, "e");

            var result = new SqlBuilder();

            Debug.Assert(e.When.Count == e.Then.Count);

            result.Append("CASE");
            for (var i = 0; i < e.When.Count; ++i)
            {
                result.Append(" WHEN (");
                result.Append(e.When[i].Accept(this));
                result.Append(") THEN ");
                result.Append(e.Then[i].Accept(this));
            }
            // REVIEW: e.Else = DbNullExpression is added by the parser if there is
            // no else clause.  So, we do not add an ELSE clause here.
            if (e.Else != null
                && !(e.Else is DbNullExpression))
            {
                result.Append(" ELSE ");
                result.Append(e.Else.Accept(this));
            }

            result.Append(" END");

            return result;
        }

        public override ISqlFragment Visit(DbCastExpression e)
        {
            Check.NotNull(e, "e");

            var result = new SqlBuilder();
            result.Append(" CAST( ");
            result.Append(e.Argument.Accept(this));
            result.Append(" AS ");
            result.Append(GetSqlPrimitiveType(e.ResultType));
            result.Append(")");

            return result;
        }

        // <summary>
        // The parser generates Not(Equals(...)) for &lt;&gt;.
        // </summary>
        // <returns>
        // A <see cref="SqlBuilder" /> .
        // </returns>
        public override ISqlFragment Visit(DbComparisonExpression e)
        {
            Check.NotNull(e, "e");

            SqlBuilder result;
            switch (e.ExpressionKind)
            {
                case DbExpressionKind.Equals:
                    result = VisitComparisonExpression(" = ", e.Left, e.Right);
                    break;
                case DbExpressionKind.LessThan:
                    result = VisitComparisonExpression(" < ", e.Left, e.Right);
                    break;
                case DbExpressionKind.LessThanOrEquals:
                    result = VisitComparisonExpression(" <= ", e.Left, e.Right);
                    break;
                case DbExpressionKind.GreaterThan:
                    result = VisitComparisonExpression(" > ", e.Left, e.Right);
                    break;
                case DbExpressionKind.GreaterThanOrEquals:
                    result = VisitComparisonExpression(" >= ", e.Left, e.Right);
                    break;
                // The parser does not generate the expression kind below.
                case DbExpressionKind.NotEquals:
                    result = VisitComparisonExpression(" <> ", e.Left, e.Right);
                    break;

                default:
                    Debug.Assert(false); // The constructor should have prevented this
                    throw ADP1.InvalidOperation(String.Empty);
            }

            return result;
        }

        // <summary>
        // Generate tsql for a constant. Avoid the explicit cast (if possible) when
        // the isCastOptional parameter is set
        // </summary>
        // <param name="e"> the constant expression </param>
        // <param name="isCastOptional"> can we avoid the CAST </param>
        // <returns> the tsql fragment </returns>
        private static ISqlFragment VisitConstant(DbConstantExpression e, bool isCastOptional)
        {
            // Constants will be send to the store as part of the generated TSQL, not as parameters

            var result = new SqlBuilder();

            PrimitiveTypeKind typeKind;
            // Model Types can be (at the time of this implementation):
            //      Binary, Boolean, Byte, DateTime, Decimal, Double, Guid, Int16, Int32, Int64,Single, String
            if (TypeHelpers.TryGetPrimitiveTypeKind(e.ResultType, out typeKind))
            {
                switch (typeKind)
                {
                    case PrimitiveTypeKind.Int32:
                        // default sql server type for integral values.
                        result.Append(e.Value.ToString());
                        break;

                    case PrimitiveTypeKind.Binary:
                        result.Append(" 0x");
                        result.Append(ByteArrayToBinaryString((Byte[])e.Value));
                        result.Append(" ");
                        break;

                    case PrimitiveTypeKind.Boolean:
                        // Bugs 450277, 430294: Need to preserve the boolean type-ness of
                        // this value for round-trippability
                        WrapWithCastIfNeeded(!isCastOptional, (bool)e.Value ? "1" : "0", "bit", result);
                        break;

                    case PrimitiveTypeKind.Byte:
                        WrapWithCastIfNeeded(!isCastOptional, e.Value.ToString(), "tinyint", result);
                        break;

                    case PrimitiveTypeKind.DateTime:
                        result.Append("convert(");
                        result.Append("datetime");
                        result.Append(", ");
                        result.Append(
                            EscapeSingleQuote(
                                ((DateTime)e.Value).ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture), false /* IsUnicode */));
                        result.Append(", 121)");
                        break;

                    case PrimitiveTypeKind.Decimal:
                        var strDecimal = ((Decimal)e.Value).ToString(CultureInfo.InvariantCulture);
                        // if the decimal value has no decimal part, cast as decimal to preserve type
                        // if the number has precision > int64 max precision, it will be handled as decimal by sql server
                        // and does not need cast. if precision is lest then 20, then cast using Max(literal precision, sql default precision)
                        var needsCast = -1 == strDecimal.IndexOf('.') && (strDecimal.TrimStart(new[] { '-' }).Length < 20);

                        var precision = Math.Max((Byte)strDecimal.Length, defaultDecimalPrecision);
                        Debug.Assert(precision > 0, "Precision must be greater than zero");

                        var decimalType = "decimal(" + precision.ToString(CultureInfo.InvariantCulture) + ")";

                        WrapWithCastIfNeeded(needsCast, strDecimal, decimalType, result);
                        break;

                    case PrimitiveTypeKind.Double:
                        WrapWithCastIfNeeded(true, ((Double)e.Value).ToString("R", CultureInfo.InvariantCulture), "float", result);
                        break;

#if REVISIT_SUPPORT_FOR_GUID_CONSTANTS
#else
                    case PrimitiveTypeKind.Guid:
                        WrapWithCastIfNeeded(true, EscapeSingleQuote(e.Value.ToString(), false /* IsUnicode */), "uniqueidentifier", result);
                        break;
#endif

                    case PrimitiveTypeKind.Int16:
                        WrapWithCastIfNeeded(!isCastOptional, e.Value.ToString(), "smallint", result);
                        break;

                    case PrimitiveTypeKind.Int64:
                        WrapWithCastIfNeeded(!isCastOptional, e.Value.ToString(), "bigint", result);
                        break;

                    case PrimitiveTypeKind.Single:
                        WrapWithCastIfNeeded(true, ((Single)e.Value).ToString("R", CultureInfo.InvariantCulture), "real", result);
                        break;

                    case PrimitiveTypeKind.String:
                        bool isUnicode;
                        if (!TypeHelpers.TryGetIsUnicode(e.ResultType, out isUnicode))
                        {
                            isUnicode = true;
                        }
                        result.Append(EscapeSingleQuote(e.Value as string, isUnicode));
                        break;

                    case PrimitiveTypeKind.Time:
                        throw ADP1.NotSupported(EntityRes.GetString(EntityRes.ProviderDoesNotSupportType, "Time"));

                    case PrimitiveTypeKind.DateTimeOffset:
                        throw ADP1.NotSupported(EntityRes.GetString(EntityRes.ProviderDoesNotSupportType, "DateTimeOffset"));

                    default:
                        // all known scalar types should been handled already.
                        throw ADP1.NotSupported();
                }
            }
            else
            {
                throw ADP1.NotSupported();
                //if/when Enum types are supported, then handle appropriately, for now is not a valid type for constants.
                //result.Append(e.Value.ToString());
            }

            return result;
        }

        // <summary>
        // Helper function for <see cref="VisitConstant" />
        // Appends the given constant value to the result either 'as is' or wrapped with a cast to the given type.
        // </summary>
        // <param name="value">A SQL string or an ISqlFragment instance.</param>
        private static void WrapWithCastIfNeeded(bool cast, object value, string typeName, SqlBuilder result)
        {
            if (!cast)
            {
                result.Append(value);
            }
            else
            {
                result.Append("cast(");
                result.Append(value);
                result.Append(" as ");
                result.Append(typeName);
                result.Append(")");
            }
        }

        // <summary>
        // We do not pass constants as parameters.
        // </summary>
        // <returns>
        // A <see cref="SqlBuilder" /> . Strings are wrapped in single quotes and escaped. Numbers are written literally.
        // </returns>
        public override ISqlFragment Visit(DbConstantExpression e)
        {
            Check.NotNull(e, "e");

            return VisitConstant(e, false /* isCastOptional */);
        }

        public override ISqlFragment Visit(DbDerefExpression e)
        {
            Check.NotNull(e, "e");

            throw ADP1.NotSupported();
        }

        // <summary>
        // The DISTINCT has to be added to the beginning of SqlSelectStatement.Select,
        // but it might be too late for that.  So, we use a flag on SqlSelectStatement
        // instead, and add the "DISTINCT" in the second phase.
        // </summary>
        // <returns>
        // A <see cref="SqlSelectStatement" />
        // </returns>
        public override ISqlFragment Visit(DbDistinctExpression e)
        {
            Check.NotNull(e, "e");

            var result = VisitExpressionEnsureSqlStatement(e.Argument);

            if (!IsCompatible(result, e.ExpressionKind))
            {
                Symbol fromSymbol;
                var inputType = TypeHelpers.GetElementTypeUsage(e.Argument.ResultType);
                result = CreateNewSelectStatement(result, "distinct", inputType, out fromSymbol);
                AddFromSymbol(result, "distinct", fromSymbol, false);
            }

            result.IsDistinct = true;
            return result;
        }

        // <summary>
        // An element expression returns a scalar - so it is translated to
        // ( Select ... )
        // </summary>
        public override ISqlFragment Visit(DbElementExpression e)
        {
            Check.NotNull(e, "e");

            // ISSUE: What happens if the DbElementExpression is used as an input expression?
            // i.e. adding the '('  might not be right in all cases.
            var result = new SqlBuilder();
            result.Append("(");
            result.Append(VisitExpressionEnsureSqlStatement(e.Argument));
            result.Append(")");

            return result;
        }

        // <summary>
        // <see cref="Visit(DbUnionAllExpression)" />
        // </summary>
        public override ISqlFragment Visit(DbExceptExpression e)
        {
            Check.NotNull(e, "e");

            return TransformIntersectOrExcept(e.Left, e.Right, true);
        }

        // <summary>
        // Only concrete expression types will be visited.
        // </summary>
        public override ISqlFragment Visit(DbExpression e)
        {
            Check.NotNull(e, "e");

            throw ADP1.InvalidOperation(
                EntityRes.GetString(
                    EntityRes.UnknownExpressionType,
                    e.GetType().ToString()));
        }

        // <returns>
        // If we are in a Join context, returns a <see cref="SqlBuilder" /> with the extent name, otherwise, a new
        // <see
        //     cref="SqlSelectStatement" />
        // with the From field set.
        // </returns>
        public override ISqlFragment Visit(DbScanExpression e)
        {
            Check.NotNull(e, "e");

            var target = e.Target;

            // ISSUE: Should we just return a string all the time, and let
            // VisitInputExpression create the SqlSelectStatement?

            if (IsParentAJoin)
            {
                var result = new SqlBuilder();
                result.Append(GetTargetTSql(target));

                return result;
            }
            else
            {
                var result = new SqlSelectStatement();
                result.From.Append(GetTargetTSql(target));

                return result;
            }
        }

        // <summary>
        // Gets escaped TSql identifier describing this entity set.
        // </summary>
        internal static string GetTargetTSql(EntitySetBase entitySetBase)
        {
            MetadataProperty definingQuery;
            MetadataProperty table;
            string targetTSql;

            if (entitySetBase.MetadataProperties.TryGetValue("DefiningQuery", false, out definingQuery)
                &&
                null != definingQuery.Value)
            {
                targetTSql = "(" + (string)definingQuery.Value + ")";
            }
            else
            {
                // construct escaped T-SQL referencing entity set
                var builder = new StringBuilder(50);

                if (entitySetBase.MetadataProperties.TryGetValue("Table", false, out table)
                    &&
                    !string.IsNullOrEmpty((string)table.Value))
                {
                    builder.Append(QuoteIdentifier((string)table.Value));
                }
                else
                {
                    builder.Append(QuoteIdentifier(entitySetBase.Name));
                }
                targetTSql = builder.ToString();
            }

            return targetTSql;
        }

        // <summary>
        // The bodies of <see cref="Visit(DbFilterExpression)" />, <see cref="Visit(DbGroupByExpression)" />,
        // <see cref="Visit(DbProjectExpression)" />, <see cref="Visit(DbSortExpression)" /> are similar.
        // Each does the following.
        // <list type="number">
        //     <item>Visit the input expression</item>
        //     <item>
        //         Determine if the input's SQL statement can be reused, or a new
        //         one must be created.
        //     </item>
        //     <item>Create a new symbol table scope</item>
        //     <item>
        //         Push the Sql statement onto a stack, so that children can
        //         update the free variable list.
        //     </item>
        //     <item>Visit the non-input expression.</item>
        //     <item>Cleanup</item>
        // </list>
        // </summary>
        // <returns>
        // A <see cref="SqlSelectStatement" />
        // </returns>
        public override ISqlFragment Visit(DbFilterExpression e)
        {
            Check.NotNull(e, "e");

            return VisitFilterExpression(e.Input, e.Predicate, false);
        }

        // <summary>
        // Lambda functions are not supported.
        // The functions supported are:
        // <list type="number">
        //     <item>Canonical Functions - We recognize these by their dataspace, it is DataSpace.CSpace</item>
        //     <item>Store Functions - We recognize these by the BuiltInAttribute and not being Canonical</item>
        //     <item>User-defined Functions - All the rest except for Lambda functions</item>
        // </list>
        // We handle Canonical and Store functions the same way: If they are in the list of functions
        // that need special handling, we invoke the appropriate handler, otherwise we translate them to
        // FunctionName(arg1, arg2, ..., argn).
        // We translate user-defined functions to NamespaceName.FunctionName(arg1, arg2, ..., argn).
        // </summary>
        // <returns>
        // A <see cref="SqlBuilder" />
        // </returns>
        public override ISqlFragment Visit(DbFunctionExpression e)
        {
            Check.NotNull(e, "e");

            //
            // check if function requires special case processing, if so, delegates to it
            //
            if (IsSpecialCanonicalFunction(e))
            {
                return HandleSpecialCanonicalFunction(e);
            }

            if (IsSpecialStoreFunction(e))
            {
                return HandleSpecialStoreFunction(e);
            }

            return HandleFunctionDefault(e);
        }

        public override ISqlFragment Visit(DbEntityRefExpression e)
        {
            Check.NotNull(e, "e");

            throw ADP1.NotSupported();
        }

        public override ISqlFragment Visit(DbRefKeyExpression e)
        {
            Check.NotNull(e, "e");

            throw ADP1.NotSupported();
        }

        // <summary>
        // <see cref="Visit(DbFilterExpression)" /> for general details.
        // We modify both the GroupBy and the Select fields of the SqlSelectStatement.
        // GroupBy gets just the keys without aliases,
        // and Select gets the keys and the aggregates with aliases.
        // Whenever there exists at least one aggregate with an argument that is not is not a simple
        // <see cref="DbPropertyExpression" />  over <see cref="DbVariableReferenceExpression" />,
        // we create a nested query in which we alias the arguments to the aggregates.
        // That is due to the following two limitations of Sql Server:
        // <list type="number">
        //     <item>
        //         If an expression being aggregated contains an outer reference, then that outer
        //         reference must be the only column referenced in the expression (SQLBUDT #488741)
        //     </item>
        //     <item>
        //         Sql Server cannot perform an aggregate function on an expression containing
        //         an aggregate or a subquery. (SQLBUDT #504600)
        //     </item>
        // </list>
        // The default translation, without inner query is:
        // SELECT
        // kexp1 AS key1, kexp2 AS key2,... kexpn AS keyn,
        // aggf1(aexpr1) AS agg1, .. aggfn(aexprn) AS aggn
        // FROM input AS a
        // GROUP BY kexp1, kexp2, .. kexpn
        // When we inject an inner query, the equivalent translation is:
        // SELECT
        // key1 AS key1, key2 AS key2, .. keyn AS keys,
        // aggf1(agg1) AS agg1, aggfn(aggn) AS aggn
        // FROM (
        // SELECT
        // kexp1 AS key1, kexp2 AS key2,... kexpn AS keyn,
        // aexpr1 AS agg1, .. aexprn AS aggn
        // FROM input AS a
        // ) as a
        // GROUP BY key1, key2, keyn
        // </summary>
        // <returns>
        // A <see cref="SqlSelectStatement" />
        // </returns>
        public override ISqlFragment Visit(DbGroupByExpression e)
        {
            Check.NotNull(e, "e");

            Symbol fromSymbol;
            var innerQuery = VisitInputExpression(
                e.Input.Expression,
                e.Input.VariableName, e.Input.VariableType, out fromSymbol);

            // GroupBy is compatible with Filter and OrderBy
            // but not with Project, GroupBy
            if (!IsCompatible(innerQuery, e.ExpressionKind))
            {
                innerQuery = CreateNewSelectStatement(innerQuery, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
            }

            selectStatementStack.Push(innerQuery);
            symbolTable.EnterScope();

            AddFromSymbol(innerQuery, e.Input.VariableName, fromSymbol);
            // This line is not present for other relational nodes.
            symbolTable.Add(e.Input.GroupVariableName, fromSymbol);

            // The enumerator is shared by both the keys and the aggregates,
            // so, we do not close it in between.
            var groupByType = TypeHelpers.GetEdmType<RowType>(TypeHelpers.GetEdmType<CollectionType>(e.ResultType).TypeUsage);

            //SQL Server does not support arbitrarily complex expressions inside aggregates, 
            // and requires keys to have reference to the input scope, 
            // so we check for the specific restrictions and if need we inject an inner query.
            var needsInnerQuery = GroupByAggregatesNeedInnerQuery(e.Aggregates, e.Input.GroupVariableName)
                                  || GroupByKeysNeedInnerQuery(e.Keys, e.Input.VariableName);

            SqlSelectStatement result;
            if (needsInnerQuery)
            {
                //Create the inner query
                result = CreateNewSelectStatement(innerQuery, e.Input.VariableName, e.Input.VariableType, false, out fromSymbol);
                AddFromSymbol(result, e.Input.VariableName, fromSymbol, false);
            }
            else
            {
                result = innerQuery;
            }

            using (IEnumerator<EdmProperty> members = groupByType.Properties.GetEnumerator())
            {
                members.MoveNext();
                Debug.Assert(result.Select.IsEmpty);

                var separator = "";

                foreach (var key in e.Keys)
                {
                    var member = members.Current;
                    var alias = QuoteIdentifier(member.Name);

                    result.GroupBy.Append(separator);

                    var keySql = key.Accept(this);

                    if (!needsInnerQuery)
                    {
                        //Default translation: Key AS Alias
                        result.Select.Append(separator);
                        result.Select.AppendLine();
                        result.Select.Append(keySql);
                        result.Select.Append(" AS ");
                        result.Select.Append(alias);

                        result.GroupBy.Append(keySql);
                    }
                    else
                    {
                        // The inner query contains the default translation Key AS Alias
                        innerQuery.Select.Append(separator);
                        innerQuery.Select.AppendLine();
                        innerQuery.Select.Append(keySql);
                        innerQuery.Select.Append(" AS ");
                        innerQuery.Select.Append(alias);

                        //The outer resulting query projects over the key aliased in the inner query: 
                        //  fromSymbol.Alias AS Alias
                        result.Select.Append(separator);
                        result.Select.AppendLine();
                        result.Select.Append(fromSymbol);
                        result.Select.Append(".");
                        result.Select.Append(alias);
                        result.Select.Append(" AS ");
                        result.Select.Append(alias);

                        result.GroupBy.Append(alias);
                    }

                    separator = ", ";
                    members.MoveNext();
                }

                foreach (var aggregate in e.Aggregates)
                {
                    var member = members.Current;
                    var alias = QuoteIdentifier(member.Name);

                    Debug.Assert(aggregate.Arguments.Count == 1);
                    var translatedAggregateArgument = aggregate.Arguments[0].Accept(this);

                    object aggregateArgument;

                    if (needsInnerQuery)
                    {
                        //In this case the argument to the aggregate is reference to the one projected out by the
                        // inner query
                        var wrappingAggregateArgument = new SqlBuilder();
                        wrappingAggregateArgument.Append(fromSymbol);
                        wrappingAggregateArgument.Append(".");
                        wrappingAggregateArgument.Append(alias);
                        aggregateArgument = wrappingAggregateArgument;

                        innerQuery.Select.Append(separator);
                        innerQuery.Select.AppendLine();
                        innerQuery.Select.Append(translatedAggregateArgument);
                        innerQuery.Select.Append(" AS ");
                        innerQuery.Select.Append(alias);
                    }
                    else
                    {
                        aggregateArgument = translatedAggregateArgument;
                    }

                    ISqlFragment aggregateResult = VisitAggregate(aggregate, aggregateArgument);

                    result.Select.Append(separator);
                    result.Select.AppendLine();
                    result.Select.Append(aggregateResult);
                    result.Select.Append(" AS ");
                    result.Select.Append(alias);

                    separator = ", ";
                    members.MoveNext();
                }
            }

            symbolTable.ExitScope();
            selectStatementStack.Pop();

            return result;
        }

        // <summary>
        // <see cref="Visit(DbUnionAllExpression)" />
        // </summary>
        public override ISqlFragment Visit(DbIntersectExpression e)
        {
            Check.NotNull(e, "e");

            return TransformIntersectOrExcept(e.Left, e.Right, false);
        }

        // <summary>
        // Not(IsEmpty) has to be handled specially, so we delegate to
        // <see cref="VisitIsEmptyExpression" />.
        // </summary>
        // <returns>
        // A <see cref="SqlBuilder" /> . <code>[NOT] EXISTS( ... )</code>
        // </returns>
        public override ISqlFragment Visit(DbIsEmptyExpression e)
        {
            Check.NotNull(e, "e");

            return VisitIsEmptyExpression(e, false);
        }

        // <summary>
        // Not(IsNull) is handled specially, so we delegate to
        // <see cref="VisitIsNullExpression" />
        // </summary>
        // <returns>
        // A <see cref="SqlBuilder" /> <code>IS [NOT] NULL</code>
        // </returns>
        public override ISqlFragment Visit(DbIsNullExpression e)
        {
            Check.NotNull(e, "e");

            return VisitIsNullExpression(e, false);
        }

        // <summary>
        // No error is raised if the store cannot support this.
        // </summary>
        // <returns>
        // A <see cref="SqlBuilder" />
        // </returns>
        public override ISqlFragment Visit(DbIsOfExpression e)
        {
            Check.NotNull(e, "e");

            throw ADP1.NotSupported();
        }

        // <summary>
        // <see cref="VisitJoinExpression" />
        // </summary>
        // <returns>
        // A <see cref="SqlSelectStatement" /> .
        // </returns>
        public override ISqlFragment Visit(DbCrossJoinExpression e)
        {
            Check.NotNull(e, "e");

            return VisitJoinExpression(e.Inputs, e.ExpressionKind, "CROSS JOIN", null);
        }

        // <summary>
        // <see cref="VisitJoinExpression" />
        // </summary>
        // <returns>
        // A <see cref="SqlSelectStatement" /> .
        // </returns>
        public override ISqlFragment Visit(DbJoinExpression e)
        {
            Check.NotNull(e, "e");

            if (e.ExpressionKind
                == DbExpressionKind.FullOuterJoin)
            {
                throw ADP1.FullOuterJoinNotSupportedException();
            }

            #region Map join type to a string

            string joinString;
            switch (e.ExpressionKind)
            {
                case DbExpressionKind.InnerJoin:
                    joinString = "INNER JOIN";
                    break;

                case DbExpressionKind.LeftOuterJoin:
                    joinString = "LEFT OUTER JOIN";
                    break;

                default:
                    Debug.Assert(false);
                    joinString = null;
                    break;
            }

            #endregion

            var inputs = new List<DbExpressionBinding>(2);
            inputs.Add(e.Left);
            inputs.Add(e.Right);

            return VisitJoinExpression(inputs, e.ExpressionKind, joinString, e.JoinCondition);
        }

        // <returns>
        // A <see cref="SqlBuilder" />
        // </returns>
        public override ISqlFragment Visit(DbLikeExpression e)
        {
            Check.NotNull(e, "e");

            var result = new SqlBuilder();
            result.Append(e.Argument.Accept(this));
            result.Append(" LIKE ");
            result.Append(e.Pattern.Accept(this));

            // if the ESCAPE expression is a DbNullExpression, then that's tantamount to 
            // not having an ESCAPE at all
            if (e.Escape.ExpressionKind
                != DbExpressionKind.Null)
            {
                result.Append(" ESCAPE ");
                result.Append(e.Escape.Accept(this));
            }

            return result;
        }

        // <summary>
        // Translates to TOP expression.
        // </summary>
        // <returns>
        // A <see cref="SqlBuilder" />
        // </returns>
        public override ISqlFragment Visit(DbLimitExpression e)
        {
            Check.NotNull(e, "e");

            Debug.Assert(
                e.Limit is DbConstantExpression || e.Limit is DbParameterReferenceExpression,
                "DbLimitExpression.Limit is of invalid expression type");

            // Throw an error if LimitExpression.WithTies is true as 
            // QP does not support it.
            //
            if (e.WithTies)
            {
                throw ADP1.WithTiesNotSupportedException();
            }

            var result = VisitExpressionEnsureSqlStatement(e.Argument, false);
            Symbol fromSymbol;

            if (!IsCompatible(result, e.ExpressionKind))
            {
                var inputType = TypeHelpers.GetElementTypeUsage(e.Argument.ResultType);

                result = CreateNewSelectStatement(result, "top", inputType, out fromSymbol);
                AddFromSymbol(result, "top", fromSymbol, false);
            }

            var topCount = HandleCountExpression(e.Limit);

            result.Top = new TopClause(topCount, false /*e.WithTies*/);
            return result;
        }

        // <summary>
        // DbNewInstanceExpression is allowed as a child of DbProjectExpression only.
        // If anyone else is the parent, we throw.
        // We also perform special casing for collections - where we could convert
        // them into Unions
        // <see cref="VisitNewInstanceExpression" /> for the actual implementation.
        // </summary>
        public override ISqlFragment Visit(DbNewInstanceExpression e)
        {
            Check.NotNull(e, "e");

            if (TypeSemantics.IsCollectionType(e.ResultType))
            {
                return VisitCollectionConstructor(e);
            }
            throw ADP1.NotSupported();
        }

        // <summary>
        // The Not expression may cause the translation of its child to change.
        // These children are
        // <list type="bullet">
        //     <item>
        //         <see cref="DbNotExpression" />
        //         NOT(Not(x)) becomes x
        //     </item>
        //     <item>
        //         <see cref="DbIsEmptyExpression" />
        //         NOT EXISTS becomes EXISTS
        //     </item>
        //     <item>
        //         <see cref="DbIsNullExpression" />
        //         IS NULL becomes IS NOT NULL
        //     </item>
        //     <item>
        //         <see cref="DbComparisonExpression" />
        //         = becomes&lt;&gt;
        //     </item>
        // </list>
        // </summary>
        // <returns>
        // A <see cref="SqlBuilder" />
        // </returns>
        public override ISqlFragment Visit(DbNotExpression e)
        {
            Check.NotNull(e, "e");

            // Flatten Not(Not(x)) to x.
            var notExpression = e.Argument as DbNotExpression;
            if (notExpression != null)
            {
                return notExpression.Argument.Accept(this);
            }

            var isEmptyExpression = e.Argument as DbIsEmptyExpression;
            if (isEmptyExpression != null)
            {
                return VisitIsEmptyExpression(isEmptyExpression, true);
            }

            var isNullExpression = e.Argument as DbIsNullExpression;
            if (isNullExpression != null)
            {
                return VisitIsNullExpression(isNullExpression, true);
            }

            var comparisonExpression = e.Argument as DbComparisonExpression;
            if (comparisonExpression != null)
            {
                if (comparisonExpression.ExpressionKind
                    == DbExpressionKind.Equals)
                {
                    return VisitBinaryExpression(" <> ", DbExpressionKind.NotEquals, comparisonExpression.Left, comparisonExpression.Right);
                }
            }

            var result = new SqlBuilder();
            result.Append(" NOT (");
            result.Append(e.Argument.Accept(this));
            result.Append(")");

            return result;
        }

        // <returns>
        // <see cref="SqlBuilder" />
        // </returns>
        public override ISqlFragment Visit(DbNullExpression e)
        {
            Check.NotNull(e, "e");

            var result = new SqlBuilder();
            // always cast nulls - sqlserver doesn't like case expressions where the "then" clause is null
            result.Append("CAST(NULL AS ");
            var type = e.ResultType;

            //
            // Use the narrowest type possible - especially for strings where we don't want 
            // to produce unicode strings always.
            //
            Debug.Assert(TypeSemantics.IsPrimitiveType(type.EdmType), "Type must be primitive type");
            var primitiveType = type.EdmType as PrimitiveType;
            switch (primitiveType.PrimitiveTypeKind)
            {
                case PrimitiveTypeKind.String:
                    result.Append("nvarchar(1)");
                    break;
                case PrimitiveTypeKind.Binary:
                    result.Append("varbinary(1)");
                    break;
                default:
                    result.Append(GetSqlPrimitiveType(type));
                    break;
            }

            result.Append(")");
            return result;
        }

        // <returns>
        // A <see cref="SqlBuilder" />
        // </returns>
        public override ISqlFragment Visit(DbOfTypeExpression e)
        {
            Check.NotNull(e, "e");

            throw ADP1.NotSupported();
        }

        // <returns>
        // A <see cref="SqlBuilder" />
        // </returns>
        // <seealso cref="Visit(DbAndExpression)" />
        public override ISqlFragment Visit(DbOrExpression e)
        {
            Check.NotNull(e, "e");

            return VisitBinaryExpression(" OR ", e.ExpressionKind, e.Left, e.Right);
        }

        // <summary>
        // Visits a DbInExpression and generates the corresponding SQL fragment.
        // </summary>
        // <param name="e"> A <see cref="DbInExpression" /> that specifies the expression to be visited. </param>
        // <returns>
        // A <see cref="SqlBuilder" /> that specifies the generated SQL fragment.
        // </returns>
        public override ISqlFragment Visit(DbInExpression e)
        {
            Check.NotNull(e, "e");

            if (e.List.Count == 0)
            {
                return Visit(DbExpressionBuilder.False);
            }

            var result = new SqlBuilder();

            result.Append(e.Item.Accept(this));
            result.Append(" IN (");

            var first = true;
            foreach (var item in e.List)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    result.Append(", ");
                }

                result.Append(item.Accept(this));
            }

            result.Append(")");

            return result;
        }

        //<returns> A <see cref="SqlBuilder" /> </returns>
        public override ISqlFragment Visit(DbParameterReferenceExpression e)
        {
            Check.NotNull(e, "e");

            var result = new SqlBuilder();
            // Do not quote this name.
            // ISSUE: We are not checking that e.Name has no illegal characters. e.g. space
            result.Append("@" + e.ParameterName);

            return result;
        }

        // <summary>
        // <see cref="Visit(DbFilterExpression)" /> for the general ideas.
        // </summary>
        // <returns>
        // A <see cref="SqlSelectStatement" />
        // </returns>
        // <seealso cref="Visit(DbFilterExpression)" />
        public override ISqlFragment Visit(DbProjectExpression e)
        {
            Check.NotNull(e, "e");

            // No need to rename the aliases in SQLCE. EF code uses it as this is an issue in Sql8.
            // So no need to set aliasesNeedRenaming and OutputColumnsRenamed properties.

            Symbol fromSymbol;
            var result = VisitInputExpression(e.Input.Expression, e.Input.VariableName, e.Input.VariableType, out fromSymbol);

            // Project is compatible with Filter
            // but not with Project, GroupBy
            if (!IsCompatible(result, e.ExpressionKind))
            {
                result = CreateNewSelectStatement(result, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
            }

            selectStatementStack.Push(result);
            symbolTable.EnterScope();

            AddFromSymbol(result, e.Input.VariableName, fromSymbol);

            // Project is the only node that can have DbNewInstanceExpression as a child
            // so we have to check it here.
            // We call VisitNewInstanceExpression instead of Visit(DbNewInstanceExpression), since
            // the latter throws.
            var separator = " OUTER APPLY";
            var ssqNameIterator = 0;
            // this flag is used to indicate if one element expression is already found
            var foundElementExpression = false;
            var newInstanceExpression = e.Projection as DbNewInstanceExpression;
            if (newInstanceExpression != null)
            {
                int i;
                // this flag is used to indicate if one element expression is already found
                for (i = 0; i < newInstanceExpression.Arguments.Count; i++)
                {
                    var argument = newInstanceExpression.Arguments[i];
                    if (argument.ExpressionKind
                        == DbExpressionKind.Element)
                    {
                        if (false == foundElementExpression)
                        {
                            foundElementExpression = true;
                            if (topElementExpression)
                            {
                                // throw and not supported exception
                                throw ADP1.NotSupported();
                            }
                            else
                            {
                                topElementExpression = true;
                            }
                        }
                        result.From.AppendLine();
                        result.From.Append(separator);
                        result.From.AppendLine();
                        result.From.Append(argument.Accept(this));

                        string subQueryTableName;
                        do
                        {
                            ssqNameIterator++;
                            subQueryTableName = "SSQTAB" + ssqNameIterator;
                        }
                        while (null != symbolTable.Lookup(subQueryTableName));
                        var subQueryTableSymbol = new Symbol(subQueryTableName, argument.ResultType);
                        AddFromSymbol(result, subQueryTableName, subQueryTableSymbol);
                        listOfScalarSubQueryTables.Add(subQueryTableName);
                        separator = ", ";
                    }
                }
                result.Select.Append(VisitNewInstanceExpression(newInstanceExpression));
            }
            else
            {
                result.Select.Append(e.Projection.Accept(this));
            }
            if (foundElementExpression && topElementExpression)
            {
                topElementExpression = false; // reseting the value
            }
            symbolTable.ExitScope();
            selectStatementStack.Pop();

            return result;
        }

        // <summary>
        // This method handles record flattening, which works as follows.
        // consider an expression <c>Prop(y, Prop(x, Prop(d, Prop(c, Prop(b, Var(a)))))</c>
        // where a,b,c are joins, d is an extent and x and y are fields.
        // b has been flattened into a, and has its own SELECT statement.
        // c has been flattened into b.
        // d has been flattened into c.
        // We visit the instance, so we reach Var(a) first.  This gives us a (join)symbol.
        // Symbol(a).b gives us a join symbol, with a SELECT statement i.e. Symbol(b).
        // From this point on , we need to remember Symbol(b) as the source alias,
        // and then try to find the column.  So, we use a SymbolPair.
        // We have reached the end when the symbol no longer points to a join symbol.
        // </summary>
        // <returns>
        // A <see cref="JoinSymbol" /> if we have not reached the first Join node that has a SELECT statement. A
        // <see
        //     cref="SymbolPair" />
        // if we have seen the JoinNode, and it has a SELECT statement. A <see cref="SqlBuilder" /> with {Input}.propertyName otherwise.
        // </returns>
        public override ISqlFragment Visit(DbPropertyExpression e)
        {
            Check.NotNull(e, "e");

            SqlBuilder result;

            var instanceSql = e.Instance.Accept(this);

            // Since the DbVariableReferenceExpression is a proper child of ours, we can reset
            // isVarSingle.
            var VariableReferenceExpression = e.Instance as DbVariableReferenceExpression;
            if (VariableReferenceExpression != null)
            {
                isVarRefSingle = false;
            }

            // We need to flatten, and have not yet seen the first nested SELECT statement.
            var joinSymbol = instanceSql as JoinSymbol;
            if (joinSymbol != null)
            {
                Debug.Assert(joinSymbol.NameToExtent.ContainsKey(e.Property.Name));
                if (joinSymbol.IsNestedJoin)
                {
                    return new SymbolPair(joinSymbol, joinSymbol.NameToExtent[e.Property.Name]);
                }
                else
                {
                    return joinSymbol.NameToExtent[e.Property.Name];
                }
            }

            // ---------------------------------------
            // We have seen the first nested SELECT statement, but not the column.
            var symbolPair = instanceSql as SymbolPair;
            if (symbolPair != null)
            {
                var columnJoinSymbol = symbolPair.Column as JoinSymbol;
                if (columnJoinSymbol != null)
                {
                    symbolPair.Column = columnJoinSymbol.NameToExtent[e.Property.Name];
                    return symbolPair;
                }
                else
                {
                    // symbolPair.Column has the base extent.
                    // we need the symbol for the column, since it might have been renamed
                    // when handling a JOIN.
                    if (symbolPair.Column.Columns.ContainsKey(e.Property.Name))
                    {
                        result = new SqlBuilder();
                        result.Append(symbolPair.Source);
                        result.Append(".");
                        result.Append(symbolPair.Column.Columns[e.Property.Name]);
                        return result;
                    }
                }
            }
            // ---------------------------------------

            result = new SqlBuilder();
            result.Append(instanceSql);
            result.Append(".");

            var symbol = instanceSql as Symbol;

            if (symbol != null
                && symbol.OutputColumnsRenamed)
            {
                result.Append(symbol.Columns[e.Property.Name]);
            }
            else
            {
                // At this point the column name cannot be renamed, so we do
                // not use a symbol.
                result.Append(QuoteIdentifier(e.Property.Name));
            }
            return result;
        }

        // <summary>
        // Any(input, x) => Exists(Filter(input,x))
        // All(input, x) => Not Exists(Filter(input, not(x))
        // </summary>
        public override ISqlFragment Visit(DbQuantifierExpression e)
        {
            Check.NotNull(e, "e");

            var result = new SqlBuilder();

            var negatePredicate = (e.ExpressionKind == DbExpressionKind.All);
            if (e.ExpressionKind
                == DbExpressionKind.Any)
            {
                result.Append("EXISTS (");
            }
            else
            {
                Debug.Assert(e.ExpressionKind == DbExpressionKind.All);
                result.Append("NOT EXISTS (");
            }

            var filter = VisitFilterExpression(e.Input, e.Predicate, negatePredicate);
            if (filter.Select.IsEmpty)
            {
                AddDefaultColumns(filter);
            }

            result.Append(filter);
            result.Append(")");

            return result;
        }

        public override ISqlFragment Visit(DbRefExpression e)
        {
            Check.NotNull(e, "e");

            throw ADP1.NotSupported();
        }

        public override ISqlFragment Visit(DbRelationshipNavigationExpression e)
        {
            Check.NotNull(e, "e");

            throw ADP1.NotSupported();
        }

        // <summary>
        // For Sql9 it translates to:
        // SELECT Y.x1, Y.x2, ..., Y.xn
        // FROM (
        // SELECT X.x1, X.x2, ..., X.xn, row_number() OVER (ORDER BY sk1, sk2, ...) AS [row_number]
        // FROM input as X
        // ) as Y
        // WHERE Y.[row_number] > count
        // ORDER BY sk1, sk2, ...
        // </summary>
        // <returns>
        // A <see cref="SqlBuilder" />
        // </returns>
        public override ISqlFragment Visit(DbSkipExpression e)
        {
            Check.NotNull(e, "e");

            Symbol fromSymbol;
            var result = VisitInputExpression(e.Input.Expression, e.Input.VariableName, e.Input.VariableType, out fromSymbol);

            // Check compatiblity.
            // If the operators are not compatible, a new sql statement must be generated.
            // 
            if (!IsCompatible(result, e.ExpressionKind))
            {
                result = CreateNewSelectStatement(result, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
            }

            selectStatementStack.Push(result);
            symbolTable.EnterScope();

            AddFromSymbol(result, e.Input.VariableName, fromSymbol);

            AddSortKeys(result.OrderBy, e.SortOrder);

            var skipCount = HandleCountExpression(e.Count);
            result.Skip = new SkipClause(skipCount);

            symbolTable.ExitScope();
            selectStatementStack.Pop();

            return result;
        }

        // <summary>
        // <see cref="Visit(DbFilterExpression)" />
        // </summary>
        // <returns>
        // A <see cref="SqlSelectStatement" />
        // </returns>
        // <seealso cref="Visit(DbFilterExpression)" />
        public override ISqlFragment Visit(DbSortExpression e)
        {
            Check.NotNull(e, "e");

            Symbol fromSymbol;
            var result = VisitInputExpression(e.Input.Expression, e.Input.VariableName, e.Input.VariableType, out fromSymbol);

            // OrderBy is compatible with Filter
            // and nothing else
            if (!IsCompatible(result, e.ExpressionKind))
            {
                result = CreateNewSelectStatement(result, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
            }

            selectStatementStack.Push(result);
            symbolTable.EnterScope();

            AddFromSymbol(result, e.Input.VariableName, fromSymbol);

            AddSortKeys(result.OrderBy, e.SortOrder);

            symbolTable.ExitScope();
            selectStatementStack.Pop();

            return result;
        }

        // <returns>
        // A <see cref="SqlBuilder" />
        // </returns>
        public override ISqlFragment Visit(DbTreatExpression e)
        {
            Check.NotNull(e, "e");

            throw ADP1.NotSupported();
        }

        // <summary>
        // This code is shared by <see cref="Visit(DbExceptExpression)" />
        // and <see cref="Visit(DbIntersectExpression)" />
        // <see cref="VisitSetOpExpression" />
        // Since the left and right expression may not be Sql select statements,
        // we must wrap them up to look like SQL select statements.
        // </summary>
        public override ISqlFragment Visit(DbUnionAllExpression e)
        {
            Check.NotNull(e, "e");

            return VisitSetOpExpression(e, "UNION ALL");
        }

        // <summary>
        // This method determines whether an extent from an outer scope(free variable)
        // is used in the CurrentSelectStatement.
        // An extent in an outer scope, if its symbol is not in the FromExtents
        // of the CurrentSelectStatement.
        // </summary>
        // <returns>
        // A <see cref="Symbol" /> .
        // </returns>
        public override ISqlFragment Visit(DbVariableReferenceExpression e)
        {
            Check.NotNull(e, "e");

            if (isVarRefSingle)
            {
                throw ADP1.NotSupported();
                // A DbVariableReferenceExpression has to be a child of DbPropertyExpression or MethodExpression
                // This is also checked in GenerateSql(...) at the end of the visiting.
            }
            isVarRefSingle = true; // This will be reset by DbPropertyExpression or MethodExpression

            var result = symbolTable.Lookup(e.VariableName);
            if (!CurrentSelectStatement.FromExtents.Contains(result))
            {
                CurrentSelectStatement.OuterExtents[result] = true;
            }

            return result;
        }

        #endregion

        #region Visitor Helper Methods

        #region 'Visitor' methods - Shared visitors and methods that do most of the visiting

        // <summary>
        // Aggregates are not visited by the normal visitor walk.
        // </summary>
        // <param name="aggregate"> The aggreate go be translated </param>
        // <param name="aggregateArgument"> The translated aggregate argument </param>
        private static SqlBuilder VisitAggregate(DbAggregate aggregate, object aggregateArgument)
        {
            var aggregateFunction = new SqlBuilder();
            var aggregateResult = new SqlBuilder();
            var functionAggregate = aggregate as DbFunctionAggregate;
            bool fCast;
            string castType;

            if (functionAggregate == null)
            {
                throw ADP1.NotSupported(String.Empty);
            }

            WriteFunctionName(aggregateFunction, functionAggregate.Function, out fCast, out castType);

            if (fCast)
            {
                aggregateResult.Append("CAST(");
            }

            aggregateResult.Append(aggregateFunction);
            aggregateResult.Append("(");

            var fnAggr = functionAggregate;
            if ((null != fnAggr)
                && (fnAggr.Distinct))
            {
                throw ADP1.NotSupported(EntityRes.GetString(EntityRes.DistinctAggregatesNotSupported));
            }

            aggregateResult.Append(aggregateArgument);

            aggregateResult.Append(")");

            if (fCast)
            {
                aggregateResult.Append(" as " + castType + ")");
            }

            return aggregateResult;
        }

        // <summary>
        // Dump out an expression - optionally wrap it with parantheses if possible
        // </summary>
        private void ParanthesizeExpressionIfNeeded(DbExpression e, SqlBuilder result)
        {
            if (IsComplexExpression(e))
            {
                result.Append("(");
                result.Append(e.Accept(this));
                result.Append(")");
            }
            else
            {
                result.Append(e.Accept(this));
            }
        }

        // <summary>
        // Handler for inline binary expressions.
        // Produces left op right.
        // For associative operations does flattening.
        // Puts parenthesis around the arguments if needed.
        // </summary>
        private SqlBuilder VisitBinaryExpression(string op, DbExpressionKind expressionKind, DbExpression left, DbExpression right)
        {
            var result = new SqlBuilder();

            var isFirst = true;
            foreach (var argument in CommandTreeUtils.FlattenAssociativeExpression(expressionKind, left, right))
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    result.Append(op);
                }
                ParanthesizeExpressionIfNeeded(argument, result);
            }
            return result;
        }

        // <summary>
        // Private handler for comparison expressions - almost identical to VisitBinaryExpression.
        // We special case constants, so that we don't emit unnecessary casts
        // </summary>
        // <param name="op"> the comparison op </param>
        // <param name="left"> the left-side expression </param>
        // <param name="right"> the right-side expression </param>
        private SqlBuilder VisitComparisonExpression(string op, DbExpression left, DbExpression right)
        {
            var result = new SqlBuilder();
            var isCastOptional = left.ResultType.EdmType == right.ResultType.EdmType;

            if (left.ExpressionKind
                == DbExpressionKind.Constant)
            {
                result.Append(VisitConstant((DbConstantExpression)left, isCastOptional));
            }
            else
            {
                ParanthesizeExpressionIfNeeded(left, result);
            }

            result.Append(op);

            if (right.ExpressionKind
                == DbExpressionKind.Constant)
            {
                result.Append(VisitConstant((DbConstantExpression)right, isCastOptional));
            }
            else
            {
                ParanthesizeExpressionIfNeeded(right, result);
            }

            return result;
        }

        // <summary>
        // This is called by the relational nodes.  It does the following
        // <list>
        //     <item>
        //         If the input is not a SqlSelectStatement, it assumes that the input
        //         is a collection expression, and creates a new SqlSelectStatement
        //     </item>
        // </list>
        // </summary>
        // <returns>
        // A <see cref="SqlSelectStatement" /> and the main fromSymbol for this select statement.
        // </returns>
        private SqlSelectStatement VisitInputExpression(
            DbExpression inputExpression,
            string inputVarName, TypeUsage inputVarType, out Symbol fromSymbol)
        {
            SqlSelectStatement result;
            var sqlFragment = inputExpression.Accept(this);
            result = sqlFragment as SqlSelectStatement;

            if (result == null)
            {
                result = new SqlSelectStatement();
                WrapNonQueryExtent(result, sqlFragment, inputExpression.ExpressionKind);
            }

            if (result.FromExtents.Count == 0)
            {
                // input was an extent
                fromSymbol = new Symbol(inputVarName, inputVarType);
            }
            else if (result.FromExtents.Count == 1)
            {
                // input was Filter/GroupBy/Project/OrderBy
                // we are likely to reuse this statement.
                fromSymbol = result.FromExtents[0];
            }
            else
            {
                // input was a join.
                // we are reusing the select statement produced by a Join node
                // we need to remove the original extents, and replace them with a
                // new extent with just the Join symbol.
                var joinSymbol = new JoinSymbol(inputVarName, inputVarType, result.FromExtents);
                joinSymbol.FlattenedExtentList = result.AllJoinExtents;

                fromSymbol = joinSymbol;
                result.FromExtents.Clear();
                result.FromExtents.Add(fromSymbol);
            }

            return result;
        }

        // <summary>
        // <see cref="Visit(DbIsEmptyExpression)" />
        // </summary>
        // <param name="negate"> Was the parent a DbNotExpression? </param>
        private SqlBuilder VisitIsEmptyExpression(DbIsEmptyExpression e, bool negate)
        {
            var result = new SqlBuilder();
            if (!negate)
            {
                result.Append(" NOT");
            }
            result.Append(" EXISTS (");
            result.Append(VisitExpressionEnsureSqlStatement(e.Argument));
            result.AppendLine();
            result.Append(")");

            return result;
        }

        // <summary>
        // Translate a NewInstance(Element(X)) expression into
        // "select top(1) * from X"
        // </summary>
        private ISqlFragment VisitCollectionConstructor(DbNewInstanceExpression e)
        {
            Debug.Assert(e.Arguments.Count <= 1);

            if (e.Arguments.Count == 1
                && e.Arguments[0].ExpressionKind == DbExpressionKind.Element)
            {
                var elementExpr = e.Arguments[0] as DbElementExpression;
                var result = VisitExpressionEnsureSqlStatement(elementExpr.Argument);

                if (!IsCompatible(result, DbExpressionKind.Element))
                {
                    Symbol fromSymbol;
                    var inputType = TypeHelpers.GetElementTypeUsage(elementExpr.Argument.ResultType);

                    result = CreateNewSelectStatement(result, "element", inputType, out fromSymbol);
                    AddFromSymbol(result, "element", fromSymbol, false);
                }
                result.Top = new TopClause(1, false);
                return result;
            }

            // Otherwise simply build this out as a union-all ladder
            var collectionType = TypeHelpers.GetEdmType<CollectionType>(e.ResultType);
            Debug.Assert(collectionType != null);
            var isScalarElement = TypeSemantics.IsPrimitiveType(collectionType.TypeUsage);

            var resultSql = new SqlBuilder();
            var separator = "";

            // handle empty table
            if (e.Arguments.Count == 0)
            {
                Debug.Assert(isScalarElement);
                resultSql.Append(" SELECT CAST(null as ");
                resultSql.Append(GetSqlPrimitiveType(collectionType.TypeUsage));
                resultSql.Append(") AS X FROM (SELECT 1) AS Y WHERE 1=0");
            }

            foreach (var arg in e.Arguments)
            {
                resultSql.Append(separator);
                resultSql.Append(" SELECT ");
                resultSql.Append(arg.Accept(this));
                // For scalar elements, no alias is appended yet. Add this.
                if (isScalarElement)
                {
                    resultSql.Append(" AS X ");
                }
                separator = " UNION ALL ";
            }

            return resultSql;
        }

        // <summary>
        // <see cref="Visit(DbIsNullExpression)" />
        // </summary>
        // <param name="negate"> Was the parent a DbNotExpression? </param>
        private SqlBuilder VisitIsNullExpression(DbIsNullExpression e, bool negate)
        {
            // Codeplex workitem #287: SqlCeProviderServices.CreateSqlCeParameter does not supply 
            // the parameter type for strings and blobs if the parameter size is not available, 
            // thus letting the QP to infer the type at execution time. That happpens because the 
            // default types, ntext and image, are not comparable, so a simple predicate like 
            // WHERE table.Column = @parameter would fail. However the inference is not possible
            // when there is an IS NULL comparison, in which case we explicitly cast to ntext 
            // and respectively image.
            // NOTE: SqlProviderServices does not have this issue because it defaults to 
            // nvarchar(max) and varbinary(max) instead of ntext and image, which cannot be done
            // for SQL CE because max is not available. 

            string castAsType = null;
            if (e.Argument.ExpressionKind == DbExpressionKind.ParameterReference)
            {
                var resultType = e.Argument.ResultType;
                int maxLength;
                if (!TypeHelpers.TryGetMaxLength(resultType, out maxLength))
                {
                    PrimitiveTypeKind primitiveTypeKind;
                    if (TypeHelpers.TryGetPrimitiveTypeKind(resultType, out primitiveTypeKind))
                    {
                        switch (primitiveTypeKind)
                        {
                            case PrimitiveTypeKind.String:
                                castAsType = "ntext";
                                break;
                            case PrimitiveTypeKind.Binary:
                                castAsType = "image";
                                break;
                        }
                    }
                }
            }

            var result = new SqlBuilder();
            WrapWithCastIfNeeded(castAsType != null, e.Argument.Accept(this), castAsType, result);

            if (!negate)
            {
                result.Append(" IS NULL");
            }
            else
            {
                result.Append(" IS NOT NULL");
            }

            return result;
        }

        // <summary>
        // This handles the processing of join expressions.
        // The extents on a left spine are flattened, while joins
        // not on the left spine give rise to new nested sub queries.
        // Joins work differently from the rest of the visiting, in that
        // the parent (i.e. the join node) creates the SqlSelectStatement
        // for the children to use.
        // The "parameter" IsInJoinContext indicates whether a child extent should
        // add its stuff to the existing SqlSelectStatement, or create a new SqlSelectStatement
        // By passing true, we ask the children to add themselves to the parent join,
        // by passing false, we ask the children to create new Select statements for
        // themselves.
        // This method is called from <see cref="Visit(DbApplyExpression)" /> and
        // <see cref="Visit(DbJoinExpression)" />.
        // </summary>
        // <returns>
        // A <see cref="SqlSelectStatement" />
        // </returns>
        private ISqlFragment VisitJoinExpression(
            IList<DbExpressionBinding> inputs, DbExpressionKind joinKind,
            string joinString, DbExpression joinCondition)
        {
            SqlSelectStatement result;
            // If the parent is not a join( or says that it is not),
            // we should create a new SqlSelectStatement.
            // otherwise, we add our child extents to the parent's FROM clause.
            if (!IsParentAJoin)
            {
                result = new SqlSelectStatement();
                result.AllJoinExtents = new List<Symbol>();
                selectStatementStack.Push(result);
            }
            else
            {
                result = CurrentSelectStatement;
            }

            // Process each of the inputs, and then the joinCondition if it exists.
            // It would be nice if we could call VisitInputExpression - that would
            // avoid some code duplication
            // but the Join postprocessing is messy and prevents this reuse.
            symbolTable.EnterScope();

            var separator = "";
            var isLeftMostInput = true;
            var inputCount = inputs.Count;
            for (var idx = 0; idx < inputCount; idx++)
            {
                var input = inputs[idx];

                if (separator.Length != 0)
                {
                    result.From.AppendLine();
                }
                result.From.Append(separator + " ");
                // Change this if other conditions are required
                // to force the child to produce a nested SqlStatement.
                var needsJoinContext = (input.Expression.ExpressionKind == DbExpressionKind.Scan)
                                       || (isLeftMostInput &&
                                           (IsJoinExpression(input.Expression)
                                            || IsApplyExpression(input.Expression)))
                    ;

                isParentAJoinStack.Push(needsJoinContext ? true : false);
                // if the child reuses our select statement, it will append the from
                // symbols to our FromExtents list.  So, we need to remember the
                // start of the child's entries.
                var fromSymbolStart = result.FromExtents.Count;

                var fromExtentFragment = input.Expression.Accept(this);

                isParentAJoinStack.Pop();

                ProcessJoinInputResult(fromExtentFragment, result, input, fromSymbolStart);
                separator = joinString;

                isLeftMostInput = false;
            }

            // Visit the on clause/join condition.
            switch (joinKind)
            {
                case DbExpressionKind.FullOuterJoin:
                case DbExpressionKind.InnerJoin:
                case DbExpressionKind.LeftOuterJoin:
                    result.From.Append(" ON ");
                    isParentAJoinStack.Push(false);
                    result.From.Append(joinCondition.Accept(this));
                    isParentAJoinStack.Pop();
                    break;
            }

            symbolTable.ExitScope();

            if (!IsParentAJoin)
            {
                selectStatementStack.Pop();
            }

            return result;
        }

        // <summary>
        // This is called from <see cref="VisitJoinExpression" />.
        // This is responsible for maintaining the symbol table after visiting
        // a child of a join expression.
        // The child's sql statement may need to be completed.
        // The child's result could be one of
        // <list type="number">
        //     <item>The same as the parent's - this is treated specially.</item>
        //     <item>A sql select statement, which may need to be completed</item>
        //     <item>An extent - just copy it to the from clause</item>
        //     <item>
        //         Anything else (from a collection-valued expression) -
        //         unnest and copy it.
        //     </item>
        // </list>
        // If the input was a Join, we need to create a new join symbol,
        // otherwise, we create a normal symbol.
        // We then call AddFromSymbol to add the AS clause, and update the symbol table.
        // If the child's result was the same as the parent's, we have to clean up
        // the list of symbols in the FromExtents list, since this contains symbols from
        // the children of both the parent and the child.
        // The happens when the child visited is a Join, and is the leftmost child of
        // the parent.
        // </summary>
        private void ProcessJoinInputResult(
            ISqlFragment fromExtentFragment, SqlSelectStatement result,
            DbExpressionBinding input, int fromSymbolStart)
        {
            Symbol fromSymbol = null;

            if (result != fromExtentFragment)
            {
                // The child has its own select statement, and is not reusing
                // our select statement.
                // This should look a lot like VisitInputExpression().
                var sqlSelectStatement = fromExtentFragment as SqlSelectStatement;
                if (sqlSelectStatement != null)
                {
                    if (sqlSelectStatement.Select.IsEmpty)
                    {
                        var columns = AddDefaultColumns(sqlSelectStatement);

                        if (IsJoinExpression(input.Expression)
                            || IsApplyExpression(input.Expression))
                        {
                            var extents = sqlSelectStatement.FromExtents;
                            var newJoinSymbol = new JoinSymbol(input.VariableName, input.VariableType, extents);
                            newJoinSymbol.IsNestedJoin = true;
                            newJoinSymbol.ColumnList = columns;

                            fromSymbol = newJoinSymbol;
                        }
                        else
                        {
                            // this is a copy of the code in CreateNewSelectStatement.

                            // if the oldStatement has a join as its input, ...
                            // clone the join symbol, so that we "reuse" the
                            // join symbol.  Normally, we create a new symbol - see the next block
                            // of code.
                            var oldJoinSymbol = sqlSelectStatement.FromExtents[0] as JoinSymbol;
                            if (oldJoinSymbol != null)
                            {
                                // Note: sqlSelectStatement.FromExtents will not do, since it might
                                // just be an alias of joinSymbol, and we want an actual JoinSymbol.
                                var newJoinSymbol = new JoinSymbol(input.VariableName, input.VariableType, oldJoinSymbol.ExtentList);
                                // This indicates that the sqlSelectStatement is a blocking scope
                                // i.e. it hides/renames extent columns
                                newJoinSymbol.IsNestedJoin = true;
                                newJoinSymbol.ColumnList = columns;
                                newJoinSymbol.FlattenedExtentList = oldJoinSymbol.FlattenedExtentList;

                                fromSymbol = newJoinSymbol;
                            }
                            else if (sqlSelectStatement.FromExtents[0].OutputColumnsRenamed)
                            {
                                fromSymbol = new Symbol(input.VariableName, input.VariableType, sqlSelectStatement.FromExtents[0].Columns);
                            }
                        }
                    }
                    else if (sqlSelectStatement.OutputColumnsRenamed)
                    {
                        fromSymbol = new Symbol(input.VariableName, input.VariableType, sqlSelectStatement.OutputColumns);
                    }
                    result.From.Append(" (");
                    result.From.Append(sqlSelectStatement);
                    result.From.Append(" )");
                }
                else if (input.Expression is DbScanExpression)
                {
                    result.From.Append(fromExtentFragment);
                }
                else // bracket it
                {
                    WrapNonQueryExtent(result, fromExtentFragment, input.Expression.ExpressionKind);
                }

                if (fromSymbol == null) // i.e. not a join symbol
                {
                    fromSymbol = new Symbol(input.VariableName, input.VariableType);
                }

                AddFromSymbol(result, input.VariableName, fromSymbol);
                result.AllJoinExtents.Add(fromSymbol);
            }
            else // result == fromExtentFragment.  The child extents have been merged into the parent's.
            {
                // we are adding extents to the current sql statement via flattening.
                // We are replacing the child's extents with a single Join symbol.
                // The child's extents are all those following the index fromSymbolStart.
                //
                var extents = new List<Symbol>();

                // We cannot call extents.AddRange, since the is no simple way to
                // get the range of symbols fromSymbolStart..result.FromExtents.Count
                // from result.FromExtents.
                // We copy these symbols to create the JoinSymbol later.
                for (var i = fromSymbolStart; i < result.FromExtents.Count; ++i)
                {
                    extents.Add(result.FromExtents[i]);
                }
                result.FromExtents.RemoveRange(fromSymbolStart, result.FromExtents.Count - fromSymbolStart);
                fromSymbol = new JoinSymbol(input.VariableName, input.VariableType, extents);
                result.FromExtents.Add(fromSymbol);
                // this Join Symbol does not have its own select statement, so we
                // do not set IsNestedJoin

                // We do not call AddFromSymbol(), since we do not want to add
                // "AS alias" to the FROM clause- it has been done when the extent was added earlier.
                symbolTable.Add(input.VariableName, fromSymbol);
            }
        }

        // <summary>
        // This is called from <see cref="VisitNewInstanceExpression" />.
        // This is responsible for extracting the Alias for and Element expression
        // </summary>
        // <returns>
        // A <see cref="string" />
        // </returns>
        private static string getAliasFromElementExpression(DbElementExpression exp)
        {
            string result;
            var e = exp.Argument;
            Debug.Assert(TypeSemantics.IsCollectionType(e.ResultType));
            var rowTp = TypeHelpers.GetEdmType<RowType>(TypeHelpers.GetEdmType<CollectionType>(e.ResultType).TypeUsage);
            IEnumerator<EdmProperty> members = rowTp.Properties.GetEnumerator();
            members.MoveNext();
            var member = members.Current;
            result = QuoteIdentifier(member.Name);
            return result;
        }

        // <summary>
        // We assume that this is only called as a child of a Project.
        // This replaces <see cref="Visit(DbNewInstanceExpression)" />, since
        // we do not allow DbNewInstanceExpression as a child of any node other than
        // DbProjectExpression.
        // We write out the translation of each of the columns in the record.
        // </summary>
        // <returns>
        // A <see cref="SqlBuilder" />
        // </returns>
        private ISqlFragment VisitNewInstanceExpression(DbNewInstanceExpression e)
        {
            var result = new SqlBuilder();
            var rowType = e.ResultType.EdmType as RowType;

            if (null != rowType)
            {
                var members = rowType.Properties;
                var separator = "";
                for (var i = 0; i < e.Arguments.Count; ++i)
                {
                    var argument = e.Arguments[i];
                    if (TypeSemantics.IsRowType(argument.ResultType))
                    {
                        // We do not support nested records or other complex objects.
                        throw ADP1.NotSupported();
                    }

                    var member = members[i];
                    result.Append(separator);
                    result.AppendLine();
                    if (argument.ExpressionKind
                        == DbExpressionKind.Element)
                    {
                        var subQueryTableName = listOfScalarSubQueryTables[0];
                        var subQueryTableSymbol = symbolTable.Lookup(subQueryTableName);
                        Debug.Assert(null != subQueryTableSymbol);
                        result.Append(subQueryTableSymbol);
                        result.Append(".");
                        result.Append(getAliasFromElementExpression((DbElementExpression)argument));
                        listOfScalarSubQueryTables.Remove(subQueryTableName);
                    }
                    else
                    {
                        result.Append(argument.Accept(this));
                    }
                    result.Append(" AS ");
                    result.Append(QuoteIdentifier(member.Name));
                    separator = ", ";
                }
            }
            else
            {
                //
                // CONSIDER revisiting other possible expressions such as NominalTypes. for the time being
                // types other then RowType (such as UDTs for instance) are not supported.
                //
                throw ADP1.NotSupported();
            }

            return result;
        }

        // <summary>
        // Handler for set operations
        // It generates left separator right.
        // </summary>
        private ISqlFragment VisitSetOpExpression(DbBinaryExpression setOpExpression, string separator)
        {
            var leafSelectStatements = new List<SqlSelectStatement>();
            VisitAndGatherSetOpLeafExpressions(setOpExpression.ExpressionKind, setOpExpression.Left, leafSelectStatements);
            VisitAndGatherSetOpLeafExpressions(setOpExpression.ExpressionKind, setOpExpression.Right, leafSelectStatements);

            var setStatement = new SqlBuilder();

            for (var i = 0; i < leafSelectStatements.Count; ++i)
            {
                if (i > 0)
                {
                    setStatement.AppendLine();
                    setStatement.Append(separator); // e.g. UNION ALL
                    setStatement.AppendLine();
                }
                setStatement.Append(leafSelectStatements[i]);
            }

            Debug.Assert(!leafSelectStatements[0].OutputColumnsRenamed, "Output columns shouldn't be renamed");
            return setStatement;
        }

        // <summary>
        // Visits the given expression, expanding nodes of type kind and collecting all visited leaf nodes in leafSelectStatements.
        // The purpose of this is to flatten UNION ALL statements to avoid deep nesting.
        // </summary>
        // <param name="kind">the kind of expression matching the tree we're visiting</param>
        // <param name="expression">the expression to visit</param>
        // <param name="leafSelectStatements">accumulator for the collected leaf select statements</param>
        private void VisitAndGatherSetOpLeafExpressions(DbExpressionKind kind, DbExpression expression, List<SqlSelectStatement> leafSelectStatements)
        {
            Debug.Assert(kind == DbExpressionKind.UnionAll, "UnionAll is the only set op supported directly in SqlCompact");

            // only allow deep flattening of set op trees when the given expression is another instance of the given kind
            if (expression.ExpressionKind == kind)
            {
                var binary = (DbBinaryExpression)expression;
                VisitAndGatherSetOpLeafExpressions(kind, binary.Left, leafSelectStatements);
                VisitAndGatherSetOpLeafExpressions(kind, binary.Right, leafSelectStatements);
            }
            else
            {
                leafSelectStatements.Add(VisitExpressionEnsureSqlStatement(expression));
            }
        }

        #endregion

        #region Function Handling Helpers

        // <summary>
        // Determines whether the given function is a store function that
        // requires special handling
        // </summary>
        private static bool IsSpecialStoreFunction(DbFunctionExpression e)
        {
            return IsStoreFunction(e.Function)
                   && _storeFunctionHandlers.ContainsKey(e.Function.Name);
        }

        // <summary>
        // Determines whether the given function is a canonical function that
        // requires special handling
        // </summary>
        private static bool IsSpecialCanonicalFunction(DbFunctionExpression e)
        {
            return TypeHelpers.IsCanonicalFunction(e.Function)
                   && _canonicalFunctionHandlers.ContainsKey(e.Function.Name);
        }

        // <summary>
        // Default handling for functions.
        // Translates them to FunctionName(arg1, arg2, ..., argn)
        // </summary>
        private ISqlFragment HandleFunctionDefault(DbFunctionExpression e)
        {
            var result = new SqlBuilder();
            var requiresCast = CastReturnTypeToInt32(e);
            if (requiresCast)
            {
                result.Append(" CAST(");
            }
            WriteFunctionName(result, e.Function);
            HandleFunctionArgumentsDefault(e, result);
            if (requiresCast)
            {
                result.Append(" AS int)");
            }
            return result;
        }

        // <summary>
        // Default handling for functions with a given name.
        // Translates them to FunctionName(arg1, arg2, ..., argn)
        // </summary>
        private ISqlFragment HandleFunctionDefaultGivenName(DbFunctionExpression e, string functionName)
        {
            var result = new SqlBuilder();
            var needsCast = CastReturnTypeToInt32(e);
            if (needsCast)
            {
                result.Append("CAST(");
            }
            result.Append(functionName);
            HandleFunctionArgumentsDefault(e, result);
            if (needsCast)
            {
                result.Append(" AS int)");
            }
            return result;
        }

        // <summary>
        // Default handling on function arguments.
        // Appends the list of arguments to the given result
        // If the function is niladic it does not append anything,
        // otherwise it appends (arg1, arg2, .., argn)
        // </summary>
        private void HandleFunctionArgumentsDefault(DbFunctionExpression e, SqlBuilder result)
        {
            MetadataProperty niladicFunctionAttribute;
            bool isNiladicFunction;

            if (e.Function.MetadataProperties.TryGetValue("NiladicFunctionAttribute", false, out niladicFunctionAttribute)
                &&
                null != niladicFunctionAttribute.Value)
            {
                isNiladicFunction = (bool)niladicFunctionAttribute.Value;
            }
            else
            {
                isNiladicFunction = false;
            }
            Debug.Assert(
                !(isNiladicFunction && (0 < e.Arguments.Count)),
                "function attributed as NiladicFunction='true' in the provider manifest cannot have arguments");
            if (isNiladicFunction && e.Arguments.Count > 0)
            {
                ADP1.Metadata(EntityRes.GetString(EntityRes.NiladicFunctionsCannotHaveParameters));
            }

            if (!isNiladicFunction)
            {
                result.Append("(");
                var separator = "";
                foreach (var arg in e.Arguments)
                {
                    result.Append(separator);
                    result.Append(arg.Accept(this));
                    separator = ", ";
                }
                result.Append(")");
            }
        }

        // <summary>
        // Handler for special build in functions
        // </summary>
        private ISqlFragment HandleSpecialStoreFunction(DbFunctionExpression e)
        {
            return HandleSpecialFunction(_storeFunctionHandlers, e);
        }

        // <summary>
        // Handler for special canonical functions
        // </summary>
        private ISqlFragment HandleSpecialCanonicalFunction(DbFunctionExpression e)
        {
            return HandleSpecialFunction(_canonicalFunctionHandlers, e);
        }

        // <summary>
        // Dispatches the special function processing to the appropriate handler
        // </summary>
        private ISqlFragment HandleSpecialFunction(Dictionary<string, FunctionHandler> handlers, DbFunctionExpression e)
        {
            Debug.Assert(
                handlers.ContainsKey(e.Function.Name),
                "Special handling should be called only for functions in the list of special functions");
            return handlers[e.Function.Name](this, e);
        }

        // <summary>
        // Handles functions that are translated into TSQL operators.
        // The given function should have one or two arguments.
        // Functions with one argument are translated into
        // op arg
        // Functions with two arguments are translated into
        // arg0 op arg1
        // Also, the arguments can be optionally enclosed in parethesis
        // </summary>
        // <param name="parenthesizeArguments"> Whether the arguments should be enclosed in parethesis </param>
        private ISqlFragment HandleSpecialFunctionToOperator(DbFunctionExpression e, bool parenthesizeArguments)
        {
            var result = new SqlBuilder();
            Debug.Assert(e.Arguments.Count > 0 && e.Arguments.Count <= 2, "There should be 1 or 2 arguments for operator");

            if (e.Arguments.Count > 1)
            {
                if (parenthesizeArguments)
                {
                    result.Append("(");
                }
                result.Append(e.Arguments[0].Accept(this));
                if (parenthesizeArguments)
                {
                    result.Append(")");
                }
            }
            result.Append(" ");
            Debug.Assert(_functionNameToOperatorDictionary.ContainsKey(e.Function.Name), "The function can not be mapped to an operator");
            result.Append(_functionNameToOperatorDictionary[e.Function.Name]);
            result.Append(" ");

            if (parenthesizeArguments)
            {
                result.Append("(");
            }
            result.Append(e.Arguments[e.Arguments.Count - 1].Accept(this));
            if (parenthesizeArguments)
            {
                result.Append(")");
            }
            return result;
        }

        // <summary>
        // <see cref="HandleSpecialFunctionToOperator" />
        // </summary>
        private static ISqlFragment HandleConcatFunction(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return sqlgen.HandleSpecialFunctionToOperator(e, false);
        }

        // <summary>
        // <see cref="HandleSpecialFunctionToOperator" />
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionBitwise(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return sqlgen.HandleSpecialFunctionToOperator(e, true);
        }

        // <summary>
        // Throw error for any unsupported canonical functions
        // </summary>
        private static ISqlFragment HandleUnsupportedFunction(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            throw ADP1.NotSupported(EntityRes.GetString(EntityRes.FunctionNotSupported, e.Function.Name));
        }

        // <summary>
        // Handles special case in which datapart 'type' parameter is present. all the functions
        // handles here have *only* the 1st parameter as datepart. datepart value is passed along
        // the QP as string and has to be expanded as TSQL keyword.
        // </summary>
        private static ISqlFragment HandleDatepartDateFunction(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            Debug.Assert(e.Arguments.Count > 0, "e.Arguments.Count > 0");

            var constExpr = e.Arguments[0] as DbConstantExpression;
            if (null == constExpr)
            {
                throw ADP1.InvalidOperation(
                    EntityRes.GetString(EntityRes.InvalidDatePartArgumentExpression, e.Function.NamespaceName, e.Function.Name));
            }

            var datepart = constExpr.Value as string;
            if (null == datepart)
            {
                throw ADP1.InvalidOperation(
                    EntityRes.GetString(EntityRes.InvalidDatePartArgumentExpression, e.Function.NamespaceName, e.Function.Name));
            }

            var result = new SqlBuilder();

            //
            // check if datepart value is valid
            //
            if (!_datepartKeywords.ContainsKey(datepart))
            {
                throw ADP1.InvalidOperation(
                    EntityRes.GetString(EntityRes.InvalidDatePartArgumentValue, datepart, e.Function.NamespaceName, e.Function.Name));
            }

            //
            // finally, expand the function name
            //
            WriteFunctionName(result, e.Function);
            result.Append("(");

            // expand the datepart literal as tsql kword
            result.Append(datepart);
            var separator = ", ";

            // expand remaining arguments
            for (var i = 1; i < e.Arguments.Count; i++)
            {
                result.Append(separator);
                result.Append(e.Arguments[i].Accept(sqlgen));
            }

            result.Append(")");

            return result;
        }

        // <summary>
        // Handler for canonical functions for extracting date parts.
        // For example:
        // Year(date) -> DATEPART( year, date)
        // </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private static ISqlFragment HandleCanonicalFunctionDatepart(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return sqlgen.HandleCanonicalFunctionDatepart(e.Function.Name.ToLowerInvariant(), e);
        }

        // <summary>
        // Handler for turning a canonical function into DATEPART
        // Results in DATEPART(datepart, e)
        // </summary>
        private ISqlFragment HandleCanonicalFunctionDatepart(string datepart, DbFunctionExpression e)
        {
            var result = new SqlBuilder();
            result.Append("DATEPART (");
            result.Append(datepart);
            result.Append(", ");

            Debug.Assert(e.Arguments.Count == 1, "Canonical datepart functions should have exactly one argument");
            result.Append(e.Arguments[0].Accept(this));

            result.Append(")");

            return result;
        }

        // <summary>
        // Handler for the canonical function GetDate
        // CurrentDate() -> GetDate()
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionCurrentDateTime(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return sqlgen.HandleFunctionDefaultGivenName(e, "GetDate");
        }

        // <summary>
        // Creates datetime.
        // The given expression is in general translated into:
        // CONVERT(@typename, [datePart] + [timePart], 121)
        // The individual parts are translated as:
        // Date part:
        // convert(varchar(255), @year) + '-' + convert(varchar(255), @month) + '-' + convert(varchar(255), @day)
        // Time part:
        // convert(varchar(255), @hour)+ ':' + convert(varchar(255), @minute)+ ':' + str(@second, 6, 3)
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionCreateDateTime(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            var args = e.Arguments;
            Debug.Assert(args.Count == 6, "CreateDateTime should have 6 arguments");

            var result = new SqlBuilder();
            var currentArgumentIndex = 0;

            result.Append("convert (");
            result.Append("datetime");
            result.Append(",");

            //  YEAR
            AppendConvertToNVarchar(sqlgen, result, args[currentArgumentIndex++]);

            //  MONTH
            result.Append(" + '-' + ");
            AppendConvertToNVarchar(sqlgen, result, args[currentArgumentIndex++]);

            //  DAY 
            result.Append(" + '-' + ");
            AppendConvertToNVarchar(sqlgen, result, args[currentArgumentIndex++]);
            result.Append(" + ' ' + ");

            //  HOUR
            AppendConvertToNVarchar(sqlgen, result, args[currentArgumentIndex++]);

            // MINUTE
            result.Append(" + ':' + ");
            AppendConvertToNVarchar(sqlgen, result, args[currentArgumentIndex++]);

            // SECOND
            result.Append(" + ':' + str(");
            result.Append(args[currentArgumentIndex++].Accept(sqlgen));

            result.Append(", 6, 3)");
            result.Append(", 121)");

            return result;
        }

        // <summary>
        // Helper method that wraps the given expression with a convert to nvarchar(255)
        // </summary>
        private static void AppendConvertToNVarchar(SqlGenerator sqlgen, SqlBuilder result, DbExpression e)
        {
            result.Append("convert(nvarchar(255), ");
            result.Append(e.Accept(sqlgen));
            result.Append(")");
        }

        // <summary>
        // TruncateTime(DateTime X)
        // TRUNCATETIME(X) => CONVERT(DATETIME, CONVERT(VARCHAR(255), expression, 102),  102)
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionTruncateTime(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            //The type that we need to return is based on the argument type.
            string typeName = "datetime";

            PrimitiveTypeKind typeKind;
            TypeHelpers.TryGetPrimitiveTypeKind(e.ResultType, out typeKind);

            if (typeKind != PrimitiveTypeKind.DateTime)
            {
                Debug.Assert(true, "Unexpected type to TruncateTime" + typeKind.ToString());
            }

            var result = new SqlBuilder();
            result.Append("convert (");
            result.Append(typeName);
            result.Append(", convert(nvarchar(255), ");
            result.Append(e.Arguments[0].Accept(sqlgen));
            result.Append(", 102) ");

            result.Append(",  102)");
            return result;
        }

        // <summary>
        // Handler for all date/time addition canonical functions.
        // Translation, e.g.
        // AddYears(datetime, number) =>  DATEADD(year, number, datetime)
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionDateAdd(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            var result = new SqlBuilder();

            result.Append("DATEADD (");
            result.Append(_dateAddFunctionNameToDatepartDictionary[e.Function.Name]);
            result.Append(", ");
            result.Append(e.Arguments[1].Accept(sqlgen));
            result.Append(", ");
            result.Append(e.Arguments[0].Accept(sqlgen));
            result.Append(")");

            return result;
        }

        // <summary>
        // Handler for all date/time addition canonical functions.
        // Translation, e.g.
        // DiffYears(datetime, number) =>  DATEDIFF(year, number, datetime)
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionDateDiff(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            var result = new SqlBuilder();

            result.Append("DATEDIFF (");
            result.Append(_dateDiffFunctionNameToDatepartDictionary[e.Function.Name]);
            result.Append(", ");
            result.Append(e.Arguments[0].Accept(sqlgen));
            result.Append(", ");
            result.Append(e.Arguments[1].Accept(sqlgen));
            result.Append(")");

            return result;
        }

        // <summary>
        // Function rename IndexOf -> CHARINDEX
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionIndexOf(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return sqlgen.HandleFunctionDefaultGivenName(e, "CHARINDEX");
        }

        // <summary>
        // Function rename NewGuid -> NEWID
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionNewGuid(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return sqlgen.HandleFunctionDefaultGivenName(e, "NEWID");
        }

        // <summary>
        // Function rename Length -> LEN
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionLength(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return sqlgen.HandleFunctionDefaultGivenName(e, "LEN");
        }

        // <summary>
        // Left(string, length) -> SUBSTRING(string, 1, length)
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionLeft(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            Debug.Assert(e.Arguments.Count == 2, "Left should have two arguments");

            var result = new SqlBuilder();

            result.Append("SUBSTRING (");
            result.Append(e.Arguments[0].Accept(sqlgen));
            result.Append(", 1, ");
            result.Append(e.Arguments[1].Accept(sqlgen));
            result.Append(")");

            return result;
        }

        // <summary>
        // Right(string, length) -> SUBSTRING(string, DATALENGHT(CAST(string as NTEXT))/2 + 1 - length, length)
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionRight(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            Debug.Assert(e.Arguments.Count == 2, "Right should have two arguments");

            var result = new SqlBuilder();

            result.Append("SUBSTRING (");
            result.Append(e.Arguments[0].Accept(sqlgen));
            result.Append(", ");
            result.Append("DATALENGTH(CAST(");
            result.Append(e.Arguments[0].Accept(sqlgen));
            result.Append("AS ntext))/2 + 1 - ");
            result.Append(e.Arguments[1].Accept(sqlgen));
            result.Append(", ");
            result.Append(e.Arguments[1].Accept(sqlgen));
            result.Append(")");

            return result;
        }

        // <summary>
        // Round(numericExpression) -> Round(numericExpression, 0);
        // Round(numericExpression, digits) -> Round(numericExpression, digits);
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionRound(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return HandleCanonicalFunctionRoundOrTruncate(sqlgen, e, true);
        }

        // <summary>
        // Truncate(numericExpression) -> Round(numericExpression, 0, 1); (does not exist as canonical function yet)
        // Truncate(numericExpression, digits) -> Round(numericExpression, digits, 1);
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionTruncate(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return HandleCanonicalFunctionRoundOrTruncate(sqlgen, e, false);
        }

        // <summary>
        // Common handler for the canonical functions ROUND and TRUNCATE
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionRoundOrTruncate(SqlGenerator sqlgen, DbFunctionExpression e, bool round)
        {
            var result = new SqlBuilder();

            // Do not add the cast for the Round() overload having two arguments. 
            // Round(Single,Int32) maps to Round(Double,Int32)due to implicit casting. 
            // We don't need to cast in that case, since the server returned type is same 
            // as the expected  type. Cast is only required for the overload - Round(Single)
            var requiresCastToSingle = false;
            if (e.Arguments.Count == 1)
            {
                requiresCastToSingle = CastReturnTypeToSingle(e);
                if (requiresCastToSingle)
                {
                    result.Append(" CAST(");
                }
            }
            result.Append("ROUND(");

            Debug.Assert(e.Arguments.Count <= 2, "Round or truncate should have at most 2 arguments");
            result.Append(e.Arguments[0].Accept(sqlgen));
            result.Append(", ");

            if (e.Arguments.Count > 1)
            {
                result.Append(e.Arguments[1].Accept(sqlgen));
            }
            else
            {
                result.Append("0");
            }

            if (!round)
            {
                result.Append(", 1");
            }

            result.Append(")");

            if (requiresCastToSingle)
            {
                result.Append(" AS real)");
            }
            return result;
        }

        // <summary>
        // TRIM(string) -> LTRIM(RTRIM(string))
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionTrim(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            var result = new SqlBuilder();

            result.Append("LTRIM(RTRIM(");

            Debug.Assert(e.Arguments.Count == 1, "Trim should have one argument");
            result.Append(e.Arguments[0].Accept(sqlgen));

            result.Append("))");

            return result;
        }

        // <summary>
        // Function rename ToLower -> LOWER
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionToLower(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return sqlgen.HandleFunctionDefaultGivenName(e, "LOWER");
        }

        // <summary>
        // Function rename ToUpper -> UPPER
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionToUpper(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return sqlgen.HandleFunctionDefaultGivenName(e, "UPPER");
        }

        // <summary>
        // Function to translate the StartsWith, EndsWith and Contains canonical functions to LIKE expression in T-SQL
        // and also add the trailing ESCAPE '~' when escaping of the search string for the LIKE expression has occurred
        // </summary>
        private static void TranslateConstantParameterForLike(
            SqlGenerator sqlgen, DbExpression targetExpression, DbConstantExpression constSearchParamExpression, SqlBuilder result,
            bool insertPercentStart, bool insertPercentEnd)
        {
            result.Append(targetExpression.Accept(sqlgen));
            result.Append(" LIKE ");

            // If it's a DbConstantExpression then escape the search parameter if necessary.
            bool escapingOccurred;

            var searchParamBuilder = new StringBuilder();
            if (insertPercentStart)
            {
                searchParamBuilder.Append("%");
            }
            searchParamBuilder.Append(
                SqlCeProviderManifest.EscapeLikeText(constSearchParamExpression.Value as string, false, out escapingOccurred));
            if (insertPercentEnd)
            {
                searchParamBuilder.Append("%");
            }

            var escapedSearchParamExpression = constSearchParamExpression.ResultType.Constant(searchParamBuilder.ToString());
            result.Append(escapedSearchParamExpression.Accept(sqlgen));

            // If escaping did occur (special characters were found), then append the escape character used.
            if (escapingOccurred)
            {
                result.Append(" ESCAPE '" + SqlCeProviderManifest.LikeEscapeChar + "'");
            }
        }

        // <summary>
        // Handler for Contains. Wraps the normal translation with a case statement
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionContains(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return WrapPredicate(HandleCanonicalFunctionContains, sqlgen, e);
        }

        // <summary>
        // CONTAINS(arg0, arg1) => arg0 LIKE '%arg1%'
        // </summary>
        private static SqlBuilder HandleCanonicalFunctionContains(SqlGenerator sqlgen, IList<DbExpression> args, SqlBuilder result)
        {
            Debug.Assert(args.Count == 2, "Contains should have two arguments");
            // Check if args[1] is a DbConstantExpression
            var constSearchParamExpression = args[1] as DbConstantExpression;
            if ((constSearchParamExpression != null)
                && (string.IsNullOrEmpty(constSearchParamExpression.Value as string) == false))
            {
                TranslateConstantParameterForLike(sqlgen, args[0], constSearchParamExpression, result, true, true);
            }
            else
            {
                result.Append("CHARINDEX( ");
                result.Append(args[1].Accept(sqlgen));
                result.Append(", ");
                result.Append(args[0].Accept(sqlgen));
                result.Append(") > 0");
            }
            return result;
        }

        // <summary>
        // Handler for StartsWith. Wraps the normal translation with a case statement
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionStartsWith(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            return WrapPredicate(HandleCanonicalFunctionStartsWith, sqlgen, e);
        }

        // <summary>
        // STARTSWITH(arg0, arg1) => arg0 LIKE 'arg1%'
        // </summary>
        private static SqlBuilder HandleCanonicalFunctionStartsWith(SqlGenerator sqlgen, IList<DbExpression> args, SqlBuilder result)
        {
            Debug.Assert(args.Count == 2, "StartsWith should have two arguments");
            // Check if args[1] is a DbConstantExpression
            var constSearchParamExpression = args[1] as DbConstantExpression;
            if ((constSearchParamExpression != null)
                && (string.IsNullOrEmpty(constSearchParamExpression.Value as string) == false))
            {
                TranslateConstantParameterForLike(sqlgen, args[0], constSearchParamExpression, result, false, true);
            }
            else
            {
                result.Append("CHARINDEX( ");
                result.Append(args[1].Accept(sqlgen));
                result.Append(", ");
                result.Append(args[0].Accept(sqlgen));
                result.Append(") = 1");
            }

            return result;
        }

        // <summary>
        // Handler for EndsWith. Wraps the normal translation with a case statement
        // </summary>
        private static ISqlFragment HandleCanonicalFunctionEndsWith(SqlGenerator sqlgen, DbFunctionExpression e)
        {
            Debug.Assert(e.Arguments.Count == 2, "EndsWith should have two arguments");
            var constSearchParamExpression = e.Arguments[1] as DbConstantExpression;
            if ((constSearchParamExpression != null)
                && (string.IsNullOrEmpty(constSearchParamExpression.Value as string) == false))
            {
                return WrapPredicate(HandleCanonicalFunctionEndsWith, sqlgen, e);
            }
            else
            {
                throw ADP1.NotSupported(EntityRes.GetString(EntityRes.FunctionNotSupported, e.Function.Name));
            }
        }

        // <summary>
        // ENDSWITH(arg0, arg1) => arg0 LIKE '%arg1'
        // </summary>
        private static SqlBuilder HandleCanonicalFunctionEndsWith(SqlGenerator sqlgen, IList<DbExpression> args, SqlBuilder result)
        {
            // Check if args[1] is a DbConstantExpression
            var constSearchParamExpression = args[1] as DbConstantExpression;
            if ((constSearchParamExpression != null)
                && (string.IsNullOrEmpty(constSearchParamExpression.Value as string) == false))
            {
                TranslateConstantParameterForLike(sqlgen, args[0], constSearchParamExpression, result, true, false);
            }
            return result;
        }

        // <summary>
        // Turns a predicate into a statement returning a bit
        // PREDICATE => CASE WHEN (PREDICATE) THEN CAST(1 AS BIT) WHEN (NOT (PREDICATE)) CAST (O AS BIT) END
        // The predicate is produced by the given predicateTranslator.
        // </summary>
        private static ISqlFragment WrapPredicate(
            Func<SqlGenerator, IList<DbExpression>, SqlBuilder, SqlBuilder> predicateTranslator, SqlGenerator sqlgen, DbFunctionExpression e)
        {
            var result = new SqlBuilder();
            result.Append("CASE WHEN (");
            predicateTranslator(sqlgen, e.Arguments, result);
            result.Append(") THEN cast(1 as bit) WHEN ( NOT (");
            predicateTranslator(sqlgen, e.Arguments, result);
            result.Append(")) THEN cast(0 as bit) END");
            return result;
        }

        // <summary>
        // Writes the function name to the given SqlBuilder.
        // </summary>
        private static void WriteFunctionName(SqlBuilder result, EdmFunction function, out bool fCast, out string castType)
        {
            string storeFunctionName;
            MetadataProperty storeFunctionNameAttribute;
            fCast = false;
            castType = "";

            if (function.MetadataProperties.TryGetValue
                    ("StoreFunctionNameAttribute", false, out storeFunctionNameAttribute)
                &&
                null != storeFunctionNameAttribute.Value)
            {
                storeFunctionName = (string)storeFunctionNameAttribute.Value;
            }
            else
            {
                storeFunctionName = function.Name;
            }

            // Change BigCount to COUNT with a CAST operator
            //
            if (String.Equals(storeFunctionName, "BigCount", StringComparison.Ordinal))
            {
                storeFunctionName = "Count";
                fCast = true;
                castType = "BIGINT";
            }

            // If the function is a builtin (i.e. the BuiltIn attribute has been
            // specified, both store and canonical functions have this attribute), 
            // then the function name should not be quoted; 
            // additionally, no namespace should be used.
            if (TypeHelpers.IsCanonicalFunction(function))
            {
                result.Append(storeFunctionName.ToUpperInvariant());
            }
            else if (IsStoreFunction(function))
            {
                result.Append(storeFunctionName);
            }
            else
            {
                throw ADP1.NotSupported(EntityRes.GetString(EntityRes.UserDefinedFunctionsNotSupported));
            }

            // Either fCast is false or we should have defined a castType also
            //
            Debug.Assert(!fCast || castType.Length > 0);
        }

        // <summary>
        // Writes the function name to the given SqlBuilder.
        // Dummy method where we don't expect cast.
        // </summary>
        private static void WriteFunctionName(SqlBuilder result, EdmFunction function)
        {
            string castType;
            bool fCast;

            WriteFunctionName(result, function, out fCast, out castType);

            // Since this is a dummy wrapper function, we shouldn't get cast = true here
            //
            Debug.Assert(!fCast);
        }

        #endregion

        #region Other Helpers

        // <summary>
        // <see cref="AddDefaultColumns" />
        // Add the column names from the referenced extent/join to the
        // select statement.
        // If the symbol is a JoinSymbol, we recursively visit all the extents,
        // halting at real extents and JoinSymbols that have an associated SqlSelectStatement.
        // The column names for a real extent can be derived from its type.
        // The column names for a Join Select statement can be got from the
        // list of columns that was created when the Join's select statement
        // was created.
        // We do the following for each column.
        // <list type="number">
        //     <item>Add the SQL string for each column to the SELECT clause</item>
        //     <item>
        //         Add the column to the list of columns - so that it can
        //         become part of the "type" of a JoinSymbol
        //     </item>
        //     <item>
        //         Check if the column name collides with a previous column added
        //         to the same select statement.  Flag both the columns for renaming if true.
        //     </item>
        //     <item>Add the column to a name lookup dictionary for collision detection.</item>
        // </list>
        // </summary>
        // <param name="selectStatement"> The select statement that started off as SELECT * </param>
        // <param name="symbol"> The symbol containing the type information for the columns to be added. </param>
        // <param name="columnList">
        // Columns that have been added to the Select statement. This is created in
        // <see
        //     cref="AddDefaultColumns" />
        // .
        // </param>
        // <param name="columnDictionary"> A dictionary of the columns above. </param>
        // <param name="separator"> Comma or nothing, depending on whether the SELECT clause is empty. </param>
        private void AddColumns(
            SqlSelectStatement selectStatement, Symbol symbol,
            List<Symbol> columnList, Dictionary<string, Symbol> columnDictionary, ref string separator)
        {
            var joinSymbol = symbol as JoinSymbol;
            if (joinSymbol != null)
            {
                if (!joinSymbol.IsNestedJoin)
                {
                    // Recurse if the join symbol is a collection of flattened extents
                    foreach (var sym in joinSymbol.ExtentList)
                    {
                        // if sym is ScalarType means we are at base case in the
                        // recursion and there are not columns to add, just skip
                        if ((sym.Type == null)
                            || TypeSemantics.IsPrimitiveType(sym.Type))
                        {
                            continue;
                        }

                        AddColumns(selectStatement, sym, columnList, columnDictionary, ref separator);
                    }
                }
                else
                {
                    foreach (var joinColumn in joinSymbol.ColumnList)
                    {
                        // we write tableName.columnName
                        // rather than tableName.columnName as alias
                        // since the column name is unique (by the way we generate new column names)
                        //
                        // We use the symbols for both the table and the column,
                        // since they are subject to renaming.
                        selectStatement.Select.Append(separator);
                        selectStatement.Select.Append(symbol);
                        selectStatement.Select.Append(".");
                        selectStatement.Select.Append(joinColumn);

                        // check for name collisions.  If there is,
                        // flag both the colliding symbols.
                        if (columnDictionary.ContainsKey(joinColumn.Name))
                        {
                            columnDictionary[joinColumn.Name].NeedsRenaming = true; // the original symbol
                            joinColumn.NeedsRenaming = true; // the current symbol.
                        }
                        else
                        {
                            columnDictionary[joinColumn.Name] = joinColumn;
                        }

                        columnList.Add(joinColumn);

                        separator = ", ";
                    }
                }
            }
            else
            {
                // This is a non-join extent/select statement, and the CQT type has
                // the relevant column information.

                // The type could be a record type(e.g. Project(...),
                // or an entity type ( e.g. EntityExpression(...)
                // so, we check whether it is a structuralType.

                // Consider an expression of the form J(a, b=P(E))
                // The inner P(E) would have been translated to a SQL statement
                // We should not use the raw names from the type, but the equivalent
                // symbols (they are present in symbol.Columns) if they exist.
                //
                // We add the new columns to the symbol's columns if they do
                // not already exist.
                //
                // If the symbol represents a SqlStatement with renamed output columns,
                // we should use these instead of the rawnames and we should also mark
                // this selectStatement as one with renamed columns

                if (symbol.OutputColumnsRenamed)
                {
                    selectStatement.OutputColumnsRenamed = true;
                    selectStatement.OutputColumns = new Dictionary<string, Symbol>();
                }

                if ((symbol.Type == null)
                    || TypeSemantics.IsPrimitiveType(symbol.Type))
                {
                    AddColumn(selectStatement, symbol, columnList, columnDictionary, ref separator, "X");
                }
                else
                {
                    foreach (var property in TypeHelpers.GetProperties(symbol.Type))
                    {
                        AddColumn(selectStatement, symbol, columnList, columnDictionary, ref separator, property.Name);
                    }
                }
            }
        }

        // <summary>
        // Helper method for AddColumns. Adds a column with the given column name
        // to the Select list of the given select statement.
        // </summary>
        // <param name="selectStatement"> The select statement to whose SELECT part the column should be added </param>
        // <param name="symbol"> The symbol from which the column to be added originated </param>
        // <param name="columnList">
        // Columns that have been added to the Select statement. This is created in
        // <see
        //     cref="AddDefaultColumns" />
        // .
        // </param>
        // <param name="columnDictionary"> A dictionary of the columns above. </param>
        // <param name="separator"> Comma or nothing, depending on whether the SELECT clause is empty. </param>
        // <param name="columnName"> The name of the column to be added. </param>
        private void AddColumn(
            SqlSelectStatement selectStatement, Symbol symbol,
            List<Symbol> columnList, Dictionary<string, Symbol> columnDictionary, ref string separator, string columnName)
        {
            // Since all renaming happens in the second phase
            // we lose nothing by setting the next column name index to 0
            // many times.
            allColumnNames[columnName] = 0;

            // Create a new symbol/reuse existing symbol for the column
            Symbol columnSymbol;
            if (!symbol.Columns.TryGetValue(columnName, out columnSymbol))
            {
                // we do not care about the types of columns, so we pass null
                // when construction the symbol.
                columnSymbol = new Symbol(columnName, null);
                symbol.Columns.Add(columnName, columnSymbol);
            }

            selectStatement.Select.Append(separator);
            selectStatement.Select.Append(symbol);
            selectStatement.Select.Append(".");

            if (symbol.OutputColumnsRenamed)
            {
                selectStatement.Select.Append(columnSymbol);
                selectStatement.OutputColumns.Add(columnSymbol.Name, columnSymbol);
            }

                // We use the actual name before the "AS", the new name goes
            // after the AS.
            else
            {
                selectStatement.Select.Append(QuoteIdentifier(columnName));
            }

            selectStatement.Select.Append(" AS ");
            selectStatement.Select.Append(columnSymbol);

            // Check for column name collisions.
            if (columnDictionary.ContainsKey(columnName))
            {
                columnDictionary[columnName].NeedsRenaming = true;
                columnSymbol.NeedsRenaming = true;
            }
            else
            {
                columnDictionary[columnName] = symbol.Columns[columnName];
            }

            columnList.Add(columnSymbol);
            separator = ", ";
        }

        // <summary>
        // Expands Select * to "select the_list_of_columns"
        // If the columns are taken from an extent, they are written as
        // {original_column_name AS Symbol(original_column)} to allow renaming.
        // If the columns are taken from a Join, they are written as just
        // {original_column_name}, since there cannot be a name collision.
        // We concatenate the columns from each of the inputs to the select statement.
        // Since the inputs may be joins that are flattened, we need to recurse.
        // The inputs are inferred from the symbols in FromExtents.
        // </summary>
        private List<Symbol> AddDefaultColumns(SqlSelectStatement selectStatement)
        {
            // This is the list of columns added in this select statement
            // This forms the "type" of the Select statement, if it has to
            // be expanded in another SELECT *
            var columnList = new List<Symbol>();

            // A lookup for the previous set of columns to aid column name
            // collision detection.
            var columnDictionary = new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase);

            var separator = "";
            // The Select should usually be empty before we are called,
            // but we do not mind if it is not.
            if (!selectStatement.Select.IsEmpty)
            {
                separator = ", ";
            }

            foreach (var symbol in selectStatement.FromExtents)
            {
                AddColumns(selectStatement, symbol, columnList, columnDictionary, ref separator);
            }

            return columnList;
        }

        // <summary>
        // <see cref="AddFromSymbol(SqlSelectStatement, string, Symbol, bool)" />
        // </summary>
        private void AddFromSymbol(SqlSelectStatement selectStatement, string inputVarName, Symbol fromSymbol)
        {
            AddFromSymbol(selectStatement, inputVarName, fromSymbol, true);
        }

        // <summary>
        // This method is called after the input to a relational node is visited.
        // <see cref="Visit(DbProjectExpression)" /> and <see cref="ProcessJoinInputResult" />
        // There are 2 scenarios
        // <list type="number">
        //     <item>
        //         The fromSymbol is new i.e. the select statement has just been
        //         created, or a join extent has been added.
        //     </item>
        //     <item>The fromSymbol is old i.e. we are reusing a select statement.</item>
        // </list>
        // If we are not reusing the select statement, we have to complete the
        // FROM clause with the alias
        // <code>-- if the input was an extent
        //     FROM = [SchemaName].[TableName]
        //     -- if the input was a Project
        //     FROM = (SELECT ... FROM ... WHERE ...)</code>
        // These become
        // <code>-- if the input was an extent
        //     FROM = [SchemaName].[TableName] AS alias
        //     -- if the input was a Project
        //     FROM = (SELECT ... FROM ... WHERE ...) AS alias</code>
        // and look like valid FROM clauses.
        // Finally, we have to add the alias to the global list of aliases used,
        // and also to the current symbol table.
        // </summary>
        // <param name="inputVarName"> The alias to be used. </param>
        private void AddFromSymbol(SqlSelectStatement selectStatement, string inputVarName, Symbol fromSymbol, bool addToSymbolTable)
        {
            // the first check is true if this is a new statement
            // the second check is true if we are in a join - we do not
            // check if we are in a join context.
            // We do not want to add "AS alias" if it has been done already
            // e.g. when we are reusing the Sql statement.
            if (selectStatement.FromExtents.Count == 0
                || fromSymbol != selectStatement.FromExtents[0])
            {
                selectStatement.FromExtents.Add(fromSymbol);
                selectStatement.From.Append(" AS ");
                selectStatement.From.Append(fromSymbol);

                // We have this inside the if statement, since
                // we only want to add extents that are actually used.
                allExtentNames[fromSymbol.Name] = 0;
            }

            if (addToSymbolTable)
            {
                symbolTable.Add(inputVarName, fromSymbol);
            }
        }

        // <summary>
        // Translates a list of SortClauses.
        // Used in the translation of OrderBy
        // </summary>
        // <param name="orderByClause"> The SqlBuilder to which the sort keys should be appended </param>
        private void AddSortKeys(SqlBuilder orderByClause, IList<DbSortClause> sortKeys)
        {
            var separator = "";
            foreach (var sortClause in sortKeys)
            {
                orderByClause.Append(separator);
                orderByClause.Append(sortClause.Expression.Accept(this));
                // Bug 431021: COLLATE clause must precede ASC/DESC
                Debug.Assert(sortClause.Collation != null);
                if (!String.IsNullOrEmpty(sortClause.Collation))
                {
                    // COLLATE is not supported in SSCE.
                    throw ADP1.CollateInOrderByNotSupportedException();
                }

                orderByClause.Append(sortClause.Ascending ? " ASC" : " DESC");

                separator = ", ";
            }
        }

        // <summary>
        // <see cref="CreateNewSelectStatement(SqlSelectStatement, string, TypeUsage, bool, out Symbol)" />
        // </summary>
        private SqlSelectStatement CreateNewSelectStatement(
            SqlSelectStatement oldStatement,
            string inputVarName, TypeUsage inputVarType, out Symbol fromSymbol)
        {
            return CreateNewSelectStatement(oldStatement, inputVarName, inputVarType, true, out fromSymbol);
        }

        // <summary>
        // This is called after a relational node's input has been visited, and the
        // input's sql statement cannot be reused.  <see cref="Visit(DbProjectExpression)" />
        // When the input's sql statement cannot be reused, we create a new sql
        // statement, with the old one as the from clause of the new statement.
        // The old statement must be completed i.e. if it has an empty select list,
        // the list of columns must be projected out.
        // If the old statement being completed has a join symbol as its from extent,
        // the new statement must have a clone of the join symbol as its extent.
        // We cannot reuse the old symbol, but the new select statement must behave
        // as though it is working over the "join" record.
        // </summary>
        // <returns> A new select statement, with the old one as the from clause. </returns>
        private SqlSelectStatement CreateNewSelectStatement(
            SqlSelectStatement oldStatement,
            string inputVarName, TypeUsage inputVarType, bool finalizeOldStatement, out Symbol fromSymbol)
        {
            fromSymbol = null;

            // Finalize the old statement
            if (finalizeOldStatement && oldStatement.Select.IsEmpty)
            {
                var columns = AddDefaultColumns(oldStatement);

                // Thid could not have been called from a join node.
                Debug.Assert(oldStatement.FromExtents.Count == 1);

                // if the oldStatement has a join as its input, ...
                // clone the join symbol, so that we "reuse" the
                // join symbol.  Normally, we create a new symbol - see the next block
                // of code.
                var oldJoinSymbol = oldStatement.FromExtents[0] as JoinSymbol;
                if (oldJoinSymbol != null)
                {
                    // Note: oldStatement.FromExtents will not do, since it might
                    // just be an alias of joinSymbol, and we want an actual JoinSymbol.
                    var newJoinSymbol = new JoinSymbol(inputVarName, inputVarType, oldJoinSymbol.ExtentList);
                    // This indicates that the oldStatement is a blocking scope
                    // i.e. it hides/renames extent columns
                    newJoinSymbol.IsNestedJoin = true;
                    newJoinSymbol.ColumnList = columns;
                    newJoinSymbol.FlattenedExtentList = oldJoinSymbol.FlattenedExtentList;

                    fromSymbol = newJoinSymbol;
                }
            }

            if (fromSymbol == null)
            {
                if (oldStatement.OutputColumnsRenamed)
                {
                    fromSymbol = new Symbol(inputVarName, inputVarType, oldStatement.OutputColumns);
                }
                else
                {
                    // This is just a simple extent/SqlSelectStatement,
                    // and we can get the column list from the type.
                    fromSymbol = new Symbol(inputVarName, inputVarType);
                }
            }

            // Observe that the following looks like the body of Visit(ExtentExpression).
            var selectStatement = new SqlSelectStatement();
            selectStatement.From.Append("( ");
            selectStatement.From.Append(oldStatement);
            selectStatement.From.AppendLine();
            selectStatement.From.Append(") ");

            return selectStatement;
        }

        // <summary>
        // Before we embed a string literal in a SQL string, we should
        // convert all ' to '', and enclose the whole string in single quotes.
        // </summary>
        // <returns> The escaped sql string. </returns>
        private static string EscapeSingleQuote(string s, bool isUnicode)
        {
            return (isUnicode ? "N'" : "'") + s.Replace("'", "''") + "'";
        }

        // <summary>
        // Returns the sql primitive/native type name.
        // It will include size, precision or scale depending on type information present in the
        // type facets
        // </summary>
        private static string GetSqlPrimitiveType(TypeUsage type)
        {
            Debug.Assert(TypeSemantics.IsPrimitiveType(type.EdmType), "Type must be primitive type");

            var storeTypeUsage = SqlCeProviderManifest.Instance.GetStoreType(type);

            var typeName = storeTypeUsage.EdmType.Name;
            var hasFacet = false;
            var maxLength = 0;
            byte decimalPrecision = 0;
            byte decimalScale = 0;

            var primitiveTypeKind = ((PrimitiveType)storeTypeUsage.EdmType).PrimitiveTypeKind;
            switch (primitiveTypeKind)
            {
                case PrimitiveTypeKind.Binary:
                    if (!TypeHelpers.IsFacetValueConstant(storeTypeUsage, ProviderManifest.MaxLengthFacetName))
                    {
                        hasFacet = TypeHelpers.TryGetMaxLength(storeTypeUsage, out maxLength);
                        Debug.Assert(hasFacet, "Binary type did not have MaxLength facet");
                        typeName = typeName + "(" + maxLength.ToString(CultureInfo.InvariantCulture) + ")";
                    }
                    break;

                case PrimitiveTypeKind.String:
                    if (!TypeHelpers.IsFacetValueConstant(storeTypeUsage, ProviderManifest.MaxLengthFacetName))
                    {
                        hasFacet = TypeHelpers.TryGetMaxLength(storeTypeUsage, out maxLength);
                        Debug.Assert(hasFacet, "String type did not have MaxLength facet");
                        typeName = typeName + "(" + maxLength.ToString(CultureInfo.InvariantCulture) + ")";
                    }
                    break;

                case PrimitiveTypeKind.DateTime:
                    typeName = "datetime";
                    break;

                case PrimitiveTypeKind.Decimal:
                    if (!TypeHelpers.IsFacetValueConstant(storeTypeUsage, ProviderManifest.PrecisionFacetName))
                    {
                        hasFacet = TypeHelpers.TryGetPrecision(storeTypeUsage, out decimalPrecision);
                        Debug.Assert(hasFacet, "decimal must have precision facet");
                        Debug.Assert(decimalPrecision > 0, "decimal precision must be greater than zero");
                        hasFacet = TypeHelpers.TryGetScale(storeTypeUsage, out decimalScale);
                        Debug.Assert(hasFacet, "decimal must have scale facet");
                        Debug.Assert(decimalPrecision >= decimalScale, "decimalPrecision must be greater or equal to decimalScale");
                        typeName = typeName + "(" + decimalPrecision + "," + decimalScale + ")";
                    }
                    break;

                case PrimitiveTypeKind.Time:
                    throw ADP1.NotSupported(EntityRes.GetString(EntityRes.ProviderDoesNotSupportType, "Time"));

                case PrimitiveTypeKind.DateTimeOffset:
                    throw ADP1.NotSupported(EntityRes.GetString(EntityRes.ProviderDoesNotSupportType, "DateTimeOffset"));

                default:
                    break;
            }

            return typeName;
        }

        // <summary>
        // Handles the expression represending DbLimitExpression.Limit and DbSkipExpression.Count.
        // If it is a constant expression, it simply does to string thus avoiding casting it to the specific value
        // (which would be done if <see cref="Visit(DbConstantExpression)" /> is called)
        // </summary>
        private ISqlFragment HandleCountExpression(DbExpression e)
        {
            ISqlFragment result;

            if (e.ExpressionKind
                == DbExpressionKind.Constant)
            {
                //For constant expression we should not cast the value, 
                // thus we don't go through the default DbConstantExpression handling
                var sqlBuilder = new SqlBuilder();
                sqlBuilder.Append(((DbConstantExpression)e).Value.ToString());
                result = sqlBuilder;
            }
            else
            {
                result = e.Accept(this);
            }

            return result;
        }

        // <summary>
        // This is used to determine if a particular expression is an Apply operation.
        // This is only the case when the DbExpressionKind is CrossApply or OuterApply.
        // </summary>
        private static bool IsApplyExpression(DbExpression e)
        {
            return (DbExpressionKind.CrossApply == e.ExpressionKind || DbExpressionKind.OuterApply == e.ExpressionKind);
        }

        // <summary>
        // This is used to determine if a particular expression is a Join operation.
        // This is true for DbCrossJoinExpression and DbJoinExpression, the
        // latter of which may have one of several different ExpressionKinds.
        // </summary>
        private static bool IsJoinExpression(DbExpression e)
        {
            return (DbExpressionKind.CrossJoin == e.ExpressionKind ||
                    DbExpressionKind.FullOuterJoin == e.ExpressionKind ||
                    DbExpressionKind.InnerJoin == e.ExpressionKind ||
                    DbExpressionKind.LeftOuterJoin == e.ExpressionKind);
        }

        // <summary>
        // This is used to determine if a calling expression needs to place
        // round brackets around the translation of the expression e.
        // Constants, parameters and properties do not require brackets,
        // everything else does.
        // </summary>
        // <returns> true, if the expression needs brackets </returns>
        private static bool IsComplexExpression(DbExpression e)
        {
            switch (e.ExpressionKind)
            {
                case DbExpressionKind.Constant:
                case DbExpressionKind.ParameterReference:
                case DbExpressionKind.Property:
                    return false;

                default:
                    return true;
            }
        }

        // <summary>
        // Determine if the owner expression can add its unique sql to the input's
        // SqlSelectStatement
        // </summary>
        // <param name="result"> The SqlSelectStatement of the input to the relational node. </param>
        // <param name="expressionKind"> The kind of the expression node(not the input's) </param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static bool IsCompatible(SqlSelectStatement result, DbExpressionKind expressionKind)
        {
            switch (expressionKind)
            {
                case DbExpressionKind.Distinct:
                    return result.Top == null
                           && result.Skip == null
                        // #494803: The projection after distinct may not project all 
                        // columns used in the Order By
                        // Improvement: Consider getting rid of the Order By instead
                           && result.OrderBy.IsEmpty;

                case DbExpressionKind.Filter:
                    return result.Select.IsEmpty
                           && result.Where.IsEmpty
                           && result.GroupBy.IsEmpty
                           && result.Top == null
                           && result.Skip == null;

                case DbExpressionKind.GroupBy:
                    return result.Select.IsEmpty
                           && result.GroupBy.IsEmpty
                           && result.OrderBy.IsEmpty
                           && result.Top == null
                           && result.Skip == null;

                case DbExpressionKind.Limit:
                case DbExpressionKind.Element:
                    return result.Top == null
                           && result.Skip == null;

                case DbExpressionKind.Project:
                    // SQLBUDT #427998: Allow a Project to be compatible with an OrderBy
                    // Otherwise we won't be able to sort an input, and project out only
                    // a subset of the input columns
                    return result.Select.IsEmpty
                           && result.GroupBy.IsEmpty
                        // SQLBUDT #513640 - If distinct is specified, the projection may affect
                        // the cardinality of the results, thus a new statement must be started.
                           && !result.IsDistinct;

                case DbExpressionKind.Skip:
                    return result.Select.IsEmpty
                           && result.GroupBy.IsEmpty
                           && result.OrderBy.IsEmpty
                           && !result.IsDistinct;

                case DbExpressionKind.Sort:
                    return result.Select.IsEmpty
                           && result.GroupBy.IsEmpty
                           && result.OrderBy.IsEmpty
                        // SQLBUDT #513640 - A Project may be on the top of the Sort, and if so, it would need
                        // to be in the same statement as the Sort (see comment above for the Project case).
                        // A Distinct in the same statement would prevent that, and therefore if Distinct is present,
                        // we need to start a new statement. 
                           && !result.IsDistinct;

                default:
                    Debug.Assert(false);
                    throw ADP1.InvalidOperation(String.Empty);
            }
        }

        // <summary>
        // We use the normal box quotes for SQL server.  We do not deal with ANSI quotes
        // i.e. double quotes.
        // </summary>
        internal static string QuoteIdentifier(string name)
        {
            DebugCheck.NotEmpty(name);
            // We assume that the names are not quoted to begin with.
            return "[" + name.Replace("]", "]]") + "]";
        }

        // <summary>
        // This method is used for transforming INTERSECT and EXCEPT for Sql8,
        // which does not support INTERSECT and EXCEPT.
        // The result is of the following format:
        // SELECT DISTINCT a.a1, a.a2, ..., a.an
        // FROM x as a
        // WHERE (NOT) EXISTS(SELECT 0
        // FROM y as b
        // WHERE (b.b1 = a.a1 or (b.b1 is null and a.a1 is null))
        // AND (b.b2 = a.a2 or (b.b2 is null and a.a2 is null))
        // AND ...
        // AND (b.bn = a.an or (b.bn is null and a.an is null)))
        // where (NOT) is present when translating EXCEPT
        // </summary>
        private ISqlFragment TransformIntersectOrExcept(DbExpression left, DbExpression right, bool isExcept)
        {
            //Create the inner statement
            var inputRightStatement = VisitExpressionEnsureSqlStatement(right);
            Symbol rightSymbol;

            var rightCollectionType = TypeHelpers.GetEdmType<CollectionType>(right.ResultType);

            var newRightSelectStatement = CreateNewSelectStatement(inputRightStatement, "b", rightCollectionType.TypeUsage, out rightSymbol);
            newRightSelectStatement.FromExtents.Add(rightSymbol);
            newRightSelectStatement.Select.Append("0");
            newRightSelectStatement.From.Append(" AS ");
            newRightSelectStatement.From.Append(rightSymbol);
            var rightColumnSymbols = AddDefaultColumns(newRightSelectStatement);

            //Create the outer statement
            var inputLeftStatement = VisitExpressionEnsureSqlStatement(left);
            Symbol leftSymbol;

            var leftCollectionType = TypeHelpers.GetEdmType<CollectionType>(left.ResultType);

            var newSelectStatement = CreateNewSelectStatement(inputLeftStatement, "a", leftCollectionType.TypeUsage, out leftSymbol);
            newSelectStatement.FromExtents.Add(leftSymbol);
            newSelectStatement.IsDistinct = true;
            newSelectStatement.From.Append(" AS ");
            newSelectStatement.From.Append(leftSymbol);
            var leftColumnSymbols = AddDefaultColumns(newSelectStatement);

            Debug.Assert(
                leftColumnSymbols.Count == rightColumnSymbols.Count,
                "The left and the right input to INTERSECT or EXCEPT have a different number of properties");
            Debug.Assert(leftColumnSymbols.Count != 0, "The inputs to INTERSECT or EXCEPT have no properties");

            var isFirst = true;
            for (var i = 0; i < leftColumnSymbols.Count; i++)
            {
                if (!isFirst)
                {
                    newRightSelectStatement.Where.Append(" AND ");
                }
                else
                {
                    isFirst = false;
                }

                newRightSelectStatement.Where.Append("( ");

                // leftSymbol.columnSymbol = rightSymbol.columnSymbol
                newRightSelectStatement.Where.Append(leftSymbol);
                newRightSelectStatement.Where.Append(".");
                newRightSelectStatement.Where.Append(leftColumnSymbols[i]);
                newRightSelectStatement.Where.Append(" = ");
                newRightSelectStatement.Where.Append(rightSymbol);
                newRightSelectStatement.Where.Append(".");
                newRightSelectStatement.Where.Append(rightColumnSymbols[i]);

                newRightSelectStatement.Where.Append(" OR (");

                // leftSymbol.columnSymbol IS NULL AND RightSymbol.columnSymbol IS NULL
                newRightSelectStatement.Where.Append(leftSymbol);
                newRightSelectStatement.Where.Append(".");
                newRightSelectStatement.Where.Append(leftColumnSymbols[i]);
                newRightSelectStatement.Where.Append(" IS NULL AND ");
                newRightSelectStatement.Where.Append(rightSymbol);
                newRightSelectStatement.Where.Append(".");
                newRightSelectStatement.Where.Append(rightColumnSymbols[i]);
                newRightSelectStatement.Where.Append(" IS NULL");

                newRightSelectStatement.Where.Append(") )");
            }

            if (isExcept)
            {
                newSelectStatement.Where.Append(" NOT ");
            }

            newSelectStatement.Where.Append(" EXISTS ( ");
            newSelectStatement.Where.Append(newRightSelectStatement);
            newSelectStatement.Where.Append(" ) ");

            return newSelectStatement;
        }

        // <summary>
        // Simply calls <see cref="VisitExpressionEnsureSqlStatement(DbExpression, bool)" />
        // with addDefaultColumns set to true
        // </summary>
        private SqlSelectStatement VisitExpressionEnsureSqlStatement(DbExpression e)
        {
            return VisitExpressionEnsureSqlStatement(e, true);
        }

        // <summary>
        // This is called from <see cref="GenerateSql(DbQueryCommandTree)" /> and nodes which require a
        // select statement as an argument e.g. <see cref="Visit(DbIsEmptyExpression)" />,
        // <see cref="Visit(DbUnionAllExpression)" />.
        // SqlGenerator needs its child to have a proper alias if the child is
        // just an extent or a join.
        // The normal relational nodes result in complete valid SQL statements.
        // For the rest, we need to treat them as there was a dummy
        // <code>-- originally {expression}
        //     -- change that to
        //     SELECT *
        //     FROM {expression} as c</code>
        // DbLimitExpression needs to start the statement but not add the default columns
        // </summary>
        private SqlSelectStatement VisitExpressionEnsureSqlStatement(DbExpression e, bool addDefaultColumns)
        {
            Debug.Assert(TypeSemantics.IsCollectionType(e.ResultType));

            SqlSelectStatement result;
            switch (e.ExpressionKind)
            {
                case DbExpressionKind.Project:
                case DbExpressionKind.Filter:
                case DbExpressionKind.GroupBy:
                case DbExpressionKind.Sort:
                    result = e.Accept(this) as SqlSelectStatement;
                    break;

                default:
                    Symbol fromSymbol;
                    var inputVarName = "c"; // any name will do - this is my random choice.
                    symbolTable.EnterScope();

                    TypeUsage type = null;
                    switch (e.ExpressionKind)
                    {
                        case DbExpressionKind.Scan:
                        case DbExpressionKind.CrossJoin:
                        case DbExpressionKind.FullOuterJoin:
                        case DbExpressionKind.InnerJoin:
                        case DbExpressionKind.LeftOuterJoin:
                        case DbExpressionKind.CrossApply:
                        case DbExpressionKind.OuterApply:
                            // #490026: It used to be type = e.ResultType. 
                            type = TypeHelpers.GetElementTypeUsage(e.ResultType);
                            break;

                        default:
                            Debug.Assert(TypeSemantics.IsCollectionType(e.ResultType));
                            type = TypeHelpers.GetEdmType<CollectionType>(e.ResultType).TypeUsage;
                            break;
                    }

                    result = VisitInputExpression(e, inputVarName, type, out fromSymbol);
                    AddFromSymbol(result, inputVarName, fromSymbol);
                    symbolTable.ExitScope();
                    break;
            }

            if (addDefaultColumns && result.Select.IsEmpty)
            {
                AddDefaultColumns(result);
            }

            return result;
        }

        // <summary>
        // This method is called by <see cref="Visit(DbFilterExpression)" /> and
        // <see cref="Visit(DbQuantifierExpression)" />
        // </summary>
        // <param name="negatePredicate">
        // This is passed from <see cref="Visit(DbQuantifierExpression)" /> in the All(...) case.
        // </param>
        private SqlSelectStatement VisitFilterExpression(DbExpressionBinding input, DbExpression predicate, bool negatePredicate)
        {
            Symbol fromSymbol;
            var result = VisitInputExpression(
                input.Expression,
                input.VariableName, input.VariableType, out fromSymbol);

            // Filter is compatible with OrderBy
            // but not with Project, another Filter or GroupBy
            if (!IsCompatible(result, DbExpressionKind.Filter))
            {
                result = CreateNewSelectStatement(result, input.VariableName, input.VariableType, out fromSymbol);
            }

            selectStatementStack.Push(result);
            symbolTable.EnterScope();

            AddFromSymbol(result, input.VariableName, fromSymbol);

            if (negatePredicate)
            {
                result.Where.Append("NOT (");
            }
            result.Where.Append(predicate.Accept(this));
            if (negatePredicate)
            {
                result.Where.Append(")");
            }

            symbolTable.ExitScope();
            selectStatementStack.Pop();

            return result;
        }

        // <summary>
        // If the sql fragment for an input expression is not a SqlSelect statement
        // or other acceptable form (e.g. an extent as a SqlBuilder), we need
        // to wrap it in a form acceptable in a FROM clause.  These are
        // primarily the
        // <list type="bullet">
        //     <item>The set operation expressions - union all, intersect, except</item>
        //     <item>TVFs, which are conceptually similar to tables</item>
        // </list>
        // </summary>
        private static void WrapNonQueryExtent(SqlSelectStatement result, ISqlFragment sqlFragment, DbExpressionKind expressionKind)
        {
            switch (expressionKind)
            {
                case DbExpressionKind.Function:
                    // TVF
                    throw ADP1.NotSupported(EntityRes.GetString(EntityRes.TVFsNotSupported));

                default:
                    result.From.Append(" (");
                    result.From.Append(sqlFragment);
                    result.From.Append(")");
                    break;
            }
        }

        // <summary>
        // Is this a Store function (ie) does it have the builtinAttribute specified and it is not a canonical function?
        // </summary>
        private static bool IsStoreFunction(EdmFunction function)
        {
            return function.BuiltInAttribute && !TypeHelpers.IsCanonicalFunction(function);
        }

        private static string ByteArrayToBinaryString(Byte[] binaryArray)
        {
            var sb = new StringBuilder(binaryArray.Length * 2);
            for (var i = 0; i < binaryArray.Length; i++)
            {
                sb.Append(_hexDigits[(binaryArray[i] & 0xF0) >> 4]).Append(_hexDigits[binaryArray[i] & 0x0F]);
            }
            return sb.ToString();
        }

        // <summary>
        // Helper method for the Group By visitor
        // Returns true if at least one of the aggregates in the given list
        // has an argument that is not a <see cref="DbConstantExpression" /> and is not
        // a <see cref="DbPropertyExpression" /> over <see cref="DbVariableReferenceExpression" />,
        // either potentially capped with a <see cref="DbCastExpression" />
        // This is really due to the following two limitations of Sql Server:
        // <list type="number">
        //     <item>
        //         If an expression being aggregated contains an outer reference, then that outer
        //         reference must be the only column referenced in the expression (SQLBUDT #488741)
        //     </item>
        //     <item>
        //         Sql Server cannot perform an aggregate function on an expression containing
        //         an aggregate or a subquery. (SQLBUDT #504600)
        //     </item>
        // </list>
        // Potentially, we could furhter optimize this.
        // </summary>
        private static bool GroupByAggregatesNeedInnerQuery(IList<DbAggregate> aggregates, string inputVarRefName)
        {
            foreach (var aggregate in aggregates)
            {
                Debug.Assert(aggregate.Arguments.Count == 1);
                if (GroupByAggregateNeedsInnerQuery(aggregate.Arguments[0], inputVarRefName))
                {
                    return true;
                }
            }
            return false;
        }

        // <summary>
        // Returns true if the given expression is not a <see cref="DbConstantExpression" /> or a
        // <see cref="DbPropertyExpression" /> over  a <see cref="DbVariableReferenceExpression" />
        // referencing the given inputVarRefName, either
        // potentially capped with a <see cref="DbCastExpression" />.
        // </summary>
        private static bool GroupByAggregateNeedsInnerQuery(DbExpression expression, string inputVarRefName)
        {
            return GroupByExpressionNeedsInnerQuery(expression, inputVarRefName, true);
        }

        // <summary>
        // Helper method for the Group By visitor
        // Returns true if at least one of the expressions in the given list
        // is not <see cref="DbPropertyExpression" /> over <see cref="DbVariableReferenceExpression" />
        // referencing the given inputVarRefName potentially capped with a <see cref="DbCastExpression" />.
        // This is really due to the following limitation: Sql Server requires each GROUP BY expression
        // (key) to contain at least one column that is not an outer reference. (SQLBUDT #616523)
        // Potentially, we could further optimize this.
        // </summary>
        private static bool GroupByKeysNeedInnerQuery(IList<DbExpression> keys, string inputVarRefName)
        {
            foreach (var key in keys)
            {
                if (GroupByKeyNeedsInnerQuery(key, inputVarRefName))
                {
                    return true;
                }
            }
            return false;
        }

        // <summary>
        // Returns true if the given expression is not <see cref="DbPropertyExpression" /> over
        // <see cref="DbVariableReferenceExpression" /> referencing the given inputVarRefName
        // potentially capped with a <see cref="DbCastExpression" />.
        // This is really due to the following limitation: Sql Server requires each GROUP BY expression
        // (key) to contain at least one column that is not an outer reference. (SQLBUDT #616523)
        // Potentially, we could further optimize this.
        // </summary>
        private static bool GroupByKeyNeedsInnerQuery(DbExpression expression, string inputVarRefName)
        {
            return GroupByExpressionNeedsInnerQuery(expression, inputVarRefName, false);
        }

        // <summary>
        // Helper method for processing Group By keys and aggregates.
        // Returns true if the given expression is not a <see cref="DbConstantExpression" />
        // (and allowConstants is specified)or a <see cref="DbPropertyExpression" /> over
        // a <see cref="DbVariableReferenceExpression" /> referencing the given inputVarRefName,
        // either potentially capped with a <see cref="DbCastExpression" />.
        // </summary>
        private static bool GroupByExpressionNeedsInnerQuery(DbExpression expression, string inputVarRefName, bool allowConstants)
        {
            //Skip a constant if constants are allowed
            if (allowConstants && (expression.ExpressionKind == DbExpressionKind.Constant))
            {
                return false;
            }

            //Skip a cast expression
            if (expression.ExpressionKind
                == DbExpressionKind.Cast)
            {
                var castExpression = (DbCastExpression)expression;
                return GroupByExpressionNeedsInnerQuery(castExpression.Argument, inputVarRefName, allowConstants);
            }

            //Allow Property(Property(...)), needed when the input is a join
            if (expression.ExpressionKind
                == DbExpressionKind.Property)
            {
                var propertyExpression = (DbPropertyExpression)expression;
                return GroupByExpressionNeedsInnerQuery(propertyExpression.Instance, inputVarRefName, allowConstants);
            }

            if (expression.ExpressionKind
                == DbExpressionKind.VariableReference)
            {
                var varRefExpression = expression as DbVariableReferenceExpression;
                return !varRefExpression.VariableName.Equals(inputVarRefName);
            }

            return true;
        }

        // <summary>
        // determines if the function requires the return type be enforced by use of a cast expression
        // </summary>
        private static bool CastReturnTypeToInt32(DbFunctionExpression e)
        {
            if (!_functionRequiresReturnTypeCast.Contains(e.Function.FullName))
            {
                return false;
            }

            for (var i = 0; i < e.Arguments.Count; i++)
            {
                var storeType = SqlCeProviderManifest.Instance.GetStoreType(e.Arguments[i].ResultType);
                if (_maxTypeNames.Contains(storeType.EdmType.Name))
                {
                    return true;
                }
            }
            return false;
        }

        // <summary>
        // determines if the function requires the return type be enforced by use of a cast expression
        // </summary>
        internal static bool CastReturnTypeToSingle(DbFunctionExpression e)
        {
            //Do not add the cast for the Round() overload having 2 arguments. 
            //Round(Single,Int32) maps to Round(Double,Int32)due to implicit casting. 
            //We don't need to cast in that case, since we expect a Double as return type there anyways.
            return CastReturnTypeToGivenType(e, _functionRequiresReturnTypeCastToSingle, PrimitiveTypeKind.Single);
        }

        // <summary>
        // Determines if the function requires the return type be enforced by use of a cast expression
        // </summary>
        private static bool CastReturnTypeToGivenType(
            DbFunctionExpression e, ISet<string> functionsRequiringReturnTypeCast, PrimitiveTypeKind type)
        {
            if (!functionsRequiringReturnTypeCast.Contains(e.Function.FullName))
            {
                return false;
            }
            for (var i = 0; i < e.Arguments.Count; i++)
            {
                var storeType = SqlCeProviderManifest.Instance.GetStoreType(e.Arguments[i].ResultType);
                if (TypeSemantics.IsPrimitiveType(e.Arguments[i].ResultType, type))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #endregion
    }
}
