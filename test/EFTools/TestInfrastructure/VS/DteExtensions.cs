// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesignerTestInfrastructure.VS
{
    #region Using Statements

    using System;
    using System.Diagnostics;
    using System.IO;
    using EnvDTE;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Constants = EnvDTE.Constants;
    using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

    #endregion

    #region VS IDE related utitilies

    /// <summary>
    ///     Helper functions relate to winfrom
    /// </summary>
    public static class DteExtensions
    {
        /// <summary>
        ///     project language
        /// </summary>
        public enum ProjectLanguage
        {
            CSharp = 0,
            VB,
        }

        /// <summary>
        ///     project kind
        /// </summary>
        public enum ProjectKind
        {
            Libary = 0,
            Executable
        }

        public static readonly string[][][] ProjectTemplates = new string[2][][]
            {
                #region C#
                new[]
                    {
                        /* { default template path, template file } */
                        new[] { @"CSharp\Windows", @"ClassLibrary.zip\csClassLibrary.vstemplate" },
                        new[] { @"CSharp\Windows", @"ConsoleApplication.zip\csConsoleApplication.vstemplate" }
                    },

                #endregion
            
                #region VB
                new[]
                    {
                        new[] { @"VisualBasic\Windows", @"ClassLibrary.zip\classlibrary.vstemplate" },
                        new[] { @"VisualBasic\Windows", @"ConsoleApplication.zip\consoleapplication.vstemplate" }
                    }

                #endregion
            };

        public static string GetProjectTemplate(this DTE dte, ProjectKind kind, ProjectLanguage lang)
        {
            Debug.Assert(dte != null, "GetTemplateRootPath: dte is null");

            return string.Format(dte.GetTemplatePath(ProjectTemplates[(int)lang][(int)kind]), "ProjectTemplatesCache");
        }

        private static String GetTemplatePath(this DTE dte, String[] template)
        {
            Debug.Assert(dte != null, "GetTemplateRootPath: dte is null");

            var skuName = Path.GetFileNameWithoutExtension(dte.FileName);
            var path = Path.GetDirectoryName(dte.FullName);

            // TODO: remove this hack and use some other mechanism to locate the path to the template files for each Sku.

            // Express Sku templates are in a subdirectory matching the Sku name.
            if (skuName.ToUpperInvariant().Contains("EXPRESS"))
            {
                path = Path.Combine(path, skuName);
            }

            // We don't know what type of template this is (project or item) so parameterize it.
            if (template.Length <= 2
                || template[2] != "NOCACHE")
            {
                path = Path.Combine(path, "{0}");
            }

            // VCS and VB sku templates are all located in the same folder. Other skus have a language and category heirarchy.
            if (!skuName.Equals("VCSExpress", StringComparison.InvariantCultureIgnoreCase)
                &&
                !skuName.Equals("VBExpress", StringComparison.InvariantCultureIgnoreCase))
            {
                path = Path.Combine(path, template[0]);
            }

            // Append the LocaleID and, finally, the actual template file.
            if (template.Length <= 2
                || template[2] != "NOCACHE")
            {
                path = Path.Combine(path, Convert.ToString(dte.LocaleID));
            }

            path = Path.Combine(path, template[1]);

            return path;
        }

        /// <summary>
        ///     Executes the specified command for the open document.
        /// </summary>
        public static void ExecuteCommandForOpenDocument(this DTE dte, string fullFilePath, string command)
        {
            Debug.Assert(dte != null, "dte must not be null.");
            Debug.Assert(!string.IsNullOrWhiteSpace(fullFilePath), "fullFilePath must not be null or emtpy string.");

            var serviceProvider = new ServiceProvider((IServiceProvider)dte);

            IVsUIHierarchy hierarchy;
            uint itemId;
            IVsWindowFrame windowFrame;
            if (VsShellUtilities.IsDocumentOpen(serviceProvider, fullFilePath, Guid.Empty, out hierarchy, out itemId, out windowFrame))
            {
                windowFrame.Show();
                dte.ExecuteCommand(command, "");
            }
            else
            {
                throw new InvalidOperationException(
                    string.Format("Executing command '{0}' failed. Document '{1}' not open.", command, fullFilePath));
            }
        }

        /// <summary>
        ///     Determines if the document is dirty by querying the doc data directly
        /// </summary>
        public static bool IsDocumentDirty(this DTE dte, string documentPath)
        {
            Debug.Assert(dte != null, "dte must not be null.");
            Debug.Assert(!string.IsNullOrWhiteSpace(documentPath), "!string.IsNullOrWhiteSpace(documentPath)");

            var serviceProvider = new ServiceProvider((IServiceProvider)dte);

            IVsHierarchy vsHierarchy;
            uint itemId, docCookie;
            IVsPersistDocData vsPersistDocData;
            VsShellUtilities.GetRDTDocumentInfo(
                serviceProvider, documentPath, out vsHierarchy, out itemId, out vsPersistDocData, out docCookie);
            if (vsPersistDocData != null)
            {
                int isDirty;
                vsPersistDocData.IsDocDataDirty(out isDirty);
                return isDirty == 1;
            }

            throw new InvalidOperationException("Could not find the doc data in IsDocumentDirty");
        }

        //[DllImport("User32.dll", EntryPoint = "BringWindowToTop", CharSet = CharSet.Auto)]
        //private static extern int BringWindowToTop(IntPtr hWnd);
        //#endregion

        //#region VS Solution Utilities

        /// <summary>
        ///     Open an existing solution
        /// </summary>
        /// <param name="name">solution name</param>
        public static void OpenSolution(this DTE dte, string name)
        {
            Debug.Assert(dte != null, "dte must not be null.");
            Debug.Assert(!string.IsNullOrWhiteSpace(name), "name must not be null or empty string.");

            dte.Solution.Open(Path.GetFullPath(name));
        }

        /// <summary>
        ///     Close a solution
        /// </summary>
        /// <param name="dte">dte instance</param>
        /// <param name="name">save or not before closing</param>
        public static void CloseSolution(this DTE dte, bool saveFirst)
        {
            Debug.Assert(dte != null, "dte must not be null.");

            dte.Solution.Close(saveFirst);
        }

        public const string VenusProjectGuid = "{E24C65DC-7377-472b-9ABA-BC803B73C61A}";

        /// <summary>
        ///     Create a new project using specified info
        /// </summary>
        /// <param name="dte">dte instance</param>
        /// <param name="projectName">project name</param>
        /// <param name="kind">project kind</param>
        /// <param name="language">project language</param>
        /// <returns>new created project</returns>
        public static Project CreateProject(
            this DTE dte, string solutionPath, string projectName, ProjectKind kind, ProjectLanguage language)
        {
            Debug.Assert(dte != null, "CreateProject: Dte is null");
            Debug.Assert(!string.IsNullOrEmpty(solutionPath), "!string.IsNullOrEmpty(solutionPath)");
            Debug.Assert(!string.IsNullOrEmpty(projectName), "!string.IsNullOrEmpty(projectName)");

            Debug.Assert(dte.Solution != null, "CreateProject: dte.Solution must be created by adding project");

            var template = GetProjectTemplate(dte, kind, language);

            // Check if the project directory exists delete it does since AddFromTemplate will throw if the directory exists.
            var projectDir = Path.Combine(solutionPath, projectName);
            if (Directory.Exists(projectDir))
            {
                Directory.Delete(projectDir, true);
            }

            dte.Solution.AddFromTemplate(template, Path.Combine(solutionPath, projectName), projectName, false);
            foreach (Project project in dte.Solution.Projects)
            {
                if (project.Name.Contains(projectName)) // Venus projects Name is the full path name - so need to use Contains()
                {
                    return project;
                }
            }

            Debug.Fail("There is no new project created.");

            return null;
        }

        /// <summary>
        ///     Project with a given name, or null.
        /// </summary>
        /// <param name="name">project name</param>
        public static Project FindProject(this DTE dte, string name)
        {
            Debug.Assert(dte != null, "FindProject: Dte is null");
            Debug.Assert(!string.IsNullOrEmpty(name));

            foreach (Project project in dte.Solution.Projects)
            {
                if (project.Name.Equals(name))
                {
                    return project;
                }
            }
            return null;
        }

        /// <summary>
        ///     Add a new item to the given project. If the given project is null, it will add the item at the solution level.
        /// </summary>
        /// <param name="dte">DTE instance</param>
        /// <param name="itemTemplateName">the template name of the item which is shown on the new item wizard, e.g., "General\\Activity Diagram"</param>
        /// <param name="itemFilename">the filename for the new item, including extension</param>
        /// <param name="containingProject">the project where the new item will be added</param>
        /// <returns>returns the added item</returns>
        public static ProjectItem AddNewItem(DTE dte, string itemTemplateName, string itemFilename, Project containingProject)
        {
            // Select the containing project
            SelectProject(dte, containingProject);
            // Add the item
            return dte.ItemOperations.AddNewItem(itemTemplateName, itemFilename);
        }

        /// <summary>
        ///     Add an existing item to the given project. If the given project is null, it will add the item at the solution level.
        /// </summary>
        /// <param name="dte">DTE instance</param>
        /// <param name="itemFilename">the filename for the new item, including extension</param>
        /// <param name="containingProject">the project where the new item will be added</param>
        public static ProjectItem AddExistingItem(this DTE dte, string itemFilename, Project containingProject)
        {
            Debug.Assert(dte != null, "dte != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(itemFilename), "!string.IsNullOrWhiteSpace(itemFilename)");
            Debug.Assert(containingProject != null, "containingProject != null");

            // Select the containing project
            SelectProject(dte, containingProject);
            // Add the item
            var projectItem = dte.ItemOperations.AddExistingItem(itemFilename);
            return projectItem;
        }

        /// <summary>
        ///     Select the given project in the solution explorer. If the project is null, it will select the solution.
        /// </summary>
        /// <param name="dte">dte instance</param>
        /// <param name="project">the project to select</param>
        public static void SelectProject(this DTE dte, Project project)
        {
            Debug.Assert(dte != null, "dte != null");

            dte.Windows.Item(Constants.vsWindowKindSolutionExplorer).Activate();

            var uiItemFullName = string.Empty;
            if (project != null)
            {
                // Venus projects have special names that needs additional process
                if (project.Kind == VenusProjectGuid)
                {
                    // Remove the useless part
                    var tokens = project.Name.Split(new[] { '\\' });
                    uiItemFullName = tokens[tokens.Length - 2];
                }
                else
                {
                    uiItemFullName = project.Name;
                }
                uiItemFullName = "\\" + uiItemFullName;
                // Add the whole path to the item full name
                while (project.ParentProjectItem != null)
                {
                    uiItemFullName = "\\" + project.ParentProjectItem.ContainingProject.Name + uiItemFullName;
                    project = project.ParentProjectItem.ContainingProject;
                }
            }

            var hierarchy = (UIHierarchy)dte.Windows.Item(Constants.vsWindowKindSolutionExplorer).Object;
            // Add the solution node to the item full name
            uiItemFullName = hierarchy.UIHierarchyItems.Item(1).Name + uiItemFullName;
            // Get the hierarchy item given the item full name and select the item
            hierarchy.GetItem(uiItemFullName).Select(vsUISelectionType.vsUISelectionTypeSelect);
        }

        /// <summary>
        ///     Opens the specified file
        /// </summary>
        /// <param name="fileName">The full file name</param>
        public static Window OpenFile(this DTE dte, string fileName)
        {
            Debug.Assert(dte != null, "dte must not be null.");

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                var win = dte.OpenFile("{00000000-0000-0000-0000-000000000000}", fileName);
                win.Activate();
                return win;
            }

            return null;
        }

        /// <summary>
        ///     Closes a document
        /// </summary>
        /// <param name="fullDocName">document full name</param>
        /// <param name="saveChanges">save change or not</param>
        /// <returns>if the operation success</returns>
        public static bool CloseDocument(this DTE dte, string fullDocName, bool saveChanges)
        {
            Debug.Assert(dte != null, "dte must not be null.");
            Debug.Assert(!string.IsNullOrWhiteSpace(fullDocName), "fullDocName must not be null or empty string.");

            foreach (Document doc in dte.Documents)
            {
                if (fullDocName.Equals(doc.FullName))
                {
                    doc.Close(saveChanges ? vsSaveChanges.vsSaveChangesYes : vsSaveChanges.vsSaveChangesNo);
                    Debug.WriteLine("CloseFiles(): called doc.Close()");
                    return true;
                }
            }
            return false;
        }
    }

    #endregion
}
