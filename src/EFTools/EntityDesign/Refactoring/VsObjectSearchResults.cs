// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Refactoring
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.InteropServices;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    internal class VsObjectSearchResult
    {
        private const string DashWithSpaces = " - ";

        internal VsObjectSearchResult(string fileName, string displayText, int lineNumber, int columnNumber)
        {
            FileName = fileName;
            DisplayText = displayText;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        internal string FileName { get; private set; }
        internal string DisplayText { get; private set; }
        internal int LineNumber { get; private set; }
        internal int ColumnNumber { get; private set; }

        internal static List<VsObjectSearchResult> Search(string name, ObjectSearchLanguage searchLanguage)
        {
            var results = new List<VsObjectSearchResult>();
            var searchService = PackageManager.Package.GetService(typeof(SVsObjectSearch)) as IVsObjectSearch;

            if (searchService != null)
            {
                IVsObjectList2 objectList;
                IVsObjectList searchResult;
                var criteria = new VSOBSEARCHCRITERIA();
                criteria.eSrchType = VSOBSEARCHTYPE.SO_ENTIREWORD;
                criteria.szName = name;

                // Need to switch between case sensitive and case insentive searches for C# and VB
                switch (searchLanguage)
                {
                    case ObjectSearchLanguage.CSharp:
                        criteria.grfOptions = (uint)_VSOBSEARCHOPTIONS.VSOBSO_LOOKINREFS | (uint)_VSOBSEARCHOPTIONS.VSOBSO_CASESENSITIVE;
                        break;
                    case ObjectSearchLanguage.VB:
                        criteria.grfOptions = (uint)_VSOBSEARCHOPTIONS.VSOBSO_LOOKINREFS;
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported language search type: " + searchLanguage.ToString());
                }

                if (searchService.Find(
                    (uint)__VSOBSEARCHFLAGS.VSOSF_NOSHOWUI | (uint)_VSOBSEARCHOPTIONS.VSOBSO_LOOKINREFS,
                    new VSOBSEARCHCRITERIA[1] { criteria }, out searchResult) == VSConstants.S_OK)
                {
                    objectList = searchResult as IVsObjectList2;
                    if (objectList != null)
                    {
                        uint pCount;
                        if (objectList.GetItemCount(out pCount) == VSConstants.S_OK)
                        {
                            for (uint i = 0; i < pCount; i++)
                            {
                                IVsObjectList2 subList;
                                if (objectList.GetList2(
                                    i, (uint)_LIB_LISTTYPE.LLT_HIERARCHY, (uint)_LIB_LISTFLAGS.LLF_NONE, new VSOBSEARCHCRITERIA2[0],
                                    out subList)
                                    == VSConstants.S_OK)
                                {
                                    // Switch to using our "safe" PInvoke interface for IVsObjectList2 to avoid potential memory management issues
                                    // when receiving strings as out params.
                                    var safeSubList = subList as ISafeVsObjectList2;
                                    if (safeSubList != null)
                                    {
                                        AddResultsToList(safeSubList, searchLanguage, results);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return results;
        }

        private static void AddResultsToList(
            ISafeVsObjectList2 objectList, ObjectSearchLanguage searchLanguage, List<VsObjectSearchResult> results)
        {
            uint objectCount;
            if (objectList.GetItemCount(out objectCount) == VSConstants.S_OK)
            {
                for (uint j = 0; j < objectCount; j++)
                {
                    var textPointer = IntPtr.Zero;

                    try
                    {
                        if (objectList.GetText(j, VSTREETEXTOPTIONS.TTO_DEFAULT, out textPointer) == VSConstants.S_OK)
                        {
                            var text = Marshal.PtrToStringUni(textPointer);

                            if (text != null)
                            {
                                string fileName;
                                int lineNumber;
                                int columnNumber;

                                // FindAllReferencesList.cs (VS) does not implement the GetSourceContext() API of IVsObjectList2, so as a
                                // workaround we will parse the description text to identify the file and line/col number.
                                if (TryParseSourceData(text, searchLanguage, out fileName, out lineNumber, out columnNumber))
                                {
                                    results.Add(new VsObjectSearchResult(fileName, text, lineNumber, columnNumber));
                                }
                            }
                        }
                    }
                    finally
                    {
                        // If this object implements IVsCoTaskMemFreeMyStrings we *must* free the IntPtr since we were passed a copy
                        // of the string the native code is using. If the object does not implement IVsCoTaskMemFreeMyStrings we
                        //  *must not* free the IntPtr since we are referencing the same string the native code is using.
                        if (textPointer != IntPtr.Zero
                            && objectList is IVsCoTaskMemFreeMyStrings)
                        {
                            Marshal.FreeCoTaskMem(textPointer);
                        }
                    }
                }
            }
        }

        internal static bool TryParseSourceData(
            string text, ObjectSearchLanguage searchLanguage, out string fileName, out int lineNumber, out int columnNumber)
        {
            switch (searchLanguage)
            {
                case ObjectSearchLanguage.CSharp:
                    return TryParseSourceDataCSharp(text, out fileName, out lineNumber, out columnNumber);
                case ObjectSearchLanguage.VB:
                    return TryParseSourceDataVB(text, out fileName, out lineNumber, out columnNumber);
                default:
                    throw new InvalidOperationException("Unsupported language search type: " + searchLanguage.ToString());
            }
        }

        // <summary>
        //     Parses out the line and column number from text, given line and column number formatting of "... (x,y)" where x is the line number
        //     and y is the column number.
        // </summary>
        private static bool TryParseLineAndColumn(string text, int endLastIndexCount, out int lineNumber, out int columnNumber)
        {
            lineNumber = 0;
            columnNumber = 0;
            var startParenIndex = text.LastIndexOf("(", endLastIndexCount, endLastIndexCount, StringComparison.Ordinal);
            var commaIndex = text.LastIndexOf(",", endLastIndexCount, endLastIndexCount, StringComparison.Ordinal);
            var endParenIndex = text.LastIndexOf(")", endLastIndexCount, endLastIndexCount, StringComparison.Ordinal);

            if (startParenIndex > 0
                && commaIndex > 0
                && endParenIndex > 0
                &&
                int.TryParse(text.Substring(startParenIndex + 1, commaIndex - (startParenIndex + 1)), out lineNumber)
                &&
                int.TryParse(text.Substring(commaIndex + 1, endParenIndex - (commaIndex + 1)), out columnNumber))
            {
                return true;
            }

            return false;
        }

        private static bool TryParseSourceDataCSharp(string text, out string fileName, out int lineNumber, out int columnNumber)
        {
            // Text is of the form (x,y = line+col number):
            // [filename] - (x,y) : [code]
            // Since filenames cannot contain colons, we search for the first colon, then go back from there
            // to find where the line numbers start (and the filename finishes) to get the filename.
            fileName = "";
            lineNumber = 0;
            columnNumber = 0;

            // Add a space either side of the colon search so we don't match against drive letters
            var colonIndex = text.IndexOf(" : ", StringComparison.Ordinal);
            if (colonIndex > 0)
            {
                var lineNumberDashIndex = text.LastIndexOf("-", colonIndex, colonIndex, StringComparison.Ordinal);

                if (lineNumberDashIndex > 0)
                {
                    // -1 from dash index to account for the space between the filename and the paren
                    fileName = text.Substring(0, lineNumberDashIndex - 1);

                    if (TryParseLineAndColumn(text, colonIndex, out lineNumber, out columnNumber))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool TryParseSourceDataVB(string text, out string fileName, out int lineNumber, out int columnNumber)
        {
            // Text is of the form (x,y = line+col number):
            // [code] - [filename](x,y)
            // Filenames and can contain dashes so this will be tricky (aka hacky) trying to determine where the code ends and where the filename starts.
            // So our approach to obtaining the filename will be search for the last instance of " - ", and try to extract the filename to the right of that.
            // We then test if that filename is valid, and if not we move to the second last instance of " - " and so forth.
            // Note that IVSObjectList2.GetSourceContext is not implemented by VB so we cannot use that as an alternative method to determine the filename.
            fileName = "";
            lineNumber = 0;
            columnNumber = 0;

            // This is where the filename ends, because the last open paren signifies the start of the line and column number
            var fileNameEndIndex = text.LastIndexOf("(", StringComparison.Ordinal);
            if (fileNameEndIndex > 0)
            {
                if (TryParseVBFileName(text, fileNameEndIndex, fileNameEndIndex, out fileName)
                    &&
                    TryParseLineAndColumn(text, text.Length, out lineNumber, out columnNumber))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryParseVBFileName(string text, int endLastIndexCount, int fileNameEndIndex, out string fileName)
        {
            fileName = null;
            var dashIndex = text.LastIndexOf(DashWithSpaces, endLastIndexCount, endLastIndexCount, StringComparison.Ordinal);

            if (dashIndex > 0)
            {
                var potentialFileNameStartIndex = dashIndex + DashWithSpaces.Length;

                if (potentialFileNameStartIndex < fileNameEndIndex)
                {
                    var potentialFileName = text.Substring(potentialFileNameStartIndex, fileNameEndIndex - potentialFileNameStartIndex);

                    // Test if this file exists. If it doesn't, it's quite likely that the file path has a " - " inside it so we
                    // need to search for the previous dash in the text string.
                    if (File.Exists(potentialFileName))
                    {
                        fileName = potentialFileName;
                        return true;
                    }
                    else
                    {
                        return TryParseVBFileName(text, dashIndex, fileNameEndIndex, out fileName);
                    }
                }
            }

            return false;
        }
    }
}
