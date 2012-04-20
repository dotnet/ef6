namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// An abstract base type for types that implement the IExpressionVisitor interface to derive from.
    /// </summary>
    public abstract class BasicCommandTreeVisitor : BasicExpressionVisitor
    {
        #region protected API, may be overridden to add functionality at specific points in the traversal

        protected virtual void VisitSetClause(DbSetClause setClause)
        {
            Contract.Requires(setClause != null);
            VisitExpression(setClause.Property);
            VisitExpression(setClause.Value);
        }

        protected virtual void VisitModificationClause(DbModificationClause modificationClause)
        {
            Contract.Requires(modificationClause != null);
            // Set clause is the only current possibility
            VisitSetClause((DbSetClause)modificationClause);
        }

        protected virtual void VisitModificationClauses(IList<DbModificationClause> modificationClauses)
        {
            Contract.Requires(modificationClauses != null);
            for (var idx = 0; idx < modificationClauses.Count; idx++)
            {
                VisitModificationClause(modificationClauses[idx]);
            }
        }

        #endregion

        #region public convenience API

        public virtual void VisitCommandTree(DbCommandTree commandTree)
        {
            Contract.Requires(commandTree != null);
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
            Contract.Requires(deleteTree != null);
            VisitExpressionBindingPre(deleteTree.Target);
            VisitExpression(deleteTree.Predicate);
            VisitExpressionBindingPost(deleteTree.Target);
        }

        protected virtual void VisitFunctionCommandTree(DbFunctionCommandTree functionTree)
        {
            Contract.Requires(functionTree != null);
        }

        protected virtual void VisitInsertCommandTree(DbInsertCommandTree insertTree)
        {
            Contract.Requires(insertTree != null);
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
            Contract.Requires(queryTree != null);
            VisitExpression(queryTree.Query);
        }

        protected virtual void VisitUpdateCommandTree(DbUpdateCommandTree updateTree)
        {
            Contract.Requires(updateTree != null);
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
