// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using cqt = System.Data.Entity.Core.Common.CommandTrees;
using md = System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The PlanCompiler class is used by the BridgeCommand to produce an 
    /// execution plan - this execution plan is the plan object. The plan compilation
    /// process takes as input a command tree (in C space), and then runs through a 
    /// set of changes before the final plan is produced. The final plan contains 
    /// one or more command trees (commands?) (in S space), with a set of assembly
    /// instructions.
    /// The compiler phases include
    /// * Convert the command tree (CTree) into an internal tree (an ITree)
    /// * Run initializations on the ITree. 
    /// * Eliminate structured types from the tree
    ///    * Eliminating named type references, refs and records from the tree
    ///    At the end of this phase, we still may have collections (and record 
    ///    arguments to collections) in the tree.
    /// * Projection pruning (ie) eliminating unused references
    /// * Tree transformations. Various transformations are run on the ITree to
    ///      (ostensibly) optimize the tree. These transformations are represented as
    ///      rules, and a rule processor is invoked.
    /// * Nest elimination. At this point, we try to get pull up nest operations
    ///      as high up the tree as possible
    /// * Code Generation. This phase produces a plan object with various subpieces 
    ///      of the ITree represented as commands (in S space).
    ///    * The subtrees of the ITree are then converted into the corresponding CTrees
    ///      and converted into S space as part of the CTree creation.
    ///    * A plan object is created and returned.
    /// </summary>
    internal class PlanCompiler
    {
        #region private state

        /// <summary>
        /// A boolean switch indicating whether we should apply transformation rules regardless of the size of the Iqt.
        /// By default, the Enabled property of a boolean switch is set using the value specified in the configuration file. 
        /// Configuring the switch with a value of 0 sets the Enabled property to false; configuring the switch with a nonzero 
        /// value to set the Enabled property to true. If the BooleanSwitch constructor cannot find initial switch settings 
        /// in the configuration file, the Enabled property of the new switch is set to false by default.
        /// </summary>
        private static readonly BooleanSwitch _applyTransformationsRegardlessOfSize =
            new BooleanSwitch(
                "System.Data.Entity.Core.EntityClient.IgnoreOptimizationLimit",
                "The Entity Framework should try to optimize the query regardless of its size");

        /// <summary>
        /// Determines the maximum size of the query in terms of Iqt nodes for which we attempt to do transformation rules.
        /// This number is ignored if applyTransformationsRegardlessOfSize is enabled.
        /// </summary>
        private const int MaxNodeCountForTransformations = 100000;

        /// <summary>
        /// The CTree we're compiling a plan for.
        /// </summary>
        private readonly cqt.DbCommandTree m_ctree;

        /// <summary>
        /// The ITree we're working on.
        /// </summary>
        private Command m_command;

        /// <summary>
        /// The phase of the process we're currently in.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private PlanCompilerPhase m_phase;

        /// <summary>
        /// Set of phases we need to go through
        /// </summary>
        private int m_neededPhases;

        /// <summary>
        /// Keeps track of foreign key relationships. Needed by Join Elimination
        /// </summary>
        private ConstraintManager m_constraintManager;

        /// <summary>
        /// Can transformation rules be applied
        /// </summary>
        private bool? m_mayApplyTransformationRules;

        #endregion

        #region constructors

        /// <summary>
        /// private constructor
        /// </summary>
        /// <param name="ctree">the input cqt</param>
        private PlanCompiler(cqt.DbCommandTree ctree)
        {
            m_ctree = ctree; // the input command tree
        }

        #endregion

        #region public interfaces

        /// <summary>
        /// Retail Assertion code.
        /// 
        /// Provides the ability to have retail asserts.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        internal static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                Debug.Fail(message);

                // NOTE: I considered, at great length, whether to have the assertion message text 
                //       included in the exception we throw; in the end, there really isn't a reliable
                //       equivalent to the C++ __LINE__ and __FILE__ macros in C# (at least not without
                //       using the C++ PreProcessor...ick)  The StackTrace object comes close but 
                //       doesn't handle inlined callers properly for our needs (MethodA() calls MethodB() 
                //       calls us, but MethodB() is inlined, so we'll get MethodA() info instead), and
                //       since these are retail "Asserts" (as in: we're not supposed to get them in our 
                //       shipping code, and we're doing this to avoid a null-ref which is even worse) I
                //       elected to simplify this by just including them as the additional info.
                throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.AssertionFailed, 0, message);
            }
        }

        /// <summary>
        /// Compile a query, and produce a plan
        /// </summary>
        /// <param name="ctree">the input CQT</param>
        /// <param name="providerCommands">list of provider commands</param>
        /// <param name="resultColumnMap">column map for result assembly</param>
        /// <param name="entitySets">the entity sets referenced in this query</param>
        /// <returns>a compiled plan object</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal static void Compile(
            cqt.DbCommandTree ctree, out List<ProviderCommandInfo> providerCommands, out ColumnMap resultColumnMap, out int columnCount,
            out Set<md.EntitySet> entitySets)
        {
            Assert(ctree != null, "Expected a valid, non-null Command Tree input");
            var pc = new PlanCompiler(ctree);
            pc.Compile(out providerCommands, out resultColumnMap, out columnCount, out entitySets);
        }

        /// <summary>
        /// Get the current command
        /// </summary>
        internal Command Command
        {
            get { return m_command; }
        }

        /// <summary>
        /// Does the command include any sort key that represents a null sentinel
        /// This may only be set to true in NominalTypeElimination and is used
        /// in Transformation Rules
        /// </summary>
        internal bool HasSortingOnNullSentinels { get; set; }

        /// <summary>
        /// Keeps track of foreign key relationships. Needed by  Join Elimination
        /// </summary>
        internal ConstraintManager ConstraintManager
        {
            get
            {
                if (m_constraintManager == null)
                {
                    m_constraintManager = new ConstraintManager();
                }
                return m_constraintManager;
            }
        }

