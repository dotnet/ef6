namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Diagnostics;

    /// <summary>
    /// The NodeInfoVisitor is a simple class (ab)using the Visitor pattern to define
    /// NodeInfo semantics for various nodes in the tree
    /// </summary>
    internal class NodeInfoVisitor : BasicOpVisitorOfT<NodeInfo>
    {
        #region public methods

        /// <summary>
        /// The only public method. Recomputes the nodeInfo for a node in the tree, 
        /// but only if the node info has already been computed.  
        /// Assumes that the NodeInfo for each child (if computed already) is valid
        /// </summary>
        /// <param name="n">Node to get NodeInfo for</param>
        internal void RecomputeNodeInfo(Node n)
        {
            if (n.IsNodeInfoInitialized)
            {
                var nodeInfo = VisitNode(n);
                nodeInfo.ComputeHashValue(m_command, n); // compute the hash value for this node
            }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="command"></param>
        internal NodeInfoVisitor(Command command)
        {
            m_command = command;
        }

        #endregion

        #region private state

        private readonly Command m_command;

        #endregion

        #region private methods

        private NodeInfo GetNodeInfo(Node n)
        {
            return n.GetNodeInfo(m_command);
        }

        private ExtendedNodeInfo GetExtendedNodeInfo(Node n)
        {
            return n.GetExtendedNodeInfo(m_command);
        }

        private NodeInfo InitNodeInfo(Node n)
        {
            var nodeInfo = GetNodeInfo(n);
            nodeInfo.Clear();
            return nodeInfo;
        }

        private ExtendedNodeInfo InitExtendedNodeInfo(Node n)
        {
            var nodeInfo = GetExtendedNodeInfo(n);
            nodeInfo.Clear();
            return nodeInfo;
        }

        #endregion

        #region VisitorHelpers

        /// <summary>
        /// Default implementation for scalarOps. Simply adds up external references
        /// from each child
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        protected override NodeInfo VisitDefault(Node n)
        {
            Debug.Assert(n.Op.IsScalarOp || n.Op.IsAncillaryOp, "not a supported optype");

            var nodeInfo = InitNodeInfo(n);
            // My external references are simply the combination of external references
            // of all my children
            foreach (var chi in n.Children)
            {
                var childNodeInfo = GetNodeInfo(chi);
                nodeInfo.ExternalReferences.Or(childNodeInfo.ExternalReferences);
            }
            return nodeInfo;
        }

        /// <summary>
        /// The given definition is non nullable if it is a non-null constant
        /// or a reference to non-nullable input
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="nonNullableInputs"></param>
        /// <returns></returns>
        private static bool IsDefinitionNonNullable(Node definition, VarVec nonNullableInputs)
        {
            return (definition.Op.OpType == OpType.Constant
                    || definition.Op.OpType == OpType.InternalConstant
                    || definition.Op.OpType == OpType.NullSentinel
                    || definition.Op.OpType == OpType.VarRef
                    && nonNullableInputs.IsSet(((VarRefOp)definition.Op).Var));
        }

        #endregion

        #region IOpVisitor<NodeInfo> Members

        #region MiscOps

        #endregion

        #region AncillarOps

        #endregion

        #region ScalarOps

        /// <summary>
        /// The only special case among all scalar and ancillaryOps. Simply adds
        /// its var to the list of unreferenced Ops
        /// </summary>
        /// <param name="op">The VarRefOp</param>
        /// <param name="n">Current node</param>
        /// <returns></returns>
        public override NodeInfo Visit(VarRefOp op, Node n)
        {
            var nodeInfo = InitNodeInfo(n);
            nodeInfo.ExternalReferences.Set(op.Var);
            return nodeInfo;
        }

        #endregion

        #region RelOps

        protected override NodeInfo VisitRelOpDefault(RelOp op, Node n)
        {
            return Unimplemented(n);
        }

        /// <summary>
        /// Definitions = Local Definitions = referenced table columns
        /// External References = none
        /// Keys = keys of entity type
        /// RowCount (default): MinRows = 0, MaxRows = * 
        /// NonNullableDefinitions : non nullable table columns that are definitions
        /// NonNullableInputDefinitions : default(empty) because cannot be used
        /// </summary>
        /// <param name="op">ScanTable/ScanView op</param>
        /// <param name="n">current subtree</param>
        /// <returns>nodeinfo for this subtree</returns>
        protected override NodeInfo VisitTableOp(ScanTableBaseOp op, Node n)
        {
            var nodeInfo = InitExtendedNodeInfo(n);
            // #479372 - only the "referenced" columns of the table should
            // show up in the definitions
            nodeInfo.LocalDefinitions.Or(op.Table.ReferencedColumns);
            nodeInfo.Definitions.Or(op.Table.ReferencedColumns);

            // get table's keys - but only if the key columns have been referenced
            if (op.Table.ReferencedColumns.Subsumes(op.Table.Keys))
            {
                nodeInfo.Keys.InitFrom(op.Table.Keys);
            }
            // no external references

            //non-nullable definitions
            nodeInfo.NonNullableDefinitions.Or(op.Table.NonNullableColumns);
            nodeInfo.NonNullableDefinitions.And(nodeInfo.Definitions);

            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for an UnnestOp.
        /// Definitions = columns of the table produced by this Op
        /// Keys = none
        /// External References = the unnestVar + any external references of the
        ///   computed Var (if any)
        /// RowCount (default): MinRows = 0; MaxRows = *
        /// NonNullableDefinitions: default(empty) 
        /// NonNullableInputDefinitions : default(empty) because cannot be used
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override NodeInfo Visit(UnnestOp op, Node n)
        {
            var nodeInfo = InitExtendedNodeInfo(n);
            foreach (var v in op.Table.Columns)
            {
                nodeInfo.LocalDefinitions.Set(v);
                nodeInfo.Definitions.Set(v);
            }

            // Process keys if it's a TVF with inferred keys, otherwise - no keys.
            if (n.Child0.Op.OpType == OpType.VarDef && n.Child0.Child0.Op.OpType == OpType.Function
                && op.Table.Keys.Count > 0)
            {
                // This is a TVF case. 
                // Get table's keys - but only if they have been referenced.
                if (op.Table.ReferencedColumns.Subsumes(op.Table.Keys))
                {
                    nodeInfo.Keys.InitFrom(op.Table.Keys);
                }
            }
            else
            {
                // no keys
                Debug.Assert(nodeInfo.Keys.NoKeys, "UnnestOp should have no keys in all cases except TVFs mapped to entities.");
            }

            // If I have a child, then my external references are my child's external references.
            // Otherwise, my external reference is my unnestVar
            if (n.HasChild0)
            {
                var childNodeInfo = GetNodeInfo(n.Child0);
                nodeInfo.ExternalReferences.Or(childNodeInfo.ExternalReferences);
            }
            else
            {
                nodeInfo.ExternalReferences.Set(op.Var);
            }

            return nodeInfo;
        }

        /// <summary>
        /// Walk through the computed vars defined by a VarDefListNode, and look for
        /// "simple" Var renames. Build up a mapping from original Vars to the renamed Vars
        /// </summary>
        /// <param name="varDefListNode">the varDefListNode subtree</param>
        /// <returns>A dictionary of Var->Var renames</returns>
        internal static Dictionary<Var, Var> ComputeVarRemappings(Node varDefListNode)
        {
            Debug.Assert(varDefListNode.Op.OpType == OpType.VarDefList);

            var varMap = new Dictionary<Var, Var>();
            foreach (var varDefNode in varDefListNode.Children)
            {
                var varRefOp = varDefNode.Child0.Op as VarRefOp;
                if (varRefOp != null)
                {
                    var varDefOp = varDefNode.Op as VarDefOp;
                    Debug.Assert(varDefOp != null);
                    varMap[varRefOp.Var] = varDefOp.Var;
                }
            }
            return varMap;
        }

        /// <summary>
        /// Computes a NodeInfo for a ProjectOp.
        /// Definitions = the Vars property of this Op
        /// LocalDefinitions = list of computed Vars produced by this node
        /// Keys = Keys of the input Relop (if they are all preserved)
        /// External References = any external references from the computed Vars
        /// RowCount = Input's RowCount
        /// NonNullabeDefinitions = Outputs that are either among the NonNullableDefinitions of the child or
        ///                         are constants defined on this node
        /// NonNullableInputDefinitions = NonNullableDefinitions of the child 
        /// </summary>
        /// <param name="op">The ProjectOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        public override NodeInfo Visit(ProjectOp op, Node n)
        {
            var nodeInfo = InitExtendedNodeInfo(n);

            // Walk through my outputs and identify my "real" definitions
            var relOpChildNodeInfo = GetExtendedNodeInfo(n.Child0);
            // In the first pass, only definitions of the child are considered
            // to be definitions - everything else is an external reference
            foreach (var v in op.Outputs)
            {
                if (relOpChildNodeInfo.Definitions.IsSet(v))
                {
                    nodeInfo.Definitions.Set(v);
                }
                else
                {
                    nodeInfo.ExternalReferences.Set(v);
                }
            }

            //Nonnullable definitions 
            nodeInfo.NonNullableDefinitions.InitFrom(relOpChildNodeInfo.NonNullableDefinitions);
            nodeInfo.NonNullableDefinitions.And(op.Outputs);
            nodeInfo.NonNullableVisibleDefinitions.InitFrom(relOpChildNodeInfo.NonNullableDefinitions);

            // Local definitions
            foreach (var chi in n.Child1.Children)
            {
                var varDefOp = chi.Op as VarDefOp;
                var chiNodeInfo = GetNodeInfo(chi.Child0);
                nodeInfo.LocalDefinitions.Set(varDefOp.Var);
                nodeInfo.ExternalReferences.Clear(varDefOp.Var);
                nodeInfo.Definitions.Set(varDefOp.Var);
                nodeInfo.ExternalReferences.Or(chiNodeInfo.ExternalReferences);

                if (IsDefinitionNonNullable(chi.Child0, nodeInfo.NonNullableVisibleDefinitions))
                {
                    nodeInfo.NonNullableDefinitions.Set(varDefOp.Var);
                }
            }
            nodeInfo.ExternalReferences.Minus(relOpChildNodeInfo.Definitions);
            nodeInfo.ExternalReferences.Or(relOpChildNodeInfo.ExternalReferences);

            // Get the set of keys - simply the list of my child's keys, unless
            // they're not all defined
            nodeInfo.Keys.NoKeys = true;
            if (!relOpChildNodeInfo.Keys.NoKeys)
            {
                // Check to see if any of my child's keys have been left by the wayside
                // in that case, mark this node as having no keys
                var keyVec = m_command.CreateVarVec(relOpChildNodeInfo.Keys.KeyVars);
                var varRenameMap = ComputeVarRemappings(n.Child1);
                var mappedKeyVec = keyVec.Remap(varRenameMap);
                var mappedKeyVecClone = mappedKeyVec.Clone();
                var opVars = m_command.CreateVarVec(op.Outputs);
                mappedKeyVec.Minus(opVars);
                if (mappedKeyVec.IsEmpty)
                {
                    nodeInfo.Keys.InitFrom(mappedKeyVecClone);
                }
            }

            nodeInfo.InitRowCountFrom(relOpChildNodeInfo);
            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for a FilterOp.
        /// Definitions = Definitions of the input Relop
        /// LocalDefinitions = None
        /// Keys = Keys of the input Relop
        /// External References = any external references from the input + any external
        ///    references from the predicate
        /// MaxOneRow = Input's RowCount
        ///    If the predicate is a "false" predicate, then max RowCount is zero
        ///    If we can infer additional info from the key-selector, we may be 
        ///     able to get better estimates
        /// NonNullabeDefinitions = NonNullabeDefinitions of the input RelOp
        /// NonNullableInputDefinitions = NonNullabeDefinitions of the input RelOp
        /// </summary>
        /// <param name="op">The FilterOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        public override NodeInfo Visit(FilterOp op, Node n)
        {
            var nodeInfo = InitExtendedNodeInfo(n);
            var relOpChildNodeInfo = GetExtendedNodeInfo(n.Child0);
            var predNodeInfo = GetNodeInfo(n.Child1);

            // definitions are my child's definitions
            nodeInfo.Definitions.Or(relOpChildNodeInfo.Definitions);
            // No local definitions

            // My external references are my child's external references + those made
            // by my predicate
            nodeInfo.ExternalReferences.Or(relOpChildNodeInfo.ExternalReferences);
            nodeInfo.ExternalReferences.Or(predNodeInfo.ExternalReferences);
            nodeInfo.ExternalReferences.Minus(relOpChildNodeInfo.Definitions);

            // my keys are my child's keys
            nodeInfo.Keys.InitFrom(relOpChildNodeInfo.Keys);

            //The non-nullable definitions are same as these of the child
            nodeInfo.NonNullableDefinitions.InitFrom(relOpChildNodeInfo.NonNullableDefinitions);
            nodeInfo.NonNullableVisibleDefinitions.InitFrom(relOpChildNodeInfo.NonNullableDefinitions);

            // inherit max RowCount from child; set min RowCount to 0, because 
            // we require way more analysis to do anything smarter
            nodeInfo.MinRows = RowCount.Zero;
            // If the predicate is a "false" predicate, then we know that MaxRows 
            // is zero as well
            var predicate = n.Child1.Op as ConstantPredicateOp;
            if (predicate != null
                && predicate.IsFalse)
            {
                nodeInfo.MaxRows = RowCount.Zero;
            }
            else
            {
                nodeInfo.MaxRows = relOpChildNodeInfo.MaxRows;
            }
            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for a GroupByOp.
        /// Definitions = Keys + aggregates
        /// LocalDefinitions = Keys + Aggregates
        /// Keys = GroupBy Keys
        /// External References = any external references from the input + any external
        ///    references from the local computed Vars
        /// RowCount = 
        ///          (1,1) if no group-by keys; 
        ///          otherwise if input MinRows is 1 then (1, input MaxRows); 
        ///          otherwise (0, input MaxRows)
        /// NonNullableDefinitions: non-nullable keys
        /// NonNullableInputDefinitions : default(empty)        
        /// </summary>
        /// <param name="op">The GroupByOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        protected override NodeInfo VisitGroupByOp(GroupByBaseOp op, Node n)
        {
            var nodeInfo = InitExtendedNodeInfo(n);
            var relOpChildNodeInfo = GetExtendedNodeInfo(n.Child0);

            // all definitions are my outputs
            nodeInfo.Definitions.InitFrom(op.Outputs);
            nodeInfo.LocalDefinitions.InitFrom(nodeInfo.Definitions);
            // my definitions are the keys and aggregates I define myself

            // My references are my child's external references + those made
            // by my keys and my aggregates
            nodeInfo.ExternalReferences.Or(relOpChildNodeInfo.ExternalReferences);
            foreach (var chi in n.Child1.Children)
            {
                var keyExprNodeInfo = GetNodeInfo(chi.Child0);
                nodeInfo.ExternalReferences.Or(keyExprNodeInfo.ExternalReferences);
                if (IsDefinitionNonNullable(chi.Child0, relOpChildNodeInfo.NonNullableDefinitions))
                {
                    nodeInfo.NonNullableDefinitions.Set(((VarDefOp)chi.Op).Var);
                }
            }

            // Non-nullable definitions: also all the keys that come from the input
            nodeInfo.NonNullableDefinitions.Or(relOpChildNodeInfo.NonNullableDefinitions);
            nodeInfo.NonNullableDefinitions.And(op.Keys);

            //Handle all aggregates
            for (var i = 2; i < n.Children.Count; i++)
            {
                foreach (var chi in n.Children[i].Children)
                {
                    var aggExprNodeInfo = GetNodeInfo(chi.Child0);
                    nodeInfo.ExternalReferences.Or(aggExprNodeInfo.ExternalReferences);
                }
            }

            // eliminate definitions of my input
            nodeInfo.ExternalReferences.Minus(relOpChildNodeInfo.Definitions);

            // my keys are my grouping keys
            nodeInfo.Keys.InitFrom(op.Keys);

            // row counts
            nodeInfo.MinRows = op.Keys.IsEmpty ? RowCount.One : (relOpChildNodeInfo.MinRows == RowCount.One ? RowCount.One : RowCount.Zero);
            nodeInfo.MaxRows = op.Keys.IsEmpty ? RowCount.One : relOpChildNodeInfo.MaxRows;

            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for a CrossJoinOp.
        /// Definitions = Definitions of my children
        /// LocalDefinitions = None
        /// Keys = Concatenation of the keys of my children (if every one of them has keys; otherwise, null)
        /// External References = any external references from the inputs
        /// RowCount: MinRows: min(min-rows of each child)
        ///              MaxRows: max(max-rows of each child)
        /// NonNullableDefinitions : The NonNullableDefinitions of the children
        /// NonNullableInputDefinitions : default(empty) because cannot be used
        /// </summary>
        /// <param name="op">The CrossJoinOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        public override NodeInfo Visit(CrossJoinOp op, Node n)
        {
            var nodeInfo = InitExtendedNodeInfo(n);

            // No definitions of my own. Simply inherit from my children
            // My external references are the union of my children's external
            // references
            // And my keys are the concatenation of the keys of each of my
            // inputs
            var keyVecList = new List<KeyVec>();
            var maxCard = RowCount.Zero;
            var minCard = RowCount.One;
            foreach (var chi in n.Children)
            {
                var chiNodeInfo = GetExtendedNodeInfo(chi);
                nodeInfo.Definitions.Or(chiNodeInfo.Definitions);
                nodeInfo.ExternalReferences.Or(chiNodeInfo.ExternalReferences);
                keyVecList.Add(chiNodeInfo.Keys);

                nodeInfo.NonNullableDefinitions.Or(chiNodeInfo.NonNullableDefinitions);

                // Not entirely precise, but good enough
                if (chiNodeInfo.MaxRows > maxCard)
                {
                    maxCard = chiNodeInfo.MaxRows;
                }
                if (chiNodeInfo.MinRows < minCard)
                {
                    minCard = chiNodeInfo.MinRows;
                }
            }
            nodeInfo.Keys.InitFrom(keyVecList);

            nodeInfo.SetRowCount(minCard, maxCard);

            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for an Inner/LeftOuter/FullOuter JoinOp.
        /// Definitions = Definitions of my children
        /// LocalDefinitions = None
        /// Keys = Concatenation of the keys of my children (if every one of them has keys; otherwise, null)
        /// External References = any external references from the inputs + any external
        ///    references from the join predicates
        /// RowCount: 
        ///    FullOuterJoin: MinRows = 0, MaxRows = N
        ///    InnerJoin: MinRows = 0; 
        ///               MaxRows = N; if both inputs have RowCount lesser than (or equal to) 1, then maxCard = 1
        ///    OuterJoin: MinRows = leftInput.MinRows
        ///               MaxRows = N; if both inputs have RowCount lesser than (or equal to) 1, then maxCard = 1
        /// NonNullableDefinitions:
        ///    FullOuterJoin: None.
        ///    InnerJoin: NonNullableDefinitions of both children
        ///    LeftOuterJoin: NonNullableDefinitions of the left child
        /// NonNullableInputDefinitions : NonNullabeDefinitions of both children  
        /// </summary>
        /// <param name="op">The JoinOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        protected override NodeInfo VisitJoinOp(JoinBaseOp op, Node n)
        {
            if (!(op.OpType == OpType.InnerJoin ||
                  op.OpType == OpType.LeftOuterJoin ||
                  op.OpType == OpType.FullOuterJoin))
            {
                return Unimplemented(n);
            }

            var nodeInfo = InitExtendedNodeInfo(n);

            // No definitions of my own. Simply inherit from my children
            // My external references are the union of my children's external
            // references
            // And my keys are the concatenation of the keys of each of my
            // inputs
            var leftRelOpNodeInfo = GetExtendedNodeInfo(n.Child0);
            var rightRelOpNodeInfo = GetExtendedNodeInfo(n.Child1);
            var predNodeInfo = GetNodeInfo(n.Child2);

            nodeInfo.Definitions.Or(leftRelOpNodeInfo.Definitions);
            nodeInfo.Definitions.Or(rightRelOpNodeInfo.Definitions);

            nodeInfo.ExternalReferences.Or(leftRelOpNodeInfo.ExternalReferences);
            nodeInfo.ExternalReferences.Or(rightRelOpNodeInfo.ExternalReferences);
            nodeInfo.ExternalReferences.Or(predNodeInfo.ExternalReferences);
            nodeInfo.ExternalReferences.Minus(nodeInfo.Definitions);

            nodeInfo.Keys.InitFrom(leftRelOpNodeInfo.Keys, rightRelOpNodeInfo.Keys);

            //Non-nullable definitions
            if (op.OpType == OpType.InnerJoin
                || op.OpType == OpType.LeftOuterJoin)
            {
                nodeInfo.NonNullableDefinitions.InitFrom(leftRelOpNodeInfo.NonNullableDefinitions);
            }
            if (op.OpType
                == OpType.InnerJoin)
            {
                nodeInfo.NonNullableDefinitions.Or(rightRelOpNodeInfo.NonNullableDefinitions);
            }
            nodeInfo.NonNullableVisibleDefinitions.InitFrom(leftRelOpNodeInfo.NonNullableDefinitions);
            nodeInfo.NonNullableVisibleDefinitions.Or(rightRelOpNodeInfo.NonNullableDefinitions);

            RowCount maxRows;
            RowCount minRows;
            if (op.OpType
                == OpType.FullOuterJoin)
            {
                minRows = RowCount.Zero;
                maxRows = RowCount.Unbounded;
            }
            else
            {
                if ((leftRelOpNodeInfo.MaxRows > RowCount.One)
                    ||
                    (rightRelOpNodeInfo.MaxRows > RowCount.One))
                {
                    maxRows = RowCount.Unbounded;
                }
                else
                {
                    maxRows = RowCount.One;
                }

                if (op.OpType
                    == OpType.LeftOuterJoin)
                {
                    minRows = leftRelOpNodeInfo.MinRows;
                }
                else
                {
                    minRows = RowCount.Zero;
                }
            }

            nodeInfo.SetRowCount(minRows, maxRows);

            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for a CrossApply/OuterApply op.
        /// Definitions = Definitions of my children
        /// LocalDefinitions = None
        /// Keys = Concatenation of the keys of my children (if every one of them has keys; otherwise, null)
        /// External References = any external references from the inputs 
        /// RowCount:
        ///    CrossApply: minRows=0; MaxRows=Unbounded 
        ///         (MaxRows = 1, if both inputs have MaxRow less than or equal to 1)
        ///    OuterApply: minRows=leftInput.MinRows; MaxRows=Unbounded
        ///         (MaxRows = 1, if both inputs have MaxRow less than or equal to 1)
        /// NonNullableDefinitions = 
        ///    CrossApply: NonNullableDefinitions of both children
        ///    OuterApply: NonNullableDefinitions of the left child
        /// NonNullableInputDefinitions = NonNullabeDefinitions of both children  
        /// </summary>
        /// <param name="op">The ApplyOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        protected override NodeInfo VisitApplyOp(ApplyBaseOp op, Node n)
        {
            var nodeInfo = InitExtendedNodeInfo(n);

            var leftRelOpNodeInfo = GetExtendedNodeInfo(n.Child0);
            var rightRelOpNodeInfo = GetExtendedNodeInfo(n.Child1);

            nodeInfo.Definitions.Or(leftRelOpNodeInfo.Definitions);
            nodeInfo.Definitions.Or(rightRelOpNodeInfo.Definitions);

            nodeInfo.ExternalReferences.Or(leftRelOpNodeInfo.ExternalReferences);
            nodeInfo.ExternalReferences.Or(rightRelOpNodeInfo.ExternalReferences);
            nodeInfo.ExternalReferences.Minus(nodeInfo.Definitions);

            nodeInfo.Keys.InitFrom(leftRelOpNodeInfo.Keys, rightRelOpNodeInfo.Keys);

            //NonNullableDefinitions
            nodeInfo.NonNullableDefinitions.InitFrom(leftRelOpNodeInfo.NonNullableDefinitions);
            if (op.OpType
                == OpType.CrossApply)
            {
                nodeInfo.NonNullableDefinitions.Or(rightRelOpNodeInfo.NonNullableDefinitions);
            }
            nodeInfo.NonNullableVisibleDefinitions.InitFrom(leftRelOpNodeInfo.NonNullableDefinitions);
            nodeInfo.NonNullableVisibleDefinitions.Or(rightRelOpNodeInfo.NonNullableDefinitions);

            RowCount maxRows;
            if (leftRelOpNodeInfo.MaxRows <= RowCount.One
                &&
                rightRelOpNodeInfo.MaxRows <= RowCount.One)
            {
                maxRows = RowCount.One;
            }
            else
            {
                maxRows = RowCount.Unbounded;
            }
            var minRows = (op.OpType == OpType.CrossApply) ? RowCount.Zero : leftRelOpNodeInfo.MinRows;
            nodeInfo.SetRowCount(minRows, maxRows);

            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for SetOps (UnionAll, Intersect, Except).
        /// Definitions = OutputVars
        /// LocalDefinitions = OutputVars
        /// Keys = Output Vars for Intersect, Except. For UnionAll ??
        /// External References = any external references from the inputs 
        /// RowCount: Min = 0, Max = unbounded.
        ///    For UnionAlls, MinRows = max(MinRows of left and right inputs)
        /// NonNullable definitions =   
        ///     UnionAll - Columns that are NonNullableDefinitions on both (children) sides
        ///     Except  - Columns that are NonNullableDefinitions on the left child side
        ///     Intersect - Columns that are NonNullableDefinitions on either side
        /// NonNullableInputDefinitions = default(empty) because cannot be used
        /// </summary>
        /// <param name="op">The SetOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        protected override NodeInfo VisitSetOp(SetOp op, Node n)
        {
            var nodeInfo = InitExtendedNodeInfo(n);

            // My definitions and my "all" definitions are simply my outputs
            nodeInfo.Definitions.InitFrom(op.Outputs);
            nodeInfo.LocalDefinitions.InitFrom(op.Outputs);

            var leftChildNodeInfo = GetExtendedNodeInfo(n.Child0);
            var rightChildNodeInfo = GetExtendedNodeInfo(n.Child1);

            var minRows = RowCount.Zero;

            // My external references are the external references of both of 
            // my inputs
            nodeInfo.ExternalReferences.Or(leftChildNodeInfo.ExternalReferences);
            nodeInfo.ExternalReferences.Or(rightChildNodeInfo.ExternalReferences);

            if (op.OpType
                == OpType.UnionAll)
            {
                minRows = (leftChildNodeInfo.MinRows > rightChildNodeInfo.MinRows) ? leftChildNodeInfo.MinRows : rightChildNodeInfo.MinRows;
            }

            // for intersect, and exceptOps, the keys are simply the outputs.
            if (op.OpType == OpType.Intersect
                || op.OpType == OpType.Except)
            {
                nodeInfo.Keys.InitFrom(op.Outputs);
            }
            else
            {
                // UnionAlls are a lot more complicated.  If we've gone through
                // keyPullup, we will have set some keys on it's input branches and
                // what we need to do here is get the keys from each branch and re-map
                // them to the output vars.
                //
                // If the branchDiscriminator is not set on the unionAllOp, then
                // we haven't been through key pullup and we can't look at the keys
                // that the child nodes have, because they're not discriminated.
                //
                // See the logic in KeyPullup, where we make sure that there are
                // actually branch discriminators on the input branches.
                var unionAllOp = (UnionAllOp)op;

                if (null == unionAllOp.BranchDiscriminator)
                {
                    nodeInfo.Keys.NoKeys = true;
                }
                else
                {
                    var nodeKeys = m_command.CreateVarVec();
                    VarVec mappedKeyVec;
                    for (var i = 0; i < n.Children.Count; i++)
                    {
                        var childNodeInfo = n.Children[i].GetExtendedNodeInfo(m_command);
                        if (!childNodeInfo.Keys.NoKeys
                            && !childNodeInfo.Keys.KeyVars.IsEmpty)
                        {
                            mappedKeyVec = childNodeInfo.Keys.KeyVars.Remap(unionAllOp.VarMap[i].GetReverseMap());
                            nodeKeys.Or(mappedKeyVec);
                        }
                        else
                        {
                            // Each branch had better have keys, or we can't continue.
                            nodeKeys.Clear();
                            break;
                        }
                    }

                    // You might be tempted to ask: "Don't we need to add the branch discriminator 
                    // to the keys as well?"  The reason we don't is that we wouldn't be here unless 
                    // we have a branch discriminator variable, which implies we've pulled up keys on
                    // the inputs, and they'll already have the branch descriminator set in the keys
                    // of each input, so we don't need to add that...
                    if (nodeKeys.IsEmpty)
                    {
                        nodeInfo.Keys.NoKeys = true;
                    }
                    else
                    {
                        nodeInfo.Keys.InitFrom(nodeKeys);
                    }
                }
            }

            //Non-nullable definitions
            var leftNonNullableVars = leftChildNodeInfo.NonNullableDefinitions.Remap(op.VarMap[0].GetReverseMap());
            nodeInfo.NonNullableDefinitions.InitFrom(leftNonNullableVars);

            if (op.OpType
                != OpType.Except)
            {
                var rightNonNullableVars = rightChildNodeInfo.NonNullableDefinitions.Remap(op.VarMap[1].GetReverseMap());
                if (op.OpType
                    == OpType.Intersect)
                {
                    nodeInfo.NonNullableDefinitions.Or(rightNonNullableVars);
                }
                else //Union all
                {
                    nodeInfo.NonNullableDefinitions.And(rightNonNullableVars);
                }
            }

            nodeInfo.NonNullableDefinitions.And(op.Outputs);

            nodeInfo.MinRows = minRows;
            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for a ConstrainedSortOp/SortOp.
        /// Definitions = Definitions of the input Relop
        /// LocalDefinitions = not allowed
        /// Keys = Keys of the input Relop
        /// External References = any external references from the input + any external
        ///    references from the keys
        /// RowCount = Input's RowCount
        /// NonNullabeDefinitions = NonNullabeDefinitions of the input RelOp
        /// NonNullableInputDefinitions = NonNullabeDefinitions of the input RelOp
        /// </summary>
        /// <param name="op">The SortOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        protected override NodeInfo VisitSortOp(SortBaseOp op, Node n)
        {
            var nodeInfo = InitExtendedNodeInfo(n);
            var relOpChildNodeInfo = GetExtendedNodeInfo(n.Child0);

            // definitions are my child's definitions
            nodeInfo.Definitions.Or(relOpChildNodeInfo.Definitions);

            // My references are my child's external references + those made
            // by my sort keys
            nodeInfo.ExternalReferences.Or(relOpChildNodeInfo.ExternalReferences);
            nodeInfo.ExternalReferences.Minus(relOpChildNodeInfo.Definitions);

            // my keys are my child's keys
            nodeInfo.Keys.InitFrom(relOpChildNodeInfo.Keys);

            //Non-nullable definitions are same as the input
            nodeInfo.NonNullableDefinitions.InitFrom(relOpChildNodeInfo.NonNullableDefinitions);
            nodeInfo.NonNullableVisibleDefinitions.InitFrom(relOpChildNodeInfo.NonNullableDefinitions);

            //Row counts are same as the input
            nodeInfo.InitRowCountFrom(relOpChildNodeInfo);

            // For constrained sort, if the Limit value is Constant(1) and WithTies is false,
            // then MinRows and MaxRows can be adjusted to 0, 1.
            if (OpType.ConstrainedSort == op.OpType &&
                OpType.Constant == n.Child2.Op.OpType
                &&
                !((ConstrainedSortOp)op).WithTies)
            {
                var constOp = (ConstantBaseOp)n.Child2.Op;
                if (TypeHelpers.IsIntegerConstant(constOp.Type, constOp.Value, 1))
                {
                    nodeInfo.SetRowCount(RowCount.Zero, RowCount.One);
                }
            }

            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for Distinct.
        /// Definitions = OutputVars that are not external references
        /// LocalDefinitions = None
        /// Keys = Output Vars 
        /// External References = any external references from the inputs 
        /// RowCount = Input's RowCount
        /// NonNullabeDefinitions : NonNullabeDefinitions of the input RelOp that are outputs
        /// NonNullableInputDefinitions : default(empty) because cannot be used
        /// </summary>
        /// <param name="op">The DistinctOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        public override NodeInfo Visit(DistinctOp op, Node n)
        {
            var nodeInfo = InitExtendedNodeInfo(n);

            //#497217 - The parameters should not be included as keys
            nodeInfo.Keys.InitFrom(op.Keys, true);

            // external references - inherit from child
            var childNodeInfo = GetExtendedNodeInfo(n.Child0);
            nodeInfo.ExternalReferences.InitFrom(childNodeInfo.ExternalReferences);

            // no local definitions - definitions are just the keys that are not external references
            foreach (var v in op.Keys)
            {
                if (childNodeInfo.Definitions.IsSet(v))
                {
                    nodeInfo.Definitions.Set(v);
                }
                else
                {
                    nodeInfo.ExternalReferences.Set(v);
                }
            }

            //Non-nullable definitions
            nodeInfo.NonNullableDefinitions.InitFrom(childNodeInfo.NonNullableDefinitions);
            nodeInfo.NonNullableDefinitions.And(op.Keys);

            nodeInfo.InitRowCountFrom(childNodeInfo);
            return nodeInfo;
        }

        /// <summary>
        /// Compute NodeInfo for a SingleRowOp.
        /// Definitions = child's definitions
        /// Keys = child's keys
        /// Local Definitions = none
        /// External references = child's external references
        /// RowCount=(0,1)
        /// NonNullabeDefinitions = NonNullabeDefinitions of the input RelOp
        /// NonNullableInputDefinitions : default(empty) because cannot be used        
        /// </summary>
        /// <param name="op">The SingleRowOp</param>
        /// <param name="n">current subtree</param>
        /// <returns>NodeInfo for this node</returns>
        public override NodeInfo Visit(SingleRowOp op, Node n)
        {
            var nodeInfo = InitExtendedNodeInfo(n);
            var childNodeInfo = GetExtendedNodeInfo(n.Child0);
            nodeInfo.Definitions.InitFrom(childNodeInfo.Definitions);
            nodeInfo.Keys.InitFrom(childNodeInfo.Keys);
            nodeInfo.ExternalReferences.InitFrom(childNodeInfo.ExternalReferences);
            nodeInfo.NonNullableDefinitions.InitFrom(childNodeInfo.NonNullableDefinitions);
            nodeInfo.SetRowCount(RowCount.Zero, RowCount.One);
            return nodeInfo;
        }

        /// <summary>
        /// SingleRowTableOp
        /// No definitions, external references, non-nullable definitions
        /// Keys = empty list (not the same as "no keys")
        /// RowCount = (1,1)
        /// </summary>
        /// <param name="op">the SingleRowTableOp</param>
        /// <param name="n">current subtree</param>
        /// <returns>nodeInfo for this subtree</returns>
        public override NodeInfo Visit(SingleRowTableOp op, Node n)
        {
            var nodeInfo = InitExtendedNodeInfo(n);
            nodeInfo.Keys.NoKeys = false;
            nodeInfo.SetRowCount(RowCount.One, RowCount.One);
            return nodeInfo;
        }

        #endregion

        #region PhysicalOps

        /// <summary>
        /// Computes a NodeInfo for a PhysicalProjectOp.
        /// Definitions = OutputVars
        /// LocalDefinitions = None
        /// Keys = None
        /// External References = any external references from the inputs
        /// RowCount=default
        /// NonNullabeDefinitions = NonNullabeDefinitions of the input RelOp that are among the definitions
        /// NonNullableInputDefinitions = NonNullabeDefinitions of the input RelOp
        /// </summary>
        /// <param name="op">The PhysicalProjectOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        public override NodeInfo Visit(PhysicalProjectOp op, Node n)
        {
            var nodeInfo = InitExtendedNodeInfo(n);
            foreach (var chi in n.Children)
            {
                var childNodeInfo = GetNodeInfo(chi);
                nodeInfo.ExternalReferences.Or(childNodeInfo.ExternalReferences);
            }
            nodeInfo.Definitions.InitFrom(op.Outputs);
            nodeInfo.LocalDefinitions.InitFrom(nodeInfo.Definitions);

            //
            // Inherit the keys from the child - but only if all the columns were projected
            // out
            // 
            var driverChildNodeInfo = GetExtendedNodeInfo(n.Child0);
            if (!driverChildNodeInfo.Keys.NoKeys)
            {
                var missingKeys = m_command.CreateVarVec(driverChildNodeInfo.Keys.KeyVars);
                missingKeys.Minus(nodeInfo.Definitions);
                if (missingKeys.IsEmpty)
                {
                    nodeInfo.Keys.InitFrom(driverChildNodeInfo.Keys);
                }
            }

            //Non-nullable definitions
            nodeInfo.NonNullableDefinitions.Or(driverChildNodeInfo.NonNullableDefinitions);
            nodeInfo.NonNullableDefinitions.And(nodeInfo.Definitions);
            nodeInfo.NonNullableVisibleDefinitions.Or(driverChildNodeInfo.NonNullableVisibleDefinitions);

            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for a NestOp (SingleStream/MultiStream).
        /// Definitions = OutputVars
        /// LocalDefinitions = Collection Vars
        /// Keys = Keys of my child
        /// External References = any external references from the inputs 
        /// RowCount=default
        /// </summary>
        /// <param name="op">The NestOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        protected override NodeInfo VisitNestOp(NestBaseOp op, Node n)
        {
            var ssnOp = op as SingleStreamNestOp;
            var nodeInfo = InitExtendedNodeInfo(n);

            foreach (var ci in op.CollectionInfo)
            {
                nodeInfo.LocalDefinitions.Set(ci.CollectionVar);
            }
            nodeInfo.Definitions.InitFrom(op.Outputs);

            // get external references from each child
            foreach (var chi in n.Children)
            {
                nodeInfo.ExternalReferences.Or(GetExtendedNodeInfo(chi).ExternalReferences);
            }

            // eliminate things I may have defined already (left correlation)
            nodeInfo.ExternalReferences.Minus(nodeInfo.Definitions);

            // Keys are from the driving node only.
            if (ssnOp == null)
            {
                nodeInfo.Keys.InitFrom(GetExtendedNodeInfo(n.Child0).Keys);
            }
            else
            {
                nodeInfo.Keys.InitFrom(ssnOp.Keys);
            }
            return nodeInfo;
        }

        #endregion

        #endregion
    }
}