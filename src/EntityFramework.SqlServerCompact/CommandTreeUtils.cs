// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact.SqlGen
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;

    internal static class CommandTreeUtils
    {
        #region Expression Flattening Helpers

        private static readonly Queue<DbExpressionKind> _associativeExpressionKinds = new Queue<DbExpressionKind>(
            new[]
                {
                    DbExpressionKind.Or,
                    DbExpressionKind.And,
                    DbExpressionKind.Plus,
                    DbExpressionKind.Multiply
                });

        // <summary>
        // Creates a flat list of the associative arguments.
        // For example, for ((A1 + (A2 - A3)) + A4) it will create A1, (A2 - A3), A4
        // </summary>
        internal static IEnumerable<DbExpression> FlattenAssociativeExpression(DbExpression expression)
        {
            return FlattenAssociativeExpression(expression.ExpressionKind, expression);
        }

        // <summary>
        // Creates a flat list of the associative arguments.
        // For example, for ((A1 + (A2 - A3)) + A4) it will create A1, (A2 - A3), A4
        // Only 'unfolds' the given arguments that are of the given expression kind.
        // </summary>
        internal static IEnumerable<DbExpression> FlattenAssociativeExpression(
            DbExpressionKind expressionKind, params DbExpression[] arguments)
        {
            if (!_associativeExpressionKinds.Contains(expressionKind))
            {
                return arguments;
            }

            var outputArguments = new List<DbExpression>();
            foreach (var argument in arguments)
            {
                ExtractAssociativeArguments(expressionKind, outputArguments, argument);
            }
            return outputArguments;
        }

        // <summary>
        // Helper method for FlattenAssociativeExpression.
        // Creates a flat list of the associative arguments and appends to the given argument list.
        // For example, for ((A1 + (A2 - A3)) + A4) it will add A1, (A2 - A3), A4 to the list.
        // Only 'unfolds' the given expression if it is of the given expression kind.
        // </summary>
        private static void ExtractAssociativeArguments(
            DbExpressionKind expressionKind, List<DbExpression> argumentList, DbExpression expression)
        {
            if (expression.ExpressionKind != expressionKind)
            {
                argumentList.Add(expression);
                return;
            }

            //All associative expressions are binary, thus we must be dealing with a DbBinaryExpresson or 
            // a DbArithmeticExpression with 2 arguments.
            var binaryExpression = expression as DbBinaryExpression;
            if (binaryExpression != null)
            {
                ExtractAssociativeArguments(expressionKind, argumentList, binaryExpression.Left);
                ExtractAssociativeArguments(expressionKind, argumentList, binaryExpression.Right);
                return;
            }

            var arithExpression = (DbArithmeticExpression)expression;
            ExtractAssociativeArguments(expressionKind, argumentList, arithExpression.Arguments[0]);
            ExtractAssociativeArguments(expressionKind, argumentList, arithExpression.Arguments[1]);
        }

        #endregion
    }
}
