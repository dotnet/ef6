// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Refactoring
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.VisualStudio.Data.Tools.Design.XmlCore;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    ///     This class contains all preview data for an RefactorOperation.
    ///     Preview dialog will use all information in this class to populate the dialog.
    ///     Information contains the top level preview tree nodes, help context for this
    ///     RefactorOperaiton, confirm button text, etc.
    /// </summary>
    internal sealed class PreviewData
    {
        #region Private Members

        private Dictionary<string, FileChange> _fileChanges;

        #endregion

        public PreviewData()
        {
        }

        public PreviewData(PreviewWindowInfo previewWindowInfo)
        {
            ArgumentValidation.CheckForNullReference(previewWindowInfo, "previewWindowInfo");

            ConfirmButtonText = previewWindowInfo.ConfirmButtonText;
            Description = previewWindowInfo.Description;
            HelpContext = previewWindowInfo.HelpContext;
            TextViewDescription = previewWindowInfo.TextViewDescription;
            Title = previewWindowInfo.Title;
            Warning = previewWindowInfo.Warning;
            WarningLevel = previewWindowInfo.WarningLevel;
        }

        /// <summary>
        ///     Top level tree nodes for changes to preview.
        /// </summary>
        public IList<PreviewChangesNode> ChangeList { get; set; }

        /// <summary>
        ///     List of FileChange will be applied and previewed.
        /// </summary>
        public IList<FileChange> FileChanges
        {
            get
            {
                var changes = new List<FileChange>(_fileChanges.Values);
                return changes;
            }
            set { SetFileChanges(value); }
        }

        /// <summary>
        ///     Apply button text
        /// </summary>
        public string ConfirmButtonText { get; set; }

        /// <summary>
        ///     Discription of this RefactorOperation.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Help context for this RefactorOperation.
        /// </summary>
        public string HelpContext { get; set; }

        /// <summary>
        ///     The text view discription, that appears on the header of the
        ///     text view in preview dialog.
        /// </summary>
        public string TextViewDescription { get; set; }

        /// <summary>
        ///     Preview dialog title for this RefactorOperation
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     Warning text in the preview dialog for this RefactorOperation.
        /// </summary>
        public string Warning { get; set; }

        /// <summary>
        ///     Warning level for this RefactorOperaiton.
        /// </summary>
        public __PREVIEWCHANGESWARNINGLEVEL WarningLevel { get; set; }

        /// <summary>
        ///     Get FileChange for a previewNode.
        ///     After user clicks a preview node, the preview dialog will show
        ///     content of the file this change will be applied to.  And the content
        ///     of that file will contains all the selected changes to that file and
        ///     will show user the version with changes already applied.
        /// </summary>
        /// <param name="previewNode"></param>
        /// <returns>
        ///     FileChange related to this node.  If this change is atomic change,
        ///     or the node is file node, it will return the FileChange object for
        ///     the file it is changing.
        ///     If the node is a group node, it will return null.
        /// </returns>
        public FileChange GetFileChange(PreviewChangesNode previewNode)
        {
            ArgumentValidation.CheckForNullReference(previewNode, "previewNode");

            string fileName = null;
            var changeProposal = previewNode.ChangeProposal;
            if (changeProposal != null)
            {
                // Preview Node for atomic change
                fileName = changeProposal.FileName;
            }
            else
            {
                // Look at direct child, if that child is an atomic change,
                // then get filename for it, otherwise, just leave the filename as null
                if (previewNode.ChildList != null
                    && previewNode.ChildList.Count > 0)
                {
                    var proposal = previewNode.ChildList[0].ChangeProposal;
                    if (proposal != null)
                    {
                        fileName = proposal.FileName;
                    }
                }
            }

            FileChange fileChange = null;

            if (fileName != null)
            {
                _fileChanges.TryGetValue(fileName, out fileChange);
            }
            return fileChange;
        }

        private void SetFileChanges(IList<FileChange> changes)
        {
            _fileChanges = new Dictionary<string, FileChange>(StringComparer.CurrentCulture);
            if (changes != null)
            {
                var fileCount = changes.Count;
                for (var fileIndex = 0; fileIndex < fileCount; fileIndex++)
                {
                    var fileChange = changes[fileIndex];
                    var filename = fileChange.FileName;
                    if (!_fileChanges.ContainsKey(filename))
                    {
                        _fileChanges.Add(filename, fileChange);
                    }
                    else
                    {
                        // Something wrong here, after merging those ChangeProposal, we should not
                        // have two FileChange with same name.
                        throw new InvalidOperationException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.Exception_DuplicateChanges,
                                filename));
                    }
                }
            }
        }
    }
}
