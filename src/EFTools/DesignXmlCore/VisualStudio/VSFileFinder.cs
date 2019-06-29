// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using VSErrorHandler = Microsoft.VisualStudio.ErrorHandler;
// This file is linked in the DataEntityDesign project as well as the DslPackage project.
// It is being used by the bootstrap package so that a dependency is not introduced on DataEntityDesign.
// This #if fixes the namespace so that each project will build correctly.
#if !IS_BOOTSTRAP_PACKAGE
using Microsoft.Data.Entity.Design.VisualStudio.Package;
#endif

#if IS_BOOTSTRAP_PACKAGE
using VSHelpers = Microsoft.Data.Entity.Design.BootstrapPackage.BootstrapUtils;
namespace Microsoft.Data.Entity.Design.BootstrapPackage
#else

namespace Microsoft.Data.Entity.Design.VisualStudio
#endif

{
    using System;
    using Microsoft.VisualStudio.Shell.Interop;
#if !IS_BOOTSTRAP_PACKAGE

    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
#endif

    /// <summary>
    ///     This class will find all  files in the specified VS project or VS solution with the specified input.
    ///     The input could be an extension, file name with extension, or file full-path name.
    /// </summary>
    internal class VSFileFinder
    {
#if !IS_BOOTSTRAP_PACKAGE

        internal struct VSFileInfo
        {
            internal string Path;
            internal uint ItemId;
            internal IVsHierarchy Hierarchy;
        }

        private readonly List<VSFileInfo> _paths = new List<VSFileInfo>();

#endif

        private readonly string _input;
        private bool _foundMatch;

        internal VSFileFinder(string input)
        {
            _input = input;
        }

        internal bool ExistInProject(IVsHierarchy projectHierarchy)
        {
            // Reset _foundMatch value.
            _foundMatch = false;

            var vsp4 = projectHierarchy as IVsProject4;
            if (vsp4 != null)
            {
                // use IVsProject4 if available - it is much faster than the other mechanism, but all projects may not implement it
                _foundMatch = ExistInProjectFast(vsp4);
            }
            else
            {
                var visitor = new HierarchyVisitor(DoesItemMatch);
                visitor.VisitHierarchy(projectHierarchy);
            }

            return _foundMatch;
        }

        private bool ExistInProjectFast(IVsProject4 vsp4)
        {
            var pfDoesContain = 0;
            VSErrorHandler.ThrowOnFailure(vsp4.ContainsFileEndingWith(_input, out pfDoesContain));
            return (pfDoesContain != 0);
        }

        private void DoesItemMatch(IVsHierarchy item, uint id, VsProjectItemPath projectItemPath)
        {
            if (DoItemNamesComparison(_input, projectItemPath))
            {
                _foundMatch = true;
            }
        }

        private static bool DoItemNamesComparison(string input, VsProjectItemPath projectItemPath)
        {
            // If input contains "\" character, the user might want to do search by full path name.
            if (input.Contains("\\"))
            {
                return projectItemPath.Path.EndsWith(input, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return projectItemPath.RelativePath.EndsWith(input, StringComparison.OrdinalIgnoreCase);
            }
        }

#if !IS_BOOTSTRAP_PACKAGE

        internal List<VSFileInfo> MatchingFiles
        {
            get { return _paths; }
        }

        internal List<VSFileInfo> FindInProject(IVsHierarchy projectHierarchy)
        {
            var vsp4 = projectHierarchy as IVsProject4;
            if (vsp4 != null)
            {
                // use IVsProject4 if available - it is much faster than the  other mechanism, but all projects may not implement it
                FindInProjectFast(vsp4, projectHierarchy);
            }
            else
            {
                var visitor = new HierarchyVisitor(AddMatchFileInfoToResult);
                visitor.VisitHierarchy(projectHierarchy);
            }

            return _paths;
        }

        private void FindInProjectFast(IVsProject4 vsp4, IVsHierarchy projectHierarchy)
        {
            uint celt = 0;
            uint[] rgItemIds = null;
            uint pcActual = 0;

            //
            // call this method twice, first time is to get the count, second time is to get the data.
            //
            VSErrorHandler.ThrowOnFailure(vsp4.GetFilesEndingWith(_input, celt, rgItemIds, out pcActual));
            if (pcActual > 0)
            {
                // now we know the actual size of the array to allocate, so invoke again
                celt = pcActual;
                rgItemIds = new uint[celt];
                VSErrorHandler.ThrowOnFailure(vsp4.GetFilesEndingWith(_input, celt, rgItemIds, out pcActual));
                Debug.Assert(celt == pcActual, "unexpected number of entries returned from GetFilesEndingWith()");

                for (var i = 0; i < celt; i++)
                {
                    object pvar;
                    // NOTE:  in cpp, this property is not the full path.  It is the full path in c# & vb projects.
                    var hr = projectHierarchy.GetProperty(rgItemIds[i], (int)__VSHPROPID.VSHPROPID_SaveName, out pvar);
                    var path = pvar as string;
                    if (VSErrorHandler.Succeeded(hr)
                        && path != null)
                    {
                        // Dev10 Bug 653879: Retrieving project item absolute URL is expensive so retrieve when we actually need it.
                        VSFileInfo fileInfo;
                        fileInfo.ItemId = rgItemIds[i];
                        fileInfo.Path = path;
                        fileInfo.Hierarchy = projectHierarchy;
                        _paths.Add(fileInfo);
                    }
                }
            }
        }

        private void AddMatchFileInfoToResult(IVsHierarchy item, uint id, VsProjectItemPath projectItemPath)
        {
            if (DoItemNamesComparison(_input, projectItemPath))
            {
                var path = projectItemPath.Path;
                var fi = new FileInfo(path);
                if (fi.Exists)
                {
                    VSFileInfo vsFileInfo;
                    vsFileInfo.Path = path;
                    vsFileInfo.ItemId = id;
                    vsFileInfo.Hierarchy = item;
                    _paths.Add(vsFileInfo);
                }
            }
        }

#endif

    }
}
