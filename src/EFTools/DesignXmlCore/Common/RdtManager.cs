// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using VsErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using VsShell = Microsoft.VisualStudio.Shell.Interop;
using VsTextMgr = Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Data.Tools.Design.XmlCore;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    ///     A static utility class that manages interaction with the RDT for any part of the system that needs such services.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rdt")]
    internal sealed class RdtManager : IDisposable
    {
        private static volatile RdtManager _instance;
        private static readonly object _lock = new Object();

        private readonly VsShell.IVsInvisibleEditorManager _invisibleEditorManager;
        private readonly VsShell.IVsRunningDocumentTable _runningDocumentTable;
        private readonly _DTE _dte;
        private readonly VsShell.IVsUIShell _uiShell;
        private readonly Dictionary<uint /* docCookie */, int /* ref count */> _docDataToKeepAliveOnClose = new Dictionary<uint, int>();
        private readonly object _docDataToKeepAliveOnCloseLock = new object();

        internal static RdtManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            InitializeInstance();
                        }
                    }
                }
                return _instance;
            }
        }

        internal static void InitializeInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new RdtManager();
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "uiShell")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "RdtManager")]
        private RdtManager()
        {
            // assert if we're not on the UI thread.  GetService calls must be made on the UI thread, so caching 
            // these services lets this class be used on multiple threads.

            if (Application.MessageLoop == false)
            {
                Debug.Fail("Must create RdtManager on the UI Thread");
                throw new InvalidOperationException("Must create RdtManager on the UI Thread");
            }

            _invisibleEditorManager =
                Package.GetGlobalService(typeof(VsShell.SVsInvisibleEditorManager)) as VsShell.IVsInvisibleEditorManager;
            _runningDocumentTable = Package.GetGlobalService(typeof(VsShell.IVsRunningDocumentTable)) as VsShell.IVsRunningDocumentTable;
            _dte = Package.GetGlobalService(typeof(_DTE)) as _DTE;
            _uiShell = Package.GetGlobalService(typeof(VsShell.SVsUIShell)) as VsShell.IVsUIShell;
            if (_uiShell == null)
            {
                throw new InvalidOperationException("Could not get _uiShell!");
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsRunningDocumentTable.LockDocument(System.UInt32,System.UInt32)")]
        public void AddKeepDocDataAliveOnCloseReference(uint docCookie)
        {
            lock (_docDataToKeepAliveOnCloseLock)
            {
                int refCount;
                if (_docDataToKeepAliveOnClose.TryGetValue(docCookie, out refCount))
                {
                    _docDataToKeepAliveOnClose[docCookie] = refCount + 1;
                }
                else
                {
                    // If this is the first request to keep the doc data alive on close, issue an edit lock to the RDT
                    // which will ensure the doc data stays alive.
                    _docDataToKeepAliveOnClose.Add(docCookie, 1);
                    GetRunningDocumentTable().LockDocument((uint)VsShell._VSRDTFLAGS.RDT_EditLock, docCookie);
                }
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsRunningDocumentTable.UnlockDocument(System.UInt32,System.UInt32)")]
        public void RemoveKeepDocDataAliveOnCloseReference(uint docCookie)
        {
            lock (_docDataToKeepAliveOnCloseLock)
            {
                int refCount;
                if (_docDataToKeepAliveOnClose.TryGetValue(docCookie, out refCount))
                {
                    if (refCount == 1)
                    {
                        // If this was the last request to keep the doc data alive on close, remove the edit lock.
                        _docDataToKeepAliveOnClose.Remove(docCookie);
                        GetRunningDocumentTable().UnlockDocument((uint)VsShell._VSRDTFLAGS.RDT_EditLock, docCookie);
                    }
                    else
                    {
                        _docDataToKeepAliveOnClose[docCookie] = refCount - 1;
                    }
                }
            }
        }

        public bool ShouldKeepDocDataAliveOnClose(uint docCookie)
        {
            bool shouldKeepAlive;
            lock (_docDataToKeepAliveOnCloseLock)
            {
                shouldKeepAlive = _docDataToKeepAliveOnClose.ContainsKey(docCookie);
            }

            return shouldKeepAlive;
        }

        /// <summary>
        ///     This routine returns the document from the RDT.  We wrote this routine because
        ///     sometimes 8.3 filenames are used and other times long filenames, so this routine
        ///     tries and, if it fails, canonicalizes the path and tries again.
        /// </summary>
        private static int FindAndLockDocument(
            uint rdtLockType,
            string fullPathFileName,
            out VsShell.IVsHierarchy ppHier,
            out uint pitemid,
            out IntPtr ppunkDocData,
            out uint pdwCookie)
        {
            var hr = VSConstants.S_FALSE;
            var rdt = Instance.GetRunningDocumentTable();

            ppHier = null;
            pitemid = 0;
            ppunkDocData = IntPtr.Zero;
            pdwCookie = 0;

            if (rdt != null)
            {
                hr = rdt.FindAndLockDocument(
                    rdtLockType,
                    fullPathFileName,
                    out ppHier,
                    out pitemid,
                    out ppunkDocData,
                    out pdwCookie);

                if (NativeMethods.Failed(hr))
                {
                    try
                    {
                        if (Path.IsPathRooted(fullPathFileName) == false)
                        {
                            fullPathFileName = Path.GetFullPath(fullPathFileName);

                            // Canonicalize the file path
                            // Use to get rid of 8.3 filenames, otherwise this'll fail
                            hr = rdt.FindAndLockDocument(
                                rdtLockType,
                                fullPathFileName,
                                out ppHier,
                                out pitemid,
                                out ppunkDocData,
                                out pdwCookie);
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                    catch (IOException)
                    {
                    }
                }
            }
            return hr;
        }

        /// <summary>
        ///     Get IVsTextLines from a file.  If that file is in RDT, get text buffer from it.
        ///     If the file is not in RDT, open that file in invisible editor and get text buffer
        ///     from it.
        ///     If failed to get text buffer, it will return null.
        /// </summary>
        /// <remarks>
        ///     This method created for refactoring usage, refactoring will work on all kinds of
        ///     docdata, not only SqlEditorDocData.  If change this method, please get let LiangZ
        ///     know.
        /// </remarks>
        /// <param name="fullPathFileName">File name with full path.</param>
        /// <returns>Text buffer for that file.</returns>
        public VsTextMgr.IVsTextLines GetTextLines(string fullPathFileName)
        {
            VsTextMgr.IVsTextLines textLines = null;

            var rdt = Instance.GetRunningDocumentTable();
            if (rdt != null)
            {
                VsShell.IVsHierarchy ppHier;
                uint pitemid, pdwCookie;
                var ppunkDocData = IntPtr.Zero;
                try
                {
                    NativeMethods.ThrowOnFailure(
                        FindAndLockDocument(
                            (uint)(VsShell._VSRDTFLAGS.RDT_NoLock),
                            fullPathFileName,
                            out ppHier,
                            out pitemid,
                            out ppunkDocData,
                            out pdwCookie));
                    if (pdwCookie != 0)
                    {
                        if (ppunkDocData != IntPtr.Zero)
                        {
                            try
                            {
                                // Get text lines from the doc data
                                textLines = Marshal.GetObjectForIUnknown(ppunkDocData) as VsTextMgr.IVsTextLines;
                            }
                            catch (ArgumentException)
                            {
                                // Do nothing here, it will return null stream at the end.
                            }
                        }
                    }
                    else
                    {
                        // The file is not in RDT, open it in invisible editor and get the text lines from it.
                        VsShell.IVsInvisibleEditor invisibleEditor = null;
                        try
                        {
                            TryGetTextLinesAndInvisibleEditor(fullPathFileName, out invisibleEditor, out textLines);
                        }
                        finally
                        {
                            if (invisibleEditor != null)
                            {
                                Marshal.ReleaseComObject(invisibleEditor);
                            }
                        }
                    }
                }
                finally
                {
                    if (ppunkDocData != IntPtr.Zero)
                    {
                        Marshal.Release(ppunkDocData);
                    }
                }
            }
            return textLines;
        }

        public bool TryGetTextLinesAndInvisibleEditor(
            string fullPathFileName, out VsShell.IVsInvisibleEditor spEditor, out VsTextMgr.IVsTextLines textLines)
        {
            return TryGetTextLinesAndInvisibleEditor(fullPathFileName, null, out spEditor, out textLines);
        }

        /// <summary>
        ///     Open the file in invisible editor in RDT, and get text buffer from it.
        /// </summary>
        /// <remarks>
        ///     This method created for refactoring usage, refactoring will work on all kinds of
        ///     docdata, not only SqlEditorDocData.  If change this method, please get let LiangZ
        ///     know.
        /// </remarks>
        /// <param name="fullPathFileName">File name with full path.</param>
        /// <param name="project">the hierarchy for the document</param>
        /// <param name="spEditor">The result invisible editor.</param>
        /// <param name="textLines">The result text buffer.</param>
        /// <returns>True, if the file is opened correctly in invisible editor.</returns>
        public bool TryGetTextLinesAndInvisibleEditor(
            string fullPathFileName, VsShell.IVsProject project, out VsShell.IVsInvisibleEditor spEditor,
            out VsTextMgr.IVsTextLines textLines)
        {
            spEditor = null;
            textLines = null;

            // Need to open this file.  Use the invisible editor manager to do so.
            VsShell.IVsInvisibleEditorManager invisibleEditorMgr;
            var ppDocData = IntPtr.Zero;
            bool result;

            var iidIVsTextLines = typeof(VsTextMgr.IVsTextLines).GUID;

            try
            {
                invisibleEditorMgr = _invisibleEditorManager;

                NativeMethods.ThrowOnFailure(
                    invisibleEditorMgr.RegisterInvisibleEditor(
                        fullPathFileName, project, (uint)VsShell._EDITORREGFLAGS.RIEF_ENABLECACHING, null, out spEditor));
                if (spEditor != null)
                {
                    var hr = spEditor.GetDocData(0, ref iidIVsTextLines, out ppDocData);
                    if (hr == VSConstants.S_OK
                        && ppDocData != IntPtr.Zero)
                    {
                        textLines = Marshal.GetTypedObjectForIUnknown(ppDocData, typeof(VsTextMgr.IVsTextLines)) as VsTextMgr.IVsTextLines;
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result = false;
                }
            }
            finally
            {
                if (ppDocData != IntPtr.Zero)
                {
                    Marshal.Release(ppDocData);
                }
            }

            return result;
        }

        /// <summary>
        ///     Get string content of a file.
        /// </summary>
        /// <remarks>
        ///     This method created for refactoring usage, refactoring will work on all kinds of
        ///     docdata, not only SqlEditorDocData.  If change this method, please get let LiangZ
        ///     know.
        /// </remarks>
        /// <param name="fullPathFileName">File name with full path.</param>
        /// <returns>Content of that file in string format.</returns>
        public string ReadFromFile(string fullPathFileName)
        {
            string content;
            VsShell.IVsInvisibleEditor invisibleEditor = null;
            VsTextMgr.IVsTextLines textLines;
            try
            {
                if (IsFileInRdt(fullPathFileName))
                {
                    // File is in RDT
                    textLines = GetTextLines(fullPathFileName);
                }
                else
                {
                    // File is not in RDT, open it in invisble editor.
                    if (!TryGetTextLinesAndInvisibleEditor(fullPathFileName, out invisibleEditor, out textLines))
                    {
                        // Failed to get text lines or invisible editor.
                        textLines = null;
                    }
                }
                content = GetAllTextFromTextLines(textLines);
            }
            finally
            {
                // Close invisible editor from RDT
                if (invisibleEditor != null)
                {
                    Marshal.ReleaseComObject(invisibleEditor);
                }
            }
            return content;
        }

        public static string GetAllTextFromTextLines(VsTextMgr.IVsTextLines textLines)
        {
            string content = null;
            if (textLines != null)
            {
                int line, column;
                var result = textLines.GetLastLineIndex(out line, out column);
                if (result == VSConstants.S_OK)
                {
                    result = textLines.GetLineText(0, 0, line, column, out content);
                    if (result != VSConstants.S_OK)
                    {
                        content = null;
                    }
                }
            }
            return content;
        }

        /// <summary>
        ///     Save the file if it is in the RDT and dirty.
        /// </summary>
        /// <param name="fullFilePath">The file to save</param>
        public void SaveDirtyFile(string fullFilePath)
        {
            if (string.IsNullOrEmpty(fullFilePath) == false)
            {
                IList<string> dirtyFiles = new List<string> { fullFilePath };
                SaveDirtyFiles(dirtyFiles);
            }
        }

        /// <summary>
        ///     Save all dirty files.
        /// </summary>
        /// <param name="dirtyFiles">A list of dirty files to save.  These must be full path.</param>
        public void SaveDirtyFiles(IList<string> dirtyFiles)
        {
            ArgumentValidation.CheckForNullReference(dirtyFiles, "dirtyFiles");

            var rdt = _runningDocumentTable;

            var fileCount = dirtyFiles.Count;
            for (var fileIndex = 0; fileIndex < fileCount; fileIndex++)
            {
                var filePath = dirtyFiles[fileIndex];
                var docData = GetDocData(filePath) as VsShell.IVsPersistDocData;
                if (docData != null)
                {
                    var cookie = GetRdtCookie(filePath);
                    string newdoc;
                    int cancelled;
                    NativeMethods.ThrowOnFailure(rdt.NotifyOnBeforeSave(cookie));
                    var result = docData.SaveDocData(VsShell.VSSAVEFLAGS.VSSAVE_Save, out newdoc, out cancelled);
                    if (result != VSConstants.S_OK
                        || cancelled != 0)
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.Exception_FailedToSaveFile, filePath));
                    }
                    NativeMethods.ThrowOnFailure(rdt.NotifyOnAfterSave(cookie));
                }
            }
        }

        /// <summary>
        ///     Saves all the dirty files that pass the predicate
        /// </summary>
        /// <param name="shouldSave"></param>
        public void SaveDirtyFiles(Predicate<string> shouldSave)
        {
            ArgumentValidation.CheckForNullReference(shouldSave, "shouldSave");

            SaveDirtyFiles(GetDirtyFiles(shouldSave));
        }

        public List<string> GetDirtyFiles(Predicate<string> shouldHandle)
        {
            ArgumentValidation.CheckForNullReference(shouldHandle, "shouldHandle");

            var dirtyFiles = new List<string>();
            var rdt = Instance.GetRunningDocumentTable();
            if (rdt != null)
            {
                // Get the UI Shell
                var uiShell = _uiShell;

                // Go through the open documents and find it
                VsShell.IEnumWindowFrames windowFramesEnum;
                ErrorHandler.ThrowOnFailure(uiShell.GetDocumentWindowEnum(out windowFramesEnum));
                var windowFrames = new VsShell.IVsWindowFrame[1];
                uint fetched;
                while (windowFramesEnum.Next(1, windowFrames, out fetched) == VSConstants.S_OK
                       && fetched == 1)
                {
                    var windowFrame = windowFrames[0];
                    object data;
                    ErrorHandler.ThrowOnFailure(windowFrame.GetProperty((int)VsShell.__VSFPROPID.VSFPROPID_DocData, out data));

                    var fileFormat = data as VsShell.IPersistFileFormat;
                    if (fileFormat != null)
                    {
                        string candidateFilename;
                        uint formatIndex;
                        int dirty;

                        // The binary editor returns notimpl for IsDirty so just continue if
                        // the interface returns E_NOTIMPL
                        var hr = fileFormat.IsDirty(out dirty);
                        if (hr == VSConstants.E_NOTIMPL)
                        {
                            continue;
                        }

                        ErrorHandler.ThrowOnFailure(hr);
                        if (dirty == 1)
                        {
                            NativeMethods.ThrowOnFailure(fileFormat.GetCurFile(out candidateFilename, out formatIndex));
                            if (string.IsNullOrEmpty(candidateFilename) == false
                                &&
                                shouldHandle(candidateFilename))
                            {
                                dirtyFiles.Add(candidateFilename);
                            }
                        }
                    }
                }
            }

            return dirtyFiles;
        }

        /// <summary>
        ///     Get the running document table.
        /// </summary>
        /// <returns>
        ///     The running document table for this run of VS.
        /// </returns>
        public VsShell.IVsRunningDocumentTable GetRunningDocumentTable()
        {
            return _runningDocumentTable;
        }

        /// <summary>
        ///     Get the running document table.
        /// </summary>
        /// <returns>
        ///     The running document table for this run of VS.
        /// </returns>
        public VsShell.IVsRunningDocumentTable2 GetRunningDocumentTable2()
        {
            return _runningDocumentTable as VsShell.IVsRunningDocumentTable2;
        }

        /// <summary>
        ///     Returns the currently active document or string.empty
        /// </summary>
        /// <returns></returns>
        public string GetActiveDocument()
        {
            var dte = _dte;
            if (dte != null)
            {
                var document = dte.ActiveDocument;
                if (document != null)
                {
                    return document.FullName;
                }
            }
            return string.Empty;
        }

        /// <summary>
        ///     Sets the focus to the active document, if there is one.
        /// </summary>
        public void SetFocusToActiveDocument()
        {
            var dte = _dte;
            if (dte != null)
            {
                var document = dte.ActiveDocument;
                if (document != null)
                {
                    document.Activate();
                }
            }
        }

        /// <summary>
        ///     Returns the hierarchy for this file on the rdt
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public VsShell.IVsHierarchy GetHierarchyFromDocCookie(uint docCookie)
        {
            uint pgrfRDTFlags;
            uint pdwReadLocks;
            uint pdwEditLocks;
            string pbstrMkDocument;
            VsShell.IVsHierarchy ppHier;
            uint pitemid;
            var ppunkDocData = IntPtr.Zero;

            try
            {
                NativeMethods.ThrowOnFailure(
                    _runningDocumentTable.GetDocumentInfo(
                        docCookie, out pgrfRDTFlags, out pdwReadLocks, out pdwEditLocks, out pbstrMkDocument, out ppHier, out pitemid,
                        out ppunkDocData));
            }
            finally
            {
                if (ppunkDocData != IntPtr.Zero)
                {
                    Marshal.Release(ppunkDocData);
                }
            }
            return ppHier;
        }

        /// <summary>
        ///     Returns the docdata for this cookie on the rdt
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
        public static bool TryGetDocDataFromCookie(uint cookie, out object docData)
        {
            docData = null;

            var rdt = Instance.GetRunningDocumentTable();
            Debug.Assert(rdt != null);
            if (rdt != null)
            {
                VsShell.IVsHierarchy hierarchy;
                uint rdtFlags;
                uint readLocks;
                uint editLocks;
                string itemName;
                uint itemId;
                IntPtr unknownDocData;

                var hr = rdt.GetDocumentInfo(
                    cookie,
                    out rdtFlags,
                    out readLocks,
                    out editLocks,
                    out itemName,
                    out hierarchy,
                    out itemId,
                    out unknownDocData);
                if (NativeMethods.Succeeded(hr))
                {
                    docData = Marshal.GetObjectForIUnknown(unknownDocData);
                    Marshal.Release(unknownDocData);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Is this document dirty?
        /// </summary>
        /// <param name="docFullPath">The full path to the document</param>
        /// <returns>True if document is dirty, false if not</returns>
        public static bool IsDirty(string docFullPath)
        {
            var docData = GetDocData(docFullPath) as VsShell.IVsPersistDocData;
            if (docData != null)
            {
                int dirty;
                NativeMethods.ThrowOnFailure(docData.IsDocDataDirty(out dirty));

                return (dirty != 0);
            }
            return false;
        }

        public static bool IsDirty(uint docCookie)
        {
            object docData;
            if (TryGetDocDataFromCookie(docCookie, out docData))
            {
                return IsDirty(docData);
            }
            return false;
        }

        public static bool IsDirty(object docData)
        {
            var persistDocData = docData as VsShell.IVsPersistDocData;
            if (persistDocData != null)
            {
                int dirty;
                NativeMethods.ThrowOnFailure(persistDocData.IsDocDataDirty(out dirty));

                return (dirty != 0);
            }

            return false;
        }

        /// <summary>
        ///     Closes the frame.  Returns true
        ///     if the editor was found.
        /// </summary>
        public void CloseFrame(string fullFileName, out int foundAndClosed)
        {
            foundAndClosed = 0;
            if (string.IsNullOrEmpty(fullFileName) == false)
            {
                var rdt2 = GetRunningDocumentTable2();
                if (rdt2 != null)
                {
                    var hr = rdt2.QueryCloseRunningDocument(fullFileName, out foundAndClosed);
                    if (hr != VSConstants.OLE_E_PROMPTSAVECANCELLED)
                    {
                        NativeMethods.ThrowOnFailure(hr);
                    }
                }
            }
        }

        /// <summary>
        ///     Returns the window frame for our document window
        /// </summary>
        /// <returns>
        ///     The IWindowFrame for our open document.
        /// </returns>
        public VsShell.IVsWindowFrame GetWindowFrame(string fullFileName)
        {
            if (string.IsNullOrEmpty(fullFileName))
            {
                return null;
            }

            VsShell.IVsWindowFrame foundFrame = null;

            var rdt = Instance.GetRunningDocumentTable();
            if (rdt != null)
            {
                // Get the UI Shell
                var uiShell = _uiShell;

                // Go through the open documents and find it
                VsShell.IEnumWindowFrames windowFramesEnum;
                ErrorHandler.ThrowOnFailure(uiShell.GetDocumentWindowEnum(out windowFramesEnum));
                var windowFrames = new VsShell.IVsWindowFrame[1];
                uint fetched;
                var thisFilename = fullFileName;

                while (windowFramesEnum.Next(1, windowFrames, out fetched) == VSConstants.S_OK
                       && fetched == 1)
                {
                    var windowFrame = windowFrames[0];
                    object data;
                    ErrorHandler.ThrowOnFailure(windowFrame.GetProperty((int)VsShell.__VSFPROPID.VSFPROPID_DocData, out data));

                    var fileFormat = data as VsShell.IPersistFileFormat;
                    if (fileFormat != null)
                    {
                        string candidateFilename;
                        uint formatIndex;

                        NativeMethods.ThrowOnFailure(fileFormat.GetCurFile(out candidateFilename, out formatIndex));
                        if (string.IsNullOrEmpty(candidateFilename) == false
                            &&
                            (string.Compare(candidateFilename, thisFilename, true, CultureInfo.CurrentCulture) == 0 ||
                             FileUtils.IsSamePath(candidateFilename, thisFilename)))
                        {
                            // Found it
                            foundFrame = windowFrame;
                            break;
                        }
                    }
                }
            }
            return foundFrame;
        }

        /// <summary>
        ///     Gets the DocCookie from the RDT for the specified fullPath FileName
        ///     Returns 0 if the file is not in the RDT
        /// </summary>
        /// <param name="fullPathFileName">the fullpath filename whose docCookie is wanted</param>
        /// <returns>the DocCookie of the specified file</returns>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults",
            MessageId =
                "Microsoft.VisualStudio.Shell.Interop.IVsRunningDocumentTable.FindAndLockDocument(System.UInt32,System.String,Microsoft.VisualStudio.Shell.Interop.IVsHierarchy@,System.UInt32@,System.IntPtr@,System.UInt32@)"
            )]
        internal static uint GetRdtCookie(string fullPathFileName)
        {
            uint result = 0;
            var rdt = Instance.GetRunningDocumentTable();
            if (rdt != null)
            {
                VsShell.IVsHierarchy ppHier;
                uint pItemId;
                var ppunkDocData = IntPtr.Zero;

                try
                {
                    try
                    {
                        if (Path.IsPathRooted(fullPathFileName))
                        {
                            // Canonicalize the file path
                            // Use to get rid of 8.3 filenames, otherwise this'll fail
                            fullPathFileName = Path.GetFullPath(fullPathFileName);
                        }
                    }
                    catch (ArgumentException)
                    {
                        // Contains invalid path characters - for instance a file-less moniker
                    }

                    FindAndLockDocument(
                        (uint)(VsShell._VSRDTFLAGS.RDT_NoLock),
                        fullPathFileName,
                        out ppHier,
                        out pItemId,
                        out ppunkDocData,
                        out result);
                }
                finally
                {
                    if (ppunkDocData != IntPtr.Zero)
                    {
                        Marshal.Release(ppunkDocData);
                    }
                }
            }
            return result;
        }

        /// <summary>
        ///     Gets the DocData object from the RDT for the specified fullPath filename
        ///     You might want to try casting this to an IVsPersistDocData2, but that may not work for all filetypes
        ///     Returns 0 if the file is not in the RDT
        /// </summary>
        /// <param name="fullPathFileName">the fullpath filename whose docCookie is wanted</param>
        /// <returns>the docData of the specified file</returns>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults",
            MessageId =
                "Microsoft.VisualStudio.Shell.Interop.IVsRunningDocumentTable.FindAndLockDocument(System.UInt32,System.String,Microsoft.VisualStudio.Shell.Interop.IVsHierarchy@,System.UInt32@,System.IntPtr@,System.UInt32@)"
            )]
        public static Object GetDocData(string fullPathFileName)
        {
            var ppunkDocData = IntPtr.Zero;
            Object returnObj = null;
            uint result;
            var rdt = Instance.GetRunningDocumentTable();
            if (rdt != null)
            {
                VsShell.IVsHierarchy ppHier;
                uint pItemId;

                try
                {
                    try
                    {
                        if (Path.IsPathRooted(fullPathFileName))
                        {
                            // Canonicalize the file path
                            // Use to get rid of 8.3 filenames, otherwise this'll fail
                            fullPathFileName = Path.GetFullPath(fullPathFileName);
                        }
                    }
                    catch (ArgumentException)
                    {
                    }

                    FindAndLockDocument(
                        (uint)(VsShell._VSRDTFLAGS.RDT_NoLock),
                        fullPathFileName,
                        out ppHier,
                        out pItemId,
                        out ppunkDocData,
                        out result);
                }
                finally
                {
                    if (ppunkDocData != IntPtr.Zero)
                    {
                        returnObj = Marshal.GetObjectForIUnknown(ppunkDocData);
                        Marshal.Release(ppunkDocData);
                    }
                }
            }
            return returnObj;
        }

        /// <summary>
        ///     Checks if a given file is in the RDT
        /// </summary>
        /// <param name="fullPathFileName">fullpath filename of the file in question</param>
        /// <returns>bool: IsFileInRdt</returns>
        public static bool IsFileInRdt(string fullPathFileName)
        {
            return !String.IsNullOrEmpty(fullPathFileName)
                   && (GetRdtCookie(fullPathFileName) != 0);
        }

        /// <summary>
        ///     Refresh a file content with passed in string.  After change, the file will not be saved.
        /// </summary>
        /// <param name="fullPathFileName"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool WriteToFile(string fullPathFileName, string content)
        {
            return WriteToFile(fullPathFileName, content, false);
        }

        /// <summary>
        ///     Refresh a file content with passed in string.  Save or not save the file according to
        ///     passed in option.
        /// </summary>
        /// <remarks>
        ///     This method created for refactoring usage, refactoring will work on all kinds of
        ///     docdata, not only SqlEditorDocData.  If change this method, please get let LiangZ
        ///     know.
        /// </remarks>
        /// <param name="fullPathFileName">File name with full path.</param>
        /// <param name="content">string content need to update the file with.</param>
        /// <param name="saveFile">Save file or not after writing.</param>
        public bool WriteToFile(string fullPathFileName, string content, bool saveFile)
        {
            return WriteToFile(fullPathFileName, content, saveFile, false);
        }

        /// <summary>
        ///     Refresh a file content with passed in string.  Save or not save the file according to
        ///     passed in option.
        /// </summary>
        /// <remarks>
        ///     This method created for refactoring usage, refactoring will work on all kinds of
        ///     docdata, not only SqlEditorDocData.  If change this method, please get let LiangZ
        ///     know.
        /// </remarks>
        /// <param name="fullPathFileName">File name with full path.</param>
        /// <param name="content">string content need to update the file with.</param>
        /// <param name="saveFile">Save file or not after writing.</param>
        /// <param name="createIfNotExist">Creates the file if it doesn't exist.</param>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public bool WriteToFile(string fullPathFileName, string content, bool saveFile, bool createIfNotExist)
        {
            var succeed = true;
            VsShell.IVsInvisibleEditor invisibleEditor = null;
            VsTextMgr.IVsTextLines textLines = null;
            var fileDidNotExistAndShouldBeCreated = false;

            try
            {
                if (IsFileInRdt(fullPathFileName))
                {
                    // File is in RDT
                    textLines = GetTextLines(fullPathFileName);
                }
                else
                {
                    if (createIfNotExist &&
                        File.Exists(fullPathFileName) == false)
                    {
                        fileDidNotExistAndShouldBeCreated = true;

                        FileStream fileStream = null;
                        StreamWriter writer = null;
                        try
                        {
                            fileStream = File.Create(fullPathFileName);
                            // Make the newly created file unicode
                            writer = new StreamWriter(fileStream, Encoding.UTF8);

                            writer.Write(content);
                        }
                        catch (IOException e)
                        {
                            throw new InvalidOperationException(e.Message);
                        }
                        catch (ArgumentException e)
                        {
                            throw new InvalidOperationException(e.Message);
                        }
                        finally
                        {
                            if (writer != null)
                            {
                                writer.Close();
                            }
                            if (fileStream != null)
                            {
                                fileStream.Close();
                            }
                        }
                    }

                    // File is not in RDT, open it in invisible editor.
                    if (fileDidNotExistAndShouldBeCreated == false
                        && !TryGetTextLinesAndInvisibleEditor(fullPathFileName, out invisibleEditor, out textLines))
                    {
                        // Failed to get text lines or invisible editor.
                        textLines = null;
                    }
                }

                if (fileDidNotExistAndShouldBeCreated == false)
                {
                    if (textLines != null)
                    {
                        int line, column;
                        var result = textLines.GetLastLineIndex(out line, out column);
                        if (result == VSConstants.S_OK)
                        {
                            var pContent = IntPtr.Zero;
                            try
                            {
                                // Copy the content to textLines.
                                pContent = Marshal.StringToHGlobalAuto(content);
                                result = textLines.ReloadLines(
                                    0, 0, line, column, pContent, content.Length,
                                    new[] { new VsTextMgr.TextSpan() });
                                if (result != VSConstants.S_OK)
                                {
                                    succeed = false;
                                }
                            }
                            finally
                            {
                                if (pContent != IntPtr.Zero)
                                {
                                    Marshal.FreeHGlobal(pContent);
                                }
                            }
                            if (saveFile)
                            {
                                var list = new List<string> { fullPathFileName };
                                Instance.SaveDirtyFiles(list);
                            }
                        }
                        else
                        {
                            succeed = false;
                        }
                    }
                    else
                    {
                        succeed = false;
                    }
                }
            }
            finally
            {
                // Close invisible editor from RDT
                if (invisibleEditor != null)
                {
                    Marshal.ReleaseComObject(invisibleEditor);
                }
            }
            return succeed;
        }

        public void Dispose()
        {
            lock (_docDataToKeepAliveOnCloseLock)
            {
                Debug.Assert(
                    _docDataToKeepAliveOnClose.Keys.Count == 0,
                    "RdtManager is still trying to keep doc data alive on dispose, this could be a symptom of memory leak from invisible doc data.");
            }
        }
    }
}
