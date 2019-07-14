// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Common
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;

    /// <summary>
    ///     Wrapper class for access to the IVSTextLines of a file even if it's not currently open in a document. This class
    ///     will use any existing documents in the RDT for the file if they exist, otherwise it will use an invisible editor.
    /// </summary>
    internal sealed class VsTextLinesFromFile : IDisposable, IVsTextLines
    {
        private bool _disposed;
        private IVsInvisibleEditor _invisibleEditor;
        private IVsTextLines _textBuffer;

        // Use private constructor so that factory method can return null if we cannot get a text buffer.
        private VsTextLinesFromFile()
        {
        }

        public static VsTextLinesFromFile Load(string fileName)
        {
            VsTextLinesFromFile vsTextLinesFromFile = null;
            IVsInvisibleEditor invisibleEditor = null;
            IVsTextLines textBuffer = null;

            if (RdtManager.IsFileInRdt(fileName))
            {
                // File is in RDT
                textBuffer = RdtManager.Instance.GetTextLines(fileName);
            }
            else
            {
                // File is not in RDT, open it in invisible editor.
                if (!RdtManager.Instance.TryGetTextLinesAndInvisibleEditor(fileName, out invisibleEditor, out textBuffer))
                {
                    // Failed to get text lines or invisible editor.
                    textBuffer = null;

                    if (invisibleEditor != null)
                    {
                        Marshal.ReleaseComObject(invisibleEditor);
                    }
                }
            }

            if (textBuffer != null)
            {
                vsTextLinesFromFile = new VsTextLinesFromFile();
                vsTextLinesFromFile._invisibleEditor = invisibleEditor;
                vsTextLinesFromFile._textBuffer = textBuffer;
            }

            return vsTextLinesFromFile;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_invisibleEditor != null && disposing)
                {
                    Marshal.ReleaseComObject(_invisibleEditor);
                    _invisibleEditor = null;
                }

                _disposed = true;
            }
        }

        #region IVsTextLines

        public int AdviseTextLinesEvents(IVsTextLinesEvents pSink, out uint pdwCookie)
        {
            return _textBuffer.AdviseTextLinesEvents(pSink, out pdwCookie);
        }

        public int CanReplaceLines(int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, int iNewLen)
        {
            return _textBuffer.CanReplaceLines(iStartLine, iStartIndex, iEndLine, iEndIndex, iNewLen);
        }

        public int CopyLineText(int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, IntPtr pszBuf, ref int pcchBuf)
        {
            return _textBuffer.CopyLineText(iStartLine, iStartIndex, iEndLine, iEndIndex, pszBuf, ref pcchBuf);
        }

        public int CreateEditPoint(int iLine, int iIndex, out object ppEditPoint)
        {
            return _textBuffer.CreateEditPoint(iLine, iIndex, out ppEditPoint);
        }

        public int CreateLineMarker(
            int iMarkerType, int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, IVsTextMarkerClient pClient,
            IVsTextLineMarker[] ppMarker)
        {
            return _textBuffer.CreateLineMarker(iMarkerType, iStartLine, iStartIndex, iEndLine, iEndIndex, pClient, ppMarker);
        }

        public int CreateTextPoint(int iLine, int iIndex, out object ppTextPoint)
        {
            return _textBuffer.CreateTextPoint(iLine, iIndex, out ppTextPoint);
        }

        public int EnumMarkers(
            int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, int iMarkerType, uint dwFlags, out IVsEnumLineMarkers ppEnum)
        {
            return _textBuffer.EnumMarkers(iStartLine, iStartIndex, iEndLine, iEndIndex, iMarkerType, dwFlags, out ppEnum);
        }

        public int FindMarkerByLineIndex(
            int iMarkerType, int iStartingLine, int iStartingIndex, uint dwFlags, out IVsTextLineMarker ppMarker)
        {
            return _textBuffer.FindMarkerByLineIndex(iMarkerType, iStartingLine, iStartingIndex, dwFlags, out ppMarker);
        }

        public int GetLanguageServiceID(out Guid pguidLangService)
        {
            return _textBuffer.GetLanguageServiceID(out pguidLangService);
        }

        public int GetLastLineIndex(out int piLine, out int piIndex)
        {
            return _textBuffer.GetLastLineIndex(out piLine, out piIndex);
        }

        public int GetLengthOfLine(int iLine, out int piLength)
        {
            return _textBuffer.GetLengthOfLine(iLine, out piLength);
        }

        public int GetLineCount(out int piLineCount)
        {
            return _textBuffer.GetLineCount(out piLineCount);
        }

        public int GetLineData(int iLine, LINEDATA[] pLineData, MARKERDATA[] pMarkerData)
        {
            return _textBuffer.GetLineData(iLine, pLineData, pMarkerData);
        }

        public int GetLineDataEx(uint dwFlags, int iLine, int iStartIndex, int iEndIndex, LINEDATAEX[] pLineData, MARKERDATA[] pMarkerData)
        {
            return _textBuffer.GetLineDataEx(dwFlags, iLine, iStartIndex, iEndIndex, pLineData, pMarkerData);
        }

        public int GetLineIndexOfPosition(int iPosition, out int piLine, out int piColumn)
        {
            return _textBuffer.GetLineIndexOfPosition(iPosition, out piLine, out piColumn);
        }

        public int GetLineText(int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, out string pbstrBuf)
        {
            return _textBuffer.GetLineText(iStartLine, iStartIndex, iEndLine, iEndIndex, out pbstrBuf);
        }

        public int GetMarkerData(int iTopLine, int iBottomLine, MARKERDATA[] pMarkerData)
        {
            return _textBuffer.GetMarkerData(iTopLine, iBottomLine, pMarkerData);
        }

        public int GetPairExtents(TextSpan[] pSpanIn, TextSpan[] pSpanOut)
        {
            return _textBuffer.GetPairExtents(pSpanIn, pSpanOut);
        }

        public int GetPositionOfLine(int iLine, out int piPosition)
        {
            return _textBuffer.GetPositionOfLine(iLine, out piPosition);
        }

        public int GetPositionOfLineIndex(int iLine, int iIndex, out int piPosition)
        {
            return _textBuffer.GetPositionOfLineIndex(iLine, iIndex, out piPosition);
        }

        public int GetSize(out int piLength)
        {
            return _textBuffer.GetSize(out piLength);
        }

        public int GetStateFlags(out uint pdwReadOnlyFlags)
        {
            return _textBuffer.GetStateFlags(out pdwReadOnlyFlags);
        }

        public int GetUndoManager(out IOleUndoManager ppUndoManager)
        {
            return _textBuffer.GetUndoManager(out ppUndoManager);
        }

        public int IVsTextLinesReserved1(int iLine, LINEDATA[] pLineData, int fAttributes)
        {
            return _textBuffer.IVsTextLinesReserved1(iLine, pLineData, fAttributes);
        }

        public int InitializeContent(string pszText, int iLength)
        {
            return _textBuffer.InitializeContent(pszText, iLength);
        }

        public int LockBuffer()
        {
            return _textBuffer.LockBuffer();
        }

        public int LockBufferEx(uint dwFlags)
        {
            return _textBuffer.LockBufferEx(dwFlags);
        }

        public int ReleaseLineData(LINEDATA[] pLineData)
        {
            return _textBuffer.ReleaseLineData(pLineData);
        }

        public int ReleaseLineDataEx(LINEDATAEX[] pLineData)
        {
            return _textBuffer.ReleaseLineDataEx(pLineData);
        }

        public int ReleaseMarkerData(MARKERDATA[] pMarkerData)
        {
            return _textBuffer.ReleaseMarkerData(pMarkerData);
        }

        public int Reload(int fUndoable)
        {
            return _textBuffer.Reload(fUndoable);
        }

        public int ReloadLines(
            int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, IntPtr pszText, int iNewLen, TextSpan[] pChangedSpan)
        {
            return _textBuffer.ReloadLines(iStartLine, iStartIndex, iEndLine, iEndIndex, pszText, iNewLen, pChangedSpan);
        }

        public int ReplaceLines(
            int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, IntPtr pszText, int iNewLen, TextSpan[] pChangedSpan)
        {
            return _textBuffer.ReplaceLines(iStartLine, iStartIndex, iEndLine, iEndIndex, pszText, iNewLen, pChangedSpan);
        }

        public int ReplaceLinesEx(
            uint dwFlags, int iStartLine, int iStartIndex, int iEndLine, int iEndIndex, IntPtr pszText, int iNewLen, TextSpan[] pChangedSpan)
        {
            return _textBuffer.ReplaceLinesEx(dwFlags, iStartLine, iStartIndex, iEndLine, iEndIndex, pszText, iNewLen, pChangedSpan);
        }

        public int Reserved1()
        {
            return _textBuffer.Reserved1();
        }

        public int Reserved10()
        {
            return _textBuffer.Reserved10();
        }

        public int Reserved2()
        {
            return _textBuffer.Reserved2();
        }

        public int Reserved3()
        {
            return _textBuffer.Reserved3();
        }

        public int Reserved4()
        {
            return _textBuffer.Reserved4();
        }

        public int Reserved5()
        {
            return _textBuffer.Reserved5();
        }

        public int Reserved6()
        {
            return _textBuffer.Reserved6();
        }

        public int Reserved7()
        {
            return _textBuffer.Reserved7();
        }

        public int Reserved8()
        {
            return _textBuffer.Reserved8();
        }

        public int Reserved9()
        {
            return _textBuffer.Reserved9();
        }

        public int SetLanguageServiceID(ref Guid guidLangService)
        {
            return _textBuffer.SetLanguageServiceID(ref guidLangService);
        }

        public int SetStateFlags(uint dwReadOnlyFlags)
        {
            return _textBuffer.SetStateFlags(dwReadOnlyFlags);
        }

        public int UnadviseTextLinesEvents(uint dwCookie)
        {
            return _textBuffer.UnadviseTextLinesEvents(dwCookie);
        }

        public int UnlockBuffer()
        {
            return _textBuffer.UnlockBuffer();
        }

        public int UnlockBufferEx(uint dwFlags)
        {
            return _textBuffer.UnlockBufferEx(dwFlags);
        }

        #endregion
    }
}
