// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI
{
    using System.Collections.Generic;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Visitor;

    /// <summary>
    ///     This class represents the ModelSearchResults context item.
    /// </summary>
    internal class ModelSearchResults
    {
        private IEnumerable<EFElement> _pendingQuery;
        private IEnumerable<EFElement> _results;
        private string _searchCriteria;
        private string _action;
        private string _targetString;

        internal void Reset()
        {
            _pendingQuery = null;
            _results = null;
            _searchCriteria = null;
            _action = null;
            _targetString = null;
        }

        internal string SearchCriteria
        {
            get { return _searchCriteria; }
            set { _searchCriteria = value; }
        }

        internal string TargetString
        {
            get { return _targetString; }
            set { _targetString = value; }
        }

        internal SearchVisitor.EFElementTextToSearch ElementTextToSearch { get; set; }

        internal string Action
        {
            get { return _action; }
            set { _action = value; }
        }

        internal bool HasResults
        {
            get { return Results.GetEnumerator().MoveNext(); }
        }

        internal IEnumerable<EFElement> Results
        {
            get
            {
                if (_results == null)
                {
                    if (_pendingQuery != null)
                    {
                        _results = new List<EFElement>(_pendingQuery);
                    }
                    else
                    {
                        return new List<EFElement>();
                    }
                }
                return _results;
            }
            set
            {
                _pendingQuery = value;
                _results = null;
            }
        }
    }
}
