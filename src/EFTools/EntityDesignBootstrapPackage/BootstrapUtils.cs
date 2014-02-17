// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.BootstrapPackage
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using EnvDTE;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using VSLangProj;

    internal static class BootstrapUtils
    {
        private const uint VSITEMID_ROOT = unchecked((uint)-2);

        private const string ProjectKindVb = PrjKind.prjKindVBProject;
        private const string ProjectKindCSharp = PrjKind.prjKindCSharpProject;
        private const string ProjectKindWeb = "{E24C65DC-7377-472b-9ABA-BC803B73C61A}";

        internal static bool IsWebProject(Project project)
        {
            Debug.Assert(project != null, "project != null");

            return String.Equals(project.Kind, ProjectKindWeb, StringComparison.OrdinalIgnoreCase);
        }

        // <summary>
        //     Determine whether EDM is supported in the current projec type.
        // </summary>
        internal static bool IsEDMSupportedInProject(IVsHierarchy hierarchy)
        {
            // given existing call paths, hierarchy should never be null.
            if (hierarchy == null)
            {
                throw new ArgumentException("hierarchy should not be null");
            }

            var currentProject = GetProject(hierarchy);

            // not all hierarchy instances will return a Project, so check for null.
            return currentProject != null &&
                   (String.Equals(currentProject.Kind, ProjectKindCSharp, StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(currentProject.Kind, ProjectKindVb, StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(currentProject.Kind, ProjectKindWeb, StringComparison.OrdinalIgnoreCase));
        }

        // <devdoc>
        //     Does the work to get a DTE Project from the given IVsHierarchy.  May return null if the given IVsHierarchy doesn't have a DTE Project.  Callers should check for null.
        // </devdoc>
        internal static Project GetProject(IVsHierarchy hierarchy)
        {
            Project project = null;

            Debug.Assert(hierarchy != null, "null hierarchy passed to GetProject?");
            if (hierarchy != null)
            {
                object o;
                var hr = hierarchy.GetProperty(VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out o);

                //Debug.Assert(NativeMethods.Succeeded(hr), "hierarchy.GetProperty(ExtObject) failed?");
                if (hr >= 0
                    && (o != null))
                {
                    project = o as Project;
                }
            }

            // NOTE: not all hierarchy instances will have a non-null value here.  
            return project;
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
    }
}
