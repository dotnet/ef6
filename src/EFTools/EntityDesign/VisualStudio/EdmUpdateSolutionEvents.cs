// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    internal class EdmUpdateSolutionEvents : IVsUpdateSolutionEvents2
    {
        private static EdmUpdateSolutionEvents _instance;

        internal static EdmUpdateSolutionEvents Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new EdmUpdateSolutionEvents();
                }

                return _instance;
            }
        }

        private EdmUpdateSolutionEvents()
        {
        }

        #region IVsUpdateSolutionEvents2 Members

        int IVsUpdateSolutionEvents2.UpdateProjectCfg_Begin(
            IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
        {
            //
            //   if clean project or solution,   dwAction == 0x100000
            //   if build project or solution,   dwAction == 0x010000
            //   if rebuild project or solution, dwAction == 0x410000
            //
            if (dwAction == 0x010000
                || dwAction == 0x410000)
            {
                var validationSuccessful =
                    VisualStudioEdmxValidator.LoadAndValidateAllFilesInProject(
                        pHierProj, /*doEscherValidation*/ false, ShouldValidateArtifactDuringBuild);

                // we cause a 'build break' for command-line builds by setting PfCancel = 1
                if (PackageManager.Package.IsBuildingFromCommandLine
                    && !validationSuccessful)
                {
                    pfCancel = 1;
                }
            }

            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateProjectCfg_Done(
            IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return NativeMethods.S_OK;
        }

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            // clear misc errors whenever a build begins
            ErrorListHelper.MiscErrorList.Clear();
            return NativeMethods.S_OK;
        }

        public int UpdateSolution_Cancel()
        {
            return NativeMethods.S_OK;
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            return NativeMethods.S_OK;
        }

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            return NativeMethods.S_OK;
        }

        #endregion

        // internal for testing
        internal static bool ShouldValidateArtifactDuringBuild(EFArtifact artifact)
        {
            Debug.Assert(artifact != null, "artifact != null");

            if (artifact.DesignerInfo() != null)
            {
                DesignerInfo designerInfo;
                if (artifact.DesignerInfo().TryGetDesignerInfo(OptionsDesignerInfo.ElementName, out designerInfo))
                {
                    var optionsDesignerInfo = (OptionsDesignerInfo)designerInfo;

                    if (optionsDesignerInfo.ValidateOnBuild != null
                        && optionsDesignerInfo.ValidateOnBuild.ValueAttr != null)
                    {
                        bool validateOnBuild;
                        if (bool.TryParse(optionsDesignerInfo.ValidateOnBuild.ValueAttr.Value, out validateOnBuild))
                        {
                            return validateOnBuild;
                        }
                    }
                }
            }

            return true;
        }
    }
}
