// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Tools.XmlDesignerBase;

    internal class ExplorerHierarchyComparer : IComparer<ExplorerEFElement>
    {
        private static ExplorerHierarchyComparer _instance;

        internal static ExplorerHierarchyComparer Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new ExplorerHierarchyComparer();
                }

                return _instance;
            }
        }

        private ExplorerHierarchyComparer()
        {
            // constructor made private to implement singleton pattern
        }

        public int Compare(ExplorerEFElement x, ExplorerEFElement y)
        {
            return ExplorerEFElement.HierarchyCompare(x, y);
        }
    }

    internal abstract class ExplorerEFElement : INotifyPropertyChanged
    {
        internal static readonly string IsKeyPropertyID = "IsKeyProperty";

        // we need to provide WPF with a consistent collection or it gets confused
        protected class ExplorerEFElementCollection : ObservableCollection<ExplorerEFElement>, ICollection<ExplorerEFElement>
        {
            void ICollection<ExplorerEFElement>.Add(ExplorerEFElement child)
            {
                if (child != null)
                {
                    // add as a child only if not null
                    base.Add(child);
                }
            }
        }

        protected interface ISortableCollection
        {
            void Sort();
        }

        // class to be used for the typed children of ExplorerEFElement classes
        // maintains the correct ordering of the children
        protected class TypedChildList<T> : ISortableCollection
            where T : ExplorerEFElement
        {
            private readonly List<T> _typedList = new List<T>();
            private readonly IComparer<T> _comparer;

            internal TypedChildList()
            {
                _comparer = new ExplorerEFElementNameComparer<T>();
            }

            internal TypedChildList(IComparer<T> comparer)
            {
                _comparer = comparer;
            }

            internal IList<T> ChildList
            {
                get { return _typedList; }
            }

            internal int Count
            {
                get { return _typedList.Count; }
            }

            void ISortableCollection.Sort()
            {
                _typedList.Sort(_comparer);
            }

            // InsertIntoList() uses the current Comparer - which can be changed. 
            // (Note: we can't use a SortedList as unable to change Comparer)
            // We assume the list is currently in order
            internal void Insert(T child)
            {
                // list is sorted, so do a binary search to find the location to insert
                var index = _typedList.BinarySearch(child, _comparer);
                if (index < 0)
                {
                    // the  value index is the bitwise-complement of
                    // the index of the first item in the list greater than child, so 
                    // we can simply insert at ~index
                    index = ~index;
                }
                _typedList.Insert(index, child);
                child.ContainingCollection = this;
            }

            internal int Remove(T child)
            {
                var indexOfChild = IndexOf(child);
                if (indexOfChild >= 0)
                {
                    _typedList.RemoveAt(indexOfChild);
                }

                return indexOfChild;
            }

            internal int IndexOf(T child)
            {
                var indexOfChild = _typedList.BinarySearch(child, _comparer);
                if (indexOfChild < 0)
                {
                    return indexOfChild;
                }
                else
                {
                    // if multiple entries have the same sort value, this index may be in the middle
                    // of the range.  Here we need account for that
                    var found = false;

                    // first walk to the beginning of this value range according to _comparer
                    while (indexOfChild > 0
                           && _comparer.Compare(child, _typedList[indexOfChild - 1]) == 0)
                    {
                        --indexOfChild;
                    }

                    // now walk forward looking for the specific node we want
                    while (indexOfChild < _typedList.Count
                           && _comparer.Compare(child, _typedList[indexOfChild]) == 0)
                    {
                        if (child == _typedList[indexOfChild])
                        {
                            found = true;
                            break;
                        }
                        else
                        {
                            ++indexOfChild;
                        }
                    }

                    if (found)
                    {
                        return indexOfChild;
                    }
                    else
                    {
                        // this will return the 1's complement of the first entry greater than the node
                        return ~indexOfChild;
                    }
                }
            }
        }

        // class to be used when inserting children to decide where they should live
        protected class ExplorerEFElementNameComparer<T> : IComparer<T>
            where T : ExplorerEFElement
        {
            public int Compare(T element1, T element2)
            {
                ExplorerEFElement efElement1 = element1;
                ExplorerEFElement efElement2 = element2;

                var name1 = efElement1.Name;
                var name2 = efElement2.Name;

                return string.Compare(name1, name2, false, CultureInfo.CurrentCulture);
            }
        }

        protected EditingContext _context;
        protected ExplorerEFElementCollection _children;
        private readonly EFElement _modelItem;
        private ExplorerEFElement _parent;
        private bool _isExpanded;
        private bool _isSelected;
        private bool _isInEditMode;
        private bool _isInSearchResults;
        private bool _hasLoadedFromModel;
        protected string _name;
        private int _ancestorOfSearchResultItemCount;

        internal static int HierarchyCompare(ExplorerEFElement element1, ExplorerEFElement element2)
        {
            var element1PathFromRoot = element1.SelfAndAncestors().Reverse();
            var element2PathFromRoot = element2.SelfAndAncestors().Reverse();

            var element1PathFromRootEnumerator = element1PathFromRoot.GetEnumerator();
            var element2PathFromRootEnumerator = element2PathFromRoot.GetEnumerator();
            ExplorerEFElement jointParent = null;
            ExplorerEFElement path1Child = null;
            ExplorerEFElement path2Child = null;
            while (true)
            {
                // Note: must update _both_ child elements so cannot use && operator in while loop
                if (element1PathFromRootEnumerator.MoveNext())
                {
                    path1Child = element1PathFromRootEnumerator.Current;
                }
                else
                {
                    path1Child = null;
                }
                if (element2PathFromRootEnumerator.MoveNext())
                {
                    path2Child = element2PathFromRootEnumerator.Current;
                }
                else
                {
                    path2Child = null;
                }

                if (null == path1Child
                    || null == path2Child)
                {
                    // have reached end of at least one path
                    break;
                }

                if (path1Child == path2Child)
                {
                    // update joint parent
                    jointParent = path1Child;
                }
                else
                {
                    // have found point where trees differ
                    break;
                }
            }

            if (null == jointParent)
            {
                Debug.Assert(
                    false,
                    "efElement1PathFromRoot and efElement2PathFromRoot have no common ancestor - they should at least have the same root node");
                return 0;
            }

            // now compare
            if (null == path1Child
                && null == path2Child)
            {
                // paths were the same - so ends were the same
                return 0;
            }
            else if (null == path1Child)
            {
                // element1 = jointParent, element2 = child of jointParent
                return -1;
            }
            else if (null == path2Child)
            {
                // element1 = child of jointParent, element2 = jointParent
                return 1;
            }
            else
            {
                // comparing 2 children of jointParent
                return jointParent.CompareChildren(path1Child, path2Child);
            }
        }

        private ISortableCollection ContainingCollection { get; set; }

        /// <summary>
        ///     used to create normal nodes in the ViewModel tree
        /// </summary>
        /// <param name="modelItem">corresponding item in the Model</param>
        /// <param name="parent">parent item in the ViewModel tree</param>
        protected ExplorerEFElement(EditingContext context, EFElement modelItem, ExplorerEFElement parent)
        {
            _context = context;
            _modelItem = modelItem;
            _parent = parent;
            _hasLoadedFromModel = false;
        }

        #region Properties

        public virtual string Name
        {
            get
            {
                if (_name != null)
                {
                    return _name;
                }

                if (ModelItem != null)
                {
                    _name = ModelItem.DisplayName;
                    return _name;
                }

                return string.Empty;
            }

            set { _name = value; }
        }

        public EFElement ModelItem
        {
            get { return _modelItem; }
        }

        public ExplorerEFElement Parent
        {
            get { return _parent; }
            internal set
            {
                if (_parent != value)
                {
                    // update _isInSearchResults and _ancestorOfSearchResultItemCount
                    var oldParent = _parent;
                    if (oldParent != null)
                    {
                        if (IsAncestorOfSearchResultItem)
                        {
                            UpdateIsAncestorOfSearchResultItem(-_ancestorOfSearchResultItemCount);
                        }
                        if (IsInSearchResults)
                        {
                            oldParent.UpdateIsAncestorOfSearchResultItem(-1);
                        }
                    }

                    // set value
                    _parent = value;

                    if (_parent == null)
                    {
                        // treeview doesn't support multiselection, so
                        // when removing item from treeview make sure 
                        // _isSelected is not set when/if it gets re-added
                        _isSelected = false;
                    }
                    else
                    {
                        if (IsInSearchResults)
                        {
                            _parent.UpdateIsAncestorOfSearchResultItem(+1);
                        }
                    }
                }
            }
        }

        public virtual string ToolTipText
        {
            // override this in specific elements if we want to display a tooltip
            // null implies no tooltip
            get { return null; }
        }

        [DebuggerDisplay("Children must not be invoked by Debugger just to show its value in the Autos Window")]
        public IEnumerable<ExplorerEFElement> Children
        {
            get
            {
                EnsureLoaded();
                return _children;
            }
        }

        private void EnsureLoaded()
        {
            // _children is only null if EnsureLoaded has never been called
            // on this object
            if (null == _children)
            {
                _children = new ExplorerEFElementCollection();
            }

            // check if we've ever loaded the ViewModel state from the model
            // if not then load here
            if (!_hasLoadedFromModel)
            {
                LoadChildrenCollection();
                _hasLoadedFromModel = true;
            }
        }

        protected virtual void LoadChildrenCollection()
        {
            ClearChildren();
            LoadChildrenFromModel();
            LoadWpfChildrenCollection();
        }

        protected abstract void LoadChildrenFromModel();

        protected abstract void LoadWpfChildrenCollection();

        internal void InsertChildIfLoaded(EFElement efElementToInsert)
        {
            // only insert this child if we have already loaded from model
            // if we have not yet loaded then this child will be picked up
            // next time Children is called
            if (_hasLoadedFromModel)
            {
                InsertChild(efElementToInsert);
            }
        }

        protected virtual void InsertChild(EFElement efElementToInsert)
        {
            throw new InvalidOperationException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.BadInsertBadChildType, efElementToInsert.GetType().FullName, GetType().FullName));
        }

        internal virtual void RemoveChildIfLoaded(EFElement efChildElementToRemove)
        {
            // only remove this child if we have already loaded from model
            // if we have not yet loaded then this child (or rather lack thereof)
            // will be picked up next time Children is called
            if (_hasLoadedFromModel)
            {
                var xref = ModelToExplorerModelXRef.GetModelToBrowserModelXRef(_context);
                var explorerElement = xref.GetExisting(efChildElementToRemove);
                var explorerSearchResults = ExplorerSearchResults.GetExplorerSearchResults(_context);

                if (explorerElement != null)
                {
                    if (RemoveChild(explorerElement))
                    {
                        if (_children.Contains(explorerElement))
                        {
                            explorerElement.ClearChildren();

                            // if in Search Results remove explorerElement from them
                            explorerSearchResults.RemoveElementFromSearchResults(explorerElement);

                            xref.Remove(efChildElementToRemove);
                            explorerElement.Parent = null;
                            _children.Remove(explorerElement);
                            return;
                        }
                    }

                    // this means efChildElementToRemove is a valid
                    // EFElement which we have mapped in our ViewModel 
                    // but that we're trying to remove it when this 
                    // ExplorerEFElement is not the child element's parent
                    Debug.Assert(
                        false, string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.BadRemoveChildNotParent, explorerElement.Name, Name));
                    return;
                    // TODO: we need to provide a general exception-handling mechanism and replace the above Assert()
                    // by e.g. the excepiton below
                    // throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    //     Resources.BadRemoveChildNotParent, explorerElement.Name, this.Name));
                }

                // otherwise the Model child element does not map to any
                // ViewModel element - this is valid as we do not display
                // all children
                return;
            }
        }

        /// <summary>
        ///     Removes a child element from this object
        /// </summary>
        /// <param name="explorerElement">the element to remove</param>
        /// <returns>true if element was successfully removed, false otherwise</returns>
        protected virtual bool RemoveChild(ExplorerEFElement explorerElement)
        {
            return false;
        }

        /// <summary>
        ///     The Explorer tree does not look exactly like the underlying
        ///     model tree because of intervening ghost nodes. If the node
        ///     represented by this object has children which are ghost nodes
        ///     this method returns the correct ghost node to represent the
        ///     parent for the childElement parameter passed in. Otherwise
        ///     just return self.
        /// </summary>
        /// <param name="childElement">Child node</param>
        /// <returns></returns>
        internal virtual ExplorerEFElement GetParentNodeForElement(EFElement childElement)
        {
            return this;
        }

        /// <summary>
        ///     Compares 2 children (which must be direct children of this node i.e. already
        ///     in its Children collection)
        /// </summary>
        /// <param name="childElement1"></param>
        /// <param name="childElement2"></param>
        /// <returns>
        ///     -1 if childElement1 is in the Children list before childElement2,
        ///     +1 if childElement1 is in the Children list after childElement2, or
        ///     0 if the 2 children are the same
        /// </returns>
        private int CompareChildren(ExplorerEFElement child1, ExplorerEFElement child2)
        {
            // we can sometimes be called before ViewModel has loaded from Model
            // if so need to ensure the children have been loaded.
            EnsureLoaded();

            var child1Index = _children.IndexOf(child1);
            var child2Index = _children.IndexOf(child2);

            if (child1Index < 0
                || child2Index < 0)
            {
                Debug.Assert(child1Index < 0, "CompareChildren(): Could not find child1 named " + child1.Name + " in parent named " + Name);
                Debug.Assert(child2Index < 0, "CompareChildren(): Could not find child2 named " + child2.Name + " in parent named " + Name);
                return 0;
            }
            else
            {
                if (child1Index < child2Index)
                {
                    return -1;
                }
                else if (child1Index > child2Index)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        // This IsExpanded property is two-way bound to the corresponding TreeViewItem IsExpanded
        //  property.
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
        }

        // This IsSelected property is two-way bound to the corresponding TreeViewItem IsSelected
        //  property.
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        // This IsInEditMode property is two-way bound to the corresponding TreeViewItem IsInEditMode
        //  property.
        public bool IsInEditMode
        {
            get { return _isInEditMode; }
            set
            {
                if (_isInEditMode != value)
                {
                    _isInEditMode = value;
                    OnPropertyChanged("IsInEditMode");
                }
            }
        }

        // returns the name of the resource in the ResourceDictionary which is used to lookup the correct icon
        internal abstract string ExplorerImageResourceKeyName { get; }

        // used to decide whether inline editing is enabled for this particular ExplorerEFElement
        public virtual bool IsEditableInline
        {
            get { return false; }
        }

        // This EditableName property is two-way bound to the corresponding TreeViewItem EditableName
        //  property.
        public virtual string EditableName
        {
            get { return Name; }
            set
            {
                if (Name != value)
                {
                    // When name is set, it should exit edit mode. This is required as change in the model loads all WPF controls in EF
                    // This can be removed when entire reload is replaced with reattach mechanism like Table Designer.
                    IsInEditMode = false;

                    var cpc = new CommandProcessorContext(
                        _context,
                        EfiTransactionOriginator.ExplorerWindowOriginatorId, RenameTransactionName, ModelItem.Artifact);

                    if (RenameModelElement(cpc, value))
                    {
                        // set this._name to null - when this.Name is called it will reset _name 
                        // based on the underlying ModelItem
                        _name = null;
                        OnPropertyChanged("EditableName");
                        IsSelected = false;
                        IsSelected = true;
                    }
                }
            }
        }

        /// <summary>
        ///     Execute command to rename ExplorerEFElement's model-item.
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        protected virtual bool RenameModelElement(CommandProcessorContext cpc, string newName)
        {
            // call RenameCommand
            var efNameableItem = ModelItem as EFNameableItem;
            if (null != efNameableItem
                &&
                null != efNameableItem.Artifact
                &&
                null != efNameableItem.Artifact.ModelManager)
            {
                CommandProcessor.InvokeSingleCommand(
                    cpc
                    , efNameableItem.Artifact.ModelManager.CreateRenameCommand(efNameableItem, newName, true));
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Returns a string which is used as the name of the transaction created when
        ///     renaming (and hence is visible in the undo stack)
        /// </summary>
        protected virtual string RenameTransactionName
        {
            get { return string.Format(CultureInfo.CurrentCulture, Resources.RenameTransactionNameFormat, _name); }
        }

        public virtual bool IsKeyProperty
        {
            get { return false; }
        }

        public bool IsInSearchResults
        {
            get { return _isInSearchResults; }
            set
            {
                if (_isInSearchResults != value)
                {
                    _isInSearchResults = value;
                    OnPropertyChanged("IsInSearchResults");
                    if (Parent != null)
                    {
                        Parent.UpdateIsAncestorOfSearchResultItem(_isInSearchResults ? +1 : -1);
                    }
                }
            }
        }

        #endregion

        // to contain useful information for AutomationTools 
        public string ItemStatus
        {
            get
            {
                // used by AutomationTools only - does not need to be localized
                return "IsInSearchResults=" + IsInSearchResults;
            }
        }

        #region TestDisplay

        protected static string DisplayIndent(int indent)
        {
            // could do this more efficiently but this is fine for simple tests
            var sb = new StringBuilder();
            if (indent > 0)
            {
                sb.Append("+");
            }
            for (var i = 1; i < indent; i++)
            {
                sb.Append("-");
            }

            return sb.ToString();
        }

        public void DisplaySelfAndChildren(TextWriter sw, int indent, int indentIncrement)
        {
            sw.Write(DisplayIndent(indent));
            sw.WriteLine(DisplaySelf());
            foreach (var child in Children)
            {
                child.DisplaySelfAndChildren(
                    sw,
                    (indent + indentIncrement), indentIncrement);
            }
        }

        // override this to display just yourself for test purposes (unindented)
        protected virtual string DisplaySelf()
        {
            string displayName;
            if (string.IsNullOrEmpty(Name))
            {
                displayName = "<null>";
            }
            else
            {
                displayName = Name;
            }

            return GetType().Name + ": " + displayName;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Handlers

        protected void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        internal virtual void OnModelPropertyChanged(string modelPropName)
        {
            if (modelPropName == EFNameableItem.AttributeName
                ||
                modelPropName == IsKeyPropertyID)
            {
                // reset the name field if the EFNameableItem.AttributeName argument is passed
                Debug.Assert(ModelItem != null, "Name changed on a explorer node with no corresponding model item");
                if (ModelItem != null
                    && modelPropName == EFNameableItem.AttributeName)
                {
                    _name = ModelItem.DisplayName;
                }

                // our "name" or "key-ness" changed, so we need to re-sort the 
                // collection we are contained in
                var containingCollection = ContainingCollection;
                if (containingCollection != null)
                {
                    containingCollection.Sort();
                }

                // we need to reload the _children collection so that it reflects the correct sort-order
                if (Parent != null)
                {
                    Parent.LoadWpfChildrenCollection();
                }

                // update the Search Results - a rename may cause the elements to change or be re-ordered
                var explorerSearchResults = ExplorerSearchResults.GetExplorerSearchResults(_context);
                explorerSearchResults.OnRenameElement(this);

                OnPropertyChanged(modelPropName);
            }

            // assume any property change will change ToolTipText
            // since it displays the XElement contents
            OnPropertyChanged("ToolTipText");
        }

        #endregion

        #region Public Interface

        public IEnumerable<ExplorerEFElement> SelfAndAncestors()
        {
            return AncestorsFromNode(this);
        }

        public IEnumerable<ExplorerEFElement> Ancestors()
        {
            return AncestorsFromNode(Parent);
        }

        // Make sure the tree view is expanded to this node.  To accomplish this walk up the
        //  ancestors and then expand each node top down (the corresponding
        //  TreeViewItems may not have been created yet so we have to do this
        //  top down forcing the creation of the all of the TreeViewItems down
        //  to this node)
        public void ExpandTreeViewToMe()
        {
            var ancestorStack = new Stack<ExplorerEFElement>();
            foreach (var brItem in Ancestors())
            {
                ancestorStack.Push(brItem);
            }
            while (ancestorStack.Count > 0)
            {
                ancestorStack.Pop().IsExpanded = true;
            }
        }

        #endregion

        #region Implementation

        internal void ClearChildren()
        {
            if (_children != null)
            {
                var xref = ModelToExplorerModelXRef.GetModelToBrowserModelXRef(_context);
                var explorerSearchResults = ExplorerSearchResults.GetExplorerSearchResults(_context);

                // remove children from xref recursively
                foreach (var child in _children)
                {
                    child.ClearChildren();
                    child.Parent = null;

                    // if in Search Results remove from them
                    explorerSearchResults.RemoveElementFromSearchResults(child);

                    if (child.ModelItem != null)
                    {
                        xref.Remove(child.ModelItem);
                    }
                }

                // clear the list
                _children.Clear();
            }
        }

        private static IEnumerable<ExplorerEFElement> AncestorsFromNode(ExplorerEFElement startNode)
        {
            var e = startNode;
            while (e != null)
            {
                yield return e;
                e = e.Parent;
            }
        }

        public bool IsAncestorOfSearchResultItem
        {
            get { return _ancestorOfSearchResultItemCount > 0; }
        }

        private void UpdateIsAncestorOfSearchResultItem(int increment)
        {
            Debug.Assert(increment != 0, "UpdateIsAncestorOfSearchResultItem: increment must not be 0");

            var previousAncestorOfSearchResultItemCount = _ancestorOfSearchResultItemCount;

            _ancestorOfSearchResultItemCount += increment;
            Debug.Assert(
                _ancestorOfSearchResultItemCount >= 0,
                "UpdateIsAncestorOfSearchResultItem: new _ancestorOfSearchResultItemCount value, " + _ancestorOfSearchResultItemCount
                + ", is incorrect - must be >= 0");

            var parentIncrement = 0;
            if (previousAncestorOfSearchResultItemCount == 0)
            {
                parentIncrement = +1;
            }
            else if (_ancestorOfSearchResultItemCount == 0)
            {
                parentIncrement = -1;
            }

            if (parentIncrement != 0)
            {
                OnPropertyChanged("IsAncestorOfSearchResultItem");
                if (Parent != null)
                {
                    Parent.UpdateIsAncestorOfSearchResultItem(parentIncrement);
                }
            }
        }

        #endregion
    }
}
