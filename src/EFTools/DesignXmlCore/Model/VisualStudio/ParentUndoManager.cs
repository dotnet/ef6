// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio
{
    using System.Diagnostics;
    using Microsoft.VisualStudio.OLE.Interop;

    internal class ParentUndoManager : IOleUndoManager
    {
        private readonly IOleUndoManager _wrappedUndoManager;
        private ParentUndoUnit _parentUndoUnit;

        /// <summary>
        ///     This class is a decorator over an IOleUndoManager.  It allows a "scope" to be started, and then all subsequent Add()
        ///     call will attach the undo unit to a single ParentUndoUnit.  When the "scope" is closed, the ParentUndoUnit is
        ///     added to the "wrapped" IOleUndoManager.  This lets multiple undo/redo units be undone in one action by the user.
        /// </summary>
        public ParentUndoManager(IOleUndoManager wrappedUndoManager)
        {
            _wrappedUndoManager = wrappedUndoManager;
        }

        /// <summary>
        ///     Start an undo scope.  All Undo Units "Add()'d" to this undo manager between a StartParentUndoScope() and CloseParentUndoScope()
        ///     will be added to a single ParentUndoUnit.
        /// </summary>
        public void StartParentUndoScope(string name)
        {
            Debug.Assert(_parentUndoUnit == null, "unexpected non-null value for _parentUndoUnit");
            _parentUndoUnit = new ParentUndoUnit(name);
        }

        /// <summary>
        ///     Close the current undo scope.  This will add the current ParentUndoUnit to the wrapped IOleUndoManager.
        /// </summary>
        public void CloseParentUndoScope()
        {
            if (_parentUndoUnit != null)
            {
                _wrappedUndoManager.Add(_parentUndoUnit);
                _parentUndoUnit = null;
            }
        }

        #region IOleUndoManager Members

        public void Add(IOleUndoUnit pUU)
        {
            // TODO AppDbproj If we initiate an XML Model transaction without a changescope, the
            // _parentUndoUnit will be null. This will go away once we introduce incremental changes
            if (_parentUndoUnit != null)
            {
                _parentUndoUnit.Add(pUU);
            }
        }

        public int Close(IOleParentUndoUnit pPUU, int fCommit)
        {
            return _wrappedUndoManager.Close(pPUU, fCommit);
        }

        public void DiscardFrom(IOleUndoUnit pUU)
        {
            _wrappedUndoManager.DiscardFrom(pUU);
        }

        public void Enable(int fEnable)
        {
            _wrappedUndoManager.Enable(fEnable);
        }

        public void EnumRedoable(out IEnumOleUndoUnits ppEnum)
        {
            _wrappedUndoManager.EnumRedoable(out ppEnum);
        }

        public void EnumUndoable(out IEnumOleUndoUnits ppEnum)
        {
            _wrappedUndoManager.EnumUndoable(out ppEnum);
        }

        public void GetLastRedoDescription(out string pBstr)
        {
            _wrappedUndoManager.GetLastRedoDescription(out pBstr);
        }

        public void GetLastUndoDescription(out string pBstr)
        {
            _wrappedUndoManager.GetLastUndoDescription(out pBstr);
        }

        public int GetOpenParentState(out uint pdwState)
        {
            return _wrappedUndoManager.GetOpenParentState(out pdwState);
        }

        public void Open(IOleParentUndoUnit pPUU)
        {
            _wrappedUndoManager.Open(pPUU);
        }

        public void RedoTo(IOleUndoUnit pUU)
        {
            _wrappedUndoManager.RedoTo(pUU);
        }

        public void UndoTo(IOleUndoUnit pUU)
        {
            _wrappedUndoManager.UndoTo(pUU);
        }

        #endregion
    }
}
