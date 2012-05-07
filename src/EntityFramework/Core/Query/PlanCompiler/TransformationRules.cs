//using System.Diagnostics; // Please use PlanCompiler.Assert instead of Debug.Assert in this class...
// It is fine to use Debug.Assert in cases where you assert an obvious thing that is supposed
// to prevent from simple mistakes during development (e.g. method argument validation 
// in cases where it was you who created the variables or the variables had already been validated or 
// in "else" clauses where due to code changes (e.g. adding a new value to an enum type) the default 
// "else" block is chosen why the new condition should be treated separately). This kind of asserts are 
// (can be) helpful when developing new code to avoid simple mistakes but have no or little value in 
// the shipped product. 
// PlanCompiler.Assert *MUST* be used to verify conditions in the trees. These would be assumptions 
// about how the tree was built etc. - in these cases we probably want to throw an exception (this is
// what PlanCompiler.Assert does when the condition is not met) if either the assumption is not correct 
// or the tree was built/rewritten not the way we thought it was.
// Use your judgment - if you rather remove an assert than ship it use Debug.Assert otherwise use
// PlanCompiler.Assert.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Query.InternalTrees;

    /// <summary>
    /// The list of all transformation rules to apply
    /// </summary>
    internal static class TransformationRules
    {
        /// <summary>
        /// A lookup table for built from all rules
        /// The lookup table is an array indexed by OpType and each entry has a list of rules.
        /// </summary>
        internal static readonly ReadOnlyCollection<ReadOnlyCollection<Rule>> AllRulesTable = BuildLookupTableForRules(AllRules);

        /// <summary>
        /// A lookup table for built only from ProjectRules
        /// The lookup table is an array indexed by OpType and each entry has a list of rules.
        /// </summary>
        internal static readonly ReadOnlyCollection<ReadOnlyCollection<Rule>> ProjectRulesTable =
            BuildLookupTableForRules(ProjectOpRules.Rules);

        /// <summary>
        /// A lookup table built only from rules that use key info
        /// The lookup table is an array indexed by OpType and each entry has a list of rules.
        /// </summary>
        internal static readonly ReadOnlyCollection<ReadOnlyCollection<Rule>> PostJoinEliminationRulesTable =
            BuildLookupTableForRules(PostJoinEliminationRules);

        /// <summary>
        /// A lookup table built only from rules that rely on nullability of vars and other rules 
        /// that may be able to perform simplificatios if these have been applied.
        /// The lookup table is an array indexed by OpType and each entry has a list of rules.
        /// </summary>
        internal static readonly ReadOnlyCollection<ReadOnlyCollection<Rule>> NullabilityRulesTable =
            BuildLookupTableForRules(NullabilityRules);

        /// <summary>
        /// A look-up table of rules that may cause modifications such that projection pruning may be useful
        /// after they have been applied.
        /// </summary>
        internal static readonly HashSet<Rule> RulesRequiringProjectionPruning = InitializeRulesRequiringProjectionPruning();

        /// <summary>
        /// A look-up table of rules that may cause modifications such that reapplying the nullability rules
        /// may be useful after they have been applied.
        /// </summary>
        internal static readonly HashSet<Rule> RulesRequiringNullabilityRulesToBeReapplied =
            InitializeRulesRequiringNullabilityRulesToBeReapplied();

        #region private state maintenance

        private static List<Rule> allRules;

        private static List<Rule> AllRules
        {
            get
            {
                if (allRules == null)
                {
                    allRules = new List<Rule>();
                    allRules.AddRange(ScalarOpRules.Rules);
                    allRules.AddRange(FilterOpRules.Rules);
                    allRules.AddRange(ProjectOpRules.Rules);
                    allRules.AddRange(ApplyOpRules.Rules);
                    allRules.AddRange(JoinOpRules.Rules);
                    allRules.AddRange(SingleRowOpRules.Rules);
                    allRules.AddRange(SetOpRules.Rules);
                    allRules.AddRange(GroupByOpRules.Rules);
                    allRules.AddRange(SortOpRules.Rules);
                    allRules.AddRange(ConstrainedSortOpRules.Rules);
                    allRules.AddRange(DistinctOpRules.Rules);
                }
                return allRules;
            }
        }

        private static List<Rule> postJoinEliminationRules;

        private static List<Rule> PostJoinEliminationRules
        {
            get
            {
                if (postJoinEliminationRules == null)
                {
                    postJoinEliminationRules = new List<Rule>();
                    postJoinEliminationRules.AddRange(ProjectOpRules.Rules);
                        //these don't use key info per-se, but can help after the distinct op rules.
                    postJoinEliminationRules.AddRange(DistinctOpRules.Rules);
                    postJoinEliminationRules.AddRange(FilterOpRules.Rules);
                    postJoinEliminationRules.AddRange(JoinOpRules.Rules);
                    postJoinEliminationRules.AddRange(NullabilityRules);
                }
                return postJoinEliminationRules;
            }
        }

        private static List<Rule> nullabilityRules;

        private static List<Rule> NullabilityRules
        {
            get
            {
                if (nullabilityRules == null)
                {
                    nullabilityRules = new List<Rule>();
                    nullabilityRules.Add(ScalarOpRules.Rule_IsNullOverVarRef);
                    nullabilityRules.Add(ScalarOpRules.Rule_AndOverConstantPred1);
                    nullabilityRules.Add(ScalarOpRules.Rule_AndOverConstantPred2);
                    nullabilityRules.Add(ScalarOpRules.Rule_SimplifyCase);
                    nullabilityRules.Add(ScalarOpRules.Rule_NotOverConstantPred);
                }
                return nullabilityRules;
            }
        }

        private static ReadOnlyCollection<ReadOnlyCollection<Rule>> BuildLookupTableForRules(IEnumerable<Rule> rules)
        {
            var NoRules = new ReadOnlyCollection<Rule>(new Rule[0]);

            var lookupTable = new List<Rule>[(int)OpType.MaxMarker];

            foreach (var rule in rules)
            {
                var opRules = lookupTable[(int)rule.RuleOpType];
                if (opRules == null)
                {
                    opRules = new List<Rule>();
                    lookupTable[(int)rule.RuleOpType] = opRules;
                }
                opRules.Add(rule);
            }

            var rulesPerType = new ReadOnlyCollection<Rule>[lookupTable.Length];
            for (var i = 0; i < lookupTable.Length; ++i)
            {
                if (null != lookupTable[i])
                {
                    rulesPerType[i] = new ReadOnlyCollection<Rule>(lookupTable[i].ToArray());
                }
                else
                {
                    rulesPerType[i] = NoRules;
                }
            }
            return new ReadOnlyCollection<ReadOnlyCollection<Rule>>(rulesPerType);
        }

        private static HashSet<Rule> InitializeRulesRequiringProjectionPruning()
        {
            var rulesRequiringProjectionPruning = new HashSet<Rule>();

            rulesRequiringProjectionPruning.Add(ApplyOpRules.Rule_OuterApplyOverProject);

            rulesRequiringProjectionPruning.Add(JoinOpRules.Rule_CrossJoinOverProject1);
            rulesRequiringProjectionPruning.Add(JoinOpRules.Rule_CrossJoinOverProject2);
            rulesRequiringProjectionPruning.Add(JoinOpRules.Rule_InnerJoinOverProject1);
            rulesRequiringProjectionPruning.Add(JoinOpRules.Rule_InnerJoinOverProject2);
            rulesRequiringProjectionPruning.Add(JoinOpRules.Rule_OuterJoinOverProject2);

            rulesRequiringProjectionPruning.Add(ProjectOpRules.Rule_ProjectWithNoLocalDefs);

            rulesRequiringProjectionPruning.Add(FilterOpRules.Rule_FilterOverProject);
            rulesRequiringProjectionPruning.Add(FilterOpRules.Rule_FilterWithConstantPredicate);

            rulesRequiringProjectionPruning.Add(GroupByOpRules.Rule_GroupByOverProject);
            rulesRequiringProjectionPruning.Add(GroupByOpRules.Rule_GroupByOpWithSimpleVarRedefinitions);

            return rulesRequiringProjectionPruning;
        }

        private static HashSet<Rule> InitializeRulesRequiringNullabilityRulesToBeReapplied()
        {
            var rulesRequiringNullabilityRulesToBeReapplied = new HashSet<Rule>();

            rulesRequiringNullabilityRulesToBeReapplied.Add(FilterOpRules.Rule_FilterOverLeftOuterJoin);

            return rulesRequiringNullabilityRulesToBeReapplied;
        }

        #endregion

        /// <summary>
        /// Apply the rules that belong to the specified group to the given query tree.
        /// </summary>
        /// <param name="compilerState"></param>
        /// <param name="rulesGroup"></param>
        internal static bool Process(PlanCompiler compilerState, TransformationRulesGroup rulesGroup)
        {
            ReadOnlyCollection<ReadOnlyCollection<Rule>> rulesTable = null;
            switch (rulesGroup)
            {
                case TransformationRulesGroup.All:
                    rulesTable = AllRulesTable;
                    break;
                case TransformationRulesGroup.PostJoinElimination:
                    rulesTable = PostJoinEliminationRulesTable;
                    break;
                case TransformationRulesGroup.Project:
                    rulesTable = ProjectRulesTable;
                    break;
            }

            // If any rule has been applied after which reapplying nullability rules may be useful,
            // reapply nullability rules.
            bool projectionPrunningRequired;
            if (Process(compilerState, rulesTable, out projectionPrunningRequired))
            {
                bool projectionPrunningRequired2;
                Process(compilerState, NullabilityRulesTable, out projectionPrunningRequired2);
                projectionPrunningRequired = projectionPrunningRequired || projectionPrunningRequired2;
            }
            return projectionPrunningRequired;
        }

        /// <summary>
        /// Apply the rules that belong to the specified rules table to the given query tree.
        /// </summary>
        /// <param name="compilerState"></param>
        /// <param name="rulesTable"></param>
        /// <param name="projectionPruningRequired">is projection pruning  required after the rule application</param>
        /// <returns>Whether any rule has been applied after which reapplying nullability rules may be useful</returns>
        private static bool Process(
            PlanCompiler compilerState, ReadOnlyCollection<ReadOnlyCollection<Rule>> rulesTable, out bool projectionPruningRequired)
        {
            var ruleProcessor = new RuleProcessor();
            var context = new TransformationRulesContext(compilerState);
            compilerState.Command.Root = ruleProcessor.ApplyRulesToSubtree(context, rulesTable, compilerState.Command.Root);
            projectionPruningRequired = context.ProjectionPrunningRequired;
            return context.ReapplyNullabilityRules;
        }
    }
}
