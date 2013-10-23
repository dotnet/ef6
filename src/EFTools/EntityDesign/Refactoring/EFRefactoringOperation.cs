// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Refactoring
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Pluralization;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using EnvDTE80;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Common;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Refactoring;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal class EFRefactoringOperation : RefactoringOperationBase
    {
        private readonly EFRenameContributorInput _contributorInput;
        private readonly string _newName;
        private readonly EFNormalizableItem _objectToRename;
        private PreviewWindowInfo _previewWindowInfo;

        internal EFRefactoringOperation(
            EFNormalizableItem objectToRename, string newName, EFRenameContributorInput contributorInput, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Debug.Assert(contributorInput != null, "contributorInput != null");

            _contributorInput = contributorInput;
            _newName = newName;
            _objectToRename = objectToRename;
        }

        protected internal override string OperationName
        {
            get { return Resources.RefactorRenameOperation_Description; }
        }

        protected internal override string OperationNameDescription
        {
            get { return Resources.RefactorRenameOperation_Description; }
        }

        protected override PreviewWindowInfo PreviewWindowInfo
        {
            get
            {
                if (_previewWindowInfo == null)
                {
                    _previewWindowInfo = new PreviewWindowInfo();
                    _previewWindowInfo.ConfirmButtonText = Resources.RefactorRenameOperation_ConfirmButtonText;
                    _previewWindowInfo.Description = Resources.RefactorRenameOperation_Description;
                    _previewWindowInfo.TextViewDescription = Resources.RefactorRenameOperation_TextViewDescription;
                    _previewWindowInfo.Title = string.Format(CultureInfo.CurrentCulture, Resources.RefactorRenameOperation_Title);
                }
                return _previewWindowInfo;
            }
        }

        protected override string UndoDescription
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.RefactorRename_UndoDescription,
                    _contributorInput.OldName,
                    _contributorInput.NewName);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.TextManager.Interop.IVsLinkedUndoTransactionManager.AbortLinkedUndo")]
        protected override void OnApplyChanges()
        {
            // SCCI operation, check out any file that need to be changed.
            // If failed to check out any file or user cancelled check out, return.
            var filesToCheckOut = new List<String>(GetListOfFilesToCheckOut());
            if (!EnsureFileCheckOut(filesToCheckOut))
            {
                return;
            }

            // Set up linked undo
            var linkedUndoManager = ServiceProvider.GetService(typeof(SVsLinkedUndoTransactionManager)) as IVsLinkedUndoTransactionManager;

            var completedLinkedUndo = false;
            try
            {
                var hr = linkedUndoManager.OpenLinkedUndo(ApplyChangesUndoScope, UndoDescription);
                if (hr == VSConstants.S_OK)
                {
                    var linkedUndoHr = 0;

                    // Remember what are invisible editors we open, we will release them explicitly
                    // after apply changes are done.
                    var invisibleEditors = new List<IVsInvisibleEditor>();
                    try
                    {
                        var changeCount = FileChanges.Count;
                        string errorMessage = null;
                        string fileName = null;
                        try
                        {
                            for (var changeIndex = 0; changeIndex < changeCount && !IsCancelled && !ErrorOccurred; changeIndex++)
                            {
                                var fileChange = FileChanges[changeIndex];
                                if (fileChange.IsFileModified)
                                {
                                    // When apply change to a file, if that file is not in RDT, it will open that file in invisible editor.
                                    fileName = fileChange.FileName;
                                    var textBuffer = GetTextBufferForFile(fileName, invisibleEditors);
                                    if (textBuffer != null)
                                    {
                                        ApplyChangesToOneFile(fileChange, textBuffer, false, null);
                                    }
                                }
                            }

                            // Execute the rename command to update the CSDL model
                            var artifact = _objectToRename.Artifact as EntityDesignArtifact;
                            Debug.Assert(artifact != null, "Object being refactored does not have an EntityDesignArtifact parent.");
                            if (artifact != null)
                            {
                                var renameCommand = new EntityDesignRenameCommand(_objectToRename, _newName, false);
                                var cpc = new CommandProcessorContext(
                                    artifact.EditingContext, "EFRefactoringOperation->OnApplyChanges", Resources.Tx_RefactorRenameCommand);
                                CommandProcessor.InvokeSingleCommand(cpc, renameCommand);
                            }
                        }
                        catch (IOException)
                        {
                            errorMessage = string.Format(CultureInfo.CurrentCulture, Resources.Error_FailedApplyChangeToFile, fileName);
                        }
                        catch (InvalidOperationException)
                        {
                            errorMessage = string.Format(CultureInfo.CurrentCulture, Resources.Error_FailedApplyChangeToFile, fileName);
                        }

                        if (errorMessage != null)
                        {
                            // Any error occur, abort the transaction.
                            linkedUndoManager.AbortLinkedUndo();
                            OnError(errorMessage);
                        }
                        else
                        {
                            // Succeed, close linked undo and submit the transaction.
                            // This operation will automatically save any dirty file.
                            linkedUndoHr = linkedUndoManager.CloseLinkedUndo();
                            if (linkedUndoHr != VSConstants.S_OK)
                            {
                                linkedUndoManager.AbortLinkedUndo();
                            }
                        }

                        completedLinkedUndo = true;
                    }
                    finally
                    {
                        var editorCount = invisibleEditors.Count;
                        for (var editorIndex = 0; editorIndex < editorCount; editorIndex++)
                        {
                            // Close invisible editor from RDT
                            var invisibleEditor = invisibleEditors[editorIndex];
                            if (invisibleEditor != null)
                            {
                                Marshal.ReleaseComObject(invisibleEditor);
                            }
                        }
                    }
                }
                else
                {
                    OnError(string.Format(CultureInfo.CurrentCulture, Resources.Error_FailedApplyChangeToFile, string.Empty));
                }
            }
            finally
            {
                if (completedLinkedUndo == false)
                {
                    linkedUndoManager.AbortLinkedUndo();
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        protected override IList<FileChange> GetFileChanges()
        {
            var fileChangeMap = new Dictionary<string, FileChange>();

            using (WaitCursorHelper.NewWaitCursor())
            {
                var artifact = _contributorInput.ObjectToBeRenamed.Artifact;
                var artifactProjectItem = VsUtils.GetProjectItemForDocument(artifact.Uri.LocalPath, Services.ServiceProvider);

                if (artifactProjectItem != null)
                {
                    // Run the custom tool to ensure the generated code is up-to-date
                    VsUtils.RunCustomTool(artifactProjectItem);
                    var generatedCodeProjectItem = VsUtils.GetGeneratedCodeProjectItem(artifactProjectItem);

                    if (generatedCodeProjectItem != null)
                    {
                        var generatedItemPath = generatedCodeProjectItem.get_FileNames(1);
                        var objectSearchLanguage = FileExtensions.VbExt.Equals(
                            Path.GetExtension(generatedItemPath), StringComparison.OrdinalIgnoreCase)
                                                       ? ObjectSearchLanguage.VB
                                                       : ObjectSearchLanguage.CSharp;

                        var codeElements = generatedCodeProjectItem.FileCodeModel.CodeElements.OfType<CodeElement2>();
                        CodeElementRenameData renameData = null;

                        // Check if we're refactoring an EntityType or Property
                        var entityType = _contributorInput.ObjectToBeRenamed as EntityType;
                        var property = _contributorInput.ObjectToBeRenamed as Property;

                        if (entityType != null)
                        {
                            var newEntitySetName = _contributorInput.NewName;
                            var pluralize = ModelHelper.GetDesignerPropertyValueFromArtifactAsBool(
                                OptionsDesignerInfo.ElementName, OptionsDesignerInfo.AttributeEnablePluralization,
                                OptionsDesignerInfo.EnablePluralizationDefault, artifact);

                            // Pluralize the entity set name if the setting it turned on
                            if (pluralize)
                            {
                                var pluralizationService = DependencyResolver.GetService<IPluralizationService>();
                                newEntitySetName = pluralizationService.Pluralize(_contributorInput.NewName);
                            }

                            renameData = new CodeElementRenameData(
                                _contributorInput.NewName, newEntitySetName, _contributorInput.OldName, entityType.EntitySet.Name.Value);
                        }
                        else if (property != null)
                        {
                            if (property.EntityType != null)
                            {
                                renameData = new CodeElementRenameData(
                                    _contributorInput.NewName, _contributorInput.OldName, property.EntityType.Name.Value);
                            }
                        }

                        if (renameData != null)
                        {
                            var codeElementsToRename = new Dictionary<CodeElement2, Tuple<string, string>>();
                            CodeElementUtilities.FindRootCodeElementsToRename(
                                codeElements, renameData, generatedItemPath, objectSearchLanguage, ref codeElementsToRename);
                            var changeProposals = new List<ChangeProposal>();

                            // We may need to rename more than one object, as type names affect functions and entity set properties. This means we need to loop through and
                            // process each root item change in the generated code designer file.
                            foreach (var codeElementToRename in codeElementsToRename.Keys)
                            {
                                var nameTuple = codeElementsToRename[codeElementToRename];

                                CodeElementUtilities.CreateChangeProposals(
                                    codeElementToRename, nameTuple.Item1, nameTuple.Item2, generatedItemPath,
                                    changeProposals, objectSearchLanguage);
                            }

                            // Now sort the change proposals by filename so that we can return a list of file changes
                            foreach (var changeProposal in changeProposals)
                            {
                                FileChange fileChange;
                                HashSet<ChangeProposal> fileChangeProposals;

                                if (fileChangeMap.TryGetValue(changeProposal.FileName, out fileChange))
                                {
                                    fileChangeProposals = fileChange.ChangeList.Values.First();
                                }
                                else
                                {
                                    fileChange = new FileChange(changeProposal.FileName);
                                    fileChangeProposals = new HashSet<ChangeProposal>();
                                    fileChange.ChangeList.Add(
                                        new KeyValuePair<RefactoringPreviewGroup, HashSet<ChangeProposal>>(
                                            new RefactoringPreviewGroup(Resources.RefactorPreviewGroupName), fileChangeProposals));
                                    fileChangeMap.Add(changeProposal.FileName, fileChange);
                                }

                                if (!fileChangeProposals.Contains(changeProposal))
                                {
                                    fileChangeProposals.Add(changeProposal);
                                }
                            }
                        }
                    }
                }
            }

            return fileChangeMap.Values.ToList();
        }

        protected override ContributorInput OnGetContributorInput()
        {
            return _contributorInput;
        }
    }
}
