// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.SqlGen
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.SqlServer.Utilities;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;

    internal class DmlFunctionSqlGenerator
    {
        private readonly SqlGenerator _sqlGenerator;

        public DmlFunctionSqlGenerator(SqlGenerator sqlGenerator)
        {
            DebugCheck.NotNull(sqlGenerator);

            _sqlGenerator = sqlGenerator;
        }

        public string GenerateInsert(ICollection<DbInsertCommandTree> commandTrees)
        {
            DebugCheck.NotNull(commandTrees);

            var sql = new StringBuilder();

            List<SqlParameter> _;

            var firstCommandTree = commandTrees.First();

            sql.Append(
                DmlSqlGenerator.GenerateInsertSql(
                    firstCommandTree,
                    _sqlGenerator,
                    out _,
                    generateReturningSql: false,
                    upperCaseKeywords: true));

            sql.AppendLine();

            var firstTable
                = (EntityType)((DbScanExpression)firstCommandTree.Target.Expression).Target.ElementType;

            sql.Append(IntroduceRequiredLocalVariables(firstTable, firstCommandTree));

            foreach (var commandTree in commandTrees.Skip(1))
            {
                sql.Append(
                    DmlSqlGenerator.GenerateInsertSql(
                        commandTree,
                        _sqlGenerator,
                        out _, generateReturningSql: false,
                        upperCaseKeywords: true));

                sql.AppendLine();
            }

            var returningCommandTrees
                = commandTrees
                    .Where(ct => ct.Returning != null)
                    .ToList();

            if (returningCommandTrees.Any())
            {
                var returningSelectSqlGenerator = new ReturningSelectSqlGenerator();

                foreach (var commandTree in returningCommandTrees)
                {
                    commandTree.Target.Expression.Accept(returningSelectSqlGenerator);
                    commandTree.Returning.Accept(returningSelectSqlGenerator);
                }

                foreach (var keyProperty in firstTable.KeyProperties)
                {
                    var parameterReference
                        = firstCommandTree
                              .SetClauses
                              .Cast<DbSetClause>()
                              .Where(sc => ((DbPropertyExpression)sc.Property).Property == keyProperty)
                              .Select(sc => sc.Value)
                              .SingleOrDefault()
                          ?? keyProperty.TypeUsage.Parameter(keyProperty.Name);

                    firstCommandTree
                        .Target
                        .Variable
                        .Property(keyProperty)
                        .Equal(parameterReference)
                        .Accept(returningSelectSqlGenerator);
                }

                sql.Append(returningSelectSqlGenerator.Sql);
            }

            return sql.ToString().TrimEnd();
        }

        private string IntroduceRequiredLocalVariables(EntityType entityType, DbInsertCommandTree commandTree)
        {
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(commandTree);

            var storeGeneratedKeys
                = entityType
                    .KeyProperties
                    .Where(p => p.IsStoreGeneratedIdentity)
                    .ToList();

            var sql = new SqlStringBuilder { UpperCaseKeywords = true };

            if (storeGeneratedKeys.Any())
            {
                foreach (var keyProperty in storeGeneratedKeys)
                {
                    sql.Append(sql.Length == 0 ? "DECLARE " : ", ");
                    sql.Append("@");
                    sql.Append(keyProperty.Name);
                    sql.Append(" ");
                    sql.Append(DmlSqlGenerator.GetVariableType(_sqlGenerator, keyProperty));
                }

                sql.AppendLine();

                var translator
                    = new DmlSqlGenerator.ExpressionTranslator(
                        sql,
                        commandTree,
                        true,
                        _sqlGenerator,
                        entityType.KeyProperties);

                DmlSqlGenerator.GenerateReturningSql(
                    sql,
                    commandTree,
                    entityType,
                    translator,
                    commandTree.Returning,
                    DmlSqlGenerator.UseGeneratedValuesVariable(commandTree, _sqlGenerator.SqlVersion));

                sql.AppendLine();
                sql.AppendLine();
            }

            return sql.ToString();
        }

        public string GenerateUpdate(ICollection<DbUpdateCommandTree> commandTrees, string rowsAffectedParameter)
        {
            DebugCheck.NotNull(commandTrees);

            List<SqlParameter> _;

            var sql = new StringBuilder();

            sql.AppendLine(
                DmlSqlGenerator.GenerateUpdateSql(
                    commandTrees.First(),
                    _sqlGenerator,
                    out _,
                    generateReturningSql: false,
                    upperCaseKeywords: true));

            foreach (var commandTree in commandTrees.Skip(1))
            {
                sql.Append(
                    DmlSqlGenerator.GenerateUpdateSql(
                        commandTree,
                        _sqlGenerator,
                        out _,
                        generateReturningSql: false,
                        upperCaseKeywords: true));

                sql.AppendLine("AND @@ROWCOUNT > 0");
                sql.AppendLine();
            }

            var returningCommandTrees
                = commandTrees
                    .Where(ct => ct.Returning != null)
                    .ToList();

            if (returningCommandTrees.Any())
            {
                var returningSelectSqlGenerator = new ReturningSelectSqlGenerator();

                foreach (var commandTree in returningCommandTrees)
                {
                    commandTree.Target.Expression.Accept(returningSelectSqlGenerator);
                    commandTree.Returning.Accept(returningSelectSqlGenerator);
                    commandTree.Predicate.Accept(returningSelectSqlGenerator);
                }

                sql.AppendLine(returningSelectSqlGenerator.Sql);
                sql.AppendLine();
            }

            AppendSetRowsAffected(sql, rowsAffectedParameter);

            return sql.ToString().TrimEnd();
        }

        public string GenerateDelete(ICollection<DbDeleteCommandTree> commandTrees, string rowsAffectedParameter)
        {
            DebugCheck.NotNull(commandTrees);

            List<SqlParameter> _;

            var sql = new StringBuilder();

            sql.AppendLine(
                DmlSqlGenerator.GenerateDeleteSql(
                    commandTrees.First(),
                    _sqlGenerator,
                    out _,
                    upperCaseKeywords: true));

            sql.AppendLine();

            foreach (var commandTree in commandTrees.Skip(1))
            {
                sql.AppendLine(
                    DmlSqlGenerator.GenerateDeleteSql(
                        commandTree,
                        _sqlGenerator,
                        out _,
                        upperCaseKeywords: true));

                sql.AppendLine("AND @@ROWCOUNT > 0");
                sql.AppendLine();
            }

            AppendSetRowsAffected(sql, rowsAffectedParameter);

            return sql.ToString().TrimEnd();
        }

        private static void AppendSetRowsAffected(StringBuilder sql, string rowsAffectedParameter)
        {
            DebugCheck.NotNull(sql);

            if (!string.IsNullOrWhiteSpace(rowsAffectedParameter))
            {
                sql.Append("SET @");
                sql.Append(rowsAffectedParameter);
                sql.AppendLine(" = @@ROWCOUNT");
                sql.AppendLine();
            }
        }

        private sealed class ReturningSelectSqlGenerator : BasicExpressionVisitor
        {
            private readonly StringBuilder _select = new StringBuilder();
            private readonly StringBuilder _from = new StringBuilder();
            private readonly StringBuilder _where = new StringBuilder();

            private int _aliasCount;
            private string _currentTableAlias;
            private EntityType _baseTable;
            private string _nextPropertyAlias;

            public string Sql
            {
                get
                {
                    var sql = new StringBuilder();

                    sql.AppendLine(_select.ToString());
                    sql.AppendLine(_from.ToString());
                    sql.Append("WHERE @@ROWCOUNT > 0");
                    sql.Append(_where);

                    return sql.ToString();
                }
            }

            public override void Visit(DbNewInstanceExpression newInstanceExpression)
            {
                DebugCheck.NotNull(newInstanceExpression);

                var properties = ((RowType)newInstanceExpression.ResultType.EdmType).Properties;

                for (var i = 0; i < properties.Count; i++)
                {
                    _select.Append(_select.Length == 0 ? "SELECT " : ", ");

                    _nextPropertyAlias = properties[i].Name;

                    newInstanceExpression.Arguments[i].Accept(this);
                }

                _nextPropertyAlias = null;
            }

            public override void Visit(DbScanExpression scanExpression)
            {
                DebugCheck.NotNull(scanExpression);

                var tableSql
                    = SqlGenerator.GetTargetTSql(scanExpression.Target)
                      + " AS "
                      + (_currentTableAlias = "t" + _aliasCount++);

                var table = scanExpression.Target.ElementType;

                if (_from.Length == 0)
                {
                    _baseTable = (EntityType)table;

                    _from.Append("FROM ");
                    _from.Append(tableSql);
                }
                else
                {
                    _from.AppendLine();
                    _from.Append("JOIN ");
                    _from.Append(tableSql);
                    _from.Append(" ON ");

                    for (var i = 0; i < table.KeyMembers.Count; i++)
                    {
                        if (i > 0)
                        {
                            _from.Append(" AND ");
                        }

                        _from.Append(_currentTableAlias + ".");
                        _from.Append(SqlGenerator.QuoteIdentifier(table.KeyMembers[i].Name));
                        _from.Append(" = t0.");
                        _from.Append(SqlGenerator.QuoteIdentifier(_baseTable.KeyMembers[i].Name));
                    }
                }
            }

            public override void Visit(DbPropertyExpression propertyExpression)
            {
                DebugCheck.NotNull(propertyExpression);

                _select.Append(_currentTableAlias);
                _select.Append(".");
                _select.Append(SqlGenerator.QuoteIdentifier(propertyExpression.Property.Name));

                if (!string.IsNullOrWhiteSpace(_nextPropertyAlias)
                    && !string.Equals(_nextPropertyAlias, propertyExpression.Property.Name, StringComparison.Ordinal))
                {
                    _select.Append(" AS ");
                    _select.Append(_nextPropertyAlias);
                }
            }

            public override void Visit(DbParameterReferenceExpression expression)
            {
                DebugCheck.NotNull(expression);

                _where.Append("@" + expression.ParameterName);
            }

            public override void Visit(DbIsNullExpression expression)
            {
                // no-op
            }

            public override void Visit(DbComparisonExpression comparisonExpression)
            {
                DebugCheck.NotNull(comparisonExpression);

                var property
                    = ((DbPropertyExpression)comparisonExpression.Left).Property;

                if (_baseTable.KeyMembers.Contains(property))
                {
                    _where.Append(" AND t0.");
                    _where.Append(SqlGenerator.QuoteIdentifier(property.Name));
                    _where.Append(" = ");

                    comparisonExpression.Right.Accept(this);
                }
            }
        }
    }
}
