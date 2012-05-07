namespace System.Data.Entity.Core.Common.CommandTrees.Internal
{
    /// <summary>
    /// Enacapsulates the logic that defines an expression 'rule' which is capable of transforming a candidate <see cref="DbExpression"/>
    /// into a result DbExpression, and indicating what action should be taken on that result expression by the rule application logic.
    /// </summary>
    internal abstract class DbExpressionRule
    {
        /// <summary>
        /// Indicates what action the rule processor should take if the rule successfully processes an expression.
        /// </summary>
        internal enum ProcessedAction
        {
            /// <summary>
            /// Continue to apply rules, from the rule immediately following this rule, to the result expression
            /// </summary>
            Continue = 0,

            /// <summary>
            /// Going back to the first rule, apply all rules to the result expression
            /// </summary>
            Reset,

            /// <summary>
            /// Stop all rule processing and return the result expression as the final result expression
            /// </summary>
            Stop
        }

        /// <summary>
        /// Indicates whether <see cref="TryProcess"/> should be called on the specified argument expression.
        /// </summary>
        /// <param name="expression">The <see cref="DbExpression"/> that the rule should inspect and determine if processing is possible</param>
        /// <returns><c>true</c> if the rule can attempt processing of the expression via the <see cref="TryProcess"/> method; otherwise <c>false</c></returns>
        internal abstract bool ShouldProcess(DbExpression expression);

        /// <summary>
        /// Attempts to process the input <paramref name="expression"/> to produce a <paramref name="result"/> <see cref="DbExpression"/>.
        /// </summary>
        /// <param name="expression">The input expression that the rule should process</param>
        /// <param name="result">The result expression produced by the rule if processing was successful</param>
        /// <returns><c>true</c> if the rule was able to successfully process the input expression and produce a result expression; otherwise <c>false</c></returns>
        internal abstract bool TryProcess(DbExpression expression, out DbExpression result);

        /// <summary>
        /// Indicates what action - as a <see cref="ProcessedAction"/> value - the rule processor should take if <see cref="TryProcess"/> returns true.
        /// </summary>
        internal abstract ProcessedAction OnExpressionProcessed { get; }
    }
}