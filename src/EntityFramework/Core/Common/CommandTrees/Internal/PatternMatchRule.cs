namespace System.Data.Entity.Core.Common.CommandTrees.Internal
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// PatternMatchRule is a specialization of <see cref="DbExpressionRule"/> that uses a Func&lt;DbExpression, bool&gt; 'pattern'
    /// to implement <see cref="DbExpressionRule.ShouldProcess"/> and a Func&lt;DbExpression, DbExpression&gt; 'processor' to implement
    /// <see cref="DbExpressionRule.TryProcess"/>. The 'processor' should return <c>null</c> to indicate that the expression was not
    /// successfully processed, otherwise it should return the new result expression.
    /// </summary>
    internal class PatternMatchRule : DbExpressionRule
    {
        private readonly Func<DbExpression, bool> isMatch;
        private readonly Func<DbExpression, DbExpression> process;
        private readonly ProcessedAction processed;

        private PatternMatchRule(
            Func<DbExpression, bool> matchFunc, Func<DbExpression, DbExpression> processor, ProcessedAction onProcessed)
        {
            isMatch = matchFunc;
            process = processor;
            processed = onProcessed;
        }

        internal override bool ShouldProcess(DbExpression expression)
        {
            return isMatch(expression);
        }

        internal override bool TryProcess(DbExpression expression, out DbExpression result)
        {
            result = process(expression);
            return (result != null);
        }

        internal override ProcessedAction OnExpressionProcessed
        {
            get { return processed; }
        }

        /// <summary>
        /// Constructs a new PatternMatch rule with the specified pattern, processor and default <see cref="DbExpressionRule.ProcessedAction"/> of <see cref="DbExpressionRule.ProcessedAction.Reset"/>
        /// </summary>
        internal static PatternMatchRule Create(Func<DbExpression, bool> matchFunc, Func<DbExpression, DbExpression> processor)
        {
            return Create(matchFunc, processor, ProcessedAction.Reset);
        }

        /// <summary>
        /// Constructs a new PatternMatchRule with the specified pattern, processor and <see cref="DbExpressionRule.ProcessedAction"/>
        /// </summary>
        internal static PatternMatchRule Create(
            Func<DbExpression, bool> matchFunc, Func<DbExpression, DbExpression> processor, ProcessedAction onProcessed)
        {
            Contract.Requires(matchFunc != null);
            Contract.Requires(processor != null);

            return new PatternMatchRule(matchFunc, processor, onProcessed);
        }
    }
}
