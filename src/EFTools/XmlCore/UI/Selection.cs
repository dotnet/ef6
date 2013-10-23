// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;

    /// <summary>
    ///     The Selection class defines a selection of EFObject.  Selections
    ///     consist of zero or more EFObject.  The first EFObject in a selection
    ///     is defined as the "primary" selection, which is used when
    ///     one object in a group must be used as a key.
    /// </summary>
    internal abstract class Selection : ContextItem
    {
        private ICollection<EFObject> _selectedObjects;

        /// <summary>
        ///     Creates an empty Selection object.
        /// </summary>
        public Selection()
        {
            _selectedObjects = new EFObject[0];
        }

        /// <summary>
        ///     Creates a collection object comprising the given
        ///     selected objects.  The first object in the enumeration
        ///     is considered the "primary" selection.
        /// </summary>
        /// <param name="selectedObjects">An enumeration of objects that should be selected.</param>
        /// <exception cref="ArgumentNullException">If selectedObjects is null.</exception>
        internal Selection(IEnumerable<EFObject> selectedObjects)
            : this(selectedObjects, null)
        {
        }

        /// <summary>
        ///     Creates a collection object comprising the given
        ///     selected objects.  The first object in the enumeration
        ///     is considered the "primary" selection.
        /// </summary>
        /// <param name="selectedObjects">An enumeration of objects that should be selected.</param>
        /// <param name="match">If provided, only those objects in selectedObjects that match the predicate will be added to the selection.</param>
        /// <exception cref="ArgumentNullException">If selectedObjects is null.</exception>
        internal Selection(IEnumerable<EFObject> selectedObjects, Predicate<EFObject> match)
        {
            if (selectedObjects == null)
            {
                throw new ArgumentNullException("selectedObjects");
            }

            var selection = new List<EFObject>();
            foreach (object o in selectedObjects)
            {
                var info = o as EFObject;
                if (info != null
                    && (match == null || match(info)))
                {
                    selection.Add(info);
                }
            }

            _selectedObjects = selection;
        }

        /// <summary>
        ///     Creates a collection object comprising the given
        ///     selected objects.  The first object in the enumeration
        ///     is considered the "primary" selection.
        /// </summary>
        /// <param name="selectedObjects">An enumeration of objects that should be selected.</param>
        /// <exception cref="ArgumentNullException">If selectedObjects is null.</exception>
        internal Selection(IEnumerable selectedObjects)
            : this(selectedObjects, null)
        {
        }

        /// <summary>
        ///     Creates a collection object comprising the given
        ///     selected objects.  The first object in the enumeration
        ///     is considered the "primary" selection.
        /// </summary>
        /// <param name="selectedObjects">An enumeration of objects that should be selected.</param>
        /// <param name="match">If provided, only those objects in selectedObjects that match the predicate will be added to the selection.</param>
        /// <exception cref="ArgumentNullException">If selectedObjects is null.</exception>
        internal Selection(IEnumerable selectedObjects, Predicate<EFObject> match)
        {
            if (selectedObjects == null)
            {
                throw new ArgumentNullException("selectedObjects");
            }

            var selection = new List<EFObject>();
            foreach (var o in selectedObjects)
            {
                var info = o as EFObject;
                if (info != null
                    && (match == null || match(info)))
                {
                    selection.Add(info);
                }
            }

            _selectedObjects = selection;
        }

        /// <summary>
        ///     Creates a collection object comprising the given
        ///     objects.  The first object is considered the "primary"
        ///     selection.
        /// </summary>
        /// <param name="selectedObjects">A parameter array of objects that should be selected.</param>
        /// <exception cref="ArgumentNullException">If selectedObjects is null.</exception>
        internal Selection(params EFObject[] selectedObjects)
            : this((IEnumerable<EFObject>)selectedObjects)
        {
        }

        /// <summary>
        ///     The primary selection.  Some functions require a "key"
        ///     element.  For example, an "align lefts" command needs
        ///     to know which element's "left" to align to.
        /// </summary>
        internal EFObject PrimarySelection
        {
            get
            {
                foreach (var obj in _selectedObjects)
                {
                    return obj;
                }

                return null;
            }
        }

        /// <summary>
        ///     The enumeration of selected objects.
        /// </summary>
        internal IEnumerable<EFObject> SelectedObjects
        {
            get { return _selectedObjects; }
        }

        /// <summary>
        ///     The number of objects that are currently selected into
        ///     this selection.
        /// </summary>
        internal int SelectionCount
        {
            get { return _selectedObjects.Count; }
        }

        /// <summary>
        ///     Override of ContextItem's ItemType property.
        /// </summary>
        internal override Type ItemType
        {
            get { return typeof(Selection); }
        }

        internal void SetSelectedObjects(List<EFObject> selectedObjects)
        {
            _selectedObjects = selectedObjects;
        }

        internal void SetSelectedObjects(params EFObject[] selectedObjects)
        {
            var selection = new List<EFObject>();
            foreach (object o in selectedObjects)
            {
                var info = o as EFObject;
                if (info != null)
                {
                    selection.Add(info);
                }
            }

            _selectedObjects = selection;
        }

        #region Static Helpers

        /// <summary>
        ///     Clears the selection contained in the editing context.
        /// </summary>
        /// <param name="context">The editing context to apply this selection change to.</param>
        /// <exception cref="ArgumentNullException">If context is null.</exception>
        internal static void Clear<T>(EditingContext context) where T : Selection
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            Selection existing = context.Items.GetValue<T>();
            if (existing.PrimarySelection != null)
            {
                context.Items.SetValue(Activator.CreateInstance<T>());
            }
        }

        /// <summary>
        ///     Selection helper method.  This takes the existing selection in the
        ///     context and selects an item into it.  If the item is already in the
        ///     selection the selection is preserved and the item is promoted
        ///     to the primary selection.
        /// </summary>
        /// <param name="context">The editing context to apply this selection change to.</param>
        /// <param name="itemToSelect">The item to select.</param>
        /// <returns>A Selection object that contains the new selection.</returns>
        /// <exception cref="ArgumentNullException">If context or itemToSelect is null.</exception>
        internal static T Select<T>(EditingContext context, EFObject itemToSelect) where T : Selection
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (itemToSelect == null)
            {
                throw new ArgumentNullException("itemToSelect");
            }

            var existing = context.Items.GetValue<T>();

            // short cut if we're already in the right state.
            if (existing.PrimarySelection == itemToSelect)
            {
                return existing;
            }

            T selection = null;

            foreach (var obj in existing.SelectedObjects)
            {
                if (obj == itemToSelect)
                {
                    var list = new List<EFObject>(existing.SelectedObjects);
                    list.Remove(itemToSelect);
                    list.Insert(0, itemToSelect);
                    selection = Activator.CreateInstance<T>();
                    selection.SetSelectedObjects(list);
                }
            }

            if (selection == null)
            {
                selection = Activator.CreateInstance<T>();
                selection.SetSelectedObjects(itemToSelect);
            }

            context.Items.SetValue(selection);
            return selection;
        }

        /// <summary>
        ///     Selection helper method.  This sets itemToSelect into the selection.
        ///     Any existing items are deselected.
        /// </summary>
        /// <param name="context">The editing context to apply this selection change to.</param>
        /// <param name="itemToSelect">The item to select.</param>
        /// <returns>A Selection object that contains the new selection.</returns>
        /// <exception cref="ArgumentNullException">If context or itemToSelect is null.</exception>
        internal static T SelectOnly<T>(EditingContext context, EFObject itemToSelect) where T : Selection
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (itemToSelect == null)
            {
                throw new ArgumentNullException("itemToSelect");
            }

            // Check to see if only this object is selected.  If so, bail.
            var existing = context.Items.GetValue<T>();
            if (existing.PrimarySelection == itemToSelect)
            {
                var en = existing.SelectedObjects.GetEnumerator();
                en.MoveNext();
                if (!en.MoveNext())
                {
                    return existing;
                }
            }

            var selection = Activator.CreateInstance<T>();
            selection.SetSelectedObjects(itemToSelect);
            context.Items.SetValue(selection);
            return selection;
        }

        /// <summary>
        ///     Helper method that subscribes to selection change events.
        /// </summary>
        /// <param name="context">The editing context to listen to.</param>
        /// <param name="handler">The handler to be invoked when the selection changes.</param>
        internal static void Subscribe<T>(EditingContext context, SubscribeContextCallback<T> handler) where T : Selection
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            context.Items.Subscribe(handler);
        }

        /// <summary>
        ///     Selection helper method.  This takes the existing selection in the
        ///     context and creates a new selection that contains the toggled
        ///     state of the item.  If the item is to be
        ///     added to the selection, it is added as the primary selection.
        /// </summary>
        /// <param name="context">The editing context to apply this selection change to.</param>
        /// <param name="itemToToggle">The item to toggle selection for.</param>
        /// <returns>A Selection object that contains the new selection.</returns>
        /// <exception cref="ArgumentNullException">If context or itemToToggle is null.</exception>
        internal static T Toggle<T>(EditingContext context, EFObject itemToToggle) where T : Selection
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (itemToToggle == null)
            {
                throw new ArgumentNullException("itemToToggle");
            }

            var existing = context.Items.GetValue<T>();

            // Is the item already in the selection?  If so, remove it.
            // If not, add it to the beginning.

            var list = new List<EFObject>(existing.SelectedObjects);
            if (list.Contains(itemToToggle))
            {
                list.Remove(itemToToggle);
            }
            else
            {
                list.Insert(0, itemToToggle);
            }

            var selection = Activator.CreateInstance<T>();
            selection.SetSelectedObjects(list);
            context.Items.SetValue(selection);
            return selection;
        }

        /// <summary>
        ///     Selection helper method.  This takes the existing selection in the
        ///     context and creates a new selection that contains the original
        ///     selection and the itemToAdd.  If itemToAdd is already in the
        ///     original selection it is promoted to the primary selection.
        /// </summary>
        /// <param name="context">The editing context to apply this selection change to.</param>
        /// <param name="itemToAdd">The item to add to the selection.</param>
        /// <returns>A Selection object that contains the new selection.</returns>
        /// <exception cref="ArgumentNullException">If context or itemToAdd is null.</exception>
        internal static T Union<T>(EditingContext context, EFObject itemToAdd) where T : Selection
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (itemToAdd == null)
            {
                throw new ArgumentNullException("itemToAdd");
            }

            var existing = context.Items.GetValue<T>();

            // short cut if we're already in the right state.
            if (existing.PrimarySelection == itemToAdd)
            {
                return existing;
            }

            // Is the item already in the selection?  If not, add it.
            var list = new List<EFObject>(existing.SelectedObjects);
            if (list.Contains(itemToAdd))
            {
                list.Remove(itemToAdd);
            }

            list.Insert(0, itemToAdd);
            var selection = Activator.CreateInstance<T>();
            selection.SetSelectedObjects(list);
            context.Items.SetValue(selection);
            return selection;
        }

        /// <summary>
        ///     Helper method that removes a previously added selection change event.
        /// </summary>
        /// <param name="context">The editing context to listen to.</param>
        /// <param name="handler">The handler to be invoked when the selection changes.</param>
        internal static void Unsubscribe<T>(EditingContext context, SubscribeContextCallback<T> handler) where T : Selection
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            context.Items.Unsubscribe(handler);
        }

        #endregion
    }
}
