// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Refactoring
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Common;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Data.Tools.Design.XmlCore;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;

    /// <summary>
    ///     The RefactorOperation base class drives the actual lifecycle of the refactoring operation.
    ///     The derived classes methods are invoked by the base class methods.
    ///     </para>
    ///     <para>
    ///         The RefactoringOperation performs the following functions:
    ///         - serves as the launch point for a refactoring operation
    ///         - presents UI to the user to gather required input for the refactoring operation
    ///         - the base class provides services such as the preview UI
    ///     </para>
    ///     <para>
    ///         Notes:
    ///         - UI thread threaded, and blocks only when presenting UI
    ///     </para>
    /// </summary>
    internal abstract class RefactoringOperationBase
    {
        /// <summary>
        ///     Raised before changes are applied
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public EventHandler<ApplyChangesEventArgs> ApplyingChanges;

        /// <summary>
        ///     Raised after changes are applied
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public EventHandler<ApplyChangesEventArgs> AppliedChanges;

        private bool _hasPreviewWindow = true;
        private PreviewData _previewData;
        private readonly IServiceProvider _serviceProvider;
        private readonly WaitDialog _waitDialog;

        // HashSet to prevent multiple registration of events on reentrant contributors
        private readonly HashSet<Type> _applyingChangesTypes = new HashSet<Type>();
        private readonly HashSet<Type> _appliedChangesTypes = new HashSet<Type>();

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal RefactoringOperationBase(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _waitDialog = new WaitDialog(serviceProvider, OperationNameDescription, cancelable: true);

            IsCancelled = false;
            ErrorOccurred = false;
            _hasPreviewWindow = true;
            FileChanges = new List<FileChange>();
        }

        #region Properties

        // TODO: investigate whether we can change this to abstract
        protected virtual string AnalysisWarningText
        {
            get { return ""; }
        }

        internal uint ApplyChangesUndoScope { get; set; }

        /// <summary>
        ///     The initial ContributorInput for this RefactorOperation
        /// </summary>
        internal ContributorInput ContributorInput { get; set; }

        protected bool ErrorOccurred { get; set; }

        protected IList<FileChange> FileChanges { get; set; }

        /// <summary>
        ///     The HasPreviewWindow flag dictates whether the standard Preview Window is displayed.
        ///     This allows certain refactoring operations to choose not to display the Preview Window.
        /// </summary>
        public bool HasPreviewWindow
        {
            get { return _hasPreviewWindow; }
            internal set { _hasPreviewWindow = value; }
        }

        protected bool IsCancelled { get; set; }

        /// <summary>
        ///     Get the non-localized name of this operation.
        /// </summary>
        protected internal abstract string OperationName { get; }

        /// <summary>
        ///     Get the localized descriptive name of this operation.
        /// </summary>
        protected internal abstract string OperationNameDescription { get; }

        /// <summary>
        ///     Preview data used in preview changes window
        /// </summary>
        internal PreviewData PreviewData
        {
            get
            {
                if (_previewData == null)
                {
                    _previewData = new PreviewData(PreviewWindowInfo);
                }
                return _previewData;
            }
            set { _previewData = value; }
        }

        /// <summary>
        ///     Preview window information, including window title, description,
        ///     confirm button text, warning text, etc.
        /// </summary>
        protected abstract PreviewWindowInfo PreviewWindowInfo { get; }

        protected IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        /// <summary>
        ///     Undo Description used in undo/redo stack for this RefactorOperation
        /// </summary>
        protected abstract string UndoDescription { get; }

        /// <summary>
        ///     Returns true if the wait dialog was canceled
        /// </summary>
        internal bool WaitCanceled
        {
            get { return _waitDialog.HasCanceled(); }
        }

        protected WaitDialog WaitDialog
        {
            get { return _waitDialog; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The DoOperation method begins the entire refactoring process.
        ///     The launch point for the refactoring operation must call this method.
        ///     overall flow
        ///     1) UI flows input into an instance of a derivation of this class ... (ctor)
        ///     2) The class may present its own UI (cancel)
        ///     3) It creates an ContributorInput object
        ///     4) Feeds it to the manager (or base class, depending on how you look at it)
        ///     5) Proposals returned
        ///     6) Merge proposals
        ///     7) Preview UI (optional)kind
        ///     8) Apply changes (or cancel or fail)
        /// </summary>
        public void DoOperation()
        {
            try
            {
                // If we don't have a ContributorInput yet, call OnGetContributorInput method to get ContributorInput for this operation.
                if (ContributorInput == null)
                {
                    ContributorInput = OnGetContributorInput();
                }

                if (ErrorOccurred || IsCancelled)
                {
                    return;
                }
                if (ContributorInput == null)
                {
                    // If not cancelled and contributorInput is null, throw exception.
                    throw new InvalidOperationException();
                }

                // Collect the list of files to change
                FileChanges = GetFileChanges();

                using (var cursor = WaitCursorHelper.NewWaitCursor())
                {
                    if (HasPreviewWindow)
                    {
                        // Log names of files with error to event log
                        OnBeforeShowPreviewWindow();

                        if (ErrorOccurred || IsCancelled)
                        {
                            return;
                        }

                        if (PreviewData.ChangeList == null
                            || PreviewData.ChangeList.Count == 0)
                        {
                            if (System.Windows.MessageBox.Show(
                                    Resources.RefactoringOperation_NoChangesToPreview,
                                    Resources.RefactoringOperation_NoChangesToPreviewTitle,
                                    MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                            {
                                ApplyChanges();
                            }
                        }
                        else
                        {
                            // After feed the preview window with PreviewChangesEngine, the engine will call back the ApplyChanges
                            // method in this Operation if user clicks Apply button.
                            ShowPreviewWindow();
                        }
                    }
                    else
                    {
                        // If no preview needed, apply the changes.
                        ApplyChanges();
                    }
                }

                if (ErrorOccurred || IsCancelled)
                {
                    return;
                }
            }
            catch (InvalidOperationException ex)
            {
                OnError(string.Format(CultureInfo.CurrentCulture, Resources.Error_FailedOperation, ex.Message));
            }
            catch (OperationCanceledException ex)
            {
                var errorMessage = ex.Message;
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = Resources.Dialog_CancelRefactoring;
                }
                CancelOperation();
                CommonVsUtilities.ShowMessageBoxEx(
                    Resources.RefactorDialog_Title, errorMessage,
                    MessageBoxButtons.OK, MessageBoxDefaultButton.Button1, MessageBoxIcon.Error);
            }
            finally
            {
                UnregisterEvents();
            }
        }

        protected virtual PreviewChangesNodeBuilder GetPreviewChangesNodeBuilder()
        {
            return new PreviewChangesNodeBuilder();
        }

        /// <summary>
        ///     Method to show the preview window.
        ///     This method will create a PreviewChangesEngine and PreviewChangesList and give these two information
        ///     to preview dialog.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsPreviewChangesService.PreviewChanges(Microsoft.VisualStudio.Shell.Interop.IVsPreviewChangesEngine)")]
        private void ShowPreviewWindow()
        {
            // Prepare Preview UI Data.
            // Create preview tree nodes from list of FileChanges.
            var builder = GetPreviewChangesNodeBuilder();
            var nodes = builder.Build(FileChanges);

            // Get RefactoringPreviewData for this RefactorOperation and assign the FileChanges list
            // and Preview tree nodes to this preview data.
            // After everything is set to that preview data, create a PreviewChangesEngine
            // and pass this operation to the engine.  Preview engine will use PreviewData 
            // property of this operation to populate the preview dialog.  And the engine
            // will also call back this operation's ApplyChanges method to apply all the changes.
            PreviewData.ChangeList = nodes;
            PreviewData.FileChanges = FileChanges;
            var warningText = AnalysisWarningText;
            if (!string.IsNullOrEmpty(warningText))
            {
                PreviewData.Warning = AnalysisWarningText;
                PreviewData.WarningLevel = __PREVIEWCHANGESWARNINGLEVEL.PCWL_Warning;
            }

            // Get the Preview Changes Service
            var previewChangeService = _serviceProvider.GetService(typeof(SVsPreviewChangesService)) as IVsPreviewChangesService;
            if (previewChangeService != null)
            {
                // Preview the Changes
                using (var previewEngine = new PreviewChangesEngine(ApplyChanges, PreviewData, _serviceProvider))
                {
                    var result = previewChangeService.PreviewChanges(previewEngine);
                    Debug.Assert(result == VSConstants.S_OK, "Failed to preview the change.");
                }

                PreviewData = null;
            }
            else
            {
                OnError(Resources.Error_FailedInvokePreviewWindow);
            }
        }

        /// <summary>
        ///     Apply refactor changes.
        ///     It calls OnPreApplyChanges, OnApplyChanges and OnPostApplyChanges in order.
        ///     If there is no preview UI needed, DoOperation will call this method to apply changes.
        ///     If there is preview UI, after user click "Apply" button, the PreviewChangesEngine will
        ///     call back related RefactorOperation.ApplyChanges() to apply the changes.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal bool ApplyChanges()
        {
            var args = new ApplyChangesEventArgs(FileChanges);

            try
            {
                OnPreApplyChanges(args);
                if (ErrorOccurred || IsCancelled)
                {
                    return false;
                }

                // Apply changes
                OnApplyChanges();
                if (ErrorOccurred || IsCancelled)
                {
                    return false;
                }

                OnPostApplyChanges(args);
                if (ErrorOccurred || IsCancelled)
                {
                    // If any error occurred during PostApplyChanges operation
                    // from different contributors, we do not need to roll back any changes.
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnError(string.Format(CultureInfo.CurrentCulture, Resources.Error_FailedOperation, ex.Message));
            }

            return true;
        }

        /// <summary>
        ///     This method allows derived RefactorOperations to bring up optional UI prior to applying changes.
        ///     This can be used for UI in place of the Preview Window.
        /// </summary>
        /// <param name="changes"></param>
        /// <returns></returns>
        protected virtual void OnPreApplyChanges(ApplyChangesEventArgs changes)
        {
            var cache = ApplyingChanges;
            if (cache != null)
            {
                cache(this, changes);
                if (changes.Cancel)
                {
                    CancelOperation();
                }
            }
        }

        /// <summary>
        ///     This method do the real work for applying changes.
        ///     Derived class override how they want to apply changes.
        /// </summary>
        protected abstract void OnApplyChanges();

        /// <summary>
        ///     This method allows derived RefactorOperations to perform actions post applying changes.
        /// </summary>
        /// <param name="changes"></param>
        /// <returns></returns>
        protected virtual void OnPostApplyChanges(ApplyChangesEventArgs changes)
        {
            var cache = AppliedChanges;
            if (cache != null)
            {
                cache(this, changes);
                if (changes.Cancel)
                {
                    CancelOperation();
                }
            }
        }

        /// <summary>
        ///     Apply changes to one file and possibly create markers
        /// </summary>
        /// <param name="file">FileChange that contains list of ChangeProposals.</param>
        /// <param name="textLines">IVsTextLines of the file content.</param>
        /// <param name="createMarker">Need to create marker or not when applying changes.</param>
        /// <param name="changeToHighlight">The ChangeProposal need to be highlighted if creating marker.</param>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults",
            MessageId =
                "Microsoft.VisualStudio.Shell.Interop.IVsSolution.GetProjectOfUniqueName(System.String,Microsoft.VisualStudio.Shell.Interop.IVsHierarchy@)"
            )]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults",
            MessageId =
                "Microsoft.VisualStudio.TextManager.Interop.IVsTextLines.ReplaceLines(System.Int32,System.Int32,System.Int32,System.Int32,System.IntPtr,System.Int32,Microsoft.VisualStudio.TextManager.Interop.TextSpan[])"
            )]
        internal static void ApplyChangesToOneFile(
            FileChange file, IVsTextLines textLines, bool createMarker, ChangeProposal changeToHighlight)
        {
            ArgumentValidation.CheckForNullReference(file, "file");
            ArgumentValidation.CheckForNullReference(textLines, "textLines");

            // Here we only work on text based ChangeProposal
            // Sort all ChangeProposal in one file by the offset.
            // After apply any change, the offset of next ChangeProposal will change.
            // We need to sort all the changes first, so it will be easy to recalculate 
            // the new offset for next change.
            var sortedChanges = SortChanges(file);

            // Changes may cause the total line number of this file to be changed.
            // This variable is used to keep this line changes.
            var totalLineOffset = 0;

            // In one line, changes may cause column number of that line to be changed.
            // This variable is used to keep this column changes.
            var totalColumnOffset = 0;

            var pContent = IntPtr.Zero;

            // Store result TextSpan info
            var resultSpan = new[] { new TextSpan() };

            foreach (var sortedChange in sortedChanges)
            {
                var change = sortedChange.Value;
                // If user choose to apply this change
                if (change.Included)
                {
                    try
                    {
                        // If current change's StartLine is different from Last change's EndLine
                        // reset the columnOffset
                        if (resultSpan[0].iEndLine != change.StartLine + totalLineOffset)
                        {
                            totalColumnOffset = 0;
                        }

                        pContent = Marshal.StringToHGlobalAuto(change.NewValue);
                        // Calculate the current change's offset by applying lineOffset
                        // and columnOffset caused by previous changes
                        var startLine = change.StartLine + totalLineOffset;
                        var startColumn = change.StartColumn + totalColumnOffset;
                        var endLine = change.EndLine + totalLineOffset;
                        // Only apply columnOffset if current change's startLine is same as endLine
                        var endColumn = change.EndColumn + (startLine == endLine ? totalColumnOffset : 0);

                        textLines.ReplaceLines(
                            startLine, startColumn, endLine, endColumn,
                            pContent, change.NewValue.Length, resultSpan);

                        // Add the line offset caused by this change to total lineOffset;
                        var currentLineOffset = resultSpan[0].iEndLine - endLine;
                        totalLineOffset += currentLineOffset;
                        // Calculate the ColumnOffset after this change
                        totalColumnOffset = resultSpan[0].iEndIndex - change.EndColumn;
                    }
                    finally
                    {
                        if (pContent != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(pContent);
                        }
                    }
                }

                // Create Marker for this change's textspan.
                // If this change is the change to be highlighted, highlight it.
                if (createMarker)
                {
                    var textChangeToHighlight = changeToHighlight as TextChangeProposal;
                    var highlight = ((textChangeToHighlight != null) &&
                                     (change.StartLine == textChangeToHighlight.StartLine) &&
                                     (change.StartColumn == textChangeToHighlight.StartColumn));
                    CreateMarker(textLines, resultSpan[0], highlight);
                }
            }
        }

        /// <summary>
        ///     Create a marker on resultSpan
        /// </summary>
        /// <param name="textLines">IVsTextLines of the file.</param>
        /// <param name="resultSpan">TextSpan to be marked.</param>
        /// <param name="highlight">Need to highlight that change or not.</param>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.TextManager.Interop.IVsTextLines.CreateLineMarker(System.Int32,System.Int32,System.Int32,System.Int32,System.Int32,Microsoft.VisualStudio.TextManager.Interop.IVsTextMarkerClient,Microsoft.VisualStudio.TextManager.Interop.IVsTextLineMarker[])")]
        private static void CreateMarker(IVsTextLines textLines, TextSpan resultSpan, bool highlight)
        {
            ArgumentValidation.CheckForNullReference(textLines, "textLines");
            ArgumentValidation.CheckForNullReference(resultSpan, "resultSpan");

            IVsTextLineMarker[] textMarker = null;
            var result = textLines.CreateLineMarker(
                highlight ? (int)MARKERTYPE2.MARKER_REFACTORING_FIELD : (int)MARKERTYPE2.MARKER_REFACTORING_DEPFIELD,
                resultSpan.iStartLine, resultSpan.iStartIndex,
                resultSpan.iEndLine, resultSpan.iEndIndex,
                null, textMarker);

            Debug.Assert(result == VSConstants.S_OK, "Failed to create line marker for text span.");
        }

        /// <summary>
        ///     Sort changes based on ChangeOffset
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static SortedList<Position, TextChangeProposal> SortChanges(FileChange file)
        {
            ArgumentValidation.CheckForNullReference(file, "file");

            var sortedChanges = new SortedList<Position, TextChangeProposal>(new PositionComparer());

            foreach (var changeList in file.ChangeList.Values)
            {
                if (changeList != null)
                {
                    foreach (var change in changeList)
                    {
                        var textChange = change as TextChangeProposal;
                        if (textChange != null)
                        {
                            var currentPosition = new Position(textChange.StartLine, textChange.StartColumn);
                            if (!sortedChanges.ContainsKey(currentPosition))
                            {
                                sortedChanges.Add(currentPosition, textChange);
                            }
                        }
                    }
                }
            }
            return sortedChanges;
        }

        /// <summary>
        ///     Show error message.
        /// </summary>
        /// <param name="errorMessage">Message to show</param>
        protected void OnError(string errorMessage)
        {
            ErrorOccurred = true;
            CommonVsUtilities.ShowMessageBoxEx(
                Resources.ErrorDialog_Title,
                errorMessage,
                MessageBoxButtons.OK,
                MessageBoxDefaultButton.Button1,
                MessageBoxIcon.Error);
        }

        /// <summary>
        ///     The CancelRefactorOperation method exists to allow derived RefactorOperations to cancel the
        ///     operation based on user input. They can call this method from any of the above methods they implement.
        /// </summary>
        protected void CancelOperation()
        {
            IsCancelled = true;
        }

        /// <summary>
        ///     SCCI operation, check out any file that need to be changed.
        ///     If failed to check out any file, prompt error message.
        /// </summary>
        /// <returns>If true, check out succeed.</returns>
        protected bool EnsureFileCheckOut(IList<String> files)
        {
            if (files.Count > 0)
            {
                var changeFiles = new string[files.Count];
                files.CopyTo(changeFiles, 0);
                var userCanceled = false;

                if (!QueryEditFiles(out userCanceled, changeFiles))
                {
                    if (userCanceled)
                    {
                        OnError(Resources.Error_FailedToCheckOut);
                    }

                    return false;
                }
            }
            return true;
        }

        protected IList<String> GetListOfFilesToCheckOut()
        {
            IList<String> filesList = new List<String>();
            if (FileChanges.Count > 0)
            {
                // Sort the files first, so the check out list will be sorted.
                var files = new SortedList<string, string>();
                var filesCount = FileChanges.Count;
                for (var fileIndex = 0; fileIndex < filesCount; fileIndex++)
                {
                    var currentFileChange = FileChanges[fileIndex];
                    if (currentFileChange.IsFileModified)
                    {
                        // If there is any change will be applied to this file, 
                        // add the file to check out list.
                        files.Add(currentFileChange.FileName, null);
                    }
                }
                filesList = files.Keys;
            }
            return filesList;
        }

        internal void RegisterApplyingChangesEvent(Type contributeType, EventHandler<ApplyChangesEventArgs> handler)
        {
            if (!_applyingChangesTypes.Contains(contributeType))
            {
                _applyingChangesTypes.Add(contributeType);
                ApplyingChanges += handler;
            }
        }

        internal void RegisterAppliedChangesEvent(Type contributeType, EventHandler<ApplyChangesEventArgs> handler)
        {
            if (!_appliedChangesTypes.Contains(contributeType))
            {
                _appliedChangesTypes.Add(contributeType);
                AppliedChanges += handler;
            }
        }

        internal void UnregisterEvents()
        {
            _appliedChangesTypes.Clear();
            _applyingChangesTypes.Clear();

            AppliedChanges = null;
            ApplyingChanges = null;
        }

        protected abstract IList<FileChange> GetFileChanges();

        /// <summary>
        ///     The method is responsible for gathering any necessary input from the user and returning the appropriate
        ///     ContributorInput class that is then processed by the all compatible contributors.
        ///     This method is responsible for raising any UI needed to gather additional input.
        ///     ex) Rename Dialog for Rename Refactoring
        ///     For each Operation, we will only have one initial ContributorInput.
        ///     Different Contributors that supports handling this ContributorInput, should regester
        ///     RefactoringManager this ContributorInput.
        /// </summary>
        protected abstract ContributorInput OnGetContributorInput();

        /// <summary>
        ///     Allows Refactoring operations to perform actions before the preview window appears. Note that this method will
        ///     not be called if Refactor is invoked without UI.
        /// </summary>
        protected virtual void OnBeforeShowPreviewWindow()
        {
            // No-op by default
        }

        // TODO: We need a common location for this method
        /// <summary>
        ///     Ask SCC if file can be checked out.  SCC will check the file out if allowed to do so.
        ///     The user may be prompted (depending on settings) before checkout.
        /// </summary>
        /// <param name="userCanceled">Return flag indicating if the user canceled the checkout.</param>
        /// <param name="files">List of file to checkout.</param>
        /// <returns>True if the files were checked out, false otherwise.</returns>
        internal bool QueryEditFiles(out bool userCanceled, params string[] files)
        {
            userCanceled = false;

            // Get the QueryEditQuerySave service
            var queryEditQuerySave = _serviceProvider.GetService(typeof(SVsQueryEditQuerySave)) as IVsQueryEditQuerySave2;

            // Now call the QueryEdit method to find the edit status of this files
            uint result;
            uint outFlags;

            // Note that this function can popup a dialog to ask the user to checkout the file.
            // When this dialog is visible, it is possible to receive other request to change
            // the file and this is the reason for the recursion guard.
            var hr = queryEditQuerySave.QueryEditFiles(
                0, // Flags
                files.Length, // Number of elements in the array
                files, // Files to edit
                null, // Input flags
                null, // Input array of VSQEQS_FILE_ATTRIBUTE_DATA
                out result, // result of the checkout
                out outFlags // Additional flags
                );

            if (ErrorHandler.Succeeded(hr)
                && (result == (uint)tagVSQueryEditResult.QER_EditOK))
            {
                // In this case (and only in this case) we can return true from this function.
                return true;
            }
            else if (result == (uint)tagVSQueryEditResult.QER_NoEdit_UserCanceled)
            {
                userCanceled = true;
            }

            return false;
        }

        protected static IVsTextLines GetTextBufferForFile(string fileName, IList<IVsInvisibleEditor> openedInvisibleEditors)
        {
            // If the file is in RDT, get the text lines from RDT cookie.
            // If the file is not is RDT, it will open that file in invisible editor and 
            // get text lines from the invisible editor docdata.
            IVsTextLines textBuffer = null;
            if (RdtManager.IsFileInRdt(fileName))
            {
                // File is in RDT
                textBuffer = RdtManager.Instance.GetTextLines(fileName);
            }
            else
            {
                IVsInvisibleEditor invisibleEditor = null;
                // File is not in RDT, open it in invisible editor.
                if (RdtManager.Instance.TryGetTextLinesAndInvisibleEditor(fileName, out invisibleEditor, out textBuffer))
                {
                    openedInvisibleEditors.Add(invisibleEditor);
                }
                else
                {
                    // Failed to get text lines or invisible editor.
                    textBuffer = null;
                }
            }
            return textBuffer;
        }

        #endregion

        #region Helper classes used to sort ChangeProposals by Offset

        /// <summary>
        ///     Helper inner class used to sort Offset
        /// </summary>
        internal struct Position
        {
            private readonly int _line;
            private readonly int _column;

            public Position(int line, int column)
            {
                _line = line;
                _column = column;
            }

            public int Line
            {
                get { return _line; }
            }

            public int Column
            {
                get { return _column; }
            }

            public override int GetHashCode()
            {
                var hashcode = 0;
                hashcode = hashcode ^ _line;
                hashcode = hashcode ^ _column;
                return hashcode;
            }
        }

        /// <summary>
        ///     Helper class used to compare two points.
        /// </summary>
        internal class PositionComparer : IComparer<Position>
        {
            #region IComparer<Point> Members

            public int Compare(Position x, Position y)
            {
                if (x.Line == y.Line)
                {
                    return x.Column - y.Column;
                }
                else
                {
                    return x.Line - y.Line;
                }
            }

            #endregion
        }

        #endregion
    }
}
