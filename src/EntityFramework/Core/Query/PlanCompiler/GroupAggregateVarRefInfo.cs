// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Data.Entity.Core.Query.InternalTrees;

    /// <summary>
    ///     Helper class to track usage of GroupAggregateVarInfo
    ///     It represents the usage of a single GroupAggregateVar.
    ///     The usage is defined by the computation, it should be a subree whose only
    ///     external reference is the group var represented by the GroupAggregateVarInfo.
    /// </summary>
    internal class GroupAggregateVarRefInfo
    {
        #region Private fields

        private readonly Node _computation;
        private readonly GroupAggregateVarInfo _groupAggregateVarInfo;
        private readonly bool _isUnnested;

        #endregion

        #region Constructor

        /// <summary>
        ///     Public constructor
        /// </summary>
        internal GroupAggregateVarRefInfo(GroupAggregateVarInfo groupAggregateVarInfo, Node computation, bool isUnnested)
        {
            _groupAggregateVarInfo = groupAggregateVarInfo;
            _computation = computation;
            _isUnnested = isUnnested;
        }

        #endregion

        #region 'Public' Properties

        /// <summary>
        ///     Subtree whose only external reference is
        ///     the group var represented by the GroupAggregateVarInfo
        /// </summary>
        internal Node Computation
        {
            get { return _computation; }
        }

        /// <summary>
        ///     The GroupAggregateVarInfo (possibly) referenced by the computation
        /// </summary>
        internal GroupAggregateVarInfo GroupAggregateVarInfo
        {
            get { return _groupAggregateVarInfo; }
        }

        /// <summary>
        ///     Is the computation over unnested group aggregate var
        /// </summary>
        internal bool IsUnnested
        {
            get { return _isUnnested; }
        }

        #endregion
    }
}
