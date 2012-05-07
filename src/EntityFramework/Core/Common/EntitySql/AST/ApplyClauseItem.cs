namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Represents apply expression.
    /// </summary>
    internal sealed class ApplyClauseItem : Node
    {
        private readonly FromClauseItem _applyLeft;
        private readonly FromClauseItem _applyRight;
        private readonly ApplyKind _applyKind;

        /// <summary>
        /// Initializes apply clause item.
        /// </summary>
        internal ApplyClauseItem(FromClauseItem applyLeft, FromClauseItem applyRight, ApplyKind applyKind)
        {
            _applyLeft = applyLeft;
            _applyRight = applyRight;
            _applyKind = applyKind;
        }

        /// <summary>
        /// Returns apply left expression.
        /// </summary>
        internal FromClauseItem LeftExpr
        {
            get { return _applyLeft; }
        }

        /// <summary>
        /// Returns apply right expression.
        /// </summary>
        internal FromClauseItem RightExpr
        {
            get { return _applyRight; }
        }

        /// <summary>
        /// Returns apply kind (cross,outer).
        /// </summary>
        internal ApplyKind ApplyKind
        {
            get { return _applyKind; }
        }
    }
}