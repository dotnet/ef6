// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Model
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;

    internal class VSDiagramArtifact : DiagramArtifact, IVsRunningDocTableEvents2
    {
        private bool _disabledBufferUndo;
        private uint _rdtCookie = VSConstants.VSCOOKIE_NIL;

        /// <summary>
        ///     Constructs a VSDiagramArtifact for the passed in URI
        /// </summary>
        /// <param name="modelManager">A reference of ModelManager</param>
        /// <param name="uri">The Diagram File URI</param>
        /// <param name="xmlModelProvider">If you pass null, then you must derive from this class and implement CreateModelProvider().</param>
        internal VSDiagramArtifact(ModelManager modelManager, Uri uri, XmlModelProvider xmlModelProvider)
            : base(modelManager, uri, xmlModelProvider)
        {
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsRunningDocumentTable.AdviseRunningDocTableEvents(Microsoft.VisualStudio.Shell.Interop.IVsRunningDocTableEvents,System.UInt32@)")]
        internal override void Init()
        {
            base.Init();

            Services.IVsRunningDocumentTable.AdviseRunningDocTableEvents(this, out _rdtCookie);
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsRunningDocumentTable.UnadviseRunningDocTableEvents(System.UInt32)")]
        protected override void Dispose(bool disposing)
        {
            if (_rdtCookie != VSConstants.VSCOOKIE_NIL)
            {
                Services.IVsRunningDocumentTable.UnadviseRunningDocTableEvents(_rdtCookie);
            }

            base.Dispose(disposing);
        }

        #region IVsRunningDocTableEvents

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChangeEx(
            uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew,
            uint itemidNew, string pszMkDocumentNew)
        {
            // We only need to worry about linked undo for artifacts that are not cached code gen models, since code gen artifacts don't exist
            // when the designer is open
            if (!IsCodeGenArtifact)
            {
                // First check to see if this is our document and it's being reloaded
                if (!_disabledBufferUndo
                    && (grfAttribs & (uint)__VSRDTATTRIB.RDTA_DocDataReloaded) == (uint)__VSRDTATTRIB.RDTA_DocDataReloaded)
                {
                    var rdt = new RunningDocumentTable(Services.ServiceProvider);
                    var rdi = rdt.GetDocumentInfo(docCookie);
                    if (rdi.Moniker.Equals(Uri.LocalPath, StringComparison.OrdinalIgnoreCase))
                    {
                        // DocData is XmlModelDocData
                        var textBufferProvider = rdi.DocData as IVsTextBufferProvider;
                        Debug.Assert(
                            textBufferProvider != null,
                            "The XML Model DocData over the diagram file is not IVsTextBufferProvider. Linked undo may not work correctly");
                        if (textBufferProvider != null)
                        {
                            IVsTextLines textLines;
                            var hr = textBufferProvider.GetTextBuffer(out textLines);
                            Debug.Assert(
                                textLines != null,
                                "The IVsTextLines could not be found from the IVsTextBufferProvider. Linked undo may not work correctly");
                            if (NativeMethods.Succeeded(hr) && textLines != null)
                            {
                                IOleUndoManager bufferUndoMgr;
                                hr = textLines.GetUndoManager(out bufferUndoMgr);

                                Debug.Assert(
                                    bufferUndoMgr != null, "Couldn't find the buffer undo manager. Linked undo may not work correctly");
                                if (NativeMethods.Succeeded(hr) && bufferUndoMgr != null)
                                {
                                    bufferUndoMgr.Enable(0);
                                    _disabledBufferUndo = true;
                                }
                            }
                        }
                    }
                }
            }

            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}
