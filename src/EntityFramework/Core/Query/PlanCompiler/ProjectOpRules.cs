namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Linq;

    /// <summary>
    /// Transformation rules for ProjectOp
    /// </summary>
    internal static class ProjectOpRules
    {
        #region ProjectOverProject

        internal static readonly PatternMatchRule Rule_ProjectOverProject =
            new PatternMatchRule(
                new Node(
                    ProjectOp.Pattern,
                    new Node(
                        ProjectOp.Pattern,
                        new Node(LeafOp.Pattern),
                        new Node(LeafOp.Pattern)),
                    new Node(LeafOp.Pattern)),
                ProcessProjectOverProject);

        /// <summary>
        /// Converts a Project(Project(X, c1,...), d1,...) => 
        ///            Project(X, d1', d2'...)
        /// where d1', d2' etc. are the "mapped" versions of d1, d2 etc.
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="projectNode">Current ProjectOp node</param>
        /// <param name="newNode">modified subtree</param>
        /// <returns>Transformation status</returns>
        private static bool ProcessProjectOverProject(RuleProcessingContext context, Node projectNode, out Node newNode)
        {
            newNode = projectNode;
            var projectOp = (ProjectOp)projectNode.Op;
            var varDefListNode = projectNode.Child1;
            var subProjectNode = projectNode.Child0;
            var subProjectOp = (ProjectOp)subProjectNode.Op;
            var trc = (TransformationRulesContext)context;

            // If any of the defining expressions is not a scalar op tree, then simply
            // quit
            var varRefMap = new Dictionary<Var, int>();
            foreach (var varDefNode in varDefListNode.Children)
            {
                if (!trc.IsScalarOpTree(varDefNode.Child0, varRefMap))
                {
                    return false;
                }
            }

            var varMap = trc.GetVarMap(subProjectNode.Child1, varRefMap);
            if (varMap == null)
            {
                return false;
            }

            // create a new varDefList node...
            var newVarDefListNode = trc.Command.CreateNode(trc.Command.CreateVarDefListOp());

            // Remap any local definitions, I have
            foreach (var varDefNode in varDefListNode.Children)
            {
                // update the defining expression
                varDefNode.Child0 = trc.ReMap(varDefNode.Child0, varMap);
                trc.Command.RecomputeNodeInfo(varDefNode);
                newVarDefListNode.Children.Add(varDefNode);
            }

            // Now, pull up any definitions of the subProject that I publish myself
            var projectNodeInfo = trc.Command.GetExtendedNodeInfo(projectNode);
            foreach (var chi in subProjectNode.Child1.Children)
            {
                var varDefOp = (VarDefOp)chi.Op;
                if (projectNodeInfo.Definitions.IsSet(varDefOp.Var))
                {
                    newVarDefListNode.Children.Add(chi);
                }
            }

            //
            // now that we have remapped all our computed vars, simply bypass the subproject
            // node
            //
            projectNode.Child0 = subProjectNode.Child0;
            projectNode.Child1 = newVarDefListNode;
            return true;
        }

        #endregion

        #region ProjectWithNoLocalDefinitions

        internal static readonly PatternMatchRule Rule_ProjectWithNoLocalDefs =
            new PatternMatchRule(
                new Node(
                    ProjectOp.Pattern,
                    new Node(LeafOp.Pattern),
                    new Node(VarDefListOp.Pattern)),
                ProcessProjectWithNoLocalDefinitions);

        /// <summary>
        /// Eliminate a ProjectOp that has no local definitions at all and 
        /// no external references, (ie) if Child1
        /// of the ProjectOp (the VarDefListOp child) has no children, then the ProjectOp
        /// is serving no useful purpose. Get rid of the ProjectOp, and replace it with its
        /// child
        /// </summary>
        /// <param name="context">rule processing context</param>
        /// <param name="n">current subtree</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>transformation status</returns>
        private static bool ProcessProjectWithNoLocalDefinitions(RuleProcessingContext context, Node n, out Node newNode)
        {
            newNode = n;
            var nodeInfo = context.Command.GetNodeInfo(n);

            // We cannot eliminate this node because it can break other rules, 
            // e.g. ProcessApplyOverAnything which relies on existance of external refs to substitute
            // CrossApply(x, y) => CrossJoin(x, y). See SQLBU #481719.
            if (!nodeInfo.ExternalReferences.IsEmpty)
            {
                return false;
            }

            newNode = n.Child0;
            return true;
        }

        #endregion

        #region ProjectOpWithSimpleVarRedefinitions

        internal static readonly SimpleRule Rule_ProjectOpWithSimpleVarRedefinitions = new SimpleRule(
            OpType.Project, ProcessProjectWithSimpleVarRedefinitions);

        /// <summary>
        /// If the ProjectOp defines some computedVars, but those computedVars are simply 
        /// redefinitions of other Vars, then eliminate the computedVars. 
        /// 
        /// Project(X, VarDefList(VarDef(cv1, VarRef(v1)), ...))
        ///    can be transformed into
        /// Project(X, VarDefList(...))
        /// where cv1 has now been replaced by v1
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="n">current subtree</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>transformation status</returns>
        private static bool ProcessProjectWithSimpleVarRedefinitions(RuleProcessingContext context, Node n, out Node newNode)
        {
            newNode = n;
            var projectOp = (ProjectOp)n.Op;

            if (n.Child1.Children.Count == 0)
            {
                return false;
            }

            var trc = (TransformationRulesContext)context;
            var command = trc.Command;

            var nodeInfo = command.GetExtendedNodeInfo(n);

            //
            // Check to see if any of the computed Vars defined by this ProjectOp
            // are simple redefinitions of other VarRefOps. Consider only those 
            // VarRefOps that are not "external" references
            var canEliminateSomeVars = false;
            foreach (var varDefNode in n.Child1.Children)
            {
                var definingExprNode = varDefNode.Child0;
                if (definingExprNode.Op.OpType
                    == OpType.VarRef)
                {
                    var varRefOp = (VarRefOp)definingExprNode.Op;
                    if (!nodeInfo.ExternalReferences.IsSet(varRefOp.Var))
                    {
                        // this is a Var that we should remove 
                        canEliminateSomeVars = true;
                        break;
                    }
                }
            }

            // Did we have any redefinitions
            if (!canEliminateSomeVars)
            {
                return false;
            }

            //
            // OK. We've now identified a set of vars that are simple redefinitions.
            // Try and replace the computed Vars with the Vars that they're redefining
            //

            // Lets now build up a new VarDefListNode
            var newVarDefNodes = new List<Node>();
            foreach (var varDefNode in n.Child1.Children)
            {
                var varDefOp = (VarDefOp)varDefNode.Op;
                var varRefOp = varDefNode.Child0.Op as VarRefOp;
                if (varRefOp != null
                    && !nodeInfo.ExternalReferences.IsSet(varRefOp.Var))
                {
                    projectOp.Outputs.Clear(varDefOp.Var);
                    projectOp.Outputs.Set(varRefOp.Var);
                    trc.AddVarMapping(varDefOp.Var, varRefOp.Var);
                }
                else
                {
                    newVarDefNodes.Add(varDefNode);
                }
            }

            // Note: Even if we don't have any local var definitions left, we should not remove
            // this project yet because: 
            //  (1) this project node may be prunning out some outputs;
            //  (2) the rule Rule_ProjectWithNoLocalDefs, would do that later anyway.

            // Create a new vardeflist node, and set that as Child1 for the projectOp
            var newVarDefListNode = command.CreateNode(command.CreateVarDefListOp(), newVarDefNodes);
            n.Child1 = newVarDefListNode;
            return true; // some part of the subtree was modified
        }

        #endregion

        #region ProjectOpWithNullSentinel

        internal static readonly SimpleRule Rule_ProjectOpWithNullSentinel = new SimpleRule(
            OpType.Project, ProcessProjectOpWithNullSentinel);

        /// <summary>
        /// Tries to remove null sentinel definitions by replacing them to vars that are guaranteed 
        /// to be non-nullable and of integer type, or with reference to other constants defined in the 
        /// same project. In particular, 
        /// 
        ///  - If based on the ancestors, the value of the null sentinel can be changed and the 
        /// input of the project has a var that is guaranteed to be non-nullable and 
        /// is of integer type, then the definitions of the vars defined as NullSentinels in the ProjectOp 
        /// are replaced with a reference to that var. I.eg:
        /// 
        /// Project(X, VarDefList(VarDef(ns_var, NullSentinel), ...))
        ///    can be transformed into
        /// Project(X, VarDefList(VarDef(ns_var, VarRef(v))...))
        /// where v is known to be non-nullable
        /// 
        /// - Else, if based on the ancestors, the value of the null sentinel can be changed and 
        /// the project already has definitions of other int constants, the definitions of the null sentinels
        /// are removed and the respective vars are remapped to the var representing the constant.
        /// 
        /// - Else, the definitions of the all null sentinels except for one are removed, and the
        /// the respective vars are remapped to the remaining null sentinel. 
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="n">current subtree</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>transformation status</returns>
        private static bool ProcessProjectOpWithNullSentinel(RuleProcessingContext context, Node n, out Node newNode)
        {
            newNode = n;
            var projectOp = (ProjectOp)n.Op;
            var varDefListNode = n.Child1;

            if (varDefListNode.Children.Where(c => c.Child0.Op.OpType == OpType.NullSentinel).Count() == 0)
            {
                return false;
            }

            var trc = (TransformationRulesContext)context;
            var command = trc.Command;
            var relOpInputNodeInfo = command.GetExtendedNodeInfo(n.Child0);
            Var inputSentinel;
            var reusingConstantFromSameProjectAsSentinel = false;

            var canChangeNullSentinelValue = trc.CanChangeNullSentinelValue;

            if (!canChangeNullSentinelValue
                || !TransformationRulesContext.TryGetInt32Var(relOpInputNodeInfo.NonNullableDefinitions, out inputSentinel))
            {
                reusingConstantFromSameProjectAsSentinel = true;
                if (!canChangeNullSentinelValue
                    ||
                    !TransformationRulesContext.TryGetInt32Var(
                        n.Child1.Children.Where(
                            child => child.Child0.Op.OpType == OpType.Constant || child.Child0.Op.OpType == OpType.InternalConstant).Select(
                                child => ((VarDefOp)(child.Op)).Var), out inputSentinel))
                {
                    inputSentinel =
                        n.Child1.Children.Where(child => child.Child0.Op.OpType == OpType.NullSentinel).Select(
                            child => ((VarDefOp)(child.Op)).Var).FirstOrDefault();
                    if (inputSentinel == null)
                    {
                        return false;
                    }
                }
            }

            var modified = false;

            for (var i = n.Child1.Children.Count - 1; i >= 0; i--)
            {
                var varDefNode = n.Child1.Children[i];
                var definingExprNode = varDefNode.Child0;
                if (definingExprNode.Op.OpType
                    == OpType.NullSentinel)
                {
                    if (!reusingConstantFromSameProjectAsSentinel)
                    {
                        var varRefOp = command.CreateVarRefOp(inputSentinel);
                        varDefNode.Child0 = command.CreateNode(varRefOp);
                        command.RecomputeNodeInfo(varDefNode);
                        modified = true;
                    }
                    else if (!inputSentinel.Equals(((VarDefOp)varDefNode.Op).Var))
                    {
                        projectOp.Outputs.Clear(((VarDefOp)varDefNode.Op).Var);
                        n.Child1.Children.RemoveAt(i);
                        trc.AddVarMapping(((VarDefOp)varDefNode.Op).Var, inputSentinel);
                        modified = true;
                    }
                }
            }

            if (modified)
            {
                command.RecomputeNodeInfo(n.Child1);
            }
            return modified;
        }

        #endregion

        #region All ProjectOp Rules

        //The order of the rules is important
        internal static readonly Rule[] Rules = new Rule[]
                                                    {
                                                        Rule_ProjectOpWithNullSentinel,
                                                        Rule_ProjectOpWithSimpleVarRedefinitions,
                                                        Rule_ProjectOverProject,
                                                        Rule_ProjectWithNoLocalDefs,
                                                    };

        #endregion
    }
}