#if DEBUG
    /// <summary>
    /// Get the current plan compiler phase
    /// </summary>
        internal PlanCompilerPhase Phase
        {
            get { return m_phase; }
        }

        /// <summary>
        /// Sets the current plan compiler trace function to <paramref name="traceCallback"/>, enabling plan compiler tracing
        /// </summary>
        internal static void TraceOn(Action<string, object> traceCallback)
        {
            s_traceCallback = traceCallback;
        }

        /// <summary>
        /// Sets the current plan compiler trace function to <c>null</c>, disabling plan compiler tracing
        /// </summary>
        internal static void TraceOff()
        {
            s_traceCallback = null;
        }

        private static Action<string, object> s_traceCallback;
#endif

        /// <summary>
        /// The MetadataWorkspace
        /// </summary>
        internal md.MetadataWorkspace MetadataWorkspace
        {
            get { return m_ctree.MetadataWorkspace; }
        }

        /// <summary>
        /// Is the specified phase needed for this query?
        /// </summary>
        /// <param name="phase">the phase in question</param>
        /// <returns></returns>
        internal bool IsPhaseNeeded(PlanCompilerPhase phase)
        {
            return ((m_neededPhases & (1 << (int)phase)) != 0);
        }

        /// <summary>
        /// Mark the specified phase as needed
        /// </summary>
        /// <param name="phase">plan compiler phase</param>
        internal void MarkPhaseAsNeeded(PlanCompilerPhase phase)
        {
            m_neededPhases = m_neededPhases | (1 << (int)phase);
        }

        #endregion

        #region private methods

        /// <summary>
        /// The real driver. 
        /// </summary>
        /// <param name="providerCommands">list of provider commands</param>
        /// <param name="resultColumnMap">column map for the result</param>
        /// <param name="entitySets">the entity sets exposed in this query</param>
        private void Compile(
            out List<ProviderCommandInfo> providerCommands, out ColumnMap resultColumnMap, out int columnCount,
            out Set<md.EntitySet> entitySets)
        {
            Initialize(); // initialize the ITree

            var beforePreProcessor = String.Empty;
            var beforeAggregatePushdown = String.Empty;
            var beforeNormalization = String.Empty;
            var beforeNTE = String.Empty;
            var beforeProjectionPruning1 = String.Empty;
            var beforeNestPullup = String.Empty;
            var beforeProjectionPruning2 = String.Empty;
            var beforeTransformationRules1 = String.Empty;
            var beforeProjectionPruning3 = String.Empty;
            var beforeTransformationRules2 = String.Empty;
            var beforeJoinElimination1 = String.Empty;
            var beforeTransformationRules3 = String.Empty;
            var beforeJoinElimination2 = String.Empty;
            var beforeTransformationRules4 = String.Empty;
            var beforeCodeGen = String.Empty;

            //
            // We always need the pre-processor and the codegen phases.
            // It is generally a good thing to run through the transformation rules, and 
            // the projection pruning phases.
            // The "optional" phases are AggregatePushdown, Normalization, NTE, NestPullup and JoinElimination
            //
            m_neededPhases = (1 << (int)PlanCompilerPhase.PreProcessor) |
                             // (1 << (int)PlanCompilerPhase.AggregatePushdown) |
                             // (1 << (int)PlanCompilerPhase.Normalization) |
                             // (1 << (int)PlanCompilerPhase.NTE) |
                             (1 << (int)PlanCompilerPhase.ProjectionPruning) |
                             // (1 << (int)PlanCompilerPhase.NestPullup) |
                             (1 << (int)PlanCompilerPhase.Transformations) |
                             // (1 << (int)PlanCompilerPhase.JoinElimination) |
                             (1 << (int)PlanCompilerPhase.CodeGen);

            // Perform any necessary preprocessing
            StructuredTypeInfo typeInfo;
            Dictionary<md.EdmFunction, md.EdmProperty[]> tvfResultKeys;
            beforePreProcessor = SwitchToPhase(PlanCompilerPhase.PreProcessor);
            PreProcessor.Process(this, out typeInfo, out tvfResultKeys);
            entitySets = typeInfo.GetEntitySets();

            if (IsPhaseNeeded(PlanCompilerPhase.AggregatePushdown))
            {
                beforeAggregatePushdown = SwitchToPhase(PlanCompilerPhase.AggregatePushdown);
                AggregatePushdown.Process(this);
            }

            if (IsPhaseNeeded(PlanCompilerPhase.Normalization))
            {
                beforeNormalization = SwitchToPhase(PlanCompilerPhase.Normalization);
                Normalizer.Process(this);
            }

            // Eliminate "structured" types.
            if (IsPhaseNeeded(PlanCompilerPhase.NTE))
            {
                beforeNTE = SwitchToPhase(PlanCompilerPhase.NTE);
                NominalTypeEliminator.Process(this, typeInfo, tvfResultKeys);
            }

            // Projection pruning - eliminate unreferenced expressions
            if (IsPhaseNeeded(PlanCompilerPhase.ProjectionPruning))
            {
                beforeProjectionPruning1 = SwitchToPhase(PlanCompilerPhase.ProjectionPruning);
                ProjectionPruner.Process(this);
            }

            // Nest Pull-up on the ITree
            if (IsPhaseNeeded(PlanCompilerPhase.NestPullup))
            {
                beforeNestPullup = SwitchToPhase(PlanCompilerPhase.NestPullup);

                NestPullup.Process(this);

                //If we do Nest Pull-up, we should again do projection pruning
                beforeProjectionPruning2 = SwitchToPhase(PlanCompilerPhase.ProjectionPruning);
                ProjectionPruner.Process(this);
            }

            // Run transformations on the tree
            if (IsPhaseNeeded(PlanCompilerPhase.Transformations))
            {
                var projectionPrunningNeeded = ApplyTransformations(ref beforeTransformationRules1, TransformationRulesGroup.All);

                if (projectionPrunningNeeded)
                {
                    beforeProjectionPruning3 = SwitchToPhase(PlanCompilerPhase.ProjectionPruning);
                    ProjectionPruner.Process(this);
                    ApplyTransformations(ref beforeTransformationRules2, TransformationRulesGroup.Project);
                }
            }

            // Join elimination
            if (IsPhaseNeeded(PlanCompilerPhase.JoinElimination))
            {
                beforeJoinElimination1 = SwitchToPhase(PlanCompilerPhase.JoinElimination);
                var modified = JoinElimination.Process(this);
                if (modified)
                {
                    ApplyTransformations(ref beforeTransformationRules3, TransformationRulesGroup.PostJoinElimination);
                    beforeJoinElimination2 = SwitchToPhase(PlanCompilerPhase.JoinElimination);
                    modified = JoinElimination.Process(this);
                    if (modified)
                    {
                        ApplyTransformations(ref beforeTransformationRules4, TransformationRulesGroup.PostJoinElimination);
                    }
                }
            }

            // Code generation
            beforeCodeGen = SwitchToPhase(PlanCompilerPhase.CodeGen);
            CodeGen.Process(this, out providerCommands, out resultColumnMap, out columnCount);

#if DEBUG
    // GC.KeepAlive makes FxCop Grumpy.
            var size = beforePreProcessor.Length;
            size = beforeAggregatePushdown.Length;
            size = beforeNormalization.Length;
            size = beforeNTE.Length;
            size = beforeProjectionPruning1.Length;
            size = beforeNestPullup.Length;
            size = beforeProjectionPruning2.Length;
            size = beforeTransformationRules1.Length;
            size = beforeProjectionPruning3.Length;
            size = beforeTransformationRules2.Length;
            size = beforeJoinElimination1.Length;
            size = beforeTransformationRules3.Length;
            size = beforeJoinElimination2.Length;
            size = beforeTransformationRules4.Length;
            size = beforeCodeGen.Length;
#endif
            // All done
            return;
        }

        /// <summary>
        /// Helper method for applying transformation rules
        /// </summary>
        /// <param name="dumpString"></param>
        /// <param name="rulesGroup"></param>
        /// <returns></returns>
        private bool ApplyTransformations(ref string dumpString, TransformationRulesGroup rulesGroup)
        {
            if (MayApplyTransformationRules)
            {
                dumpString = SwitchToPhase(PlanCompilerPhase.Transformations);
                return TransformationRules.Process(this, rulesGroup);
            }
            return false;
        }

        /// <summary>
        /// Logic to perform between each compile phase
        /// </summary>
        /// <param name="newPhase"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "newPhase")]
        private string SwitchToPhase(PlanCompilerPhase newPhase)
        {
            var iqtDumpResult = string.Empty;

            m_phase = newPhase;
#if DEBUG
            if (s_traceCallback != null)
            {
                s_traceCallback(Enum.GetName(typeof(PlanCompilerPhase), newPhase), m_command);
            }
            else
            {
                iqtDumpResult = Dump.ToXml(m_command);
            }

            Validator.Validate(this);
#endif
            return iqtDumpResult;
        }

        /// <summary>
        /// To avoid processing huge trees, transformation rules are applied only if the number of nodes 
        /// is less than MaxNodeCountForTransformations
        /// or if it is specified that they should be applied regardless of the size of the query.
        /// Whether to apply transformations is only computed the first time this property is requested, 
        /// and is cached afterwards. This is because we don't expect the tree to get larger 
        /// from applying transformations. 
        /// </summary>
        private bool MayApplyTransformationRules
        {
            get
            {
                if (m_mayApplyTransformationRules == null)
                {
                    m_mayApplyTransformationRules = ComputeMayApplyTransformations();
                }
                return m_mayApplyTransformationRules.Value;
            }
        }

        /// <summary>
        /// Compute whether transformations may be applied. 
        /// Transformation rules may be applied only if the number of nodes is less than 
        /// MaxNodeCountForTransformations or if it is specified that they should be applied 
        /// regardless of the size of the query.
        /// </summary>
        /// <returns></returns>
        private bool ComputeMayApplyTransformations()
        {
            //
            // If the nextNodeId is less than MaxNodeCountForTransformations then we don't need to 
            // calculate the acutal node count, it must be less than  MaxNodeCountForTransformations
            //
            if (_applyTransformationsRegardlessOfSize.Enabled
                || m_command.NextNodeId < MaxNodeCountForTransformations)
            {
                return true;
            }

            //Compute the actual node count
            var actualCount = NodeCounter.Count(m_command.Root);
            return (actualCount < MaxNodeCountForTransformations);
        }

        /// <summary>
        /// Converts the CTree into an ITree, and initializes the plan
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private void Initialize()
        {
            // Only support queries for now
            var cqtree = m_ctree as cqt.DbQueryCommandTree;
            Assert(cqtree != null, "Unexpected command tree kind. Only query command tree is supported.");

            // Generate the ITree
            m_command = ITreeGenerator.Generate(cqtree);
            Assert(m_command != null, "Unable to generate internal tree from Command Tree");
        }

        #endregion
    }
}
