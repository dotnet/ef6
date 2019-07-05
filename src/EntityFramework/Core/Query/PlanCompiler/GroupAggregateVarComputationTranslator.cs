// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;

    // <summary>
    // Utility class that tries to produce an equivalent tree to the input tree over
    // a single group aggregate variable and no other external references
    // </summary>
    internal class GroupAggregateVarComputationTranslator : BasicOpVisitorOfNode
    {
        #region Private State

        private GroupAggregateVarInfo _targetGroupAggregateVarInfo;
        private bool _isUnnested;
        private readonly Command _command;
        private readonly GroupAggregateVarInfoManager _groupAggregateVarInfoManager;

        #endregion

        #region Constructor

        // <summary>
        // Private constructor
        // </summary>
        private GroupAggregateVarComputationTranslator(
            Command command,
            GroupAggregateVarInfoManager groupAggregateVarInfoManager)
        {
            _command = command;
            _groupAggregateVarInfoManager = groupAggregateVarInfoManager;
        }

        #endregion

        #region 'Public' Surface

        // <summary>
        // Try to produce an equivalent tree to the input subtree, over a single group aggregate variable.
        // Such translation can only be produced if all external references of the input subtree are to a
        // single group aggregate var, or to vars that are can be translated over that single group
        // aggregate var
        // </summary>
        // <param name="subtree"> The input subtree </param>
        // <param name="groupAggregateVarInfo"> The groupAggregateVarInfo over which the input subtree can be translated </param>
        // <param name="templateNode"> A tree that is equvalent to the input tree, but over the group aggregate variable represented by the groupAggregetVarInfo </param>
        // <returns> True, if the translation can be done, false otherwise </returns>
        public static bool TryTranslateOverGroupAggregateVar(
            Node subtree,
            bool isVarDefinition,
            Command command,
            GroupAggregateVarInfoManager groupAggregateVarInfoManager,
            out GroupAggregateVarInfo groupAggregateVarInfo,
            out Node templateNode,
            out bool isUnnested)
        {
            var handler = new GroupAggregateVarComputationTranslator(command, groupAggregateVarInfoManager);

            var inputNode = subtree;
            SoftCastOp softCastOp = null;
            bool isCollect;
            if (inputNode.Op.OpType
                == OpType.SoftCast)
            {
                softCastOp = (SoftCastOp)inputNode.Op;
                inputNode = inputNode.Child0;
            }

            if (inputNode.Op.OpType
                == OpType.Collect)
            {
                templateNode = handler.VisitCollect(inputNode);
                isCollect = true;
            }
            else
            {
                templateNode = handler.VisitNode(inputNode);
                isCollect = false;
            }

            groupAggregateVarInfo = handler._targetGroupAggregateVarInfo;
            isUnnested = handler._isUnnested;

            if (handler._targetGroupAggregateVarInfo == null
                || templateNode == null)
            {
                return false;
            }
            if (softCastOp != null)
            {
                SoftCastOp newSoftCastOp;
                // 
                // The type needs to be fixed only if the unnesting happened during this translation.
                // That can be recognized by these two cases: 
                //      1) if the input node was a collect, or 
                //      2) if the input did not represent a var definition, but a function aggregate argument and 
                //              the template is VarRef of a group aggregate var.
                //
                if (isCollect
                    ||
                    !isVarDefinition
                    && AggregatePushdownUtil.IsVarRefOverGivenVar(templateNode, handler._targetGroupAggregateVarInfo.GroupAggregateVar))
                {
                    newSoftCastOp = command.CreateSoftCastOp(TypeHelpers.GetEdmType<CollectionType>(softCastOp.Type).TypeUsage);
                }
                else
                {
                    newSoftCastOp = softCastOp;
                }
                templateNode = command.CreateNode(newSoftCastOp, templateNode);
            }
            return true;
        }

        #endregion

        #region Visitor Methods

        // <summary>
        // See <see cref="TryTranslateOverGroupAggregateVar" />
        // </summary>
        public override Node Visit(VarRefOp op, Node n)
        {
            return TranslateOverGroupAggregateVar(op.Var, null);
        }

        // <summary>
        // If the child is VarRef check if the subtree PropertyOp(VarRef) is reference to a
        // group aggregate var.
        // Otherwise do default processing
        // </summary>
        public override Node Visit(PropertyOp op, Node n)
        {
            if (n.Child0.Op.OpType
                != OpType.VarRef)
            {
                return base.Visit(op, n);
            }
            var varRefOp = (VarRefOp)n.Child0.Op;
            return TranslateOverGroupAggregateVar(varRefOp.Var, op.PropertyInfo);
        }

        // <summary>
        // If the Subtree rooted at the collect is of the following structure:
        // PhysicalProject(outputVar)
        // |
        // Project(s)
        // |
        // Unnest
        // where the unnest is over the group aggregate var and the output var
        // is either a reference to the group aggregate var or to a constant, it returns the
        // translation of the output var.
        // </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private Node VisitCollect(Node n)
        {
            //Make sure the only children are projects over unnest
            var currentNode = n.Child0;
            var constantDefinitions = new Dictionary<Var, Node>();
            while (currentNode.Child0.Op.OpType
                   == OpType.Project)
            {
                currentNode = currentNode.Child0;
                //Visit the VarDefListOp child
                if (VisitDefault(currentNode.Child1) == null)
                {
                    return null;
                }
                foreach (var definitionNode in currentNode.Child1.Children)
                {
                    if (IsConstant(definitionNode.Child0))
                    {
                        constantDefinitions.Add(((VarDefOp)definitionNode.Op).Var, definitionNode.Child0);
                    }
                }
            }

            if (currentNode.Child0.Op.OpType
                != OpType.Unnest)
            {
                return null;
            }

            // Handle the unnest
            var unnestOp = (UnnestOp)currentNode.Child0.Op;
            GroupAggregateVarRefInfo groupAggregateVarRefInfo;
            if (_groupAggregateVarInfoManager.TryGetReferencedGroupAggregateVarInfo(unnestOp.Var, out groupAggregateVarRefInfo))
            {
                if (_targetGroupAggregateVarInfo == null)
                {
                    _targetGroupAggregateVarInfo = groupAggregateVarRefInfo.GroupAggregateVarInfo;
                }
                else if (_targetGroupAggregateVarInfo != groupAggregateVarRefInfo.GroupAggregateVarInfo)
                {
                    return null;
                }
                if (!_isUnnested)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            var physicalProjectOp = (PhysicalProjectOp)n.Child0.Op;
            PlanCompiler.Assert(physicalProjectOp.Outputs.Count == 1, "Physical project should only have one output at this stage");
            var outputVar = physicalProjectOp.Outputs[0];

            var computationTemplate = TranslateOverGroupAggregateVar(outputVar, null);
            if (computationTemplate != null)
            {
                _isUnnested = true;
                return computationTemplate;
            }

            Node constantDefinitionNode;
            if (constantDefinitions.TryGetValue(outputVar, out constantDefinitionNode))
            {
                _isUnnested = true;
                return constantDefinitionNode;
            }
            return null;
        }

        // <summary>
        // Determines whether the given Node is a constant subtree
        // It only recognizes any of the constant base ops
        // and possibly casts over these nodes.
        // </summary>
        private static bool IsConstant(Node node)
        {
            var currentNode = node;
            while (currentNode.Op.OpType
                   == OpType.Cast)
            {
                currentNode = currentNode.Child0;
            }
            return PlanCompilerUtil.IsConstantBaseOp(currentNode.Op.OpType);
        }

        // <summary>
        // (1) If the given var or the given property of the given var are defined over a group aggregate var,
        // (2) and if that group aggregate var matches the var represented by represented by _targetGroupAggregateVarInfo
        // if any
        // it returns the corresponding translation over the group aggregate var. Also, if _targetGroupAggregateVarInfo
        // is not set, it sets it to the group aggregate var representing the referenced var.
        // </summary>
        private Node TranslateOverGroupAggregateVar(Var var, EdmMember property)
        {
            GroupAggregateVarRefInfo groupAggregateVarRefInfo;
            EdmMember localProperty;
            if (_groupAggregateVarInfoManager.TryGetReferencedGroupAggregateVarInfo(var, out groupAggregateVarRefInfo))
            {
                localProperty = property;
            }
            else if (_groupAggregateVarInfoManager.TryGetReferencedGroupAggregateVarInfo(var, property, out groupAggregateVarRefInfo))
            {
                localProperty = null;
            }
            else
            {
                return null;
            }

            if (_targetGroupAggregateVarInfo == null)
            {
                _targetGroupAggregateVarInfo = groupAggregateVarRefInfo.GroupAggregateVarInfo;
                _isUnnested = groupAggregateVarRefInfo.IsUnnested;
            }
            else if (_targetGroupAggregateVarInfo != groupAggregateVarRefInfo.GroupAggregateVarInfo
                     || _isUnnested != groupAggregateVarRefInfo.IsUnnested)
            {
                return null;
            }

            var computationTemplate = groupAggregateVarRefInfo.Computation;
            if (localProperty != null)
            {
                computationTemplate = _command.CreateNode(_command.CreatePropertyOp(localProperty), computationTemplate);
            }
            return computationTemplate;
        }

        // <summary>
        // Default processing for nodes.
        // Visits the children and if any child has changed it creates a new node
        // for the parent.
        // If the reference of the child node did not change, the child node did not change either,
        // this is because a node can only be reused "as is" when building a template.
        // </summary>
        protected override Node VisitDefault(Node n)
        {
            var newChildren = new List<Node>(n.Children.Count);
            var anyChildChanged = false;
            for (var i = 0; i < n.Children.Count; i++)
            {
                var processedChild = VisitNode(n.Children[i]);
                if (processedChild == null)
                {
                    return null;
                }
                if (!anyChildChanged
                    && !ReferenceEquals(n.Children[i], processedChild))
                {
                    anyChildChanged = true;
                }
                newChildren.Add(processedChild);
            }

            if (!anyChildChanged)
            {
                return n;
            }
            else
            {
                return _command.CreateNode(n.Op, newChildren);
            }
        }

        #region Unsupported node types

        protected override Node VisitRelOpDefault(RelOp op, Node n)
        {
            return null;
        }

        public override Node Visit(AggregateOp op, Node n)
        {
            return null;
        }

        public override Node Visit(CollectOp op, Node n)
        {
            return null;
        }

        public override Node Visit(ElementOp op, Node n)
        {
            return null;
        }

        #endregion

        #endregion
    }
}
