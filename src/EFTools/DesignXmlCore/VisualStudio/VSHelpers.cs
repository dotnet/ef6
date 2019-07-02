// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using EnvDTE;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.PlatformUI;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;
    using VSErrorHandler = Microsoft.VisualStudio.ErrorHandler;

    internal static class VSHelpers
    {
        // keeping as int so the GetVsColor would cache into the same table if we need it
        private static readonly Dictionary<int, Color> _cachedColors = new Dictionary<int, Color>();

        /// <summary>
        ///     Return environment VS Font
        /// </summary>
        internal static Font GetVSFont(IServiceProvider serviceProvider)
        {
            Debug.Assert(serviceProvider != null, "Service Provider is null");

            if (serviceProvider != null)
            {
                try
                {
                    var hostLocale = serviceProvider.GetService(typeof(SUIHostLocale)) as IUIHostLocale2;
                    if (hostLocale != null)
                    {
                        var fonts = new UIDLGLOGFONT[1];
                        var hr = hostLocale.GetDialogFont(fonts);
                        ErrorHandler.ThrowOnFailure(hr);

                        if (ErrorHandler.Succeeded(hr)
                            && (fonts.Length > 0))
                        {
                            return FontFromUIDLGLOGFONT(fonts[0]);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Fail(e.Message);
                    throw;
                }
            }
            return null;
        }

        // call the VS color service
        internal static Color GetColor(IServiceProvider serviceProvider, __VSSYSCOLOREX colorToFetch)
        {
            return FetchColor(serviceProvider, colorToFetch);
        }

        internal static Color GetColor(IServiceProvider serviceProvider, __VSSYSCOLOREX3 colorToFetch)
        {
            return FetchColor(serviceProvider, colorToFetch);
        }

        private static Color FetchColor(IServiceProvider serviceProvider, object colorToFetch)
        {
            if (_cachedColors.ContainsKey((int)colorToFetch))
            {
                return _cachedColors[(int)colorToFetch];
            }

            if (serviceProvider != null)
            {
                var vsUIShell = serviceProvider.GetService(typeof(IVsUIShell)) as IVsUIShell2;
                if (vsUIShell != null)
                {
                    uint vscolor;
                    NativeMethods.ThrowOnFailure(vsUIShell.GetVSSysColorEx((int)colorToFetch, out vscolor));

                    var vsColor = ColorTranslator.FromWin32((int)vscolor);
                    _cachedColors.Add((int)colorToFetch, vsColor);
                    return vsColor;
                }
                else
                {
                    Debug.Fail("Failed to get the IVsUIShell2 service");
                    return Color.Empty;
                }
            }
            else
            {
                Debug.Fail("incorrectly constructed color service");
                return Color.Empty;
            }
        }

        /// <summary>
        ///     Convert UIDLGLOGFONT type to Font type.
        ///     The code is copied over from env\vscore\package\CommonIDEStatics.cs
        /// </summary>
        /// <param name="logFont"></param>
        /// <returns></returns>
        private static Font FontFromUIDLGLOGFONT(UIDLGLOGFONT logFont)
        {
            var conversion = new char[logFont.lfFaceName.Length];
            var i = 0;

            foreach (var convertChar in logFont.lfFaceName)
            {
                conversion[i++] = (char)convertChar;
            }

            var familyName = new String(conversion);
            Single emSize = 0 - logFont.lfHeight;

            var style = FontStyle.Regular;
            const int FW_NORMAL = 400;

            if (logFont.lfItalic > 0)
            {
                style |= FontStyle.Italic;
            }

            if (logFont.lfUnderline > 0)
            {
                style |= FontStyle.Underline;
            }

            if (logFont.lfStrikeOut > 0)
            {
                style |= FontStyle.Strikeout;
            }

            if (logFont.lfWeight > FW_NORMAL)
            {
                style |= FontStyle.Bold;
            }

            var unit = GraphicsUnit.Pixel;
            var gdiCharSet = logFont.lfCharSet;
            return new Font(familyName, emSize, style, unit, gdiCharSet);
        }

        public static object GetDocData(IServiceProvider site, string documentPath)
        {
            uint docCookie;
            return GetDocData(site, documentPath, _VSRDTFLAGS.RDT_NoLock, out docCookie);
        }

        public static object GetDocData(IServiceProvider site, string documentPath, _VSRDTFLAGS lockFlags, out uint docCookie)
        {
            if (site == null)
            {
                VSErrorHandler.ThrowOnFailure(VSConstants.E_UNEXPECTED);
            }

            var rdtService = site.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;

            IVsHierarchy hierarchy;
            uint itemId;
            var docDataPtr = IntPtr.Zero;
            docCookie = VSConstants.VSCOOKIE_NIL;
            object ret = null;

            try
            {
                if (
                    ErrorHandler.Succeeded(
                        rdtService.FindAndLockDocument(
                            (uint)lockFlags, documentPath, out hierarchy, out itemId, out docDataPtr, out docCookie))
                    && docDataPtr != IntPtr.Zero)
                {
                    ret = Marshal.GetObjectForIUnknown(docDataPtr);
                }
            }
            finally
            {
                if (docDataPtr != IntPtr.Zero)
                {
                    Marshal.Release(docDataPtr);
                }
            }

            return ret;
        }

        /// <summary>
        ///     Given a path to a document, this method will return the Project that the document is contained in.
        ///     This method uses a cached, sited VsPackage as an IServiceProvider (Package.GetGlobalService)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static Project GetProjectForDocument(string path)
        {
            uint itemId = 0;
            var isDocInProject = false;
            IVsHierarchy projectHierarchy = null;
            Project project = null;
            GetProjectAndFileInfoForPath(path, out projectHierarchy, out project, out itemId, out isDocInProject);
            return project;
        }

        /// <summary>
        ///     Given a path to a document, this method will return the Project that the document is contained in
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static Project GetProjectForDocument(string path, IServiceProvider serviceProvider)
        {
            uint itemId = 0;
            var isDocInProject = false;
            IVsHierarchy projectHierarchy = null;
            Project project = null;
            GetProjectAndFileInfoForPath(path, serviceProvider, out projectHierarchy, out project, out itemId, out isDocInProject);
            return project;
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IEnumHierarchies.Next(System.UInt32,Microsoft.VisualStudio.Shell.Interop.IVsHierarchy[],System.UInt32@)")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IEnumHierarchies.Reset")]
        private static void GetProjectAndFileInfoForPath(
            string originalPath, IServiceProvider serviceProvider, IVsSolution solution, out IVsHierarchy projectHierarchy,
            out Project project, out uint fileItemId, out bool isDocumentInProject)
        {
            var guid = Guid.Empty;
            IEnumHierarchies hierarchyEnum = null;
            fileItemId = VSConstants.VSITEMID_NIL;
            projectHierarchy = null;
            isDocumentInProject = false;
            project = null;

            if (solution != null)
            {
                var hr = solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_ALLPROJECTS, ref guid, out hierarchyEnum);
                if (NativeMethods.Succeeded(hr) && hierarchyEnum != null)
                {
                    hierarchyEnum.Reset();

                    uint numFetched = 1;
                    var item = new IVsHierarchy[1];

                    hierarchyEnum.Next(1, item, out numFetched);
                    while (numFetched == 1)
                    {
                        var vsProject = item[0] as IVsProject;
                        if (vsProject != null)
                        {
                            GetProjectAndFileInfoForPath(
                                vsProject, originalPath, out projectHierarchy, out project, out fileItemId, out isDocumentInProject);
                            if (isDocumentInProject)
                            {
                                break;
                            }
                        }
                        hierarchyEnum.Next(1, item, out numFetched);
                    }
                }
            }

            // didn't find a project, so check the misc files project
            if (project == null)
            {
                //
                // try not to create the Misc files project externally - in some rare cases it has caused Access violoations in VS (e.g., Dev10 Bug 864725).  
                // So, only create this if we really need it. 
                //
                IVsProject3 miscFilesProject = null;
                if (serviceProvider == null)
                {
                    miscFilesProject = GetMiscellaneousProject();
                }
                else
                {
                    miscFilesProject = GetMiscellaneousProject(serviceProvider);
                }

                if (miscFilesProject != null)
                {
                    GetProjectAndFileInfoForPath(
                        miscFilesProject, originalPath, out projectHierarchy, out project, out fileItemId, out isDocumentInProject);
                    if (project == null)
                    {
                        projectHierarchy = miscFilesProject as IVsHierarchy;
                        if (projectHierarchy != null)
                        {
                            project = GetProject(projectHierarchy);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Get the itemId of the item specified by path, and the IVsHierarchy of the project containing the item using
        ///     a cached VsPackage (Package.GetGlobalService)
        /// </summary>
        /// <param name="originalPath">the full path to a document</param>
        /// <param name="projectHierarchy">will contain the IVsHierarchy for the project</param>
        /// <param name="itemId">will contain the itemID of the document</param>
        /// <param name="isDocumentInProject">will be true if the document is contained in a project</param>
        /// <param name="project">Out param will contain the Project that this item is contained in, or null if the item isn't in a project</param>
        private static void GetProjectAndFileInfoForPath(
            string originalPath, out IVsHierarchy projectHierarchy, out Project project, out uint fileItemId, out bool isDocumentInProject)
        {
            var solution = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            GetProjectAndFileInfoForPath(
                originalPath, null, solution, out projectHierarchy, out project, out fileItemId, out isDocumentInProject);
        }

        /// <summary>
        ///     Get the itemId of the item specified by path, and the IVsHierarchy of the project containing the item.
        /// </summary>
        /// <param name="originalPath">the full path to a document</param>
        /// <param name="serviceProvider">the service provider to retrieve the solution and miscellaneous files project reference used by this method</param>
        /// <param name="projectHierarchy">will contain the IVsHierarchy for the project</param>
        /// <param name="itemId">will contain the itemID of the document</param>
        /// <param name="isDocumentInProject">will be true if the document is contained in a project</param>
        /// <param name="project">Out param will contain the Project that this item is contained in, or null if the item isn't in a project</param>
        internal static void GetProjectAndFileInfoForPath(
            string originalPath, IServiceProvider serviceProvider, out IVsHierarchy projectHierarchy, out Project project,
            out uint fileItemId, out bool isDocumentInProject)
        {
            var solution = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            GetProjectAndFileInfoForPath(
                originalPath, serviceProvider, solution, out projectHierarchy, out project, out fileItemId, out isDocumentInProject);
        }

        public static bool GetProjectAndFileInfoForPath(
            IVsProject vsProject, string originalPath, out IVsHierarchy projectHierarchy, out Project project, out uint fileItemId,
            out bool isDocInProject)
        {
            isDocInProject = false;
            fileItemId = 0;
            projectHierarchy = null;
            project = null;

            var priority = new VSDOCUMENTPRIORITY[1];
            var isDocInProjectInt = 0;

            uint foundItemId = 0;
            var hr = vsProject.IsDocumentInProject(originalPath, out isDocInProjectInt, priority, out foundItemId);
            if (NativeMethods.Succeeded(hr) && isDocInProjectInt == 1)
            {
                projectHierarchy = vsProject as IVsHierarchy;
                if (projectHierarchy != null)
                {
                    project = GetProject(projectHierarchy);
                }
                fileItemId = foundItemId;
                isDocInProject = true;
            }
            return isDocInProject;
        }

        /// <devdoc>
        ///     Does the work to get a DTE Project from the given IVsHierarchy.
        /// </devdoc>
        internal static Project GetProject(IVsHierarchy hierarchy)
        {
            Project project = null;

            Debug.Assert(hierarchy != null, "null hierarchy passed to GetProject?");
            if (hierarchy != null)
            {
                object o;
                var hr = hierarchy.GetProperty(NativeMethods.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out o);

                //Debug.Assert(NativeMethods.Succeeded(hr), "hierarchy.GetProperty(ExtObject) failed?");
                if (NativeMethods.Succeeded(hr)
                    && (o != null))
                {
                    project = o as Project;
                }
            }

            return project;
        }

        /// <summary>
        ///     Gets the miscellaneous project using a cached VsPackage (Package.GetGlobalService). NOTE: this
        ///     implementation much different than that in VsShellUtilities due
        ///     to a bug in their implementation.
        /// </summary>
        /// <param name="provider">
        ///     The service provider
        /// </param>
        /// <returns>
        ///     A reference to the IVsProject3 interface for the miscellaneous project.
        /// </returns>
        private static IVsProject3 GetMiscellaneousProject()
        {
            var miscFiles =
                Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsExternalFilesManager)) as IVsExternalFilesManager;
            IVsProject project = null;
            if (miscFiles != null)
            {
                NativeMethods.ThrowOnFailure(miscFiles.GetExternalFilesProject(out project));
            }
            return (project as IVsProject3);
        }

        /// <summary>
        ///     Get miscellaneous project from current solution. NOTE: this
        ///     implementation much different than that in VsShellUtilities due
        ///     to a bug in their implementation.
        /// </summary>
        /// <param name="provider">
        ///     The service provider
        /// </param>
        /// <returns>
        ///     A reference to the IVsProject3 interface for the miscellaneous project.
        /// </returns>
        public static IVsProject3 GetMiscellaneousProject(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            var miscFiles = provider.GetService(typeof(SVsExternalFilesManager)) as IVsExternalFilesManager;
            IVsProject project = null;
            if (miscFiles != null)
            {
                NativeMethods.ThrowOnFailure(miscFiles.GetExternalFilesProject(out project));
            }
            return (project as IVsProject3);
        }

        /// <summary>
        ///     returns the IVsTextLines for the given docData object
        /// </summary>
        /// <param name="docData"></param>
        /// <returns></returns>
        internal static IVsTextLines GetVsTextLinesFromDocData(object docData)
        {
            if (docData == null)
            {
                return null;
            }
            var buffer = docData as IVsTextLines;
            if (buffer == null)
            {
                var bp = docData as IVsTextBufferProvider;
                if (bp != null)
                {
                    VSErrorHandler.ThrowOnFailure(bp.GetTextBuffer(out buffer));
                }
            }
            return buffer;
        }

        internal static string GetTextFromVsTextLines(IVsTextLines vsTextLines)
        {
            int lines;
            int lastLineLength;
            VSErrorHandler.ThrowOnFailure(vsTextLines.GetLineCount(out lines));
            VSErrorHandler.ThrowOnFailure(vsTextLines.GetLengthOfLine(lines - 1, out lastLineLength));

            string text;
            VSErrorHandler.ThrowOnFailure(vsTextLines.GetLineText(0, 0, lines - 1, lastLineLength, out text));
            return text;
        }

        internal static IVsHierarchy GetVsHierarchy(Project project, IServiceProvider serviceProvider)
        {
            var vsSolution = serviceProvider.GetService(typeof(IVsSolution)) as IVsSolution;
            IVsHierarchy hier;
            NativeMethods.ThrowOnFailure(vsSolution.GetProjectOfUniqueName(project.UniqueName, out hier));
            Debug.Assert(hier != null, "Could not get project for name " + project.UniqueName);
            return hier;
        }

        /// <summary>
        ///     Navigates to the document and places the selection at the specified location.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.TextManager.Interop.IVsTextManager.NavigateToLineAndColumn(Microsoft.VisualStudio.TextManager.Interop.IVsTextBuffer,System.Guid@,System.Int32,System.Int32,System.Int32,System.Int32)")]
        internal static void TextBufferNavigateTo(
            IServiceProvider serviceProvider, object docData, Guid logicalViewGuid, int lineNumber, int columnNumber)
        {
            // get the VsTextBuffer
            var buffer = docData as VsTextBuffer;
            if (buffer == null)
            {
                var bufferProvider = docData as IVsTextBufferProvider;
                if (bufferProvider != null)
                {
                    IVsTextLines lines;
                    NativeMethods.ThrowOnFailure(bufferProvider.GetTextBuffer(out lines));
                    buffer = lines as VsTextBuffer;
                    Debug.Assert(buffer != null, "IVsTextLines does not implement IVsTextBuffer");
                }
            }

            if (buffer == null)
            {
                return;
            }

            // finally, perform the navigation.
            var mgr = serviceProvider.GetService(typeof(VsTextManagerClass)) as IVsTextManager;
            if (mgr == null)
            {
                return;
            }

            var logicalView = logicalViewGuid;

            mgr.NavigateToLineAndColumn(buffer, ref logicalView, lineNumber, columnNumber, lineNumber, columnNumber);
        }

        // assign LinkLabel color approved by UX
        internal static void AssignLinkLabelColor(LinkLabel linkLabel)
        {
            linkLabel.ForeColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);
            linkLabel.LinkColor = VSColorTheme.GetThemedColor(EnvironmentColors.CommandBarMenuLinkTextColorKey);
            linkLabel.ActiveLinkColor = VSColorTheme.GetThemedColor(EnvironmentColors.CommandBarMenuLinkTextHoverColorKey);
            linkLabel.VisitedLinkColor = VSColorTheme.GetThemedColor(EnvironmentColors.ControlLinkTextPressedColorKey);
        }

        internal static IEnumerable<IVsHierarchy> GetProjectsInSolution(IVsSolution solution)
        {
            if (solution == null)
            {
                throw new ArgumentNullException("solution");
            }

            IEnumHierarchies penum = null;
            var nullGuid = Guid.Empty;
            var hr = solution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_ALLPROJECTS, ref nullGuid, out penum);
            if (ErrorHandler.Succeeded(hr)
                && (penum != null))
            {
                uint fetched = 0;
                var rgelt = new IVsHierarchy[1];
                while (penum.Next(1, rgelt, out fetched) == 0
                       && fetched == 1)
                {
                    yield return rgelt[0];
                }
            }
        }

        /// <summary>
        ///     Determines if the file can be checked out on edit and checks out if necessary
        /// </summary>
        /// <param name="serviceProvider">IServiceProvider</param>
        /// <param name="moniker">file to checkout</param>
        /// <returns>true/false depending on </returns>
        internal static bool CheckOutFilesIfSaveable(IServiceProvider serviceProvider, string[] documents)
        {
            // query the VS SCC provider and ask if we can edit the file
            if (documents.Length > 0)
            {
                var queryEditQuerySave = serviceProvider.GetService(typeof(SVsQueryEditQuerySave)) as IVsQueryEditQuerySave2;
                if (queryEditQuerySave != null)
                {
                    uint result;

                    // This may bring up a UI to ask the user to checkout a file depending on the user's settings.
                    // NOTE that we should not allow the QEF_ImplicitEdit input flag because this is the wrong user experience and the user
                    // can override this in Tools->Options, which can lead to false assumptions here and more bugs.
                    var hr = queryEditQuerySave.QuerySaveFiles(
                        0, // Flags tagVSQueryEditFlags.QEF_AllowInMemoryEdits=0
                        documents.Length, // Number of elements in the array
                        documents, // Files to edit
                        null, // Input flags
                        null, // Input array of VSQEQS_FILE_ATTRIBUTE_DATA
                        out result // result of the checkout
                        );
                    if (NativeMethods.Succeeded(hr)
                        && (result == (uint)tagVSQueryEditResult.QER_EditOK))
                    {
                        return true;
                    }
                }
                else
                {
                    Debug.Fail("Not able to get the IVsQueryEditQuerySave2 service");
                }
            }
            else
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Determines if the file can be checked out on edit and checks out if necessary
        /// </summary>
        /// <param name="serviceProvider">IServiceProvider</param>
        /// <param name="moniker">file to checkout</param>
        /// <returns>true/false depending on </returns>
        internal static bool CheckOutFilesIfEditable(IServiceProvider serviceProvider, string[] documents)
        {
            // query the VS SCC provider and ask if we can edit the file
            if (documents.Length > 0)
            {
                var queryEditQuerySave = serviceProvider.GetService(typeof(SVsQueryEditQuerySave)) as IVsQueryEditQuerySave2;
                if (queryEditQuerySave != null)
                {
                    uint result;
                    uint outFlags;

                    // This may bring up a UI to ask the user to checkout a file depending on the user's settings.
                    // NOTE that we should not allow the QEF_ImplicitEdit input flag because this is the wrong user experience and the user
                    // can override this in Tools->Options, which can lead to false assumptions here and more bugs.
                    var hr = queryEditQuerySave.QueryEditFiles(
                        0, // Flags tagVSQueryEditFlags.QEF_AllowInMemoryEdits=0
                        documents.Length, // Number of elements in the array
                        documents, // Files to edit
                        null, // Input flags
                        null, // Input array of VSQEQS_FILE_ATTRIBUTE_DATA
                        out result, // result of the checkout
                        out outFlags // Additional flags
                        );
                    if (NativeMethods.Succeeded(hr)
                        && (result == (uint)tagVSQueryEditResult.QER_EditOK))
                    {
                        return true;
                    }
                }
                else
                {
                    Debug.Fail("Not able to get the IVsQueryEditQuerySave2 service");
                }
            }
            else
            {
                return true;
            }

            return false;
        }
    }
}
