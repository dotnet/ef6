// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    ///     The DocumentFrameMgr class manages all document window frames that
    ///     are associated to a designer file document if they were loaded in the default designer or in the XML editor
    /// </summary>
    internal abstract class DocumentFrameMgr : IVsRunningDocTableEvents3, IDisposable, IVsSelectionEvents
    {
        private IVsRunningDocumentTable _rdt;
        private uint _rdtEventsCookie;
        private IVsMonitorSelection _sel;
        private uint _selEventsCookie;
        private readonly IXmlDesignerPackage _package;
        private EditingContextManager _editingContextMgr;
        private bool _doNotChangeArtifactInBrowserForNextOpeningDoc;
        private bool _disposed;
        private bool _handlingOnElementValueChanged;

        internal EFArtifact CurrentArtifact
        {
            get
            {
                object pvarValue;
                NativeMethods.ThrowOnFailure(_sel.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_DocumentFrame, out pvarValue));
                var activeFrame = CreateFrameWrapper(pvarValue as IVsWindowFrame);
                if (activeFrame != null
                    && activeFrame.Uri != null)
                {
                    return _package.ModelManager.GetArtifact(activeFrame.Uri);
                }
                return null;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsMonitorSelection.AdviseSelectionEvents(Microsoft.VisualStudio.Shell.Interop.IVsSelectionEvents,System.UInt32@)")]
        protected DocumentFrameMgr(IXmlDesignerPackage package)
        {
            _package = package;
            _editingContextMgr = new EditingContextManager(package);

            _package.FileNameChanged += OnAfterFileNameChanged;

            IServiceProvider sp = _package;
            _rdt = sp.GetService(typeof(IVsRunningDocumentTable)) as IVsRunningDocumentTable;
            if (_rdt != null)
            {
                NativeMethods.ThrowOnFailure(_rdt.AdviseRunningDocTableEvents(this, out _rdtEventsCookie));
            }

            _sel = sp.GetService(typeof(IVsMonitorSelection)) as IVsMonitorSelection;
            if (_sel != null)
            {
                _sel.AdviseSelectionEvents(this, out _selEventsCookie);
            }
        }

        ~DocumentFrameMgr()
        {
            Dispose(false);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsMonitorSelection.UnadviseSelectionEvents(System.UInt32)")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsRunningDocumentTable.UnadviseRunningDocTableEvents(System.UInt32)")]
        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // dispose-only, i.e. non-finalizable logic
                    if (_editingContextMgr != null)
                    {
                        _editingContextMgr.Dispose();
                        _editingContextMgr = null;
                    }
                }

                // shared cleanup logic
                if (_rdt != null)
                {
                    _rdt.UnadviseRunningDocTableEvents(_rdtEventsCookie);
                    _rdt = null;
                    _rdtEventsCookie = 0;
                }

                if (_sel != null)
                {
                    _sel.UnadviseSelectionEvents(_selEventsCookie);
                    _selEventsCookie = 0;
                    _sel = null;
                }

                _package.FileNameChanged -= OnAfterFileNameChanged;

                _disposed = true;
            }
        }

        #endregion

        internal EditingContextManager EditingContextManager
        {
            get { return _editingContextMgr; }
        }

        protected internal abstract FrameWrapper CreateFrameWrapper(IVsWindowFrame frame);

        protected internal abstract void SetCurrentContext(EditingContext context);

        protected abstract void ClearErrorList(Uri oldUri, Uri newUri);

        protected abstract void ClearErrorList(IVsHierarchy pHier, uint ItemID);

        protected abstract bool HasDesignerExtension(Uri uri);

        protected abstract void OnAfterDesignerDocumentWindowHide(Uri docUri);

        protected virtual void OnAfterLastDesignerDocumentUnlock(Uri docUri)
        {
            // Do nothing by default
        }

        protected virtual void OnAfterSave()
        {
            // Do nothing by default
        }

        protected abstract void OnBeforeLastDesignerDocumentUnlock(Uri docUri);

        internal void SetItemForActiveFrame(Uri itemUri)
        {
            object pvarValue;
            NativeMethods.ThrowOnFailure(_sel.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_DocumentFrame, out pvarValue));
            var activeFrame = CreateFrameWrapper(pvarValue as IVsWindowFrame);
            _editingContextMgr.SetCurrentUri(activeFrame, itemUri);
            UpdateToolWindowsAndCmdsForFrame(activeFrame);
        }

        /// <summary>
        ///     Used when we need to update the tool windows for the active frame. This is a wrapper around
        ///     UpdateToolWindowsForFrame.
        /// </summary>
        /// <param name="closingFrame"></param>
        private void UpdateToolWindowsAndCmdsForActiveFrame(FrameWrapper closingFrame)
        {
            object pvarValue;
            NativeMethods.ThrowOnFailure(_sel.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_DocumentFrame, out pvarValue));
            var activeFrame = CreateFrameWrapper(pvarValue as IVsWindowFrame);
            if (!activeFrame.Equals(closingFrame))
            {
                UpdateToolWindowsAndCmdsForFrame(activeFrame);
            }
        }

        /// <summary>
        ///     Used whenever we need to change window frame selection (switching/opening/closing). This is used
        ///     when we are explicitly given a new window frame.
        /// </summary>
        /// <param name="frame"></param>
        private void UpdateToolWindowsAndCmdsForFrame(FrameWrapper newFrame)
        {
            if (newFrame.ShouldShowToolWindows)
            {
                var artifactUri = _editingContextMgr.GetCurrentUri(newFrame);

                var context = _editingContextMgr.GetNewOrExistingContext(artifactUri);
                Debug.Assert(context != null, "Context is null in UpdateToolWindowsForFrame! The tool windows may not show.");

                SetCurrentContext(context);
            }
            else
            {
                SetCurrentContext(null);
            }
        }

