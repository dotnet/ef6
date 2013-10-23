// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Refactoring
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Common;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Data.Tools.Design.XmlCore;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    ///     Converts a list of file change nodes into preview nodes to be displayed in the preview dialog. Different implementers of
    ///     the RefactoringOperation can
    /// </summary>
    internal class PreviewChangesNodeBuilder
    {
        private const string CSharpRootNodeText = "C#";
        private const string VBRootNodeText = "VB";

        /// <summary>
        ///     Creates a list of preview nodes to be displayed in the refactoring preview dialog.
        /// </summary>
        /// <param name="fileChanges">List of all file-based change proposals</param>
        /// <returns></returns>
        internal virtual IList<PreviewChangesNode> Build(IList<FileChange> fileChanges)
        {
            var previews = new List<PreviewChangesNode>();

            // Loop through all file changes, create related preview group nodes under different projects.
            if (fileChanges != null)
            {
                // VsLang nodes should appear after T-Sql nodes
                previews.AddRange(CreatePreviewNodesForVsLang(fileChanges));
            }

            // No change proposals so return an empty preview node
            if (previews.Count == 0)
            {
                previews.Add(CreateEmptyNode());
            }

            return previews;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected static List<PreviewChangesNode> CreatePreviewNodesForVsLang(
            IList<FileChange> fileChanges, bool placeNodesUnderSingleRoot = false)
        {
            var vsLangObjectNodes = new List<PreviewChangesNode>();
            var rootToFileToChangesMap = new Dictionary<string, Dictionary<string, List<PreviewChangesNode>>>();
            var isCSharpChange = true;

            foreach (var fileChange in fileChanges)
            {
                if (fileChange.ChangeList != null)
                {
                    if (fileChange.ChangeList.Count > 0)
                    {
                        using (var textBuffer = VsTextLinesFromFile.Load(fileChange.FileName))
                        {
                            if (textBuffer != null)
                            {
                                foreach (
                                    var vsLangTextChange in
                                        fileChange.ChangeList.SelectMany(cl => cl.Value).OfType<VsLangTextChangeProposal>())
                                {
                                    if (vsLangTextChange.IsRootChange)
                                    {
                                        // Create object definition node                                        
                                        var rootNode = new PreviewChangesNode(
                                            vsLangTextChange.ObjectDefinitionFullName
                                            , new VSTREEDISPLAYDATA()
                                            , vsLangTextChange.ObjectDefinitionFullName
                                            , new List<PreviewChangesNode>()
                                            , vsLangTextChange);

                                        rootNode.CheckState = vsLangTextChange.Included
                                                                  ? __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked
                                                                  : __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Unchecked;
                                        vsLangObjectNodes.Add(rootNode);

                                        // It's possible that a non-root change was processed first, in which case the dictionary will already be updated
                                        // with the root node name. This means we need to check the dictionary before adding to it.
                                        if (!rootToFileToChangesMap.ContainsKey(vsLangTextChange.ObjectDefinitionFullName))
                                        {
                                            rootToFileToChangesMap.Add(
                                                vsLangTextChange.ObjectDefinitionFullName,
                                                new Dictionary<string, List<PreviewChangesNode>>());
                                        }
                                    }
                                    else
                                    {
                                        // Get display text, trim the leading space of the text for display purpose.
                                        var displayText = string.Empty;
                                        int lineLength;

                                        if (ErrorHandler.Succeeded(textBuffer.GetLengthOfLine(vsLangTextChange.StartLine, out lineLength)))
                                        {
                                            if (
                                                ErrorHandler.Succeeded(
                                                    textBuffer.GetLineText(
                                                        vsLangTextChange.StartLine, 0, vsLangTextChange.StartLine, lineLength,
                                                        out displayText)))
                                            {
                                                var length = displayText.Length;
                                                displayText = displayText.TrimStart();
                                                var spaceLength = length - displayText.Length;

                                                Debug.Assert(
                                                    spaceLength <= vsLangTextChange.StartColumn,
                                                    "Start column of selection is negative, HydratedVsLangRefactor.CreatePreviewNodeForChanges()");

                                                if (spaceLength <= vsLangTextChange.StartColumn)
                                                {
                                                    var changeNodeDisplayData = new VSTREEDISPLAYDATA();
                                                    changeNodeDisplayData.State = (uint)_VSTREEDISPLAYSTATE.TDS_FORCESELECT;
                                                    changeNodeDisplayData.ForceSelectStart =
                                                        (ushort)(vsLangTextChange.StartColumn - spaceLength);
                                                    changeNodeDisplayData.ForceSelectLength = (ushort)(vsLangTextChange.Length);

                                                    var changeNode = new PreviewChangesNode(
                                                        displayText, changeNodeDisplayData, displayText, null, vsLangTextChange);

                                                    // Add checked checkbox
                                                    changeNode.ShowCheckBox = true;
                                                    changeNode.CheckState = vsLangTextChange.Included
                                                                                ? __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked
                                                                                : __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Unchecked;

                                                    // Apply the language service to this change.
                                                    Guid languageServiceId;
                                                    textBuffer.GetLanguageServiceID(out languageServiceId);
                                                    changeNode.LanguageServiceID = languageServiceId;

                                                    Dictionary<string, List<PreviewChangesNode>> fileToChangesMap;
                                                    if (rootToFileToChangesMap.TryGetValue(
                                                        vsLangTextChange.ObjectDefinitionFullName, out fileToChangesMap))
                                                    {
                                                        List<PreviewChangesNode> changeNodes;
                                                        if (fileToChangesMap.TryGetValue(fileChange.FileName, out changeNodes))
                                                        {
                                                            changeNodes.Add(changeNode);
                                                        }
                                                        else
                                                        {
                                                            // There are no changes for the file listed under this root node, so we need to create it
                                                            changeNodes = new List<PreviewChangesNode> { changeNode };
                                                            fileToChangesMap.Add(fileChange.FileName, changeNodes);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // There are no changes processed yet for this object name, so we need to update our dictionary with
                                                        // markers for creating a new root node, a new file node, and a new change node.
                                                        fileToChangesMap = new Dictionary<string, List<PreviewChangesNode>>();
                                                        fileToChangesMap.Add(
                                                            fileChange.FileName, new List<PreviewChangesNode> { changeNode });
                                                        rootToFileToChangesMap.Add(
                                                            vsLangTextChange.ObjectDefinitionFullName, fileToChangesMap);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Now that all the changes have been sorted under the correct root nodes in our dictionary, create the File nodes that connect
            // the root nodes to the change nodes.
            foreach (var rootNode in vsLangObjectNodes)
            {
                Dictionary<string, List<PreviewChangesNode>> fileToChangesMap;
                if (rootToFileToChangesMap.TryGetValue(rootNode.DisplayText, out fileToChangesMap))
                {
                    if (fileToChangesMap != null)
                    {
                        foreach (var fileName in fileToChangesMap.Keys)
                        {
                            var fileNodeDisplayData = new VSTREEDISPLAYDATA();

                            if (FileExtensions.VbExt.Equals(Path.GetExtension(fileName), StringComparison.OrdinalIgnoreCase))
                            {
                                fileNodeDisplayData.Image = fileNodeDisplayData.SelectedImage = CommonConstants.OM_GLYPH_VBPROJECT;
                                isCSharpChange = false;
                            }
                            else
                            {
                                fileNodeDisplayData.Image = fileNodeDisplayData.SelectedImage = CommonConstants.OM_GLYPH_CSHARPFILE;
                            }

                            var shortFileName = Path.GetFileName(fileName);
                            var checkState = DetermineCheckState(fileToChangesMap[fileName]);
                            var fileNode = new PreviewChangesNode(shortFileName, fileNodeDisplayData, shortFileName, null, null);
                            fileNode.AddChildNodes(fileToChangesMap[fileName]);

                            // Add checked checkbox
                            fileNode.ShowCheckBox = true;
                            fileNode.CheckState = checkState;
                            rootNode.AddChildNode(fileNode);

                            // Update root check state
                            if (rootNode.ChildList.Count == 1)
                            {
                                // This is the first child, so the root should match the child check state
                                rootNode.CheckState = checkState;
                            }
                            else
                            {
                                switch (rootNode.CheckState)
                                {
                                    case __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked:
                                        {
                                            if (checkState == __PREVIEWCHANGESITEMCHECKSTATE.PCCS_PartiallyChecked
                                                || checkState == __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Unchecked)
                                            {
                                                rootNode.CheckState = __PREVIEWCHANGESITEMCHECKSTATE.PCCS_PartiallyChecked;
                                            }
                                            break;
                                        }
                                    case __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Unchecked:
                                        {
                                            if (checkState == __PREVIEWCHANGESITEMCHECKSTATE.PCCS_PartiallyChecked
                                                || checkState == __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked)
                                            {
                                                rootNode.CheckState = __PREVIEWCHANGESITEMCHECKSTATE.PCCS_PartiallyChecked;
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                    }
                }
            }

            if (placeNodesUnderSingleRoot)
            {
                PreviewChangesNode rootNode;
                var fileNodeDisplayData = new VSTREEDISPLAYDATA();

                if (isCSharpChange)
                {
                    fileNodeDisplayData.Image = fileNodeDisplayData.SelectedImage = CommonConstants.OM_GLYPH_CSHARPFILE;
                    rootNode = new PreviewChangesNode(CSharpRootNodeText, fileNodeDisplayData, null, null, null);
                }
                else
                {
                    fileNodeDisplayData.Image = fileNodeDisplayData.SelectedImage = CommonConstants.OM_GLYPH_VBPROJECT;
                    rootNode = new PreviewChangesNode(VBRootNodeText, fileNodeDisplayData, null, null, null);
                }

                rootNode.ShowCheckBox = true;
                rootNode.CheckState = DetermineCheckState(vsLangObjectNodes);
                rootNode.AddChildNodes(vsLangObjectNodes);
                return new List<PreviewChangesNode> { rootNode };
            }
            else
            {
                return vsLangObjectNodes;
            }
        }

        protected virtual PreviewChangesNode CreateEmptyNode()
        {
            return CreatePreviewNode(
                Resources.RefactoringOperation_NoChanges, CommonConstants.OM_GLYPH_REFERENCE, IntPtr.Zero, null, false, false, false);
        }

        protected static PreviewChangesNode CreatePreviewNode(
            string displayText,
            ushort icon,
            IntPtr imageList,
            TextChangeProposal proposal,
            bool forceSelection,
            bool enableChangeUncheck,
            bool isChecked)
        {
            var displayData = new VSTREEDISPLAYDATA();

            if (forceSelection)
            {
                Debug.Assert(!string.IsNullOrEmpty(displayText), "display text is null or empty");
                Debug.Assert(proposal != null, "proposal is null");

                var length = displayText.Length;
                displayText = displayText.TrimStart();
                var spaceLength = length - displayText.Length;

                displayData.State = (uint)_VSTREEDISPLAYSTATE.TDS_FORCESELECT;
                displayData.ForceSelectStart = (ushort)(proposal.StartColumn - spaceLength);
                displayData.ForceSelectLength = (ushort)(proposal.EndColumn - proposal.StartColumn);
            }

            if (imageList != IntPtr.Zero)
            {
                displayData.hImageList = imageList;
            }

            displayData.Image = displayData.SelectedImage = icon;

            var node = new PreviewChangesNode(displayText, displayData, displayText, null, proposal);

            if (enableChangeUncheck)
            {
                AddCheckBoxToPreviewNode(node, true, isChecked);
            }

            return node;
        }

        protected static void AddCheckBoxToPreviewNode(PreviewChangesNode previewNode, bool setCheckState, bool check)
        {
            ArgumentValidation.CheckForNullReference(previewNode, "previewNode");
            previewNode.ShowCheckBox = true;
            if (setCheckState)
            {
                if (check)
                {
                    previewNode.CheckState = __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked;
                }
                else
                {
                    previewNode.CheckState = __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Unchecked;
                }
            }
        }

        protected static __PREVIEWCHANGESITEMCHECKSTATE DetermineCheckState(IList<PreviewChangesNode> children)
        {
            if (children == null
                || children.Count == 0)
            {
                return __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked;
            }
            else
            {
                var foundUncheckedChild = false;
                var foundCheckedChild = false;

                foreach (var child in children)
                {
                    if (child.CheckState == __PREVIEWCHANGESITEMCHECKSTATE.PCCS_PartiallyChecked)
                    {
                        // If any child is partially checked, the parent must be partially checked regardless of the state of other children
                        return __PREVIEWCHANGESITEMCHECKSTATE.PCCS_PartiallyChecked;
                    }
                    else if (child.CheckState == __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Unchecked)
                    {
                        foundUncheckedChild = true;
                    }
                    else if (child.CheckState == __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked)
                    {
                        foundCheckedChild = true;
                    }

                    // If we have at least one unchecked child and at least one checked child, the parent must be partially checked
                    if (foundCheckedChild && foundUncheckedChild)
                    {
                        return __PREVIEWCHANGESITEMCHECKSTATE.PCCS_PartiallyChecked;
                    }
                }

                // If we get down here, either all children were all checked or they were all unchecked
                if (foundCheckedChild)
                {
                    return __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Checked;
                }
                else
                {
                    return __PREVIEWCHANGESITEMCHECKSTATE.PCCS_Unchecked;
                }
            }
        }
    }
}
