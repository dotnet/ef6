// ﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees;
using System.Data.Common.Utils;
using System.Data.Mapping.Update.Internal;


namespace SampleEntityFrameworkProvider
{
    /// <summary>
    /// Class generating SQL for a DML command tree.
    /// </summary>
    internal static class DmlSqlGenerator
    {
        private static readonly int s_commandTextBuilderInitialCapacity = 256;

        internal static string GenerateUpdateSql(DbUpdateCommandTree tree, out List<DbParameter> parameters)
        {
            StringBuilder commandText = new StringBuilder(s_commandTextBuilderInitialCapacity);
            ExpressionTranslator translator = new ExpressionTranslator(commandText, tree, null != tree.Returning);

            // update [schemaName].[tableName]
            commandText.Append("update ");
            tree.Target.Expression.Accept(translator);
            commandText.AppendLine();

            // set c1 = ..., c2 = ..., ...
            bool first = true;
            commandText.Append("set ");
            foreach (DbSetClause setClause in tree.SetClauses)
            {
                if (first) { first = false; }
                else { commandText.Append(", "); }
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
                //  set @i = 0
                //  where ...
                DbParameter parameter = 
                    translator.CreateParameter(
                    default(Int32), 
                    TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)));

                commandText.Append(parameter.ParameterName);
                commandText.Append(" = 0");
            }
            commandText.AppendLine();

            // where c1 = ..., c2 = ...
            commandText.Append("where ");
            tree.Predicate.Accept(translator);
            commandText.AppendLine();

            // generate returning sql
            GenerateReturningSql(commandText, tree, translator, tree.Returning); 

