// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model.Visitor;
    using Microsoft.Data.Tools.XmlDesignerBase;

    /// <summary>
    ///     This class represents the ExplorerSearchResults context item kept in sorted order.
    /// </summary>
    internal class ExplorerSearchResults : ContextItem
    {
        private List<ExplorerEFElement> _results;
        private ExplorerEFElement _previousSearchResultItem;
        private int _previousSearchResultItemIndex = -1;
        private ExplorerEFElement _nextSearchResultItem;
        private int _nextSearchResultItemIndex = -1;
        private bool _currentSelectionIsInResults;
        private string _targetString;
        private SearchVisitor.EFElementTextToSearch _elementTextToSearch;

        // Must have public constructor to allow creation by reflection 
        // in EditingContext.GetValue()

        internal void Reset()
        {
            if (null != _results)
            {
                foreach (var explorerElement in _results)
                {
                    explorerElement.IsInSearchResults = false;
                }
                _results.Clear();
            }
            _previousSearchResultItem = null;
            _previousSearchResultItemIndex = -1;
            _nextSearchResultItem = null;
            _nextSearchResultItemIndex = -1;
            _currentSelectionIsInResults = false;
            _targetString = null;
            _elementTextToSearch = null;
        }

        internal override Type ItemType
        {
            get { return typeof(ExplorerSearchResults); }
        }

        internal void RecalculateResults(EditingContext context, ModelSearchResults modelSearchResults)
        {
            // reset all old IsInSearchResults values
            foreach (var oldSearchResult in Results)
            {
                oldSearchResult.IsInSearchResults = false;
            }

            // now recalculate the results based on the new ModelSearchResults
            Reset();
            _targetString = modelSearchResults.TargetString;
            _elementTextToSearch = modelSearchResults.ElementTextToSearch;
            var modelToExplorerModelXRef = ModelToExplorerModelXRef.GetModelToBrowserModelXRef(context);
            if (null != modelToExplorerModelXRef)
            {
                // add all the ExplorerEFElements to _results
                foreach (var result in modelSearchResults.Results)
                {
                    var resultsExplorerElement = modelToExplorerModelXRef.GetExisting(result);
                    if (resultsExplorerElement != null)
                    {
                        resultsExplorerElement.IsInSearchResults = true;
                        _results.Add(resultsExplorerElement);
                    }
                }

                // now sort _results according to the order they appear in the Explorer
                SortResults();
            }
        }

        /// <summary>
        ///     Recalculate the Next and Previous items based on the passed in current selection
        ///     which may or may not be in the Search Results.
        ///     Note: if currentlySelectedItem _is_ in Search Results then Previous and Next items
        ///     will bracket it (i.e. neither Next nor Previous will be the currently selected item).
        /// </summary>
        internal void RecalculateNextAndPrevious(ExplorerEFElement currentlySelectedItem)
        {
            if (null == _results)
            {
                Debug.Assert(false, "RecalculateNextAndPrevious called when _results is null");
                return;
            }

            // recalculate, note that _results is in increasing order so can perform
            // binary search to find the item equal to or just above this element
            var indexOfCurrentSelectionInResults = _results.BinarySearch(currentlySelectedItem, ExplorerHierarchyComparer.Instance);
            if (indexOfCurrentSelectionInResults >= 0)
            {
                // currentlySelectedItem is in _results list
                _currentSelectionIsInResults = true;

                if (indexOfCurrentSelectionInResults > 0)
                {
                    _previousSearchResultItemIndex = indexOfCurrentSelectionInResults - 1;
                    _previousSearchResultItem = _results[_previousSearchResultItemIndex];
                }
                else
                {
                    _previousSearchResultItemIndex = -1;
                    _previousSearchResultItem = null;
                }

                if ((indexOfCurrentSelectionInResults + 1) < _results.Count)
                {
                    _nextSearchResultItemIndex = indexOfCurrentSelectionInResults + 1;
                    _nextSearchResultItem = _results[_nextSearchResultItemIndex];
                }
                else
                {
                    _nextSearchResultItemIndex = -1;
                    _nextSearchResultItem = null;
                }
            }
            else
            {
                // currentlySelectedItem is _not_ in _results list
                _currentSelectionIsInResults = false;

                var indexOfNextGreatestItem = ~indexOfCurrentSelectionInResults;
                if (indexOfNextGreatestItem == _results.Count)
                {
                    // there is no item "greater" than selected item in list
                    _nextSearchResultItemIndex = -1;
                    _nextSearchResultItem = null;
                }
                else
                {
                    _nextSearchResultItemIndex = indexOfNextGreatestItem;
                    _nextSearchResultItem = _results[_nextSearchResultItemIndex];
                }

                if (_nextSearchResultItemIndex > 0)
                {
                    // current selection is not in list and _nextSearchResultItemIndex > 0
                    // so _previousSearchResultItemIndex must be the next item down the list
                    _previousSearchResultItemIndex = _nextSearchResultItemIndex - 1;
                    _previousSearchResultItem = _results[_previousSearchResultItemIndex];
                }
                else if (_nextSearchResultItemIndex == 0)
                {
                    // _nextSearchResultItemIndex == 0, so _previousSearchResultItem does not exist
                    _previousSearchResultItemIndex = -1;
                    _previousSearchResultItem = null;
                }
                else if (_results.Count > 0)
                {
                    // there is no _nextSearchResultItem, so current selection is "greater" than
                    // all items in the list, so _previousSearchResultItem is the last item in the list
                    _previousSearchResultItemIndex = _results.Count - 1;
                    _previousSearchResultItem = _results[_previousSearchResultItemIndex];
                }
                else
                {
                    // the list is empty so both _nextSearchResultItem and 
                    // _previousSearchResultItem do not exist
                    _previousSearchResultItemIndex = -1;
                    _previousSearchResultItem = null;
                }
            }
        }

        /// <summary>
        ///     Get next Search Result and update Next and Previous items
        /// </summary>
        internal ExplorerEFElement SelectNextSearchResult()
        {
            var itemToBeReturned = _nextSearchResultItem;
            if (null != _nextSearchResultItem)
            {
                if (_currentSelectionIsInResults)
                {
                    if (null == _previousSearchResultItem)
                    {
                        // there was no previous search item - make the previous item the start of the list
                        _previousSearchResultItemIndex = 0;
                    }
                    else
                    {
                        _previousSearchResultItemIndex++;
                    }
                    _previousSearchResultItem = _results[_previousSearchResultItemIndex];
                }
                else
                {
                    // the currently selected item is now in the results
                    // but Previous does not need to be updated
                    _currentSelectionIsInResults = true;
                }

                _nextSearchResultItemIndex++;
                if (_nextSearchResultItemIndex >= _results.Count)
                {
                    _nextSearchResultItem = null;
                    _nextSearchResultItemIndex = -1;
                }
                else
                {
                    _nextSearchResultItem = _results[_nextSearchResultItemIndex];
                }
            }

            return itemToBeReturned;
        }

        /// <summary>
        ///     Get previous Search Result and update Next and Previous pointers
        /// </summary>
        internal ExplorerEFElement SelectPreviousSearchResult()
        {
            var itemToBeReturned = _previousSearchResultItem;
            if (null != _previousSearchResultItem)
            {
                if (_currentSelectionIsInResults)
                {
                    if (null == _nextSearchResultItem)
                    {
                        // there was no next search item - make the next item the end of the list
                        _nextSearchResultItemIndex = _results.Count - 1;
                    }
                    else
                    {
                        _nextSearchResultItemIndex--;
                    }
                    _nextSearchResultItem = _results[_nextSearchResultItemIndex];
                }
                else
                {
                    // the currently selected item is now in the results
                    // but Next does not need to be updated
                    _currentSelectionIsInResults = true;
                }

                _previousSearchResultItemIndex--;
                if (_previousSearchResultItemIndex < 0)
                {
                    _previousSearchResultItem = null;
                    _previousSearchResultItemIndex = -1;
                }
                else
                {
                    _previousSearchResultItem = _results[_previousSearchResultItemIndex];
                }
            }

            return itemToBeReturned;
        }

        internal bool CanGoToNextSearchResult
        {
            get { return null != _nextSearchResultItem; }
        }

        internal bool CanGoToPreviousSearchResult
        {
            get { return null != _previousSearchResultItem; }
        }

        internal IEnumerable<ExplorerEFElement> Results
        {
            get
            {
                EnsureResults();
                return _results;
            }
        }

        internal int Count
        {
            get
            {
                EnsureResults();
                return _results.Count;
            }
        }

        /// <summary>
        ///     If the element being removed is part of the Search Results then
        ///     removes it from the Search Results list
        /// </summary>
        /// <param name="elementToBeRemoved">element to be removed</param>
        /// <returns>whether removal was successful</returns>
        internal bool RemoveElementFromSearchResults(ExplorerEFElement elementToBeRemoved)
        {
            if (null == elementToBeRemoved)
            {
                Debug.Fail(
                    typeof(ExplorerSearchResults).Name + ".RemoveElementFromSearchResults() received null elementToBeRemoved");
                return false;
            }

            if (elementToBeRemoved.IsInSearchResults)
            {
                elementToBeRemoved.IsInSearchResults = false;
                return _results.Remove(elementToBeRemoved);
            }

            return false;
        }

        /// <summary>
        ///     If the element being renamed is part of the Search Results then
        ///     a) element may no longer be in Search Results and b) if it is
        ///     then the elements may need to be resorted
        /// </summary>
        /// <param name="elementBeingRenamed">element being renamed</param>
        /// <returns>true if an element being renamed was in the Search Results</returns>
        internal bool OnRenameElement(ExplorerEFElement elementBeingRenamed)
        {
            if (null == elementBeingRenamed)
            {
                Debug.Assert(false, typeof(ExplorerSearchResults).Name + ".RenameElement() received null elementBeingRenamed");
                return false;
            }

            if (elementBeingRenamed.IsInSearchResults)
            {
                if (!MatchesTargetString(elementBeingRenamed))
                {
                    // elementBeingRenamed no longer matches Target String - remove from results
                    elementBeingRenamed.IsInSearchResults = false;
                    _results.Remove(elementBeingRenamed);
                }

                // need to re-sort whether element is removed from Search Results or not
                // because the rename may have changed the order of the Results
                SortResults();
                return true;
            }

            return false;
        }

        internal static ExplorerSearchResults GetExplorerSearchResults(EditingContext context)
        {
            var explorerSearchResults = context.Items.GetValue<ExplorerSearchResults>();
            if (null == explorerSearchResults)
            {
                explorerSearchResults = new ExplorerSearchResults();
                context.Items.SetValue(explorerSearchResults);
            }

            return explorerSearchResults;
        }

        private bool MatchesTargetString(ExplorerEFElement explorerElement)
        {
            if (null == _targetString)
            {
                Debug.Assert(false, "MatchesTargetString() for explorerElement has null _targetString");
                return false;
            }

            if (null == _elementTextToSearch)
            {
                Debug.Assert(false, "MatchesTargetString() has null _elementTextToSearch delegate");
                return false;
            }

            if (null == explorerElement)
            {
                Debug.Assert(false, "MatchesTargetString() was passed null explorerElement");
                return false;
            }

            if (null == explorerElement.ModelItem)
            {
                Debug.Fail(
                    "MatchesTargetString() was passed explorerElement with null Model item (name = " + explorerElement.Name + ")");
                return false;
            }

            var stringToMatch = _elementTextToSearch(explorerElement.ModelItem);
            return SearchVisitor.StringToSearchContainsTargetString(stringToMatch, _targetString);
        }

        private void SortResults()
        {
            EnsureResults();

            // sort _results according to the order they appear in the Explorer
            _results.Sort(ExplorerHierarchyComparer.Instance);
        }

        private void EnsureResults()
        {
            if (null == _results)
            {
                _results = new List<ExplorerEFElement>();
            }
        }

        public override string ToString()
        {
            EnsureResults();

            var individualResultsAsStringBuilder = new StringBuilder();
            var index = 0;
            foreach (var explorerElement in Results)
            {
                if (0 == index)
                {
                    individualResultsAsStringBuilder.AppendLine();
                }
                individualResultsAsStringBuilder.AppendLine(
                    string.Format(
                        CultureInfo.CurrentCulture, Resources.ExplorerSearchResults_IndividualFormat, index++, explorerElement.Name,
                        explorerElement.GetType().Name));
            }

            return string.Format(
                CultureInfo.CurrentCulture, Resources.ExplorerSearchResults_OverallFormat, _results.Count, individualResultsAsStringBuilder,
                _previousSearchResultItemIndex, _nextSearchResultItemIndex, _currentSelectionIsInResults);
        }
    }
}
