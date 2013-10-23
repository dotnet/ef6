// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Refactoring
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Common;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Refactoring;

    internal static class CodeElementUtilities
    {
        private const string AddToFuncFormat = "AddTo{0}";
        private const string CreateFuncFormat = "Create{0}";
        private const string VBClassKeyword = "Class";
        private const string VBPropertyKeyword = "Property";
        private const string VBFunctionKeyword = "Function";

        private struct RefactorChange
        {
            public RefactorChange(string displayText, string toolTipText, int lineNumber, int columnNumber)
            {
                DisplayText = displayText;
                ToolTipText = toolTipText;
                LineNumber = lineNumber;
                ColumnNumber = columnNumber;
            }

            public readonly int ColumnNumber;
            public string DisplayText;
            public readonly int LineNumber;
            public string ToolTipText;
        }

        internal static void CreateChangeProposals(CodeElement2 codeElement, string newName, string oldName,
            string generatedItemPath, IList<ChangeProposal> changeProposals, ObjectSearchLanguage objectSearchLanguage)
        {
            if (codeElement != null)
            {
                var searchResults = VsObjectSearchResult.Search(codeElement.FullName, objectSearchLanguage);

                if (searchResults != null)
                {
                    // VB search results don't include the designer file, since the IVsObjectList item for the designer file in VB does not contain the
                    // column number for designer file references (since VB returns a hierarchical IVsObjectList and does not show column numbers in the
                    // root nodes). So instead we'll put the info from the CodeElement into the search results, which is less
                    // than ideal since the display text is not consistent with the IVsObjectList items...
                    if (objectSearchLanguage == ObjectSearchLanguage.VB
                        && codeElement.ProjectItem != null)
                    {
                        searchResults.Add(CreateVBCodeElementSearchResult(codeElement));
                    }

                    if (searchResults.Count > 0)
                    {
                        var fileToChanges = new Dictionary<string, List<RefactorChange>>();
                        var designerFileChanges = new List<RefactorChange>();

                        foreach (var searchResult in searchResults)
                        {
                            // Don't show changes in generated file in the preview dialog since those changes will always be applied after
                            // hydration completes. Instead we just show the changes from the exercising (application) code.
                            if (!string.Equals(searchResult.FileName, generatedItemPath, StringComparison.OrdinalIgnoreCase))
                            {
                                List<RefactorChange> refactorChanges;
                                if (!fileToChanges.TryGetValue(searchResult.FileName, out refactorChanges))
                                {
                                    refactorChanges = new List<RefactorChange>();
                                    fileToChanges.Add(searchResult.FileName, refactorChanges);
                                }

                                refactorChanges.Add(
                                    new RefactorChange(
                                        searchResult.DisplayText, searchResult.DisplayText, searchResult.LineNumber,
                                        searchResult.ColumnNumber));
                            }
                            else
                            {
                                // We need to know the location of the type definition in the generated code file so we can populate the
                                // root node of the preview window, so we save off a list of all references to the renamed object in
                                // the designer file here.
                                designerFileChanges.Add(
                                    new RefactorChange(
                                        searchResult.DisplayText, searchResult.DisplayText, searchResult.LineNumber,
                                        searchResult.ColumnNumber));
                            }
                        }

                        // Add designer file change
                        var rootFileName = codeElement.ProjectItem.get_FileNames(1);
                        bool doesProjectHaveFileName;
                        var rootProjectName = VsUtils.GetProjectPathWithName(
                            codeElement.ProjectItem.ContainingProject, out doesProjectHaveFileName);

                        // The code element start line will include attributes for the class, so the get the line where the actual class name is
                        // we need to use the results from the IVsObjectSearch and get the search result immediately after the CodeElements startline.
                        var minDelta = int.MaxValue;
                        RefactorChange? rootNodeChange = null;
                        foreach (var change in designerFileChanges)
                        {
                            var lineDelta = change.LineNumber - codeElement.StartPoint.Line;

                            if (lineDelta >= 0
                                && lineDelta < minDelta)
                            {
                                minDelta = lineDelta;
                                rootNodeChange = change;
                            }
                        }

                        if (rootNodeChange != null)
                        {
                            var textChangeProposal = new VsLangTextChangeProposal(
                                rootProjectName, rootFileName, newName, codeElement.FullName, true);
                            textChangeProposal.StartColumn = rootNodeChange.Value.ColumnNumber - 1;
                            textChangeProposal.EndColumn = textChangeProposal.StartColumn + oldName.Length;
                            textChangeProposal.Length = oldName.Length;
                            textChangeProposal.StartLine = rootNodeChange.Value.LineNumber - 1;
                            textChangeProposal.EndLine = textChangeProposal.StartLine;
                            textChangeProposal.Included = true;
                            changeProposals.Add(textChangeProposal);
                        }

                        // Add application code changes
                        foreach (var fileName in fileToChanges.Keys)
                        {
                            var owningProject = VSHelpers.GetProjectForDocument(fileName);
                            var changes = fileToChanges[fileName];

                            if (changes.Count > 0
                                && owningProject != null)
                            {
                                foreach (var change in changes)
                                {
                                    // Text buffer is zero based but find all refs is one based, so subtract 1.
                                    var textBufferLine = change.LineNumber - 1;
                                    var textBufferColumn = change.ColumnNumber - 1;

                                    bool projectHasFilename;
                                    var projectFullPath = VsUtils.GetProjectPathWithName(owningProject, out projectHasFilename);
                                    var textChangeProposal = new VsLangTextChangeProposal(
                                        projectFullPath, fileName, newName, codeElement.FullName);
                                    textChangeProposal.StartColumn = textBufferColumn;
                                    textChangeProposal.EndColumn = textChangeProposal.StartColumn + oldName.Length;
                                    textChangeProposal.Length = oldName.Length;
                                    textChangeProposal.StartLine = textBufferLine;
                                    textChangeProposal.EndLine = textBufferLine;
                                    textChangeProposal.Included = true;
                                    changeProposals.Add(textChangeProposal);
                                }
                            }
                        }
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "CodeElement")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "CreateVBCodeElementSearchResult")]
        private static VsObjectSearchResult CreateVBCodeElementSearchResult(CodeElement2 codeElement)
        {
            var column = codeElement.StartPoint.LineCharOffset;
            var line = codeElement.StartPoint.Line;
            int lineLength;
            string lineText;
            string prefix;

            // Try to locate the element name on the startline.
            using (var textBuffer = VsTextLinesFromFile.Load(codeElement.ProjectItem.get_FileNames(1)))
            {
                // Buffer values are zero based, codeElement values are 1 based.
                var bufferLine = line - 1;
                NativeMethods.ThrowOnFailure(textBuffer.GetLengthOfLine(bufferLine, out lineLength));
                NativeMethods.ThrowOnFailure(textBuffer.GetLineText(bufferLine, 0, bufferLine, lineLength, out lineText));
            }

            // Skip past the prefix, which is different depending on whether we are processing a class/property/function
            switch (codeElement.Kind)
            {
                case vsCMElement.vsCMElementClass:
                    prefix = VBClassKeyword;
                    break;
                case vsCMElement.vsCMElementProperty:
                    prefix = VBPropertyKeyword;
                    break;
                case vsCMElement.vsCMElementFunction:
                    prefix = VBFunctionKeyword;
                    break;
                default:
                    throw new NotImplementedException(
                        "CreateVBCodeElementSearchResult() does not implement handlers for CodeElement type: " + codeElement.Kind.ToString());
            }

            var prefixStartIndex = lineText.IndexOf(prefix, 0, StringComparison.OrdinalIgnoreCase);
            if (prefixStartIndex >= 0)
            {
                var prefixEndIndex = prefixStartIndex + prefix.Length;
                if (prefixEndIndex < lineText.Length)
                {
                    var elementNameStartIndex = lineText.IndexOf(codeElement.Name, prefixEndIndex, StringComparison.OrdinalIgnoreCase);
                    if (elementNameStartIndex >= 0)
                    {
                        // Buffer values are zero based, FindAllRef values are 1 based.
                        column = elementNameStartIndex + 1;
                    }
                }
            }

            return new VsObjectSearchResult(codeElement.ProjectItem.get_FileNames(1), codeElement.FullName, line, column);
        }

        internal static void FindRootCodeElementsToRename(
            IEnumerable<CodeElement2> codeElements,
            CodeElementRenameData renameData,
            string generatedItemPath,
            ObjectSearchLanguage objectSearchLanguage,
            ref Dictionary<CodeElement2, Tuple<string, string>> codeElementsToRename)
        {
            if (codeElementsToRename == null)
            {
                codeElementsToRename = new Dictionary<CodeElement2, Tuple<string, string>>();
            }

            var newName = renameData.NewName;
            var oldName = renameData.OldName;
            var targetType = renameData.RefactorTargetType;

            // Rename the symbols that match the old name
            foreach (var codeElement in codeElements)
            {
                if (codeElement.Kind == vsCMElement.vsCMElementNamespace)
                {
                    FindRootCodeElementsToRename(
                        codeElement.Children.OfType<CodeElement2>(), renameData, generatedItemPath, objectSearchLanguage,
                        ref codeElementsToRename);
                }
                else if (codeElement.Kind == vsCMElement.vsCMElementClass)
                {
                    if (targetType == RefactorTargetType.Class)
                    {
                        if (codeElement.Name.Equals(oldName, StringComparison.Ordinal))
                        {
                            codeElementsToRename.Add(codeElement, new Tuple<string, string>(newName, oldName));
                        }

                        // If we're refactoring a class, we need to iterate another level deeper even if the name of this Type doesn't match what we're refactoring
                        // since we need to rename the AddTo*() functions
                        FindRootCodeElementsToRename(
                            codeElement.Children.OfType<CodeElement2>(), renameData, generatedItemPath, objectSearchLanguage,
                            ref codeElementsToRename);
                    }
                    else if (targetType == RefactorTargetType.Property)
                    {
                        // If we're refactoring a property we need to iterate another level deeper to find the property elements.
                        FindRootCodeElementsToRename(
                            codeElement.Children.OfType<CodeElement2>(), renameData, generatedItemPath, objectSearchLanguage,
                            ref codeElementsToRename);
                    }
                }
                else if (codeElement.Kind == vsCMElement.vsCMElementFunction
                         && targetType == RefactorTargetType.Class)
                {
                    // Functions use the entity set name, so check for pluralization
                    string oldFuncName;
                    string newFuncName;

                    var function = (CodeFunction)codeElement;
                    var parentType = function.Parent as CodeType;
                    var oldFactoryFuncName = string.Format(CultureInfo.InvariantCulture, CreateFuncFormat, oldName);
                    if (parentType != null
                        && parentType.Name.Equals(oldName, StringComparison.Ordinal)
                        && function.Name.Equals(oldFactoryFuncName, StringComparison.Ordinal))
                    {
                        // We need to rename Create* factory funcs as they use the class name.
                        codeElementsToRename.Add(
                            codeElement,
                            new Tuple<string, string>(
                                string.Format(CultureInfo.InvariantCulture, CreateFuncFormat, newName), oldFactoryFuncName));
                    }
                    else
                    {
                        // We also need to rename the AddTo* funcs when we rename classes, since the class name is used in the generated func name.
                        oldFuncName = string.Format(CultureInfo.InvariantCulture, AddToFuncFormat, renameData.OldEntitySetName);

                        if (codeElement.Name.Equals(oldFuncName, StringComparison.Ordinal))
                        {
                            newFuncName = string.Format(CultureInfo.InvariantCulture, AddToFuncFormat, renameData.NewEntitySetName);
                            codeElementsToRename.Add(codeElement, new Tuple<string, string>(newFuncName, oldFuncName));
                        }
                    }
                }
                else if (codeElement.Kind == vsCMElement.vsCMElementProperty)
                {
                    if (targetType == RefactorTargetType.Property)
                    {
                        if (codeElement.Name.Equals(oldName, StringComparison.Ordinal))
                        {
                            // Ensure the class name matches as well
                            var splitFullName = codeElement.FullName.Split('.');

                            if (splitFullName.Length >= 2
                                && splitFullName[splitFullName.Length - 2].Equals(renameData.ParentEntityTypeName, StringComparison.Ordinal))
                            {
                                codeElementsToRename.Add(codeElement, new Tuple<string, string>(newName, oldName));
                            }
                        }
                    }
                    else if (targetType == RefactorTargetType.Class)
                    {
                        // There is a property on the container which contains the entity set.
                        if (codeElement.Name.Equals(renameData.OldEntitySetName, StringComparison.Ordinal))
                        {
                            codeElementsToRename.Add(
                                codeElement, new Tuple<string, string>(renameData.NewEntitySetName, renameData.OldEntitySetName));
                        }
                    }
                }
            }
        }
    }
}