            parameters = translator.Parameters;
            return commandText.ToString();
        }

        internal static string GenerateDeleteSql(DbDeleteCommandTree tree, out List<DbParameter> parameters)
        {
            StringBuilder commandText = new StringBuilder(s_commandTextBuilderInitialCapacity);
            ExpressionTranslator translator = new ExpressionTranslator(commandText, tree, false);

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

        internal static string GenerateInsertSql(DbInsertCommandTree tree, out List<DbParameter> parameters)
        {
            StringBuilder commandText = new StringBuilder(s_commandTextBuilderInitialCapacity);
            ExpressionTranslator translator = new ExpressionTranslator(commandText, tree,
                null != tree.Returning);

            // insert [schemaName].[tableName]
            commandText.Append("insert ");
            tree.Target.Expression.Accept(translator);
            
            // (c1, c2, c3, ...)
            commandText.Append("(");
            bool first = true;
            foreach (DbSetClause setClause in tree.SetClauses)
            {
                if (first) { first = false; }
                else { commandText.Append(", "); }
                setClause.Property.Accept(translator);
            }
            commandText.AppendLine(")");

            // values c1, c2, ...
            first = true;
            commandText.Append("values (");
            foreach (DbSetClause setClause in tree.SetClauses)
            {
                if (first) { first = false; }
                else { commandText.Append(", "); }
                setClause.Value.Accept(translator);

                translator.RegisterMemberValue(setClause.Property, setClause.Value);
            }
            commandText.AppendLine(")");

            // generate returning sql
            GenerateReturningSql(commandText, tree, translator, tree.Returning); 

            parameters = translator.Parameters;
            return commandText.ToString();
        }

        // Generates T-SQL describing a member
        // Requires: member must belong to an entity type (a safe requirement for DML
        // SQL gen, where we only access table columns)
        private static string GenerateMemberTSql(EdmMember member)
        {
            return SqlGenerator.QuoteIdentifier(member.Name);
        }

        /// <summary>
        /// Generates SQL fragment returning server-generated values.
        /// Requires: translator knows about member values so that we can figure out
        /// how to construct the key predicate.
        /// <code>
        /// Sample SQL:
        ///     
        ///     select IdentityValue
        ///     from dbo.MyTable
        ///     where @@ROWCOUNT > 0 and IdentityValue = scope_identity()
        /// 
        /// or
        /// 
        ///     select TimestamptValue
        ///     from dbo.MyTable
        ///     where @@ROWCOUNT > 0 and Id = 1
        /// 
        /// Note that we filter on rowcount to ensure no rows are returned if no rows were modified.
        /// </code>
        /// </summary>
        /// <param name="commandText">Builder containing command text</param>
        /// <param name="tree">Modification command tree</param>
        /// <param name="translator">Translator used to produce DML SQL statement
        /// for the tree</param>
        /// <param name="returning">Returning expression. If null, the method returns
        /// immediately without producing a SELECT statement.</param>
        private static void GenerateReturningSql(StringBuilder commandText, DbModificationCommandTree tree, 
            ExpressionTranslator translator, DbExpression returning)
        {
            // Nothing to do if there is no Returning expression
            if (null == returning) { return; }

            // select
            commandText.Append("select ");
            returning.Accept(translator);
            commandText.AppendLine();

            // from
            commandText.Append("from ");
            tree.Target.Expression.Accept(translator);
            commandText.AppendLine();

            // where
            commandText.Append("where @@ROWCOUNT > 0");
            EntitySetBase table = ((DbScanExpression)tree.Target.Expression).Target;
            bool identity = false;
            foreach (EdmMember keyMember in table.ElementType.KeyMembers)
            {
                commandText.Append(" and ");
                commandText.Append(GenerateMemberTSql(keyMember));
                commandText.Append(" = ");

                // retrieve member value sql. the translator remembers member values
                // as it constructs the DML statement (which precedes the "returning"
                // SQL)
                DbParameter value;
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
                        throw new NotSupportedException(string.Format("Server generated keys are only supported for identity columns. More than one key column is marked as server generated in table '{0}'.", table.Name));
                    }
                    commandText.Append("scope_identity()");
                    identity = true;
                }
            }
        }

        /// <summary>
        /// Lightweight expression translator for DML expression trees, which have constrained
        /// scope and support.
        /// </summary>
        private class ExpressionTranslator : DbExpressionVisitor
        {
            #region UnsupportedVisitMethods

            public override void Visit(DbApplyExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbApplyExpression\") is not supported.");
            }

            public override void Visit(DbArithmeticExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbArithmeticExpression\") is not supported.");
            }

            public override void Visit(DbCaseExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbCaseExpression\") is not supported.");
            }

            public override void Visit(DbCastExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbCastExpression\") is not supported.");
            }

            public override void Visit(DbCrossJoinExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbCrossJoinExpression\") is not supported.");
            }

            public override void Visit(DbDerefExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbDerefExpression\") is not supported.");
            }

            public override void Visit(DbDistinctExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbDistinctExpression\") is not supported.");
            }

            public override void Visit(DbElementExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbElementExpression\") is not supported.");
            }

            public override void Visit(DbEntityRefExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbEntityRefExpression\") is not supported.");
            }

            public override void Visit(DbExceptExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbExceptExpression\") is not supported.");
            }

            public override void Visit(DbExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbExpression\") is not supported.");
            }

            public override void Visit(DbFilterExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbFilterExpression\") is not supported.");
            }

            public override void Visit(DbFunctionExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbFunctionExpression\") is not supported.");
            }

            public override void Visit(DbGroupByExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbGroupByExpression\") is not supported.");
            }

            public override void Visit(DbIntersectExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbIntersectExpression\") is not supported.");
            }

            public override void Visit(DbIsEmptyExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbIsEmptyExpression\") is not supported.");
            }

            public override void Visit(DbIsOfExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbIsOfExpression\") is not supported.");
            }

            public override void Visit(DbJoinExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbJoinExpression\") is not supported.");
            }

            public override void Visit(DbLikeExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbLikeExpression\") is not supported.");
            }

            public override void Visit(DbLimitExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbLimitExpression\") is not supported.");
            }

            public override void Visit(DbOfTypeExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbOfTypeExpression\") is not supported.");
            }

            public override void Visit(DbParameterReferenceExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbParameterReferenceExpression\") is not supported.");
            }

            public override void Visit(DbProjectExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbProjectExpression\") is not supported.");
            }

            public override void Visit(DbQuantifierExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbQuantifierExpression\") is not supported.");
            }

            public override void Visit(DbRefExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbRefExpression\") is not supported.");
            }

            public override void Visit(DbRefKeyExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbRefKeyExpression\") is not supported.");
            }

            public override void Visit(DbRelationshipNavigationExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbRelationshipNavigationExpression\") is not supported.");
            }

            public override void Visit(DbSkipExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbSkipExpression\") is not supported.");
            }

            public override void Visit(DbSortExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbSortExpression\") is not supported.");
            }

            public override void Visit(DbTreatExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbTreatExpression\") is not supported.");
            }

            public override void Visit(DbUnionAllExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbUnionAllExpression\") is not supported.");
            }

            public override void Visit(DbVariableReferenceExpression expression)
            {
                throw new NotSupportedException("Visit(\"DbVariableReferenceExpression\") is not supported.");
            }

            #endregion UnsupportedVisitMethods

            /// <summary>
            /// Initialize a new expression translator populating the given string builder
            /// with command text. Command text builder and command tree must not be null.
            /// </summary>
            /// <param name="commandText">Command text with which to populate commands</param>
            /// <param name="commandTree">Command tree generating SQL</param>
            /// <param name="preserveMemberValues">Indicates whether the translator should preserve
            /// member values while compiling t-SQL (only needed for server generation)</param>
            internal ExpressionTranslator(StringBuilder commandText, DbModificationCommandTree commandTree, 
                bool preserveMemberValues)
            {
                Debug.Assert(null != commandText);
                Debug.Assert(null != commandTree);
                _commandText = commandText;
                _commandTree = commandTree;
                _parameters = new List<DbParameter>();
                _memberValues = preserveMemberValues ? new Dictionary<EdmMember, DbParameter>() :
                    null;
            }

            private readonly StringBuilder _commandText;
            private readonly DbModificationCommandTree _commandTree;
            private readonly List<DbParameter> _parameters;
            private readonly Dictionary<EdmMember, DbParameter> _memberValues;
            private int parameterNameCount = 0;

            internal List<DbParameter> Parameters { get { return _parameters; } }
            internal Dictionary<EdmMember, DbParameter> MemberValues { get { return _memberValues; } }

            // generate parameter (name based on parameter ordinal)
            internal SqlParameter CreateParameter(object value, TypeUsage type)
            {
                var parameter = SampleProviderServices.CreateSqlParameter(
                    string.Concat("@p", parameterNameCount.ToString(CultureInfo.InvariantCulture)),
                    type,
                    ParameterMode.In,
                    value);

                parameterNameCount++;
                _parameters.Add(parameter);

                return parameter;
            }

            public override void Visit(DbAndExpression expression)
            {
                VisitBinary(expression, " and ");
            }

            public override void Visit(DbOrExpression expression)
            {
                VisitBinary(expression, " or ");
            }

            public override void Visit(DbComparisonExpression expression)
            {
                Debug.Assert(expression.ExpressionKind == DbExpressionKind.Equals,
                    "only equals comparison expressions are produced in DML command trees in V1");

                VisitBinary(expression, " = ");

                RegisterMemberValue(expression.Left, expression.Right);
            }

            /// <summary>
            /// Call this method to register a property value pair so the translator "remembers"
            /// the values for members of the row being modified. These values can then be used
            /// to form a predicate for server-generation (based on the key of the row)
            /// </summary>
            /// <param name="propertyExpression">DbExpression containing the column reference (property expression).</param>
            /// <param name="value">DbExpression containing the value of the column.</param>
            internal void RegisterMemberValue(DbExpression propertyExpression, DbExpression value)
            {
                if (null != _memberValues)
                {
                    // register the value for this property
                    Debug.Assert(propertyExpression.ExpressionKind == DbExpressionKind.Property,
                        "DML predicates and setters must be of the form property = value");

                    // get name of left property 
                    EdmMember property = ((DbPropertyExpression)propertyExpression).Property;

                    // don't track null values
                    if (value.ExpressionKind != DbExpressionKind.Null)
                    {
                        Debug.Assert(value.ExpressionKind == DbExpressionKind.Constant,
                            "value must either constant or null");
                        // retrieve the last parameter added (which describes the parameter)
                        _memberValues[property] = _parameters[_parameters.Count - 1];
                    }
                }
            }

            public override void Visit(DbIsNullExpression expression)
            {
                expression.Argument.Accept(this);
                _commandText.Append(" is null");
            }

            public override void Visit(DbNotExpression expression)
            {
                _commandText.Append("not (");
                expression.Accept(this);
                _commandText.Append(")");
            }

            public override void Visit(DbConstantExpression expression)
            {
                SqlParameter parameter = CreateParameter(expression.Value, expression.ResultType);
                _commandText.Append(parameter.ParameterName);
            }

            public override void Visit(DbScanExpression expression)
            {
                _commandText.Append(SqlGenerator.GetTargetTSql(expression.Target));
            }

            public override void Visit(DbPropertyExpression expression)
            {
                _commandText.Append(GenerateMemberTSql(expression.Property));
            }

            public override void Visit(DbNullExpression expression)
            {
                _commandText.Append("null");
            }

            public override void Visit(DbNewInstanceExpression expression)
            {
                // assumes all arguments are self-describing (no need to use aliases
                // because no renames are ever used in the projection)
                bool first = true;
                foreach (DbExpression argument in expression.Arguments)
                {
                    if (first) { first = false; }
                    else { _commandText.Append(", "); }
                    argument.Accept(this);
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

