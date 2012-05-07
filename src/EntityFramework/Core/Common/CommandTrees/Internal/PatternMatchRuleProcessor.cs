namespace System.Data.Entity.Core.Common.CommandTrees.Internal
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// PatternMatchRuleProcessor is a specialization of <see cref="DbExpressionRuleProcessingVisitor"/> that uses a collection of <see cref="PatternMatchRule"/>s 
    /// as its ruleset. The static Create methods can be used to construct a new PatternMatchRuleProcessor that applies the specified PatternMatchRules, which is
    /// returned as a Func&lt;DbExpression, DbExpression&gt; that can be invoked directly on an expression to apply the ruleset to it.
    /// </summary>
    internal class PatternMatchRuleProcessor : DbExpressionRuleProcessingVisitor
    {
        private readonly ReadOnlyCollection<PatternMatchRule> ruleSet;

        private PatternMatchRuleProcessor(ReadOnlyCollection<PatternMatchRule> rules)
        {
            Debug.Assert(rules.Count() != 0, "At least one PatternMatchRule is required");
            Debug.Assert(rules.Where(r => r == null).Count() == 0, "Individual PatternMatchRules must not be null");

            ruleSet = rules;
        }

        private DbExpression Process(DbExpression expression)
        {
            Contract.Requires(expression != null);
            expression = VisitExpression(expression);
            return expression;
        }

        protected override IEnumerable<DbExpressionRule> GetRules()
        {
            return ruleSet;
        }

        internal static Func<DbExpression, DbExpression> Create(params PatternMatchRule[] rules)
        {
            Contract.Requires(rules != null);

            return new PatternMatchRuleProcessor(new ReadOnlyCollection<PatternMatchRule>(rules)).Process;
        }
    }
}