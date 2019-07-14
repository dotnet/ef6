// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Refactoring
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Common;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Data.Tools.Design.XmlCore;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;

    internal sealed class PreviewBuffer : IDisposable
    {
        // Dictionary used to store preview temp file information used in one preview session.
        // It's the mapping between file extension and the preview temp file info.
        // Each file extension will have one preview temp file.
        private readonly Dictionary<string, PreviewTempFile> _tempFiles;
        private bool _disposed;

        private IVsTextView _lastTextView;
        private FileChange _lastDisplayedFileChange;
        private PreviewChangesNode _lastDisplayedNode;
        private readonly IVsTextLines _bufferForNoChanges;

        public PreviewBuffer(IServiceProvider serviceProvider)
        {
            _tempFiles = new Dictionary<string, PreviewTempFile>();
            _bufferForNoChanges = CreateEmptyEditor(serviceProvider);
        }

        private static IVsTextLines CreateEmptyEditor(IServiceProvider serviceProvider)
        {
            IVsTextLines vsTextLines = null;
            // get the ILocalRegistry interface so we can use it to create the text buffer from the shell's local registry
            var localRegistry = (ILocalRegistry)serviceProvider.GetService(typeof(ILocalRegistry));
            if (localRegistry != null)
            {
                vsTextLines = VsTextBufferFactory.CreateInstance<IVsTextLines>(serviceProvider, localRegistry);
                // Initialize contents
                NativeMethods.ThrowOnFailure(vsTextLines.InitializeContent(string.Empty, string.Empty.Length));
            }

            return vsTextLines;
        }

        /// <summary>
        ///     Get the preview temp file for a file extension.
        ///     If the temp file already exists, use that one.  Otherwise, create a new one.
        /// </summary>
        /// <param name="fileExtension">The file extension.</param>
        /// <returns>The preview temp file with file path, invisible editor and text buffer information.</returns>
        private PreviewTempFile GetPreviewTempFile(string fileExtension)
        {
            ArgumentValidation.CheckForNullReference(fileExtension, "fileExtension");

            PreviewTempFile tempFile = null;
            // If the temp file already exists, use that one.
            if (!_tempFiles.TryGetValue(fileExtension, out tempFile))
            {
                var failed = false;
                // Create a new preview temp file
                var tempFilePath = CreatePreviewTempFile(fileExtension);
                if (!File.Exists(tempFilePath))
                {
                    // Failed to create the temp file.
                    failed = true;
                }
                else
                {
                    // Open the file in invisible editor and get text buffer from it.
                    IVsInvisibleEditor invisibleEditor = null;
                    IVsTextLines textBuffer = null;
                    if (RdtManager.Instance.TryGetTextLinesAndInvisibleEditor(tempFilePath, out invisibleEditor, out textBuffer)
                        && invisibleEditor != null
                        && textBuffer != null)
                    {
                        // Temp file is opened in invisible editor, and we got the text buffer from it.
                        tempFile = new PreviewTempFile(tempFilePath, invisibleEditor, textBuffer);
                        _tempFiles.Add(fileExtension, tempFile);
                    }
                    else
                    {
                        failed = true;
                    }
                }
                if (failed)
                {
                    // Failed to get InvisibleEditor or TextBuffer for that file,
                    // throw exception.
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.Exception_CannotGetTextBuffer,
                            tempFilePath));
                }
            }
            return tempFile;
        }

        /// <summary>
        ///     This method will refresh the text view when preview request is changed.
        ///     It will be called when some node is checked/unchecked.
        /// </summary>
        public void RefreshTextView()
        {
            if (_lastTextView != null
                && _lastDisplayedFileChange != null
                && _lastDisplayedNode != null)
            {
                DisplayPreview(_lastTextView, _lastDisplayedFileChange, _lastDisplayedNode);
            }
        }

        /// <summary>
        ///     Display the refactoring preview for a selected preview changes node.
        /// </summary>
        /// <param name="vsTextView">The text view to show the file contents.</param>
        /// <param name="fileChange">All changes in one file.</param>
        /// <param name="node">The selected PreviewChangesNode.</param>
        public void DisplayPreview(IVsTextView vsTextView, FileChange fileChange, PreviewChangesNode node)
        {
            ArgumentValidation.CheckForNullReference(vsTextView, "vsTextView");
            ArgumentValidation.CheckForNullReference(node, "previewChangeNode");

            if (fileChange != null)
            {
                // Get the temp file for this file extension, and copy all file content to the temp file text buffer
                PreviewTempFile tempFile = null;
                try
                {
                    tempFile = GetPreviewTempFile(Path.GetExtension(fileChange.FileName));
                }
                catch (InvalidOperationException)
                {
                    // Failed to get text buffer, just set the text view to nothing.
                    NativeMethods.ThrowOnFailure(vsTextView.SetBuffer(_bufferForNoChanges));
                    return;
                }

                // Copy the content of source file to that temp file text buffer
                CopyFileToBuffer(fileChange.FileName, tempFile.TextBuffer);

                // Create text markers on all changes on this file
                RefactoringOperationBase.ApplyChangesToOneFile(fileChange, tempFile.TextBuffer, true, node.ChangeProposal);

                // Set language service ID
                // Get Language service ID on this change node
                var languageServiceID = node.LanguageServiceID;
                if (languageServiceID == Guid.Empty
                    && node.ChildList != null
                    && node.ChildList.Count > 0)
                {
                    // If can not get the language service ID, check if it has child nodes
                    // if so, get the language service ID for first change in this file node.
                    languageServiceID = node.ChildList[0].LanguageServiceID;
                }
                if (languageServiceID != Guid.Empty)
                {
                    NativeMethods.ThrowOnFailure(tempFile.TextBuffer.SetLanguageServiceID(ref languageServiceID));
                }

                // Set the vsTextView with textBuffer
                NativeMethods.ThrowOnFailure(vsTextView.SetBuffer(tempFile.TextBuffer));

                // Ensure visible of first line and set the caret to position (0,0)
                ScrollInView(vsTextView, 0, 0, 0, 1);

                // If there is ChangeProposal, make sure that change is visible in the text view.
                // If ChangeProposal is null, that might be file node, make the first change visible.
                // Here we will only work with Text based change proposal.
                var visibleChange = node.ChangeProposal as TextChangeProposal;
                if (visibleChange == null)
                {
                    // Try to get first change in this file
                    if (node.ChildList != null
                        && node.ChildList.Count > 0)
                    {
                        visibleChange = node.ChildList[0].ChangeProposal as TextChangeProposal;
                    }
                }

                if (visibleChange != null)
                {
                    // There are some changes, create TextSpan for first change, 
                    // and make the cursor position to that TextSpan.
                    ScrollInView(
                        vsTextView, visibleChange.StartLine, visibleChange.StartColumn,
                        visibleChange.EndLine, visibleChange.EndColumn);
                }

                // Save the state, this will be used to refresh the text view when preview request
                // changed, such as check/uncheck
                _lastTextView = vsTextView;
                _lastDisplayedFileChange = fileChange;
                _lastDisplayedNode = node;
            }
            else
            {
                // No related file to this node, set nothing for the text view.
                NativeMethods.ThrowOnFailure(vsTextView.SetBuffer(_bufferForNoChanges));
            }
        }

        private static void ScrollInView(IVsTextView vsTextView, int startLine, int startColumn, int endLine, int endColumn)
        {
            ArgumentValidation.CheckForNullReference(vsTextView, "vsTextView");

            var textSpan = new TextSpan();
            textSpan.iStartLine = startLine;
            textSpan.iEndLine = endLine;
            textSpan.iStartIndex = startColumn;
            textSpan.iEndIndex = endColumn;
            NativeMethods.ThrowOnFailure(vsTextView.EnsureSpanVisible(textSpan));
            NativeMethods.ThrowOnFailure(vsTextView.SetCaretPos(textSpan.iStartLine, textSpan.iStartIndex));
        }

        /// <summary>
        ///     Copy file content to the TextLines buffer
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults",
            MessageId =
                "Microsoft.VisualStudio.TextManager.Interop.IVsTextLines.ReplaceLines(System.Int32,System.Int32,System.Int32,System.Int32,System.IntPtr,System.Int32,Microsoft.VisualStudio.TextManager.Interop.TextSpan[])"
            )]
        private static void CopyFileToBuffer(string sourceFilename, IVsTextLines buffer)
        {
            ArgumentValidation.CheckForEmptyString(sourceFilename, "sourceFileName");
            ArgumentValidation.CheckForNullReference(buffer, "buffer");

            var spanDst = GetBufferSpan(buffer);

            var pContent = IntPtr.Zero;
            try
            {
                var content = RdtManager.Instance.ReadFromFile(sourceFilename);
                pContent = Marshal.StringToHGlobalAuto(content);
                buffer.ReplaceLines(
                    spanDst.iStartLine, spanDst.iStartIndex,
                    spanDst.iEndLine, spanDst.iEndIndex, pContent, content.Length, new[] { new TextSpan() });
            }
            finally
            {
                if (pContent != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pContent);
                }
            }
        }

        /// <summary>
        ///     Get entire TextLines buffer as a TextSpan
        /// </summary>
        /// <param name="pBuffer"></param>
        /// <returns></returns>
        private static TextSpan GetBufferSpan(IVsTextLines textBuffer)
        {
            ArgumentValidation.CheckForNullReference(textBuffer, "textBuffer");

            var lineCount = 0;
            var charCount = 0;

            // Get line count for the whole buffer.
            var result = textBuffer.GetLineCount(out lineCount);
            if (result == VSConstants.S_OK
                && lineCount > 0)
            {
                // Get char count for last line.
                result = textBuffer.GetLengthOfLine(lineCount - 1, out charCount);
                if (result != VSConstants.S_OK)
                {
                    charCount = 0;
                }
            }
            else
            {
                lineCount = 0;
            }

            // Create a TextSpan from begin to end of the text buffer.
            var span = new TextSpan();
            span.iStartLine = 0;
            span.iStartIndex = 0;
            span.iEndLine = lineCount - 1 > 0 ? lineCount - 1 : 0;
            span.iEndIndex = charCount > 0 ? charCount : 0;
            return span;
        }

        /// <summary>
        ///     Create a preview temp file, so we can open it to get TextLines buffer
        /// </summary>
        /// <param name="fileExtension"></param>
        /// <returns></returns>
        private static string CreatePreviewTempFile(string fileExtension)
        {
            ArgumentValidation.CheckForEmptyString(fileExtension, "fileExtension");

            string tempFileName = null;
            FileUtils.CreateUniqueFilename("vstdRefactoring", fileExtension, out tempFileName);
            return tempFileName;
        }

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
                var dispBufferForNoChanges = _bufferForNoChanges as IDisposable;
                if (dispBufferForNoChanges != null)
                {
                    dispBufferForNoChanges.Dispose();
                }

                // Release invisible editor
                if (_tempFiles != null)
                {
                    foreach (var tempFile in _tempFiles.Values)
                    {
                        if (tempFile.InvisibleEditor != null)
                        {
                            Marshal.ReleaseComObject(tempFile.InvisibleEditor);
                            tempFile.InvisibleEditor = null;
                        }

                        // Delete all temp files
                        if (File.Exists(tempFile.TempFileFullPath))
                        {
                            try
                            {
                                File.Delete(tempFile.TempFileFullPath);
                            }
                            catch (ArgumentException)
                            {
                                // If the file cannot be deleted, ignore it.
                            }
                            catch (DirectoryNotFoundException)
                            {
                                // If the file cannot be deleted, ignore it.
                            }
                            catch (IOException)
                            {
                                // If the file cannot be deleted, ignore it.
                            }
                            catch (NotSupportedException)
                            {
                                // If the file cannot be deleted, ignore it.
                            }
                            catch (UnauthorizedAccessException)
                            {
                                // If the file cannot be deleted, ignore it.
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Inner class PreviewTempFile

        /// <summary>
        ///     A class stores preview temp file infomation
        /// </summary>
        private class PreviewTempFile
        {
            private readonly string _tempFileFullPath;
            private readonly IVsTextLines _textBuffer;

            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="fileFullPath">Full path of the file.</param>
            /// <param name="invisibleEditor">Invisible editor for that temp file.</param>
            /// <param name="textBuffer">Text buffer for that temp file.</param>
            public PreviewTempFile(string fileFullPath, IVsInvisibleEditor invisibleEditor, IVsTextLines textBuffer)
            {
                ArgumentValidation.CheckForEmptyString(fileFullPath, "fileFullPath");
                ArgumentValidation.CheckForNullReference(invisibleEditor, "invisibleEditor");
                ArgumentValidation.CheckForNullReference(textBuffer, "textBuffer");

                _tempFileFullPath = fileFullPath;
                InvisibleEditor = invisibleEditor;
                _textBuffer = textBuffer;
            }

            /// <summary>
            ///     Full path file name for the preview temp file
            /// </summary>
            public string TempFileFullPath
            {
                get { return _tempFileFullPath; }
            }

            /// <summary>
            ///     The invisible editor used to open the preview temp file
            /// </summary>
            public IVsInvisibleEditor InvisibleEditor { get; set; }

            /// <summary>
            ///     The text buffer associated with that preview temp file
            /// </summary>
            public IVsTextLines TextBuffer
            {
                get { return _textBuffer; }
            }
        }

        #endregion //Inner class PreviewTempFile
    }
}
