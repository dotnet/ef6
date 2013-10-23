// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

 // using System.Text.RegularExpressions;

namespace Microsoft.Data.Entity.Design.Model.Visitor
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;

    internal class SearchVisitor : Visitor
    {
        // delegate defining which property of the EFElement to search
        internal delegate string EFElementTextToSearch(EFElement element);

        private readonly string _targetString;
        private readonly EFElementTextToSearch _elementTextToSearch;
        private readonly HashSet<EFElement> _objectsSatisfyingSearch = new HashSet<EFElement>();

        internal SearchVisitor(string targetString, EFElementTextToSearch elementTextToSearch)
        {
            Debug.Assert(null != targetString, typeof(SearchVisitor).Name + " requires non-null targetString");
            _targetString = targetString;

            Debug.Assert(null != elementTextToSearch, typeof(SearchVisitor).Name + " requires non-null elementTextToSearch");
            _elementTextToSearch = elementTextToSearch;
        }

        internal override void Visit(IVisitable visitable)
        {
            var efElement = visitable as EFElement;

            // search all EFElements
            if (null != efElement)
            {
                var stringToSearch = _elementTextToSearch(efElement);
                if (StringToSearchContainsTargetString(stringToSearch, _targetString))
                {
                    _objectsSatisfyingSearch.Add(efElement);
                }
            }
        }

        internal static bool StringToSearchContainsTargetString(string stringToSearch, string targetString)
        {
            if (string.IsNullOrEmpty(stringToSearch))
            {
                Debug.Assert(false, "stringToSearch is null or empty");
                return false;
            }

            if (string.IsNullOrEmpty(targetString))
            {
                Debug.Assert(false, "targetString is null or empty");
                return false;
            }

            if (stringToSearch.ToUpper(CultureInfo.CurrentCulture).Contains(targetString.ToUpper(CultureInfo.CurrentCulture)))
            {
                return true;
            }

            return false;
        }

        internal void ResetSearchResults()
        {
            _objectsSatisfyingSearch.Clear();
        }

        internal IEnumerable<EFElement> SearchResults
        {
            get { return _objectsSatisfyingSearch; }
        }
    }
}
