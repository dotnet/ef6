// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Refactoring
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    [Guid(RefactoringGuids.RefactoringPreviewChangesEngineString)]
    internal sealed class PreviewChangesEngine : IVsPreviewChangesEngine, IDisposable
    {
        private readonly Func<bool> _applyChangesDelegate;
        private readonly PreviewData _previewData;
        private readonly PreviewBuffer _previewBuffer;
        private bool _disposed;

        public PreviewChangesEngine(Func<bool> applyChangesDelegate, PreviewData previewData, IServiceProvider serviceProvider)
        {
            // Cache the PreviewData from the operation.
            _applyChangesDelegate = applyChangesDelegate;
            _previewData = previewData;
            _previewBuffer = new PreviewBuffer(serviceProvider);
        }

        #region IVsPreviewChangesEngine Members

        /// <summary>
        ///     Callback to RefactorOperation.ApplyChanges
        /// </summary>
        /// <returns></returns>
        public int ApplyChanges()
        {
            if (_applyChangesDelegate())
            {
                return VSConstants.S_OK;
            }
            else
            {
                return VSConstants.E_FAIL;
            }
        }

        /// <summary>
        ///     Get top level preview group nodes.
        /// </summary>
        /// <param name="ppIUnknownPreviewChangesList"></param>
        /// <returns></returns>
        public int GetRootChangesList(out object ppIUnknownPreviewChangesList)
        {
            // First create root preview list
            var previewChangesList = new PreviewChangesList(_previewData.ChangeList, _previewData, _previewBuffer);
            ppIUnknownPreviewChangesList = previewChangesList;
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Text for Apply button
        /// </summary>
        /// <param name="pbstrConfirmation"></param>
        /// <returns></returns>
        public int GetConfirmation(out string pbstrConfirmation)
        {
            pbstrConfirmation = _previewData.ConfirmButtonText;
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Description of this refactoring operation in preview dialog
        /// </summary>
        /// <param name="pbstrDescription"></param>
        /// <returns></returns>
        public int GetDescription(out string pbstrDescription)
        {
            pbstrDescription = _previewData.Description;
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Help context of the preview dialog.
        /// </summary>
        /// <param name="pbstrHelpContext"></param>
        /// <returns></returns>
        public int GetHelpContext(out string pbstrHelpContext)
        {
            pbstrHelpContext = _previewData.HelpContext;
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Description of the text view at bottom half of the preview dialog
        /// </summary>
        /// <param name="pbstrTextViewDescription"></param>
        /// <returns></returns>
        public int GetTextViewDescription(out string pbstrTextViewDescription)
        {
            pbstrTextViewDescription = _previewData.TextViewDescription;
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Title of the preview dialog.
        /// </summary>
        /// <param name="pbstrTitle"></param>
        /// <returns></returns>
        public int GetTitle(out string pbstrTitle)
        {
            pbstrTitle = _previewData.Title;
            return VSConstants.S_OK;
        }

        /// <summary>
        ///     Warning message in preview dialog.
        /// </summary>
        /// <param name="pbstrWarning"></param>
        /// <param name="ppcwlWarningLevel"></param>
        /// <returns></returns>
        public int GetWarning(out string pbstrWarning, out int ppcwlWarningLevel)
        {
            pbstrWarning = _previewData.Warning;
            ppcwlWarningLevel = (int)_previewData.WarningLevel;
            return VSConstants.S_OK;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            _disposed = true;
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _previewBuffer.Dispose();
            }
        }

        #endregion
    }
}
