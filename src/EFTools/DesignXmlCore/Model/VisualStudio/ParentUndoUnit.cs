// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using VSErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.OLE.Interop;

    /// <summary>
    ///     This class is a single undo unit that can hold multiple child undo units.  When an instance is
    ///     undone or redone, all child units will be undone or redone.
    /// </summary>
    internal class ParentUndoUnit : IOleParentUndoUnit
    {
        private List<IOleUndoUnit> _children = new List<IOleUndoUnit>();
        private IOleParentUndoUnit _openParent;
        private readonly string _name;

        public ParentUndoUnit(string name)
        {
            _name = name;
        }

        #region IOleParentUndoUnit Members

        public void Add(IOleUndoUnit pUU)
        {
            _children.Add(pUU);
        }

        public void Do(IOleUndoManager pUndoManager)
        {
            // docs say this can be null.
            if (pUndoManager != null)
            {
                // use this as the undo unit also.
                pUndoManager.Open(this);
            }

            var units = _children;
            _children = new List<IOleUndoUnit>();

            // Invoke child units in reverse order.
            for (var i = units.Count - 1; i >= 0; i--)
            {
                var child = units[i];
                child.Do(pUndoManager);
            }

            if (pUndoManager != null)
            {
                NativeMethods.ThrowOnFailure(pUndoManager.Close(this, 1));
            }
        }

        public int FindUnit(IOleUndoUnit pUU)
        {
            if (_children.Contains(pUU))
            {
                return VSConstants.S_OK;
            }
            return VSConstants.S_FALSE;
        }

        public void GetDescription(out string pBstr)
        {
            pBstr = _name;
        }

        public void GetParentState(out uint pdwState)
        {
            if (_openParent != null)
            {
                _openParent.GetParentState(out pdwState);
            }
            else
            {
                pdwState = (uint)UASFLAGS.UAS_NORMAL;
            }
        }

        public void GetUnitType(out Guid pClsid, out int plID)
        {
            pClsid = Guid.Empty;
            plID = 0;
        }

        public void OnNextAdd()
        {
            // we don't have any merging to do with prior commands, so we can ignore this.
        }

        public void Open(IOleParentUndoUnit pPUU)
        {
            if (_openParent == null)
            {
                _openParent = pPUU;
            }
            else
            {
                _openParent.Open(pPUU);
            }
        }

        public int Close(IOleParentUndoUnit pPUU, int fCommit)
        {
            if (pPUU == null)
            {
                return VSConstants.S_FALSE;
            }

            if (_openParent == null)
            {
                return VSConstants.S_FALSE;
            }

            var hr = _openParent.Close(pPUU, fCommit);
            if (VSErrorHandler.Failed(hr))
            {
                return hr;
            }

            if (_openParent == pPUU)
            {
                _openParent = null;
                if (fCommit != 0)
                {
                    _children.Add(pPUU);
                }
                return VSConstants.S_OK;
            }
            else
            {
                throw new ArgumentException("Closing the wrong parent unit!");
            }
        }

        #endregion
    }
}
