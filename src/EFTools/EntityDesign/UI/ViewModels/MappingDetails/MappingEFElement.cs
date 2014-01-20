// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    // <summary>
    //     An enum that is passed to the GetListOfValues() method.
    // </summary>
    internal enum ListOfValuesCollection
    {
        // <summary>
        //     This is the collection for the first column in the Trid
        // </summary>
        FirstColumn,

        // <summary>
        //     This is the collection for the second column in the Trid
        // </summary>
        SecondColumn,

        // <summary>
        //     This is the collection for the third column in the Trid
        // </summary>
        ThirdColumn
    }

    // <summary>
    //     Base class for all of our mapping view model items.
    // </summary>
    internal abstract class MappingEFElement : IDisposable
    {
        internal static readonly MappingLovEFElement LovBlankPlaceHolder = new MappingLovEFElement(string.Empty);
        internal static readonly MappingLovEFElement LovEmptyPlaceHolder = new MappingLovEFElement(Resources.MappingDetails_LovEmpty);
        internal static readonly MappingLovEFElement LovDeletePlaceHolder = new MappingLovEFElement(Resources.MappingDetails_LovDelete);

        protected EditingContext _context;
        protected EFElement _modelItem;
        protected MappingEFElement _parent;
        protected List<MappingEFElement> _children;
        protected string _name; // can be used for new objects that don't have a modelItem yet
        protected bool _isDeleting = false;
        protected bool _isDisposed = false;

        // <summary>
        //     Creates a new item.
        // </summary>
        // <param name="context">The current EditingContext; can be null</param>
        // <param name="modelItem">The underlying model item; can be null for view model items that don't have an underlying model item yet.</param>
        // <param name="parent">This item's parent; this should only be null for root level items</param>
        protected MappingEFElement(EditingContext context, EFElement modelItem, MappingEFElement parent)
        {
            _context = context;
            _modelItem = modelItem;
            _parent = parent;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                Dispose(true);
                _isDisposed = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_children != null)
                {
                    foreach (var o in _children)
                    {
                        if (o != null)
                        {
                            o.Dispose();
                        }
                    }
                }
            }
        }

        // <summary>
        //     Gets or sets the EditingContext.
        // </summary>
        internal EditingContext Context
        {
            get { return _context; }
            set { _context = value; }
        }

        // <summary>
        //     If a name has been set explicitly, then this will return that.  Otherwise, this will return the
        //     DisplayName of the underlying model item (if any).  Override to change this behavior.
        // </summary>
        internal virtual string Name
        {
            get
            {
                if (_name != null)
                {
                    return _name;
                }

                if (ModelItem != null)
                {
                    return ModelItem.DisplayName;
                }

                return string.Empty;
            }
            set { _name = value; }
        }

        // <summary>
        //     Gets or sets the underlying ModelItem.
        // </summary>
        internal virtual EFElement ModelItem
        {
            get { return _modelItem; }
            set
            {
                if (_modelItem == value)
                {
                    return;
                }

                Debug.Assert(_context != null, "You must set the Context before setting a ModelItem");
                var xref = ModelToMappingModelXRef.GetModelToMappingModelXRef(_context);

                // remove any existing xref
                if (_modelItem != null)
                {
                    xref.Remove(_modelItem);
                }

                // set the new xref if we aren't changing to null
                if (value != null)
                {
                    xref.Set(value, this);
                }

                _modelItem = value;

                _isDisposed = false;
            }
        }

        // <summary>
        //     Gets or sets this view model item's Parent.
        // </summary>
        internal MappingEFElement Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        // <summary>
        //     Returns this item's Children collection.  The first time this is called, the LoadChildrenCollection() method is called
        //     and the collection is populated by the derived class's override (if any).
        // </summary>
        internal IList<MappingEFElement> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new List<MappingEFElement>();
                    LoadChildrenCollection();
                }
                return _children.AsReadOnly();
            }
        }

        // <summary>
        //     Adds a child to the children collection.
        // </summary>
        internal void AddChild(MappingEFElement newChild)
        {
            if (Children.Contains(newChild) == false)
            {
                _children.Add(newChild);
            }
        }

        // <summary>
        //     Removes a child from the children collection.
        // </summary>
        internal void RemoveChild(MappingEFElement child)
        {
            if (Children.Contains(child))
            {
                _children.Remove(child);
            }
        }

        // <summary>
        //     This must be implemented by derived clases who have children.
        // </summary>
        protected virtual void LoadChildrenCollection()
        {
        }

        // <summary>
        //     This must be implemented by derived clases who have lists of values.
        // </summary>
        internal virtual Dictionary<MappingLovEFElement, string> GetListOfValues(ListOfValuesCollection type)
        {
            var lov = new Dictionary<MappingLovEFElement, string>();
            lov.Add(LovEmptyPlaceHolder, LovEmptyPlaceHolder.DisplayName);
            return lov;
        }

        // <summary>
        //     Looks for an LOV Element in this item's list of values.
        // </summary>
        // <param name="lovDisplayName">What to look for</param>
        // <param name="lovType">Which LOV collection to search</param>
        // <returns>If none are found it will return a null item.</returns>
        internal MappingLovEFElement FindMappingLovElement(string lovDisplayName, ListOfValuesCollection lovType)
        {
            MappingLovEFElement mappingLovElement = null;
            Debug.Assert(lovDisplayName != null, "Null lovDisplayName in FindMappingLovElement()");
            if (lovDisplayName != null)
            {
                var lov = GetListOfValues(lovType);
                foreach (var entry in lov)
                {
                    if (lovDisplayName == entry.Value)
                    {
                        mappingLovElement = entry.Key;
                        break;
                    }
                }

                Debug.Assert(
                    mappingLovElement != null,
                    "Could not find MappingLovElement for lovDisplayName " + lovDisplayName + ", lovType " + lovType.ToString());
            }

            return mappingLovElement;
        }

        // <summary>
        //     Utility method. If already have a MappingLovEFElement just return it. If not
        //     then lookup the MappingLovEFElement using the string and ListOfValuesType.
        //     This latter situation means that a user has used the keyboard to select
        //     a value from a drop-down which has come through to here as text.
        // </summary>
        internal MappingLovEFElement GetLovElementFromLovElementOrString(
            MappingLovEFElement lovElement, string lovDisplayName, ListOfValuesCollection lovType)
        {
            if (lovElement != null)
            {
                return lovElement;
            }

            return FindMappingLovElement(lovDisplayName, lovType);
        }

        // <summary>
        //     Returns the index of the passed in child in this item's Children collection, returns -1 if it isn't found
        // </summary>
        internal int IndexOfChild(MappingEFElement childToFind)
        {
            if (childToFind != null)
            {
                for (var i = 0; i < Children.Count; i++)
                {
                    if (Children[i] == childToFind)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        // <summary>
        //     Walks up the tree and returns the first parent that is of the Type passed in.
        // </summary>
        // <param name="type">The type of parent to find.</param>
        internal MappingEFElement GetParentOfType(Type type)
        {
            if (GetType() == type)
            {
                return this;
            }

            var item = Parent;
            while (item != null)
            {
                if (item.GetType() == type)
                {
                    return item;
                }
                item = item.Parent;
            }

            return null;
        }

        // <summary>
        //     Derived classes need to override this and implement their own creation logic.
        // </summary>
        // <param name="cpc">If 'null' is passed then a cpc and transaction will be created for this call, pass a non-null cpc to include this in a larger transaction</param>
        // <param name="context"></param>
        // <param name="underlyingModelItem"></param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        internal virtual void CreateModelItem(CommandProcessorContext cpc, EditingContext context, EFElement underlyingModelItem)
        {
        }

        // <summary>
        //     Derived classes need to override this and implement their own deletion logic; note that this is not the same as Delete().  This
        //     method just deletes the model item that underlies this view model item.
        // </summary>
        // <param name="cpc">If 'null' is passed then a cpc and transaction will be created for this call, pass a non-null cpc to include this in a larger transaction</param>
        internal virtual void DeleteModelItem(CommandProcessorContext cpc)
        {
        }

        // <summary>
        //     This lets you switch the underlyingModelItem.
        // </summary>
        // <param name="cpc">The transaction to use for this entire process, cannot be null</param>
        // <param name="context">The current EditingContext</param>
        // <param name="newUnderlyingModelItem">The new model item to switch to</param>
        // <param name="deleteModelItemOnly">If 'true' then the MappingEFElement will just have its model item switched, if 'false' then a new MappingEFElement will be create and this one will be deleted</param>
        internal void SwitchModelItem(
            CommandProcessorContext cpc, EditingContext context, EFElement newUnderlyingModelItem, bool deleteModelItemOnly)
        {
            Debug.Assert(cpc != null, "You should send a cpc to this function so that the entire switch is in a single transaction");

            var cmd = new DelegateCommand(
                () =>
                    {
                        if (deleteModelItemOnly)
                        {
                            DeleteModelItemsRecursive(this, cpc);
                            CreateModelItem(cpc, context, newUnderlyingModelItem);
                        }
                        else
                        {
                            Delete(cpc);
                            var newElement = CreateCreatorNodeCopy();
                            newElement.CreateModelItem(cpc, context, newUnderlyingModelItem);
                        }
                    });

            var cp = new CommandProcessor(cpc, cmd);
            cp.Invoke();
        }

        // <summary>
        //     Used to support the SwitchModelItem() method when the user passed false to 'deleteModelItemOnly'; derived classes
        //     should override to provide their own creation logic if needed.
        // </summary>
        protected virtual MappingEFElement CreateCreatorNodeCopy()
        {
            // by default just create a new one passing typical contructor arguments
            return Activator.CreateInstance(GetType(), Context, null, Parent) as MappingEFElement;
        }

        // <summary>
        //     Return 'true' if the ModelItem is not null and the ModelItem's XObject is null.  This means that this view model item's
        //     underlying model item exists, but its been deleted in the XLinq tree.
        // </summary>
        internal bool IsModelItemDeleted()
        {
            if (ModelItem != null
                && ModelItem.XObject == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // <summary>
        //     Deletes this view model item's underlying model item, its children and finally removes it from the view model.
        // </summary>
        // <param name="cpc">If 'null' is passed then a cpc and transaction will be created for this call, pass a non-null cpc to include this in a larger transaction</param>
        internal void Delete(CommandProcessorContext cpc)
        {
            if (!_isDeleting)
            {
                _isDeleting = true;
                OnDelete(cpc);
                Dispose();
                _isDeleting = false;
            }
        }

        // <summary>
        //     Private helper method to delete a view model item.
        // </summary>
        // <param name="cpc">If 'null' is passed then a cpc and transaction will be created for this call, pass a non-null cpc to include this in a larger transaction</param>
        private void OnDelete(CommandProcessorContext cpc)
        {
            if (ModelItem != null)
            {
                DeleteModelItem(cpc);
            }

            if (_children != null)
            {
                for (var i = Children.Count - 1; i >= 0; i--)
                {
                    var child = Children[i];
                    child.Delete(cpc);
                }
                _children = null;
            }

            if (Parent != null)
            {
                Parent.OnChildDeleted(this);
            }
        }

        // <summary>
        //     Derived classes should override this and implement their own logic for removing an item from
        //     their children collection.
        // </summary>
        protected virtual void OnChildDeleted(MappingEFElement melem)
        {
        }

        // <summary>
        //     Recurses this item's children and calls DeleteModelItem() (not Delete) on them, then calls this on itself.
        // </summary>
        // <param name="melem"></param>
        // <param name="cpc">If 'null' is passed then a cpc and transaction will be created for this call, pass a non-null cpc to include this in a larger transaction</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        private static void DeleteModelItemsRecursive(MappingEFElement melem, CommandProcessorContext cpc)
        {
            foreach (var childMappingEFElement in melem.Children)
            {
                DeleteModelItemsRecursive(childMappingEFElement, cpc);
            }
            if (melem.ModelItem != null
                && melem.IsModelItemDeleted() == false)
            {
                melem.DeleteModelItem(cpc);
            }
        }
    }
}
