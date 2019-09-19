// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;

    // <summary>
    // The VarRemapper is a utility class that can be used to "remap" Var references
    // in a node, or a subtree.
    // </summary>
    internal class VarRemapper : BasicOpVisitor
    {
        #region Private state

        private readonly Dictionary<Var, Var> m_varMap;
        protected readonly Command m_command;

        #endregion

        #region Constructors

        // <summary>
        // Internal constructor
        // </summary>
        // <param name="command"> Current iqt command </param>
        internal VarRemapper(Command command)
            : this(command, new Dictionary<Var, Var>())
        {
        }

        // <summary>
        // Internal constructor
        // </summary>
        // <param name="command"> Current iqt command </param>
        // <param name="varMap"> Var map to be used </param>
        internal VarRemapper(Command command, Dictionary<Var, Var> varMap)
        {
            m_command = command;
            m_varMap = varMap;
        }

        #endregion

        #region Public surface

        // <summary>
        // Add a mapping for "oldVar" - when the replace methods are invoked, they
        // will replace all references to "oldVar" by "newVar"
        // </summary>
        // <param name="oldVar"> var to replace </param>
        // <param name="newVar"> the replacement var </param>
        internal void AddMapping(Var oldVar, Var newVar)
        {
            m_varMap[oldVar] = newVar;
        }

        // <summary>
        // Update vars in just this node (and not the entire subtree)
        // Does *not* recompute the nodeinfo - there are at least some consumers of this
        // function that do not want the recomputation - transformation rules, for example
        // </summary>
        // <param name="node"> current node </param>
        internal virtual void RemapNode(Node node)
        {
            if (m_varMap.Count == 0)
            {
                return;
            }
            VisitNode(node);
        }

        // <summary>
        // Update vars in this subtree. Recompute the nodeinfo along the way
        // </summary>
        // <param name="subTree"> subtree to "remap" </param>
        internal virtual void RemapSubtree(Node subTree)
        {
            if (m_varMap.Count == 0)
            {
                return;
            }

            foreach (var chi in subTree.Children)
            {
                RemapSubtree(chi);
            }

            RemapNode(subTree);
            m_command.RecomputeNodeInfo(subTree);
        }

        // <summary>
        // Produce a new remapped varList
        // </summary>
        // <returns> remapped varList </returns>
        internal VarList RemapVarList(VarList varList)
        {
            return Command.CreateVarList(MapVars(varList));
        }

        // <summary>
        // Remap the given varList using the given varMap
        // </summary>
        internal static VarList RemapVarList(Command command, Dictionary<Var, Var> varMap, VarList varList)
        {
            var varRemapper = new VarRemapper(command, varMap);
            return varRemapper.RemapVarList(varList);
        }

        #endregion

        #region Private methods

        // <summary>
        // Get the mapping for a Var - returns the var itself, mapping was found
        // </summary>
        private Var Map(Var v)
        {
            Var newVar;
            while (true)
            {
                if (!m_varMap.TryGetValue(v, out newVar))
                {
                    return v;
                }
                v = newVar;
            }
        }

        private IEnumerable<Var> MapVars(IEnumerable<Var> vars)
        {
            foreach (var v in vars)
            {
                yield return Map(v);
            }
        }

        private void Map(VarVec vec)
        {
            var newVec = m_command.CreateVarVec(MapVars(vec));
            vec.InitFrom(newVec);
        }

        private void Map(VarList varList)
        {
            var newList = Command.CreateVarList(MapVars(varList));
            varList.Clear();
            varList.AddRange(newList);
        }

        private void Map(VarMap varMap)
        {
            var newVarMap = new VarMap();
            foreach (var kv in varMap)
            {
                var newVar = Map(kv.Value);
                newVarMap.Add(kv.Key, newVar);
            }
            varMap.Clear();
            foreach (var kv in newVarMap)
            {
                varMap.Add(kv.Key, kv.Value);
            }
        }

        private void Map(List<SortKey> sortKeys)
        {
            var sortVars = m_command.CreateVarVec();
            var hasDuplicates = false;

            // 
            // Map each var in the sort list. Remapping may introduce duplicates, and
            // we should get rid of duplicates, since sql doesn't like them
            //
            foreach (var sk in sortKeys)
            {
                sk.Var = Map(sk.Var);
                if (sortVars.IsSet(sk.Var))
                {
                    hasDuplicates = true;
                }
                sortVars.Set(sk.Var);
            }

            //
            // Get rid of any duplicates
            //
            if (hasDuplicates)
            {
                var newSortKeys = new List<SortKey>(sortKeys);
                sortKeys.Clear();
                sortVars.Clear();
                foreach (var sk in newSortKeys)
                {
                    if (!sortVars.IsSet(sk.Var))
                    {
                        sortKeys.Add(sk);
                    }
                    sortVars.Set(sk.Var);
                }
            }
        }

        #region VisitorMethods

        // <summary>
        // Default visitor for a node - does not visit the children
        // The reason we have this method is because the default VisitDefault
        // actually visits the children, and we don't want to do that
        // </summary>
        protected override void VisitDefault(Node n)
        {
            // Do nothing. 
        }

        #region ScalarOps

        public override void Visit(VarRefOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            var newVar = Map(op.Var);
            if (newVar != op.Var)
            {
                n.Op = m_command.CreateVarRefOp(newVar);
            }
        }

        #endregion

        #region AncillaryOps

        #endregion

        #region PhysicalOps

        protected override void VisitNestOp(NestBaseOp op, Node n)
        {
            throw new NotSupportedException();
        }

        public override void Visit(PhysicalProjectOp op, Node n)
        {
            VisitPhysicalOpDefault(op, n);
            Map(op.Outputs);

            var newColumnMap = (SimpleCollectionColumnMap)ColumnMapTranslator.Translate(op.ColumnMap, m_varMap);
            n.Op = m_command.CreatePhysicalProjectOp(op.Outputs, newColumnMap);
        }

        #endregion

        #region RelOps

        protected override void VisitGroupByOp(GroupByBaseOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            Map(op.Outputs);
            Map(op.Keys);
        }

        public override void Visit(GroupByIntoOp op, Node n)
        {
            VisitGroupByOp(op, n);
            Map(op.Inputs);
        }

        public override void Visit(DistinctOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            Map(op.Keys);
        }

        public override void Visit(ProjectOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            Map(op.Outputs);
        }

        public override void Visit(UnnestOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            var newVar = Map(op.Var);
            if (newVar != op.Var)
            {
                n.Op = m_command.CreateUnnestOp(newVar, op.Table);
            }
        }

        protected override void VisitSetOp(SetOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            Map(op.VarMap[0]);
            Map(op.VarMap[1]);
        }

        protected override void VisitSortOp(SortBaseOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            Map(op.Keys);
        }

        #endregion

        #endregion

        #endregion
    }
}
