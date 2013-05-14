// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.InternalTrees;

    /// <summary>
    ///     Manages refereces to groupAggregate variables.
    /// </summary>
    internal class GroupAggregateVarInfoManager
    {
        #region Private state

        private readonly Dictionary<Var, GroupAggregateVarRefInfo> _groupAggregateVarRelatedVarToInfo =
            new Dictionary<Var, GroupAggregateVarRefInfo>();

        private Dictionary<Var, Dictionary<EdmMember, GroupAggregateVarRefInfo>> _groupAggregateVarRelatedVarPropertyToInfo;
        private readonly HashSet<GroupAggregateVarInfo> _groupAggregateVarInfos = new HashSet<GroupAggregateVarInfo>();

        #endregion

        #region Public Surface

        /// <summary>
        ///     Get all the groupAggregateVarInfos
        /// </summary>
        internal IEnumerable<GroupAggregateVarInfo> GroupAggregateVarInfos
        {
            get { return _groupAggregateVarInfos; }
        }

        /// <summary>
        ///     Add an entry that var is a computation represented by the computationTemplate
        ///     over the var represented by the given groupAggregateVarInfo
        /// </summary>
        internal void Add(Var var, GroupAggregateVarInfo groupAggregateVarInfo, Node computationTemplate, bool isUnnested)
        {
            _groupAggregateVarRelatedVarToInfo.Add(
                var, new GroupAggregateVarRefInfo(groupAggregateVarInfo, computationTemplate, isUnnested));
            _groupAggregateVarInfos.Add(groupAggregateVarInfo);
        }

        /// <summary>
        ///     Add an entry that the given property of the given var is a computation represented
        ///     by the computationTemplate over the var represented by the given groupAggregateVarInfo
        /// </summary>
        internal void Add(
            Var var, GroupAggregateVarInfo groupAggregateVarInfo, Node computationTemplate, bool isUnnested, EdmMember property)
        {
            if (property == null)
            {
                Add(var, groupAggregateVarInfo, computationTemplate, isUnnested);
                return;
            }
            if (_groupAggregateVarRelatedVarPropertyToInfo == null)
            {
                _groupAggregateVarRelatedVarPropertyToInfo = new Dictionary<Var, Dictionary<EdmMember, GroupAggregateVarRefInfo>>();
            }
            Dictionary<EdmMember, GroupAggregateVarRefInfo> varPropertyDictionary;
            if (!_groupAggregateVarRelatedVarPropertyToInfo.TryGetValue(var, out varPropertyDictionary))
            {
                varPropertyDictionary = new Dictionary<EdmMember, GroupAggregateVarRefInfo>();
                _groupAggregateVarRelatedVarPropertyToInfo.Add(var, varPropertyDictionary);
            }
            varPropertyDictionary.Add(property, new GroupAggregateVarRefInfo(groupAggregateVarInfo, computationTemplate, isUnnested));

            // Note: The following line is not necessary with the current usage pattern, this method is 
            // never called with a new groupAggregateVarInfo thus it is a no-op.
            _groupAggregateVarInfos.Add(groupAggregateVarInfo);
        }

        /// <summary>
        ///     Gets the groupAggregateVarRefInfo representing the definition of the given var over
        ///     a group aggregate var if any.
        /// </summary>
        internal bool TryGetReferencedGroupAggregateVarInfo(Var var, out GroupAggregateVarRefInfo groupAggregateVarRefInfo)
        {
            return _groupAggregateVarRelatedVarToInfo.TryGetValue(var, out groupAggregateVarRefInfo);
        }

        /// <summary>
        ///     Gets the groupAggregateVarRefInfo representing the definition of the given property of the given
        ///     var over a group aggregate var if any.
        /// </summary>
        internal bool TryGetReferencedGroupAggregateVarInfo(
            Var var, EdmMember property, out GroupAggregateVarRefInfo groupAggregateVarRefInfo)
        {
            if (property == null)
            {
                return TryGetReferencedGroupAggregateVarInfo(var, out groupAggregateVarRefInfo);
            }

            Dictionary<EdmMember, GroupAggregateVarRefInfo> varPropertyDictionary;
            if (_groupAggregateVarRelatedVarPropertyToInfo == null
                || !_groupAggregateVarRelatedVarPropertyToInfo.TryGetValue(var, out varPropertyDictionary))
            {
                groupAggregateVarRefInfo = null;
                return false;
            }
            return varPropertyDictionary.TryGetValue(property, out groupAggregateVarRefInfo);
        }

        #endregion
    }
}
