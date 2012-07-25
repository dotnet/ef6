// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;

    /// <summary>
    /// Wrapper around OpCopier to keep track of the defining subtrees
    /// of collection vars defined in the subtree being returned as a copy.
    /// </summary>
    internal class OpCopierTrackingCollectionVars : OpCopier
    {
        #region Private State

        private readonly Dictionary<Var, Node> m_newCollectionVarDefinitions = new Dictionary<Var, Node>();

        #endregion

        #region Private Constructor

        private OpCopierTrackingCollectionVars(Command cmd)
            : base(cmd)
        {
        }

        #endregion

        #region Public Surface

        /// <summary>
        /// Equivalent to OpCopier.Copy, only in addition it keeps track of the defining subtrees
        /// of collection vars defined in the subtree rooted at the copy of the input node n.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="n"></param>
        /// <param name="varMap"></param>
        /// <param name="newCollectionVarDefinitions"></param>
        /// <returns></returns>
        internal static Node Copy(Command cmd, Node n, out VarMap varMap, out Dictionary<Var, Node> newCollectionVarDefinitions)
        {
            var oc = new OpCopierTrackingCollectionVars(cmd);
            var newNode = oc.CopyNode(n);
            varMap = oc.m_varMap;
            newCollectionVarDefinitions = oc.m_newCollectionVarDefinitions;
            return newNode;
        }

        #endregion

        #region Visitor Members

        /// <summary>
        /// Tracks the collection vars after calling the base implementation
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(MultiStreamNestOp op, Node n)
        {
            var result = base.Visit(op, n);
            var newOp = (MultiStreamNestOp)result.Op;

            for (var i = 0; i < newOp.CollectionInfo.Count; i++)
            {
                m_newCollectionVarDefinitions.Add(newOp.CollectionInfo[i].CollectionVar, result.Children[i + 1]);
            }
            return result;
        }

        #endregion
    }
}
