// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Helper class to track the aggregate nodes that are candidates to be
    /// pushed into the definingGroupByNode.
    /// </summary>
    internal class GroupAggregateVarInfo
    {
        #region Private Fields

        private readonly Node _definingGroupByNode;
        private HashSet<KeyValuePair<Node, Node>> _candidateAggregateNodes;
        private readonly Var _groupAggregateVar;

        #endregion

        #region Constructor

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="defingingGroupNode"> The GroupIntoOp node </param>
        /// <param name="groupAggregateVar"> </param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        internal GroupAggregateVarInfo(Node defingingGroupNode, Var groupAggregateVar)
        {
            _definingGroupByNode = defingingGroupNode;
            _groupAggregateVar = groupAggregateVar;
        }

        #endregion

        #region 'Public' Properties

        /// <summary>
        /// Each key value pair represents a candidate aggregate.
        /// The key is the function aggregate subtree and the value is a 'template' of translation of the
        /// function aggregate's argument over the var representing the group aggregate.
        /// A valid candidate has an argument that does not have any external references
        /// except for the group aggregate corresponding to the DefiningGroupNode.
        /// </summary>
        internal HashSet<KeyValuePair<Node, Node>> CandidateAggregateNodes
        {
            get
            {
                if (_candidateAggregateNodes == null)
                {
                    _candidateAggregateNodes = new HashSet<KeyValuePair<Node, Node>>();
                }
                return _candidateAggregateNodes;
            }
        }

        /// <summary>
        /// Are there are agregates that are candidates to be pushed into the DefiningGroupNode
        /// </summary>
        internal bool HasCandidateAggregateNodes
        {
            get { return (_candidateAggregateNodes != null && _candidateAggregateNodes.Count != 0); }
        }

        /// <summary>
        /// The GroupIntoOp node that this GroupAggregateVarInfo represents
        /// </summary>
        internal Node DefiningGroupNode
        {
            get { return _definingGroupByNode; }
        }

        internal Var GroupAggregateVar
        {
            get { return _groupAggregateVar; }
        }

        #endregion
    }
}
