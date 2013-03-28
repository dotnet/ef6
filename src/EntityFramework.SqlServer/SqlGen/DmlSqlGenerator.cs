// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.SqlGen
{
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.SqlServer.Resources;
    using System.Data.Entity.SqlServer.Utilities;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     Class generating SQL for a DML command tree.
    /// </summary>
    internal static class DmlSqlGenerator
    {
        private const int s_commandTextBuilderInitialCapacity = 256;
        private const string s_generatedValuesVariableName = "@generated_keys";

        internal static string GenerateUpdateSql(
            DbUpdateCommandTree tree,
            SqlGenerator sqlGenerator,
            out List<SqlParameter> parameters,
            bool generateReturningSql = true)
        {
            const string dummySetParameter = "@p";

            var commandText = new StringBuilder(s_commandTextBuilderInitialCapacity);
            var translator = new ExpressionTranslator(commandText, tree, null != tree.Returning, sqlGenerator);

            if (tree.SetClauses.Count == 0)
            {
                commandText.AppendLine("declare " + dummySetParameter + " int");
            }

            // update [schemaName].[tableName]
            commandText.Append("update ");
            tree.Target.Expression.Accept(translator);
            commandText.AppendLine();

            // set c1 = ..., c2 = ..., ...
            var first = true;
            commandText.Append("set ");
            foreach (DbSetClause setClause in tree.SetClauses)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    commandText.Append(", ");
                }
                setClause.Property.Accept(translator);
                commandText.Append(" = ");
                setClause.Value.Accept(translator);
            }

            if (first)
            {
                // If first is still true, it indicates there were no set
                // clauses. Introduce a fake set clause so that:
                // - we acquire the appropriate locks
                // - server-gen columns (e.g. timestamp) get recomputed
                //
                // We use the following pattern:
                //
                //  update Foo
                //  set @p = 0
                //  where ...
                commandText.Append(dummySetParameter + " = 0");
            }
            commandText.AppendLine();

            // where c1 = ..., c2 = ...
            commandText.Append("where ");
            tree.Predicate.Accept(translator);
            commandText.AppendLine();

            if (generateReturningSql)
            {
                GenerateReturningSql(commandText, tree, null, translator, tree.Returning, false);
            }

            parameters = translator.Parameters;

            return commandText.ToString();
        }

        internal static string GenerateDeleteSql(DbDeleteCommandTree tree, SqlGenerator sqlGenerator, out List<SqlParameter> parameters)
        {
            var commandText = new StringBuilder(s_commandTextBuilderInitialCapacity);
            var translator = new ExpressionTranslator(commandText, tree, false, sqlGenerator);

            // delete [schemaName].[tableName]
            commandText.Append("delete ");
            tree.Target.Expression.Accept(translator);
            commandText.AppendLine();

            // where c1 = ... AND c2 = ...
            commandText.Append("where ");
            tree.Predicate.Accept(translator);

            parameters = translator.Parameters;
            return commandText.ToString();
        }

        internal static string GenerateInsertSql(
            DbInsertCommandTree tree,
            SqlGenerator sqlGenerator,
            out List<SqlParameter> parameters,
            bool generateReturningSql = true)
        {
            var commandText = new StringBuilder(s_commandTextBuilderInitialCapacity);
            var translator = new ExpressionTranslator(
                commandText, tree,
                null != tree.Returning, sqlGenerator);

            var useGeneratedValuesVariable = UseGeneratedValuesVariable(tree, sqlGenerator.SqlVersion);
            var tableType = (EntityType)((DbScanExpression)tree.Target.Expression).Target.ElementType;

            if (useGeneratedValuesVariable)
            {
                // manufacture the variable, e.g. "declare @generated_values table(id uniqueidentifier)"
                commandText
                    .Append("declare ")
                    .Append(s_generatedValuesVariableName)
                    .Append(" table(");
                var first = true;
                foreach (var column in tableType.KeyMembers)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        commandText.Append(", ");
                    }
                    commandText
                        .Append(GenerateMemberTSql(column))
                        .Append(" ")
                        .Append(GetVariableType(sqlGenerator, column));
                    Facet collationFacet;
                    if (column.TypeUsage.Facets.TryGetValue(DbProviderManifest.CollationFacetName, false, out collationFacet))
                    {
                        var collation = collationFacet.Value as string;
                        if (!string.IsNullOrEmpty(collation))
                        {
                            commandText.Append(" collate ").Append(collation);
                        }
                    }
                }
                Debug.Assert(!first, "if useGeneratedValuesVariable is true, it implies some columns do not have values");
                commandText.AppendLine(")");
            }

            // insert [schemaName].[tableName]
            commandText.Append("insert ");
            tree.Target.Expression.Accept(translator);

            if (0 < tree.SetClauses.Count)
            {
                // (c1, c2, c3, ...)
                commandText.Append("(");
                var first = true;
                foreach (DbSetClause setClause in tree.SetClauses)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        commandText.Append(", ");
                    }
                    setClause.Property.Accept(translator);
                }
                commandText.AppendLine(")");
            }
            else
            {
                commandText.AppendLine();
            }

            if (useGeneratedValuesVariable)
            {
                // output inserted.id into @generated_values
                commandText.Append("output ");
                var first = true;
                foreach (var column in tableType.KeyMembers)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        commandText.Append(", ");
                    }
                    commandText.Append("inserted.");
                    commandText.Append(GenerateMemberTSql(column));
                }
                commandText
                    .Append(" into ")
                    .AppendLine(s_generatedValuesVariableName);
            }

            if (0 < tree.SetClauses.Count)
            {
                // values c1, c2, ...
                var first = true;
                commandText.Append("values (");
                foreach (DbSetClause setClause in tree.SetClauses)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        commandText.Append(", ");
                    }
                    setClause.Value.Accept(translator);

                    translator.RegisterMemberValue(setClause.Property, setClause.Value);
                }
                commandText.AppendLine(")");
            }
            else
            {
                // default values
                commandText.AppendLine("default values");
            }

            if (generateReturningSql)
            {
                GenerateReturningSql(commandText, tree, tableType, translator, tree.Returning, useGeneratedValuesVariable);
            }

            parameters = translator.Parameters;

            return commandText.ToString();
        }

        internal static string GetVariableType(SqlGenerator sqlGenerator, EdmMember column)
        {
            DebugCheck.NotNull(sqlGenerator);
            DebugCheck.NotNull(column);

            var columnType 
                = SqlGenerator.GenerateSqlForStoreType(sqlGenerator.SqlVersion, column.TypeUsage);

            if (columnType == "rowversion"
                || columnType == "timestamp")
            {
                // rowversion and timestamp are intrinsically read-only. use binary to gather server generated
                // values for these types.
                columnType = "binary(8)";
            }

            return columnType;
        }

        /// <summary>
        ///     Determine whether we should use a generated values variable to return server generated values.
        ///     This is true when we're attempting to insert a row where the primary key is server generated
        ///     but is not an integer type (and therefore can't be used with scope_identity()). It is also true
        ///     where there is a compound server generated key.
        /// </summary>
        internal static bool UseGeneratedValuesVariable(DbInsertCommandTree tree, SqlVersion sqlVersion)
        {
            var useGeneratedValuesVariable = false;
            if (sqlVersion > SqlVersion.Sql8
                && tree.Returning != null)
            {
                // Figure out which columns have values
                var columnsWithValues =
                    new HashSet<EdmMember>(tree.SetClauses.Cast<DbSetClause>().Select(s => ((DbPropertyExpression)s.Property).Property));

                // Only SQL Server 2005+ support an output clause for inserts
                var firstKeyFound = false;
                foreach (var keyMember in ((DbScanExpression)tree.Target.Expression).Target.ElementType.KeyMembers)
                {
                    if (!columnsWithValues.Contains(keyMember))
                    {
                        if (firstKeyFound)
                        {
                            // compound server gen key
                            useGeneratedValuesVariable = true;
                            break;
                        }

                        firstKeyFound = true;

                        if (!IsValidScopeIdentityColumnType(keyMember.TypeUsage))
                        {
                            // unsupported type
                            useGeneratedValuesVariable = true;
                            break;
                        }
                    }
                }
            }
            return useGeneratedValuesVariable;
        }

        // Generates T-SQL describing a member
        // Requires: member must belong to an entity type (a safe requirement for DML
        // SQL gen, where we only access table columns)
        internal static string GenerateMemberTSql(EdmMember member)
        {
            return SqlGenerator.QuoteIdentifier(member.Name);
        }

        /// <summary>
        ///     Generates SQL fragment returning server-generated values.
        ///     Requires: translator knows about member values so that we can figure out
        ///     how to construct the key predicate.
        ///     <code>Sample SQL:
        ///     
        ///         select IdentityValue
        ///         from dbo.MyTable
        ///         where @@ROWCOUNT > 0 and IdentityValue = scope_identity()
        /// 
        ///         or
        /// 
        ///         select TimestampValue
        ///         from dbo.MyTable
        ///         where @@ROWCOUNT > 0 and Id = 1
        /// 
        ///         Note that we filter on rowcount to ensure no rows are returned if no rows were modified.
        /// 
        ///         On SQL Server 2005 and up, we have an additional syntax used for non integer return types:
        /// 
        ///         declare @generatedValues table(ID uniqueidentifier)
        ///         insert dbo.MyTable
        ///         output ID into @generated_values
        ///         values (...);
        ///         select ID
        ///         from @generatedValues as g join dbo.MyTable as t on g.ID = t.ID
        ///         where @@ROWCOUNT > 0;</code>
        /// </summary>
        /// <param name="commandText"> Builder containing command text </param>
        /// <param name="tree"> Modification command tree </param>
        /// <param name="tableType"> Type of table. </param>
        /// <param name="translator"> Translator used to produce DML SQL statement for the tree </param>
        /// <param name="returning"> Returning expression. If null, the method returns immediately without producing a SELECT statement. </param>
        internal static void GenerateReturningSql(
            StringBuilder commandText, DbModificationCommandTree tree, EntityType tableType,
            ExpressionTranslator translator, DbExpression returning, bool useGeneratedValuesVariable)
        {
            // Nothing to do if there is no Returning expression
            if (null == returning)
            {
                return;
            }

            // select
            commandText.Append("select ");
            if (useGeneratedValuesVariable)
            {
                translator.PropertyAlias = "t";
            }
            returning.Accept(translator);
            if (useGeneratedValuesVariable)
            {
                translator.PropertyAlias = null;
            }
            commandText.AppendLine();

            if (useGeneratedValuesVariable)
            {
                // from @generated_values
                commandText.Append("from ");
                commandText.Append(s_generatedValuesVariableName);
                commandText.Append(" as g join ");
                tree.Target.Expression.Accept(translator);
                commandText.Append(" as t on ");
                var separator = string.Empty;
                foreach (var keyMember in tableType.KeyMembers)
                {
                    commandText.Append(separator);
                    separator = " and ";
                    commandText.Append("g.");
                    var memberTSql = GenerateMemberTSql(keyMember);
                    commandText.Append(memberTSql);
                    commandText.Append(" = t.");
                    commandText.Append(memberTSql);
                }
                commandText.AppendLine();
                commandText.Append("where @@ROWCOUNT > 0");
            }
            else
            {
                // from
                commandText.Append("from ");
                tree.Target.Expression.Accept(translator);
                commandText.AppendLine();

                // where
                commandText.Append("where @@ROWCOUNT > 0");
                var table = ((DbScanExpression)tree.Target.Expression).Target;
                var identity = false;
                foreach (var keyMember in table.ElementType.KeyMembers)
                {
                    commandText.Append(" and ");
                    commandText.Append(GenerateMemberTSql(keyMember));
                    commandText.Append(" = ");

                    // retrieve member value sql. the translator remembers member values
                    // as it constructs the DML statement (which precedes the "returning"
                    // SQL)
                    SqlParameter value;
                    if (translator.MemberValues.TryGetValue(keyMember, out value))
                    {
                        commandText.Append(value.ParameterName);
                    }
                    else
                    {
                        // if no value is registered for the key member, it means it is an identity
                        // which can be retrieved using the scope_identity() function
                        if (identity)
                        {
                            // there can be only one server generated key
                            throw new NotSupportedException(Strings.Update_NotSupportedServerGenKey(table.Name));
                        }

                        if (!IsValidScopeIdentityColumnType(keyMember.TypeUsage))
                        {
                            throw new InvalidOperationException(
                                Strings.Update_NotSupportedIdentityType(
                                    keyMember.Name, keyMember.TypeUsage.ToString()));
                        }

                        commandText.Append("scope_identity()");
                        identity = true;
                    }
                }
            }
        }

        private static bool IsValidScopeIdentityColumnType(TypeUsage typeUsage)
        {
            // SQL Server supports the following types for identity columns:
            // tinyint, smallint, int, bigint, decimal(p,0), or numeric(p,0)

            // make sure it's a primitive type
            if (typeUsage.EdmType.BuiltInTypeKind
                != BuiltInTypeKind.PrimitiveType)
            {
                return false;
            }

            // check if this is a supported primitive type (compare by name)
            var typeName = typeUsage.EdmType.Name;

            // integer types
            if (typeName == "tinyint"
                || typeName == "smallint"
                ||
                typeName == "int"
                || typeName == "bigint")
            {
                return true;
            }

            // variable scale types (require scale = 0)
            if (typeName == "decimal"
                || typeName == "numeric")
            {
                Facet scaleFacet;
                return (typeUsage.Facets.TryGetValue(
                    DbProviderManifest.ScaleFacetName,
                    false, out scaleFacet) && Convert.ToInt32(scaleFacet.Value, CultureInfo.InvariantCulture) == 0);
            }

            // type not in supported list
            return false;
        }

        /// <summary>
        ///     Lightweight expression translator for DML expression trees, which have constrained
        ///     scope and support.
        /// </summary>
        internal class ExpressionTranslator : BasicExpressionVisitor
        {
            /// <summary>
            ///     Initialize a new expression translator populating the given string builder
            ///     with command text. Command text builder and command tree must not be null.
            /// </summary>
            /// <param name="commandText"> Command text with which to populate commands </param>
            /// <param name="commandTree"> Command tree generating SQL </param>
            /// <param name="preserveMemberValues"> Indicates whether the translator should preserve member values while compiling t-SQL (only needed for server generation) </param>
            internal ExpressionTranslator(
                StringBuilder commandText,
                DbModificationCommandTree commandTree,
                bool preserveMemberValues,
                SqlGenerator sqlGenerator,
                ICollection<EdmProperty> localVariableBindings = null)
            {
                DebugCheck.NotNull(commandText);
                DebugCheck.NotNull(commandTree);

                _commandText = commandText;
                _commandTree = commandTree;
                _sqlGenerator = sqlGenerator;
                _localVariableBindings = localVariableBindings;

                _parameters = new List<SqlParameter>();

                _memberValues = preserveMemberValues
                                    ? new Dictionary<EdmMember, SqlParameter>()
                                    : null;
            }

            private readonly StringBuilder _commandText;
            private readonly DbModificationCommandTree _commandTree;
            private readonly List<SqlParameter> _parameters;
            private readonly Dictionary<EdmMember, SqlParameter> _memberValues;
            private readonly SqlGenerator _sqlGenerator;
            private readonly ICollection<EdmProperty> _localVariableBindings;

            internal List<SqlParameter> Parameters
            {
                get { return _parameters; }
            }

            internal Dictionary<EdmMember, SqlParameter> MemberValues
            {
                get { return _memberValues; }
            }

            internal string PropertyAlias { get; set; }

            // generate parameter (name based on parameter ordinal)
            internal SqlParameter CreateParameter(object value, TypeUsage type, string name = null)
            {
                // Suppress the MaxLength facet in the type usage because
                // SqlClient will silently truncate data when SqlParameter.Size < |SqlParameter.Value|.
                const bool preventTruncation = true;

                var parameter = SqlProviderServices.CreateSqlParameter(
                    name ?? GetParameterName(_parameters.Count), type, ParameterMode.In, value, preventTruncation, _sqlGenerator.SqlVersion);

                _parameters.Add(parameter);

                return parameter;
            }

            internal static string GetParameterName(int index)
            {
                return string.Concat("@", index.ToString(CultureInfo.InvariantCulture));
            }

            public override void Visit(DbAndExpression expression)
            {
                Check.NotNull(expression, "expression");

                VisitBinary(expression, " and ");
            }

            public override void Visit(DbOrExpression expression)
            {
                Check.NotNull(expression, "expression");

                VisitBinary(expression, " or ");
            }

            public override void Visit(DbComparisonExpression expression)
            {
                Check.NotNull(expression, "expression");

                Debug.Assert(
                    expression.ExpressionKind == DbExpressionKind.Equals,
                    "only equals comparison expressions are produced in DML command trees in V1");

                VisitBinary(expression, " = ");

                RegisterMemberValue(expression.Left, expression.Right);
            }

            /// <summary>
            ///     Call this method to register a property value pair so the translator "remembers"
            ///     the values for members of the row being modified. These values can then be used
            ///     to form a predicate for server-generation (based on the key of the row)
            /// </summary>
            /// <param name="propertyExpression"> DbExpression containing the column reference (property expression). </param>
            /// <param name="value"> DbExpression containing the value of the column. </param>
            internal void RegisterMemberValue(DbExpression propertyExpression, DbExpression value)
            {
                if (null != _memberValues)
                {
                    // register the value for this property
                    Debug.Assert(
                        propertyExpression.ExpressionKind == DbExpressionKind.Property,
                        "DML predicates and setters must be of the form property = value");

                    // get name of left property 
                    var property = ((DbPropertyExpression)propertyExpression).Property;

                    // don't track null values
                    if (value.ExpressionKind
                        != DbExpressionKind.Null)
                    {
                        Debug.Assert(
                            value.ExpressionKind == DbExpressionKind.Constant
                            || value.ExpressionKind == DbExpressionKind.ParameterReference,
                            "value must either constant or null");
                        // retrieve the last parameter added (which describes the parameter)
                        _memberValues[property] = _parameters[_parameters.Count - 1];
                    }
                }
            }

            public override void Visit(DbIsNullExpression expression)
            {
                Check.NotNull(expression, "expression");

                expression.Argument.Accept(this);
                _commandText.Append(" is null");
            }

            public override void Visit(DbNotExpression expression)
            {
                Check.NotNull(expression, "expression");

                _commandText.Append("not (");
                expression.Accept(this);
                _commandText.Append(")");
            }

            public override void Visit(DbConstantExpression expression)
            {
                Check.NotNull(expression, "expression");

                if (!_commandTree.Parameters.Any())
                {
                    var parameter = CreateParameter(expression.Value, expression.ResultType);

                    _commandText.Append(parameter.ParameterName);
                }
                else
                {
                    _commandText.Append(_sqlGenerator.WriteSql(expression.Accept(_sqlGenerator)));
                }
            }

            public override void Visit(DbParameterReferenceExpression expression)
            {
                Check.NotNull(expression, "expression");

                var parameter
                    = CreateParameter(
                        DBNull.Value,
                        expression.ResultType,
                        "@" + expression.ParameterName);

                _commandText.Append(parameter.ParameterName);
            }

            public override void Visit(DbScanExpression expression)
            {
                Check.NotNull(expression, "expression");

                // we know we won't hit this code unless there is no function defined for this
                // ModificationOperation, so if this EntitySet is using a DefiningQuery, instead
                // of a table, that is an error
                if (expression.Target.GetMetadataPropertyValue<string>("DefiningQuery") != null)
                {
                    string missingCudElement;
                    if (_commandTree is DbDeleteCommandTree)
                    {
                        missingCudElement = "DeleteFunction";
                    }
                    else if (_commandTree is DbInsertCommandTree)
                    {
                        missingCudElement = "InsertFunction";
                    }
                    else
                    {
                        Debug.Assert(_commandTree is DbUpdateCommandTree);
                        missingCudElement = "UpdateFunction";
                    }
                    throw new UpdateException(
                        Strings.Update_SqlEntitySetWithoutDmlFunctions(
                            expression.Target.Name, missingCudElement, "ModificationFunctionMapping"));
                }

                _commandText.Append(SqlGenerator.GetTargetTSql(expression.Target));
            }

            public override void Visit(DbPropertyExpression expression)
            {
                Check.NotNull(expression, "expression");

                if (!string.IsNullOrEmpty(PropertyAlias))
                {
                    _commandText.Append(PropertyAlias);
                    _commandText.Append(".");
                }

                _commandText.Append(GenerateMemberTSql(expression.Property));
            }

            public override void Visit(DbNullExpression expression)
            {
                Check.NotNull(expression, "expression");

                _commandText.Append("null");
            }

            public override void Visit(DbNewInstanceExpression expression)
            {
                Check.NotNull(expression, "expression");

                // assumes all arguments are self-describing (no need to use aliases
                // because no renames are ever used in the projection)

                var first = true;

                foreach (var argument in expression.Arguments)
                {
                    var property = ((DbPropertyExpression)argument).Property;

                    var variableAssignment
                        = (_localVariableBindings != null)
                              ? (_localVariableBindings.Contains(property)
                                     ? "@" + property.Name + " = "
                                     : null)
                              : string.Empty;

                    if (variableAssignment != null)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            _commandText.Append(", ");
                        }

                        _commandText.Append(variableAssignment);

                        argument.Accept(this);
                    }
                }
            }

            private void VisitBinary(DbBinaryExpression expression, string separator)
            {
                _commandText.Append("(");
                expression.Left.Accept(this);
                _commandText.Append(separator);
                expression.Right.Accept(this);
                _commandText.Append(")");
            }
        }
    }
}
