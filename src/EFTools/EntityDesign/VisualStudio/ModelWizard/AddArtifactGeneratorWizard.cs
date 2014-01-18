// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.TemplateWizard;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class AddArtifactGeneratorWizard : IWizard
    {
        private static Uri _edmxUri;

        // <summary>
        //     Use this EdmxUri property to pass in the selected EDMX file when the "add new item" dialog is launched from the escher designer
        // </summary>
        internal static Uri EdmxUri
        {
            get { return _edmxUri; }
            set
            {
                // Assert below because we expect the _edmxUri value to be cleared in a finally block after being set.
                Debug.Assert(_edmxUri == null || value == null, "_edmxUri is not empty when being set");
                _edmxUri = value;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="projectItem"> This API supports the Entity Framework infrastructure and is not intended to be used directly from your code. </param>
        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="project"> This API supports the Entity Framework infrastructure and is not intended to be used directly from your code. </param>
        public void ProjectFinishedGenerating(Project project)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="projectItem"> This API supports the Entity Framework infrastructure and is not intended to be used directly from your code. </param>
        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
#if DEBUG
            // assert that the EDMX & .tt file are in the same project
            if (EdmxUri != null)
            {
                var edmxProjectItem = VsUtils.GetProjectItemForDocument(EdmxUri.LocalPath, Services.ServiceProvider);
                var p1 = edmxProjectItem.ContainingProject;
                var p2 = projectItem.ContainingProject;
                Debug.Assert(p1 == p2, "Edmx file & .tt file are not in the same project!");
            }
#endif
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public void RunFinished()
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="automationObject"> This API supports the Entity Framework infrastructure and is not intended to be used directly from your code. </param>
        /// <param name="replacementsDictionary"> This API supports the Entity Framework infrastructure and is not intended to be used directly from your code. </param>
        /// <param name="runKind"> This API supports the Entity Framework infrastructure and is not intended to be used directly from your code. </param>
        /// <param name="customParams"> This API supports the Entity Framework infrastructure and is not intended to be used directly from your code. </param>
        public void RunStarted(
            object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            Project project = null;

            var dte = automationObject as DTE2;

            if (EdmxUri == null)
            {
                // Wizard not launched from Escher Designer
                project = VsUtils.GetActiveProject(dte);
                Debug.Assert(project != null, "Unable to retrieve ActiveSolutionProject from DTE");
            }
            else
            {
                // Try to get ProjectItem & project
                var projectItem = VsUtils.GetProjectItemForDocument(EdmxUri.LocalPath, Services.ServiceProvider);
                Debug.Assert(projectItem != null, "Unable to find project item for document");
                if (projectItem != null)
                {
                    // get the project that the input file is running in.
                    project = projectItem.ContainingProject;

                    // turn off code generation on the EDMX file
                    var editingContextMgr = PackageManager.Package.DocumentFrameMgr.EditingContextManager;

                    Debug.Assert(
                        editingContextMgr.DoesContextExist(EdmxUri),
                        "If the AddArtifactGeneratorWizard was launched from Escher, there should be an existing editing context");

                    if (editingContextMgr.DoesContextExist(EdmxUri))
                    {
                        var cpc = new CommandProcessorContext(
                            editingContextMgr.GetNewOrExistingContext(EdmxUri), EfiTransactionOriginator.AddNewArtifactGenerationItemId,
                            Resources.Tx_SetCodeGenerationStrategy);
                        var cmd = EdmUtils.SetCodeGenStrategyToNoneCommand(cpc.Artifact);
                        if (cmd != null)
                        {
                            CommandProcessor.InvokeSingleCommand(cpc, cmd);
                        }
                    }
                }
                else
                {
                    // Couldn't get projectItem for some reason, so default to the active project
                    project = VsUtils.GetActiveProject(dte);
                }

                // Wizard is launched from Escher Designer.  Name of the file in the designer is in the static EdmxUri property.
                var artifactProjectItem = VsUtils.GetProjectItemForDocument(EdmxUri.LocalPath, Services.ServiceProvider);
                var edmxFilePathInTemplate = String.Empty;
                if (VsUtils.IsLinkProjectItem(artifactProjectItem))
                {
                    // In order to determine the filename that will be placed in the .tt file, we need to first determine the 
                    // relative path of the *project item* to the root of the project, since the .tt file will be added as a sibling
                    // of the EDMX file and the project item itself is referencing a linked file. The scenario in mind is:
                    // ProjectRoot
                    //    '- FooFolder
                    //          '- Model.edmx (linked outside the project)
                    //          '- Model.tt <-- we want the .tt file here and the path in the template is relative to this location.
                    var parentItem = artifactProjectItem.Collection.Parent as ProjectItem;
                    var relativeProjectItemParentDir = String.Empty;
                    while (parentItem != null)
                    {
                        relativeProjectItemParentDir = Path.Combine(parentItem.Name, relativeProjectItemParentDir);
                        parentItem = parentItem.Collection.Parent as ProjectItem;
                    }

                    var projectDirectory = VsUtils.GetProjectRoot(project, Services.ServiceProvider);
                    if (projectDirectory != null)
                    {
                        var absoluteProjectItemParentDir = Path.Combine(projectDirectory.FullName, relativeProjectItemParentDir);

                        // Now we determine the relative path between the folder in the project that contains the ProjectItem and the actual path
                        // of the artifact. 
                        var artifactParentDirInfo = new DirectoryInfo(Path.GetDirectoryName(EdmxUri.LocalPath));
                        var absoluteProjectItemParentDirInfo = new DirectoryInfo(absoluteProjectItemParentDir);
                        var relativeDirPath = EdmUtils.GetRelativePath(artifactParentDirInfo, absoluteProjectItemParentDirInfo);
                        var fileName = Path.GetFileName(EdmxUri.LocalPath);
                        edmxFilePathInTemplate = Path.Combine(relativeDirPath, fileName);
                    }
                }
                else
                {
                    var fi = new FileInfo(EdmxUri.LocalPath);
                    edmxFilePathInTemplate = fi.Name;
                }

                Debug.Assert(!String.IsNullOrEmpty(edmxFilePathInTemplate), "edmxFilePathInTemplate was found to be null or empty");

                replacementsDictionary.Add("$edmxInputFile$", edmxFilePathInTemplate);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="filePath"> This API supports the Entity Framework infrastructure and is not intended to be used directly from your code. </param>
        /// <returns> This API supports the Entity Framework infrastructure and is not intended to be used directly from your code. </returns>
        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}
