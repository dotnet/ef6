// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;

    // <summary>
    // The list of all transformation rules to apply
    // </summary>
    internal static class TransformationRules
    {
        // <summary>
        // A lookup table for built from all rules
        // The lookup table is an array indexed by OpType and each entry has a list of rules.
        // </summary>
        internal static readonly ReadOnlyCollection<ReadOnlyCollection<Rule>> AllRulesTable = BuildLookupTableForRules(AllRules);

        // <summary>
        // A lookup table for built only from ProjectRules
        // The lookup table is an array indexed by OpType and each entry has a list of rules.
        // </summary>
        internal static readonly ReadOnlyCollection<ReadOnlyCollection<Rule>> ProjectRulesTable =
            BuildLookupTableForRules(ProjectOpRules.Rules);

        // <summary>
        // A lookup table built only from rules that use key info
        // The lookup table is an array indexed by OpType and each entry has a list of rules.
        // </summary>
        internal static readonly ReadOnlyCollection<ReadOnlyCollection<Rule>> PostJoinEliminationRulesTable =
            BuildLookupTableForRules(PostJoinEliminationRules);

        // <summary>
        // A lookup table built only from rules that rely on nullability of vars and other rules
        // that may be able to perform simplifications if these have been applied.
        // The lookup table is an array indexed by OpType and each entry has a list of rules.
        // </summary>
        internal static readonly ReadOnlyCollection<ReadOnlyCollection<Rule>> NullabilityRulesTable =
            BuildLookupTableForRules(NullabilityRules);

        // <summary>
        // A look-up table of rules that may cause modifications such that projection pruning may be useful
        // after they have been applied.
        // </summary>
        internal static readonly HashSet<Rule> RulesRequiringProjectionPruning = InitializeRulesRequiringProjectionPruning();

        // <summary>
        // A look-up table of rules that may cause modifications such that reapplying the nullability rules
        // may be useful after they have been applied.
        // </summary>
        internal static readonly HashSet<Rule> RulesRequiringNullabilityRulesToBeReapplied =
            InitializeRulesRequiringNullabilityRulesToBeReapplied();

        internal static readonly ReadOnlyCollection<ReadOnlyCollection<Rule>> NullSemanticsRulesTable =
            BuildLookupTableForRules(NullSemanticsRules);

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
                    postJoinEliminationRules.AddRange(ApplyOpRules.Rules);
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

        private static List<Rule> nullSemanticsRules;

        private static List<Rule> NullSemanticsRules
        {
            get
            {
                if (nullSemanticsRules == null)
                {
                    nullSemanticsRules = new List<Rule>();
                    nullSemanticsRules.Add(ScalarOpRules.Rule_IsNullOverAnything);
                    nullSemanticsRules.Add(ScalarOpRules.Rule_NullCast);
                    nullSemanticsRules.Add(ScalarOpRules.Rule_EqualsOverConstant);
                    nullSemanticsRules.Add(ScalarOpRules.Rule_AndOverConstantPred1);
                    nullSemanticsRules.Add(ScalarOpRules.Rule_AndOverConstantPred2);
                    nullSemanticsRules.Add(ScalarOpRules.Rule_OrOverConstantPred1);
                    nullSemanticsRules.Add(ScalarOpRules.Rule_OrOverConstantPred2);
                    nullSemanticsRules.Add(ScalarOpRules.Rule_NotOverConstantPred);
                    nullSemanticsRules.Add(ScalarOpRules.Rule_LikeOverConstants);
                    nullSemanticsRules.Add(ScalarOpRules.Rule_SimplifyCase);
                    nullSemanticsRules.Add(ScalarOpRules.Rule_FlattenCase);
                }
                return nullSemanticsRules;
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

        // <summary>
        // Apply the rules that belong to the specified group to the given query tree.
        // </summary>
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
                case TransformationRulesGroup.NullSemantics:
                    rulesTable = NullSemanticsRulesTable;
                    break;
            }

            // If any rule has been applied after which reapplying nullability rules may be useful,
            // reapply nullability rules.
            bool projectionPruningRequired;
            if (Process(compilerState, rulesTable, out projectionPruningRequired))
            {
                bool projectionPruningRequired2;
                Process(compilerState, NullabilityRulesTable, out projectionPruningRequired2);
                projectionPruningRequired = projectionPruningRequired || projectionPruningRequired2;
            }
            return projectionPruningRequired;
        }

        // <summary>
        // Apply the rules that belong to the specified rules table to the given query tree.
        // </summary>
        // <param name="projectionPruningRequired"> is projection pruning required after the rule application </param>
        // <returns> Whether any rule has been applied after which reapplying nullability rules may be useful </returns>
        private static bool Process(
            PlanCompiler compilerState, ReadOnlyCollection<ReadOnlyCollection<Rule>> rulesTable, out bool projectionPruningRequired)
        {
            var ruleProcessor = new RuleProcessor();
            var context = new TransformationRulesContext(compilerState);
            compilerState.Command.Root = ruleProcessor.ApplyRulesToSubtree(context, rulesTable, compilerState.Command.Root);
            projectionPruningRequired = context.ProjectionPruningRequired;
            return context.ReapplyNullabilityRules;
        }
    }
}