#if VIEWSOURCE
        internal void ViewSource(Uri docUri, XObject nodeToSelect)
        {
            _doNotChangeArtifactInBrowserForNextOpeningDoc = true;
            try
            {
                FrameWrapper frame = this.CreateFrameWrapper(
                    VsShellUtilities.OpenDocumentWithSpecificEditor(_package, docUri.LocalPath, CommonPackageConstants.xmlEditorGuid, VSConstants.LOGVIEWID_Primary));

                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                    new DispatcherOperationCallback(delegate(object arg)
                {
                    frame.Show();
                    return null;
                }), null);

                if (nodeToSelect != null)
                {
                    IVsTextView view = frame.TextView;
                    if (view != null)
                    {
                        TextSpan textSpan;
                        if (GetTextSpan(frame.Uri, nodeToSelect, out textSpan))
                        {
                            if (textSpan.iStartLine != textSpan.iEndLine)
                            {
                                // select only the first line
                                textSpan.iEndLine = textSpan.iStartLine + 1;
                                textSpan.iEndIndex = 0;
                            }
                            view.EnsureSpanVisible(textSpan);
                            view.SetSelection(textSpan.iEndLine, textSpan.iEndIndex, textSpan.iStartLine, textSpan.iStartIndex);
                        }
                    }
                }
            }
            finally
            {
                _doNotChangeArtifactInBrowserForNextOpeningDoc = false;
            }
        }
