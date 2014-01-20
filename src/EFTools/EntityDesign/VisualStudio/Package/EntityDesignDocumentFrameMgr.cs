// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.VisualStudio.Model;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    // <summary>
    //     The EntityDesignDocumentFrameMgr class manages all document window frames that
    //     are associated to an EDMX file document if they were loaded in Escher or in the XML editor
    // </summary>
    internal class EntityDesignDocumentFrameMgr : DocumentFrameMgr
    {
        private readonly HashSet<Uri> _dirtyArtifactsOnClose = null;

        internal EntityDesignDocumentFrameMgr(IXmlDesignerPackage package)
            : base(package)
        {
        }

        protected internal override FrameWrapper CreateFrameWrapper(IVsWindowFrame frame)
        {
            return new EntityDesignFrameWrapper(frame);
        }

        // <summary>
        //     This method will set the editing context for the mapping details and model browser. This will
        //     also show/hide these tool windows.
        // </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected internal override void SetCurrentContext(EditingContext context)
        {
            try
            {
                if (PackageManager.Package != null)
                {
                    if (PackageManager.Package.MappingDetailsWindow != null)
                    {
                        PackageManager.Package.MappingDetailsWindow.Context = context;
                    }
                    if (PackageManager.Package.ExplorerWindow != null)
                    {
                        PackageManager.Package.ExplorerWindow.Context = context;
                    }
                }
            }
            catch
            {
                // Hack Hack; FindToolWindow will throw an exception if we can't find the ExplorerWindow. We should find
                // a more specific way to handle this if we run into this situation.
            }
        }

        protected override void ClearErrorList(Uri oldUri, Uri newUri)
        {
            // clear out the error list so after the rename the errors are bound to the correct artifacts
            ErrorListHelper.ClearErrorsForDocAcrossLists(newUri);

            // for a save-as the errors are associated with the oldUri
            ErrorListHelper.ClearErrorsForDocAcrossLists(oldUri);
        }

        protected override void ClearErrorList(IVsHierarchy pHier, uint ItemID)
        {
            ErrorListHelper.ClearErrorsForDocAcrossLists(pHier, ItemID);
        }

        protected override bool HasDesignerExtension(Uri uri)
        {
            return VSArtifact.GetVSArtifactFileExtensions().Contains(Path.GetExtension(uri.LocalPath));
        }

        protected override void OnAfterDesignerDocumentWindowHide(Uri docUri)
        {
            // see if the browser is showing this
            var explorerWindow = PackageManager.Package.ExplorerWindow;
            if (explorerWindow != null)
            {
                var explorerUri = EditingContextManager.GetArtifactUri(explorerWindow.Context);
                if (UriComparer.OrdinalIgnoreCase.Equals(docUri, explorerUri))
                {
                    // the browser's Uri is closing, so clear out the browser
                    explorerWindow.Context = null;
                }
            }

            // see if the mapping window is showing this
            var mappingWindow = PackageManager.Package.MappingDetailsWindow;
            if (mappingWindow != null)
            {
                var mappingUri = EditingContextManager.GetArtifactUri(mappingWindow.Context);
                if (UriComparer.OrdinalIgnoreCase.Equals(docUri, mappingUri))
                {
                    // the mapping's Uri is closing, so clear it out
                    mappingWindow.Context = null;
                }
            }
        }

        public override int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            if (fFirstShow != 0)
            {
                if (pFrame != null)
                {
                    var frameWrapper = new EntityDesignFrameWrapper(pFrame);
                    if (frameWrapper.IsEscherDocInXmlEditor)
                    {
                        // we have an EDMX file that is being opened in the XML editor so we want to validate
                        // so users can fix up safe-mode errors or even non-safe mode errors
                        VisualStudioEdmxValidator.LoadAndValidateFiles(frameWrapper.Uri);
                    }
                }
            }
            return NativeMethods.S_OK;
        }

        protected override void OnAfterSave()
        {
            base.OnAfterSave();

            if (_dirtyArtifactsOnClose != null)
            {
                _dirtyArtifactsOnClose.Clear();
            }
        }

        protected override void OnBeforeLastDesignerDocumentUnlock(Uri docUri)
        {
            var vsArtifact = CurrentArtifact as VSArtifact;
            if (vsArtifact != null && vsArtifact.Uri == docUri
                && vsArtifact.LayerManager != null)
            {
                vsArtifact.LayerManager.Unload();
            }
        }

        public override int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
            var hr = base.OnElementValueChanged(elementid, varValueOld, varValueNew);

            if (elementid == (uint)VSConstants.VSSELELEMID.SEID_DocumentFrame)
            {
                if (varValueOld != null)
                {
                    var oldFrame = new EntityDesignFrameWrapper(varValueOld as IVsWindowFrame);
                    if (oldFrame.IsEscherDocInEntityDesigner)
                    {
                        var oldVsArtifact = PackageManager.Package.ModelManager.GetArtifact(oldFrame.Uri) as VSArtifact;
                        if (oldVsArtifact != null)
                        {
                            oldVsArtifact.LayerManager.Unload();
                        }
                    }
                }

                if (varValueNew != null)
                {
                    var newFrame = new EntityDesignFrameWrapper(varValueNew as IVsWindowFrame);
                    if (newFrame.IsEscherDocInEntityDesigner)
                    {
                        var vsArtifact = PackageManager.Package.ModelManager.GetArtifact(newFrame.Uri) as VSArtifact;
                        if (vsArtifact != null)
                        {
                            vsArtifact.LayerManager.Load();
                        }
                    }
                }
            }

            return hr;
        }
    }
}
