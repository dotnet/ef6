// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     An abstract base type for types that implement the IExpressionVisitor interface to derive from.
    /// </summary>
    public abstract class BasicCommandTreeVisitor : BasicExpressionVisitor
    {
        #region protected API, may be overridden to add functionality at specific points in the traversal

        protected virtual void VisitSetClause(DbSetClause setClause)
        {
            Check.NotNull(setClause, "setClause");
            VisitExpression(setClause.Property);
            VisitExpression(setClause.Value);
        }

        protected virtual void VisitModificationClause(DbModificationClause modificationClause)
        {
            Check.NotNull(modificationClause, "modificationClause");
            // Set clause is the only current possibility
            VisitSetClause((DbSetClause)modificationClause);
        }

        protected virtual void VisitModificationClauses(IList<DbModificationClause> modificationClauses)
        {
            Check.NotNull(modificationClauses, "modificationClauses");
            for (var idx = 0; idx < modificationClauses.Count; idx++)
            {
                VisitModificationClause(modificationClauses[idx]);
            }
        }

        #endregion

        #region public convenience API

        public virtual void VisitCommandTree(DbCommandTree commandTree)
        {
            Check.NotNull(commandTree, "commandTree");
            switch (commandTree.CommandTreeKind)
            {
                case DbCommandTreeKind.Delete:
                    VisitDeleteCommandTree((DbDeleteCommandTree)commandTree);
                    break;

                case DbCommandTreeKind.Function:
                    VisitFunctionCommandTree((DbFunctionCommandTree)commandTree);
                    break;

                case DbCommandTreeKind.Insert:
                    VisitInsertCommandTree((DbInsertCommandTree)commandTree);
                    break;

                case DbCommandTreeKind.Query:
                    VisitQueryCommandTree((DbQueryCommandTree)commandTree);
                    break;

                case DbCommandTreeKind.Update:
                    VisitUpdateCommandTree((DbUpdateCommandTree)commandTree);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        #endregion

        #region CommandTree-specific Visitor Methods

        protected virtual void VisitDeleteCommandTree(DbDeleteCommandTree deleteTree)
        {
            Check.NotNull(deleteTree, "deleteTree");
            VisitExpressionBindingPre(deleteTree.Target);
            VisitExpression(deleteTree.Predicate);
            VisitExpressionBindingPost(deleteTree.Target);
        }

        protected virtual void VisitFunctionCommandTree(DbFunctionCommandTree functionTree)
        {
            Check.NotNull(functionTree, "functionTree");
        }

        protected virtual void VisitInsertCommandTree(DbInsertCommandTree insertTree)
        {
            Check.NotNull(insertTree, "insertTree");
            VisitExpressionBindingPre(insertTree.Target);
            VisitModificationClauses(insertTree.SetClauses);
            if (insertTree.Returning != null)
            {
                VisitExpression(insertTree.Returning);
            }
            VisitExpressionBindingPost(insertTree.Target);
        }

        protected virtual void VisitQueryCommandTree(DbQueryCommandTree queryTree)
        {
            Check.NotNull(queryTree, "queryTree");
            VisitExpression(queryTree.Query);
        }

        protected virtual void VisitUpdateCommandTree(DbUpdateCommandTree updateTree)
        {
            Check.NotNull(updateTree, "updateTree");
            VisitExpressionBindingPre(updateTree.Target);
            VisitModificationClauses(updateTree.SetClauses);
            VisitExpression(updateTree.Predicate);
            if (updateTree.Returning != null)
            {
                VisitExpression(updateTree.Returning);
            }
            VisitExpressionBindingPost(updateTree.Target);
        }

        #endregion
    }
}