#endif

        /// <summary>
        ///     Updates artifact URI and error list when a document is renamed/moved/save-as'd WHEN IT IS OPEN
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="cpc"></param>
        /// <returns></returns>
        private int OnAfterFileNameChanged(object sender, ModelChangeEventArgs args)
        {
            // after a file name change we have to rename the artifact
            if (args.OldFileName != null
                && args.NewFileName != null)
            {
                // if the user renamed the extension of the file, we will still take it
                var oldUri = Utils.FileName2Uri(args.OldFileName);
                if (oldUri != null
                    &&
                    _package != null
                    &&
                    _package.ModelManager != null
                    &&
                    _package.ModelManager.GetArtifact(oldUri) != null)
                {
                    var newUri = Utils.FileName2Uri(args.NewFileName);

                    // first we need to remove the errors attached to this soon-to-be stale artifact
                    // this applies to rename/move
                    var oldArtifact = _package.ModelManager.GetArtifact(oldUri);
                    if (oldArtifact != null
                        && oldArtifact.ArtifactSet != null)
                    {
                        oldArtifact.ArtifactSet.RemoveErrorsForArtifact(oldArtifact);
                    }

                    ClearErrorList(oldUri, newUri);

                    _package.ModelManager.RenameArtifact(oldUri, newUri);
                }
            }

            return VSConstants.S_OK;
        }

        #region IVsRunningDocTableEvents3 Members

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            // frame is being closed
            var closingFrame = CreateFrameWrapper(pFrame);

            // get the Uri for the frame
            var docUri = closingFrame.Uri;

            // the implementation of IVsWindowFrame doesn't have to return the right moniker, so
            // we won't do anything.
            if (null != docUri)
            {
                // It is important here to check that the file is open in the designer's editor 
                // and not just check the extension since the document could be open in the Xml Editor. 
                // In which case, the document would the designer's extension but we don't want to do anything
                // designer-related
                if (closingFrame.IsDesignerDocInDesigner)
                {
                    OnAfterDesignerDocumentWindowHide(docUri);

                    // tell the artifact manager we are closing
                    _editingContextMgr.OnCloseFrame(closingFrame);

                    // switch the browser to the new active frame if one is visible
                    UpdateToolWindowsAndCmdsForActiveFrame(closingFrame);
                }
            }
            return NativeMethods.S_OK;
        }

        private void ReloadArtifactIfNecessary(FrameWrapper frameWrapper)
        {
            if (frameWrapper != null
                &&
                frameWrapper.Uri != null
                &&
                frameWrapper.IsDocumentOpen(_package)) // don't need to reload if the document isn't open at all
            {
                // It is important here to check that the file is open in the designer's editor 
                // and not just check the extension since the document could be open in the Xml Editor. 
                // In which case, the document would the designer's extension but we don't want to do anything
                // designer-related
                if (frameWrapper.IsDesignerDocInDesigner)
                {
                    var context = EditingContextManager.GetNewOrExistingContext(frameWrapper.Uri);
                    var artifact = context.GetEFArtifactService().Artifact;
                    if (artifact.RequireDelayedReload)
                    {
                        try
                        {
                            artifact.ReloadArtifact();
                        }
                        finally
                        {
                            artifact.RequireDelayedReload = false;
                        }
                    }
                }
            }
        }

        public abstract int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame);

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return NativeMethods.S_OK;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return NativeMethods.S_OK;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            if (((dwRDTLockType & (uint)(_VSRDTFLAGS.RDT_EditLock)) > 0)
                && dwEditLocksRemaining <= 1)
            {
                var IVsRDT = _package.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
                Debug.Assert(IVsRDT != null, "Failed to get IVsRunningDocumentTable!");

                // Clear the errors associated with a given document when the document is closed
                if (IVsRDT != null)
                {
                    IVsHierarchy pHier = null;
                    uint ItemID;

                    // Dummy variables
                    uint pgrfRDTFlags, pdwReadLocks, pdwEditLocks;
                    string pbstrsMrDocument;
                    var ppunkDocData = IntPtr.Zero;

                    try
                    {
                        var hr = IVsRDT.GetDocumentInfo(
                            docCookie, out pgrfRDTFlags, out pdwReadLocks, out pdwEditLocks, out pbstrsMrDocument, out pHier, out ItemID,
                            out ppunkDocData);
                        if (ppunkDocData != IntPtr.Zero)
                        {
                            Marshal.Release(ppunkDocData);
                        }

                        if (hr == VSConstants.S_OK
                            && pHier != null
                            && ItemID != VSConstants.VSITEMID_NIL
                            && ItemID != VSConstants.VSITEMID_ROOT
                            && ItemID != VSConstants.VSITEMID_SELECTION)
                        {
                            ClearErrorList(pHier, ItemID);
                        }

                        if (pbstrsMrDocument != null
                            && ((pgrfRDTFlags & (uint)_VSRDTFLAGS.RDT_ProjSlnDocument) == 0))
                        {
                            var uri = Utils.FileName2Uri(pbstrsMrDocument);
                            if (uri != null
                                && _package != null
                                && _package.ModelManager != null
                                && _package.ModelManager.GetArtifact(uri) != null)
                            {
                                // dispose of the artifact now that no-one has a lock on it any longer. 
                                if (String.IsNullOrEmpty(pbstrsMrDocument) == false)
                                {
                                    OnBeforeLastDesignerDocumentUnlock(uri);

                                    EditingContextManager.CloseArtifact(uri);

                                    OnAfterLastDesignerDocumentUnlock(uri);
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChangeEx(
            uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew,
            uint itemidNew, string pszMkDocumentNew)
        {
            return NativeMethods.S_OK;
        }

        public int OnBeforeSave(uint docCookie)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            OnAfterSave();
            return VSConstants.S_OK;
        }

        #endregion

        #region IVsSelectionEvents Members

        /// <summary>
        ///     this event is triggered when the user opens/closes/switches to a different document window
        /// </summary>
        /// <param name="elementid"></param>
        /// <param name="varValueOld"></param>
        /// <param name="varValueNew"></param>
        /// <returns></returns>
        public virtual int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
            // don't allow this method to be re-entrant.  That causes some bugs (eg, calling SetDocumentContext below 
            // will trigger another selection changed event. 
            if (!_handlingOnElementValueChanged)
            {
                try
                {
                    _handlingOnElementValueChanged = true;

                    if (elementid == (uint)VSConstants.VSSELELEMID.SEID_DocumentFrame)
                    {
                        // Temporary fix for bug 555830.
                        // On solution closing, don't set the state of Command UI context because this might cause unhandled exception in VS (tracked by 556145).
                        var solution = _package.GetService(typeof(IVsSolution)) as IVsSolution;
                        if (solution != null)
                        {
                            object isSolutionClosing = false;

                            NativeMethods.ThrowOnFailure(
                                solution.GetProperty((int)__VSPROPID2.VSPROPID_IsSolutionClosing, out isSolutionClosing));

                            if ((bool)isSolutionClosing)
                            {
                                return NativeMethods.S_OK;
                            }
                        }

                        // Skip the operation when the user switching between diagram windows of the same model.
                        var newFrame = CreateFrameWrapper(varValueNew as IVsWindowFrame);
                        var oldFrame = CreateFrameWrapper(varValueOld as IVsWindowFrame);

                        if (newFrame != null
                            && oldFrame != null
                            && newFrame.Uri == oldFrame.Uri)
                        {
                            return NativeMethods.S_OK;
                        }

                        if (varValueOld != varValueNew)
                        {
                            ReloadArtifactIfNecessary(newFrame);
                            if (oldFrame.ShouldShowToolWindows)
                            {
                                SetCurrentContext(null);
                            }

                            if (newFrame.ShouldShowToolWindows)
                            {
                                if (_doNotChangeArtifactInBrowserForNextOpeningDoc)
                                {
                                    // make sure schema is opened in the context of the current schema set
                                    _editingContextMgr.SetCurrentUri(newFrame, newFrame.Uri);
                                    _doNotChangeArtifactInBrowserForNextOpeningDoc = false;
                                }
                            }

                            // This will show or hide the tool windows for the new window frame
                            UpdateToolWindowsAndCmdsForFrame(newFrame);
                        }
                    }
                }
                finally
                {
                    _handlingOnElementValueChanged = false;
                }
            }
            return NativeMethods.S_OK;
        }

        public int OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
        {
            return NativeMethods.S_OK;
        }

        public int OnSelectionChanged(
            IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew,
            uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
        {
            return NativeMethods.S_OK;
        }

        #endregion
    }
}
