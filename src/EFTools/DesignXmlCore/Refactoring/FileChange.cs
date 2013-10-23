// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Refactoring
{
    using System.Collections.Generic;
    using Microsoft.Data.Entity.Design.Common;

    /// <summary>
    ///     Class represents changes for one file.
    ///     Those changes can from different RefactoringContributor and can from
    ///     different RefactoringPreviewGroup.
    ///     After RefactoringContributorManager returns a list of ChangeProposal,
    ///     RefactorOperation will group those changes to a list of FileChange.
    ///     RefactorOperation will use this list of FileChange to apply change to
    ///     each file.  And it will also convert this list to list of PreviewChangesNode,
    ///     and show these changes in preview dialog.
    /// </summary>
    internal sealed class FileChange
    {
        private readonly Dictionary<RefactoringPreviewGroup, HashSet<ChangeProposal>> _changeList;
        private readonly string _filename;

        /// <summary>
        ///     Create FileChange associated with file
        /// </summary>
        /// <param name="fileName">Full path to the file being changed</param>
        public FileChange(string fileName)
            : this(fileName, string.Empty)
        {
        }

        /// <summary>
        ///     Create FileChange associated with file and project
        /// </summary>
        /// <param name="fileName">Full path to the file being changed</param>
        /// <param name="projectName">Name of the project the file belongs to</param>
        public FileChange(string fileName, string projectName)
        {
            ArgumentValidation.CheckForEmptyString(fileName, "fileName");
            ArgumentValidation.CheckForNullReference(projectName, "projectName");

            _filename = fileName;
            ProjectName = projectName;
            _changeList = new Dictionary<RefactoringPreviewGroup, HashSet<ChangeProposal>>();
        }

        /// <summary>
        ///     The name of this file.
        /// </summary>
        public string FileName
        {
            get { return _filename; }
        }

        /// <summary>
        ///     The name of the project project of this file if there is a parent project.
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        ///     All the ChangeProposal in this file.
        ///     They are groupped by RefactoringPreviewGroup instance.  So they can show under
        ///     different Preview group node in the preview dialog.
        /// </summary>
        internal IDictionary<RefactoringPreviewGroup, HashSet<ChangeProposal>> ChangeList
        {
            get { return _changeList; }
        }

        /// <summary>
        ///     Check if any change in this file is included in apply changes, if any change
        ///     is included, that means the related file will be modified.
        /// </summary>
        public bool IsFileModified
        {
            get
            {
                foreach (var previewGroup in _changeList.Keys)
                {
                    var changes = _changeList[previewGroup];
                    foreach (var change in changes)
                    {
                        if (change.Included)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        ///     Get the number of change proposal in this file that are included.
        /// </summary>
        internal int AppliedChangesCount
        {
            get
            {
                var includedCount = 0;
                foreach (var previewGroup in _changeList.Keys)
                {
                    var changes = _changeList[previewGroup];
                    foreach (var change in changes)
                    {
                        if (change.Included)
                        {
                            includedCount++;
                        }
                    }
                }
                return includedCount;
            }
        }

        /// <summary>
        ///     Add a ChangeProposal to change list of a related PreviewGroup.
        /// </summary>
        /// <param name="previewGroup">Preview group this change will be added to.</param>
        /// <param name="change">The ChangeProposal.</param>
        internal void AddChange(RefactoringPreviewGroup previewGroup, ChangeProposal change)
        {
            ArgumentValidation.CheckForNullReference(previewGroup, "previewGroup");
            ArgumentValidation.CheckForNullReference(change, "change");

            // First check if changes for same location exists or not, if exist, we will not add it anymore
            var exist = false;
            foreach (var existingChanges in _changeList.Values)
            {
                if (existingChanges.Contains(change))
                {
                    exist = true;
                    break;
                }
            }
            if (!exist)
            {
                HashSet<ChangeProposal> changes = null;
                if (_changeList.TryGetValue(previewGroup, out changes) == false)
                {
                    // There is no change list for this preview group,
                    // create one and add to the dictionary.
                    changes = new HashSet<ChangeProposal>();
                    _changeList.Add(previewGroup, changes);
                }
                // Add change to the change list.
                changes.Add(change);
            }
        }

        /// <summary>
        ///     Get list of ChangeProposal for a RefactoringPreviewGroup in this file.
        /// </summary>
        /// <param name="previewGroup"></param>
        /// <returns>A list of ChangeProposal for a RefactoringPreviewGroup in this file.</returns>
        internal IList<ChangeProposal> GetChanges(RefactoringPreviewGroup previewGroup)
        {
            ArgumentValidation.CheckForNullReference(previewGroup, "previewGroup");

            List<ChangeProposal> results = null;
            HashSet<ChangeProposal> changes = null;
            if (_changeList.TryGetValue(previewGroup, out changes) == false)
            {
                results = new List<ChangeProposal>();
            }
            else
            {
                results = new List<ChangeProposal>(changes);
            }
            return results;
        }
    }
}
