// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
#if VS12ORNEWER
    using Microsoft.Data.Entity.Design.VisualStudio;
#endif
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.XmlEditor;
    using IServiceProvider = System.IServiceProvider;
    using XmlModel = Microsoft.Data.Tools.XmlDesignerBase.Model.XmlModel;

    /// <summary>
    ///     The VS implementation of the XmlModelProvider. This uses
    ///     our VSModelInformationService to provide the model data.
    /// </summary>
    internal sealed class VSXmlModelProvider : XmlModelProvider
    {
        private readonly IServiceProvider _services;
        private XmlStore _xmlStore;
        private Dictionary<Uri, VSXmlModel> _xmlModels = new Dictionary<Uri, VSXmlModel>();

        private Dictionary<XmlEditingScope, VSXmlTransaction> _txDictionary =
            new Dictionary<XmlEditingScope, VSXmlTransaction>();

        private readonly IXmlDesignerPackage _xmlDesignerPackage;

        /// <summary>
        ///     Create a new XML model provider.
        /// </summary>
        public VSXmlModelProvider(IServiceProvider services, IXmlDesignerPackage xmlDesignerPackage)
        {
            Debug.Assert(services != null);
            Debug.Assert(xmlDesignerPackage != null);
            _xmlDesignerPackage = xmlDesignerPackage;
            _services = services;
            if (_xmlStore == null)
            {
                var xmlEditorService = (XmlEditorService)services.GetService(
                    typeof(XmlEditorService));
                _xmlStore = xmlEditorService.CreateXmlStore();
                _xmlStore.EditingScopeCompleted += OnXmlModelTransactionCompleted;
                _xmlStore.UndoRedoCompleted += OnXmlModelUndoRedoCompleted;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_txDictionary != null)
                {
                    try
                    {
                        foreach (var tx in _txDictionary.Values)
                        {
                            tx.Dispose();
                        }
                    }
                    finally
                    {
                        _txDictionary = null;
                    }
                }

                if (_xmlModels != null)
                {
                    try
                    {
                        foreach (var vsXmlModel in _xmlModels.Values)
                        {
                            vsXmlModel.Dispose();
                        }
                    }
                    finally
                    {
                        _xmlModels = null;
                    }
                }

                if (_xmlStore != null)
                {
                    try
                    {
                        _xmlStore.EditingScopeCompleted -= OnXmlModelTransactionCompleted;
                        _xmlStore.UndoRedoCompleted -= OnXmlModelUndoRedoCompleted;
                        _xmlStore.Dispose();
                    }
                    finally
                    {
                        _xmlStore = null;
                    }
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///     Returns the Xml model for a given file token, or null
        ///     if there is no Xml model for the token.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override XmlModel GetXmlModel(Uri sourceUri)
        {
            Debug.Assert(_xmlDesignerPackage.IsForegroundThread, "Can't request an XmlModel on background thread");

            // assert that an entry already exists in the RDT for this document and confirm with the designer package that the entry
            // is owned by the designer.  If this is not the case, the xml editor will create a doc data, and this may not be the doc 
            // data we want.  This can result in failure to open the document in the desired designer because the editor is incorrect.
#if DEBUG
            var skipChecks = false;

#if VS12ORNEWER
    // The behaviour of VsShellUtilities.IsDocumentOpen changed in VS2013. In VS2012 IsDocumentOpen would return true if the document
    // has been loaded even though loading the solution has not finished yet. In VS2013 IsOpenDocument returns false if the solution 
    // is still being loaded. This caused multiple asserts when opening a project after VS was closed when edmx file was active/opened
    // See http://entityframework.codeplex.com/workitem/1163 for more details and repro steps.
            var solution = (IVsSolution)_services.GetService(typeof (IVsSolution));
            object propertyValue;
            if(solution != null && NativeMethods.Succeeded(solution.GetProperty((int)__VSPROPID2.VSPROPID_IsSolutionOpeningDocs, out propertyValue)))
            {
                skipChecks = true;
            }
#endif

            // Alert: when we try to load the XmlModel for diagram file, the document is not opened in VS.
            //  The If statement is added to skip the check for diagram files.
            if (!skipChecks
                && sourceUri.LocalPath.EndsWith(".edmx", StringComparison.OrdinalIgnoreCase))
            {
                IVsUIHierarchy hier;
                uint itemId;
                IVsWindowFrame windowFrame;
                var isDocumentOpen = VsShellUtilities.IsDocumentOpen(
                    _services, sourceUri.LocalPath, Guid.Empty, out hier, out itemId, out windowFrame);

                Debug.Assert(isDocumentOpen, "Running Document Table does not contain document in GetXmlModel()");
                if (isDocumentOpen)
                {
                    var frameWrapper = _xmlDesignerPackage.DocumentFrameMgr.CreateFrameWrapper(windowFrame);
                    Debug.Assert(frameWrapper != null, "Could not construct FrameWrapper for IVsWindowFrame in debug code in GetXmlModel()");
                    if (frameWrapper != null)
                    {
                        Debug.Assert(
                            frameWrapper.IsDesignerDocInDesigner,
                            "We are trying to GetXmlModel() for a document that is not owned by the designer");
                    }
                }
            }
#endif

            VSXmlModel vsXmlModel = null;
            if (!_xmlModels.TryGetValue(sourceUri, out vsXmlModel))
            {
                Microsoft.VisualStudio.XmlEditor.XmlModel xmlModel = null;
                try
                {
                    xmlModel = _xmlStore.OpenXmlModel(sourceUri);
                }
                catch (Exception)
                {
                    xmlModel = null;
                }
                if (xmlModel != null)
                {
                    vsXmlModel = new VSXmlModel(_services, xmlModel);
                    _xmlModels.Add(sourceUri, vsXmlModel);
                }
            }

            return vsXmlModel;
        }

        public override IEnumerable<XmlModel> OpenXmlModels
        {
            get
            {
                foreach (var xmlModel in _xmlModels.Values)
                {
                    yield return xmlModel;
                }
            }
        }

        public override void CloseXmlModel(Uri xmlModelUri)
        {
            VSXmlModel vsXmlModel = null;
            // Note: _xmlModels can be null if we have already been disposed
            if (_xmlModels != null
                && _xmlModels.TryGetValue(xmlModelUri, out vsXmlModel))
            {
                _xmlModels.Remove(xmlModelUri);
                vsXmlModel.Dispose();
            }
            base.CloseXmlModel(xmlModelUri);
        }

        public override XmlTransaction BeginTransaction(string name, object userState)
        {
            var editorTx = _xmlStore.BeginEditingScope(name, userState);
            var tx = GetTransaction(editorTx);
            return tx;
        }

        public override void BeginUndoScope(string name)
        {
            var pum = UndoManager as ParentUndoManager;
            if (pum != null)
            {
                pum.StartParentUndoScope(name);
            }
        }

        public override void EndUndoScope()
        {
            var pum = UndoManager as ParentUndoManager;
            if (pum != null)
            {
                pum.CloseParentUndoScope();
            }
        }

        public override XmlTransaction CurrentTransaction
        {
            get
            {
                if (_xmlStore.CurrentEditingScope == null)
                {
                    return null;
                }
                return GetTransaction(_xmlStore.CurrentEditingScope);
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal VSXmlTransaction GetTransaction(XmlEditingScope editorTx)
        {
            VSXmlTransaction tx = null;
            if (!(_txDictionary.TryGetValue(editorTx, out tx)))
            {
                tx = new VSXmlTransaction(this, editorTx);
                _txDictionary[editorTx] = tx;
            }
            return tx;
        }

        private void OnXmlModelTransactionCompleted(object senderId, XmlEditingScopeEventArgs e)
        {
            _xmlDesignerPackage.InvokeOnForeground(
                () =>
                    {
                        var designerTx = ((senderId != null) && (senderId is XmlStore) && (senderId == _xmlStore));
                        XmlTransaction tx = GetTransaction(e.EditingScope);
                        var args = new XmlTransactionEventArgs(tx, designerTx);
                        OnTransactionCompleted(args);
                        _txDictionary.Remove(e.EditingScope);
                    });
        }

        private void OnXmlModelUndoRedoCompleted(object senderId, XmlEditingScopeEventArgs e)
        {
            _xmlDesignerPackage.InvokeOnForeground(
                () =>
                    {
                        var designerTx = ((senderId != null) && (senderId is XmlStore) && (senderId == _xmlStore));
                        XmlTransaction tx = GetTransaction(e.EditingScope);
                        var args = new XmlTransactionEventArgs(tx, designerTx);
                        OnUndoRedoCompleted(args);
                        _txDictionary.Remove(e.EditingScope);
                    });
        }

        public override bool RenameXmlModel(Uri oldName, Uri newName)
        {
            VSXmlModel vsXmlModel = null;
            if (_xmlModels.TryGetValue(oldName, out vsXmlModel))
            {
                _xmlModels.Remove(oldName);
                _xmlModels.Add(newName, vsXmlModel);
                Debug.Assert(new Uri(vsXmlModel.Name) == newName);
                return true;
            }
            return false;
        }

        public IOleUndoManager UndoManager
        {
            get { return _xmlStore != null ? _xmlStore.UndoManager : null; }
            set
            {
                Debug.Assert(_xmlStore != null);
                if (_xmlStore != null)
                {
                    _xmlStore.UndoManager = value;
                }
            }
        }
    }
}
