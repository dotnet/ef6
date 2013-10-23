// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Threading;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.UI;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors;
    using Microsoft.Data.Entity.Design.VisualStudio.Providers;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    ///     This is a simple selection container object that
    ///     wraps the designers selection system.
    /// </summary>
    internal abstract class SelectionContainer<T> : ISelectionContainer
        where T : Selection
    {
        private readonly IServiceProvider _shellServices;
        private readonly EditingContext _editingContext;
        private readonly IXmlDesignerPackage _package;
        private List<object> _currentSelection;
        private bool _selectionChangePending;
        private bool _schemaChangePending;

        // Creates a new SelectionContainer object.
        protected SelectionContainer(IServiceProvider shellServices, EditingContext editingContext, IXmlDesignerPackage package)
        {
            _shellServices = shellServices;
            _editingContext = editingContext;
            _package = package;

            // We need to track selection and object model changes
            _editingContext.Items.Subscribe<T>(OnSelectionChanged);

            _package.ModelManager.ModelChangesCommitted += OnModelChangesCommitted;

            // Prime the selection
            RaiseSelectionChange();
        }

        // Disconnects this container from selection
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.ITrackSelection.OnSelectChange(Microsoft.VisualStudio.Shell.Interop.ISelectionContainer)")]
        internal void Dispose()
        {
            _editingContext.Items.Unsubscribe<T>(OnSelectionChanged);

            _package.ModelManager.ModelChangesCommitted -= OnModelChangesCommitted;

            var trackSel = _shellServices.GetService(typeof(ITrackSelection)) as ITrackSelection;
            if (trackSel != null)
            {
                trackSel.OnSelectChange(null);
            }

            var trackSel2 = ParentServiceProvider.GetParentService<ITrackSelection>(_shellServices);
            if (trackSel2 != null)
            {
                trackSel2.OnSelectChange(null);
            }

            _selectionChangePending = false;
            _schemaChangePending = false;
        }

        /// <summary>
        ///     Called by the notification service when the designer's selection has
        ///     changed.
        /// </summary>
        private void OnSelectionChanged(T newSelection)
        {
            _currentSelection = new List<object>(newSelection.SelectionCount);
            foreach (EFElement obj in newSelection.SelectedObjects)
            {
                var desc = GetObjectDescriptor(obj, _editingContext);
                if (desc == null)
                {
                    // we don't have a descriptor for this object so just return without
                    // changing the property window
                    return;
                }
                else
                {
                    _currentSelection.Add(desc);
                }
            }
            RaiseSelectionChange();
        }

        /// <summary>
        ///     Returns a wrapper for the specified EFObject. The wrapper is the type descriptor
        ///     that describes the properties that should be displayed for the EFObject.
        /// </summary>
        protected abstract ObjectDescriptor GetObjectDescriptor(EFElement obj, EditingContext editingContext);

        /// <summary>
        ///     Called by the editing store when its contents change
        /// </summary>
        private void OnModelChangesCommitted(object sender, EfiChangedEventArgs args)
        {
            // this will refresh Property window, so do this only if the change didn't originated from Property window
            if (args.ChangeGroup.Transaction.OriginatorId != EfiTransactionOriginator.PropertyWindowOriginatorId)
            {
                RaiseSchemaChange();
            }
        }

        /// <summary>
        ///     Defers raising a change event by using a Post.
        /// </summary>
        private void RaiseSelectionChange()
        {
            // Expensive to do this, so only do it at idle.
            if (!_selectionChangePending)
            {
                _selectionChangePending = true;
                var d = Dispatcher.CurrentDispatcher;
                d.BeginInvoke(DispatcherPriority.Background, new Callback(OnRaiseSelectionChange));
            }
        }

        /// <summary>
        ///     The result of the Post during RaiseChange ends up here, and actually
        ///     raises the change to VS.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsUIShell.RefreshPropertyBrowser(System.Int32)")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.ITrackSelection.OnSelectChange(Microsoft.VisualStudio.Shell.Interop.ISelectionContainer)")]
        private void OnRaiseSelectionChange()
        {
            if (_selectionChangePending)
            {
                _selectionChangePending = false;
                var trackSel = _shellServices.GetService(typeof(ITrackSelection)) as ITrackSelection;
                if (trackSel != null)
                {
                    trackSel.OnSelectChange(this);
                }

                var trackSel2 = ParentServiceProvider.GetParentService<ITrackSelection>(_shellServices);
                if (trackSel2 != null)
                {
                    trackSel2.OnSelectChange(this);
                }

                var uiShell = _shellServices.GetService(typeof(IVsUIShell)) as IVsUIShell;
                if (uiShell != null)
                {
                    uiShell.RefreshPropertyBrowser(0);
                }
            }
        }

        /// <summary>
        ///     Defers raising a change event by using a Post.
        /// </summary>
        private void RaiseSchemaChange()
        {
            // Expensive to do this, so only do it at idle.
            if (!_schemaChangePending)
            {
                _schemaChangePending = true;
                var d = Dispatcher.CurrentDispatcher;
                d.BeginInvoke(DispatcherPriority.Background, new Callback(OnRaiseSchemaChange));
            }
        }

        /// <summary>
        ///     The result of the Post during RaiseChange ends up here, and actually
        ///     raises the change to VS.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsUIShell.RefreshPropertyBrowser(System.Int32)")]
        private void OnRaiseSchemaChange()
        {
            if (_schemaChangePending)
            {
                _schemaChangePending = false;

                var uiShell = _shellServices.GetService(typeof(IVsUIShell)) as IVsUIShell;
                if (uiShell != null)
                {
                    uiShell.RefreshPropertyBrowser(0);
                }
            }
        }

        #region ISelectionContainer Members

        /// <summary>
        ///     Part of ISelectionContainer.
        /// </summary>
        public int CountObjects(uint dwFlags, out uint pc)
        {
            if ((dwFlags & NativeMethods.ALL) == NativeMethods.ALL
                ||
                (dwFlags & NativeMethods.SELECTED) == NativeMethods.SELECTED)
            {
                if (_currentSelection == null)
                {
                    pc = 0;
                }
                else
                {
                    pc = (uint)_currentSelection.Count;
                }
            }
            else
            {
                pc = 0;
                return NativeMethods.E_INVALIDARG;
            }

            return NativeMethods.S_OK;
        }

        /// <summary>
        ///     Part of ISelectionContainer.
        /// </summary>
        public int GetObjects(uint dwFlags, uint cObjects, object[] apUnkObjects)
        {
            if ((dwFlags & NativeMethods.ALL) == NativeMethods.ALL
                ||
                (dwFlags & NativeMethods.SELECTED) == NativeMethods.SELECTED)
            {
                if (_currentSelection != null)
                {
                    var cnt = 0;
                    foreach (var o in _currentSelection)
                    {
                        if (cnt == cObjects)
                        {
                            break;
                        }
                        apUnkObjects[cnt++] = o;
                    }
                }
            }
            else
            {
                return NativeMethods.E_INVALIDARG;
            }

            return NativeMethods.S_OK;
        }

        /// <summary>
        ///     Part of ISelectionContainer.
        /// </summary>
        public int SelectObjects(uint cSelect, object[] apUnkSelect, uint dwFlags)
        {
            var objectsToSelect = new List<EFObject>();
            foreach (var o in apUnkSelect)
            {
                var typeDesc = o as ObjectDescriptor;
                if (typeDesc != null)
                {
                    objectsToSelect.Add(typeDesc.WrappedItem);
                }
            }

            var s = Activator.CreateInstance<T>();
            s.SetSelectedObjects(objectsToSelect);
            _editingContext.Items.SetValue(s);
            return NativeMethods.S_OK;
        }

        #endregion

        /// <summary>
        ///     Callback delegate we use for the Dispatcher callback.
        /// </summary>
        private delegate void Callback();
    }
}
