namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;

    /// <summary>
    /// An abstract base type for types that implement the IExpressionVisitor interface to derive from.
    /// </summary>
    /*CQT_PUBLIC_API(*/
    internal /*)*/ abstract class BasicCommandTreeVisitor : BasicExpressionVisitor
    {
        #region protected API, may be overridden to add functionality at specific points in the traversal

        protected virtual void VisitSetClause(DbSetClause setClause)
        {
            EntityUtil.CheckArgumentNull(setClause, "setClause");
            VisitExpression(setClause.Property);
            VisitExpression(setClause.Value);
        }

        protected virtual void VisitModificationClause(DbModificationClause modificationClause)
        {
            EntityUtil.CheckArgumentNull(modificationClause, "modificationClause");
            // Set clause is the only current possibility
            VisitSetClause((DbSetClause)modificationClause);
        }

        protected virtual void VisitModificationClauses(IList<DbModificationClause> modificationClauses)
        {
            EntityUtil.CheckArgumentNull(modificationClauses, "modificationClauses");
            for (var idx = 0; idx < modificationClauses.Count; idx++)
            {
                VisitModificationClause(modificationClauses[idx]);
            }
        }

        #endregion

        #region public convenience API

        public virtual void VisitCommandTree(DbCommandTree commandTree)
        {
            EntityUtil.CheckArgumentNull(commandTree, "commandTree");
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
                    throw EntityUtil.NotSupported();
            }
        }

        #endregion

        #region CommandTree-specific Visitor Methods

        protected virtual void VisitDeleteCommandTree(DbDeleteCommandTree deleteTree)
        {
            EntityUtil.CheckArgumentNull(deleteTree, "deleteTree");
            VisitExpressionBindingPre(deleteTree.Target);
            VisitExpression(deleteTree.Predicate);
            VisitExpressionBindingPost(deleteTree.Target);
        }

        protected virtual void VisitFunctionCommandTree(DbFunctionCommandTree functionTree)
        {
            EntityUtil.CheckArgumentNull(functionTree, "functionTree");
        }

        protected virtual void VisitInsertCommandTree(DbInsertCommandTree insertTree)
        {
            EntityUtil.CheckArgumentNull(insertTree, "insertTree");
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
            EntityUtil.CheckArgumentNull(queryTree, "queryTree");
            VisitExpression(queryTree.Query);
        }

        protected virtual void VisitUpdateCommandTree(DbUpdateCommandTree updateTree)
        {
            EntityUtil.CheckArgumentNull(updateTree, "updateTree");
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
