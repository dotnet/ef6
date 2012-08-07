// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees.Internal
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Abstract base class for a DbExpression visitor that can apply a collection of <see cref="DbExpressionRule" />s during the visitor pass, returning the final result expression.
    ///     This class encapsulates the rule application logic that applies regardless of how the ruleset - modelled as the abstract <see
    ///      cref="GetRules" /> method - is provided.
    /// </summary>
    internal abstract class DbExpressionRuleProcessingVisitor : DefaultExpressionVisitor
    {
        protected abstract IEnumerable<DbExpressionRule> GetRules();

        private static Tuple<DbExpression, DbExpressionRule.ProcessedAction> ProcessRules(
            DbExpression expression, List<DbExpressionRule> rules)
        {
            // Considering each rule in the rule set in turn, if the rule indicates that it can process the
            // input expression, call TryProcess to attempt processing. If successful, take the action specified
            // by the rule's OnExpressionProcessed action, which may involve returning the action and the result
            // expression so that processing can be reset or halted.

            for (var idx = 0; idx < rules.Count; idx++)
            {
                var currentRule = rules[idx];
                if (currentRule.ShouldProcess(expression))
                {
                    DbExpression result;
                    if (currentRule.TryProcess(expression, out result))
                    {
                        if (currentRule.OnExpressionProcessed
                            != DbExpressionRule.ProcessedAction.Continue)
                        {
                            return Tuple.Create(result, currentRule.OnExpressionProcessed);
                        }
                        else
                        {
                            expression = result;
                        }
                    }
                }
            }
            return Tuple.Create(expression, DbExpressionRule.ProcessedAction.Continue);
        }

        private bool _stopped;

        private DbExpression ApplyRules(DbExpression expression)
        {
            // Driver loop to apply rules while the status of processing is 'Reset',
            // or correctly set the _stopped flag if status is 'Stopped'.

            var currentRules = GetRules().ToList();
            var ruleResult = ProcessRules(expression, currentRules);
            while (ruleResult.Item2
                   == DbExpressionRule.ProcessedAction.Reset)
            {
                currentRules = GetRules().ToList();
                ruleResult = ProcessRules(ruleResult.Item1, currentRules);
            }
            if (ruleResult.Item2
                == DbExpressionRule.ProcessedAction.Stop)
            {
                _stopped = true;
            }
            return ruleResult.Item1;
        }

        protected override DbExpression VisitExpression(DbExpression expression)
        {
            // Pre-process this visitor's rules
            var result = ApplyRules(expression);
            if (_stopped)
            {
                // If rule processing was stopped, the result expression must be returned immediately
                return result;
            }

            // Visit the expression to recursively apply rules to subexpressions
            result = base.VisitExpression(result);
            if (_stopped)
            {
                // If rule processing was stopped, the result expression must be returned immediately
                return result;
            }

            // Post-process the rules over the resulting expression and return the result.
            // This is done so that rules that did not match the original structure of the
            // expression have an opportunity to examine the structure of the result expression.
            result = ApplyRules(result);
            return result;
        }
    }
}
