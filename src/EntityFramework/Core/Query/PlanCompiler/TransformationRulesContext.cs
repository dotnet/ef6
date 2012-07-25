// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;

    internal class TransformationRulesContext : RuleProcessingContext
    {
        #region public methods and properties

        /// <summary>
        /// Whether any rule was applied that may have caused modifications such that projection pruning 
        /// may be useful
        /// </summary>
        internal bool ProjectionPrunningRequired
        {
            get { return m_projectionPrunningRequired; }
        }

        /// <summary>
        /// Whether any rule was applied that may have caused modifications such that reapplying
        /// the nullability rules may be useful
        /// </summary>
        internal bool ReapplyNullabilityRules
        {
            get { return m_reapplyNullabilityRules; }
        }

        /// <summary>
        /// Remap the given subree using the current remapper
        /// </summary>
        /// <param name="subTree"></param>
        internal void RemapSubtree(Node subTree)
        {
            m_remapper.RemapSubtree(subTree);
        }

        /// <summary>
        /// Adds a mapping from oldVar to newVar
        /// </summary>
        /// <param name="oldVar"></param>
        /// <param name="newVar"></param>
        internal void AddVarMapping(Var oldVar, Var newVar)
        {
            m_remapper.AddMapping(oldVar, newVar);
            m_remappedVars.Set(oldVar);
        }

        /// <summary>
        /// "Remap" an expression tree, replacing all references to vars in varMap with
        /// copies of the corresponding expression
        /// The subtree is modified *inplace* - it is the caller's responsibility to make
        /// a copy of the subtree if necessary. 
        /// The "replacement" expression (the replacement for the VarRef) is copied and then
        /// inserted into the appropriate location into the subtree. 
        /// 
        /// Note: we only support replacements in simple ScalarOp trees. This must be 
        /// validated by the caller.
        /// 
        /// </summary>
        /// <param name="node">Current subtree to process</param>
        /// <param name="varMap"></param>
        /// <returns>The updated subtree</returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "scalarOp")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal Node ReMap(Node node, Dictionary<Var, Node> varMap)
        {
            PlanCompiler.Assert(node.Op.IsScalarOp, "Expected a scalarOp: Found " + Dump.AutoString.ToString(node.Op.OpType));

            // Replace varRefOps by the corresponding expression in the map, if any
            if (node.Op.OpType
                == OpType.VarRef)
            {
                var varRefOp = node.Op as VarRefOp;
                Node newNode = null;
                if (varMap.TryGetValue(varRefOp.Var, out newNode))
                {
                    newNode = Copy(newNode);
                    return newNode;
                }
                else
                {
                    return node;
                }
            }

            // Simply process the result of the children.
            for (var i = 0; i < node.Children.Count; i++)
            {
                node.Children[i] = ReMap(node.Children[i], varMap);
            }

            // We may have changed something deep down
            Command.RecomputeNodeInfo(node);
            return node;
        }

        /// <summary>
        /// Makes a copy of the appropriate subtree - with a simple accelerator for VarRefOp
        /// since that's likely to be the most command case
        /// </summary>
        /// <param name="node">the subtree to copy</param>
        /// <returns>the copy of the subtree</returns>
        internal Node Copy(Node node)
        {
            if (node.Op.OpType
                == OpType.VarRef)
            {
                var op = node.Op as VarRefOp;
                return Command.CreateNode(Command.CreateVarRefOp(op.Var));
            }
            else
            {
                return OpCopier.Copy(Command, node);
            }
        }

        /// <summary>
        /// Checks to see if the current subtree only contains ScalarOps
        /// </summary>
        /// <param name="node">current subtree</param>
        /// <returns>true, if the subtree contains only ScalarOps</returns>
        internal bool IsScalarOpTree(Node node)
        {
            var nodeCount = 0;
            return IsScalarOpTree(node, null, ref nodeCount);
        }

        /// <summary>
        /// Is the given var guaranteed to be non-nullable with regards to the node
        /// that is currently being processed.
        /// True, if it is listed as such on any on the node infos on any of the 
        /// current relop ancestors.
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        internal bool IsNonNullable(Var var)
        {
            foreach (var relOpAncestor in m_relOpAncestors)
            {
                // Rules applied to the children of the relOpAncestor may have caused it change. 
                // Thus, if the node is used, it has to have its node info recomputed
                Command.RecomputeNodeInfo(relOpAncestor);
                var nodeInfo = Command.GetExtendedNodeInfo(relOpAncestor);
                if (nodeInfo.NonNullableVisibleDefinitions.IsSet(var))
                {
                    return true;
                }
                else if (nodeInfo.LocalDefinitions.IsSet(var))
                {
                    //The var is defined on this ancestor but is not non-nullable,
                    // therefore there is no need to further check other ancestors
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Is it safe to use a null sentinel with any value?
        /// It may not be safe if:
        /// 1. The top most sort includes null sentinels. If the null sentinel is replaced with a different value
        /// and is used as a sort key it may change the sorting results 
        /// 2. If any of the ancestors is Distinct, GroupBy, Intersect or Except,
        /// because the null sentinel may be used as a key.  
        /// 3. If the null sentinel is defined in the left child of an apply it may be used at the right side, 
        /// thus in these cases we also verify that the right hand side does not have any Distinct, GroupBy, 
        /// Intersect or Except.
        /// </summary>
        internal bool CanChangeNullSentinelValue
        {
            get
            {
                //Is there a sort that includes null sentinels
                if (m_compilerState.HasSortingOnNullSentinels)
                {
                    return false;
                }

                //Is any of the ancestors Distinct, GroupBy, Intersect or Except
                if (m_relOpAncestors.Any(a => IsOpNotSafeForNullSentinelValueChange(a.Op.OpType)))
                {
                    return false;
                }

                // Is the null sentinel defined in the left child of an apply and if so, 
                // does the right hand side have any Distinct, GroupBy, Intersect or Except.
                var applyAncestors = m_relOpAncestors.Where(
                    a =>
                    a.Op.OpType == OpType.CrossApply ||
                    a.Op.OpType == OpType.OuterApply);

                //If the sentinel comes from the right hand side it is ok.
                foreach (var applyAncestor in applyAncestors)
                {
                    if (!m_relOpAncestors.Contains(applyAncestor.Child1)
                        && HasOpNotSafeForNullSentinelValueChange(applyAncestor.Child1))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Is the op not safe for null sentinel value change
        /// </summary>
        /// <param name="optype"></param>
        /// <returns></returns>
        internal static bool IsOpNotSafeForNullSentinelValueChange(OpType optype)
        {
            return optype == OpType.Distinct ||
                   optype == OpType.GroupBy ||
                   optype == OpType.Intersect ||
                   optype == OpType.Except;
        }

        /// <summary>
        /// Does the given subtree contain a node with an op that
        /// is not safer for null sentinel value change
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        internal static bool HasOpNotSafeForNullSentinelValueChange(Node n)
        {
            if (IsOpNotSafeForNullSentinelValueChange(n.Op.OpType))
            {
                return true;
            }
            foreach (var child in n.Children)
            {
                if (HasOpNotSafeForNullSentinelValueChange(child))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Is this is a scalar-op tree? Also return a dictionary of var refcounts (ie)
        /// for each var encountered in the tree, determine the number of times it has
        /// been seen
        /// </summary>
        /// <param name="node">current subtree</param>
        /// <param name="varRefMap">dictionary of var refcounts to fill in</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "varRef")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal bool IsScalarOpTree(Node node, Dictionary<Var, int> varRefMap)
        {
            PlanCompiler.Assert(varRefMap != null, "Null varRef map");

            var nodeCount = 0;
            return IsScalarOpTree(node, varRefMap, ref nodeCount);
        }

        /// <summary>
        /// Get a mapping from Var->Expression for a VarDefListOp tree. This information
        /// will be used by later stages to replace all references to the Vars by the 
        /// corresponding expressions
        /// 
        /// This function uses a few heuristics along the way. It uses the varRefMap
        /// parameter to determine if a computed Var (defined by this VarDefListOp)
        /// has been referenced multiple times, and if it has, it checks to see if
        /// the defining expression is too big (> 100 nodes). This is to avoid 
        /// bloating up the entire query tree with too many copies. 
        /// 
        /// </summary>
        /// <param name="varDefListNode">The varDefListOp subtree</param>
        /// <param name="varRefMap">ref counts for each referenced var</param>
        /// <returns>mapping from Var->replacement xpressions</returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "varDef")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal Dictionary<Var, Node> GetVarMap(Node varDefListNode, Dictionary<Var, int> varRefMap)
        {
            var varDefListOp = (VarDefListOp)varDefListNode.Op;

            var varMap = new Dictionary<Var, Node>();
            foreach (var chi in varDefListNode.Children)
            {
                var varDefOp = (VarDefOp)chi.Op;
                var nonLeafNodeCount = 0;
                var refCount = 0;
                if (!IsScalarOpTree(chi.Child0, null, ref nonLeafNodeCount))
                {
                    return null;
                }
                //
                // More heuristics. If there are multiple references to this Var *and*
                // the defining expression for the Var is "expensive" (ie) has larger than
                // 100 nodes, then simply pretend that this is too hard to do
                // Note: we check for more than 2 references, (rather than just more than 1) - this
                // is simply to let some additional cases through
                // 
                if ((nonLeafNodeCount > 100) &&
                    (varRefMap != null) &&
                    varRefMap.TryGetValue(varDefOp.Var, out refCount)
                    &&
                    (refCount > 2))
                {
                    return null;
                }

                Node n;
                if (varMap.TryGetValue(varDefOp.Var, out n))
                {
                    PlanCompiler.Assert(n == chi.Child0, "reusing varDef for different Node?");
                }
                else
                {
                    varMap.Add(varDefOp.Var, chi.Child0);
                }
            }

            return varMap;
        }

        /// <summary>
        /// Builds a NULLIF expression (ie) a Case expression that looks like
        ///    CASE WHEN v is null THEN null ELSE expr END
        /// where v is the conditionVar parameter, and expr is the value of the expression
        /// when v is non-null
        /// </summary>
        /// <param name="conditionVar">null discriminator var</param>
        /// <param name="expr">expression</param>
        /// <returns></returns>
        internal Node BuildNullIfExpression(Var conditionVar, Node expr)
        {
            var varRefOp = Command.CreateVarRefOp(conditionVar);
            var varRefNode = Command.CreateNode(varRefOp);
            var whenNode = Command.CreateNode(Command.CreateConditionalOp(OpType.IsNull), varRefNode);
            var elseNode = expr;
            var thenNode = Command.CreateNode(Command.CreateNullOp(elseNode.Op.Type));
            var caseNode = Command.CreateNode(Command.CreateCaseOp(elseNode.Op.Type), whenNode, thenNode, elseNode);

            return caseNode;
        }

        #region Rule Interactions

        /// <summary>
        /// Shut off filter pushdown for this subtree
        /// </summary>
        /// <param name="n"></param>
        internal void SuppressFilterPushdown(Node n)
        {
            m_suppressions[n] = n;
        }

        /// <summary>
        /// Is filter pushdown shut off for this subtree?
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        internal bool IsFilterPushdownSuppressed(Node n)
        {
            return m_suppressions.ContainsKey(n);
        }

        /// <summary>
        /// Given a list of vars try to get one that is of type Int32
        /// </summary>
        /// <param name="varList"></param>
        /// <param name="int32Var"></param>
        /// <returns></returns>
        internal static bool TryGetInt32Var(IEnumerable<Var> varList, out Var int32Var)
        {
            foreach (var v in varList)
            {
                // Any Int32 var regardless of the fasets will do
                PrimitiveTypeKind typeKind;
                if (TypeHelpers.TryGetPrimitiveTypeKind(v.Type, out typeKind)
                    && typeKind == PrimitiveTypeKind.Int32)
                {
                    int32Var = v;
                    return true;
                }
            }
            int32Var = null;
            return false;
        }

        #endregion

        #endregion

        #region constructors

        internal TransformationRulesContext(PlanCompiler compilerState)
            : base(compilerState.Command)
        {
            m_compilerState = compilerState;
            m_remapper = new VarRemapper(compilerState.Command);
            m_suppressions = new Dictionary<Node, Node>();
            m_remappedVars = compilerState.Command.CreateVarVec();
        }

        #endregion

        #region private state

        private readonly PlanCompiler m_compilerState;
        private readonly VarRemapper m_remapper;
        private readonly Dictionary<Node, Node> m_suppressions;
        private readonly VarVec m_remappedVars;
        private bool m_projectionPrunningRequired;
        private bool m_reapplyNullabilityRules;
        private readonly Stack<Node> m_relOpAncestors = new Stack<Node>();
#if DEBUG
    /// <summary>
    /// Used to see all the applied rules. 
    /// One way to use it is to put a conditional breakpoint at the end of
    /// PostProcessSubTree with the condition m_relOpAncestors.Count == 0
    /// </summary>
        internal readonly StringBuilder appliedRules = new StringBuilder();
#endif

        #endregion

        #region RuleProcessingContext Overrides

        /// <summary>
        /// Callback function to invoke *before* rules are applied. 
        /// Calls the VarRemapper to update any Vars in this node, and recomputes 
        /// the nodeinfo
        /// </summary>
        /// <param name="n"></param>
        internal override void PreProcess(Node n)
        {
            m_remapper.RemapNode(n);
            Command.RecomputeNodeInfo(n);
        }

        /// <summary>
        /// Callback function to invoke *before* rules are applied. 
        /// Calls the VarRemapper to update any Vars in the entire subtree
        /// If the given node has a RelOp it is pushed on the relOp ancestors stack.
        /// </summary>
        /// <param name="subTree"></param>
        internal override void PreProcessSubTree(Node subTree)
        {
            if (subTree.Op.IsRelOp)
            {
                m_relOpAncestors.Push(subTree);
            }

            if (m_remappedVars.IsEmpty)
            {
                return;
            }

            var nodeInfo = Command.GetNodeInfo(subTree);

            //We need to do remapping only if m_remappedVars overlaps with nodeInfo.ExternalReferences
            foreach (var v in nodeInfo.ExternalReferences)
            {
                if (m_remappedVars.IsSet(v))
                {
                    m_remapper.RemapSubtree(subTree);
                    break;
                }
            }
        }

        /// <summary>
        /// If the given node has a RelOp it is popped from the relOp ancestors stack.
        /// </summary>
        /// <param name="subtree"></param>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "RelOp")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal override void PostProcessSubTree(Node subtree)
        {
            if (subtree.Op.IsRelOp)
            {
                PlanCompiler.Assert(m_relOpAncestors.Count != 0, "The RelOp ancestors stack is empty when post processing a RelOp subtree");
                var poppedNode = m_relOpAncestors.Pop();
                PlanCompiler.Assert(
                    ReferenceEquals(subtree, poppedNode), "The popped ancestor is not equal to the root of the subtree being post processed");
            }
        }

        /// <summary>
        /// Callback function to invoke *after* rules are applied
        /// Recomputes the node info, if this node has changed
        /// If the rule is among the rules after which projection pruning may be beneficial, 
        /// m_projectionPrunningRequired is set to true.
        /// If the rule is among the rules after which reapplying the nullability rules may be beneficial,
        /// m_reapplyNullabilityRules is set to true.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="rule">the rule that was applied</param>
        internal override void PostProcess(Node n, Rule rule)
        {
            if (rule != null)
            {
#if DEBUG
                appliedRules.Append(rule.MethodName);
                appliedRules.AppendLine();
#endif
                if (!m_projectionPrunningRequired
                    && TransformationRules.RulesRequiringProjectionPruning.Contains(rule))
                {
                    m_projectionPrunningRequired = true;
                }
                if (!m_reapplyNullabilityRules
                    && TransformationRules.RulesRequiringNullabilityRulesToBeReapplied.Contains(rule))
                {
                    m_reapplyNullabilityRules = true;
                }
                Command.RecomputeNodeInfo(n);
            }
        }

        /// <summary>
        /// Get the hash value for this subtree
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        internal override int GetHashCode(Node node)
        {
            var nodeInfo = Command.GetNodeInfo(node);
            return nodeInfo.HashValue;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Check to see if the current subtree is a scalar-op subtree (ie) does
        /// the subtree only comprise of scalarOps?
        /// Additionally, compute the number of non-leaf nodes (ie) nodes with at least one child
        /// that are found in the subtree. Note that this count is approximate - it is only
        /// intended to be used as a hint. It is the caller's responsibility to initialize
        /// nodeCount to a sane value on entry into this function
        /// And finally, if the varRefMap parameter is non-null, we keep track of 
        /// how often a Var is referenced within the subtree
        /// 
        /// The non-leaf-node count and the varRefMap are used by GetVarMap to determine
        /// if expressions can be composed together
        /// </summary>
        /// <param name="node">root of the subtree</param>
        /// <param name="varRefMap">Ref counts for each Var encountered in the subtree</param>
        /// <param name="nonLeafNodeCount">count of non-leaf nodes encountered in the subtree</param>
        /// <returns>true, if this node only contains scalarOps</returns>
        private bool IsScalarOpTree(Node node, Dictionary<Var, int> varRefMap, ref int nonLeafNodeCount)
        {
            if (!node.Op.IsScalarOp)
            {
                return false;
            }

            if (node.HasChild0)
            {
                nonLeafNodeCount++;
            }

            if (varRefMap != null
                && node.Op.OpType == OpType.VarRef)
            {
                var varRefOp = (VarRefOp)node.Op;
                int refCount;
                if (!varRefMap.TryGetValue(varRefOp.Var, out refCount))
                {
                    refCount = 1;
                }
                else
                {
                    refCount++;
                }
                varRefMap[varRefOp.Var] = refCount;
            }

            foreach (var chi in node.Children)
            {
                if (!IsScalarOpTree(chi, varRefMap, ref nonLeafNodeCount))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}
